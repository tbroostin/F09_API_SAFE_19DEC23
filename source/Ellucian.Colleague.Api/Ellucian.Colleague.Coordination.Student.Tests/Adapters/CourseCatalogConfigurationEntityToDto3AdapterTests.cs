// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class CourseCatalogConfigurationEntityToDto3AdapterTests
    {
        [TestMethod]
        public void CourseCatalogConfigurationEntityToDto3Adapter_MapToType()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();
            var catalogConfigAdapter = new CourseCatalogConfigurationEntityToDto3Adapter(adapterRegistry, loggerMock.Object);

            var catalogConfig = new Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration(DateTime.Now.AddDays(-10), DateTime.Now);
            catalogConfig.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.Availability, true);
            catalogConfig.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.Locations, false);

            catalogConfig.AddCatalogSearchResultHeaderOption(Ellucian.Colleague.Domain.Student.Entities.CatalogSearchResultHeaderType.AcademicLevel, true);
            catalogConfig.AddCatalogSearchResultHeaderOption(Ellucian.Colleague.Domain.Student.Entities.CatalogSearchResultHeaderType.Location, false);

            var catalogConfigDto = catalogConfigAdapter.MapToType(catalogConfig);

            Assert.AreEqual(catalogConfig.EarliestSearchDate, catalogConfigDto.EarliestSearchDate);
            Assert.AreEqual(catalogConfig.CatalogFilterOptions.Count(), catalogConfigDto.CatalogFilterOptions.Count());
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[0].IsHidden, catalogConfigDto.CatalogFilterOptions[0].IsHidden);
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[0].Type.ToString(), catalogConfigDto.CatalogFilterOptions[0].Type.ToString());
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[1].IsHidden, catalogConfigDto.CatalogFilterOptions[1].IsHidden);
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[1].Type.ToString(), catalogConfigDto.CatalogFilterOptions[1].Type.ToString());

            Assert.AreEqual(catalogConfig.CatalogSearchResultHeaderOptions.Count(), catalogConfigDto.CatalogSearchResultHeaderOptions.Count());
            Assert.AreEqual(catalogConfig.CatalogSearchResultHeaderOptions[0].IsHidden, catalogConfigDto.CatalogSearchResultHeaderOptions[0].IsHidden);
            Assert.AreEqual(catalogConfig.CatalogSearchResultHeaderOptions[0].Type.ToString(), catalogConfigDto.CatalogSearchResultHeaderOptions[0].Type.ToString());
            Assert.AreEqual(catalogConfig.CatalogSearchResultHeaderOptions[1].IsHidden, catalogConfigDto.CatalogSearchResultHeaderOptions[1].IsHidden);
            Assert.AreEqual(catalogConfig.CatalogSearchResultHeaderOptions[1].Type.ToString(), catalogConfigDto.CatalogSearchResultHeaderOptions[1].Type.ToString());
        }
    }
}
