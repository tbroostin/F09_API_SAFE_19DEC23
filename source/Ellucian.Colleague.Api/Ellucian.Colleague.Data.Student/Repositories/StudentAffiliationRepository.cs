// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Data.Base.DataContracts;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAffiliationRepository : BaseColleagueRepository, IStudentAffiliationRepository
    {
        private ApplValcodes MemberStatuses;
        private IEnumerable<CampusOrgRole> RolesTable;
        private IEnumerable<CampusOrgType> OrgTypesTable;

        public StudentAffiliationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region Validation Tables
        private async Task<ApplValcodes> GetMemberStatusesAsync()
        {
            if (MemberStatuses != null)
            {
                return MemberStatuses;
            }

            MemberStatuses = await GetOrAddToCacheAsync<ApplValcodes>("CampusOrgMemberStatuses",
               async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "CAMPUS.ORG.MEMBER.STATUSES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access CAMPUS.ORG.MEMBER.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return MemberStatuses;
        }
        /// <summary>
        /// Get ROLES table from Colleague
        /// </summary>
        /// <returns>Roles Validation Table</returns>
        private async Task<IEnumerable<CampusOrgRole>> GetOrgRolesAsync()
        {
            if (RolesTable != null)
            {
                return RolesTable;
            }

            RolesTable = await GetCodeItemAsync<Roles, CampusOrgRole>("AllRoles", "ROLES",
                r => new CampusOrgRole(r.Recordkey, r.RolesDesc, r.RolesPilotPriority));
            if (RolesTable == null)
            {
                var errorMessage = "Unable to access ROLES code table.";
                logger.Info(errorMessage);
            }
            return RolesTable;
        }

        /// <summary>
        /// Get Organization Type for translate of code to description
        /// </summary>
        /// <returns>Organization Type Validation table</returns>
        private async Task<IEnumerable<CampusOrgType>> GetOrgTypesAsync()
        {
            if (OrgTypesTable != null)
            {
                return OrgTypesTable;
            }

            OrgTypesTable =await GetCodeItemAsync<OrgTypes, CampusOrgType>("AllOrgTypes", "ORG.TYPES",
                r => new CampusOrgType(r.Recordkey, r.OrgtDesc, (r.OrgtPilotFlag == "Y" ? true : false)));
            if (OrgTypesTable == null)
            {
                var errorMessage = "Unable to access ORG.TYPES code table.";
                logger.Info(errorMessage);
                throw new Exception(errorMessage);
            }
            return OrgTypesTable;
        }
        #endregion

        /// <summary>
        /// Get Student Affiliations for a list of Students.
        /// </summary>
        /// <param name="studentIds">List of Student IDs</param>
        /// <param name="termId">Restrict selection to specific terms</param>
        /// <param name="affiliationId">Restrict selection to specific Affiliations</param>
        /// <returns>Dictionary containing Student IDs and StudentAffiliation entity objects</returns>    
        public async Task<IEnumerable<StudentAffiliation>> GetStudentAffiliationsByStudentIdsAsync(IEnumerable<string> studentIds, Term termData, string affiliationId = null)
        {
            List<StudentAffiliation> studentAffiliations = new List<StudentAffiliation>();
            bool error = false;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // Get PERSON.ST data
                Collection<PersonSt> personStData = await DataReader.BulkReadRecordAsync<PersonSt>(studentIds.ToArray());

                if (personStData == null || personStData.Count <= 0)
                {
                    logger.Error(string.Format("No PERSON.ST data returned for students : {0}", string.Join(",", studentIds)));
                }
                else
                {
                    // Get Campus Org Data
                    string[] campusOrgIds = personStData.SelectMany(p => p.PstCampusOrgsMember).ToArray();
                    Collection<CampusOrgs> campusOrgData = await DataReader.BulkReadRecordAsync<CampusOrgs>(campusOrgIds);

                    List<string> campusOrgMembersIds = new List<string>();
                    foreach (var personSt in personStData)
                    {
                        try
                        {
                            foreach (var orgId in personSt.PstCampusOrgsMember)
                            {
                                if ((string.IsNullOrEmpty(affiliationId) || affiliationId == orgId) && !string.IsNullOrEmpty(orgId))
                                {
                                    // Only get keys for Campus Organization where the Pilot Flag is set
                                    var campusOrg = campusOrgData.Where(c => c.Recordkey == orgId).FirstOrDefault();
                                    //var orgType =  GetOrgTypes().Where(o => o.Code == campusOrg.CmpOrgType).FirstOrDefault();
                                    var orgType =(await GetOrgTypesAsync()).Where(o => o.Code == campusOrg.CmpOrgType).FirstOrDefault();
                                    if (orgType != null && orgType.PilotFlag)
                                    {
                                        campusOrgMembersIds.Add(orgId + "*" + personSt.Recordkey);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Failed to retrieve campus org membership for person : {0}", personSt.Recordkey));
                            logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                    if (campusOrgMembersIds.Count > 0)
                    {
                        Collection<CampusOrgMembers> campusOrgMembersData = await DataReader.BulkReadRecordAsync<CampusOrgMembers>(campusOrgMembersIds.ToArray());
                        foreach (var campusOrgMemberData in campusOrgMembersData)
                        {
                            try
                            {
                                var studentAffiliation = await BuildAffiliationAsync(campusOrgMemberData, campusOrgData, termData);
                                if (studentAffiliation != null)
                                {
                                    studentAffiliations.Add(studentAffiliation);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error(string.Format("Could not build student affiliation for campus org member : {0}", campusOrgMemberData.Recordkey));
                                logger.Error(e.GetBaseException().Message);
                                logger.Error(e.GetBaseException().StackTrace);
                                error = true;
                            }
                        }
                    }
                }
            }
            if (error && studentAffiliations.Count() == 0)
                throw new Exception("Unexpected errors occurred. No student affiiation records returned. Check API error log.");

            return studentAffiliations;
        }

        public async Task<StudentAffiliation> BuildAffiliationAsync(CampusOrgMembers campusOrgMemberData, Collection<CampusOrgs> campusOrgData, Term termData)
        {
            StudentAffiliation studentAffiliation = null;
            string affiliationId = campusOrgMemberData.Recordkey.Split('*')[0];
            string studentId = campusOrgMemberData.Recordkey.Split('*')[1];

            if (campusOrgMemberData.CampusOrgRolesEntityAssociation != null && campusOrgMemberData.CampusOrgRolesEntityAssociation.Count > 0)
            {
                CampusOrgMembersCampusOrgRoles roleToUse = new CampusOrgMembersCampusOrgRoles();

                // When a person has more than one role in an organization, 
                // take the one with the highest priority
                foreach (var roleData in campusOrgMemberData.CampusOrgRolesEntityAssociation)
                {                    
                    var newRole = roleData.CmpmRolesAssocMember;
                    var newRoleStartDate = roleData.CmpmRoleStartDatesAssocMember;
                    var newRoleEndDate = roleData.CmpmRoleEndDatesAssocMember;

                    var newRolePriority =(await GetOrgRolesAsync()).Where(r => r.Code == newRole).Select(r => r.PilotPriority).FirstOrDefault();

                    Int32 newRolePriorityNumeric;
                    if (!string.IsNullOrEmpty(newRolePriority))
                    {
                        if (Int32.TryParse(newRolePriority, out newRolePriorityNumeric))
                        {
                            var oldRole = roleToUse.CmpmRolesAssocMember;
                            var oldRolePriority =(await GetOrgRolesAsync()).Where(r => r.Code == oldRole).Select(r => r.PilotPriority).FirstOrDefault();
                            int newPriority = 9999;
                            int oldPriority = 9999;
                            newPriority = newRolePriorityNumeric;

                            Int32 oldRolePriorityNumeric;
                            if (!string.IsNullOrEmpty(oldRolePriority))
                            {
                                if (Int32.TryParse(oldRolePriority, out oldRolePriorityNumeric))
                                {
                                    oldPriority = oldRolePriorityNumeric;
                                }
                                else
                                {
                                    var errorMessage = "Pilot priority value " + oldRolePriority + " for role " + oldRole + " must be numeric.";
                                    logger.Error(errorMessage);
                                }
                            }
                            if (newPriority < oldPriority)
                            {
                                // Need to make sure role with higher priority (lower priority value)
                                // is valid for given term
                                if (termData != null)
                                {
                                    var termStartDate = termData.StartDate;
                                    var termEndDate = termData.EndDate;
                                    //
                                    // valid role if any of the following are true to indicate role dates intersect with
                                    // term dates:
                                    // - role start date is within term start and end
                                    // - role end date exists and term start is within role start and end
                                    // - role end date does not exist and term starts after role starts
                                    //
                                    if ((termStartDate.CompareTo(newRoleStartDate) <= 0 && termEndDate.CompareTo(newRoleStartDate) >= 0) ||
                                    (termStartDate.CompareTo(newRoleStartDate) >= 0 && (newRoleEndDate != null && termStartDate.CompareTo(newRoleEndDate) <= 0)) ||
                                    (termStartDate.CompareTo(newRoleStartDate) >= 0 && (newRoleEndDate == null)))
                                    {
                                        roleToUse = roleData;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var errorMessage = "Pilot priority value " + newRolePriority + " for role " + newRole + " must be numeric.";
                            logger.Error(errorMessage);
                        }
                    }
                }
                if (roleToUse != null && !string.IsNullOrEmpty(roleToUse.CmpmRolesAssocMember))
                {
                    var role = roleToUse.CmpmRolesAssocMember;
                    var startDate = roleToUse.CmpmRoleStartDatesAssocMember;
                    var endDate = roleToUse.CmpmRoleEndDatesAssocMember;
                    var status = roleToUse.CmpmRoleStatusesAssocMember;
                    studentAffiliation = new StudentAffiliation(studentId, affiliationId);
                    studentAffiliation.AffiliationName = campusOrgData.Where(c => c.Recordkey == affiliationId).Select(a => a.CmpDesc).FirstOrDefault();
                    studentAffiliation.RoleCode = role;
                    studentAffiliation.RoleName = (await GetOrgRolesAsync()).Where(r => r.Code == role).Select(r => r.Description).FirstOrDefault();
                    studentAffiliation.StartDate = startDate;
                    studentAffiliation.EndDate = endDate;
                    studentAffiliation.StatusCode = status;
                    var codeAssoc = (await GetMemberStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == status).FirstOrDefault();
                    if (codeAssoc != null)
                    {
                        studentAffiliation.StatusName = codeAssoc.ValExternalRepresentationAssocMember;
                    }
                }
            }
            return studentAffiliation;
        }
    }
}
