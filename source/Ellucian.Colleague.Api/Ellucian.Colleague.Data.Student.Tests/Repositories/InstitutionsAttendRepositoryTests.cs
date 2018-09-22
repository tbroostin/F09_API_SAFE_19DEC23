using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Ellucian.Colleague.Data.Student.Transactions;
using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class InstitutionsAttendRepositoryTests_V10
    {
        [TestClass]
        public class InstitutionsAttendRepositoryTests_GETALL_GETBYID : BaseRepositorySetup
        {
            #region DECLARATIONS

            Base.DataContracts.Defaults defaults;
            ApplValcodes applValCodes;
            Collection<InstitutionsAttend> institutionsAttends;

            private InstitutionsAttendRepository institutionsAttendRepository;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                institutionsAttendRepository = new InstitutionsAttendRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                institutionsAttendRepository = null;
            }

            private void InitializeTestData()
            {
                defaults = new Defaults() { DefaultHostCorpId = "4" };
                applValCodes = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                   {
                       new ApplValcodesVals()
                       {
                           ValActionCode1AssocMember = "1",
                           ValInternalCodeAssocMember = "1"
                       }
                   }
                };
                institutionsAttends = new Collection<InstitutionsAttend>()
                {
                    new InstitutionsAttend()
                    {
                        RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                        Recordkey = "1*1",
                        DatesAttendedEntityAssociation = new List<InstitutionsAttendDatesAttended>()
                        {
                            new InstitutionsAttendDatesAttended() { InstaStartDatesAssocMember = DateTime.Today, InstaEndDatesAssocMember = DateTime.Today.AddDays(100)}
                        }
                    },
                    new InstitutionsAttend()
                    {
                        RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e875",
                        Recordkey = "2*2",
                        DatesAttendedEntityAssociation = null
                    }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(d => d.ReadRecord<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(defaults);
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES", true)).ReturnsAsync(applValCodes);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1*1", "2*2" }.ToArray<string>());
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(institutionsAttends);
                dataReaderMock.Setup(d => d.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(institutionsAttends.FirstOrDefault());
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstitutionsAttendRepository_GetInstitutionTypesAsync_Exception()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES", true)).ReturnsAsync(null);
                await institutionsAttendRepository.GetInstitutionsAttendAsync(0, 10, personId: "1");
            }

            [TestMethod]
            public async Task InstitutionsAttendRepository_GetInstitutionTypesAsync()
            {
                var result = await institutionsAttendRepository.GetInstitutionsAttendAsync(0, 10, personId: "1");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstitutionsAttendRepository_GetInstitutionAttendByIdAsync_EmptyId()
            {
                await institutionsAttendRepository.GetInstitutionAttendByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstitutionsAttendRepository_GetInstitutionAttendByIdAsync_Record_Notfound()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<InstitutionsAttend>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await institutionsAttendRepository.GetInstitutionAttendByIdAsync("1*1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionsAttendRepository_GetInstitutionAttendByIdAsync_InvalidId_Format()
            {
                await institutionsAttendRepository.GetInstitutionAttendByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionsAttendRepository_GetInstitutionAttendByIdAsync_Id_EqualTo_DefaultHostCorpId()
            {
                await institutionsAttendRepository.GetInstitutionAttendByIdAsync("1*4");
            }

            [TestMethod]
            public async Task InstitutionsAttendRepository_GetInstitutionAttendByIdAsync()
            {
                dataReaderMock.Setup(d => d.ReadRecord<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns((Defaults)null);

                var result = await institutionsAttendRepository.GetInstitutionAttendByIdAsync("1*1");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, institutionsAttends.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            public async Task InstitutionsAttendRepository_GetInsAttendGuidsCollectionAsync_Sending_Ids_Null()
            {
                var result = await institutionsAttendRepository.GetInsAttendGuidsCollectionAsync(null);

                Assert.IsTrue(result.Count == 0);
            }

            [TestMethod]
            public async Task InstitutionsAttendRepository_GetInsAttendGuidsCollectionAsync_Sending_Ids_Empty()
            {
                var result = await institutionsAttendRepository.GetInsAttendGuidsCollectionAsync(new List<string>() { });

                Assert.IsTrue(result.Count == 0);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstitutionsAttendRepository_GetInsAttendGuidsCollectionAsync_Exception()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ThrowsAsync(new Exception());
                await institutionsAttendRepository.GetInsAttendGuidsCollectionAsync(new List<string>() { "1", "2" });
            }

            [TestMethod]
            public async Task InstitutionsAttendRepository_GetInsAttendGuidsCollectionAsync()
            {
                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResult = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResult.Add("1+2", new RecordKeyLookupResult() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" });

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResult);

                var result = await institutionsAttendRepository.GetInsAttendGuidsCollectionAsync(new List<string>() { "1", "2" });

                Assert.IsNotNull(result);
                Assert.AreEqual(result.FirstOrDefault().Value, "1a49eed8-5fe7-4120-b1cf-f23266b9e874");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstitutionsAttendRepository_GetInstitutionsAttendIdFromGuidAsync_Sending_EmptyId()
            {
                await institutionsAttendRepository.GetInstitutionsAttendIdFromGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstitutionsAttendRepository_GetInstitutionsAttendIdFromGuidAsync_KeyNotFoundException()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                await institutionsAttendRepository.GetInstitutionsAttendIdFromGuidAsync("1a49eed8-5fe7-4120-b1cf-f23266b9e874");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstitutionsAttendRepository_GetInstitutionsAttendIdFromGuidAsync_KeyNotFoundException_When_Value_Null()
            {
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("1", null);

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);

                await institutionsAttendRepository.GetInstitutionsAttendIdFromGuidAsync("1a49eed8-5fe7-4120-b1cf-f23266b9e874");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task InstitutionsAttendRepository_GetInstitutionsAttendIdFromGuidAsync_Invalid_Entity_In_Result()
            {
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("1", new GuidLookupResult() { Entity = "INSTITUTIONS.ATTEND1" });

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);

                await institutionsAttendRepository.GetInstitutionsAttendIdFromGuidAsync("1a49eed8-5fe7-4120-b1cf-f23266b9e874");
            }

            [TestMethod]
            public async Task InstitutionsAttendRepository_GetInstitutionsAttendIdFromGuidAsync()
            {
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("1", new GuidLookupResult() { Entity = "INSTITUTIONS.ATTEND", PrimaryKey = "1" });

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);

                await institutionsAttendRepository.GetInstitutionsAttendIdFromGuidAsync("1a49eed8-5fe7-4120-b1cf-f23266b9e874");
            }
        }
    }
}
