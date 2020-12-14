using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class AdmissionApplicationSupportingItemsRepositoryTests_V12
    {
        [TestClass]
        public class AdmissionApplicationSupportingItemsRepositoryTests_GETALL_GETBYID : BaseRepositorySetup
        {
            #region DECLARATIONS

            private AdmissionApplicationSupportingItemsRepository admissionApplicationSupportingItemsRepository;

            private Applications application;

            private Mailing mailing;

            private CcComments comments;

            private Collection<Coreq> coreqCollection;

            private ApplValcodes applValcodes;



            private string guid = "34f1a5e3-4c9a-475d-9914-9ada10be7958";

            #endregion

            #region TESTSETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                admissionApplicationSupportingItemsRepository = new AdmissionApplicationSupportingItemsRepository(cacheProviderMock.Object,
                    transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationSupportingItemsRepository = null;
            }

            private void InitializeTestData()
            {
                application = new Applications()
                {
                    ApplApplicant = "1",
                    Recordkey = "1",
                    RecordGuid = Guid.NewGuid().ToString()
                };

                mailing = new Mailing()
                {
                    ChCorrEntityAssociation = new List<MailingChCorr>()
                    {
                        new MailingChCorr()
                        {
                            MailingCorrReceivedAssocMember = "1",
                            MailingCorrRecvdAsgnDtAssocMember  = DateTime.Today,
                            MailingCorrRecvdInstanceAssocMember = "1",
                            MailingCorrRecvdCommentAssocMember = "1",
                            MailingCorrRecvdStatusAssocMember = "1"
                        }
                    },
                    Recordkey = "1",
                    MailingCurrentCrcCode = new List<string>() { "USD" },
                    MailingCorrRecvdComment = new List<string>() { "1" },
                    MailingAdmAppSiIdx = new List<string>() { "1*1*" + (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1" }
                };

                comments = new CcComments()
                {
                    Recordkey = "1",
                    CcCommentsText = "comments_VM"
                };

                coreqCollection = new Collection<Coreq>()
                {
                    new Coreq()
                    {
                        Recordkey = "1*USD",
                        CoreqRequestsEntityAssociation = new List<CoreqCoreqRequests>()
                        {
                            new CoreqCoreqRequests()
                            {
                                CoreqCcCodeAssocMember = "1",
                                CoreqCcRequiredAssocMember = "Y"
                            }
                        }
                    }
                };

                applValcodes = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "1",
                            ValActionCode1AssocMember = "1"
                        }
                    }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), true)).ReturnsAsync(application);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Mailing>(It.IsAny<string>(), true)).ReturnsAsync(mailing);
                dataReaderMock.Setup(d => d.ReadRecordAsync<CcComments>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(comments);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Coreq>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(coreqCollection);
                //dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new string[] { guid });
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .ReturnsAsync(new Dictionary<string, RecordKeyLookupResult>() 
                    { 
                        { 
                            "MAILING+1+1*1*" + (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1", new RecordKeyLookupResult() { Guid = guid } 
                        } 
                    });
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.SelectAsync("MAILING", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[]
                { "1ý1*1*" + (new DateTime(1967, 12, 31) - DateTime.Today).TotalDays.ToString() + "*1", "2ý2*2*" + (new DateTime(1967, 12, 31) - DateTime.Today).TotalDays.ToString() + "*2" });
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2" });

                var response = new GetCacheApiKeysResponse()
                {
                    KeyCacheInfo = new List<KeyCacheInfo>() { new KeyCacheInfo() { KeyCacheMax = 1, KeyCacheMin = 1, KeyCachePart = "cache", KeyCacheSize = 1 } },
                    Limit = 1,
                    Offset = 0,
                    TotalCount = 1,
                    Sublist = new List<string>() { "1*1*" + (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1" }
                };
                transManagerMock.Setup(t => t.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>())).ReturnsAsync(response);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAdmissionApplicationSupportingItemsIdFromGuidAsync_ArgumentNullException()
            {
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsIdFromGuidAsync_Returns_Dictionary_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsIdFromGuidAsync_Returns_Empty_Dictionary()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { });
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsIdFromGuidAsync_Returns_Empty_Value()
            {
                /* This test to validate the dictionary result with empty value */
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", null } });
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetAdmissionApplicationSupportingItemsIdFromGuidAsync_Returns_Invalid_Entity()
            {
                /* This test to validate the dictionary result with invalid entity name */
                var result = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "INVALID" } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(guid);
            }

            [TestMethod]
            public async Task AdmnApplSupprngItemsRepo_GetAdmissionApplicationSupportingItemsIdFromGuidAsync()
            {
                var dicResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "MAILING", PrimaryKey = "1" } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                var result = await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Value.Entity, dicResult.FirstOrDefault().Value.Entity);
                Assert.AreEqual(result.Value.PrimaryKey, dicResult.FirstOrDefault().Value.PrimaryKey);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAdmissionApplicationSupportingItemsByIdAsync_ArgumentNullException()
            {
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByIdAsync(guid, null, "1", DateTime.Today, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsByIdAsync_Application_KeyNotFoundException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByIdAsync(guid, "1", "1", DateTime.Today, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsByIdAsync_MailingId_KeyNotFoundException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Mailing>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByIdAsync(guid, "1", "1", DateTime.Today, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsByIdAsync_MailingChCorr_Exception()
            {
                mailing = new Mailing()
                {
                    ChCorrEntityAssociation = new List<MailingChCorr>() { },
                    MailingCurrentCrcCode = new List<string>() { }
                };
                dataReaderMock.Setup(d => d.ReadRecordAsync<Mailing>(It.IsAny<string>(), true)).ReturnsAsync(mailing);
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByIdAsync(guid, "1", "1", DateTime.Today, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAdmissionApplicationSupportingItemsByIdAsync_Record_NotExist()
            {
                /* This test helps to cover the code when there is no record exists in LDM.GUID table for the given id*/

                var dateInput = new DateTime(1967, 12, 31).AddDays((new DateTime(1967, 12, 31) - DateTime.Today).TotalDays);
                var result = await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByIdAsync(null, "1", "1", dateInput, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetAdmissionApplicationSupportingItemsByIdAsync_CoreStatuses_Empty()
            {
                var dateInput = DateTime.Today;
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(null);
                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByIdAsync(guid, "1", "1", dateInput, "1");
            }

            [TestMethod]
            public async Task AdmnApplSupprngItemsRepo_GetAdmissionApplicationSupportingItemsByIdAsync()
            {
                var dateInput = DateTime.Today;

                var result = await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByIdAsync(guid, "1", "1", dateInput, "1");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_KeyNotFoundException()
            {
                /* This test validating the keynotfoundexception when secondary key is null or empty */
                var expected = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "MAILING", PrimaryKey = "1" } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(expected);

                await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task AdmnApplSupprngItemsRepo_GetAdmissionApplicationSupportingItemsByGuidAsync()
            {
                var secondaryKey = "1*1*" + (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1";
                var expected = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "MAILING", PrimaryKey = "1", SecondaryKey = secondaryKey } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(expected);

                var result = await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }

           
            [TestMethod]
            public async Task GetAdmissionApplicationSupportingItemsAsync_Returns_MailingSubList_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(null);
                var result = await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsAsync(0, 2);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task GetAdmissionApplicationSupportingItemsAsync_Returns_MailingSubList_Empty()
            {
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });

                var result = await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsAsync(0, 2);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task GetAdmissionApplicationSupportingItemsAsync()
            {
                dataReaderMock.Setup(d => d.SelectAsync("MAILING", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[]
                { "1ý1*1*" + (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1"});
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<Applications>() { application });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Mailing>("MAILING", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<Mailing>() { mailing });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<CcComments>("CC.COMMENTS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<CcComments>() { comments });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Coreq>("COREQ", It.IsAny<string[]>(), true)).ReturnsAsync(coreqCollection);

                var result = await admissionApplicationSupportingItemsRepository.GetAdmissionApplicationSupportingItemsAsync(0, 2);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 1);
                Assert.AreEqual(result.Item1.FirstOrDefault().Guid, guid);
            }
        }

        [TestClass]
        public class AdmissionApplicationSupportingItemsRepositoryTests_POST_PUT : BaseRepositorySetup
        {
            #region DECLARATIONS

            private AdmissionApplicationSupportingItemsRepository admissionApplicationSupportingItemsRepository;

            private Applications application;

            private Mailing mailing;

            private CcComments comments;

            private Collection<Coreq> coreqCollection;

            private ApplValcodes applValcodes;

            private AdmissionApplicationSupportingItem admissionApplicationSupportingItem;

            private UpdateApplicationSupportingItemResponse response;

            private Dictionary<string, GuidLookupResult> guidDetails;

            private string guid = "34f1a5e3-4c9a-475d-9914-9ada10be7958";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                admissionApplicationSupportingItemsRepository = new AdmissionApplicationSupportingItemsRepository(cacheProviderMock.Object,
                    transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationSupportingItemsRepository = null;
            }

            private void InitializeTestData()
            {
                application = new Applications()
                {
                    ApplApplicant = "1",
                    Recordkey = "1",
                    RecordGuid = Guid.NewGuid().ToString()
                };

                mailing = new Mailing()
                {
                    ChCorrEntityAssociation = new List<MailingChCorr>()
                    {
                        new MailingChCorr()
                        {
                            MailingCorrReceivedAssocMember = "1",
                            MailingCorrRecvdAsgnDtAssocMember  = DateTime.Today,
                            MailingCorrRecvdInstanceAssocMember = "1",
                            MailingCorrRecvdCommentAssocMember = "1",
                            MailingCorrRecvdStatusAssocMember = "1"
                        }
                    },
                    Recordkey = "1",
                    MailingCurrentCrcCode = new List<string>() { "USD" },
                    MailingCorrRecvdComment = new List<string>() { "1" },
                    MailingAdmAppSiIdx = new List<string>() { "1*1*" + (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1" }
                };

                comments = new CcComments()
                {
                    Recordkey = "1",
                    CcCommentsText = "comments_VM"
                };

                coreqCollection = new Collection<Coreq>()
                {
                    new Coreq()
                    {
                        Recordkey = "1*USD",
                        CoreqRequestsEntityAssociation = new List<CoreqCoreqRequests>()
                        {
                            new CoreqCoreqRequests()
                            {
                                CoreqCcCodeAssocMember = "1",
                                CoreqCcRequiredAssocMember = "Y"
                            }
                        }
                    }
                };

                applValcodes = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "1",
                            ValActionCode1AssocMember = "1"
                        }
                    }
                };

                admissionApplicationSupportingItem = new AdmissionApplicationSupportingItem(guid, "1", "1", "1", "instance", DateTime.Today, "1")
                {
                    ReceivedDate = DateTime.Today,
                    ActionDate = DateTime.Today,
                    StatusAction = "1"
                };

                response = new UpdateApplicationSupportingItemResponse() { ApplicationId = "111d73eb-7194-4988-872d-0d425df8dfd0", CorrespondenceAssignDate = DateTime.Now, CorrespondenceInstanceName = "instance", CorrespondenceType = "type", Guid = guid, PersonId = "1" };
                guidDetails = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "MAILING", PrimaryKey = "1", SecondaryKey = "1*1*"+ (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1" } } };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), true)).ReturnsAsync(application);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Mailing>(It.IsAny<string>(), true)).ReturnsAsync(mailing);
                dataReaderMock.Setup(d => d.ReadRecordAsync<CcComments>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(comments);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Coreq>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(coreqCollection);
                //dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new string[] { guid });
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .ReturnsAsync(new Dictionary<string, RecordKeyLookupResult>()
                    {
                        {
                            "MAILING+1+1*1*" + (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString() + "*1", new RecordKeyLookupResult() { Guid = guid }
                        }
                    });
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE NE ''")).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.SelectAsync("MAILING", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(new string[]
                { "1ý1*1*" + (new DateTime(1967, 12, 31) - DateTime.Today).TotalDays.ToString() + "*1", "2ý2*2*" + (new DateTime(1967, 12, 31) - DateTime.Today).TotalDays.ToString() + "*2" });
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2" });

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidDetails);
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<UpdateApplicationSupportingItemRequest, UpdateApplicationSupportingItemResponse>(It.IsAny<UpdateApplicationSupportingItemRequest>())).ReturnsAsync(response);
            }
            #endregion

            #region POST

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Entity_Null() {
                await admissionApplicationSupportingItemsRepository.CreateAdmissionApplicationSupportingItemsAsync(null);
            }

            [TestMethod]
            
            public async Task CreateAdmissionApplicationSupportingItemsAsync()
            {
                var result =   await admissionApplicationSupportingItemsRepository.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItem);

                Assert.IsNotNull(result);

                Assert.AreEqual(result.Guid, admissionApplicationSupportingItem.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_RepositoryException()
            {
                response = new UpdateApplicationSupportingItemResponse() { ApplicationId = "111d73eb-7194-4988-872d-0d425df8dfd0", CorrespondenceAssignDate = DateTime.Now, CorrespondenceInstanceName = "instance", CorrespondenceType = "type", Guid = guid, PersonId = "1", Error=true, UpdateAdmApplSupportingItemsErrors = new List<UpdateAdmApplSupportingItemsErrors>() { new UpdateAdmApplSupportingItemsErrors() { ErrorCodes="0001", ErrorMessages="Repository Exception" } } };
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<UpdateApplicationSupportingItemRequest, UpdateApplicationSupportingItemResponse>(It.IsAny<UpdateApplicationSupportingItemRequest>())).ReturnsAsync(response);
                await admissionApplicationSupportingItemsRepository.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItem);
            }

            #endregion

            #region PUT

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Entity_Null()
            {
                await admissionApplicationSupportingItemsRepository.UpdateAdmissionApplicationSupportingItemsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Entity_Id_Null()
            {
                admissionApplicationSupportingItem = new AdmissionApplicationSupportingItem("", "1", "1", "1", "instance", DateTime.Today, "1")
                {
                    ReceivedDate = DateTime.Today,
                    ActionDate = DateTime.Today,
                    StatusAction = "1"
                };

                await admissionApplicationSupportingItemsRepository.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItem);
            }

            [TestMethod]

            public async Task UpdateAdmissionApplicationSupportingItemsAsync()
            {
                var result = await admissionApplicationSupportingItemsRepository.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItem);

                Assert.IsNotNull(result);

                Assert.AreEqual(result.Guid, admissionApplicationSupportingItem.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_RepositoryException()
            {
                response = new UpdateApplicationSupportingItemResponse() { ApplicationId = "111d73eb-7194-4988-872d-0d425df8dfd0", CorrespondenceAssignDate = DateTime.Now, CorrespondenceInstanceName = "instance", CorrespondenceType = "type", Guid = guid, PersonId = "1", Error = true, UpdateAdmApplSupportingItemsErrors = new List<UpdateAdmApplSupportingItemsErrors>() { new UpdateAdmApplSupportingItemsErrors() { ErrorCodes = "0001", ErrorMessages = "Repository Exception" } } };
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<UpdateApplicationSupportingItemRequest, UpdateApplicationSupportingItemResponse>(It.IsAny<UpdateApplicationSupportingItemRequest>())).ReturnsAsync(response);
                await admissionApplicationSupportingItemsRepository.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItem);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Create_if_No_Guid()
            {
                guidDetails = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "MAILING", PrimaryKey = "1", SecondaryKey = "" } } };

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidDetails);

                await admissionApplicationSupportingItemsRepository.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItem);
                
            }


            #endregion


        }
    }
}
