/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class BenefitsEnrollmentControllerTests
    {
        protected Mock<ILogger> loggerMock;
        protected Mock<IBenefitsEnrollmentService> benefitsEnrollmentServiceMock;
        protected BenefitsEnrollmentController controller;

        protected const string employeeId = "0014697";
        private EmployeeBenefitsEnrollmentPackage enrollmentPackage;
        private EmployeeBenefitsEnrollmentPoolItem benefitEnrollmentToBeUpdated;
        private EmployeeBenefitsEnrollmentPoolItem updatedBenefitsEnrollment;

        protected void InitializeDefaults()
        {
            InitializeMocks();

            enrollmentPackage = new EmployeeBenefitsEnrollmentPackage()
            {
                BenefitsEnrollmentPeriodId = "FALL19",
                EmployeeId = employeeId,
                PackageDescription = "19FALLFT",
                PackageId = "FALLFT",
                EmployeeEligibleBenefitTypes = new List<EmployeeBenefitType>()
                {
                    new EmployeeBenefitType()
                    {
                        BenefitType = "MED",
                        BenefitTypeDescription = "Medical"
                    }
                }
            };

            #region DataSetup for updating benefit enrollment pool

            benefitEnrollmentToBeUpdated = new EmployeeBenefitsEnrollmentPoolItem()
            {
                Id = "262",
                PersonId = "0018073",
                IsTrust = false,
                Prefix = "Miss",
                FirstName = "Neha",
                MiddleName = "K",
                LastName = "Prasad",
                Suffix = "Ph.D",
                AddressLine1 = "1982 Walnut Street",
                AddressLine2 = "Palm Meadows",
                City = "Beaufort",
                State = "WI",
                PostalCode = "53226",
                Country = "US",
                Relationship = "C",
                BirthDate = null,
                GovernmentId = "984561076",
                Gender = "F",
                MaritalStatus = "M",
                IsFullTimeStudent = true,
                OrganizationId = null,
                OrganizationName = null
            };

            updatedBenefitsEnrollment = new EmployeeBenefitsEnrollmentPoolItem()
            {
                Id = "262",
                PersonId = employeeId,
                IsTrust = false,
                Prefix = "Miss",
                FirstName = "Neha",
                MiddleName = "K",
                LastName = "Prasad",
                Suffix = "Ph.D",
                AddressLine1 = "1982 Walnut Street",
                AddressLine2 = "Palm Meadows",
                City = "Beaufort",
                State = "WI",
                PostalCode = "53226",
                Country = "US",
                Relationship = "C",
                BirthDate = null,
                GovernmentId = "984561076",
                Gender = "F",
                MaritalStatus = "M",
                IsFullTimeStudent = true,
                OrganizationId = null,
                OrganizationName = null
            };

            benefitsEnrollmentServiceMock.Setup(ebes => ebes.UpdateEmployeeBenefitsEnrollmentPoolAsync(null, benefitEnrollmentToBeUpdated)).ReturnsAsync(updatedBenefitsEnrollment);
            benefitsEnrollmentServiceMock.Setup(ebes => ebes.UpdateEmployeeBenefitsEnrollmentPoolAsync(null, null)).Throws(new Exception());
            benefitsEnrollmentServiceMock.Setup(ebes => ebes.UpdateEmployeeBenefitsEnrollmentPoolAsync("0018073", benefitEnrollmentToBeUpdated)).Throws(new PermissionsException());

            #endregion

            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), new HttpResponse(new StringWriter()));

            BuildController();
        }

        protected void BuildController()
        {
            controller = new BenefitsEnrollmentController(benefitsEnrollmentServiceMock.Object, loggerMock.Object);
        }

        protected void InitializeMocks()
        {
            loggerMock = new Mock<ILogger>();
            benefitsEnrollmentServiceMock = new Mock<IBenefitsEnrollmentService>();
        }

        [TestClass]
        public class BenefitsEnrollmentControllerGetTests : BenefitsEnrollmentControllerTests
        {
            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext { get; set; }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                InitializeDefaults();
            }

            #region GetEmployeeBenefitsEnrollmentEligibilityAsync

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_Throws_ArgumentNullException()
            {
                await controller.GetEmployeeBenefitsEnrollmentEligibilityAsync(string.Empty);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_Throws_PermissionsException()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentEligibilityAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await controller.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_Throws_Exception()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentEligibilityAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                await controller.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
            }

            [TestMethod]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_With_ValidData_From_Service()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentEligibilityAsync(It.IsAny<string>())).ReturnsAsync(new EmployeeBenefitsEnrollmentEligibility());
                var result = await controller.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
                Assert.IsInstanceOfType(result, typeof(EmployeeBenefitsEnrollmentEligibility));
            }

            #endregion GetEmployeeBenefitsEnrollmentEligibilityAsync

            #region GetEmployeeBenefitsEnrollmentPoolAsync

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_ArgumentNullException()
            {
                await controller.GetEmployeeBenefitsEnrollmentPoolAsync(string.Empty);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_PermissionsException()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await controller.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeBenefitsEnrollmentPoolAsync_Throws_Exception()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                await controller.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
            }

            [TestMethod]
            public async Task GetEmployeeBenefitsEnrollmentPoolAsync_With_ValidData_From_Service()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>())).ReturnsAsync(new List<EmployeeBenefitsEnrollmentPoolItem>());
                var result = await controller.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
                Assert.IsTrue(result.Count() == 0);
                Assert.IsInstanceOfType(result, typeof(List<EmployeeBenefitsEnrollmentPoolItem>));
            }

            #endregion GetEmployeeBenefitsEnrollmentPoolAsync

            #region GetEmployeeBenefitsEnrollmentPackageAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_NullEmployeeId_ExceptionThrownTest()
            {
                await controller.GetEmployeeBenefitsEnrollmentPackageAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_PermissionsException_ExceptionThrownTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentPackageAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException());
                await controller.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_GenericException_ExceptionThrownTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentPackageAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());
                await controller.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            }

            [TestMethod]
            public async Task GetEmployeeBenefitsEnrollmentEligibilityAsync_ReturnsExpectedResultTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetEmployeeBenefitsEnrollmentPackageAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(enrollmentPackage);
                var actualEnrollmentPackage = await controller.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
                Assert.AreEqual(enrollmentPackage.GetType(), actualEnrollmentPackage.GetType());
                Assert.AreEqual(enrollmentPackage.BenefitsEnrollmentPeriodId, actualEnrollmentPackage.BenefitsEnrollmentPeriodId);
                Assert.AreEqual(enrollmentPackage.EmployeeId, actualEnrollmentPackage.EmployeeId);
                Assert.AreEqual(enrollmentPackage.PackageDescription, actualEnrollmentPackage.PackageDescription);
                Assert.AreEqual(enrollmentPackage.PackageId, actualEnrollmentPackage.PackageId);
                Assert.AreEqual(enrollmentPackage.EmployeeEligibleBenefitTypes.Count(), actualEnrollmentPackage.EmployeeEligibleBenefitTypes.Count());
            }
            #endregion

            #region AddEmployeeBenefitsEnrollmentPoolAsync

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_HttpResponseException_When_EmployeeId_Is_Null()
            {
                await controller.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>());
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_HttpResponseException_When_Request_Body_Is_Null()
            {
                await controller.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, It.IsAny<EmployeeBenefitsEnrollmentPoolItem>());
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_HttpResponseException_When_Required_Fields_Are_NullOrEmpty()
            {
                await controller.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem() { Id = string.Empty });
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_PermissionsException()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>())).ThrowsAsync(new PermissionsException());
                await controller.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem());
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task AddEmployeeBenefitsEnrollmentPoolAsync_Throws_Exception()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>())).ThrowsAsync(new Exception());
                await controller.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem());
            }

            [TestMethod]
            public async Task AddEmployeeBenefitsEnrollmentPoolAsync_With_ValidData_From_Service()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.AddEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>())).ReturnsAsync(new EmployeeBenefitsEnrollmentPoolItem()
                {
                    Id = "NEWID"
                });
                var result = await controller.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem() { Id = string.Empty, LastName = "LastName" });
                Assert.IsNotNull(result.Id);
                Assert.IsInstanceOfType(result, typeof(EmployeeBenefitsEnrollmentPoolItem));
            }

            #endregion AddEmployeeBenefitsEnrollmentPoolAsync

            #region UpdateEmployeeBenefitsEnrollmentPoolAsync

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_NullEmployeeId_Exception()
            {
                await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync(string.Empty, benefitEnrollmentToBeUpdated);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_NullEmployeeBenefitsEnrollmentPoolItemObject_Exception()
            {
                await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_NullEmployeeBenefitsEnrollmentPoolID_Exception()
            {
                await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, new EmployeeBenefitsEnrollmentPoolItem() { Id = null });
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_HttpResponseException_When_Required_Fields_Are_NullOrEmpty()
            {
                benefitEnrollmentToBeUpdated.OrganizationName = string.Empty;
                benefitEnrollmentToBeUpdated.LastName = string.Empty;
                await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, benefitEnrollmentToBeUpdated);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_PermissionException()
            {
                benefitsEnrollmentServiceMock.Setup(ebes => ebes.UpdateEmployeeBenefitsEnrollmentPoolAsync("0018073", benefitEnrollmentToBeUpdated)).Throws(new PermissionsException());
                var result = await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync("0018073", benefitEnrollmentToBeUpdated);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_KeyNotFoundException()
            {
                benefitsEnrollmentServiceMock.Setup(ebes => ebes.UpdateEmployeeBenefitsEnrollmentPoolAsync(null, benefitEnrollmentToBeUpdated)).Throws(new KeyNotFoundException());
                var result = await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync(null, benefitEnrollmentToBeUpdated);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_Throws_Exception()
            {
                benefitsEnrollmentServiceMock.Setup(ebes => ebes.UpdateEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>())).ThrowsAsync(new Exception());
                await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, benefitEnrollmentToBeUpdated);
            }

            [TestMethod]
            public async Task UpdateEmployeeBenefitsEnrollmentPoolAsync_MethodExecutesWithoutErrors()
            {
                benefitsEnrollmentServiceMock.Setup(ebes => ebes.UpdateEmployeeBenefitsEnrollmentPoolAsync(It.IsAny<string>(), It.IsAny<EmployeeBenefitsEnrollmentPoolItem>())).ReturnsAsync(updatedBenefitsEnrollment);
                var actual = await controller.UpdateEmployeeBenefitsEnrollmentPoolAsync("0018073", benefitEnrollmentToBeUpdated);
                Assert.IsInstanceOfType(actual, typeof(EmployeeBenefitsEnrollmentPoolItem));
                Assert.AreEqual(benefitEnrollmentToBeUpdated.Id, actual.Id);
                Assert.AreEqual(benefitEnrollmentToBeUpdated.FirstName, actual.FirstName);
            }

            #endregion

            #region GetBenefitsEnrollmentAcknowledgementReportAsync

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetBenefitsEnrollmentAcknowledgementReportAsync_Throws_ArgumentNullException()
            {
                await controller.GetBenefitsEnrollmentAcknowledgementReportAsync(string.Empty);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetBenefitsEnrollmentAcknowledgementReportAsync_Throws_ArgumentException()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetBenefitsInformationForAcknowledgementReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                await controller.GetBenefitsEnrollmentAcknowledgementReportAsync(employeeId);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetBenefitsEnrollmentAcknowledgementReportAsync_Throws_PermissionsException()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetBenefitsInformationForAcknowledgementReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await controller.GetBenefitsEnrollmentAcknowledgementReportAsync(employeeId);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetBenefitsEnrollmentAcknowledgementReportAsync_Throws_ApplicationException()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetBenefitsInformationForAcknowledgementReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ApplicationException());
                await controller.GetBenefitsEnrollmentAcknowledgementReportAsync(employeeId);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetBenefitsEnrollmentAcknowledgementReportAsync_Throws_InvalidOperationException()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetBenefitsInformationForAcknowledgementReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                await controller.GetBenefitsEnrollmentAcknowledgementReportAsync(employeeId);
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetBenefitsEnrollmentAcknowledgementReportAsync_Throws_Exception()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetBenefitsInformationForAcknowledgementReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
                await controller.GetBenefitsEnrollmentAcknowledgementReportAsync(employeeId);
            }

            [TestMethod]
            public async Task GetBenefitsEnrollmentAcknowledgementReportAsync_With_ValidData_From_Service()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetBenefitsInformationForAcknowledgementReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new byte[1000]);
                var result = await controller.GetBenefitsEnrollmentAcknowledgementReportAsync(employeeId);

                Assert.IsInstanceOfType(result, typeof(HttpResponseMessage));
            }

            #endregion GetBenefitsEnrollmentAcknowledgementReportAsync

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                benefitsEnrollmentServiceMock = null;
            }
        }

        [TestClass]
        public class BenefitsEnrollmentController_QueryEnrollmentPeriodBenefitsAsyncTests : BenefitsEnrollmentControllerTests
        {
            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext { get; set; }
            private IEnumerable<EnrollmentPeriodBenefit> benefitDtos;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                InitializeMocks();
                benefitDtos = new List<EnrollmentPeriodBenefit>()
                {
                    new EnrollmentPeriodBenefit()
                    {
                        BenefitId = "MED",
                        BenefitDescription = "medical",
                        BenefitTypeId = "MED",
                        EnrollmentPeriodBenefitId = "MED2020"
                    },
                    new EnrollmentPeriodBenefit()
                    {
                        BenefitId = "DEN",
                        BenefitDescription = "dental",
                        BenefitTypeId = "DEN",
                        EnrollmentPeriodBenefitId = "DEN2020"
                    }
                };
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEnrollmentPeriodBenefitsAsync(It.IsAny<BenefitEnrollmentBenefitsQueryCriteria>()))
                .ReturnsAsync(benefitDtos);
                BuildController();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEnrollmentPeriodBenefitsAsync_NullCriteria_ExceptionThrownTest()
            {
                await controller.QueryEnrollmentPeriodBenefitsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEnrollmentPeriodBenefitsAsync_RepositoryExceptionCaughtTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEnrollmentPeriodBenefitsAsync(It.IsAny<BenefitEnrollmentBenefitsQueryCriteria>()))
                    .Throws(new RepositoryException());
                BuildController();
                await controller.QueryEnrollmentPeriodBenefitsAsync(new BenefitEnrollmentBenefitsQueryCriteria() { BenefitTypeId = "MED" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEnrollmentPeriodBenefitsAsync_ArgumentNullExceptionCaughtTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEnrollmentPeriodBenefitsAsync(It.IsAny<BenefitEnrollmentBenefitsQueryCriteria>()))
                    .Throws(new ArgumentNullException());
                BuildController();
                await controller.QueryEnrollmentPeriodBenefitsAsync(new BenefitEnrollmentBenefitsQueryCriteria() { BenefitTypeId = "MED" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEnrollmentPeriodBenefitsAsync_GenericExceptionCaughtTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEnrollmentPeriodBenefitsAsync(It.IsAny<BenefitEnrollmentBenefitsQueryCriteria>()))
                    .Throws(new Exception());
                BuildController();
                await controller.QueryEnrollmentPeriodBenefitsAsync(new BenefitEnrollmentBenefitsQueryCriteria() { BenefitTypeId = "MED" });
            }

            [TestMethod]
            public async Task QueryEnrollmentPeriodBenefitsAsync_ReturnsExpectedResultTest()
            {
                var benefits = await controller.QueryEnrollmentPeriodBenefitsAsync(new BenefitEnrollmentBenefitsQueryCriteria() { BenefitTypeId = "MED" });
                Assert.AreEqual(benefitDtos.Count(), benefits.Count());
            }

        }

        [TestClass]
        public class BenefitsEnrollmentController_QueryEmployeeBenefitsEnrollmentInfoAsyncTests : BenefitsEnrollmentControllerTests
        {
            /// <summary>
            /// Gets or sets the test context which provides
            /// information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }
            private EmployeeBenefitsEnrollmentInfo benefitInfoDtos;
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                InitializeMocks();
                benefitInfoDtos = new EmployeeBenefitsEnrollmentInfo()
                {
                    EnrollmentPeriodId = "19FALL",
                    ConfirmationDate = DateTime.Today,
                    BenefitPackageId = "19FALLFT",
                    EmployeeId = "0014697",
                    OptOutBenefitTypes = new List<string>() { "19FLCHARIT", "19FLOTHER" },
                    Id = "1",
                };
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEmployeeBenefitsEnrollmentInfoAsync(It.IsAny<EmployeeBenefitsEnrollmentInfoQueryCriteria>()))
                .ReturnsAsync(benefitInfoDtos);
                BuildController();
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_NullCriteria_ExceptionThrownTest()
            {
                await controller.QueryEmployeeBenefitsEnrollmentInfoAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_RepositoryExceptionCaughtTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEmployeeBenefitsEnrollmentInfoAsync(It.IsAny<EmployeeBenefitsEnrollmentInfoQueryCriteria>()))
                    .Throws(new RepositoryException());
                BuildController();
                await controller.QueryEmployeeBenefitsEnrollmentInfoAsync(new EmployeeBenefitsEnrollmentInfoQueryCriteria() { BenefitTypeId = "19FLDENT" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_ArgumentNullExceptionCaughtTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEmployeeBenefitsEnrollmentInfoAsync(It.IsAny<EmployeeBenefitsEnrollmentInfoQueryCriteria>()))
                    .Throws(new ArgumentNullException());
                BuildController();
                await controller.QueryEmployeeBenefitsEnrollmentInfoAsync(new EmployeeBenefitsEnrollmentInfoQueryCriteria() { BenefitTypeId = "19FLDENT" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryEmployeeBenefitsEnrollmentInfoAsync_GenericExceptionCaughtTest()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.QueryEmployeeBenefitsEnrollmentInfoAsync(It.IsAny<EmployeeBenefitsEnrollmentInfoQueryCriteria>()))
                    .Throws(new Exception());
                BuildController();
                await controller.QueryEmployeeBenefitsEnrollmentInfoAsync(new EmployeeBenefitsEnrollmentInfoQueryCriteria() { BenefitTypeId = "19FLDENT" });
            }

            [TestMethod]
            public async Task QueryEnrollmentPeriodBenefitsAsync_ReturnsExpectedResultTest()
            {
                var benefits = await controller.QueryEmployeeBenefitsEnrollmentInfoAsync(new EmployeeBenefitsEnrollmentInfoQueryCriteria() { BenefitTypeId = "19FLDENT" });
                Assert.AreEqual(benefitInfoDtos.BenefitPackageId, benefits.BenefitPackageId);
                Assert.AreEqual(benefitInfoDtos.ConfirmationDate, benefits.ConfirmationDate);
                Assert.AreEqual(benefitInfoDtos.EmployeeId, benefits.EmployeeId);
                Assert.AreEqual(benefitInfoDtos.EnrollmentPeriodId, benefits.EnrollmentPeriodId);
                Assert.AreEqual(benefitInfoDtos.OptOutBenefitTypes, benefits.OptOutBenefitTypes);
                Assert.AreEqual(benefitInfoDtos.Id, benefits.Id);
                Assert.IsInstanceOfType(benefits, typeof(EmployeeBenefitsEnrollmentInfo));
            }
        }

        [TestClass]
        public class BenefitsEnrollmentController_GetBeneficiaryCategoriesAsync : BenefitsEnrollmentControllerTests
        {
            /// <summary>
            /// Gets or sets the test context which provides
            /// information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }
            private List<BeneficiaryCategory> benefitInfoDtos;
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                InitializeMocks();
                benefitInfoDtos = new List<BeneficiaryCategory>()
                {
                    new BeneficiaryCategory()
                    {
                        Code = "CO",
                        Description = "Co-Owner",
                        IsPrimary = false
                    } ,
                    {
                        new BeneficiaryCategory()
                        {
                            Code = "ALT",
                            Description = "Alternate Owner",
                            IsPrimary = false
                        }
                     },
                    {
                        new BeneficiaryCategory()
                        {
                            Code = "BENEF",
                            Description = "Beneficiary",
                            IsPrimary = true
                        }
                     }
                };
                benefitsEnrollmentServiceMock.Setup(s => s.GetBeneficiaryCategoriesAsync()).ReturnsAsync(benefitInfoDtos);
                BuildController();
            }
            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetBeneficiaryCategoriesAsync_Throws_Exception()
            {
                benefitsEnrollmentServiceMock.Setup(s => s.GetBeneficiaryCategoriesAsync()).ThrowsAsync(new Exception());
                await controller.GetBeneficiaryCategoriesAsync();
            }
            [TestMethod]
            public async Task GetBeneficiaryCategoriesAsync_ReturnsExpectedResultTest()
            {
                var beneficiaryCategoryList = await controller.GetBeneficiaryCategoriesAsync();
                benefitsEnrollmentServiceMock.Setup(s => s.GetBeneficiaryCategoriesAsync()).ReturnsAsync(benefitInfoDtos);
                Assert.IsNotNull(beneficiaryCategoryList);
                Assert.IsTrue(beneficiaryCategoryList.Any());
                Assert.AreEqual(beneficiaryCategoryList.Count(), benefitInfoDtos.Count());
                Assert.IsInstanceOfType(beneficiaryCategoryList.First(), typeof(BeneficiaryCategory));
            }
        }

    }
}

