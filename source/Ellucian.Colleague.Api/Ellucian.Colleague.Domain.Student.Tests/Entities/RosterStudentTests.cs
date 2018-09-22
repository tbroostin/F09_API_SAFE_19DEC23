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
    public class RosterStudentTests
    {
        [TestClass]
        public class RosterStudent_Constructor
        {
            private string studentId;
            private string lastName;
            private RosterStudent student;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003315";
                lastName = "Smith";
                student = new RosterStudent(studentId, lastName);
            }

            [TestMethod]
            public void RosterStudent_Id()
            {
                Assert.AreEqual(studentId, student.Id);
            }

            [TestMethod]
            public void RosterStudent_LastName()
            {
                Assert.AreEqual(lastName, student.LastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RosterStudent_IdNullException()
            {
                new RosterStudent(null, lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RosterStudentIdEmptyException()
            {
                new RosterStudent(string.Empty, lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RosterStudentLastNameEmptyException()
            {
                new RosterStudent(studentId, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RosterStudent_LastNameNullException()
            {
                new RosterStudent(studentId, null);
            }

        }
    }
}