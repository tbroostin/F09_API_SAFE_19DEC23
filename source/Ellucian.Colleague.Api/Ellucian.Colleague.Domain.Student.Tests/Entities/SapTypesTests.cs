//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
     [TestClass]
    public class SaptypeTests
    {
        [TestClass]
        public class SaptypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private SapType sapType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                sapType = new SapType(guid, code, desc);
            }

            [TestMethod]
            public void Saptype_Code()
            {
                Assert.AreEqual(code, sapType.Code);
            }

            [TestMethod]
            public void Saptype_Description()
            {
                Assert.AreEqual(desc, sapType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Saptype_GuidNullException()
            {
                new SapType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Saptype_CodeNullException()
            {
                new SapType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Saptype_DescNullException()
            {
                new SapType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SaptypeGuidEmptyException()
            {
                new SapType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SaptypeCodeEmptyException()
            {
                new SapType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SaptypeDescEmptyException()
            {
                new SapType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class Saptype_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private SapType financialAidAcademicProgressTypes1;
            private SapType financialAidAcademicProgressTypes2;
            private SapType financialAidAcademicProgressTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                financialAidAcademicProgressTypes1 = new SapType(guid, code, desc);
                financialAidAcademicProgressTypes2 = new SapType(guid, code, "Second Year");
                financialAidAcademicProgressTypes3 = new SapType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void SaptypeSameCodesEqual()
            {
                Assert.IsTrue(financialAidAcademicProgressTypes1.Equals(financialAidAcademicProgressTypes2));
            }

            [TestMethod]
            public void SaptypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(financialAidAcademicProgressTypes1.Equals(financialAidAcademicProgressTypes3));
            }
        }

        [TestClass]
        public class Saptype_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private SapType financialAidAcademicProgressTypes1;
            private SapType financialAidAcademicProgressTypes2;
            private SapType financialAidAcademicProgressTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                financialAidAcademicProgressTypes1 = new SapType(guid, code, desc);
                financialAidAcademicProgressTypes2 = new SapType(guid, code, "Second Year");
                financialAidAcademicProgressTypes3 = new SapType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void SaptypeSameCodeHashEqual()
            {
                Assert.AreEqual(financialAidAcademicProgressTypes1.GetHashCode(), financialAidAcademicProgressTypes2.GetHashCode());
            }

            [TestMethod]
            public void SaptypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(financialAidAcademicProgressTypes1.GetHashCode(), financialAidAcademicProgressTypes3.GetHashCode());
            }
        }
    }
}
