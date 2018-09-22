/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Interact with Earnings Type data from database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EarningsTypeRepository : BaseColleagueRepository, IEarningsTypeRepository
    {
        private const string EarningsTypesCacheKey = "EarningsTypes";
        private IEnumerable<LeaveType> AllLeaveTypes;

        /// <summary>
        /// Instantiate EarningsTypeRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public EarningsTypeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            :base(cacheProvider, transactionFactory, logger)
        { }

        /// <summary>
        /// Get all Earnings Type objects, built from database data
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<EarningsType>> GetEarningsTypesAsync()
        {
            AllLeaveTypes = await GetLeaveTypesAsync(false);
            return await GetOrAddToCacheAsync<IEnumerable<EarningsType>>(EarningsTypesCacheKey, async () => await this.BuildAllEarningsTypes(), Level1CacheTimeoutValue);
        }    
        
        private async Task<IEnumerable<EarningsType>> BuildAllEarningsTypes()
        {
            var earningsTypeEntities = new List<EarningsType>();
            var earningsTypeRecords = await DataReader.BulkReadRecordAsync<DataContracts.Earntype>("");
            if (earningsTypeRecords == null)
            {
                var message = string.Format("Unexpected: Bulk data read of earnings type records returned null.");
                logger.Error(message);
                throw new ApplicationException(message);
            }
            else
            {
                foreach (var earningsTypeRecord in earningsTypeRecords)
                {
                    try
                    {
                        earningsTypeEntities.Add(BuildEarningsType(earningsTypeRecord));
                    }
                    catch (Exception e)
                    {
                        LogDataError("EarningsType", earningsTypeRecord.Recordkey, earningsTypeRecord, e, e.Message);
                    }
                }
            }
            return earningsTypeEntities;
        }

        private EarningsCategory ConvertRecordColumnToEarningsCategory(string earningsCategoryCode)
        {
            if (string.IsNullOrEmpty(earningsCategoryCode))
            {
                throw new ArgumentNullException("earningsCategoryCode","Cannot convert null or empty earningsCategoryCode");
            }

            switch (earningsCategoryCode.ToUpperInvariant())
            {
                case "R":
                    return EarningsCategory.Regular;
                case "O":
                    return EarningsCategory.Overtime;
                case "L":
                    return EarningsCategory.Leave;
                case "C":
                    return EarningsCategory.CollegeWorkStudy;
                case "M":
                    return EarningsCategory.Miscellaneous;
                default:
                    throw new ApplicationException("Unknown earningsTypeCategory " + earningsCategoryCode);
            }

        }
        
        private EarningsMethod ConvertRecordColumnToEarningsMethod(string earningsMethodCode)
        {
            if (string.IsNullOrEmpty(earningsMethodCode))
            {
                return EarningsMethod.None;
            }

            switch (earningsMethodCode.ToUpperInvariant())
            {
                case "A":
                    return EarningsMethod.Accrued;
                case "P":
                    return EarningsMethod.Taken;
                case "N":
                    return EarningsMethod.NoPay;
                default:
                    throw new ApplicationException("Unknown earningsMethodCode " + earningsMethodCode);
            }
        }

        private async Task<IEnumerable<LeaveType>> GetLeaveTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<LeaveType>("HR", "LEAVE.TYPES",
                (cl, g) => new LeaveType(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember))
                { TimeType = TranslateTimeType(cl.ValActionCode1AssocMember) }, bypassCache: ignoreCache);
        }

        private LeaveTypeCategory TranslateTimeType(string valActionCode1)
        {
            switch (valActionCode1)
            {
                case "1":
                    return LeaveTypeCategory.Vacation;
                case "2":
                    return LeaveTypeCategory.Sick;
                case "3":
                    return LeaveTypeCategory.Compensatory;
                default:
                    return LeaveTypeCategory.None;
            }
        }

        private LeaveType GetLeaveType(string leaveCode)
        {
            return AllLeaveTypes.FirstOrDefault(l => l.Code == leaveCode);
        }

        private EarningsType BuildEarningsType(DataContracts.Earntype earningsTypeRecord)
        {
            if (earningsTypeRecord == null)
            {
                throw new ArgumentNullException("earningsTypeRecord");

            }

            if (string.IsNullOrEmpty(earningsTypeRecord.EtpDesc))
            {
                throw new ArgumentException("Earnings Type Description must have a value.");
            }

            // Earnings Type is active if database value is "A"; otherwise it's inactive
            var isActive = earningsTypeRecord.EtpActiveFlag == null ? false : earningsTypeRecord.EtpActiveFlag.Equals("A", StringComparison.InvariantCultureIgnoreCase);

            var earningsTypeEntity = new EarningsType(earningsTypeRecord.Recordkey, earningsTypeRecord.EtpDesc, isActive, ConvertRecordColumnToEarningsCategory(earningsTypeRecord.EtpCategory), ConvertRecordColumnToEarningsMethod(earningsTypeRecord.EtpEarningMethod), earningsTypeRecord.EtpOtFactor, GetLeaveType(earningsTypeRecord.EtpLeaveType))
            { };

            return earningsTypeEntity;

        }

    }

}
