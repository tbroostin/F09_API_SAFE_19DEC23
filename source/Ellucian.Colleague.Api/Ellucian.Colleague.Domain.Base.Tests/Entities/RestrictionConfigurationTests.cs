// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RestrictionConfigurationTests
    {
        private SeverityStyleMapping mapping1 = new SeverityStyleMapping(0, 100, AlertStyle.Critical);
        private SeverityStyleMapping mapping2 = new SeverityStyleMapping(101, 200, AlertStyle.Critical);
        private SeverityStyleMapping mapping3 = new SeverityStyleMapping(50, 150, AlertStyle.Critical);

        [TestMethod]
        public void Constructor_Test()
        {
            var configuration = new RestrictionConfiguration();
            Assert.AreEqual(0, configuration.Mapping.Count);
        }
        
        [TestMethod]
        public void AddItem_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.AddItem(mapping1);
            Assert.AreEqual(1, configuration.Mapping.Count);
            Assert.AreEqual(mapping1.SeverityStart, configuration.Mapping[0].SeverityStart);
            Assert.AreEqual(mapping1.SeverityEnd, configuration.Mapping[0].SeverityEnd);
            Assert.AreEqual(mapping1.Style, configuration.Mapping[0].Style);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddDuplicateMapping_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.AddItem(mapping1);
            configuration.AddItem(mapping1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOverlappingMapping_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.AddItem(mapping1);
            configuration.AddItem(mapping3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOverlappingMapping2_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.AddItem(mapping2);
            configuration.AddItem(mapping3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullItem_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.AddItem(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveNullItem_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.RemoveItem(null);
        }

        [TestMethod]
        public void RemoveMissingItemFromMapping_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.AddItem(mapping1);
            var actual = configuration.RemoveItem(mapping2);
            Assert.AreEqual(false, actual);
            Assert.AreEqual(mapping1.SeverityStart, configuration.Mapping[0].SeverityStart);
            Assert.AreEqual(mapping1.SeverityEnd, configuration.Mapping[0].SeverityEnd);
            Assert.AreEqual(mapping1.Style, configuration.Mapping[0].Style);
        }

        [TestMethod]
        public void RemoveItem_Test()
        {
            var configuration = new RestrictionConfiguration();
            configuration.AddItem(mapping1);
            Assert.AreEqual(1, configuration.Mapping.Count);
            Assert.AreEqual(mapping1.SeverityStart, configuration.Mapping[0].SeverityStart);
            Assert.AreEqual(mapping1.SeverityEnd, configuration.Mapping[0].SeverityEnd);
            Assert.AreEqual(mapping1.Style, configuration.Mapping[0].Style);

            configuration.RemoveItem(mapping1);
            Assert.AreEqual(0, configuration.Mapping.Count);
        }
    }
}
