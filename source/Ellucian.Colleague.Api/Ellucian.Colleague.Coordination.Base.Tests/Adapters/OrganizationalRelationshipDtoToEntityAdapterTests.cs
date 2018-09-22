using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using slf4net;
using Moq;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    /// <summary>
    /// Summary description for OrganizationalRelationshipDtoToEntityAdapterTests
    /// </summary>
    [TestClass]
    public class OrganizationalRelationshipDtoToEntityAdapterTests
    {

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _logger = new Mock<ILogger>().Object;
        }      

        [TestMethod]
        public void OrganizationalRelationshipDtoToEntityAdapter_MapToType_MapsProperties()
        {
            // Test that all properties are adapted to Entity
            var dtoAdapter = new OrganizationalRelationshipDtoToEntityAdapter(_adapterRegistry, _logger);

            // Give dto
            var expectedDto = new OrganizationalRelationship
            {
                Id = "RR1",
                Category = "MGR",
                OrganizationalPersonPositionId = "RS1",
                RelatedOrganizationalPersonPositionId = "RS2",
                RelatedPersonId = "1",
                RelatedPersonName = "Walter White",
                RelatedPositionId = "P1",
                RelatedPositionTitle = "Chemistry Teacher"
            };

            var actualEntity = dtoAdapter.MapToType(expectedDto);

            Assert.AreEqual(expectedDto.Id, actualEntity.Id);
            Assert.AreEqual(expectedDto.OrganizationalPersonPositionId, actualEntity.OrganizationalPersonPositionId);
            Assert.AreEqual(expectedDto.RelatedOrganizationalPersonPositionId, actualEntity.RelatedOrganizationalPersonPositionId);
            Assert.AreEqual(expectedDto.Category, actualEntity.Category);
        }
    }
}
