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
    public class PersonHighSchoolTests
    {
        private string personId;
        private string highSchoolId;
        private decimal gpa;

        [TestInitialize]
        public void Initialize()
        {
            personId = "00012345";
            highSchoolId = "0023456";
            gpa = 3.351m;
        }

        [TestMethod]
        public void PersonHighSchool_Constructor_PersonId()
        {
            var target = new PersonHighSchool(personId, highSchoolId, gpa);
            Assert.AreEqual(personId, target.PersonId);
        }

        [TestMethod]
        public void PersonHighSchool_Constructor_HighSchoolId()
        {
            var target = new PersonHighSchool(personId, highSchoolId, gpa);
            Assert.AreEqual(highSchoolId, target.HighSchoolId);
        }

        [TestMethod]
        public void PersonHighSchool_Constructor_GradePointAverage()
        {
            var target = new PersonHighSchool(personId, highSchoolId, gpa);
            Assert.AreEqual(gpa, target.GradePointAverage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonHighSchool_Constructor_NullPersonId()
        {
            var target = new PersonHighSchool(null, highSchoolId, gpa);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonHighSchool_Constructor_EmptyPersonId()
        {
            var target = new PersonHighSchool(string.Empty, highSchoolId, gpa);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonHighSchool_Constructor_NullHighSchoolId()
        {
            var target = new PersonHighSchool(personId, null, gpa);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonHighSchool_Constructor_EmptyHighSchoolId()
        {
            var target = new PersonHighSchool(personId, string.Empty, gpa);
        }
    }
}
