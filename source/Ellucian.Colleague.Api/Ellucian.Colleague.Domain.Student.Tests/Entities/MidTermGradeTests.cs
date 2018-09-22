// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    // Tests for the MidTermGrade Entity
    public class MidTermGradeTests
    {
        private int _position;
        private string _gradeId;
        private string _gradeKey;
        private string _grade;
        private DateTimeOffset? _gradeTimestamp;
        private string _submittedBy;
        private string _gradeChangeReason;
        private string _gradeTypeCode;

        Student.Entities.MidTermGrade _midTermGradeEntity;

        [TestInitialize]
        public void Initialize()
        {
            _position = 1;
            _gradeId = "1";
            _gradeKey = "14";
            _grade = "A";
            _gradeTimestamp = new DateTime(2016, 04, 01);
            _submittedBy = "0012297";
            _gradeChangeReason = "IC";
            _gradeTypeCode = "MID1";

            _midTermGradeEntity = new MidTermGrade(_position, _gradeId, _gradeTimestamp, _submittedBy)
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
            _midTermGradeEntity = null;
        }

        [TestMethod]
        public void MidTermGrade_Position()
        {
            Assert.AreEqual(_position, _midTermGradeEntity.Position);
        }

        [TestMethod]
        public void MidTermGrade_GradeId()
        {
            Assert.AreEqual(_gradeId, _midTermGradeEntity.GradeId);
        }

        [TestMethod]
        public void MidTermGrade_GradeKey()
        {
            Assert.AreEqual(_gradeKey, _midTermGradeEntity.GradeKey);
        }

        [TestMethod]
        public void MidTermGrade_Grade()
        {
            Assert.AreEqual(_grade, _midTermGradeEntity.Grade);
        }

        [TestMethod]
        public void MidTermGrade_GradeTimestamp()
        {
            Assert.AreEqual(_gradeTimestamp, _midTermGradeEntity.GradeTimestamp);
        }

        [TestMethod]
        public void MidTermGrade_SubmittedBy()
        {
            Assert.AreEqual(_submittedBy, _midTermGradeEntity.SubmittedBy);
        }

        [TestMethod]
        public void MidTermGrade_GradeChangeReason()
        {
            Assert.AreEqual(_gradeChangeReason, _midTermGradeEntity.GradeChangeReason);
        }

        [TestMethod]
        public void MidTermGrade_GradeTypeCode()
        {
            Assert.AreEqual(_gradeTypeCode, _midTermGradeEntity.GradeTypeCode);
        }
    }
}