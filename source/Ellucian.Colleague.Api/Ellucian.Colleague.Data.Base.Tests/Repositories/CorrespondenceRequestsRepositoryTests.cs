//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class CorrespondenceRequestsRepositoryTests
    {
        #region GetCorrespondenceRequests Tests
        [TestClass]
        public class GetCorrespondenceRequestsTests : BaseRepositorySetup
        {
            private string personId;

            private IEnumerable<CorrespondenceRequest> actualCorrespondenceRequests;

            private CorrespondenceRequestsRepository actualCorrespondenceRequestsRepository;

            private IEnumerable<CorrespondenceRequest> expectedCorrespondenceRequests;

            private TestCorrespondenceRequestsRepository expectedCorrespondenceRequestsRepository;

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                expectedCorrespondenceRequestsRepository = new TestCorrespondenceRequestsRepository();
                personId = expectedCorrespondenceRequestsRepository.personId;
                expectedCorrespondenceRequests = await expectedCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

                actualCorrespondenceRequestsRepository = BuildCorrespondenceRequestsRepository();

                actualCorrespondenceRequests = await actualCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);
            }



            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                actualCorrespondenceRequests = null;
                actualCorrespondenceRequestsRepository = null;
                expectedCorrespondenceRequests = null;
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
                                                MailingCorrRecvdInstanceAssocMember = chCorr.Instance
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
                                                        CoreqCcStatusAssocMember = coreq.StatusCode
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

                return new CorrespondenceRequestsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }

        #endregion


    }
}
