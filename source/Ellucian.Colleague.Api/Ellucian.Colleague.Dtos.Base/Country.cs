// Copyright 2015 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Code and description for a country
    /// </summary>
    public class Country
    {
        /// <summary>
        /// Unique system code for this country
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Country description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// ISO code for the country
        /// </summary>
        public string IsoCode { get; set; }
        /// <summary>
        /// Indicates if this country no longer exist as a mailable country. If true, the country no longer exists.
        /// </summary>
        public bool IsNotInUse { get; set; }
    }
}
