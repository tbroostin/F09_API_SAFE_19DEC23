/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Moq;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    /// <summary>
    /// OutsideAwardsService Tests
    /// </summary>
    [TestClass]
    public class OutsideAwardsServiceTests
    {
        /// <summary>
        /// Test class for CreateOutsideAwardAsync method of the OutsideAwardsService
        /// </summary>
        [TestClass]
        public class CreateOutsideAwardTests : FinancialAidServiceTestsSetup
        {
            private TestOutsideAwardsRepository testOutsideAwardsRepository;

            private OutsideAwardsService outsideAwardsService;

            private Mock<IOutsideAwardsRepository> outsideAwardsRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private OutsideAward inputOutsideAwardEntity;

            private Colleague.Dtos.FinancialAid.OutsideAward expectedOutsideAwardDto;
            private Colleague.Dtos.FinancialAid.OutsideAward actualOutsideAwardDto;

            private AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward, Colleague.Dtos.FinancialAid.OutsideAward> outsideAwardEntityToDtoAdapter;
            private OutsideAwardDtoToEntityAdapter outsideAwardDtoToEntityAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                testOutsideAwardsRepository = new TestOutsideAwardsRepository();

                outsideAwardsRepositoryMock = new Mock<IOutsideAwardsRepository>();

                var dataRecord = testOutsideAwardsRepository.outsideAwardRecords.First();
                inputOutsideAwardEntity = new OutsideAward(dataRecord.recordId, dataRecord.studentId, dataRecord.awardYear, dataRecord.awardName, 
                    dataRecord.awardType, dataRecord.awardAmount, dataRecord.fundingSource);
                outsideAwardsRepositoryMock.Setup(r => r.CreateOutsideAwardAsync(It.IsAny<Domain.FinancialAid.Entities.OutsideAward>())).ReturnsAsync(inputOutsideAwardEntity);

                outsideAwardEntityToDtoAdapter = new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward, 
                    Colleague.Dtos.FinancialAid.OutsideAward>(adapterRegistryMock.Object, loggerMock.Object);

                outsideAwardDtoToEntityAdapter = new OutsideAwardDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward,
                    Colleague.Dtos.FinancialAid.OutsideAward>>(a => a.GetAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward,
                    Colleague.Dtos.FinancialAid.OutsideAward>()).Returns(outsideAwardEntityToDtoAdapter);

                adapterRegistryMock.Setup(a => a.GetAdapter<Colleague.Dtos.FinancialAid.OutsideAward,
                    Colleague.Domain.FinancialAid.Entities.OutsideAward>()).Returns(outsideAwardDtoToEntityAdapter);

                expectedOutsideAwardDto = outsideAwardEntityToDtoAdapter.MapToType(inputOutsideAwardEntity);

                BuildOutsideAwardsService();
            }

            private void BuildOutsideAwardsService()
            {
                outsideAwardsService = new OutsideAwardsService(adapterRegistryMock.Object,
                    outsideAwardsRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                testOutsideAwardsRepository = null;
                outsideAwardsRepositoryMock = null;
                inputOutsideAwardEntity = null;
                outsideAwardEntityToDtoAdapter = null;
                adapterRegistryMock = null;
                outsideAwardsService = null;
            }

            [TestMethod]
            public async Task OutsideAwardDto_IsNotNullTest()
            {
                actualOutsideAwardDto = await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
                Assert.IsNotNull(actualOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullOutsideAward_ArgumentNullExceptionThrownTest()
            {
                await outsideAwardsService.CreateOutsideAwardAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullStudentId_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.StudentId = null;
                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardYearCode_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardYearCode = null;
                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardName_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardName = null;
                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardType_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardType = null;
                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task LessThanZeroAwardAmount_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardAmount = -4;
                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardFundingSource_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardFundingSource = null;
                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            public async Task OutsideAwardDto_EqualsExpectedTest()
            {
                actualOutsideAwardDto = await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
                Assert.AreEqual(expectedOutsideAwardDto.Id, actualOutsideAwardDto.Id);
                Assert.AreEqual(expectedOutsideAwardDto.StudentId, actualOutsideAwardDto.StudentId);
                Assert.AreEqual(expectedOutsideAwardDto.AwardName, actualOutsideAwardDto.AwardName);
                Assert.AreEqual(expectedOutsideAwardDto.AwardType, actualOutsideAwardDto.AwardType);
                Assert.AreEqual(expectedOutsideAwardDto.AwardAmount, actualOutsideAwardDto.AwardAmount);
                Assert.AreEqual(expectedOutsideAwardDto.AwardYearCode, actualOutsideAwardDto.AwardYearCode);
                Assert.AreEqual(expectedOutsideAwardDto.AwardFundingSource, actualOutsideAwardDto.AwardFundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task NullCreatedOutsideAward_ExceptionThrownTest()
            {
                outsideAwardsRepositoryMock.Setup(r => r.CreateOutsideAwardAsync(It.IsAny<Domain.FinancialAid.Entities.OutsideAward>())).ReturnsAsync(() => null);
                BuildOutsideAwardsService();
                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsNotSelf_PermissionsExceptionThrownTest()
            {
                inputOutsideAwardEntity = new OutsideAward(expectedOutsideAwardDto.Id, "foo", expectedOutsideAwardDto.AwardYearCode, expectedOutsideAwardDto.AwardName,
                    expectedOutsideAwardDto.AwardType, expectedOutsideAwardDto.AwardAmount, expectedOutsideAwardDto.AwardFundingSource);

                expectedOutsideAwardDto = outsideAwardEntityToDtoAdapter.MapToType(inputOutsideAwardEntity);

                await outsideAwardsService.CreateOutsideAwardAsync(expectedOutsideAwardDto);
            }
        }

        [TestClass]
        public class GetOutsideAwardsAsyncTests : FinancialAidServiceTestsSetup
        {
            private TestOutsideAwardsRepository testOutsideAwardsRepository;

            private OutsideAwardsService outsideAwardsService;

            private Mock<IOutsideAwardsRepository> outsideAwardsRepositoryMock;

            private AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward, Colleague.Dtos.FinancialAid.OutsideAward> outsideAwardEntityToDtoAdapter;
           
            private List<Colleague.Dtos.FinancialAid.OutsideAward> expectedOutsideAwardDtos;
            private List<Colleague.Dtos.FinancialAid.OutsideAward> actualOutsideAwardDtos;
            private List<OutsideAward>  outsideAwardEntities;

            private string studentId;
            private string awardYearCode;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                testOutsideAwardsRepository = new TestOutsideAwardsRepository();

                outsideAwardsRepositoryMock = new Mock<IOutsideAwardsRepository>();

                studentId = "0003914";
                awardYearCode = testOutsideAwardsRepository.outsideAwardRecords.First().awardYear;

                outsideAwardEntities = (testOutsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode).Result).ToList();

                outsideAwardsRepositoryMock.Setup(r => r.GetOutsideAwardsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(outsideAwardEntities);

                outsideAwardEntityToDtoAdapter = new AutoMapperAdapter<OutsideAward, Dtos.FinancialAid.OutsideAward>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<OutsideAward, Dtos.FinancialAid.OutsideAward>()).Returns(outsideAwardEntityToDtoAdapter);

                expectedOutsideAwardDtos = new List<Dtos.FinancialAid.OutsideAward>();
                foreach (var entity in outsideAwardEntities)
                {
                    expectedOutsideAwardDtos.Add(new Dtos.FinancialAid.OutsideAward()
                    {
                        Id = entity.Id,
                        StudentId = entity.StudentId,
                        AwardName = entity.AwardName,
                        AwardYearCode = entity.AwardYearCode,
                        AwardType = entity.AwardType,
                        AwardAmount = entity.AwardAmount,
                        AwardFundingSource = entity.AwardFundingSource
                    });
                }
                
                var roles = new List<Role>()
                {
                    new Role(1, "FINANCIAL AID COUNSELOR")                  
                };
                roles[0].AddPermission(new Permission("VIEW.FINANCIAL.AID.INFORMATION"));
                roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

                BuildOutsideAwardsService();
            }

            private void BuildOutsideAwardsService()
            {
                outsideAwardsService = new OutsideAwardsService(adapterRegistryMock.Object, outsideAwardsRepositoryMock.Object, baseConfigurationRepository,
                    currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                testOutsideAwardsRepository = null;
                outsideAwardsRepositoryMock = null;                
                outsideAwardEntityToDtoAdapter = null;
                adapterRegistryMock = null;
                outsideAwardsService = null;
                expectedOutsideAwardDtos = null;
                actualOutsideAwardDtos = null;
                outsideAwardEntities = null;
            }

            [TestMethod]
            public async Task GetOutsideAwardsAsync_ReturnsNonNullValueTest()
            {
                actualOutsideAwardDtos = (await outsideAwardsService.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.IsNotNull(actualOutsideAwardDtos);
            }

            [TestMethod]
            public async Task GetOutsideAwardsAsync_ReturnsNonEmptyListTest()
            {
                actualOutsideAwardDtos = (await outsideAwardsService.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.IsTrue(actualOutsideAwardDtos.Any());
            }

            [TestMethod]
            public async Task GetOutsideAwardsAsync_ReturnsExpectedOutsideAwardsCountTest()
            {
                actualOutsideAwardDtos = (await outsideAwardsService.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.AreEqual(expectedOutsideAwardDtos.Count, actualOutsideAwardDtos.Count);
            }

            [TestMethod]
            public async Task ActualOutsideAwards_EqualExpectedOutsideAwardsTest()
            {
                actualOutsideAwardDtos = (await outsideAwardsService.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                foreach (var expectedAward in expectedOutsideAwardDtos)
                {
                    var actualAward = actualOutsideAwardDtos.First(a => a.Id == expectedAward.Id);
                    Assert.AreEqual(expectedAward.StudentId, actualAward.StudentId);
                    Assert.AreEqual(expectedAward.AwardYearCode, actualAward.AwardYearCode);
                    Assert.AreEqual(expectedAward.AwardName, actualAward.AwardName);
                    Assert.AreEqual(expectedAward.AwardType, actualAward.AwardType);
                    Assert.AreEqual(expectedAward.AwardAmount, actualAward.AwardAmount);
                    Assert.AreEqual(expectedAward.AwardFundingSource, actualAward.AwardFundingSource);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ArgumentNullExceptionThrownTest()
            {
                await outsideAwardsService.GetOutsideAwardsAsync(null, awardYearCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardYearCode_ArgumentNullExceptionThrownTest()
            {
                await outsideAwardsService.GetOutsideAwardsAsync(studentId, null);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsNotSelf_PermissionsExceptionThrownTest()
            {
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>());
                await outsideAwardsService.GetOutsideAwardsAsync("foo", awardYearCode);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsNotSelfButCounselor_NoExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                BuildOutsideAwardsService();
                bool exceptionThrown = false;
                try
                {
                    await outsideAwardsService.GetOutsideAwardsAsync(studentId, awardYearCode);
                }
                catch
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsNotSelfButCounselorWithNoPermissions_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>());
                BuildOutsideAwardsService();
                await outsideAwardsService.GetOutsideAwardsAsync(studentId, awardYearCode);               
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsNotSelfButProxy_NoExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildOutsideAwardsService();
                bool exceptionThrown = false;
                try
                {
                    await outsideAwardsService.GetOutsideAwardsAsync(currentUserFactory.CurrentUser.ProxySubjects.First().PersonId, awardYearCode);
                }
                catch
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsNotSelfButProxyForDifferentPerson_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
                BuildOutsideAwardsService();
                await outsideAwardsService.GetOutsideAwardsAsync(studentId, awardYearCode);
            }

        }

        [TestClass]
        public class DeleteOutsideAwardAsyncTests : FinancialAidServiceTestsSetup
        {
            private OutsideAwardsService outsideAwardsService;

            private Mock<IOutsideAwardsRepository> outsideAwardsRepositoryMock;

            private string studentId, recordId;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                outsideAwardsRepositoryMock = new Mock<IOutsideAwardsRepository>();
                outsideAwardsRepositoryMock.Setup(r => r.DeleteOutsideAwardAsync(It.IsAny<string>())).Returns(Task.FromResult(""));

                var roles = new List<Role>()
                {
                    new Role(1, "FINANCIAL AID COUNSELOR")                  
                };
                roles[0].AddPermission(new Permission("VIEW.FINANCIAL.AID.INFORMATION"));
                roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

                BuildOutsideAwardsService();

                studentId = "0003914";
                recordId = "foo";
            }

            private void BuildOutsideAwardsService()
            {
                outsideAwardsService = new OutsideAwardsService(adapterRegistryMock.Object,
                    outsideAwardsRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                outsideAwardsRepositoryMock = null;
                adapterRegistryMock = null;
                outsideAwardsService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullRecordId_ArgumentNullExceptionThrownTest()
            {
                await outsideAwardsService.DeleteOutsideAwardAsync(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ArgumentNullExceptionThrownTest()
            {
                await outsideAwardsService.DeleteOutsideAwardAsync(null, recordId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotIsUserSelf_PermissionsExceptionThrownTest()
            {
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>());
                await outsideAwardsService.DeleteOutsideAwardAsync("foo", recordId);
            }

            [Ignore]
            [TestMethod]
            public async Task UserIsSelf_NoExceptionThrownTest()
            {
                bool exceptionThrown = false;
                try
                {
                    await outsideAwardsService.DeleteOutsideAwardAsync(studentId, recordId);
                }
                catch
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
            }
        }

        [TestClass]
        public class UpdateOutsideAwardsAsyncTests : FinancialAidServiceTestsSetup
        {
            private TestOutsideAwardsRepository testOutsideAwardsRepository;

            private OutsideAwardsService outsideAwardsService;

            private Mock<IOutsideAwardsRepository> outsideAwardsRepositoryMock;
            
            private OutsideAward inputOutsideAwardEntity;

            private Colleague.Dtos.FinancialAid.OutsideAward expectedOutsideAwardDto;
            private Colleague.Dtos.FinancialAid.OutsideAward actualOutsideAwardDto;

            private AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward, Colleague.Dtos.FinancialAid.OutsideAward> outsideAwardEntityToDtoAdapter;
            private OutsideAwardDtoToEntityAdapter outsideAwardDtoToEntityAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                testOutsideAwardsRepository = new TestOutsideAwardsRepository();

                outsideAwardsRepositoryMock = new Mock<IOutsideAwardsRepository>();

                var dataRecord = testOutsideAwardsRepository.outsideAwardRecords.First();
                inputOutsideAwardEntity = new OutsideAward(dataRecord.recordId, dataRecord.studentId, dataRecord.awardYear, dataRecord.awardName,
                    dataRecord.awardType, dataRecord.awardAmount, dataRecord.fundingSource);
                outsideAwardsRepositoryMock.Setup(r => r.UpdateOutsideAwardAsync(It.IsAny<Domain.FinancialAid.Entities.OutsideAward>())).ReturnsAsync(inputOutsideAwardEntity);

                outsideAwardEntityToDtoAdapter = new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward,
                    Colleague.Dtos.FinancialAid.OutsideAward>(adapterRegistryMock.Object, loggerMock.Object);

                outsideAwardDtoToEntityAdapter = new OutsideAwardDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward,
                    Colleague.Dtos.FinancialAid.OutsideAward>>(a => a.GetAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward,
                    Colleague.Dtos.FinancialAid.OutsideAward>()).Returns(outsideAwardEntityToDtoAdapter);

                adapterRegistryMock.Setup(a => a.GetAdapter<Colleague.Dtos.FinancialAid.OutsideAward,
                    Colleague.Domain.FinancialAid.Entities.OutsideAward>()).Returns(outsideAwardDtoToEntityAdapter);

                expectedOutsideAwardDto = outsideAwardEntityToDtoAdapter.MapToType(inputOutsideAwardEntity);

                BuildOutsideAwardsService();
            }

            private void BuildOutsideAwardsService()
            {
                outsideAwardsService = new OutsideAwardsService(adapterRegistryMock.Object,
                    outsideAwardsRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }

            [Ignore]
            [TestMethod]
            public async Task OutsideAwardDto_IsNotNullTest()
            {
                actualOutsideAwardDto = await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
                Assert.IsNotNull(actualOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullOutsideAward_ArgumentNullExceptionThrownTest()
            {
                await outsideAwardsService.UpdateOutsideAwardAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullId_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.Id = null;
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullStudentId_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.StudentId = null;
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardYearCode_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardYearCode = null;
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardName_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardName = null;
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardType_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardType = null;
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task LessThanZeroAwardAmount_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardAmount = -4;
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullAwardFundingSource_ArgumentExceptionThrownTest()
            {
                expectedOutsideAwardDto.AwardFundingSource = null;
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [Ignore]
            [TestMethod]
            public async Task OutsideAwardDto_EqualsExpectedTest()
            {
                expectedOutsideAwardDto.AwardAmount = 6745;
                outsideAwardsRepositoryMock.Setup(r => r.UpdateOutsideAwardAsync(It.IsAny<Domain.FinancialAid.Entities.OutsideAward>())).ReturnsAsync(outsideAwardDtoToEntityAdapter.MapToType(expectedOutsideAwardDto));
                BuildOutsideAwardsService();
                actualOutsideAwardDto = await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
                Assert.AreEqual(expectedOutsideAwardDto.Id, actualOutsideAwardDto.Id);
                Assert.AreEqual(expectedOutsideAwardDto.StudentId, actualOutsideAwardDto.StudentId);
                Assert.AreEqual(expectedOutsideAwardDto.AwardName, actualOutsideAwardDto.AwardName);
                Assert.AreEqual(expectedOutsideAwardDto.AwardType, actualOutsideAwardDto.AwardType);
                Assert.AreEqual(expectedOutsideAwardDto.AwardAmount, actualOutsideAwardDto.AwardAmount);
                Assert.AreEqual(expectedOutsideAwardDto.AwardYearCode, actualOutsideAwardDto.AwardYearCode);
                Assert.AreEqual(expectedOutsideAwardDto.AwardFundingSource, actualOutsideAwardDto.AwardFundingSource);
            }

            [Ignore]
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task NullUpdatedOutsideAward_ExceptionThrownTest()
            {
                outsideAwardsRepositoryMock.Setup(r => r.UpdateOutsideAwardAsync(It.IsAny<Domain.FinancialAid.Entities.OutsideAward>())).ReturnsAsync(() => null);
                BuildOutsideAwardsService();
                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsNotSelf_PermissionsExceptionThrownTest()
            {
                inputOutsideAwardEntity = new OutsideAward(expectedOutsideAwardDto.Id, "foo", expectedOutsideAwardDto.AwardYearCode, expectedOutsideAwardDto.AwardName,
                    expectedOutsideAwardDto.AwardType, expectedOutsideAwardDto.AwardAmount, expectedOutsideAwardDto.AwardFundingSource);

                expectedOutsideAwardDto = outsideAwardEntityToDtoAdapter.MapToType(inputOutsideAwardEntity);

                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsNotSelfButCounselor_PermissionsExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                var roles = new List<Role>()
                {
                    new Role(1, "FINANCIAL AID COUNSELOR")
                };
                roles[0].AddPermission(new Permission("VIEW.FINANCIAL.AID.INFORMATION"));
                roleRepositoryMock.Setup(r => r.Roles).Returns(roles);
                BuildOutsideAwardsService();

                await outsideAwardsService.UpdateOutsideAwardAsync(expectedOutsideAwardDto);
            }
        }
    }
}
