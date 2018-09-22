// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    // Tests for the StudentStanding Entity
    public class StudentStandingTests
    {
        private string studentId;
        private string level;
        private string program;
        private string standing;
        private DateTime standingDate;
        private string term;
        private string id;

        Student.Entities.StudentStanding studentStandingEntity;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0000001";
            level = "UG";
            program = "BA.HIST";
            standing = "GOOD";
            standingDate = DateTime.Parse("2014-01-21");
            term = "2014/FA";
            id = "999";
            studentStandingEntity = new Student.Entities.StudentStanding(id, studentId, standing, standingDate);
            studentStandingEntity.Level = level;
            studentStandingEntity.Program = program;
            studentStandingEntity.Term = term;
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentStandingEntity = null;
        }

        [TestMethod]
        public void VerifyIdProp_Set()
        {
            Assert.AreEqual(id, studentStandingEntity.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Id_NotNull()
        {
            new Student.Entities.StudentStanding(null, studentId, standing, standingDate);
        }

        [TestMethod]
        public void VerifyStudentIdProp_Set()
        {
            Assert.AreEqual(studentId, studentStandingEntity.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentId_NotNull()
        {
            new Student.Entities.StudentStanding(id, null, standing, standingDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentStanding_NotNull()
        {
            new Student.Entities.StudentStanding(id, studentId, null, standingDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentStandingDate_NotNull()
        {
            new Student.Entities.StudentStanding(id, studentId, standing, default(DateTime));
        }

        [TestMethod]
        public void VerifyStandingIdProp_Set()
        {
            Assert.AreEqual(standing, studentStandingEntity.StandingCode);
        }

        [TestMethod]
        public void StandingDateIdProp_Set()
        {
            Assert.AreEqual(standingDate, studentStandingEntity.StandingDate);
        }
    }
}
