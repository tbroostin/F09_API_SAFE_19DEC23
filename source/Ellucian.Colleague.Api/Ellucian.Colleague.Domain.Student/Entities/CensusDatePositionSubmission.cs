// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// a census submission configuration
    /// </summary>
    [Serializable]
    public class CensusDatePositionSubmission
    {
        /// <summary>
        /// The position of the census date
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

        /// <summary>
        /// Creates a new <see cref="CensusDatePositionSubmission"/>
        /// </summary>
        /// <param name="position">the position number for the section census date</param>
        /// <param name="label">the descriptive label for the census date</param>
        /// <param name="certifyDaysBeforeOffset">the number of days prior to the census date that it can be submitted</param>
        public CensusDatePositionSubmission(int? position, string label, int? certifyDaysBeforeOffset)
        {
            if (!position.HasValue)
            {
                throw new ArgumentNullException("position", "A census date position is required when building an section census date position submission.");
            }

            Position = position.Value;
            Label = label;
            CertifyDaysBeforeOffset = certifyDaysBeforeOffset;
        }
    }
}
