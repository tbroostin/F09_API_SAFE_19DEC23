// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CareerGoalTests
    {
        [TestClass]
        public class CareerGoal_Constructor
        {
            private string code;
            private string desc;
            private CareerGoal careerGoal;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADM";
                desc = "Admitted";
                careerGoal = new CareerGoal(code, desc);
            }

            [TestMethod]
            public void CareerGoal_Code()
            {
                Assert.AreEqual(code, careerGoal.Code);
            }

            [TestMethod]
            public void CareerGoal_Description()
            {
                Assert.AreEqual(desc, careerGoal.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoal_CodeNullException()
            {
                new CareerGoal(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoalCodeEmptyException()
            {
                new CareerGoal(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoalDescEmptyException()
            {
                new CareerGoal(code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoal_DescNullException()
            {
                new CareerGoal(code, null);
            }

        }
    }
}