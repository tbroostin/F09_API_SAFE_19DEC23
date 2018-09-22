// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Data.Base
{
    public class RuleConversionOptions
    { 
        /// <summary>
        /// Date format to expect in date-based rules
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Date delimiter to expect in date-based rules
        /// </summary>
        public string DateDelimiter { get; set; }

        /// <summary>
        /// If a two digit year specified in date literals, the lowest year value that is to be converted to 1900s.
        /// Any two digit year lower than this is assumed to be 2000s. (This is a Unidata thing.)
        /// ex: If threshold is 68, 5/1/68 ==> 5/1/1968 and 5/1/67 ==> 5/1/2067
        /// </summary>
        public int CenturyThreshhold { get; set; }
    }
}
