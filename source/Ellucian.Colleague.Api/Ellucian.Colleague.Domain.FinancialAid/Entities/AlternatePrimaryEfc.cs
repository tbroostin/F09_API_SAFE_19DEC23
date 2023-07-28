// Copyright 2023 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class AlternatePrimaryEfc
    {
        /// <summary>
        /// Primary alternate EFC for 1 month.
        /// </summary>             
        public int? OneMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 2 month.
        /// </summary>             
        public int? TwoMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 3 month.
        /// </summary>             
        public int? ThreeMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 4 month.
        /// </summary>             
        public int? FourMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 5 month.
        /// </summary>             
        public int? FiveMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 6 month.
        /// </summary>             
        public int? SixMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 7 month.
        /// </summary>             
        public int? SevenMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 8 month.
        /// </summary>             
        public int? EightMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 10 month. Skipped 9 month.
        /// </summary>             
        public int? TenMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 11 month.
        /// </summary>             
        public int? ElevenMonth { get; set; }

        /// <summary>
        /// Primary alternate EFC for 12 month.
        /// </summary>             
        public int? TwelveMonth { get; set; }
    }
}
