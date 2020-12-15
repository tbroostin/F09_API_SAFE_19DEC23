// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class InstantEnrollmentEcheckRegistrationDtoToEntityAdapterTests
    {
        private InstantEnrollmentEcheckRegistrationDtoToEntityAdapter regAdapter;
        private InstantEnrollmentPersonDemographicsDtoToEntityAdapter demoAdapter;
        private InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter sectAdapter;
        private PhoneDtoAdapter phoneAdapter;
        private InstantEnrollmentEcheckRegistration dto;

        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            regAdapter = new InstantEnrollmentEcheckRegistrationDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            demoAdapter = new InstantEnrollmentPersonDemographicsDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            phoneAdapter = new PhoneDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            sectAdapter = new InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration>()).Returns(regAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic>()).Returns(demoAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>()).Returns(phoneAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister,
                        Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>()).Returns(sectAdapter);

            dto = new InstantEnrollmentEcheckRegistration()
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
                PaymentAmount = 10098,
                PaymentMethod = "ECHK",
                ProviderAccount = "PPECHK",
                BankAccountOwner = "Parent UnitTest",
                BankAccountRoutingNumber = "Routing",
                BankAccountCheckNumber = "100",
                BankAccountNumber = "Number",
                BankAccountType = "CHECK",
                ConvenienceFeeAmount = 98,
                ConvenienceFeeDesc = "Convenience Fee",
                ConvenienceFeeGlAccount = "Conv GL",
                GovernmentId = "98798",
                GovernmentIdState = "VA",
                PayerAddress = "567 Main St",
                PayerCity = "Manassas",
                PayerState = "VA",
                PayerPostalCode = "20110",
                PayerEmailAddress = "chump@email.com",
            };
        }

        [TestMethod]
        public void InstantEnrollmentEcheckRegistrationDtoToEntityAdapter_Valid()
        {
            var entity = regAdapter.MapToType(dto);
            Assert.AreEqual(dto.AcademicProgram, entity.AcademicProgram);
            Assert.AreEqual(dto.BankAccountCheckNumber, entity.BankAccountCheckNumber);
            Assert.AreEqual(dto.BankAccountNumber, entity.BankAccountNumber);
            Assert.AreEqual(dto.BankAccountOwner, entity.BankAccountOwner);
            Assert.AreEqual(dto.BankAccountRoutingNumber, entity.BankAccountRoutingNumber);
            Assert.AreEqual(dto.BankAccountType, entity.BankAccountType);
            Assert.AreEqual(dto.Catalog, entity.Catalog);
            Assert.AreEqual(dto.ConvenienceFeeAmount, entity.ConvenienceFeeAmount);
            Assert.AreEqual(dto.ConvenienceFeeDesc, entity.ConvenienceFeeDesc);
            Assert.AreEqual(dto.ConvenienceFeeGlAccount, entity.ConvenienceFeeGlAccount);
            Assert.AreEqual(dto.EducationalGoal, entity.EducationalGoal);
            Assert.AreEqual(dto.GovernmentId, entity.GovernmentId);
            Assert.AreEqual(dto.GovernmentIdState, entity.GovernmentIdState);
            Assert.AreEqual(dto.PayerAddress, entity.PayerAddress);
            Assert.AreEqual(dto.PayerCity, entity.PayerCity);
            Assert.AreEqual(dto.PayerEmailAddress, entity.PayerEmailAddress);
            Assert.AreEqual(dto.PayerPostalCode, entity.PayerPostalCode);
            Assert.AreEqual(dto.PayerState, entity.PayerState);
            Assert.AreEqual(dto.PaymentAmount, entity.PaymentAmount);
            Assert.AreEqual(dto.PaymentMethod, entity.PaymentMethod);
            Assert.AreEqual(dto.PersonDemographic.BirthDate, entity.PersonDemographic.BirthDate);
            Assert.AreEqual(dto.PersonDemographic.CitizenshipCountryCode, entity.PersonDemographic.CitizenshipCountryCode);
            Assert.AreEqual(dto.PersonDemographic.City, entity.PersonDemographic.City);
            Assert.AreEqual(dto.PersonDemographic.CountryCode, entity.PersonDemographic.CountryCode);
            Assert.AreEqual(dto.PersonDemographic.CountyCode, entity.PersonDemographic.CountyCode);
            Assert.AreEqual(dto.PersonDemographic.EmailAddress, entity.PersonDemographic.EmailAddress);
            Assert.AreEqual(1, entity.PersonDemographic.EthnicGroups.Count); // Nulls and duplicates should be removed
            Assert.AreEqual(dto.PersonDemographic.EthnicGroups[0], entity.PersonDemographic.EthnicGroups[0]);
            Assert.AreEqual(dto.PersonDemographic.GovernmentId, entity.PersonDemographic.GovernmentId);
            Assert.AreEqual(dto.PersonDemographic.FirstName, entity.PersonDemographic.FirstName);
            Assert.AreEqual(dto.PersonDemographic.Gender, entity.PersonDemographic.Gender);
            Assert.AreEqual(dto.PersonDemographic.LastName, entity.PersonDemographic.LastName);
            Assert.AreEqual(dto.PersonDemographic.MiddleName, entity.PersonDemographic.MiddleName);
            Assert.AreEqual(dto.PersonDemographic.Prefix, entity.PersonDemographic.Prefix);
            Assert.AreEqual(1, entity.PersonDemographic.RacialGroups.Count); // Nulls and duplicates should be removed
            Assert.AreEqual(dto.PersonDemographic.RacialGroups[0], entity.PersonDemographic.RacialGroups[0]);
            Assert.AreEqual(dto.PersonDemographic.State, entity.PersonDemographic.State);
            Assert.AreEqual(dto.PersonDemographic.Suffix, entity.PersonDemographic.Suffix);
            Assert.AreEqual(dto.PersonDemographic.ZipCode, entity.PersonDemographic.ZipCode);
            Assert.AreEqual(string.Empty, entity.PersonId);
            Assert.AreEqual(dto.ProposedSections[0].AcademicCredits, entity.ProposedSections[0].AcademicCredits);
            Assert.AreEqual(dto.ProposedSections[0].MarketingSource, entity.ProposedSections[0].MarketingSource);
            Assert.AreEqual(dto.ProposedSections[0].RegistrationReason, entity.ProposedSections[0].RegistrationReason);
            Assert.AreEqual(dto.ProposedSections[0].SectionId, entity.ProposedSections[0].SectionId);
            Assert.AreEqual(dto.ProposedSections[1].AcademicCredits, entity.ProposedSections[1].AcademicCredits);
            Assert.AreEqual(dto.ProposedSections[1].MarketingSource, entity.ProposedSections[1].MarketingSource);
            Assert.AreEqual(dto.ProposedSections[1].RegistrationReason, entity.ProposedSections[1].RegistrationReason);
            Assert.AreEqual(dto.ProposedSections[1].SectionId, entity.ProposedSections[1].SectionId);
            Assert.AreEqual(dto.ProviderAccount, entity.ProviderAccount);
        }

        [TestMethod]
        public void InstantEnrollmentEcheckRegistrationDtoToEntityAdapter_Valid_PersonId()
        {
            dto.PersonDemographic = null;
            dto.PersonId = "0012345";
            var entity = regAdapter.MapToType(dto);
            Assert.AreEqual(dto.AcademicProgram, entity.AcademicProgram);
            Assert.AreEqual(dto.Catalog, entity.Catalog);
            Assert.AreEqual(dto.EducationalGoal, entity.EducationalGoal);
            Assert.AreEqual(dto.PaymentAmount, entity.PaymentAmount);
            Assert.AreEqual(dto.PaymentMethod, entity.PaymentMethod);
            Assert.AreEqual(dto.PersonId, entity.PersonId);
            Assert.AreEqual(dto.ProposedSections.Count, entity.ProposedSections.Count);
            Assert.AreEqual(dto.ProposedSections[0].AcademicCredits, entity.ProposedSections[0].AcademicCredits);
            Assert.AreEqual(dto.ProposedSections[0].MarketingSource, entity.ProposedSections[0].MarketingSource);
            Assert.AreEqual(dto.ProposedSections[0].RegistrationReason, entity.ProposedSections[0].RegistrationReason);
            Assert.AreEqual(dto.ProposedSections[0].SectionId, entity.ProposedSections[0].SectionId);
            Assert.AreEqual(dto.ProposedSections[1].AcademicCredits, entity.ProposedSections[1].AcademicCredits);
            Assert.AreEqual(dto.ProposedSections[1].MarketingSource, entity.ProposedSections[1].MarketingSource);
            Assert.AreEqual(dto.ProposedSections[1].RegistrationReason, entity.ProposedSections[1].RegistrationReason);
            Assert.AreEqual(dto.ProposedSections[1].SectionId, entity.ProposedSections[1].SectionId);
        }

    }
}
