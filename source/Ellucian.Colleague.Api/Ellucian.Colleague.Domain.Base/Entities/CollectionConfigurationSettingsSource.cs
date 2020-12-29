//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// IntgConfigSettings for Ethos Configurations
    /// </summary>
    [Serializable]
    public class CollectionConfigurationSettingsSource
    {

        /// <summary>
        /// The title/description of the source data value (translation)
        /// </summary>
        public string SourceTitle { get; set; }

        /// <summary>
        /// The Code value of the source data
        /// </summary>
        public string SourceValue { get; set; }
    }
}