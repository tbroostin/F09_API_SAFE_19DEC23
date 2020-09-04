//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
     [TestClass]
    public class EducationGoalsTests
    {
        [TestClass]
        public class EducationGoalsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private EducationGoals educationalGoals;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                educationalGoals = new EducationGoals(guid, code, desc);
            }

            [TestMethod]
            public void EducationGoals_Code()
            {
                Assert.AreEqual(code, educationalGoals.Code);
            }

            [TestMethod]
            public void EducationGoals_Description()
            {
                Assert.AreEqual(desc, educationalGoals.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoals_GuidNullException()
            {
                new EducationGoals(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoals_CodeNullException()
            {
                new EducationGoals(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoals_DescNullException()
            {
                new EducationGoals(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoalsGuidEmptyException()
            {
                new EducationGoals(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoalsCodeEmptyException()
            {
                new EducationGoals(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoalsDescEmptyException()
            {
                new EducationGoals(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class EducationGoals_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private EducationGoals educationalGoals1;
            private EducationGoals educationalGoals2;
            private EducationGoals educationalGoals3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                educationalGoals1 = new EducationGoals(guid, code, desc);
                educationalGoals2 = new EducationGoals(guid, code, "Second Year");
                educationalGoals3 = new EducationGoals(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EducationGoalsSameCodesEqual()
            {
                Assert.IsTrue(educationalGoals1.Equals(educationalGoals2));
            }

            [TestMethod]
            public void EducationGoalsDifferentCodeNotEqual()
            {
                Assert.IsFalse(educationalGoals1.Equals(educationalGoals3));
            }
        }

        [TestClass]
        public class EducationGoals_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private EducationGoals educationalGoals1;
            private EducationGoals educationalGoals2;
            private EducationGoals educationalGoals3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                educationalGoals1 = new EducationGoals(guid, code, desc);
                educationalGoals2 = new EducationGoals(guid, code, "Second Year");
                educationalGoals3 = new EducationGoals(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EducationGoalsSameCodeHashEqual()
            {
                Assert.AreEqual(educationalGoals1.GetHashCode(), educationalGoals2.GetHashCode());
            }

            [TestMethod]
            public void EducationGoalsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(educationalGoals1.GetHashCode(), educationalGoals3.GetHashCode());
            }
        }
    }
}
