// Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RegistrationDate = Ellucian.Colleague.Domain.Student.Entities.RegistrationDate;
using Section = Ellucian.Colleague.Domain.Student.Entities.Section;
using Term = Ellucian.Colleague.Domain.Student.Entities.Term;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentConfigurationServiceTests
    {

        [TestClass]
        public class GetCourseCatalogConfiguration4Async 
        {
            private StudentConfigurationService studentCoordinationService;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentConfigurationRepository> studentConfigRepoMock;
            private IStudentConfigurationRepository studentConfigRepo;
            private ILogger logger;
            private Domain.Student.Entities.CourseCatalogConfiguration catalogConfig;

            [TestInitialize]
            public void Initialize()
            {
                studentConfigRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigRepo = studentConfigRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                var catalogConfigFilterOption = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CatalogFilterOption, CatalogFilterOption3>(adapterRegistry, logger);
                var catalogConfigHeaderOption = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CatalogSearchResultHeaderOption, CatalogSearchResultHeaderOption2>(adapterRegistry, logger);
                var catalogConfigSearchView = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SelfServiceCourseCatalogSearchView, Dtos.Student.SelfServiceCourseCatalogSearchView>(adapterRegistry, logger);
                var catalogConfigSearchResultView = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SelfServiceCourseCatalogSearchResultView, Dtos.Student.SelfServiceCourseCatalogSearchResultView>(adapterRegistry, logger);

                // Mock the adapter registry to return the midterm grading complete entity to dto adapter
                var catalogConfigAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, CourseCatalogConfiguration4>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x =>
                    x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, CourseCatalogConfiguration4>()).Returns(catalogConfigAdapter);




                catalogConfig = new Domain.Student.Entities.CourseCatalogConfiguration(DateTime.Today, null);
                catalogConfig.AddCatalogFilterOption(Domain.Student.Entities.CatalogFilterType.AcademicLevels, false);
                catalogConfig.AddCatalogFilterOption(Domain.Student.Entities.CatalogFilterType.CourseLevels, true);
                catalogConfig.AddCatalogFilterOption(Domain.Student.Entities.CatalogFilterType.CourseTypes, false);

                catalogConfig.AddCatalogSearchResultHeaderOption(Domain.Student.Entities.CatalogSearchResultHeaderType.AcademicLevel, false);
                catalogConfig.AddCatalogSearchResultHeaderOption(Domain.Student.Entities.CatalogSearchResultHeaderType.Location, true);
                catalogConfig.AddCatalogSearchResultHeaderOption(Domain.Student.Entities.CatalogSearchResultHeaderType.PlannedStatus, false);
                catalogConfig.AddCatalogSearchResultHeaderOption(Domain.Student.Entities.CatalogSearchResultHeaderType.InstructionalMethods, true);
                catalogConfig.AddCatalogSearchResultHeaderOption(Domain.Student.Entities.CatalogSearchResultHeaderType.CourseTypes, false);
                catalogConfig.AddCatalogSearchResultHeaderOption(Domain.Student.Entities.CatalogSearchResultHeaderType.Comments, true);
                catalogConfig.AddCatalogSearchResultHeaderOption(Domain.Student.Entities.CatalogSearchResultHeaderType.BookstoreLink, false);
                logger = new Mock<ILogger>().Object;
                studentConfigRepoMock.Setup(repo => repo.GetCourseCatalogConfiguration4Async()).ReturnsAsync(catalogConfig);

                // Mock the section service
                studentCoordinationService = new StudentConfigurationService(studentConfigRepo, adapterRegistry, logger);
            }

            [TestMethod]
            public async Task GetCourseCatalogConfiguration4Async_ValidateFiltersMapping()
            {
                Dtos.Student.CourseCatalogConfiguration4 config = await studentCoordinationService.GetCourseCatalogConfiguration4Async();
                Assert.IsNotNull(config);
                Assert.AreEqual(7,config.CatalogSearchResultHeaderOptions.Count);
                Assert.AreEqual(CatalogSearchResultHeaderType2.AcademicLevel, config.CatalogSearchResultHeaderOptions[0].Type);
                Assert.IsFalse( config.CatalogSearchResultHeaderOptions[0].IsHidden);
                Assert.AreEqual(CatalogSearchResultHeaderType2.Location, config.CatalogSearchResultHeaderOptions[1].Type);
                Assert.IsTrue(config.CatalogSearchResultHeaderOptions[1].IsHidden);
                Assert.AreEqual(CatalogSearchResultHeaderType2.PlannedStatus, config.CatalogSearchResultHeaderOptions[2].Type);
                Assert.IsFalse(config.CatalogSearchResultHeaderOptions[2].IsHidden);
                Assert.AreEqual(CatalogSearchResultHeaderType2.InstructionalMethods, config.CatalogSearchResultHeaderOptions[3].Type);
                Assert.IsTrue(config.CatalogSearchResultHeaderOptions[3].IsHidden);
                Assert.AreEqual(CatalogSearchResultHeaderType2.CourseTypes, config.CatalogSearchResultHeaderOptions[4].Type);
                Assert.IsFalse(config.CatalogSearchResultHeaderOptions[4].IsHidden);
                Assert.AreEqual(CatalogSearchResultHeaderType2.Comments, config.CatalogSearchResultHeaderOptions[5].Type);
                Assert.IsTrue(config.CatalogSearchResultHeaderOptions[5].IsHidden);
                Assert.AreEqual(CatalogSearchResultHeaderType2.BookstoreLink, config.CatalogSearchResultHeaderOptions[6].Type);
                Assert.IsFalse(config.CatalogSearchResultHeaderOptions[6].IsHidden);

                Assert.AreEqual(3, config.CatalogFilterOptions.Count);
                Assert.AreEqual(CatalogFilterType3.AcademicLevels, config.CatalogFilterOptions[0].Type);
                Assert.IsFalse(config.CatalogSearchResultHeaderOptions[0].IsHidden);
                Assert.AreEqual(CatalogFilterType3.CourseLevels, config.CatalogFilterOptions[1].Type);
                Assert.IsTrue(config.CatalogSearchResultHeaderOptions[1].IsHidden);
                Assert.AreEqual(CatalogFilterType3.CourseTypes, config.CatalogFilterOptions[2].Type);
                Assert.IsFalse(config.CatalogSearchResultHeaderOptions[2].IsHidden);
            }
        }
    }
}
