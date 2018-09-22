// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    // Tests for the VerifiedTermGrade Entity
    public class VerifiedTermGradeTests
    {
        private string _gradeId;
        private string _gradeKey;
        private string _grade;
        private DateTimeOffset? _gradeTimestamp;
        private string _submittedBy;
        private string _gradeChangeReason;
        private string _gradeTypeCode;

        Student.Entities.VerifiedTermGrade _verifiedTermGradeEntity;

        [TestInitialize]
        public void Initialize()
        {
            _gradeId = "1";
            _gradeKey = "14";
            _grade = "A";
            _gradeTimestamp = new DateTime(2016, 04, 01);
            _submittedBy = "0012297";
            _gradeChangeReason = "IC";
            _gradeTypeCode = "VERIFIED";

            _verifiedTermGradeEntity = new VerifiedTermGrade(_gradeId, _gradeTimestamp, _submittedBy)
            {
                GradeChangeReason = _gradeChangeReason,
                GradeTypeCode = _gradeTypeCode,
                GradeKey = _gradeKey,
                Grade = _grade
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _verifiedTermGradeEntity = null;
        }

        [TestMethod]
        public void VerifiedTermGrade_GradeId()
        {
            Assert.AreEqual(_gradeId, _verifiedTermGradeEntity.GradeId);
        }

        [TestMethod]
        public void VerifiedTermGrade_GradeKey()
        {
            Assert.AreEqual(_gradeKey, _verifiedTermGradeEntity.GradeKey);
        }

        [TestMethod]
        public void VerifiedTermGrade_Grade()
        {
            Assert.AreEqual(_grade, _verifiedTermGradeEntity.Grade);
        }

        [TestMethod]
        public void VerifiedTermGrade_SubmittedOn()
        {
            Assert.AreEqual(_gradeTimestamp, _verifiedTermGradeEntity.SubmittedOn);
        }

        [TestMethod]
        public void VerifiedTermGrade_SubmittedBy()
        {
            Assert.AreEqual(_submittedBy, _verifiedTermGradeEntity.SubmittedBy);
        }

        [TestMethod]
        public void VerifiedTermGrade_GradeChangeReason()
        {
            Assert.AreEqual(_gradeChangeReason, _verifiedTermGradeEntity.GradeChangeReason);
        }

        [TestMethod]
        public void VerifiedTermGrade_GradeTypeCode()
        {
            Assert.AreEqual(_gradeTypeCode, _verifiedTermGradeEntity.GradeTypeCode);
        }
    }
}