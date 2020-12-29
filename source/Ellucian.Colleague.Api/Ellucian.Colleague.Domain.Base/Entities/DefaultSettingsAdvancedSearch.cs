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
    public class DefaultSettingsAdvancedSearch
    {
        /// <summary>
        /// INTG.DEFAULT.SETTINGS key
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// StudentTranscriptGradesHistory collection
        /// </summary>
        public List<DefaultSettingsAdvancedSearchOptions> DefaultSettingsAdvancedSearchOptions { get; set; }

        public DefaultSettingsAdvancedSearch(string id, List<DefaultSettingsAdvancedSearchOptions> defaultsettingsAdvancedSearchOptions)
        {
            Id = id;
            DefaultSettingsAdvancedSearchOptions = defaultsettingsAdvancedSearchOptions;
        }
    }
}