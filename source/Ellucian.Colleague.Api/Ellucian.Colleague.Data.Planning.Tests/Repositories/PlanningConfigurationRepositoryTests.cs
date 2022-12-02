// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Planning.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Planning.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.Planning.Tests.Repositories
{
    [TestClass]
    public class PlanningConfigurationRepositoryTests : BaseRepositorySetup
    {
        PlanningConfigurationRepository planningConfigurationRepo;
        StwebDefaults stWebDefaults;
        StwebDefaults2 stWebDefaults2;

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();

            stWebDefaults = new StwebDefaults()
            {
                StwebDefaultCtk = "DEFAULT",
                StwebShowAdviseComplete = "Y",
                StwebCatalogYearPolicy = "1",
                StwebAdviseByOfficeFlag = "Y"
            };
            stWebDefaults2 = new StwebDefaults2()
            {
                Stweb2AllowAdvByAssnmnt = "Y"
            };


            dataReaderMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", It.IsAny<bool>())).Returns(Task.FromResult<StwebDefaults>(stWebDefaults));
            dataReaderMock.Setup<Task<StwebDefaults2>>(acc => acc.ReadRecordAsync<StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2", It.IsAny<bool>())).Returns(Task.FromResult<StwebDefaults2>(stWebDefaults2));
            planningConfigurationRepo = new PlanningConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            planningConfigurationRepo = null;
            cacheProviderMock = null;
            dataReaderMock = null;
            transFactoryMock = null;
        }

        [TestClass]
        public class GetPlanningConfigurationAsync_Tests : PlanningConfigurationRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task GetPlanningConfigurationAsync_DataReader_Returns_Null()
            {
                dataReaderMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", It.IsAny<bool>())).Returns(Task.FromResult<StwebDefaults>(null));
                planningConfigurationRepo = new PlanningConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                var config = await planningConfigurationRepo.GetPlanningConfigurationAsync();
            }

            [TestMethod]
            public async Task GetPlanningConfigurationAsync_DefaultCatalogPolicy_StudentCatalogYear()
            {
                var config = await planningConfigurationRepo.GetPlanningConfigurationAsync();
                Assert.AreEqual(stWebDefaults.StwebDefaultCtk, config.DefaultCurriculumTrack);
                Assert.IsTrue(config.ShowAdvisementCompleteWorkflow);
                Assert.AreEqual(CatalogPolicy.StudentCatalogYear, config.DefaultCatalogPolicy);
            }

            [TestMethod]
            public async Task GetPlanningConfigurationAsync_DefaultCatalogPolicy_CurrentCatalogYear()
            {
                stWebDefaults.StwebCatalogYearPolicy = "2";
                var config = await planningConfigurationRepo.GetPlanningConfigurationAsync();
                Assert.AreEqual(stWebDefaults.StwebDefaultCtk, config.DefaultCurriculumTrack);
                Assert.IsTrue(config.ShowAdvisementCompleteWorkflow);
                Assert.AreEqual(CatalogPolicy.CurrentCatalogYear, config.DefaultCatalogPolicy);
            }

            [TestMethod]
            public async Task GetPlanningConfigurationAsync_ShowAdvisementCompleteWorkflow_False_when_StwebShowAdviseComplete_is_Null()
            {
                stWebDefaults.StwebShowAdviseComplete = null;
                var config = await planningConfigurationRepo.GetPlanningConfigurationAsync();
                Assert.AreEqual(stWebDefaults.StwebDefaultCtk, config.DefaultCurriculumTrack);
                Assert.IsFalse(config.ShowAdvisementCompleteWorkflow);
                Assert.AreEqual(CatalogPolicy.StudentCatalogYear, config.DefaultCatalogPolicy);
            }

            [TestMethod]
            public async Task GetPlanningConfigurationAsync_ShowAdvisementCompleteWorkflow_False_when_StwebShowAdviseComplete_is_not_Yes()
            {
                stWebDefaults.StwebShowAdviseComplete = "N";
                var config = await planningConfigurationRepo.GetPlanningConfigurationAsync();
                Assert.AreEqual(stWebDefaults.StwebDefaultCtk, config.DefaultCurriculumTrack);
                Assert.IsFalse(config.ShowAdvisementCompleteWorkflow);
                Assert.AreEqual(CatalogPolicy.StudentCatalogYear, config.DefaultCatalogPolicy);
            }

            [TestMethod]
            public async Task GetPlanningConfigurationAsync_AdvisngByOffice_AdvisingByAssignment()
            {
                stWebDefaults.StwebAdviseByOfficeFlag = "Y";
                stWebDefaults2.Stweb2AllowAdvByAssnmnt = "Y";
                var config = await planningConfigurationRepo.GetPlanningConfigurationAsync();
                Assert.IsTrue(config.AdviseByOfficeFlag);              
                Assert.IsTrue( config.AdviseByAssignmentFlag);
            }
        }
    }
}
