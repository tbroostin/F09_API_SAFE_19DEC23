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
    public class BuildingEntityAdapterTests
    {
        string bldgGuid;
        string bldgCode;
        string bldgDesc;
        string bldgLoc;
        string bldgType;
        string bldgLongDesc;
        List<string> bldgAddress;
        string bldgCity;
        string bldgState;
        string bldgPostal;
        string bldgCountry;
        decimal? bldgLat;
        decimal? bldgLong;
        string bldgImage;
        string bldgSvcs;
        string bldgMobile;
        
        Building buildingDto;
        Ellucian.Colleague.Domain.Base.Entities.Building buildingEntity;
        BuildingEntityAdapter buildingEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            bldgGuid = "03ca846d-1c28-4366-8ca1-711a6bc4ddd0";
            bldgCode = "EAR";
            bldgDesc = "Amelia Earhart Apartments";
            bldgLoc = "MC";
            bldgType = "LEC";
            bldgLongDesc = "Long description of Amelia Earhart Apartments";
            bldgCity = "Fairfax";
            bldgState = "VA";
            bldgPostal = "22033";
            bldgCountry = "USA";
            bldgAddress = new List<string>() { "123 Main St", bldgCity, bldgState, bldgPostal, bldgCountry };
            bldgLat = 38.723921m;
            bldgLong = -77.487772m;
            bldgImage = "http://cupegraf.com/data_images/wallpapers/12/332051-apartment.jpg";
            bldgSvcs = "Apartment complex";
            bldgMobile = "Y";

            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            buildingEntityAdapter = new BuildingEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var coordinateEntityAdapter = new AutoMapperAdapter<Domain.Base.Entities.Coordinate, Coordinate>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Coordinate, Coordinate>()).Returns(coordinateEntityAdapter);

            buildingEntity = new Domain.Base.Entities.Building(bldgGuid, bldgCode, bldgDesc, bldgLoc, bldgType,
                bldgLongDesc, bldgAddress, bldgCity, bldgState, bldgPostal, bldgCountry, bldgLat, bldgLong, bldgImage, bldgSvcs, bldgMobile);

            buildingDto = buildingEntityAdapter.MapToType(buildingEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void BuildingEntityAdapterTests_AdditionalServices()
        {
            Assert.AreEqual(bldgSvcs, buildingDto.AdditionalServices);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_AddressLines()
        {
            CollectionAssert.AreEqual(bldgAddress, buildingDto.AddressLines);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_BuildingCoordinate()
        {
            Assert.AreEqual(bldgLat, buildingDto.BuildingCoordinate.Latitude);
            Assert.AreEqual(bldgLong, buildingDto.BuildingCoordinate.Longitude);
            Assert.AreEqual(bldgMobile != "Y", buildingDto.BuildingCoordinate.OfficeUseOnly);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_BuildingType()
        {
            Assert.AreEqual(bldgType, buildingDto.BuildingType);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_City()
        {
            Assert.AreEqual(bldgCity, buildingDto.City);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_BuildingCode()
        {
            Assert.AreEqual(bldgCode, buildingDto.Code);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_Country()
        {
            Assert.AreEqual(bldgCountry, buildingDto.Country);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_Description()
        {
            Assert.AreEqual(bldgDesc, buildingDto.Description);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_ImageUrl()
        {
            Assert.AreEqual(bldgImage, buildingDto.ImageUrl);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_LocationId()
        {
            Assert.AreEqual(bldgLoc, buildingDto.LocationId);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_LongDescription()
        {
            Assert.AreEqual(bldgLongDesc, buildingDto.LongDescription);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_PostalCode()
        {
            Assert.AreEqual(bldgPostal, buildingDto.PostalCode);
        }

        [TestMethod]
        public void BuildingEntityAdapterTests_State()
        {
            Assert.AreEqual(bldgState, buildingDto.State);
        }
   }
}