// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class InstantEnrollmentZeroCostRegistrationDtoToEntityAdapterTests
    {
        private InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter _regAdapter;
        private InstantEnrollmentPersonDemographicsDtoToEntityAdapter _demoAdapter;
        private InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter _sectAdapter;
        private PhoneDtoAdapter _phoneAdapter;
        private InstantEnrollmentZeroCostRegistration _zeroCostRegistration;

        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _regAdapter = new InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter(_adapterRegistryMock.Object, _loggerMock.Object);
            _demoAdapter = new InstantEnrollmentPersonDemographicsDtoToEntityAdapter(_adapterRegistryMock.Object, _loggerMock.Object);
            _phoneAdapter = new PhoneDtoAdapter(_adapterRegistryMock.Object, _loggerMock.Object);
            _sectAdapter = new InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter(_adapterRegistryMock.Object, _loggerMock.Object);

            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration>()).Returns(_regAdapter);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic>()).Returns(_demoAdapter);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>()).Returns(_phoneAdapter);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister,
                        Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>()).Returns(_sectAdapter);

            _zeroCostRegistration = new InstantEnrollmentZeroCostRegistration()
            {
                PersonId = null,
                PersonDemographic = new InstantEnrollmentPersonDemographic()
                {
                    FirstName = "Joe",
                    LastName = "UnitTest",
                    MiddleName = "MiddleName",
                    Prefix = "Mr",
                    Suffix = "Jr",
                    Gender = "M",
                    BirthDate = new DateTime(1963, 11, 12),
                    EmailAddress = "testClass@testClass.com",
                    AddressLines = new List<string>() { "123 Main St" },
                    City = "Fairfax",
                    State = "VA",
                    ZipCode = "22033",
                    CountyCode = "FFX",
                    CountryCode = "US",
                    CitizenshipCountryCode = "US",
                    PersonPhones = new List<Dtos.Base.Phone>()
                    {
                        new Dtos.Base.Phone(){ Number = "7037037030", TypeCode = "BUS", Extension = "100"}
                    },
                    EthnicGroups = new List<string>() { "ETH1", "ETH1", null },
                    GovernmentId = "123-45-6789",
                    RacialGroups = new List<string>() { "HUMAN", "HUMAN", null },
                },
                AcademicProgram = "NURSE",
                ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
                {
                    new InstantEnrollmentRegistrationBaseSectionToRegister(){SectionId = "1", AcademicCredits = null, MarketingSource = "", RegistrationReason = "" },
                    new InstantEnrollmentRegistrationBaseSectionToRegister(){SectionId = "2", AcademicCredits = 9, MarketingSource = "RADIO", RegistrationReason = "Trust Fund" },
                },
                Catalog = "Sears Roebuck",
                EducationalGoal = "6 Years or bust",
            };
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter_Valid()
        {
            var entity = _regAdapter.MapToType(_zeroCostRegistration);
            Assert.AreEqual(_zeroCostRegistration.AcademicProgram, entity.AcademicProgram);
            Assert.AreEqual(_zeroCostRegistration.Catalog, entity.Catalog);
            Assert.AreEqual(_zeroCostRegistration.EducationalGoal, entity.EducationalGoal);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.BirthDate, entity.PersonDemographic.BirthDate);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.CitizenshipCountryCode, entity.PersonDemographic.CitizenshipCountryCode);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.City, entity.PersonDemographic.City);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.CountryCode, entity.PersonDemographic.CountryCode);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.CountyCode, entity.PersonDemographic.CountyCode);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.EmailAddress, entity.PersonDemographic.EmailAddress);
            Assert.AreEqual(1, entity.PersonDemographic.EthnicGroups.Count); // Nulls and duplicates should be removed
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.EthnicGroups[0], entity.PersonDemographic.EthnicGroups[0]);
            Assert.AreEqual(1, entity.PersonDemographic.RacialGroups.Count); // Nulls and duplicates should be removed
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.RacialGroups[0], entity.PersonDemographic.RacialGroups[0]);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.FirstName, entity.PersonDemographic.FirstName);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.Gender, entity.PersonDemographic.Gender);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.LastName, entity.PersonDemographic.LastName);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.MiddleName, entity.PersonDemographic.MiddleName);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.Prefix, entity.PersonDemographic.Prefix);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.State, entity.PersonDemographic.State);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.Suffix, entity.PersonDemographic.Suffix);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.GovernmentId, entity.PersonDemographic.GovernmentId);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.ZipCode, entity.PersonDemographic.ZipCode);
            Assert.AreEqual(string.Empty, entity.PersonId);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].AcademicCredits, entity.ProposedSections[0].AcademicCredits);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].MarketingSource, entity.ProposedSections[0].MarketingSource);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].RegistrationReason, entity.ProposedSections[0].RegistrationReason);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].SectionId, entity.ProposedSections[0].SectionId);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].AcademicCredits, entity.ProposedSections[1].AcademicCredits);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].MarketingSource, entity.ProposedSections[1].MarketingSource);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].RegistrationReason, entity.ProposedSections[1].RegistrationReason);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].SectionId, entity.ProposedSections[1].SectionId);
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter_Valid_PersonId()
        {
            _zeroCostRegistration.PersonId = "0012345";
            _zeroCostRegistration.PersonDemographic = null;

            var entity = _regAdapter.MapToType(_zeroCostRegistration);
            Assert.AreEqual(_zeroCostRegistration.AcademicProgram, entity.AcademicProgram);
            Assert.AreEqual(_zeroCostRegistration.Catalog, entity.Catalog);
            Assert.AreEqual(_zeroCostRegistration.EducationalGoal, entity.EducationalGoal);
            Assert.AreEqual(_zeroCostRegistration.PersonId, entity.PersonId);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].AcademicCredits, entity.ProposedSections[0].AcademicCredits);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].MarketingSource, entity.ProposedSections[0].MarketingSource);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].RegistrationReason, entity.ProposedSections[0].RegistrationReason);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[0].SectionId, entity.ProposedSections[0].SectionId);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].AcademicCredits, entity.ProposedSections[1].AcademicCredits);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].MarketingSource, entity.ProposedSections[1].MarketingSource);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].RegistrationReason, entity.ProposedSections[1].RegistrationReason);
            Assert.AreEqual(_zeroCostRegistration.ProposedSections[1].SectionId, entity.ProposedSections[1].SectionId);
        }

        [TestMethod]
        public void InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter_with_PersonId_use_source_EmailAddress()
        {
            _zeroCostRegistration.PersonId = "0012345";
            _zeroCostRegistration.PersonDemographic = new InstantEnrollmentPersonDemographic()
            {
                FirstName = "Tim",
                LastName = "Smith",
                EmailAddress = "tim.smith@ellucian.edu"
            };

            var entity = _regAdapter.MapToType(_zeroCostRegistration);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.FirstName, entity.PersonDemographic.FirstName);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.LastName, entity.PersonDemographic.LastName);
            Assert.AreEqual(_zeroCostRegistration.PersonDemographic.EmailAddress, entity.PersonDemographic.EmailAddress);
        }
        [TestMethod]
        public void InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter_with_PersonId_no_EmailAddress()
        {
            _zeroCostRegistration.PersonId = "0012345";
            _zeroCostRegistration.PersonDemographic = new InstantEnrollmentPersonDemographic()
            {
                FirstName = "Tim",
                LastName = "Smith",
            };

            var entity = _regAdapter.MapToType(_zeroCostRegistration);
            Assert.IsNull(entity.PersonDemographic);
        }
    }
}
