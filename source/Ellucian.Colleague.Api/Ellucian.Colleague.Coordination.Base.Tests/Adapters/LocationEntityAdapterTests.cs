// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class LocationEntityAdapterTests
    {
        string locGuid;
        string locCode;
        string locDesc;
        decimal? locLat1;
        decimal? locLong1;
        decimal? locLat2;
        decimal? locLong2;
        string locMobile;
        List<string> locBldgs;

        Location LocationDto;
        Ellucian.Colleague.Domain.Base.Entities.Location LocationEntity;
        LocationEntityAdapter LocationEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            locGuid = "03ca846d-1c28-4366-8ca1-711a6bc4ddd0";
            locCode = "EAR";
            locDesc = "Amelia Earhart Apartments";
            locLat1 = 38.723921m;
            locLong1 = -77.487772m;
            locLat2 = 37.723921m;
            locLong2 = -78.487772m;
            locMobile = "Y";
            locBldgs = new List<string>() { "EAR", "LBP", "DER" };

            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            LocationEntityAdapter = new LocationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var coordinateEntityAdapter = new AutoMapperAdapter<Domain.Base.Entities.Coordinate, Coordinate>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Coordinate, Coordinate>()).Returns(coordinateEntityAdapter);

            LocationEntity = new Domain.Base.Entities.Location(locGuid, locCode, locDesc, locLat1, locLong1, locLat2, 
                locLong2, locMobile, locBldgs);

            LocationDto = LocationEntityAdapter.MapToType(LocationEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void LocationEntityAdapterTests_BuildingCodes()
        {
            CollectionAssert.AreEqual(locBldgs, LocationDto.BuildingCodes);
        }

        [TestMethod]
        public void LocationEntityAdapterTests_Code()
        {
            Assert.AreEqual(locCode, LocationDto.Code);
        }

        [TestMethod]
        public void LocationEntityAdapterTests_Description()
        {
            Assert.AreEqual(locDesc, LocationDto.Description);
        }

        [TestMethod]
        public void LocationEntityAdapterTests_NorthwestCoordinate()
        {
            Assert.AreEqual(locLat1, LocationDto.NorthWestCoordinate.Latitude);
            Assert.AreEqual(locLong1, LocationDto.NorthWestCoordinate.Longitude);
        }

        [TestMethod]
        public void LocationEntityAdapterTests_SouthEastCoordinate()
        {
            Assert.AreEqual(locLat2, LocationDto.SouthEastCoordinate.Latitude);
            Assert.AreEqual(locLong2, LocationDto.SouthEastCoordinate.Longitude);
        }
    }
}