//Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class CorrespondenceRequestsRepositoryTests : BaseRepositorySetup
    {
        private string personId;

        private IEnumerable<CorrespondenceRequest> actualCorrespondenceRequests;

        private CorrespondenceRequestsRepository actualCorrespondenceRequestsRepository;

        private IEnumerable<CorrespondenceRequest> expectedCorrespondenceRequests;

        private TestCorrespondenceRequestsRepository expectedCorrespondenceRequestsRepository;

        private NotifyCommReqAttachmentRequest updateRequest;

        private CorrespondenceRequest expectedNotifyCorrespondenceRequest;

        [TestInitialize]
        public async void BaseInitialize()
        {
            MockInitialize();

            expectedCorrespondenceRequestsRepository = new TestCorrespondenceRequestsRepository();
            personId = expectedCorrespondenceRequestsRepository.personId;
            expectedCorrespondenceRequests = await expectedCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);
            expectedNotifyCorrespondenceRequest = expectedCorrespondenceRequests.Where(cc => cc.PersonId == personId && cc.Code == "FA14SAP" && cc.AssignDate == DateTime.Today && cc.Instance == "SAP Document").FirstOrDefault();
            
            actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();

            actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);
        }



        [TestCleanup]
        public void BaseCleanup()
        {
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            actualCorrespondenceRequests = null;
            actualCorrespondenceRequestsRepository = null;
            expectedCorrespondenceRequests = null;
        }

        private CorrespondenceRequestsRepository BuildCorrespondenceRequestsRepository()
        {
            dataReaderMock.Setup(m => m.ReadRecordAsync<Mailing>(personId, true))
                .Returns<string, bool>(
                    (a, b) =>
                    {
                        if (expectedCorrespondenceRequestsRepository.MailingData == null) return null;
                        return Task.FromResult(new Mailing()
                        {
                            Recordkey = personId,
                            MailingCurrentCrcCode = expectedCorrespondenceRequestsRepository.MailingData.CurrentCorrespondanceRequestCodes,
                            ChCorrEntityAssociation =
                                (expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData == null) ? null :
                                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData
                                    .Where(chCorr => chCorr != null)
                                    .Select(chCorr =>
                                        new MailingChCorr()
                                        {
                                            MailingCorrReceivedAssocMember = chCorr.Code,
                                            MailingCorrRecvdActDtAssocMember = chCorr.DueDate,
                                            MailingCorrRecvdStatusAssocMember = chCorr.StatusCode,
                                            MailingCorrReceivedDateAssocMember = chCorr.StatusDate,
                                            MailingCorrRecvdInstanceAssocMember = chCorr.Instance,
                                            MailingCorrRecvdAsgnDtAssocMember = chCorr.AssignDate
                                        }
                                    ).ToList()
                        });
                    }
                );

            dataReaderMock.Setup(r => r.BulkReadRecordAsync<Coreq>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>(
                    (recordIds, b) =>
                    {
                        if (expectedCorrespondenceRequestsRepository.CorrespondanceTrackData == null) return null;
                        return Task.FromResult(new Collection<Coreq>(
                            expectedCorrespondenceRequestsRepository.CorrespondanceTrackData
                                .Where(track => track != null && recordIds.Contains(string.Format("{0}*{1}", personId, track.Code)) && track.CorrespondanceRequests != null)
                                .Select(track =>
                                    new Coreq()
                                    {
                                        Recordkey = string.Format("{0}*{1}", personId, track.Code),
                                        CoreqRequestsEntityAssociation = track.CorrespondanceRequests
                                            .Where(coreq => coreq != null)
                                            .Select(coreq =>
                                                new CoreqCoreqRequests()
                                                {
                                                    CoreqCcCodeAssocMember = coreq.Code,
                                                    CoreqCcExpActDtAssocMember = coreq.DueDate,
                                                    CoreqCcDateAssocMember = coreq.StatusDate,
                                                    CoreqCcInstanceAssocMember = coreq.Instance,
                                                    CoreqCcStatusAssocMember = coreq.StatusCode,
                                                    CoreqCcAssignDtAssocMember = coreq.AssignDate
                                                }
                                            ).ToList()
                                    }
                                ).ToList()
                            ));
                    }
                );

            dataReaderMock.Setup(r => r.ReadRecord<ApplValcodes>("CORE.VALCODES", "CORR.STATUSES", true))
                .Returns<string, string, bool>(
                    (a, b, c) =>
                    {
                        if (expectedCorrespondenceRequestsRepository.statusValcodeValues == null) return null;
                        return new ApplValcodes()
                        {
                            ValsEntityAssociation = expectedCorrespondenceRequestsRepository.statusValcodeValues
                                .Select(val =>
                                    new ApplValcodesVals()
                                    {
                                        ValInternalCodeAssocMember = val.InternalCode,
                                        ValActionCode1AssocMember = val.Action1Code,
                                        ValExternalRepresentationAssocMember = val.Desc
                                    }
                                ).ToList()
                        };
                    }
                );

            // Set up a valid NotifyCommReqAttachment response
            NotifyCommReqAttachmentResponse notifyResponse = new NotifyCommReqAttachmentResponse();
            notifyResponse.ErrorMessages = null;
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<NotifyCommReqAttachmentRequest, NotifyCommReqAttachmentResponse>(It.Is<NotifyCommReqAttachmentRequest>(r => r.PersonId == personId))).ReturnsAsync(notifyResponse).Callback<NotifyCommReqAttachmentRequest>(req => updateRequest = req);
            

            return new CorrespondenceRequestsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        #region GetCorrespondenceRequests Tests
        [TestClass]
        public class GetCorrespondenceRequestsTests : CorrespondenceRequestsRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.BaseInitialize();
            }

            [TestCleanup]
            public void Cleanup()
            {
                base.BaseCleanup();
            }

            [TestMethod]
            public void NumberOfCorrespondenceRequestsAreEqual()
            {
                Assert.IsTrue(actualCorrespondenceRequests.Count() > 0);
                Assert.AreEqual(expectedCorrespondenceRequests.Count(), actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public void CorrespondenceRequestsListsAreEqual()
            {
                Assert.IsTrue(actualCorrespondenceRequests.Count() > 0);
                foreach (var testCorrReq in expectedCorrespondenceRequests)
                {
                    var repositoryCorrReq = actualCorrespondenceRequests.FirstOrDefault(sd => sd.Code == testCorrReq.Code);
                    Assert.IsNotNull(repositoryCorrReq);

                    Assert.AreEqual(testCorrReq.DueDate, repositoryCorrReq.DueDate);
                    Assert.AreEqual(testCorrReq.Status, repositoryCorrReq.Status);
                    Assert.AreEqual(testCorrReq.StatusDate, repositoryCorrReq.StatusDate);
                    Assert.AreEqual(testCorrReq.Instance, repositoryCorrReq.Instance);
                    Assert.AreEqual(testCorrReq.AssignDate, repositoryCorrReq.AssignDate);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task personId_RequiredTest()
            {
                await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync("");
            }

            [TestMethod]
            public async Task NoMailingRecord_ReturnsEmptyList()
            {
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync("foobar");
                Assert.IsNotNull(actualCorrespondenceRequests);
                Assert.IsFalse(actualCorrespondenceRequests.Any());
            }

            [TestMethod]
            public async Task EmptyCorrespondanceLists_ReturnsEmptyCorrespondenceRequestsListTest()
            {
                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData = new List<TestCorrespondenceRequestsRepository.ChangeCorrespondanceRecord>();
                expectedCorrespondenceRequestsRepository.MailingData.CurrentCorrespondanceRequestCodes = null;
                //CorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                Assert.AreEqual(0, actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public async Task EmptyCorrespondenceRequests_ReturnsOnlyCorrespondanceRequests()
            {
                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData = new List<TestCorrespondenceRequestsRepository.ChangeCorrespondanceRecord>();
                //CorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var corrReqDocs = expectedCorrespondenceRequestsRepository.CorrespondanceTrackData.SelectMany(t => t.CorrespondanceRequests);

                Assert.AreEqual(corrReqDocs.Count(), actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public async Task NullChCorrRecord_SkipTest()
            {
                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData[0] = null;

                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                Assert.AreEqual(expectedCorrespondenceRequests.Count() - 1, actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public async Task NullMailingDataCode_CatchLogExceptionTest()
            {
                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData[0].Code = string.Empty;
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                Assert.AreEqual(expectedCorrespondenceRequests.Count() - 1, actualCorrespondenceRequests.Count());
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NullCoreqTrackCode_SkipTrackCorrespondenceRequestsTest()
            {
                expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].Code = null;
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var skipRequestsCount = expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].CorrespondanceRequests.Count();

                Assert.AreEqual(expectedCorrespondenceRequests.Count() - skipRequestsCount, actualCorrespondenceRequests.Count());

            }

            [TestMethod]
            public async Task NullCoreqDataCode_CatchLogExceptionTest()
            {
                expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].CorrespondanceRequests[0].Code = null;
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                Assert.AreEqual(expectedCorrespondenceRequests.Count() - 1, actualCorrespondenceRequests.Count());
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task EmptyCoReq_ReturnsOnlyCorrespondenceRequests()
            {
                expectedCorrespondenceRequestsRepository.MailingData.CurrentCorrespondanceRequestCodes = new List<string>();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var corrReqs = expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData;
                Assert.AreEqual(corrReqs.Count(), actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public async Task IgnoreEmptyTrackCodesTest()
            {
                expectedCorrespondenceRequestsRepository.MailingData.CurrentCorrespondanceRequestCodes.Add(string.Empty);
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);
                Assert.AreEqual(expectedCorrespondenceRequests.Count(), actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public async Task GetCoreqRecordsOnlyWithIdsContainedInMailingListTest()
            {
                var origTrackCode = expectedCorrespondenceRequestsRepository.MailingData.CurrentCorrespondanceRequestCodes[0];
                var trackCorrespondenceRequests = expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].CorrespondanceRequests;
                expectedCorrespondenceRequestsRepository.MailingData.CurrentCorrespondanceRequestCodes = new List<string>() { "foobar" };

                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                Assert.AreEqual(expectedCorrespondenceRequests.Count() - trackCorrespondenceRequests.Count(), actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public async Task NullCoreqRequestsRecord_SkipTest()
            {
                expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].CorrespondanceRequests[0] = null;

                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                Assert.AreEqual(expectedCorrespondenceRequests.Count() - 1, actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public async Task ValcodeReadError_CatchExceptionLogMessageNoCorrReqTest()
            {
                expectedCorrespondenceRequestsRepository.statusValcodeValues = null;
                actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();

                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);
                Assert.AreEqual(0, actualCorrespondenceRequests.Count());

                loggerMock.Verify(l => l.Error("Unable to get CORE->CORR.STATUSES valcode table"));
            }

            [TestMethod]
            public async Task BadStatusCode_SetIncompleteCorrespondenceRequestStatusAndLogsMessageTest()
            {
                var testCorrReq = expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData[0];
                testCorrReq.StatusCode = "FOOBAR";

                actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var actualCorrReq = actualCorrespondenceRequests.First(d => d.Code == testCorrReq.Code);
                Assert.AreEqual(CorrespondenceRequestStatus.Incomplete, actualCorrReq.Status);

                loggerMock.Verify(l => l.Error(string.Format("Correspondence request has status {0} not present in CORR.STATUSES. Using default status values.", testCorrReq.StatusCode)));
            }

            [TestMethod]
            public void ZeroActionCode_SetWaivedCorrespondenceRequestStatusTest()
            {
                var waivedStatus = expectedCorrespondenceRequestsRepository.statusValcodeValues.First(v => v.Action1Code == "0");
                var testCorrReq = expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData.First(c => c.StatusCode == waivedStatus.InternalCode);

                var actualCorrReq = actualCorrespondenceRequests.First(d => d.Code == testCorrReq.Code);
                Assert.AreEqual(CorrespondenceRequestStatus.Waived, actualCorrReq.Status);
            }

            [TestMethod]
            public void OneActionCode_SetReceivedCorrespondenceRequestStatusTest()
            {
                var receivedStatus = expectedCorrespondenceRequestsRepository.statusValcodeValues.First(v => v.Action1Code == "1");
                var testCorrReq = expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData.First(c => c.StatusCode == receivedStatus.InternalCode);

                var actualCorrReq = actualCorrespondenceRequests.First(d => d.Code == testCorrReq.Code);
                Assert.AreEqual(CorrespondenceRequestStatus.Received, actualCorrReq.Status);
            }

            [TestMethod]
            public void OtherActionCode_SetIncompleteCorrespondenceRequestStatusTest()
            {
                var incompleteStatus = expectedCorrespondenceRequestsRepository.statusValcodeValues.First(v => v.Action1Code != "1" && v.Action1Code != "0");
                var testCorrReq = expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData.First(c => c.StatusCode == incompleteStatus.InternalCode);

                var actualCorrReq = actualCorrespondenceRequests.First(d => d.Code == testCorrReq.Code);
                Assert.AreEqual(CorrespondenceRequestStatus.Incomplete, actualCorrReq.Status);
            }

            /// <summary>
            /// Test if we have only one instance of the same correspondence request (date and instance fields are the same)
            /// in the correspondenceRequestsList
            /// </summary>
            [TestMethod]
            public async Task TwoIdenticalCorrReqs_OneCorrReqInstanceAddedTest()
            {
                var correspondenceRequest = expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].CorrespondanceRequests.FirstOrDefault(cr => cr.Code == "FA14ISIR");

                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData.Add(new Ellucian.Colleague.Domain.Base.Tests.TestCorrespondenceRequestsRepository.ChangeCorrespondanceRecord()
                {
                    Code = correspondenceRequest.Code,
                    StatusDate = correspondenceRequest.StatusDate
                });

                actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var sameCodeCorrReqList = actualCorrespondenceRequests.Where(asd => asd.Code == correspondenceRequest.Code);
                Assert.IsTrue(sameCodeCorrReqList.Count() == 1);
            }

            /// <summary>
            /// Test if we have two correspondence requests instances with the same correspondence request code (dates are different)
            /// in the correspondenceRequestsList
            /// </summary>
            [TestMethod]
            public async Task SameCodeCorrReqDifferentDates_TwoCorrReqInstancesAddedTest()
            {
                var correspondenceRequest = expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].CorrespondanceRequests.FirstOrDefault(cr => cr.Code == "FA14ISIR");

                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData.Add(
                    new Ellucian.Colleague.Domain.Base.Tests.TestCorrespondenceRequestsRepository.ChangeCorrespondanceRecord()
                    {
                        Code = correspondenceRequest.Code,
                        StatusDate = DateTime.Today
                    });

                actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var sameCodeCorrReqsList = actualCorrespondenceRequests.Where(asd => asd.Code == correspondenceRequest.Code);
                Assert.IsTrue(sameCodeCorrReqsList.Count() > 1);
            }

            /// <summary>
            /// Test if we have two correspondence requests instances with the same correspondence request code (instance fileds are different)
            /// in the correspondenceRequestsList
            /// </summary>
            [TestMethod]
            public async Task SameCodeCorrReqDifferentInstances_TwoCorrReqInstancesAddedTest()
            {
                var correspondenceRequest = expectedCorrespondenceRequestsRepository.CorrespondanceTrackData[0].CorrespondanceRequests.FirstOrDefault(cr => cr.Code == "FA14ISIR");

                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData.Add(
                    new Ellucian.Colleague.Domain.Base.Tests.TestCorrespondenceRequestsRepository.ChangeCorrespondanceRecord()
                    {
                        Code = correspondenceRequest.Code,
                        Instance = "Instance"
                    });

                actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var sameCodeCorrReqList = actualCorrespondenceRequests.Where(asd => asd.Code == correspondenceRequest.Code);
                Assert.IsTrue(sameCodeCorrReqList.Count() > 1);
            }

            [TestMethod]
            public void CorrespondenceRequestStatusDescription_AssignedExpectedValueTest()
            {
                foreach (var corrReq in expectedCorrespondenceRequests)
                {
                    var actualCorrRequment = actualCorrespondenceRequests.FirstOrDefault(d => d.Code == corrReq.Code);
                    Assert.AreEqual(corrReq.StatusDescription, actualCorrRequment.StatusDescription);
                }
            }

            [TestMethod]
            public async Task NoMatchingPersonCodeObj_IncompleteStatusDescriptionAssignedTest()
            {
                expectedCorrespondenceRequestsRepository.MailingData.ChangeCorespondanceData.Add(
                    new Ellucian.Colleague.Domain.Base.Tests.TestCorrespondenceRequestsRepository.ChangeCorrespondanceRecord()
                    {
                        Code = "CodeFoo",
                        StatusCode = "foo"
                    });
                actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();
                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                var actualCorrReq = actualCorrespondenceRequests.FirstOrDefault(d => d.Code == "CodeFoo");
                Assert.IsTrue(actualCorrReq.StatusDescription.Equals(""));
            }



        }

        #endregion

        [TestClass]
        public class AttachmentNotificationAsyncTests : CorrespondenceRequestsRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.BaseInitialize();
            }

            [TestCleanup]
            public void Cleanup()
            {
                base.BaseCleanup();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentNotificationAsyncTests_PersonIdNull()
            {
                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(null, "CCode", DateTime.Today, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentNotificationAsyncTests_PersonIdEmpty()
            {
                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(string.Empty, "CCode", DateTime.Today, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentNotificationAsyncTests_CommunicationCodeNull()
            {
                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync("PersonId", null, DateTime.Today, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentNotificationAsyncTests_CommunicationCodeEmpty()
            {
                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync("PersonId", string.Empty, DateTime.Today, null);
            }

            [TestMethod]
            public async Task AttachmentNotificationAsyncTests_Success()
            {
                var correspondenceRequestResult = await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(personId, "FA14SAP", DateTime.Today, "SAP Document");
                Assert.IsInstanceOfType(correspondenceRequestResult, typeof(CorrespondenceRequest));
                Assert.AreEqual(expectedNotifyCorrespondenceRequest.PersonId, correspondenceRequestResult.PersonId);
                Assert.AreEqual(expectedNotifyCorrespondenceRequest.Code, correspondenceRequestResult.Code);
                Assert.AreEqual(expectedNotifyCorrespondenceRequest.AssignDate, correspondenceRequestResult.AssignDate);
                Assert.AreEqual(expectedNotifyCorrespondenceRequest.Instance, correspondenceRequestResult.Instance);
                Assert.AreEqual(expectedNotifyCorrespondenceRequest.Status, correspondenceRequestResult.Status);
                Assert.AreEqual(expectedNotifyCorrespondenceRequest.StatusDescription, correspondenceRequestResult.StatusDescription);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentNotificationAsyncTests_CannotFindRequestAfterSuccessfulNotification()
            {
                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(personId, "Code", DateTime.Today, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RecordLockException))]
            public async Task AttachmentNotificationAsyncTests_CTXReturnsLockedMessage()
            {
                // Set up NotifyCommReqAttachment response contains lock error
                NotifyCommReqAttachmentResponse notifyResponse2 = new NotifyCommReqAttachmentResponse();
                notifyResponse2.ErrorMessages = new List<string>() { "Message 1", "MAILING record is locked by a user or process." };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<NotifyCommReqAttachmentRequest, NotifyCommReqAttachmentResponse>(It.Is<NotifyCommReqAttachmentRequest>(r => r.PersonId == personId))).ReturnsAsync(notifyResponse2).Callback<NotifyCommReqAttachmentRequest>(req => updateRequest = req);

                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(personId, "FA14SAP", DateTime.Today, "SAP Document");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentNotificationAsyncTests_CTXReturnsNotFoundMessage()
            {
                // Set up NotifyCommReqAttachment response contains lock error
                NotifyCommReqAttachmentResponse notifyResponse2 = new NotifyCommReqAttachmentResponse();
                notifyResponse2.ErrorMessages = new List<string>() { "Message 1", "MAILING record does not exist." };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<NotifyCommReqAttachmentRequest, NotifyCommReqAttachmentResponse>(It.Is<NotifyCommReqAttachmentRequest>(r => r.PersonId == personId))).ReturnsAsync(notifyResponse2).Callback<NotifyCommReqAttachmentRequest>(req => updateRequest = req);

                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(personId, "FA14SAP", DateTime.Today, "SAP Document");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentNotificationAsyncTests_CTXReturnsAnyOtherMessage()
            {
                // Set up NotifyCommReqAttachment response contains lock error
                NotifyCommReqAttachmentResponse notifyResponse2 = new NotifyCommReqAttachmentResponse();
                notifyResponse2.ErrorMessages = new List<string>() { "Message 1", "Message 2." };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<NotifyCommReqAttachmentRequest, NotifyCommReqAttachmentResponse>(It.Is<NotifyCommReqAttachmentRequest>(r => r.PersonId == personId))).ReturnsAsync(notifyResponse2).Callback<NotifyCommReqAttachmentRequest>(req => updateRequest = req);

                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(personId, "FA14SAP", DateTime.Today, "SAP Document");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentNotificationAsyncTests_CTXReturnsException()
            {
                // Set up NotifyCommReqAttachment response to throw an exception
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<NotifyCommReqAttachmentRequest, NotifyCommReqAttachmentResponse>(It.Is<NotifyCommReqAttachmentRequest>(r => r.PersonId == personId))).ThrowsAsync(new TimeoutException());

                await actualCorrespondenceRequestsRepository.AttachmentNotificationAsync(personId, "FA14SAP", DateTime.Today, "SAP Document");
            }
        }
    }
}
