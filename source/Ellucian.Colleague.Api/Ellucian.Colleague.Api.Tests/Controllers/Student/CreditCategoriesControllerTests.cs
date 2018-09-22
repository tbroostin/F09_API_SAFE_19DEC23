// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CreditCategoriesControllerTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<ICurriculumService> curriculumServiceMock;
        private ICurriculumService curriculumService;
        private ILogger logger = new Mock<ILogger>().Object;

        private CreditCategoriesController creditCategoriesController;
        List<CreditCategory> allCreditCategories = new List<CreditCategory>();
        private List<Dtos.CreditCategory2> allCreditCategoryDtos = new List<Dtos.CreditCategory2>();
        private List<Dtos.CreditCategory3> allCreditCategory3Dtos = new List<Dtos.CreditCategory3>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            curriculumServiceMock = new Mock<ICurriculumService>();
            curriculumService = curriculumServiceMock.Object;

            allCreditCategories.Add(new CreditCategory("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education", CreditType.ContinuingEducation));
            allCreditCategories.Add(new CreditCategory("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional", CreditType.Institutional));
            allCreditCategories.Add(new CreditCategory("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer", CreditType.Transfer));

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.CreditCategory, Dtos.CreditCategory2>();
            foreach (var creditCategory in allCreditCategories)
            {
                Dtos.CreditCategory2 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.CreditCategory, Dtos.CreditCategory2>(creditCategory);
                allCreditCategoryDtos.Add(target);
            }

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.CreditCategory, Dtos.CreditCategory3>();
            foreach (var creditCategory in allCreditCategories)
            {
                Dtos.CreditCategory3 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.CreditCategory, Dtos.CreditCategory3>(creditCategory);
                target.Id = creditCategory.Guid;
                allCreditCategory3Dtos.Add(target);
            }

            creditCategoriesController = new CreditCategoriesController(curriculumService, logger);
            creditCategoriesController.Request = new HttpRequestMessage();
            creditCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            creditCategoriesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task CreditCategoriessController_GetHedmAsync_V6_CacheControlNotNull()
        {
            creditCategoriesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            curriculumServiceMock.Setup(x => x.GetCreditCategories3Async(It.IsAny<bool>())).ReturnsAsync(allCreditCategory3Dtos);

            List<Dtos.CreditCategory3> CreditCategories = await creditCategoriesController.GetCreditCategories3Async() as List<Dtos.CreditCategory3>;
            Dtos.CreditCategory3 cc = CreditCategories.Where(a => a.Code == "CE").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.CreditCategory cct = allCreditCategories.Where(a => a.Code == "CE").FirstOrDefault();
            Assert.AreEqual(cct.Code, cc.Code);
            Assert.AreEqual(cct.Description, cc.Description);
        }

        [TestMethod]
        public async Task CreditCategoriesController_GetHedmAsync_V6_NoCache()
        {
            creditCategoriesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            creditCategoriesController.Request.Headers.CacheControl.NoCache = true;
            curriculumServiceMock.Setup(x => x.GetCreditCategories3Async(It.IsAny<bool>())).ReturnsAsync(allCreditCategory3Dtos);

            List<Dtos.CreditCategory3> CreditCategories = await creditCategoriesController.GetCreditCategories3Async() as List<Dtos.CreditCategory3>;
            Dtos.CreditCategory3 cc = CreditCategories.Where(a => a.Code == "CE").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.CreditCategory cct = allCreditCategories.Where(a => a.Code == "CE").FirstOrDefault();
            Assert.AreEqual(cct.Code, cc.Code);
            Assert.AreEqual(cct.Description, cc.Description);
        }

        [TestMethod]
        public async Task CreditCategoriesController_GetHedmAsync_V6_Cache()
        {
            creditCategoriesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            creditCategoriesController.Request.Headers.CacheControl.NoCache = false;
            curriculumServiceMock.Setup(x => x.GetCreditCategories3Async(It.IsAny<bool>())).ReturnsAsync(allCreditCategory3Dtos);

            List<Dtos.CreditCategory3> CreditCategories = await creditCategoriesController.GetCreditCategories3Async() as List<Dtos.CreditCategory3>;
            Dtos.CreditCategory3 cc = CreditCategories.Where(a => a.Code == "CE").FirstOrDefault();
            Ellucian.Colleague.Domain.Student.Entities.CreditCategory cct = allCreditCategories.Where(a => a.Code == "CE").FirstOrDefault();
            Assert.AreEqual(cct.Code, cc.Code);
            Assert.AreEqual(cct.Description, cc.Description);
        }

        [TestMethod]
        public async Task CreditCategoriesController_GetHedmByIdAsync_V6_Cache()
        {
            string id = "b5cc288b-8692-474e-91be-bdc55778e2f5";
            var expected = allCreditCategory3Dtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            curriculumServiceMock.Setup(x => x.GetCreditCategoryByGuid3Async(id)).ReturnsAsync(expected);

            var result = await creditCategoriesController.GetCreditCategoryByGuid3Async(id);
            Assert.IsNotNull(result);

            Assert.AreEqual(expected.Id, result.Id);
            Assert.AreEqual(expected.Code, result.Code);
            Assert.AreEqual(expected.CreditType.Value, result.CreditType.Value);
            Assert.AreEqual(expected.Description, result.Description);
            Assert.AreEqual(expected.Title, result.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_Get_V6_ThrowsIntAppiExc()
        {
            curriculumServiceMock.Setup(gc => gc.GetCreditCategories3Async(It.IsAny<bool>())).Throws<Exception>();

            await creditCategoriesController.GetCreditCategories3Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_GetById_V6_ThrowsIntAppiExc()
        {
            curriculumServiceMock.Setup(gc => gc.GetCreditCategoryByGuid3Async(It.IsAny<string>())).Throws<Exception>();

            await creditCategoriesController.GetCreditCategoryByGuid3Async("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_GetById_V6_ArgumentNullException()
        {
            curriculumServiceMock.Setup(gc => gc.GetCreditCategoryByGuid3Async(It.IsAny<string>())).Throws<ArgumentNullException>();

            await creditCategoriesController.GetCreditCategoryByGuid3Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_PostCreditCategoryAsync()
        {
            var response = await creditCategoriesController.PostCreditCategoryAsync(allCreditCategoryDtos.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_PostCreditCategoryV6Async()
        {
            var response = await creditCategoriesController.PostCreditCategoryV6Async(allCreditCategory3Dtos.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_PutCreditCategoryAsync()
        {
            var creditCategory = allCreditCategoryDtos.FirstOrDefault();
            var response = await creditCategoriesController.PutCreditCategoryAsync(creditCategory.Id, creditCategory);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_PutCreditCategoryV6Async()
        {
            var creditCategory = allCreditCategory3Dtos.FirstOrDefault();
            var response = await creditCategoriesController.PutCreditCategoryV6Async(creditCategory.Id, creditCategory);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_DeleteCreditCategoryAsync()
        {
            var response = await creditCategoriesController.DeleteCreditCategoryAsync(allCreditCategoryDtos.FirstOrDefault().Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreditCategoriesController_DeleteCreditCategoryV6Async()
        {
            var response = await creditCategoriesController.DeleteCreditCategoryV6Async(allCreditCategory3Dtos.FirstOrDefault().Id);
        }
    }
}