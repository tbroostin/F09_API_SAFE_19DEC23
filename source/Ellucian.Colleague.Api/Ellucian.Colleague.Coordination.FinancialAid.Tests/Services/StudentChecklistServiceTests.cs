/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentChecklistServiceTests : FinancialAidServiceTestsSetup
    {
        public string studentId;

        public TestStudentChecklistRepository studentChecklistRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestFinancialAidOfficeRepository financialAidOfficeRepository;
        public TestFinancialAidReferenceDataRepository referenceDataRepository;

        public Mock<IStudentChecklistRepository> studentChecklistRepositoryMock;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
        public Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
        public Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;
        public Mock<IProfileRepository> profileRepositoryMock;
        public Mock<IRelationshipRepository> relationshipRepositoryMock;
        public Mock<IProxyRepository> proxyRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public ITypeAdapter<Domain.FinancialAid.Entities.StudentFinancialAidChecklist, StudentFinancialAidChecklist> studentChecklistEntityToDtoAdapter;

        public FunctionEqualityComparer<StudentFinancialAidChecklist> studentChecklistDtoComparer;
        public FunctionEqualityComparer<StudentChecklistItem> studentChecklistItemDtoComparer;

        public IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> financialAidOfficeEntities
        { get { return financialAidOfficeRepository.GetFinancialAidOffices(); } }

        public IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities
        { get { return studentAwardYearRepository.GetStudentAwardYears(studentId, new Domain.FinancialAid.Services.CurrentOfficeService(financialAidOfficeEntities)); } }

        public StudentChecklistService studentChecklistService;

        public void StudentChecklistServiceTestsInitialize()
        {
            BaseInitialize();

            studentId = currentUserFactory.CurrentUser.PersonId;

            studentChecklistDtoComparer = new FunctionEqualityComparer<StudentFinancialAidChecklist>(
                (c1, c2) => c1.AwardYear == c2.AwardYear && c1.StudentId == c2.StudentId && c1.ChecklistItems.Count == c2.ChecklistItems.Count,
                (c) => c.AwardYear.GetHashCode() ^ c.StudentId.GetHashCode() ^ c.ChecklistItems.Count);

            studentChecklistItemDtoComparer = new FunctionEqualityComparer<StudentChecklistItem>(
                (i1, i2) => i1.Code == i2.Code && i1.ControlStatus == i2.ControlStatus,
                (i) => i.Code.GetHashCode() ^ i.ControlStatus.GetHashCode());


            studentChecklistRepository = new TestStudentChecklistRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            referenceDataRepository = new TestFinancialAidReferenceDataRepository();

            studentChecklistRepositoryMock = new Mock<IStudentChecklistRepository>();
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            referenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();

            //NAR
            profileRepositoryMock = new Mock<IProfileRepository>();
            relationshipRepositoryMock = new Mock<IRelationshipRepository>();
            proxyRepositoryMock = new Mock<IProxyRepository>();


        baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            studentChecklistEntityToDtoAdapter = new StudentChecklistEntitytoDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.FinancialAid.Entities.StudentFinancialAidChecklist, StudentFinancialAidChecklist>())
                .Returns(() => studentChecklistEntityToDtoAdapter);

            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Services.CurrentOfficeService>(), It.IsAny<bool>()))
                .Returns<string, Domain.FinancialAid.Services.CurrentOfficeService, bool>((id, currentOfficeService, b) => studentAwardYearRepository.GetStudentAwardYearsAsync(id, currentOfficeService));

            financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
                .Returns(() => financialAidOfficeRepository.GetFinancialAidOfficesAsync());

            referenceDataRepositoryMock.Setup(r => r.ChecklistItems)
                .Returns(() => referenceDataRepository.ChecklistItems);

            studentChecklistRepositoryMock.Setup(r => r.GetStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((id, year) => studentChecklistRepository.GetStudentChecklistAsync(id, year));

            studentChecklistRepositoryMock.Setup(r => r.GetStudentChecklistsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns<string, IEnumerable<string>>((id, years) => studentChecklistRepository.GetStudentChecklistsAsync(id, years));

            studentChecklistRepositoryMock.Setup(r => r.CreateStudentChecklistAsync(It.IsAny<Domain.FinancialAid.Entities.StudentFinancialAidChecklist>()))
                .Returns<Domain.FinancialAid.Entities.StudentFinancialAidChecklist>((checklistEntity) => studentChecklistRepository.CreateStudentChecklistAsync(checklistEntity));

            

            BuildService();
        }

        private void BuildService()
        {
            studentChecklistService = new StudentChecklistService(
                            adapterRegistryMock.Object,
                            studentChecklistRepositoryMock.Object,
                            studentAwardYearRepositoryMock.Object,
                            financialAidOfficeRepositoryMock.Object,
                            referenceDataRepositoryMock.Object,
                            profileRepositoryMock.Object,
                            proxyRepositoryMock.Object,
                            relationshipRepositoryMock.Object,
                            baseConfigurationRepository,
                            currentUserFactory,
                            roleRepositoryMock.Object,
                            loggerMock.Object);
        }

        [TestClass]
        public class CreateStudentChecklistTests : StudentChecklistServiceTests
        {
            public string inputAwardYear;

            public StudentFinancialAidChecklist actualChecklist;           

            [TestInitialize]
            public void Initialize()
            {
                StudentChecklistServiceTestsInitialize();
                inputAwardYear = "2016";
                studentAwardYearRepository.FaStudentData.FaCsYears.Add(inputAwardYear);                
            }

            [TestMethod]
            public async Task ChecklistCreatedTest()
            {
                actualChecklist = await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                Assert.IsNotNull(actualChecklist);               
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = string.Empty;
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearRequiredTest()
            {
                inputAwardYear = string.Empty;
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
            }

            /// <summary>
            /// User is not self (counselor)
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorCannotCreateChecklistTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                studentChecklistService = new StudentChecklistService(
                    adapterRegistryMock.Object,
                    studentChecklistRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    financialAidOfficeRepositoryMock.Object,
                    referenceDataRepositoryMock.Object,
                    profileRepositoryMock.Object,
                    proxyRepositoryMock.Object,
                    relationshipRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsNotSelf_CannotCreateChecklistTest()
            {
                await studentChecklistService.CreateStudentChecklistAsync("foo", inputAwardYear);
            }

            [TestMethod]
            public async Task VerifyGetStudentChecklistRepositoryTest()
            {
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                studentChecklistRepositoryMock.Verify(r => r.GetStudentChecklistAsync(studentId, inputAwardYear));
            }

            [TestMethod]
            public async Task NoExistingChecklistLogsInfoMessageTest()
            {
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(string.Format("Student {0} has no checklist for {1}.", studentId, inputAwardYear)))));
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public async Task ExistingChecklistTest()
            {
                studentChecklistRepository.ysStudentRecords.First(r => r.awardYear == inputAwardYear).checklistItems = 
                    new List<TestStudentChecklistRepository.ChecklistItemsRecord>()
                        {
                            new TestStudentChecklistRepository.ChecklistItemsRecord() {checklistItem = "FAFSA", displayAction = "Q"}
                        };
                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (ExistingResourceException ere)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    Assert.AreEqual(inputAwardYear, ere.ExistingResourceId);
                    throw;
                }
            }

            [TestMethod]
            public async Task VerifyStudentAwardYearRepositoryTest()
            {
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                studentAwardYearRepositoryMock.Verify(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<Domain.FinancialAid.Services.CurrentOfficeService>(), It.IsAny<bool>()));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullStudentAwardYearDataTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> nullYears = null;
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Services.CurrentOfficeService>(), It.IsAny<bool>()))
                    .ReturnsAsync(nullYears);

                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task EmptyStudentAwardYearDataTest()
            {
                studentAwardYearRepository.ClearAwardYears();

                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task StudentDoesNotHaveAwardYearSpecifiedTest()
            {
                inputAwardYear = "foobar";
                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            public async Task VerifyChecklistItemsRepositoryTest()
            {
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                referenceDataRepositoryMock.Verify(r => r.ChecklistItems);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullChecklistItemsTest()
            {
                referenceDataRepository.checklistItemData = null;

                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task EmptyChecklistItemsTest()
            {
                referenceDataRepository.checklistItemData = new List<TestFinancialAidReferenceDataRepository.FAChecklistItem>();

                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }
          

            [TestMethod]
            public async Task VerifyStudentChecklistRepositoryTest()
            {
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                studentChecklistRepositoryMock.Verify(r =>
                    r.CreateStudentChecklistAsync(It.IsAny<Domain.FinancialAid.Entities.StudentFinancialAidChecklist>()));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullChecklistFromRepositoryTest()
            {
                studentChecklistRepositoryMock.Setup(r => r.CreateStudentChecklistAsync(It.IsAny<Domain.FinancialAid.Entities.StudentFinancialAidChecklist>()))
                    .ReturnsAsync(() => null);

                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }            

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullCurrentConfig_ExceptionThrownTest()
            {
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                financialAidOfficeRepository.officeParameterRecordData.Remove(financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear));
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoChecklistItemCodesCurrentConfig_ExceptionThrownTest()
            {
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemCodes = new List<string>();
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoChecklistItemControlStatusesCurrentConfig_ExceptionThrownTest()
            {
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemControlStatuses = new List<string>();
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoChecklistItemDefaultFlags_ExceptionThrownTest()
            {
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemDefaultFlags = new List<string>();
                await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
            }

            [TestMethod]
            public async Task ChecklistItemDefaultFlagsSetToY_ExpectedNumberOfChecklistItemsCreatedTest()
            {
                actualChecklist = await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                int expectedCount = financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemCodes.Count;
                Assert.AreEqual(expectedCount, actualChecklist.ChecklistItems.Count);
            }

            [TestMethod]
            public async Task ChecklistItemDefaultFlagsSomeSetToNo_ExpectedNumberOfChecklistItemsCreatedTest()
            {
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemDefaultFlags = new List<string>() { "N", "Y", "Y", "Y", "N" };
                int expectedCount = financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemCodes.Count - 2;

                actualChecklist = await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                Assert.AreEqual(expectedCount, actualChecklist.ChecklistItems.Count);
            }

            [TestMethod]            
            public async Task ChecklistItemDefaultFlagsEmpty_ExceptionThrownTest()
            {                
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemDefaultFlags = new List<string>() { "", "", "", "", ""};
                bool exceptionThrown = false;
                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch { exceptionThrown = true; }
                Assert.IsTrue(exceptionThrown);
            }

            [TestMethod]
            public async Task ChecklistItemDefaultFlagsNotSetToY_ExceptionThrownTest()
            {
                var officeId = studentAwardYearEntities.First(y => y.Code == inputAwardYear).FinancialAidOfficeId;
                financialAidOfficeRepository.officeParameterRecordData.First(r => r.OfficeCode == officeId && r.AwardYear == inputAwardYear).ChecklistItemDefaultFlags = new List<string>() { "a", "b", "c", "d", "e" };
                bool exceptionThrown = false;
                try
                {
                    await studentChecklistService.CreateStudentChecklistAsync(studentId, inputAwardYear);
                }
                catch { exceptionThrown = true; }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestClass]
        public class GetAllStudentChecklistsTests : StudentChecklistServiceTests
        {
            public List<StudentFinancialAidChecklist> expectedChecklists
            {
                get
                {
                    return (studentChecklistRepository.GetStudentChecklistsAsync(studentId, studentAwardYearEntities.Select(y => y.Code))).Result
                        .Select(domain => studentChecklistEntityToDtoAdapter.MapToType(domain))
                        .ToList();
                }
            }

            public List<StudentFinancialAidChecklist> actualChecklists;
            
            [TestInitialize]
            public async void Initialize()
            {
                StudentChecklistServiceTestsInitialize();
                actualChecklists = (await studentChecklistService.GetAllStudentChecklistsAsync(studentId)).ToList();
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedChecklists, actualChecklists, studentChecklistDtoComparer);
                CollectionAssert.AreEqual(expectedChecklists.SelectMany(c => c.ChecklistItems).ToList(), actualChecklists.SelectMany(c => c.ChecklistItems).ToList(), studentChecklistItemDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = null;
                await studentChecklistService.GetAllStudentChecklistsAsync(studentId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildService();
                actualChecklists = (await studentChecklistService.GetAllStudentChecklistsAsync(studentId)).ToList();

                CollectionAssert.AreEqual(actualChecklists, expectedChecklists, studentChecklistDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                BuildService();
                await studentChecklistService.GetAllStudentChecklistsAsync(studentId);                
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ProxyCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();
                actualChecklists = (await studentChecklistService.GetAllStudentChecklistsAsync(studentId)).ToList();

                CollectionAssert.AreEqual(actualChecklists, expectedChecklists, studentChecklistDtoComparer);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildService();
                await studentChecklistService.GetAllStudentChecklistsAsync(studentId);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task OnlyStudentProxyOrCounselorCanAccessDataTest()
            {
                studentId = "foobar";
                try
                {
                    await studentChecklistService.GetAllStudentChecklistsAsync(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("User {0} does not have permission to get StudentChecklists for student {1}", currentUserFactory.CurrentUser.PersonId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentHasNullAwardYearsTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> nullYears = null;
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Services.CurrentOfficeService>(), It.IsAny<bool>()))
                    .ReturnsAsync(nullYears);
                actualChecklists = (await studentChecklistService.GetAllStudentChecklistsAsync(studentId)).ToList();
                Assert.IsFalse(actualChecklists.Any());

                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no award years for which to get StudentChecklist objects", studentId)));

            }

            [TestMethod]
            public async Task StudentHasNoAwardYearsTest()
            {
                studentAwardYearRepository.ClearAwardYears();
                actualChecklists = (await studentChecklistService.GetAllStudentChecklistsAsync(studentId)).ToList();

                Assert.IsFalse(actualChecklists.Any());

                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no award years for which to get StudentChecklist objects", studentId)));
            }

            [TestMethod]
            public async Task StudentHasNullChecklistsTest()
            {
                studentChecklistRepositoryMock.Setup(r => r.GetStudentChecklistsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                    .ReturnsAsync(() => null);
                actualChecklists = (await studentChecklistService.GetAllStudentChecklistsAsync(studentId)).ToList();
                Assert.IsFalse(actualChecklists.Any());

                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no checklist items for any award years", studentId)));
            }

            [TestMethod]
            public async Task StudentHasEmptyChecklistsTest()
            {
                studentChecklistRepository.ysStudentRecords = new List<TestStudentChecklistRepository.YsStudentRecord>();
                actualChecklists = (await studentChecklistService.GetAllStudentChecklistsAsync(studentId)).ToList();
                Assert.IsFalse(actualChecklists.Any());

                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no checklist items for any award years", studentId)));
            }
        }

        [TestClass]
        public class GetSingleStudentChecklistTests : StudentChecklistServiceTests
        {
            public string awardYear;

            public StudentFinancialAidChecklist expectedChecklist
            {
                get
                {
                    return studentChecklistEntityToDtoAdapter.MapToType(
                        studentChecklistRepository.GetStudentChecklistAsync(studentId, awardYear).Result);
                }
            }

            public StudentFinancialAidChecklist actualChecklist;
           

            [TestInitialize]
            public async void Initialize()
            {
                StudentChecklistServiceTestsInitialize();
                awardYear = studentAwardYearEntities.First().Code;
                actualChecklist = await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.IsTrue(studentChecklistDtoComparer.Equals(expectedChecklist, actualChecklist));
                CollectionAssert.AreEqual(expectedChecklist.ChecklistItems, actualChecklist.ChecklistItems, studentChecklistItemDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = null;
                await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task YearRequiredTest()
            {
                awardYear = null;
                await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildService();

                actualChecklist = await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
                Assert.IsTrue(studentChecklistDtoComparer.Equals(expectedChecklist, actualChecklist));
                CollectionAssert.AreEqual(expectedChecklist.ChecklistItems, actualChecklist.ChecklistItems, studentChecklistItemDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                BuildService();

                await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);                
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ProxyCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();

                actualChecklist = await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
                Assert.IsTrue(studentChecklistDtoComparer.Equals(expectedChecklist, actualChecklist));
                CollectionAssert.AreEqual(expectedChecklist.ChecklistItems, actualChecklist.ChecklistItems, studentChecklistItemDtoComparer);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
                BuildService();

                await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task OnlyStudentProxyOrCounselorCanAccessDataTest()
            {
                studentId = "foobar";
                try
                {
                    await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("User {0} does not have permission to get StudentChecklist for student {1} and awardYear {2}", currentUserFactory.CurrentUser.PersonId, studentId, awardYear)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AwardYearDoesNotExistForStudentTest()
            {
                awardYear = "foobar";
                try
                {
                    await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no financial aid data for award year {1}", studentId, awardYear)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentHasNullAwardYearsTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> nullYears = null;
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Services.CurrentOfficeService>(), It.IsAny<bool>()))
                    .ReturnsAsync(nullYears);

                try
                {
                    await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch(Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentHasNoAwardYearsTest()
            {
                studentAwardYearRepository.ClearAwardYears();

                try
                {
                    await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ChecklistDoesNotExistForStudentTest()
            {
                studentChecklistRepositoryMock.Setup(r => r.GetStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(() => null);

                try
                {
                    await studentChecklistService.GetStudentChecklistAsync(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no checklist items for award year {1}", studentId, awardYear)));
                    throw;
                }
            }
        }
    }


}
