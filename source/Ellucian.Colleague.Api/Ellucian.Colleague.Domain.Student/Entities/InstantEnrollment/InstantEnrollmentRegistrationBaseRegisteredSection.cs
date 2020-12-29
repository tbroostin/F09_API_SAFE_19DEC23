// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the section and related information for an Instant Enrollment section in which an 
    /// individual has successfully enrolled.
    /// </summary>
    public class InstantEnrollmentRegistrationBaseRegisteredSection
    {
        /// <summary>
        /// The identifier of the section of interest.
        /// </summary>
        public string SectionId { get; private set; }

        /// <summary>
        /// The cost of the section.
        /// </summary>
        public decimal SectionCost { get; private set; }

        public InstantEnrollmentRegistrationBaseRegisteredSection(string sectionId, decimal? sectionCost)
        {
            if (String.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId");
            }

            SectionId = sectionId;
            SectionCost = sectionCost.HasValue ? sectionCost.Value : 0;
        }
    }
}
