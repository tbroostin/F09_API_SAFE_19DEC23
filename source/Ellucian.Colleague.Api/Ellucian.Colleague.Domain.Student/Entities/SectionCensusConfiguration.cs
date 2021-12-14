// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// census configuration parameters
    /// </summary>
    [Serializable]
    public class SectionCensusConfiguration
    {

        /// <summary>
        /// a collection of section census date parameters
        /// </summary>
        public ReadOnlyCollection<CensusDatePositionSubmission> CensusDatePositionSubmissionRange { get; private set; }
        private List<CensusDatePositionSubmission> _censusDatePositionSubmissionRange = new List<CensusDatePositionSubmission>();

        /// <summary>
        /// Determines if and/or how the Last Date Attended / Never Attended field will be displayed for a section census
        /// </summary>
        public LastDateAttendedNeverAttendedFieldDisplayType LastDateAttendedNeverAttendedCensusRoster { get; set; }

        /// <summary>
        /// The drop reason code that will be used whtn a faculty member drops a student from a section in the Drop Roster.
        /// </summary>
        public string FacultyDropReasonCode { get; set; }


        /// <summary>
        /// Creates a new <see cref="SectionCensusConfiguration"/>
        /// </summary>
        /// <param name="lastDateAttendedNeverAttendedCensusRoster"></param>
        /// <param name="censusDatePositionSubmissions"></param>
        /// <param name="facultyDropReasonCode"></param>
        public SectionCensusConfiguration(LastDateAttendedNeverAttendedFieldDisplayType lastDateAttendedNeverAttendedCensusRoster, 
            List<CensusDatePositionSubmission> censusDatePositionSubmissions, string facultyDropReasonCode)
        {
            LastDateAttendedNeverAttendedCensusRoster = lastDateAttendedNeverAttendedCensusRoster;
            FacultyDropReasonCode = facultyDropReasonCode;

            censusDatePositionSubmissions = censusDatePositionSubmissions != null ? censusDatePositionSubmissions.Where(cps => cps != null).ToList() : new List<CensusDatePositionSubmission>();
            foreach (var censusDatePositionSubmission in censusDatePositionSubmissions)
            {
                AddCensusDateSubmission(censusDatePositionSubmission);
            }
            CensusDatePositionSubmissionRange = _censusDatePositionSubmissionRange.AsReadOnly();
        }

        /// <summary>
        /// Add an <see cref="CensusDatePositionSubmission"/> to the <see cref="CensusDatePositionSubmissionRange"/>
        /// </summary>
        /// <param name="censusDatePositionSubmission"><see cref="CensusDatePositionSubmission"/> to add</param>
        public void AddCensusDateSubmission(CensusDatePositionSubmission censusDatePositionSubmission)
        {
            if (censusDatePositionSubmission == null)
            {
                throw new ArgumentNullException("censusDatePositionSubmission", "Cannot add a null census date position submission to a section census configuration.");
            }

            if (!_censusDatePositionSubmissionRange.Any(submission => submission.Position == censusDatePositionSubmission.Position))
            {
                _censusDatePositionSubmissionRange.Add(censusDatePositionSubmission);
            }
        }
    }
}
