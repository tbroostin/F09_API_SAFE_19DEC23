//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Mapping Settings Options domain entity for Ethos integration
    /// </summary>
    [Serializable]
    public class MappingSettingsOptions : GuidCodeItem
    {
        /// <summary>
        /// Resource which uses this mapping settings options item
        /// </summary>
        public string EthosResource { get; set; }

        /// <summary>
        /// Property name in the resource for this mapping settings options item
        /// </summary>
        public string EthosPropertyName { get; set; }

        ///
        /// The enumeration values available for the mapping
        /// 
        public List<string> Enumerations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingSettingsOptions"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public MappingSettingsOptions(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}