// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PlaceTests
    {
        private string placesCountry { get; set; }
        private string placesRegion { get; set; }
        private string placesSubRegion { get; set; }
        private string placesDesc { get; set; }

        private Place place;

        [TestInitialize]
        public void Initialize()
        {
            placesCountry = "AUS";
            placesRegion = "ACT";
            placesSubRegion = "South";
            placesDesc = "Victoria";
        }

        [TestMethod]
        public void PlaceConstructorEmptyGuid()
        {
            place = new Place();
        }

        [TestMethod]
        public void PlacesCountry()
        {
            place = new Place();
            place.PlacesCountry = placesCountry;
            Assert.AreEqual(placesCountry, place.PlacesCountry);
        }

        [TestMethod]
        public void PlacesRegion()
        {
            place = new Place();
            place.PlacesRegion = placesRegion;
            Assert.AreEqual(placesRegion, place.PlacesRegion);
        }
        
        [TestMethod]
        public void PlacesSubRegion()
        {
            place = new Place();
            place.PlacesSubRegion = placesSubRegion;
            Assert.AreEqual(placesSubRegion, place.PlacesSubRegion);
        }

        [TestMethod]
        public void PlacesDesc()
        {
            place = new Place();
            place.PlacesDesc = placesDesc;
            Assert.AreEqual(placesDesc, place.PlacesDesc);
        }
    }
}