using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class GradeRestrictionTests
    {
        [TestClass]
        public class GradeRestrictionConstructor
        {
            GradeRestriction gradeRestriction;
            bool isRestricted;
            
            [TestInitialize]
            public void Initialize()
            {
                isRestricted = false;
                gradeRestriction = new GradeRestriction(isRestricted);
            }

            [TestMethod]
            public void IsRestricted()
            {
                Assert.AreEqual(isRestricted, gradeRestriction.IsRestricted);
            }

            [TestMethod]
            public void Reasons()
            {
                Assert.IsNotNull(gradeRestriction.Reasons);
                Assert.AreEqual(0, gradeRestriction.Reasons.Count());
            }
        }

        [TestClass]
        public class GradeRestrictionAddReason
        {
            GradeRestriction gradeRestriction;
            bool isRestricted;

            [TestInitialize]
            public void Initialize()
            {
                isRestricted = false;
                gradeRestriction = new GradeRestriction(isRestricted);
                gradeRestriction.AddReason("Library Fines");
            }

            [TestMethod]
            public void IsRestricted()
            {
                // Adding a reason changes IsRestricted to true.
                Assert.AreEqual(true, gradeRestriction.IsRestricted);
            }

            [TestMethod]
            public void Reasons()
            {
                Assert.IsNotNull(gradeRestriction.Reasons);
                Assert.AreEqual(1, gradeRestriction.Reasons.Count());
            }
        }
    }
}
