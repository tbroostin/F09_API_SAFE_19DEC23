// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for organizational relationships
    /// </summary>
    [RegisterType]
    public class OrganizationalRelationshipRepository : BaseColleagueRepository, IOrganizationalRelationshipRepository
    {
        private readonly int _readSize;
        private ApplValcodes relationshipCategories;

        /// <summary>
        /// Constructor for organizational relationship repository.
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public OrganizationalRelationshipRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _readSize = apiSettings != null && apiSettings.BulkReadSize > 0 ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Returns the organizational relationships for the given IDs
        /// </summary>
        /// <param name="ids">Organizational relationship IDs to retrieve</param>
        /// <returns>The organizational relationships</returns>
        public async Task<IEnumerable<OrganizationalRelationship>> GetAsync(List<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }
            try
            {
                List<OrgEntityRoleRel> orgEntityRoleRelRecords = new List<OrgEntityRoleRel>();
                for (int i = 0; i < ids.Count(); i += _readSize)
                {
                    var subList = ids.Skip(i).Take(_readSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<OrgEntityRoleRel>(subList);
                    if (bulkRecords != null)
                    {
                        orgEntityRoleRelRecords.AddRange(bulkRecords);
                    }
                }

                var organizationalRelationships = new List<OrganizationalRelationship>();

                foreach (var orgEntityRoleRel in orgEntityRoleRelRecords)
                {
                    organizationalRelationships.Add(new OrganizationalRelationship(orgEntityRoleRel.Recordkey, orgEntityRoleRel.OerrelOerId, orgEntityRoleRel.OerrelRelatedOerId, orgEntityRoleRel.OerrelRelationshipCategory));
                }

                return organizationalRelationships;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error reading IDs");
                throw;
            }
        }

        /// <summary>
        /// Adds an organizational relationship
        /// </summary>
        /// <param name="organizationalRelationshipEntity">The organizational relationship to add</param>
        /// <returns>The new organizational relationship</returns>
        public async Task<OrganizationalRelationship> AddAsync(OrganizationalRelationship organizationalRelationshipEntity)
        {
            if (organizationalRelationshipEntity == null)
            {
                throw new ArgumentNullException("organizationalRelationshipEntity");
            }
            await CheckForConflicts(organizationalRelationshipEntity);

            var organizationalRelationshipAddRequest = new UpdateOrgEntityRoleRelMultiRequest()
            {
                Action = "A",
                OrgEntityRoleRelId = "",
                OrgEntityRoleId = organizationalRelationshipEntity.OrganizationalPersonPositionId,
                RelatedOrgEntityRoleId = organizationalRelationshipEntity.RelatedOrganizationalPersonPositionId,
                RelationshipCategory = string.IsNullOrEmpty(organizationalRelationshipEntity.Category) ? await GetDefaultManagerCategoryAsync() : organizationalRelationshipEntity.Category
            };

            var organizationalRelationshipAddResponse = await transactionInvoker.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(organizationalRelationshipAddRequest);

            // Check response for errors
            //ErrorOccurred: OUT-- Code representing error that occurred
            //           1 = ORG.ENTITY.ROLE record does not exist for a value in OrgEntityRole
            //           2 = ORG.ENTITY.ROLE record does not exist for a value in RelatedOrgEntityRole
            //           3 = The two ORG.ENTITY.ROLE records reference the same person and
            //                  therefore cannot be related
            //           4 = ORG.ENTITY.ROLE.REL record does not exist for a value in OrgEntityRoleRel
            //           5 = An existing relationship for OrgEntityRel already exists for the specified category
            //           6 = Requested relationship would cause a circular relationship with an existing one
            //           7 = Invalid argument (see message)
            //  Message: OUT-- Detailed message describing the error
            if (!string.IsNullOrEmpty(organizationalRelationshipAddResponse.ErrorOccurred) && organizationalRelationshipAddResponse.ErrorOccurred != "0")
            {
                var errorMessage = "Error(s) occurred adding relationships '" + organizationalRelationshipAddRequest.OrgEntityRoleRelId + "':";
                errorMessage += organizationalRelationshipAddResponse.Message;
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException("Error occurred adding relationships");
            }

            var newId = organizationalRelationshipAddResponse.OrgEntityRoleRelId;
            var newOrganizationalRelationships = await GetAsync(new List<string> { newId });

            if (newOrganizationalRelationships.Any())
            {
                return newOrganizationalRelationships.First();
            }
            else
            {
                throw new InvalidOperationException("Error occurred retrieving relationship after addition.");
            }
        }

        /// <summary>
        /// Deletes an organizational relationship with the given ID
        /// </summary>
        /// <param name="id">The organizational relationship ID to delete</param>
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            var organizationalRelationshipDeleteRequest = new UpdateOrgEntityRoleRelMultiRequest()
            {
                Action = "D",
                OrgEntityRoleRelId = id,
            };

            var organizationalRelationshipDeleteResponse = await transactionInvoker.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(organizationalRelationshipDeleteRequest);

            // Check response for errors
            //ErrorOccurred: OUT-- Code representing error that occurred
            //           1 = ORG.ENTITY.ROLE record does not exist for a value in OrgEntityRole
            //           2 = ORG.ENTITY.ROLE record does not exist for a value in RelatedOrgEntityRole
            //           3 = The two ORG.ENTITY.ROLE records reference the same person and
            //                  therefore cannot be related
            //           4 = ORG.ENTITY.ROLE.REL record does not exist for a value in OrgEntityRoleRel
            //           5 = An existing relationship for OrgEntityRel already exists for the specified category
            //           6 = Requested relationship would cause a circular relationship with an existing one
            //           7 = Invalid argument (see message)
            //  Message: OUT-- Detailed message describing the error
            if (!string.IsNullOrEmpty(organizationalRelationshipDeleteResponse.ErrorOccurred) && organizationalRelationshipDeleteResponse.ErrorOccurred != "0")
            {
                var errorMessage = "Error(s) occurred updating relationships '" + organizationalRelationshipDeleteRequest.OrgEntityRoleRelId + "':";
                errorMessage += organizationalRelationshipDeleteResponse.Message;
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException("Error occurred deleting relationships");
            }

            return;
        }

        /// <summary>
        /// Updates an organizational relationship
        /// </summary>
        /// <param name="organizationalRelationshipEntity">The organizational relationship to update</param>
        /// <returns>The updated organizational relationship</returns>
        public async Task<OrganizationalRelationship> UpdateAsync(OrganizationalRelationship organizationalRelationshipEntity)
        {
            if (organizationalRelationshipEntity == null)
            {
                throw new ArgumentNullException("organizationalRelationshipEntity");
            }
            if (string.IsNullOrEmpty(organizationalRelationshipEntity.Id))
            {
                throw new ArgumentNullException("organiationalRelationshipEntity", "Must specify ID to update an organizational relationship");
            }
            // Check for conflicts with existing relationships before submitting for update
            // Throws InvalidOperationException if any issues found.
            await CheckForConflicts(organizationalRelationshipEntity);

            var organizationalRelationshipUpdateRequest = new UpdateOrgEntityRoleRelMultiRequest()
            {
                Action = "U",
                OrgEntityRoleRelId = organizationalRelationshipEntity.Id,
                OrgEntityRoleId = organizationalRelationshipEntity.OrganizationalPersonPositionId,
                RelatedOrgEntityRoleId = organizationalRelationshipEntity.RelatedOrganizationalPersonPositionId,
                RelationshipCategory = string.IsNullOrEmpty(organizationalRelationshipEntity.Category) ? await GetDefaultManagerCategoryAsync() : organizationalRelationshipEntity.Category
            };

            var organizationalRelationshipUpdateResponse = await transactionInvoker.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(organizationalRelationshipUpdateRequest);

            // Check response for errors
            //ErrorOccurred: OUT-- Code representing error that occurred
            //           1 = ORG.ENTITY.ROLE record does not exist for a value in OrgEntityRole
            //           2 = ORG.ENTITY.ROLE record does not exist for a value in RelatedOrgEntityRole
            //           3 = The two ORG.ENTITY.ROLE records reference the same person and
            //                  therefore cannot be related
            //           4 = ORG.ENTITY.ROLE.REL record does not exist for a value in OrgEntityRoleRel
            //           5 = An existing relationship for OrgEntityRel already exists for the specified category
            //           6 = Requested relationship would cause a circular relationship with an existing one
            //           7 = Invalid argument (see message)
            //  Message: OUT-- Detailed message describing the error
            if (!string.IsNullOrEmpty(organizationalRelationshipUpdateResponse.ErrorOccurred) && organizationalRelationshipUpdateResponse.ErrorOccurred != "0")
            {
                var errorMessage = "Error(s) occurred updating relationships '" + organizationalRelationshipUpdateRequest.OrgEntityRoleRelId + "':";
                errorMessage += organizationalRelationshipUpdateResponse.Message;
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException("Error occurred updating relationships");
            }

            var organizationalRelationships = await GetAsync(new List<string> { organizationalRelationshipUpdateRequest.OrgEntityRoleRelId });
            return organizationalRelationships.First();
        }

        private async Task CheckForConflicts(OrganizationalRelationship orgRelEntity)
        {
            // Read all relationships from Colleague
            var allRels = await DataReader.BulkReadRecordAsync<OrgEntityRoleRel>("ORG.ENTITY.ROLE.REL", "");
            // If category not supplied, use the default management category
            var categoryToCheck = string.IsNullOrEmpty(orgRelEntity.Category) ? await GetDefaultManagerCategoryAsync() : orgRelEntity.Category;
            // Do not allow if there is not another relationship establishing a superior for the primary org.entity.role
            var existingSupervisor = allRels.Where(a => a.OerrelOerId == orgRelEntity.OrganizationalPersonPositionId && a.OerrelRelationshipCategory == categoryToCheck && a.Recordkey != orgRelEntity.Id).FirstOrDefault();
            if (existingSupervisor != null)
            {
                var errorMessage = "ORG.ENTITY.ROLE " + orgRelEntity.OrganizationalPersonPositionId + " already has a supervisor.";
                throw new InvalidOperationException(errorMessage);
            }
            try
            {
                // Step through all superior relationships up the chain from the proposed superior to ensure that the proposed 
                // subordinate is not defined as a superior anywhere.
                CheckSuperiors(orgRelEntity.RelatedOrganizationalPersonPositionId, orgRelEntity.OrganizationalPersonPositionId, categoryToCheck, allRels.ToList());
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = "ORG.ENTITY.ROLE " + orgRelEntity.OrganizationalPersonPositionId + " is found up the chain from " + orgRelEntity.RelatedOrganizationalPersonPositionId;
                logger.Warn(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Make the org.entity.role "to check" is not up the chain from the original org.entity.role
        /// </summary>
        /// <param name="nextOrgEntityRoleId">The ID of the next org.entity.role (person/position) to find superiors for</param>
        /// <param name="orgEntityRoleIdToCheck">The org.entity.role to check for all the way up the chain</param>
        /// <param name="category">The category to examine</param>
        /// <param name="relationships">The list of all relationships to check</param>
        private void CheckSuperiors(string nextOrgEntityRoleId, string orgEntityRoleIdToCheck, string category, List<OrgEntityRoleRel> relationships)
        {
            // Find the superior relationships of the org.entity.role
            var superiorRelationships = relationships.Where(r => r.OerrelOerId == nextOrgEntityRoleId && r.OerrelRelationshipCategory == category).ToList();
            if (superiorRelationships != null)
            {
                // If the original org.entity.role appears in the list of supervisors for the related org.entity.role, we have a circular relationship
                var circularRelationship = superiorRelationships.Where(r => r.OerrelRelatedOerId == orgEntityRoleIdToCheck).FirstOrDefault();
                if (circularRelationship != null)
                {
                    // Log the specific information, but this will be followed up with another message in the catching method
                    logger.Error("Circular relationship for ORG.ENTITY.ROLE " + nextOrgEntityRoleId + " found in ORG.ENTITY.ROLE.REL " + circularRelationship.Recordkey);
                    throw new InvalidOperationException();
                }
                else
                {
                    foreach (OrgEntityRoleRel superior in superiorRelationships)
                    {
                        // If not, check each superior (even though there should be only one) to determine if this org.entity.role 
                        // appears in the next level up
                        CheckSuperiors(superior.OerrelRelatedOerId, orgEntityRoleIdToCheck, category, relationships);
                    }
                }
            }
            return;
        }


        private async Task<ApplValcodes> GetRelationshipCategoriesAsync()
        {
            if (relationshipCategories != null)
            {
                return relationshipCategories;
            }

            relationshipCategories = await GetOrAddToCacheAsync<ApplValcodes>("RelationshipCategories",
                async () =>
                {
                    ApplValcodes categoriesTable = await DataReader.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "RELATIONSHIP.CATEGORIES");
                    if (categoriesTable == null)
                    {
                        var errorMessage = "Unable to access RELATIONSHIP.CATEGORIES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return categoriesTable;
                }, Level1CacheTimeoutValue);
            return relationshipCategories;
        }

        /// <summary>
        /// When a category is not provided, get the default manager category. Throws an exception if not found.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetDefaultManagerCategoryAsync()
        {
            try
            {
                var assocEntry = (await GetRelationshipCategoriesAsync()).ValsEntityAssociation.Where(v => v.ValActionCode1AssocMember == "ORG").First();
                return assocEntry.ValInternalCodeAssocMember;
            }
            catch
            {
                var errorMessage = "RELATIONSHIP.CATEGORIES must have a default category designated with special action ORG. Unable to update relationship.";
                logger.Error(errorMessage);
                throw new ColleagueWebApiException(errorMessage);
            }
        }

    }
}
