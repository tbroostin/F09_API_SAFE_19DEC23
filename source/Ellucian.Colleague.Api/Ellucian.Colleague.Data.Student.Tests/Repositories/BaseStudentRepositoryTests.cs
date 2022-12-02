// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class BaseStudentRepositoryTests : BaseRepositorySetup
    {
        private BaseStudentRepository _repository;

        [TestInitialize]
        public void BaseStudentRepositoryTests_Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Set up data reads
            BaseStudentRepository_DataReader_Setup();

            // Build the test repository
            _repository = new BaseStudentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class BaseStudentRepository_GetRegistrationConfigurationAsync_Tests : BaseStudentRepositoryTests
        {
            [TestInitialize]
            public void BaseStudentRepository_GetRegistrationConfigurationAsync_Initialize()
            {
                base.BaseStudentRepositoryTests_Initialize();
            }

            [TestMethod]
            public async Task BaseStudentRepository_GetRegistrationConfigurationAsync_null_RegDefaults_returns_RegistrationConfiguration()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS", It.IsAny<bool>())).ReturnsAsync(() => null);
                var data = await _repository.GetRegistrationConfigurationAsync();
                Assert.AreEqual(false, data.RequireFacultyAddAuthorization);
                Assert.AreEqual(0, data.AddAuthorizationStartOffsetDays);
                Assert.AreEqual(false, data.RequireDropReason);
                Assert.AreEqual(false, data.PromptForDropReason);
                Assert.AreEqual(false, data.ShowBooksOnPrintedSchedules);
                Assert.AreEqual(false, data.ShowCommentsOnPrintedSchedules);
                Assert.AreEqual(true, data.AddDefaultTermsToDegreePlan);
                Assert.AreEqual(false, data.QuickRegistrationIsEnabled);
                Assert.AreEqual(0, data.QuickRegistrationTermCodes.Count);
                Assert.IsFalse(data.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
                Assert.IsNull(data.CensusDateNumberForPromptingIntentToWithdraw);
            }

            [TestMethod]
            public async Task BaseStudentRepository_GetRegistrationConfigurationAsync_null_StwebDefaults_returns_RegistrationConfiguration()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", It.IsAny<bool>())).ReturnsAsync(() => null);
                var data = await _repository.GetRegistrationConfigurationAsync();
                Assert.AreEqual(true, data.RequireFacultyAddAuthorization);
                Assert.AreEqual(2, data.AddAuthorizationStartOffsetDays);
                Assert.AreEqual(false, data.RequireDropReason);
                Assert.AreEqual(false, data.PromptForDropReason);
                Assert.AreEqual(false, data.ShowBooksOnPrintedSchedules);
                Assert.AreEqual(false, data.ShowCommentsOnPrintedSchedules);
                Assert.AreEqual(true, data.AddDefaultTermsToDegreePlan);
                Assert.AreEqual(false, data.QuickRegistrationIsEnabled);
                Assert.AreEqual(0, data.QuickRegistrationTermCodes.Count);
                Assert.IsFalse(data.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
                Assert.IsNull(data.CensusDateNumberForPromptingIntentToWithdraw);
            }

            [TestMethod]
            public async Task BaseStudentRepository_GetRegistrationConfigurationAsync_returns_RegistrationConfiguration()
            {
                var data = await _repository.GetRegistrationConfigurationAsync();
                Assert.AreEqual(true, data.RequireFacultyAddAuthorization);
                Assert.AreEqual(2, data.AddAuthorizationStartOffsetDays);
                Assert.AreEqual(false, data.RequireDropReason);
                Assert.AreEqual(false, data.PromptForDropReason);
                Assert.AreEqual(false, data.ShowBooksOnPrintedSchedules);
                Assert.AreEqual(false, data.ShowCommentsOnPrintedSchedules);
                Assert.AreEqual(true, data.AddDefaultTermsToDegreePlan);
                Assert.AreEqual(true, data.QuickRegistrationIsEnabled);
                Assert.AreEqual(2, data.QuickRegistrationTermCodes.Count);
                Assert.IsFalse(data.AlwaysPromptUsersForIntentToWithdrawWhenDropping);
                Assert.IsNull(data.CensusDateNumberForPromptingIntentToWithdraw);
            }
        }

        /// <summary>
        /// Set up data reads
        /// </summary>
        private void BaseStudentRepository_DataReader_Setup()
        {
            // STWEB.DEFAULTS
            var stwebDefaults = new StwebDefaults()
            {
                Recordkey = "STWEB.DEFAULTS",
                StwebEnableQuickReg = "Y",
                StwebQuickRegTerms = new List<string>() { string.Empty, null, "2019/FA", "2020/SP", "2020/SP" }
            };
            MockRecordsAsync<Student.DataContracts.StwebDefaults>("ST.PARMS", new List<Student.DataContracts.StwebDefaults>() { stwebDefaults });

            // REG.DEFAULTS
            var regDefaults = new RegDefaults()
            {
                Recordkey = "REG.DEFAULTS",
                RgdRequireAddAuthFlag = "Y",
                RgdAddAuthStartOffset = 2
            };
            MockRecordsAsync<Student.DataContracts.RegDefaults>("ST.PARMS", new List<Student.DataContracts.RegDefaults>() { regDefaults });
        }
    }
}
