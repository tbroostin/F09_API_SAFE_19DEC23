//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class EmploymentFrequencyTests
    {
        [TestClass]
         public class EmploymentFrequencyConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentFrequency employmentFrequencies;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentFrequencies = new EmploymentFrequency(guid, code, desc, "26");
            }

            [TestMethod]
            public void EmploymentFrequency_Code()
            {
                Assert.AreEqual(code, employmentFrequencies.Code);
            }

            [TestMethod]
            public void EmploymentFrequency_Description()
            {
                Assert.AreEqual(desc, employmentFrequencies.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentFrequency_GuidNullException()
            {
                new EmploymentFrequency(null, code, desc, "24");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentFrequency_CodeNullException()
            {
                new EmploymentFrequency(guid, null, desc, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentFrequency_DescNullException()
            {
                new EmploymentFrequency(guid, code, null, "52");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentFrequencyGuidEmptyException()
            {
                new EmploymentFrequency(string.Empty, code, desc, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentFrequencyCodeEmptyException()
            {
                new EmploymentFrequency(guid, string.Empty, desc, "4");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentFrequencyDescEmptyException()
            {
                new EmploymentFrequency(guid, code, string.Empty, "4");
            }

        }

        [TestClass]
        public class EmploymentFrequency_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentFrequency employmentFrequencies1;
            private EmploymentFrequency employmentFrequencies2;
            private EmploymentFrequency employmentFrequencies3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentFrequencies1 = new EmploymentFrequency(guid, code, desc, "1");
                employmentFrequencies2 = new EmploymentFrequency(guid, code, "Second Year", "1");
                employmentFrequencies3 = new EmploymentFrequency(Guid.NewGuid().ToString(), "200", desc, "1");
            }

            [TestMethod]
            public void EmploymentFrequencySameCodesEqual()
            {
                Assert.IsTrue(employmentFrequencies1.Equals(employmentFrequencies2));
            }

            [TestMethod]
            public void EmploymentFrequencyDifferentCodeNotEqual()
            {
                Assert.IsFalse(employmentFrequencies1.Equals(employmentFrequencies3));
            }
        }

        [TestClass]
        public class EmploymentFrequency_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentFrequency employmentFrequencies1;
            private EmploymentFrequency employmentFrequencies2;
            private EmploymentFrequency employmentFrequencies3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentFrequencies1 = new EmploymentFrequency(guid, code, desc, "4");
                employmentFrequencies2 = new EmploymentFrequency(guid, code, "Second Year", "4");
                employmentFrequencies3 = new EmploymentFrequency(Guid.NewGuid().ToString(), "200", desc, "4");
            }

            [TestMethod]
            public void EmploymentFrequencySameCodeHashEqual()
            {
                Assert.AreEqual(employmentFrequencies1.GetHashCode(), employmentFrequencies2.GetHashCode());
            }

            [TestMethod]
            public void EmploymentFrequencyDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(employmentFrequencies1.GetHashCode(), employmentFrequencies3.GetHashCode());
            }
        }
    }
}
