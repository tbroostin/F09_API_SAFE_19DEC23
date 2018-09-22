// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance
{
    [Serializable]
    public static class FinanceTimeframeCodes
    {
        /// <summary>
        /// Code for Past Period
        /// </summary>
        public const string PastPeriod = "PAST";

        /// <summary>
        /// Code for Current Period
        /// </summary>
        public const string CurrentPeriod = "CUR";

        /// <summary>
        /// Code for Future Period
        /// </summary>
        public const string FuturePeriod = "FTR";

        /// <summary>
        /// Code for Non-Term Timeframe
        /// </summary>
        public const string NonTerm = "NON-TERM";
    }
}
