// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the section and related cost for a section in which an 
    /// individual has successfully enrolled.
    /// </summary>
    public class InstantEnrollmentProposedRegisteredSection
    {
        /// <summary>
        /// The identifier of the section of interest.
        /// </summary>
        public string SectionId { get;  set; }

        /// <summary>
        /// The cost of the section.
        /// </summary>
        public double SectionCost { get;  set; }
    }
}
