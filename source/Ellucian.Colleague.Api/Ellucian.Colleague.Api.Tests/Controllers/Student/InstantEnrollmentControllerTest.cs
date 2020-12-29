// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstantEnrollmentControllerTest
    {
        [TestClass]
        public class StartPaymentGatewayRegistrationTests
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

            private InstantEnrollmentController IeController;
            private Mock<IInstantEnrollmentService> IeServiceMock;
            private Mock<ILogger> loggerMock;
            private Dtos.Student.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult IeStartPGResult;
            private Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration GoodDto;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                IeServiceMock = new Mock<IInstantEnrollmentService>();
                IeController = new InstantEnrollmentController(IeServiceMock.Object, loggerMock.Object);

                GoodDto = new Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration();
                GoodDto.ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
                GoodDto.ProposedSections.Add(new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister() { SectionId = "sect1" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Dto_Null()
            {
                var result = await IeController.PostStartInstantEnrollmentPaymentGatewayTransaction(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_ProposedSections_Null()
            {
                Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration dto = new Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration();
                dto.ProposedSections = null;
                var result = await IeController.PostStartInstantEnrollmentPaymentGatewayTransaction(dto);
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Success()
            {
                IeStartPGResult = new Dtos.Student.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult();
                IeStartPGResult.PaymentProviderRedirectUrl = "Url";
                IeServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>())).ReturnsAsync(IeStartPGResult);

                var result = await IeController.PostStartInstantEnrollmentPaymentGatewayTransaction(GoodDto);
                Assert.AreEqual(result.PaymentProviderRedirectUrl, "Url");
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Service_ArgNull_Exception()
            {
                IeServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>())).Throws(new ArgumentNullException());
                string exMsg = "";
                HttpResponseException httpEx = null;
                bool failed = true;

                try
                {
                    var result = await IeController.PostStartInstantEnrollmentPaymentGatewayTransaction(GoodDto);
                    failed = false;
                }
                catch (HttpResponseException ex)
                {
                    httpEx = ex;
                }

                if (failed)
                {
                    exMsg = await httpEx.Response.Content.ReadAsStringAsync();
                }
                // The message is contained within the string along with other elements, so check that it contains the expected message
                Assert.IsTrue(exMsg.Contains("A required argument was not provided."));
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Service_ArgException()
            {
                IeServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>())).Throws(new ArgumentException());
                string exMsg = "";
                HttpResponseException httpEx = null;
                bool failed = true;

                try
                {
                    var result = await IeController.PostStartInstantEnrollmentPaymentGatewayTransaction(GoodDto);
                    failed = false;
                }
                catch (HttpResponseException ex)
                {
                    httpEx = ex;
                }

                if (failed)
                {
                    exMsg = await httpEx.Response.Content.ReadAsStringAsync();
                }
                // The message is contained within the string along with other elements, so check that it contains the expected message
                Assert.IsTrue(exMsg.Contains("An invalid argument was supplied."));
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Service_Generic_Exception()
            {
                IeServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>())).Throws(new Exception());
                string exMsg = "";
                HttpResponseException httpEx = null;
                bool failed = true;

                try
                {
                    var result = await IeController.PostStartInstantEnrollmentPaymentGatewayTransaction(GoodDto);
                    failed = false;
                }
                catch (HttpResponseException ex)
                {
                    httpEx = ex;
                }

                if (failed)
                {
                    exMsg = await httpEx.Response.Content.ReadAsStringAsync();
                }

                // The message is contained within the string along with other elements, so check that it contains the expected message
                Assert.IsTrue(exMsg.Contains("Unable to start the payment gateway instant enrollment registration"));
            }
        }

        [TestClass]
        public class ZeroCostRegistrationForClassesTests
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

            private InstantEnrollmentController _ieController;
            private Mock<IInstantEnrollmentService> _ieServiceMock;
            private Mock<ILogger> _loggerMock;
            private Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration _zeroCostRegistration;
            private Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult _zeroCostRegistrationResult;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                _loggerMock = new Mock<ILogger>();
                _ieServiceMock = new Mock<IInstantEnrollmentService>();
                _ieController = new InstantEnrollmentController(_ieServiceMock.Object, _loggerMock.Object);

                _zeroCostRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration();
                _zeroCostRegistration.ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
                _zeroCostRegistration.ProposedSections.Add(new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "sect1"
                });

                _zeroCostRegistrationResult = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult();
                _zeroCostRegistrationResult.RegisteredSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>();
                _zeroCostRegistrationResult.RegisteredSections.Add(
                 new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection
                 {
                     SectionId = "sect1",
                     SectionCost = 0
                 });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_Dto_Null()
            {
                var result = await _ieController.PostZeroCostRegistrationForClassesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_ProposedSections_Null()
            {
                _zeroCostRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration();
                var result = await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_ProposedSections_Empty()
            {
                _zeroCostRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration();
                _zeroCostRegistration.ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
                var result = await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_Success()
            {
                _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).ReturnsAsync(_zeroCostRegistrationResult);

                var result = await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                Assert.AreEqual(result.RegisteredSections[0].SectionId, _zeroCostRegistration.ProposedSections[0].SectionId);
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_ServiceThrows_ArgumentNullException()
            {
                _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).Throws(new ArgumentNullException());
                string exMsg = "";
                HttpResponseException httpEx = null;
                bool failed = true;

                try
                {
                    var result = await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                    failed = false;
                }
                catch (HttpResponseException ex)
                {
                    httpEx = ex;
                }

                if (failed)
                {
                    exMsg = await httpEx.Response.Content.ReadAsStringAsync();
                }

                // The message is contained within the string along with other elements, so check that it contains the expected message
                Assert.IsTrue(exMsg.Contains("A required zero cost registration argument was not provided to register for classes for instant enrollment."));
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_ServiceThrows_ArgumentException()
            {
                _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).Throws(new ArgumentException());
                string exMsg = "";
                HttpResponseException httpEx = null;
                bool failed = true;

                try
                {
                    var result = await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                    failed = false;
                }
                catch (HttpResponseException ex)
                {
                    httpEx = ex;
                }

                if (failed)
                {
                    exMsg = await httpEx.Response.Content.ReadAsStringAsync();
                }

                // The message is contained within the string along with other elements, so check that it contains the expected message
                Assert.IsTrue(exMsg.Contains("An invalid argument was supplied for the zero cost registration."));
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_ServiceThrows_GenericException()
            {
                _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).Throws(new Exception());
                string exMsg = "";
                HttpResponseException httpEx = null;
                bool failed = true;

                try
                {
                    var result = await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                    failed = false;
                }
                catch (HttpResponseException ex)
                {
                    httpEx = ex;
                }

                if (failed)
                {
                    exMsg = await httpEx.Response.Content.ReadAsStringAsync();
                }

                // The message is contained within the string along with other elements, so check that it contains the expected message
                Assert.IsTrue(exMsg.Contains("Could not complete the zero cost registration for selected classes for instant enrollment."));
            }
        }

        [TestClass]
        public class GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public void GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_Null_Request()
            {
                var text = await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_Permissions_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                    ThrowsAsync(new PermissionsException());
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                {
                    CashReceiptId = "123",
                    PersonId = "0001234"
                };
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);

                var text = await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_general_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                    ThrowsAsync(new ApplicationException());
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                {
                    CashReceiptId = "123",
                    PersonId = "0001234"
                };
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);

                var text = await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_valid()
            {
                var response = new List<string>()
                {
                    "Line 1",
                    "Line 2"
                };
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                    ReturnsAsync(response);
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                {
                    CashReceiptId = "123",
                    PersonId = "0001234"
                };
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);

                var text = await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
                CollectionAssert.AreEqual(response, text.ToList());
            }
        }

        #region QueryPersonMatchResultsInstantEnrollmentByPostAsync Tests

        [TestClass]
        public class QueryPersonMatchResultsInstantEnrollmentByPostAsyncTests
        {
            private Mock<IInstantEnrollmentService> _ieServiceMock;
            private InstantEnrollmentController _ieController;
            ILogger logger = new Mock<ILogger>().Object;

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

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                _ieServiceMock = new Mock<IInstantEnrollmentService>();

                _ieController = new InstantEnrollmentController(_ieServiceMock.Object, logger);

            }

            [TestCleanup]
            public void Cleanup()
            {
                _ieController = null;
                _ieServiceMock = null;
                logger = null;
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsInstantEnrollmentByPostAsync_Success()
            {
                PersonMatchCriteriaInstantEnrollment criteriaDto;
                var resultDto = new InstantEnrollmentPersonMatchResult()
                {
                    HasPotentialMatches = true,
                    PersonId = null
                };

                criteriaDto = new PersonMatchCriteriaInstantEnrollment()
                {
                    LastName = "Enrollment",
                    FirstName = "Instant"
                };

                _ieServiceMock.Setup(s => s.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteriaDto)).ReturnsAsync(resultDto);

                var results = await _ieController.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteriaDto);
                Assert.AreEqual(resultDto.HasPotentialMatches, results.HasPotentialMatches);
                Assert.AreEqual(resultDto.PersonId, results.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryPersonMatchResultsByPostAsync_NullCriteriaException()
            {
                _ieServiceMock.Setup(s => s.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null)).Throws(new Exception("An error occurred"));
                var results = await _ieController.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null);
            }
        }
        #endregion


        [TestClass]
        public class GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public void GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_Null_Request()
            {
                await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_CashReceiptId_Permissions_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                    ThrowsAsync(new PermissionsException());
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "",
                    CashReceiptId = "123",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);

                await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_TransactionId_Permissions_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                    ThrowsAsync(new PermissionsException());
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "123",
                    CashReceiptId = "",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);

                await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);
            }

            [TestMethod]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_CashReceiptId_valid()
            {
                var convenienceFees = new List<ConvenienceFee>();
                convenienceFees.Add(new ConvenienceFee() { Code = "code", Amount = 1.2M, Description = "convience fee" });

                var paymentMethods = new List<PaymentMethod>();
                paymentMethods.Add(new PaymentMethod()
                {
                    PayMethodCode = "e-check",
                    PayMethodDescription = "e-Check",
                    ControlNumber = "8858",
                    ConfirmationNumber = "",
                    TransactionNumber = "A45D8542",
                    TransactionDescription = "Payment on account",
                    TransactionAmount = 81.2M
                });

                var response = new InstantEnrollmentCashReceiptAcknowledgement()
                {
                    CashReceiptsId = "123",
                    ReceiptDate = new DateTime(2020, 01, 01),
                    MerchantNameAddress = new List<string>() { "Mechant Name", "Address Line 1" },
                    ReceiptPayerId = "0001",
                    ReceiptPayerName = "John Smith",
                    Status = EcommerceProcessStatus.None,
                    Username = "",
                    UsernameCreationErrors = new List<string>(),
                    ConvenienceFees = convenienceFees,
                    PaymentMethods = paymentMethods,
                    RegisteredSections = new List<InstantEnrollmentRegistrationPaymentGatewayRegisteredSection>(),
                    FailedSections = new List<InstantEnrollmentRegistrationPaymentGatewayFailedSection>()
                };
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                        ReturnsAsync(response);
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "",
                    CashReceiptId = "123",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);

                var cashReceiptAcknowledgement = await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);

                Assert.AreEqual(cashReceiptAcknowledgement.CashReceiptsId, response.CashReceiptsId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptDate, response.ReceiptDate);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.MerchantNameAddress, response.MerchantNameAddress);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerId, response.ReceiptPayerId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerName, response.ReceiptPayerName);
                Assert.AreEqual(cashReceiptAcknowledgement.Status, response.Status);
                Assert.AreEqual(cashReceiptAcknowledgement.Username, response.Username);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.UsernameCreationErrors, response.UsernameCreationErrors);
                Assert.AreEqual(cashReceiptAcknowledgement.ConvenienceFees.Count(), response.ConvenienceFees.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.PaymentMethods.Count(), response.PaymentMethods.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.RegisteredSections.Count(), response.RegisteredSections.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.FailedSections.Count(), response.FailedSections.Count());
            }

            [TestMethod]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_TransactionId_valid()
            {
                var convenienceFees = new List<ConvenienceFee>();
                convenienceFees.Add(new ConvenienceFee() { Code = "code", Amount = 1.2M, Description = "convience fee" });

                var paymentMethods = new List<PaymentMethod>();
                paymentMethods.Add(new PaymentMethod()
                {
                    PayMethodCode = "VSA",
                    PayMethodDescription = "Visa",
                    ControlNumber = "8858",
                    ConfirmationNumber = "A45D8542",
                    TransactionNumber = "123",
                    TransactionDescription = "Payment on account",
                    TransactionAmount = 81.2M
                });

                var registeredSections = new List<InstantEnrollmentRegistrationPaymentGatewayRegisteredSection>();
                registeredSections.Add(new InstantEnrollmentRegistrationPaymentGatewayRegisteredSection()
                {
                    SectionId = "251",
                    Ceus = 1.5M,
                    SectionCost = 80.12M
                });
                var response = new InstantEnrollmentCashReceiptAcknowledgement()
                {
                    CashReceiptsId = "123",
                    ReceiptDate = new DateTime(2020, 01, 01),
                    MerchantNameAddress = new List<string>() { "Mechant Name", "Address Line 1" },
                    ReceiptPayerId = "0001",
                    ReceiptPayerName = "John Smith",
                    Status = EcommerceProcessStatus.None,
                    Username = "john_smith",
                    UsernameCreationErrors = new List<string>(),
                    ConvenienceFees = convenienceFees,
                    PaymentMethods = paymentMethods,
                    RegisteredSections = registeredSections,
                    FailedSections = new List<InstantEnrollmentRegistrationPaymentGatewayFailedSection>()
                };
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                        ReturnsAsync(response);
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "123",
                    CashReceiptId = "",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);

                var cashReceiptAcknowledgement = await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);

                Assert.AreEqual(cashReceiptAcknowledgement.CashReceiptsId, response.CashReceiptsId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptDate, response.ReceiptDate);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.MerchantNameAddress, response.MerchantNameAddress);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerId, response.ReceiptPayerId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerName, response.ReceiptPayerName);
                Assert.AreEqual(cashReceiptAcknowledgement.Status, response.Status);
                Assert.AreEqual(cashReceiptAcknowledgement.Username, response.Username);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.UsernameCreationErrors, response.UsernameCreationErrors);
                Assert.AreEqual(cashReceiptAcknowledgement.ConvenienceFees.Count(), response.ConvenienceFees.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.PaymentMethods.Count(), response.PaymentMethods.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.RegisteredSections.Count(), response.RegisteredSections.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.FailedSections.Count(), response.FailedSections.Count());
            }

        }


        [TestClass]
        public class GetInstantEnrollmentStudentPrograms2AsyncTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ILogger> loggerMock;

            private IEnumerable<StudentProgram> sprog;

            [TestInitialize]
            public void GetInstantEnrollmentStudentPrograms2AsyncTests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentStudentPrograms2AsyncTests_Null_StudentId_Paramter()
            {
                await ieController.GetInstantEnrollmentStudentPrograms2Async(null, true);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentStudentPrograms2AsyncTests_Empty_StudentId_Paramter()
            {
                await ieController.GetInstantEnrollmentStudentPrograms2Async(string.Empty, true);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentStudentPrograms2AsyncTests_StudentId_Permissions_Exception()
            {

                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentStudentPrograms2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                ieController = new InstantEnrollmentController(ieServiceMock.Object, loggerMock.Object);
                await ieController.GetInstantEnrollmentStudentPrograms2Async("0001", true);
            }
        }
    }
}