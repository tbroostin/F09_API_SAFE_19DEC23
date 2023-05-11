// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class InstantEnrollmentRepositoryTests : BaseRepositorySetup
    {
        private InstantEnrollmentRepository repository;
        private ApiSettings apiSettingsMock;
        private InstEnrollProposedRgstrtnResponse registrationResponse;
        private InstEnrollZeroCostRgstrtnResponse zeroCostRegistrationResponse;
        private InstEnrollEcheckRgstrtnResponse echeckResponse;

        [TestInitialize]
        public void Initialize()
        {
            registrationResponse = new InstEnrollProposedRgstrtnResponse();
            registrationResponse.ErrorOccurred = false;
            registrationResponse.RegisteredSectionInformation = new List<RegisteredSectionInformation>() {
                new RegisteredSectionInformation() { RegisteredSection="s001", RegisteredSectionCost=100 } ,
                null,
                new RegisteredSectionInformation(){ RegisteredSection = "s002", RegisteredSectionCost = null } ,
                new RegisteredSectionInformation(){ RegisteredSection = null, RegisteredSectionCost = 99.99m },
                new RegisteredSectionInformation(){ RegisteredSection = "s003", RegisteredSectionCost = 2.456m },
                new RegisteredSectionInformation(){ RegisteredSection = null, RegisteredSectionCost = null },
            };

            //zero cost registration response
            zeroCostRegistrationResponse = new InstEnrollZeroCostRgstrtnResponse();
            zeroCostRegistrationResponse.StudentId = "001";
            zeroCostRegistrationResponse.ErrorOccurred = false;
            zeroCostRegistrationResponse.AUserName = "aaubergine";
            zeroCostRegistrationResponse.ZeroCostRegisteredSectionInformation = new List<ZeroCostRegisteredSectionInformation>() {
                new ZeroCostRegisteredSectionInformation() { RegisteredSection="s001", RegisteredSectionCost=100 } ,
                null,
                new ZeroCostRegisteredSectionInformation(){ RegisteredSection = "s002", RegisteredSectionCost = null } ,
                new ZeroCostRegisteredSectionInformation(){ RegisteredSection = null, RegisteredSectionCost = 0.00m },
                new ZeroCostRegisteredSectionInformation(){ RegisteredSection = "s003", RegisteredSectionCost = 0.00m },
                new ZeroCostRegisteredSectionInformation(){ RegisteredSection = null, RegisteredSectionCost = null },
                new ZeroCostRegisteredSectionInformation(){ RegisteredSection = "s004", RegisteredSectionCost = 1.99m },
            };

            zeroCostRegistrationResponse.ZeroCostRegistrationMessages = new List<ZeroCostRegistrationMessages>() {
                new ZeroCostRegistrationMessages() { Message = "msg1", MessageSection = "s004" },
                null,
                new ZeroCostRegistrationMessages() { Message = null, MessageSection = "s005" },
                new ZeroCostRegistrationMessages() { Message = "msg2", MessageSection = null }
            };

            // echeck
            echeckResponse = new InstEnrollEcheckRgstrtnResponse()
            {
                CashReceiptId = "RCPT",
                EcheckRegisteredSectionInformation = new List<EcheckRegisteredSectionInformation>()
                {
                    new EcheckRegisteredSectionInformation() {RegisteredSection = "SECT1", RegisteredSectionCost = 10000},
                    new EcheckRegisteredSectionInformation() {RegisteredSection = "SECT2", RegisteredSectionCost = 20000 }
                },
                EcheckRegistrationMessages = new List<EcheckRegistrationMessages>()
                {
                    new EcheckRegistrationMessages(){MessageSection = "SECT1", Message = "SECT1 Message"},
                    new EcheckRegistrationMessages(){MessageSection = "SECT2", Message = "SECT2 Message"},
                },
                ErrorOccurred = false,
                AUserName = "aaubergine"
            };

            // Initialize Mock framework
            MockInitialize();
            transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstEnrollProposedRgstrtnRequest, InstEnrollProposedRgstrtnResponse>(It.IsAny<InstEnrollProposedRgstrtnRequest>())).ReturnsAsync(registrationResponse);
            transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstEnrollZeroCostRgstrtnRequest, InstEnrollZeroCostRgstrtnResponse>(It.IsAny<InstEnrollZeroCostRgstrtnRequest>())).ReturnsAsync(zeroCostRegistrationResponse);
            transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstEnrollEcheckRgstrtnRequest, InstEnrollEcheckRgstrtnResponse>(It.IsAny<InstEnrollEcheckRgstrtnRequest>())).ReturnsAsync(echeckResponse);

            // Initialize API settings
            apiSettingsMock = new ApiSettings("TEST");

            // Build the test repository
            repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
        }

        [TestClass]
        public class InstantEnrollmentRepository_GetProposedRegistrationResultAync : InstantEnrollmentRepositoryTests
        {
            List<InstantEnrollmentRegistrationBaseSectionToRegister> sectionsToRegister = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
            InstantEnrollmentPersonDemographic personDemographic = new InstantEnrollmentPersonDemographic("firstname", "lastname");
            List<Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();

            [TestInitialize]
            public void InstantEnrollmentRepository_GetProposedRegistrationResultAync_Initialize()
            {
                base.Initialize();
                sectionsToRegister.Add(new InstantEnrollmentRegistrationBaseSectionToRegister("s001", 0));
                sectionsToRegister.Add(null);
                sectionsToRegister.Add(new InstantEnrollmentRegistrationBaseSectionToRegister("s002", 1));
                sectionsToRegister.Add(new InstantEnrollmentRegistrationBaseSectionToRegister("s003", 2));

                //person demographic
                personDemographic.EmailAddress = "aa@email.com";

                //phones
                phones.Add(new Domain.Base.Entities.Phone("111"));
                phones.Add(null);
                phones.Add(new Domain.Base.Entities.Phone("222", "t1"));
                phones.Add(new Domain.Base.Entities.Phone("222", "t1", "ext01"));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_Proposedregistration_IsNull()
            {
                await repository.GetProposedRegistrationResultAync(null);
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_ProposedSections_IsNotEmpty()
            {
                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "MATH.BA", "CATALOG", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.RegisteredSections.Count());
            }

            //with students phones
            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_PersonDemoPhones_NotEmpty()
            {
                personDemographic.PersonPhones = phones;
                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "MATH.BA", "CATALOG", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.RegisteredSections.Count());
            }

            //with addresses
            //with students phones
            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_PersonDemoAddress_NotEmpty()
            {
                personDemographic.PersonPhones = phones;
                personDemographic.AddressLines = new List<string>() { "address1", "address2" };
                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "MATH.BA", "CATALOG", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.RegisteredSections.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_PersonDemoPhones_IsNull()
            {
                personDemographic.PersonPhones = null;
                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "MATH.BA", "CaTALOG", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.RegisteredSections.Count());

            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_PersonDemoAddress_IsNull()
            {
                personDemographic.PersonPhones = phones;
                personDemographic.AddressLines = null;
                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "MATH.BA", "CATALOG", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.RegisteredSections.Count());

            }

            //when reponse have registration messages only- sections are null
            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_Response_Sections_AreNull()
            {
                registrationResponse.RegisteredSectionInformation = null;
                registrationResponse.RegistrationMessages = null;
                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "CATALOG", "MATH.BA", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.RegisteredSections.Count());
                Assert.AreEqual(0, result.RegistrationMessages.Count());

            }

            //when response have registration messages with few with empty messages and sections
            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_Response_Messages_NoTNull_Sections_Null()
            {
                registrationResponse.RegisteredSectionInformation = null;
                registrationResponse.RegistrationMessages = new List<RegistrationMessages>() {
                    new RegistrationMessages() { Messages="msg1", MessageSections="s004" },
                    null,
                    new RegistrationMessages() { Messages=null, MessageSections="s005" },
                    new RegistrationMessages() { Messages="msg2", MessageSections=null }


                };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstEnrollProposedRgstrtnRequest, InstEnrollProposedRgstrtnResponse>(It.IsAny<InstEnrollProposedRgstrtnRequest>())).ReturnsAsync(registrationResponse);

                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "MATH.BA", "CATALOG", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.RegisteredSections.Count());
                Assert.AreEqual(2, result.RegistrationMessages.Count());

            }
            //when response have registration sections as well as messages
            [TestMethod]
            public async Task InstantEnrollmentRepository_GetProposedRegistrationResultAync_Response_Messages_NoTNull_Sections_NotNull()
            {
                registrationResponse.RegistrationMessages = new List<RegistrationMessages>() {
                    new RegistrationMessages() { Messages="msg1", MessageSections="s004" },
                    null,
                    new RegistrationMessages() { Messages=null, MessageSections="s005" },
                    new RegistrationMessages() { Messages="msg2", MessageSections=null }


                };
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstEnrollProposedRgstrtnRequest, InstEnrollProposedRgstrtnResponse>(It.IsAny<InstEnrollProposedRgstrtnRequest>())).ReturnsAsync(registrationResponse);

                InstantEnrollmentProposedRegistration iepr = new InstantEnrollmentProposedRegistration("0011", personDemographic, "MATH.BA", "CATALOG", sectionsToRegister);
                InstantEnrollmentProposedRegistrationResult result = await repository.GetProposedRegistrationResultAync(iepr);
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.RegisteredSections.Count());
                Assert.AreEqual(2, result.RegistrationMessages.Count());
            }
        }


        [TestClass]
        public class InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync : InstantEnrollmentRepositoryTests
        {
            private InstantEnrollmentZeroCostRegistration zeroCostRegistration;
            private List<InstantEnrollmentRegistrationBaseSectionToRegister> sectionsToRegister = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
            private InstantEnrollmentPersonDemographic personDemographic = new InstantEnrollmentPersonDemographic("firstname", "lastname");
            private List<Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();

            [TestInitialize]
            public void InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_Initialize()
            {
                base.Initialize();
                sectionsToRegister.Add(new InstantEnrollmentRegistrationBaseSectionToRegister("s001", 0));
                sectionsToRegister.Add(null);
                sectionsToRegister.Add(new InstantEnrollmentRegistrationBaseSectionToRegister("s002", 1));
                sectionsToRegister.Add(new InstantEnrollmentRegistrationBaseSectionToRegister("s003", null));
                sectionsToRegister.Add(new InstantEnrollmentRegistrationBaseSectionToRegister("s004", null));

                //person demographic
                personDemographic.EmailAddress = "aa@email.com";

                //phones
                phones.Add(new Domain.Base.Entities.Phone("111"));
                phones.Add(null);
                phones.Add(new Domain.Base.Entities.Phone("222", "t1"));
                phones.Add(new Domain.Base.Entities.Phone("222", "t1", "ext01"));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_ZeroCostRegistration_IsNull()
            {
                await repository.GetZeroCostRegistrationResultAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_AcadProgram_IsNull()
            {
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration(null, personDemographic, null, "2019", sectionsToRegister);
                await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_AcadProgram_IsEmpty()
            {
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration(null, personDemographic, string.Empty, "2019", sectionsToRegister);
                await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_Catalog_IsNull()
            {
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration(null, personDemographic, "POLI.BA", null, sectionsToRegister);
                await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_Catalog_IsEmpty()
            {
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration(null, personDemographic, "POLI.BA", string.Empty, sectionsToRegister);
                await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_ProposedSections_AreNull()
            {
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", null);
                await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_ProposedSections_AreEmpty()
            {
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", new List<InstantEnrollmentRegistrationBaseSectionToRegister>());
                await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueSessionExpiredException))]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_RepositoryThrowsColleagueExpiredException()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstEnrollZeroCostRgstrtnRequest, InstEnrollZeroCostRgstrtnResponse>(
                    It.IsAny<InstEnrollZeroCostRgstrtnRequest>()))
                    .Returns(() =>
                    {
                        throw new ColleagueSessionExpiredException("session timeout");
                    });

                zeroCostRegistrationResponse.StudentId = null;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);

                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_Returns_NullStudentId()
            {
                zeroCostRegistrationResponse.StudentId = null;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.AreEqual(null, result.PersonId);
                Assert.AreEqual(zeroCostRegistrationResponse.AUserName, result.UserName);
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_Returns_EmptyStudentId()
            {
                zeroCostRegistrationResponse.StudentId = string.Empty;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.AreEqual(string.Empty, result.PersonId);
                Assert.AreEqual(zeroCostRegistrationResponse.AUserName, result.UserName);
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_PersonDemoPhones_AreNull()
            {
                personDemographic.PersonPhones = null;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_PersonDemoPhones_AreEmpty()
            {
                personDemographic.PersonPhones = new List<Phone>();
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_PersonDemoPhones_AreNotEmpty()
            {
                personDemographic.PersonPhones = phones;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_PersonDemoAddress_AreNull()
            {
                personDemographic.AddressLines = null;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_PersonDemoAddress_AreEmpty()
            {
                personDemographic.AddressLines = new List<string>();
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_PersonDemoAddress_AreNotEmpty()
            {
                personDemographic.AddressLines = new List<string>() { "address1", "address2" };
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_ResponseSectionsAndMessages_AreNull()
            {
                zeroCostRegistrationResponse.ZeroCostRegisteredSectionInformation = null;
                zeroCostRegistrationResponse.ZeroCostRegistrationMessages = null;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.RegisteredSections.Count());
                Assert.AreEqual(0, result.RegistrationMessages.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_ResponseSections_AreNull()
            {
                zeroCostRegistrationResponse.ZeroCostRegisteredSectionInformation = null;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.RegisteredSections.Count());
                Assert.AreEqual(2, result.RegistrationMessages.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_ResponseMessages_AreNull()
            {
                zeroCostRegistrationResponse.ZeroCostRegistrationMessages = null;
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
                Assert.AreEqual(0, result.RegistrationMessages.Count());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetZeroCostRegistrationResultAsync_ResponseSectionsAndMessages_AreNotNull()
            {
                zeroCostRegistration = new InstantEnrollmentZeroCostRegistration("001", personDemographic, "POLI.BA", "2018", sectionsToRegister);
                var result = await repository.GetZeroCostRegistrationResultAsync(zeroCostRegistration);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.RegisteredSections.Count());
                Assert.AreEqual(2, result.RegistrationMessages.Count());
            }
        }

        [TestClass]
        public class InstantEnrollmentRepository_GetEcheckRegistrationResultAsync : InstantEnrollmentRepositoryTests
        {
            private InstantEnrollmentEcheckRegistration ieer;
            [TestInitialize]
            public void InstantEnrollmentRepository_GetEcheckRegistrationResultAsync_Initialize()
            {
                ieer = new InstantEnrollmentEcheckRegistration("0000001",
                    new InstantEnrollmentPersonDemographic("Joe", "TestClass")
                    {
                        City = "CITY",
                        State = "STATE",
                        ZipCode = "ZIP",
                        EmailAddress = "JOE@EMAIL.COM",
                    },
                    "PROGRAM",
                    "CATALOG",
                    new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
                    {
                        new InstantEnrollmentRegistrationBaseSectionToRegister("SECT1", 300),
                        new InstantEnrollmentRegistrationBaseSectionToRegister("SECT2", 300),
                    },
                    10000,
                    "METHOD",
                    "PROVIDER.ACCOUNT",
                    "PAYER",
                    "ROUTING",
                    "BANK.ACCOUNT.NUMBER",
                    "CHECK.NUMBER",
                    "CHECK",
                    "", null, "",
                    "PAYER@EMAIL.COM",
                    "PAYER.ADDRESS",
                    "PAYER.CITY",
                    "PAYER.STATE",
                    "PAYER.ZIP");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetEcheckRegistrationResultAync_ProposedRegistration_IsNull()
            {
                await repository.GetEcheckRegistrationResultAsync(null);
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetEcheckRegistrationResultAync_ProposedSections_IsEmpty_responseIsNUll()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstEnrollEcheckRgstrtnRequest, InstEnrollEcheckRgstrtnResponse>(It.IsAny<InstEnrollEcheckRgstrtnRequest>())).ReturnsAsync(echeckResponse);

                var resp = await repository.GetEcheckRegistrationResultAsync(ieer);
                Assert.AreEqual(false, resp.ErrorOccurred);
                Assert.AreEqual("SECT1", resp.RegisteredSections[0].SectionId);
                Assert.AreEqual(10000, resp.RegisteredSections[0].SectionCost);
                Assert.AreEqual("SECT2", resp.RegisteredSections[1].SectionId);
                Assert.AreEqual(20000, resp.RegisteredSections[1].SectionCost);
                Assert.AreEqual("SECT1", resp.RegistrationMessages[0].MessageSection);
                Assert.AreEqual("SECT1 Message", resp.RegistrationMessages[0].Message);
                Assert.AreEqual("SECT2", resp.RegistrationMessages[1].MessageSection);
                Assert.AreEqual("SECT2 Message", resp.RegistrationMessages[1].Message);
                Assert.AreEqual(echeckResponse.AUserName, resp.UserName);
            }
        }

        [TestClass]
        public class InstantEnrollmentRepository_StartInstantEnrollmentPaymentGatewayTransactionAsync : InstantEnrollmentRepositoryTests
        {
            [TestInitialize]
            public void InstantEnrollmentRepository_StartInstantEnrollmentPaymentGatewayTransactionAsync_Initialize()
            {
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_StartInstantEnrollmentPaymentGatewayTransactionAsync_RegRequestNull()
            {
                await repository.StartInstantEnrollmentPaymentGatewayTransactionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task InstantEnrollmentRepository_StartInstantEnrollmentPaymentGatewayTransactionAsync_ReturnObjNull()
            {

                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentPaymentGatewayRegRequest, InstantEnrollmentPaymentGatewayRegResponse>(It.Is<InstantEnrollmentPaymentGatewayRegRequest>(
                    rq => rq.StudentId == "S1"))).ReturnsAsync(() => null);
                InstantEnrollmentPaymentGatewayRegistration req = new InstantEnrollmentPaymentGatewayRegistration("S1", null, "PROG", "CAT",
                        new List<InstantEnrollmentRegistrationBaseSectionToRegister>() { new InstantEnrollmentRegistrationBaseSectionToRegister("SID1", 1) }, 100, "CC", "URL", "Gldistr", "provAcct", "ConvFee", 23.50m, "ConvGL");
                await repository.StartInstantEnrollmentPaymentGatewayTransactionAsync(req);
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_StartInstantEnrollmentPaymentGatewayTransactionAsync_ReturnMessages()
            {

                // Create a response
                InstantEnrollmentPaymentGatewayRegResponse response = new InstantEnrollmentPaymentGatewayRegResponse()
                {
                    PmtGatewayRegistrationMessages = new List<PmtGatewayRegistrationMessages>() { new PmtGatewayRegistrationMessages() { Messages = "Msg1" }, new PmtGatewayRegistrationMessages() { Messages = "Msg2" } }
                };

                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentPaymentGatewayRegRequest, InstantEnrollmentPaymentGatewayRegResponse>(It.Is<InstantEnrollmentPaymentGatewayRegRequest>(
                    rq => rq.StudentId == "S1"))).ReturnsAsync(response);
                InstantEnrollmentPaymentGatewayRegistration req = new InstantEnrollmentPaymentGatewayRegistration("S1", null, "PROG", "CAT",
                        new List<InstantEnrollmentRegistrationBaseSectionToRegister>() { new InstantEnrollmentRegistrationBaseSectionToRegister("SID1", 1) }, 100, "CC", "URL", "Gldistr", "provAcct", "ConvFee", 23.50m, "ConvGL");
                var result = await repository.StartInstantEnrollmentPaymentGatewayTransactionAsync(req);
                Assert.AreEqual(result.ErrorMessages.Count, 2);
                Assert.AreEqual(result.ErrorMessages[0], "Msg1");
                Assert.AreEqual(result.ErrorMessages[1], "Msg2");

            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_StartInstantEnrollmentPaymentGatewayTransactionAsync_AllAttributesMapped()
            {

                // Create an entity with every attribute set
                InstantEnrollmentPersonDemographic demoFull = new InstantEnrollmentPersonDemographic("Joe", "TestClass")
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
                    PersonPhones = new List<Phone>() { new Phone("1111", "type", "ext"), new Phone("2222", "type2", "ext2") },
                    Prefix = "MR",
                    State = "VA",
                    Suffix = "JR",
                    ZipCode = "ZIP"
                };
                demoFull.AddEthnicGroup("ETH1");
                demoFull.AddEthnicGroup("ETH2");
                demoFull.AddRacialGroup("HIS");
                demoFull.AddRacialGroup("RAC2");

                List<InstantEnrollmentRegistrationBaseSectionToRegister> sections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
                {
                    new InstantEnrollmentRegistrationBaseSectionToRegister("SECT1",1) {MarketingSource = "MKT1", RegistrationReason = "RSN1" },
                    new InstantEnrollmentRegistrationBaseSectionToRegister("SECT2",2) {MarketingSource = "MKT2", RegistrationReason = "RSN2" },
                };

                InstantEnrollmentPaymentGatewayRegistration entity =
                        new InstantEnrollmentPaymentGatewayRegistration
                            ("PID", demoFull, "Program", "Catalog", sections, 100, "CC", "ReturnUrl",
                              "Gldistr", "provAcct", "ConvFee", 23.50m, "ConvGL")
                        { EducationalGoal = "GOAL" };

                // Create a response
                InstantEnrollmentPaymentGatewayRegResponse response = new InstantEnrollmentPaymentGatewayRegResponse()
                {
                    ExtlPaymentUrl = "ReturnUrl333"
                };

                // Mock a call to the invoker in which every entity attribute matches.
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentPaymentGatewayRegRequest, InstantEnrollmentPaymentGatewayRegResponse>
                    (It.Is<InstantEnrollmentPaymentGatewayRegRequest>(
                    rq => (rq.StudentId == "PID") && (rq.AcadProgram == "Program") && (rq.Catalog == "Catalog") &&
                          (rq.PaymentAmt == 100) && (rq.PaymentMethod == "CC") && (rq.ReturnUrl == "ReturnUrl") &&
                          (rq.GlDistribution == "Gldistr") && (rq.ProviderAccount == "provAcct") &&
                          (rq.ConvenienceFeeAmt == 23.50m) && (rq.ConvenienceFeeDesc == "ConvFee") && (rq.ConvenienceFeeGlNo == "ConvGL") &&
                          (rq.EducationalGoal == "GOAL") &&
                          (rq.PmtGatewayProposedSectionInformation.Count() == 2) &&
                          (rq.PmtGatewayProposedSectionInformation[0].ProposedSectionCredits == 1) && (rq.PmtGatewayProposedSectionInformation[0].ProposedSections == "SECT1") &&
                          (rq.PmtGatewayProposedSectionInformation[0].ProposedSectionMktgSrc == "MKT1") && (rq.PmtGatewayProposedSectionInformation[0].ProposedSectionRegReason == "RSN1") &&
                          (rq.PmtGatewayProposedSectionInformation[1].ProposedSectionCredits == 2) && (rq.PmtGatewayProposedSectionInformation[1].ProposedSections == "SECT2") &&
                          (rq.PmtGatewayProposedSectionInformation[1].ProposedSectionMktgSrc == "MKT2") && (rq.PmtGatewayProposedSectionInformation[1].ProposedSectionRegReason == "RSN2") &&
                          (rq.StudentAddress.Count == 2) && (rq.StudentAddress[0] == "Line1") && (rq.StudentAddress[1] == "Line2") &&
                          (DateTime.Compare((DateTime)rq.StudentBirthDate, new DateTime(1970, 3, 1)) == 0) &&
                          (rq.StudentCitizenship == "USA") && (rq.StudentCity == "City") && (rq.StudentCountry == "CAN") &&
                          (rq.StudentCounty == "Fairfax") && (rq.StudentEmailAddress == "email@email.org") &&
                          (rq.StudentEthnics.Count == 2) && (rq.StudentEthnics[0] == "ETH1") && (rq.StudentEthnics[1] == "ETH2") &&
                          (rq.StudentGivenName == "Joe") && (rq.StudentId == "PID") && (rq.StudentMiddleName == "Middle") &&
                          (rq.StudentPostalCode == "ZIP") && (rq.StudentPrefix == "MR") &&
                          (rq.StudentRacialGroups.Count == 2) && (rq.StudentRacialGroups[0] == "HIS") && (rq.StudentRacialGroups[1] == "RAC2") &&
                          (rq.StudentState == "VA") && (rq.StudentSuffix == "JR") &&
                          (rq.StudentTaxId == null) &&
                          (rq.PmtGatewayStudentPhones.Count == 2) &&
                          (rq.PmtGatewayStudentPhones[0].StudentPhoneNumbers == "1111") && (rq.PmtGatewayStudentPhones[0].StudentPhoneExtensions == "ext") &&
                          (rq.PmtGatewayStudentPhones[0].StudentPhoneTypes == "type") &&
                          (rq.PmtGatewayStudentPhones[1].StudentPhoneNumbers == "2222") && (rq.PmtGatewayStudentPhones[1].StudentPhoneExtensions == "ext2") &&
                          (rq.PmtGatewayStudentPhones[1].StudentPhoneTypes == "type2")
                    ))).ReturnsAsync(response);

                var result = await repository.StartInstantEnrollmentPaymentGatewayTransactionAsync(entity);
                Assert.AreEqual(result.PaymentProviderRedirectUrl, "ReturnUrl333");
                Assert.AreEqual(result.ErrorMessages.Count, 0);
            }
        }

        [TestClass]
        public class InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_Tests : InstantEnrollmentRepositoryTests
        {
            [TestInitialize]
            public void InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_Tests_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_null_request()
            {
                await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_ctx_exception()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(It.IsAny<InstantEnrollmentBuildAcknowledgementParagraphRequest>())).
                    ThrowsAsync(new ApplicationException("CTX exception during execution."));
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
                await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_null_ctx_response()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(It.IsAny<InstantEnrollmentBuildAcknowledgementParagraphRequest>())).
                    ReturnsAsync(() => null);
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
                await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_ctx_response_with_error()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(It.IsAny<InstantEnrollmentBuildAcknowledgementParagraphRequest>())).
                    ReturnsAsync(new InstantEnrollmentBuildAcknowledgementParagraphResponse()
                    {
                        AError = "Error from CTX"
                    });
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
                await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueSessionExpiredException))]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_RepositoryThrowsColleagueExpiredException()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(
                    It.IsAny<InstantEnrollmentBuildAcknowledgementParagraphRequest>()))
                    .Returns(() =>
                    {
                        throw new ColleagueSessionExpiredException("session timeout");
                    });

                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
                await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_ctx_response_with_null_text()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(It.IsAny<InstantEnrollmentBuildAcknowledgementParagraphRequest>())).
                    ReturnsAsync(new InstantEnrollmentBuildAcknowledgementParagraphResponse()
                    {
                        ParaText = null
                    });
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
                var text = await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
                CollectionAssert.AreEqual(new List<string>(), text.ToList());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_ctx_response_with_empty_text()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(It.IsAny<InstantEnrollmentBuildAcknowledgementParagraphRequest>())).
                    ReturnsAsync(new InstantEnrollmentBuildAcknowledgementParagraphResponse()
                    {
                        ParaText = new List<string>()
                    });
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
                var text = await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
                CollectionAssert.AreEqual(new List<string>(), text.ToList());
            }

            [TestMethod]
            public async Task InstantEnrollmentRepository_GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_ctx_response_with_text()
            {
                transManagerMock.Setup(transInv => transInv.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(It.IsAny<InstantEnrollmentBuildAcknowledgementParagraphRequest>())).
                    ReturnsAsync(new InstantEnrollmentBuildAcknowledgementParagraphResponse()
                    {
                        ParaText = new List<string>() { "Line 1", "Line 2" }
                    });
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
                var text = await repository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
                CollectionAssert.AreEqual(new List<string>() { "Line 1", "Line 2" }, text.ToList());
            }
        }

        #region GetMatchingPersonResultsInstantEnrollmentAsync

        [TestClass]
        public class GetMatchingPersonResultsInstantEnrollmentAsync : InstantEnrollmentRepositoryTests
        {
            PersonMatchCriteriaInstantEnrollment criteria;
            private Mock<IColleagueDataReader> dataAccessorMock;

            [TestInitialize]
            public void Initialize_GetMatchingPersonResultsInstantEnrollmentAsync()
            {
                base.Initialize();
                // setup person object

                criteria = new PersonMatchCriteriaInstantEnrollment("First", "Last");

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataAccessorMock.Setup(da => da.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new string[0]);
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Build the test repository
                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_NullCriteria()
            {
                var results = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_Errors()
            {
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.IsAny<Transactions.GetPersonMatchInstEnrlRequest>())
                    ).ReturnsAsync(
                        new Transactions.GetPersonMatchInstEnrlResponse()
                        {
                            AlErrorMessages = new List<string>() { "An error occurred." },
                            MatchedPersons = new List<Transactions.MatchedPersons>()
                        });

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var results = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(criteria);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_Valid_1_Definite_Match()
            {
                // Setup a fully populated entity
                Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment entity = new Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment("First", "Last");
                var addressLines = new List<string>() { "Line1", "Line2" };
                entity.AddressLines = addressLines;
                entity.BirthDate = new DateTime(2020, 1, 2);
                entity.CitizenshipCountryCode = "CAN";
                entity.City = "Fairfax";
                entity.CountryCode = "USA";
                entity.CountyCode = "Montgomery";
                entity.EmailAddress = "email@address.com";
                entity.Gender = "M";
                entity.MiddleName = "Middle";
                entity.Prefix = "Mr.";
                entity.State = "VA";
                entity.Suffix = "Jr.";
                entity.ZipCode = "22333";

                // Verify that all entity attributes are copied to the CTX request object
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.Is<Transactions.GetPersonMatchInstEnrlRequest>(rq => rq.ABirthDate == new DateTime(2020, 1, 2) &&
                        rq.ACitizenshipCountry == "CAN" &&
                        rq.ACity == "Fairfax" &&
                        rq.ACountry == "USA" &&
                        rq.ACounty == "Montgomery" &&
                        rq.AEmailAddress == "email@address.com" &&
                        rq.AFirstName == "First" &&
                        rq.AGender == "M" &&
                        rq.AlAddressLines.Count == 2 &&
                        rq.AlAddressLines.ElementAt(0) == "Line1" &&
                        rq.AlAddressLines.ElementAt(1) == "Line2" &&
                        rq.ALastName == "Last" &&
                        rq.AMiddleName == "Middle" &&
                        rq.APrefix == "Mr." &&
                        rq.ASsn == null &&
                        rq.AState == "VA" &&
                        rq.ASuffix == "Jr." &&
                        rq.AZip == "22333"))).ReturnsAsync(
                            new Transactions.GetPersonMatchInstEnrlResponse()
                            {
                                AlErrorMessages = new List<String>(),
                                MatchedPersons = new List<Transactions.MatchedPersons>()
                                {
                                    new Transactions.MatchedPersons()
                                    {
                                        AlMatchedCategories = "D",
                                        AlMatchedPersonIds = "0003315",
                                        AlMatchedScores = 100
                                    }
                                }
                            });

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(entity);
                Assert.AreEqual("0003315", result.PersonId);
                Assert.AreEqual(false, result.HasPotentialMatches);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_Valid_1_Potential_Match()
            {
                // Setup a fully populated entity
                Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment entity = new Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment("First", "Last");
                var addressLines = new List<string>() { "Line1", "Line2" };
                entity.AddressLines = addressLines;
                entity.BirthDate = new DateTime(2020, 1, 2);
                entity.CitizenshipCountryCode = "CAN";
                entity.City = "Fairfax";
                entity.CountryCode = "USA";
                entity.CountyCode = "Montgomery";
                entity.EmailAddress = "email@address.com";
                entity.Gender = "M";
                entity.MiddleName = "Middle";
                entity.Prefix = "Mr.";
                entity.State = "VA";
                entity.Suffix = "Jr.";
                entity.ZipCode = "22333";

                // Verify that all entity attributes are copied to the CTX request object
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.Is<Transactions.GetPersonMatchInstEnrlRequest>(rq => rq.ABirthDate == new DateTime(2020, 1, 2) &&
                        rq.ACitizenshipCountry == "CAN" &&
                        rq.ACity == "Fairfax" &&
                        rq.ACountry == "USA" &&
                        rq.ACounty == "Montgomery" &&
                        rq.AEmailAddress == "email@address.com" &&
                        rq.AFirstName == "First" &&
                        rq.AGender == "M" &&
                        rq.AlAddressLines.Count == 2 &&
                        rq.AlAddressLines.ElementAt(0) == "Line1" &&
                        rq.AlAddressLines.ElementAt(1) == "Line2" &&
                        rq.ALastName == "Last" &&
                        rq.AMiddleName == "Middle" &&
                        rq.APrefix == "Mr." &&
                        rq.ASsn == null &&
                        rq.AState == "VA" &&
                        rq.ASuffix == "Jr." &&
                        rq.AZip == "22333"))).ReturnsAsync(
                            new Transactions.GetPersonMatchInstEnrlResponse()
                            {
                                AlErrorMessages = new List<String>(),
                                MatchedPersons = new List<Transactions.MatchedPersons>()
                                {
                                    new Transactions.MatchedPersons()
                                    {
                                        AlMatchedCategories = "P",
                                        AlMatchedPersonIds = "0003315",
                                        AlMatchedScores = 60
                                    }
                                }
                            });

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(entity);
                Assert.AreEqual(null, result.PersonId);
                Assert.AreEqual(true, result.HasPotentialMatches);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_Valid_Multiple_Definite_Matches()
            {
                // Setup a fully populated entity
                Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment entity = new Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment("First", "Last");
                var addressLines = new List<string>() { "Line1", "Line2" };
                entity.AddressLines = addressLines;
                entity.BirthDate = new DateTime(2020, 1, 2);
                entity.CitizenshipCountryCode = "CAN";
                entity.City = "Fairfax";
                entity.CountryCode = "USA";
                entity.CountyCode = "Montgomery";
                entity.EmailAddress = "email@address.com";
                entity.Gender = "M";
                entity.MiddleName = "Middle";
                entity.Prefix = "Mr.";
                entity.State = "VA";
                entity.Suffix = "Jr.";
                entity.ZipCode = "22333";

                // Verify that all entity attributes are copied to the CTX request object
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.Is<Transactions.GetPersonMatchInstEnrlRequest>(rq => rq.ABirthDate == new DateTime(2020, 1, 2) &&
                        rq.ACitizenshipCountry == "CAN" &&
                        rq.ACity == "Fairfax" &&
                        rq.ACountry == "USA" &&
                        rq.ACounty == "Montgomery" &&
                        rq.AEmailAddress == "email@address.com" &&
                        rq.AFirstName == "First" &&
                        rq.AGender == "M" &&
                        rq.AlAddressLines.Count == 2 &&
                        rq.AlAddressLines.ElementAt(0) == "Line1" &&
                        rq.AlAddressLines.ElementAt(1) == "Line2" &&
                        rq.ALastName == "Last" &&
                        rq.AMiddleName == "Middle" &&
                        rq.APrefix == "Mr." &&
                        rq.ASsn == null &&
                        rq.AState == "VA" &&
                        rq.ASuffix == "Jr." &&
                        rq.AZip == "22333"))).ReturnsAsync(
                            new Transactions.GetPersonMatchInstEnrlResponse()
                            {
                                AlErrorMessages = new List<String>(),
                                MatchedPersons = new List<Transactions.MatchedPersons>()
                                {
                                    new Transactions.MatchedPersons()
                                    {
                                        AlMatchedCategories = "D",
                                        AlMatchedPersonIds = "0003315",
                                        AlMatchedScores = 100
                                    },
                                    new Transactions.MatchedPersons()
                                    {
                                        AlMatchedCategories = "D",
                                        AlMatchedPersonIds = "0003316",
                                        AlMatchedScores = 100
                                    }
                                }
                            });

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(entity);
                Assert.AreEqual(null, result.PersonId);
                Assert.AreEqual(true, result.HasPotentialMatches);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_Valid_Multiple_Potential_Matches()
            {
                // Setup a fully populated entity
                Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment entity = new Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment("First", "Last");
                var addressLines = new List<string>() { "Line1", "Line2" };
                entity.AddressLines = addressLines;
                entity.BirthDate = new DateTime(2020, 1, 2);
                entity.CitizenshipCountryCode = "CAN";
                entity.City = "Fairfax";
                entity.CountryCode = "USA";
                entity.CountyCode = "Montgomery";
                entity.EmailAddress = "email@address.com";
                entity.Gender = "M";
                entity.MiddleName = "Middle";
                entity.Prefix = "Mr.";
                entity.State = "VA";
                entity.Suffix = "Jr.";
                entity.ZipCode = "22333";

                // Verify that all entity attributes are copied to the CTX request object
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.Is<Transactions.GetPersonMatchInstEnrlRequest>(rq => rq.ABirthDate == new DateTime(2020, 1, 2) &&
                        rq.ACitizenshipCountry == "CAN" &&
                        rq.ACity == "Fairfax" &&
                        rq.ACountry == "USA" &&
                        rq.ACounty == "Montgomery" &&
                        rq.AEmailAddress == "email@address.com" &&
                        rq.AFirstName == "First" &&
                        rq.AGender == "M" &&
                        rq.AlAddressLines.Count == 2 &&
                        rq.AlAddressLines.ElementAt(0) == "Line1" &&
                        rq.AlAddressLines.ElementAt(1) == "Line2" &&
                        rq.ALastName == "Last" &&
                        rq.AMiddleName == "Middle" &&
                        rq.APrefix == "Mr." &&
                        rq.ASsn == null &&
                        rq.AState == "VA" &&
                        rq.ASuffix == "Jr." &&
                        rq.AZip == "22333"))).ReturnsAsync(
                            new Transactions.GetPersonMatchInstEnrlResponse()
                            {
                                AlErrorMessages = new List<String>(),
                                MatchedPersons = new List<Transactions.MatchedPersons>()
                                {
                                    new Transactions.MatchedPersons()
                                    {
                                        AlMatchedCategories = "P",
                                        AlMatchedPersonIds = "0003315",
                                        AlMatchedScores = 60
                                    },
                                    new Transactions.MatchedPersons()
                                    {
                                        AlMatchedCategories = "P",
                                        AlMatchedPersonIds = "0003316",
                                        AlMatchedScores = 60
                                    }
                                }
                            });

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(entity);
                Assert.AreEqual(null, result.PersonId);
                Assert.AreEqual(true, result.HasPotentialMatches);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_NullResponse()
            {
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.IsAny<Transactions.GetPersonMatchInstEnrlRequest>())
                    ).ReturnsAsync(() => null);

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var results = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(criteria);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_NullMatchedPersons()
            {
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.IsAny<Transactions.GetPersonMatchInstEnrlRequest>())
                    ).ReturnsAsync(
                        new Transactions.GetPersonMatchInstEnrlResponse()
                        {
                            MatchedPersons = null
                        });

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(criteria);
                Assert.AreEqual(null, result.PersonId);
                Assert.AreEqual(false, result.HasPotentialMatches);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_NoMatchResults()
            {
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.IsAny<Transactions.GetPersonMatchInstEnrlRequest>())
                    ).ReturnsAsync(
                        new Transactions.GetPersonMatchInstEnrlResponse()
                        {
                            MatchedPersons = new List<Transactions.MatchedPersons>()
                        });

                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(criteria);
                Assert.AreEqual(null, result.PersonId);
                Assert.AreEqual(false, result.HasPotentialMatches);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsInstantEnrollmentAsync_NoMatchResults_matching_GovernmentId()
            {
                transManagerMock.Setup<Task<Transactions.GetPersonMatchInstEnrlResponse>>(
                    manager => manager.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(
                        It.IsAny<Transactions.GetPersonMatchInstEnrlRequest>())
                    ).ReturnsAsync(
                        new Transactions.GetPersonMatchInstEnrlResponse()
                        {
                            MatchedPersons = new List<Transactions.MatchedPersons>(),
                            ADuplicateGovtId = true
                        });
                dataAccessorMock.Setup(da => da.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new string[1] { "0003315" });
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Build the test repository
                repository = new InstantEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                criteria.GovernmentId = "123-45-6789";
                var result = await repository.GetMatchingPersonResultsInstantEnrollmentAsync(criteria);
                Assert.AreEqual(null, result.PersonId);
                Assert.AreEqual(false, result.HasPotentialMatches);
                Assert.AreEqual(true, result.DuplicateGovernmentIdFound);
            }
        }

        #endregion
    }
}