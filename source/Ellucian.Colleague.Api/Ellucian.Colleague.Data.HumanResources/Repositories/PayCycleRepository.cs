/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Interact with Pay Cycle data from database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayCycleRepository : BaseColleagueRepository, IPayCycleRepository
    {
        private const string PayCyclesCacheKey = "PayCycles";
        private const string PayCycleFrequencyCacheKey = "PayCycleFrequency";
        private const string HRWebDefaultsCacheKey = "HRWebDefaults";
        private const string HRSSDefaultsCacheKey = "HRSSDefaults";
        private readonly int bulkReadSize;
        private readonly string colleagueTimeZone;
        /// <summary>
        /// Repository constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PayCycleRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            colleagueTimeZone = settings.ColleagueTimeZone;

        }

        /// <summary>
        /// Get All PayControl db records
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<DataContracts.Paycntrl>> GetAllPayControlAsync()
        {
            var payControlRecords = new List<DataContracts.Paycntrl>();

            // verify that pay control records exist...
            var payControlIds = await DataReader.SelectAsync("PAYCNTRL", "");
            if (payControlIds == null)
            {
                var message = "Unexpected: Null pay control Ids returned from select";
                logger.Error(message);
                throw new ApplicationException(message);
            }



            // read chunks of pay control records and add them to return argument...
            for (int i = 0; i < payControlIds.Count(); i += bulkReadSize)
            {
                var subList = payControlIds.Skip(i).Take(bulkReadSize);
                var payControlRecordsPart = await DataReader.BulkReadRecordAsync<DataContracts.Paycntrl>(subList.ToArray());
                if (payControlRecordsPart == null)
                {
                    var message = string.Format("Unexpected: Bulk data read using keys of pay control records returned null.");
                    logger.Error(message);
                }
                else
                {
                    payControlRecords.AddRange(payControlRecordsPart);
                }
            }
            return payControlRecords;
        }

        /// <summary>
        /// Get all PayCycle objects, built from database data
        /// </summary>
        /// <param name="lookbackDate">A optional date which is used to filter previous pay periods with end dates prior to this date.</param>
        /// <returns></returns>
        public async Task<IEnumerable<PayCycle>> GetPayCyclesAsync(DateTime? lookbackDate = null)
        {
            var payControlRecords = await GetAllPayControlAsync();

            var payCycleEntities = new List<PayCycle>();           
            var payCycleRecords = await GetAllPayCyclesAsync();
            if (payCycleRecords == null)
            {
                var message = string.Format("Unexpected: Bulk data read of pay cycle records returned null.");
                logger.Info(message);
                return new List<PayCycle>();
            }
            else
            {
                foreach (var payCycleRecord in payCycleRecords)
                {
                    if (payCycleRecord != null)
                    {
                        try
                        {
                            var payControlRecordsForThisCycle = payControlRecords.Where(pc => pc.Recordkey.EndsWith("*" + payCycleRecord.Recordkey));
                            // build the paycycle object (whose value will be referenced by the validPayCycleEntityToAdd variable)
                            var validPayCycleEntityToAdd = await BuildPayCycle(payCycleRecord, payControlRecordsForThisCycle, lookbackDate);
                            // if it was successfully created, we can add it to our list, and reset the variable
                            if (validPayCycleEntityToAdd != null)
                            {
                                payCycleEntities.Add(validPayCycleEntityToAdd);
                            }

                        }
                        catch (Exception e)
                        {
                            LogDataError("PAYCYCLE", payCycleRecord.Recordkey, payCycleRecord, e);
                        }
                    }
                }
            }
            return payCycleEntities;
        }

        /// <summary>
        /// Gets all reference pay cycles and caches them if not already cached
        /// </summary>
        /// <returns>List of Paycycle data contracts</returns>
        private async Task<IEnumerable<DataContracts.Paycycle>> GetAllPayCyclesAsync()
        {
            var allPayCycles = await GetOrAddToCacheAsync<IEnumerable<DataContracts.Paycycle>>("AllPayCycles",
                async () =>
                {
                    return await DataReader.BulkReadRecordAsync<DataContracts.Paycycle>("");                    
                }, Level1CacheTimeoutValue);
            return allPayCycles;
        }

        private PayPeriodStatus GetPayPeriodStatus(DataContracts.Paycntrl payControlRecord)
        {
            if (payControlRecord != null)
            {
                var lastProgram = payControlRecord.PclLastProgram;
                var currentProgram = payControlRecord.PclCurrProgram.FirstOrDefault();

                switch (lastProgram)
                {
                    case "1":
                        return PayPeriodStatus.Generated;
                    case "2":
                        return PayPeriodStatus.Calculation;
                    case "3":
                        return PayPeriodStatus.Print;
                    case "3I":
                        return PayPeriodStatus.Print;
                    case "4":
                        return PayPeriodStatus.Posting;
                    case "5":
                        return PayPeriodStatus.History;
                    case "9":
                        return PayPeriodStatus.Reversal;
                    default:
                        return PayPeriodStatus.Prepared;
                }
            }

            else
            {
                return PayPeriodStatus.New;
            }
        }

        private async Task<IEnumerable<PayCycleFrequency>> GetPayCycleFrequenciesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<PayCycleFrequency>>(PayCycleFrequencyCacheKey,
                async () =>
                  (await GetValcodeAsync<PayCycleFrequency>("HR", "TIME.FREQUENCIES",
                  tf =>
                  {
                      try
                      {
                          int annualPayFrequency;
                          if (int.TryParse(tf.ValActionCode1AssocMember, out annualPayFrequency))
                          {
                              return new PayCycleFrequency(tf.ValInternalCodeAssocMember, tf.ValExternalRepresentationAssocMember, annualPayFrequency);
                          }
                          LogDataError("TIME.FREQUENCIES", tf.ValInternalCodeAssocMember, tf, null, "ActionCode1 - AnnualPayFrequency - is not an integer");
                          return null;
                      }
                      catch (Exception e)
                      {
                          LogDataError("TIME.FREQUENCIES", tf.ValInternalCodeAssocMember, tf, e);
                          return null;
                      }
                  }
                  , Level1CacheTimeoutValue)).Where(pcf => pcf != null));
        }

        private async Task<PayCycle> BuildPayCycle(DataContracts.Paycycle payCycleRecord, IEnumerable<DataContracts.Paycntrl> payControlRecordsForThisCycle, DateTime? lookbackDate = null)
        {
            if (payCycleRecord == null)
            {
                throw new ArgumentNullException("payCycleRecord");
            }

            if (string.IsNullOrEmpty(payCycleRecord.PcyDesc))
            {
                throw new ArgumentNullException("Pay Cycle Description must have a value.");
            }
            var payPeriodEntities = new List<PayPeriod>();

            foreach (var payPeriodRecord in payCycleRecord.PayperiodsEntityAssociation)
            {
                try
                {
                    // if there is a lookback start date set: only add pays periods that have end dates AFTER that date
                    if (lookbackDate.HasValue && payPeriodRecord.PcyEndDateAssocMember.HasValue)
                    {
                        // date checking for end dates that are on or after the lookback date
                        if (DateTime.Compare(payPeriodRecord.PcyEndDateAssocMember.Value, lookbackDate.Value) >= 0) {
                            payPeriodEntities.Add(await BuildPayPeriod(payPeriodRecord, payCycleRecord, payControlRecordsForThisCycle));
                        }
                    }
                    // else if: check to see if there is a start and end date set. If so- add the pay period
                    else if (payPeriodRecord.PcyStartDateAssocMember.HasValue && payPeriodRecord.PcyEndDateAssocMember.HasValue)
                    {
                        payPeriodEntities.Add(await BuildPayPeriod(payPeriodRecord, payCycleRecord, payControlRecordsForThisCycle));
                    }
                }
                catch (Exception e)
                {
                    LogDataError("PAYCYCLE_PAYPERIODS", payCycleRecord.Recordkey, payPeriodRecord, e, e.Message);
                }
            }

            int annualPayFrequency = -1;
            var payCycleFrequencies = (await this.GetPayCycleFrequenciesAsync());
            if (payCycleFrequencies.Any(f => f.Code == payCycleRecord.PcyFrequency))
            {
                annualPayFrequency = payCycleFrequencies.FirstOrDefault(f => f.Code == payCycleRecord.PcyFrequency).AnnualPayFrequency;
            }

            // obtain the specified work week start day and time 
            // - first try the pay cycle definition, otherwise the hrss default
            DayOfWeek workWeekStartDay = (DayOfWeek)0;
            if (ConvertValueToDay(payCycleRecord.PcyWorkWeekStartDy, ref workWeekStartDay) ||
                ConvertValueToDay((await GetHRSSDefaultsAsync()).HrssDfltWorkWeekStartDy, ref workWeekStartDay))
            {
                //pay cycle description: self-service description field value if present, default otherwise
                string description = !string.IsNullOrEmpty(payCycleRecord.PcySelfServiceDesc) ? payCycleRecord.PcySelfServiceDesc : payCycleRecord.PcyDesc;
                return new PayCycle(payCycleRecord.Recordkey, description, workWeekStartDay)
                {
                    PayClassIds = payCycleRecord.PcyPayclasses,
                    PayPeriods = payPeriodEntities,
                    AnnualPayFrequency = annualPayFrequency,
                    DisplayInSelfService = !string.IsNullOrEmpty(payCycleRecord.PcyDisplayInSelfService) && payCycleRecord.PcyDisplayInSelfService.ToUpper() == "Y"
                };
            }
            else // make a note if the default cannot be obtained.  no pay cycle will be added in this scenario
            {
                var message = string.Format("Unable to determine work week start day from provided data: {0}, {1}",
                    payCycleRecord.PcyWorkWeekStartDy,
                    (await GetHRSSDefaultsAsync()).HrssDfltWorkWeekStartDy
                );
                logger.Info(message);
                throw new Exception(message);
            }
        }

        private async Task<PayPeriod> BuildPayPeriod(DataContracts.PaycyclePayperiods payPeriodRecord, DataContracts.Paycycle payCycleRecord, IEnumerable<DataContracts.Paycntrl> payControlRecordsForThisCycle)
        {
            if (!payPeriodRecord.PcyStartDateAssocMember.HasValue)
            {
                throw new ArgumentException("Pay Period Start Date must have a value.", "payPeriodRecord");
            }

            else if (!payPeriodRecord.PcyEndDateAssocMember.HasValue)
            {
                throw new ArgumentException("Pay Period End Date must have a value.", "payPeriodRecord");
            }

            else
            {
                // get the first pay control record that matches this start date for the pay period...
                var payControlRecord = payControlRecordsForThisCycle.FirstOrDefault(pc => pc.PclPeriodStartDate == payPeriodRecord.PcyStartDateAssocMember.Value);

                if (payControlRecord == null)
                {
                    var message = string.Format("No matching pay control record for payperiod record with PcyStartDateAssocMember {0}", payPeriodRecord.PcyStartDateAssocMember.ToString());
                    logger.Info(message);
                }

                // get the employee cutoff dates from the pay control if available, otherwise get them from defaults
                DateTimeOffset? employeeCutoffDate = null;
                if (payControlRecord != null && payControlRecord.PclEmployeeCutoffDate != null && payControlRecord.PclSupervisorCutoffTime != null)
                {
                    employeeCutoffDate = payControlRecord.PclEmployeeCutoffTime.ToPointInTimeDateTimeOffset(payControlRecord.PclEmployeeCutoffDate, colleagueTimeZone);
                }
                else
                {
                    var defaults = (await GetHRWebDefaultsAsync()).EmplcutoffEntityAssociation.FirstOrDefault(cutoff => cutoff.HrwebEmplPaycycleAssocMember == payCycleRecord.Recordkey);
                    if (defaults != null)
                    {
                        employeeCutoffDate = FindCutoffDateTime(
                            payPeriodRecord.PcyEndDateAssocMember.Value,
                            defaults.HrwebEmplBeforeDaysAssocMember,
                            defaults.HrwebEmplAfterDaysAssocMember,
                            defaults.HrwebEmplCutoffTimeAssocMember
                            );
                    }
                }
                // get the supervisor cutoff dates from the pay control if available, otherwise get them from defaults
                DateTimeOffset? supervisorCutoffDate = null;
                if (payControlRecord != null && payControlRecord.PclSupervisorCutoffDate != null && payControlRecord.PclSupervisorCutoffTime != null)
                {
                    supervisorCutoffDate = payControlRecord.PclSupervisorCutoffTime.ToPointInTimeDateTimeOffset(payControlRecord.PclSupervisorCutoffDate, colleagueTimeZone);
                }
                else
                {
                    var defaults = (await GetHRWebDefaultsAsync()).SupercutoffEntityAssociation.FirstOrDefault(cutoff => cutoff.HrwebSupervisorPaycycleAssocMember == payCycleRecord.Recordkey);
                    if (defaults != null)
                    {
                        supervisorCutoffDate = FindCutoffDateTime(
                            payPeriodRecord.PcyEndDateAssocMember.Value,
                            defaults.HrwebSupervisorBeforeDaysAssocMember,
                            defaults.HrwebSupervisorAfterDaysAssocMember,
                            defaults.HrwebSupervisorCutoffTimeAssocMember
                            );
                    }
                }

                // get the status based on the pay control record found...
                var payPeriodStatus = GetPayPeriodStatus(payControlRecord);

                // add this pay period's dates and status to the pay cycle...
                var payPeriodEntity = new PayPeriod(payPeriodRecord.PcyStartDateAssocMember.Value, payPeriodRecord.PcyEndDateAssocMember.Value, employeeCutoffDate, supervisorCutoffDate, payPeriodStatus, payCycleRecord.Recordkey);
                return payPeriodEntity;
            }
        }

        private async Task<DataContracts.HrwebDefaults> GetHRWebDefaultsAsync()
        {
            return await GetOrAddToCacheAsync<DataContracts.HrwebDefaults>(HRWebDefaultsCacheKey, async () =>
            {
                var hrwebDefaults = await DataReader.ReadRecordAsync<Data.HumanResources.DataContracts.HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");

                if (hrwebDefaults == null)
                {
                    var message = "Unable to find HrWebDefaults record";
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                };

                return hrwebDefaults;
            },
            Level1CacheTimeoutValue);
        }

        private async Task<DataContracts.HrssDefaults> GetHRSSDefaultsAsync()
        {
            return await GetOrAddToCacheAsync<DataContracts.HrssDefaults>(HRSSDefaultsCacheKey, async () =>
            {
                var hrssDefaults = await DataReader.ReadRecordAsync<Data.HumanResources.DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS");

                if (hrssDefaults == null)
                {
                    var message = "Unable to find HrWebDefaults record";
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                };

                return hrssDefaults;
            },
            Level1CacheTimeoutValue);
        }

        private DateTime FindCutoffDateTime(DateTime payPeriodEndDate, int? beforeDays, int? afterDays, DateTime? cutoffTime)
        {
            var cutoffTimeSpan = cutoffTime.HasValue ? cutoffTime.Value.TimeOfDay : new TimeSpan(0);

            if (beforeDays != null)
            {
                return payPeriodEndDate.AddDays(-(double)beforeDays).Add(cutoffTimeSpan);
            }
            else
            {
                return payPeriodEndDate.AddDays((double)afterDays).Add(cutoffTimeSpan);
            }
        }

        private bool ConvertValueToDay(string value, ref DayOfWeek day)
        {
            switch (value.ToUpper())
            {
                case "SU":
                    day = DayOfWeek.Sunday;
                    return true;
                case "M":
                    day = DayOfWeek.Monday;
                    return true;
                case "T":
                    day = DayOfWeek.Tuesday;
                    return true;
                case "W":
                    day = DayOfWeek.Wednesday;
                    return true;
                case "TH":
                    day = DayOfWeek.Thursday;
                    return true;
                case "F":
                    day = DayOfWeek.Friday;
                    return true;
                case "S":
                    day = DayOfWeek.Saturday;
                    return true;
                default:
                    return false;
            }
        }
    }
}
