// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
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
        /// 
        /// </summary>
        public IDictionary<string, dynamic> Preferences { get; set; }

        /// <summary>
        /// A Self Service Preference that relates a Person Id and Key to a set of Data
        /// </summary>
        /// <param name="id">Id of the Self Service Preference</param>
        /// <param name="personId">Person Id for the Self Service Preference</param>
        /// <param name="preferenceType">Key for the Self Service Preference</param>
        /// <param name="preferences">Dictionary of Self Service Preference settings</param>
        public SelfservicePreference(string id, string personId, string preferenceType, IDictionary<string, dynamic> preferences)
        {
            Id = id;
            PersonId = personId;
            PreferenceType = preferenceType;
            Preferences = preferences;
        }
    }
}
