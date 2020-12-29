//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// IntgConfigSettings for Ethos Configurations
    /// </summary>
    [Serializable]
    public class MappingSettings : GuidCodeItem
    {
        /// <summary>
        /// Resource which uses this mapping settings item
        /// </summary>
        public string EthosResource { get; set; }

        /// <summary>
        /// Property name in the resource for this mapping settings item
        /// </summary>
        public string EthosPropertyName { get; set; }

        /// <summary>
        /// The title/description of the source data value (translation)
        /// </summary>
        public string SourceTitle { get; set; }

        /// <summary>
        /// The Code value of the source data
        /// </summary>
        public string SourceValue { get; set; }

        ///
        /// The enumeration value mapped from the Colleague source
        /// 
        public string Enumeration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntgConfigSettings"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public MappingSettings(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}