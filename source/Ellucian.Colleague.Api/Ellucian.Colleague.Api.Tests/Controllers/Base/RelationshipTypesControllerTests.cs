// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class RelationshipTypesControllerTests
    {
        //[TestClass]
        //public class RelationshipTypesControllerGet
        //{
        //    #region Test Context

        //    private TestContext testContextInstance;

        //    /// <summary>
        //    ///Gets or sets the test context which provides
        //    ///information about and functionality for the current test run.
        //    ///</summary>
        //    public TestContext TestContext
        //    {
        //        get
        //        {
        //            return testContextInstance;
        //        }
        //        set
        //        {
        //            testContextInstance = value;
        //        }
        //    }

        //    #endregion

        //    private RelationshipTypesController relationshipTypesController;

        //    private Mock<IReferenceDataRepository> refDataRepoMock;
        //    private IReferenceDataRepository refDataRepo;

        //    private IAdapterRegistry AdapterRegistry;

        //    private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RelationshipType> allRelTypes;

        //    ILogger logger = new Mock<ILogger>().Object;

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
        //        EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

        //        refDataRepoMock = new Mock<IReferenceDataRepository>();
        //        refDataRepo = refDataRepoMock.Object;

        //        HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
        //        AdapterRegistry = new AdapterRegistry(adapters, logger);
        //        var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>(AdapterRegistry, logger);
        //        AdapterRegistry.AddAdapter(testAdapter);

        //        allRelTypes = new TestRelationshipTypeRepository().Get();
        //        var relTypeList = new List<RelationshipType>();

        //        relationshipTypesController = new RelationshipTypesController(AdapterRegistry, refDataRepo);
        //        Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>();
        //        foreach (var RelationshipType in allRelTypes)
        //        {
        //            RelationshipType target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>(RelationshipType);
        //            relTypeList.Add(target);
        //        }
        //        refDataRepoMock.Setup(x => x.GetRelationshipTypesAsync()).ReturnsAsync(allRelTypes);
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        relationshipTypesController = null;
        //        refDataRepo = null;
        //    }


        //    [TestMethod]
        //    public async Task ReturnsAllRelationshipTypesAsync()
        //    {
        //        var relationshipTypes = await relationshipTypesController.GetAsync();
        //        Assert.AreEqual(relationshipTypes.Count(), allRelTypes.Count());
        //    }

        //}
    }
}