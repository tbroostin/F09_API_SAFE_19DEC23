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
    public class SeverityStyleMappingTests
    {
        [TestMethod]
        public void ValidSeverityStyleMapping_Test()
        {
            SeverityStyleMapping mapping = new SeverityStyleMapping(0, 999, AlertStyle.Information);
            Assert.AreEqual(0, mapping.SeverityStart);
            Assert.AreEqual(999, mapping.SeverityEnd);
            Assert.AreEqual(AlertStyle.Information, mapping.Style);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddNegativeStart_Test()
        {
            SeverityStyleMapping invMapping = new SeverityStyleMapping(-1, 999, AlertStyle.Information);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOver999Start_Test()
        {
            SeverityStyleMapping invMapping = new SeverityStyleMapping(1000, 999, AlertStyle.Information);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddInvalidStartEndOrder_Test()
        {
            SeverityStyleMapping invMapping = new SeverityStyleMapping(100, 1, AlertStyle.Information);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddNegativeEnd_Test()
        {
            SeverityStyleMapping invMapping = new SeverityStyleMapping(0, -1, AlertStyle.Information);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOver999End_Test()
        {
            SeverityStyleMapping invMapping = new SeverityStyleMapping(0, 1000, AlertStyle.Information);
        }
    }
}
