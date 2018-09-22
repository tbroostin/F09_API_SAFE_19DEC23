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
    public class PersonBenefitDependentTests
    {
        [TestClass]
        public class PersonBenefitDependentConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private PersonBenefitDependent personBenefitDependents;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                personBenefitDependents = new PersonBenefitDependent(guid, code, desc);
            }

            [TestMethod]
            public void PersonBenefitDependent_DeductionArrangement()
            {
                Assert.AreEqual(code, personBenefitDependents.DeductionArrangement);
            }

            [TestMethod]
            public void PersonBenefitDependent_DependentPersonId()
            {
                Assert.AreEqual(desc, personBenefitDependents.DependentPersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonBenefitDependent_GuidNullException()
            {
                new PersonBenefitDependent(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonBenefitDependent_CodeNullException()
            {
                new PersonBenefitDependent(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonBenefitDependent_DescNullException()
            {
                new PersonBenefitDependent(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonBenefitDependentGuidEmptyException()
            {
                new PersonBenefitDependent(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonBenefitDependentCodeEmptyException()
            {
                new PersonBenefitDependent(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonBenefitDependentDescEmptyException()
            {
                new PersonBenefitDependent(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class PersonBenefitDependent_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private PersonBenefitDependent personBenefitDependents1;
            private PersonBenefitDependent personBenefitDependents2;
            private PersonBenefitDependent personBenefitDependents3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                personBenefitDependents1 = new PersonBenefitDependent(guid, code, desc);
                personBenefitDependents2 = new PersonBenefitDependent(guid, code, "Admissions");
                personBenefitDependents3 = new PersonBenefitDependent(Guid.NewGuid().ToString(), "200", desc);
            }

            //[TestMethod]
            //public void PersonBenefitDependentSameCodesEqual()
            //{
            //    Assert.IsTrue(personBenefitDependents1.Equals(personBenefitDependents2));
            //}

            [TestMethod]
            public void PersonBenefitDependentDifferentCodeNotEqual()
            {
                Assert.IsFalse(personBenefitDependents1.Equals(personBenefitDependents3));
            }
        }

        [TestClass]
        public class PersonBenefitDependent_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private PersonBenefitDependent personBenefitDependents1;
            private PersonBenefitDependent personBenefitDependents2;
            private PersonBenefitDependent personBenefitDependents3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                personBenefitDependents1 = new PersonBenefitDependent(guid, code, desc);
                personBenefitDependents2 = new PersonBenefitDependent(guid, code, "Second Year");
                personBenefitDependents3 = new PersonBenefitDependent(Guid.NewGuid().ToString(), "200", desc);
            }

            //[TestMethod]
            //public void PersonBenefitDependentSameCodeHashEqual()
            //{
            //    Assert.AreEqual(personBenefitDependents1.GetHashCode(), personBenefitDependents2.GetHashCode());
            //}

            [TestMethod]
            public void PersonBenefitDependentDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(personBenefitDependents1.GetHashCode(), personBenefitDependents3.GetHashCode());
            }
        }
    }
}
