// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Coordination.Finance.Services;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Finance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class RegistrationBillingServiceTests : BaseRepositorySetup
    {
        RegistrationBillingService service;
        Mock<IAdapterRegistry> adapterMock;
        Mock<ICurrentUserFactory> userFactoryMock;
        Mock<IRoleRepository> roleRepoMock;
        Mock<IRegistrationBillingRepository> rbRepoMock;
        Mock<IAccountsReceivableRepository> arRepoMock;
        Mock<IAccountDueRepository> adRepoMock;
        Mock<IFinanceConfigurationRepository> fcRepoMock;
        Mock<IPaymentRepository> pmtRepoMock;
        Mock<IPaymentPlanRepository> ppRepoMock;
        Mock<IRuleRepository> ruleRepoMock;
        Mock<IDocumentRepository> docRepoMock;
        Mock<IImmediatePaymentService> ipcServiceMock;
        Mock<IPaymentPlanProcessor> planProcessorMock;
        Mock<IApprovalService> apprSvcMock;

        Domain.Entities.Role adminRole = new Domain.Entities.Role(101, "Finance Administrator");
        Collection<IpcPaymentReqs> ipcps = TestIpcPaymentReqsRepository.IpcPaymentReqs;
        List<Domain.Finance.Entities.PaymentRequirement> payReqs = TestPaymentRequirementRepository.PaymentRequirements;

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();

            adapterMock = new Mock<IAdapterRegistry>();
            userFactoryMock = new Mock<ICurrentUserFactory>();
            roleRepoMock = new Mock<IRoleRepository>();
            rbRepoMock = new Mock<IRegistrationBillingRepository>();
            arRepoMock = new Mock<IAccountsReceivableRepository>();
            adRepoMock = new Mock<IAccountDueRepository>();
            fcRepoMock = new Mock<IFinanceConfigurationRepository>();
            ppRepoMock = new Mock<IPaymentPlanRepository>();
            ruleRepoMock = new Mock<IRuleRepository>();
            pmtRepoMock = new Mock<IPaymentRepository>();
            apprSvcMock = new Mock<IApprovalService>();
            docRepoMock = new Mock<IDocumentRepository>();
            ipcServiceMock = new Mock<IImmediatePaymentService>();
            planProcessorMock = new Mock<IPaymentPlanProcessor>();

            adminRole.AddPermission(new Domain.Entities.Permission(FinancePermissionCodes.ViewStudentAccountActivity));

            service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
        }

        #region GetRegistrationPaymentControl tests

        [TestClass]
        public class GetPaymentControl : RegistrationBillingServiceTests
        {
            string rpcId = "145";
            string studentId = "1234567";
            string studentId2 = "2345678";
            string termId = "2013/FA";
            Domain.Finance.Entities.RegistrationPaymentStatus entityStatus = Domain.Finance.Entities.RegistrationPaymentStatus.New;
            RegistrationPaymentStatus dtoStatus = RegistrationPaymentStatus.New;
            List<string> regSections = new List<string>() { "111", "222", "333" };
            List<string> invoiceIds = new List<string>() { "987", "654" };

            [TestInitialize]
            public void GetRegistrationPaymentControl_Initialize()
            {
                base.Initialize();

                // Mock Adapters
                var rpcDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationPaymentControl, RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, RegistrationPaymentControl>()).Returns(rpcDtoAdapter);


                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(repo => repo.GetPaymentControl(It.IsAny<string>()))
                    .Returns<string>(id => 
                        {
                            int test;
                            // Any numeric ID returns a record, anything else returns null
                            if (Int32.TryParse(id, out test))
                            {
                                var rpc1 = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                                foreach (var sec in regSections) { rpc1.AddRegisteredSection(sec); }
                                foreach (var inv in invoiceIds) { rpc1.AddInvoice(inv); }
                                return rpc1;
                            }
                            return null;
                        });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentControl_NullId()
            {
                var result = service.GetPaymentControl(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentControl_EmptyId()
            {
                var result = service.GetPaymentControl(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void RegistrationBillingService_GetPaymentControl_InvalidId()
            {
                var result = service.GetPaymentControl("INVALID");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_ValidDtoId()
            {
                SetupStudentUser();
                var result = service.GetPaymentControl(rpcId);

                Assert.AreEqual(rpcId, result.Id);
            }

            
            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_ValidDtoStudentId()
            {
                SetupStudentUser();
                var result = service.GetPaymentControl(rpcId);

                Assert.AreEqual(studentId, result.StudentId);
            }

            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_ValidDtoTermId()
            {
                SetupStudentUser();
                var result = service.GetPaymentControl(rpcId);

                Assert.AreEqual(termId, result.TermId);
            }

            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_ValidDtoPaymentStatus()
            {
                SetupStudentUser();
                var result = service.GetPaymentControl(rpcId);

                Assert.AreEqual(dtoStatus, result.PaymentStatus);
            }

            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_ValidDtoRegisteredSectionIds()
            {
                SetupStudentUser();
                var result = service.GetPaymentControl(rpcId);

                CollectionAssert.AreEqual(regSections, result.RegisteredSectionIds.ToList());
            }

            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_ValidDtoInvoiceIds()
            {
                SetupStudentUser();
                var result = service.GetPaymentControl(rpcId);

                CollectionAssert.AreEqual(invoiceIds, result.InvoiceIds.ToList());
            }

            /// <summary>
            /// User is neither self, nor admin, nor proxy
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentControl_NotStudentNoPermission()
            {
                SetupStudentUser();
                var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId2, termId, entityStatus);
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(repo => repo.GetPaymentControl(It.IsAny<string>())).Returns(rpc);
                var result = service.GetPaymentControl(rpcId);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_AdminUser()
            {
                SetupAdminUser();
                var result = service.GetPaymentControl(rpcId);

                // Verify that we got the expected data for the expected student, and that the current
                // user is NOT the student
                Assert.AreEqual(rpcId, result.Id);
                Assert.AreEqual(studentId, result.StudentId);
                Assert.AreNotEqual(userFactoryMock.Object.CurrentUser.PersonId, result.StudentId);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentControl_AdminUserNoPermissions()
            {
                SetupAdminUserWithNoPermissions();
                service.GetPaymentControl(rpcId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentControl_ProxyUser()
            {
                SetupProxyUser();
                studentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId;
                var result = service.GetPaymentControl(rpcId);

                // Verify that we got the expected data for the expected student, and that the current
                // user is NOT the student
                Assert.AreEqual(rpcId, result.Id);
                Assert.AreEqual(studentId, result.StudentId);
                Assert.AreNotEqual(userFactoryMock.Object.CurrentUser.PersonId, result.StudentId);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentControl_ProxyUserDifferentPerson()
            {
                SetupProxyUserForDifferentPerson();
                service.GetPaymentControl(rpcId);
            }
        }

        #endregion

        #region GetStudentPaymentControls tests

        [TestClass]
        public class GetStudentPaymentControls : RegistrationBillingServiceTests
        {
            string rpcId = "145";
            string studentId = "1234567";
            string studentId2 = "2345678";
            string termId = "2013/FA";
            Domain.Finance.Entities.RegistrationPaymentStatus entityStatus = Domain.Finance.Entities.RegistrationPaymentStatus.New;
            RegistrationPaymentStatus dtoStatus = RegistrationPaymentStatus.New;
            List<string> regSections = new List<string>() { "111", "222", "333" };
            List<string> invoiceIds = new List<string>() { "987", "654" };

            [TestInitialize]
            public void Initialize_GetStudentPaymentControls()
            {
                base.Initialize();

                // Mock Adapters
                var rpcDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>()).Returns(rpcDtoAdapter);
                

                rbRepoMock.Setup<IEnumerable<Domain.Finance.Entities.RegistrationPaymentControl>>(repo => repo.GetStudentPaymentControls(It.IsAny<string>()))
                    .Returns<string>(studentId =>
                    {
                        var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                        foreach (var sec in regSections) { rpc.AddRegisteredSection(sec); }
                        foreach (var inv in invoiceIds) { rpc.AddInvoice(inv); }
                        var rpcList = new List<Domain.Finance.Entities.RegistrationPaymentControl>() { rpc };
                        return rpcList;
                    });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetStudentPaymentControls_NullId()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetStudentPaymentControls_EmptyId()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(String.Empty);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_ValidId()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                Assert.AreEqual(rpcId, result.Id);
            }

            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_ValidStudentId()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                Assert.AreEqual(studentId, result.StudentId);
            }

            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_ValidTermId()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                Assert.AreEqual(termId, result.TermId);
            }

            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_ValidPaymentStatus()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                Assert.AreEqual(dtoStatus, result.PaymentStatus);
            }

            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_ValidRegisteredSectionIds()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                CollectionAssert.AreEqual(regSections, result.RegisteredSectionIds.ToList());
            }

            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_ValidInvoiceIds()
            {
                SetupStudentUser();
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                CollectionAssert.AreEqual(invoiceIds, result.InvoiceIds.ToList());
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetStudentPaymentControls_NoPermissions()
            {
                SetupStudentUser();
                var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId2, termId, entityStatus);
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(repo => repo.GetPaymentControl(It.IsAny<string>())).Returns(rpc);
                var result = service.GetStudentPaymentControls(studentId2);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_AdminUser()
            {
                SetupAdminUser();
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                // Verify that we got the expected data for the expected student, and that the current
                // user is NOT the student
                Assert.AreEqual(rpcId, result.Id);
                Assert.AreEqual(studentId, result.StudentId);
                Assert.AreNotEqual(userFactoryMock.Object.CurrentUser.PersonId, result.StudentId);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetStudentPaymentControls_AdminNoPermissions()
            {
                SetupAdminUserWithNoPermissions();
                service.GetStudentPaymentControls(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetStudentPaymentControls_ProxyUser()
            {
                SetupProxyUser();
                studentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId;
                var result = service.GetStudentPaymentControls(studentId).ElementAt(0);

                // Verify that we got the expected data for the expected student, and that the current
                // user is NOT the student
                Assert.AreEqual(rpcId, result.Id);
                Assert.AreEqual(studentId, result.StudentId);
                Assert.AreNotEqual(userFactoryMock.Object.CurrentUser.PersonId, result.StudentId);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetStudentPaymentControls_ProxyUserForDifferentPerson()
            {
                SetupProxyUserForDifferentPerson();
                service.GetStudentPaymentControls(studentId);
            }
        }

        #endregion

        #region GetPaymentControlDocument tests

        [TestClass]
        public class GetPaymentControlDocument : RegistrationBillingServiceTests
        {
            string rpcId = "145";
            string studentId = "1234567";
            string termId = "2013/FA";
            Domain.Finance.Entities.RegistrationPaymentStatus entityStatus = Domain.Finance.Entities.RegistrationPaymentStatus.New;
            List<string> regSections = new List<string>() { "111", "222", "333" };
            List<string> invoiceIds = new List<string>() { "987", "654" };

            string docId = "256";
            List<string> docText = new List<string>() { "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." };


            [TestInitialize]
            public void Initialize_GetPaymentControlDocument()
            {
                // Mock Adapters
                var rpcDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>()).Returns(rpcDtoAdapter);

                //var rpcList = new List<Domain.Finance.Entities.RegistrationPaymentControl>() { rpc };

                var doc = new Domain.Base.Entities.TextDocument(docText);
             
                var docDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>()).Returns(docDtoAdapter);

                docRepoMock.Setup<Domain.Base.Entities.TextDocument>(
                    repo => repo.Build(It.IsAny<string>(), "IPC.REGISTRATION", It.IsAny<string>(), It.IsAny<string>(), null)).Returns(doc);
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(
                    repo => repo.GetPaymentControl(It.IsAny<string>())).Returns<string>(id =>
                        {
                            // Return different results based on the value passed in as the key
                            switch (id)
                            {
                                case null: throw new ArgumentNullException();
                                case "": throw new ArgumentNullException();
                                case "INVALID": return null;
                                default:
                                    {
                                        var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                                        foreach (var sec in regSections) { rpc.AddRegisteredSection(sec); }
                                        foreach (var inv in invoiceIds) { rpc.AddInvoice(inv); };
                                        return rpc;
                                    }
                            }
                        });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentControlDocument_NullId()
            {
                var result = service.GetPaymentControlDocument(null, docId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentControlDocument_EmptyId()
            {
                var result = service.GetPaymentControlDocument(string.Empty, docId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void RegistrationBillingService_GetPaymentControlDocument_InvalidId()
            {
                var result = service.GetPaymentControlDocument("INVALID", docId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentControlDocument_NullDocumentId()
            {
                var result = service.GetPaymentControlDocument(rpcId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentControlDocument_EmptyDocumentId()
            {
                var result = service.GetPaymentControlDocument(rpcId, string.Empty);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentControlDocument_ValidId()
            {
                SetupStudentUser();
                var result = service.GetPaymentControlDocument(rpcId, docId);

                CollectionAssert.AreEqual(docText, result.Text);
            }

            /// <summary>
            /// User is neither self, nor admin, nor proxy
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentControlDocument_NoPermission()
            {
                SetupStudentUser();
                var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, "foo", termId, entityStatus);
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(repo => repo.GetPaymentControl(It.IsAny<string>())).Returns(rpc);
                var result = service.GetPaymentControlDocument(rpcId, docId);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentControlDocument_AdminUser()
            {
                SetupAdminUser();
                Assert.IsNotNull(service.GetPaymentControlDocument(rpcId, docId));
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentControlDocument_AdminUserNoPermissions()
            {
                SetupAdminUserWithNoPermissions();
                service.GetPaymentControlDocument(rpcId, docId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentControlDocument_ProxyUser()
            {
                SetupProxyUser();
                studentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId;
                Assert.IsNotNull(service.GetPaymentControlDocument(rpcId, docId));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentControlDocument_ProxyUserDifferentPerson()
            {
                SetupProxyUserForDifferentPerson();
                service.GetPaymentControlDocument(rpcId, docId);
            }
        }

        #endregion

        #region ApproveRegistrationTerms tests

        [TestClass]
        public class ApproveRegistrationTerms : RegistrationBillingServiceTests
        {
            [TestInitialize]
            public void Initialize_ApproveRegistrationTerms()
            {
                Initialize();

                // Mock Adapters
                var ptaAdapter = new PaymentTermsAcceptanceDtoAdapter(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<PaymentTermsAcceptance, Domain.Finance.Entities.PaymentTermsAcceptance>()).Returns(ptaAdapter);

                var rtaDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>()).Returns(rtaDtoAdapter);

                ApproveRegistrationTermsResponse validTermsResponse = new ApproveRegistrationTermsResponse()
                {
                    AcknowledgementDocumentId = "1",
                    ErrorMessage = null,
                    RegistrationApprovalId = "100",
                    TermsDocumentId = "IPCREGTC",
                    TermsResponseId = "100"
                };
                ApproveRegistrationTermsResponse errorTermsResponse = new ApproveRegistrationTermsResponse()
                {
                    AcknowledgementDocumentId = null,
                    ErrorMessage = "Could not accept the terms.",
                    RegistrationApprovalId = null,
                    TermsDocumentId = null,
                    TermsResponseId = null
                };

                transManagerMock.Setup<ApproveRegistrationTermsResponse>(
                    trans => trans.Execute<ApproveRegistrationTermsRequest, ApproveRegistrationTermsResponse>(It.IsAny<ApproveRegistrationTermsRequest>()))
                        .Returns<ApproveRegistrationTermsRequest>(request =>
                        {
                            if (request.StudentId == "1234567")
                            {
                                return validTermsResponse;
                            }
                            return errorTermsResponse;
                        });

                rbRepoMock.Setup(repo => repo.ApproveRegistrationTerms(It.IsAny<Domain.Finance.Entities.PaymentTermsAcceptance>())).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", "1234567", DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC") { AcknowledgementDocumentId = "123" });
                rbRepoMock.Setup(repo => repo.GetTermsApproval(It.IsAny<string>())).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", "1234567", DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC") { AcknowledgementDocumentId = "123" });
                apprSvcMock.Setup(svc => svc.GetApprovalDocument("123")).Returns(new Dtos.Base.ApprovalDocument()
                    {
                        Id = "1",
                        PersonId = "1234567",
                        Text = new List<string>() { "This is some", "acknowledgement text." }
                    });
                apprSvcMock.Setup(svc => svc.GetApprovalDocument("124")).Returns(new Dtos.Base.ApprovalDocument()
                {
                        Id = "2",
                        PersonId = "1234567",
                        Text = new List<string>() { "This is also some", "acknowledgement text." }
                });
                apprSvcMock.Setup(svc => svc.GetApprovalResponse(It.IsAny<string>())).Returns(new Dtos.Base.ApprovalResponse()
                {
                    Id = "1",
                    PersonId = "1234567",
                    DocumentId = "124",
                    IsApproved = true,
                    Received = DateTime.Now.AddMinutes(-3),
                    UserId = "john_smith"
                });

                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

            }

            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void ApproveRegistrationTerms_UnauthorizedUser()
            {
                SetupStudentUser();
                var acceptance = new PaymentTermsAcceptance()
                {
                    StudentId = "0001234",
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                var result = service.ApproveRegistrationTerms(acceptance);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ApproveRegistrationTerms_Valid()
            {
                SetupStudentUser();
                var acceptance = new PaymentTermsAcceptance()
                {
                    StudentId = "1234567",
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                var result = service.ApproveRegistrationTerms(acceptance);
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// Admin user cannot accept terms
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void ApproveRegistrationTerms_AdminNotAuthorized()
            {
                SetupAdminUser();
                var acceptance = new PaymentTermsAcceptance()
                {
                    StudentId = "0001234",
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                service.ApproveRegistrationTerms(acceptance);
            }

            /// <summary>
            /// Proxy user cannot accept terms
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void ApproveRegistrationTerms_ProxyNotAuthorized()
            {
                SetupProxyUser();
                var acceptance = new PaymentTermsAcceptance()
                {
                    StudentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId,
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                service.ApproveRegistrationTerms(acceptance);
            }
        }

        #endregion

        #region ApproveRegistrationTerms2 tests

        [TestClass]
        public class ApproveRegistrationTerms2 : RegistrationBillingServiceTests
        {
            [TestInitialize]
            public void Initialize_ApproveRegistrationTerms2()
            {
                Initialize();
                SetupStudentUser();

                // Mock Adapters
                var pta2Adapter = new PaymentTermsAcceptance2DtoAdapter(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<PaymentTermsAcceptance2, Domain.Finance.Entities.PaymentTermsAcceptance>()).Returns(pta2Adapter);

                var rtaDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>()).Returns(rtaDtoAdapter);

                ApproveRegistrationTermsResponse validTermsResponse = new ApproveRegistrationTermsResponse()
                {
                    AcknowledgementDocumentId = "1",
                    ErrorMessage = null,
                    RegistrationApprovalId = "100",
                    TermsDocumentId = "IPCREGTC",
                    TermsResponseId = "100"
                };
                ApproveRegistrationTermsResponse errorTermsResponse = new ApproveRegistrationTermsResponse()
                {
                    AcknowledgementDocumentId = null,
                    ErrorMessage = "Could not accept the terms.",
                    RegistrationApprovalId = null,
                    TermsDocumentId = null,
                    TermsResponseId = null
                };

                transManagerMock.Setup<ApproveRegistrationTermsResponse>(
                    trans => trans.Execute<ApproveRegistrationTermsRequest, ApproveRegistrationTermsResponse>(It.IsAny<ApproveRegistrationTermsRequest>()))
                        .Returns<ApproveRegistrationTermsRequest>(request =>
                        {
                            if (request.StudentId == "1234567")
                            {
                                return validTermsResponse;
                            }
                            return errorTermsResponse;
                        });

                rbRepoMock.Setup(repo => repo.ApproveRegistrationTerms(It.IsAny<Domain.Finance.Entities.PaymentTermsAcceptance>())).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", "1234567", DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC"));
                rbRepoMock.Setup(repo => repo.GetTermsApproval(It.IsAny<string>())).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", "1234567", DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC"));
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void ApproveRegistrationTerms2_UnauthorizedUser()
            {
                var acceptance = new PaymentTermsAcceptance2()
                {
                    StudentId = "0001234",
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                var result = service.ApproveRegistrationTerms2(acceptance);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ApproveRegistrationTerms2_Valid()
            {
                var acceptance = new PaymentTermsAcceptance2()
                {
                    StudentId = "1234567",
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                var result = service.ApproveRegistrationTerms2(acceptance);
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// Admin user cannot accept terms
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void ApproveRegistrationTerms2_AdminNotAuthorized()
            {
                SetupAdminUser();
                var acceptance = new PaymentTermsAcceptance2()
                {
                    StudentId = "0001234",
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                service.ApproveRegistrationTerms2(acceptance);
            }

            /// <summary>
            /// Proxy user cannot accept terms
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void ApproveRegistrationTerms2_ProxyNotAuthorized()
            {
                SetupProxyUser();
                var acceptance = new PaymentTermsAcceptance2()
                {
                    StudentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId,
                    PaymentControlId = "145",
                    AcknowledgementDateTime = DateTime.Now.AddMinutes(-3),
                    AcknowledgementText = new List<string>() { "This is an", "acknowledgement." },
                    TermsText = new List<string>() { "This is some", "text." },
                    SectionIds = new List<string>() { "101", "102" },
                    InvoiceIds = new List<string>() { "201", "202" },
                    ApprovalUserId = "john_smith",
                    ApprovalReceived = DateTime.Now.AddMinutes(-1)
                };
                service.ApproveRegistrationTerms2(acceptance);
            }
        }

        #endregion

        #region UpdatePaymentControl tests

        [TestClass]
        public class UpdatePaymentControl : RegistrationBillingServiceTests
        {
            [TestInitialize]
            public void Initialize_UpdatePaymentControl()
            {
                Initialize();
                SetupStudentUser();

                // Mock Adapters
                var rpcAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationPaymentControl, RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, RegistrationPaymentControl>()).Returns(rpcAdapter);

                var rpcDtoAdapter = new AutoMapperAdapter<RegistrationPaymentControl, Domain.Finance.Entities.RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<RegistrationPaymentControl, Domain.Finance.Entities.RegistrationPaymentControl>()).Returns(rpcDtoAdapter);

                rbRepoMock.Setup(rep => rep.UpdatePaymentControl(It.IsAny<Domain.Finance.Entities.RegistrationPaymentControl>()))
                    .Returns<Domain.Finance.Entities.RegistrationPaymentControl>(payControl =>
                    {
                        return new Domain.Finance.Entities.RegistrationPaymentControl(payControl.Id, payControl.StudentId, payControl.TermId, Domain.Finance.Entities.RegistrationPaymentStatus.Accepted);
                    });

                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void UpdatePaymentControl_NullDto()
            {
                var result = service.UpdatePaymentControl(null);
            }

            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void UpdatePaymentControl_UnauthorizedUser()
            {
                var result = service.UpdatePaymentControl(new RegistrationPaymentControl()
                    {
                        AcademicCredits = new List<string>() { "123", "124" },
                        Id = "101",
                        InvoiceIds = new List<string>() { "201", "202" },
                        LastPlanApprovalId = "301",
                        LastTermsApprovalId = "401",
                        PaymentPlanId = "501",
                        Payments = new List<string>() { "601", "602" },
                        PaymentStatus = RegistrationPaymentStatus.New,
                        RegisteredSectionIds = new List<string>() { "701", "702" },
                        StudentId = "0001234",
                        TermId = "2014/FA"
                    });
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void UpdatePaymentControl_Valid()
            {
                var result = service.UpdatePaymentControl(new RegistrationPaymentControl()
                {
                    AcademicCredits = new List<string>() { "123", "124" },
                    Id = "101",
                    InvoiceIds = new List<string>() { "201", "202" },
                    LastPlanApprovalId = "301",
                    LastTermsApprovalId = "401",
                    PaymentPlanId = "501",
                    Payments = new List<string>() { "601", "602" },
                    PaymentStatus = RegistrationPaymentStatus.New,
                    RegisteredSectionIds = new List<string>() { "701", "702" },
                    StudentId = "1234567",
                    TermId = "2014/FA"
                });
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is admin - not authorized
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void UpdatePaymentControl_AdminUnauthorized()
            {
                SetupAdminUser();
                service.UpdatePaymentControl(new RegistrationPaymentControl()
                {
                    AcademicCredits = new List<string>() { "123", "124" },
                    Id = "101",
                    InvoiceIds = new List<string>() { "201", "202" },
                    LastPlanApprovalId = "301",
                    LastTermsApprovalId = "401",
                    PaymentPlanId = "501",
                    Payments = new List<string>() { "601", "602" },
                    PaymentStatus = RegistrationPaymentStatus.New,
                    RegisteredSectionIds = new List<string>() { "701", "702" },
                    StudentId = "0001234",
                    TermId = "2014/FA"
                });
            }

            /// <summary>
            /// User is proxy - not authorized
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void UpdatePaymentControl_ProxyUnauthorized()
            {
                SetupProxyUser();
                service.UpdatePaymentControl(new RegistrationPaymentControl()
                {
                    AcademicCredits = new List<string>() { "123", "124" },
                    Id = "101",
                    InvoiceIds = new List<string>() { "201", "202" },
                    LastPlanApprovalId = "301",
                    LastTermsApprovalId = "401",
                    PaymentPlanId = "501",
                    Payments = new List<string>() { "601", "602" },
                    PaymentStatus = RegistrationPaymentStatus.New,
                    RegisteredSectionIds = new List<string>() { "701", "702" },
                    StudentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId,
                    TermId = "2014/FA"
                });
            }
        }

        #endregion

        #region GetPaymentOptions tests

        [TestClass]
        public class GetPaymentOptions : RegistrationBillingServiceTests
        {
            string rpcId = "145";
            string studentId = "1234567";
            string termId = "2013/FA";
            Domain.Finance.Entities.RegistrationPaymentStatus entityStatus = Domain.Finance.Entities.RegistrationPaymentStatus.New;
            List<string> regSections = new List<string>() { "111", "222", "333" };
            List<string> invoiceIds = new List<string>() { "987", "654" };

            List<string> docText = new List<string>() { "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." };

            [TestInitialize]
            public void Initialize_GetPaymentOptions()
            {
                Initialize();
                SetupStudentUser();

                // Mock Adapters
                var rpcDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>()).Returns(rpcDtoAdapter);

                var planStatusAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>()).Returns(planStatusAdapter);

                var spAdapter = new AutoMapperAdapter<Domain.Finance.Entities.ScheduledPayment, Ellucian.Colleague.Dtos.Finance.ScheduledPayment>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.ScheduledPayment, Ellucian.Colleague.Dtos.Finance.ScheduledPayment>()).Returns(spAdapter);

                var planChargeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>()).Returns(planChargeAdapter);

                var chargeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>()).Returns(chargeAdapter);

                var ppAdapter = new PaymentPlanEntityAdapter(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PaymentPlan, Ellucian.Colleague.Dtos.Finance.PaymentPlan>()).Returns(ppAdapter);

                var pmtAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.Payment, Payment>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Payments.Payment, Payment>()).Returns(pmtAdapter);

                var checkPmtAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.CheckPayment, CheckPayment>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Payments.CheckPayment, CheckPayment>()).Returns(checkPmtAdapter);

                var pmtItemAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.PaymentItem, PaymentItem>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Payments.PaymentItem, PaymentItem>()).Returns(pmtItemAdapter);

                var ipoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.ImmediatePaymentOptions, ImmediatePaymentOptions>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.ImmediatePaymentOptions, ImmediatePaymentOptions>()).Returns(ipoAdapter);

                var docAdapter = new AutoMapperAdapter<Domain.Base.Entities.TextDocument, TextDocument>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TextDocument, TextDocument>()).Returns(docAdapter);
                
                //var rpcList = new List<Domain.Finance.Entities.RegistrationPaymentControl>() { rpc };

                var doc = new Domain.Base.Entities.TextDocument(docText);

                var docDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>()).Returns(docDtoAdapter);

                docRepoMock.Setup<Domain.Base.Entities.TextDocument>(
                    repo => repo.Build(It.IsAny<string>(), "IPC.REGISTRATION", It.IsAny<string>(), It.IsAny<string>(), null)).Returns(doc);
                var validRpc = new Domain.Finance.Entities.RegistrationPaymentControl("124", studentId, "Current Term", entityStatus);
                validRpc.AddInvoice("10");
                validRpc.AddInvoice("11");
                validRpc.AddPayment("986");
                validRpc.AddPayment("987");
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(
                    repo => repo.GetPaymentControl(It.IsAny<string>())).Returns<string>(id =>
                    {
                        // Return different results based on the value passed in as the key
                        switch (id)
                        {
                            case null: throw new ArgumentNullException();
                            case "": throw new ArgumentNullException();
                            case "INVALID": return null;
                            case "123": return new Domain.Finance.Entities.RegistrationPaymentControl("123", "0001234", "2014/FA", Domain.Finance.Entities.RegistrationPaymentStatus.New);
                            case "124": return validRpc;
                            default:
                                {
                                    var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                                    foreach (var sec in regSections) { rpc.AddRegisteredSection(sec); }
                                    foreach (var inv in invoiceIds) { rpc.AddInvoice(inv); }
                                    return rpc;
                                }
                        }
                    });

                arRepoMock.Setup(repo => repo.GetInvoices(It.IsAny<IEnumerable<string>>())).Returns(TestInvoiceRepository.Invoices.Where(inv => inv.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetPayments(It.IsAny<IEnumerable<string>>())).Returns(TestReceivablePaymentRepository.Payments.Where(pmt => pmt.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetDistributions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Returns(new List<string>() { "BANK" });
                pmtRepoMock.Setup(repo => repo.GetConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
                    Returns(new Domain.Finance.Entities.Payments.PaymentConfirmation()
                    {
                        ConfirmationText = new List<string>() { "Confirmation", "blah blah blah." },
                        ConvenienceFeeAmount = 5m,
                        ConvenienceFeeCode = "CF5",
                        ConvenienceFeeDescription = "$5.00 Fee",
                        ConvenienceFeeGeneralLedgerNumber = "110101000000010100",
                        ProviderAccount = "PPCC"
                    });
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentOptions_NullPaymentControlId()
            {
                var result = service.GetPaymentOptions(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_GetPaymentOptions_EmptyPaymentControlId()
            {
                var result = service.GetPaymentOptions(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void RegistrationBillingService_GetPaymentOptions_Invalid()
            {
                var result = service.GetPaymentOptions("INVALID");
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentOptions_UnauthorizedUser()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentControl("123")).Returns(
                    new Domain.Finance.Entities.RegistrationPaymentControl("123", "9191919", "2014/FA", Domain.Finance.Entities.RegistrationPaymentStatus.New));
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
                var result = service.GetPaymentOptions("123");
            }

            /// <summary>
            /// Uer is self
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentOptions_DefaultPaymentOptions()
            {
                SetupDefaultPaymentOptions();

                var result = service.GetPaymentOptions("124");
                Assert.IsNull(result.PaymentPlanTemplateId);
                Assert.IsNull(result.PaymentPlanReceivableTypeCode);
                Assert.IsNull(result.PaymentPlanFirstDueDate);
                Assert.AreEqual(100m, result.DeferralPercentage);
                Assert.AreEqual(0m, result.DownPaymentAmount);
                Assert.IsNull(result.DownPaymentDate);
                Assert.AreEqual(0m, result.MinimumPayment);
                Assert.IsFalse(result.ChargesOnPaymentPlan);
                Assert.AreEqual(0m, result.PaymentPlanAmount);
                Assert.AreEqual(0m, result.RegistrationBalance);
            }            

            [TestMethod]
            public void RegistrationBillingService_GetPaymentOptions_PaymentPlanOption_ZeroDollarsOnPlan()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentRequirements(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "Current Term", null, 0, new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(60), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })
                    });
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(TestFinanceConfigurationRepository.PeriodFinanceConfiguration);
                adRepoMock.Setup(repo => repo.GetPeriods(It.IsAny<string>())).Returns(TestAccountDuePeriodRepository.AccountDuePeriod("1234567"));
                arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(TestReceivableTypesRepository.ReceivableTypes);
                arRepoMock.Setup(repo => repo.ChargeCodes).Returns(TestChargeCodesRepository.ChargeCodes);
                ipcServiceMock.Setup(svc => svc.GetPaymentPlanOption(It.IsAny<Domain.Finance.Entities.PaymentRequirement>())).
                    Returns(new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT", DateTime.Today.AddDays(7)));
                ipcServiceMock.Setup(svc => svc.IsValidTemplate(It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>())).Returns(true);
                ipcServiceMock.Setup(svc => svc.GetPaymentOptions(It.IsAny<Domain.Finance.Entities.RegistrationPaymentControl>(), It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(), It.IsAny<List<Domain.Finance.Entities.AccountDue.AccountTerm>>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<Domain.Finance.Entities.PaymentRequirement>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(),
                    It.IsAny<IEnumerable<string>>())).Returns(new Domain.Finance.Entities.ImmediatePaymentOptions(false, 500m, 100m, 75m));
                ppRepoMock.Setup(repo => repo.GetTemplate("DEFAULT")).Returns(TestPaymentPlanTemplateRepository.PayPlanTemplates.FirstOrDefault(t => t.Id == "DEFAULT"));
                string typeCode = "01";
                planProcessorMock.Setup(pp => pp.GetPlanAmount(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(), It.IsAny<bool>(), out typeCode)).Returns(500m);
                docRepoMock.Setup(repo => repo.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).
                    Returns(new Domain.Base.Entities.TextDocument(docText));
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.GetPaymentOptions("124");
                Assert.IsNull(result.PaymentPlanTemplateId);
                Assert.IsNull(result.PaymentPlanReceivableTypeCode);
                Assert.IsNull(result.PaymentPlanFirstDueDate);
                Assert.AreEqual(100m, result.DeferralPercentage);
                Assert.AreEqual(0m, result.DownPaymentAmount);
                Assert.IsNull(result.DownPaymentDate);
                Assert.AreEqual(0m, result.MinimumPayment);
                Assert.IsFalse(result.ChargesOnPaymentPlan);
                Assert.AreEqual(0m, result.PaymentPlanAmount);
                Assert.AreEqual(0m, result.RegistrationBalance);
            }

            [TestMethod]
            public void RegistrationBillingService_GetPaymentOptions_ValidPaymentPlanOption()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentRequirements(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "Current Term", null, 0, new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(60), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })
                    });
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(TestFinanceConfigurationRepository.PeriodFinanceConfiguration);
                adRepoMock.Setup(repo => repo.GetPeriods(It.IsAny<string>())).Returns(TestAccountDuePeriodRepository.AccountDuePeriod("1234567"));
                arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(TestReceivableTypesRepository.ReceivableTypes);
                arRepoMock.Setup(repo => repo.ChargeCodes).Returns(TestChargeCodesRepository.ChargeCodes);
                ipcServiceMock.Setup(svc => svc.GetPaymentPlanOption(It.IsAny<Domain.Finance.Entities.PaymentRequirement>())).
                    Returns(new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT", DateTime.Today.AddDays(7)));
                ipcServiceMock.Setup(svc => svc.IsValidTemplate(It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>())).Returns(true);
                ipcServiceMock.Setup(svc => svc.GetPaymentOptions(It.IsAny<Domain.Finance.Entities.RegistrationPaymentControl>(), It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(), It.IsAny<List<Domain.Finance.Entities.AccountDue.AccountTerm>>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<Domain.Finance.Entities.PaymentRequirement>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(),
                    It.IsAny<IEnumerable<string>>())).Returns(new Domain.Finance.Entities.ImmediatePaymentOptions(false, 500m, 100m, 75m));
                ppRepoMock.Setup(repo => repo.GetTemplate("DEFAULT")).Returns(TestPaymentPlanTemplateRepository.PayPlanTemplates.FirstOrDefault(t => t.Id == "DEFAULT"));
                string typeCode = "01";
                planProcessorMock.Setup(pp => pp.GetPlanAmount(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(), It.IsAny<bool>(), out typeCode)).Returns(500m);
                docRepoMock.Setup(repo => repo.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).
                    Returns(new Domain.Base.Entities.TextDocument(docText));
                var ad = TestAccountDuePeriodRepository.AccountDuePeriod("1234567");
                ad.Current.AccountTerms.ForEach(at => at.AccountDetails.ForEach(acd => acd.AccountType = "01"));
                adRepoMock.Setup(repo => repo.GetPeriods(It.IsAny<string>())).Returns(ad);

                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.GetPaymentOptions("124");
                Assert.AreEqual("DEFAULT", result.PaymentPlanTemplateId);
                Assert.AreEqual("01", result.PaymentPlanReceivableTypeCode);
                Assert.IsNotNull(result.PaymentPlanFirstDueDate);
                Assert.AreEqual(0m, result.DeferralPercentage);
                Assert.AreEqual(0m, result.DownPaymentAmount);
                Assert.IsNull(result.DownPaymentDate);
                Assert.AreEqual(1500m, result.MinimumPayment);
                Assert.IsFalse(result.ChargesOnPaymentPlan);
                Assert.AreEqual(1500m, result.PaymentPlanAmount);
                Assert.AreEqual(1500m, result.RegistrationBalance);
            }

            [TestMethod]
            public void RegistrationBillingService_GetPaymentOptions_InvalidPlanTermsDocument()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentRequirements(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "Current Term", null, 0, new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(60), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })
                    });
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(TestFinanceConfigurationRepository.PeriodFinanceConfiguration);
                adRepoMock.Setup(repo => repo.GetPeriods(It.IsAny<string>())).Returns(TestAccountDuePeriodRepository.AccountDuePeriod("1234567"));
                arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(TestReceivableTypesRepository.ReceivableTypes);
                arRepoMock.Setup(repo => repo.ChargeCodes).Returns(TestChargeCodesRepository.ChargeCodes);
                ipcServiceMock.Setup(svc => svc.GetPaymentPlanOption(It.IsAny<Domain.Finance.Entities.PaymentRequirement>())).
                    Returns(new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT", DateTime.Today.AddDays(7)));
                ipcServiceMock.Setup(svc => svc.IsValidTemplate(It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>())).Returns(true);
                ipcServiceMock.Setup(svc => svc.GetPaymentOptions(It.IsAny<Domain.Finance.Entities.RegistrationPaymentControl>(), It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(), It.IsAny<List<Domain.Finance.Entities.AccountDue.AccountTerm>>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<Domain.Finance.Entities.PaymentRequirement>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(),
                    It.IsAny<IEnumerable<string>>())).Returns(new Domain.Finance.Entities.ImmediatePaymentOptions(false, 500m, 100m, 75m));
                ppRepoMock.Setup(repo => repo.GetTemplate("DEFAULT")).Returns(TestPaymentPlanTemplateRepository.PayPlanTemplates.FirstOrDefault(t => t.Id == "DEFAULT"));
                string typeCode = "01";
                planProcessorMock.Setup(pp => pp.GetPlanAmount(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(), It.IsAny<bool>(), out typeCode)).Returns(500m);
                docRepoMock.Setup(repo => repo.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).
                    Throws(new InvalidOperationException());
                var ad = TestAccountDuePeriodRepository.AccountDuePeriod("1234567");
                ad.Current.AccountTerms.ForEach(at => at.AccountDetails.ForEach(acd => acd.AccountType = "01"));
                adRepoMock.Setup(repo => repo.GetPeriods(It.IsAny<string>())).Returns(ad);

                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.GetPaymentOptions("124");
                Assert.IsNull(result.PaymentPlanTemplateId);
                Assert.IsNull(result.PaymentPlanReceivableTypeCode);
                Assert.IsNull(result.PaymentPlanFirstDueDate);
                Assert.AreEqual(0m, result.DeferralPercentage);
                Assert.AreEqual(0m, result.DownPaymentAmount);
                Assert.IsNull(result.DownPaymentDate);
                Assert.AreEqual(1500m, result.MinimumPayment);
                Assert.IsFalse(result.ChargesOnPaymentPlan);
                Assert.AreEqual(0m, result.PaymentPlanAmount);
                Assert.AreEqual(1500m, result.RegistrationBalance);
            }

            [TestMethod]
            public void RegistrationBillingService_GetPaymentOptions_DefaultPaymentOptions_Term()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentRequirements(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "Current Term", null, 0, new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })
                    });
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(TestFinanceConfigurationRepository.TermFinanceConfiguration);
                adRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns(TestAccountDuePeriodRepository.AccountDuePeriod("1234567").Current);
                arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(TestReceivableTypesRepository.ReceivableTypes);
                arRepoMock.Setup(repo => repo.ChargeCodes).Returns(TestChargeCodesRepository.ChargeCodes);
                ipcServiceMock.Setup(svc => svc.GetPaymentPlanOption(It.IsAny<Domain.Finance.Entities.PaymentRequirement>())).
                    Returns(new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT", DateTime.Today.AddDays(7)));
                ipcServiceMock.Setup(svc => svc.IsValidTemplate(It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>())).Returns(true);
                ipcServiceMock.Setup(svc => svc.GetPaymentOptions(It.IsAny<Domain.Finance.Entities.RegistrationPaymentControl>(), It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(), It.IsAny<List<Domain.Finance.Entities.AccountDue.AccountTerm>>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<Domain.Finance.Entities.PaymentRequirement>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(),
                    It.IsAny<IEnumerable<string>>())).Returns(new Domain.Finance.Entities.ImmediatePaymentOptions(false, 500m, 100m, 75m));
                ppRepoMock.Setup(repo => repo.GetTemplate("DEFAULT")).Returns(TestPaymentPlanTemplateRepository.PayPlanTemplates.FirstOrDefault(t => t.Id == "DEFAULT"));
                string typeCode = "01";
                planProcessorMock.Setup(pp => pp.GetPlanAmount(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(), It.IsAny<bool>(), out typeCode)).Returns(500m);
                docRepoMock.Setup(repo => repo.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).
                    Returns(new Domain.Base.Entities.TextDocument(docText));
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.GetPaymentOptions("124");
                Assert.IsNull(result.PaymentPlanTemplateId);
                Assert.IsNull(result.PaymentPlanReceivableTypeCode);
                Assert.IsNull(result.PaymentPlanFirstDueDate);
                Assert.AreEqual(100m, result.DeferralPercentage);
                Assert.AreEqual(0m, result.DownPaymentAmount);
                Assert.IsNull(result.DownPaymentDate);
                Assert.AreEqual(0m, result.MinimumPayment);
                Assert.IsFalse(result.ChargesOnPaymentPlan);
                Assert.AreEqual(0m, result.PaymentPlanAmount);
                Assert.AreEqual(0m, result.RegistrationBalance);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentOptions_UserIsAdminTest(){
                SetupAdminUser();
                SetupDefaultPaymentOptions();
                Assert.IsNotNull(service.GetPaymentOptions("125"));
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentOptions_UserIsAdminWithNoPermissionsTest()
            {
                SetupAdminUserWithNoPermissions();
                SetupDefaultPaymentOptions();
                service.GetPaymentOptions("125");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void RegistrationBillingService_GetPaymentOptions_UserIsProxyTest()
            {
                SetupProxyUser();
                studentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId;
                SetupDefaultPaymentOptions();
                Assert.IsNotNull(service.GetPaymentOptions("125"));
            }

            /// <summary>
            /// User is proxy for differet person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void RegistrationBillingService_GetPaymentOptions_UserIsProxyForDifferentPersonTest()
            {
                SetupProxyUserForDifferentPerson();
                SetupDefaultPaymentOptions();
                service.GetPaymentOptions("125");
            }

            private void SetupDefaultPaymentOptions()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentRequirements(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "Current Term", null, 0, new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })
                    });
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(TestFinanceConfigurationRepository.PeriodFinanceConfiguration);
                adRepoMock.Setup(repo => repo.GetPeriods(It.IsAny<string>())).Returns(TestAccountDuePeriodRepository.AccountDuePeriod("1234567"));
                arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(TestReceivableTypesRepository.ReceivableTypes);
                arRepoMock.Setup(repo => repo.ChargeCodes).Returns(TestChargeCodesRepository.ChargeCodes);
                ipcServiceMock.Setup(svc => svc.GetPaymentPlanOption(It.IsAny<Domain.Finance.Entities.PaymentRequirement>())).
                    Returns(new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT", DateTime.Today.AddDays(7)));
                ipcServiceMock.Setup(svc => svc.IsValidTemplate(It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>())).Returns(true);
                ipcServiceMock.Setup(svc => svc.GetPaymentOptions(It.IsAny<Domain.Finance.Entities.RegistrationPaymentControl>(), It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(), It.IsAny<List<Domain.Finance.Entities.AccountDue.AccountTerm>>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<Domain.Finance.Entities.PaymentRequirement>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(),
                    It.IsAny<IEnumerable<string>>())).Returns(new Domain.Finance.Entities.ImmediatePaymentOptions(false, 500m, 100m, 75m));
                ppRepoMock.Setup(repo => repo.GetTemplate("DEFAULT")).Returns(TestPaymentPlanTemplateRepository.PayPlanTemplates.FirstOrDefault(t => t.Id == "DEFAULT"));
                string typeCode = "01";
                planProcessorMock.Setup(pp => pp.GetPlanAmount(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(), It.IsAny<bool>(), out typeCode)).Returns(500m);
                docRepoMock.Setup(repo => repo.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).
                    Returns(new Domain.Base.Entities.TextDocument(docText));
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }
        }

        #endregion

        #region GetPaymentSummary tests

        [TestClass]
        public class GetPaymentSummary : RegistrationBillingServiceTests
        {
            string rpcId = "145";
            string studentId = "1234567";
            string termId = "2013/FA";
            Domain.Finance.Entities.RegistrationPaymentStatus entityStatus = Domain.Finance.Entities.RegistrationPaymentStatus.New;
            List<string> regSections = new List<string>() { "111", "222", "333" };
            List<string> invoiceIds = new List<string>() { "987", "654" };

            List<string> docText = new List<string>() { "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." };

            [TestInitialize]
            public void Initialize_GetPaymentSummary()
            {
                Initialize();
                SetupStudentUser();

                // Mock Adapters
                var rpcDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>()).Returns(rpcDtoAdapter);

                var planStatusAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>()).Returns(planStatusAdapter);

                var spAdapter = new AutoMapperAdapter<Domain.Finance.Entities.ScheduledPayment, Ellucian.Colleague.Dtos.Finance.ScheduledPayment>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.ScheduledPayment, Ellucian.Colleague.Dtos.Finance.ScheduledPayment>()).Returns(spAdapter);

                var planChargeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>()).Returns(planChargeAdapter);

                var chargeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>()).Returns(chargeAdapter);

                var ppAdapter = new PaymentPlanEntityAdapter(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PaymentPlan, Ellucian.Colleague.Dtos.Finance.PaymentPlan>()).Returns(ppAdapter);

                var pmtAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.Payment, Payment>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Payments.Payment, Payment>()).Returns(pmtAdapter);

                var checkPmtAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.CheckPayment, CheckPayment>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Payments.CheckPayment, CheckPayment>()).Returns(checkPmtAdapter);

                var pmtItemAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.PaymentItem, PaymentItem>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Payments.PaymentItem, PaymentItem>()).Returns(pmtItemAdapter);
                
                //var rpcList = new List<Domain.Finance.Entities.RegistrationPaymentControl>() { rpc };

                var doc = new Domain.Base.Entities.TextDocument(docText);

                var docDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>()).Returns(docDtoAdapter);

                docRepoMock.Setup<Domain.Base.Entities.TextDocument>(
                    repo => repo.Build(It.IsAny<string>(), "IPC.REGISTRATION", It.IsAny<string>(), It.IsAny<string>(), null)).Returns(doc);
                var validRpc = new Domain.Finance.Entities.RegistrationPaymentControl("124", studentId, "Current Term", entityStatus);
                validRpc.AddInvoice("10");
                validRpc.AddInvoice("11");
                validRpc.AddPayment("986");
                validRpc.AddPayment("987");
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(
                    repo => repo.GetPaymentControl(It.IsAny<string>())).Returns<string>(id =>
                    {
                        // Return different results based on the value passed in as the key
                        switch (id)
                        {
                            case null: throw new ArgumentNullException();
                            case "": throw new ArgumentNullException();
                            case "INVALID": return null;
                            case "123": return new Domain.Finance.Entities.RegistrationPaymentControl("123", "0001234", "2014/FA", Domain.Finance.Entities.RegistrationPaymentStatus.New);
                            case "124": return validRpc;
                            default:
                                {
                                    var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                                    foreach (var sec in regSections) { rpc.AddRegisteredSection(sec); }
                                    foreach (var inv in invoiceIds) { rpc.AddInvoice(inv); }
                                    return rpc;
                                }
                        }
                    });

                arRepoMock.Setup(repo => repo.GetInvoices(It.IsAny<IEnumerable<string>>())).Returns(TestInvoiceRepository.Invoices.Where(inv => inv.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetPayments(It.IsAny<IEnumerable<string>>())).Returns(TestReceivablePaymentRepository.Payments.Where(pmt => pmt.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetDistributions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Returns(new List<string>() { "BANK" });
                arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(TestReceivableTypesRepository.ReceivableTypes);
                pmtRepoMock.Setup(repo => repo.GetConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
                    Returns(new Domain.Finance.Entities.Payments.PaymentConfirmation()
                    {
                        ConfirmationText = new List<string>() { "Confirmation", "blah blah blah." },
                        ConvenienceFeeAmount = 5m,
                        ConvenienceFeeCode = "CF5",
                        ConvenienceFeeDescription = "$5.00 Fee",
                        ConvenienceFeeGeneralLedgerNumber = "110101000000010100",
                        ProviderAccount = "PPCC"
                    });
                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(TestFinanceConfigurationRepository.TermFinanceConfiguration);
                var ad = TestAccountDuePeriodRepository.AccountDuePeriod("1234567").Current;
                ad.AccountTerms.ForEach(at => at.AccountDetails.ForEach(acd => acd.AccountType = "01"));
                adRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns(ad);

                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetPaymentSummary_NullId()
            {
                var result = service.GetPaymentSummary(null, null, 0m);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetPaymentSummary_EmptyId()
            {
                var result = service.GetPaymentSummary(string.Empty, null, 0m);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetPaymentSummary_NullPayMethod()
            {
                var result = service.GetPaymentSummary("123", null, 0m);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetPaymentSummary_EmptyPayMethod()
            {
                var result = service.GetPaymentSummary("123", string.Empty, 0m);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void GetPaymentSummary_InvalidAmount()
            {
                var result = service.GetPaymentSummary("123", "ECHK", 0m);
            }

            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetPaymentSummary_UnauthorizedUser()
            {
                var result = service.GetPaymentSummary("123", "ECHK", 500m);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void GetPaymentSummary_Valid()
            {
                var result = service.GetPaymentSummary("124", "ECHK", 500m);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(505m, result.ToList()[0].AmountToPay);
            }

            /// <summary>
            /// Admin has no access
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetPaymentSummary_AdminUser()
            {
                SetupAdminUser();
                var result = service.GetPaymentSummary("123", "ECHK", 500m);
            }

            /// <summary>
            /// Proxy has no access
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetPaymentSummary_ProxyUser()
            {
                SetupProxyUser();
                studentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId;
                var result = service.GetPaymentSummary("123", "ECHK", 500m);
            }
        }

        #endregion

        #region StartRegistrationPayment tests

        [TestClass]
        public class StartRegistrationPayment : RegistrationBillingServiceTests
        {
            [TestInitialize]
            public void Initialize_StartRegistrationPayment()
            {
                Initialize();
                SetupStudentUser();

                // Mock Adapters
                var pmtAdapter = new PaymentDtoAdapter(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Dtos.Finance.Payments.Payment, Domain.Finance.Entities.Payments.Payment>()).Returns(pmtAdapter);

                var providerAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.PaymentProvider, PaymentProvider>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Payments.PaymentProvider, PaymentProvider>()).Returns(providerAdapter);

                Domain.Finance.Entities.Payments.PaymentProvider validProvider = new Domain.Finance.Entities.Payments.PaymentProvider()
                {
                    RedirectUrl = "http://www.ellucianuniversity.edu"
                };

                rbRepoMock.Setup(rep => rep.StartRegistrationPayment(It.IsAny<Domain.Finance.Entities.Payments.Payment>())).Returns(validProvider);

                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StartRegistrationPayment_NullPayment()
            {
                var result = service.StartRegistrationPayment(null);
            }

            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void StartRegistrationPayment_UnauthorizedUser()
            {
                var result = service.StartRegistrationPayment(new Dtos.Finance.Payments.Payment()
                    {
                        AmountToPay = 500m,
                        PersonId = "0001234"
                    });
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void StartRegistrationPayment_Valid()
            {
                var result = service.StartRegistrationPayment(new Dtos.Finance.Payments.Payment()
                {
                    AmountToPay = 500m,
                    PersonId = "1234567"
                });
                Assert.AreEqual("http://www.ellucianuniversity.edu", result.RedirectUrl);
            }

            /// <summary>
            /// Admin cannot make a payment
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void StartRegistrationPayment_AdminUser()
            {
                SetupAdminUser();
                var result = service.StartRegistrationPayment(new Dtos.Finance.Payments.Payment()
                {
                    AmountToPay = 500m,
                    PersonId = "1234567"
                });
            }

            /// <summary>
            /// Proxy cannot make a payment
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void StartRegistrationPayment_ProxyUser()
            {
                SetupProxyUser();
                var result = service.StartRegistrationPayment(new Dtos.Finance.Payments.Payment()
                {
                    AmountToPay = 500m,
                    PersonId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId
                });
            }
        }

        #endregion

        #region GetTermsApproval tests

        [TestClass]
        public class GetTermsApproval : RegistrationBillingServiceTests
        {
            private string personId;

            [TestInitialize]
            public void Initialize_GetTermsApproval()
            {
                Initialize();
                SetupStudentUser();

                personId = "1234567";

                // Mock Adapters
                var rtaDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>()).Returns(rtaDtoAdapter);

                rbRepoMock.Setup(repo => repo.GetTermsApproval("123")).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", personId, DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC"));
                rbRepoMock.Setup(repo => repo.GetTermsApproval("124")).Returns(
                new Domain.Finance.Entities.RegistrationTermsApproval("123", "0001234", DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                    new List<string>() { "201", "202" }, "IPCREGTC"));
                apprSvcMock.Setup(svc => svc.GetApprovalDocument("123")).Returns(new Dtos.Base.ApprovalDocument()
                {
                    Id = "1",
                    PersonId = personId,
                    Text = new List<string>() { "This is some", "acknowledgement text." }
                });
                apprSvcMock.Setup(svc => svc.GetApprovalDocument("124")).Returns(new Dtos.Base.ApprovalDocument()
                {
                    Id = "2",
                    PersonId = personId,
                    Text = new List<string>() { "This is also some", "acknowledgement text." }
                });
                apprSvcMock.Setup(svc => svc.GetApprovalResponse(It.IsAny<string>())).Returns(new Dtos.Base.ApprovalResponse()
                {
                    Id = "1",
                    PersonId = personId,
                    DocumentId = "124",
                    IsApproved = true,
                    Received = DateTime.Now.AddMinutes(-3),
                    UserId = "john_smith"
                });

                BuildService();
            }

            private void BuildService()
            {
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetTermsApproval_UnauthorizedUser()
            {
                var result = service.GetTermsApproval("124");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetTermsApproval_NullId()
            {
                var result = service.GetTermsApproval(null);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetTermsApproval_EmptyId()
            {
                var result = service.GetTermsApproval(string.Empty);
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void GetTermsApproval_Valid()
            {
                var result = service.GetTermsApproval("123");
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void GetTermsApproval_UserIsAdmin()
            {
                SetupAdminUser();
                var result = service.GetTermsApproval("123");
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetTermsApproval_AdminNoPermissions()
            {
                SetupAdminUserWithNoPermissions();
                service.GetTermsApproval("123");                
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void GetTermsApproval_UserIsProxy()
            {
                SetupProxyUser();
                personId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId;
                rbRepoMock.Setup(repo => repo.GetTermsApproval("123")).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", personId, DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC"));
                BuildService();
                var result = service.GetTermsApproval("123");
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetTermsApproval_ProxyForDifferentPerson()
            {
                SetupProxyUserForDifferentPerson();
                service.GetTermsApproval("123");
            }
        }

        #endregion

        #region GetTermsApproval2 tests

        [TestClass]
        public class GetTermsApproval2 : RegistrationBillingServiceTests
        {
            [TestInitialize]
            public void Initialize_GetTermsApproval2()
            {
                Initialize();
                SetupStudentUser();

                // Mock Adapters
                var rtaDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>()).Returns(rtaDtoAdapter);

                rbRepoMock.Setup(repo => repo.GetTermsApproval("123")).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", "1234567", DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC"));
                rbRepoMock.Setup(repo => repo.GetTermsApproval("124")).Returns(
                new Domain.Finance.Entities.RegistrationTermsApproval("123", "0001234", DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                    new List<string>() { "201", "202" }, "IPCREGTC"));
                BuildService();
            }

            private void BuildService()
            {
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            /// <summary>
            /// User is neither self, nor admin, nor proxy
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetTermsApproval2_UnauthorizedUser()
            {
                var result = service.GetTermsApproval2("124");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetTermsApproval2_NullId()
            {
                var result = service.GetTermsApproval2(null);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetTermsApproval2_EmptyId()
            {
                var result = service.GetTermsApproval2(string.Empty);
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void GetTermsApproval2_Valid()
            {
                var result = service.GetTermsApproval2("123");
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void GetTermsApproval2_UserIsAdmin()
            {
                SetupAdminUser();
                var result = service.GetTermsApproval2("123");
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetTermsApproval2_AdminNoPermissions()
            {
                SetupAdminUserWithNoPermissions();
                service.GetTermsApproval2("123");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void GetTermsApproval2_UserIsProxy()
            {
                SetupProxyUser();                
                rbRepoMock.Setup(repo => repo.GetTermsApproval("123")).Returns(
                    new Domain.Finance.Entities.RegistrationTermsApproval("123", userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId, DateTime.Now.AddMinutes(-3), "101", new List<string>() { "101", "102" },
                        new List<string>() { "201", "202" }, "IPCREGTC"));
                BuildService();
                var result = service.GetTermsApproval2("123");
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetTermsApproval2_ProxyForDifferentPerson()
            {
                SetupProxyUserForDifferentPerson();
                service.GetTermsApproval2("123");
            }
        }

        #endregion

        #region GetProposedPaymentPlan tests

        [TestClass]
        public class GetProposedPaymentPlan : RegistrationBillingServiceTests
        {
            string rpcId = "145";
            string studentId = "1234567";
            string termId = "2013/FA";
            Domain.Finance.Entities.RegistrationPaymentStatus entityStatus = Domain.Finance.Entities.RegistrationPaymentStatus.New;
            List<string> regSections = new List<string>() { "111", "222", "333" };
            List<string> invoiceIds = new List<string>() { "987", "654" };

            List<string> docText = new List<string>() { "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." };

            [TestInitialize]
            public void Initialize_GetProposedPaymentPlan()
            {
                Initialize();
                SetupStudentUser();

                // Mock Adapters
                var rpcDtoAdapter = new AutoMapperAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, Ellucian.Colleague.Dtos.Finance.RegistrationPaymentControl>()).Returns(rpcDtoAdapter);
                
                var planStatusAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>()).Returns(planStatusAdapter);

                var spAdapter = new AutoMapperAdapter<Domain.Finance.Entities.ScheduledPayment, Ellucian.Colleague.Dtos.Finance.ScheduledPayment>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.ScheduledPayment, Ellucian.Colleague.Dtos.Finance.ScheduledPayment>()).Returns(spAdapter);

                var planChargeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>()).Returns(planChargeAdapter);

                var chargeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>()).Returns(chargeAdapter);

                var ppAdapter = new PaymentPlanEntityAdapter(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Finance.Entities.PaymentPlan, Ellucian.Colleague.Dtos.Finance.PaymentPlan>()).Returns(ppAdapter);

                var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                foreach (var id in regSections) { rpc.AddRegisteredSection(id); }
                foreach (var id in invoiceIds) { rpc.AddInvoice(id); }
                var rpcList = new List<Domain.Finance.Entities.RegistrationPaymentControl>() { rpc };

                var doc = new Domain.Base.Entities.TextDocument(docText);

                var docDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>(adapterMock.Object, loggerMock.Object);
                adapterMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TextDocument, Ellucian.Colleague.Dtos.Base.TextDocument>()).Returns(docDtoAdapter);
                
                docRepoMock.Setup<Domain.Base.Entities.TextDocument>(
                    repo => repo.Build(It.IsAny<string>(), "IPC.REGISTRATION", It.IsAny<string>(), It.IsAny<string>(), null)).Returns(doc);
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(
                    repo => repo.GetPaymentControl(It.IsAny<string>())).Returns<string>(id =>
                    {
                        // Return different results based on the value passed in as the key
                        switch (id)
                        {
                            case null: throw new ArgumentNullException();
                            case "": throw new ArgumentNullException();
                            case "INVALID": return null;
                            case "123": return new Domain.Finance.Entities.RegistrationPaymentControl("123", "0001234", "2014/FA", Domain.Finance.Entities.RegistrationPaymentStatus.New);
                            case "124": return new Domain.Finance.Entities.RegistrationPaymentControl("124", studentId, "Current Term", entityStatus);
                            default: return rpc;
                        }
                    });


                fcRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(TestFinanceConfigurationRepository.TermFinanceConfiguration);
                var ad = TestAccountDuePeriodRepository.AccountDuePeriod("1234567").Current;
                ad.AccountTerms.ForEach(at => at.AccountDetails.ForEach(acd => acd.AccountType = "01"));
                adRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns(ad);

                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetProposedPaymentPlan_NullPayControlId()
            {
                var result = service.GetProposedPaymentPlan(null, "01");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetProposedPaymentPlan_EmptyPayControlId()
            {
                var result = service.GetProposedPaymentPlan(string.Empty, "01");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetProposedPaymentPlan_NullReceivableType()
            {
                var result = service.GetProposedPaymentPlan("123", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetProposedPaymentPlan_EmptyReceivableType()
            {
                var result = service.GetProposedPaymentPlan("123", string.Empty);
            }

            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetProposedPaymentPlan_UnauthorizedUser()
            {
                var result = service.GetProposedPaymentPlan("123", "01");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void GetProposedPaymentPlan_Valid()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentRequirements(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "Current Term", null, 0, new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })
                    });

                ipcServiceMock.Setup(svc => svc.GetPaymentPlanOption(It.IsAny<Domain.Finance.Entities.PaymentRequirement>())).
                    Returns(new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), "DEFAULT", DateTime.Today.AddDays(7)));
                ppRepoMock.Setup(repo => repo.GetTemplate("DEFAULT")).Returns(TestPaymentPlanTemplateRepository.PayPlanTemplates.FirstOrDefault(ppt => ppt.Id == "DEFAULT")); 
                string typeCode = null;
                planProcessorMock.Setup(pp => pp.GetPlanAmount(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(), It.IsAny<bool>(), out typeCode)).Returns(500m);
                planProcessorMock.Setup(pp => pp.GetPlanCharges(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>())).Returns(new List<Domain.Finance.Entities.PlanCharge>()
                    {
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("101", "201", new List<string>() { "CHG101" }, "CHG1", 100m),
                            100m,
                            true,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("102", "201", new List<string>() { "CHG102" }, "CHG2", 100m),
                            100m,
                            false,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("103", "201", new List<string>() { "CHG103" }, "CHG3", 100m),
                            100m,
                            false,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("104", "201", new List<string>() { "CHG104" }, "CHG4", 100m),
                            100m,
                            false,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("105", "201", new List<string>() { "CHG105" }, "CHG5", 100m),
                            100m,
                            false,
                            true),
                    });
                planProcessorMock.Setup(pp => pp.GetProposedPlan(It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<Domain.Finance.Entities.PlanCharge>>())).Returns(TestPaymentPlanRepository.PayPlans[0]);

                var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                foreach (var id in regSections) { rpc.AddRegisteredSection(id); }
                foreach (var id in invoiceIds) { rpc.AddInvoice(id); }
                var rpcList = new List<Domain.Finance.Entities.RegistrationPaymentControl>() { rpc };

                var validRpc = new Domain.Finance.Entities.RegistrationPaymentControl("124", studentId, "Current Term", entityStatus);
                validRpc.AddInvoice("10");
                validRpc.AddInvoice("11");
                validRpc.AddPayment("986");
                validRpc.AddPayment("987");
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(
                    repo => repo.GetPaymentControl(It.IsAny<string>())).Returns<string>(id =>
                    {
                        // Return different results based on the value passed in as the key
                        switch (id)
                        {
                            case null: throw new ArgumentNullException();
                            case "": throw new ArgumentNullException();
                            case "INVALID": return null;
                            case "123": return new Domain.Finance.Entities.RegistrationPaymentControl("123", "0001234", "2014/FA", Domain.Finance.Entities.RegistrationPaymentStatus.New);
                            case "124": return validRpc;
                            default: return rpc;
                        }
                    });

                arRepoMock.Setup(ar => ar.ChargeCodes).Returns(TestChargeCodesRepository.ChargeCodes);
                arRepoMock.Setup(repo => repo.GetInvoices(It.IsAny<IEnumerable<string>>())).Returns(TestInvoiceRepository.Invoices.Where(inv => inv.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetPayments(It.IsAny<IEnumerable<string>>())).Returns(TestReceivablePaymentRepository.Payments.Where(pmt => pmt.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetDistributions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Returns(new List<string>() { "BANK" });
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.GetProposedPaymentPlan("124", "01");
                Assert.IsNotNull(result);
            }

            [Ignore]
            [TestMethod]
            public void GetProposedPaymentPlan_CustomFrequency()
            {
                rbRepoMock.Setup(repo => repo.GetPaymentRequirements(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "Current Term", null, 0, new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "CUSTOM",
                                    DateTime.Today.AddDays(7))
                            })
                    });

                ipcServiceMock.Setup(svc => svc.GetPaymentPlanOption(It.IsAny<Domain.Finance.Entities.PaymentRequirement>())).
                    Returns(new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), "CUSTOM", DateTime.Today.AddDays(7)));
                ppRepoMock.Setup(repo => repo.GetTemplate("CUSTOM")).Returns(TestPaymentPlanTemplateRepository.PayPlanTemplates.FirstOrDefault(ppt => ppt.Id == "CUSTOM"));
                ppRepoMock.Setup(repo => repo.GetPlanCustomScheduleDates(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime>(), It.IsAny<string>())).Returns(new List<DateTime?>() { null, DateTime.Today.AddDays(7), DateTime.Today.AddDays(14), DateTime.Today.AddDays(24) });

                string typeCode = null;
                planProcessorMock.Setup(pp => pp.GetPlanAmount(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ReceivablePayment>>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>(), It.IsAny<bool>(), out typeCode)).Returns(500m);
                planProcessorMock.Setup(pp => pp.GetPlanCharges(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(),
                    It.IsAny<IEnumerable<Domain.Finance.Entities.Invoice>>(), It.IsAny<IEnumerable<Domain.Finance.Entities.ChargeCode>>())).Returns(new List<Domain.Finance.Entities.PlanCharge>()
                    {
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("101", "201", new List<string>() { "CHG101" }, "CHG1", 100m),
                            100m,
                            true,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("102", "201", new List<string>() { "CHG102" }, "CHG2", 100m),
                            100m,
                            false,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("103", "201", new List<string>() { "CHG103" }, "CHG3", 100m),
                            100m,
                            false,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("104", "201", new List<string>() { "CHG104" }, "CHG4", 100m),
                            100m,
                            false,
                            true),
                        new Domain.Finance.Entities.PlanCharge(null, 
                            new Domain.Finance.Entities.Charge("105", "201", new List<string>() { "CHG105" }, "CHG5", 100m),
                            100m,
                            false,
                            true),
                    });
                planProcessorMock.Setup(pp => pp.GetProposedPlan(It.IsAny<Domain.Finance.Entities.PaymentPlanTemplate>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<Domain.Finance.Entities.PlanCharge>>())).Returns(TestPaymentPlanRepository.PayPlans[0]);

                var rpc = new Domain.Finance.Entities.RegistrationPaymentControl(rpcId, studentId, termId, entityStatus);
                foreach (var id in regSections) { rpc.AddRegisteredSection(id); }
                foreach (var id in invoiceIds) { rpc.AddInvoice(id); }
                var rpcList = new List<Domain.Finance.Entities.RegistrationPaymentControl>() { rpc };

                var validRpc = new Domain.Finance.Entities.RegistrationPaymentControl("124", studentId, "Current Term", entityStatus);
                validRpc.AddInvoice("10");
                validRpc.AddInvoice("11");
                validRpc.AddPayment("986");
                validRpc.AddPayment("987");
                rbRepoMock.Setup<Domain.Finance.Entities.RegistrationPaymentControl>(
                    repo => repo.GetPaymentControl(It.IsAny<string>())).Returns<string>(id =>
                    {
                        // Return different results based on the value passed in as the key
                        switch (id)
                        {
                            case null: throw new ArgumentNullException();
                            case "": throw new ArgumentNullException();
                            case "INVALID": return null;
                            case "123": return new Domain.Finance.Entities.RegistrationPaymentControl("123", "0001234", "2014/FA", Domain.Finance.Entities.RegistrationPaymentStatus.New);
                            case "124": return validRpc;
                            default: return rpc;
                        }
                    });

                arRepoMock.Setup(ar => ar.ChargeCodes).Returns(TestChargeCodesRepository.ChargeCodes);
                arRepoMock.Setup(repo => repo.GetInvoices(It.IsAny<IEnumerable<string>>())).Returns(TestInvoiceRepository.Invoices.Where(inv => inv.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetPayments(It.IsAny<IEnumerable<string>>())).Returns(TestReceivablePaymentRepository.Payments.Where(pmt => pmt.PersonId == studentId));
                arRepoMock.Setup(repo => repo.GetDistributions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Returns(new List<string>() { "BANK" });
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.GetProposedPaymentPlan("124", "01");
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// Admin cannot get data
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetProposedPaymentPlan_AdminCannotGetData()
            {
                SetupAdminUser();
                service.GetProposedPaymentPlan("124", "01");
            }

            /// <summary>
            /// Proxy cannot get data
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void GetProposedPaymentPlan_ProxyCannotGetData()
            {
                SetupProxyUser();
                studentId = userFactoryMock.Object.CurrentUser.ProxySubjects.First().PersonId;
                service.GetProposedPaymentPlan("124", "01");
            }
        }

        #endregion

        #region ProcessInvoiceExclusionRules tests

        [TestClass]
        public class ProcessInvoiceExclusionRules : RegistrationBillingServiceTests
        {
            [TestInitialize]
            public void Initialize_ProcessInvoiceExclusionRules()
            {
                Initialize();
                SetupStudentUser();

                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.Invoice>(It.IsAny<IEnumerable<Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.Invoice>>>())).
                    Returns(new List<RuleResult>()
                    {
                        new RuleResult() { Passed = false, RuleId = "RULE1"},
                    });
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProcessInvoiceExclusionRules_NullInvoices()
            {
                var result = service.ProcessInvoiceExclusionRules(null, new List<string>() { "RULE1" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProcessInvoiceExclusionRules_EmptyInvoices()
            {
                var result = service.ProcessInvoiceExclusionRules(new List<Domain.Finance.Entities.Invoice>(), new List<string>() { "RULE1" });
            }

            [TestMethod]
            public void ProcessInvoiceExclusionRules_NullRuleIds()
            {
                var result = service.ProcessInvoiceExclusionRules(new List<Domain.Finance.Entities.Invoice>()
                    {
                        new Domain.Finance.Entities.Invoice("123", "1234567", "01", "2014/FA", "REF123", DateTime.Today, DateTime.Today.AddDays(7), 
                            DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DESC123", new List<Domain.Finance.Entities.Charge>()
                            {
                                new Domain.Finance.Entities.Charge("201", "123", new List<string>() { "CHG201" }, "CHG1", 500m),
                                new Domain.Finance.Entities.Charge("202", "123", new List<string>() { "CHG202" }, "CHG2", 250m),
                            })
                    }, null);
                Assert.AreEqual(1, result.Count);
            }

            [TestMethod]
            public void ProcessInvoiceExclusionRules_EmptyRuleIds()
            {
                var result = service.ProcessInvoiceExclusionRules(new List<Domain.Finance.Entities.Invoice>()
                    {
                        new Domain.Finance.Entities.Invoice("123", "1234567", "01", "2014/FA", "REF123", DateTime.Today, DateTime.Today.AddDays(7), 
                            DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DESC123", new List<Domain.Finance.Entities.Charge>()
                            {
                                new Domain.Finance.Entities.Charge("201", "123", new List<string>() { "CHG201" }, "CHG1", 500m),
                                new Domain.Finance.Entities.Charge("202", "123", new List<string>() { "CHG202" }, "CHG2", 250m),
                            })
                    }, new List<string>());
                Assert.AreEqual(1, result.Count);
            }

            [TestMethod]
            public void ProcessInvoiceExclusionRules_RuleFailure()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.Invoice>(It.IsAny<IEnumerable<Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.Invoice>>>())).
                    Returns(new List<RuleResult>()
                                    {
                                        new RuleResult() { Passed = true, RuleId = "RULE1"},
                                    });
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.ProcessInvoiceExclusionRules(new List<Domain.Finance.Entities.Invoice>()
                    {
                        new Domain.Finance.Entities.Invoice("123", "1234567", "01", "2014/FA", "REF123", DateTime.Today, DateTime.Today.AddDays(7), 
                            DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DESC123", new List<Domain.Finance.Entities.Charge>()
                            {
                                new Domain.Finance.Entities.Charge("201", "123", new List<string>() { "CHG201" }, "CHG1", 500m),
                                new Domain.Finance.Entities.Charge("202", "123", new List<string>() { "CHG202" }, "CHG2", 250m),
                            })
                    }, new List<string>() { "RULE1" });
                Assert.AreEqual(0, result.Count);
            }

            [TestMethod]
            public void ProcessInvoiceExclusionRules_RulePasses()
            {
                var result = service.ProcessInvoiceExclusionRules(new List<Domain.Finance.Entities.Invoice>()
                    {
                        new Domain.Finance.Entities.Invoice("123", "1234567", "01", "2014/FA", "REF123", DateTime.Today, DateTime.Today.AddDays(7), 
                            DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DESC123", new List<Domain.Finance.Entities.Charge>()
                            {
                                new Domain.Finance.Entities.Charge("201", "123", new List<string>() { "CHG201" }, "CHG1", 500m),
                                new Domain.Finance.Entities.Charge("202", "123", new List<string>() { "CHG202" }, "CHG2", 250m),
                            })
                    }, new List<string>() { "RULE1" });
                Assert.AreEqual(1, result.Count);
            }

        }

        #endregion

        #region Evaluate Payment Requirements tests

        [TestClass]
        public class EvaluatePaymentRequirements : RegistrationBillingServiceTests
        {
            string studentId = "0003315";
            List<Domain.Finance.Entities.PaymentRequirement> paymentRequirements;
            List<string> termIds;

            [TestInitialize]
            public void EvaluatePaymentRequirements_Initialize()
            {
                base.Initialize();
                SetupPaymentRequirements();

                termIds = this.ipcps.Select(x => x.IpcpTerm).Distinct().ToList();
                foreach (string term in termIds)
                {
                    paymentRequirements = this.payReqs.Where(x => x.TermId == term).ToList();
                }

                arRepoMock.Setup(repo => repo.GetAccountHolder(It.IsAny<string>())).Returns(new Domain.Finance.Entities.AccountHolder("1234567", "Smith", null));
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_EvaluatePaymentRequirements_NullStudentId()
            {
                var result = service.EvaluatePaymentRequirements( null, paymentRequirements);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_EvaluatePaymentRequirements_EmptyStudentId()
            {
                var result = service.EvaluatePaymentRequirements( string.Empty, paymentRequirements);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_EvaluatePaymentRequirements_NullPaymentRequirements()
            {
                var result = service.EvaluatePaymentRequirements( studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingService_EvaluatePaymentRequirements_EmptyPaymentRequirements()
            {
                var result = service.EvaluatePaymentRequirements( studentId, new List<Domain.Finance.Entities.PaymentRequirement>());
            }

            [TestMethod]
            public void RegistrationBillingService_EvaluatePaymentRequirements_NoRules()
            {
                var result = service.EvaluatePaymentRequirements(studentId, new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "2014/FA", null, 0, 
                            new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })
                    });
                Assert.AreEqual("1", result.Id);
                Assert.AreEqual(2, result.DeferralOptions.Count);
                Assert.AreEqual(1, result.PaymentPlanOptions.Count);
            }

            [TestMethod]
            public void RegistrationBillingService_EvaluatePaymentRequirements_Rules()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).
                    Returns(new List<RuleResult>()
                    {
                        new RuleResult() { Passed = true, RuleId = "RULE1"},
                    });
                service = new RegistrationBillingService(adapterMock.Object, rbRepoMock.Object, arRepoMock.Object, adRepoMock.Object, fcRepoMock.Object,
                    pmtRepoMock.Object, ppRepoMock.Object, apprSvcMock.Object, ruleRepoMock.Object, docRepoMock.Object,
                    userFactoryMock.Object, roleRepoMock.Object, loggerMock.Object);

                var result = service.EvaluatePaymentRequirements(studentId, new List<Domain.Finance.Entities.PaymentRequirement>()
                    {
                        new Domain.Finance.Entities.PaymentRequirement("1", "2014/FA", null, 0, 
                            new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 75m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), "DEFAULT",
                                    DateTime.Today.AddDays(7)),
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT",
                                    DateTime.Today.AddDays(14))
                            }),
                        new Domain.Finance.Entities.PaymentRequirement("2", "2014/FA", "RULE1", 0, 
                            new List<Domain.Finance.Entities.PaymentDeferralOption>()
                            {
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), 50m),
                                new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), null, 0m)
                            },
                            new List<Domain.Finance.Entities.PaymentPlanOption>()
                            {
                                new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(-31), "DEFAULT",
                                    DateTime.Today.AddDays(7))
                            })

                    });
                Assert.AreEqual("2", result.Id);
                Assert.AreEqual(2, result.DeferralOptions.Count);
                Assert.AreEqual(1, result.PaymentPlanOptions.Count);
            }
        }

        #endregion

        #region Payment Requirements Setup

        private void SetupPaymentRequirements()
        {
            dataReaderMock.Setup<Collection<IpcPaymentReqs>>(
                accessor => accessor.BulkReadRecord<IpcPaymentReqs>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((criteria, flag) =>
                    {
                        // Get the term ID from the selection criteria
                        string termId = criteria.Split('\'')[1];
                        // Return the payment requirements collection for the desired person
                        return new Collection<IpcPaymentReqs>(this.ipcps.Where(ipcp => ipcp.IpcpTerm == termId).ToList());
                    });
        }

        #endregion

        #region User Setup

        private void SetupAdminUser()
        {
            var user = new CurrentUser(new Claims()
            {
                ControlId = "127",
                Name = "Bursar",
                PersonId = "0034567",
                UserName = "Admin",
                SecurityToken = "wxyz",
                SessionFixationId = "aabc",
                SessionTimeout = 30,
                Roles = new List<string>() { "Finance Administrator" }
            }
            );
            userFactoryMock.Setup(x => x.CurrentUser).Returns(user);

            adminRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Domain.Finance.FinancePermissionCodes.ViewStudentAccountActivity));
            roleRepoMock.Setup(role => role.Roles).Returns(new List<Domain.Entities.Role>() { adminRole });
        }

        private void SetupAdminUserWithNoPermissions()
        {
            var user = new CurrentUser(new Claims()
            {
                ControlId = "127",
                Name = "Bursar",
                PersonId = "0034567",
                UserName = "Admin",
                SecurityToken = "wxyz",
                SessionFixationId = "aabc",
                SessionTimeout = 30,
                Roles = new List<string>() { "Finance Administrator" }
            }
            );
            userFactoryMock.Setup(x => x.CurrentUser).Returns(user);
        }

        private void SetupStudentUser()
        {
            userFactoryMock.Setup(x => x.CurrentUser).Returns(new CurrentUser(new Claims()
            {
                ControlId = "123",
                Name = "Tester",
                PersonId = "1234567",
                UserName = "Student",
                SecurityToken = "xyz",
                SessionFixationId = "abac",
                SessionTimeout = 30,
                Roles = new List<string>()
            }
            ));
        }

        private void SetupProxyUser()
        {
            userFactoryMock.Setup(x => x.CurrentUser).Returns(new CurrentUser(new Claims()
            {
                ControlId = "123",
                Name = "Greg",
                PersonId = "0003943",
                SecurityToken = "321",
                SessionTimeout = 30,
                UserName = "Student",
                Roles = new List<string>() { },
                SessionFixationId = "abc123",
                ProxySubjectClaims = new ProxySubjectClaims()
                {
                    PersonId = "0003315"
                }
            }));
        }

        private void SetupProxyUserForDifferentPerson()
        {
            userFactoryMock.Setup(x => x.CurrentUser).Returns(new CurrentUser(new Claims()
            {
                ControlId = "123",
                Name = "Greg",
                PersonId = "0003943",
                SecurityToken = "321",
                SessionTimeout = 30,
                UserName = "Student",
                Roles = new List<string>() { },
                SessionFixationId = "abc123",
                ProxySubjectClaims = new ProxySubjectClaims()
                {
                    PersonId = "foo"
                }
            }));
        }

        #endregion
    }
}
