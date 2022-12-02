//Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Compound Configuration Settings Options domain entity for Ethos integration
    /// </summary>
    [Serializable]
    public class CompoundConfigurationSettingsOptions : GuidCodeItem
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
        /// The primary source
        /// 
        public string PrimarySource { get; set; }

        ///
        /// The secondary source
        /// 
        public string SecondarySource { get; set; }

        ///
        /// The tertiary source
        /// 
        public string TertiarySource { get; set; }

        /// <summary>
        /// The primary source data (titles and values) 
        /// </summary>
        public List<CompoundConfigurationSettingsOptionsSource> PrimarySourceData
        {
            get { return primarySourceData; }
            set { if (value != null) { primarySourceData = value; } }
        }
        private List<CompoundConfigurationSettingsOptionsSource> primarySourceData;

        /// <summary>
        /// The secondary source data (titles and values) 
        /// </summary>
        public List<CompoundConfigurationSettingsOptionsSource> SecondarySourceData
        {
            get { return secondarySourceData; }
            set { if (value != null) { secondarySourceData = value; } }
        }
        private List<CompoundConfigurationSettingsOptionsSource> secondarySourceData;

        /// <summary>
        /// The tertiary source data (titles and values) 
        /// </summary>
        public List<CompoundConfigurationSettingsOptionsSource> TertiarySourceData
        {
            get { return tertiarySourceData; }
            set { if (value != null) { tertiarySourceData = value; } }
        }
        private List<CompoundConfigurationSettingsOptionsSource> tertiarySourceData;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundConfigurationSettingsOptions"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CompoundConfigurationSettingsOptions(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}