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
    public class CompoundConfigurationSettings : GuidCodeItem
    {
        /// <summary>
        /// The title of the compound configuration setting.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// List of resources which use this configuration settings item
        /// </summary>
        public List<DefaultSettingsResource> EthosResources { get; set; }
               
        /// <summary>
        /// List of properties
        /// </summary>
        public List<CompoundConfigurationSettingsProperty> Properties { get; set; }

        /// <summary>
        /// The primary display Label of the compound configuration setting.
        /// </summary>
        public string PrimaryLabel { get; set; }

        /// <summary>
        /// The secondary label of the compound configuration setting.
        /// </summary>
        public string SecondaryLabel { get; set; }

        /// <summary>
        /// The tertiary label of the compound configuration setting.
        /// </summary>
        public string TertiaryLabel { get; set; }

        /// <summary>
        /// The primary validation entity of the compound configuration setting.
        /// </summary>
        public string PrimaryEntity { get; set; }

        /// <summary>
        /// The secondary validation entity of the compound configuration setting.
        /// </summary>
        public string SecondaryEntity { get; set; }

        /// <summary>
        /// The tertiary validation entity of the compound configuration setting.
        /// </summary>
        public string TertiaryEntity { get; set; }

        /// <summary>
        /// The primary validation valcode Table of the compound configuration setting.
        /// </summary>
        public string PrimaryValcode { get; set; }

        /// <summary>
        /// The secondary validation valcode Table of the compound configuration setting.
        /// </summary>
        public string SecondaryValcode { get; set; }

        /// <summary>
        /// The tertiary validation valcode Table of the compound configuration setting.
        /// </summary>
        public string TertiaryValcode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntgConfigSettings"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CompoundConfigurationSettings(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}