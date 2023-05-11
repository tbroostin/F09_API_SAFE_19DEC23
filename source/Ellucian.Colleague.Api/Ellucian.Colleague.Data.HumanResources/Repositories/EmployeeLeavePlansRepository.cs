/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EmployeeLeavePlansRepository : BaseColleagueRepository, IEmployeeLeavePlansRepository
    {
        private readonly ApiSettings apiSettings;
        const string AllEmployeeLeavePlanCache = "AllEmployeeLeavePlan";
        const int AllEmployeeLeavePlanCacheTimeout = 20; // Clear from cache every 20 minutes
        RepositoryException exception = null;
        public EmployeeLeavePlansRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Get EmployeeLeavePlans objects for all EmployeeLeavePlans bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <returns>Tuple of Perleave Entity objects <see cref="Perleave"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Perleave>, int>> GetEmployeeLeavePlansAsync(int offset, int limit, bool bypassCache = false)
        {
            int totalCount = 0;
            string[] subList = null;
            var employeeLeavePlansRecords = new List<Domain.HumanResources.Entities.Perleave>();
            Collection<DataContracts.Perleave> empLeaveplanRecords = null;

            try
            {
                var employeeLeavePlanCacheKey = CacheSupport.BuildCacheKey(AllEmployeeLeavePlanCache);

                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    employeeLeavePlanCacheKey,
                    "PERLEAVE",
                    offset,
                    limit,
                    AllEmployeeLeavePlanCacheTimeout,
                    async () =>
                    {
                        return new CacheSupport.KeyCacheRequirements() { };
                    });

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<Domain.HumanResources.Entities.Perleave>, int>(null, 0);
                }
                subList = keyCache.Sublist.ToArray();

                totalCount = keyCache.TotalCount.Value;

                empLeaveplanRecords = await DataReader.BulkReadRecordAsync<DataContracts.Perleave>("PERLEAVE", subList);
                if (empLeaveplanRecords == null || empLeaveplanRecords.Count == 0)
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "An unexpected error occurred extracting PERLEAVE data."));

                }

            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            foreach (var plan in empLeaveplanRecords)
            {
                try
                {
                    bool error = false;
                    if (string.IsNullOrEmpty(plan.RecordGuid))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data",
                            "EmployeeLeavePlans guid can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (string.IsNullOrEmpty(plan.Recordkey))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans id can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (string.IsNullOrEmpty(plan.PerlvHrpId))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans PersonId can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (string.IsNullOrEmpty(plan.PerlvLpnId))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans plan can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if ((plan.PerlvStartDate == null) || (!plan.PerlvStartDate.HasValue))
                    {
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans startDate can not be null or empty. Entity: ‘LEAVPLAN’")
                        {
                            SourceId = plan.Recordkey,
                            Id = plan.RecordGuid
                        });
                        error = true;
                    }
                    if (!error)
                        employeeLeavePlansRecords.Add(BuildEmployeeLeavePlans(plan));
                }

                catch (Exception ex)
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                    {
                        SourceId = plan.Recordkey,
                        Id = plan.RecordGuid
                    });
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<Domain.HumanResources.Entities.Perleave>, int>(employeeLeavePlansRecords, totalCount);
        }

        /// <summary>
        /// Get EmployeeLeavePlans entity for a specific guid
        /// </summary>   
        /// <param name="guid">id of the EmployeeLeavePlans record.</param>
        /// <returns>EmployeeLeavePlans Entity <see cref="Domain.HumanResources.Entities.Perleave"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.Perleave> GetEmployeeLeavePlansByGuidAsync(string guid)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(guid);
            if (entity == null || entity.Entity != "PERLEAVE")
            {
                throw new KeyNotFoundException(string.Format("EmployeeLeavePlans not found for id {0}", guid));
            }
            var empLeaveplanId = entity.PrimaryKey;
            if (string.IsNullOrWhiteSpace(empLeaveplanId))
            {
                throw new KeyNotFoundException("EmployeeLeavePlan id " + guid + "does not exist");
            }
            DataContracts.Perleave empLeavePlanRecord = null;
            try
            {
                empLeavePlanRecord = await DataReader.ReadRecordAsync<DataContracts.Perleave>("PERLEAVE", empLeaveplanId);
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                throw exception;
            }
            if (empLeavePlanRecord == null)
            {
                throw new KeyNotFoundException("EmployeeLeavePlan not found with id " + guid);
            }
            Perleave retval = null;
            try
            {
                bool error = false;
                if (string.IsNullOrEmpty(empLeavePlanRecord.RecordGuid))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data",
                        "EmployeeLeavePlans guid can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = empLeavePlanRecord.Recordkey,
                        Id = empLeavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (string.IsNullOrEmpty(empLeavePlanRecord.Recordkey))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans id can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = empLeavePlanRecord.Recordkey,
                        Id = empLeavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (string.IsNullOrEmpty(empLeavePlanRecord.PerlvHrpId))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans PersonId can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = empLeavePlanRecord.Recordkey,
                        Id = empLeavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (string.IsNullOrEmpty(empLeavePlanRecord.PerlvLpnId))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans plan can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = empLeavePlanRecord.Recordkey,
                        Id = empLeavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if ((empLeavePlanRecord.PerlvStartDate == null) || (!empLeavePlanRecord.PerlvStartDate.HasValue))
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeavePlans startDate can not be null or empty. Entity: ‘LEAVPLAN’")
                    {
                        SourceId = empLeavePlanRecord.Recordkey,
                        Id = empLeavePlanRecord.RecordGuid
                    });
                    error = true;
                }
                if (!error)
                    retval = BuildEmployeeLeavePlans(empLeavePlanRecord);
            }

            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    SourceId = empLeavePlanRecord.Recordkey,
                    Id = empLeavePlanRecord.RecordGuid
                });
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return retval;

        }

        /// <summary>
        /// Get Collection of PerLeave GUIDs and IDs
        /// </summary>
        /// <param name="perleaveIds">collection of PerLeave ids</param>
        /// <returns>Dictionary consisting of a perLeaveId (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetPerleaveGuidsCollectionAsync(IEnumerable<string> perleaveIds)
        {
            if ((perleaveIds == null) || (perleaveIds != null && !perleaveIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var perleaveGuidCollection = new Dictionary<string, string>();

            var perleaveGuidLookup = perleaveIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("PERLEAVE", p, false)).ToArray();
            var recordKeyLookupResults = await DataReader.SelectAsync(perleaveGuidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!perleaveGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        perleaveGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch (Exception ex)
                {
                    // do not throw error
                    logger.Error(ex, "Unable to get perleave by guid.");
                }
            }

            return perleaveGuidCollection;
        }

        /// <summary>
        /// Get EmployeeLeavePlans entity for a specific id
        /// </summary>   
        /// <param name="guid">id of the EmployeeLeavePlans record.</param>
        /// <returns>EmployeeLeavePlans Entity <see cref="Domain.HumanResources.Entities.Perleave"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.Perleave> GetEmployeeLeavePlansByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new KeyNotFoundException("EmployeeLeavePlan id " + id + "does not exist");
            }
            try
            {
                var empLeavePlanRecord = await DataReader.ReadRecordAsync<DataContracts.Perleave>("PERLEAVE", id);
                if (empLeavePlanRecord == null)
                {
                    throw new KeyNotFoundException("EmployeeLeavePlan not found with id " + id);
                }
                return BuildEmployeeLeavePlans(empLeavePlanRecord);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets EmployeeLeavePlan entities for a given person Id. 
        /// Not all employees will have PERLEAVE, so return an empty list if none are found. 
        /// If PERLEAVE are found, build a list of EmployeeLeavePlan entities based on the employee PERLEAVE, PERLVDTL
        /// and associated LeavePlan, LeaveType, and EarningsType data passed into this method.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="leavePlans"></param>
        /// <param name="leaveTypes"></param>
        /// <param name="earnTypes"></param>
        /// <param name="includeLeavePlansWithNoEarningsTypes">Optional parameter that indicates whether or not to include leave plans without associated any earnigs types</param>
        /// <returns>EmployeeLeavePlan entities for given employee Id</returns>
        public async Task<IEnumerable<EmployeeLeavePlan>> GetEmployeeLeavePlansByEmployeeIdsAsync(IEnumerable<string> employeeIds,
            IEnumerable<LeavePlan> leavePlans,
            IEnumerable<LeaveType> leaveTypes,
            IEnumerable<EarningType2> earnTypes,
            bool includeLeavePlansWithNoEarningsTypes = false)
        {
            if (employeeIds == null || !employeeIds.Any())
            {
                throw new ArgumentNullException("employeeId");
            }
            if (leavePlans == null || !leavePlans.Any())
            {
                logger.Error("leavePlans are required to build EmployeeLeavePlan objects");
                leavePlans = new List<LeavePlan>();
            }

            if (earnTypes == null || !earnTypes.Any())
            {
                logger.Error("earnTypes are required to build EmployeeLeavePlan objects");
                earnTypes = new List<EarningType2>();
            }
            if (leaveTypes == null || !leaveTypes.Any())
            {
                logger.Error("EmployeeLeavePlans may not be correct due to null leaveTypes input");
                leaveTypes = new List<LeaveType>();
            }


            var employeeLeavePlans = new List<EmployeeLeavePlan>();

            var distinctEmployeeIds = employeeIds.Distinct().ToArray();

            var perLeaveCriteria = "WITH PERLV.HRP.ID.INDEX EQ '?'";
            var perLeaveKeys = await DataReader.SelectAsync("PERLEAVE", perLeaveCriteria, distinctEmployeeIds);


            if (perLeaveKeys == null || !perLeaveKeys.Any())
            {
                return employeeLeavePlans;
            }

            var perLeaveDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Perleave>(perLeaveKeys.ToArray());

            var perLeaveDetailCriteria = "WITH PLD.HRP.ID EQ '?'";
            var perLeaveDetailKeys = await DataReader.SelectAsync("PERLVDTL", perLeaveDetailCriteria, distinctEmployeeIds);


            if (perLeaveDetailKeys == null)
            {
                perLeaveDetailKeys = new string[0];
            }

            var perLeaveDetailDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Perlvdtl>(perLeaveDetailKeys);

            // Accrual Details for leave plan id
            var perLeaveAccrualDetailCriteria = "WITH PLA.HRP.ID EQ '?'";
            var perLeaveAccrualDetailKeys = await DataReader.SelectAsync("PERLVACC", perLeaveAccrualDetailCriteria, distinctEmployeeIds);


            if (perLeaveAccrualDetailKeys == null)
            {
                perLeaveAccrualDetailKeys = new string[0];
            }

            // Per Leave accrual details data contract
            var perlvaccDetailDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.Perlvacc>(perLeaveAccrualDetailKeys);

            //create a dictionary of leave plans
            var leavePlanDictionary = leavePlans.ToDictionary(lp => lp.Id);

            //create a lookup for the detail records on the PerleaveId
            var perLeaveDetailLookup = perLeaveDetailDataContracts.ToLookup(dtl => dtl.PldPerleaveId);


            foreach (var perLeaveDataContract in perLeaveDataContracts)
            {
                try
                {
                    if (!leavePlanDictionary.ContainsKey(perLeaveDataContract.PerlvLpnId))
                    {
                        logger.Error(string.Format("LeavePlan {0} not found in currently defined leave plans; unable to build EmployeeLeavePlan entity for {1}.", perLeaveDataContract.PerlvLpnId, perLeaveDataContract.Recordkey));
                    }
                    else
                    {
                        var leavePlan = leavePlanDictionary[perLeaveDataContract.PerlvLpnId];
                        var leaveType = leaveTypes.FirstOrDefault(lt => lt.Code == leavePlan.Type);
                        var leaveCategory = LeaveTypeCategory.None;
                        if (leaveType != null)
                        {
                            leaveCategory = leaveType.TimeType;
                        }

                        if (!perLeaveDataContract.PerlvStartDate.HasValue)
                        {
                            logger.Error(string.Format("No startdate specified for Perleave LeavePlan {0}; unable to build EmployeeLeavePlan entity.", perLeaveDataContract.Recordkey));
                        }
                        // SS Leave Balance page doesn't mandate the earnigs type to be associated with a leave plan.
                        else if ((leavePlan.EarningsTypes == null || !leavePlan.EarningsTypes.Any()) && !includeLeavePlansWithNoEarningsTypes)
                        {
                            logger.Error(string.Format("No earnings types defined for LeavePlan {0}; unable to build EmployeeLeavePlan entity for {1}.", perLeaveDataContract.PerlvLpnId, perLeaveDataContract.Recordkey));
                        }
                        else
                        {
                            string earningsTypeId = null, earningsTypeDescription = "No Description";
                            IEnumerable<string> earningTypeList = new List<string>();
                            EarningType2 earningsTypeEntity;

                            if (leavePlan.EarningsTypes != null && leavePlan.EarningsTypes.Any())
                            {
                                // a leave plan's earnings type is defined as a list, but only the first value matters
                                earningsTypeId = leavePlan.EarningsTypes.First();
                                earningsTypeEntity = earnTypes.FirstOrDefault(et => et.Code == earningsTypeId);

                                if (earningsTypeEntity != null)
                                {
                                    earningsTypeDescription = earningsTypeEntity.Description;
                                }
                                else
                                {
                                    logger.Error(string.Format("EarningsTypeId defined on leavePlan does not exist as an EARNTYPE record"));
                                }

                                //Used for comp time display in Time Entry
                                earningTypeList = leavePlan.EarningsTypes;
                            }

                            var leaveAllowedDate = perLeaveDataContract.PerlvAllowedDate.HasValue ? perLeaveDataContract.PerlvAllowedDate.Value : perLeaveDataContract.PerlvStartDate.Value;
                            var priorPeriodBalance = perLeaveDataContract.PerlvBalance.HasValue ? perLeaveDataContract.PerlvBalance.Value : 0.00m;

                            var allowNegative = !(string.IsNullOrWhiteSpace(leavePlan.AllowNegative) || leavePlan.AllowNegative.Equals("N", StringComparison.OrdinalIgnoreCase));

                            var yearlyStartMonth = leavePlan.YearlyStartDate.HasValue ? leavePlan.YearlyStartDate.Value.Month : 1;
                            var yearlyStartDay = leavePlan.YearlyStartDate.HasValue ? leavePlan.YearlyStartDate.Value.Day : 1;
                            var isPlanYearStartDateDefined = leavePlan.YearlyStartDate.HasValue ? true : false;


                            DataContracts.Perlvacc leaveAccrualDetails = null;
                            if (perlvaccDetailDataContracts != null)
                            {
                                // Getting the active Accrual Details for leave plan id
                                leaveAccrualDetails = perlvaccDetailDataContracts.FirstOrDefault(placc => (placc.PlaHrpId == perLeaveDataContract.PerlvHrpId
                                                                                                && placc.PlaLpnId == perLeaveDataContract.PerlvLpnId
                                                                                                && (placc.PlaEndDate == null || ((DateTime?)DateTime.Now.Date <= placc.PlaEndDate))
                                                                                                ));
                            }
                            if (leaveAccrualDetails == null)
                            {
                                leaveAccrualDetails = new DataContracts.Perlvacc();
                            }

                            bool isLeaveReportingPlan = (!string.IsNullOrWhiteSpace(perLeaveDataContract.PerlvLeaveReporting) && perLeaveDataContract.PerlvLeaveReporting.Equals("Y", StringComparison.OrdinalIgnoreCase)) || (leavePlan.IsLeaveReportingPlan && string.IsNullOrWhiteSpace(perLeaveDataContract.PerlvLeaveReporting));

                            var employeeLeavePlan = new EmployeeLeavePlan(perLeaveDataContract.Recordkey,
                                perLeaveDataContract.PerlvHrpId,
                                perLeaveDataContract.PerlvStartDate.Value,
                                perLeaveDataContract.PerlvEndDate,
                                perLeaveDataContract.PerlvLpnId,
                                leavePlan.Title,
                                leavePlan.StartDate.Value,
                                leavePlan.EndDate,
                                leaveCategory,
                                earningsTypeId,
                                earningsTypeDescription,
                                leaveAllowedDate,
                                priorPeriodBalance,
                                yearlyStartMonth,
                                yearlyStartDay,
                                isLeaveReportingPlan,
                                earningTypeList,
                                leaveAccrualDetails.PlaAccrualHours,
                                leaveAccrualDetails.PlaAccrualLimit,
                                leaveAccrualDetails.PlaCarryoverHours,
                                leaveAccrualDetails.PlaRolloverMaximum,
                                leavePlan.AccrualMethod,
                                isPlanYearStartDateDefined,
                                perLeaveDataContract.PerlvLatestStartBalUpdt,
                                allowNegative,
                                includeLeavePlansWithNoEarningsTypes);

                            employeeLeavePlans.Add(employeeLeavePlan);


                            if (perLeaveDetailLookup.Contains(employeeLeavePlan.Id))
                            {
                                var perLeaveDetails = perLeaveDetailLookup[employeeLeavePlan.Id];

                                foreach (var detailRecord in perLeaveDetails)
                                {
                                    try
                                    {
                                        if (detailRecord.PldDate.HasValue)
                                        {

                                            int transactionId = int.Parse(detailRecord.Recordkey);

                                            var transaction = new EmployeeLeaveTransaction(transactionId,
                                                detailRecord.PldLpnId,
                                                detailRecord.PldPerleaveId,
                                                detailRecord.PldHours ?? 0,
                                                detailRecord.PldDate.ToPointInTimeDateTimeOffset(detailRecord.PldDate, apiSettings.ColleagueTimeZone).Value, //this is correct. "midnight" of the colleague timezone on the PldDate
                                                translateTransactionType(detailRecord.PldAction),
                                                detailRecord.PldForwardingBalance ?? 0,
                                                detailRecord.PldCurrentBalance ?? 0);

                                            employeeLeavePlan.AddLeaveTransaction(transaction);


                                        }
                                        else
                                        {
                                            LogDataError("PERLVDTL", detailRecord.Recordkey, detailRecord, null, "PldDate does not have value");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        LogDataError("PERLVDTL", detailRecord.Recordkey, detailRecord, e);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogDataError("EmployeeLeavePlan", perLeaveDataContract.Recordkey, perLeaveDataContract, e, "Unable to build EmployeeLeavePlan record from db data");
                }
            }
            return employeeLeavePlans;
        }

        private LeaveTransactionType translateTransactionType(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentNullException("action");
            }

            switch (action.ToUpper())
            {
                case "A":
                    return LeaveTransactionType.Earned;
                case "U":
                    return LeaveTransactionType.Used;
                case "J":
                    return LeaveTransactionType.Adjusted;
                case "L":
                    return LeaveTransactionType.LeaveReporting;
                case "S":
                    return LeaveTransactionType.StartingBalanceAdjustment;
                case "B":
                    return LeaveTransactionType.StartingBalance;
                case "C":
                    return LeaveTransactionType.MidYearBalanceAdjustment;
                case "R":
                    return LeaveTransactionType.Rollover;
                default:
                    throw new ArgumentException("leave transaction action does not match allowable values, A, U, L, J, S, B, C or R", "action");
            }
        }

        /// <summary>
        /// Helper to build Employee Leave plans entity
        /// </summary>
        /// <param name="empLeavePlanRecord">the perleave db record</param>
        /// <returns></returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.Perleave BuildEmployeeLeavePlans(DataContracts.Perleave empLeavePlanRecord)
        {
            return new Domain.HumanResources.Entities.Perleave(empLeavePlanRecord.RecordGuid, empLeavePlanRecord.Recordkey, empLeavePlanRecord.PerlvStartDate, empLeavePlanRecord.PerlvHrpId, empLeavePlanRecord.PerlvLpnId)
            {
                EndDate = empLeavePlanRecord.PerlvEndDate
            };

        }
    }
}
