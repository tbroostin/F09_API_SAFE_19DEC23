using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
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
    public class AcademicCredentialsRepositoryTests_V11
    {
        [TestClass]
        public class AcademicCredentialsRepositoryTests_GETALL_GETBYID : BaseRepositorySetup
        {
            #region DECLARATIONS

            Defaults defaults;
            ApplValcodes applValCodes;
            Collection<AcadCredentials> acadCredentials;

            private AcademicCredentialsRepository academicCredentialsRepository;

            private Dictionary<string, GuidLookupResult> guidLookupResult;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            private string[] acadCredIds;
            private string[] acadCredGuids;


            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                academicCredentialsRepository = new AcademicCredentialsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                academicCredentialsRepository = null;
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

                acadCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                        Recordkey = "1*1",
                    },
                    new AcadCredentials()
                    {
                        RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e875",
                        Recordkey = "2*2",
                    }
                };

                guidLookupResult = new Dictionary<string, GuidLookupResult>()
                {
                    { "1", new GuidLookupResult() { Entity = "ACAD.CREDENTIALS", PrimaryKey = "1" } }
                };

                acadCredIds = new string[2] { "1*1", "2*2"};
                acadCredGuids = new string[2] { "1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1a49eed8-5fe7-4120-b1cf-f23266b9e875" };

            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(d => d.ReadRecord<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(defaults);

                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES", true)).ReturnsAsync(applValCodes);
                dataReaderMock.Setup(d => d.ReadRecordAsync<AcadCredentials>(It.IsAny<string>(), true)).ReturnsAsync(acadCredentials.FirstOrDefault());


                dataReaderMock.Setup(d => d.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string>())).ReturnsAsync(acadCredIds);
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(acadCredGuids);
                dataReaderMock.Setup(d => d.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(acadCredIds);
                dataReaderMock.Setup(d => d.SelectAsync("INSTITUTIONS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
                //dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1*1", "2*2" }.ToArray<string>());
                //dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
                //dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);


                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadCredentials>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(acadCredentials);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsAsync_InstitutionIds_Null()
            {
                dataReaderMock.Setup(d => d.ReadRecord<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns((Defaults)null);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>())).ReturnsAsync(null);
                await academicCredentialsRepository.GetAcademicCredentialsAsync(0, 10);
            }

            [TestMethod]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsAsync()
            {
                var result = await academicCredentialsRepository.GetAcademicCredentialsAsync(0, 2);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 2);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AcademicCredentialsRepository_GetInstitutionTypesAsync_Returns_Null()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES", true)).ReturnsAsync(null);
                await academicCredentialsRepository.GetAcademicCredentialsAsync(0, 2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsIdFromGuidAsync_Guid_NullOrEmpty()
            {
                await academicCredentialsRepository.GetAcademicCredentialsIdFromGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsIdFromGuidAsync_Null_Result_For_Guid()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                await academicCredentialsRepository.GetAcademicCredentialsIdFromGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsIdFromGuidAsync_Empty_Result_For_Guid()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { });
                await academicCredentialsRepository.GetAcademicCredentialsIdFromGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsIdFromGuidAsync_Result_Value_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", null } });
                await academicCredentialsRepository.GetAcademicCredentialsIdFromGuidAsync(guid);
            }

            [TestMethod]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsIdFromGuidAsync()
            {
                var ldmGuid = new LdmGuid()
                {                
                    LdmGuidEntity = "ACAD.CREDENTIALS",
                    LdmGuidSecondaryFld = "ACAD.INTG.KEY.IDX",
                    LdmGuidPrimaryKey = "1"
                };
                
                dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(ldmGuid);
                var result = await academicCredentialsRepository.GetAcademicCredentialsIdFromGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]    
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsByIdAsync_Sending_Id_As_Null()
            {
                await academicCredentialsRepository.GetAcademicCredentialsByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsByIdAsync_Record_NotFound_For_Id()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<AcadCredentials>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await academicCredentialsRepository.GetAcademicCredentialsByIdAsync(guid);
            }

            [TestMethod]
            public async Task AcademicCredentialsRepository_GetAcademicCredentialsByIdAsync()
            {
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new string[1]{guid});

                var result = await academicCredentialsRepository.GetAcademicCredentialsByIdAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, acadCredentials.FirstOrDefault().RecordGuid);
            }
        }
    }
}
