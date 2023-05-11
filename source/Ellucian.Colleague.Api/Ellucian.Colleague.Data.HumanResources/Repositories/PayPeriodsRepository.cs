//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using System.Linq;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayPeriodsRepository : BaseColleagueRepository, IPayPeriodsRepository
    {
        private static char _VM = Convert.ToChar(DynamicArray.VM);

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public PayPeriodsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region GET Method
        /// <summary>
        ///  Get a collection of PayPeriod domain entity objects
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>collection of PayPeriod domain entity objects</returns>
        public async Task<Tuple<IEnumerable<PayPeriod>, int>> GetPayPeriodsAsync(int offset, int limit, string payCycleCode = "",
            string convertedStartOn = "", string convertedEndOn = "", bool bypassCache = false)
        {
            var totalCount = 0;
            var payPeriodEntities = new List<PayPeriod>();
            var criteria = "WITH PCY.START.DATE NE ''";

            //startOn - select any records in PAYCYCLE using PCY.START.DATE
            //endOn - select any records in PAYCYCLE using PCY.END.DATE 
            if (!string.IsNullOrEmpty(convertedStartOn) || !string.IsNullOrEmpty(convertedEndOn))
            {
                if (!string.IsNullOrEmpty(convertedStartOn))
                {
                    criteria = string.Concat(criteria, " AND WITH PCY.START.DATE EQ '");
                    criteria = string.Concat(criteria, convertedStartOn);
                    criteria = string.Concat(criteria, "'");
                }
                if (!string.IsNullOrEmpty(convertedEndOn))
                {
                    criteria = string.Concat(criteria, " AND WITH PCY.END.DATE EQ '");
                    criteria = string.Concat(criteria, convertedEndOn);
                    criteria = string.Concat(criteria, "'");
                }
            }

            //payCycle - select any record in PAYCYCLE where the PAYCYCLE.ID matches the payCycle specified in the filter
            if (!string.IsNullOrEmpty(payCycleCode))
            {
                criteria = string.Concat(criteria, " AND WITH PAYCYCLE.ID = '");
                criteria = string.Concat(criteria, payCycleCode);
                criteria = string.Concat(criteria, "'");
            }
            criteria = string.Concat(criteria, " BY.EXP PCY.START.DATE");

            var payCyclesIds = await DataReader.SelectAsync("PAYCYCLE", criteria);

            criteria = string.Concat(criteria, " SAVING PCY.START.DATE");
            var payCycleDates = await DataReader.SelectAsync("PAYCYCLE", criteria);

            var payCyclesIds2 = new List<string>();
            int index = 0;
            foreach (var payCycleId in payCyclesIds)
            {
                var payCycleKey = payCycleId.Split(_VM)[0] + '*' + payCycleDates[index];
                payCyclesIds2.Add(payCycleKey);
                index++;
            }

            totalCount = payCyclesIds2.Count();
            payCyclesIds2.Sort();
            
            var keysSubList = payCyclesIds2.Skip(offset).Take(limit).ToArray().Distinct();
            if (!string.IsNullOrEmpty(convertedStartOn) || !string.IsNullOrEmpty(convertedEndOn))
            {
                // If we are filtering on starton or endon then we need
                // to process all records since we require data evaluation
                // before we actually know which records are to be processed.
                keysSubList = payCyclesIds2.ToArray().Distinct();
            }

            if (keysSubList.Any())
            {
                var keysPayCycleSubList = keysSubList.Select(k => k.Split('*')[0]).Distinct();
                var payCyclesCollection = await DataReader.BulkReadRecordAsync<Paycycle>("PAYCYCLE", keysPayCycleSubList.ToArray());

                List<string> payCntrlKeys = new List<string>();

                if (payCyclesCollection != null && payCyclesCollection.Any())
                {
                    payCyclesCollection.ToList().ForEach(pc =>
                        {
                            foreach (var p in pc.PcyEndDate)
                            {
                                if (p.HasValue)
                                    payCntrlKeys.Add(string.Concat(DmiString.DateTimeToPickDate((DateTime) p), "*", pc.Recordkey));
                            }
                        });
                }

                var payCentralCollection = await DataReader.BulkReadRecordAsync<Paycntrl>("PAYCNTRL", payCntrlKeys.ToArray());

                var exception = new RepositoryException();
                var totalProcessed = 0;
                foreach (var keyPlusDate in keysSubList)
                {
                    var key = keyPlusDate.Split('*')[0];
                    var dateSelected = DmiString.PickDateToDateTime(Convert.ToInt16(keyPlusDate.Split('*')[1]));
                    var payCycles = payCyclesCollection.FirstOrDefault(x => x.Recordkey == key);
                    var effectiveStartDate = DateTime.MinValue;
                    var effectiveEndDate = DateTime.MinValue;

                    try
                    {
                        index = 0;
                        foreach (var payCyclesDateIndex in payCycles.PcyStartDate)
                        {
                            if (payCyclesDateIndex.Value.Date == dateSelected.Date)
                            {
                                var compareStartDate = payCyclesDateIndex;
                                if (payCycles.PcyEndDate != null && payCycles.PcyEndDate.Count > index && payCycles.PcyEndDate.ElementAt(index) != null)
                                {
                                    var compareEndDate = payCycles.PcyEndDate[index];

                                    if (!string.IsNullOrEmpty(convertedStartOn) || !string.IsNullOrEmpty(convertedEndOn))
                                    {
                                        if (!string.IsNullOrEmpty(convertedStartOn))
                                        {
                                            try
                                            {
                                                compareStartDate = DateTime.Parse(convertedStartOn).Date;
                                            }
                                            catch
                                            {
                                                compareStartDate = payCyclesDateIndex;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(convertedEndOn))
                                        {
                                            try
                                            {
                                                compareEndDate = DateTime.Parse(convertedEndOn).Date;
                                            }
                                            catch
                                            {
                                                compareEndDate = payCycles.PcyEndDate[index];
                                            }
                                        }
                                    }
                                    if (payCycles.PcyStartDate[index] == compareStartDate && payCycles.PcyEndDate[index] == compareEndDate)
                                    {
                                        totalProcessed++;
                                        effectiveStartDate = Convert.ToDateTime(payCyclesDateIndex);
                                        //convert a datetime to a unidata internal value 
                                        var offsetStartDate = DmiString.DateTimeToPickDate(effectiveStartDate);

                                        //convert a datetime to a unidata internal value 
                                        var offsetEndDate = DmiString.DateTimeToPickDate((DateTime)payCycles.PcyEndDate[index]);


                                        var payPeriodGuidInfo = await GetGuidFromIdAsync("PAYCYCLE", payCycles.Recordkey, "PCY.START.DATE", offsetStartDate.ToString());
                                        DateTime? timeEntryEndOnDate = null;
                                        DateTime? timeEntryEndOnTime = null;
                                        var timeEntryEndOnConcat = "";
                                        DateTime? timeEntryEndOnDateTime = null;

                                        var pyCntrlRecord = payCentralCollection.FirstOrDefault(p => (p.Recordkey.Equals(string.Concat(offsetEndDate, "*", key))));

                                        if (pyCntrlRecord != null)
                                        {
                                            timeEntryEndOnDate = pyCntrlRecord.PclEmployeeCutoffDate;
                                            timeEntryEndOnTime = pyCntrlRecord.PclEmployeeCutoffTime;
                                            if (timeEntryEndOnDate != null && timeEntryEndOnTime != null)
                                            {
                                                DateTime dateTimeValue;
                                                timeEntryEndOnConcat = string.Concat(timeEntryEndOnDate.Value.ToShortDateString(), " ", timeEntryEndOnTime.Value.ToShortTimeString());
                                                if (DateTime.TryParse(timeEntryEndOnConcat, out dateTimeValue))
                                                {
                                                    timeEntryEndOnDateTime = dateTimeValue;
                                                }
                                            }
                                        }
                                        var paycycleValid = true;
                                        // check for required data for PayPeriod entity
                                        if (string.IsNullOrEmpty(payPeriodGuidInfo))
                                        {
                                            exception.AddError(new RepositoryError("GUID.Not.Found", string.Concat("GUID not found for pay-periods for paycycle ", key, " for ", dateSelected, "."))
                                            {
                                                Id = key
                                            });
                                            paycycleValid = false;
                                        }
                                        if (string.IsNullOrEmpty(payCycles.PcyDesc))
                                        {
                                            exception.AddError(new RepositoryError("Bad.Data", string.Concat("Description not found for pay-periods for paycycle ", key, "."))
                                            {
                                                Id = key
                                            });
                                            paycycleValid = false;
                                        }
                                        if (payCycles.PcyStartDate[index] == null)
                                        {
                                            // should never happen since previous logic to extract data is on existing values in start date
                                            exception.AddError(new RepositoryError("Bad.Data", string.Concat("Start date not found for pay-periods for paycycle ", key, " at position ", index, "."))
                                            {
                                                Id = key
                                            });
                                            paycycleValid = false;
                                        }
                                        if (payCycles.Recordkey == null)
                                        {
                                            // should never happen - should never have been able to read record without a key
                                            exception.AddError(new RepositoryError("Bad.Data", string.Concat("Record not found for pay-periods for paycycle ", key, "."))
                                            {
                                                Id = key
                                            });
                                            paycycleValid = false;
                                        }
                                        if (payCycles.PcyStartDate[index] != null && payCycles.PcyEndDate[index] != null && payCycles.PcyStartDate[index] > payCycles.PcyEndDate[index])
                                        {
                                            exception.AddError(new RepositoryError("Bad.Data", string.Concat("Start date ", payCycles.PcyStartDate[index],
                                                " cannot be greater than end date ", payCycles.PcyEndDate[index], " for pay-periods for paycycle ", key, "."))
                                            {
                                                Id = key
                                            });
                                            paycycleValid = false;
                                        }

                                        if (paycycleValid == true)
                                        {
                                            payPeriodEntities.Add(new PayPeriod(payPeriodGuidInfo, payCycles.PcyDesc, (DateTime)payCycles.PcyStartDate[index], (DateTime)payCycles.PcyEndDate[index], (DateTime)payCycles.PcyPaycheckDate[index], payCycles.Recordkey)
                                            {
                                                TimeEntryEndOn = timeEntryEndOnDateTime != null ? timeEntryEndOnDateTime : null
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    exception.AddError(new RepositoryError("Bad.Data", string.Concat("End date not found for pay-periods for paycycle ", key, " with start date ", payCycles.PcyStartDate[index], "."))
                                    {
                                        Id = key
                                    });
                                }
                            }
                            index++;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ColleagueWebApiException(e.Message);
                    }
                    // Because there is no way to select end date associated to the start date
                    // the total count could be off.  We may have selected records with a start
                    // date that matches and with an end date that matches, but they may not be
                    // associated, therefore the total counts could be off when we have both as
                    // a filter.  Adjust the total to be the number of items processed.
                    if (!string.IsNullOrEmpty(convertedStartOn) || !string.IsNullOrEmpty(convertedEndOn))
                    {
                        totalCount = totalProcessed;
                    }
                }
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
            }
            return new Tuple<IEnumerable<PayPeriod>, int>(payPeriodEntities, totalCount);

        }

        /// <summary>
        /// Returns a review for a specified Job Application key.
        /// </summary>
        /// <param name="ids">Key to Job Application to be returned</param>
        /// <returns>PayPeriod Objects</returns>
        public async Task<PayPeriod> GetPayPeriodByIdAsync(string id)
        {
            var payPeriodId = await GetRecordInfoFromGuidAsync(id);

            if (payPeriodId == null)
                throw new KeyNotFoundException();

            var payCycles = await DataReader.ReadRecordAsync<Paycycle>("PAYCYCLE", payPeriodId.PrimaryKey);

            List<string> payCntrlKeys = new List<string>();

            if (payCycles != null)
            {
                foreach (var p in payCycles.PcyEndDate)
                {
                    if (p.HasValue)
                        payCntrlKeys.Add(string.Concat(DmiString.DateTimeToPickDate((DateTime)p), "*", payCycles.Recordkey));
                }
            }
            else
            {
                throw new KeyNotFoundException();
            }

            var payCentralCollection = await DataReader.BulkReadRecordAsync<Paycntrl>("PAYCNTRL", payCntrlKeys.ToArray());

            int index = 0;
            var effectiveStartDate = DateTime.MinValue;

            foreach (var payCyclesDateIndex in payCycles.PcyStartDate)
            {
                effectiveStartDate = Convert.ToDateTime(payCyclesDateIndex);
                //convert a datetime to a unidata internal value 
                var offsetStartDate = DmiString.DateTimeToPickDate(effectiveStartDate);

                //convert a datetime to a unidata internal value 
                var offsetEndDate = DmiString.DateTimeToPickDate((DateTime)payCycles.PcyEndDate[index]);

                var payPeriodGuidInfo = await GetGuidFromIdAsync("PAYCYCLE", payCycles.Recordkey, "PCY.START.DATE", offsetStartDate.ToString());

                if (payPeriodGuidInfo.Equals(id))
                {
                    DateTime? timeEntryEndOnDate = null;
                    DateTime? timeEntryEndOnTime = null;
                    var timeEntryEndOnConcat = "";
                    DateTime? timeEntryEndOnDateTime = null;

                    var pyCntrlRecord = payCentralCollection.FirstOrDefault(p => (p.Recordkey.Equals(string.Concat(offsetEndDate, "*", payCycles.Recordkey))));

                    if (pyCntrlRecord != null)
                    {
                        timeEntryEndOnDate = pyCntrlRecord.PclEmployeeCutoffDate;
                        timeEntryEndOnTime = pyCntrlRecord.PclEmployeeCutoffTime;
                        if (timeEntryEndOnDate != null && timeEntryEndOnTime != null)
                        {
                            DateTime dateTimeValue;
                            timeEntryEndOnConcat = string.Concat(timeEntryEndOnDate.Value.ToShortDateString(), " ", timeEntryEndOnTime.Value.ToShortTimeString());
                            if (DateTime.TryParse(timeEntryEndOnConcat, out dateTimeValue))
                            {
                                timeEntryEndOnDateTime = dateTimeValue;
                            }
                        }
                    }

                    return new PayPeriod(payPeriodGuidInfo, payCycles.PcyDesc, (DateTime)payCycles.PcyStartDate[index], (DateTime)payCycles.PcyEndDate[index], (DateTime)payCycles.PcyPaycheckDate[index], payCycles.Recordkey)
                    {
                        TimeEntryEndOn = timeEntryEndOnDateTime != null ? timeEntryEndOnDateTime : null
                    };
                }
                index++;
                //}
            }

            throw new KeyNotFoundException(String.Format("No pay period was found for guid '{0}'.", id));
        }

        #endregion
        
        /// <summary>
        /// Return a GUID for an Entity and Record Key
        /// </summary>
        /// <param name="entity">Entity Name</param>
        /// <param name="id">Record Key</param>
        /// <returns>GUID associated to the entity and key</returns>
        private async Task<string> GetGuidFromIdAsync(string entity, string id, string secondaryField = "", string secondaryKey = "")
        {
            if (!string.IsNullOrEmpty(entity) && !string.IsNullOrEmpty(id))
            {
                var lookup = new RecordKeyLookup(entity, id, secondaryField, secondaryKey, false);
                var result = await DataReader.SelectAsync(new RecordKeyLookup[] { lookup });
                if (result != null && result.Count > 0)
                {
                    RecordKeyLookupResult lookupResult = null;
                    if (result.TryGetValue(lookup.ResultKey, out lookupResult))
                    {
                        if (lookupResult != null)
                        {
                            return lookupResult.Guid;
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
