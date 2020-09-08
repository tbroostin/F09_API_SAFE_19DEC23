/* Copyright 2019-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class BenefitsEnrollmentRepositoryTests : BaseRepositorySetup
    {
        private BenefitsEnrollmentRepository repository;
        private string employeeId;
        private string benefitsEnrollmentPoolId;
        private string[] benefitsEnrollmentPoolIds;
        private GetBenefitEnrollmentEligibilityResponse benefitEnrollmentEligibilityResponse;
        private GetBenefitEnrollmentPoolResponse benefitsEnrollmentPoolResponse;
        private AddBenefitEnrollmentPoolResponse addBenefitEnrollmentPoolResponse;
        private UpdateBenEnrPoolResponse updateBenefitsEnrollmentPoolResponse;
        private EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem;

        public TestBenefitsEnrollmentRepository testRepo;
        public EmployeeBenefitsEnrollmentPackage expectedPackage;
        private IEnumerable<EnrollmentPeriodBenefit> expectedEnrollmentPeriodBenefits;
        private EmployeeBenefitsEnrollmentInfo expectedBenefitsEnrollmentInfo;
        private EmployeeBenefitsEnrollmentEligibility expectedbenefitEnrollmentEligibility;

        [TestInitialize]
        public void Initialize()
        {
            testRepo = new TestBenefitsEnrollmentRepository();

            MockInitialize();

            InitializeTestData();

            InitializeMock();

            repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        private void InitializeMock()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentEligibilityRequest, GetBenefitEnrollmentEligibilityResponse>(It.IsAny<GetBenefitEnrollmentEligibilityRequest>()))
                .ReturnsAsync(testRepo.benefitEnrollmentEligibilityResponse);

            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPoolRequest, GetBenefitEnrollmentPoolResponse>(It.IsAny<GetBenefitEnrollmentPoolRequest>()))
                .ReturnsAsync(benefitsEnrollmentPoolResponse);

            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPackageRequest, GetBenefitEnrollmentPackageResponse>(It.IsAny<GetBenefitEnrollmentPackageRequest>()))
                .ReturnsAsync(testRepo.enrollmentPackageResponse);

            transManagerMock.Setup(t => t.ExecuteAsync<AddBenefitEnrollmentPoolRequest, AddBenefitEnrollmentPoolResponse>(It.IsAny<AddBenefitEnrollmentPoolRequest>()))
                .ReturnsAsync(addBenefitEnrollmentPoolResponse);

            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(benefitsEnrollmentPoolIds);

            transManagerMock.Setup(t => t.ExecuteAsync<UpdateBenEnrPoolRequest, UpdateBenEnrPoolResponse>(It.IsAny<UpdateBenEnrPoolRequest>()))
                   .ReturnsAsync(updateBenefitsEnrollmentPoolResponse);

            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitTypeBenefitsRequest, GetBenefitTypeBenefitsResponse>(It.IsAny<GetBenefitTypeBenefitsRequest>()))
                .Returns<GetBenefitTypeBenefitsRequest>(
                (request) =>
                {
                    return Task.FromResult(testRepo.enrollmentPeriodBenefitsResponses
                        .FirstOrDefault(r => r.EnrPeriodBenTypeId == request.EnrPeriodBenTypeId));

                });

            transManagerMock.Setup(t => t.ExecuteAsync<GetEmployeeBenefitsEnrollmentInfoRequest, GetEmployeeBenefitsEnrollmentInfoResponse>(It.IsAny<GetEmployeeBenefitsEnrollmentInfoRequest>()))
               .Returns<GetEmployeeBenefitsEnrollmentInfoRequest>(
               (request) =>
               {
                   return Task.FromResult(testRepo.enrollmentBenefitsInfoResponses
                       .FirstOrDefault());

               });
        }

        private async void InitializeTestData()
        {
            employeeId = "0014697";

            benefitsEnrollmentPoolId = "1";

            benefitsEnrollmentPoolIds = new string[] { "1", "2" };

            expectedbenefitEnrollmentEligibility = testRepo.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId).Result;

            benefitsEnrollmentPoolResponse = new GetBenefitEnrollmentPoolResponse()
            {
                BenefitEnrollmentPool = new List<BenefitEnrollmentPool>()
                {
                    new BenefitEnrollmentPool() { BenEnrPoolId = "1", BeplTrustFlag = string.Empty, BeplLastName = "Last Name", BeplFullTimeFlag = "Y"},
                    new BenefitEnrollmentPool() { BenEnrPoolId = "2", BeplTrustFlag = string.Empty, BeplOrgName = "Organization Name", BeplFullTimeFlag = string.Empty},
                    new BenefitEnrollmentPool() { BenEnrPoolId = "3", BeplTrustFlag = "Y", BeplOrgName = "Organization Name"},
                    new BenefitEnrollmentPool() { BenEnrPoolId = "4", BeplTrustFlag = "N", BeplOrgName = "Organization Name"},
                }
            };

            addBenefitEnrollmentPoolResponse = new AddBenefitEnrollmentPoolResponse() { Id = "1" };

            expectedPackage = await testRepo.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);

            updateBenefitsEnrollmentPoolResponse = new UpdateBenEnrPoolResponse() { BenEnrPoolId = "1" };

            employeeBenefitsEnrollmentPoolItem = new EmployeeBenefitsEnrollmentPoolItem() { Id = benefitsEnrollmentPoolId };

            expectedEnrollmentPeriodBenefits = await testRepo.QueryEnrollmentPeriodBenefitsAsync("MED", "2020PER");

            expectedBenefitsEnrollmentInfo = await testRepo.QueryEmployeeBenefitsEnrollmentInfoAsync("0014697", "19FALL", "19FLDENT");

        }

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_Throws_ApplicationException_When_ResponseNull()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentEligibilityRequest, GetBenefitEnrollmentEligibilityResponse>(It.IsAny<GetBenefitEnrollmentEligibilityRequest>()))
                .ReturnsAsync(null);
            await repository.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
        }

        #region GetEmployeeBenefitsEnrollmentEligibilityAsync tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_NullId_ExceptionThrownTest()
        {
            await repository.GetEmployeeBenefitsEnrollmentEligibilityAsync(null);
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_ReturnsExpectedResultTest()
        {
            var actualEligibility = await repository.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.EmployeeId, actualEligibility.EmployeeId);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.EligibilityPeriod, actualEligibility.EligibilityPeriod);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.EligibilityPackage, actualEligibility.EligibilityPackage);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.EndDate, actualEligibility.EndDate);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.StartDate, actualEligibility.StartDate);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.Description, actualEligibility.Description);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.IsEnrollmentInitiated, actualEligibility.IsEnrollmentInitiated);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.IsPackageSubmitted, actualEligibility.IsPackageSubmitted);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.BenefitsPageCustomText, actualEligibility.BenefitsPageCustomText);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.BenefitsEnrollmentPageCustomText, actualEligibility.BenefitsEnrollmentPageCustomText);
            Assert.AreEqual(expectedbenefitEnrollmentEligibility.ManageDepBenPageCustomText, actualEligibility.ManageDepBenPageCustomText);
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_IneligibleReasonReturnedTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentEligibilityRequest, GetBenefitEnrollmentEligibilityResponse>(It.IsAny<GetBenefitEnrollmentEligibilityRequest>()))
                .ReturnsAsync(testRepo.benefitEnrollmentEligibilityResponseWithIneligibilityReason);
            repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            var actualEligibility = await repository.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
            Assert.IsTrue(string.IsNullOrEmpty(actualEligibility.EligibilityPeriod));

            Assert.IsTrue(!string.IsNullOrEmpty(actualEligibility.IneligibleReason));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_NullResponse_ApplicationExceptionThrownTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentEligibilityRequest, GetBenefitEnrollmentEligibilityResponse>(It.IsAny<GetBenefitEnrollmentEligibilityRequest>()))
                .ReturnsAsync(null);
            repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            await repository.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_ResponseWithError_RepositoryExceptionThrownTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentEligibilityRequest, GetBenefitEnrollmentEligibilityResponse>(It.IsAny<GetBenefitEnrollmentEligibilityRequest>()))
                .ReturnsAsync(testRepo.benefitEnrollmentEligibilityResponseWithError);
            repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            await repository.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
        }

        #endregion

        #region GetEmployeeBenefitsEnrollmentPoolAsync

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_ArgumentNullException()
        {
            await repository.GetEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>());
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_Exception()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPoolRequest, GetBenefitEnrollmentPoolResponse>(It.IsAny<GetBenefitEnrollmentPoolRequest>()))
                .ThrowsAsync(new Exception());
            await repository.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
        }

        [TestMethod, ExpectedException(typeof(RepositoryException))]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_RepositoryException_When_ResponseNull()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPoolRequest, GetBenefitEnrollmentPoolResponse>(It.IsAny<GetBenefitEnrollmentPoolRequest>()))
                .ReturnsAsync(null);
            await repository.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
        }

        [TestMethod, ExpectedException(typeof(RepositoryException))]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_RepositoryException_When_ErrorResponse()
        {
            benefitsEnrollmentPoolResponse.ErrorMessage = "Error Message";

            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPoolRequest, GetBenefitEnrollmentPoolResponse>(It.IsAny<GetBenefitEnrollmentPoolRequest>()))
                .ReturnsAsync(benefitsEnrollmentPoolResponse);

            await repository.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPoolAsync_With_Proper_Response()
        {
            var result = await repository.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);

            Assert.IsNotNull(result, "Validate that result is not null.");
            Assert.IsInstanceOfType(result, typeof(List<EmployeeBenefitsEnrollmentPoolItem>), "Validate that result matches the instance of type List<EmployeeBenefitsEnrollmentPoolItem>.");
            Assert.AreEqual(result.Count(), benefitsEnrollmentPoolResponse.BenefitEnrollmentPool.Count, "Validate that result count is matching as expected.");
            Assert.IsFalse(result.ElementAt(1).IsTrust, "Validate IsTrust flag set to false when the data is empty");
            Assert.IsTrue(result.ElementAt(2).IsTrust, "Validate IsTrust flag set to true when the data is equal to 'Y'");
            Assert.IsFalse(result.ElementAt(3).IsTrust, "Validate IsTrust flag set to false when the data is equal to 'N'");
        }

        #endregion GetEmployeeBenefitsEnrollmentPoolAsync

        #region GetEmployeeBenefitsEnrollmentPoolByIdAsync

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEmployeeBenefitsEnrollmentPoolByIdAsync_Throws_ArgumentNullException_When_EmployeeId_Is_Null()
        {
            await repository.GetEmployeeBenefitsEnrollmentPoolByIdAsync(It.IsAny<string>(), It.IsAny<string>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEmployeeBenefitsEnrollmentPoolByIdAsync_Throws_ArgumentNullException_When_BenefitsEnrollmentPoolId_Is_Null()
        {
            await repository.GetEmployeeBenefitsEnrollmentPoolByIdAsync(employeeId, It.IsAny<string>());
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public async Task GetEmployeeBenefitsEnrollmentPoolByIdAsync_Throws_Exception()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPoolRequest, GetBenefitEnrollmentPoolResponse>(It.IsAny<GetBenefitEnrollmentPoolRequest>()))
                .ThrowsAsync(new Exception());
            await repository.GetEmployeeBenefitsEnrollmentPoolByIdAsync(employeeId, benefitsEnrollmentPoolId);
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPoolByIdAsync_Returns_Null()
        {
            // empty response to make sure that get by id returns null
            transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPoolRequest, GetBenefitEnrollmentPoolResponse>(It.IsAny<GetBenefitEnrollmentPoolRequest>()))
                .ReturnsAsync(new GetBenefitEnrollmentPoolResponse() { BenefitEnrollmentPool = new List<BenefitEnrollmentPool>() { } });

            var result = await repository.GetEmployeeBenefitsEnrollmentPoolByIdAsync(employeeId, benefitsEnrollmentPoolId);

            Assert.IsNull(result, "Validate that result is null when there are no records exists");
        }

        [TestMethod]
        public async Task GetEmployeeBenefitsEnrollmentPoolByIdAsync_Returns_Proper_Data()
        {
            var result = await repository.GetEmployeeBenefitsEnrollmentPoolByIdAsync(employeeId, benefitsEnrollmentPoolId);

            Assert.IsNotNull(result, "Validate that result is not null.");
            Assert.IsInstanceOfType(result, typeof(EmployeeBenefitsEnrollmentPoolItem), "Validate that result matches the instance of type EmployeeBenefitsEnrollmentPoolItem.");
            Assert.AreEqual(result.Id, benefitsEnrollmentPoolId);
            Assert.IsFalse(result.IsTrust, "Validate IsTrust flag set to false when the data is empty");
        }

        #endregion GetEmployeeBenefitsEnrollmentPoolByIdAsync

        #region AddEmployeeBenefitsEnrollmentPoolAsync

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_ArgumentNullException_When_EmployeeId_Is_Null()
        {
            await repository.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_ArgumentNullException_When_EmployeeBenefitsEnrollmentPoolItem_Is_Null()
        {
            await repository.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, It.IsAny<EmployeeBenefitsEnrollmentPoolItem>());
        }

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_ApplicationException_When_Response_Is_Null()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<AddBenefitEnrollmentPoolRequest, AddBenefitEnrollmentPoolResponse>(It.IsAny<AddBenefitEnrollmentPoolRequest>()))
                .ReturnsAsync(null);
            await repository.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem());
        }

        [TestMethod, ExpectedException(typeof(RepositoryException))]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_RepositoryException_When_ErrorResponse()
        {
            addBenefitEnrollmentPoolResponse.Error = "Error";
            addBenefitEnrollmentPoolResponse.BenefitEnrollmentPoolErrors = new List<BenefitEnrollmentPoolErrors>()
            {
                new BenefitEnrollmentPoolErrors(){ ErrorCodes = "ONE", ErrorMessages = "INVALID ERROR"},
            };

            transManagerMock.Setup(t => t.ExecuteAsync<AddBenefitEnrollmentPoolRequest, AddBenefitEnrollmentPoolResponse>(It.IsAny<AddBenefitEnrollmentPoolRequest>()))
                .ReturnsAsync(addBenefitEnrollmentPoolResponse);

            await repository.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem());
        }

        [TestMethod]
        public async Task AddEmployeeBenefitsEnrollmentPoolAsync_With_Proper_Response()
        {
            var result = await repository.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem() { BirthDate = DateTime.Now });

            Assert.IsNotNull(result, "Validate that result is not null.");
            Assert.IsInstanceOfType(result, typeof(EmployeeBenefitsEnrollmentPoolItem), "Validate that result matches the instance of type EmployeeBenefitsEnrollmentPoolItem.");
            Assert.IsNotNull(result.Id, "Validate that id is not null after record created.");
            Assert.IsFalse(result.IsTrust, "Validate IsTrust flag set to false when the data is empty");
        }

        #endregion AddEmployeeBenefitsEnrollmentPoolAsync

        #region CheckDependentExistsAsync

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task CheckDependentExistsAsync_Throws_ArgumentNullException_When_BenefitsEnrollmentPoolId_Is_Null()
        {
            await repository.CheckDependentExistsAsync(It.IsAny<string>());
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public async Task CheckDependentExistsAsync_Throws_KeyNotFoundException_When_BenefitsEnrollmentPoolId_Is_NotFound()
        {
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);

            await repository.CheckDependentExistsAsync(benefitsEnrollmentPoolId);
        }

        [TestMethod]
        public async Task CheckDependentExistsAsync_Returns_False_When_BenefitsEnrollmentPoolId_Is_NotFound_In_Database()
        {
            var result = await repository.CheckDependentExistsAsync("3");

            Assert.IsFalse(result, "Validate that result is false when the id passed is not exists in database.");
        }

        [TestMethod]
        public async Task CheckDependentExistsAsync_Returns_True_When_BenefitsEnrollmentPoolId_Found_In_Database()
        {
            var result = await repository.CheckDependentExistsAsync(benefitsEnrollmentPoolId);

            Assert.IsTrue(result, "Validate that result is true since the id passed is exists in database.");
        }

        #endregion CheckDependentExistsAsync

        #region UpdateEmployeeBenefitsEnrollmentPoolAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_ArgumentNullException_When_EmployeeId_Is_Null()
        {
            await repository.UpdateEmployeeBenefitsEnrollmentPoolAsync(null, It.IsAny<EmployeeBenefitsEnrollmentPoolItem>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullEmployeeBenefitsEnrollmentPoolItemObject_ExceptionThrownTest()
        {
            await repository.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullEmployeeBenefitsEnrollmentPoolItemId_ExceptionThrownTest()
        {
            employeeBenefitsEnrollmentPoolItem.Id = null;
            await repository.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);
        }

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_ApplicationException_When_ResponseNull()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<UpdateBenEnrPoolRequest, UpdateBenEnrPoolResponse>(It.IsAny<UpdateBenEnrPoolRequest>()))
                .ReturnsAsync(null);
            await repository.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);
        }

        [TestMethod, ExpectedException(typeof(RepositoryException))]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_RepositoryException_When_ErrorResponse()
        {
            updateBenefitsEnrollmentPoolResponse.Error = "Error";
            updateBenefitsEnrollmentPoolResponse.UpdateBenefitEnrollmentPoolErrors = new List<UpdateBenefitEnrollmentPoolErrors>()
            {
                new UpdateBenefitEnrollmentPoolErrors { ErrorCodes = "ONE", ErrorMessages = "INVALID ERROR"},
            };

            transManagerMock.Setup(t => t.ExecuteAsync<UpdateBenEnrPoolRequest, UpdateBenEnrPoolResponse>(It.IsAny<UpdateBenEnrPoolRequest>()))
                .ReturnsAsync(updateBenefitsEnrollmentPoolResponse);

            await repository.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);
        }

        [TestMethod]
        public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_With_Proper_Response()
        {
            var expected = await repository.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);

            Assert.IsNotNull(expected, "Validate that result is not null.");
            Assert.IsInstanceOfType(expected, typeof(EmployeeBenefitsEnrollmentPoolItem), "Validate that result matches the instance of type EmployeeBenefitsEnrollmentPoolItem.");
            Assert.IsNotNull(expected.Id, "Validate that id is not null after record created.");
        }

        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();

            employeeId = string.Empty;
            benefitsEnrollmentPoolId = string.Empty;
            repository = null;
        }

        [TestClass]
        public class GetEmployeeBenefitsEnrollmentPackageAsyncTests : BenefitsEnrollmentRepositoryTests
        {
            public EmployeeBenefitsEnrollmentPackage actualPackage;

            [TestMethod]
            public async Task EmployeeBenefitsEnrollmentPackage_IsNotNullTest()
            {
                Assert.IsNotNull(await repository.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId));
            }

            [TestMethod]
            public async Task EmployeeBenefitsEnrollmentPackage_MatchesExpectedTest()
            {
                var actualPackage = await repository.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
                Assert.AreEqual(expectedPackage.EmployeeId, actualPackage.EmployeeId);
                Assert.AreEqual(expectedPackage.BenefitsEnrollmentPeriodId, actualPackage.BenefitsEnrollmentPeriodId);
                Assert.AreEqual(expectedPackage.PackageDescription, actualPackage.PackageDescription);
                Assert.AreEqual(expectedPackage.PackageId, actualPackage.PackageId);
                Assert.AreEqual(expectedPackage.EmployeeEligibleBenefitTypes.Count(), actualPackage.EmployeeEligibleBenefitTypes.Count());
            }

            [TestMethod]
            public async Task EmployeeBenefitsEnrollmentPackage_NoBenefitTypesTest()
            {
                testRepo.enrollmentPackageResponse.BenefitTypesGroup = null;
                transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPackageRequest, GetBenefitEnrollmentPackageResponse>(It.IsAny<GetBenefitEnrollmentPackageRequest>()))
                .ReturnsAsync(testRepo.enrollmentPackageResponse);
                var actualPackage = await repository.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
                Assert.IsNotNull(actualPackage);
                Assert.IsFalse(actualPackage.EmployeeEligibleBenefitTypes.Any());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullEmployeeId_ExceptionThrownTest()
            {
                await repository.GetEmployeeBenefitsEnrollmentPackageAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetBenefitEnrollmentPackageTransactionThrowsException_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPackageRequest, GetBenefitEnrollmentPackageResponse>(It.IsAny<GetBenefitEnrollmentPackageRequest>()))
                .Throws(new Exception());
                await repository.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBenefitEnrollmentPackageNullResponse_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPackageRequest, GetBenefitEnrollmentPackageResponse>(It.IsAny<GetBenefitEnrollmentPackageRequest>()))
                .ReturnsAsync(null);
                await repository.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBenefitEnrollmentPackageResponseWithError_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitEnrollmentPackageRequest, GetBenefitEnrollmentPackageResponse>(It.IsAny<GetBenefitEnrollmentPackageRequest>()))
                .ReturnsAsync(testRepo.enrollmentPackageResponseWithError);
                await repository.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            }
        }

        [TestClass]
        public class QueryEnrollmentPeriodBenefitsAsyncTests : BenefitsEnrollmentRepositoryTests
        {
            private IEnumerable<EnrollmentPeriodBenefit> actualEnrollmentPeriodBenefits;

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullBenefitTypeId_ExceptionThrownTest()
            {
                await repository.QueryEnrollmentPeriodBenefitsAsync(null);
            }

            [TestMethod]
            public async Task QueryEnrollmentPeriodBenefitsAsync_ReturnExpectedResultTest()
            {
                actualEnrollmentPeriodBenefits = await repository.QueryEnrollmentPeriodBenefitsAsync("MED");
                Assert.AreEqual(expectedEnrollmentPeriodBenefits.Count(), actualEnrollmentPeriodBenefits.Count());
                foreach (var expectedBenefit in expectedEnrollmentPeriodBenefits)
                {
                    var actualBenefit = actualEnrollmentPeriodBenefits
                        .FirstOrDefault(b => b.EnrollmentPeriodBenefitId == expectedBenefit.EnrollmentPeriodBenefitId);
                    Assert.IsNotNull(actualBenefit);
                    Assert.AreEqual(expectedBenefit.BenefitDescription, actualBenefit.BenefitDescription);
                    Assert.AreEqual(expectedBenefit.BenefitId, actualBenefit.BenefitId);
                    Assert.AreEqual(expectedBenefit.BenefitTypeId, actualBenefit.BenefitTypeId);

                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ExceptionCaught_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitTypeBenefitsRequest, GetBenefitTypeBenefitsResponse>(It.IsAny<GetBenefitTypeBenefitsRequest>()))
                    .Throws(new Exception());
                repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                await repository.QueryEnrollmentPeriodBenefitsAsync("MED");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task NullTransactionResponse_ExceptionThrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitTypeBenefitsRequest, GetBenefitTypeBenefitsResponse>(It.IsAny<GetBenefitTypeBenefitsRequest>()))
                    .ReturnsAsync(null);
                repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                await repository.QueryEnrollmentPeriodBenefitsAsync("MED");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task TransactionReturnsError_ExceptionThrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetBenefitTypeBenefitsRequest, GetBenefitTypeBenefitsResponse>(It.IsAny<GetBenefitTypeBenefitsRequest>()))
                    .ReturnsAsync(testRepo.enrollmentPeriodBenefitsResponseWithError);
                repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                await repository.QueryEnrollmentPeriodBenefitsAsync("MED");
            }

        }

        [TestClass]
        public class QueryEmployeeBenefitsEnrollmentInfoAsyncTests : BenefitsEnrollmentRepositoryTests
        {
            private EmployeeBenefitsEnrollmentInfo actualEmployeeBenefitsEnrollmentInfo;

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_NullEmployeeId_ExceptionThrownTest()
            {
                await repository.QueryEmployeeBenefitsEnrollmentInfoAsync(null, It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_NullBenefitTypeId_ExceptionThrownTest()
            {
                await repository.QueryEmployeeBenefitsEnrollmentInfoAsync(It.IsAny<string>(), It.IsAny<string>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_ExceptionCaught_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetEmployeeBenefitsEnrollmentInfoRequest, GetEmployeeBenefitsEnrollmentInfoResponse>(It.IsAny<GetEmployeeBenefitsEnrollmentInfoRequest>()))
                    .Throws(new Exception());
                repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                await repository.QueryEmployeeBenefitsEnrollmentInfoAsync("0014697", "19FALL", "19FLDENT");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_NullTransactionResponse_ExceptionThrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<GetEmployeeBenefitsEnrollmentInfoRequest, GetEmployeeBenefitsEnrollmentInfoResponse>(It.IsAny<GetEmployeeBenefitsEnrollmentInfoRequest>()))
                    .ReturnsAsync(null);
                repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                await repository.QueryEmployeeBenefitsEnrollmentInfoAsync("0014697", "19FALL", "19FLDENT");
            }

            [TestMethod]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_ReturnExpectedResultTest()
            {
                actualEmployeeBenefitsEnrollmentInfo = await repository.QueryEmployeeBenefitsEnrollmentInfoAsync("0014697", "19FALL", "19FLDENT");
                Assert.IsNotNull(actualEmployeeBenefitsEnrollmentInfo);
                Assert.AreEqual(expectedBenefitsEnrollmentInfo.ConfirmationDate, actualEmployeeBenefitsEnrollmentInfo.ConfirmationDate);
                Assert.AreEqual(expectedBenefitsEnrollmentInfo.Id, actualEmployeeBenefitsEnrollmentInfo.Id);
                Assert.AreEqual(expectedBenefitsEnrollmentInfo.EmployeeId, actualEmployeeBenefitsEnrollmentInfo.EmployeeId);
                Assert.AreEqual(expectedBenefitsEnrollmentInfo.BenefitPackageId, actualEmployeeBenefitsEnrollmentInfo.BenefitPackageId);
                Assert.AreEqual(expectedBenefitsEnrollmentInfo.EnrollmentPeriodId, actualEmployeeBenefitsEnrollmentInfo.EnrollmentPeriodId);
            }
        }

        [TestClass]
        public class SubmitBenefitElectionAsyncTests : BenefitsEnrollmentRepositoryTests
        {
            private BenefitEnrollmentCompletionInfo actualBenefitEnrollmentCompletionInfoEntity = null;

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SubmitBenefitElectionAsync_EmployeeId_ArgumentNullExceptionTest()
            {
                await repository.SubmitBenefitElectionAsync(null, It.IsAny<string>(), It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SubmitBenefitElectionAsync_EnrollmentPeriodId_ArgumentNullExceptionTest()
            {
                await repository.SubmitBenefitElectionAsync(It.IsAny<string>(), null, It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SubmitBenefitElectionAsync_BenefitPackageId_ArgumentNullExceptionTest()
            {
                await repository.SubmitBenefitElectionAsync(It.IsAny<string>(), It.IsAny<string>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task SubmitBenefitElectionAsync_ExceptionCaught_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<TxSubmitBenefitElectionRequest, TxSubmitBenefitElectionResponse>(It.IsAny<TxSubmitBenefitElectionRequest>()))
                    .Throws(new Exception());
                repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                await repository.SubmitBenefitElectionAsync("0014697", "19FALL", "19FALLFT");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task TxSubmitBenefitElectionTransactionThrowsException_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<TxSubmitBenefitElectionRequest, TxSubmitBenefitElectionResponse>(It.IsAny<TxSubmitBenefitElectionRequest>()))
                .Throws(new Exception());
                await repository.SubmitBenefitElectionAsync("0014697", "19FALL", "19FALLFT");
            }

            [TestMethod]
            public async Task SubmitBenefitElectionAsync_ReturnsValidData()
            {

                transManagerMock.Setup(t => t.ExecuteAsync<TxSubmitBenefitElectionRequest, TxSubmitBenefitElectionResponse>(It.IsAny<TxSubmitBenefitElectionRequest>()))
                    .Returns<TxSubmitBenefitElectionRequest>(
                    (request) =>
                    {
                        return Task.FromResult(testRepo.TxSubmitBenefitElectionResponseWithValidData);
                    });

                actualBenefitEnrollmentCompletionInfoEntity = await repository.SubmitBenefitElectionAsync("0014697", "19FALL", "19FALLFT");

                Assert.IsNotNull(actualBenefitEnrollmentCompletionInfoEntity);
                Assert.IsNotNull(actualBenefitEnrollmentCompletionInfoEntity.EnrollmentConfirmationDate);
                Assert.IsNotNull(actualBenefitEnrollmentCompletionInfoEntity.ErrorMessages);
                Assert.IsTrue(actualBenefitEnrollmentCompletionInfoEntity.ErrorMessages.Count() == 0);
            }

            [TestMethod]
            public async Task SubmitBenefitElectionAsync_ReturnsDataWithErrorMessage()
            {

                transManagerMock.Setup(t => t.ExecuteAsync<TxSubmitBenefitElectionRequest, TxSubmitBenefitElectionResponse>(It.IsAny<TxSubmitBenefitElectionRequest>()))
                    .Returns<TxSubmitBenefitElectionRequest>(
                    (request) =>
                    {
                        return Task.FromResult(testRepo.TxSubmitBenefitElectionResponseWithErrorMessage);
                    });

                actualBenefitEnrollmentCompletionInfoEntity = await repository.SubmitBenefitElectionAsync("0014697", "19FALL", "19FALLFT");

                Assert.IsNotNull(actualBenefitEnrollmentCompletionInfoEntity);
                Assert.IsNull(actualBenefitEnrollmentCompletionInfoEntity.EnrollmentConfirmationDate);
                Assert.IsNotNull(actualBenefitEnrollmentCompletionInfoEntity.ErrorMessages);
                Assert.IsTrue(actualBenefitEnrollmentCompletionInfoEntity.ErrorMessages.Count() > 0);
            }
        }

        [TestClass]
        public class ReOpenBenefitElectionsAsync : BenefitsEnrollmentRepositoryTests
        {
            private BenefitEnrollmentCompletionInfo actualBenefitEnrollmentCompletionInfoEntity = null;

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ReOpenBenefitElectionsAsync_EmployeeId_ArgumentNullExceptionTest()
            {
                await repository.ReOpenBenefitElectionsAsync(null, It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ReOpenBenefitElectionsAsync_EnrollmentPeriodId_ArgumentNullExceptionTest()
            {
                await repository.ReOpenBenefitElectionsAsync(It.IsAny<string>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CTXReturnsNullTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<ReopenBenefitSelectionRequest, ReopenBenefitSelectionResponse>(It.IsAny<ReopenBenefitSelectionRequest>()))
                    .ReturnsAsync(null);
                repository = new BenefitsEnrollmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                await repository.ReOpenBenefitElectionsAsync("0014697", "19FALL");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CTXThrowsException_ExceptionRethrownTest()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<ReopenBenefitSelectionRequest, ReopenBenefitSelectionResponse>(It.IsAny<ReopenBenefitSelectionRequest>()))
                .Throws(new Exception());
                await repository.ReOpenBenefitElectionsAsync("0014697", "19FALL");
            }

            [TestMethod]
            public async Task SubmitBenefitElectionAsync_ReturnsValidData()
            {

                transManagerMock.Setup(t => t.ExecuteAsync<ReopenBenefitSelectionRequest, ReopenBenefitSelectionResponse>(It.IsAny<ReopenBenefitSelectionRequest>()))
                    .Returns<ReopenBenefitSelectionRequest>(
                    (request) =>
                    {
                        return Task.FromResult(testRepo.ReopenCTXResponseWithValidData);
                    });

                actualBenefitEnrollmentCompletionInfoEntity = await repository.ReOpenBenefitElectionsAsync("0014697", "19FALL");

                Assert.IsNotNull(actualBenefitEnrollmentCompletionInfoEntity);
                Assert.IsNull(actualBenefitEnrollmentCompletionInfoEntity.EnrollmentConfirmationDate);
                Assert.IsNull(actualBenefitEnrollmentCompletionInfoEntity.ErrorMessages);
            }
        }
    }
}

