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
    public class DefaultSettingsAdvancedSearchOptions
    {
        /// <summary>
        /// The full name of the default settings advanced search option.
        /// Ex. data from PREFERRRED.NAME or POS.TITLE
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The value for the default settings advanced search option.
        /// Ex. data from CORP.FOUNDS.ID or ID
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 	The origin of the default settings advanced search option.
        /// 	Ex. Colleague file name of CORP.FOUNDS or STAFF.ID
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Initializes a new instance of the default settings advance search options <see cref="DefaultSettingsAdvancedSearchOptions"/> class.
        /// </summary>
        /// <param name="title">The full names of the default settings advanced search option.</param>
        /// <param name="value">The values of the default settings advanced search option.</param>
        /// <param name="origin">The origins of the default settings advanced search option..</param>
        public DefaultSettingsAdvancedSearchOptions(string title, string value, string origin)
        {
            Title = title;
            Value = value;
            Origin = origin;
        }
    }
}