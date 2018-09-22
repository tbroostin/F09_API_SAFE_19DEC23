//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class StudentDocumentRepositoryTests
    {
        #region GetStudentDocuments Tests
        [TestClass]
        public class GetStudentDocumentsTests : BaseRepositorySetup
        {
            private string studentId;

            private IEnumerable<StudentDocument> actualStudentDocuments;

            private StudentDocumentRepository actualStudentDocumentRepository;

            private IEnumerable<StudentDocument> expectedStudentDocuments;

            private TestFinancialAidReferenceDataRepository testReferenceDataRepository;
            private TestStudentDocumentRepository expectedStudentDocumentRepository;

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                expectedStudentDocumentRepository = new TestStudentDocumentRepository();
                testReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
                studentId = expectedStudentDocumentRepository.studentId;
                expectedStudentDocuments = await expectedStudentDocumentRepository.GetDocumentsAsync(studentId);

                actualStudentDocumentRepository = BuildStudentDocumentRepository();

                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);
            }



            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                actualStudentDocuments = null;
                actualStudentDocumentRepository = null;
                expectedStudentDocuments = null;
            }

            [TestMethod]
            public void NumberOfStudentDocumentsAreEqual()
            {
                Assert.IsTrue(actualStudentDocuments.Count() > 0);
                Assert.AreEqual(expectedStudentDocuments.Count(), actualStudentDocuments.Count());
            }

            [TestMethod]
            public void StudentDocumentListsAreEqual()
            {
                Assert.IsTrue(actualStudentDocuments.Count() > 0);
                foreach (var testStudentDoc in expectedStudentDocuments)
                {
                    var repositoryStudentDoc = actualStudentDocuments.FirstOrDefault(sd => sd.Code == testStudentDoc.Code);
                    Assert.IsNotNull(repositoryStudentDoc);

                    Assert.AreEqual(testStudentDoc.DueDate, repositoryStudentDoc.DueDate);
                    Assert.AreEqual(testStudentDoc.Status, repositoryStudentDoc.Status);
                    Assert.AreEqual(testStudentDoc.StatusDate, repositoryStudentDoc.StatusDate);
                    Assert.AreEqual(testStudentDoc.Instance, repositoryStudentDoc.Instance);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentId_RequiredTest()
            {
                await actualStudentDocumentRepository.GetDocumentsAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoMailingRecord_ThrowsExceptionTest()
            {
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync("foobar");
            }

            [TestMethod]
            public async Task EmptyCorrespondanceLists_ReturnsEmptyStudentDocumentListTest()
            {
                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData = new List<TestStudentDocumentRepository.ChangeCorrespondanceRecord>();
                expectedStudentDocumentRepository.MailingData.CurrentCorrespondanceRequestCodes = null;
                //studentDocumentRepository = BuildStudentDocumentRepository();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                Assert.AreEqual(0, actualStudentDocuments.Count());
            }

            [TestMethod]
            public async Task EmptyDocuments_ReturnsOnlyCorrespondanceRequestDocuments()
            {
                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData = new List<TestStudentDocumentRepository.ChangeCorrespondanceRecord>();
                //studentDocumentRepository = BuildStudentDocumentRepository();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var corrReqDocs = expectedStudentDocumentRepository.CorrespondanceTrackData.SelectMany(t => t.CorrespondanceRequests);

                Assert.AreEqual(corrReqDocs.Count(), actualStudentDocuments.Count());
            }

            [TestMethod]
            public async Task NullChCorrRecord_SkipTest()
            {
                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData[0] = null;

                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                Assert.AreEqual(expectedStudentDocuments.Count() - 1, actualStudentDocuments.Count());
            }

            [TestMethod]
            public async Task NullMailingDataCode_CatchLogExceptionTest()
            {
                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData[0].Code = string.Empty;
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                Assert.AreEqual(expectedStudentDocuments.Count() - 1, actualStudentDocuments.Count());
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NullCoreqTrackCode_SkipTrackDocumentsTest()
            {
                expectedStudentDocumentRepository.CorrespondanceTrackData[0].Code = null;
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var skipDocsCount = expectedStudentDocumentRepository.CorrespondanceTrackData[0].CorrespondanceRequests.Count();

                Assert.AreEqual(expectedStudentDocuments.Count() - skipDocsCount, actualStudentDocuments.Count());

            }

            [TestMethod]
            public async Task NullCoreqDataCode_CatchLogExceptionTest()
            {
                expectedStudentDocumentRepository.CorrespondanceTrackData[0].CorrespondanceRequests[0].Code = null;
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                Assert.AreEqual(expectedStudentDocuments.Count() - 1, actualStudentDocuments.Count());
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task EmptyCoReq_ReturnsOnlyDocuments()
            {
                expectedStudentDocumentRepository.MailingData.CurrentCorrespondanceRequestCodes = new List<string>();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var docs = expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData;
                Assert.AreEqual(docs.Count(), actualStudentDocuments.Count());
            }

            [TestMethod]
            public async Task IgnoreEmptyTrackCodesTest()
            {
                expectedStudentDocumentRepository.MailingData.CurrentCorrespondanceRequestCodes.Add(string.Empty);
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);
                Assert.AreEqual(expectedStudentDocuments.Count(), actualStudentDocuments.Count());
            }

            [TestMethod]
            public async Task GetCoreqRecordsOnlyWithIdsContainedInMailingListTest()
            {
                var origTrackCode = expectedStudentDocumentRepository.MailingData.CurrentCorrespondanceRequestCodes[0];
                var trackDocuments = expectedStudentDocumentRepository.CorrespondanceTrackData[0].CorrespondanceRequests;
                expectedStudentDocumentRepository.MailingData.CurrentCorrespondanceRequestCodes = new List<string>() { "foobar" };

                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                Assert.AreEqual(expectedStudentDocuments.Count() - trackDocuments.Count(), actualStudentDocuments.Count());
            }

            [TestMethod]
            public async Task NullCoreqRequestsRecord_SkipTest()
            {
                expectedStudentDocumentRepository.CorrespondanceTrackData[0].CorrespondanceRequests[0] = null;

                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                Assert.AreEqual(expectedStudentDocuments.Count() - 1, actualStudentDocuments.Count());
            }

            [TestMethod]
            public async Task ValcodeReadError_CatchExceptionLogMessageNoDocumentsTest()
            {
                expectedStudentDocumentRepository.statusValcodeValues = null;
                actualStudentDocumentRepository = BuildStudentDocumentRepository();

                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);
                Assert.AreEqual(0, actualStudentDocuments.Count());

                loggerMock.Verify(l => l.Error("Unable to get CORE->CORR.STATUSES valcode table"));
            }

            [TestMethod]
            public async Task BadStatusCode_SetIncompleteDocumentStatusAndLogsMessageTest()
            {
                var testDoc = expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData[0];
                testDoc.StatusCode = "FOOBAR";

                actualStudentDocumentRepository = BuildStudentDocumentRepository();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var actualDoc = actualStudentDocuments.First(d => d.Code == testDoc.Code);
                Assert.AreEqual(DocumentStatus.Incomplete, actualDoc.Status);

                loggerMock.Verify(l => l.Info(string.Format("{0} is not a valid status code in CORR.STATUSES", testDoc.StatusCode)));
            }

            [TestMethod]
            public void ZeroActionCode_SetWaivedDocumentStatusTest()
            {
                var waivedStatus = expectedStudentDocumentRepository.statusValcodeValues.First(v => v.Action1Code == "0");
                var testDoc = expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData.First(c => c.StatusCode == waivedStatus.InternalCode);

                var actualDoc = actualStudentDocuments.First(d => d.Code == testDoc.Code);
                Assert.AreEqual(DocumentStatus.Waived, actualDoc.Status);
            }

            [TestMethod]
            public void OneActionCode_SetReceivedDocumentStatusTest()
            {
                var receivedStatus = expectedStudentDocumentRepository.statusValcodeValues.First(v => v.Action1Code == "1");
                var testDoc = expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData.First(c => c.StatusCode == receivedStatus.InternalCode);

                var actualDoc = actualStudentDocuments.First(d => d.Code == testDoc.Code);
                Assert.AreEqual(DocumentStatus.Received, actualDoc.Status);
            }

            [TestMethod]
            public void OtherActionCode_SetIncompleteDocumentStatusTest()
            {
                var incompleteStatus = expectedStudentDocumentRepository.statusValcodeValues.First(v => v.Action1Code != "1" && v.Action1Code != "0");
                var testDoc = expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData.First(c => c.StatusCode == incompleteStatus.InternalCode);

                var actualDoc = actualStudentDocuments.First(d => d.Code == testDoc.Code);
                Assert.AreEqual(DocumentStatus.Incomplete, actualDoc.Status);
            }

            /// <summary>
            /// Test if we have only one instance of the same document (date and instance fields are the same)
            /// in the studentDocumentList
            /// </summary>
            [TestMethod]
            public async Task TwoIdenticalDocs_OneDocInstanceAddedTest()
            {
                var document = expectedStudentDocumentRepository.CorrespondanceTrackData[0].CorrespondanceRequests.FirstOrDefault(cr => cr.Code == "FA14ISIR");

                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData.Add(new Ellucian.Colleague.Domain.FinancialAid.Tests.TestStudentDocumentRepository.ChangeCorrespondanceRecord()
                {
                    Code = document.Code,
                    StatusDate = document.StatusDate
                });

                actualStudentDocumentRepository = BuildStudentDocumentRepository();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var sameCodeDocumentsList = actualStudentDocuments.Where(asd => asd.Code == document.Code);
                Assert.IsTrue(sameCodeDocumentsList.Count() == 1);
            }

            /// <summary>
            /// Test if we have two document instances with the same document code (dates are different)
            /// in the studentDocumentList
            /// </summary>
            [TestMethod]
            public async Task SameCodeDocsDifferentDates_TwoDocInstancesAddedTest()
            {
                var document = expectedStudentDocumentRepository.CorrespondanceTrackData[0].CorrespondanceRequests.FirstOrDefault(cr => cr.Code == "FA14ISIR");

                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData.Add(
                    new Ellucian.Colleague.Domain.FinancialAid.Tests.TestStudentDocumentRepository.ChangeCorrespondanceRecord()
                    {
                        Code = document.Code,
                        StatusDate = DateTime.Today
                    });

                actualStudentDocumentRepository = BuildStudentDocumentRepository();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var sameCodeDocumentsList = actualStudentDocuments.Where(asd => asd.Code == document.Code);
                Assert.IsTrue(sameCodeDocumentsList.Count() > 1);
            }

            /// <summary>
            /// Test if we have two document instances with the same document code (instance fileds are different)
            /// in the studentDocumentList
            /// </summary>
            [TestMethod]
            public async Task SameCodeDocsDifferentInstances_TwoDocInstancesAddedTest()
            {
                var document = expectedStudentDocumentRepository.CorrespondanceTrackData[0].CorrespondanceRequests.FirstOrDefault(cr => cr.Code == "FA14ISIR");

                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData.Add(
                    new Ellucian.Colleague.Domain.FinancialAid.Tests.TestStudentDocumentRepository.ChangeCorrespondanceRecord()
                    {
                        Code = document.Code,
                        Instance = "Instance"
                    });

                actualStudentDocumentRepository = BuildStudentDocumentRepository();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var sameCodeDocumentsList = actualStudentDocuments.Where(asd => asd.Code == document.Code);
                Assert.IsTrue(sameCodeDocumentsList.Count() > 1);
            }

            [TestMethod]
            public void DocumentStatusDescription_AssignedExpectedValueTest()
            {
                foreach (var doc in expectedStudentDocuments)
                {
                    var actualDocument = actualStudentDocuments.FirstOrDefault(d => d.Code == doc.Code);
                    Assert.AreEqual(doc.StatusDescription, actualDocument.StatusDescription);
                }
            }

            [TestMethod]
            public async Task NoMatchingStudentCodeObj_IncompleteStatusDescriptionAssignedTest()
            {
                expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData.Add(
                    new Ellucian.Colleague.Domain.FinancialAid.Tests.TestStudentDocumentRepository.ChangeCorrespondanceRecord()
                    {
                        Code = "CodeFoo",
                        StatusCode = "foo"
                    });
                actualStudentDocumentRepository = BuildStudentDocumentRepository();
                actualStudentDocuments = await actualStudentDocumentRepository.GetDocumentsAsync(studentId);

                var actualDocument = actualStudentDocuments.FirstOrDefault(d => d.Code == "CodeFoo");
                Assert.IsTrue(actualDocument.StatusDescription.Equals("Incomplete"));
            }


            private StudentDocumentRepository BuildStudentDocumentRepository()
            {
                dataReaderMock.Setup(m => m.ReadRecordAsync<Mailing>(studentId, true))
                    .Returns<string, bool>(
                        (a, b) =>
                        {
                            if (expectedStudentDocumentRepository.MailingData == null) return null;
                            return Task.FromResult(new Mailing()
                            {
                                Recordkey = studentId,
                                MailingCurrentCrcCode = expectedStudentDocumentRepository.MailingData.CurrentCorrespondanceRequestCodes,
                                ChCorrEntityAssociation =
                                    (expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData == null) ? null :
                                    expectedStudentDocumentRepository.MailingData.ChangeCorespondanceData
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
                            if (expectedStudentDocumentRepository.CorrespondanceTrackData == null) return null;
                            return Task.FromResult(new Collection<Coreq>(
                                expectedStudentDocumentRepository.CorrespondanceTrackData
                                    .Where(track => track != null && recordIds.Contains(string.Format("{0}*{1}", studentId, track.Code)) && track.CorrespondanceRequests != null)
                                    .Select(track =>
                                        new Coreq()
                                        {
                                            Recordkey = string.Format("{0}*{1}", studentId, track.Code),
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
                            if (expectedStudentDocumentRepository.statusValcodeValues == null) return null;
                            return new ApplValcodes()
                            {
                                ValsEntityAssociation = expectedStudentDocumentRepository.statusValcodeValues
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

                return new StudentDocumentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }

        #endregion

        
    }
}
