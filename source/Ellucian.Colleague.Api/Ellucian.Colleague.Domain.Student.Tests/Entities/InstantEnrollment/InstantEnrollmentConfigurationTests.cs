// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentConfigurationTests
    {
        private AddNewStudentProgramBehavior behavior;
        private List<AcademicProgramOption> academicProgramOptions;
        private List<DemographicField> demographicFields;
        private string paymentDistributionCode;
        private string citizenshipHomeCountryCode;
        private bool webPaymentsImplemented;
        private string registrationUserRole;
        private CatalogFilterType types;
        private bool showInstantEnrollmentBookstoreLink;

        [TestInitialize]
        public void InstantEnrollmentConfigurationTests_Initialize()
        {
            behavior = AddNewStudentProgramBehavior.Any;
            academicProgramOptions = new List<AcademicProgramOption>()
            {
                null, // Nulls should be handled gracefully
                new AcademicProgramOption("CE.DFLT", "2014X"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2016")
            };
            paymentDistributionCode = "BANK";
            citizenshipHomeCountryCode = "US";
            webPaymentsImplemented = true;
            registrationUserRole = "CEUSER";
            types = new CatalogFilterType();
            showInstantEnrollmentBookstoreLink = false;
            demographicFields = new List<DemographicField>()
            {
                null, // Nulls should be handled gracefully
                new DemographicField("FIRST_NAME", "First Name", DemographicFieldRequirement.Required),
                new DemographicField("MIDDLE_NAME","Middle Name",DemographicFieldRequirement.Optional),
                new DemographicField("LAST_NAME","Last Name",DemographicFieldRequirement.Required),
            };
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentConfigurationTests_null_PaymentDistributionCode()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions, null, citizenshipHomeCountryCode,
                webPaymentsImplemented, registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentConfigurationTests_empty_PaymentDistributionCode()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions, string.Empty, citizenshipHomeCountryCode,
                webPaymentsImplemented, registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentConfigurationTests_null_CitizenshipHomeCountryCode()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions, paymentDistributionCode, null, webPaymentsImplemented,
                registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentConfigurationTests_empty_CitizenshipHomeCountryCode()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions, paymentDistributionCode, string.Empty,
                webPaymentsImplemented, registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_null_RegistrationUserRole_DoesNotThrow()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions, paymentDistributionCode, citizenshipHomeCountryCode,
                webPaymentsImplemented, null, null, showInstantEnrollmentBookstoreLink, demographicFields);
            Assert.AreEqual(null, entity.RegistrationUserRole);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_empty_RegistrationUserRole_DoesNotThrow()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions, paymentDistributionCode, citizenshipHomeCountryCode,
                webPaymentsImplemented, string.Empty, null, showInstantEnrollmentBookstoreLink, demographicFields);
            Assert.AreEqual(string.Empty, entity.RegistrationUserRole); 
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentConfigurationTests_null_AcademicProgramOptions()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, null, paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented,
                registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentConfigurationTests_empty_AcademicProgramOptions()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, new List<AcademicProgramOption>(), paymentDistributionCode, citizenshipHomeCountryCode,
                webPaymentsImplemented, registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentConfigurationTests_AcademicProgramOptions_only_contains_nulls()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, new List<AcademicProgramOption>() { null, null }, paymentDistributionCode, citizenshipHomeCountryCode,
                webPaymentsImplemented, registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_AcademicProgramOptions_valid()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions, paymentDistributionCode, citizenshipHomeCountryCode,
                webPaymentsImplemented, registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);
            Assert.AreEqual(behavior, entity.StudentProgramAssignmentBehavior);
            Assert.AreEqual(2, entity.AcademicProgramOptions.Count); // Null should be stripped out
            Assert.AreEqual(paymentDistributionCode, entity.PaymentDistributionCode);
            Assert.AreEqual(citizenshipHomeCountryCode, entity.CitizenshipHomeCountryCode);
            Assert.AreEqual(webPaymentsImplemented, entity.WebPaymentsImplemented);
            Assert.AreEqual(registrationUserRole, entity.RegistrationUserRole);
            Assert.AreEqual(0, entity.SubjectCodesToDisplayInCatalog.Count);
            Assert.AreEqual(showInstantEnrollmentBookstoreLink, entity.ShowInstantEnrollmentBookstoreLink);
            Assert.AreEqual(3, entity.DemographicFields.Count);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_AcademicProgramOptions_no_duplicates()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, new List<AcademicProgramOption>()
            {
                null, // Nulls should be handled gracefully
                new AcademicProgramOption("CE.DFLT", "2014X"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2018")
            }, paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented, 
            registrationUserRole, null, showInstantEnrollmentBookstoreLink, demographicFields);

            Assert.AreEqual(behavior, entity.StudentProgramAssignmentBehavior);
            Assert.AreEqual(2, entity.AcademicProgramOptions.Count); // Duplicates and nulls should be stripped out
            Assert.AreEqual(paymentDistributionCode, entity.PaymentDistributionCode, citizenshipHomeCountryCode);
            Assert.AreEqual(citizenshipHomeCountryCode, entity.CitizenshipHomeCountryCode);
            Assert.AreEqual(webPaymentsImplemented, entity.WebPaymentsImplemented);
            Assert.AreEqual(registrationUserRole, entity.RegistrationUserRole);
            Assert.AreEqual(0, entity.SubjectCodesToDisplayInCatalog.Count);
            Assert.AreEqual(showInstantEnrollmentBookstoreLink, entity.ShowInstantEnrollmentBookstoreLink);
            Assert.AreEqual(3, entity.DemographicFields.Count);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_AddSubjectCodeToDisplayInCatalog_null()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, new List<AcademicProgramOption>()
            {
                null, // Nulls should be handled gracefully
                new AcademicProgramOption("CE.DFLT", "2014X"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2018")
            }, paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented, registrationUserRole, 
            null, showInstantEnrollmentBookstoreLink, demographicFields);
            Assert.AreEqual(0, entity.SubjectCodesToDisplayInCatalog.Count);
            entity.AddSubjectCodeToDisplayInCatalog(null);
            Assert.AreEqual(0, entity.SubjectCodesToDisplayInCatalog.Count);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_AddSubjectCodeToDisplayInCatalog_empty()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, new List<AcademicProgramOption>()
            {
                null, // Nulls should be handled gracefully
                new AcademicProgramOption("CE.DFLT", "2014X"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2018")
            }, paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented, registrationUserRole, 
            null, showInstantEnrollmentBookstoreLink, demographicFields);
            Assert.AreEqual(0, entity.SubjectCodesToDisplayInCatalog.Count);
            entity.AddSubjectCodeToDisplayInCatalog(string.Empty);
            Assert.AreEqual(0, entity.SubjectCodesToDisplayInCatalog.Count);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_AddSubjectCodeToDisplayInCatalog_no_duplicates()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, new List<AcademicProgramOption>()
            {
                null, // Nulls should be handled gracefully
                new AcademicProgramOption("CE.DFLT", "2014X"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                new AcademicProgramOption("CE.SYSTEMASSIGNED", "2018")
            }, paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented, registrationUserRole, 
            null, showInstantEnrollmentBookstoreLink, demographicFields);
            Assert.AreEqual(0, entity.SubjectCodesToDisplayInCatalog.Count);
            entity.AddSubjectCodeToDisplayInCatalog("CNED");
            Assert.AreEqual(1, entity.SubjectCodesToDisplayInCatalog.Count);
            entity.AddSubjectCodeToDisplayInCatalog("CNED");
            Assert.AreEqual(1, entity.SubjectCodesToDisplayInCatalog.Count);
            entity.AddSubjectCodeToDisplayInCatalog("CNED2");
            Assert.AreEqual(2, entity.SubjectCodesToDisplayInCatalog.Count);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_CatalogSearchElements_elements_added()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions,
            paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented, registrationUserRole, 
            null, showInstantEnrollmentBookstoreLink, demographicFields);
            entity.AddCatalogFilterOption(CatalogFilterType.CourseTypes, true);

            Assert.IsNotNull(entity.CatalogFilterOptions);
            Assert.AreEqual(1, entity.CatalogFilterOptions.Count);
            Assert.AreEqual(CatalogFilterType.CourseTypes, entity.CatalogFilterOptions[0].Type);
            Assert.IsTrue(entity.CatalogFilterOptions[0].IsHidden);
        }

        [TestMethod]
        public void InstantEnrollmentConfigurationTests_CatalogSearchElements_no_duplicates()
        {
            var entity = new InstantEnrollmentConfiguration(behavior, academicProgramOptions,
            paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented, registrationUserRole, 
            null, showInstantEnrollmentBookstoreLink, demographicFields);
            entity.AddCatalogFilterOption(CatalogFilterType.CourseTypes, true);
            // try to add a duplicate
            entity.AddCatalogFilterOption(CatalogFilterType.CourseTypes, true);

            Assert.IsNotNull(entity.CatalogFilterOptions);
            Assert.AreEqual(1, entity.CatalogFilterOptions.Count);
            Assert.AreEqual(CatalogFilterType.CourseTypes, entity.CatalogFilterOptions[0].Type);
            Assert.IsTrue(entity.CatalogFilterOptions[0].IsHidden);
        }
    }
}