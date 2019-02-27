// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonHoldTypesTypesController_GetAllTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        private PersonHoldTypesController personHoldTypesController;
        private Mock<IPersonHoldTypeService> personHoldTypeServiceMock;
        private IPersonHoldTypeService personHoldTypeService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Base.Entities.Restriction> allRestrictionTypesEntities;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            personHoldTypeServiceMock = new Mock<IPersonHoldTypeService>();
            personHoldTypeService = personHoldTypeServiceMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allRestrictionTypesEntities = new TestRestrictionRepository().Get() as List<Ellucian.Colleague.Domain.Base.Entities.Restriction>;
            var personHoldTypeList = new List<PersonHoldType>();

            personHoldTypesController = new PersonHoldTypesController(AdapterRegistry, personHoldTypeService, logger);
            personHoldTypesController.Request = new HttpRequestMessage();
            personHoldTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var restrictionType in allRestrictionTypesEntities)
            {
                PersonHoldType target = ConvertRestrictionEntitytoPersonHoldTypeDto(restrictionType);
                personHoldTypeList.Add(target);
            }

            personHoldTypeServiceMock.Setup<Task<IEnumerable<PersonHoldType>>>(s => s.GetPersonHoldTypesAsync(It.IsAny<bool>())).ReturnsAsync(personHoldTypeList);
        }


        [TestCleanup]
        public void Cleanup()
        {
            personHoldTypesController = null;
            personHoldTypeService = null;
        }

        [TestMethod]
        public async Task ReturnsAllPersonHoldTypes()
        {
            List<PersonHoldType> personHoldTypes = await personHoldTypesController.GetPersonHoldTypesAsync() as List<PersonHoldType>;
            Assert.AreEqual(personHoldTypes.Count, allRestrictionTypesEntities.Count);
        }

        [TestMethod]
        public async Task GetPersonHoldTypes_Properties()
        {
            List<PersonHoldType> personHoldTypes = await personHoldTypesController.GetPersonHoldTypesAsync() as List<PersonHoldType>;
            PersonHoldType al = personHoldTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction alt = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(alt.Code, al.Code);
            Assert.AreEqual(alt.Description, al.Title);
        }

        [TestMethod]
        public async Task RestrictionTypesController_GetHedmAsync_CacheControlNotNull()
        {
            personHoldTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            List<PersonHoldType> RestrictionTypes = await personHoldTypesController.GetPersonHoldTypesAsync() as List<PersonHoldType>;
            PersonHoldType ht = RestrictionTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction ret = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(ret.Code, ht.Code);
            Assert.AreEqual(ret.Description, ht.Title);
            Assert.AreEqual((int)ret.RestIntgCategory, (int)ht.Category);
        }

        [TestMethod]
        public async Task RestrictionTypesController_GetHedmAsync_NoCache()
        {
            personHoldTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            personHoldTypesController.Request.Headers.CacheControl.NoCache = true;

            List<PersonHoldType> personHoldTypes = await personHoldTypesController.GetPersonHoldTypesAsync() as List<PersonHoldType>;
            PersonHoldType ht = personHoldTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction ret = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(ret.Code, ht.Code);
            Assert.AreEqual(ret.Description, ht.Title);
            Assert.AreEqual((int)ret.RestIntgCategory, (int)ht.Category);
        }

        [TestMethod]
        public async Task RestrictionTypesController_GetHedmAsync_Cache()
        {
            personHoldTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            personHoldTypesController.Request.Headers.CacheControl.NoCache = false;

            List<PersonHoldType> RestrictionTypes = await personHoldTypesController.GetPersonHoldTypesAsync() as List<PersonHoldType>;
            PersonHoldType ht = RestrictionTypes.Where(a => a.Code == "ACC30").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Restriction ret = allRestrictionTypesEntities.Where(a => a.Code == "ACC30").FirstOrDefault();
            Assert.AreEqual(ret.Code, ht.Code);
            Assert.AreEqual(ret.Description, ht.Title);
            Assert.AreEqual((int)ret.RestIntgCategory, (int)ht.Category);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RestrictionTypeController_GetThrowsIntAppiExc()
        {
            personHoldTypeServiceMock.Setup(gc => gc.GetPersonHoldTypesAsync(It.IsAny<bool>())).Throws<Exception>();

            await personHoldTypesController.GetPersonHoldTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RestrictionTypeController_GetByIdThrowsIntAppiExc()
        {
            personHoldTypeServiceMock.Setup(gc => gc.GetPersonHoldTypeByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await personHoldTypesController.GetPersonHoldTypeByIdAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PerRestTypeController_DeleteThrowsIntApiExc()
        {
            await personHoldTypesController.DeletePersonHoldTypesAsync("ACC30");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PerRestTypeController_PostThrowsIntAppiExc()
        {
            PersonHoldType htDTO = await personHoldTypesController.PostPersonHoldTypesAsync(new PersonHoldType());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PerRestTypeController_PutThrowsIntAppiExc()
        {
            PersonHoldType rtDTO = await personHoldTypesController.PutPersonHoldTypesAsync(new PersonHoldType());
        }

        private PersonHoldType ConvertRestrictionEntitytoPersonHoldTypeDto(Domain.Base.Entities.Restriction source)
        {
            var personHoldType = new Dtos.PersonHoldType();
            personHoldType.Id = source.Guid;
            personHoldType.Code = source.Code;
            personHoldType.Title = source.Description;
            personHoldType.Description = null;
            personHoldType.Category = ConvertCategoryEntityEnumToDtoEnum(source.RestIntgCategory);
            return personHoldType;
        }

        private PersonHoldCategoryTypes ConvertCategoryEntityEnumToDtoEnum(Domain.Base.Entities.RestrictionCategoryType restrictionCategoryType)
        {
            switch (restrictionCategoryType)
            {
                case RestrictionCategoryType.Academic:
                    return PersonHoldCategoryTypes.Academic;
                case RestrictionCategoryType.Administrative:
                    return PersonHoldCategoryTypes.Administrative;
                case RestrictionCategoryType.Disciplinary:
                    return PersonHoldCategoryTypes.Disciplinary;
                case RestrictionCategoryType.Financial:
                    return PersonHoldCategoryTypes.Financial;
                case RestrictionCategoryType.Health:
                    return PersonHoldCategoryTypes.Health;
                default:
                    return PersonHoldCategoryTypes.Academic;
            }
        }
    }
}
