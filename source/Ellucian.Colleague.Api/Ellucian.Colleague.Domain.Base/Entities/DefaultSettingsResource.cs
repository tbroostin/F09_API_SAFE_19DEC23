//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// IntgDefaultSettings for Ethos Configurations Resources
    /// </summary>
    [Serializable]
    public class DefaultSettingsResource
    {
        /// <summary>
        /// resources which use this configuration settings item
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The property name for which the default is needed, if applicable.
        /// </summary>
        public string PropertyName { get; set; }
    }
}