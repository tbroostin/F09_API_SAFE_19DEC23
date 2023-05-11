// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CampusOrganizationRepository : BaseColleagueRepository, ICampusOrganizationRepository
    {
        private readonly int bulkReadSize;
        const string AllCampusInvolvementsCache = "AllCampusInvolvements";
        const int AllCampusInvolvementsCacheTimeout = 20;

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
            int totalCount = 0;
            string campusInvolvementsCacheKey = CacheSupport.BuildCacheKey(AllCampusInvolvementsCache);
            string[] subList = null;

            var campusInvolvementEntities = new List<Domain.Student.Entities.CampusInvolvement>();

            var repositoryException = new RepositoryException();

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                campusInvolvementsCacheKey,
                "",
                offset,
                limit,
                AllCampusInvolvementsCacheTimeout,

                async () =>
                {
                    var campusOrgMemberIds = await DataReader.SelectAsync("CAMPUS.ORG.MEMBERS", null);
                    if (campusOrgMemberIds == null || !campusOrgMemberIds.Any())
                    {
                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                    }
                    var requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = campusOrgMemberIds.Distinct().ToList(),
                    };

                    return requirements;
                });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.CampusInvolvement>, int>(new List<Domain.Student.Entities.CampusInvolvement>(), 0);
            }
            subList = keyCache.Sublist.ToArray();

            totalCount = keyCache.TotalCount.Value;
            var results = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.CampusOrgMembers>("CAMPUS.ORG.MEMBERS", subList);

            if (results.Equals(default(BulkReadOutput<DataContracts.CampusOrgMembers>)))
                return new Tuple<IEnumerable<Domain.Student.Entities.CampusInvolvement>, int>(new List<Domain.Student.Entities.CampusInvolvement>(), 0);
           
            if (results.InvalidKeys.Any() || results.InvalidRecords.Any())
            {
                if (results.InvalidKeys.Any())
                {
                    repositoryException.AddErrors(results.InvalidKeys
                        .Select(key => new RepositoryError("invalid.key",
                        string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                }
                if (results.InvalidRecords.Any())
                {
                    repositoryException.AddErrors(results.InvalidRecords
                       .Select(r => new RepositoryError("invalid.record",
                       string.Format("Error: '{0}'. Entity: 'CAMPUS.ORG.MEMBERS', Record ID: '{1}' ", r.Value, r.Key))
                       { }));
                }
                throw repositoryException;
            }
            try
            {
                campusInvolvementEntities = BuildCampusInvolvemments(results.BulkRecordsRead.ToList()).ToList();

            }
            catch (RepositoryException ex)
            {
                repositoryException.AddErrors(ex.Errors);
            }
            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return new Tuple<IEnumerable<Domain.Student.Entities.CampusInvolvement>, int>(campusInvolvementEntities, totalCount);
        }

        /// <summary>
        /// Returns campus involvement
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>CampusInvolvement</returns>
        public async Task<CampusInvolvement> GetGetCampusInvolvementByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var errorMessage = "ID is required to get a campus involvement.";
                throw new ArgumentException(errorMessage);
            }
            var campusInvId = await GetRecordKeyFromGuidAsync(id);
            if (string.IsNullOrEmpty(campusInvId))
            {
                var errorMessage = string.Format("Campus Org Members record with ID : '{0}' is not a valid campus involvement.", id);
                throw new KeyNotFoundException(errorMessage);
            }
            var campusOrgsDataContract = await DataReader.ReadRecordAsync<CampusOrgMembers>("CAMPUS.ORG.MEMBERS", campusInvId);
            if (campusOrgsDataContract == null)
            {
                throw new KeyNotFoundException("Invalid Campus Org Member ID: " + campusInvId);}
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
            var repositoryException = new RepositoryException();
            try
            {
                string[] ids = campusOrgsDataContract.Recordkey.Split('*');
                if (string.IsNullOrEmpty(ids[0]))
                {
                    repositoryException.AddError(new RepositoryError("Bad.Data", "Missing Person ID")
                    {
                        SourceId = campusOrgsDataContract != null ? campusOrgsDataContract.Recordkey : "",
                        Id = campusOrgsDataContract != null ? campusOrgsDataContract.RecordGuid : ""
                    });
                }
                if (string.IsNullOrEmpty(ids[1]))
                {
                    repositoryException.AddError(new RepositoryError("Bad.Data", "Missing Campus Organization ID")
                    {
                        SourceId = campusOrgsDataContract != null ? campusOrgsDataContract.Recordkey : "",
                        Id = campusOrgsDataContract != null ? campusOrgsDataContract.RecordGuid : ""
                    });
                }
                if (string.IsNullOrEmpty(campusOrgsDataContract.RecordGuid))
                {
                    repositoryException.AddError(new RepositoryError("Bad.Data", "Missing Campus Involvements GUID, Entity: 'CAMPUS.ORG.MEMBERS', Record ID: '" + campusOrgsDataContract.Recordkey + "'")
                    {
                        SourceId = campusOrgsDataContract != null ? campusOrgsDataContract.Recordkey : ""
                    });
                }
                // check for required fields, otherwise the domain entity constructor will throw errors.
                if (repositoryException.Errors.Any())
                {
                    throw repositoryException;
                }
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
            catch (Exception ex)
            {
                repositoryException.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    SourceId = campusOrgsDataContract != null ? campusOrgsDataContract.Recordkey : "",
                    Id = campusOrgsDataContract != null ? campusOrgsDataContract.RecordGuid : ""
                });
                throw repositoryException;
            }
        }

        #endregion

        /// <summary>
        /// Using a collection of  ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup(filename, p, false)).ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }
    }
}
