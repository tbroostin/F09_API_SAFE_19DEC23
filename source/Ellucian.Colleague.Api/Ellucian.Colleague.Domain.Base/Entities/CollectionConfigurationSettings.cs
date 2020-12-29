//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// IntgConfigSettings for Ethos Configurations
    /// </summary>
    [Serializable]
    public class CollectionConfigurationSettings : GuidCodeItem
    {
        /// <summary>
        /// List of resources which use this configuration settings item
        /// </summary>
        public List<DefaultSettingsResource> EthosResources { get; set; }

        /// <summary>
        /// The title/description of the source data value (translation)
        /// </summary>
        public List<CollectionConfigurationSettingsSource> Source { get; set; }

        /// <summary>
        /// Field Help or Description in the API Management Center
        /// </summary>
        public string FieldHelp { get; set; }

        /// <summary>
        /// Field Name in LDM.DEFAULTS to be updated
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Entity for selection of options records
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Valcode table for selection of items for options records
        /// </summary>
        public string ValcodeTableName { get; set; }
       
        /// <summary>
        /// Initializes a new instance of the <see cref="IntgCollectSettings"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CollectionConfigurationSettings(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}