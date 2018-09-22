// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class CatalogTests
    {
        private string code;
        private DateTime startDate;
        private DateTime endDate;
        private Catalog catalog;
        
        [TestInitialize]
        public void Initialize()
        {
            code = "2012";
            startDate = new DateTime(2012, 1, 1);
            endDate = new DateTime(202, 12, 31);
            // Basic constructor statement
            catalog = new Catalog(code, startDate);
        }

        [TestCleanup]
        public void CleanUp()
        {

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CatalogExceptionCodeNull()
        {
            new Catalog(null, startDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CatalogExceptionStartDateMinValue()
        {
            new Catalog(code, DateTime.MinValue);
        }

        [TestMethod]
        public void CatalogCode()
        {
            Assert.AreEqual(code, catalog.Code);
        }
          
        [TestMethod]
        public void CatalogStartDate()
        {
            Assert.AreEqual(startDate, catalog.StartDate);
        }

        [TestMethod]
        public void CatalogEndDate()
        {
            catalog.EndDate = endDate;
            Assert.AreEqual(endDate, catalog.EndDate);
        }
    }

    [TestClass]
    public class CatalogTestsWithGuid
    {
        private string guid; 
        private string code;
        private DateTime startDate;
        private DateTime endDate;
        private Catalog catalog;
        private List<string> acadPrograms;

        [TestInitialize]
        public void Initialize()
        {
            guid = "FBCD5B0A-7551-4260-9B01-1529D4B52CBE";
            code = "2012";
            startDate = new DateTime(2012, 1, 1);
            endDate = new DateTime(2013, 12, 31);
            acadPrograms = new List<string>() { "MATH.BS" };
            // Basic constructor statement
            catalog = new Catalog(guid, code, startDate) 
                { AcadPrograms = acadPrograms };
        }

        [TestCleanup]
        public void CleanUp()
        {

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CatalogExceptionCodeNull()
        {
            new Catalog(guid, null, startDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CatalogExceptionStartDateMinValue()
        {
            new Catalog(guid, code, DateTime.MinValue);
        }
        
        [TestMethod]
        public void CatalogGuid()
        {
            Assert.AreEqual(guid, catalog.Guid);
        }

        [TestMethod]
        public void CatalogCode()
        {
            Assert.AreEqual(code, catalog.Code);
        }

        [TestMethod]
        public void CatalogStartDate()
        {
            Assert.AreEqual(startDate, catalog.StartDate);
        }

        [TestMethod]
        public void CatalogEndDate()
        {
            catalog.EndDate = endDate;
            Assert.AreEqual(endDate, catalog.EndDate);
        }

        [TestMethod]
        public void CatalogIsActive_NoEndDate()
        {
            Assert.IsTrue(catalog.IsActive);
        }

        [TestMethod]
        public void CatalogIsActive_EndDate_Future()
        {
            DateTime d1 = DateTime.Now;
            catalog.EndDate = d1.AddDays(2);
            Assert.IsTrue(catalog.IsActive);
        }


        [TestMethod]
        public void CatalogIsActive_EndDate_Past()
        {
            DateTime d1 = DateTime.Now;
            catalog.EndDate = d1.AddDays(-2);
            Assert.IsFalse(catalog.IsActive);
        }

        [TestMethod]
        public void CatalogAcadProgams()
        {
            Assert.AreEqual(acadPrograms, catalog.AcadPrograms);
        }
    }
}
