// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class EducationGoalTests
    {
        [TestClass]
        public class EducationGoal_Constructor_Tests
        {
            private string code;
            private string desc;
            private EducationGoal entity;

            [TestInitialize]
            public void Initialize()
            {
                code = "BA";
                desc = "Bachelors Degree";
                entity = new EducationGoal(code, desc);
            }

            [TestMethod]
            public void EducationGoal_Code()
            {
                Assert.AreEqual(code, entity.Code);
            }

            [TestMethod]
            public void EducationGoal_Description()
            {
                Assert.AreEqual(desc, entity.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoal_CodeNullException()
            {
                new EducationGoal(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoal_DescNullException()
            {
                new EducationGoal(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoalCodeEmptyException()
            {
                new EducationGoal(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EducationGoalDescEmptyException()
            {
                new EducationGoal(code, string.Empty);
            }

        }
    }
}
