// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Utility;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CampusOrganizationRepository : BaseColleagueRepository, ICampusOrganizationRepository
    {
        private readonly int bulkReadSize;
        public CampusOrganizationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        public async Task<IEnumerable<CampusOrgAdvisorRole>> GetCampusOrgAdvisorsAsync(IEnumerable<string> advisorIds)
        {
            var advisorCriteria = "WITH CMPA.PERSON.ST.ID EQ '?'";
            var advIds = await DataReader.SelectAsync("CAMPUS.ORG.ADVISORS", advisorCriteria, advisorIds.ToArray());
            List<CampusOrgAdvisors> advisorRecords = new List<CampusOrgAdvisors>();
            if (advIds != null && advIds.Count() > 0)
            {
                for (int i = 0; i < advIds.Count(); i += bulkReadSize)
                {
                    var subList = advIds.Skip(i).Take(bulkReadSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<CampusOrgAdvisors>(subList);
                    if (bulkRecords != null)
                    {
                        advisorRecords.AddRange(bulkRecords);
                    }
                }
            }
            var advisorEntities = new List<CampusOrgAdvisorRole>();
            foreach(var record in advisorRecords)
            {
                foreach (var entityAssociation in record.AdvisorLoadEntityAssociation)
                {
                    CampusOrgAdvisorRole advisor = new CampusOrgAdvisorRole(record.Recordkey, entityAssociation.CmpaRolesAssocMember, entityAssociation.CmpaLoadsAssocMember,
                        entityAssociation.CmpaPacLpAsgmtsAssocMember, entityAssociation.CmpaStartDatesAssocMember, entityAssociation.CmpaEndDatesAssocMember);
                    advisorEntities.Add(advisor);
                }
            }
            return advisorEntities;
        }

        public async Task<IEnumerable<CampusOrgMemberRole>> GetCampusOrgMembersAsync(IEnumerable<string> hrpID)
        {
            var memberCriteria = "WITH CMPM.PERSON.ST.ID EQ '?'";
            var memIds = await DataReader.SelectAsync("CAMPUS.ORG.MEMBERS", memberCriteria, hrpID.ToArray());
            List<CampusOrgMembers> memberRecords = new List<CampusOrgMembers>();
            if (memIds != null && memIds.Count() > 0)
            {
                for (int i = 0; i < memIds.Count(); i += bulkReadSize)
                {
                    var subList = memIds.Skip(i).Take(bulkReadSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<CampusOrgMembers>(subList);
                    if (bulkRecords != null)
                    {
                        memberRecords.AddRange(bulkRecords);
                    }
                }
            }
            var memberEntities = new List<CampusOrgMemberRole>();
            foreach (var record in memberRecords)
            {
                foreach(var entityAssociation in record.CampusOrgRolesEntityAssociation)
                {
                    CampusOrgMemberRole member = new CampusOrgMemberRole(record.Recordkey, entityAssociation.CmpmRolesAssocMember, entityAssociation.CmpmLoadAssocMember,
                        entityAssociation.CmpmRolePacLpAsgmtsAssocMember, entityAssociation.CmpmRoleStartDatesAssocMember, entityAssociation.CmpmRoleEndDatesAssocMember);
                    memberEntities.Add(member);
                }
            }

            return memberEntities;
        }

        #region CampusOrganizations

        /// <summary>
        /// Gets campus organizations
        /// </summary>
        /// <param name="bypassCache">bypass cache</param>
        /// <returns>IEnumerable<CampusOrganization></returns>
        public async Task<IEnumerable<CampusOrganization>> GetCampusOrganizationsAsync(bool bypassCache)
        {
            return await GetGuidCodeItemAsync<CampusOrgs, CampusOrganization>("AllCampusOrganizations", "CAMPUS.ORGS",
                   (campusOrg, g) => new CampusOrganization(campusOrg.Recordkey, g, campusOrg.CmpDesc, campusOrg.CmpCorpId, campusOrg.CmpOrgType), bypassCache: bypassCache);
        }

        #endregion

        #region CampusInvolvements
        /// <summary>
        /// Gets campus involvements
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<CampusInvolvement>, int>> GetCampusInvolvementsAsync(int offset, int limit)
        {
            var campusInlvIds = await DataReader.SelectAsync("CAMPUS.ORG.MEMBERS", "");
            Array.Sort(campusInlvIds);

            var totalCount = campusInlvIds.Count();

            var campusOrgsDataContracts = new List<CampusOrgMembers>();

            var sublist = campusInlvIds.Skip(offset).Take(limit);

            var newCampusInlvIds = sublist.ToArray();

            if (newCampusInlvIds.Any())
            {
                var bulkData = await DataReader.BulkReadRecordAsync<CampusOrgMembers>("CAMPUS.ORG.MEMBERS", newCampusInlvIds);
                campusOrgsDataContracts.AddRange(bulkData);
            }
            var campusInvolvementList = BuildCampusInvolvemments(campusOrgsDataContracts);

            return new Tuple<IEnumerable<CampusInvolvement>, int>(campusInvolvementList, totalCount); 
        }

        /// <summary>
        /// Returns campus involvement
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>CampusInvolvement</returns>
        public async Task<CampusInvolvement> GetGetCampusInvolvementByIdAsync(string id)
        {
            var campusInvId = await GetRecordKeyFromGuidAsync(id);
            var campusOrgsDataContract = await DataReader.ReadRecordAsync<CampusOrgMembers>("CAMPUS.ORG.MEMBERS", campusInvId);
            var campusInvolvementEntity = BuildCampusInvolvemment(campusOrgsDataContract);

            return campusInvolvementEntity;
        }

        /// <summary>
        /// Build campus involvement entites
        /// </summary>
        /// <param name="campusOrgsDataContracts"></param>
        /// <returns></returns>
        private IEnumerable<CampusInvolvement> BuildCampusInvolvemments(IEnumerable<CampusOrgMembers> campusOrgsDataContracts)
        {
            List<CampusInvolvement> campusInvolvementList = new List<CampusInvolvement>();
            foreach (var campusOrgsDataContract in campusOrgsDataContracts)
            {
                CampusInvolvement campusInvolvement = BuildCampusInvolvemment(campusOrgsDataContract);
                campusInvolvementList.Add(campusInvolvement);
            }
            return campusInvolvementList;
        }

        /// <summary>
        /// Build campus involvement entity
        /// </summary>
        /// <param name="campusOrgsDataContract"></param>
        /// <returns></returns>
        private CampusInvolvement BuildCampusInvolvemment(CampusOrgMembers campusOrgsDataContract)
        {
            string[] ids = campusOrgsDataContract.Recordkey.Split('*');
            CampusInvolvement campusInvolvement = new CampusInvolvement(campusOrgsDataContract.RecordGuid, ids[0], ids[1]);

            if (campusOrgsDataContract.CmpmStartDates != null && campusOrgsDataContract.CmpmStartDates.Any())
            {
                campusInvolvement.StartOn = campusOrgsDataContract.CmpmStartDates.Where(dt => dt != null).OrderByDescending(d => d.Value).FirstOrDefault();
            }

            if (campusOrgsDataContract.CmpmEndDates != null && campusOrgsDataContract.CmpmEndDates.Any())
            {
                campusInvolvement.EndOn = campusOrgsDataContract.CmpmEndDates.Where(dt => dt != null).OrderByDescending(d => d.Value).FirstOrDefault();
            }

            campusInvolvement.AcademicPeriodId = string.Empty;

            if (campusOrgsDataContract.CmpmRoles.Any())
            {
                var roleId = campusOrgsDataContract.CmpmRoles.FirstOrDefault();
                if (!string.IsNullOrEmpty(roleId))
                {
                    campusInvolvement.RoleId = roleId;
                }
            }
            return campusInvolvement;
        }

        #endregion
    }
}
