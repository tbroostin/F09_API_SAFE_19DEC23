// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Registration date overrides specific to a particular section based on the requesting person's registration group.
    /// </summary>
    public class SectionRegistrationDate
    {
        /// <summary>
        /// Section Id related to the associated dates
        /// </summary>
        public string SectionId { get; set; }
        /// <summary>
        /// Preregistration start date for this section
        /// </summary>
        public DateTime? PreRegistrationStartDate { get; set; }
        /// <summary>
        /// Preregistration end date for this section
        /// </summary>
        public DateTime? PreRegistrationEndDate { get; set; }
        /// <summary>
        /// Registration start date for this section
        /// </summary>
        public DateTime? RegistrationStartDate { get; set; }
        /// <summary>
        /// Registration end date for this section
        /// </summary>
        public DateTime? RegistrationEndDate { get; set; }
        /// <summary>
        /// Add period start date for this section
        /// </summary>
        public DateTime? AddStartDate { get; set; }
        /// <summary>
        /// Add period end date for this section
        /// </summary>
        public DateTime? AddEndDate { get; set; }
        /// <summary>
        /// Drop period start date for this section
        /// </summary>
        public DateTime? DropStartDate { get; set; }
        /// <summary>
        /// Drop period end date for this section
        /// </summary>
        public DateTime? DropEndDate { get; set; }
        /// <summary>
        /// Date when a drop grade is required for dropping this section
        /// </summary>
        public DateTime? DropGradeRequiredDate { get; set; }
        /// <summary>
        /// List of Census Dates for this section
        /// </summary>
        public List<DateTime?> CensusDates { get; set; }
        /// <summary>
        /// Source of the registration date information:
        ///  Term (ACTM),
        ///  Section (SRGD),
        ///  TermLocation (TLOC),
        ///  RegistrationUserTerm (RGUT),
        ///  RegistrationUserSection (RGUC),
        ///  RegistrationUserTermLocation (RGUL)
        /// </summary>
        public RegistrationDateSource RegistrationDateSource { get; set; }
    }
}
