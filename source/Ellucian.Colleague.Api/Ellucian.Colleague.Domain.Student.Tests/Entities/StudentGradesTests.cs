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
    // Tests for the StudentGrades Entity
    public class StudentGradesTests
    {
        private string studentId;
        private bool clearFinalGradeExpirationDateFlag;
        private bool clearLastAttendanceDateFlag;
        private DateTime effectiveStartDate;
        private DateTime effectiveEndDate;
        private string finalGrade;
        private DateTime finalGradeExpirationDate;
        private DateTime lastAttendanceDate;
        private string midtermGrade1;
        private string midtermGrade2;
        private string midtermGrade3;
        private string midtermGrade4;
        private string midtermGrade5;
        private string midtermGrade6;
        private bool neverAttended;

        Student.Entities.StudentGrade studentGradeEntity;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0000001";
            clearFinalGradeExpirationDateFlag = false;
            clearLastAttendanceDateFlag = false;
            effectiveEndDate = DateTime.Now;
            effectiveStartDate = DateTime.Now.AddDays(30);
            finalGrade = "14";
            finalGradeExpirationDate = DateTime.Now;
            lastAttendanceDate = DateTime.Now.AddDays(30);
            midtermGrade1 = "14";
            midtermGrade2 = "14";
            midtermGrade3 = "14";
            midtermGrade4 = "14";
            midtermGrade5 = "14";
            midtermGrade6 = "14";
            neverAttended = false;

            studentGradeEntity = new StudentGrade();
            studentGradeEntity.StudentId = studentId;
            studentGradeEntity.ClearFinalGradeExpirationDateFlag = clearFinalGradeExpirationDateFlag;
            studentGradeEntity.ClearLastAttendanceDateFlag = clearLastAttendanceDateFlag;
            studentGradeEntity.EffectiveEndDate = effectiveEndDate;
            studentGradeEntity.EffectiveStartDate = effectiveStartDate;
            studentGradeEntity.FinalGrade = finalGrade;
            studentGradeEntity.FinalGradeExpirationDate = finalGradeExpirationDate;
            studentGradeEntity.LastAttendanceDate = lastAttendanceDate;
            studentGradeEntity.MidtermGrade1 = midtermGrade1;
            studentGradeEntity.MidtermGrade2 = midtermGrade2;
            studentGradeEntity.MidtermGrade3 = midtermGrade3;
            studentGradeEntity.MidtermGrade4 = midtermGrade4;
            studentGradeEntity.MidtermGrade5 = midtermGrade5;
            studentGradeEntity.MidtermGrade6 = midtermGrade6;
            studentGradeEntity.NeverAttended = neverAttended;
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentGradeEntity = null;
        }

        [TestMethod]
        public void StudentGradeEntity_StudentId()
        {
            Assert.AreEqual(studentId, studentGradeEntity.StudentId);
        }

        [TestMethod]
        public void StudentGradeEntity_ClearFinalGradeExpirationDateFlag()
        {
            Assert.AreEqual(clearFinalGradeExpirationDateFlag, studentGradeEntity.ClearFinalGradeExpirationDateFlag);
        }

        [TestMethod]
        public void StudentGradeEntity_ClearLastAttendanceDateFlag()
        {
            Assert.AreEqual(clearLastAttendanceDateFlag, studentGradeEntity.ClearLastAttendanceDateFlag);
        }

        [TestMethod]
        public void StudentGradeEntity_EffectiveEndDate()
        {
            Assert.AreEqual(effectiveEndDate, studentGradeEntity.EffectiveEndDate);
        }

        [TestMethod]
        public void StudentGradeEntity_EffectiveStartDate()
        {
            Assert.AreEqual(effectiveStartDate, studentGradeEntity.EffectiveStartDate);
        }

        [TestMethod]
        public void StudentGradeEntity_FinalGrade()
        {
            Assert.AreEqual(finalGrade, studentGradeEntity.FinalGrade);
        }

        [TestMethod]
        public void StudentGradeEntity_FinalGradeExpirationDate()
        {
            Assert.AreEqual(finalGradeExpirationDate, studentGradeEntity.FinalGradeExpirationDate);
        }

        [TestMethod]
        public void StudentGradeEntity_LastAttendanceDate()
        {
            Assert.AreEqual(lastAttendanceDate, studentGradeEntity.LastAttendanceDate);
        }

        [TestMethod]
        public void StudentGradeEntity_MidtermGrade1()
        {
            Assert.AreEqual(midtermGrade1, studentGradeEntity.MidtermGrade1);
        }

        [TestMethod]
        public void StudentGradeEntity_MidtermGrade2()
        {
            Assert.AreEqual(midtermGrade2, studentGradeEntity.MidtermGrade2);
        }

        [TestMethod]
        public void StudentGradeEntity_MidtermGrade3()
        {
            Assert.AreEqual(midtermGrade3, studentGradeEntity.MidtermGrade3);
        }
        [TestMethod]
        public void StudentGradeEntity_MidtermGrade4()
        {
            Assert.AreEqual(midtermGrade4, studentGradeEntity.MidtermGrade4);
        }

        [TestMethod]
        public void StudentGradeEntity_MidtermGrade5()
        {
            Assert.AreEqual(midtermGrade5, studentGradeEntity.MidtermGrade5);
        }

        [TestMethod]
        public void StudentGradeEntity_MidtermGrade6()
        {
            Assert.AreEqual(midtermGrade6, studentGradeEntity.MidtermGrade6);
        }

        [TestMethod]
        public void StudentGradeEntity_NeverAttended()
        {
            Assert.AreEqual(neverAttended, studentGradeEntity.NeverAttended);
        }

    }
}