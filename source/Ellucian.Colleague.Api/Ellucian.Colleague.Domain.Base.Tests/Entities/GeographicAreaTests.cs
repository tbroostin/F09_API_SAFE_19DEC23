// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class GeographicAreaTests
    {
        private string guid;
        private string code;
        private string description;
        private Chapter chapter;
        private County county;
        private ZipcodeXlat zipCodeXlat;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "EU";
            description = "European Union";
        }

        [TestClass]
        public class GeographicAreaConstructor : GeographicAreaTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChapterConstructorNullGuid()
            {
                chapter = new Chapter(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CountyConstructorNullGuid()
            {
                county = new County(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ZipCodeXlatConstructorNullGuid()
            {
                zipCodeXlat = new ZipcodeXlat(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChapterConstructorEmptyGuid()
            {
                chapter = new Chapter(string.Empty, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CountyConstructorEmptyGuid()
            {
                county = new County(string.Empty, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ZipCodeXlatConstructorEmptyGuid()
            {
                zipCodeXlat = new ZipcodeXlat(string.Empty, code, description);
            }

            [TestMethod]
            public void ChapterConstructorValidGuid()
            {
                chapter = new Chapter(guid, code, description);
                Assert.AreEqual(guid, chapter.Guid);
            }

            [TestMethod]
            public void CountyConstructorValidGuid()
            {
                county = new County(guid, code, description);
                Assert.AreEqual(guid, county.Guid);
            }

            [TestMethod]
            public void ZipCodeXlatConstructorValidGuid()
            {
                zipCodeXlat = new ZipcodeXlat(guid, code, description);
                Assert.AreEqual(guid, zipCodeXlat.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChapterConstructorNullCode()
            {
                chapter = new Chapter(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CountyConstructorNullCode()
            {
                county = new County(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ZipCodeXlatConstructorNullCode()
            {
                zipCodeXlat = new ZipcodeXlat(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChapterConstructorEmptyCode()
            {
                chapter = new Chapter(guid, string.Empty, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CountyConstructorEmptyCode()
            {
                county = new County(guid, string.Empty, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ZipCodeXlatConstructorEmptyCode()
            {
                zipCodeXlat = new ZipcodeXlat(guid, string.Empty, description);
            }

            [TestMethod]
            public void ChapterConstructorValidCode()
            {
                chapter = new Chapter(guid, code, description);
                Assert.AreEqual(code, chapter.Code);
            }

            [TestMethod]
            public void CountyConstructorValidCode()
            {
                county = new County(guid, code, description);
                Assert.AreEqual(code, county.Code);
            }

            [TestMethod]
            public void ZipCodeXlatConstructorValidCode()
            {
                zipCodeXlat = new ZipcodeXlat(guid, code, description);
                Assert.AreEqual(code, zipCodeXlat.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChapterConstructorNullDescription()
            {
                chapter = new Chapter(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CountyConstructorNullDescription()
            {
                county = new County(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ZipCodeXlatConstructorNullDescription()
            {
                zipCodeXlat = new ZipcodeXlat(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChapterConstructorEmptyDescription()
            {
                chapter = new Chapter(guid, code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CountyConstructorEmptyDescription()
            {
                county = new County(guid, code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ZipCodeXlatConstructorEmptyDescription()
            {
                zipCodeXlat = new ZipcodeXlat(guid, code, string.Empty);
            }

            [TestMethod]
            public void ChapterConstructorValidDescription()
            {
                chapter = new Chapter(guid, code, description);
                Assert.AreEqual(description, chapter.Description);
            }

            [TestMethod]
            public void CountyConstructorValidDescription()
            {
                county = new County(guid, code, description);
                Assert.AreEqual(description, county.Description);
            }

            [TestMethod]
            public void ZipCodeXlatConstructorValidDescription()
            {
                zipCodeXlat = new ZipcodeXlat(guid, code, description);
                Assert.AreEqual(description, zipCodeXlat.Description);
            }
        }
    }
}
