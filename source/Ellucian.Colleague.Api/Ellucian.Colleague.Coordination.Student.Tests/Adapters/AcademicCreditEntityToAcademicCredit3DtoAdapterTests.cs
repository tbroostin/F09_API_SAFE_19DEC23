// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    [TestClass]
    public class AcademicCreditEntityToAcademicCredit3DtoAdapterTests
    {
        Ellucian.Colleague.Domain.Student.Entities.AcademicCredit academicCreditEntity;
        AcademicCredit3 academicCreditDto;
        AcademicCreditEntityToAcademicCredit3DtoAdapter adapter;
        IAdapterRegistry adapterRegistry;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();

            // Data required to create an academic credit domain entity
            string id = "1";
            string courseId = "111";
            string sectionId = "11111";
            var verifiedGrade = new Ellucian.Colleague.Domain.Student.Entities.Grade("B", "B", "UG");
            var course = new Ellucian.Colleague.Domain.Student.Entities.Course(courseId, "Introduction to Art", null, new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, "ART", "100", "UG", new List<string>() { "Type" }, 3.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) { Status = Ellucian.Colleague.Domain.Student.Entities.CourseStatus.Active } });

            // Create the domain entity to be converted
            academicCreditEntity = new Ellucian.Colleague.Domain.Student.Entities.AcademicCredit(id, course, sectionId);
            academicCreditEntity.AdjustedCredit = .5m;
            academicCreditEntity.CompletedCredit = 1.0m;
            academicCreditEntity.ContinuingEducationUnits = 3;
            academicCreditEntity.GradingType = Ellucian.Colleague.Domain.Student.Entities.GradingType.PassFail;
            academicCreditEntity.CourseName = "ART*100";
            academicCreditEntity.Title = "Title Override";
            academicCreditEntity.Credit = 3.0m;
            academicCreditEntity.GpaCredit = 3.0m;
            academicCreditEntity.GradePoints = 9.0m;
            academicCreditEntity.ContinuingEducationUnits = 2.0m;
            academicCreditEntity.Status = Ellucian.Colleague.Domain.Student.Entities.CreditStatus.Add;
            academicCreditEntity.SectionNumber = "99";
            academicCreditEntity.VerifiedGrade = verifiedGrade;
            academicCreditEntity.IsNonCourse = true;
            academicCreditEntity.AddMidTermGrade(new Domain.Student.Entities.MidTermGrade(1, "1", DateTimeOffset.Now));
            academicCreditEntity.AddMidTermGrade(new Domain.Student.Entities.MidTermGrade(2, "2", DateTimeOffset.Now));

            // Map the domain entity to DTO
            adapter = new AcademicCreditEntityToAcademicCredit3DtoAdapter(adapterRegistry, loggerMock.Object);
            academicCreditDto = adapter.MapToType(academicCreditEntity);
        }

        [TestMethod]
        public void AcademicCreditDtoAdapter_CourseId()
        {
            Assert.AreEqual(academicCreditEntity.Course.Id, academicCreditDto.CourseId);
        }

        [TestMethod]
        public void AcademicCreditDtoAdapter_VerifiedGradeId()
        {
            Assert.AreEqual(academicCreditEntity.VerifiedGrade.Id, academicCreditDto.VerifiedGradeId);
        }

        [TestMethod]
        public void AcademicCreditDtoAdapter_Standard_Mapping()
        {
            Assert.AreEqual(academicCreditEntity.AdjustedCredit, academicCreditDto.AdjustedCredit);
            Assert.AreEqual(academicCreditEntity.CompletedCredit, academicCreditDto.CompletedCredit);
            Assert.AreEqual(academicCreditEntity.ContinuingEducationUnits, academicCreditDto.ContinuingEducationUnits);
            Assert.AreEqual(academicCreditEntity.GradingType.ToString(), academicCreditDto.GradingType.ToString());
            Assert.AreEqual(academicCreditEntity.CourseName, academicCreditDto.CourseName);
            Assert.AreEqual(academicCreditEntity.Title, academicCreditDto.Title);
            Assert.AreEqual(academicCreditEntity.Credit, academicCreditDto.Credit);
            Assert.AreEqual(academicCreditEntity.GpaCredit, academicCreditDto.GpaCredit);
            Assert.AreEqual(academicCreditEntity.GradePoints, academicCreditDto.GradePoints);
            Assert.AreEqual(academicCreditEntity.ContinuingEducationUnits, academicCreditDto.ContinuingEducationUnits);
            Assert.AreEqual(academicCreditEntity.Status.ToString(), academicCreditDto.Status.ToString());
            Assert.AreEqual(academicCreditEntity.SectionNumber, academicCreditDto.SectionNumber);
            Assert.AreEqual(academicCreditEntity.IsNonCourse, academicCreditDto.IsNonCourse);
            Assert.AreEqual(academicCreditEntity.ContinuingEducationUnits, academicCreditDto.ContinuingEducationUnits);
            Assert.AreEqual(academicCreditEntity.MidTermGrades.Count, academicCreditDto.MidTermGrades.Count);
        }
    }
}

