// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class AgreementsRepositoryTests : BaseRepositorySetup
    {
        private AgreementsRepository repository;

        [TestInitialize]
        public void AgreementsRepositoryTests_Initialize()
        {
            MockInitialize();
            apiSettings = new ApiSettings("TEST") { ColleagueTimeZone = "Eastern Standard Time" };
            repository = new AgreementsRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        [TestClass]
        public class AgreementsRepository_GetAgreementPeriodsAsync_Tests : AgreementsRepositoryTests
        {
            private List<Data.Base.DataContracts.AgreementPeriod> agreementPeriodsData;

            [TestInitialize]
            public void AgreementsRepositoryTests_GetAgreementPeriodsAsync_Initialize()
            {
                base.AgreementsRepositoryTests_Initialize();

                agreementPeriodsData = new List<Data.Base.DataContracts.AgreementPeriod>()
                {
                    new Data.Base.DataContracts.AgreementPeriod() { Recordkey = "2019FA", AgreementPeriodDescription = "2019 Fall" },
                    new Data.Base.DataContracts.AgreementPeriod() { Recordkey = "2020SP", AgreementPeriodDescription = "2020 Spring" },
                    new Data.Base.DataContracts.AgreementPeriod() { Recordkey = string.Empty, AgreementPeriodDescription = "No Code" } // No AGREEMENT.PERIOD.ID, should not be built
                };
                SetupAgreementPeriodData();
            }

            [TestMethod]
            public async Task AgreementsRepository_GetAgreementPeriodsAsync_Success()
            {
                var agreementPeriods = await repository.GetAgreementPeriodsAsync();
                Assert.IsNotNull(agreementPeriods);
                Assert.AreEqual(agreementPeriodsData.Count - 1, agreementPeriods.Count());
                for (int i = 0; i < agreementPeriodsData.Count - 1; i++)
                {
                    Assert.AreEqual(agreementPeriodsData[i].Recordkey, agreementPeriods.ElementAt(i).Code);
                    Assert.AreEqual(agreementPeriodsData[i].AgreementPeriodDescription, agreementPeriods.ElementAt(i).Description);
                }
            }

            private void SetupAgreementPeriodData()
            {
                MockRecordsAsync("AGREEMENT.PERIOD", agreementPeriodsData);
            }
        }

        [TestClass]
        public class AgreementsRepository_GetPersonAgreementsByPersonIdAsync_Tests : AgreementsRepositoryTests
        {
            private List<Data.Base.DataContracts.PersonAgreements> personAgreementsData;

            [TestInitialize]
            public void AgreementsRepositoryTests_GetAgreementsAsync_Initialize()
            {
                base.AgreementsRepositoryTests_Initialize();

                personAgreementsData = new List<Data.Base.DataContracts.PersonAgreements>()
                {
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "1", PagrTitle = "Person Agreement 1", PagrAllowDecline = "Y", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "2019FA", PagrText = "This is the text of Agreement 1.", PagrCode = "AGR1", PagrPersonId = "0001234", PagrAgreeStatus = "A", PagrActionDate = DateTime.Today.AddDays(-3), PagrActionTime = DateTime.Today.AddHours(-4) },
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "2", PagrTitle = "Person Agreement 2", PagrAllowDecline = "N", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "2019FA", PagrText = null, PagrCode = "AGR2", PagrPersonId = "0001234", PagrAgreeStatus = string.Empty, PagrActionDate = null, PagrActionTime = null },
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "3", PagrTitle = "Person Agreement 3"+DmiString._VM+"Line 2"+DmiString._VM+"Line 3", PagrAllowDecline = "Y", PagrDueDate = DateTime.Today.AddDays(28), PagrPeriod = "2019FA", PagrText = "This is the text of Agreement 3."+DmiString._VM+"Line 2"+DmiString._VM+"Line 3", PagrCode = "AGR2", PagrPersonId = "0001234", PagrAgreeStatus = "D", PagrActionDate = DateTime.Today.AddDays(-2), PagrActionTime = DateTime.Today.AddHours(-3) },
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "4", PagrTitle = "Person Agreement 4", PagrAllowDecline = "N", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "2019FA", PagrText = null, PagrCode = "AGR2", PagrPersonId = string.Empty, PagrAgreeStatus = "A"}, // No PAGR.PERSON.ID, should not be built
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = string.Empty, PagrTitle = "No Code", PagrAllowDecline = "N", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "2019FA", PagrText = null, PagrCode = "AGR2", PagrPersonId = "0001234", PagrAgreeStatus = "A"}, // No PERSON.AGREEMENTS.ID, should not be built
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "5", PagrTitle = "Person Agreement 5", PagrAllowDecline = "N", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = string.Empty, PagrText = null, PagrCode = "AGR2", PagrPersonId = "0001234", PagrAgreeStatus = "A"}, // No PAGR.PERIOD, should not be built
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "6", PagrTitle = "Person Agreement 6", PagrAllowDecline = "N", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "2019FA", PagrText = null, PagrCode = string.Empty, PagrPersonId = "0001234", PagrAgreeStatus = "A"}, // No PAGR.PERIOD, should not be built
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "7", PagrTitle = "Person Agreement 7", PagrAllowDecline = "N", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "2019FA", PagrText = null, PagrCode = "AGR2", PagrPersonId = "0001234", PagrAgreeStatus = "D"}, // PAGR.ALLOW.DECLINE is N but PAGR.AGREE.STATUS is D, should not be built
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "8", PagrTitle = "Person Agreement 8", PagrAllowDecline = "Y", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "2019FA", PagrText = "This is the text of Agreement 8.", PagrCode = "AGR1", PagrPersonId = "0001234", PagrAgreeStatus = "X", PagrActionDate = null, PagrActionTime = null }, // PAGR.AGREE.STATUS is not a valid value, should not be built
                    new Data.Base.DataContracts.PersonAgreements() { Recordkey = "9", PagrTitle = "Person Agreement 9", PagrAllowDecline = "Y", PagrDueDate = null, PagrPeriod = "2019FA", PagrText = "This is the text of Agreement 9.", PagrCode = "AGR2", PagrPersonId = "0001234", PagrAgreeStatus = "D", PagrActionDate = DateTime.Today.AddDays(-2), PagrActionTime = DateTime.Today.AddHours(-3) }, // No PAGR.DUE.DATE, should not be built
                };
                SetupPersonAgreementsData();
            }

            [TestMethod]
            public async Task AgreementsRepository_GetPersonAgreementsByPersonIdAsync_Success()
            {
                var personAgreements = await repository.GetPersonAgreementsByPersonIdAsync("0001234");
                Assert.IsNotNull(personAgreements);
                Assert.AreEqual(3, personAgreements.Count()); // Only the first 3 PERSON.AGREEMENTS records from the sample data set are valid; the others should not be built

                // 1
                var expectedTimestamp1 = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(personAgreementsData[0].PagrActionTime, personAgreementsData[0].PagrActionDate, apiSettings.ColleagueTimeZone);
                Assert.AreEqual(personAgreementsData[0].Recordkey, personAgreements.ElementAt(0).Id);
                Assert.AreEqual(personAgreementsData[0].PagrPersonId, personAgreements.ElementAt(0).PersonId);
                Assert.AreEqual(personAgreementsData[0].PagrCode, personAgreements.ElementAt(0).AgreementCode);
                Assert.AreEqual(personAgreementsData[0].PagrPeriod, personAgreements.ElementAt(0).AgreementPeriodCode);
                Assert.IsTrue(personAgreements.ElementAt(0).PersonCanDeclineAgreement);
                Assert.AreEqual(personAgreementsData[0].PagrTitle, personAgreements.ElementAt(0).Title);
                Assert.AreEqual(personAgreementsData[0].PagrDueDate, personAgreements.ElementAt(0).DueDate);
                Assert.AreEqual(1, personAgreements.ElementAt(0).Text.Count());
                Assert.AreEqual(personAgreementsData[0].PagrText, personAgreements.ElementAt(0).Text.ElementAt(0));
                Assert.AreEqual(PersonAgreementStatus.Accepted, personAgreements.ElementAt(0).Status);
                Assert.AreEqual(expectedTimestamp1, personAgreements.ElementAt(0).ActionTimestamp);

                // 2
                Assert.AreEqual(personAgreementsData[1].Recordkey, personAgreements.ElementAt(1).Id);
                Assert.AreEqual(personAgreementsData[1].PagrTitle, personAgreements.ElementAt(1).Title);
                Assert.IsFalse(personAgreements.ElementAt(1).PersonCanDeclineAgreement);
                Assert.AreEqual(personAgreementsData[1].PagrDueDate, personAgreements.ElementAt(1).DueDate);
                Assert.AreEqual(personAgreementsData[1].PagrCode, personAgreements.ElementAt(1).AgreementCode);
                Assert.AreEqual(personAgreementsData[1].PagrPeriod, personAgreements.ElementAt(1).AgreementPeriodCode);
                CollectionAssert.AreEqual(new List<string>(), personAgreements.ElementAt(1).Text.ToList());
                Assert.IsNull(personAgreements.ElementAt(1).Status);
                Assert.IsNull(personAgreements.ElementAt(1).ActionTimestamp);

                // 3
                var expectedTimestamp2 = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(personAgreementsData[2].PagrActionTime, personAgreementsData[2].PagrActionDate, apiSettings.ColleagueTimeZone);
                Assert.AreEqual(personAgreementsData[2].Recordkey, personAgreements.ElementAt(2).Id);
                Assert.AreEqual(personAgreementsData[2].PagrPersonId, personAgreements.ElementAt(2).PersonId);
                Assert.AreEqual(personAgreementsData[2].PagrCode, personAgreements.ElementAt(2).AgreementCode);
                Assert.AreEqual(personAgreementsData[2].PagrPeriod, personAgreements.ElementAt(2).AgreementPeriodCode);
                Assert.IsTrue(personAgreements.ElementAt(2).PersonCanDeclineAgreement);
                Assert.AreEqual("Person Agreement 3" + DmiString._VM + "Line 2" + DmiString._VM + "Line 3", personAgreements.ElementAt(2).Title);
                Assert.AreEqual(personAgreementsData[2].PagrDueDate, personAgreements.ElementAt(2).DueDate);
                Assert.AreEqual(3, personAgreements.ElementAt(2).Text.Count());
                Assert.AreEqual("This is the text of Agreement 3.", personAgreements.ElementAt(2).Text.ElementAt(0));
                Assert.AreEqual("Line 2", personAgreements.ElementAt(2).Text.ElementAt(1));
                Assert.AreEqual("Line 3", personAgreements.ElementAt(2).Text.ElementAt(2));

                Assert.AreEqual(PersonAgreementStatus.Declined, personAgreements.ElementAt(2).Status);
                Assert.AreEqual(expectedTimestamp2, personAgreements.ElementAt(2).ActionTimestamp);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AgreementsRepository_GetPersonAgreementsByPersonIdAsync_null_PersonId_throws_Exception()
            {
                var personAgreements = await repository.GetPersonAgreementsByPersonIdAsync(null);
            }

            private void SetupPersonAgreementsData()
            {
                MockRecordsAsync("PERSON.AGREEMENTS", personAgreementsData);
            }
        }

        [TestClass]
        public class AgreementsRepository_UpdatePersonAgreementAsync_Tests : AgreementsRepositoryTests
        {
            private PersonAgreement agreement;
            private PersonAgreements agreementData;
            private DateTimeOffset now;

            [TestInitialize]
            public void AgreementsRepository_UpdatePersonAgreementAsync_Tests_Initialize()
            {
                base.AgreementsRepositoryTests_Initialize();

                now = DateTimeOffset.Now;
                agreement = new PersonAgreement("1", "0001234", "AGR1", "PER1", true, "Agreement 1 Line 2 Line 3", DateTime.Today.AddDays(30), new List<string>() { "This is the text of agreement 1.", "Line 2", "Line 3" }, PersonAgreementStatus.Accepted, now);
                agreementData = new Data.Base.DataContracts.PersonAgreements() { Recordkey = "1", PagrTitle = "Agreement 1 Line 2 Line 3", PagrAllowDecline = "Y", PagrDueDate = DateTime.Today.AddDays(30), PagrPeriod = "PER1", PagrText = "This is the text of agreement 1." + DmiString._VM + "Line 2" + DmiString._VM + "Line 3", PagrCode = "AGR1", PagrPersonId = "0001234", PagrAgreeStatus = "A", PagrActionDate = now.Date, PagrActionTime = now.DateTime };

                SetupPersonAgreementsData(agreementData);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AgreementsRepository_UpdatePersonAgreementAsync_null_PersonAgreement_throws_Exception()
            {
                var personAgreement = await repository.UpdatePersonAgreementAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AgreementsRepository_UpdatePersonAgreementAsync_CTX_exception_caught_and_throws_ApplicationException()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonAgreementRequest, UpdatePersonAgreementResponse>(It.IsAny<UpdatePersonAgreementRequest>())).ThrowsAsync(new Exception("CTX exception!"));
                var personAgreement = await repository.UpdatePersonAgreementAsync(agreement);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AgreementsRepository_UpdatePersonAgreementAsync_CTX_error_throws_ApplicationException()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonAgreementRequest, UpdatePersonAgreementResponse>(It.IsAny<UpdatePersonAgreementRequest>())).ReturnsAsync(new UpdatePersonAgreementResponse() { AErrorMsg = "CTX error!" });
                var personAgreement = await repository.UpdatePersonAgreementAsync(agreement);
            }

            [TestMethod]
            public async Task AgreementsRepository_UpdatePersonAgreementAsync_success_returns_updated_agreement()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonAgreementRequest, UpdatePersonAgreementResponse>(It.IsAny<UpdatePersonAgreementRequest>())).ReturnsAsync(new UpdatePersonAgreementResponse());
                var personAgreement = await repository.UpdatePersonAgreementAsync(agreement);

                Assert.IsNotNull(personAgreement);

                var expectedTimestamp1 = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(now.DateTime, now.Date, apiSettings.ColleagueTimeZone);
                Assert.AreEqual(agreementData.Recordkey, personAgreement.Id);
                Assert.AreEqual(agreementData.PagrPersonId, personAgreement.PersonId);
                Assert.AreEqual(agreementData.PagrCode, personAgreement.AgreementCode);
                Assert.AreEqual(agreementData.PagrPeriod, personAgreement.AgreementPeriodCode);
                Assert.IsTrue(personAgreement.PersonCanDeclineAgreement);
                Assert.AreEqual("Agreement 1 Line 2 Line 3", personAgreement.Title);
                Assert.AreEqual(agreementData.PagrDueDate, personAgreement.DueDate);
                Assert.AreEqual("This is the text of agreement 1.", personAgreement.Text.ElementAt(0));
                Assert.AreEqual("Line 2", personAgreement.Text.ElementAt(1));
                Assert.AreEqual("Line 3", personAgreement.Text.ElementAt(2));
                Assert.AreEqual(PersonAgreementStatus.Accepted, personAgreement.Status);
                Assert.AreEqual(expectedTimestamp1, personAgreement.ActionTimestamp);
            }

            private void SetupPersonAgreementsData(PersonAgreements data)
            {
                MockRecordsAsync("PERSON.AGREEMENTS", new List<PersonAgreements>() { data });
            }
        }
    }
}
