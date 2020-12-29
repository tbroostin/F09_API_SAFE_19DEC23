// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Parameters releated to instant enrollment and continuing education
    /// </summary>
    [Serializable]
    public class InstantEnrollmentConfiguration
    {
        /// <summary>
        /// Behavior for assigning academic program to students in Colleague Self-Service instant enrollment workflows
        /// </summary>
        public AddNewStudentProgramBehavior StudentProgramAssignmentBehavior { get; private set; }

        /// <summary>
        /// List of academic programs and associated catalog codes from which students can choose in Colleague Self-Service instant enrollment workflows
        /// </summary>
        public ReadOnlyCollection<AcademicProgramOption> AcademicProgramOptions { get; private set; }
        private readonly List<AcademicProgramOption> _academicProgramOptions = new List<AcademicProgramOption>();

        /// <summary>
        /// Code for payment distribution used in processing Colleague Self-Service instant enrollment payments
        /// </summary>
        public string PaymentDistributionCode { get; private set; }

        /// <summary>
        /// Default code for citizenship home country for Colleague Self-Service instant enrollment registrants
        /// </summary>
        public string CitizenshipHomeCountryCode { get; private set; }

        /// <summary>
        /// Flag indicating whether or not users may make payments online
        /// </summary>
        public bool WebPaymentsImplemented { get; private set; }

        /// <summary>
        /// Role assigned to users created through Colleague Self-Service instant enrollment
        /// </summary>
        public string RegistrationUserRole { get; private set; }

        /// <summary>
        /// Start date of sections within the range of this end date.
        /// It is the range of valid IE sections that have start date greater than  today and start date less than the date in this field.
        /// </summary>
        public DateTime? SearchEndDate { get; private set; }

        /// <summary>
        /// Flag indicating whether or not any instant enrollment sections should show bookstore links
        /// </summary>
        public bool ShowInstantEnrollmentBookstoreLink { get; private set; }

        /// <summary>
        /// List of subject codes used to restrict which course sections are available in the Colleague Self-Service instant enrollment workflows; only course sections with subjects in this list are available
        /// </summary>
        public ReadOnlyCollection<string> SubjectCodesToDisplayInCatalog { get; private set; }
        private readonly List<string> _subjectCodesToDisplayInCatalog = new List<string>();


        /// <summary>
        /// The parameters controlling how to display filters and criteria on the course catalog
        /// </summary>
        public ReadOnlyCollection<CatalogFilterOption> CatalogFilterOptions { get; private set; }
        private readonly List<CatalogFilterOption> _catalogFilterOptions = new List<CatalogFilterOption>();

        /// <summary>
        /// Demographic information collected when adding a person to Colleague from the instant enrollment functionality
        /// </summary>
        public ReadOnlyCollection<DemographicField> DemographicFields { get; private set; }
        private readonly List<DemographicField> _demographicFields = new List<DemographicField>();


        /// <summary>
        /// Creates a new <see cref="InstantEnrollmentConfiguration"/>
        /// </summary>
        /// <param name="behavior">Behavior for assigning academic program to students in Colleague Self-Service instant enrollment workflows</param>
        /// <param name="academicProgramOptions">List of academic programs and associated catalog codes from which students can choose in Colleague Self-Service instant enrollment workflows</param>
        /// <param name="paymentDistributionCode">Code for payment distribution used in processing Colleague Self-Service instant enrollment payments</param>
        /// <param name="citizenshipHomeCountryCode">Default code for citizenship home country for Colleague Self-Service instant enrollment registrants</param>
        /// <param name="webPaymentsImplemented">Flag indicating whether or not users may make payments online</param>
        /// <param name="registrationUserRole">Role assigned to users created through Colleague Self-Service instant enrollment</param>
        /// <param name="showInstantEnrollmentBookstoreLink">Flag indicating whether or not any instant enrollment sections should show bookstore links</param>
        /// <param name="demographicFields">Demographic information collected when adding persons to Colleague from the instant enrollment functionality</param>
        public InstantEnrollmentConfiguration(AddNewStudentProgramBehavior behavior, List<AcademicProgramOption> academicProgramOptions, 
            string paymentDistributionCode, string citizenshipHomeCountryCode, bool webPaymentsImplemented, string registrationUserRole, DateTime? searchEndDate, 
            bool showInstantEnrollmentBookstoreLink, List<DemographicField> demographicFields)
        {
            if (string.IsNullOrEmpty(paymentDistributionCode))
            {
                throw new ArgumentNullException("paymentDistributionCode", "A payment distribution code is required for Colleague Self-Service instant enrollment.");
            }
            if (string.IsNullOrEmpty(citizenshipHomeCountryCode))
            {
                throw new ArgumentNullException("citizenshipHomeCountryCode", "A citizenship home country code is required for Colleague Self-Service instant enrollment.");
            }
            StudentProgramAssignmentBehavior = behavior;
            PaymentDistributionCode = paymentDistributionCode;
            CitizenshipHomeCountryCode = citizenshipHomeCountryCode;
            RegistrationUserRole = registrationUserRole;
            SearchEndDate = searchEndDate;

            academicProgramOptions = academicProgramOptions != null ? academicProgramOptions.Where(ap => ap != null).ToList() : new List<AcademicProgramOption>();
            foreach(var option in academicProgramOptions)
            {
                AddAcademicProgramOption(option);
            }
            AcademicProgramOptions = _academicProgramOptions.AsReadOnly();
            if (!AcademicProgramOptions.Any())
            {
                throw new ArgumentNullException("AcademicProgramOptions", "At least one academic program option must be listed for Colleague Self-Service instant enrollment.");
            }

            SubjectCodesToDisplayInCatalog = _subjectCodesToDisplayInCatalog.AsReadOnly();
            WebPaymentsImplemented = webPaymentsImplemented;
            CatalogFilterOptions = _catalogFilterOptions.AsReadOnly();
            ShowInstantEnrollmentBookstoreLink = showInstantEnrollmentBookstoreLink;

            demographicFields = demographicFields != null ? demographicFields.Where(field => field != null).ToList() : new List<DemographicField>();
            foreach (var demographicField in demographicFields)
            {
                AddDemographicFields(demographicField);
            }
            DemographicFields = _demographicFields.AsReadOnly();
            if (DemographicFields == null || !DemographicFields.Any())
            {
                throw new ArgumentNullException("DemographicFields", "Demographic Fields are required for Colleague Self-Service instant enrollment.");
            }
        }

        /// <summary>
        /// Add an <see cref="AcademicProgramOption"/> to the <see cref="InstantEnrollmentConfiguration"/>
        /// </summary>
        /// <param name="option"><see cref="AcademicProgramOption"/> to add</param>
        private void AddAcademicProgramOption(AcademicProgramOption option)
        {
            if (!_academicProgramOptions.Any(o => o.Code == option.Code))
            {
                _academicProgramOptions.Add(option);
            }
        }

        /// <summary>
        /// Add a subject code to the list of subject codes used to restrict which course sections are available in the Colleague Self-Service instant enrollment workflows
        /// </summary>
        /// <remarks>Only non-null, non-empty subject codes may be added, and a subject code may only be added to the list once</remarks>
        /// <param name="subjectCode">Subject code</param>
        public void AddSubjectCodeToDisplayInCatalog(string subjectCode)
        {
            if (!string.IsNullOrEmpty(subjectCode) && !_subjectCodesToDisplayInCatalog.Any(o => o == subjectCode))
            {
                _subjectCodesToDisplayInCatalog.Add(subjectCode);
            }
        }

        /// <summary>
        /// Add a search option, and a boolean as to whether offer that option to the student, to the list of options implemented by the Instant Enrollment Catalog Search
        /// </summary>
        /// <param name="type">One of the elements in the <see cref="CatalogFilterType"/> enumeration of search options</param>
        /// <param name="isHidden">Boolean indicating whether to enable that search option</param>
        public void AddCatalogFilterOption(CatalogFilterType type, bool isHidden)
        {
            // Can only have one filter option in the list with a specific type
            if (!CatalogFilterOptions.Any(a => a.Type.Equals(type)))
            {
                CatalogFilterOption newOption = new CatalogFilterOption(type, isHidden);
                _catalogFilterOptions.Add(newOption);
            }
        }

        /// <summary>
        /// Add a <see cref="DemographicField"/> to the <see cref="InstantEnrollmentConfiguration"/>
        /// </summary>
        /// <param name="demographicField"><see cref="DemographicField"/> to add</param>
        private void AddDemographicFields(DemographicField demographicField)
        {
            if (!_demographicFields.Any(field => field.Code == demographicField.Code))
            {
                _demographicFields.Add(demographicField);
            }
        }

    }
}
