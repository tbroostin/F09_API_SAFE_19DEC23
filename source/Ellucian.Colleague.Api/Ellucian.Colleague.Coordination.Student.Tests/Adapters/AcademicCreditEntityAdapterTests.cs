// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    [TestClass]
    public class AcademicCreditEntityToAcademicCreditDtoAdapterTests
    {
        [TestMethod]
        public void AcademicCreditAdapter_MapToType()
        {
            // ARRANGE
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();
            
            // mock up various adapters
            var academicCreditAdapter = new AcademicCreditEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>()).Returns(academicCreditAdapter);

            var midtermGradeAdapter = new MidTermGradeEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(midtermGradeAdapter);

            var gradingTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>()).Returns(gradingTypeAdapter);

            var replacedStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>()).Returns(replacedStatusAdapter);

            var replacementStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>()).Returns(replacementStatusAdapter);
            
            // Data required to create an academic credit domain entity
            string id = "1";
            string courseId = "111";
            string sectionId = "11111";
            var verifiedGrade = new Ellucian.Colleague.Domain.Student.Entities.Grade("B", "B", "UG");
            var course = new Ellucian.Colleague.Domain.Student.Entities.Course(courseId, "Introduction to Art", null, new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, "ART", "100", "UG", new List<string>() { "Type" }, 3.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) { Status = Ellucian.Colleague.Domain.Student.Entities.CourseStatus.Active }});

            // Create the domain entity to be converted
            var academicCreditEntity = new Ellucian.Colleague.Domain.Student.Entities.AcademicCredit(id, course, sectionId);
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
            academicCreditEntity.VerifiedGradeTimestamp = new DateTime(2014, 06, 12, 08, 50, 22);

            
            // ACT -- Map the domain entity to DTO
            var academicCreditDto = academicCreditAdapter.MapToType(academicCreditEntity);

            // ASSERT
            Assert.AreEqual(academicCreditEntity.AdjustedCredit, academicCreditDto.AdjustedCredit);
            Assert.AreEqual(academicCreditEntity.CompletedCredit, academicCreditDto.CompletedCredit);
            Assert.AreEqual(academicCreditEntity.ContinuingEducationUnits, academicCreditDto.ContinuingEducationUnits);
            Assert.AreEqual(academicCreditEntity.Course.Id, academicCreditDto.CourseId);
            Assert.AreEqual(academicCreditEntity.CourseName, academicCreditDto.CourseName);
            Assert.AreEqual(2, academicCreditDto.MidTermGrades.Count());
            Assert.AreEqual(academicCreditEntity.SectionId, academicCreditDto.SectionId);
            Assert.AreEqual(academicCreditEntity.SectionNumber, academicCreditDto.SectionNumber);
            Assert.AreEqual(academicCreditEntity.Title, academicCreditDto.Title);
            Assert.AreEqual(academicCreditEntity.VerifiedGrade.Id, academicCreditDto.VerifiedGradeId);
            Assert.AreEqual(academicCreditEntity.Credit, academicCreditDto.Credit);
            Assert.AreEqual(academicCreditEntity.GpaCredit, academicCreditDto.GpaCredit);
            Assert.AreEqual(academicCreditEntity.GradePoints, academicCreditDto.GradePoints);
            Assert.AreEqual(academicCreditEntity.Status.ToString(), academicCreditDto.Status);
            Assert.AreEqual(academicCreditEntity.IsNonCourse, academicCreditDto.IsNonCourse);
            Assert.IsTrue(academicCreditEntity.IsCompletedCredit);
            Assert.AreEqual(academicCreditEntity.IsCompletedCredit, academicCreditDto.IsCompletedCredit);
            Assert.IsNotNull(academicCreditDto.VerifiedGradeTimestamp);
            Assert.AreEqual(academicCreditEntity.GradingType.ToString(), academicCreditDto.GradingType.ToString());
            Assert.AreEqual(academicCreditEntity.HasVerifiedGrade, academicCreditDto.HasVerifiedGrade);
            Assert.AreEqual(academicCreditEntity.Id, academicCreditDto.Id);
            Assert.AreEqual(academicCreditEntity.ReplacedStatus.ToString(), academicCreditDto.ReplacedStatus.ToString());
            Assert.AreEqual(academicCreditEntity.TermCode, academicCreditDto.TermCode);
        }

        [TestMethod]
        public void AcademicCreditAdapter_MapToType_AdjustedCredit_Null()
        {
            // ARRANGE
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();

            // mock up various adapters
            var academicCreditAdapter = new AcademicCreditEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>()).Returns(academicCreditAdapter);

            var midtermGradeAdapter = new MidTermGradeEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(midtermGradeAdapter);

            var gradingTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>()).Returns(gradingTypeAdapter);

            var replacedStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>()).Returns(replacedStatusAdapter);

            var replacementStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>()).Returns(replacementStatusAdapter);

            // Data required to create an academic credit domain entity
            string id = "1";
            string courseId = "111";
            string sectionId = "11111";
            var verifiedGrade = new Ellucian.Colleague.Domain.Student.Entities.Grade("B", "B", "UG");
            var course = new Ellucian.Colleague.Domain.Student.Entities.Course(courseId, "Introduction to Art", null, new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, "ART", "100", "UG", new List<string>() { "Type" }, 3.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) { Status = Ellucian.Colleague.Domain.Student.Entities.CourseStatus.Active } });

            // Create the domain entity to be converted
            var academicCreditEntity = new Ellucian.Colleague.Domain.Student.Entities.AcademicCredit(id, course, sectionId);
            academicCreditEntity.AdjustedCredit = null;
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
            academicCreditEntity.VerifiedGradeTimestamp = new DateTime(2014, 06, 12, 08, 50, 22);


            // ACT -- Map the domain entity to DTO
            var academicCreditDto = academicCreditAdapter.MapToType(academicCreditEntity);

            // ASSERT
            Assert.AreEqual(0m, academicCreditDto.AdjustedCredit);
            Assert.AreEqual(academicCreditEntity.CompletedCredit, academicCreditDto.CompletedCredit);
            Assert.AreEqual(academicCreditEntity.ContinuingEducationUnits, academicCreditDto.ContinuingEducationUnits);
            Assert.AreEqual(academicCreditEntity.Course.Id, academicCreditDto.CourseId);
            Assert.AreEqual(academicCreditEntity.CourseName, academicCreditDto.CourseName);
            Assert.AreEqual(2, academicCreditDto.MidTermGrades.Count());
            Assert.AreEqual(academicCreditEntity.SectionId, academicCreditDto.SectionId);
            Assert.AreEqual(academicCreditEntity.SectionNumber, academicCreditDto.SectionNumber);
            Assert.AreEqual(academicCreditEntity.Title, academicCreditDto.Title);
            Assert.AreEqual(academicCreditEntity.VerifiedGrade.Id, academicCreditDto.VerifiedGradeId);
            Assert.AreEqual(academicCreditEntity.Credit, academicCreditDto.Credit);
            Assert.AreEqual(academicCreditEntity.GpaCredit, academicCreditDto.GpaCredit);
            Assert.AreEqual(academicCreditEntity.GradePoints, academicCreditDto.GradePoints);
            Assert.AreEqual(academicCreditEntity.Status.ToString(), academicCreditDto.Status);
            Assert.AreEqual(academicCreditEntity.IsNonCourse, academicCreditDto.IsNonCourse);
            Assert.IsTrue(academicCreditEntity.IsCompletedCredit);
            Assert.AreEqual(academicCreditEntity.IsCompletedCredit, academicCreditDto.IsCompletedCredit);
            Assert.IsNotNull(academicCreditDto.VerifiedGradeTimestamp);
            Assert.AreEqual(academicCreditEntity.GradingType.ToString(), academicCreditDto.GradingType.ToString());
            Assert.AreEqual(academicCreditEntity.HasVerifiedGrade, academicCreditDto.HasVerifiedGrade);
            Assert.AreEqual(academicCreditEntity.Id, academicCreditDto.Id);
            Assert.AreEqual(academicCreditEntity.ReplacedStatus.ToString(), academicCreditDto.ReplacedStatus.ToString());
            Assert.AreEqual(academicCreditEntity.TermCode, academicCreditDto.TermCode);
        }

        [TestMethod]
        public void AcademicCreditAdapter_MapToType_GPACredit_Null()
        {
            // ARRANGE
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();

            // mock up various adapters
            var academicCreditAdapter = new AcademicCreditEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>()).Returns(academicCreditAdapter);

            var midtermGradeAdapter = new MidTermGradeEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(midtermGradeAdapter);

            var gradingTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>()).Returns(gradingTypeAdapter);

            var replacedStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>()).Returns(replacedStatusAdapter);

            var replacementStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>()).Returns(replacementStatusAdapter);

            // Data required to create an academic credit domain entity
            string id = "1";
            string courseId = "111";
            string sectionId = "11111";
            var verifiedGrade = new Ellucian.Colleague.Domain.Student.Entities.Grade("B", "B", "UG");
            var course = new Ellucian.Colleague.Domain.Student.Entities.Course(courseId, "Introduction to Art", null, new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, "ART", "100", "UG", new List<string>() { "Type" }, 3.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) { Status = Ellucian.Colleague.Domain.Student.Entities.CourseStatus.Active } });

            // Create the domain entity to be converted
            var academicCreditEntity = new Ellucian.Colleague.Domain.Student.Entities.AcademicCredit(id, course, sectionId);
            academicCreditEntity.AdjustedCredit = 2.0m;
            academicCreditEntity.CompletedCredit = 1.0m;
            academicCreditEntity.ContinuingEducationUnits = 3;
            academicCreditEntity.GradingType = Ellucian.Colleague.Domain.Student.Entities.GradingType.PassFail;
            academicCreditEntity.CourseName = "ART*100";
            academicCreditEntity.Title = "Title Override";
            academicCreditEntity.Credit = 3.0m;
            academicCreditEntity.GpaCredit =null;
            academicCreditEntity.GradePoints = 9.0m;
            academicCreditEntity.ContinuingEducationUnits = 2.0m;
            academicCreditEntity.Status = Ellucian.Colleague.Domain.Student.Entities.CreditStatus.Add;
            academicCreditEntity.SectionNumber = "99";
            academicCreditEntity.VerifiedGrade = verifiedGrade;
            academicCreditEntity.IsNonCourse = true;
            academicCreditEntity.AddMidTermGrade(new Domain.Student.Entities.MidTermGrade(1, "1", DateTimeOffset.Now));
            academicCreditEntity.AddMidTermGrade(new Domain.Student.Entities.MidTermGrade(2, "2", DateTimeOffset.Now));
            academicCreditEntity.VerifiedGradeTimestamp = new DateTime(2014, 06, 12, 08, 50, 22);


            // ACT -- Map the domain entity to DTO
            var academicCreditDto = academicCreditAdapter.MapToType(academicCreditEntity);

            // ASSERT
            Assert.AreEqual(academicCreditEntity.AdjustedCredit, academicCreditDto.AdjustedCredit);
            Assert.AreEqual(academicCreditEntity.CompletedCredit, academicCreditDto.CompletedCredit);
            Assert.AreEqual(academicCreditEntity.ContinuingEducationUnits, academicCreditDto.ContinuingEducationUnits);
            Assert.AreEqual(academicCreditEntity.Course.Id, academicCreditDto.CourseId);
            Assert.AreEqual(academicCreditEntity.CourseName, academicCreditDto.CourseName);
            Assert.AreEqual(2, academicCreditDto.MidTermGrades.Count());
            Assert.AreEqual(academicCreditEntity.SectionId, academicCreditDto.SectionId);
            Assert.AreEqual(academicCreditEntity.SectionNumber, academicCreditDto.SectionNumber);
            Assert.AreEqual(academicCreditEntity.Title, academicCreditDto.Title);
            Assert.AreEqual(academicCreditEntity.VerifiedGrade.Id, academicCreditDto.VerifiedGradeId);
            Assert.AreEqual(academicCreditEntity.Credit, academicCreditDto.Credit);
            Assert.AreEqual(0m, academicCreditDto.GpaCredit);
            Assert.AreEqual(academicCreditEntity.GradePoints, academicCreditDto.GradePoints);
            Assert.AreEqual(academicCreditEntity.Status.ToString(), academicCreditDto.Status);
            Assert.AreEqual(academicCreditEntity.IsNonCourse, academicCreditDto.IsNonCourse);
            Assert.IsTrue(academicCreditEntity.IsCompletedCredit);
            Assert.AreEqual(academicCreditEntity.IsCompletedCredit, academicCreditDto.IsCompletedCredit);
            Assert.IsNotNull(academicCreditDto.VerifiedGradeTimestamp);
            Assert.AreEqual(academicCreditEntity.GradingType.ToString(), academicCreditDto.GradingType.ToString());
            Assert.AreEqual(academicCreditEntity.HasVerifiedGrade, academicCreditDto.HasVerifiedGrade);
            Assert.AreEqual(academicCreditEntity.Id, academicCreditDto.Id);
            Assert.AreEqual(academicCreditEntity.ReplacedStatus.ToString(), academicCreditDto.ReplacedStatus.ToString());
            Assert.AreEqual(academicCreditEntity.TermCode, academicCreditDto.TermCode);
        }

        [TestMethod]
        public void AcademicCreditAdapter_MapToType_CompletedCredit_Null()
        {
            // ARRANGE
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();

            // mock up various adapters
            var academicCreditAdapter = new AcademicCreditEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>()).Returns(academicCreditAdapter);

            var midtermGradeAdapter = new MidTermGradeEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(midtermGradeAdapter);

            var gradingTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>()).Returns(gradingTypeAdapter);

            var replacedStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>()).Returns(replacedStatusAdapter);

            var replacementStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>()).Returns(replacementStatusAdapter);

            // Data required to create an academic credit domain entity
            string id = "1";
            string courseId = "111";
            string sectionId = "11111";
            var verifiedGrade = new Ellucian.Colleague.Domain.Student.Entities.Grade("B", "B", "UG");
            var course = new Ellucian.Colleague.Domain.Student.Entities.Course(courseId, "Introduction to Art", null, new List<OfferingDepartment>() { new OfferingDepartment("ART", 100m) }, "ART", "100", "UG", new List<string>() { "Type" }, 3.0m, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) { Status = Ellucian.Colleague.Domain.Student.Entities.CourseStatus.Active } });

            // Create the domain entity to be converted
            var academicCreditEntity = new Ellucian.Colleague.Domain.Student.Entities.AcademicCredit(id, course, sectionId);
            academicCreditEntity.AdjustedCredit = 2.0m;
            academicCreditEntity.CompletedCredit =null;
            academicCreditEntity.ContinuingEducationUnits = 3;
            academicCreditEntity.GradingType = Ellucian.Colleague.Domain.Student.Entities.GradingType.PassFail;
            academicCreditEntity.CourseName = "ART*100";
            academicCreditEntity.Title = "Title Override";
            academicCreditEntity.Credit = 3.0m;
            academicCreditEntity.GpaCredit = 1.0m;
            academicCreditEntity.GradePoints = 9.0m;
            academicCreditEntity.ContinuingEducationUnits = 2.0m;
            academicCreditEntity.Status = Ellucian.Colleague.Domain.Student.Entities.CreditStatus.Add;
            academicCreditEntity.SectionNumber = "99";
            academicCreditEntity.VerifiedGrade = verifiedGrade;
            academicCreditEntity.IsNonCourse = true;
            academicCreditEntity.AddMidTermGrade(new Domain.Student.Entities.MidTermGrade(1, "1", DateTimeOffset.Now));
            academicCreditEntity.AddMidTermGrade(new Domain.Student.Entities.MidTermGrade(2, "2", DateTimeOffset.Now));
            academicCreditEntity.VerifiedGradeTimestamp = new DateTime(2014, 06, 12, 08, 50, 22);


            // ACT -- Map the domain entity to DTO
            var academicCreditDto = academicCreditAdapter.MapToType(academicCreditEntity);

            // ASSERT
            Assert.AreEqual(academicCreditEntity.AdjustedCredit, academicCreditDto.AdjustedCredit);
            Assert.AreEqual(0m, academicCreditDto.CompletedCredit);
            Assert.AreEqual(academicCreditEntity.ContinuingEducationUnits, academicCreditDto.ContinuingEducationUnits);
            Assert.AreEqual(academicCreditEntity.Course.Id, academicCreditDto.CourseId);
            Assert.AreEqual(academicCreditEntity.CourseName, academicCreditDto.CourseName);
            Assert.AreEqual(2, academicCreditDto.MidTermGrades.Count());
            Assert.AreEqual(academicCreditEntity.SectionId, academicCreditDto.SectionId);
            Assert.AreEqual(academicCreditEntity.SectionNumber, academicCreditDto.SectionNumber);
            Assert.AreEqual(academicCreditEntity.Title, academicCreditDto.Title);
            Assert.AreEqual(academicCreditEntity.VerifiedGrade.Id, academicCreditDto.VerifiedGradeId);
            Assert.AreEqual(academicCreditEntity.Credit, academicCreditDto.Credit);
            Assert.AreEqual(academicCreditEntity.GpaCredit, academicCreditDto.GpaCredit);
            Assert.AreEqual(academicCreditEntity.GradePoints, academicCreditDto.GradePoints);
            Assert.AreEqual(academicCreditEntity.Status.ToString(), academicCreditDto.Status);
            Assert.AreEqual(academicCreditEntity.IsNonCourse, academicCreditDto.IsNonCourse);
            Assert.IsTrue(academicCreditEntity.IsCompletedCredit);
            Assert.AreEqual(academicCreditEntity.IsCompletedCredit, academicCreditDto.IsCompletedCredit);
            Assert.IsNotNull(academicCreditDto.VerifiedGradeTimestamp);
            Assert.AreEqual(academicCreditEntity.GradingType.ToString(), academicCreditDto.GradingType.ToString());
            Assert.AreEqual(academicCreditEntity.HasVerifiedGrade, academicCreditDto.HasVerifiedGrade);
            Assert.AreEqual(academicCreditEntity.Id, academicCreditDto.Id);
            Assert.AreEqual(academicCreditEntity.ReplacedStatus.ToString(), academicCreditDto.ReplacedStatus.ToString());
            Assert.AreEqual(academicCreditEntity.TermCode, academicCreditDto.TermCode);
        }
    }
}
