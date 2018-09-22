// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Data.Student.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using System.Collections;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class RegistrationBillingRepositoryTests : BaseRepositorySetup
    {
        RegistrationBillingRepository repository;

        List<RegArPostings> regArPostings = new List<RegArPostings>();
        List<RegArPostingItems> regArPostingItems = new List<RegArPostingItems>();
        List<StudentCourseSec> studentCourseSec = new List<StudentCourseSec>();
        List<StudentAcadCred> studentAcadCred = new List<StudentAcadCred>();
        List<StudentAcadCredCc> studentAcadCredCc = new List<StudentAcadCredCc>();
        List<CourseSections> courseSections = new List<CourseSections>();
        Collection<Courses> courses = new Collection<Courses>();
        static PaymentTermsAcceptance validAcceptance;
        static PaymentTermsAcceptance errorAcceptance;
        static ApproveRegistrationTermsRequest validTermsRequest;
        static ApproveRegistrationTermsRequest errorTermsRequest;
        static ApproveRegistrationTermsResponse validTermsResponse;
        static ApproveRegistrationTermsResponse errorTermsResponse;
        static RegistrationPaymentControl newPaymentControl;
        static RegistrationPaymentControl acceptPaymentControl;
        static RegistrationPaymentControl completePaymentControl;
        static RegistrationPaymentControl errorPaymentControl;
        static RegistrationPaymentControl errorPaymentControl2;
        static UpdateRegistrationPaymentControlRequest validUpdateRequest;
        static UpdateRegistrationPaymentControlRequest errorUpdateRequest;
        static UpdateRegistrationPaymentControlResponse validUpdateResponse;
        static UpdateRegistrationPaymentControlResponse errorUpdateResponse;
        static Payment validPayment;
        static Payment errorPayment;
        static StartStudentPaymentRequest validPaymentRequest;
        static StartStudentPaymentRequest errorPaymentRequest;
        static StartStudentPaymentResponse validPaymentResponse;
        static StartStudentPaymentResponse errorPaymentResponse;

        Collection<IpcPaymentReqs> ipcps = TestIpcPaymentReqsRepository.IpcPaymentReqs;
        Collection<IpcRegApprovals> ipcras = TestIpcRegApprovalsRepository.IpcRegApprovals;
        Collection<IpcRegistration> ipcrs = TestIpcRegistrationRepository.IpcRegistrations;
        Collection<StudentCourseSec> scss = TestStudentCourseSecRepository.StudentCourseSecs;
        Collection<PayPlanApprovals> ppas = TestPayPlanApprovalsRepository.PayPlanApprovals;
        List<Rules> rules = new List<Rules>();
        List<Ellucian.Colleague.Data.Base.DataContracts.Person> accountHolders = new List<Ellucian.Colleague.Data.Base.DataContracts.Person>();

        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();
            SetupImmediatePaymentControl();

            // Build the test repository
            this.repository = new RegistrationBillingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        #region Get Student Payment Controls tests

        [TestClass]
        public class GetStudentPaymentControls : RegistrationBillingRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetStudentPaymentControls()
            {
                base.Initialize();
                SetupStudentCourseSecs();
                SetupIpcRegistrations();
                var ipControl = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "Y" };
                dataReaderMock.Setup(reader => reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", It.IsAny<bool>()))
                    .Returns(ipControl);
                var repository = new RegistrationBillingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetStudentPaymentControls_NullStudentId()
            {
                var result = repository.GetStudentPaymentControls(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetStudentPaymentControls_EmptyStudentId()
            {
                var result = repository.GetStudentPaymentControls(string.Empty);
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetStudentPaymentControls_IpcDisabled()
            {
                var ipControl = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "N" };
                dataReaderMock.Setup(reader => reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", It.IsAny<bool>()))
                    .Returns(ipControl);
                var repository = new RegistrationBillingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = repository.GetStudentPaymentControls(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetStudentPaymentControls_NullIpcConfig()
            {
                DataContracts.ImmediatePaymentControl ipcControl = null;
                dataReaderMock.Setup(reader => reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", It.IsAny<bool>()))
                    .Returns(ipcControl);
                var repository = new RegistrationBillingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = repository.GetStudentPaymentControls(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetStudentPaymentControls_ValidStudentId()
            {
                var personIds = ipcrs.Select(i => i.IpcrStudent);
                foreach (var personId in personIds)
                {
                    var result = repository.GetStudentPaymentControls(personId);
                    Assert.AreEqual(ipcrs.Where(i => i.IpcrStudent == personId).Count(), result.Count());
                }
            }
        }

        #endregion

        #region Get Payment Control tests

        [TestClass]
        public class GetPaymentControl : RegistrationBillingRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetPaymentControl()
            {
                base.Initialize();
                SetupIpcRegistrations();
                SetupStudentCourseSecs();
                var ipControl = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "Y" };
                dataReaderMock.Setup(reader => reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", It.IsAny<bool>()))
                    .Returns(ipControl);
                var repository = new RegistrationBillingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetPaymentControl_NullId()
            {
                var result = repository.GetPaymentControl(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetPaymentControl_EmptyId()
            {
                var result = repository.GetPaymentControl(string.Empty);
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetPaymentControl_IpcDisabled()
            {
                var ipControl = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "N" };
                dataReaderMock.Setup(reader => reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", It.IsAny<bool>()))
                    .Returns(ipControl);
                var repository = new RegistrationBillingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = repository.GetPaymentControl(null);
                Assert.AreEqual(null, result);
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetPaymentControl_Verify()
            {
                var result = repository.GetPaymentControl(ipcrs[0].Recordkey);
                Assert.AreEqual(ipcrs[0].Recordkey, result.Id);
                Assert.AreEqual(ipcrs[0].IpcrStudent, result.StudentId);
                Assert.AreEqual(ipcrs[0].IpcrTerm, result.TermId);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void RegistrationBillingRepository_GetPaymentControl_BadRecord()
            {
                var result = repository.GetPaymentControl("BADID");
            }
        }

        #endregion

        #region Get Registration Billing tests

        [TestClass]
        public class GetRegistrationBilling : RegistrationBillingRepositoryTests
        {
            List<string> rgarIds = new List<string>();

            [TestInitialize]
            public async void GetRegistrationBilling_Initialize()
            {
                base.Initialize();

                SetupRegArPostings();
                SetupRegArPostingItems();
                SetupStudentCourseSec();
                SetupStudentAcadCred();
                SetupCourses();
                await SetupAcademicCreditCodes();
                SetupCoursesCodes();

                this.rgarIds = this.regArPostings.Select(x => x.Recordkey).Where(x => !x.EndsWith("FAIL") && !x.EndsWith("NULL")).ToList();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetRegistrationBilling_NullId()
            {
                var result = this.repository.GetRegistrationBilling(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetRegistrationBilling_EmptyId()
            {
                var result = this.repository.GetRegistrationBilling(String.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RegistrationBillingRepository_GetRegistrationBilling_InvalidId()
            {
                var result = this.repository.GetRegistrationBilling("InvalidId");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetRegistrationBilling_NoRgariIds()
            {
                var result = this.repository.GetRegistrationBilling("RGARINULL");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RegistrationBillingRepository_GetRegistrationBilling_InvalidRgariIds()
            {
                var result = this.repository.GetRegistrationBilling("RGARIFAIL");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RegistrationBillingRepository_GetRegistrationBilling_InvalidScsId()
            {
                var result = this.repository.GetRegistrationBilling("SCSFAIL");
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidId()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.Recordkey, result.Id, "ID is " + source.Recordkey);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidPersonId()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.RgarStudent, result.PersonId, "ID is " + source.Recordkey);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidAccountTypeCode()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.RgarArType, result.AccountTypeCode, "ID is " + source.Recordkey);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidTermId()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.RgarTerm, result.TermId, "ID is " + source.Recordkey);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidBillingStart()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.RgarBillingStartDate, result.BillingStart, "ID is " + source.Recordkey);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidBillingEnd()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.RgarBillingEndDate, result.BillingEnd, "ID is " + source.Recordkey);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidInvoiceId()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.RgarInvoice, result.InvoiceId, "ID is " + source.Recordkey);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBilling_ValidItemsCount()
            {
                foreach (var rgar in rgarIds)
                {
                    var source = this.regArPostings.Where(x => x.Recordkey == rgar).FirstOrDefault();
                    var result = this.repository.GetRegistrationBilling(rgar);

                    Assert.AreEqual(source.RgarRegArPostingItems.Count, result.Items.Count(), "ID is " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Get Registration Billing Item tests

        [TestClass]
        public class GetRegistrationBillingItems : RegistrationBillingRepositoryTests
        {
            List<string> rgariIds = new List<string>();

            [TestInitialize]
            public void GetRegistrationBillingItems_Initialize()
            {
                base.Initialize();
                SetupRegArPostingItems();
                SetupStudentCourseSec();

                this.rgariIds = this.regArPostingItems.Select(x => x.Recordkey).Where(x => !x.EndsWith("FAIL") && !x.EndsWith("NULL")).ToList();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetRegistrationBillingItems_NullIds()
            {
                var result = this.repository.GetRegistrationBillingItems(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetRegistrationBillingItems_NoIds()
            {
                var result = this.repository.GetRegistrationBillingItems(new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RegistrationBillingRepository_GetRegistrationBillingItems_InvalidId()
            {
                var result = this.repository.GetRegistrationBillingItems(new List<string>() { "INVALID" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RegistrationBillingRepository_GetRegistrationBillingItems_NoScs()
            {
                var result = this.repository.GetRegistrationBillingItems(new List<string>() { "SCSFAIL" });
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBillingItems_ValidId()
            {
                foreach (var rgari in this.rgariIds)
                {
                    var source = this.regArPostingItems.Where(x => x.Recordkey == rgari).FirstOrDefault();
                    var result = this.repository.GetRegistrationBillingItems(new List<string>() { rgari });

                    Assert.IsNotNull(result, "ID=" + rgari);
                    Assert.IsNotNull(result.First(), "ID=" + rgari);
                    Assert.AreEqual(source.Recordkey, result.First().Id, "ID=" + rgari);
                }
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetRegistrationBillingItems_ValidAcademicCredit()
            {
                foreach (var rgari in this.rgariIds)
                {
                    var source = this.regArPostingItems.Where(x => x.Recordkey == rgari).FirstOrDefault();
                    var result = this.repository.GetRegistrationBillingItems(new List<string>() { rgari });

                    Assert.IsNotNull(result, "ID=" + rgari);
                    Assert.IsNotNull(result.First(), "ID=" + rgari);
                    Assert.IsNotNull(result.First().AcademicCreditId, "ID=" + rgari);
                    var scs = this.studentCourseSec.Where(x => x.Recordkey == source.RgariStudentCourseSec).FirstOrDefault();
                    Assert.AreEqual(scs.ScsStudentAcadCred, result.First().AcademicCreditId, "ID=" + rgari);
                }
            }
        }

        #endregion

        #region Get Payment Requirements tests

        [TestClass]
        public class GetPaymentRequirements : RegistrationBillingRepositoryTests
        {
            List<string> termIds = new List<string>();

            [TestInitialize]
            public void GetPaymentRequirements_Initialize()
            {
                base.Initialize();
                SetupPaymentRequirements();

                this.termIds = this.ipcps.Select(x => x.IpcpTerm).Distinct().ToList();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetPaymentRequirements_NullTermId()
            {
                var result = this.repository.GetPaymentRequirements(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_GetPaymentRequirements_EmptyTermId()
            {
                var result = this.repository.GetPaymentRequirements(String.Empty);
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetPaymentRequirements_ValidTermNoReqs()
            {
                var result = this.repository.GetPaymentRequirements("VALIDTERM");
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void RegistrationBillingRepository_GetPaymentRequirements_ValidTermId()
            {
                foreach (var term in termIds)
                {
                    List<IpcPaymentReqs> source = this.ipcps.Where(x => x.IpcpTerm == term).ToList();
                    List<PaymentRequirement> result = this.repository.GetPaymentRequirements(term).ToList();
                    Assert.AreEqual(source.Count, result.Count, "Counts not equal for term " + term);
                    for (int i = 0; i < source.Count(); i++)
                    {
                        Assert.AreEqual(source[i].Recordkey, result[i].Id);
                        Assert.AreEqual(source[i].IpcpTerm, result[i].TermId);
                        Assert.AreEqual(source[i].IpcpEligibilityRule, result[i].EligibilityRuleId);
                        Assert.AreEqual((int)source[i].IpcpRuleEvalOrder, result[i].ProcessingOrder);

                        for (int j = 0; j < source[i].IpcpDeferralsEntityAssociation.Count(); j++)
                        {
                            Assert.AreEqual(source[i].IpcpDeferralsEntityAssociation[j].IpcpDeferEffectiveStartAssocMember, result[i].DeferralOptions.ToList()[j].EffectiveStart);
                            Assert.AreEqual(source[i].IpcpDeferralsEntityAssociation[j].IpcpDeferEffectiveEndAssocMember, result[i].DeferralOptions.ToList()[j].EffectiveEnd);
                            Assert.AreEqual(source[i].IpcpDeferralsEntityAssociation[j].IpcpDeferPctAssocMember, result[i].DeferralOptions.ToList()[j].DeferralPercent);
                        }

                        for (int k = 0; k < source[i].IpcpPayPlansEntityAssociation.Count(); k++)
                        {
                            Assert.AreEqual(source[i].IpcpPayPlansEntityAssociation[k].IpcpPlanEffectiveStartAssocMember, result[i].PaymentPlanOptions.ToList()[k].EffectiveStart);
                            Assert.AreEqual(source[i].IpcpPayPlansEntityAssociation[k].IpcpPlanEffectiveEndAssocMember, result[i].PaymentPlanOptions.ToList()[k].EffectiveEnd);
                            Assert.AreEqual(source[i].IpcpPayPlansEntityAssociation[k].IpcpPayPlanTemplateAssocMember, result[i].PaymentPlanOptions.ToList()[k].TemplateId);
                            Assert.AreEqual(source[i].IpcpPayPlansEntityAssociation[k].IpcpPlanStartDateAssocMember, result[i].PaymentPlanOptions.ToList()[k].FirstPaymentDate);
                        }
                    }
                }
            }
        }
        #endregion

        #region Update Payment Control tests

        [TestClass]
        public class UpdatePaymentControl : RegistrationBillingRepositoryTests
        {
            [TestInitialize]
            public void UpdatePaymentControl_Initialize()
            {
                base.Initialize();
                SetupIpcRegistrations();
                SetupStudentCourseSecs();
                newPaymentControl = new RegistrationPaymentControl("102", "0005635", "2014/FA", RegistrationPaymentStatus.New);
                acceptPaymentControl = new RegistrationPaymentControl("100", "0010906", "2014/FA", RegistrationPaymentStatus.Accepted);
                completePaymentControl = new RegistrationPaymentControl("101", "0003985", "2014/FA", RegistrationPaymentStatus.Complete);
                errorPaymentControl = new RegistrationPaymentControl("300", "0003315", "2015/SP", RegistrationPaymentStatus.Error);
                errorPaymentControl2 = new RegistrationPaymentControl("44", "0003939", "2014/FA", RegistrationPaymentStatus.Error);
                validUpdateRequest = new UpdateRegistrationPaymentControlRequest()
                {
                    PaymentControlId = "100",
                    PaymentIds = new List<string>() { "1000" },
                    PaymentStatus = "ACCEPT"
                };

                errorUpdateRequest = new UpdateRegistrationPaymentControlRequest()
                {
                    PaymentControlId = "300",
                    PaymentIds = new List<string>(),
                    PaymentStatus = "BAD"
                };
                validUpdateResponse = new UpdateRegistrationPaymentControlResponse();
                errorUpdateResponse = new UpdateRegistrationPaymentControlResponse()
                {
                    ErrorMessage = "There was a problem updating the IPC record."
                };
                SetupRegistrationPaymentControls();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_UpdatePaymentControl_NullRegistrationPaymentControl()
            {
                this.repository.UpdatePaymentControl(null);
            }

            [TestMethod]
            public void RegistrationBillingRepository_UpdatePaymentControl_ValidNewRequest()
            {
                var result = this.repository.UpdatePaymentControl(newPaymentControl);
                Assert.AreEqual(RegistrationPaymentStatus.New, result.PaymentStatus);
            }

            [TestMethod]
            public void RegistrationBillingRepository_UpdatePaymentControl_ValidAcceptRequest()
            {
                var result = this.repository.UpdatePaymentControl(acceptPaymentControl);
                Assert.AreEqual(RegistrationPaymentStatus.Accepted, result.PaymentStatus);
            }

            [TestMethod]
            public void RegistrationBillingRepository_UpdatePaymentControl_ValidCompleteRequest()
            {
                var result = this.repository.UpdatePaymentControl(completePaymentControl);
                Assert.AreEqual(RegistrationPaymentStatus.Complete, result.PaymentStatus);
            }

            [TestMethod]
            public void RegistrationBillingRepository_UpdatePaymentControl_ValidErrorRequest()
            {
                var result = this.repository.UpdatePaymentControl(errorPaymentControl2);
                Assert.AreEqual(RegistrationPaymentStatus.Error, result.PaymentStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void RegistrationBillingRepository_UpdatePaymentControl_BadRequest()
            {
                var result = this.repository.UpdatePaymentControl(errorPaymentControl);
            }
        }
        #endregion

        #region ApproveRegistrationTerms tests

        [TestClass]
        public class ApproveRegistration : RegistrationBillingRepositoryTests
        {
            [TestInitialize]
            public void ApproveRegistration_Initialize()
            {
                base.Initialize();
                SetupRegistrationPaymentControls();
                SetupRegistrationApprovals();
                SetupIpcRegistrations();
                SetupStudentCourseSecs();

                var threeMinutesAgo = DateTime.Now.AddMinutes(-3);
                var now = DateTime.Now;
                validAcceptance = new PaymentTermsAcceptance("0003315", "100", threeMinutesAgo, new List<string>() { "1796", "1797" },
                    new List<string>() { "123", "124" }, new List<string>() { "This is the terms text." }, "joe_student", now)
                    {
                        AcknowledgementText = new List<string>() { "This is the acknowledgement text." }
                    };
                errorAcceptance = new PaymentTermsAcceptance("0003316", "101", threeMinutesAgo, new List<string>() { "1796", "1797" },
                    new List<string>() { "123", "124" }, new List<string>() { "This is the terms text." }, "bad_student", now)
                    {
                        AcknowledgementText = new List<string>() { "This is the acknowledgement text." }
                    };
                validTermsRequest = new ApproveRegistrationTermsRequest()
                {
                    AcknowledgementDate = DateTime.Today,
                    AcknowledgementDocumentId = "101",
                    AcknowledgementText = new List<string>() { "This is the acknowledgement text." },
                    AcknowledgementTime = threeMinutesAgo,
                    ApprovalDate = DateTime.Today,
                    ApprovalTime = now,
                    ApprovalUserid = "joe_student",
                    InvoiceIds = new List<string>() { "1796", "1797"},
                    SectionIds = new List<string>() { "123", "124"},
                    PaymentControlId = "100",
                    StudentId = "0003315",
                    TermsDocumentId = "IPCREGTC",
                    TermsResponseId = "102",
                    TermsText = new List<string>() { "This is the terms text." }
                };
                errorTermsRequest = new ApproveRegistrationTermsRequest()
                {
                    AcknowledgementDate = DateTime.Today,
                    AcknowledgementDocumentId = "101",
                    AcknowledgementText = new List<string>() { "This is the acknowledgement text." },
                    AcknowledgementTime = threeMinutesAgo,
                    ApprovalDate = DateTime.Today,
                    ApprovalTime = now,
                    ApprovalUserid = "bad_student",
                    InvoiceIds = new List<string>() { "1796", "1797" },
                    SectionIds = new List<string>() { "123", "124" },
                    PaymentControlId = "100",
                    StudentId = "0003316",
                    TermsDocumentId = "IPCREGTC",
                    TermsResponseId = "103",
                    TermsText = new List<string>() { "This is the terms text." }
                };
                validTermsResponse = new ApproveRegistrationTermsResponse()
                {
                    AcknowledgementDocumentId = "1",
                    ErrorMessage = null,
                    RegistrationApprovalId = "100",
                    TermsDocumentId = "IPCREGTC",
                    TermsResponseId = "100"
                };
                errorTermsResponse = new ApproveRegistrationTermsResponse()
                {
                    AcknowledgementDocumentId = null,
                    ErrorMessage = "Could not accept the terms.",
                    RegistrationApprovalId = null,
                    TermsDocumentId = null,
                    TermsResponseId = null
                };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_ApproveRegistration_NullRegistrationApproval()
            {
                var approval = this.repository.ApproveRegistrationTerms(null);
            }

            [TestMethod]
            public void RegistrationBillingRepository_ApproveRegistration_ValidRegistrationApproval()
            {
                var approval = this.repository.ApproveRegistrationTerms(validAcceptance);
                Assert.AreEqual("100", approval.Id);
                Assert.AreEqual("2", approval.PaymentControlId);
                Assert.AreEqual("0003900", approval.StudentId);
                Assert.AreEqual("100", approval.TermsResponseId);
                Assert.AreEqual("1", approval.AcknowledgementDocumentId);
                Assert.AreEqual(4, approval.SectionIds.Count);
                Assert.AreEqual(1, approval.InvoiceIds.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void RegistrationBillingRepository_ApproveRegistration_InvalidRegistrationApproval()
            {
                var approval = this.repository.ApproveRegistrationTerms(errorAcceptance);
            }
        }

        #endregion

        #region StartRegistrationPayment tests

        [TestClass]
        public class StartRegistrationPayment : RegistrationBillingRepositoryTests
        {
            [TestInitialize]
            public void StartRegistrationPayment_Initialize()
            {
                base.Initialize();
                validPayment = new Payment()
                {
                    AmountToPay = 100m,
                    CheckDetails = new CheckPayment()
                    {
                        AbaRoutingNumber = "062203298",
                        BankAccountNumber = "1234554321",
                        BillingAddress1 = "123 Main Street",
                        CheckNumber = "101",
                        City = "Fairfax",
                        State = "VA",
                        ZipCode = "22033",
                        FirstName = "John",
                        LastName = "Smith",
                        EmailAddress = "john.smith@ellucianuniv.edu"
                    },
                    ConvenienceFee = "CVFE",
                    ConvenienceFeeAmount = 5m,
                    ConvenienceFeeGeneralLedgerNumber = null,
                    Distribution = "BANK",
                    PayMethod = "ECHK",
                    PaymentItems = new List<PaymentItem>()
                    {
                        new PaymentItem()
                        {
                            AccountType = "01",
                            Description = "Payment on account",
                            Overdue = false,
                            InvoiceId = "1234",
                            PaymentAmount = 105m,
                            PaymentComplete = false,
                            PaymentControlId = "100",
                            Term = "2014/FA"
                        }
                    },
                    PersonId = "0003315",
                    ProviderAccount = "PPECHK",
                    ReturnUrl = "http://www.ellucianuniv.edu/Finance",
                    ReturnToOriginUrl = "/Finance/"
                };
                errorPayment = new Payment()
                {
                    AmountToPay = 200m,
                    CheckDetails = new CheckPayment()
                    {
                        AbaRoutingNumber = "062203298",
                        BankAccountNumber = "1234554321",
                        BillingAddress1 = "123 Main Street",
                        CheckNumber = "101",
                        City = "Fairfax",
                        State = "VA",
                        ZipCode = "22033",
                        FirstName = "John",
                        LastName = "Smith",
                        EmailAddress = "john.smith@ellucianuniv.edu"
                    },
                    ConvenienceFee = "CVFE",
                    ConvenienceFeeAmount = 5m,
                    ConvenienceFeeGeneralLedgerNumber = null,
                    Distribution = "TRAV",
                    PayMethod = "OPCC",
                    PaymentItems = new List<PaymentItem>()
                    {
                        new PaymentItem()
                        {
                            AccountType = "01",
                            Description = "Payment on account",
                            Overdue = false,
                            InvoiceId = "1234",
                            PaymentAmount = 205m,
                            PaymentComplete = false,
                            PaymentControlId = "100",
                            Term = "2014/FA"
                        }
                    },
                    PersonId = "0003315",
                    ProviderAccount = "PPECHK",
                    ReturnUrl = "http://www.ellucianuniv.edu/Finance",
                    ReturnToOriginUrl = "/Finance/"
                };
                validPaymentRequest = new StartStudentPaymentRequest()
                {
                    InAmtToPay = validPayment.AmountToPay,
                    InConvFee = validPayment.ConvenienceFee,
                    InConvFeeAmt = validPayment.ConvenienceFeeAmount,
                    InConvFeeGlNo = validPayment.ConvenienceFeeGeneralLedgerNumber,
                    InDistribution = validPayment.Distribution,
                    InPayMethod = validPayment.PayMethod,
                    InPersonId = validPayment.PersonId,
                    InProviderAcct = validPayment.ProviderAccount,
                    InReturnUrl = validPayment.ReturnUrl,
                    InSfipcReturnUrl = validPayment.ReturnToOriginUrl,
                    InPayments = new List<InPayments>()
                    {
                        new InPayments()
                        {
                            InPmtAmts = validPayment.PaymentItems[0].PaymentAmount,
                            InPmtArTypes = validPayment.PaymentItems[0].AccountType,
                            InPmtDescs = validPayment.PaymentItems[0].Description,
                            InPmtInvoices = validPayment.PaymentItems[0].InvoiceId,
                            InPmtOverdues = validPayment.PaymentItems[0].Overdue,
                            InPmtTerms = validPayment.PaymentItems[0].Term,
                            InSfipcPmtComplete = validPayment.PaymentItems[0].PaymentComplete ? "Y" : "N",
                            InSfipcRegControlId = validPayment.PaymentItems[0].PaymentControlId
                        }
                    }
                };
                errorPaymentRequest = new StartStudentPaymentRequest()
                {
                    InAmtToPay = errorPayment.AmountToPay,
                    InConvFee = errorPayment.ConvenienceFee,
                    InConvFeeAmt = errorPayment.ConvenienceFeeAmount,
                    InConvFeeGlNo = errorPayment.ConvenienceFeeGeneralLedgerNumber,
                    InDistribution = errorPayment.Distribution,
                    InPayMethod = errorPayment.PayMethod,
                    InPersonId = errorPayment.PersonId,
                    InProviderAcct = errorPayment.ProviderAccount,
                    InReturnUrl = errorPayment.ReturnUrl,
                    InSfipcReturnUrl = errorPayment.ReturnToOriginUrl,
                    InPayments = new List<InPayments>()
                    {
                        new InPayments()
                        {
                            InPmtAmts = errorPayment.PaymentItems[0].PaymentAmount,
                            InPmtArTypes = errorPayment.PaymentItems[0].AccountType,
                            InPmtDescs = errorPayment.PaymentItems[0].Description,
                            InPmtInvoices = errorPayment.PaymentItems[0].InvoiceId,
                            InPmtOverdues = errorPayment.PaymentItems[0].Overdue,
                            InPmtTerms = errorPayment.PaymentItems[0].Term,
                            InSfipcPmtComplete = errorPayment.PaymentItems[0].PaymentComplete ? "Y" : "N",
                            InSfipcRegControlId = errorPayment.PaymentItems[0].PaymentControlId
                        }
                    }
                };
                validPaymentResponse = new StartStudentPaymentResponse()
                {
                    OutStartUrl = "http://www.ellucianuniv.edu"
                };
                errorPaymentResponse = new StartStudentPaymentResponse()
                {
                    OutErrorMsg = "Unable to start student payment."
                };
                SetupStudentPayments();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationBillingRepository_StartRegistrationPayment_NullPayment()
            {
                this.repository.StartRegistrationPayment(null);
            }

            [TestMethod]
            public void RegistrationBillingRepository_StartRegistrationPayment_ValidPayment()
            {
                this.repository.StartRegistrationPayment(validPayment);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void RegistrationBillingRepository_StartRegistrationPayment_ErrorPayment()
            {
                this.repository.StartRegistrationPayment(errorPayment);
            }
        }

        #endregion

        #region Get Terms Approval tests

        [TestClass]
        public class GetTermsApproval : RegistrationBillingRepositoryTests
        {
            [TestInitialize]
            public void GetTermsApproval_Initialize()
            {
                base.Initialize();
                SetupIpcRegistrations();
                SetupRegistrationApprovals();
                SetupStudentCourseSecs();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetTermsApproval_NullId()
            {
                var approval = this.repository.GetTermsApproval(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void GetTermsApproval_InvalidId()
            {
                var approval = this.repository.GetTermsApproval("InvalidId");
            }

            [TestMethod]
            public void GetTermsApproval_ValidId()
            {
                var approval = this.repository.GetTermsApproval(ipcras[0].Recordkey);
                Assert.AreEqual(ipcras[0].Recordkey, approval.Id);
                Assert.AreEqual(ipcras[0].IpcraRegistration, approval.PaymentControlId);
                Assert.AreEqual(ipcras[0].IpcraTermsResponse, approval.TermsResponseId);
            }
        }

        #endregion

        #region Private data definition setup

        #region Registration Approval setup

        private void SetupRegistrationApprovals()
        {
            string[,] regApprovals = GetRegistrationApprovalsData();
            transManagerMock.Setup<ApproveRegistrationTermsResponse>(
                trans => trans.Execute<ApproveRegistrationTermsRequest, ApproveRegistrationTermsResponse>(It.IsAny<ApproveRegistrationTermsRequest>()))
                    .Returns<ApproveRegistrationTermsRequest>(request =>
                    {
                        if (request.StudentId == "0003315")
                        {
                            return validTermsResponse;
                        }
                        return errorTermsResponse;
                    });

            dataReaderMock.Setup<IpcRegApprovals>(
                reader => reader.ReadRecord<IpcRegApprovals>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return ipcras.FirstOrDefault(x => x.Recordkey == id); });
            var ipcraKeys = ipcras.Select(ipcra => ipcra.Recordkey).ToArray();
            foreach (var key in ipcraKeys)
            {
                var ipcraIds = ipcras.Where(i => i.Recordkey == key).Select(j => j.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.Select("IPC.REG.APPROVALS",
                    String.Format("IPCRA.REGISTRATION EQ '{0}' BY.DSND IPCRA.APPROVAL.DATE BY.DSND IPCRA.APPROVAL.TIME", key))).Returns(ipcraIds);
            }
        }

        private string[,] GetRegistrationApprovalsData()
        {
            string[,] regApprovalsData = {
                                            ////        ID     student  AR type  term      bill start    bill end     inv ID   adj rgar
                                            //{ "     1796", "0003500", "01", "2013/SP", "01/23/2013", "05/15/2013", "2313", "    " },
                                            //{ "     1797", "0003501", "01", "2013/SP", "01/23/2013", "05/15/2013", "2314", "    " },
                                            //{ "     1798", "0003502", "01", "2013/SP", "01/23/2013", "05/15/2013", "2315", "    " },
                                        };
            return regApprovalsData;
        }

        #endregion

        #region Registration Payment Control setup

        private void SetupRegistrationPaymentControls()
        {
            transManagerMock.Setup<UpdateRegistrationPaymentControlResponse>(
                trans => trans.Execute<UpdateRegistrationPaymentControlRequest, UpdateRegistrationPaymentControlResponse>(It.IsAny<UpdateRegistrationPaymentControlRequest>()))
                    .Returns<UpdateRegistrationPaymentControlRequest>(request =>
                    {
                        var response = new UpdateRegistrationPaymentControlResponse();
                        switch (request.PaymentStatus)
                        {
                            case "NEW":
                            case "ACCEPT":
                            case "COMPLETE":
                                response = validUpdateResponse;
                                break;
                            default:
                                if (request.PaymentControlId == "44")
                                {
                                    response = validUpdateResponse;
                                }
                                else
                                {
                                    response = errorUpdateResponse;
                                }
                                break;
                        }
                        return response;
                    });
        }

        #endregion

        #region Student Payment setup

        private void SetupStudentPayments()
        {
            transManagerMock.Setup<StartStudentPaymentResponse>(
                trans => trans.Execute<StartStudentPaymentRequest, StartStudentPaymentResponse>(It.IsAny<StartStudentPaymentRequest>()))
                    .Returns<StartStudentPaymentRequest>(request =>
                    {
                        if (request.InPayMethod == "ECHK")
                        {
                            return validPaymentResponse;
                        }
                        return errorPaymentResponse;
                    });
        }

        #endregion

        #region IPC Registration setup

        private void SetupIpcRegistrations()
        {
            dataReaderMock.Setup<IpcRegistration>(
                reader => reader.ReadRecord<IpcRegistration>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return ipcrs.FirstOrDefault(x => x.Recordkey == id); });

            foreach(var id in ipcrs.Select(i => i.IpcrStudent))
            {
                var criteria = string.Format("IPCR.STUDENT EQ '{0}'", id);
                var recordIds = ipcrs.Where(x => x.IpcrStudent == id).Select(i => i.Recordkey);
                dataReaderMock.Setup<string[]>(acc => acc.Select("IPC.REGISTRATION", criteria)).Returns(recordIds.ToArray());
            }

            dataReaderMock.Setup<Collection<IpcRegistration>>(
                accessor => accessor.BulkReadRecord<IpcRegistration>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((criteria, flag) =>
                    {
                        // Get the term ID from the selection criteria
                        string studentId = criteria.Split('\'')[1];
                        // Return the payment requirements collection for the desired person
                        return new Collection<IpcRegistration>(this.ipcrs.Where(ipcr => ipcr.IpcrStudent == studentId).ToList());
                    });
            transManagerMock.Setup<RefreshIpcRegistrationResponse>(
                trans => trans.Execute<RefreshIpcRegistrationRequest, RefreshIpcRegistrationResponse>(It.IsAny<RefreshIpcRegistrationRequest>()))
                    .Returns<RefreshIpcRegistrationRequest>(request =>
                    {
                        if (request.IpcRegistrationId == "BADID")
                        {
                            return new RefreshIpcRegistrationResponse()
                            {
                                ErrorMessages = new List<string>() { "Error occurred." }
                            };
                        }
                        return new RefreshIpcRegistrationResponse()
                            {
                            };
                    });

        }

        #endregion

        #region Pay Plan Approvals setup

        public void SetupPayPlanApprovals()
        {
            MockRecords("PAY.PLAN.APPROVALS", ppas);
        }

        #endregion
        
        #region Immediate Payment Control Setup

        private void SetupImmediatePaymentControl()
        {
            dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(
                reader => reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", It.IsAny<bool>()))
                    .Returns(new DataContracts.ImmediatePaymentControl()
                    {
                        IpcCancellationDoc = "IPCCANCEL",
                        IpcDeferralDoc = "IPCDEFER",
                        IpcEnabled = "Y",
                        IpcRegAcknowledgementDoc = "IPCACK",
                        IpcTermsAndConditionsDoc = "IPCTERMS",
                        Recordkey = "IMMEDIATE.PAYMENT.CONTROL"
                    });
        }

        #endregion

        #region Student Course Sec Setup

        public void SetupStudentCourseSecs()
        {
            var scsKeys = scss.Select(scs => scs.Recordkey).ToArray();
            foreach (var key in scsKeys)
            {
                var scsIds = scss.Where(i => i.Recordkey == key).Select(j => j.Recordkey).ToArray();
                dataReaderMock.Setup<Collection<StudentCourseSec>>(reader => reader.BulkReadRecord<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, flag) =>
                {
                    var scsCollection = new Collection<StudentCourseSec>();
                    foreach (var id in ids)
                    {
                        var inv = this.scss.Where(x => x.Recordkey == id).FirstOrDefault();
                        if (inv != null)
                        {
                            scsCollection.Add(inv);
                        }
                    }
                    return scsCollection;
                }
                );
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

        #region Rule Setup

        private void SetupRules()
        {
            string[,] ruleCheckData = GetRuleCheckData();
            int ruleCheckCount = ruleCheckData.Length / 5;
            Dictionary<string, List<RulesRulesCheck>> ruleCheckDict = new Dictionary<string, List<RulesRulesCheck>>();
            List<RulesRulesCheck> rlCheckList = new List<RulesRulesCheck>();
            for (int i = 0; i < ruleCheckCount; i++)
            {
                string ruleId = ruleCheckData[i, 0].Trim();
                string dataElement = ruleCheckData[i, 1].Trim();
                string op = ruleCheckData[i, 2].Trim();
                string value = ruleCheckData[i, 3].Trim();
                string connector = ruleCheckData[i, 4].Trim();

                if (ruleCheckDict.TryGetValue(ruleId, out rlCheckList))
                {
                    ruleCheckDict[ruleId].Add(new RulesRulesCheck(dataElement, op, value, null, null, null, null, null, null, null, null, connector, null));
                }
                else
                {
                    ruleCheckDict.Add(ruleId, new List<RulesRulesCheck>() { new RulesRulesCheck(dataElement, op, value, null, null, null, null, null, null, null, null, connector, null) });
                }
            }

            string[,] ruleData = GetRuleData();
            int ruleCount = ruleData.Length / 2;
            for (int i = 0; i < ruleCount; i++)
            {
                string ruleId = ruleData[i, 0].Trim();
                string primaryView = ruleData[i, 1].Trim();

                this.rules.Add(new Rules()
                {
                    Recordkey = ruleId,
                    RlPrimaryView = primaryView,
                    RulesCheckEntityAssociation = ruleCheckDict[ruleId],
                });
            }

            dataReaderMock.Setup<Collection<Rules>>(
                accessor => accessor.BulkReadRecord<Rules>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var items = new Collection<Rules>();
                        foreach (var id in ids)
                        {
                            var rule = this.rules.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (rules != null) items.Add(rule);
                        }
                        return items;
                    }
                );
        }

        private string[,] GetRuleData()
        {
            string[,] ruleData = {   // ID     Primary View   Data Elem    Op    Value  Connector
                                     { "WMK", "STUDENTS"},
                                     { "NRA", "PERSON"  },
                                 };
            return ruleData;
        }

        private string[,] GetRuleCheckData()
        {
            string[,] ruleCheckData = {  // ID      Data Elem    Op    Value  Connector
                                           {"WMK", "LAST.NAME", "EQ", "Wmk",  "WITH"},
                                          { "NRA", "FIRST.NAME","EQ", "Tina", "WITH"},
                                      };
            return ruleCheckData;
        }

        #endregion

        #region Account Holder Setup

        private void SetupAccountHolders()
        {
            string[,] accountHolderData = GetAccountHolderData();
            int accountHolderCount = accountHolderData.Length / 3;
            for (int i = 0; i < accountHolderCount; i++)
            {
                string id = accountHolderData[i, 0].Trim();
                string last = accountHolderData[i, 1].Trim();
                string first = accountHolderData[i, 2].Trim();

                this.accountHolders.Add(new Ellucian.Colleague.Data.Base.DataContracts.Person()
                {
                    Recordkey = id,
                    LastName = last,
                    FirstName = first,
                });
            }

            dataReaderMock.Setup<Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>>(
                accessor => accessor.BulkReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Person>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var persons = new Collection<Ellucian.Colleague.Data.Base.DataContracts.Person>();
                        foreach (var id in ids)
                        {
                            var person = this.accountHolders.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (accountHolders != null) persons.Add(person);
                        }
                        return persons;
                    }
                );
        }

        private string[,] GetAccountHolderData()
        {
            string[,] acctHolderData = {   // ID         Last       First     Gender
                                           { "0003315", "Wmk",     "Thomas", "M" },
                                           { "0000927", "Dashner", "Tina",   "F" },
                                       };
            return acctHolderData;
        }

        #endregion

        #region Registration Billing setup

        private void SetupRegArPostings()
        {
            string[,] regArPostingsTable = GetRegArPostingsTable();
            int regArPostingItemsCount = regArPostingsTable.Length / 2;
            Dictionary<string, List<string>> itemsTable = new Dictionary<string, List<string>>();
            for (int i = 0; i < regArPostingItemsCount; i++)
            {
                string id = regArPostingsTable[i, 0].Trim();
                string item = regArPostingsTable[i, 1].Trim();
                if (itemsTable.ContainsKey(id))
                {
                    itemsTable[id].Add(item);
                }
                else
                {
                    itemsTable.Add(id, String.IsNullOrEmpty(item) ? new List<string>() : new List<string>() { item });
                }
            }

            string[,] regArPostingsData = GetRegArPostingsData();
            int regArPostingsCount = regArPostingsData.Length / 8;
            for (int i = 0; i < regArPostingsCount; i++)
            {
                // Parse out the data
                string id = regArPostingsData[i, 0].Trim();
                string studentId = regArPostingsData[i, 1].Trim();
                string arType = regArPostingsData[i, 2].Trim();
                string termId = regArPostingsData[i, 3].Trim();
                string startDate = regArPostingsData[i, 4].Trim();
                string endDate = regArPostingsData[i, 5].Trim();
                string invoiceId = regArPostingsData[i, 6].Trim();
                string adjId = regArPostingsData[i, 7].Trim();
                List<string> items = (itemsTable.ContainsKey(id)) ? itemsTable[id] : null;

                // Add a new "record" to the list
                this.regArPostings.Add(new RegArPostings()
                {
                    Recordkey = id,
                    RgarStudent = studentId,
                    RgarTerm = termId,
                    RgarArType = arType,
                    RgarBillingStartDate = DateTime.Parse(startDate),
                    RgarBillingEndDate = DateTime.Parse(endDate),
                    RgarInvoice = invoiceId,
                    RgarAdjByRegArPosting = String.IsNullOrEmpty(adjId) ? null : adjId,
                    RgarRegArPostingItems = (items == null) ? new List<string>() : new List<string>(items)
                });
            }

            dataReaderMock.Setup<RegArPostings>(
                accessor => accessor.ReadRecord<RegArPostings>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) =>
                    {
                        // Return the reg billing object for the desired id
                        return this.regArPostings.Where(x => x.Recordkey == id).FirstOrDefault();
                    }
                );
        }

        private void SetupRegArPostingItems()
        {
            string[,] regArPostingItemsData = GetRegArPostingItemsData();
            int regArPostingItemsCount = regArPostingItemsData.Length / 2;
            for (int i = 0; i < regArPostingItemsCount; i++)
            {
                string id = regArPostingItemsData[i, 0].Trim();
                string scs = regArPostingItemsData[i, 1].Trim();

                this.regArPostingItems.Add(new RegArPostingItems()
                {
                    Recordkey = id,
                    RgariStudentCourseSec = scs
                });
            }

            dataReaderMock.Setup<Collection<RegArPostingItems>>(
                accessor => accessor.BulkReadRecord<RegArPostingItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var items = new Collection<RegArPostingItems>();
                        foreach (var id in ids)
                        {
                            var rgari = this.regArPostingItems.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (rgari != null) items.Add(rgari);
                        }
                        return items;
                    }
                );
        }

        private void SetupStudentCourseSec()
        {
            string[,] studentCourseSecData = GetStudentCourseSecData();
            int studentCourseSecCount = studentCourseSecData.Length / 4;
            for (int i = 0; i < studentCourseSecCount; i++)
            {
                string id = studentCourseSecData[i, 0].Trim();
                string stc = studentCourseSecData[i, 1].Trim();
                string sec = studentCourseSecData[i, 2].Trim();
                string stu = studentCourseSecData[i, 3].Trim();

                this.studentCourseSec.Add(new StudentCourseSec()
                {
                    Recordkey = id,
                    ScsStudentAcadCred = stc,
                    ScsCourseSection = sec,
                    ScsStudent = stu
                });
            }

            dataReaderMock.Setup<Collection<StudentCourseSec>>(
                accessor => accessor.BulkReadRecord<StudentCourseSec>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var items = new Collection<StudentCourseSec>();
                        foreach (var id in ids)
                        {
                            var scs = this.studentCourseSec.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (scs != null) items.Add(scs);
                        }
                        return items;
                    }
                );
            dataReaderMock.Setup<Collection<StudentCourseSec>>(
                accessor => accessor.BulkReadRecord<StudentCourseSec>("STUDENT.COURSE.SEC", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string, string[], bool>((file, ids, flag) =>
                    {
                        var items = new Collection<StudentCourseSec>();
                        foreach (var id in ids)
                        {
                            var scs = this.studentCourseSec.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (scs != null) items.Add(scs);
                        }
                        return items;
                    }
                );
            dataReaderMock.Setup<Collection<StudentCourseSecCc>>(
                accessor => accessor.BulkReadRecord<StudentCourseSecCc>("STUDENT.COURSE.SEC.CC", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string, string[], bool>((file, ids, flag) =>
                    {
                        var items = new Collection<StudentCourseSecCc>();
                        foreach (var id in ids)
                        {
                            items.Add(new StudentCourseSecCc()
                            {
                                Recordkey = id
                            });
                        }
                        return items;
                    }
                );
        }

        private void SetupStudentAcadCred()
        {
            string[,] stcStatusData = GetStcStatusData();
            int stcStatusCount = stcStatusData.Length / 4;
            var stcStatuses = new Dictionary<string, List<StudentAcadCredStcStatuses>>();
            for (int i = 0; i < stcStatusCount; i++)
            {
                string id = stcStatusData[i, 0].Trim();
                string status = stcStatusData[i, 1].Trim();
                DateTime date = DateTime.Parse(stcStatusData[i, 2].Trim());
                DateTime time = DateTime.Parse(stcStatusData[i, 3].Trim());

                if (stcStatuses.ContainsKey(id))
                {
                    stcStatuses[id].Add(new StudentAcadCredStcStatuses(time, status, date, ""));
                }
                else
                {
                    stcStatuses.Add(id, new List<StudentAcadCredStcStatuses>() { new StudentAcadCredStcStatuses(time, status, date, "") });
                }
            }

            string[,] studentAcadCredData = GetStudentAcadCredData();
            int studentAcadCredCount = studentAcadCredData.Length / 25;
            for (int i = 0; i < studentAcadCredCount; i++)
            {
                string id = studentAcadCredData[i, 0].Trim();
                string title = studentAcadCredData[i, 1].Trim();
                string dept = studentAcadCredData[i, 2].Trim();
                string acadLevel = studentAcadCredData[i, 3].Trim();
                string subject = studentAcadCredData[i, 4].Trim();
                string courseNo = studentAcadCredData[i, 5].Trim();
                string cred = studentAcadCredData[i, 6].Trim();
                string cmplCred = studentAcadCredData[i, 7].Trim();
                string gpaCred = studentAcadCredData[i, 8].Trim();
                string credType = studentAcadCredData[i, 9].Trim();
                string gradeScheme = studentAcadCredData[i, 10].Trim();
                string verifiedGrade = studentAcadCredData[i, 11].Trim();
                string verifiedDate = studentAcadCredData[i, 12].Trim();
                string courseName = studentAcadCredData[i, 13].Trim();
                string term = studentAcadCredData[i, 14].Trim();
                string scs = studentAcadCredData[i, 15].Trim();
                string startDate = studentAcadCredData[i, 16].Trim();
                string endDate = studentAcadCredData[i, 17].Trim();
                string altCmplCred = studentAcadCredData[i, 18].Trim();
                string altGpaCred = studentAcadCredData[i, 19].Trim();
                string gradePts = studentAcadCredData[i, 20].Trim();
                string altGradePts = studentAcadCredData[i, 21].Trim();
                string replFlag = studentAcadCredData[i, 22].Trim();
                string section = studentAcadCredData[i, 23].Trim();
                string verifiedTime = studentAcadCredData[i, 24].Trim();

                var statuses = stcStatuses[id];

                var stc = new StudentAcadCred();
                stc.Recordkey = id;
                stc.StcTitle = title;
                stc.StcDepts = new List<string>() { dept };
                stc.StcAcadLevel = acadLevel;
                stc.StcSubject = subject;
                stc.StcCourse = courseNo;
                if (String.IsNullOrEmpty(cred)) stc.StcCred = null; else stc.StcCred = Decimal.Parse(cred);
                if (String.IsNullOrEmpty(cmplCred)) stc.StcCmplCred = null; else stc.StcCmplCred = Decimal.Parse(cmplCred);
                if (String.IsNullOrEmpty(gpaCred)) stc.StcGpaCred = null; else stc.StcGpaCred = Decimal.Parse(gpaCred);
                stc.StcCredType = credType;
                stc.StcGradeScheme = gradeScheme;
                stc.StcVerifiedGrade = verifiedGrade;
                if (String.IsNullOrEmpty(verifiedDate)) stc.StcVerifiedGradeDate = null; else stc.StcVerifiedGradeDate = DateTime.Parse(verifiedDate);
                stc.StcCourseName = courseName;
                stc.StcTerm = term;
                stc.StcStudentCourseSec = scs;
                stc.StcStartDate = DateTime.Parse(startDate);
                stc.StcEndDate = DateTime.Parse(endDate);
                if (String.IsNullOrEmpty(altCmplCred)) stc.StcAltcumContribCmplCred = null; else stc.StcAltcumContribCmplCred = Decimal.Parse(altCmplCred);
                if (String.IsNullOrEmpty(altGpaCred)) stc.StcAltcumContribGpaCred = null; else stc.StcAltcumContribGpaCred = Decimal.Parse(altGpaCred);
                if (String.IsNullOrEmpty(gradePts)) stc.StcGradePts = null; else stc.StcGradePts = Decimal.Parse(gradePts);
                if (String.IsNullOrEmpty(altGradePts)) stc.StcAltcumContribGradePts = null; else stc.StcAltcumContribGradePts = Decimal.Parse(altGradePts);
                stc.StcAllowReplFlag = replFlag;
                stc.StcSectionNo = section;
                stc.StcStatus = statuses.Select(x => x.StcStatusAssocMember).ToList();
                stc.StcStatusDate = statuses.Select(x => x.StcStatusDateAssocMember).ToList();
                stc.StcStatusTime = statuses.Select(x => x.StcStatusTimeAssocMember).ToList();
                stc.StcStatusReason = statuses.Select(x => x.StcStatusReasonAssocMember).ToList();
                this.studentAcadCred.Add(stc);

                var stccc = new StudentAcadCredCc();
                stccc.Recordkey = id;
                if (String.IsNullOrEmpty(verifiedTime)) stccc.StcccVerifiedGradeTime = null; else stccc.StcccVerifiedGradeTime = DateTime.Parse(verifiedTime);
                this.studentAcadCredCc.Add(stccc);
            }

            dataReaderMock.Setup<Collection<StudentAcadCred>>(
                accessor => accessor.BulkReadRecord<StudentAcadCred>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var items = new Collection<StudentAcadCred>();
                        foreach (var id in ids)
                        {
                            var stc = this.studentAcadCred.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (stc != null) items.Add(stc);
                        }
                        return items;
                    }
                );
            dataReaderMock.Setup<Collection<StudentAcadCred>>(
                accessor => accessor.BulkReadRecord<StudentAcadCred>("STUDENT.ACAD.CRED", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string, string[], bool>((file, ids, flag) =>
                    {
                        var items = new Collection<StudentAcadCred>();
                        foreach (var id in ids)
                        {
                            var stc = this.studentAcadCred.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (stc != null) items.Add(stc);
                        }
                        return items;
                    }
                );
            dataReaderMock.Setup<Collection<StudentAcadCredCc>>(
                accessor => accessor.BulkReadRecord<StudentAcadCredCc>("STUDENT.ACAD.CRED.CC", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string, string[], bool>((file, ids, flag) =>
                    {
                        var items = new Collection<StudentAcadCredCc>();
                        foreach (var id in ids)
                        {
                            var stc = this.studentAcadCredCc.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (stc != null) items.Add(stc);
                        }
                        return items;
                    }
                );
        }

        private void SetupCourses()
        {
            string[,] coursesData = GetCoursesData();
            int coursesCount = coursesData.Length / 8;
            for (int i = 0; i < coursesCount; i++)
            {
                string id = coursesData[i, 0].Trim();
                string title = coursesData[i, 1].Trim();
                string subject = coursesData[i, 2].Trim();
                string crsNo = coursesData[i, 3].Trim();
                string acadLevel = coursesData[i, 4].Trim();
                decimal credit = Decimal.Parse(coursesData[i, 5].Trim());
                string dept = coursesData[i, 6].Trim();
                string level = coursesData[i, 7].Trim();

                this.courses.Add(new Courses()
                {
                    Recordkey = id,
                    CrsShortTitle = title,
                    CrsSubject = subject,
                    CrsNo = crsNo,
                    CrsAcadLevel = acadLevel,
                    CrsMinCred = credit,
                    CrsDepts = new List<string>() { dept },
                    CrsLevels = new List<string>() { level },
                    CrsStatus = new List<string>() { "A" },
                    CrsEquateCodes = new List<string>()
                });
            }

            dataReaderMock.Setup<string[]>(
                accessor => accessor.Select("COURSES", "")).Returns(this.courses.Select(x => x.Recordkey).ToArray());
            dataReaderMock.Setup<Collection<Courses>>(
                accessor => accessor.BulkReadRecord<Courses>("COURSES", "", It.IsAny<bool>())).Returns(this.courses);
            dataReaderMock.Setup<Collection<Courses>>(
                accessor => accessor.BulkReadRecord<Courses>("COURSES", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string, string[], bool>((file, ids, flag) =>
                    {
                        var items = new Collection<Courses>();
                        foreach (var id in ids)
                        {
                            var crs = this.courses.Where(x => x.Recordkey == id).FirstOrDefault();
                            if (crs != null) items.Add(crs);
                        }
                        return items;
                    });
        }

        private void SetupCoursesCodes()
        {
            var sessionCycles = new Collection<SessionCycles>();
            sessionCycles.Add(new SessionCycles()
            {
                Recordkey = "F",
                ScDesc = "Fall Term Only"
            });
            sessionCycles.Add(new SessionCycles()
            {
                Recordkey = "EI",
                ScDesc = "Even Year Intercession"
            });
            dataReaderMock.Setup<ICollection<SessionCycles>>(acc => acc.BulkReadRecord<SessionCycles>("SESSION.CYCLES", "", It.IsAny<bool>())).Returns(sessionCycles);

            var yearlyCycles = new Collection<YearlyCycles>();
            yearlyCycles.Add(new YearlyCycles()
            {
                Recordkey = "A",
                YcDesc = "Every Other Year"
            });
            yearlyCycles.Add(new YearlyCycles()
            {
                Recordkey = "B",
                YcDesc = "Every Third Year"
            });
            dataReaderMock.Setup<ICollection<YearlyCycles>>(acc => acc.BulkReadRecord<YearlyCycles>("YEARLY.CYCLES", "", It.IsAny<bool>())).Returns(yearlyCycles);

            // CourseStatuses mock
            ApplValcodes courseStatusesResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "1" },
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "T", ValActionCode1AssocMember = "2"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "P", ValActionCode1AssocMember = ""}}
            };
            dataReaderMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("ST.VALCODES", "COURSE.STATUSES", It.IsAny<bool>())).Returns(courseStatusesResponse);

            // course types
            ApplValcodes courseTypesValcode = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "CORE", ValActionCode1AssocMember = "" },
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "PSE", ValActionCode1AssocMember = "p"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "REM", ValActionCode1AssocMember = ""}}
            };
            dataReaderMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).Returns(courseTypesValcode);

        }

        private async Task SetupAcademicCreditCodes()
        {
            // Mock up response for grade repository
            Collection<Grades> graderesp = await BuildValidGradeResponse();
            dataReaderMock.Setup<Collection<Grades>>(grds => grds.BulkReadRecord<Grades>("GRADES", "", It.IsAny<bool>())).Returns(graderesp);

            // Credit Types transaction mock
            Collection<CredTypes> credTypeResponse = new Collection<CredTypes>(){ new CredTypes() { Recordkey = "IN", CrtpCategory = "I"},
                                                                                new CredTypes() { Recordkey = "TR", CrtpCategory = "T"},
                                                                                new CredTypes() { Recordkey = "CE", CrtpCategory = "C"}};
            dataReaderMock.Setup<Collection<CredTypes>>(acc => acc.BulkReadRecord<CredTypes>("CRED.TYPES", "", It.IsAny<bool>())).Returns(credTypeResponse);

            // StudentAcadCredStatus mock
            ApplValcodes statusCodeResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "N", ValActionCode1AssocMember = "1"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "A", ValActionCode1AssocMember = "2"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "D", ValActionCode1AssocMember = "3"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "W", ValActionCode1AssocMember = "4"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "5"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "C", ValActionCode1AssocMember = "6"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "TR", ValActionCode1AssocMember = "7"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "NC", ValActionCode1AssocMember = "7"},
                                                                    new ApplValcodesVals() { ValInternalCodeAssocMember = "PR", ValActionCode1AssocMember = "8"},}
            };
            dataReaderMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", It.IsAny<bool>())).Returns(statusCodeResponse);
        }

        private string[,] GetRegArPostingsData()
        {
            string[,] regArPostingsData = {
                                            //        id     student  AR type  term      bill start    bill end     inv ID   adj rgar
                                            { "     1796", "0003500", "01", "2013/SP", "01/23/2013", "05/15/2013", "2313", "    " },
                                            { "     1797", "0003501", "01", "2013/SP", "01/23/2013", "05/15/2013", "2314", "    " },
                                            { "     1798", "0003502", "01", "2013/SP", "01/23/2013", "05/15/2013", "2315", "    " },
                                            { "     1799", "0003503", "01", "2013/SP", "01/23/2013", "05/15/2013", "2316", "    " },
                                            { "     2258", "0003584", "01", "2013/SP", "01/23/2013", "05/15/2013", "5197", "    " },
                                            { "     2649", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7048", "2650" },
                                            { "     2650", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7052", "2652" },
                                            { "     2651", "0003583", "01", "2013/SP", "01/23/2013", "05/15/2013", "7054", "    " },
                                            { "     2652", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7055", "2653" },
                                            { "     2653", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7057", "2666" },
                                            { "     2666", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7133", "2667" },
                                            { "     2667", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7135", "2668" },
                                            { "     2668", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7137", "2669" },
                                            { "     2669", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7139", "2670" },
                                            { "     2670", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7141", "2671" },
                                            { "     2671", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7143", "2672" },
                                            { "     2672", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7145", "2673" },
                                            { "     2673", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7147", "2674" },
                                            { "     2674", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7149", "2675" },
                                            { "     2675", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7151", "2676" },
                                            { "     2676", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7153", "2677" },
                                            { "     2677", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7155", "2678" },
                                            { "     2678", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7157", "2679" },
                                            { "     2679", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7159", "2680" },
                                            { "     2680", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7161", "2681" },
                                            { "     2681", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7163", "2706" },
                                            { "     2692", "0003584", "01", "2013/S1", "05/23/2013", "07/03/2013", "7375", "    " },
                                            { "     2706", "0003583", "01", "2013/S1", "05/23/2013", "07/03/2013", "7722", "    " },
                                            { "RGARINULL", "0003500", "01", "2013/S2", "07/04/2013", "08/15/2013", "xyz ", "    " },
                                            { "RGARIFAIL", "0003500", "01", "2013/S2", "07/04/2013", "08/15/2013", "xyz ", "    " },
                                            { "SCSFAIL  ", "0003500", "01", "2013/S2", "07/04/2013", "08/15/2013", "xyz ", "    " },
                                            { "STCFAIL  ", "0003500", "01", "2013/S2", "07/04/2013", "08/15/2013", "xyz ", "    " }
                                        };
            return regArPostingsData;
        }

        private string[,] GetRegArPostingsTable()
        {
            string[,] regArPostingItemsData = {
                                                //    RGAR      RGARI
                                                { "   1796", "   5746" },
                                                { "   1796", "   5747" },
                                                { "   1796", "   5748" },
                                                { "   1796", "   5749" },
                                                { "   1797", "   5750" },
                                                { "   1797", "   5751" },
                                                { "   1797", "   5752" },
                                                { "   1798", "   5753" },
                                                { "   1798", "   5754" },
                                                { "   1799", "   5755" },
                                                { "   2258", "   7489" },
                                                { "   2649", "   8381" },
                                                { "   2649", "   8382" },
                                                { "   2650", "   8383" },
                                                { "   2650", "   8384" },
                                                { "   2651", "   8385" },
                                                { "   2652", "   8386" },
                                                { "   2652", "   8387" },
                                                { "   2653", "   8388" },
                                                { "   2653", "   8389" },
                                                { "   2666", "   8413" },
                                                { "   2666", "   8414" },
                                                { "   2667", "   8415" },
                                                { "   2667", "   8416" },
                                                { "   2668", "   8417" },
                                                { "   2668", "   8418" },
                                                { "   2669", "   8419" },
                                                { "   2669", "   8420" },
                                                { "   2670", "   8421" },
                                                { "   2670", "   8422" },
                                                { "   2671", "   8423" },
                                                { "   2671", "   8424" },
                                                { "   2672", "   8425" },
                                                { "   2672", "   8426" },
                                                { "   2673", "   8427" },
                                                { "   2673", "   8428" },
                                                { "   2674", "   8429" },
                                                { "   2674", "   8430" },
                                                { "   2675", "   8431" },
                                                { "   2675", "   8432" },
                                                { "   2676", "   8433" },
                                                { "   2676", "   8434" },
                                                { "   2677", "   8435" },
                                                { "   2677", "   8436" },
                                                { "   2678", "   8437" },
                                                { "   2678", "   8438" },
                                                { "   2679", "   8439" },
                                                { "   2679", "   8440" },
                                                { "   2680", "   8441" },
                                                { "   2680", "   8442" },
                                                { "   2681", "   8443" },
                                                { "   2681", "   8444" },
                                                { "   2692", "   8479" },
                                                { "   2706", "   8501" },
                                                { "   2706", "   8502" },
                                                { "SCSFAIL", "SCSFAIL" },
                                                { "STCFAIL", "STCFAIL" },
                                                { "RGARINULL", "" },
                                                { "RGARIFAIL", "RGARIFAIL" }
                                            };
            return regArPostingItemsData;
        }

        private string[,] GetRegArPostingItemsData()
        {
            string[,] regArPostingItemsData = {
                                                // RGARI    SCS
                                                { "   5746", "   7933" },
                                                { "   5747", "   7934" },
                                                { "   5748", "   7935" },
                                                { "   5749", "   7936" },
                                                { "   5750", "   7937" },
                                                { "   5751", "   7938" },
                                                { "   5752", "   7939" },
                                                { "   5753", "   7940" },
                                                { "   5754", "   7941" },
                                                { "   5755", "   7942" },
                                                { "   7489", "  10868" },
                                                { "   8381", "  11351" },
                                                { "   8382", "  11352" },
                                                { "   8383", "  11352" },
                                                { "   8384", "  11351" },
                                                { "   8385", "  11356" },
                                                { "   8386", "  11351" },
                                                { "   8387", "  11352" },
                                                { "   8388", "  11352" },
                                                { "   8389", "  11351" },
                                                { "   8413", "  11351" },
                                                { "   8414", "  11352" },
                                                { "   8415", "  11351" },
                                                { "   8416", "  11352" },
                                                { "   8417", "  11351" },
                                                { "   8418", "  11352" },
                                                { "   8419", "  11351" },
                                                { "   8420", "  11352" },
                                                { "   8421", "  11351" },
                                                { "   8422", "  11352" },
                                                { "   8423", "  11351" },
                                                { "   8424", "  11352" },
                                                { "   8425", "  11351" },
                                                { "   8426", "  11352" },
                                                { "   8427", "  11351" },
                                                { "   8428", "  11352" },
                                                { "   8429", "  11351" },
                                                { "   8430", "  11352" },
                                                { "   8431", "  11351" },
                                                { "   8432", "  11352" },
                                                { "   8433", "  11351" },
                                                { "   8434", "  11352" },
                                                { "   8435", "  11351" },
                                                { "   8436", "  11352" },
                                                { "   8437", "  11351" },
                                                { "   8438", "  11352" },
                                                { "   8439", "  11351" },
                                                { "   8440", "  11352" },
                                                { "   8441", "  11351" },
                                                { "   8442", "  11352" },
                                                { "   8443", "  11351" },
                                                { "   8444", "  11352" },
                                                { "   8479", "  11441" },
                                                { "   8501", "  11351" },
                                                { "   8502", "  11352" },
                                                { "SCSFAIL", "SCSFAIL" },
                                                { "STCFAIL", "STCFAIL" }
                                            };
            return regArPostingItemsData;
        }

        private string[,] GetStudentCourseSecData()
        {
            string[,] studentCourseSecData = {  //   SCS        STC      SEC      STUDENT
                                            { "  10868", "  11116", "14394", "0003584" },
                                            { "  11351", "  11639", "14837", "0003583" },
                                            { "  11352", "  11640", "14840", "0003583" },
                                            { "  11356", "  11644", "14175", "0003583" },
                                            { "  11441", "  11736", "14837", "0003584" },
                                            { "   7933", "   8146", "14240", "0003500" },
                                            { "   7934", "   8147", "13992", "0003500" },
                                            { "   7935", "   8148", "14434", "0003500" },
                                            { "   7936", "   8149", "14438", "0003500" },
                                            { "   7937", "   8150", "14240", "0003501" },
                                            { "   7938", "   8151", "13992", "0003501" },
                                            { "   7939", "   8152", "14434", "0003501" },
                                            { "   7940", "   8153", "14240", "0003502" },
                                            { "   7941", "   8154", "14434", "0003502" },
                                            { "   7942", "   8155", "14240", "0003503" },
                                            { "STCFAIL", "STCFAIL", "14240", "0003500" }
                                            };
            return studentCourseSecData;
        }

        private string[,] GetStudentAcadCredData()
        {
            string[,] studentAcadCredData = {
                                            //                                                   acad           crs            cmpl    gpa    cred  grd   ver   verified    course                            start        end      altcum  altcum  grade   altcum allow sect   verified
                                            // id        title                            dept   lvl    subj     id    cred    cred    cred   type  schm  grd     date       name         term       scs       date        date     cmpcrd  gpacrd   pts    grdpts  repl num      time
                                            { "11116", "Advanced Mech Engineering     ", "ENGR", "GR", "ENGR", "  8", "4.00", "    ", "    ", "IN", "GR", " ", "        ", "ENGR-500 ", "2013/SP", "10868", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "A ", "        " },
                                            { "11639", "Molecular Biology             ", "BIOL", "UG", "BIOL", "110", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "BIOL-100 ", "2013/S1", "11351", "05/23/13", "07/03/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { "11640", "Molecular Biology-Lab         ", "BIOL", "UG", "BIOL", "367", "1.00", "    ", "    ", "IN", "UG", " ", "        ", "BIOL-100L", "2013/S1", "11352", "05/23/13", "07/03/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { "11644", "Molecular Biology-Lab         ", "BIOL", "UG", "BIOL", "367", "1.00", "0.00", "1.00", "IN", "UG", "W", "05/04/13", "BIOL-100L", "2013/SP", "11356", "01/23/13", "05/15/13", "0.00", "1.00", "0.00", "0.00", "Y", "01", "13:35:43" },
                                            { "11736", "Molecular Biology             ", "BIOL", "UG", "BIOL", "110", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "BIOL-100 ", "2013/S1", "11441", "05/23/13", "07/03/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8146", "Intro. to Comp. Science Theory", "COMP", "UG", "COMP", " 26", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "COMP-200 ", "2013/SP", " 7933", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8147", "Accounting for Non-Profits    ", "BUSN", "UG", "ACCT", "192", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "ACCT-104 ", "2013/SP", " 7934", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8148", "Renaissance and Revolutions   ", "HIST", "UG", "HIST", "191", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "HIST-110 ", "2013/SP", " 7935", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8149", "Imperialism and Revolution    ", "HIST", "UG", "HIST", "236", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "HIST-120 ", "2013/SP", " 7936", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "02", "        " },
                                            { " 8150", "Intro. to Comp. Science Theory", "COMP", "UG", "COMP", " 26", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "COMP-200 ", "2013/SP", " 7937", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8151", "Accounting for Non-Profits    ", "BUSN", "UG", "ACCT", "192", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "ACCT-104 ", "2013/SP", " 7938", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8152", "Renaissance and Revolutions   ", "HIST", "UG", "HIST", "191", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "HIST-110 ", "2013/SP", " 7939", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8153", "Intro. to Comp. Science Theory", "COMP", "UG", "COMP", " 26", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "COMP-200 ", "2013/SP", " 7940", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8154", "Renaissance and Revolutions   ", "HIST", "UG", "HIST", "191", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "HIST-110 ", "2013/SP", " 7941", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                            { " 8155", "Intro. to Comp. Science Theory", "COMP", "UG", "COMP", " 26", "3.00", "    ", "    ", "IN", "UG", " ", "        ", "COMP-200 ", "2013/SP", " 7942", "01/23/13", "05/15/13", "    ", "    ", "    ", "    ", "Y", "01", "        " },
                                        };
            return studentAcadCredData;
        }

        private string[,] GetStcStatusData()
        {
            string[,] stcStatusData = {
                                    //  stc    sts     date          time
                                    { "11116", "D", "05/30/2013", "10:30:14" },
                                    { "11116", "A", "02/11/2013", "16:56:25" },
                                    { "11639", "D", "06/11/2013", "18:40:21" },
                                    { "11639", "N", "05/04/2013", "13:36:31" },
                                    { "11639", "D", "05/04/2013", "13:25:49" },
                                    { "11639", "N", "05/02/2013", "17:28:11" },
                                    { "11640", "D", "05/14/2013", "22:35:50" },
                                    { "11640", "N", "05/14/2013", "16:08:11" },
                                    { "11640", "D", "05/14/2013", "15:10:10" },
                                    { "11640", "N", "05/14/2013", "15:04:30" },
                                    { "11640", "D", "05/14/2013", "14:41:28" },
                                    { "11640", "N", "05/14/2013", "14:36:42" },
                                    { "11640", "D", "05/14/2013", "14:36:23" },
                                    { "11640", "N", "05/14/2013", "14:30:31" },
                                    { "11640", "D", "05/14/2013", "14:30:01" },
                                    { "11640", "N", "05/13/2013", "20:04:41" },
                                    { "11640", "D", "05/13/2013", "20:00:35" },
                                    { "11640", "N", "05/13/2013", "19:55:26" },
                                    { "11640", "D", "05/13/2013", "19:54:09" },
                                    { "11640", "N", "05/13/2013", "19:44:56" },
                                    { "11640", "D", "05/13/2013", "19:44:18" },
                                    { "11640", "N", "05/13/2013", "19:38:54" },
                                    { "11640", "D", "05/04/2013", "13:37:18" },
                                    { "11640", "N", "05/04/2013", "13:36:31" },
                                    { "11640", "D", "05/04/2013", "13:25:49" },
                                    { "11640", "N", "05/02/2013", "17:28:12" },
                                    { "11644", "N", "01/01/2013", "13:26:32" },
                                    { "11736", "N", "05/30/2013", "10:29:54" },
                                    { " 8146", "N", "02/16/2012", "10:30:56" },
                                    { " 8147", "N", "02/16/2012", "10:31:33" },
                                    { " 8148", "N", "02/16/2012", "10:32:15" },
                                    { " 8149", "N", "02/16/2012", "10:32:43" },
                                    { " 8150", "N", "02/16/2012", "10:33:44" },
                                    { " 8151", "N", "02/16/2012", "10:33:56" },
                                    { " 8152", "N", "02/16/2012", "10:34:04" },
                                    { " 8153", "N", "02/16/2012", "10:35:41" },
                                    { " 8154", "N", "02/16/2012", "10:36:33" },
                                    { " 8155", "N", "02/16/2012", "10:37:16" },
                                    };
            return stcStatusData;
        }

        private string[,] GetCoursesData()
        {
            string[,] coursesData = {
                                    //  id      short title                     subj    num     lvl   cred    dept   crslv
                                    { "110", "Molecular Biology             ", "BIOL", "100 ", "UG", "3.00", "BIOL", "100" },
                                    { "191", "Renaissance and Revolutions   ", "HIST", "110 ", "UG", "3.00", "HIST", "100" },
                                    { "192", "Accounting for Non-Profits    ", "ACCT", "104 ", "UG", "3.00", "BUSN", "100" },
                                    { "236", "Imperialism and Revolution    ", "HIST", "120 ", "UG", "3.00", "HIST", "100" },
                                    { " 26", "Intro. to Comp. Science Theory", "COMP", "200 ", "UG", "3.00", "COMP", "100" },
                                    { "367", "Molecular Biology-Lab         ", "BIOL", "100L", "UG", "1.00", "BIOL", "100" },
                                    { "  8", "Advanced Mech Engineering     ", "ENGR", "500 ", "GR", "4.00", "ENGR", "100" },
                                };
            return coursesData;
        }

        private async Task<Collection<Grades>> BuildValidGradeResponse()
        {
            TestGradeRepository tgr = new TestGradeRepository();
            Collection<Grades> grades = new Collection<Grades>();
            foreach (var grade in (await tgr.GetAsync()))
            {
                Grades g = new Grades();
                g.Recordkey = grade.Id;
                g.GrdGrade = grade.LetterGrade;
                g.GrdGradeScheme = grade.GradeSchemeCode;
                g.GrdLegend = grade.Description;
                g.GrdValue = grade.GradeValue;

                grades.Add(g);
            }
            return grades;
        }

        #endregion


        #endregion


        }
    }