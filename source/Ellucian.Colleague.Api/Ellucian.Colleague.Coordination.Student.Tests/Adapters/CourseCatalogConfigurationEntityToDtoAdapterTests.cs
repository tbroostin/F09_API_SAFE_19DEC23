// Copyright 2017 Ellucian Company L.P. and its affiliates.
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
    public class CourseCatalogConfigurationEntityToDtoAdapterTests
    {
        [TestMethod]
        public void CourseCatalogConfigurationEntityToDtoAdapter_MapToType()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();
            var catalogConfigAdapter = new CourseCatalogConfigurationEntityToDtoAdapter(adapterRegistry, loggerMock.Object);

            var catalogConfig = new Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration(DateTime.Now.AddDays(-10), DateTime.Now);
            catalogConfig.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.Availability, true);
            catalogConfig.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.Locations, false);

            var catalogConfigDto = catalogConfigAdapter.MapToType(catalogConfig);

            Assert.AreEqual(catalogConfig.EarliestSearchDate, catalogConfigDto.EarliestSearchDate);
            Assert.AreEqual(catalogConfig.CatalogFilterOptions.Count(), catalogConfigDto.CatalogFilterOptions.Count());
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[0].IsHidden, catalogConfigDto.CatalogFilterOptions[0].IsHidden);
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[0].Type.ToString(), catalogConfigDto.CatalogFilterOptions[0].Type.ToString());
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[1].IsHidden, catalogConfigDto.CatalogFilterOptions[1].IsHidden);
            Assert.AreEqual(catalogConfig.CatalogFilterOptions[1].Type.ToString(), catalogConfigDto.CatalogFilterOptions[1].Type.ToString());
        }
    }
}
