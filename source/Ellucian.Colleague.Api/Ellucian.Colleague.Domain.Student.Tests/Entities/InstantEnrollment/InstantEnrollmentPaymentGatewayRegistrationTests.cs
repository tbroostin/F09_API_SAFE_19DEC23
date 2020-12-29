// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentPaymentGatewayRegistrationTests
    {
        private InstantEnrollmentPersonDemographic demoFull;
        private List<InstantEnrollmentRegistrationBaseSectionToRegister> sections;

        [TestInitialize]
        public void InstantEnrollmentPaymentGatewayRegistrationTests_Initialize()
        {
            demoFull = new InstantEnrollmentPersonDemographic("Joe", "TestClass")
            {
                AddressLines = new List<string>() { "Line1", "Line2" },
                BirthDate = new DateTime(1970, 3, 1),
                CitizenshipCountryCode = "USA",
                City = "City",
                CountryCode = "CAN",
                CountyCode = "Fairfax",
                EmailAddress = "email@email.org",
                Gender = "M",
                MiddleName = "Middle",
                PersonPhones = new List<Base.Entities.Phone>() { new Base.Entities.Phone("1111", "type", "ext") },
                Prefix = "MR",
                State = "VA",
                Suffix = "JR",
                ZipCode = "ZIP"
            };
            demoFull.AddEthnicGroup("HIS");
            demoFull.AddRacialGroup("CAU");


            sections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister("SECT1",1) {MarketingSource = "MKT1", RegistrationReason = "RSN1" }
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InstantEnrollmentPaymentGatewayRegistrationTests_PayAmount_Zero()
        {
            var entity = new InstantEnrollmentPaymentGatewayRegistration(null, demoFull, "Program", "Catalog", sections, 0, "CC", "ReturnUrl","distr","provAcct", "ConvFee", 23.50m, "GL");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentPaymentGatewayRegistrationTests_Null_PayMethod()
        {
            var entity = new InstantEnrollmentPaymentGatewayRegistration(null, demoFull, "Program", "Catalog", sections, 100, null, "ReturnUrl", "distr", "provAcct", "ConvFee", 23.50m, "GL");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentPaymentGatewayRegistrationTests_Null_ReturnUrl()
        {
            var entity = new InstantEnrollmentPaymentGatewayRegistration(null, demoFull, "Program", "Catalog", sections, 100, "CC", null, "distr", "provAcct", "ConvFee", 23.50m, "GL");
        }

        [TestMethod]
        public void InstantEnrollmentPaymentGatewayRegistrationTests_Success_All_Properties_Set()
        {
            var entity = new InstantEnrollmentPaymentGatewayRegistration("PID", demoFull, "Program", "Catalog", sections, 100, "CC", "ReturnUrl", "Gldistr", "provAcct", "ConvFee", 23.50m, "ConvGL") { EducationalGoal = "GOAL"};
            Assert.AreEqual(entity.AcademicProgram, "Program");
            Assert.AreEqual(entity.Catalog, "Catalog");
            Assert.AreEqual(entity.EducationalGoal, "GOAL");
            Assert.AreEqual(entity.PaymentAmount, 100);
            Assert.AreEqual(entity.PaymentMethod, "CC");
            Assert.AreEqual(entity.GlDistribution, "Gldistr");
            Assert.AreEqual(entity.ProviderAccount, "provAcct");
            Assert.AreEqual(entity.ConvenienceFeeGlAccount, "ConvGL");
            Assert.AreEqual(entity.ConvenienceFeeDesc, "ConvFee");
            Assert.AreEqual(entity.ConvenienceFeeAmount, 23.50m);
            Assert.AreEqual(entity.ProposedSections.Count, 1);
            Assert.AreEqual(entity.ProposedSections[0].AcademicCredits, 1);
            Assert.AreEqual(entity.ProposedSections[0].SectionId, "SECT1");
            Assert.AreEqual(entity.ProposedSections[0].MarketingSource, "MKT1");
            Assert.AreEqual(entity.ProposedSections[0].RegistrationReason, "RSN1");
            Assert.AreEqual(entity.PersonDemographic.AddressLines.Count<string>(), 2);
            Assert.AreEqual(entity.PersonDemographic.AddressLines.ElementAt<string>(0), "Line1");
            Assert.AreEqual(entity.PersonDemographic.AddressLines.ElementAt<string>(1), "Line2");
            Assert.AreEqual(entity.PersonDemographic.BirthDate, new DateTime(1970, 3, 1));
            Assert.AreEqual(entity.PersonDemographic.CitizenshipCountryCode, "USA");
            Assert.AreEqual(entity.PersonDemographic.City, "City");
            Assert.AreEqual(entity.PersonDemographic.CountryCode, "CAN");
            Assert.AreEqual(entity.PersonDemographic.CountyCode, "Fairfax");
            Assert.AreEqual(entity.PersonDemographic.EmailAddress, "email@email.org");
            Assert.AreEqual(entity.PersonDemographic.EthnicGroups[0], demoFull.EthnicGroups[0]);
            Assert.AreEqual(entity.PersonDemographic.FirstName, "Joe");
            Assert.AreEqual(entity.PersonDemographic.Gender, "M");
            Assert.AreEqual(entity.PersonDemographic.LastName, "TestClass");
            Assert.AreEqual(entity.PersonDemographic.MiddleName, "Middle");
            Assert.AreEqual(entity.PersonDemographic.PersonPhones.Count<Base.Entities.Phone>(),1);
            Assert.AreEqual(entity.PersonDemographic.PersonPhones.ElementAt<Base.Entities.Phone>(0).Number, "1111");
            Assert.AreEqual(entity.PersonDemographic.PersonPhones.ElementAt<Base.Entities.Phone>(0).TypeCode, "type");
            Assert.AreEqual(entity.PersonDemographic.PersonPhones.ElementAt<Base.Entities.Phone>(0).Extension, "ext");
            Assert.AreEqual(entity.PersonDemographic.Prefix, "MR");
            Assert.AreEqual(entity.PersonDemographic.RacialGroups[0], demoFull.RacialGroups[0]);
            Assert.AreEqual(entity.PersonDemographic.State, "VA");
            Assert.AreEqual(entity.PersonDemographic.Suffix, "JR");
            Assert.AreEqual(entity.PersonDemographic.ZipCode, "ZIP");
        }

    }
}
