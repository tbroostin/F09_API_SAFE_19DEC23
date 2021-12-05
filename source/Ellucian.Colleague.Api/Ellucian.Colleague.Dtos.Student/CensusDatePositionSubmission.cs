// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// a census date submission configuration
    /// </summary>
    public class CensusDatePositionSubmission
    {
        /// <summary>
        /// The position number of the census date
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The descriptive label for the census date
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The number of days prior to the census date that it can be submitted
        /// </summary>
        public int? CertifyDaysBeforeOffset { get; set; }
    }
}
