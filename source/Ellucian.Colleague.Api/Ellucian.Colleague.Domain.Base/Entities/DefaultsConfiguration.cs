// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents configuration data
    /// </summary>
    [Serializable]
    public class DefaultsConfiguration
    {
        private readonly Dictionary<string, string> _defaultMappings = new Dictionary<string, string>();

        public string AddressDuplicateCriteriaId { get; set; }
        public string AddressTypeMappingId { get; set; }
        public string EmailAddressTypeMappingId { get; set; }
        public string PersonDuplicateCriteriaId { get; set; }
        public string SubjectDepartmentMappingId { get; set; }
        public string CampusCalendarId { get; set; }
        public string HostInstitutionCodeId { get; set; }

        public Dictionary<string, string> DefaultMappings
        {
            get
            {
                return _defaultMappings;       
            }
        }

        /// <summary>
        /// Add a default mapping
        /// </summary>
        /// <param name="field">Name of the field being defaulted</param>
        /// <param name="defaultValue">The default value associated to the field</param>
        public void AddDefaultMapping(string field, string defaultValue)
        {
            if (string.IsNullOrEmpty(field))
                throw new ArgumentNullException("field", "A field must be specified.");

            // Prevent duplicates
            if (!_defaultMappings.ContainsKey(field))
            {
                _defaultMappings.Add(field, defaultValue);
            }
        }

        /// <summary>
        /// Get the default value of the field
        /// </summary>
        /// <param name="field">Name of the field being defaulted</param>
        public string GetFieldDefault(string field)
        {
            if (string.IsNullOrEmpty(field))
                throw new ArgumentNullException("field", "A field must be specified.");

            if (_defaultMappings.ContainsKey(field))
            {
                return _defaultMappings[field];
            }
            else
            {
                throw new InvalidOperationException("No default mapping found");
            }
        }
    }
}