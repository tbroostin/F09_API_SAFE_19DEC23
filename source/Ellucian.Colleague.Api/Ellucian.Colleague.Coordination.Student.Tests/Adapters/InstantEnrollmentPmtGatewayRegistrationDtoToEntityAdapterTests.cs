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
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapterTests
    {
        private InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapter regAdapter;
        private InstantEnrollmentPersonDemographicsDtoToEntityAdapter demoAdapter;
        private InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter sectAdapter;
        private PhoneDtoAdapter phoneAdapter;
        private InstantEnrollmentPaymentGatewayRegistration dto;

        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            regAdapter = new InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            demoAdapter = new InstantEnrollmentPersonDemographicsDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            phoneAdapter = new PhoneDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            sectAdapter = new InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>()).Returns(regAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic>()).Returns(demoAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>()).Returns(phoneAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister,
                        Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>()).Returns(sectAdapter);

            dto = new InstantEnrollmentPaymentGatewayRegistration()
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
                    AddressLines = new List<string>() { "123 Main St", "Second" },
                    City = "Fairfax",
                    State = "VA",
                    ZipCode = "22033",
                    CountyCode = "FFX",
                    CountryCode = "US",
                    CitizenshipCountryCode = "US",
                    PersonPhones = new List<Dtos.Base.Phone>()
                    {
                        new Dtos.Base.Phone(){ Number = "7037037030", TypeCode = "BUS", Extension = "100"},
                        new Dtos.Base.Phone(){ Number = "222", TypeCode = "HM", Extension = "333"},
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
                ReturnUrl = "Url",
                GlDistribution = "GLD",
                ProviderAccount = "ProvAcct",
                ConvenienceFeeAmount = 1.23m,
                ConvenienceFeeDesc = "ConvDesc",
                ConvenienceFeeGlAccount = "ConvGl"
            };
        }

        [TestMethod]
        public void InstantEnrollmentPmtGatewayRegistrationDtoToEntity_Valid()
        {
            var entity = regAdapter.MapToType(dto);
            Assert.AreEqual(dto.AcademicProgram, entity.AcademicProgram);
            Assert.AreEqual(dto.Catalog, entity.Catalog);
            Assert.AreEqual(dto.EducationalGoal, entity.EducationalGoal);
            Assert.AreEqual(dto.PaymentAmount, entity.PaymentAmount);
            Assert.AreEqual(dto.PaymentMethod, entity.PaymentMethod);
            Assert.AreEqual(dto.PersonDemographic.AddressLines.Count, entity.PersonDemographic.AddressLines.Count<string>());
            Assert.AreEqual(dto.PersonDemographic.AddressLines[0], entity.PersonDemographic.AddressLines.ElementAt<string>(0));
            Assert.AreEqual(dto.PersonDemographic.AddressLines[1], entity.PersonDemographic.AddressLines.ElementAt<string>(1));
            Assert.AreEqual(dto.PersonDemographic.BirthDate, entity.PersonDemographic.BirthDate);
            Assert.AreEqual(dto.PersonDemographic.CitizenshipCountryCode, entity.PersonDemographic.CitizenshipCountryCode);
            Assert.AreEqual(dto.PersonDemographic.City, entity.PersonDemographic.City);
            Assert.AreEqual(dto.PersonDemographic.CountryCode, entity.PersonDemographic.CountryCode);
            Assert.AreEqual(dto.PersonDemographic.CountyCode, entity.PersonDemographic.CountyCode);
            Assert.AreEqual(dto.PersonDemographic.EmailAddress, entity.PersonDemographic.EmailAddress);
            Assert.AreEqual(1, entity.PersonDemographic.EthnicGroups.Count); // Nulls and duplicates should be removed
            Assert.AreEqual(dto.PersonDemographic.EthnicGroups[0], entity.PersonDemographic.EthnicGroups[0]);
            Assert.AreEqual(dto.PersonDemographic.FirstName, entity.PersonDemographic.FirstName);
            Assert.AreEqual(dto.PersonDemographic.Gender, entity.PersonDemographic.Gender);
            Assert.AreEqual(dto.PersonDemographic.LastName, entity.PersonDemographic.LastName);
            Assert.AreEqual(dto.PersonDemographic.MiddleName, entity.PersonDemographic.MiddleName);
            Assert.AreEqual(dto.PersonDemographic.PersonPhones.Count, entity.PersonDemographic.PersonPhones.Count<Domain.Base.Entities.Phone>());
            Assert.AreEqual(dto.PersonDemographic.PersonPhones[0].Number, entity.PersonDemographic.PersonPhones.ElementAt<Domain.Base.Entities.Phone>(0).Number);
            Assert.AreEqual(dto.PersonDemographic.PersonPhones[0].TypeCode, entity.PersonDemographic.PersonPhones.ElementAt<Domain.Base.Entities.Phone>(0).TypeCode);
            Assert.AreEqual(dto.PersonDemographic.PersonPhones[0].Extension, entity.PersonDemographic.PersonPhones.ElementAt<Domain.Base.Entities.Phone>(0).Extension);
            Assert.AreEqual(dto.PersonDemographic.PersonPhones[1].Number, entity.PersonDemographic.PersonPhones.ElementAt<Domain.Base.Entities.Phone>(1).Number);
            Assert.AreEqual(dto.PersonDemographic.PersonPhones[1].TypeCode, entity.PersonDemographic.PersonPhones.ElementAt<Domain.Base.Entities.Phone>(1).TypeCode);
            Assert.AreEqual(dto.PersonDemographic.PersonPhones[1].Extension, entity.PersonDemographic.PersonPhones.ElementAt<Domain.Base.Entities.Phone>(1).Extension);
            Assert.AreEqual(dto.PersonDemographic.Prefix, entity.PersonDemographic.Prefix);
            Assert.AreEqual(1, entity.PersonDemographic.RacialGroups.Count); // Nulls and duplicates should be removed
            Assert.AreEqual(dto.PersonDemographic.RacialGroups[0], entity.PersonDemographic.RacialGroups[0]);
            Assert.AreEqual(dto.PersonDemographic.State, entity.PersonDemographic.State);
            Assert.AreEqual(dto.PersonDemographic.Suffix, entity.PersonDemographic.Suffix);
            Assert.AreEqual(dto.PersonDemographic.ZipCode, entity.PersonDemographic.ZipCode);
            Assert.AreEqual(dto.PersonDemographic.GovernmentId, entity.PersonDemographic.GovernmentId);
            Assert.AreEqual(string.Empty, entity.PersonId);
            Assert.AreEqual(dto.ProposedSections.Count, entity.ProposedSections.Count);
            Assert.AreEqual(dto.ProposedSections[0].AcademicCredits, entity.ProposedSections[0].AcademicCredits);
            Assert.AreEqual(dto.ProposedSections[0].MarketingSource, entity.ProposedSections[0].MarketingSource);
            Assert.AreEqual(dto.ProposedSections[0].RegistrationReason, entity.ProposedSections[0].RegistrationReason);
            Assert.AreEqual(dto.ProposedSections[0].SectionId, entity.ProposedSections[0].SectionId);
            Assert.AreEqual(dto.ProposedSections[1].AcademicCredits, entity.ProposedSections[1].AcademicCredits);
            Assert.AreEqual(dto.ProposedSections[1].MarketingSource, entity.ProposedSections[1].MarketingSource);
            Assert.AreEqual(dto.ProposedSections[1].RegistrationReason, entity.ProposedSections[1].RegistrationReason);
            Assert.AreEqual(dto.ProposedSections[1].SectionId, entity.ProposedSections[1].SectionId);
            Assert.AreEqual(dto.ReturnUrl, entity.ReturnUrl);
            Assert.AreEqual(dto.GlDistribution, entity.GlDistribution);
            Assert.AreEqual(dto.ProviderAccount, entity.ProviderAccount);
            Assert.AreEqual(dto.ConvenienceFeeAmount, entity.ConvenienceFeeAmount);
            Assert.AreEqual(dto.ConvenienceFeeDesc, entity.ConvenienceFeeDesc);
            Assert.AreEqual(dto.ConvenienceFeeGlAccount, entity.ConvenienceFeeGlAccount);
        }

        [TestMethod]
        public void InstantEnrollmentPmtGatewayRegistrationDtoToEntity_Valid_PersonId()
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
            Assert.AreEqual(dto.ReturnUrl, entity.ReturnUrl);
        }

        [TestMethod]
        public void InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapter_with_PersonId_use_source_EmailAddress()
        {
            dto.PersonId = "0012345";
            dto.PersonDemographic = new InstantEnrollmentPersonDemographic()
            {
                FirstName = "Tim",
                LastName = "Smith",
                EmailAddress = "tim.smith@ellucian.edu"
            };

            var entity = regAdapter.MapToType(dto);
            Assert.AreEqual(dto.PersonDemographic.FirstName, entity.PersonDemographic.FirstName);
            Assert.AreEqual(dto.PersonDemographic.LastName, entity.PersonDemographic.LastName);
            Assert.AreEqual(dto.PersonDemographic.EmailAddress, entity.PersonDemographic.EmailAddress);
        }
        [TestMethod]
        public void InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapter_with_PersonId_no_EmailAddress()
        {
            dto.PersonId = "0012345";
            dto.PersonDemographic = new InstantEnrollmentPersonDemographic()
            {
                FirstName = "Tim",
                LastName = "Smith",
            };

            var entity = regAdapter.MapToType(dto);
            Assert.IsNull(entity.PersonDemographic);
        }
    }
}
