// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Data.Colleague.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for organizational positions
    /// </summary>
    [RegisterType]
    public class OrganizationalPositionRepository : BaseColleagueRepository, IOrganizationalPositionRepository
    {
        private readonly int _readSize;
        private readonly string _orgIndicator;

        public OrganizationalPositionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _readSize = apiSettings != null && apiSettings.BulkReadSize > 0 ? apiSettings.BulkReadSize : 5000;
            _orgIndicator = "ORG";
        }

        /// <summary>
        /// Retrieves organizational positions for the given ids
        /// </summary>
        /// <param name="ids">Organizational position ids</param>
        /// <returns>Organizational position entities</returns>
        public async Task<IEnumerable<OrganizationalPosition>> GetOrganizationalPositionsByIdsAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids", "ids is required to get organizational positions");
            }

            var orgPositions = new List<OrganizationalPosition>();
            var roleTypes = await GetOrganizationalRoleTypes();
            var orgRoles = await GetOrgRoleRecordByIdsAsync(ids.ToArray());
            var orgPosRelationships = await GetOrganizationalPositionRelationshipsByRolesAsync(orgRoles.Select(or => or.Recordkey), orgRoles);
            foreach (var role in orgRoles)
            {
                if (roleTypes.Contains(role.OroleType))
                {
                    var orgPos = new OrganizationalPosition(role.Recordkey, role.OroleTitle);
                    if (orgPosRelationships != null)
                    {
                        foreach (var roleRel in orgPosRelationships.Where(opr => opr.OrganizationalPositionId == role.Recordkey || opr.RelatedOrganizationalPositionId == role.Recordkey))
                        {
                            orgPos.AddPositionRelationship(roleRel);
                        }
                    }
                    orgPositions.Add(orgPos);
                }
            }

            return orgPositions;
        }

        /// <summary>
        /// Retrieves organizational positions for the given search criteria or ids
        /// </summary>
        /// <param name="searchCriteria">Id or Partial position title</param>
        /// <param name="ids">Organizational position ids</param>
        /// <returns>Organizational position entities</returns>
        public async Task<IEnumerable<OrganizationalPosition>> GetOrganizationalPositionsAsync(string searchCriteria, IEnumerable<string> ids)
        {

            if (string.IsNullOrEmpty(searchCriteria) && (ids == null || !ids.Any()))
            {
                throw new ArgumentException("You must provide a search criteria or ids");
            }

            var idsList = new List<string>();
            if (ids != null)
            {
                idsList.AddRange(ids);
            }
            int id;
            if (int.TryParse(searchCriteria, out id))
            {
                idsList.Add(searchCriteria);
            }

            // Convert search criteria to uppercase and find titles based on partial string matches
            var criteria = "WITH OROLE.TITLE LIKE '...?...'";
            var searchCriteriaTitles = new List<string> { searchCriteria.ToUpper() };
            var matchedTitleIds = await DataReader.SelectAsync("ORG.ROLE", criteria, searchCriteriaTitles.ToArray());
            if (matchedTitleIds != null)
            {
                idsList.AddRange(matchedTitleIds);
            }
            return await GetOrganizationalPositionsByIdsAsync(idsList);
        }

        /// <summary>
        /// Gets org role records for the given ids
        /// </summary>
        /// <param name="ids">Ids for org role records</param>
        /// <returns>Org role records</returns>
        private async Task<List<OrgRole>> GetOrgRoleRecordByIdsAsync(string[] ids)
        {
            List<OrgRole> relatedOrgRoleRecords = new List<OrgRole>();
            if (ids != null && ids.Count() > 0)
            {
                for (int i = 0; i < ids.Count(); i += _readSize)
                {
                    var subList = ids.Skip(i).Take(_readSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<OrgRole>(subList);
                    if (bulkRecords != null)
                    {
                        relatedOrgRoleRecords.AddRange(bulkRecords);
                    }
                }
            }

            return relatedOrgRoleRecords;
        }

        /// <summary>
        /// Return only the list of roles types relevant to organizational positions
        /// </summary>
        /// <returns>Role type val codes representing organizational positions</returns>
        private async Task<IEnumerable<string>> GetOrganizationalRoleTypes()
        {
            var roleTypes = await GetOrAddToCacheAsync("OrganizationalRoleTypes",
                async () =>
                {
                    Ellucian.Data.Colleague.DataContracts.ApplValcodes roleTypesTable = await DataReader.ReadRecordAsync<Ellucian.Data.Colleague.DataContracts.ApplValcodes>("UT.VALCODES", "ORG.ROLE.TYPES");
                    if (roleTypesTable == null)
                    {
                        var errorMessage = "Unable to access ORG.ROLE.TYPES valcode table.";
                        logger.Error(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return roleTypesTable;
                }, Level1CacheTimeoutValue);

            var orgRoleTypes = roleTypes.ValsEntityAssociation.Where(v => v.ValActionCode1AssocMember == _orgIndicator).Select(v => v.ValInternalCodeAssocMember).ToList();
            if (orgRoleTypes.Count() == 0)
            {
                logger.Error("No ORG.ROLE.TYPES found with Action Code 1 of ORG, Cannot build organizational relationships.");
            }
            return orgRoleTypes;
        }

        /// <summary>
        /// Gets the organizational position relationships associated with the given role ids
        /// </summary>
        /// <param name="roleIds">Roles to retrieve position (role) relationships for</param>
        /// <param name="orgRoles">Role records to be used for relationship building.</param>
        /// <returns>Collection of Organizational Position Relationships</returns>
        private async Task<IEnumerable<OrganizationalPositionRelationship>> GetOrganizationalPositionRelationshipsByRolesAsync(IEnumerable<string> roleIds, IEnumerable<OrgRole> orgRoles)
        {
            var organizationalPositionRelationships = new List<OrganizationalPositionRelationship>();
            if (roleIds != null && roleIds.Any())
            {
                var roleRelationshipsFile = "ROLE.RELATIONSHIPS";
                // Get all Supervisor-Subordinate relationships, ignoring peer relationships
                var primaryCriteria = "RRLS.TYPE EQ 'S' AND RRLS.ORG.ROLE EQ '?'";
                var secondaryCriteria = "RRLS.TYPE EQ 'S' AND RRLS.RELATED.ORG.ROLE EQ '?'";
                var rolesArray = roleIds.ToArray();
                var primaryIds = await DataReader.SelectAsync(roleRelationshipsFile, primaryCriteria, rolesArray);
                var secondaryIds = await DataReader.SelectAsync(roleRelationshipsFile, secondaryCriteria, rolesArray);
                var primaryRoleRelationships = new List<RoleRelationships>();
                if (primaryIds.Any())
                {
                    primaryRoleRelationships.AddRange(await DataReader.BulkReadRecordAsync<RoleRelationships>(primaryIds));
                }
                var secondaryRoleRelationships = new List<RoleRelationships>();
                if (secondaryIds.Any())
                {
                    secondaryRoleRelationships.AddRange(await DataReader.BulkReadRecordAsync<RoleRelationships>(secondaryIds));
                }
                var allRelationships = new List<RoleRelationships>();
                if (primaryRoleRelationships != null)
                {
                    allRelationships.AddRange(primaryRoleRelationships);
                }
                if (secondaryRoleRelationships != null)
                {
                    var existingRelationshipIds = allRelationships.Select(rr => rr.Recordkey);
                    allRelationships.AddRange(secondaryRoleRelationships.Where(rr => !existingRelationshipIds.Contains(rr.Recordkey)));
                }

                // Filter relationships that are not current (already ended or start in future)
                var currentRelationships = allRelationships.Where(rr => rr.RrlsStartDate.HasValue
                                                                        && rr.RrlsStartDate.Value <= DateTime.Today
                                                                        && (!rr.RrlsEndDate.HasValue || rr.RrlsEndDate >= DateTime.Today));

                // Retrieve org role records that we do not already have that are part of a relationship
                var allOrgRoleIds = allRelationships.Select(rel => rel.RrlsOrgRole).Concat(allRelationships.Select(rel => rel.RrlsRelatedOrgRole));
                var notYetRetrievedOrgRoleIds = allOrgRoleIds.Except(orgRoles.Select(or => or.Recordkey));
                var allOrgRoles = orgRoles.ToList();
                if (notYetRetrievedOrgRoleIds.Any())
                {
                    allOrgRoles.AddRange(await GetOrgRoleRecordByIdsAsync(notYetRetrievedOrgRoleIds.ToArray()));
                }

                foreach (var relationship in currentRelationships)
                {
                    var primaryRole = allOrgRoles.Where(or => or.Recordkey == relationship.RrlsOrgRole).FirstOrDefault();
                    var primaryTitle = primaryRole != null ? primaryRole.OroleTitle : string.Empty;
                    var relatedRole = allOrgRoles.Where(or => or.Recordkey == relationship.RrlsRelatedOrgRole).FirstOrDefault();
                    var relatedTitle = relatedRole != null ? relatedRole.OroleTitle : string.Empty;
                    organizationalPositionRelationships.Add(new OrganizationalPositionRelationship(relationship.Recordkey,
                                                                                                   relationship.RrlsOrgRole,
                                                                                                   primaryTitle,
                                                                                                   relationship.RrlsRelatedOrgRole,
                                                                                                   relatedTitle,
                                                                                                   relationship.RrlsRelationshipCategory));
                }
            }

            return organizationalPositionRelationships;
        }

    }
}
