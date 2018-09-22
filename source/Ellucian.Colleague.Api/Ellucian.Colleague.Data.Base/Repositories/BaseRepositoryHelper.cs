// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Web.Http.Configuration;
using Ellucian.Data.Colleague.DataContracts;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    public class BaseRepositoryHelper : BaseApiRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider">Cache provider interface</param>
        /// <param name="transactionFactory">Transaction factory interface</param>
        /// <param name="logger">Logging interface</param>
        /// <param name="apiSettings">API settings interface</param>
        public BaseRepositoryHelper(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Schedule repeat codes
        /// </summary>
        public IEnumerable<ScheduleRepeat> ScheduleRepeats
        {
            get
            {
                return GetValcode<ScheduleRepeat>("CORE", "SCHED.REPEATS", r =>
                    (new ScheduleRepeat(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember, r.ValActionCode1AssocMember,
                        ConvertFrequencyCodeToFrequencyType(r.ValActionCode2AssocMember))), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Convert a frequency code to a frequency type
        /// </summary>
        /// <param name="code">Frequency code</param>
        /// <returns>Frequency type</returns>
        public static FrequencyType? ConvertFrequencyCodeToFrequencyType(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            switch (code)
            {
                case "D":
                    return FrequencyType.Daily;
                case "W":
                    return FrequencyType.Weekly;
                case "M":
                    return FrequencyType.Monthly;
                case "Y":
                    return FrequencyType.Yearly;
            }
            return null;
        }
    }
}
