// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class EnrollmentStatusTests
    {
        private string guid;
        private string code;
        private string description;
        private EnrollmentStatusType type;
        private EnrollmentStatus enrollmentStatus;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "A";
            description = "Active";
            type = EnrollmentStatusType.active;
        }

        [TestClass]
        public class EnrollmentStatusConstructor : EnrollmentStatusTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EnrollmentStatusConstructorNullGuid()
            {
                enrollmentStatus = new EnrollmentStatus(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EnrollmentStatusConstructorEmptyGuid()
            {
                enrollmentStatus = new EnrollmentStatus(string.Empty, code, description, type);
            }

            [TestMethod]
            public void EnrollmentStatusConstructorValidGuid()
            {
                enrollmentStatus = new EnrollmentStatus(guid, code, description, type);
                Assert.AreEqual(guid, enrollmentStatus.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EnrollmentStatusConstructorNullCode()
            {
                enrollmentStatus = new EnrollmentStatus(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EnrollmentStatusConstructorEmptyCode()
            {
                enrollmentStatus = new EnrollmentStatus(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void EnrollmentStatusConstructorValidCode()
            {
                enrollmentStatus = new EnrollmentStatus(guid, code, description, type);
                Assert.AreEqual(code, enrollmentStatus.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EnrollmentStatusConstructorNullDescription()
            {
                enrollmentStatus = new EnrollmentStatus(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EnrollmentStatusConstructorEmptyDescription()
            {
                enrollmentStatus = new EnrollmentStatus(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void EnrollmentStatusConstructorValidDescription()
            {
                enrollmentStatus = new EnrollmentStatus(guid, code, description, type);
                Assert.AreEqual(description, enrollmentStatus.Description);
            }

            [TestMethod]
            public void EnrollmentStatusConstructorValidType()
            {
                enrollmentStatus = new EnrollmentStatus(guid, code, description, type);
                Assert.AreEqual(type, enrollmentStatus.EnrollmentStatusType);
            }
        }
    }
}
