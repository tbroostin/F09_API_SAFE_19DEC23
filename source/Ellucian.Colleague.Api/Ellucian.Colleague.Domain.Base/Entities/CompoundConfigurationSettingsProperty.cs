//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// IntgDefaultSettings for Ethos Configurations Resources
    /// </summary>
    [Serializable]
    public class CompoundConfigurationSettingsProperty
    {
        /// <summary>
        /// The primary value of the compound configuration setting.
        /// </summary>
        public string PrimaryValue { get; set; }

        /// <summary>
        /// The primary title of the compound configuration setting.
        /// </summary>
        public string PrimaryTitle { get; set; }       

        /// <summary>
        /// The secondary value compound configuration setting.
        /// </summary>
        public string SecondaryValue { get; set; }

        /// <summary>
        /// The secondary title compound configuration setting.
        /// </summary>
        public string SecondaryTitle { get; set; }

        /// <summary>
        /// The tertiary value compound configuration setting.
        /// </summary>
        public string TertiaryValue { get; set; }

        /// <summary>
        /// The tertiary title compound configuration setting.
        /// </summary>
        public string TertiaryTitle { get; set; }
    }
}