// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class InstitutionTests
    {
        [TestClass]
        public class InstitutionConstructor
        {
            private string instId;
            private InstType instType;
            private string name;
            private string city;
            private string state;
            private bool isHostInstitution;
            private string financialAidInstitutionName;
            private Institution institution;

            [TestInitialize]
            public void Initialize()
            {
                instId = "0000456";
                instType = InstType.College;
                name = "Cambridge College";
                city = "Annopolis";
                state = "MD";
                financialAidInstitutionName = "Cambridge College FA";
                isHostInstitution = true;

                institution = new Institution(instId, instType) { Name = name, City = city, State = state, IsHostInstitution = isHostInstitution, FinancialAidInstitutionName = financialAidInstitutionName };
            }

            [TestMethod]
            public void InstitutionId()
            {
                Assert.AreEqual(instId, institution.Id);
            }

            [TestMethod]
            public void InstitutionName()
            {
                Assert.AreEqual(name, institution.Name);
            }

            [TestMethod]
            public void InstitutionType() {
                Assert.AreEqual(instType, institution.InstitutionType);
            }

            [TestMethod]
            public void InstitutionCity() {
                Assert.AreEqual(city, institution.City);
            }

            [TestMethod]
            public void InstitutionState() {
                Assert.AreEqual(state, institution.State);
            }

            [TestMethod]
            public void IsHostInstitution()
            {
                Assert.AreEqual(isHostInstitution, institution.IsHostInstitution);
            }

            [TestMethod]
            public void FinancialAidInstitutionName()
            {
                Assert.AreEqual(financialAidInstitutionName, institution.FinancialAidInstitutionName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstitutionIdNullException()
            {
                new Institution(null, instType);
            }

            [TestMethod]
            public void InstitutionConstructor2()
            {
                new Institution();
            }
        }

        [TestClass]
        public class InstitutionEquals
        {
            private string instId;
            private InstType instType;
            private Institution inst1;
            private Institution inst2;
            private Institution inst3;

            [TestInitialize]
            public void Initialize()
            {
                instId = "0001356";
                instType = InstType.College;

                inst1 = new Institution(instId, instType);
                inst2 = new Institution(instId, InstType.College);
                inst3 = new Institution("0003478", instType);
            }

            [TestMethod]
            public void InstitutionSameIdDifferentTypeNotEqual()
            {
                Assert.IsFalse(inst1.Equals(inst2));
            }

            [TestMethod]
            public void InstitutionDifferentIdNotEqual()
            {
                Assert.IsFalse(inst1.Equals(inst3));
            }
        }

        [TestClass]
        public class InstitutionGetHashCode
        {
            private string instId;
            private InstType instType;
            private Institution inst1;
            private Institution inst2;
            private Institution inst3;

            [TestInitialize]
            public void Initialize()
            {
                instId = "0001356";
                instType = InstType.College;

                inst1 = new Institution(instId, instType);
                inst2 = new Institution(instId, InstType.College);
                inst3 = new Institution("0003478", instType);
            }

            [TestMethod]
            public void InstitutionSameIdDifferentTypeHashNotEqual()
            {
                Assert.AreNotEqual(inst1.GetHashCode(), inst2.GetHashCode());
            }

            [TestMethod]
            public void InstitutionDifferentIdHashNotEqual()
            {
                Assert.AreNotEqual(inst1.GetHashCode(), inst3.GetHashCode());
            }
        }
    }
}