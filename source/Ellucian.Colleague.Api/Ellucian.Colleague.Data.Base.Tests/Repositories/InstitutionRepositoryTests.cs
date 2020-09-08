// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Data.Colleague;
using System.Runtime.Caching;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.DataContracts;
using slf4net;
using Ellucian.Web.Cache;
using System;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Http.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class InstitutionRepositoryTests : BaseRepositorySetup
    {
        //protected ApiSettings apiSettings;
        protected List<string> institutionIds;
        protected Dictionary<string, Person> personRecords;
        protected Dictionary<string, Address> addressRecords;
        protected Dictionary<string, Institutions> institutionRecords;
               
        protected Collection<Person> personResponseData;
        protected Collection<Address> addressResponseData;
        protected Collection<Institutions> institutionResponseData;        
        protected Defaults coreDefaultResponseData;
        protected FaSysParams faSystemParamsResponseData;
        protected InstitutionRepository institutionsRepo;

        #region Private data array setup

        private string[,] _personData = { // ID         Name                   Preferred Address
                                            {"0000044","Datatel Community College",  "44"},
                                            {"0000128","Alaska Pacific University", "128"},
                                            {"0000129","Amarillo College",          "129"},
                                            {"0000130","Aquinas College",           "130"},
                                            {"0000131","Bacone College",            "131"},
                                            {"0000132","Bismark State College",     "132"},
                                            {"0000133","Bryant College",            "133"},
                                            {"0000134","Burlington College",        "134"},
                                            {"0000135","Butler University",         "135"},
                                            {"0000136","Carleton College",          "136"},
                                            {"0000137","Carnegie Mellon University","137"},
                                            {"0000138","Carroll College",           "138"},
                                            {"0000139","Chaffey College",           "139"},
                                            {"0000140","Chaminade University",      "140"},
                                            {"0000141","Clark College",             "141"},
                                   };

        private string[,] _addressData = { // ID   City           State
                                            {"44","Fairfax",      "VA"},
                                            {"128","Anchorage",   "AK"},
                                            {"129","Amarillo",    "TX"},
                                            {"130","Nashville",   "TN"},
                                            {"131","Muskogee",    "OK"},
                                            {"132","Bismark",     "ND"},
                                            {"133","Smithfield",  "RI"},
                                            {"134","Burlington",  "VT"},
                                            {"135","Indianapolis","IN"},
                                            {"136","Northfield",  "MN"},
                                            {"137","Pittsburgh",  "PA"},
                                            {"138","Helena",      "MT"},
                                            {"139","Dover",       "DE"},
                                            {"140","Honolulu",    "HI"},
                                            {"141","Vancouver",   "WA"}
                                   };

        private string[,] _institutionData = { //  ID     Type
                                                {"0000044","C"},
                                                {"0000128","C"},
                                                {"0000129","C"},
                                                {"0000130","C"},
                                                {"0000131","C"},
                                                {"0000132","C"},
                                                {"0000133","C"},
                                                {"0000134","C"},
                                                {"0000135","C"},
                                                {"0000136","C"},
                                                {"0000137","C"},
                                                {"0000138","C"},
                                                {"0000139","C"},
                                                {"0000140","C"},
                                                {"0000141","C"}
                                   };

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            personRecords = SetupPersons();
            addressRecords = SetupAddresses();
            institutionRecords = SetupInstitutions(out institutionIds);
            coreDefaultResponseData = new Defaults()
            {
                DefaultHostCorpId = "0000044"
            };
            faSystemParamsResponseData = new FaSysParams()
            {
                FspInstitutionName = "Datatel Community College FA"
            };
            institutionsRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            personRecords = null;
            addressRecords = null;
            institutionRecords = null;
            institutionIds = null;
            institutionsRepo = null;
        }

        [TestMethod]
        public void Get_ReturnsAllInstitutions()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            Assert.AreEqual(institutionResponseData.Count(), institutions.Count());
        }

        [TestMethod]
        public void Get_Id()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.ElementAt(0);
            Assert.AreEqual("0000044", institution.Id);
        }

        [TestMethod]
        public void Get_Type()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.ElementAt(0);
            Assert.AreEqual(Ellucian.Colleague.Domain.Base.Entities.InstType.College, institution.InstitutionType);
        }

        [TestMethod]
        public void Get_Name()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.ElementAt(0);
            Assert.AreEqual("Datatel Community College", institution.Name);
        }

        [TestMethod]
        public void Get_City()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.ElementAt(0);
            Assert.AreEqual("Fairfax", institution.City);
        }

        [TestMethod]
        public void Get_State()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.ElementAt(0);
            Assert.AreEqual("VA", institution.State);
        }

        /// <summary>
        /// Tests if IsHostInstitution flag is set to true when
        /// the institution id matches the one in the coreDefaults record
        /// </summary>
        [TestMethod]
        public void Get_IsHostInstitution()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.ElementAt(0);
            Assert.IsTrue(institution.IsHostInstitution);
        }
        /// <summary>
        /// Tests if the IsHostInstitution property i set to false when the institution id
        /// does not match the host institution id
        /// </summary>
        [TestMethod]
        public void NotIsHostInstitutionTest()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.FirstOrDefault(i => i.Id != coreDefaultResponseData.DefaultHostCorpId);
            Assert.IsFalse(institution.IsHostInstitution);
        }
        /// <summary>
        /// Tests if the FinancialAidInstitutionName for the host
        /// institution equals the name on the faSysParam record
        /// </summary>
        [TestMethod]
        public void Get_FinancialAidInstitutionName()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.FirstOrDefault(i => i.Id == coreDefaultResponseData.DefaultHostCorpId);
            Assert.AreEqual(faSystemParamsResponseData.FspInstitutionName, institution.FinancialAidInstitutionName);
        }
        /// <summary>
        /// Tests if the FinancialAidInstitutionName property equals null
        /// if the institution is not host
        /// </summary>
        [TestMethod]
        public void NullFinancialAidInstitutionNameTest()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Institution> institutions = institutionsRepo.Get();
            var institution = institutions.FirstOrDefault(i => i.Id != coreDefaultResponseData.DefaultHostCorpId);
            Assert.IsNull(institution.FinancialAidInstitutionName);
        }

        /// <summary>
        /// Tests if correct message logged if coreDefaults record
        /// is null
        /// </summary>
        [TestMethod]
        public void NullCoreDefaultRecord_MessageLoggedTest()
        {
            coreDefaultResponseData = null;
            BuildValidRepository();
            institutionsRepo.Get();
            loggerMock.Verify(l => l.Info("Unable to access DEFAULTS from CORE.PARMS table."));
        }
        /// <summary>
        /// Tests if an appropriate message is logged when faSysParam record
        /// is null
        /// </summary>
        [TestMethod]
        public void NullFaSysParamsRecord_MessageLoggedTest()
        {
            faSystemParamsResponseData = null;
            BuildValidRepository();
            institutionsRepo.Get();
            loggerMock.Verify(l => l.Info("Unable to read FA.SYS.PARAMS from database"));
        }

        private InstitutionRepository BuildValidRepository()
        {
            MockInitialize();
            //var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            //loggerMock = new Mock<ILogger>();

            // Cache mocking
            //var cacheProviderMock = new Mock<ICacheProvider>();
            //var localCacheMock = new Mock<ObjectCache>();
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            // Set up data accessor for mocking 
            //var dataAccessorMock = new Mock<IColleagueDataReader>();
            //transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Mock the call to get INST.TYPES validation table
            var valcodeData = BuildValcodeResponse();
            dataReaderMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "INST.TYPES", true)).Returns(valcodeData);

            // Mock the call for getting the Institutions Records
            institutionResponseData = BuildInstitutionsResponse(institutionRecords);
            dataReaderMock.Setup<string[]>(acc => acc.Select("INSTITUTIONS", "WITH INST.TYPE NE ''")).Returns(institutionIds.ToArray());
            dataReaderMock.Setup<ICollection<Institutions>>(acc => acc.BulkReadRecord<Institutions>("INSTITUTIONS", It.IsAny<string[]>(), true)).Returns(institutionResponseData);

            // Mock the call for getting the person records
            personResponseData = BuildPersonResponse(personRecords);
            dataReaderMock.Setup<ICollection<Person>>(acc => acc.BulkReadRecord<Person>("PERSON",It.IsAny<string[]>(), true)).Returns(personResponseData);

            // Mock the call for getting the address records
            addressResponseData = BuildAddressResponse(addressRecords);
            dataReaderMock.Setup<ICollection<Address>>(acc => acc.BulkReadRecord<Address>("ADDRESS", It.IsAny<string[]>(), true)).Returns(addressResponseData);

            dataReaderMock.Setup<Defaults>(acc => acc.ReadRecord<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(coreDefaultResponseData);
            dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);

            // Construct institution repository
            institutionsRepo = new InstitutionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            
            return institutionsRepo;
        }
        private Dictionary<string, Person> SetupPersons()
        {
            string[,] recordData = _personData;

            int personCount = recordData.Length / 3;
            Dictionary<string, Person> records = new Dictionary<string, Person>();
            for (int i = 0; i < personCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string preferredName = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string preferredAddress = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd();
                
                DataContracts.Person record = new DataContracts.Person();
                record.Recordkey = id;
                record.LastName = preferredName;
                record.PreferredAddress = preferredAddress;

                records.Add(id, record);
            }
            return records;
        }

        private Collection<Person> BuildPersonResponse(Dictionary<string, Person> personRecords)
        {
            Collection<Person> personContracts = new Collection<Person>();
            foreach (var personItem in personRecords)
            {
                personContracts.Add(personItem.Value);
            }
            return personContracts;
        }

        private Dictionary<string, Address> SetupAddresses()
        {
            string[,] recordData = _addressData;

            int recordCount = recordData.Length / 3;
            Dictionary<string, Address> results = new Dictionary<string, Address>();
            for (int i = 0; i < recordCount; i++)
            {
                Address response = new Address();
                string addressId = recordData[i, 0].TrimEnd();
                string city = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string state = (recordData[i, 2] == null) ? String.Empty : recordData[i, 2].TrimEnd();

                response.Recordkey = addressId;
                response.City = city;
                response.State = state;

                results.Add(addressId, response);
            }
            return results;
        }

        private Collection<Address> BuildAddressResponse(Dictionary<string, Address> addressRecords)
        {
            Collection<Address> addressContracts = new Collection<Address>();
            foreach (var addressItem in addressRecords)
            {
                addressContracts.Add(addressItem.Value);
            }
            return addressContracts;
        }
        private Dictionary<string, Institutions> SetupInstitutions(out List<string> Ids)
        {
            Ids = new List<string>();
            string[,] recordData = _institutionData;

            int institutionCount = recordData.Length / 2;
            Dictionary<string, Institutions> records = new Dictionary<string, Institutions>();
            for (int i = 0; i < institutionCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string type = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();

                DataContracts.Institutions record = new DataContracts.Institutions();
                record.Recordkey = id;
                record.InstType = type;

                Ids.Add(id);
                records.Add(id, record);
            }
            return records;
        }

        private Collection<Institutions> BuildInstitutionsResponse(Dictionary<string, Institutions> institutionRecords)
        {
            Collection<Institutions> institutionContracts = new Collection<Institutions>();
            foreach (var institution in institutionRecords)
            {
                institutionContracts.Add(institution.Value);
            }
            return institutionContracts;
        }

        private ApplValcodes BuildValcodeResponse()
        {
            ApplValcodes instTypesResponse = new ApplValcodes();
            instTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            instTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("C", "College", "C", "C", "", "", ""));
            instTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("H", "High School", "H", "H", "", "", ""));

            return instTypesResponse;
        }
    }

    [TestClass]
    public class InstitutionRepositoryTests_EducationalInstitutions : BaseRepositorySetup
    {
        protected List<string> institutionIds;
        protected Dictionary<string, Person> personRecords;
        protected Dictionary<string, PersonIntg> personIntgRecords;
        protected Dictionary<string, Address> addressRecords;
        protected Dictionary<string, Institutions> institutionRecords;
        protected List<string> socialMediaHandleIds;
        protected Dictionary<string, SocialMediaHandles> socialMediaHandleRecords;

        protected Collection<Person> personResponseData;
        protected Collection<PersonIntg> personIntgResponseData;
        protected Collection<Address> addressResponseData;
        protected Collection<Institutions> institutionResponseData;
        protected Collection<SocialMediaHandles> socialMediaHandlesResponseData;
        protected Defaults coreDefaultResponseData;
        protected FaSysParams faSystemParamsResponseData;
        protected InstitutionRepository institutionsRepo;

        #region Private data array setup

        private string[,] _personData = { // ID         Name                         Preferred Address  Person Addresses  Person Corp Indicator
                                            {"0000044","Datatel Community College",  "44",              "44",             "Y"},
                                            {"0000128","Alaska Pacific University", "128",              "128",            "Y"},
                                            {"0000129","Amarillo College",          "129",              "129",            "Y"},
                                            {"0000130","Aquinas College",           "130",              "130",            "Y"},
                                            {"0000131","Bacone College",            "131",              "131",            "Y"},
                                            {"0000132","Bismark State College",     "132",              "132",            "Y"},
                                            {"0000133","Bryant College",            "133",              "133",            "Y"},
                                            {"0000134","Burlington College",        "134",              "134",            "Y"},
                                            {"0000135","Butler University",         "135",              "135",            "Y"},
                                            {"0000136","Carleton College",          "136",              "136",            "Y"},
                                            {"0000137","Carnegie Mellon University","137",              "137",            "Y"},
                                            {"0000138","Carroll College",           "138",              "138",            "Y"},
                                            {"0000139","Chaffey College",           "139",              "139",            "Y"},
                                            {"0000140","Chaminade University",      "140",              "140",            "Y"},
                                            {"0000141","Clark College",             "141",              "141",            "Y"},
                                   };

        private string[,] _addressData = { // ID   City           State Phone Number        Preferred     Country Calling Code
                                            {"44","Fairfax",      "VA", "703-333-4444"    , "Y"           , "02"},
                                            {"128","Anchorage",   "AK", "703-333-4441"    , "Y"           , null},
                                            {"129","Amarillo",    "TX", "703-333-4442"    , "Y"           , null},
                                            {"130","Nashville",   "TN", "703-333-4443"    , "Y"           , null},
                                            {"131","Muskogee",    "OK", "703-333-4445"    , "Y"           , null},
                                            {"132","Bismark",     "ND", "703-333-4446"    , "Y"           , "01"},
                                            {"133","Smithfield",  "RI", "703-333-4447"    , "Y"           , null},
                                            {"134","Burlington",  "VT", "703-333-4448"    , "Y"           , null},
                                            {"135","Indianapolis","IN", "703-333-4449"    , "Y"           , null},
                                            {"136","Northfield",  "MN", "703-333-4440"    , "Y"           , null},
                                            {"137","Pittsburgh",  "PA", "703-333-4414"    , "Y"           , null},
                                            {"138","Helena",      "MT", "703-333-4424"    , "Y"           , null},
                                            {"139","Dover",       "DE", "703-333-4434"    , "Y"           , null},
                                            {"140","Honolulu",    "HI", "703-333-4454"    , "Y"           , null},
                                            {"141","Vancouver",   "WA", "703-333-4464"    , "Y"           , null},
                                   };

        private string[,] _personIntgData = {//ID             Phone number        Preferred       Country Calling Code
                                            {"0000044"        , "703-333-4444"    , "Y"           , "02"},
                                            {"0000128"        , "703-333-4441"    , "Y"           , null},
                                            {"0000129"        , "703-333-4442"    , "Y"           , null},
                                            {"0000130"        , "703-333-4443"    , "Y"           , null},
                                            {"0000131"        , "703-333-4445"    , "Y"           , null},
                                            {"0000132"        , "703-333-4446"    , "Y"           , "01"},
                                            {"0000133"        , "703-333-4447"    , "Y"           , null},
                                            {"0000134"        , "703-333-4448"    , "Y"           , null},
                                            {"0000135"        , "703-333-4449"    , "Y"           , null},
                                            {"0000136"        , "703-333-4440"    , "Y"           , null},
                                            {"0000137"        , "703-333-4414"    , "Y"           , null},
                                            {"0000138"        , "703-333-4424"    , "Y"           , null},
                                            {"0000139"        , "703-333-4434"    , "Y"           , null},
                                            {"0000140"        , "703-333-4454"    , "Y"           , null},
                                            {"0000141"        , "703-333-4464"    , "Y"           , null},
                                   };


        private string[,] _institutionData = { //  ID     Type
                                                {"0000044","C"},
                                                {"0000128","C"},
                                                {"0000129","C"},
                                                {"0000130","C"},
                                                {"0000131","C"},
                                                {"0000132","C"},
                                                {"0000133","C"},
                                                {"0000134","C"},
                                                {"0000135","C"},
                                                {"0000136","C"},
                                                {"0000137","C"},
                                                {"0000138","C"},
                                                {"0000139","C"},
                                                {"0000140","C"},
                                                {"0000141","C"}
                                   };
        

        private string[,] _socialMediaHandles = { //  ID     Handle                  Network            Preferred
                                            {"0000044","datatel_community_college",  "FB",              "Y"},
                                            {"0000128","alaska_pacific_university",  "AOL",             "N"},
                                            {"0000129","amarillo_college",           "FB",              "Y"},
                                            {"0000130","aquinas_college",            "FB",              "Y"},
                                            {"0000131","bacone_college",             "FB",              "Y"},
                                            {"0000132","bismark_state_college",      "FB",              "Y"},
                                            {"0000133","bryant_college",             "FB",              "Y"},
                                            {"0000134","burlington_college",         "FB",              "Y"},
                                            {"0000135","butler_university",          "FB",              "Y"},
                                            {"0000136","carleton_college",           "FB",              "Y"},
                                            {"0000137","carnegie_mellon_university", "FB",              "Y"},
                                            {"0000138","carroll_college",            "FB",              "Y"},
                                            {"0000139","chaffey_college",            "FB",              "Y"},
                                            {"0000140","chaminade_university",       "FB",              "Y"},
                                            {"0000141","clark_college",              "FB",              "Y"},
                                   };
        #endregion

        [TestInitialize]
        public void Initialize()
        {
            personRecords = SetupPersons();
            personIntgRecords = SetupPersonIntg();
            addressRecords = SetupAddresses();
            institutionRecords = SetupInstitutions(out institutionIds);
            socialMediaHandleRecords = SetupSocialMediaHandles(out socialMediaHandleIds);
            coreDefaultResponseData = new Defaults()
            {
                DefaultHostCorpId = "0000044"
            };
            faSystemParamsResponseData = new FaSysParams()
            {
                FspInstitutionName = "Datatel Community College FA"
            };
            institutionsRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            personRecords = null;
            personIntgRecords = null;
            addressRecords = null;
            institutionRecords = null;
            socialMediaHandleRecords = null;
            institutionIds = null;
            institutionsRepo = null;
        }
        
        [TestMethod]
        public async Task InstitutionRepository_GetInstitutionAsync()
        {
            var results = await institutionsRepo.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Domain.Base.Entities.InstType>());
            Assert.AreEqual(15, results.Item2);                
        }

        [TestMethod]
        public async Task InstitutionRepository_GetInstitutionByGuidAsync()
        {
            var guid = "8accdb4e-6f00-49b5-9710-7e2acc427482";
            Dictionary<string, GuidLookupResult> guidLookUpResults = new Dictionary<string, GuidLookupResult>();
            guidLookUpResults.Add("A", new GuidLookupResult() { Entity = "PERSON", PrimaryKey = "0000044" });
            dataReaderMock.Setup(reader => reader.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookUpResults);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<Institutions>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionResponseData.FirstOrDefault());
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<Person>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personResponseData.FirstOrDefault());
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<PersonIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntgResponseData.FirstOrDefault());

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Address>(It.IsAny<string[]>(), true)).ReturnsAsync(addressResponseData);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<SocialMediaHandles>("SOCIAL.MEDIA.HANDLES", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(socialMediaHandlesResponseData);

            var result = await institutionsRepo.GetInstitutionByGuidAsync(guid);
            Assert.IsNotNull(result);
        }

        private InstitutionRepository BuildValidRepository()
        {
            MockInitialize();

            // Mock the call to get INST.TYPES validation table
            var valcodeData = BuildValcodeResponse();
            dataReaderMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "INST.TYPES", true)).Returns(valcodeData);

            // Mock the call for getting the Institutions Records
            institutionResponseData = BuildInstitutionsResponse(institutionRecords);
            dataReaderMock.Setup<string[]>(acc => acc.Select("INSTITUTIONS", "WITH INST.TYPE NE ''")).Returns(institutionIds.ToArray());
            dataReaderMock.Setup<ICollection<Institutions>>(acc => acc.BulkReadRecord<Institutions>("INSTITUTIONS", It.IsAny<string[]>(), true)).Returns(institutionResponseData);

            // Mock the call for getting the person records
            personResponseData = BuildPersonResponse(personRecords);
            dataReaderMock.Setup<ICollection<Person>>(acc => acc.BulkReadRecord<Person>("PERSON", It.IsAny<string[]>(), true)).Returns(personResponseData);

            // Mock the call for getting the person intg records
            personIntgResponseData = BuildPersonIntgResponse(personIntgRecords);
            dataReaderMock.Setup<ICollection<PersonIntg>>(acc => acc.BulkReadRecord<PersonIntg>("PERSON.INTG", It.IsAny<string[]>(), true)).Returns(personIntgResponseData);

            // Mock the call for getting the address records
            addressResponseData = BuildAddressResponse(addressRecords);
            dataReaderMock.Setup<ICollection<Address>>(acc => acc.BulkReadRecord<Address>("ADDRESS", It.IsAny<string[]>(), true)).Returns(addressResponseData);

            // Mock the call for getting the social medial handle records
            socialMediaHandlesResponseData = BuildSocialMediaHandlesResponse(socialMediaHandleRecords);
            dataReaderMock.Setup<string[]>(acc => acc.Select("SOCIAL.MEDIA.HANDLES", It.IsAny<string>())).Returns(socialMediaHandleIds.ToArray());
            dataReaderMock.Setup<ICollection<SocialMediaHandles>>(acc => acc.BulkReadRecord<SocialMediaHandles>("SOCIAL.MEDIA.HANDLES", It.IsAny<string[]>(), true)).Returns(socialMediaHandlesResponseData);

            dataReaderMock.Setup<Defaults>(acc => acc.ReadRecord<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(coreDefaultResponseData);
            dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);

            dataReaderMock.Setup(acc => acc.SelectAsync("INSTITUTIONS", It.IsAny<string>())).ReturnsAsync(institutionIds.ToArray());
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Institutions>("INSTITUTIONS", It.IsAny<string[]>(), true)).ReturnsAsync(institutionResponseData);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(personResponseData);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<PersonIntg>("PERSON.INTG", It.IsAny<string[]>(), true)).ReturnsAsync(personIntgResponseData);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Address>("ADDRESS", It.IsAny<string[]>(), true)).ReturnsAsync(addressResponseData);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<SocialMediaHandles>("SOCIAL.MEDIA.HANDLES", It.IsAny<string[]>(), true)).ReturnsAsync(socialMediaHandlesResponseData);


            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            Mock<IColleagueTransactionInvoker> mockManager = new Mock<IColleagueTransactionInvoker>();

            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
            var resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 2,
                CacheName = "AllInstitutions",
                Entity = "INSTITUTIONS",
                Sublist = institutionIds,
                TotalCount = institutionIds.Count(),
                KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
            };
            mockManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(resp);


            // Construct institution repository
            institutionsRepo = new InstitutionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            return institutionsRepo;
        }
        private Dictionary<string, Person> SetupPersons()
        {
            string[,] recordData = _personData;

            int personCount = recordData.Length / 5;
            Dictionary<string, Person> records = new Dictionary<string, Person>();
            for (int i = 0; i < personCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string preferredName = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string preferredAddress = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd();
                string address = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd();
                string corpIndicator = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                var personAddresses = new List<string>();
                if (!string.IsNullOrEmpty(address))
                {
                    personAddresses.Add(address);
                }                

                DataContracts.Person record = new DataContracts.Person();
                record.Recordkey = id;
                record.LastName = preferredName;
                record.PreferredAddress = preferredAddress;
                record.PersonAddresses = personAddresses;
                var addressObj = new PersonPseason()
                {
                    PersonAddressesAssocMember = address
                };
                var pseason = new List<PersonPseason> { addressObj };
                record.PseasonEntityAssociation = pseason;
                record.PersonCorpIndicator = corpIndicator;
                records.Add(id, record);
            }
            return records;
        }

        private Collection<Person> BuildPersonResponse(Dictionary<string, Person> personRecords)
        {
            Collection<Person> personContracts = new Collection<Person>();
            foreach (var personItem in personRecords)
            {
                personContracts.Add(personItem.Value);
            }
            return personContracts;
        }

        private Dictionary<string, PersonIntg> SetupPersonIntg()
        {
            string[,] recordData = _personIntgData;

            int personCount = recordData.Length / 4;
            Dictionary<string, PersonIntg> records = new Dictionary<string, PersonIntg>();
            for (int i = 0; i < personCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string phoneNumber = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string preferredFlag = (recordData[i, 2] == null) ? String.Empty : recordData[i, 2].TrimEnd();            
                string countryCode = (recordData[i, 3] == null) ? String.Empty : recordData[i, 3].TrimEnd();
                List<string> phoneNumbers = new List<string>() { phoneNumber };
                List<string> preferredFlags = new List<string>() { preferredFlag };
                List<string> countryCodes = new List<string>() { countryCode };
                    

                DataContracts.PersonIntg record = new DataContracts.PersonIntg();
                record.Recordkey = id;
                record.PerIntgPhoneNumber = phoneNumbers;
                record.PerIntgPhonePref = preferredFlags;
                record.PerIntgCtryCallingCode = countryCodes;
                
                var phoneObj = new PersonIntgPerIntgPhones()
                {
                    PerIntgPhoneNumberAssocMember = phoneNumber,
                    PerIntgCtryCallingCodeAssocMember = countryCode,
                    PerIntgPhonePrefAssocMember = preferredFlag
                };
                var phones = new List<PersonIntgPerIntgPhones> { phoneObj };
                record.PerIntgPhonesEntityAssociation = phones;

                records.Add(id, record);
            }
            return records;
        }

        private Collection<PersonIntg> BuildPersonIntgResponse(Dictionary<string, PersonIntg> personIntgRecords)
        {
            Collection<PersonIntg> personIntgContracts = new Collection<PersonIntg>();
            foreach (var personIntgItem in personIntgRecords)
            {
                personIntgContracts.Add(personIntgItem.Value);
            }
            return personIntgContracts;
        }

        private Dictionary<string, Address> SetupAddresses()
        {
            string[,] recordData = _addressData;

            int recordCount = recordData.Length / 6;
            Dictionary<string, Address> results = new Dictionary<string, Address>();
            for (int i = 0; i < recordCount; i++)
            {
                Address response = new Address();
                string addressId = recordData[i, 0].TrimEnd();
                string city = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string state = (recordData[i, 2] == null) ? String.Empty : recordData[i, 2].TrimEnd();
                string phone = (recordData[i, 3] == null) ? String.Empty : recordData[i, 3].TrimEnd();
                string preferred = (recordData[i, 4] == null) ? String.Empty : recordData[i, 4].TrimEnd();
                string countryCode = (recordData[i, 5] == null) ? String.Empty : recordData[i, 5].TrimEnd();

                response.Recordkey = addressId;
                response.City = city;
                response.State = state;
                var phoneObj = new AddressAdrPhones() { AddressPhonesAssocMember = phone};
                var phones = new List<AddressAdrPhones> {phoneObj};
                response.AdrPhonesEntityAssociation = phones;

                results.Add(addressId, response);
            }
            return results;
        }

        private Collection<Address> BuildAddressResponse(Dictionary<string, Address> addressRecords)
        {
            Collection<Address> addressContracts = new Collection<Address>();
            foreach (var addressItem in addressRecords)
            {
                addressContracts.Add(addressItem.Value);
            }
            return addressContracts;
        }

        private Dictionary<string, SocialMediaHandles> SetupSocialMediaHandles(out List<string> Ids)
        {
            Ids = new List<string>();
            string[,] recordData = _socialMediaHandles;

            int recordCount = recordData.Length / 4;
            Dictionary<string, SocialMediaHandles> results = new Dictionary<string, SocialMediaHandles>();
            for (int i = 0; i < recordCount; i++)
            {
                SocialMediaHandles response = new SocialMediaHandles();
                string socialMediaHandleId = recordData[i, 0].TrimEnd();
                string handle = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string network = (recordData[i, 2] == null) ? String.Empty : recordData[i, 2].TrimEnd();
                string preferred = (recordData[i, 3] == null) ? String.Empty : recordData[i, 3].TrimEnd();

                response.Recordkey = socialMediaHandleId;
                response.SmhPersonId = socialMediaHandleId;
                response.SmhHandle = handle;
                response.SmhNetwork = network;
                response.SmhPreferred = preferred;
                results.Add(socialMediaHandleId, response);

                Ids.Add(socialMediaHandleId);
            }
            return results;
        }

        private Collection<SocialMediaHandles> BuildSocialMediaHandlesResponse(Dictionary<string, SocialMediaHandles> socialMediaHandlesRecords)
        {
            Collection<SocialMediaHandles> socialMediaHandlesContracts = new Collection<SocialMediaHandles>();
            foreach (var socialMediaHandlesItem in socialMediaHandlesRecords)
            {
                socialMediaHandlesContracts.Add(socialMediaHandlesItem.Value);
            }
            return socialMediaHandlesContracts;
        }

        private Dictionary<string, Institutions> SetupInstitutions(out List<string> Ids)
        {
            Ids = new List<string>();
            string[,] recordData = _institutionData;

            int institutionCount = recordData.Length / 2;
            Dictionary<string, Institutions> records = new Dictionary<string, Institutions>();
            for (int i = 0; i < institutionCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string type = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();

                DataContracts.Institutions record = new DataContracts.Institutions();
                record.Recordkey = id;
                record.InstType = type;

                Ids.Add(id);
                records.Add(id, record);
            }
            return records;
        }

        private Collection<Institutions> BuildInstitutionsResponse(Dictionary<string, Institutions> institutionRecords)
        {
            Collection<Institutions> institutionContracts = new Collection<Institutions>();
            foreach (var institution in institutionRecords)
            {
                institutionContracts.Add(institution.Value);
            }
            return institutionContracts;
        }

        private ApplValcodes BuildValcodeResponse()
        {
            ApplValcodes instTypesResponse = new ApplValcodes();
            instTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            instTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("C", "College", "C", "C", "", "", ""));
            instTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("H", "High School", "H", "H", "", "", ""));

            return instTypesResponse;
        }
    }
}
