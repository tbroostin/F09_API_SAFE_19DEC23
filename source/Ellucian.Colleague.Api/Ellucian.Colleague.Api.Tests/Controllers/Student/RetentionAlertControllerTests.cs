// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class RetentionAlertControllerTests
    {
        [TestClass]
        public class RetentionAlertController_GetCaseTypesAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<CaseType> caseTypes;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                caseTypes = new List<CaseType>()
                {
                    new CaseType()
                    {
                        CaseTypeId = "1",
                         Code= "PLAGARISM",
                         Description= "Plagiarism",
                         Category= "3",
                         Priority= "H",
                         IsActive= true,
                         AllowCaseContribution= true,
                         AvailableCommunicationCodes = new List<string>() { "3","4"}
                    },
                   new CaseType()
                    {
                        CaseTypeId = "2",
                         Code= "FABRICATION",
                         Description= "Fabrication",
                         Category= "3",
                         Priority= "H",
                         IsActive= true,
                         AllowCaseContribution= true,
                          AvailableCommunicationCodes = new List<string>() { "6","4"}
                    },
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task RetentionAlertController_GetCaseTypesAsync_ValidOutput()
            {
                retentionAlertServiceMock.Setup(s => s.GetCaseTypesAsync()).ReturnsAsync(caseTypes);
                var response = await retentionAlertController.GetCaseTypesAsync();
                var responseList = response.ToList();
                Assert.IsNotNull(response);
                Assert.AreEqual(response.Count(), caseTypes.Count());
                for (var i = 0; i < caseTypes.Count; i++)
                {
                    Assert.AreEqual(caseTypes[i].CaseTypeId, responseList[i].CaseTypeId);
                    Assert.AreEqual(caseTypes[i].Category, responseList[i].Category);
                    Assert.AreEqual(caseTypes[i].Code, responseList[i].Code);
                    Assert.AreEqual(caseTypes[i].Description, responseList[i].Description);
                    Assert.AreEqual(caseTypes[i].IsActive, responseList[i].IsActive);
                    Assert.AreEqual(caseTypes[i].AllowCaseContribution, responseList[i].AllowCaseContribution);
                    Assert.AreEqual(caseTypes[i].Priority, responseList[i].Priority);
                    Assert.AreEqual(caseTypes[i].AvailableCommunicationCodes, responseList[i].AvailableCommunicationCodes);
                }

            }
        }

        [TestClass]
        public class RetentionAlertController_GetCasePrioritiesAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<CasePriority> casePriorities;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                casePriorities = new List<CasePriority>()
                {
                    new CasePriority()
                    {
                         Code= "H",
                         Description= "High"
                    },
                   new CasePriority()
                    {
                         Code= "M",
                         Description= "Medium"
                    },
                   new CasePriority()
                    {
                         Code= "L",
                         Description= "Low"
                    }
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task RetentionAlertController_GetCasePrioritiesAsync_ValidOutput()
            {
                retentionAlertServiceMock.Setup(s => s.GetCasePrioritiesAsync()).ReturnsAsync(casePriorities);
                var response = await retentionAlertController.GetCasePrioritiesAsync();
                var responseList = response.ToList();
                Assert.IsNotNull(response);
                Assert.AreEqual(response.Count(), casePriorities.Count());
                for (var i = 0; i < casePriorities.Count; i++)
                {
                    Assert.AreEqual(casePriorities[i].Code, responseList[i].Code);
                    Assert.AreEqual(casePriorities[i].Description, responseList[i].Description);
                }

            }
        }

        [TestClass]
        public class RetentionAlertController_GetCaseCategoriesAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<CaseCategory> caseCategories;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                caseCategories = new List<CaseCategory>()
                {
                    new CaseCategory()
                    {
                         CategoryId = "1",
                         Code= "EARLY.ALERT",
                         Description= "Alerting very early",
                         CaseTypes = new List<string>() { "3","4","5"},
                         CaseClosureReasons = new List<string>() { "1","2" }
                    },
                   new CaseCategory()
                    {
                         CategoryId = "2",
                         Code= "ADVISING.ALERT",
                         Description= "Alert from Advising",
                         CaseTypes = new List<string>() { "1","6","7"},
                         CaseClosureReasons = new List<string>() { "3","2" }
                    },
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task RetentionAlertController_GetCaseCategoriesAsync_ValidOutput()
            {
                retentionAlertServiceMock.Setup(s => s.GetCaseCategoriesAsync()).ReturnsAsync(caseCategories);
                var response = await retentionAlertController.GetCaseCategoriesAsync();
                var responseList = response.ToList();
                Assert.IsNotNull(response);
                Assert.AreEqual(response.Count(), caseCategories.Count());
                for (var i = 0; i < caseCategories.Count; i++)
                {
                    Assert.AreEqual(caseCategories[i].CategoryId, responseList[i].CategoryId);
                    Assert.AreEqual(caseCategories[i].Code, responseList[i].Code);
                    Assert.AreEqual(caseCategories[i].Description, responseList[i].Description);
                    Assert.AreEqual(caseCategories[i].CaseTypes, responseList[i].CaseTypes);
                    Assert.AreEqual(caseCategories[i].CaseClosureReasons, responseList[i].CaseClosureReasons);
                }

            }
        }

        [TestClass]
        public class RetentionAlertController_QueryRetentionAlertCaseCategoryOrgRoles_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<RetentionAlertCaseCategoryOrgRoles> caseCategoryOrgRolesList;
            private List<string> caseCategoryIds;

            [TestInitialize]
            public void Initalize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                caseCategoryIds = new List<string>();
                caseCategoryIds.Add("2");

                var caseCategoryOrgRoles = new List<RetentionAlertCaseCategoryOrgRole>();
                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "1",
                    OrgRoleName = "Advisor",
                    IsAssignedInitially = "Y",
                    IsAvailableForReassignment = "N",
                    IsReportingAndAdministrative = "A"
                });

                caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                {
                    OrgRoleId = "2",
                    OrgRoleName = "Faculty",
                    IsAssignedInitially = "N",
                    IsAvailableForReassignment = "Y",
                    IsReportingAndAdministrative = "N"
                });
                caseCategoryOrgRolesList = new List<RetentionAlertCaseCategoryOrgRoles>()
                {
                    new RetentionAlertCaseCategoryOrgRoles()
                    {
                        CaseCategoryId = "2",
                        CaseCategoryOrgRoles = caseCategoryOrgRoles
                    }
                };


                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_QueryRetentionAlertCaseCategoryOrgRoles_NullArgumentCheck()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseCategoryOrgRolesAsync(It.IsAny<List<string>>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.QueryRetentionAlertCaseCategoryOrgRolesAsync(null);
                var responseList = response.ToList();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_QueryRetentionAlertCaseCategoryOrgRoles_EmptyListCheck()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseCategoryOrgRolesAsync(It.IsAny<List<string>>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.QueryRetentionAlertCaseCategoryOrgRolesAsync(new List<string>());
                var responseList = response.ToList();
            }

            [TestMethod]
            public async Task RetentionAlertController_QueryRetentionAlertCaseCategoryOrgRoles_1()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds)).ReturnsAsync(caseCategoryOrgRolesList);
                var response = await retentionAlertController.QueryRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds);
                var responseList = response.ToList();
                Assert.IsNotNull(response);
                Assert.AreEqual(response.Count(), caseCategoryOrgRolesList.Count());
                for (var i = 0; i< caseCategoryOrgRolesList.Count; i++)
                {
                    Assert.AreEqual(caseCategoryOrgRolesList[i].CaseCategoryId, responseList[i].CaseCategoryId);
                    Assert.AreEqual(caseCategoryOrgRolesList[i].CaseCategoryOrgRoles, responseList[i].CaseCategoryOrgRoles);
                }

            }

            [TestMethod]
            public async Task RetentionAlertController_QueryRetentionAlertCaseCategoryOrgRoles_2()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds)).ReturnsAsync(caseCategoryOrgRolesList);
                var response = await retentionAlertController.QueryRetentionAlertCaseCategoryOrgRolesAsync(new List<string>() { "2" });
                var responseList = response.ToList();
                Assert.IsNotNull(response);
                Assert.AreEqual(1, response.Count());

                for ( var i = 0; i < responseList.Count; i++)
                {
                    Assert.AreEqual(caseCategoryOrgRolesList[i].CaseCategoryId, responseList[i].CaseCategoryId);
                    Assert.AreEqual(caseCategoryOrgRolesList[i].CaseCategoryOrgRoles, responseList[i].CaseCategoryOrgRoles);
                }
            }

            [TestMethod]
            public async Task RetentionAlertController_QueryRetentionAlertCaseCategoryOrgRoles_3()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds)).ReturnsAsync(caseCategoryOrgRolesList);
                var response = await retentionAlertController.QueryRetentionAlertCaseCategoryOrgRolesAsync(new List<string>() { "1" });
                var responseList = response.ToList();
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Count());
            }

        }

        [TestClass]
        public class RetentionAlertController_GetCaseClosureReasonsAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<CaseClosureReason> caseClosureReasons;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                caseClosureReasons = new List<CaseClosureReason>()
                {
                    new CaseClosureReason()
                    {
                         ClosureReasonId = "1",
                         Code= "Resolved",
                         Description= "This case is resolved",
                         CaseCategories = new List<string>() {"5","6","7"}

                    },
                   new CaseClosureReason()
                    {
                         ClosureReasonId = "2",
                         Code= "Resolved",
                         Description= "This case is resolved",
                         CaseCategories = new List<string>() {"3","4","5"}

                    },
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task RetentionAlertController_GetCaseClosureReasonsAsync_ValidOutput()
            {
                retentionAlertServiceMock.Setup(s => s.GetCaseClosureReasonsAsync()).ReturnsAsync(caseClosureReasons);
                var response = await retentionAlertController.GetCaseClosureReasonsAsync();
                var responseList = response.ToList();
                Assert.IsNotNull(response);
                Assert.AreEqual(response.Count(), caseClosureReasons.Count());
                for (var i = 0; i < caseClosureReasons.Count; i++)
                {
                    Assert.AreEqual(caseClosureReasons[i].ClosureReasonId, responseList[i].ClosureReasonId);
                    Assert.AreEqual(caseClosureReasons[i].CaseCategories, responseList[i].CaseCategories);
                    Assert.AreEqual(caseClosureReasons[i].Code, responseList[i].Code);
                    Assert.AreEqual(caseClosureReasons[i].Description, responseList[i].Description);
                }

            }
        }

        [TestClass]
        public class RetentionAlertController_QueryRetentionAlertWorkCases_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<RetentionAlertWorkCase> retentionAlertWorkCase;

            private RetentionAlertQueryCriteria retentionAlertQueryCriteria;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                retentionAlertWorkCase = new List<RetentionAlertWorkCase>()
                {
                    new RetentionAlertWorkCase()
                    {
                        CaseId = "31",
                        StudentId = "0000011",
                        CaseOwner = "case owner",
                        Category = "Early.Alert",
                        Priority = "Medium",
                        DateCreated = DateTime.Now.AddDays(-10),
                        Status = "New",
                        CategoryDescription = "Category Description"
                    }
                };

                retentionAlertQueryCriteria = new RetentionAlertQueryCriteria() { CaseIds = new List<string> { "31" }, StudentSearchKeyword = "000011" };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryRetentionAlertWorkCases_PermissionsException()
            {
                retentionAlertServiceMock.Setup(x => x.GetRetentionAlertCasesAsync(retentionAlertQueryCriteria))
                    .ThrowsAsync(new PermissionsException());
                var casesDtos = await retentionAlertController.QueryRetentionAlertWorkCasesByPostAsync(retentionAlertQueryCriteria);
            }

            [TestMethod]
            public async Task QueryRetentionAlertWorkCases_ReturnsSuccess()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCasesAsync(retentionAlertQueryCriteria)).ReturnsAsync(retentionAlertWorkCase);
                var casesDtos = await retentionAlertController.QueryRetentionAlertWorkCasesByPostAsync(retentionAlertQueryCriteria);
                Assert.AreEqual(1, casesDtos.Count());
            }
        }

        [TestClass]
        public class RetentionAlertController_PostRetentionAlertCaseAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;
            private RetentionAlertCaseCreateResponse caseCreateResponse;
            private RetentionAlertCase addCase;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                caseCreateResponse = new RetentionAlertCaseCreateResponse()
                {
                    CaseId = "31",
                    CaseItemsId = "32",
                    CaseStatus = "ACTIVE",
                    OwnerIds = new List<string>(),
                    OwnerRoles = new List<string>(),
                    ErrorMessages = new List<string>()
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task AddOrUpdateRetentionAlertCase_Success()
            {
                addCase = new RetentionAlertCase
                {
                    StudentId = "0000011",
                    CaseType = "EARLY.ALERT",
                    MethodOfContact = new List<string> { "Email" },
                    Notes = new List<string> { "case notes" },
                    Summary = "case summary"
                };

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseAsync(It.IsAny<RetentionAlertCase>())).ReturnsAsync(caseCreateResponse);
                var createResponse = await retentionAlertService.AddRetentionAlertCaseAsync(addCase);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual("32", createResponse.CaseItemsId);
                Assert.AreEqual("ACTIVE", createResponse.CaseStatus);
                Assert.AreEqual(0, createResponse.OwnerIds.Count());
                Assert.AreEqual(0, createResponse.OwnerRoles.Count());
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }
        }

        [TestClass]
        public class RetentionAlertController_PutRetentionAlertCaseAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;
            private RetentionAlertCaseCreateResponse caseCreateResponse;
            private RetentionAlertCase addCase;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                caseCreateResponse = new RetentionAlertCaseCreateResponse()
                {
                    CaseId = "31",
                    CaseItemsId = "32",
                    CaseStatus = "ACTIVE",
                    OwnerIds = new List<string>(),
                    OwnerRoles = new List<string>(),
                    ErrorMessages = new List<string>()
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task AddOrUpdateRetentionAlertCase_Success()
            {
                addCase = new RetentionAlertCase
                {
                    StudentId = "0000011",
                    CaseType = "EARLY.ALERT",
                    MethodOfContact = new List<string> { "Email" },
                    Notes = new List<string> { "case notes" },
                    Summary = "case summary"
                };

                retentionAlertServiceMock.Setup(s => s.UpdateRetentionAlertCaseAsync(It.IsAny<string>(), It.IsAny<RetentionAlertCase>())).ReturnsAsync(caseCreateResponse);
                var createResponse = await retentionAlertService.UpdateRetentionAlertCaseAsync("31", addCase);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual("32", createResponse.CaseItemsId);
                Assert.AreEqual("ACTIVE", createResponse.CaseStatus);
                Assert.AreEqual(0, createResponse.OwnerIds.Count());
                Assert.AreEqual(0, createResponse.OwnerRoles.Count());
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }
        }

        [TestClass]
        public class RetentionAlertController_QueryRetentionAlertContributionsAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<RetentionAlertWorkCase> retentionAlertContributions;

            private ContributionsQueryCriteria contributionsQueryCriteria;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                retentionAlertContributions = new List<RetentionAlertWorkCase>()
                {
                    new RetentionAlertWorkCase()
                    {
                        CaseId = "31",
                        StudentId = "0000011",
                        CaseOwner = "case owner",
                        Category = "Early.Alert",
                        Priority = "Medium",
                        DateCreated = DateTime.Now.AddDays(-10),
                        Status = "New",
                        CategoryDescription = "Category Description",
                        Summary = "Case Summary"
                    }
                };

                contributionsQueryCriteria = new ContributionsQueryCriteria() { IncludeCasesOverOneYear = false, IncludeClosedCases = false, IncludeOwnedCases = false };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetRetentionAlertContributions_PermissionsException()
            {
                retentionAlertServiceMock.Setup(x => x.GetRetentionAlertContributionsAsync(contributionsQueryCriteria))
                    .ThrowsAsync(new PermissionsException());
                var contributionsDtos = await retentionAlertController.QueryRetentionAlertContributionsAsync(contributionsQueryCriteria);
            }

            [TestMethod]
            public async Task QueryRetentionAlertWorkCases_ReturnsSuccess()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertContributionsAsync(contributionsQueryCriteria)).ReturnsAsync(retentionAlertContributions);
                var contributionsDtos = await retentionAlertController.QueryRetentionAlertContributionsAsync(contributionsQueryCriteria);
                Assert.AreEqual(1, contributionsDtos.Count());
            }
        }

        [TestClass]
        public class RetentionAlertController_GetRetentionAlertOpenCases_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private List<RetentionAlertOpenCase> retentionAlertOpenCase;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                retentionAlertOpenCase = new List<RetentionAlertOpenCase>()
                {
                    new RetentionAlertOpenCase()
                    {
                        Category = "Early Alert",
                        ThirtyDaysOld = new List<string> { "30,40,50" },
                        SixtyDaysOld = new List<string> { "45" },
                        NinetyDaysOld = new List<string> { "" },
                        OverNinetyDaysOld = new List<string> { "12,23" },
                        TotalOpenCases = new List<string> { "6" }
                    },
                    new RetentionAlertOpenCase()
                    {
                        Category = "Financial Problem",
                        ThirtyDaysOld = new List<string> { "30" },
                        SixtyDaysOld = new List<string> { "45,40,50, 55" } ,
                        NinetyDaysOld = new List<string> { "34" },
                        OverNinetyDaysOld = new List<string> { "11" },
                        TotalOpenCases = new List<string> { "7" }
                    }
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetRetentionAlertOpenCases_PermissionsException()
            {
                retentionAlertServiceMock.Setup(x => x.GetRetentionAlertOpenCasesAsync())
                    .ThrowsAsync(new PermissionsException());
                var openCasesDto = await retentionAlertController.GetRetentionAlertOpenCasesAsync();
            }

            [TestMethod]
            public async Task QueryRetentionAlertWorkCases_ReturnsSuccess()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertOpenCasesAsync()).ReturnsAsync(retentionAlertOpenCase);
                var openCasesDto = await retentionAlertController.GetRetentionAlertOpenCasesAsync();
                Assert.AreEqual(2, openCasesDto.Count());
            }
        }

        [TestClass]
        public class RetentionAlertController_GetRetentionAlertCaseDetailAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private RetentionAlertCaseDetail retentionAlertCaseDetail;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                List<RetentionAlertCaseHistory> caseHistory = new List<RetentionAlertCaseHistory>()
                {
                    new RetentionAlertCaseHistory()
                    {
                        CaseItemType = "Case Item Type 1",
                        ContactMethod = "Phone",
                        DetailedNote =  new List<string>{ "Detailed Notes" },
                        UpdatedBy = "0000011",
                        CaseClosureReason = "3"
                    },
                    new RetentionAlertCaseHistory()
                    {
                        CaseItemType = "Case Item Type 2",
                        ContactMethod = "Email",
                        DetailedNote = new List<string>{ "Detailed Notes 2" },
                        UpdatedBy = "0000012",
                        CaseClosureReason = ""
                    }
                };

                retentionAlertCaseDetail = new RetentionAlertCaseDetail()
                {
                    CaseHistory = caseHistory,
                    CaseId = "32",
                    Status = "New",
                    CaseOwner = "case owner",
                    CaseType = "Case Type",
                    CategoryName = "EARLY.ALERT",                    
                    CategoryId = "1",
                    CreatedBy = "0000010",
                    Priority = "Medium",
                    StudentId = "0000015",
                    CaseReassignmentList = new List<RetentionAlertCaseReassignmentDetail>()
                    {
                        new RetentionAlertCaseReassignmentDetail()
                        {
                            AssignmentCode ="67",
                            Title ="Jose",
                            Role="0",
                            IsSelected = false
                        },
                        new RetentionAlertCaseReassignmentDetail()
                        {
                            AssignmentCode ="68",
                            Title ="Miny",
                            Role="0",
                            IsSelected = true
                        }
                    }
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetRetentionAlertCaseDetail_PermissionsException()
            {
                retentionAlertServiceMock.Setup(x => x.GetRetentionAlertCaseDetailAsync(It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                var contributionsDtos = await retentionAlertController.GetRetentionAlertCaseDetailAsync("31");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetRetentionAlertCaseDetail_NullCaseId()
            {
                retentionAlertServiceMock.Setup(x => x.GetRetentionAlertCaseDetailAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var contributionsDtos = await retentionAlertController.GetRetentionAlertCaseDetailAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetRetentionAlertCaseDetail_Exception()
            {
                retentionAlertServiceMock.Setup(x => x.GetRetentionAlertCaseDetailAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var contributionsDtos = await retentionAlertController.GetRetentionAlertCaseDetailAsync("31");
            }

            [TestMethod]
            public async Task GetRetentionAlertCaseDetail_ReturnsSuccess()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseDetailAsync(It.IsAny<string>())).ReturnsAsync(retentionAlertCaseDetail);
                var caseDetailDtos = await retentionAlertController.GetRetentionAlertCaseDetailAsync("31");
                Assert.IsNotNull(caseDetailDtos);
                Assert.AreEqual("32", caseDetailDtos.CaseId);
                Assert.AreEqual(2, caseDetailDtos.CaseReassignmentList.Count());
            }

            [TestMethod]
            public async Task GetRetentionAlertCaseDetail_ReturnsSuccess2()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseDetailAsync(It.IsAny<string>())).ReturnsAsync(retentionAlertCaseDetail);
                var caseDetailDtos = await retentionAlertController.GetRetentionAlertCaseDetailAsync("31");
                Assert.IsNotNull(caseDetailDtos);

                Assert.AreEqual(retentionAlertCaseDetail.CaseReassignmentList.Count(), caseDetailDtos.CaseReassignmentList.Count());

                Assert.AreEqual(retentionAlertCaseDetail.CaseId, caseDetailDtos.CaseId);
                Assert.AreEqual(retentionAlertCaseDetail.Status, caseDetailDtos.Status);
                Assert.AreEqual(retentionAlertCaseDetail.CaseOwner, caseDetailDtos.CaseOwner);
                Assert.AreEqual(retentionAlertCaseDetail.CaseType, caseDetailDtos.CaseType);
                Assert.AreEqual(retentionAlertCaseDetail.CategoryName, caseDetailDtos.CategoryName);
                Assert.AreEqual(retentionAlertCaseDetail.CategoryId, caseDetailDtos.CategoryId);
                Assert.AreEqual(retentionAlertCaseDetail.CreatedBy, caseDetailDtos.CreatedBy);
                Assert.AreEqual(retentionAlertCaseDetail.Priority, caseDetailDtos.Priority);
                Assert.AreEqual(retentionAlertCaseDetail.StudentId, caseDetailDtos.StudentId);
            }
        }
        [TestClass]
        public class RetentionAlertController_WorkCaseAction_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;
            private RetentionAlertWorkCaseActionResponse caseActionResponse;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                caseActionResponse = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = "31",
                    HasError = false,
                    ErrorMessages = new List<string>()
                };

                // controller that will be tested using mock objects
                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseNote_Success()
            {
                RetentionAlertWorkCaseNote note = new RetentionAlertWorkCaseNote()
                {
                    Notes = new List<string> { "case notes" },
                    Summary = "case summary"
                };

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseNoteAsync("31", It.IsAny<RetentionAlertWorkCaseNote>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.AddRetentionAlertCaseNoteAsync("31", note);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual(false, createResponse.HasError);
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AddRetentionAlertCaseNote_Failure()
            {
                RetentionAlertWorkCaseNote note = null;

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseNoteAsync("31", It.IsAny<RetentionAlertWorkCaseNote>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.AddRetentionAlertCaseNoteAsync("31", note);

            }

            [TestMethod]
            public async Task AddRetentionAlertCaseCommCodeAsync_Success()
            {
                RetentionAlertWorkCaseCommCode commCode = new RetentionAlertWorkCaseCommCode()
                {
                    CommunicationCode = "AMC5FAC"
                };

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseCommCodeAsync("31", It.IsAny<RetentionAlertWorkCaseCommCode>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.AddRetentionAlertCaseCommCodeAsync("31", commCode);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual(false, createResponse.HasError);
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AddRetentionAlertCaseCommCodeAsync_Failure()
            {
                RetentionAlertWorkCaseCommCode commCode = null;

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseCommCodeAsync("31", It.IsAny<RetentionAlertWorkCaseCommCode>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.AddRetentionAlertCaseCommCodeAsync("31", commCode);

            }

            [TestMethod]
            public async Task AddRetentionAlertCaseTypeAsync_Success()
            {
                RetentionAlertWorkCaseType type = new RetentionAlertWorkCaseType
                {
                    CaseType = "5",
                    Notes = new List<string> { "case notes" }
                };

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseTypeAsync("31", It.IsAny<RetentionAlertWorkCaseType>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.AddRetentionAlertCaseTypeAsync("31", type);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual(false, createResponse.HasError);
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AddRetentionAlertCaseTypeAsync_Failure()
            {
                RetentionAlertWorkCaseType type = null;

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseTypeAsync("31", It.IsAny<RetentionAlertWorkCaseType>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.AddRetentionAlertCaseTypeAsync("31", type);

            }

            [TestMethod]
            public async Task ChangeRetentionAlertCasePriorityAsync_Success()
            {
                RetentionAlertWorkCasePriority priority = new RetentionAlertWorkCasePriority()
                {
                    Priority = "H"
                };

                retentionAlertServiceMock.Setup(s => s.ChangeRetentionAlertCasePriorityAsync("31", It.IsAny<RetentionAlertWorkCasePriority>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.ChangeRetentionAlertCasePriorityAsync("31", priority);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual(false, createResponse.HasError);
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ChangeRetentionAlertCasePriorityAsync_Failure()
            {
                RetentionAlertWorkCasePriority priority = null;

                retentionAlertServiceMock.Setup(s => s.ChangeRetentionAlertCasePriorityAsync("31", It.IsAny<RetentionAlertWorkCasePriority>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.ChangeRetentionAlertCasePriorityAsync("31", priority);

            }

            [TestMethod]
            public async Task CloseRetentionAlertCaseAsync_Success()
            {
                RetentionAlertWorkCaseClose close = new RetentionAlertWorkCaseClose()
                {
                    ClosureReason = "WITHDRAW",
                    Summary = "Closing Retention Alert Case because the student has withdrawn",
                    Notes = new List<string> { "Closing Retention Alert Case because the student has withdrawn due to poor grades." }
                };

                retentionAlertServiceMock.Setup(s => s.CloseRetentionAlertCaseAsync("31", It.IsAny<RetentionAlertWorkCaseClose>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.CloseRetentionAlertCaseAsync("31", close);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual(false, createResponse.HasError);
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CloseRetentionAlertCaseAsync_Failure()
            {
                RetentionAlertWorkCaseClose close = null;

                retentionAlertServiceMock.Setup(s => s.CloseRetentionAlertCaseAsync("31", It.IsAny<RetentionAlertWorkCaseClose>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.CloseRetentionAlertCaseAsync("31", close);

            }

            [TestMethod]
            public async Task ReassignRetentionAlertCaseAsync_Success()
            {
                RetentionAlertWorkCaseReassign reassign = new RetentionAlertWorkCaseReassign()
                {
                    UpdatedBy = "John Snow",
                    ReassignOwners = new List<RetentionAlertCaseReassignmentDetail>()
                    {
                        new RetentionAlertCaseReassignmentDetail()
                        {
                            AssignmentCode = "112",
                            Title = "Rahul JD",
                            IsSelected = true,
                            Role = "0"
                        }
                    },
                    Notes = new List<string> { "Reassigning Retention Alert Case because the student needs change." }
                };

                retentionAlertServiceMock.Setup(s => s.ReassignRetentionAlertWorkCaseAsync("31", It.IsAny<RetentionAlertWorkCaseReassign>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.ReassignRetentionAlertWorkCaseAsync("31", reassign);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual("31", createResponse.CaseId);
                Assert.AreEqual(false, createResponse.HasError);
                Assert.AreEqual(0, createResponse.ErrorMessages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReassignRetentionAlertWorkCaseAsync_Failure()
            {
                RetentionAlertWorkCaseReassign reassign = null;

                retentionAlertServiceMock.Setup(s => s.ReassignRetentionAlertWorkCaseAsync("31", It.IsAny<RetentionAlertWorkCaseReassign>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.ReassignRetentionAlertWorkCaseAsync("31", reassign);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAddRetentionAlertCaseReminderAsync_Failure()
            {
                RetentionAlertWorkCaseSetReminder reminder = null;

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseReminderAsync("31", It.IsAny<RetentionAlertWorkCaseSetReminder>())).ReturnsAsync(caseActionResponse);
                var createResponse = await retentionAlertController.AddRetentionAlertCaseReminderAsync("31", reminder);
            }

            [TestMethod]
            public async Task PostAddRetentionAlertCaseReminderAsync_Success()
            {
                RetentionAlertWorkCaseSetReminder dto = new RetentionAlertWorkCaseSetReminder()
                {
                    UpdatedBy = "1234567",
                    ReminderDate = new DateTime(2019, 01, 01),
                    Summary = "Summary",
                    Notes = new List<string>() { "Notes" }
                };

                retentionAlertServiceMock.Setup(s => s.AddRetentionAlertCaseReminderAsync("31", It.IsAny<RetentionAlertWorkCaseSetReminder>())).ReturnsAsync(caseActionResponse);
                var response = await retentionAlertController.AddRetentionAlertCaseReminderAsync("31", dto);

                Assert.AreEqual(caseActionResponse.CaseId, response.CaseId);
                Assert.AreEqual(caseActionResponse.HasError, response.HasError);
                CollectionAssert.AreEqual(caseActionResponse.ErrorMessages.ToList(), response.ErrorMessages.ToList());
            }

            [TestMethod]
            public async Task RetentionAlertController_PostManageRetentionAlertCaseRemindersAsync_Success1()
            {                
                RetentionAlertWorkCaseManageReminders reminders = new RetentionAlertWorkCaseManageReminders()
                {
                    UpdatedBy = "1234567",
                    Reminders = new List<RetentionAlertWorkCaseManageReminder>()
                    {
                        new RetentionAlertWorkCaseManageReminder()
                        {
                            CaseItemsId = "1",
                            ClearReminderDate = "Y"
                        }
                    }
                };

                retentionAlertServiceMock.Setup(s => s.ManageRetentionAlertCaseRemindersAsync(It.IsAny<string>(), It.IsAny<RetentionAlertWorkCaseManageReminders>())).ReturnsAsync(caseActionResponse);
                var response = await retentionAlertController.ManageRetentionAlertCaseRemindersAsync("1", reminders);

                Assert.AreEqual(caseActionResponse.CaseId, response.CaseId);
                Assert.AreEqual(caseActionResponse.HasError, response.HasError);
                CollectionAssert.AreEqual(caseActionResponse.ErrorMessages.ToList(), response.ErrorMessages.ToList());
            }
        }

        [TestClass]
        public class RetentionAlertController_GetRetentionAlertPermissionsAsync_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private RetentionAlertPermissions permissions;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                permissions = new RetentionAlertPermissions()
                {
                    CanWorkCases = true,
                    CanWorkAnyCase = true,
                    CanContributeToCases = true
                };

                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            public async Task GetRetentionAlertPermissionsAsync_Success()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertPermissionsAsync()).ReturnsAsync(permissions);
                var response = await retentionAlertController.GetRetentionAlertPermissionsAsync();
                Assert.IsNotNull(response);
                Assert.IsTrue(response.CanWorkCases);
                Assert.IsTrue(response.CanWorkAnyCase);
                Assert.IsTrue(response.CanContributeToCases);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetRetentionAlertPermissionsAsync_Exception()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertPermissionsAsync()).ThrowsAsync(new Exception());
                var reesponse = await retentionAlertController.GetRetentionAlertPermissionsAsync();
            }
        }

        [TestClass]
        public class RetentionAlertController_QueryRetentionAlertGroupOfCasesSummaryByPostAsync
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;

            private RetentionAlertGroupOfCasesSummary groupOfCasesSummary;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                groupOfCasesSummary = new RetentionAlertGroupOfCasesSummary()
                {
                    Summary = "Advising Alert",
                    EntityCases = new List<RetentionAlertGroupOfCases>()
                    {
                        new RetentionAlertGroupOfCases()
                        {
                            Name = "John Smith",
                            CaseIds = new List<string>(){"1", "2", "3"}
                        },
                        new RetentionAlertGroupOfCases()
                        {
                            Name = "Jay Thunderbolt",
                            CaseIds = new List<string>(){"1","3"}
                        }
                    },
                    RoleCases = new List<RetentionAlertGroupOfCases>()
                    {
                        new RetentionAlertGroupOfCases()
                        {
                            Name = "ADVISOR",
                            CaseIds = new List<string>(){"4", "5", "6"}
                        },
                        new RetentionAlertGroupOfCases()
                        {
                            Name = "FACULTY",
                            CaseIds = new List<string>(){"4", "6"}
                        }
                    }
                };

                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_QueryRetentionAlertGroupOfCasesSummaryByPostAsync_NullArgumentCheck()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseOwnerSummaryAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.GetRetentionAlertCaseOwnerSummaryAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_QueryRetentionAlertGroupOfCasesSummaryByPostAsync_EmptyArgumentCheck()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseOwnerSummaryAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.GetRetentionAlertCaseOwnerSummaryAsync("");
            }

            [TestMethod]            
            public async Task RetentionAlertController_QueryRetentionAlertGroupOfCasesSummaryByPostAsync_Success()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertCaseOwnerSummaryAsync(It.IsAny<string>())).ReturnsAsync(groupOfCasesSummary);
                var response = await retentionAlertController.GetRetentionAlertCaseOwnerSummaryAsync( "1"  );

                Assert.AreEqual(groupOfCasesSummary.Summary, response.Summary);
                for (var i = 0; i < response.EntityCases.Count; i++)
                {
                    Assert.AreEqual(groupOfCasesSummary.EntityCases[i].Name, response.EntityCases[i].Name);
                    Assert.AreEqual(groupOfCasesSummary.EntityCases[i].NumberOfCases, response.EntityCases[i].NumberOfCases);
                    for (var j = 0; j < response.EntityCases[i].CaseIds.Count; j++)
                    {
                        Assert.AreEqual(groupOfCasesSummary.EntityCases[i].CaseIds[j], response.EntityCases[i].CaseIds[j]);
                    }                    
                }

                for (var i = 0; i < response.RoleCases.Count; i++)
                {
                    Assert.AreEqual(groupOfCasesSummary.RoleCases[i].Name, response.RoleCases[i].Name);
                    Assert.AreEqual(groupOfCasesSummary.RoleCases[i].NumberOfCases, response.RoleCases[i].NumberOfCases);
                    for (var j = 0; j < response.RoleCases[i].CaseIds.Count; j++)
                    {
                        Assert.AreEqual(groupOfCasesSummary.RoleCases[i].CaseIds[j], response.RoleCases[i].CaseIds[j]);
                    }
                }
            }
        }

        [TestClass]
        public class RetentionAlertController_GetRetentionAlertEmailPreferenceAsync
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_GetRetentionAlertEmailPreferenceAsync_NullArgumentCheck()
            {
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertEmailPreferenceAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.GetRetentionAlertEmailPreferenceAsync(null);
            }

            [TestMethod]
            public async Task RetentionAlertController_GetRetentionAlertEmailPreferenceAsync_Success1()
            {
                RetentionAlertSendEmailPreference raEmailPreference = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertEmailPreferenceAsync(It.IsAny<string>())).ReturnsAsync(raEmailPreference);
                var response = await retentionAlertController.GetRetentionAlertEmailPreferenceAsync("1");

                Assert.AreEqual(raEmailPreference.HasSendEmailFlag, response.HasSendEmailFlag);
                Assert.AreEqual(raEmailPreference.Message, response.Message);
            }

            [TestMethod]
            public async Task RetentionAlertController_GetRetentionAlertEmailPreferenceAsync_Success2()
            {
                RetentionAlertSendEmailPreference raEmailPreference = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = false,
                    Message = "Your current setting is to not receive e-mail reminders when cases are assigned to you."
                };
                retentionAlertServiceMock.Setup(s => s.GetRetentionAlertEmailPreferenceAsync(It.IsAny<string>())).ReturnsAsync(raEmailPreference);
                var response = await retentionAlertController.GetRetentionAlertEmailPreferenceAsync("1");

                Assert.AreEqual(raEmailPreference.HasSendEmailFlag, response.HasSendEmailFlag);
                Assert.AreEqual(raEmailPreference.Message, response.Message);
            }
        }

        [TestClass]
        public class RetentionAlertController_PostSetRetentionAlertEmailPreferenceAsync
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private RetentionAlertController retentionAlertController;
            private Mock<IRetentionAlertService> retentionAlertServiceMock;
            private IRetentionAlertService retentionAlertService;
            private ILogger logger;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                retentionAlertServiceMock = new Mock<IRetentionAlertService>();
                retentionAlertService = retentionAlertServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                retentionAlertController = new RetentionAlertController(retentionAlertService, logger);
                retentionAlertController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                retentionAlertController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                retentionAlertController = null;
                retentionAlertService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_PostSetRetentionAlertEmailPreferenceAsync_NullArgumentCheck1()
            {
                retentionAlertServiceMock.Setup(s => s.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.SetRetentionAlertEmailPreferenceAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_PostSetRetentionAlertEmailPreferenceAsync_NullArgumentCheck2()
            {
                retentionAlertServiceMock.Setup(s => s.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.SetRetentionAlertEmailPreferenceAsync("", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RetentionAlertController_PostSetRetentionAlertEmailPreferenceAsync_NullArgumentCheck3()
            {
                retentionAlertServiceMock.Setup(s => s.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ThrowsAsync(new ArgumentNullException());
                var response = await retentionAlertController.SetRetentionAlertEmailPreferenceAsync("1", null);
            }

            [TestMethod]
            public async Task RetentionAlertController_GetRetentionAlertEmailPreferenceAsync_Success1()
            {
                RetentionAlertSendEmailPreference raEmailPreference = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };
                retentionAlertServiceMock.Setup(s => s.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ReturnsAsync(raEmailPreference);
                var response = await retentionAlertController.SetRetentionAlertEmailPreferenceAsync("1", raEmailPreference);

                Assert.AreEqual(raEmailPreference.HasSendEmailFlag, response.HasSendEmailFlag);
                Assert.AreEqual(raEmailPreference.Message, response.Message);
            }

            [TestMethod]
            public async Task RetentionAlertController_GetRetentionAlertEmailPreferenceAsync_Success2()
            {
                RetentionAlertSendEmailPreference raEmailPreference = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = false,
                    Message = "Your current setting is to not receive e-mail reminders when cases are assigned to you."
                };
                retentionAlertServiceMock.Setup(s => s.SetRetentionAlertEmailPreferenceAsync(It.IsAny<string>(), It.IsAny<RetentionAlertSendEmailPreference>())).ReturnsAsync(raEmailPreference);
                var response = await retentionAlertController.SetRetentionAlertEmailPreferenceAsync("1", raEmailPreference);

                Assert.AreEqual(raEmailPreference.HasSendEmailFlag, response.HasSendEmailFlag);
                Assert.AreEqual(raEmailPreference.Message, response.Message);
            }
        }
    }
}
