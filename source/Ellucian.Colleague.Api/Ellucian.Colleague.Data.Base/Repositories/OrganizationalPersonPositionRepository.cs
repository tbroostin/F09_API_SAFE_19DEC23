// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrgRole = Ellucian.Data.Colleague.DataContracts.OrgRole;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Accesses Colleague for a person's contact information: Name, address, phones, emails.
    /// </summary>
    [RegisterType]
    public class OrganizationalPersonPositionRepository : BaseColleagueRepository, IOrganizationalPersonPositionRepository
    {
        private readonly int _readSize;
        private readonly string _orgIndicator;

        /// <summary>
        /// Constructor for person profile repository.
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public OrganizationalPersonPositionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            _readSize = apiSettings != null && apiSettings.BulkReadSize > 0 ? apiSettings.BulkReadSize : 5000;
            _orgIndicator = "ORG";
        }

        /// <summary>
        /// Get the position assignments and relevant relationships for the given IDs
        /// </summary>
        /// <param name="ids">Organizational Person Position IDs</param>
        /// <returns>The Collection of Organizational Person Positions with the given IDs</returns>
        public async Task<IEnumerable<OrganizationalPersonPosition>> GetOrganizationalPersonPositionsByIdsAsync(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids", "At least one id is required to get organizational person positions");
            }

            // Org Roles representing person positions
            var orgRoleRecords = await GetOrganizationalRoleRecordsForPersonPositionsAsync();

            //Read all OrgEntityRole (person positions at the UT level). Later will pare down to include only those with allowable ORG.ROLES based on role type
            var orgEntityRoleIds = ids.ToArray();
            List<OrgEntityRolePosition> orgEntityRoleRecords = await GetOrgEntityRoleRecordByIdsAsync(orgEntityRoleIds);

            // Add role relationships for this orgEntityRole's role
            var oerRoles = orgEntityRoleRecords.Select(oer => oer.OerOrgRole);
            var organizationalPositionRelationships = await GetOrganizationalPositionRelationshipsByRolesAsync(oerRoles);
            var positionRelationshipRoles = new List<string>();
            foreach (var organizationalPositionRelationship in organizationalPositionRelationships)
            {
                positionRelationshipRoles.Add(organizationalPositionRelationship.OrganizationalPositionId);
                positionRelationshipRoles.Add(organizationalPositionRelationship.RelatedOrganizationalPositionId);
            }
            var positionRelationshipMembers = await GetOrganizationalPersonPositionBasesInOrganizationalPositionsAsync(positionRelationshipRoles);
            var orgEntityRoleIdsForRoleRelRecords = orgEntityRoleIds.Union(positionRelationshipMembers.Select(prm => prm.Id)).ToArray();

            var orgEntityRoleRelRecords = await GetOrgEntityRoleRelRecordsByOrgEntityRoleIdsAsync(orgEntityRoleIdsForRoleRelRecords);

            // Read ORG.ENTITY.ROLE one more time now that we have the related org entity role ids from the relationships. 
            // Keep these in a separate list since they are not the primary ones we are building.
            var relatedIds = orgEntityRoleRelRecords.Select(oerr => oerr.OerrelRelatedOerId).ToList();
            relatedIds.AddRange(orgEntityRoleRelRecords.Select(oerr => oerr.OerrelOerId));

            List<OrgEntityRolePosition> relatedOrgEntityRoleRecords = await GetOrgEntityRoleRecordByIdsAsync(relatedIds.Distinct().ToArray());


            return BuildOrganizationalPersonPosition(orgRoleRecords, orgEntityRoleRecords, orgEntityRoleRelRecords, relatedOrgEntityRoleRecords, organizationalPositionRelationships, positionRelationshipMembers);
        }

        /// <summary>
        /// Get the position assignments for the specified person and relevant relationships
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<OrganizationalPersonPosition>> GetOrganizationalPersonPositionAsync(IEnumerable<string> personIds, IEnumerable<string> orgPersonPositionIds)
        {
            if (personIds == null && personIds.Count() == 0 && orgPersonPositionIds == null && orgPersonPositionIds.Count() == 0)
            {
                throw new ArgumentException("At least one piece of criteria is required to search for organizational person positions");
            }
            // Get the IDs for the ORG.ENTITY.ROLE with the given Person IDs
            string[] orgEntityRoleIds = new List<string>().ToArray();
            if (personIds != null && personIds.Count() > 0)
            {
                var orgEntityRoleCriteria = "WITH OER.PERSON EQ '?'";
                orgEntityRoleIds = await DataReader.SelectAsync("ORG.ENTITY.ROLE", orgEntityRoleCriteria, personIds.ToArray());
            }
            // Combine the list of person position IDs to create distinct list of person postion ids to get
            var orgPersonPositionIdsToGet = new List<string>();
            if (orgPersonPositionIds != null)
            {
                orgPersonPositionIdsToGet = orgPersonPositionIds.ToList();
            }
            if (orgEntityRoleIds != null)
            {
                orgPersonPositionIdsToGet.AddRange(orgEntityRoleIds.ToList());
            }
            if (orgPersonPositionIdsToGet == null || orgPersonPositionIdsToGet.Count() == 0)
            {
                // No person positions match this criteria
                return new List<OrganizationalPersonPosition>();
            }
            return await GetOrganizationalPersonPositionsByIdsAsync(orgPersonPositionIdsToGet.Distinct());
        }

        /// <summary>
        /// Retrieves the person position assignments for all persons in the given positions.
        /// </summary>
        /// <param name="organizationalPositionIds">Organizational positions for which to get person position assignments</param>
        /// <returns>The person positions assigned to the given position</returns>
        private async Task<IEnumerable<OrganizationalPersonPositionBase>> GetOrganizationalPersonPositionBasesInOrganizationalPositionsAsync(IEnumerable<string> organizationalPositionIds)
        {
            var organizationalPersonPositionBases = new List<OrganizationalPersonPositionBase>();

            if (organizationalPositionIds != null && organizationalPositionIds.Any())
            {
                var orgEntityRoleFile = "ORG.ENTITY.ROLE";
                var orgEntityRoleCriteria = "WITH OER.ORG.ROLE EQ '?'";
                var orgEntityRoleIds = await DataReader.SelectAsync(orgEntityRoleFile, orgEntityRoleCriteria, organizationalPositionIds.ToArray());
                if (orgEntityRoleIds != null)
                {
                    var orgPerPosBases = await GetOrganizationalPersonPositionBasesByIdsAsync(orgEntityRoleIds);
                    if (orgPerPosBases != null) {
                        organizationalPersonPositionBases.AddRange(orgPerPosBases );
                    }
                }
            }

            return organizationalPersonPositionBases;
        }

        /// <summary>
        /// Constructs a collection of OrganizationalPersonPosition objects from records and relationships
        /// </summary>
        /// <param name="orgRoleRecords">Org.Role records</param>
        /// <param name="orgEntityRoleRecords">Org.Entity.Role records, one for each OrganizationalPersonPosition to build</param>
        /// <param name="orgEntityRoleRelRecords">Org.Entity.Role.Rel records</param>
        /// <param name="relatedOrgEntityRoleRecords">Org.Entity.Role records for related entities</param>
        /// <param name="organizationalPositionRelationships">Organizational position relationships</param>
        /// <param name="positionRelationshipMembers">Members of roles in organizational position relationships</param>
        /// <returns>An organizational person position</returns>
        private IEnumerable<OrganizationalPersonPosition> BuildOrganizationalPersonPosition(IEnumerable<OrgRole> orgRoleRecords, IEnumerable<OrgEntityRolePosition> orgEntityRoleRecords, IEnumerable<OrgEntityRoleRel> orgEntityRoleRelRecords, IEnumerable<OrgEntityRolePosition> relatedOrgEntityRoleRecords, IEnumerable<OrganizationalPositionRelationship> organizationalPositionRelationships, IEnumerable<OrganizationalPersonPositionBase> positionRelationshipMembers)
        {
            var orgPersonPositions = new List<OrganizationalPersonPosition>();
            // Populate position relationships with members
            foreach (var positionRelationship in organizationalPositionRelationships)
            {
                // Only add subordinate members if they do not have an overriding relationship
                positionRelationship.AddOrganizationalPersonPositions(
                    positionRelationshipMembers.Where(
                        oppb => oppb.PositionId == positionRelationship.OrganizationalPositionId
                                && oppb.Status == OrganizationalPersonPositionStatus.Current
                                && !orgEntityRoleRelRecords.Any(oerr => oerr.OerrelRelationshipCategory == positionRelationship.RelationshipCategory
                                                                        && oerr.OerrelOerId == oppb.Id)
                    )
                );
                positionRelationship.AddRelatedOrganizationalPersonPositions(
                    positionRelationshipMembers.Where(
                        oppb => oppb.PositionId == positionRelationship.RelatedOrganizationalPositionId
                                && oppb.Status == OrganizationalPersonPositionStatus.Current
                    )
                );
            }

            // Create a dictionary of the organizational roles, selected using valid role types
            var orgRoleRecordDict = orgRoleRecords.ToDictionary(r => r.Recordkey, r => r);
            // Process the selected orgEntityRole
            foreach (var orgEntityRoleRecord in orgEntityRoleRecords)
            {
                if (orgRoleRecordDict.ContainsKey(orgEntityRoleRecord.OerOrgRole))
                {
                    // Create the organizational person position
                    var orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleRecord.Recordkey, orgEntityRoleRecord.OerPerson, orgEntityRoleRecord.OerOrgRole, orgRoleRecordDict[orgEntityRoleRecord.OerOrgRole].OroleTitle, orgEntityRoleRecord.OerStartDate, orgEntityRoleRecord.OerEndDate);
                    // Add any relationships where this orgEntityRole is a participant
                    var orgEntityRoleRelationships = orgEntityRoleRelRecords
                        .Where(orrr => orrr.OerrelOerId == orgEntityRoleRecord.Recordkey || orrr.OerrelRelatedOerId == orgEntityRoleRecord.Recordkey)
                        .ToList();
                    if (orgEntityRoleRelationships != null)
                    {
                        foreach (var relation in orgEntityRoleRelationships)
                        {
                            try
                            {
                                var primaryOrgEntityRoleRecord = relatedOrgEntityRoleRecords.Where(roerr => roerr.Recordkey == relation.OerrelOerId).First();
                                var relatedOrgEntityRoleRecord = relatedOrgEntityRoleRecords.Where(roerr => roerr.Recordkey == relation.OerrelRelatedOerId).First();
                                if (orgRoleRecordDict.ContainsKey(relatedOrgEntityRoleRecord.OerOrgRole))
                                {
                                    if (orgRoleRecordDict.ContainsKey(primaryOrgEntityRoleRecord.OerOrgRole))
                                    {
                                        var relationshipToAdd = new OrganizationalRelationship(
                                            relation.Recordkey,
                                            relation.OerrelOerId,
                                            primaryOrgEntityRoleRecord.OerPerson,
                                            primaryOrgEntityRoleRecord.OerOrgRole,
                                            orgRoleRecordDict[primaryOrgEntityRoleRecord.OerOrgRole].OroleTitle,
                                            primaryOrgEntityRoleRecord.OerStartDate,
                                            primaryOrgEntityRoleRecord.OerEndDate,
                                            relation.OerrelRelatedOerId,
                                            relatedOrgEntityRoleRecord.OerPerson,
                                            relatedOrgEntityRoleRecord.OerOrgRole,
                                            orgRoleRecordDict[relatedOrgEntityRoleRecord.OerOrgRole].OroleTitle,
                                            relatedOrgEntityRoleRecord.OerStartDate,
                                            relatedOrgEntityRoleRecord.OerEndDate,
                                            relation.OerrelRelationshipCategory
                                            );
                                        orgPersonPosition.AddRelationship(relationshipToAdd);
                                    }
                                    else
                                    {
                                        LogDataError("ORG.ENTITY.ROLE.REL", relation.Recordkey, relation, null, "Could not build organizational relationship " + relation.Recordkey + " with person " + primaryOrgEntityRoleRecord.OerPerson + " because primary role " + primaryOrgEntityRoleRecord.OerOrgRole + " does not have special processing code ORG.");
                                    }
                                }
                                else
                                {
                                    LogDataError("ORG.ENTITY.ROLE.REL", relation.Recordkey, relation, null, "Could not build organizational relationship " + relation.Recordkey + " with person " + relatedOrgEntityRoleRecord.OerPerson + " because related role " + relatedOrgEntityRoleRecord.OerOrgRole + " does not have special processing code ORG.");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogDataError("ORG.ENTITY.ROLE.REL", relation.Recordkey, relation, ex, "Could not build organizational relationship " + relation.Recordkey + " for person " + orgEntityRoleRecord.OerPerson + ".");
                            }
                        }
                        orgPersonPositions.Add(orgPersonPosition);
                    }

                    // Add position relationships if it relates to this person's current position
                    if (orgPersonPosition.Status == OrganizationalPersonPositionStatus.Current)
                    {
                        var relevantOrganizationalPositionRelationships = organizationalPositionRelationships.Where(opr => opr.OrganizationalPositionId == orgPersonPosition.PositionId || opr.RelatedOrganizationalPositionId == orgPersonPosition.PositionId);
                        foreach (var orgPositionRelationship in relevantOrganizationalPositionRelationships)
                        {
                            orgPersonPosition.AddPositionRelationship(orgPositionRelationship);
                        }
                    }

                }
            }
            return orgPersonPositions.AsEnumerable();
        }

        /// <summary>
        /// Gets the organizational position relationships associated with the given roles
        /// </summary>
        /// <param name="roles">Roles to retrieve position (role) relationships for</param>
        /// <returns>Collection of Organizational Position Relationships</returns>
        private async Task<IEnumerable<OrganizationalPositionRelationship>> GetOrganizationalPositionRelationshipsByRolesAsync(IEnumerable<string> roles)
        {
            var organizationalPositionRelationships = new List<OrganizationalPositionRelationship>();
            if (roles != null && roles.Any())
            {
                var roleRelationshipsFile = "ROLE.RELATIONSHIPS";
                // Get all Supervisor-Subordinate relationships, ignoring peer relationships
                var primaryCriteria = "RRLS.TYPE EQ 'S' AND RRLS.ORG.ROLE EQ '?'";
                var secondaryCriteria = "RRLS.TYPE EQ 'S' AND RRLS.RELATED.ORG.ROLE EQ '?'";
                var rolesArray = roles.ToArray();
                var primaryIds = await DataReader.SelectAsync(roleRelationshipsFile, primaryCriteria, rolesArray);
                var secondaryIds = await DataReader.SelectAsync(roleRelationshipsFile, secondaryCriteria, rolesArray);
                var primaryRoleRelationships = await DataReader.BulkReadRecordAsync<RoleRelationships>(primaryIds);
                var secondaryRoleRelationships = await DataReader.BulkReadRecordAsync<RoleRelationships>(secondaryIds);
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
                var currentRelationships = allRelationships.Where(rr => rr.RrlsStartDate.HasValue
                                                                        && rr.RrlsStartDate.Value <= DateTime.Today
                                                                        && (!rr.RrlsEndDate.HasValue || rr.RrlsEndDate >= DateTime.Today));
                foreach (var relationship in currentRelationships)
                {
                    organizationalPositionRelationships.Add(new OrganizationalPositionRelationship(relationship.Recordkey,
                                                                                                   relationship.RrlsOrgRole,
                                                                                                   "",
                                                                                                   relationship.RrlsRelatedOrgRole,
                                                                                                   "",
                                                                                                   relationship.RrlsRelationshipCategory));
                }
            }

            return organizationalPositionRelationships;
        }

        /// <summary>
        /// Return only the list of roles types relevant to Organizational Members
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<string>> GetOrganizationalRoleTypes()
        {
            var roleTypes = await GetOrAddToCacheAsync<ApplValcodes>("OrganizationalRoleTypes",
                async () =>
                {
                    ApplValcodes roleTypesTable = await DataReader.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "ORG.ROLE.TYPES");
                    if (roleTypesTable == null)
                    {
                        var errorMessage = "Unable to access ORG.ROLE.TYPES valcode table.";
                        logger.Error(errorMessage);
                        throw new Exception(errorMessage);
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
        /// Gets the OrgRole records that represent person positions
        /// </summary>
        /// <returns>OrgRole records representing person positions</returns>
        private async Task<IEnumerable<OrgRole>> GetOrganizationalRoleRecordsForPersonPositionsAsync()
        {

            //Read async org role types
            var roleTypes = await GetOrganizationalRoleTypes();

            //Read roles with any "ORG" role type
            var criteria = "WITH OROLE.TYPE EQ '?'";
            var selectedIds = await DataReader.SelectAsync("ORG.ROLE", criteria, roleTypes.ToArray());
            List<OrgRole> orgRoleRecords = new List<OrgRole>();
            if (selectedIds != null && selectedIds.Count() > 0)
            {
                for (int i = 0; i < selectedIds.Count(); i += _readSize)
                {
                    var subList = selectedIds.Skip(i).Take(_readSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<OrgRole>(subList);
                    if (bulkRecords != null)
                    {
                        orgRoleRecords.AddRange(bulkRecords);
                    }
                }
            }
            return orgRoleRecords;
        }

        /// <summary>
        /// Gets OrgEntityRoleRel records by OrgEntityRole ID on either side of the relationship.
        /// </summary>
        /// <param name="orgEntityRoleIds">OrgEntityRole IDs to retrieve relationships for</param>
        /// <returns>OrgEntityRoleRel records with the given OrgEntityRole IDs</returns>
        private async Task<IEnumerable<OrgEntityRoleRel>> GetOrgEntityRoleRelRecordsByOrgEntityRoleIdsAsync(string[] orgEntityRoleIds)
        {
            //read async OrgEntityRoleRel (org entity role relationships for primary relationship participant)
            var criteria = "WITH OERREL.OER.ID EQ '?'";
            var selectedPrimaryIds = new List<string>().ToArray();
            selectedPrimaryIds = await DataReader.SelectAsync("ORG.ENTITY.ROLE.REL", criteria, orgEntityRoleIds);
            var relatedCriteria = "WITH OERREL.RELATED.OER.ID EQ '?'";
            var selectedRelatedIds = new List<string>().ToArray();
            selectedRelatedIds = await DataReader.SelectAsync("ORG.ENTITY.ROLE.REL", relatedCriteria, orgEntityRoleIds);
            var combinedIds = selectedPrimaryIds.Concat(selectedRelatedIds).Distinct();
            List<OrgEntityRoleRel> orgEntityRoleRelRecords = new List<OrgEntityRoleRel>();
            if (combinedIds != null && combinedIds.Count() > 0)
            {
                for (int i = 0; i < combinedIds.Count(); i += _readSize)
                {
                    var subList = combinedIds.Skip(i).Take(_readSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<OrgEntityRoleRel>(subList);
                    if (bulkRecords != null)
                    {
                        orgEntityRoleRelRecords.AddRange(bulkRecords);
                    }
                }
            }

            return orgEntityRoleRelRecords;

        }

        /// <summary>
        /// Get OrgEntityRole records by IDs
        /// </summary>
        /// <param name="ids">OrgEntityRole record keys</param>
        /// <returns>Last of OrgEntityRole records matching the IDs given</returns>
        private async Task<List<OrgEntityRolePosition>> GetOrgEntityRoleRecordByIdsAsync(string[] ids)
        {
            List<OrgEntityRolePosition> relatedOrgEntityRoleRecords = new List<OrgEntityRolePosition>();
            if (ids != null && ids.Count() > 0)
            {
                for (int i = 0; i < ids.Count(); i += _readSize)
                {
                    var subList = ids.Skip(i).Take(_readSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<OrgEntityRolePosition>(subList);
                    if (bulkRecords != null)
                    {
                        relatedOrgEntityRoleRecords.AddRange(bulkRecords);
                    }
                }
            }

            return relatedOrgEntityRoleRecords;
        }

        /// <summary>
        /// Gets the organizational person position bases for the given ids
        /// </summary>
        /// <param name="ids">Person position ids</param>
        /// <returns>Collection of organizational person position base objects</returns>
        private async Task<IEnumerable<OrganizationalPersonPositionBase>> GetOrganizationalPersonPositionBasesByIdsAsync(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids", "At least one id is required to get organizational person positions");
            }

            // Org Roles representing person positions
            var orgRoleRecords = await GetOrganizationalRoleRecordsForPersonPositionsAsync();

            //Read all OrgEntityRole (person positions at the UT level). Later will pare down to include only those with allowable ORG.ROLES based on role type
            var orgEntityRoleIds = ids.ToArray();
            List<OrgEntityRolePosition> orgEntityRoleRecords = await GetOrgEntityRoleRecordByIdsAsync(orgEntityRoleIds);

            return BuildOrganizationalPersonPositionBases(orgRoleRecords, orgEntityRoleRecords);
        }

        /// <summary>
        /// Constructs a collection of OrganizationalPersonPositionBase objects from records
        /// </summary>
        /// <param name="orgRoleRecords">Org.Role records</param>
        /// <param name="orgEntityRoleRecords">Org.Entity.Role records, each representing an OrganizationalPersonPositionBase to build</param>
        /// <returns>Collection of OrganizationalPersonPositionBase objects </returns>
        private IEnumerable<OrganizationalPersonPositionBase> BuildOrganizationalPersonPositionBases(
            IEnumerable<OrgRole> orgRoleRecords,
            IEnumerable<OrgEntityRolePosition> orgEntityRoleRecords)
        {
            var orgPersonPositions = new List<OrganizationalPersonPositionBase>();
            // Create a dictionary of the organizational roles, selected using valid role types
            var orgRoleRecordDict = orgRoleRecords.ToDictionary(r => r.Recordkey, r => r);
            // Process the selected orgEntityRole
            foreach (var orgEntityRoleRecord in orgEntityRoleRecords)
            {
                if (orgRoleRecordDict.ContainsKey(orgEntityRoleRecord.OerOrgRole))
                {
                    // Create the organizational person position
                    var orgPersonPosition = new OrganizationalPersonPositionBase(orgEntityRoleRecord.Recordkey, orgEntityRoleRecord.OerPerson, orgEntityRoleRecord.OerOrgRole, orgRoleRecordDict[orgEntityRoleRecord.OerOrgRole].OroleTitle, orgEntityRoleRecord.OerStartDate, orgEntityRoleRecord.OerEndDate);
                    orgPersonPositions.Add(orgPersonPosition);
                }
            }
            return orgPersonPositions;
        }

    }
}
