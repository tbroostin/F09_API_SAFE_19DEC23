/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A Link object
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Title of this link
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The URL for this link
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// The FinancialAid type of link. Used to categorize Financial Aid Links
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LinkTypes LinkType { get; set; }

    }

}
