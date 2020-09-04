/* Copyright 2019-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dto = Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class BenefitsEnrollmentServiceTests : HumanResourcesServiceTestsSetup
    {
        private Mock<IBenefitsEnrollmentRepository> benefitsEnrollmentRepositoryMock;
        private Mock<IHumanResourcesReferenceDataRepository> humanResourceRepositoryMock;
        private ICurrentUserFactory benefitEnrollmentUserFactory;
        private BenefitsEnrollmentService service;
        private TestBenefitsEnrollmentRepository testRepo;
        public ITypeAdapter<EmployeeBenefitsEnrollmentPackage, Dto.EmployeeBenefitsEnrollmentPackage> enrollmentPackageEntityToDtoAdapter;

        private string personId = string.Empty;
        private EmployeeBenefitsEnrollmentEligibility employeeBenefitsEnrollmentEligibility;
        private List<EmployeeBenefitsEnrollmentPoolItem> employeeBenefitsEnrollmentPoolItems;
        private EmployeeBenefitsEnrollmentPackage enrollmentPackageEntity;
        private Colleague.Dtos.HumanResources.EmployeeBenefitsEnrollmentPackage enrollmentPackageDto;
        private EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItemEntity;
        private Dto.EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItemDto;
        private IEnumerable<EnrollmentPeriodBenefit> enrollmentPeriodBenefitEntities;
        private IEnumerable<Colleague.Dtos.HumanResources.EnrollmentPeriodBenefit> enrollmentPeriodBenefitDtos;
        private Dto.EmployeeBenefitsEnrollmentInfo benefitEnrollmentInfoDto;
        private EmployeeBenefitsEnrollmentInfo benefitEnrollmentInfoEntities;
        private BenefitEnrollmentCompletionInfo benefitEnrollmentCompletionInfoEntity_Submit;
        private Dto.BenefitEnrollmentCompletionInfo benefitEnrollmentCompletionInfoDTO_Submit;
        private BenefitEnrollmentCompletionInfo benefitEnrollmentCompletionInfoEntity_Reopen;
        private Dto.BenefitEnrollmentCompletionInfo benefitEnrollmentCompletionInfoDTO_Reopen;
        private IEnumerable<BeneficiaryCategory> beneficiaryCategoryEntities;
        private IEnumerable<Dto.BeneficiaryCategory> beneficiaryCategoryDtos;
        [TestInitialize]
        public void Initialize()
        {
            InitializeMock();

            testRepo = new TestBenefitsEnrollmentRepository();

            personId = new BenefitsEnrollmentUserFactory().CurrentUser.PersonId;

            SetupData();

            SetupMocks();

            BuildService();
        }

        private void BuildService()
        {
            service = new BenefitsEnrollmentService(benefitsEnrollmentRepositoryMock.Object, humanResourceRepositoryMock.Object, adapterRegistryMock.Object, benefitEnrollmentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        private void InitializeMock()
        {
            MockInitialize();

            benefitsEnrollmentRepositoryMock = new Mock<IBenefitsEnrollmentRepository>();
            humanResourceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            benefitEnrollmentUserFactory = new BenefitsEnrollmentUserFactory();
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            benefitEnrollmentUserFactory = null;
            benefitsEnrollmentRepositoryMock = null;
            humanResourceRepositoryMock = null;
            service = null;
            personId = string.Empty;
        }

        private void SetupMocks()
        {
            benefitsEnrollmentRepositoryMock.Setup(i => i.GetEmployeeBenefitsEnrollmentEligibilityAsync(personId)).ReturnsAsync(employeeBenefitsEnrollmentEligibility);

            benefitsEnrollmentRepositoryMock.Setup(i => i.GetEmployeeBenefitsEnrollmentPoolAsync(personId)).ReturnsAsync(employeeBenefitsEnrollmentPoolItems);

            benefitsEnrollmentRepositoryMock.Setup(i => i.GetEmployeeBenefitsEnrollmentPackageAsync(personId, It.IsAny<string>()))
                .ReturnsAsync(enrollmentPackageEntity);

            benefitsEnrollmentRepositoryMock.Setup(i => i.UpdateEmployeeBenefitsEnrollmentPoolAsync(personId, It.IsAny<EmployeeBenefitsEnrollmentPoolItem>())).ReturnsAsync(employeeBenefitsEnrollmentPoolItemEntity);

            benefitsEnrollmentRepositoryMock.Setup(i => i.CheckDependentExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            benefitsEnrollmentRepositoryMock.Setup(i => i.QueryEnrollmentPeriodBenefitsAsync("MED", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(enrollmentPeriodBenefitEntities);

            benefitsEnrollmentRepositoryMock.Setup(i => i.QueryEmployeeBenefitsEnrollmentInfoAsync(personId, It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(benefitEnrollmentInfoEntities);

            benefitsEnrollmentRepositoryMock.Setup(s => s.SubmitBenefitElectionAsync(personId, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(benefitEnrollmentCompletionInfoEntity_Submit);

            benefitsEnrollmentRepositoryMock.Setup(s => s.ReOpenBenefitElectionsAsync(personId, It.IsAny<string>())).ReturnsAsync(benefitEnrollmentCompletionInfoEntity_Reopen);

            humanResourceRepositoryMock.Setup(s => s.GetBeneficiaryCategoriesAsync()).ReturnsAsync(beneficiaryCategoryEntities);

            adapterRegistryMock.Setup(a => a.GetAdapter<EmployeeBenefitsEnrollmentEligibility, Dto.EmployeeBenefitsEnrollmentEligibility>())
               .Returns(() => new AutoMapperAdapter<EmployeeBenefitsEnrollmentEligibility, Dto.EmployeeBenefitsEnrollmentEligibility>(adapterRegistryMock.Object, loggerMock.Object));

            // entity to dto
            adapterRegistryMock.Setup(a => a.GetAdapter<EmployeeBenefitsEnrollmentPoolItem, Dto.EmployeeBenefitsEnrollmentPoolItem>())
               .Returns(() => new AutoMapperAdapter<EmployeeBenefitsEnrollmentPoolItem, Dto.EmployeeBenefitsEnrollmentPoolItem>(adapterRegistryMock.Object, loggerMock.Object));

            // dto to entity
            adapterRegistryMock.Setup(a => a.GetAdapter<Dto.EmployeeBenefitsEnrollmentPoolItem, EmployeeBenefitsEnrollmentPoolItem>())
               .Returns(() => new AutoMapperAdapter<Dto.EmployeeBenefitsEnrollmentPoolItem, EmployeeBenefitsEnrollmentPoolItem>(adapterRegistryMock.Object, loggerMock.Object));

            enrollmentPackageEntityToDtoAdapter = new BenefitsEnrollmentPackageEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(a => a.GetAdapter<EmployeeBenefitsEnrollmentPackage, Dto.EmployeeBenefitsEnrollmentPackage>())
               .Returns(enrollmentPackageEntityToDtoAdapter);

            adapterRegistryMock.Setup(a => a.GetAdapter<EmployeeBenefitsEnrollmentPoolItem, Dto.EmployeeBenefitsEnrollmentPoolItem>())
               .Returns(() => new AutoMapperAdapter<EmployeeBenefitsEnrollmentPoolItem, Dto.EmployeeBenefitsEnrollmentPoolItem>(adapterRegistryMock.Object, loggerMock.Object));

            adapterRegistryMock.Setup(a => a.GetAdapter<EnrollmentPeriodBenefit, Dto.EnrollmentPeriodBenefit>())
               .Returns(() => new AutoMapperAdapter<EnrollmentPeriodBenefit, Dto.EnrollmentPeriodBenefit>(adapterRegistryMock.Object, loggerMock.Object));

            adapterRegistryMock.Setup(a => a.GetAdapter<EmployeeBenefitsEnrollmentInfo, Dto.EmployeeBenefitsEnrollmentInfo>())
               .Returns(() => new AutoMapperAdapter<EmployeeBenefitsEnrollmentInfo, Dto.EmployeeBenefitsEnrollmentInfo>(adapterRegistryMock.Object, loggerMock.Object));

            adapterRegistryMock.Setup(a => a.GetAdapter<BenefitEnrollmentCompletionInfo, Dto.BenefitEnrollmentCompletionInfo>())
            .Returns(() => new AutoMapperAdapter<BenefitEnrollmentCompletionInfo, Dto.BenefitEnrollmentCompletionInfo>(adapterRegistryMock.Object, loggerMock.Object));

            adapterRegistryMock.Setup(a => a.GetAdapter<BeneficiaryCategory, Dto.BeneficiaryCategory>())
              .Returns(() => new AutoMapperAdapter<BeneficiaryCategory, Dto.BeneficiaryCategory>(adapterRegistryMock.Object, loggerMock.Object));

        }

        private async void SetupData()
        {
            employeeBenefitsEnrollmentEligibility = new EmployeeBenefitsEnrollmentEligibility(personId, It.IsAny<string>(), It.IsAny<string>());
            employeeBenefitsEnrollmentPoolItems = testRepo.expectedPoolItems;
            enrollmentPackageEntity = await testRepo.GetEmployeeBenefitsEnrollmentPackageAsync(personId);

            employeeBenefitsEnrollmentPoolItemEntity = testRepo.EmployeeBenefitsEnrollmentPoolItemEntity;

            employeeBenefitsEnrollmentPoolItemDto = testRepo.EmployeeBenefitsEnrollmentPoolItemDto;

            enrollmentPeriodBenefitEntities = await testRepo.QueryEnrollmentPeriodBenefitsAsync("MED");

            benefitEnrollmentInfoEntities = await testRepo.QueryEmployeeBenefitsEnrollmentInfoAsync("0014697");

            benefitEnrollmentCompletionInfoEntity_Submit = await testRepo.SubmitBenefitElectionAsync("0014697", "19FALL", "19FALLFT");

            benefitEnrollmentCompletionInfoEntity_Reopen = await testRepo.ReOpenBenefitElectionsAsync("0014697", "19FALL");

            beneficiaryCategoryEntities = await testRepo.GetBeneficiaryCategoriesAsync();
        }

        #region GetEmployeeBenefitsEnrollmentEligibilityAsync

        [TestMethod, ExpectedException(typeof(PermissionsException))]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_Throws_PermissionException()
        {
            await service.GetEmployeeBenefitsEnrollmentEligibilityAsync(It.IsAny<string>());
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_With_ValidData_From_Repository()
        {
            var result = await service.GetEmployeeBenefitsEnrollmentEligibilityAsync(personId);

            Assert.AreEqual(result.EmployeeId, personId);
            Assert.IsInstanceOfType(result, typeof(Dto.EmployeeBenefitsEnrollmentEligibility));
        }

        #endregion GetEmployeeBenefitsEnrollmentEligibilityAsync

        #region GetEmployeeBenefitsEnrollmentPoolAsync

        [TestMethod, ExpectedException(typeof(PermissionsException))]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_PermissionException()
        {
            await service.GetEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>());
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_With_ValidData_From_Repository()
        {
            var result = await service.GetEmployeeBenefitsEnrollmentPoolAsync(personId);

            Assert.IsInstanceOfType(result, typeof(List<Dto.EmployeeBenefitsEnrollmentPoolItem>));
            Assert.IsTrue((result.Count() == employeeBenefitsEnrollmentPoolItems.Count), "Results are matching with expected count");
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_SortedAsExpectedTest()
        {
            var result = await service.GetEmployeeBenefitsEnrollmentPoolAsync(personId);

            Assert.IsTrue((result.Count() == employeeBenefitsEnrollmentPoolItems.Count), "Results are matching with expected count");
            Assert.IsTrue(string.IsNullOrEmpty(result.First().OrganizationName));
            Assert.IsTrue(!string.IsNullOrEmpty(result.Last().OrganizationName));
        }

        #endregion GetEmployeeBenefitsEnrollmentPoolAsync

        #region GetEmployeeBenefitsEnrollmentPackageAsync

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPackageAsync_ReturnsExpectedTest()
        {
            enrollmentPackageDto = await service.GetEmployeeBenefitsEnrollmentPackageAsync(personId);
            Assert.IsNotNull(enrollmentPackageDto);
            Assert.AreEqual(enrollmentPackageEntity.BenefitsEnrollmentPeriodId, enrollmentPackageDto.BenefitsEnrollmentPeriodId);
            Assert.AreEqual(enrollmentPackageEntity.BenefitsEnrollmentPeriodId, enrollmentPackageDto.BenefitsEnrollmentPeriodId);
            Assert.AreEqual(enrollmentPackageEntity.EmployeeId, enrollmentPackageDto.EmployeeId);
            Assert.AreEqual(enrollmentPackageEntity.PackageDescription, enrollmentPackageDto.PackageDescription);
            Assert.AreEqual(enrollmentPackageEntity.PackageId, enrollmentPackageDto.PackageId);
            Assert.AreEqual(enrollmentPackageEntity.EmployeeEligibleBenefitTypes.Count(), enrollmentPackageDto.EmployeeEligibleBenefitTypes.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEmployeeBenefitsEnrollmentPackageAsync_NullEmployeeIdTest()
        {
            await service.GetEmployeeBenefitsEnrollmentPackageAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetEmployeeBenefitsEnrollmentPackageAsync_NoPermissionToAccessTest()
        {
            benefitEnrollmentUserFactory = new BenefitsEnrollmentDifferentUserFactory();
            BuildService();
            await service.GetEmployeeBenefitsEnrollmentPackageAsync(personId);
        }
        #endregion

        #region AddEmployeeBenefitsEnrollmentPoolAsync

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_ArgumentNullException_When_EmployeeId_Is_Null()
        {
            await service.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<Dto.EmployeeBenefitsEnrollmentPoolItem>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_ArgumentNullException_When_EmployeeBenefitsEnrollmentPoolItem_Is_Null()
        {
            await service.AddEmployeeBenefitsEnrollmentPoolAsync(personId, It.IsAny<Dto.EmployeeBenefitsEnrollmentPoolItem>());
        }

        [TestMethod, ExpectedException(typeof(PermissionsException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_PermissionException()
        {
            await service.AddEmployeeBenefitsEnrollmentPoolAsync("INVALIDPERSON", new Dto.EmployeeBenefitsEnrollmentPoolItem());
        }

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_ApplicationException_When_Repository_Returns_Null()
        {
            benefitsEnrollmentRepositoryMock.Setup(i => i.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>()))
                .ReturnsAsync(null);
            await service.AddEmployeeBenefitsEnrollmentPoolAsync(personId, new Dto.EmployeeBenefitsEnrollmentPoolItem());
        }

        [TestMethod]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_With_ValidData_From_Repository()
        {
            benefitsEnrollmentRepositoryMock.Setup(i => i.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>()))
                .ReturnsAsync(new EmployeeBenefitsEnrollmentPoolItem() { Id = "NEWID" });

            var result = await service.AddEmployeeBenefitsEnrollmentPoolAsync(personId, new Dto.EmployeeBenefitsEnrollmentPoolItem());

            Assert.IsInstanceOfType(result, typeof(Dto.EmployeeBenefitsEnrollmentPoolItem));
            Assert.IsNotNull(result.Id, "Result should contain newely generated id.");
        }

        #endregion AddEmployeeBenefitsEnrollmentPoolAsync

        #region UpdateEmployeeBenefitsEnrollmentPoolAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_NullEmployeeIdTest()
        {
            await service.UpdateEmployeeBenefitsEnrollmentPoolAsync(null, It.IsAny<Dto.EmployeeBenefitsEnrollmentPoolItem>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_NullEmployeeBenefitsEnrollmentPoolItemObject()
        {
            await service.UpdateEmployeeBenefitsEnrollmentPoolAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_NullEmployeeBenefitsEnrollmentPoolItemId()
        {
            await service.UpdateEmployeeBenefitsEnrollmentPoolAsync(personId, new Dtos.HumanResources.EmployeeBenefitsEnrollmentPoolItem() { Id = null });
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_PermissionException()
        {
            await service.UpdateEmployeeBenefitsEnrollmentPoolAsync("12345", employeeBenefitsEnrollmentPoolItemDto);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_KeyNotFoundException()
        {
            benefitsEnrollmentRepositoryMock.Setup(i => i.CheckDependentExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            await service.UpdateEmployeeBenefitsEnrollmentPoolAsync(personId, employeeBenefitsEnrollmentPoolItemDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_ApplicationException()
        {
            benefitsEnrollmentRepositoryMock.Setup(i => i.UpdateEmployeeBenefitsEnrollmentPoolAsync(personId, It.IsAny<EmployeeBenefitsEnrollmentPoolItem>())).ReturnsAsync(null);
            await service.UpdateEmployeeBenefitsEnrollmentPoolAsync(personId, employeeBenefitsEnrollmentPoolItemDto);
        }

        [TestMethod]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_ReturnsExpectedTest()
        {
            employeeBenefitsEnrollmentPoolItemDto = await service.UpdateEmployeeBenefitsEnrollmentPoolAsync(personId, employeeBenefitsEnrollmentPoolItemDto);
            Assert.IsNotNull(employeeBenefitsEnrollmentPoolItemDto);
            Assert.AreEqual(employeeBenefitsEnrollmentPoolItemEntity.Id, employeeBenefitsEnrollmentPoolItemDto.Id);
            Assert.AreEqual(employeeBenefitsEnrollmentPoolItemEntity.LastName, employeeBenefitsEnrollmentPoolItemDto.LastName);
            Assert.AreEqual(employeeBenefitsEnrollmentPoolItemEntity.GovernmentId, employeeBenefitsEnrollmentPoolItemDto.GovernmentId);
            Assert.AreEqual(employeeBenefitsEnrollmentPoolItemEntity.OrganizationName, employeeBenefitsEnrollmentPoolItemDto.OrganizationName);
            Assert.AreEqual(employeeBenefitsEnrollmentPoolItemEntity.PostalCode, employeeBenefitsEnrollmentPoolItemDto.PostalCode);
        }
        #endregion

        #region QueryEnrollmentPeriodBenefitsAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryEnrollmentPeriodBenefitsAsync_NoCriteria_ArgumentNullExceptionThrownTest()
        {
            await service.QueryEnrollmentPeriodBenefitsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QueryEnrollmentPeriodBenefitsAsync_NoBenefitTypeId_ArgumentExceptionThrownTest()
        {
            await service.QueryEnrollmentPeriodBenefitsAsync(new Dtos.HumanResources.BenefitEnrollmentBenefitsQueryCriteria());
        }

        [TestMethod]
        public async Task QueryEnrollmentPeriodBenefitsAsync_ReturnsExpectedResultTest()
        {
            enrollmentPeriodBenefitDtos = await service.QueryEnrollmentPeriodBenefitsAsync(new Dtos.HumanResources.BenefitEnrollmentBenefitsQueryCriteria()
            {
                BenefitTypeId = "MED"
            });
            Assert.AreEqual(enrollmentPeriodBenefitEntities.Count(), enrollmentPeriodBenefitDtos.Count());
            foreach (var entity in enrollmentPeriodBenefitEntities)
            {
                var dto = enrollmentPeriodBenefitDtos.FirstOrDefault(d => d.EnrollmentPeriodBenefitId == entity.EnrollmentPeriodBenefitId);
                Assert.IsNotNull(dto);
                Assert.AreEqual(entity.BenefitDescription, dto.BenefitDescription);
                Assert.AreEqual(entity.BenefitId, dto.BenefitId);
                Assert.AreEqual(entity.BenefitTypeId, dto.BenefitTypeId);
            }
        }

        #endregion

        #region QueryEmployeeBenefitsEnrollmentInfoAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_NoCriteria_ArgumentNullExceptionThrownTest()
        {
            await service.QueryEmployeeBenefitsEnrollmentInfoAsync(null);
        }

        [TestMethod]
        public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_ReturnsExpectedResultTest()
        {
            benefitEnrollmentInfoDto = await service.QueryEmployeeBenefitsEnrollmentInfoAsync(new Dtos.HumanResources.EmployeeBenefitsEnrollmentInfoQueryCriteria()
            {
                EmployeeId = "0014697",
                BenefitTypeId = "19FLDENT",
                EnrollmentPeriodId = "19FALL"
            });
            Assert.IsNotNull(benefitEnrollmentInfoEntities);
            Assert.IsNotNull(benefitEnrollmentInfoDto);
            Assert.AreEqual(benefitEnrollmentInfoEntities.Id, benefitEnrollmentInfoDto.Id);
            Assert.AreEqual(benefitEnrollmentInfoEntities.ConfirmationDate, benefitEnrollmentInfoDto.ConfirmationDate);
            Assert.AreEqual(benefitEnrollmentInfoEntities.EmployeeId, benefitEnrollmentInfoDto.EmployeeId);
            Assert.AreEqual(benefitEnrollmentInfoEntities.BenefitPackageId, benefitEnrollmentInfoDto.BenefitPackageId);
            Assert.AreEqual(benefitEnrollmentInfoEntities.EnrollmentPeriodId, benefitEnrollmentInfoDto.EnrollmentPeriodId);
            Assert.AreEqual(benefitEnrollmentInfoEntities.OptOutBenefitTypes.Count(), benefitEnrollmentInfoDto.OptOutBenefitTypes.Count());
        }
        #endregion

        #region SubmitOrReOpenBenefitElectionsAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SubmitOrReOpenBenefitElectionsAsync_NoCriteria_ArgumentNullExceptionTest()
        {
            await service.SubmitOrReOpenBenefitElectionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SubmitOrReOpenBenefitElectionsAsync_NoEnrollmentPeriodId_ArgumentExceptionTest()
        {
            await service.SubmitOrReOpenBenefitElectionsAsync(new Dtos.HumanResources.BenefitEnrollmentCompletionCriteria()
            {
                EmployeeId = "0018640",
                EnrollmentPeriodId = null,
                BenefitsPackageId = "19FALLFT",
                SubmitBenefitElections = true
            });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SubmitOrReOpenBenefitElectionsAsync_NoEmployeeId_ArgumentExceptionTest()
        {
            await service.SubmitOrReOpenBenefitElectionsAsync(new Dtos.HumanResources.BenefitEnrollmentCompletionCriteria()
            {
                EmployeeId = null,
                EnrollmentPeriodId = "19FALL",
                BenefitsPackageId = "19FALLFT",
                SubmitBenefitElections = true
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SubmitOrReOpenBenefitElectionsAsync_NoBenefitsPackageId_ArgumentExceptionTest()
        {
            await service.SubmitOrReOpenBenefitElectionsAsync(new Dtos.HumanResources.BenefitEnrollmentCompletionCriteria()
            {
                EmployeeId = null,
                EnrollmentPeriodId = "19FALL",
                BenefitsPackageId = null,
                SubmitBenefitElections = true
            });
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task SubmitOrReOpenBenefitElectionsAsync_PermissionExceptionTest()
        {
            await service.SubmitOrReOpenBenefitElectionsAsync(new Dtos.HumanResources.BenefitEnrollmentCompletionCriteria()
            {
                EmployeeId = "0018640",
                EnrollmentPeriodId = "19FALL",
                BenefitsPackageId = "19FALLFT",
                SubmitBenefitElections = true
            });
        }

        [TestMethod]
        public async Task SubmitOrReOpenBenefitElectionsAsync_Submit_ReturnsValidInformation()
        {
            benefitEnrollmentCompletionInfoDTO_Submit = await service.SubmitOrReOpenBenefitElectionsAsync(new Dtos.HumanResources.BenefitEnrollmentCompletionCriteria()
            {
                EmployeeId = "0014697",
                EnrollmentPeriodId = "19FALL",
                BenefitsPackageId = "19FALLFT",
                SubmitBenefitElections = true
            });

            Assert.IsNotNull(benefitEnrollmentCompletionInfoDTO_Submit);
            Assert.IsNotNull(benefitEnrollmentCompletionInfoEntity_Submit);
            Assert.AreEqual(benefitEnrollmentCompletionInfoEntity_Submit.EmployeeId, benefitEnrollmentCompletionInfoDTO_Submit.EmployeeId);
            Assert.AreEqual(benefitEnrollmentCompletionInfoEntity_Submit.EnrollmentPeriodId, benefitEnrollmentCompletionInfoDTO_Submit.EnrollmentPeriodId);
            Assert.AreEqual(benefitEnrollmentCompletionInfoEntity_Submit.EnrollmentConfirmationDate, benefitEnrollmentCompletionInfoDTO_Submit.EnrollmentConfirmationDate);
            Assert.AreEqual(benefitEnrollmentCompletionInfoEntity_Submit.ErrorMessages.Count(), benefitEnrollmentCompletionInfoDTO_Submit.ErrorMessages.Count());
        }

        [TestMethod]
        public async Task SubmitOrReOpenBenefitElectionsAsync_ReOpen_ReturnsValidInformation()
        {
            benefitEnrollmentCompletionInfoDTO_Reopen = await service.SubmitOrReOpenBenefitElectionsAsync(new Dtos.HumanResources.BenefitEnrollmentCompletionCriteria()
            {
                EmployeeId = "0014697",
                EnrollmentPeriodId = "19FALL",
                BenefitsPackageId = null,
                SubmitBenefitElections = false
            });

            Assert.IsNotNull(benefitEnrollmentCompletionInfoDTO_Reopen);
            Assert.IsNotNull(benefitEnrollmentCompletionInfoEntity_Reopen);
            Assert.AreEqual(benefitEnrollmentCompletionInfoEntity_Reopen.EmployeeId, benefitEnrollmentCompletionInfoDTO_Reopen.EmployeeId);
            Assert.AreEqual(benefitEnrollmentCompletionInfoEntity_Reopen.EnrollmentPeriodId, benefitEnrollmentCompletionInfoDTO_Reopen.EnrollmentPeriodId);
            Assert.AreEqual(benefitEnrollmentCompletionInfoEntity_Reopen.EnrollmentConfirmationDate, benefitEnrollmentCompletionInfoDTO_Reopen.EnrollmentConfirmationDate);
        }
        #endregion

        #region GetBeneficiaryCategoriesAsync
        [TestMethod]
        public async Task GetBeneficiaryCategoriesAsync_ReturnsExpectedResultTest()
        {
            beneficiaryCategoryDtos = await service.GetBeneficiaryCategoriesAsync();
            Assert.AreEqual(beneficiaryCategoryEntities.Count(), beneficiaryCategoryDtos.Count());
            foreach (var entity in beneficiaryCategoryEntities)
            {
                var dto = beneficiaryCategoryDtos.FirstOrDefault(d => d.Code == entity.Code);
                Assert.IsNotNull(dto);
                Assert.AreEqual(entity.Code, dto.Code);
                Assert.AreEqual(entity.Description, dto.Description);
            }
        }
        #endregion

        #region GetBenefitsInformationForAcknowledgementReport

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBenefitsInformationForAcknowledgementReport_Throws_ArgumentNullException_EmployeeId()
        {
            await service.GetBenefitsInformationForAcknowledgementReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBenefitsInformationForAcknowledgementReport_Throws_ArgumentNullException_ReportPath()
        {
            await service.GetBenefitsInformationForAcknowledgementReport(personId, It.IsAny<string>(), It.IsAny<string>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBenefitsInformationForAcknowledgementReport_Throws_ArgumentNullException_ResourceFilePath()
        {
            await service.GetBenefitsInformationForAcknowledgementReport(personId, "report path", It.IsAny<string>());
        }

        [TestMethod, ExpectedException(typeof(FileNotFoundException))]
        public async Task GetBenefitsInformationForAcknowledgementReport_Throws_ArgumentNullException()
        {
            await service.GetBenefitsInformationForAcknowledgementReport(personId, "report path", "invalid resource file path");
        }

        #endregion
    }
}
