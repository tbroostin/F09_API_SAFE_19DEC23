/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

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
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using System.Data;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Interact with Positions data from database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ContributionPayrollDeductionsRepository : BaseColleagueRepository, IContributionPayrollDeductionsRepository
    {
        private const string ContributionPayrollDeductionsCacheKey = "ContributionPayrollDeductions";
        private readonly int _bulkReadSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public ContributionPayrollDeductionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        RepositoryException exception = null;

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        public async Task<string> GetKeyFromGuidAsync(string guid)
        {
            // return await GetRecordKeyFromGuidAsync(guid);
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction GUID ", guid, " not found."));
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction GUID ", guid, " not found."));
            }

            if (foundEntry.Value.Entity != "PAYTODAT")
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("GUID.Wrong.Type", "GUID '" + guid + "' has different entity, '" + foundEntry.Value.Entity + "', than expected, 'PAYTODAT'"));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        public async Task<string> GetArrangementKeyFromGuidAsync(string guid)
        {
            // return await GetRecordKeyFromGuidAsync(guid);
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Concat("payroll-deduction-arrangements GUID ", guid, " not found."));
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Concat("payroll-deduction-arrangements GUID ", guid, " not found."));
            }

            if (foundEntry.Value.Entity != "PERBEN")
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("GUID.Wrong.Type", "GUID '" + guid + "' has different entity, '" + foundEntry.Value.Entity + "', than expected, 'PERBEN'"));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        ///  Get a collection of PayrollDeduction domain entity objects
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="arrangement"></param>
        /// <param name="bypassCache"></param>
        /// <returns>collection of PayrollDeduction domain entity objects</returns>
        public async Task<Tuple<IEnumerable<PayrollDeduction>, int>> GetContributionPayrollDeductionsAsync(int offset, int limit,
            string arrangement = "", string deductedOn = "", Dictionary<string, string> filterQualifiers = null, bool bypassCache = false)
        {
            var totalCount = 0;
            var payrollDeductionEntities = new List<PayrollDeduction>();
            string criteria = "";
            string[] limitingKeys = null;
            var deductedOnOperation = filterQualifiers != null && filterQualifiers.ContainsKey("DeductedOn") ? filterQualifiers["DeductedOn"] : "EQ";

            if (!string.IsNullOrEmpty(arrangement))
            {
                limitingKeys = await DataReader.SelectAsync("PERBEN", string.Concat("WITH PERBEN.ID EQ '", arrangement, "'"));
            }
            criteria = string.Concat(criteria, "WITH PERBEN.INTG.INTERVAL NE '' " +
                                               "OR WITH PERBEN.INTG.MO.PAY.PERIODS NE ''");

            var perbenIds = await DataReader.SelectAsync("PERBEN", limitingKeys, criteria);

            if (perbenIds == null)
                return new Tuple<IEnumerable<PayrollDeduction>, int>(new List<PayrollDeduction>(), 0);


            var payToDatIds = await DataReader.SelectAsync("PAYTODAT", "WITH PTD.PERBEN.ID EQ ? ",
                perbenIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

            if (!string.IsNullOrEmpty(deductedOn))
            {
                var newDeductedOn = await GetUnidataFormattedDate(deductedOn);
                if (!string.IsNullOrEmpty(newDeductedOn))
                {
                    criteria = string.Concat("WITH PTD.CHECK.DATE NE '' AND WITH PTD.ADVICE.DATE EQ '' AND WITH PTD.CHECK.DATE ", deductedOnOperation, " '", newDeductedOn, "'");
                    var newCheckPayToDatIds = (await DataReader.SelectAsync("PAYTODAT", payToDatIds, criteria)).ToList();

                    criteria = string.Concat("WITH PTD.ADVICE.DATE NE '' AND WITH PTD.CHECK.DATE EQ '' AND WITH PTD.ADVICE.DATE ", deductedOnOperation, " '", newDeductedOn, "'");
                    var newAdvicePayToDatIds = (await DataReader.SelectAsync("PAYTODAT", payToDatIds, criteria)).ToList();

                    criteria = string.Concat("WITH PTD.ADVICE.DATE EQ '' AND WITH PTD.CHECK.DATE EQ '' AND WITH PTD.PERIOD.DATE ", deductedOnOperation, " '", newDeductedOn, "'");
                    var newDefaultPayToDatIds = (await DataReader.SelectAsync("PAYTODAT", payToDatIds, criteria)).ToList();

                    payToDatIds = newCheckPayToDatIds.Union(newAdvicePayToDatIds).Union(newDefaultPayToDatIds).Distinct().ToArray();
                }
            }

            if (payToDatIds == null || !payToDatIds.Any())
            {
                return new Tuple<IEnumerable<PayrollDeduction>, int>(new List<PayrollDeduction>(), 0);
            }

            var payToDatRecords = await DataReader.BulkReadRecordAsync<Paytodat>("PAYTODAT", payToDatIds);

            if (payToDatRecords == null)
            {
                return new Tuple<IEnumerable<PayrollDeduction>, int>(new List<PayrollDeduction>(), 0);
            }
            
            var payToDatCollection = new List<Paytodat>(payToDatRecords);
            foreach (var payToDatRecord in payToDatRecords)
            {
                var ptdPerbenIds = new List<string>(payToDatRecord.PtdPerbenId);
                var payToDat = payToDatCollection.FirstOrDefault(x => x.Recordkey == payToDatRecord.Recordkey);
                foreach (var perbenId in ptdPerbenIds)
                {
                    if (!perbenIds.ToList().Contains(perbenId))
                    {
                        payToDat.PtdPerbenId.Remove(perbenId);
                    }
                }
            }

            var keys = new List<string>();
            foreach (var payToDatRecord in payToDatCollection)
            {
                keys.AddRange(payToDatRecord.PtdPerbenId.Select(perbenId => string.Concat(payToDatRecord.Recordkey, "|", perbenId)));
            }

            totalCount = keys.Count();
            keys.Sort();
            //Array.Sort(keys.ToArray());
            var keysSubList = keys.Skip(offset).Take(limit).ToArray();

            if (keysSubList.Any())
            {
                var perbens = await DataReader.BulkReadRecordAsync<Perben>("PERBEN", perbenIds);

                foreach (var key in keysSubList)
                {

                    var payToDatKey = key.Split('|');
                    var payToDat = payToDatCollection.FirstOrDefault(x => x.Recordkey == payToDatKey[0] && x.PtdPerbenId.Contains(payToDatKey[1]));     //( perbn => perbn == payToDatKey[1]);
                    try
                    {
                        payrollDeductionEntities.Add(await BuildPayrollDeductionsAsync(payToDat, perbens, payToDatKey[1]));
                    }
                    catch (Exception ex)
                    {
                        if (exception == null)
                            exception = new RepositoryException();

                        exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                    }

                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<PayrollDeduction>, int>(payrollDeductionEntities, totalCount);

        }

        /// <summary>
        /// Get Position by a guid
        /// </summary>
        /// <param name="guid">Guids</param>
        /// <returns>Positions entity objects</returns>
        public async Task<PayrollDeduction> GetContributionPayrollDeductionByGuidAsync(string payToDatGuid)
        {
            //PayrollDeduction payrollDeduction = null;
            var payToDatId = string.Empty;
            //var payToDatGuid = string.Empty;
            var arrangementId = string.Empty;
            // var arrangementGuid = string.Empty;
            DateTime deductionDate = default(DateTime);
            Decimal amountValue = default(decimal);
            string amountCurrency = string.Empty;

            if (string.IsNullOrEmpty(payToDatGuid))
            {
                throw new RepositoryException("Guid is a required field.");
            }

            //get paytodat guid info
            var payToDatGuidInfo = await GetRecordInfoFromGuidAsync(payToDatGuid);
           
            //if it is null there is not contribution-payroll-deduction for this guid
            if (payToDatGuidInfo == null)
            {
                throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction GUID ", payToDatGuid, " not found."));

            }


            if (string.IsNullOrEmpty(payToDatGuidInfo.PrimaryKey))
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "contribution-payroll-deduction GUID '", payToDatGuid, "' primary colleague key was not found on ldm_guid record..")
                {
                    Id = payToDatGuid,
                    SourceId = payToDatId
                });
            }
            else if (payToDatGuidInfo.Entity != "PAYTODAT")
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("GUID.Wrong.Type", "GUID '" + payToDatGuid + "' has different entity, '" + payToDatGuidInfo.Entity + "', than expected, 'PAYTODAT'"));
                throw exception;
            }
            else
            { 
                payToDatId = payToDatGuidInfo.PrimaryKey;
            }

            var payToDatCollRecord = await DataReader.ReadRecordAsync<Paytodat>("PAYTODAT", payToDatGuidInfo.PrimaryKey);

            if (payToDatCollRecord == null)
            {
                throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction GUID '", payToDatGuid, "' not found."));
            }

            //deduction date comes from either CheckDate or AdviceDate, only one can have a value
            if (payToDatCollRecord.PtdCheckDate.HasValue)
            {
                deductionDate = payToDatCollRecord.PtdCheckDate.Value;
            }
            if (payToDatCollRecord.PtdAdviceDate.HasValue)
            {
                deductionDate = payToDatCollRecord.PtdAdviceDate.Value;
            }
            //if there still no deduction date then use the Period date as the deduction date
            if (deductionDate == DateTime.MinValue)
            {
                var payToDatKey = payToDatCollRecord.Recordkey.Split('*');
                string periodDate = payToDatKey[0];

                //convert a unidata internal value into a datetime
                DateTime zeroDate = new DateTime(1967, 12, 31);
                double offsetDate = zeroDate.ToOADate();

                var date = DateTime.FromOADate(double.Parse(periodDate) + offsetDate);

                deductionDate = date;
            }

            //now we have to get the actual Ptdbnded record the guid is associated to, this lets us get everything else correctly
            int indexForPtdbnded = -1;
            if (payToDatCollRecord.PtdbndedEntityAssociation != null && payToDatCollRecord.PtdbndedEntityAssociation.Any())
            {
                indexForPtdbnded = payToDatCollRecord.PtdbndedEntityAssociation.FindIndex(
                    i => i.PtdBdCodesAssocMember == payToDatGuidInfo.SecondaryKey);
                //nothing was found, not a valid record
                if (indexForPtdbnded == -1)
                {
                   if (exception == null)
                        exception = new RepositoryException();

                    exception.AddError(new RepositoryError("Bad.Data", "There are no matching Ptdbnded associations.")
                    {
                        Id = payToDatGuid,
                        SourceId = payToDatId
                    });
                }

                arrangementId = payToDatCollRecord.PtdbndedEntityAssociation[indexForPtdbnded].PtdPerbenIdAssocMember;
            }
            else
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "There are no matching Ptdbnded associations.")
                {
                    Id = payToDatGuid,
                    SourceId = payToDatId
                });
            }

            //get the amount from the matching index position
            if ((payToDatCollRecord.PtdBdEmplyeCalcAmts != null && payToDatCollRecord.PtdBdEmplyeCalcAmts.Count > indexForPtdbnded)
                && (payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].HasValue))
            {
                amountValue = payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].Value;
            }
            else
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "There is no amount for this record.")
                {
                    Id = payToDatGuid,
                    SourceId = payToDatId
                });
            }
            
            //set the currency country
            amountCurrency = await GetHostCountryAsync();

            if (!string.IsNullOrEmpty(arrangementId))
            {
                //has either the PERBEN.INTG.INTERVAL or PERBEN.INTG.MON.PAY.PERIODS 
                var perBenRecord = await DataReader.ReadRecordAsync<Perben>("PERBEN", arrangementId);
                if (!((perBenRecord.PerbenIntgInterval.HasValue) ||
                      (perBenRecord.PerbenIntgMonPayPeriods != null && perBenRecord.PerbenIntgMonPayPeriods.Any())))
                {
                    if (exception == null)
                        exception = new RepositoryException();

                    exception.AddError(new RepositoryError("Bad.Data", "Missing either the PERBEN.INTG.INTERVAL or PERBEN.INTG.MON.PAY.PERIODS")
                    {
                        Id = payToDatGuid,
                        SourceId = payToDatId
                    });
                }
            }
            PayrollDeduction retval = null;
            try
            {
                retval = new PayrollDeduction(payToDatGuid, payToDatId, arrangementId, deductionDate, amountCurrency, amountValue);
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    Id = payToDatGuid,
                    SourceId = payToDatId
                });
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return retval;
        }

        public async Task<PayrollDeduction> BuildPayrollDeductionsAsync(Paytodat payToDatCollRecord, ICollection<Perben> perbens, string perbenId = "")
        {
            if (payToDatCollRecord == null)
            {
               if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "PayToDat is required."));
                return null;
            }

            if (string.IsNullOrEmpty(perbenId))
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "contribution-payroll-deduction perbenId not found.")
                {
                   
                    SourceId = payToDatCollRecord != null ? payToDatCollRecord.Recordkey : ""
                });
                return null;
            }

            var payToDatGuidInfo = string.Empty;

            

            if (perbens == null)
                perbens = await DataReader.BulkReadRecordAsync<Perben>("PERBEN", perbenId);

            var perben = perbens.FirstOrDefault(p => p.Recordkey == perbenId);
            var perbenBdId = (perben != null) ? perben.PerbenBdId : "";
            //Unique guids are generated for each paytodat and perben pair.   Each record shares the same payToDatCollRecord
            if (!string.IsNullOrEmpty(perbenBdId))
            {
                payToDatGuidInfo = await GetGuidFromRecordInfoAsync("PAYTODAT", payToDatCollRecord.Recordkey, "PTD.BD.CODES", perbenBdId);
            }

            //if it is null there is not contribution-payroll-deduction for this guid
            if (payToDatGuidInfo == null)
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "PAYTODAT GUID not found with secondary key PTD.BD.CODES.")
                {
                    Id = payToDatGuidInfo,
                    SourceId = payToDatCollRecord != null ? payToDatCollRecord.Recordkey : ""
                });
            }

            var payToDatId = payToDatCollRecord.Recordkey;
            //var payToDatGuid = payToDatGuidInfo;
            var arrangementId = string.Empty;
            // var arrangementGuid = string.Empty;
            DateTime deductionDate = default(DateTime);
            Decimal amountValue = default(decimal);
            var amountCurrency = string.Empty;

            //deduction date comes from either CheckDate or AdviceDate, only one can have a value
            if (payToDatCollRecord.PtdCheckDate.HasValue)
            {
                deductionDate = payToDatCollRecord.PtdCheckDate.Value;
            }
            if (payToDatCollRecord.PtdAdviceDate.HasValue)
            {
                deductionDate = payToDatCollRecord.PtdAdviceDate.Value;
            }
            //if there still no deduction date then use the Period date as the deduction date
            if (deductionDate == DateTime.MinValue)
            {
                var payToDatKey = payToDatCollRecord.Recordkey.Split('*');
                string periodDate = payToDatKey[0];

                //convert a unidata internal value into a datetime
                DateTime zeroDate = new DateTime(1967, 12, 31);
                double offsetDate = zeroDate.ToOADate();

                var date = DateTime.FromOADate(double.Parse(periodDate) + offsetDate);

                deductionDate = date;
            }

            //now we have to get the actual Ptdbnded record the guid is associated to, this lets us get everything else correctly
            int indexForPtdbnded = -1;
            if (!string.IsNullOrEmpty(perbenId) && payToDatCollRecord.PtdbndedEntityAssociation != null && payToDatCollRecord.PtdbndedEntityAssociation.Any())
            {
                
                indexForPtdbnded = payToDatCollRecord.PtdbndedEntityAssociation.FindIndex(
                    i => i.PtdPerbenIdAssocMember == perbenId); //payToDatCollRecord.PtdPerbenId[0]);
                //nothing was found, not a valid record
                if (indexForPtdbnded == -1)
                {
                   if (exception == null)
                        exception = new RepositoryException();

                    exception.AddError(new RepositoryError("Bad.Data", "There are no matching Ptdbnded associations.")
                    {
                        Id = payToDatGuidInfo,
                        SourceId = payToDatCollRecord != null ? payToDatCollRecord.Recordkey : ""
                    });
                }
                arrangementId = payToDatCollRecord.PtdbndedEntityAssociation[indexForPtdbnded].PtdPerbenIdAssocMember;
                // arrangementId = payToDatCollRecord.PtdbndedEntityAssociation[indexForPtdbnded].PtdBendedcsIdAssocMember;
            }
            else
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "There are no matching Ptdbnded associations.")
                {
                    Id = payToDatGuidInfo,
                    SourceId = payToDatCollRecord != null ? payToDatCollRecord.Recordkey : ""
                });
            }

            //get the amount from the matching index position
            if ((payToDatCollRecord.PtdBdEmplyeCalcAmts != null && payToDatCollRecord.PtdBdEmplyeCalcAmts.Count > indexForPtdbnded)
                && (payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].HasValue))
            {
                amountValue = payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].Value;
            }
            else
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "There is no amount for this record.")
                {
                    Id = payToDatGuidInfo,
                    SourceId = payToDatCollRecord != null ? payToDatCollRecord.Recordkey : ""
                });
            }
            
            //set the currency country
            amountCurrency = await GetHostCountryAsync();

            if (!string.IsNullOrEmpty(arrangementId))
            {
                //has either the PERBEN.INTG.INTERVAL or PERBEN.INTG.MON.PAY.PERIODS 
                var perBenRecord = await DataReader.ReadRecordAsync<Perben>("PERBEN", arrangementId);
                if (!((perBenRecord.PerbenIntgInterval.HasValue) ||
                      (perBenRecord.PerbenIntgMonPayPeriods != null && perBenRecord.PerbenIntgMonPayPeriods.Any())))
                {
                    if (exception == null)
                        exception = new RepositoryException();

                    exception.AddError(new RepositoryError("Bad.Data", "Missing either the PERBEN.INTG.INTERVAL or PERBEN.INTG.MON.PAY.PERIODS.")
                    {
                        Id = payToDatGuidInfo,
                        SourceId = payToDatCollRecord != null ? payToDatCollRecord.Recordkey : ""
                    });

                }
            }

            PayrollDeduction retval = null;
            try
            {
                retval = new PayrollDeduction(payToDatGuidInfo, payToDatId, arrangementId, deductionDate, amountCurrency, amountValue);
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    Id = payToDatGuidInfo,
                    SourceId = payToDatCollRecord != null ? payToDatCollRecord.Recordkey : ""
                });
            }
            return retval; 
        }

        /// <summary>
        /// Using a collection of perben ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="perbenIds">collection of perben ids</param>
        /// <returns>Dictionary consisting of a personId (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetPerbenGuidsCollectionAsync(IEnumerable<string> perbenIds)
        {
            if ((perbenIds == null) || (perbenIds != null && !perbenIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var perbenGuidCollection = new Dictionary<string, string>();

            var perbenGuidLookup = perbenIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("PERBEN", p, false)).ToArray();
            var recordKeyLookupResults = await DataReader.SelectAsync(perbenGuidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!perbenGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        perbenGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch(Exception ex)
                {
                    // do not throw error
                    logger.Error(ex, "Unable to get perben by guid.");
                }
            }

            return perbenGuidCollection;
        }


        private async Task<string> GetHostCountryAsync()
        {
            if (_internationalParameters == null)
                _internationalParameters = await GetInternationalParametersAsync();
            return _internationalParameters.HostCountry;
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