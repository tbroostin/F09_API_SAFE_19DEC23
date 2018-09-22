// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    /// <summary>
    /// nonacademic attendance requirement for a timeframe for a person
    /// </summary>
    public class NonAcademicAttendanceRequirement
    {
        private string _id;
        /// <summary>
        /// Unique identifier for the requirement
        /// </summary>
        public string Id { get { return _id; } }

        private string _personId;
        /// <summary>
        /// Unique identifier for the person to whom the requirement applies
        /// </summary>
        public string PersonId { get { return _personId; } }

        private string _termCode;
        /// <summary>
        /// Code for the academic term for which the requirement applies
        /// </summary>
        public string TermCode { get { return _termCode; } }

        /// <summary>
        /// Default number of units the person must earn to satisfy the requirement
        /// </summary>
        public decimal? DefaultRequiredUnits { get; set; }

        /// <summary>
        /// Number of units the person must earn to satisfy the requirement if the <see cref="DefaultRequiredUnits"/> were overridden for the person
        /// </summary>
        public decimal? RequiredUnitsOverride { get; set; }

        /// <summary>
        /// Number of units the person must earn to satisfy the requirement
        /// </summary>
        /// <remarks>This is either the <see cref="DefaultRequiredUnits"/> or the <see cref="RequiredUnitsOverride"/> if it exists</remarks>
        public decimal? RequiredUnits
        {
            get
            {
                if (RequiredUnitsOverride.HasValue)
                {
                    return RequiredUnitsOverride.Value;
                }
                return DefaultRequiredUnits;
            }
        }

        private readonly List<string> _nonAcademicAttendanceIds = new List<string>();
        /// <summary>
        /// Collection of nonacademic attendance IDs
        /// </summary>
        public ReadOnlyCollection<string> NonAcademicAttendanceIds { get; private set; }

        /// <summary>
        /// Creates a new <see cref="NonAcademicAttendanceRequirement"/> object.
        /// </summary>
        /// <param name="id">Unique identifier for the requirement</param>
        /// <param name="personId">Unique identifier for the person to whom the requirement applies</param>
        /// <param name="termCode">Code for the academic term for which the requirement applies</param>
        /// <param name="nonAcademicAttendanceIds">Collection of nonacademic attendance IDs</param>
        /// <param name="defaultRequiredUnits">Default number of units the person must earn to satisfy the requirement</param>
        /// <param name="requiredUnitsOverride">Number of units the person must earn to satisfy the requirement if the <see cref="DefaultRequiredUnits"/> were overridden for the person</param>
        public NonAcademicAttendanceRequirement(string id, string personId, string termCode, IEnumerable<string> nonAcademicAttendanceIds = null, decimal? defaultRequiredUnits = null, decimal? requiredUnitsOverride = null) : base()
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A nonacademic attendance requirement must have a unique identifier.");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A nonacademic attendance requirement must have a unique identifier for the person to whom it applies.");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "A nonacademic attendance requirement must have a code for the academic term for which it applies.");
            }
            if (defaultRequiredUnits.HasValue && defaultRequiredUnits.Value < 0)
            {
                throw new ArgumentOutOfRangeException("defaultRequiredUnits", "The required units for a nonacademic attendance requirement must be greater than or equal to zero, if specified.");
            }
            if (requiredUnitsOverride.HasValue && requiredUnitsOverride.Value < 0)
            {
                throw new ArgumentOutOfRangeException("requiredUnitsOverride", "The overridden required units for a nonacademic attendance requirement must be greater than or equal to zero, if specified.");
            }

            _id = id;
            _personId = personId;
            _termCode = termCode;
            DefaultRequiredUnits = defaultRequiredUnits;
            RequiredUnitsOverride = requiredUnitsOverride;

            _nonAcademicAttendanceIds = new List<string>();
            if (nonAcademicAttendanceIds != null)
            {
                foreach(var naaId in nonAcademicAttendanceIds)
                {
                    AddNonacademicAttendanceId(naaId);
                }
            }
            NonAcademicAttendanceIds = _nonAcademicAttendanceIds.AsReadOnly();
        }

        /// <summary>
        /// Adds a nonacademic attendance ID to the requirement
        /// </summary>
        /// <param name="id">Unique identifier for a nonacademic attendance record</param>
        private void AddNonacademicAttendanceId(string id)
        {
            if (string.IsNullOrEmpty(id) || _nonAcademicAttendanceIds.Contains(id))
            {
                return;
            }
            _nonAcademicAttendanceIds.Add(id);
        }
    }
}
