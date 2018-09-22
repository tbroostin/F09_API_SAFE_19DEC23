// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class NonAcademicAttendanceEventTypeTests
    {
        private string code;
        private string description;
        private NonAcademicAttendanceEventType entity;

        [TestInitialize]
        public void Initialize()
        {
            code = "CODE";
            description = "Description";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicAttendanceEventType_Constructor_Null_Code()
        {
            entity = new NonAcademicAttendanceEventType(null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicAttendanceEventType_Constructor_Null_Description()
        {
            entity = new NonAcademicAttendanceEventType(code, null);
        }

        [TestMethod]
        public void NonAcademicAttendanceEventType_Constructor_Valid()
        {
            entity = new NonAcademicAttendanceEventType(code, description);
            Assert.AreEqual(code, entity.Code);
            Assert.AreEqual(description, entity.Description);
        }
    }
}
