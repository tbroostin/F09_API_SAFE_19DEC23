using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class CommunicationRepositoryTests : BaseRepositorySetup
    {
        public string personId;

        public List<string> expectedTransactionErrorMessages;

        public TestCommunicationRepository expectedRepository;
        public CommunicationRepository actualRepository;

        public CreateCommunicationRequest actualCreateCommunicationRequestCtx;
        public UpdateCommunicationRequest actualUpdateCommunicationRequestCtx;

        public void CommunicationRepositoryTestsInitialize()
        {
            MockInitialize();
            personId = "0003914";
            expectedRepository = new TestCommunicationRepository();

            expectedTransactionErrorMessages = new List<string>();

            actualRepository = BuildRepository();
        }

        private CommunicationRepository BuildRepository()
        {
            dataReaderMock.Setup(dr => dr.ReadRecord<Mailing>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((id, b) => (expectedRepository.mailingData == null || expectedRepository.mailingData.ChangeCorespondanceData == null) ? null
                    : new Mailing()
                    {
                        Recordkey = id,
                        ChCorrEntityAssociation = expectedRepository.mailingData.ChangeCorespondanceData.Select(cc =>
                            new MailingChCorr()
                            {
                                MailingCorrReceivedAssocMember = cc.Code,
                                MailingCorrRecvdInstanceAssocMember = cc.Instance,
                                MailingCorrRecvdAsgnDtAssocMember = cc.AssignedDate,
                                MailingCorrRecvdStatusAssocMember = cc.StatusCode,
                                MailingCorrReceivedDateAssocMember = cc.StatusDate,
                                MailingCorrRecvdActDtAssocMember = cc.ActionDate,
                                MailingCorrRecvdCommentAssocMember = cc.CommentId
                            }).ToList(),
                        MailingCurrentCrcCode = expectedRepository.mailingData.CurrentCorrespondanceRequestCodes
                    });

            dataReaderMock.Setup(dr => dr.BulkReadRecord<Coreq>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => (expectedRepository.correspondanceTrackData == null) ? null : new Collection<Coreq>(
                    expectedRepository.correspondanceTrackData
                    .Where(t => ids.Contains(string.Format("{0}*{1}", personId, t.Code)))
                    .Select(t => new Coreq()
                    {
                        Recordkey = string.Format("{0}*{1}", personId, t.Code),
                        CoreqRequestsEntityAssociation = (t.CorrespondanceRequests == null) ? null : t.CorrespondanceRequests.Select(cc => new CoreqCoreqRequests()
                        {
                            CoreqCcCodeAssocMember = cc.Code,
                            CoreqCcInstanceAssocMember = cc.Instance,
                            CoreqCcAssignDtAssocMember = cc.AssignedDate,
                            CoreqCcStatusAssocMember = cc.StatusCode,
                            CoreqCcDateAssocMember = cc.StatusDate,
                            CoreqCcExpActDtAssocMember = cc.ActionDate,
                            CoreqCcCommentAssocMember = cc.CommentId
                        }).ToList()
                    }).ToList()));

            transManagerMock.Setup(tr => tr.Execute<CreateCommunicationRequest, CreateCommunicationResponse>(It.IsAny<CreateCommunicationRequest>()))
                .Callback<CreateCommunicationRequest>((req) => actualCreateCommunicationRequestCtx = req)
                .Returns<CreateCommunicationRequest>((req) => new CreateCommunicationResponse()
                {
                    ErrorMessages = expectedTransactionErrorMessages
                });

            transManagerMock.Setup(tr => tr.Execute<UpdateCommunicationRequest, UpdateCommunicationResponse>(It.IsAny<UpdateCommunicationRequest>()))
                .Callback<UpdateCommunicationRequest>((req) => actualUpdateCommunicationRequestCtx = req)
                .Returns<UpdateCommunicationRequest>((req) => new UpdateCommunicationResponse()
                {
                    ErrorMessages = expectedTransactionErrorMessages
                });

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            loggerMock.Setup(l => l.IsWarnEnabled).Returns(true);
            loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

            return new CommunicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetCommuncationsTests : CommunicationRepositoryTests
        {
            public List<Communication> expectedCommunications
            { get { return expectedRepository.GetCommunications(personId).ToList(); } }
            public List<Communication> actualCommunications
            { get { return actualRepository.GetCommunications(personId).ToList(); } }

            [TestInitialize]
            public void Initialize()
            {
                CommunicationRepositoryTestsInitialize();
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedCommunications, actualCommunications);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdRequiredTest()
            {
                personId = null;
                var test = actualCommunications;
            }

            [TestMethod]
            public void NullMailingRecordReturnsEmptyCommunicationsTest()
            {
                expectedRepository.mailingData = null;
                Assert.AreEqual(0, actualCommunications.Count());
                loggerMock.Verify(l => l.Warn("Person has no mailing record"));
            }

            [TestMethod]
            public void EmptyChCorrEntitiesAndCurrentCrcCodesReturnsEmptyCommunicationsTest()
            {
                expectedRepository.mailingData.ChangeCorespondanceData = new List<TestCommunicationRepository.CorrespondanceRecord>();
                expectedRepository.mailingData.CurrentCorrespondanceRequestCodes = new List<string>();
                Assert.AreEqual(0, actualCommunications.Count());
            }

            [TestMethod]
            public void DuplicateCommunicationInChCorrEntityNotAddedAndLogsDataErrorTest()
            {
                var dup = expectedRepository.mailingData.ChangeCorespondanceData.First();
                expectedRepository.mailingData.ChangeCorespondanceData.Add(new TestCommunicationRepository.CorrespondanceRecord()
                    {
                        Code = dup.Code,
                        Instance = dup.Instance,
                        AssignedDate = dup.AssignedDate,
                        ActionDate = dup.ActionDate,
                        CommentId = dup.CommentId,
                        StatusCode = dup.StatusCode,
                        StatusDate = dup.StatusDate
                    });

                var similarComparer = new FunctionEqualityComparer<Communication>((c1, c2) => c1.Similar(c2), c => c.GetHashCode());

                var distinctCommunications = actualCommunications.Distinct(similarComparer);
                CollectionAssert.AreEqual(distinctCommunications.ToList(), actualCommunications);

                loggerMock.Verify(l => l.Info(It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public void ErrorCreatingCommunicationLogsErrorTest()
            {
                var beforeCount = actualCommunications.Count();
                expectedRepository.mailingData.ChangeCorespondanceData.First().Code = null;

                var test = actualCommunications;

                Assert.AreEqual(beforeCount - 1, actualCommunications.Count());

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public void IgnoreEmptyCrcCodeTest()
            {
                expectedRepository.mailingData.ChangeCorespondanceData = new List<TestCommunicationRepository.CorrespondanceRecord>();
                expectedRepository.mailingData.CurrentCorrespondanceRequestCodes = new List<string>() { "", "" };

                Assert.AreEqual(0, actualCommunications.Count());
            }

            [TestMethod]
            public void EmptyCoreqRecordsTest()
            {
                expectedRepository.mailingData.ChangeCorespondanceData = null;
                expectedRepository.correspondanceTrackData = new List<TestCommunicationRepository.CorrespondanceTrackRecord>();

                Assert.AreEqual(0, actualCommunications.Count());
            }

            [TestMethod]
            public void DuplicateCommunicationIsCoreqRequestsNotAddedTest()
            {
                expectedRepository.mailingData.CurrentCorrespondanceRequestCodes.Add("FOOBAR");

                var dup = expectedRepository.mailingData.ChangeCorespondanceData.First();
                expectedRepository.correspondanceTrackData.Add(new TestCommunicationRepository.CorrespondanceTrackRecord()
                    {
                        Code = "FOOBAR",
                        CorrespondanceRequests = new List<TestCommunicationRepository.CorrespondanceRequestRecord>()
                        {
                            new TestCommunicationRepository.CorrespondanceRequestRecord()
                            {
                                Code = dup.Code,
                                Instance = dup.Instance,
                                AssignedDate = dup.AssignedDate,
                                ActionDate = dup.ActionDate,
                                CommentId = dup.CommentId,
                                StatusCode = dup.StatusCode,
                                StatusDate = dup.StatusDate
                            }
                        }
                    });

                var similarComparer = new FunctionEqualityComparer<Communication>((c1, c2) => c1.Similar(c2), c => c.GetHashCode());

                var distinctCommunications = actualCommunications.Distinct(similarComparer);
                CollectionAssert.AreEqual(distinctCommunications.ToList(), actualCommunications);
            }

            [TestMethod]
            public void ErrorCreatingCommunicationFromTrackLogsErrorTest()
            {
                expectedRepository.correspondanceTrackData.First().CorrespondanceRequests.First().Code = null;
                var test = actualCommunications;

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }

        }

        [TestClass]
        public class UpdateCommunicationsTests : CommunicationRepositoryTests
        {
            public Communication inputCommunication;

            public IEnumerable<Communication> inputExistingCommunications;

            public Communication expectedUpdatedCommunication
            { get { return expectedRepository.UpdateCommunication(inputCommunication, inputExistingCommunications); } }

            public Communication actualUpdateCommunication
            { get { return actualRepository.UpdateCommunication(inputCommunication, inputExistingCommunications); } }

            public string statusCode;
            public DateTime? statusDate;

            [TestInitialize]
            public void Initialize()
            {
                CommunicationRepositoryTestsInitialize();

                inputExistingCommunications = expectedRepository.GetCommunications(personId);

                inputCommunication = inputExistingCommunications.First().DeepCopy();

                statusCode = "W";
                statusDate = DateTime.Today;

                inputCommunication.StatusCode = statusCode;
                inputCommunication.StatusDate = statusDate;
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.AreEqual(expectedUpdatedCommunication, actualUpdateCommunication);
                Assert.AreEqual(inputCommunication.StatusCode, actualUpdateCommunication.StatusCode);
                Assert.AreEqual(inputCommunication.StatusDate, actualUpdateCommunication.StatusDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InputCommunicationRequiredTest()
            {
                inputCommunication = null;
                var test = actualUpdateCommunication;
            }

            [TestMethod]
            public void ExistingCommunicationsNullSameResultTest()
            {
                inputExistingCommunications = null;
                Assert.AreEqual(expectedUpdatedCommunication, actualUpdateCommunication);
                Assert.AreEqual(inputCommunication.StatusCode, actualUpdateCommunication.StatusCode);
                Assert.AreEqual(inputCommunication.StatusDate, actualUpdateCommunication.StatusDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void InputCommunicationNotAnUpdateTest()
            {
                inputCommunication.InstanceDescription = "FOOBAR";
                var test = actualUpdateCommunication;
            }

            [TestMethod]
            public void ActualRequestTransactionTest()
            {
                var test = actualUpdateCommunication;
                var updatedCommunication = inputCommunication.ReviseDatesForCreateOrUpdate(inputExistingCommunications);

                Assert.IsNotNull(actualUpdateCommunicationRequestCtx);
                Assert.AreEqual(updatedCommunication.PersonId, actualUpdateCommunicationRequestCtx.PersonId);
                Assert.AreEqual(updatedCommunication.Code, actualUpdateCommunicationRequestCtx.CommunicationCode);
                Assert.AreEqual(updatedCommunication.InstanceDescription, actualUpdateCommunicationRequestCtx.Instance);
                Assert.AreEqual(updatedCommunication.AssignedDate, actualUpdateCommunicationRequestCtx.AssignedDate);
                Assert.AreEqual(updatedCommunication.StatusCode, actualUpdateCommunicationRequestCtx.StatusCode);
                Assert.AreEqual(updatedCommunication.StatusDate, actualUpdateCommunicationRequestCtx.StatusDate);
                Assert.AreEqual(updatedCommunication.ActionDate, actualUpdateCommunicationRequestCtx.ActionDate);
                Assert.AreEqual(updatedCommunication.CommentId, actualUpdateCommunicationRequestCtx.CommentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void ErrorMessagesInResponseCtxTest()
            {
                expectedTransactionErrorMessages = new List<string>() { "Error1", "Error2" };
                try
                {
                    var test = actualUpdateCommunication;
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class CreateCommunicationsTests : CommunicationRepositoryTests
        {
            public Communication inputCommunication;
            public IEnumerable<Communication> inputExistingCommunications;

            public Communication expectedNewCommunication
            { get { return expectedRepository.CreateCommunication(inputCommunication, inputExistingCommunications); } }

            public Communication actualNewCommunication
            { get { return actualRepository.CreateCommunication(inputCommunication, inputExistingCommunications); } }

            public string code;
            public string instance;
            public DateTime? assignedDate;

            [TestInitialize]
            public void Initialize()
            {
                CommunicationRepositoryTestsInitialize();

                inputExistingCommunications = expectedRepository.GetCommunications(personId);

                code = "FOO";
                instance = "BAR";
                assignedDate = new DateTime(2015, 1, 1);

                inputCommunication = new Communication(personId, code)
                {
                    InstanceDescription = instance,
                    AssignedDate = assignedDate
                };
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.AreEqual(expectedNewCommunication, actualNewCommunication);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InputCommunicationRequiredTest()
            {
                inputCommunication = null;
                var test = actualNewCommunication;
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public void InputCommunicationNotACreateTest()
            {
                inputCommunication = inputExistingCommunications.First().DeepCopy();
                try
                {
                    var test = actualNewCommunication;
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Communication {0} already exists for person {1}", inputCommunication, personId)));
                    var ere = e as ExistingResourceException;
                    Assert.AreEqual(inputCommunication.ToString(), ere.ExistingResourceId);
                    throw;
                }
            }


            //actual req trans

            //error messages

            //get returns null?
        }
    }
}

