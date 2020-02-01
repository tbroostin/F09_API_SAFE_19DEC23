/*Copyright 2017-2019 Ellucian Company L.P. and its affiliates.*/
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
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Contains methods to get PayrollRegisterEntry objects from the Payroll Register (PAYTODAT) records
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayrollRegisterRepository : BaseColleagueRepository, IPayrollRegisterRepository
    {
        private readonly int bulkReadSize;
        private const string PayrollRegisterCacheKeySuffix = "EmployeePayrollRegisterCache";
        private const string PayControlCacheKey = "PayControlCache";

        /// <summary>
        /// Repository Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PayrollRegisterRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;

        }

        /// <summary>
        /// Helper to build an employee specific cache key.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        private string BuildEmployeePayrollRegisterCacheKey(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            return string.Format("{0}-{1}", employeeId, PayrollRegisterCacheKeySuffix);
        }

        /// <summary>
        /// Get PayrollRegister entries for the given employee id
        /// </summary>
        /// <param name="employeeIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollRegisterEntry>> GetPayrollRegisterByEmployeeIdsAsync(IEnumerable<string> employeeIds, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (employeeIds == null || !employeeIds.Any())
            {
                return new List<PayrollRegisterEntry>();
            }

            employeeIds = employeeIds.Distinct();

            //this is the return list
            var payrollRegister = new List<PayrollRegisterEntry>();


            //build a set of tuples with
            //item1: personId
            //item2: full cache key
            var employeeIdCacheKeys = employeeIds
                .Select(id => new Tuple<string, string>(id, BuildFullCacheKey(BuildEmployeePayrollRegisterCacheKey(id))))
                .ToList();


            //first try to get employee paytodats from cache
            var keysInCache = employeeIdCacheKeys.Where(tuple => _cacheProvider.Contains(tuple.Item2));
            foreach (var entry in keysInCache)
            {
                try
                {
                    var cachedEntities = (IEnumerable<PayrollRegisterEntry>)_cacheProvider.Get(entry.Item2);
                    if (cachedEntities != null)
                    {
                        payrollRegister.AddRange(cachedEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Info(e, string.Format("Unable to get cached PayrollRegisterEntries from cacheKey {0}", entry.Item2));
                }
            }


            //now get remaining uncached employeeids
            //-----------------------------------------------------
            var uncachedEmployeeIds = employeeIdCacheKeys
                .Where(tuple => !_cacheProvider.Contains(tuple.Item2))
                .Select(tuple => tuple.Item1)
                .ToList();

            if (!uncachedEmployeeIds.Any())
            {
                return payrollRegister;
            }

            var criteria = "WITH PTD.EMPLOYEE.ID EQ ? AND PTD.SEQ.NO NE \"\"";
            var values = uncachedEmployeeIds.Select(id => string.Format("\"{0}\"", id)).ToArray();

            if (startDate.HasValue)
            {
                var uniDataStartDate = await GetUnidataFormatDateAsync(startDate.Value);
                criteria = string.Concat(criteria, string.Format(" AND WITH PTD.CHECK.ADVICE.DATE GE '{0}'", uniDataStartDate)); //PTD.CHECK.ADVICE.DATE is a computed column so we use the Pick Date here (as opposed to a date string, like in the PayStatementRepository
            }

            if (endDate.HasValue)
            {
                var uniDataEndDate = await GetUnidataFormatDateAsync(endDate.Value);
                criteria = string.Concat(criteria, string.Format(" AND WITH PTD.CHECK.ADVICE.DATE LE '{0}'", uniDataEndDate)); //PTD.CHECK.ADVICE.DATE is a computed column so we use the Pick Date here (as opposed to a date string, like in the PayStatementRepository
            }

            var paytodatRecords = await ReadPaytodatRecords(DataReader.SelectAsync("PAYTODAT", criteria, values));

            if (paytodatRecords == null || !paytodatRecords.Any())
            {
                return payrollRegister;
            }

            //group by employee id
            //item1 = paytodat
            //item2 = paycontrol
            var entryDictByEmployeeId = new Dictionary<string, List<PayrollRegisterEntry>>();
            foreach (var paytodatRecord in paytodatRecords)
            {
                try
                {
                    var entry = await BuildPayrollRegisterEntry(paytodatRecord);
                    if (entryDictByEmployeeId.ContainsKey(entry.EmployeeId))
                    {
                        entryDictByEmployeeId[entry.EmployeeId].Add(entry);
                    }
                    else
                    {
                        entryDictByEmployeeId.Add(entry.EmployeeId, new List<PayrollRegisterEntry>() { entry });
                    }
                }
                catch (Exception e)
                {
                    LogDataError("PAYTODAT", paytodatRecord.Recordkey, paytodatRecord, e);
                }
            }

            //create cache entries
            foreach (var entry in entryDictByEmployeeId)
            {
                var employeeEntries = GetOrAddToCache(BuildEmployeePayrollRegisterCacheKey(entry.Key),
                    () => entry.Value,
                    Level1CacheTimeoutValue);

                payrollRegister.AddRange(employeeEntries);

            }


            return payrollRegister;
        }


        /// <summary>
        /// Helper to bulk read paytodat records in chunks
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Paytodat>> ReadPaytodatRecords(Task<string[]> recordIdSelectTask)
        {
            var paytodatRecords = new List<Paytodat>();

            var paytodatIds = await recordIdSelectTask;
            if (paytodatIds == null || !paytodatIds.Any())
            {
                logger.Info("DataReader selected zero PAYTODAT records");
                return paytodatRecords;
            }

            for (int i = 0; i < paytodatIds.Length; i += bulkReadSize)
            {
                var chunkedIds = paytodatIds.Skip(i).Take(bulkReadSize);
                var chunkedRecords = await DataReader.BulkReadRecordAsync<Paytodat>(chunkedIds.ToArray());
                if (chunkedRecords != null)
                {
                    paytodatRecords.AddRange(chunkedRecords);
                }
            }

            return paytodatRecords;
        }


        /// <summary>
        /// Helper to bulk read PayControl records
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private async Task<IDictionary<string, Paycntrl>> ReadPaycntrlRecords()
        {

            var records = await GetOrAddToCacheAsync(PayControlCacheKey,
                async () =>
                {
                    var payControlRecords = new List<Paycntrl>();
                    var allIds = await DataReader.SelectAsync("PAYCNTRL", "");
                    for (int i = 0; i < allIds.Count(); i += bulkReadSize)
                    {
                        var chunkedIds = allIds.Skip(i).Take(bulkReadSize);
                        var chunkedRecords = await DataReader.BulkReadRecordAsync<Paycntrl>(chunkedIds.ToArray());
                        if (chunkedRecords != null)
                        {
                            payControlRecords.AddRange(chunkedRecords);
                        }
                    }

                    return payControlRecords.ToDictionary(p => p.Recordkey);
                }, Level1CacheTimeoutValue);

            return records;
        }

        /// <summary>
        /// Helper to build a PayrollRegisterEntry object from a db record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private async Task<PayrollRegisterEntry> BuildPayrollRegisterEntry(Paytodat record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            DateTime periodEndDate;
            string payCycleId;
            string employeeId;
            int sequenceNumber;

            string payControlKey;

            try
            {
                var keyParts = record.Recordkey.Split('*');
                var internalEndDateString = keyParts[0];
                payCycleId = keyParts[1];
                employeeId = keyParts[2];
                var sequence = keyParts[3];
                sequenceNumber = int.Parse(sequence);
                periodEndDate = DmiString.PickDateToDateTime(int.Parse(internalEndDateString));

                payControlKey = string.Format("{0}*{1}", internalEndDateString, payCycleId);

            }
            catch (Exception e)
            {
                throw new ArgumentException("PAYTODAT record key does not contain the right parts, EndDate*PayCycle*PersonId*Sequence", "record", e);
            }

            Paycntrl paycontrolRecord = null;
            (await ReadPaycntrlRecords()).TryGetValue(payControlKey, out paycontrolRecord);

            if (paycontrolRecord == null)
            {
                throw new ApplicationException("PAYTODAT record key has no matching PAYCNTRL record");
            }
            if (!paycontrolRecord.PclPeriodStartDate.HasValue)
            {
                // throw new ApplicationException(string.Format("PaycontrolRecord {0} does not have a valid period start date", paycontrolRecord.Recordkey));
                logger.Error(string.Format("PaycontrolRecord {0} does not have a valid period start date", paycontrolRecord.Recordkey));
            }

            var registerEntry = new PayrollRegisterEntry(record.Recordkey, employeeId, paycontrolRecord.PclPeriodStartDate, periodEndDate, payCycleId, sequenceNumber, record.PtdCheckNo, record.PtdAdviceNo, !string.IsNullOrEmpty(record.PtdW4NewFlag) && record.PtdW4NewFlag.Equals("Y", StringComparison.CurrentCultureIgnoreCase));

            //build earnings
            foreach (var earningsRecord in record.PtdearnEntityAssociation)
            {
                try
                {
                    registerEntry.EarningsEntries.Add(BuildPayrollRegisterEarningsEntry(earningsRecord));
                }
                catch (Exception e)
                {
                    LogDataError("PAYTODAT~PTDEARN", record.Recordkey, earningsRecord, e);
                    throw;
                }
            }

            //build taxes         
            ILookup<string, PaytodatPtdtaxexp> employerTaxLookup = null;
            try
            {
                employerTaxLookup = record.PtdtaxexpEntityAssociation.ToLookup(ptdtax => ptdtax.PtdTaxExpControllerAssocMember.Split('*')[0]);
            }
            catch (Exception e)
            {
                LogDataError("PAYTODAT~PTDTAXEXP", record.Recordkey, record, e, "Error building Ptdtaxexp lookup from the tax code (first postiion) of the PtdTaxExpController");
            }

            foreach (var taxRecord in record.PtdtaxesEntityAssociation)
            {
                var associatedEmployerTaxRecords = employerTaxLookup != null && employerTaxLookup.Contains(taxRecord.PtdTaxCodesAssocMember) ?
                    employerTaxLookup[taxRecord.PtdTaxCodesAssocMember] :
                    new List<PaytodatPtdtaxexp>();

                try
                {
                    registerEntry.TaxEntries.Add(BuildPayrollRegisterTaxEntry(taxRecord, associatedEmployerTaxRecords));
                }
                catch (Exception e)
                {
                    LogDataError("PAYTODAT~PTDTAXES", record.Recordkey, taxRecord, e);
                    throw;
                }
            }

            //build benefits and deductions
            ILookup<string, PaytodatPtdbdexp> employerBenDedLookup = null;
            try
            {
                employerBenDedLookup = record.PtdbdexpEntityAssociation.ToLookup(ptdbd => ptdbd.PtdBdExpControllerAssocMember.Split('*')[0]);
            }
            catch (Exception e)
            {
                LogDataError("PAYTODAT~PTDBDEXP", record.Recordkey, record, e, "Error building Ptdbdexp lookup from the bended code (first postiion) of the PtdBdExpController");
            }

            foreach (var benefitDeductionRecord in record.PtdbndedEntityAssociation)
            {
                var associatedEmployerBenefitDeductionRecords = employerBenDedLookup != null && employerBenDedLookup.Contains(benefitDeductionRecord.PtdBdCodesAssocMember) ?
                    employerBenDedLookup[benefitDeductionRecord.PtdBdCodesAssocMember] :
                    new List<PaytodatPtdbdexp>();

                try
                {
                    registerEntry.BenefitDeductionEntries.Add(BuildPayrollRegisterBenefitDeductionEntry(benefitDeductionRecord, associatedEmployerBenefitDeductionRecords));
                }
                catch (Exception e)
                {
                    LogDataError("PAYTODAT~PTDBNDED", record.Recordkey, benefitDeductionRecord, e);
                }
            }

            //build leave
            foreach (var leaveRecord in record.PtdleaveEntityAssociation)
            {
                try
                {
                    registerEntry.LeaveEntries.Add(BuildPayrollRegisterLeaveEntry(leaveRecord, record.PtdlvtknEntityAssociation));
                }
                catch (Exception e)
                {
                    LogDataError("PAYTODAT~PTDLEAVE", record.Recordkey, leaveRecord, e);
                }
            }

            //build taxable benefits
            foreach (var taxableBenefit in record.PtdtxblbdEntityAssociation)
            {
                try
                {
                    registerEntry.TaxableBenefitEntries.Add(BuildPayrollRegisterTaxableBenefitEntry(taxableBenefit));
                }
                catch (Exception e)
                {
                    LogDataError("PAYTODAT~PTDTXBLBD", record.Recordkey, taxableBenefit, e);
                }
            }

            return registerEntry;
        }

        /// <summary>
        /// Helper to build a PayrollRegisterBenefitDeductionEntry from a db record and employer benefit/deduction db records
        /// </summary>
        /// <param name="benefitDeductionRecord"></param>
        /// <param name="employerBenefitDeductionRecords"></param>
        /// <returns></returns>
        private PayrollRegisterBenefitDeductionEntry BuildPayrollRegisterBenefitDeductionEntry(PaytodatPtdbnded benefitDeductionRecord, IEnumerable<PaytodatPtdbdexp> employerBenefitDeductionRecords)
        {
            if (benefitDeductionRecord == null)
            {
                throw new ArgumentNullException("benefitDeductionRecord");
            }

            if (employerBenefitDeductionRecords == null)
            {
                employerBenefitDeductionRecords = new List<PaytodatPtdbdexp>();
            }

            var benefitDeductionEntry = new PayrollRegisterBenefitDeductionEntry(benefitDeductionRecord.PtdBdCodesAssocMember)
            {
                EmployeeAmount = benefitDeductionRecord.PtdBdEmplyeCalcAmtsAssocMember,
                EmployeeBasisAmount = benefitDeductionRecord.PtdBdEmplyeBaseAmtsAssocMember,
                EmployerAmount = employerBenefitDeductionRecords.Any() ? employerBenefitDeductionRecords.Sum(bd => bd.PtdBdExpEmplyrCalcAmtsAssocMember) : null,
                EmployerBasisAmount = benefitDeductionRecord.PtdBdEmplyrBaseAmtsAssocMember
            };

            return benefitDeductionEntry;
        }

        /// <summary>
        /// Helper to build a PayrollRegisterTaxEntry from a db record and a list of employer tax db records
        /// </summary>
        /// <param name="taxRecord"></param>
        /// <param name="employerTaxRecords"></param>
        /// <returns></returns>
        private PayrollRegisterTaxEntry BuildPayrollRegisterTaxEntry(PaytodatPtdtaxes taxRecord, IEnumerable<PaytodatPtdtaxexp> employerTaxRecords)
        {
            if (taxRecord == null)
            {
                throw new ArgumentNullException("taxRecord");
            }
            if (employerTaxRecords == null)
            {
                employerTaxRecords = new List<PaytodatPtdtaxexp>();
            }

            var taxEntry = new PayrollRegisterTaxEntry(taxRecord.PtdTaxCodesAssocMember, ConvertInternalCode(taxRecord.PtdTaxFaterdCodesAssocMember))
            {
                SpecialProcessingAmount = taxRecord.PtdTaxFaterdAmtsAssocMember,
                Exemptions = taxRecord.PtdTaxExemptionsAssocMember ?? 0,
                EmployeeTaxAmount = taxRecord.PtdEmplyeTaxAmtsAssocMember,
                EmployeeTaxableAmount = taxRecord.PtdEmplyeTaxableAmtsAssocMember,
                EmployerTaxAmount = employerTaxRecords.Any() ? employerTaxRecords.Sum(t => t.PtdTaxExpEmplyrTaxAmtsAssocMember) : null,
                EmployerTaxableAmount = taxRecord.PtdEmplyrTaxableAmtsAssocMember
            };

            return taxEntry;
        }

        private PayrollTaxProcessingCode ConvertInternalCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return PayrollTaxProcessingCode.Regular;
            }

            switch (code.ToUpperInvariant())
            {
                case "F":
                    return PayrollTaxProcessingCode.FixedAmount;
                case "A":
                    return PayrollTaxProcessingCode.AdditionalTaxAmount;
                case "T":
                    return PayrollTaxProcessingCode.AdditionalTaxableAmount;
                case "E":
                    return PayrollTaxProcessingCode.TaxExempt;
                case "R":
                    return PayrollTaxProcessingCode.Regular;
                case "D":
                    return PayrollTaxProcessingCode.Inactive;
                case "X":
                    return PayrollTaxProcessingCode.TaxableExempt;
                default:
                    return PayrollTaxProcessingCode.Regular;
            }
        }

        /// <summary>
        /// Helper method to build a PayrollRegisterEarningsEntry from a db record
        /// </summary>
        /// <param name="earningsRecord"></param>
        /// <returns></returns>
        private PayrollRegisterEarningsEntry BuildPayrollRegisterEarningsEntry(PaytodatPtdearn earningsRecord)
        {
            if (earningsRecord == null)
            {
                throw new ArgumentNullException("earningsRecord");
            }
            if (!earningsRecord.PtdAmountsAssocMember.HasValue)
            {
                throw new ArgumentException("earningsRecord has no value in PTD.AMOUNTS column. earnings amount is required");
            }

            var hsIndicator = earningsRecord.PtdHSFlagsAssocMember.Equals("H", StringComparison.InvariantCultureIgnoreCase) ? HourlySalaryIndicator.Hourly : HourlySalaryIndicator.Salary;


            //the rate column is spec'd as an MD4, but for Salary type earnings, the HR processes
            //use a manual MD2 conversion instead.
            var rateMultiplier = 1;
            if (hsIndicator == HourlySalaryIndicator.Salary)
            {
                rateMultiplier = 100;
            }
            var rate = earningsRecord.PtdRatesAssocMember.HasValue ? earningsRecord.PtdRatesAssocMember.Value * rateMultiplier : (decimal?)null;

            if (string.IsNullOrWhiteSpace(earningsRecord.PtdStipendIdAssocMember))
            {
                var earningsEntry = new PayrollRegisterEarningsEntry(earningsRecord.PtdEarnTypesAssocMember,
                    earningsRecord.PtdAmountsAssocMember.Value,
                    earningsRecord.PtdBaseEarningsAssocMember ?? 0,
                    earningsRecord.PtdEarnFactorEarningsAssocMember ?? 0,
                    earningsRecord.PtdHoursAssocMember,
                    rate,
                    hsIndicator);

                if (!string.IsNullOrEmpty(earningsRecord.PtdEarndiffIdAssocMember))
                {
                    if (!earningsRecord.PtdEarnDiffEarningsAssocMember.HasValue)
                    {
                        throw new ArgumentException("earningsRecord has no value in PTD.EARN.DIFF.EARNINGS column. differential earnings are required when differential id is specified");
                    }
                    if (!earningsRecord.PtdEarnDiffRatesAssocMember.HasValue)
                    {
                        throw new ArgumentException("earningsRecord has no value in PTD.EARN.DIFF.RATES column. differential rates are required when differential id is specified");

                    }
                    earningsEntry.SetEarningsDifferential(earningsRecord.PtdEarndiffIdAssocMember, earningsRecord.PtdEarnDiffEarningsAssocMember.Value, earningsRecord.PtdEarnDiffUnitsAssocMember, earningsRecord.PtdEarnDiffRatesAssocMember.Value);
                }

                return earningsEntry;
            }
            else
            {
                return new PayrollRegisterEarningsEntry(earningsRecord.PtdEarnTypesAssocMember,
                    earningsRecord.PtdStipendIdAssocMember,
                    earningsRecord.PtdAmountsAssocMember.Value,
                    earningsRecord.PtdBaseEarningsAssocMember ?? 0,
                    earningsRecord.PtdEarnFactorEarningsAssocMember ?? 0,
                    earningsRecord.PtdHoursAssocMember,
                    rate,
                    hsIndicator);
            }
        }

        /// <summary>
        /// Builds the PayrollRegisterLeaveEntry from the db record
        /// </summary>
        /// <param name="leaveRecord"></param>
        /// <param name="leaveTaken"></param>
        /// <returns></returns>
        private PayrollRegisterLeaveEntry BuildPayrollRegisterLeaveEntry(PaytodatPtdleave leaveRecord, List<PaytodatPtdlvtkn> leaveTakenList)
        {
            // the PaytodatPtdlvtkn contains a controller listing each leave code + leave type when leave is taken. if no leave
            // is taken for this period, no PaytodatPtdlvtkn will exist. match each PaytodatPtdlvtkn with a  PaytodatPtdleave
            // and calculatue the current leave balance ((hours from prior balance + hours accured this period)) - hours taken this period.
            var priorBalancehours = leaveRecord.PtdLvPriorBalancesAssocMember.HasValue ? leaveRecord.PtdLvPriorBalancesAssocMember.Value : 0;
            var accruedHours = leaveRecord.PtdLvAccruedHoursAssocMember.HasValue ? leaveRecord.PtdLvAccruedHoursAssocMember.Value : 0;

            foreach (var leaveTaken in leaveTakenList)
            {
                if (leaveTaken.PtdLvTknControllerAssocMember.IndexOf('*') == -1)
                {
                    logger.Error("PTDLVTKN does not have a valid controller key");
                    continue;
                }

                var controller = leaveTaken.PtdLvTknControllerAssocMember.Split('*');

                if (string.IsNullOrWhiteSpace(controller[0]))
                {
                    logger.Error("PTDLVTKN does not have a leave code specified in ID.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(controller[1]))
                {
                    logger.Error("PTDLVTKN does not have a leave type specified in ID.");
                    continue;
                }

                var leaveCode = controller[0];
                var leaveType = controller[1];
                var takenHours = leaveTaken.PtdLvTknHoursAssocMember.HasValue ? leaveTaken.PtdLvTknHoursAssocMember.Value : 0;

                if (leaveRecord.PtdLvCodesAssocMember == leaveCode && takenHours > 0)
                {
                    var currentBalance = (priorBalancehours + accruedHours) - takenHours;
                    return new PayrollRegisterLeaveEntry(leaveCode, leaveRecord.PtdLvTypesAssocMember, leaveTaken.PtdLvTknHoursAssocMember, currentBalance);
                }
            }
            return new PayrollRegisterLeaveEntry(leaveRecord.PtdLvCodesAssocMember, leaveRecord.PtdLvTypesAssocMember, 0, (priorBalancehours + accruedHours));
        }

        /// <summary>
        /// Builds the PayrollRegisterTaxableBenefitEntry from the database record...
        /// </summary>
        /// <param name="taxableBenefit"></param>
        /// <returns></returns>
        private PayrollRegisterTaxableBenefitEntry BuildPayrollRegisterTaxableBenefitEntry(PaytodatPtdtxblbd taxableBenefit)
        {
            if (taxableBenefit.PtdTxblBdControllerAssocMember.IndexOf('*') == -1)
            {
                logger.Error("PTDTXBLBD does not have a valid controller key");
            }

            var controller = taxableBenefit.PtdTxblBdControllerAssocMember.Split('*');

            if (string.IsNullOrWhiteSpace(controller[0]))
            {
                logger.Error("PTDTXBLBD does not have a tax code specified in ID.");
            }

            if (string.IsNullOrWhiteSpace(controller[1]))
            {
                logger.Error("PTDTXBLBD does not have a bended code specified in ID.");
            }

            var taxCode = controller[0];
            var benefitCode = controller[1];

            return new PayrollRegisterTaxableBenefitEntry(benefitCode, taxCode, taxableBenefit.PtdTxblBdEmplyrAmtsAssocMember);
        }
    }
}
