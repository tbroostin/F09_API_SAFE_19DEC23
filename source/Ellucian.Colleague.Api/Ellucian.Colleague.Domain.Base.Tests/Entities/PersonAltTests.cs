// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonAltTests
    {
        private string personAltId;

        [TestInitialize]
        public void Initialize()
        {
            personAltId = "00012345";
        }

        [TestMethod]
        public void PersonAltConstructorTest()
        {
            var target = new PersonAlt(personAltId, PersonAlt.ElevatePersonAltType);

            Assert.AreEqual(personAltId, target.Id);
            Assert.AreEqual(PersonAlt.ElevatePersonAltType, target.Type);
        }

        [TestMethod]
        public void PersonAltConstructorNullTypeTest()
        {
            var target = new PersonAlt(personAltId, null);

            Assert.AreEqual(personAltId, target.Id);
            Assert.AreEqual(null, target.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonAltConstructorExceptionNullIdArgument()
        {
            new PersonAlt(null, PersonAlt.ElevatePersonAltType);
        }

        [TestMethod]
        public void PersonAlt_EqualsNull_VerifyFalse()
        {
            var target = new PersonAlt(personAltId, PersonAlt.ElevatePersonAltType);
            Assert.IsFalse(target.Equals(null));
        }

        [TestMethod]
        public void PersonAlt_EqualsNonPersonAlt_VerifyFalse()
        {
            var target = new PersonAlt(personAltId, PersonAlt.ElevatePersonAltType);
            Assert.IsFalse(target.Equals("abc"));
        }

        [TestMethod]
        public void PersonAlt_EqualsSameIdDifferentType_VerifyFalse()
        {
            var target = new PersonAlt(personAltId, "TEST");
            var target2 = new PersonAlt(personAltId, "TEST2");
            Assert.IsFalse(target.Equals(target2));
        }

        [TestMethod]
        public void PersonAlt_EqualsSameIdSameType_VerifyTrue()
        {
            var target = new PersonAlt(personAltId, "TEST");
            var target2 = new PersonAlt(personAltId, "TEST");
            Assert.IsTrue(target.Equals(target2));
        }

        [TestMethod]
        public void PersonAlt_GetHashCode()
        {
            var target = new PersonAlt(personAltId, "TEST");
            var target2 = new PersonAlt(personAltId, "TEST2");
            Assert.AreEqual(target.GetHashCode(), target2.GetHashCode());
        }

        [TestMethod]
        public void PersonAlt_ToString()
        {
            var target = new PersonAlt(personAltId, PersonAlt.ElevatePersonAltType);
            Assert.AreEqual(personAltId, target.ToString());
        }
    }
}
