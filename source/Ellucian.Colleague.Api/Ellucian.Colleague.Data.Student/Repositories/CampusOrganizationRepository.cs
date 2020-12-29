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


        /// <summary>
        /// Gets CampusOrganization2 objects matching the campusOrgIds.
        /// </summary>
        /// <param name="campusOrgIds">List of campus organization ids</param>
        /// <returns>IEnumerable<CampusOrganization2></returns>
        public async Task<IEnumerable<CampusOrganization2>> GetCampusOrganizations2Async(List<string> campusOrgIds)
        {
            if (campusOrgIds == null)
            {
                throw new ArgumentNullException("campusOrgIds");
            }
            if (!campusOrgIds.Any())
            {
                throw new ArgumentException("campusOrgIds are required to get CampusOrganization2 objects");
            }

            // Select all the CampusOrgs with CAMPUS.ORGS.ID equal to the input campusOrgIds
            var criteria = "WITH CAMPUS.ORGS.ID EQ ?";
            var campusOrgKeys = await DataReader.SelectAsync("CAMPUS.ORGS", criteria, campusOrgIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
            if (campusOrgKeys == null)
            {
                var message = "Unexpected null returned from CAMPUS.ORGS SelectAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!campusOrgKeys.Any())
            {
                var message = "No CAMPUS.ORGS keys exist for the given campusOrgIds ";
                logger.Error(message + string.Join(",", campusOrgIds));
                throw new KeyNotFoundException(message);
            }

            //bulkread the records in chunks for all the keys
            var campusOrgsRecords = new List<CampusOrgs>();
            for (int i = 0; i < campusOrgKeys.Count(); i += bulkReadSize)
            {
                var subList = campusOrgKeys.Skip(i).Take(bulkReadSize);
                var selectedRecords = await DataReader.BulkReadRecordAsync<CampusOrgs>(subList.ToArray());
                if (selectedRecords == null)
                {
                    logger.Error("Unexpected: Null returned from bulk read of CampusOrgs records");
                }
                else
                {
                    campusOrgsRecords.AddRange(selectedRecords);
                }
            }

            if (!campusOrgsRecords.Any())
            {
                var message = "No CampusOrganization2 records available for given campusOrgIds";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var campusOrganization2Entities = new List<CampusOrganization2>();

            foreach (var campusOrgsRecord in campusOrgsRecords)
            {
                if (campusOrgsRecord != null)
                {
                    try
                    {
                        // Build the CampusOrganization2 object
                        campusOrganization2Entities.Add(BuildCampusOrganization2Item(campusOrgsRecord));
                    }
                    catch (Exception e)
                    {
                        LogDataError("CampusOrgs", campusOrgsRecord.Recordkey, campusOrgsRecord, e, e.Message);
                    }
                }
            }

            return campusOrganization2Entities;
        }

        /// <summary>
        /// Helper to build a CampusOrganization2 object based on a CampusOrgs record
        /// </summary>
        /// <param name="campusOrgsRecord"></param>
        private CampusOrganization2 BuildCampusOrganization2Item(CampusOrgs campusOrgsRecord)
        {
            if (campusOrgsRecord == null)
            {
                throw new ArgumentNullException("campusOrgsRecord");
            }

            var campusOrganization2Object = new CampusOrganization2(campusOrgsRecord.Recordkey, campusOrgsRecord.CmpDesc);

            return campusOrganization2Object;
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
