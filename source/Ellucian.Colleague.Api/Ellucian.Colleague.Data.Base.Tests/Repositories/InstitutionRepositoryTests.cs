// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
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
}
