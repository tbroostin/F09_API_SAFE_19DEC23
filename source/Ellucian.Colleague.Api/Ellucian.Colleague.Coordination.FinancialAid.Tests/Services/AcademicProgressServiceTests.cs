/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class AcademicProgressServiceTests : FinancialAidServiceTestsSetup
    {
        public string studentId;

        public TestAcademicProgressRepository academicProgressRepository;
        public TestStudentProgramRepository studentProgramRepository;
        public TestProgramRequirementsRepository programRequirementsRepository;
        public TestAcademicProgressAppealRepository academicProgressAppealRepository;

        public Mock<IAcademicProgressRepository> academicProgressRepositoryMock;
        public Mock<IStudentProgramRepository> studentProgramRepositoryMock;
        public Mock<IProgramRequirementsRepository> programRequirementsRepositoryMock;
        public Mock<IAcademicProgressAppealRepository> academicProgressAppealRepositoryMock;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public ITypeAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation, AcademicProgressEvaluation> academicProgressEvaluationEntityToDtoAdapter;

        public FunctionEqualityComparer<AcademicProgressEvaluation> academicProgressEvaluationComparer;

        public AcademicProgressService actualService
        {
            get
            {
                return new AcademicProgressService(
                    adapterRegistryMock.Object,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object,
                    academicProgressRepositoryMock.Object,
                    studentProgramRepositoryMock.Object,
                    programRequirementsRepositoryMock.Object,
                    academicProgressAppealRepositoryMock.Object, baseConfigurationRepository);
            }
        }

        public void AcademicProgressServiceTestsInitialize()
        {
            BaseInitialize();

            academicProgressRepository = new TestAcademicProgressRepository();
            studentProgramRepository = new TestStudentProgramRepository();
            programRequirementsRepository = new TestProgramRequirementsRepository();
            academicProgressAppealRepository = new TestAcademicProgressAppealRepository();

            academicProgressRepositoryMock = new Mock<IAcademicProgressRepository>();
            studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
            programRequirementsRepositoryMock = new Mock<IProgramRequirementsRepository>();
            academicProgressAppealRepositoryMock = new Mock<IAcademicProgressAppealRepository>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            studentId = currentUserFactory.CurrentUser.PersonId;

            academicProgressEvaluationComparer = new FunctionEqualityComparer<AcademicProgressEvaluation>(
                (e1, e2) => e1.Id == e2.Id,
                (e) => e.Id.GetHashCode());

            academicProgressEvaluationEntityToDtoAdapter = new AcademicProgressEvaluationEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            academicProgressRepositoryMock.Setup(r => r.GetStudentAcademicProgressEvaluationResultsAsync(It.IsAny<string>()))
                .Returns<string>((id) => academicProgressRepository.GetStudentAcademicProgressEvaluationResultsAsync(id));

            academicProgressAppealRepositoryMock.Setup(r => r.GetStudentAcademicProgressAppealsAsync(It.IsAny<string>()))
                .Returns<string>((id) => academicProgressAppealRepository.GetStudentAcademicProgressAppealsAsync(id));

            studentProgramRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns<string>((stuId) => academicProgressRepository.GetStudentProgramsAsync(stuId));

            programRequirementsRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((progCode, catCode) => academicProgressRepository.GetProgramRequirementsAsync(progCode, catCode));

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation, AcademicProgressEvaluation>())
                .Returns(() => academicProgressEvaluationEntityToDtoAdapter);
        }

        [TestClass]
        public class GetAcademicProgressEvaluationsTests : AcademicProgressServiceTests
        {
            public List<AcademicProgressEvaluation> expectedEvaluations
            {
                get
                {
                    return academicProgressRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId).Result
                        .Select(result =>
                            {
                                var program = academicProgressRepository.GetStudentProgramsAsync(studentId).Result.First<Domain.Student.Entities.StudentProgram>(p => p.ProgramCode == result.AcademicProgramCode);
                                var appeals = academicProgressAppealRepository.GetStudentAcademicProgressAppealsAsync(studentId).Result;
                                var eval = new Domain.FinancialAid.Entities.AcademicProgressEvaluation(result, academicProgressRepository
                                    .GetProgramRequirementsAsync(program.ProgramCode, program.CatalogCode).Result, appeals);
                                return academicProgressEvaluationEntityToDtoAdapter.MapToType(eval);
                            }).ToList();
                }
            }

            public IEnumerable<AcademicProgressEvaluation> actualEvaluations;
            
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressServiceTestsInitialize();
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualEvaluations = await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                Assert.IsNotNull(actualEvaluations);
                Assert.IsTrue(actualEvaluations.Count() > 0);
                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList(), academicProgressEvaluationComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = string.Empty;
                await actualService.GetAcademicProgressEvaluationsAsync(studentId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsCounselorTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                actualEvaluations = await actualService.GetAcademicProgressEvaluationsAsync(studentId);

                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList(), academicProgressEvaluationComparer);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxyTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                actualEvaluations = await actualService.GetAcademicProgressEvaluationsAsync(studentId);

                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList(), academicProgressEvaluationComparer);
            }

            /// <summary>
            /// User is not self nor admin nor proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndNotCounselorNorProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));
                Assert.IsFalse(currentUserFactory.CurrentUser.ProxySubjects.Any());

                studentId = "foobar";
                try
                {
                    await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to get academic progress evaluations for student {1}", currentUserFactory.CurrentUser.PersonId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            public async Task NullEvaluationsFromRepositoryReturnsEmptyListTest()
            {
                academicProgressRepositoryMock.Setup(r => r.GetStudentAcademicProgressEvaluationResultsAsync(It.IsAny<string>()))
                    .ReturnsAsync(() => null);
                actualEvaluations = await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());
                loggerMock.Verify(l => l.Debug(string.Format("No evaluationResults exist for student {0}", studentId)));
            }

            [TestMethod]
            public async Task EmptyEvaluationsFromRepositoryReturnsEmptyListTest()
            {
                academicProgressRepository.financialAidStudentData.sapResultsIds = new List<string>();
                actualEvaluations = await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());
                loggerMock.Verify(l => l.Debug(string.Format("No evaluationResults exist for student {0}", studentId)));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullStudentProgramsThrowsExceptionTest()
            {
                IEnumerable<Domain.Student.Entities.StudentProgram> nullPrograms = null;
                studentProgramRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>()))
                    .Returns<string>((stuId) => Task.FromResult(nullPrograms));

                try
                {
                    await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No StudentPrograms exist for student {0}. This is unexpected", studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task EmptyStudentProgramsThrowsExceptionTest()
            {
                academicProgressRepository.academicProgramCodes = new List<string>();

                try
                {
                    await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No StudentPrograms exist for student {0}. This is unexpected", studentId)));
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentDoesNotHaveProgramFromResultTest()
            {
                academicProgressRepository.SapResultsData.ForEach(r => r.academicProgram = "foobar");
                actualEvaluations = await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                Assert.IsFalse(actualEvaluations.Any());

                loggerMock.Verify(l => l.Debug("StudentProgram does not exist for programId {0} in AcademicProgressEvaluation {1} for student {2}", It.IsAny<object[]>()));
                //loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Unable to create AcademicProgressEvaluation {0} for student {1}", It.IsAny<object[]>()));

            }

            [TestMethod]
            public async Task MissingProgramRequirementsTest()
            {
                Domain.Student.Entities.Requirements.ProgramRequirements nullReq = null;
                programRequirementsRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(nullReq);
                actualEvaluations = await actualService.GetAcademicProgressEvaluationsAsync(studentId);
                Assert.IsFalse(actualEvaluations.Any());

                loggerMock.Verify(l => l.Debug("ProgramRequirements do not exist for programId {0} and catalogCode {1} in AcademicProgressEvaluation {2} for student {3}", It.IsAny<object[]>()));
                //loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Unable to create AcademicProgressEvaluation {0} for student {1}", It.IsAny<object[]>()));

            }

        }

        [TestClass]
        public class GetAcademicProgressEvaluations2Tests : AcademicProgressServiceTests
        {
            private ITypeAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation2, AcademicProgressEvaluation2> academicProgressEvaluation2EntityToDtoAdapter;

            private FunctionEqualityComparer<AcademicProgressEvaluation2> academicProgressEvaluation2Comparer
            {
                get
                {
                    return new FunctionEqualityComparer<AcademicProgressEvaluation2>(
                        (e1, e2) => e1.Id == e2.Id,
                        (e) => e.Id.GetHashCode());
                }
            }

            private List<AcademicProgressEvaluation2> expectedEvaluations
            {
                get
                {
                    return academicProgressRepository.GetStudentAcademicProgressEvaluationResultsAsync(studentId).Result
                        .Select(result =>
                        {
                            var program = academicProgressRepository.GetStudentProgramsAsync(studentId).Result.First<Domain.Student.Entities.StudentProgram>(p => p.ProgramCode == result.AcademicProgramCode);
                            var appeals = academicProgressAppealRepository.GetStudentAcademicProgressAppealsAsync(studentId).Result;
                            var eval = new Domain.FinancialAid.Entities.AcademicProgressEvaluation2(result, academicProgressRepository
                                .GetStudentAcademicProgressProgramDetailAsync(program.ProgramCode, program.CatalogCode).Result, appeals);
                            return academicProgressEvaluation2EntityToDtoAdapter.MapToType(eval);
                        }).ToList();
                }
            }

            public IEnumerable<AcademicProgressEvaluation2> actualEvaluations;

            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressServiceTestsInitialize();

                academicProgressRepositoryMock.Setup(r => r.GetStudentAcademicProgressProgramDetailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((prCode, catalog) => academicProgressRepository.GetStudentAcademicProgressProgramDetailAsync(prCode, catalog));

                academicProgressEvaluation2EntityToDtoAdapter = new AcademicProgressEvaluation2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

                adapterRegistryMock.Setup(r => r.GetAdapter<Domain.FinancialAid.Entities.AcademicProgressEvaluation2, AcademicProgressEvaluation2>())
                .Returns(() => academicProgressEvaluation2EntityToDtoAdapter);                
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualEvaluations = await actualService.GetAcademicProgressEvaluations2Async(studentId);
                Assert.IsNotNull(actualEvaluations);
                Assert.IsTrue(actualEvaluations.Count() > 0);
                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList(), academicProgressEvaluation2Comparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = string.Empty;
                await actualService.GetAcademicProgressEvaluations2Async(studentId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsCounselorTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                actualEvaluations = await actualService.GetAcademicProgressEvaluations2Async(studentId);

                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList(), academicProgressEvaluation2Comparer);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxyTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                actualEvaluations = await actualService.GetAcademicProgressEvaluations2Async(studentId);

                CollectionAssert.AreEqual(expectedEvaluations, actualEvaluations.ToList(), academicProgressEvaluation2Comparer);
            }

            
            /// <summary>
            /// Current user is not self, nor admin, nor proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndNotCounselorOrProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));
                Assert.IsFalse(currentUserFactory.CurrentUser.ProxySubjects.Any());

                studentId = "foobar";
                try
                {
                    await actualService.GetAcademicProgressEvaluations2Async(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to get academic progress evaluations for student {1}", currentUserFactory.CurrentUser.PersonId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            public async Task NullEvaluationsFromRepositoryReturnsEmptyListTest()
            {
                academicProgressRepositoryMock.Setup(r => r.GetStudentAcademicProgressEvaluationResultsAsync(It.IsAny<string>()))
                    .ReturnsAsync(() => null);
                actualEvaluations = await actualService.GetAcademicProgressEvaluations2Async(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());
                loggerMock.Verify(l => l.Debug(string.Format("No evaluationResults exist for student {0}", studentId)));
            }

            [TestMethod]
            public async Task EmptyEvaluationsFromRepositoryReturnsEmptyListTest()
            {
                academicProgressRepository.financialAidStudentData.sapResultsIds = new List<string>();
                actualEvaluations = await actualService.GetAcademicProgressEvaluations2Async(studentId);
                Assert.AreEqual(0, actualEvaluations.Count());
                loggerMock.Verify(l => l.Debug(string.Format("No evaluationResults exist for student {0}", studentId)));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullStudentProgramsThrowsExceptionTest()
            {
                IEnumerable<Domain.Student.Entities.StudentProgram> nullPrograms = null;
                studentProgramRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>()))
                    .Returns<string>((stuId) => Task.FromResult(nullPrograms));

                try
                {
                    await actualService.GetAcademicProgressEvaluations2Async(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No StudentPrograms exist for student {0}. This is unexpected", studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task EmptyStudentProgramsThrowsExceptionTest()
            {
                academicProgressRepository.academicProgramCodes = new List<string>();

                try
                {
                    await actualService.GetAcademicProgressEvaluations2Async(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No StudentPrograms exist for student {0}. This is unexpected", studentId)));
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentDoesNotHaveProgramFromResultTest()
            {
                academicProgressRepository.SapResultsData.ForEach(r => r.academicProgram = "foobar");
                actualEvaluations = await actualService.GetAcademicProgressEvaluations2Async(studentId);
                Assert.IsFalse(actualEvaluations.Any());

                loggerMock.Verify(l => l.Debug("StudentProgram does not exist for programId {0} in AcademicProgressEvaluation {1} for student {2}", It.IsAny<object[]>()));
            }

            [TestMethod]
            public async Task MissingProgramDetailTest()
            {
                academicProgressRepositoryMock.Setup(r => r.GetStudentAcademicProgressProgramDetailAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                actualEvaluations = await actualService.GetAcademicProgressEvaluations2Async(studentId);
                Assert.IsTrue(actualEvaluations.Any());

                loggerMock.Verify(l => l.Debug("ProgramRequirements do not exist for programId {0} and catalogCode {1} in AcademicProgressEvaluation {2} for student {3}", It.IsAny<object[]>()));                
            }

            [TestMethod]
            public async Task GenericExceptionThrownCreatingEvaluation_CaughtAndLoggedTest()
            {
                academicProgressRepositoryMock.Setup(r => r.GetStudentAcademicProgressProgramDetailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Domain.FinancialAid.Entities.AcademicProgressProgramDetail("foo", 100, 40));

                bool exceptionThrown = false;
                try
                {
                    await actualService.GetAcademicProgressEvaluations2Async(studentId);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Unable to create AcademicProgressEvaluation {0} for student {1}", It.IsAny<string>(), studentId));
            }
        }
    }
}
