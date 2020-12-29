// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Parameters releated to instant enrollment and continuing education
    /// </summary>
    public class InstantEnrollmentConfiguration
    {
        /// <summary>
        /// Behavior for assigning academic program to students in Colleague Self-Service instant enrollment workflows
        /// </summary>
        public AddNewStudentProgramBehavior StudentProgramAssignmentBehavior { get; set; }

        /// <summary>
        /// List of academic programs and associated catalog codes from which students can choose in Colleague Self-Service instant enrollment workflows
        /// </summary>
        public IEnumerable<AcademicProgramOption> AcademicProgramOptions { get; set; }

        /// <summary>
        /// Code for payment distribution used in processing Colleague Self-Service instant enrollment payments
        /// </summary>
        public string PaymentDistributionCode { get; set; }

        /// <summary>
        /// Default code for citizenship home country for Colleague Self-Service instant enrollment registrants
        /// </summary>
        public string CitizenshipHomeCountryCode { get; set; }

        /// <summary>
        /// Flag indicating whether or not users may make payments online
        /// </summary>
        public bool WebPaymentsImplemented { get; set; }

        /// <summary>
        /// Role assigned to users created through Colleague Self-Service instant enrollment
        /// </summary>
        public string RegistrationUserRole { get; set; }

        /// <summary>
        /// Start date of the sections within the range of this end date.
        /// It is the maximum range of sections with the start date from today. 
        /// This will only restrict to have sections in cache that have start date greater than  today and start date less than the date in this field.
        /// </summary>
        public DateTime? SearchEndDate { get;  set; }

        /// <summary>
        /// Flag indicating whether or not any instant enrollment sections should show bookstore links
        /// </summary>
        public bool ShowInstantEnrollmentBookstoreLink { get; set; }

        /// <summary>
        /// List of subject codes used to restrict which course sections are available in the Colleague Self-Service instant enrollment workflows; only course sections with subjects in this list are available
        /// </summary>
        public IEnumerable<string> SubjectCodesToDisplayInCatalog { get; set; }

        /// <summary>
        /// List of <see cref="CatalogFilterOption3" /> representing searchable elements and whether to use that element in
        /// searching the Instant Enrollment catalog.
        /// </summary>
        public IEnumerable<CatalogFilterOption3> CatalogFilterOptions { get; set; }

        /// <summary>
        /// Demographic information collected when adding a person to Colleague from the instant enrollment functionality
        /// </summary>
        public IEnumerable<DemographicField> DemographicFields { get; set; }
    }
}
