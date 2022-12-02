// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Data.Colleague.DataContracts;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for organizational position relationships
    /// </summary>
    [RegisterType]
    public class OrganizationalPositionRelationshipRepository : BaseColleagueRepository, IOrganizationalPositionRelationshipRepository
    {

        public OrganizationalPositionRelationshipRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Add a position relationship
        /// </summary>
        /// <param name="organizationalPositionRelationshipEntity">Organizational position relationship entity</param>
        /// <returns>Added organizational position relationship</returns>
        public async Task<OrganizationalPositionRelationship> AddAsync(OrganizationalPositionRelationship organizationalPositionRelationshipEntity)
        {
            if (organizationalPositionRelationshipEntity == null)
            {
                throw new ArgumentNullException("organizationalPositionRelationshipEntity", "organizationalPositionRelationshipEntity cannot be null.");
            }

            var category = string.IsNullOrEmpty(organizationalPositionRelationshipEntity.RelationshipCategory)
                ? await GetDefaultManagerCategoryAsync()
                : organizationalPositionRelationshipEntity.RelationshipCategory;
            var transAddRequest = new UpdateRoleRelMultiRequest()
            {
                Action = "A",           // Add
                RelationshipType = "S", // Superior relationship
                RelationshipCategory = category,
                OrgRole = organizationalPositionRelationshipEntity.OrganizationalPositionId,
                RelatedOrgRole = organizationalPositionRelationshipEntity.RelatedOrganizationalPositionId,
                StartDate = DateTime.Today
            };

            var transAddResponse = await transactionInvoker.ExecuteAsync<UpdateRoleRelMultiRequest, UpdateRoleRelMultiResponse>(transAddRequest);
            if (!string.IsNullOrWhiteSpace(transAddResponse.ErrorOccurred) && !transAddResponse.ErrorOccurred.Equals("0"))
            {
                throw new ApplicationException(transAddResponse.Message);
            }

            var newOrgPosRelId = transAddResponse.RelationshipId;
            if (string.IsNullOrEmpty(newOrgPosRelId))
            {
                throw new ApplicationException("Add organizational position relationship error occurred. Unable to retrieve new relationship ID.");
            }
            var newOrgPosRelEntity = await GetOrganizationalPositionRelationshipAsync(newOrgPosRelId);

            return newOrgPosRelEntity;
        }

        /// <summary>
        /// Delete a relationship
        /// </summary>
        /// <param name="organizationalPositionRelationshipId">Organizational position relationship ID</param>
        public async Task DeleteAsync(string organizationalPositionRelationshipId)
        {
            if (string.IsNullOrWhiteSpace(organizationalPositionRelationshipId))
            {
                throw new ArgumentNullException("organizationalPositionRelationshipId", "organizationalPositionRelationshipId cannot be null or whitespace.");
            }

            var transDeleteRequest = new UpdateRoleRelMultiRequest()
            {
                Action = "D",
                RelationshipId = organizationalPositionRelationshipId
            };

            try
            {
                var transDeleteResponse = await transactionInvoker.ExecuteAsync<UpdateRoleRelMultiRequest, UpdateRoleRelMultiResponse>(transDeleteRequest);
                if (!string.IsNullOrWhiteSpace(transDeleteResponse.ErrorOccurred) && !transDeleteResponse.ErrorOccurred.Equals("0"))
                {
                    throw new ApplicationException(transDeleteResponse.Message);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error deleting organizational position relationship");
                throw;
            }
        }

        private async Task<OrganizationalPositionRelationship> GetOrganizationalPositionRelationshipAsync(string organizationalPositionRelationshipId)
        {
            if (string.IsNullOrWhiteSpace(organizationalPositionRelationshipId))
            {
                throw new ArgumentNullException("organizationalPositionRelationshipId", "organizationalPositionRelationshipId cannot be null or whitespace.");
            }

            var orgPosRelRecord = await DataReader.ReadRecordAsync<RoleRelationships>("ROLE.RELATIONSHIPS", organizationalPositionRelationshipId);
            var organizationalPositionRelationshipEntity = new OrganizationalPositionRelationship(orgPosRelRecord.Recordkey,
                                                                                                  orgPosRelRecord.RrlsOrgRole,
                                                                                                  "",
                                                                                                  orgPosRelRecord.RrlsRelatedOrgRole,
                                                                                                  "",
                                                                                                  orgPosRelRecord.RrlsRelationshipCategory);

            return organizationalPositionRelationshipEntity;
        }

        private async Task<ApplValcodes> GetRelationshipCategoriesAsync()
        {
            var relationshipCategories = await GetOrAddToCacheAsync<ApplValcodes>("RelationshipCategories",
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
