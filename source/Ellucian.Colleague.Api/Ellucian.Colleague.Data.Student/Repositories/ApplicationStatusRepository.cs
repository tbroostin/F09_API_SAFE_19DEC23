// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
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

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Applicant Repository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ApplicationStatusRepository : BaseColleagueRepository, IApplicationStatusRepository
    {
        private readonly string colleagueTimeZone;
        private int bulkReadSize;
        const string AllApplicationStatusCacheKey = "AllApplicationStatusKeys";
        const int AllApplicationStatusCacheTimeout = 20;

        public ApplicationStatusRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
            bulkReadSize = apiSettings.BulkReadSize;
        }

        /// <summary>
        /// Gets all admission statusess.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>> GetApplicationStatusesAsync(int offset, int limit, string applicationId,
            string[] filterPersonIds = null, DateTimeOffset? decidedOn = null, Dictionary<string, string> filterQualifiers = null, bool bypassCache = false)
        {
            int totalCount = 0;

            var exception = new RepositoryException();

            var selectionCriteria = new StringBuilder();

            var dateFilterOperation = string.Empty;
            var convertedDecidedOnDate = 0;
            var convertedDecidedOnTime = 0;
            var applicationLimitingKeys = new List<string>();
            var admissionStatusesEntities = new List<Domain.Student.Entities.ApplicationStatus2>();

            string applicationStatusKey = CacheSupport.BuildCacheKey(AllApplicationStatusCacheKey,
                        !string.IsNullOrWhiteSpace(applicationId) ? applicationId : string.Empty,
                        decidedOn.HasValue ? decidedOn.Value : default(DateTimeOffset?),
                        filterQualifiers != null && filterQualifiers.Any() ? filterQualifiers : null,
                        filterPersonIds != null && filterPersonIds.Any() ? filterPersonIds : null);

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                   this,
                   ContainsKey,
                   GetOrAddToCacheAsync,
                   AddOrUpdateCacheAsync,
                   transactionInvoker,
                   applicationStatusKey,
                   "",
                   offset,
                   limit,
                   AllApplicationStatusCacheTimeout,
                   async () =>
                   {
                       if (decidedOn != null && decidedOn != DateTimeOffset.MinValue)
                       {
                           var localDateTime = Convert.ToDateTime(decidedOn.ToLocalDateTime(colleagueTimeZone));

                           convertedDecidedOnDate = DmiString.DateTimeToPickDate(localDateTime);
                           convertedDecidedOnTime = DmiString.DateTimeToPickTime(localDateTime);
                           dateFilterOperation = filterQualifiers != null && filterQualifiers.ContainsKey("DecidedOn") ? filterQualifiers["DecidedOn"] : "EQ";
                       }

                       string[] limitingKeys = null;
                       if (filterPersonIds != null && filterPersonIds.ToList().Any())
                       {
                           // Set limiting keys to previously retrieved personIds from SAVE.LIST.PARMS
                           limitingKeys = filterPersonIds;
                           var applicantApplicationId = (await DataReader.SelectAsync("APPLICANTS", limitingKeys, "WITH APP.APPLICATIONS NE '' BY.EXP APP.APPLICATIONS SAVING APP.APPLICATIONS")).ToList();
                           if (applicantApplicationId != null && applicantApplicationId.Any())
                           {
                               applicationLimitingKeys.AddRange(applicantApplicationId);
                           }
                           if (applicationLimitingKeys == null || !applicationLimitingKeys.Any())
                           {
                               return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                           }
                       }

                       if (!string.IsNullOrEmpty(applicationId))
                       {
                           var recordInfo = await GetRecordInfoFromGuidAsync(applicationId);
                           if (recordInfo == null)
                           {
                               return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                           }
                           else if ((!recordInfo.Entity.Equals("APPLICATIONS", StringComparison.OrdinalIgnoreCase))
                                   || (!string.IsNullOrEmpty(recordInfo.SecondaryKey)) || (string.IsNullOrEmpty(recordInfo.PrimaryKey)))
                           {
                               return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                           }
                           else
                           {
                               if (applicationLimitingKeys != null && applicationLimitingKeys.Any() && !applicationLimitingKeys.Contains(recordInfo.PrimaryKey))
                               {
                                   return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                               }
                               applicationLimitingKeys = new List<string>() { recordInfo.PrimaryKey };
                           }
                       }

                       selectionCriteria.Append("WITH APPL.STATUS.DATE.TIME.IDX NE ''");

                       // get all the special processing codes from application.statuses
                       var applStatusesNoSpCodeIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE NE ''");

                       if (applStatusesNoSpCodeIds == null || !applStatusesNoSpCodeIds.Any())
                       {
                           return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                       }

                       // apply the date filter
                       if (convertedDecidedOnDate != 0)
                       {
                           selectionCriteria.Append(string.Format(" AND WITH APPL.STATUS.DATE {0} '{1}'", dateFilterOperation, convertedDecidedOnDate));
                       }
                       // special conditions to account for the date-filter operation which includes date + time.
                       // if EQ, then we want to make sure the time matches exactly
                       // allother operations need to make sure records from the actual date are included since the previous select can possibly remove them. 
                       if (convertedDecidedOnTime != 0)
                       {
                           switch (dateFilterOperation)
                           {
                               case "EQ":
                                   {
                                       selectionCriteria.Append(string.Format(" WITH APPL.STATUS.TIME {0} '{1}'", dateFilterOperation, convertedDecidedOnTime));
                                       break;
                                   }
                               default:
                                   selectionCriteria.Append(string.Format(@" OR WITH APPL.STATUS.DATE EQ '{0}' AND APPL.STATUS.TIME {1} '{2}'", convertedDecidedOnDate, dateFilterOperation, convertedDecidedOnTime));
                                   break;
                           }
                       }

                       var applIdsWithStatus = await DataReader.SelectAsync("APPLICATIONS", applicationLimitingKeys.ToArray(),
                              selectionCriteria.ToString());

                       if (applIdsWithStatus == null || !applIdsWithStatus.Any())
                       {
                           return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                       }

                       // we need to assocaite the APPLICATION.ID with the APPL.STATUS.DATE.TIME.IDX
                       var applIds = (await DataReader.SelectAsync("APPLICATIONS", applIdsWithStatus,
                          "BY.EXP APPL.STATUS.DATE.TIME.IDX")).ToList();
                       var applIdxs = (await DataReader.SelectAsync("APPLICATIONS", applIdsWithStatus,
                          "BY.EXP APPL.STATUS.DATE.TIME.IDX SAVING APPL.STATUS.DATE.TIME.IDX")).ToList();

                       // this code makes an assumption that both data sets contain the same number of records,
                       // in the same order.   
                       var idx = 0; var keys = new List<string>();
                       foreach (var applId in applIds)
                       {
                           var applId2 = applId.Split(DmiString._VM)[0];
                           var statusCode = applIdxs.ElementAt(idx).Split(new[] { '*' })[0];
                           if (applStatusesNoSpCodeIds.Contains(statusCode))
                           {
                               keys.Add(String.Concat(applId2, "|", applIdxs.ElementAt(idx)));
                               idx++;
                           }
                           else
                           {
                               idx++;
                           }
                       }

                       // once the result set is exploded, we now have additional records which may not meet the filter criteria
                       // using the index, apply the filter based on the filter-operator
                       if (decidedOn != null && decidedOn != DateTimeOffset.MinValue)
                       {
                           try
                           {
                               keys = FilterDecidedOnDate(dateFilterOperation, decidedOn, keys);
                           }
                           catch (Exception ex)
                           {
                               exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                               throw exception;
                           }
                       }

                       return new CacheSupport.KeyCacheRequirements()
                       {
                           limitingKeys = keys != null && keys.Any() ? keys.Distinct().ToList() : null,
                           criteria = string.Empty,
                       };
                   });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<ApplicationStatus2>, int>(new List<ApplicationStatus2>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;
            var keysSubList = keyCacheObject.Sublist;

            if (keysSubList.Any())
            {
                var subList = new List<string>();

                foreach (var key in keysSubList)
                {
                    var applKey = key.Split('|')[0];
                    subList.Add(applKey);
                }
                var applications = await DataReader.BulkReadRecordAsync<Applications>("APPLICATIONS", subList.Distinct().ToArray());

                // Get a guid collection containing the guid and keySubList containing an application.id|appl.status.date.time.idx
                Dictionary<string, string> dict = null;
                try
                {
                    dict = await GetGuidsCollectionAsync(keysSubList);
                }
                catch (Exception ex)
                {
                    exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                    exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATION with APPL.STATUS.DATE.TIME.IDX."));
                    throw exception;
                }

                if (dict == null || !dict.Any())
                {
                    exception.AddError(new RepositoryError("Bad.Data", "Guids not found for APPLICATION with APPL.STATUS.DATE.TIME.IDX."));
                    throw exception;
                }

                foreach (var key in keysSubList)
                {
                    try
                    {
                        var splitKey = key.Split('|');
                        var applicationKey = splitKey[0];
                        var applStatusIdx = splitKey[1];
                        var application = applications.FirstOrDefault(x => x.Recordkey == applicationKey);

                        string applGuidInfo = string.Empty;
                        dict.TryGetValue(key, out applGuidInfo);

                        if (string.IsNullOrEmpty(applGuidInfo))
                        {
                            exception.AddError(new RepositoryError("Bad.Data", string.Format("Guid not found for APPLICATION '{0}' with APPL.STATUS.DATE.TIME.IDX '{1}'.", applicationKey, applStatusIdx))
                            { SourceId = key });
                            continue;
                        }

                        var splitValues = applStatusIdx.Split(new[] { '*' });
                        var applStatus = splitValues[0];
                        var applStatusDate = Convert.ToDateTime(DmiString.PickDateToDateTime(Convert.ToInt32(splitValues[1])));
                        var convertedTime = DmiString.PickTimeToDateTime(Convert.ToInt32(splitValues[2]));
                        var applStatusTime = new DateTime(1, 1, 1, convertedTime.Hours, convertedTime.Minutes, convertedTime.Seconds);

                        if (!string.IsNullOrEmpty(applGuidInfo))
                        {
                            var adminStatus = new ApplicationStatus2(applGuidInfo, application.Recordkey,
                                applStatus, applStatusDate, applStatusTime);
                            admissionStatusesEntities.Add(adminStatus);
                        }
                    }
                    catch (Exception ex)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Format("Application error occurred found for key {0}. {1}", key, ex.Message))
                        { SourceId = key });
                    }
                }
            }

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return admissionStatusesEntities.Any() ? new Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>(admissionStatusesEntities, totalCount) :
                new Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>(new List<Domain.Student.Entities.ApplicationStatus2>(), 0);
        }

        /// <summary>
        /// Using a collection of ids in the format application.id|appl.status.date.time.idx
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a application.id|appl.status.date.time.idx (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids.Select(s => new
                {
                    recordKey = s.Split(new[] { '|' })[0],
                    secondardaryKey = s.Split(new[] { '|' })[1],
                })
                    .Where(s => !string.IsNullOrWhiteSpace(s.recordKey))
                    .Distinct().ToList()
                    .ConvertAll(applicationKey => new RecordKeyLookup("APPLICATIONS", applicationKey.recordKey,
                    "APPL.STATUS.DATE.TIME.IDX", applicationKey.secondardaryKey, false))
                    .ToArray();

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
                                guidCollection.Add(string.Concat(splitKeys[1], "|", splitKeys[2]), recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", "APPLICATIONS"), ex);
            }

            return guidCollection;
        }

        /// <summary>
        ///  When the data is set is exploded, we can get back dates that do not meet
        ///     the specified date filter. Thereare two options... I can convert all the
        ///     APPL.STATUS.DATE.TIME.IDX values into a DateTime and apply date functions
        ///     or evaluate the dates as using integers using the internal date format
        /// </summary>
        /// <param name="dateFilterOperation"></param>
        /// <param name="convertedDecidedOn"></param>
        /// <param name="convertedDecidedOnTime"></param>
        /// <param name="keys"></param>
        /// <returns>Collection of strings</string></returns>
        private List<string> FilterDecidedOnDate(string dateFilterOperation, DateTimeOffset? decidedOn, List<string> keys)
        {
            var retVal = new List<string>();

            if (string.IsNullOrEmpty(dateFilterOperation) || decidedOn == null || decidedOn == DateTime.MinValue)
                return retVal;

            var localDateTime = Convert.ToDateTime(decidedOn.ToLocalDateTime(colleagueTimeZone));

            var internalDate = DmiString.DateTimeToPickDate(localDateTime);
            var internalTime = DmiString.DateTimeToPickTime(localDateTime);

            switch (dateFilterOperation)
            {
                case "EQ":
                    {

                        retVal = (keys.Where(k => k.Contains('|'))
                            .Select(s => new
                            {
                                recordKey = s.Split(new[] { '|' }),
                                num = s.Split(new[] { '|' })[1].Split(new[] { '*' }),
                                str = s
                            })
                        .Where(s => (!string.IsNullOrEmpty(s.num[1]) && !string.IsNullOrEmpty(s.num[2]))
                            && (Convert.ToInt32(s.num[1]) == internalDate
                                && Convert.ToInt32(s.num[2]) == internalTime))
                         .Select(s => s.str)).ToList();

                        break;
                    }
                case "LE":
                    {
                        retVal = (keys.Select(s => new
                        {
                            recordKey = s.Split(new[] { '|' }),
                            num = s.Split(new[] { '|' })[1].Split(new[] { '*' }),
                            str = s
                        })
                        .Where(s => (!string.IsNullOrEmpty(s.num[1]) && !string.IsNullOrEmpty(s.num[2]))
                            && ((Convert.ToInt32(s.num[1]) < internalDate)
                                 || (Convert.ToInt32(s.num[1]) == internalDate
                                 && Convert.ToInt32(s.num[2]) <= internalTime)))
                          .Select(s => s.str)).ToList();
                        break;
                    }
                //case "LT":
                //    {
                //        retVal = (keys.Select(s => new
                //        {
                //            recordKey = s.Split(new[] { '|' }),
                //            num = s.Split(new[] { '|' })[1].Split(new[] { '*' }),
                //            str = s
                //        })
                //          .Where(s => (!string.IsNullOrEmpty(s.num[1]) && !string.IsNullOrEmpty(s.num[2]))
                //             && ((Convert.ToInt32(s.num[1]) < internalDate)
                //                 || (Convert.ToInt32(s.num[1]) == internalDate
                //                 && Convert.ToInt32(s.num[2]) < internalTime)))
                //          .Select(s => s.str)).ToList();
                //        break;
                //    }
                case "GE":
                    {
                        retVal = (keys.Select(s => new
                        {
                            recordKey = s.Split(new[] { '|' }),
                            num = s.Split(new[] { '|' })[1].Split(new[] { '*' }),
                            str = s
                        })
                         .Where(s => (!string.IsNullOrEmpty(s.num[1]) && !string.IsNullOrEmpty(s.num[2]))
                            && ((Convert.ToInt32(s.num[1]) > internalDate)
                                 || (Convert.ToInt32(s.num[1]) == internalDate
                                 && Convert.ToInt32(s.num[2]) >= internalTime)))
                          .Select(s => s.str)).ToList();
                        break;
                    }
                //case "GT":
                //    {
                //        retVal = (keys.Select(s => new
                //        {
                //            recordKey = s.Split(new[] { '|' }),
                //            num = s.Split(new[] { '|' })[1].Split(new[] { '*' }),
                //            str = s
                //        })
                //          .Where(s => (!string.IsNullOrEmpty(s.num[1]) && !string.IsNullOrEmpty(s.num[2]))
                //            && ((Convert.ToInt32(s.num[1]) > internalDate)
                //                 || (Convert.ToInt32(s.num[1]) == internalDate
                //                 && Convert.ToInt32(s.num[2]) > internalTime)))
                //          .Select(s => s.str)).ToList();
                //        break;
                //    }
                default: break;
            }

            return retVal;
        }

        /// <summary>
        /// Gets admission status by id.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Domain.Student.Entities.ApplicationStatus2> GetApplicationStatusByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Admission decision guid is required.");
            }

            var applicationId = await GetRecordInfoFromGuidAsync(guid);

            if (applicationId == null)
                throw new KeyNotFoundException(string.Format("Application record not found for guid {0}", guid));

            var application = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", applicationId.PrimaryKey);

            var applStatusesNoSpCodeIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");


            Domain.Student.Entities.ApplicationStatus2 adminStatus = null;

            foreach (var applStatusEntity in application.ApplStatusesEntityAssociation)
            {
                var applStatusKey = string.Format("{0}*{1}*{2}",
                                    applStatusEntity.ApplStatusAssocMember,
                                    DmiString.DateTimeToPickDate(applStatusEntity.ApplStatusDateAssocMember.Value),
                                    DmiString.DateTimeToPickTime(applStatusEntity.ApplStatusTimeAssocMember.Value));

                var applGuidInfo = await GetGuidFromRecordInfoAsync("APPLICATIONS", applicationId.PrimaryKey, "APPL.STATUS.DATE.TIME.IDX", applStatusKey);

                if (!string.IsNullOrEmpty(applGuidInfo))
                {
                    if (applGuidInfo.Equals(guid, StringComparison.OrdinalIgnoreCase))
                    {
                        if (applStatusesNoSpCodeIds.Contains(applStatusEntity.ApplStatusAssocMember))
                        {
                            break;
                        }
                        adminStatus = new Domain.Student.Entities.ApplicationStatus2(applGuidInfo, application.Recordkey, applStatusEntity.ApplStatusAssocMember,
                                    applStatusEntity.ApplStatusDateAssocMember.Value, applStatusEntity.ApplStatusTimeAssocMember.Value);
                        break;
                    }
                }
            }

            return adminStatus;
        }

        /// <summary>
        /// Creates admission decision.
        /// </summary>
        /// <param name="appStatusEntity"></param>
        /// <returns></returns>
        public async Task<ApplicationStatus2> UpdateAdmissionDecisionAsync(ApplicationStatus2 appStatusEntity)
        {
            if (appStatusEntity == null)
            {
                throw new ArgumentNullException("Admission Decision", "Admission decision must be provided.");
            }

            var request = new UpdateAdmApplStatusesRequest()
            {
                AdmdecGuid = appStatusEntity.Guid,
                AdmdecApplication = appStatusEntity.ApplicantRecordKey,
                AdmdecDate = appStatusEntity.DecidedOn.ToLocalDateTime(colleagueTimeZone),
                AdmdecTime = appStatusEntity.DecidedOn.ToLocalDateTime(colleagueTimeZone),
                AdmdecStatus = appStatusEntity.DecisionType
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateAdmApplStatusesRequest, UpdateAdmApplStatusesResponse>(request);

            if (response.ApplicationStatusErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating admission decision '{0}':", appStatusEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                response.ApplicationStatusErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCode) ? "" : e.ErrorCode, e.ErrorMsg)));
                logger.Error(errorMessage);
                throw exception;
            }


            return await GetApplicationStatusByGuidAsync(response.AdmdecGuid, true);
        }

        /// <summary>
        /// Gets tuple with entity, primary key & secondary key.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Tuple<string, string, string>> GetApplicationStatusKey(string guid)
        {
            var result = await GetRecordInfoFromGuidAsync(guid);
            return result == null ? null : new Tuple<string, string, string>(result.Entity, result.PrimaryKey, result.SecondaryKey);
        }


        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        private async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }
    }
}