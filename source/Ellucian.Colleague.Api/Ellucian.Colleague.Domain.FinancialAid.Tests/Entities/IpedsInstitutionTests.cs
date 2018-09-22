// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class IpedsInstitutionTests
    {
        [TestClass]
        public class IpedsInstitutionConstructorTests
        {
            public string id;
            public string unitId;
            public string name;
            public string opeId;
            public DateTime lastModifiedDate;

            public IpedsInstitution ipedsInstitution;

            [TestInitialize]
            public void Initialize()
            {
                id = "1";
                unitId = "2380923";
                name = "Ellucian University";
                opeId = "090888444";
                lastModifiedDate = DateTime.Today;

                ipedsInstitution = new IpedsInstitution(id, unitId, name, opeId, lastModifiedDate);
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, ipedsInstitution.Id);
            }

            [TestMethod]
            public void UnitIdTest()
            {
                Assert.AreEqual(unitId, ipedsInstitution.UnitId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void UnitIdRequiredTest()
            {
                new IpedsInstitution(id, "", name, opeId, lastModifiedDate);
            }

            [TestMethod]
            public void NameTest()
            {
                Assert.AreEqual(name, ipedsInstitution.Name);
            }

            [TestMethod]
            public void OpeIdTest()
            {
                Assert.AreEqual(opeId, ipedsInstitution.OpeId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OpeIdRequiredTest()
            {
                new IpedsInstitution(id, unitId, name, null, lastModifiedDate);
            }

            [TestMethod]
            public void LastModifiedDateTest()
            {
                Assert.AreEqual(lastModifiedDate, ipedsInstitution.LastModifiedDate);
            }
        }

        [TestClass]
        public class IpedsInstitutionEqualsTests
        {
            public string id;
            public string unitId;
            public string name;
            public string opeId;
            public DateTime lastModifiedDate;

            public IpedsInstitution ipedsInstitution;

            [TestInitialize]
            public void Initialize()
            {
                id = "1";
                unitId = "2380923";
                name = "Ellucian University";
                opeId = "090888444";
                lastModifiedDate = DateTime.Today;

                ipedsInstitution = new IpedsInstitution(id, unitId, name, opeId, lastModifiedDate);
            }

            [TestMethod]
            public void SameUnitId_EqualTest()
            {
                var testIpedsInstitution = new IpedsInstitution(id, unitId, name, opeId, lastModifiedDate);
                Assert.AreEqual(testIpedsInstitution, ipedsInstitution);
            }

            [TestMethod]
            public void DiffUnitId_NotEqualTest()
            {
                var testIpedsInstitution = new IpedsInstitution(id, "foobar", name, opeId, lastModifiedDate);
                Assert.AreNotEqual(testIpedsInstitution, ipedsInstitution);
            }

            [TestMethod]
            public void DiffId_EqualTest()
            {
                var testIpedsInstitution = new IpedsInstitution("foobar", unitId, name, opeId, lastModifiedDate);
                Assert.AreEqual(testIpedsInstitution, ipedsInstitution);
            }

            [TestMethod]
            public void DiffName_EqualTest()
            {
                var testIpedsInstitution = new IpedsInstitution(id, unitId, "foobar", opeId, lastModifiedDate);
                Assert.AreEqual(testIpedsInstitution, ipedsInstitution);
            }

            [TestMethod]
            public void DiffOpeId_EqualTest()
            {
                var testIpedsInstitution = new IpedsInstitution(id, unitId, name, "foobar", lastModifiedDate);
                Assert.AreEqual(testIpedsInstitution, ipedsInstitution);
            }

            [TestMethod]
            public void DiffLastModifiedDate_EqualTest()
            {
                var testIpedsInstitution = new IpedsInstitution(id, unitId, name, opeId, DateTime.Now.Add(TimeSpan.FromDays(1)));
                Assert.AreEqual(testIpedsInstitution, ipedsInstitution);
            }
        }
    }
}
