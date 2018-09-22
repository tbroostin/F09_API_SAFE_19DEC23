//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
     [TestClass]
    public class MealTypeTests
    {
        [TestClass]
        public class MealTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private MealType mealTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                mealTypes = new MealType(guid, code, desc);
            }

            [TestMethod]
            public void MealType_Code()
            {
                Assert.AreEqual(code, mealTypes.Code);
            }

            [TestMethod]
            public void MealType_Description()
            {
                Assert.AreEqual(desc, mealTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MealType_GuidNullException()
            {
                new MealType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MealType_CodeNullException()
            {
                new MealType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MealType_DescNullException()
            {
                new MealType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MealTypeGuidEmptyException()
            {
                new MealType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MealTypeCodeEmptyException()
            {
                new MealType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MealTypeDescEmptyException()
            {
                new MealType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class MealType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private MealType mealTypes1;
            private MealType mealTypes2;
            private MealType mealTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                mealTypes1 = new MealType(guid, code, desc);
                mealTypes2 = new MealType(guid, code, "Second Year");
                mealTypes3 = new MealType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void MealTypeSameCodesEqual()
            {
                Assert.IsTrue(mealTypes1.Equals(mealTypes2));
            }

            [TestMethod]
            public void MealTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(mealTypes1.Equals(mealTypes3));
            }
        }

        [TestClass]
        public class MealType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private MealType mealTypes1;
            private MealType mealTypes2;
            private MealType mealTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                mealTypes1 = new MealType(guid, code, desc);
                mealTypes2 = new MealType(guid, code, "Second Year");
                mealTypes3 = new MealType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void MealTypeSameCodeHashEqual()
            {
                Assert.AreEqual(mealTypes1.GetHashCode(), mealTypes2.GetHashCode());
            }

            [TestMethod]
            public void MealTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(mealTypes1.GetHashCode(), mealTypes3.GetHashCode());
            }
        }
    }
}
