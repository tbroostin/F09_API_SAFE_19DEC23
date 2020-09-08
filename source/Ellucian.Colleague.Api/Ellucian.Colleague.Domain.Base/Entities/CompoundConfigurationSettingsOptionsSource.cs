//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Source data for compound configuration settings options
    /// </summary>
    [Serializable]
    public class CompoundConfigurationSettingsOptionsSource
    {
        /// <summary>
        /// Source title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Source value
        /// </summary>
        public string Value { get; set; }       
    }
}