// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents a user's preference for a given module in self service
    /// </summary>
    public class SelfservicePreference
    {
        /// <summary>
        /// Id of the Self Service Preference
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Person Id for the Self Service Preference
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Key for the Self Service Preference
        /// </summary>
        public string PreferenceType { get; set; }

        /// <summary>
        /// The Self Service Preferences to be saved
        /// </summary>
        public IDictionary<string, dynamic> Preferences { get; set; }
    }
}
