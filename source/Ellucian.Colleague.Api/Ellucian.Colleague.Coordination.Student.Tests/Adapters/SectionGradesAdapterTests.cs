// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class SectionGradesAdapterTests
    {
        [TestMethod]
        public void SectionGradesAdapter_MapToType()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();
            var sectionGradesAdapter = new SectionGradesAdapter(adapterRegistry, loggerMock.Object);
            
            var sectionGradesDto = new Ellucian.Colleague.Dtos.Student.SectionGrades();
            sectionGradesDto.SectionId = "123";
            sectionGradesDto.StudentGrades = new List<StudentGrade>();
            var studentGrade = new Ellucian.Colleague.Dtos.Student.StudentGrade();
            studentGrade.StudentId = "789";
            studentGrade.MidtermGrade1 = "A";
            studentGrade.MidtermGrade2 = "B";
            studentGrade.MidtermGrade3 = "C";
            studentGrade.MidtermGrade4 = "D";
            studentGrade.MidtermGrade5 = "E";
            studentGrade.MidtermGrade6 = "F";
            studentGrade.FinalGrade = "G";
            studentGrade.NeverAttended = true;
            studentGrade.LastAttendanceDate = DateTime.Now;
            studentGrade.FinalGradeExpirationDate = DateTime.Now;
            sectionGradesDto.StudentGrades.Add(studentGrade);

            var sectionGradesEntity = sectionGradesAdapter.MapToType(sectionGradesDto);

            Assert.AreEqual(sectionGradesDto.SectionId, sectionGradesEntity.SectionId);
            Assert.AreEqual(sectionGradesDto.StudentGrades.Count(), sectionGradesEntity.StudentGrades.Count());
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].StudentId, sectionGradesEntity.StudentGrades[0].StudentId);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].MidtermGrade1, sectionGradesEntity.StudentGrades[0].MidtermGrade1);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].MidtermGrade2, sectionGradesEntity.StudentGrades[0].MidtermGrade2);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].MidtermGrade3, sectionGradesEntity.StudentGrades[0].MidtermGrade3);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].MidtermGrade4, sectionGradesEntity.StudentGrades[0].MidtermGrade4);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].MidtermGrade5, sectionGradesEntity.StudentGrades[0].MidtermGrade5);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].MidtermGrade6, sectionGradesEntity.StudentGrades[0].MidtermGrade6);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].FinalGrade, sectionGradesEntity.StudentGrades[0].FinalGrade);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].FinalGradeExpirationDate, sectionGradesEntity.StudentGrades[0].FinalGradeExpirationDate);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].NeverAttended, sectionGradesEntity.StudentGrades[0].NeverAttended);
            Assert.AreEqual(sectionGradesDto.StudentGrades[0].LastAttendanceDate, sectionGradesEntity.StudentGrades[0].LastAttendanceDate);
        }
    }
}
