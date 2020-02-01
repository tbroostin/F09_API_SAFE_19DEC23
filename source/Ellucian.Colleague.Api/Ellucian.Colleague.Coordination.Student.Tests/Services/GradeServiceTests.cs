// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class GradeServiceTests
    {
        [TestClass]
        public class GetGrade : StudentUserFactory
        {
            protected Domain.Entities.Role viewStudent = new Domain.Entities.Role(1, "VIEW.STUDENT.INFORMATION");

            private Mock<IStudentReferenceDataRepository> studentReferenceRepositoryMock;
            private Mock<IGradeRepository> gradeRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            
            private GradeUser currentUserFactoryMock;

            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IAcademicCreditRepository> academicCreditRepositoryMock;
            private IAcademicCreditRepository academicCreditRepository;
            private ILogger logger;
            private GradeService gradeService;
            private ICollection<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> gradeSchemeCollection;
            private ICollection<Ellucian.Colleague.Domain.Student.Entities.Grade> gradeCollection;
            private ICollection<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> acadLevelCollection;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private string GradeID = "d874e05d-9d97-4fa3-8862-5044ef2384d0";

            [TestInitialize]
            public async void Initialize()
            {
                studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                gradeRepositoryMock = new Mock<IGradeRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                
                currentUserFactoryMock = new GradeUser();
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                academicCreditRepositoryMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepository = academicCreditRepositoryMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;
            
                gradeSchemeCollection = (await new TestStudentReferenceDataRepository().GetGradeSchemesAsync()).ToList();
                acadLevelCollection = (await new TestAcademicLevelRepository().GetAsync()).ToList();
                gradeCollection = (await new TestGradeRepository().GetHedmAsync()).ToList();
                               
                gradeService = new GradeService(gradeRepositoryMock.Object, studentReferenceRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactoryMock, roleRepositoryMock.Object, logger, studentRepositoryMock.Object, academicCreditRepository, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                gradeCollection = null;
                acadLevelCollection = null;
                gradeSchemeCollection = null;
                studentReferenceRepositoryMock = null;
                gradeRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                currentUserFactoryMock = null;
                logger = null;
                gradeService = null;               
            }

            #region grades
            [TestMethod]
            public async Task GradeService__Grades()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);
                var results = await gradeService.GetAsync();
                Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.Grade>); 
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task GradeService_Grades_Count()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);
                
                var results = await gradeService.GetAsync();
                Assert.AreEqual(16, results.Count());
            }

            [TestMethod]
            public async Task GradeService__Grades_Properties()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);
                var results = (await gradeService.GetAsync()).ToList();
                Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.Grade>);
                Assert.IsNotNull(results);

                for (var i = 0; i < results.Count(); i++)
                {
                    var expectedResult = gradeCollection.ToList()[i];
                    var grade = results[i]; 
                  
                    var expectedGradeScheme = gradeSchemeCollection.FirstOrDefault(x => x.Code == expectedResult.GradeSchemeCode);
                 
                    Assert.IsNotNull(grade.Id);
                    Assert.AreEqual(expectedResult.Guid, grade.Id);
                    if ((grade.GradeScheme != null) && (grade.GradeScheme.Id != null))
                        Assert.AreEqual(expectedGradeScheme.Guid, grade.GradeScheme.Id);
                    if (grade.GradeItem != null)
                    {
                        Assert.AreEqual(Dtos.GradeItemType.Literal, grade.GradeItem.GradeItemType);
                        Assert.AreEqual(expectedResult.LetterGrade, grade.GradeItem.GradeValue);
                    }
                    Assert.AreEqual(null, grade.EquivalentTo);
                    switch (expectedResult.Credit)
                    {
                        case "Y":
                            Assert.AreEqual(Dtos.GradeCmplCreditType.Full, grade.GradeCmplCreditType); break;
                        case "N":
                            Assert.AreEqual(Dtos.GradeCmplCreditType.None, grade.GradeCmplCreditType); break;
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GradeService_GetPilotGradesAsync_PermissionsException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudent });
                var students = new List<string>() { "Student1", "Student2" };
                await gradeService.GetPilotGradesAsync(students, "TestTerm");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeService_Grade_NullLogger()
            {
                new GradeService(gradeRepositoryMock.Object, studentReferenceRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactoryMock, roleRepositoryMock.Object, null, studentRepositoryMock.Object, academicCreditRepository, baseConfigurationRepository);
            }
                
            [TestMethod]
            [ExpectedException(typeof(NullReferenceException))]
            public async Task GradeService_GetGradeById_Empty()
            {
                await gradeService.GetGradeByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(NullReferenceException))]
            public async Task GradeService_GetGradeById_Null()
            {
                await gradeService.GetGradeByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GradeService_GetGradeById_InvalidId()
            {
                gradeRepositoryMock.Setup(repo => repo.GetHedmGradeByIdAsync("99")).Throws<InvalidOperationException>();

                await gradeService.GetGradeByIdAsync("99");
            }

            [TestMethod]
            public async Task GradeService_GetGradeById_Expected()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);

                var expectedResults = gradeCollection.Where(c => c.Guid == GradeID).FirstOrDefault();
                var x = gradeCollection.Where(g => g.Guid == GradeID).FirstOrDefault();
                gradeRepositoryMock.Setup(repo => repo.GetHedmGradeByIdAsync(It.IsAny<string>())).ReturnsAsync(x);

                var grade = await gradeService.GetGradeByIdAsync(GradeID);
                Assert.AreEqual(expectedResults.Guid, grade.Id);
            }

            [TestMethod]
            public async Task GradeService_GetGradeById_Properties()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);

                var expectedResults = gradeCollection.Where(g => g.Guid == GradeID).FirstOrDefault();
                gradeRepositoryMock.Setup(repo => repo.GetHedmGradeByIdAsync(It.IsAny<string>())).ReturnsAsync(expectedResults);

                var expectedGradeScheme = gradeSchemeCollection.FirstOrDefault(x => x.Code == expectedResults.GradeSchemeCode);

                var grade = await gradeService.GetGradeByIdAsync(GradeID);
                Assert.IsNotNull(grade.Id);
                Assert.AreEqual(expectedResults.Guid, grade.Id);
                Assert.AreEqual(expectedGradeScheme.Guid, grade.GradeScheme.Id);
                Assert.AreEqual(Dtos.GradeItemType.Literal, grade.GradeItem.GradeItemType);
                Assert.AreEqual(expectedResults.LetterGrade, grade.GradeItem.GradeValue);
                Assert.AreEqual(null, grade.EquivalentTo);
                switch (expectedResults.Credit)
                {
                    case "Y":
                        Assert.AreEqual(Dtos.GradeCmplCreditType.Full, grade.GradeCmplCreditType); break;
                    case "N":
                        Assert.AreEqual(Dtos.GradeCmplCreditType.None, grade.GradeCmplCreditType); break;
                }
            }

            [TestMethod]
            public async Task GradeService_GetPilotGradesAsync()
            {
                var credits = new Dictionary<string, List<PilotAcademicCredit>>();
                var grades = new List<Domain.Student.Entities.Grade>();
                var students = new List<string>() { "Student1", "Student2" };
                foreach (var student in students)
                {
                    var creditList = new List<PilotAcademicCredit>();
                    var credit = new PilotAcademicCredit(student + "Credit") { VerifiedGradeId = student + "VerifiedId", HasVerifiedGrade = true, SectionId = student + "Section" };
                    credit.AddMidTermGrade(new MidTermGrade(1, student + "MidtermId", null));
                    if (student == "Student2")
                        credit.SectionId = null;
                    creditList.Add(credit);
                    credits.Add(student, creditList);
                    var midterm = new Domain.Student.Entities.Grade(student + "MidtermId", "F", "mt", string.Empty) { GradePriority = 1 };
                    var verified = new Domain.Student.Entities.Grade(student + "VerifiedId", "A", "v", string.Empty) { GradePriority = 2 };
                    grades.Add(midterm);
                    grades.Add(verified);
                }
                academicCreditRepositoryMock.Setup(repo => repo.GetPilotAcademicCreditsByStudentIdsAsync(It.IsAny<IEnumerable<string>>(), AcademicCreditDataSubset.StudentCourseSec, false, true, It.IsAny<string>())).Returns(Task.FromResult<Dictionary<string, List<PilotAcademicCredit>>>(credits));
                gradeRepositoryMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult<ICollection<Domain.Student.Entities.Grade>>((ICollection<Domain.Student.Entities.Grade>)grades));
                
                viewStudent.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudent });
                var result = await gradeService.GetPilotGradesAsync(students, "TestTerm");
                Assert.AreEqual(2, result.Count()); // Student2's grades should not be returned, grades with no section id should be filtered out.
                foreach (var grade in result)
                {
                    Assert.AreEqual("Student1", grade.StudentId);
                    Assert.IsNotNull(grade.SectionId);
                }
            }

            #endregion

            #region grades-definitions-maximum
            [TestMethod]
            public async Task GradeService_GradesDefinitionsMaximum_Grades()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(acadLevelCollection);
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);
                var results = await gradeService.GetGradesDefinitionsMaximumAsync();
                Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.GradeDefinitionsMaximum>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task GradeService_GradesDefinitionsMaximum_Count()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(acadLevelCollection);
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);

                var results = await gradeService.GetGradesDefinitionsMaximumAsync();
                Assert.AreEqual(16, results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(NullReferenceException))]
            public async Task GradeService_GradesDefinitionsMaximum_GetGradeById_Empty()
            {
                await gradeService.GetGradesDefinitionsMaximumIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(NullReferenceException))]
            public async Task GradeService_GradesDefinitionsMaximum_GetGradeById_Null()
            {
                await gradeService.GetGradesDefinitionsMaximumIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GradeService_GradesDefinitionsMaximum_GetGradeById_InvalidId()
            {
                gradeRepositoryMock.Setup(repo => repo.GetHedmGradeByIdAsync("99")).Throws<InvalidOperationException>();

                await gradeService.GetGradesDefinitionsMaximumIdAsync("99");
            }

            [TestMethod]
            public async Task GradeService_GradesDefinitionsMaximum_GetGradeById_Expected()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(acadLevelCollection);
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);

                var expectedResults = gradeCollection.Where(c => c.Guid == GradeID).FirstOrDefault();
                var x = gradeCollection.Where(g => g.Guid == GradeID).FirstOrDefault();
                gradeRepositoryMock.Setup(repo => repo.GetHedmGradeByIdAsync(It.IsAny<string>())).ReturnsAsync(x);
                var grade = await gradeService.GetGradesDefinitionsMaximumIdAsync(GradeID);
                Assert.AreEqual(expectedResults.Guid, grade.Id);
            }
          
            [TestMethod]
            public async Task GradeService_GradesDefinitionsMaximum_GetGradeById_Properties()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(acadLevelCollection);
                studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(gradeSchemeCollection);
                gradeRepositoryMock.Setup(repo => repo.GetHedmAsync(false)).ReturnsAsync(gradeCollection);

                var x = gradeCollection.Where(g => g.Guid == GradeID).FirstOrDefault();
                gradeRepositoryMock.Setup(repo => repo.GetHedmGradeByIdAsync(It.IsAny<string>())).ReturnsAsync(x);

                var grade = await gradeService.GetGradesDefinitionsMaximumIdAsync(GradeID);
                Assert.IsNotNull(grade.Id);
                Assert.IsNotNull(grade.GradeScheme);
            }
            #endregion
        }
    }
}
