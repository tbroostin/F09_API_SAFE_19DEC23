/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */

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

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        public async Task<string> GetKeyFromGuidAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
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
            string arrangement = "", bool bypassCache = false)
        {
            var totalCount = 0;
            var payrollDeductionEntities = new List<PayrollDeduction>();
            var criteria = "";

            if (!string.IsNullOrEmpty(arrangement))
            {
                criteria = string.Concat(criteria, "WITH PERBEN.ID EQ '", arrangement, "'");
            }

            if (!string.IsNullOrEmpty(criteria))
            {
                criteria = string.Concat(criteria, " AND ");
            }
            criteria = string.Concat(criteria, "WITH PERBEN.INTG.INTERVAL NE '' " +
                                               "OR WITH PERBEN.INTG.MO.PAY.PERIODS NE ''");


            var perbenIds = await DataReader.SelectAsync("PERBEN", criteria.ToString());

            if (perbenIds == null)
                throw new KeyNotFoundException("No qualifying records selected from PERBEN in Colleague.");


            var payToDatIds = await DataReader.SelectAsync("PAYTODAT", "WITH PTD.PERBEN.ID EQ ? ",
                perbenIds.Select(id => string.Format("\"{0}\"", id)).ToArray());

            var payToDatRecords = await DataReader.BulkReadRecordAsync<Paytodat>("PAYTODAT", payToDatIds);
            {
                if (payToDatRecords == null)
                {
                    throw new KeyNotFoundException("No qualifying records selected from PAYTODAT in Colleague.");
                }
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
                    payrollDeductionEntities.Add(await BuildPayrollDeductionsAsync(payToDat, perbens, payToDatKey[1])); 
                }
            }
            return new Tuple<IEnumerable<PayrollDeduction>, int>(payrollDeductionEntities, totalCount);

        }

        /// <summary>
        /// Get Position by a guid
        /// </summary>
        /// <param name="guid">Guids</param>
        /// <returns>Positions entity objects</returns>
        public async Task<PayrollDeduction> GetContributionPayrollDeductionByGuidAsync(string guid)
        {
            PayrollDeduction payrollDeduction = null;
            var payToDatId = string.Empty;
            var payToDatGuid = string.Empty;
            var arrangementId = string.Empty;
            var arrangementGuid = string.Empty;
            DateTime deductionDate = default(DateTime);
            Decimal amountValue = default(decimal);
            string amountCurrency = string.Empty;

            if (!string.IsNullOrEmpty(guid))
            {
                //get paytodat guid info
                var payToDatGuidInfo = await GetRecordInfoFromGuidAsync(guid);
                //if it is null there is not contribution-payroll-deduction for this guid
                if (payToDatGuidInfo == null)
                {
                    throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction GUID ", guid, " not found."));
                }

                payToDatId = payToDatGuidInfo.PrimaryKey;
                payToDatGuid = guid;


                if (string.IsNullOrEmpty(payToDatGuidInfo.PrimaryKey))
                {
                    throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction GUID ", guid, " primary colleague key was not found on ldm_guid record."));
                }

                Paytodat payToDatCollRecord = await DataReader.ReadRecordAsync<Paytodat>("PAYTODAT", payToDatGuidInfo.PrimaryKey);


                //deduction date comes from either CheckDate or AdviceDate, only one can have a value
                if (payToDatCollRecord.PtdCheckDate.HasValue)
                {
                    deductionDate = payToDatCollRecord.PtdCheckDate.Value;
                }
                if (payToDatCollRecord.PtdAdviceDate.HasValue)
                {
                    deductionDate = payToDatCollRecord.PtdAdviceDate.Value;
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
                        throw new RepositoryException(string.Concat("There are no matching Ptdbnded associations, record", guid, " is not valid"));
                    }

                    arrangementId = payToDatCollRecord.PtdbndedEntityAssociation[indexForPtdbnded].PtdPerbenIdAssocMember;
                }
                else
                {
                    throw new RepositoryException(string.Concat("There are no Ptdbnded associations, record", guid, " is not valid"));
                }

                //get the amount from the matching index position
                if (payToDatCollRecord.PtdBdEmplyeCalcAmts != null && payToDatCollRecord.PtdBdEmplyeCalcAmts.Count > indexForPtdbnded)
                {
                    if (payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].HasValue)
                    {
                        amountValue = payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].Value;
                    }
                    else
                    {
                        throw new RepositoryException(string.Concat("There is no amount for this record", guid, " it is not valid"));
                    }
                }
                else
                {
                    throw new RepositoryException(string.Concat("There is no amount for this record", guid, " it is not valid"));
                }

                //set the currency country
                amountCurrency = await GetHostCountryAsync();


                //var perBenGuidInfo = await GetGuidFromRecordInfoAsync("PERBEN", arrangementId);

                //has either the PERBEN.INTG.INTERVAL or PERBEN.INTG.MON.PAY.PERIODS 
                var perBenRecord = await DataReader.ReadRecordAsync<Perben>("PERBEN", arrangementId);
                if (!((perBenRecord.PerbenIntgInterval.HasValue) ||
                      (perBenRecord.PerbenIntgMonPayPeriods != null && perBenRecord.PerbenIntgMonPayPeriods.Any())))
                {
                    throw new RepositoryException("No payroll deduction arrangement found for the contribution payroll deduction requested");

                }
                //get the guid for the arrangement
                var perBenGuidInfo = perBenRecord.RecordGuid;

                if (!string.IsNullOrEmpty(perBenGuidInfo))
                {
                    arrangementGuid = perBenGuidInfo;
                }

            }

            return new PayrollDeduction(payToDatGuid, payToDatId, arrangementId, arrangementGuid, deductionDate, amountCurrency, amountValue);
        }

        public async Task<PayrollDeduction> BuildPayrollDeductionsAsync(Paytodat payToDatCollRecord, ICollection<Perben> perbens, string perbenId = "")
        {
            if (payToDatCollRecord == null) 
            {
                throw new KeyNotFoundException("contribution-payroll-deduction not found.");
            }

            if (string.IsNullOrEmpty(perbenId))
            {
                throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction perbenId not found.", payToDatCollRecord.Recordkey));
            }

            var payToDatGuidInfo = string.Empty;

            if (perbens == null)
                perbens = await DataReader.BulkReadRecordAsync<Perben>("PERBEN");

            var perben = perbens.FirstOrDefault(p => p.Recordkey == perbenId);
            var perbenBdId = (perben != null) ? perben.PerbenBdId : "";
            //Unique guids are generated for each paytodat and perben pair.   Each record shares the same payToDatCollRecord
            if (!string.IsNullOrEmpty(perbenBdId))
            {
                payToDatGuidInfo = await GetGuidFromRecordInfoAsync("PAYTODAT", payToDatCollRecord.Recordkey, "PTD.BD.CODES", perbenBdId);
            }

            // var payToDatGuidInfo = await GetRecordInfoFromGuidAsync(payToDatCollRecord.RecordGuid);
            //if it is null there is not contribution-payroll-deduction for this guid
            if (payToDatGuidInfo == null)
            {
                throw new KeyNotFoundException(string.Concat("contribution-payroll-deduction GUID ", payToDatCollRecord.RecordGuid, " not found."));
            }

            var payToDatId = payToDatCollRecord.Recordkey;
            var payToDatGuid = payToDatGuidInfo;
            var arrangementId = string.Empty;
            var arrangementGuid = string.Empty;
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
                double  offsetDate = zeroDate.ToOADate();

                var date = DateTime.FromOADate(double.Parse(periodDate) + offsetDate);

                deductionDate = date;
            }

            //now we have to get the actual Ptdbnded record the guid is associated to, this lets us get everything else correctly
            int indexForPtdbnded = -1;
            if (!string.IsNullOrEmpty(perbenId) && payToDatCollRecord.PtdbndedEntityAssociation != null && payToDatCollRecord.PtdbndedEntityAssociation.Any())
            {
                //indexForPtdbnded = payToDatCollRecord.PtdbndedEntityAssociation.FindIndex(
                //        i => i.PtdBdCodesAssocMember == payToDatGuidInfo.SecondaryKey);

                indexForPtdbnded = payToDatCollRecord.PtdbndedEntityAssociation.FindIndex(
                    i => i.PtdPerbenIdAssocMember == perbenId); //payToDatCollRecord.PtdPerbenId[0]);
                //nothing was found, not a valid record
                if (indexForPtdbnded == -1)
                {
                    throw new RepositoryException(string.Concat("There are no matching Ptdbnded associations, record ", payToDatGuid, " is not valid"));
                }
                arrangementId = payToDatCollRecord.PtdbndedEntityAssociation[indexForPtdbnded].PtdPerbenIdAssocMember;
                // arrangementId = payToDatCollRecord.PtdbndedEntityAssociation[indexForPtdbnded].PtdBendedcsIdAssocMember;
            }
            else
            {
                throw new RepositoryException(string.Concat("There are no Ptdbnded associations, record", payToDatCollRecord.RecordGuid, " is not valid"));
            }

            //get the amount from the matching index position
            if (payToDatCollRecord.PtdBdEmplyeCalcAmts != null && payToDatCollRecord.PtdBdEmplyeCalcAmts.Count > indexForPtdbnded)
            {
                if (payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].HasValue)
                {
                    amountValue = payToDatCollRecord.PtdBdEmplyeCalcAmts[indexForPtdbnded].Value;
                }
                else
                {
                    throw new RepositoryException(string.Concat("There is no amount for this record", payToDatCollRecord.RecordGuid, " it is not valid"));
                }
            }
            else
            {
                throw new RepositoryException(string.Concat("There is no amount for this record", payToDatCollRecord.RecordGuid, " it is not valid"));
            }

            //set the currency country
            amountCurrency = await GetHostCountryAsync();


            //has either the PERBEN.INTG.INTERVAL or PERBEN.INTG.MON.PAY.PERIODS 
            var perBenRecord = await DataReader.ReadRecordAsync<Perben>("PERBEN", arrangementId);
            if (!((perBenRecord.PerbenIntgInterval.HasValue) ||
                  (perBenRecord.PerbenIntgMonPayPeriods != null && perBenRecord.PerbenIntgMonPayPeriods.Any())))
            {
                throw new RepositoryException("No payroll deduction arrangement found for the contribution payroll deduction requested");

            }
            //get the guid for the arrangement
            var perBenGuidInfo = perBenRecord.RecordGuid;

            if (!string.IsNullOrEmpty(perBenGuidInfo))
            {
                arrangementGuid = perBenGuidInfo;
            }

            return new PayrollDeduction(payToDatGuid, payToDatId, arrangementId, arrangementGuid, deductionDate, amountCurrency, amountValue);

            
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