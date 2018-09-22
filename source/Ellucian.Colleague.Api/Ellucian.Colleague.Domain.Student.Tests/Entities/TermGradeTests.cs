// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    // Tests for the TermGrade Entity
    public class TermGradeTests
    {
        private string _gradeId;
        private string _gradeKey;
        private string _grade;
        private DateTimeOffset? _gradeTimestamp;
        private string _submittedBy;
        private string _gradeChangeReason;
        private string _gradeTypeCode;

        Student.Entities.TermGrade _termGradeEntity;

        [TestInitialize]
        public void Initialize()
        {
            _gradeId = "1";
            _gradeKey = "14";
            _grade = "A";
            _gradeTimestamp = new DateTime(2016, 04, 01);
            _submittedBy = "0012297";
            _gradeChangeReason = "IC";
            _gradeTypeCode = "MID1";

            _termGradeEntity = new TermGrade(_gradeId, _gradeTimestamp, _submittedBy)
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
            _termGradeEntity = null;
        }

        [TestMethod]
        public void TermGrade_GradeId()
        {
            Assert.AreEqual(_gradeId, _termGradeEntity.GradeId);
        }

        [TestMethod]
        public void TermGrade_GradeKey()
        {
            Assert.AreEqual(_gradeKey, _termGradeEntity.GradeKey);
        }

        [TestMethod]
        public void TermGrade_Grade()
        {
            Assert.AreEqual(_grade, _termGradeEntity.Grade);
        }

        [TestMethod]
        public void TermGrade_SubmittedOn()
        {
            Assert.AreEqual(_gradeTimestamp, _termGradeEntity.SubmittedOn);
        }

        [TestMethod]
        public void TermGrade_SubmittedBy()
        {
            Assert.AreEqual(_submittedBy, _termGradeEntity.SubmittedBy);
        }

        [TestMethod]
        public void TermGrade_GradeChangeReason()
        {
            Assert.AreEqual(_gradeChangeReason, _termGradeEntity.GradeChangeReason);
        }

        [TestMethod]
        public void TermGrade_GradeTypeCode()
        {
            Assert.AreEqual(_gradeTypeCode, _termGradeEntity.GradeTypeCode);
        }
    }
}