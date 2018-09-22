using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AdmissionResidencyTypeTests
    {
        [TestClass]
        public class AdmissionResidencyTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AdmissionResidencyType admissionResidencyType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Student Receivable";
                admissionResidencyType = new AdmissionResidencyType(guid, code, desc);
            }

            [TestMethod]
            public void AdmissionResidencyType_Code()
            {
                Assert.AreEqual(code, admissionResidencyType.Code);
            }

            [TestMethod]
            public void AdmissionResidencyType_Description()
            {
                Assert.AreEqual(desc, admissionResidencyType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmissionResidencyType_GuidNullException()
            {
                new AdmissionResidencyType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmissionResidencyType_CodeNullException()
            {
                new AdmissionResidencyType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmissionResidencyType_DescNullException()
            {
                new AdmissionResidencyType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmissionResidencyTypeGuidEmptyException()
            {
                new AdmissionResidencyType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmissionResidencyTypeCodeEmptyException()
            {
                new AdmissionResidencyType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmissionResidencyTypeDescEmptyException()
            {
                new AdmissionResidencyType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class AdmissionResidencyType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AdmissionResidencyType admissionResidencyType1;
            private AdmissionResidencyType admissionResidencyType2;
            private AdmissionResidencyType admissionResidencyType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Student Receivable";
                admissionResidencyType1 = new AdmissionResidencyType(guid, code, desc);
                admissionResidencyType2 = new AdmissionResidencyType(guid, code, "Project Receivables");
                admissionResidencyType3 = new AdmissionResidencyType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void AdmissionResidencyTypeSameCodesEqual()
            {
                Assert.IsTrue(admissionResidencyType1.Equals(admissionResidencyType2));
            }

            [TestMethod]
            public void AdmissionResidencyTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(admissionResidencyType1.Equals(admissionResidencyType3));
            }
        }

        [TestClass]
        public class AdmissionResidencyType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AdmissionResidencyType admissionResidencyType1;
            private AdmissionResidencyType admissionResidencyType2;
            private AdmissionResidencyType admissionResidencyType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Student Receivable";
                admissionResidencyType1 = new AdmissionResidencyType(guid, code, desc);
                admissionResidencyType2 = new AdmissionResidencyType(guid, code, "Project Receivables");
                admissionResidencyType3 = new AdmissionResidencyType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void AdmissionResidencyTypeSameCodeHashEqual()
            {
                Assert.AreEqual(admissionResidencyType1.GetHashCode(), admissionResidencyType2.GetHashCode());
            }

            [TestMethod]
            public void AdmissionResidencyTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(admissionResidencyType1.GetHashCode(), admissionResidencyType3.GetHashCode());
            }
        }
    }
}
