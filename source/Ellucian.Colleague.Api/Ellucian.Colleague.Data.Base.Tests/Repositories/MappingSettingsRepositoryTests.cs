//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.  
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class MappingSettingsRepositoryTests
    {
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        /// <summary>
        /// Test class for MappingSettings codes
        /// </summary>
        [TestClass]
        public class MappingSettingsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<MappingSettings> _mappingSettingsCollection;
            IEnumerable<MappingSettingsOptions> _mappingSettingsOptionsCollection;
            TestMappingSettingsRepository _testMappingSettingsRepository;
            string codeItemName;
            MappingSettingsRepository mappingSettingsRepo;
            

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _testMappingSettingsRepository = new TestMappingSettingsRepository();
                _mappingSettingsCollection = _testMappingSettingsRepository.GetMappingSettingsAsync(false);
                _mappingSettingsOptionsCollection = _testMappingSettingsRepository.GetMappingSettingsOptionsAsync(false);

                // Build repository
                mappingSettingsRepo = BuildValidMappingSettingsRepository();
                codeItemName = mappingSettingsRepo.BuildFullCacheKey("  IntgMappingSettings");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _mappingSettingsCollection = null;
                mappingSettingsRepo = null;
            }

            [TestMethod]
            public async Task GetsMappingSettingsCacheAsync()
            {

                var result = await mappingSettingsRepo.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false);
                Assert.AreEqual(result.Item2, 20);
            }

            [TestMethod]
            public async Task GetsMappingSettingsNonCacheAsync()
            {
                var result = await mappingSettingsRepo.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), true);
                Assert.AreEqual(result.Item2, 20);
            }

            [TestMethod]
            public async Task GetsMappingSettingsByGuidCacheAsync()
            {
                var expected = _mappingSettingsCollection.FirstOrDefault();
                var result = await mappingSettingsRepo.GetMappingSettingsByGuidAsync(expected.Guid, false);

                Assert.AreEqual(expected.Guid, result.Guid);
                Assert.AreEqual(expected.Code, result.Code);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.EthosResource, result.EthosResource);
                Assert.AreEqual(expected.EthosPropertyName, result.EthosPropertyName);
                Assert.AreEqual(expected.Enumeration, result.Enumeration);
                Assert.AreEqual(expected.SourceTitle, result.SourceTitle);
                Assert.AreEqual(expected.SourceValue, result.SourceValue);
            }

            [TestMethod]
            public async Task GetsMappingSettingsByGuidNonCacheAsync()
            {
                var expected = _mappingSettingsCollection.FirstOrDefault();
                var result = await mappingSettingsRepo.GetMappingSettingsByGuidAsync(expected.Guid, true);

                Assert.AreEqual(expected.Guid, result.Guid);
                Assert.AreEqual(expected.Code, result.Code);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.EthosResource, result.EthosResource);
                Assert.AreEqual(expected.EthosPropertyName, result.EthosPropertyName);
                Assert.AreEqual(expected.Enumeration, result.Enumeration);
                Assert.AreEqual(expected.SourceTitle, result.SourceTitle);
                Assert.AreEqual(expected.SourceValue, result.SourceValue);
            }

            [TestMethod]
            public async Task GetsMappingSettingsOptionsCacheAync()
            {
                var result = await mappingSettingsRepo.GetMappingSettingsOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), false);

                var actuals = result.Item1.ToList();
                for (int i = 0; i < result.Item1.Count(); i++)
                {
                    var actual = actuals[i];
                    Assert.IsNotNull(actual.Guid, "Guid");
                    Assert.IsNotNull(actual.EthosPropertyName, "PropertyName");
                    Assert.IsNotNull(actual.EthosResource, "Resource");
                    Assert.AreNotEqual(actual.Enumerations.Count(), 0,  "Ethos Enumerations");                    
                }
            }

            [TestMethod]
            public async Task GetsMappingSettingsOptionsNonCacheAync()
            {
                var result = await mappingSettingsRepo.GetMappingSettingsOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), true);

                var actuals = result.Item1.ToList();
                for (int i = 0; i < result.Item1.Count(); i++)
                {
                    var actual = actuals[i];
                    Assert.IsNotNull(actual.Guid, "Guid");
                    Assert.IsNotNull(actual.EthosPropertyName, "PropertyName");
                    Assert.IsNotNull(actual.EthosResource, "Resource");
                    Assert.AreNotEqual(actual.Enumerations.Count(), 0, "Ethos Enumerations");
                }
            }

            [TestMethod]
            public async Task GetsMappingSettingsOptionsByGuidCacheAsync()
            {
                var expected = _mappingSettingsOptionsCollection.FirstOrDefault();
                var result = await mappingSettingsRepo.GetMappingSettingsOptionsByGuidAsync(expected.Guid, false);

                Assert.AreEqual(expected.Code, result.Code);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.EthosResource, result.EthosResource);
                Assert.AreEqual(expected.EthosPropertyName, result.EthosPropertyName);
                Assert.AreNotEqual(expected.Enumerations.Count(), 0, "Ethos Enumerations");
            }

            [TestMethod]
            public async Task GetsMappingSettingsOptionsByGuidNonCacheAsync()
            {
                var expected = _mappingSettingsOptionsCollection.FirstOrDefault();
                var result = await mappingSettingsRepo.GetMappingSettingsOptionsByGuidAsync(expected.Guid, true);

                Assert.AreEqual(expected.Code, result.Code);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.EthosResource, result.EthosResource);
                Assert.AreEqual(expected.EthosPropertyName, result.EthosPropertyName);
                Assert.AreNotEqual(expected.Enumerations.Count(), 0, "Ethos Enumerations");
            }

            private MappingSettingsRepository BuildValidMappingSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);                

                // Setup response to MappingSettings read
                var mappingSettingRecords = new List<IntgMappingSettings>();
                var mappingSettingRecord = new IntgMappingSettings()
                {
                    Recordkey = "1*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "CORE.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "PERSON.EMAIL.TYPES",
                    ImsEthosResource = "email-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "10*FI",
                    Recordkey = "10*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "CORE.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "VISA.TYPES",
                    ImsEthosResource = "visa-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "11*RH",
                    Recordkey = "11*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "HR.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "REHIRE.ELIGIBILITY.CODES",
                    ImsEthosResource = "employees"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "12*AF",
                    Recordkey = "12*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "HR.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "HR.STATUSES",
                    ImsEthosResource = "employees"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "13*A",
                    Recordkey = "13*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "ST.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "SECTION.STATUSES",
                    ImsEthosResource = "section-statuses"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    Recordkey = "14*AN",
                    RecordGuid = "8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "SESSIONS",
                    ImsCollFieldName = "SESS.INTG.CATEGORY",
                    ImsEthosResource = "academic-periods"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "15*001",
                    Recordkey = "15*AN",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "ROOM.TYPES",
                    ImsCollFieldName = "RMTP.INTG.ROOM.TYPE",
                    ImsEthosResource = "academic-periods"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "16*D",
                    Recordkey = "16*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "ST.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "CONTACT.MEASURES",
                    ImsEthosResource = "courses"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "17*ACG1",
                    Recordkey = "17*AN",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "AWARD.CATEGORIES",
                    ImsCollFieldName = "AC.INTG.NAME",
                    ImsEthosResource = "financial-aid-funds"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "18*ACG1",
                    Recordkey = "18*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "ST.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "ROOM.ASSIGN.STATUSES",
                    ImsEthosResource = "housing-assignments"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "19*ACG1",
                    Recordkey = "19*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "ST.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.4",
                    ImsCollValcodeId = "ROOM.ASSIGN.STATUSES",
                    ImsEthosResource = "housing-requests"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "2*AL",
                    Recordkey = "2*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "CORE.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "PHONE.TYPES",
                    ImsEthosResource = "phone-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "20*A",
                    Recordkey = "20*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "ST.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "MEAL.PLAN.STATUSES",
                    ImsEthosResource = "meal-plan-assignments"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "3*FAX",
                    Recordkey = "3*AN",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "RESTRICTIONS",
                    ImsCollFieldName = "REST.INTG.CATEGORY",
                    ImsEthosResource = "person-hold-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "4*AL",
                    Recordkey = "4*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "CORE.VALCODES",
                    ImsCollFieldName = "VAL.ACTION.CODE.3",
                    ImsCollValcodeId = "ADREL.TYPES",
                    ImsEthosResource = "address-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "5*AL",
                    Recordkey = "5*AN",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "RELATION.TYPES",
                    ImsCollFieldName = "RELTY.INTG.PERSON.REL.TYPE",
                    ImsEthosResource = "personal-relationship-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "6*M",
                    Recordkey = "6*AN",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "RELATION.TYPES",
                    ImsCollFieldName = "RELTY.INTG.FEMALE.REL.TYPE",
                    ImsEthosResource = "personal-relationship-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "7*F",
                    Recordkey = "7*AN",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "RELATION.TYPES",
                    ImsCollFieldName = "RELTY.INTG.MALE.REL.TYPE",
                    ImsEthosResource = "personal-relationship-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "8*ACT",
                    Recordkey = "8*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "CORE.VALCODES",
                    ImsCollValcodeId = "MIL.STATUSES",
                    ImsEthosResource = "veteran-statuses"
                };
                mappingSettingRecords.Add(mappingSettingRecord);
                mappingSettingRecord = new IntgMappingSettings()
                {
                    //Recordkey = "9*FB",
                    Recordkey = "9*PRI",
                    RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                    ImsCollEntity = "CORE.VALCODES",
                    ImsCollValcodeId = "SOCIAL.MEDIA.NETWORKS",
                    ImsEthosResource = "social-media-types"
                };
                mappingSettingRecords.Add(mappingSettingRecord);


                var mappingSettingContracts = new Collection<IntgMappingSettings>(mappingSettingRecords);

                dataAccessorMock.Setup(ac => ac.SelectAsync("INTG.MAPPING.SETTINGS", ""))
                    .ReturnsAsync(mappingSettingContracts.Select(ec => ec.Recordkey).ToArray());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<IntgMappingSettings>("INTG.MAPPING.SETTINGS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(mappingSettingContracts);

                var mappingInfoRecords = new List<IntgMappingInfo>();
                var mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "1",
                    ImnEthosResource = "email-types",
                    ImnEthosPropertyName = "emailType",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.EMAIL.TYPES",
                    ImnCollEntity = "CORE.VALCODES",
                    ImnCollValcodeId = "PERSON.EMAIL.TYPES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Email Types",
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "10",
                    ImnEthosResource = "visa-types",
                    ImnEthosPropertyName = "category",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.VISA.TYPES",
                    ImnCollEntity = "CORE.VALCODES",
                    ImnCollValcodeId = "VISA.TYPES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Visa Types",
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "11",
                    ImnEthosResource = "employees",
                    ImnEthosPropertyName = "rehirableStatus.eligibility",
                    ImnEthosEnumsEntity = "HR.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.REHIRE.ELIGIBILITY",
                    ImnCollEntity = "HR.VALCODES",
                    ImnCollValcodeId = "REHIRE.ELIGIBILITY.CODES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Rehire Eligibility",
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "12",
                    ImnEthosResource = "employees",
                    ImnEthosPropertyName = "contract.Type",
                    ImnEthosEnumsEntity = "HR.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.HR.STATUSES",
                    ImnCollEntity = "HR.VALCODES",
                    ImnCollValcodeId = "HR.STATUSES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Human Resource Statuses"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "13",
                    ImnEthosResource = "section-statuses",
                    ImnEthosPropertyName = "catgory",
                    ImnEthosEnumsEntity = "ST.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.SECTION.STATUSES",
                    ImnCollEntity = "ST.VALCODES",
                    ImnCollValcodeId = "SECTION.STATUSES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Section Statuses"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "14",
                    ImnEthosResource = "academic-periods",
                    ImnEthosPropertyName = "category",
                    ImnEthosEnumsEntity = "ST.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.SESS.CATEGORIES",
                    ImnCollEntity = "SESSIONS",
                    ImnCollFieldName = "SESS.INTG.CATEGORY",
                    ImnCollMappingTitle = "Sessions"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "15",
                    ImnEthosResource = "room-types",
                    ImnEthosPropertyName = "type",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.ROOM.TYPES",
                    ImnCollEntity = "ROOM.TYPES",
                    ImnCollFieldName = "RMTP.INTG.ROOM.TYPE",
                    ImnCollMappingTitle = "Room Types"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "16",
                    ImnEthosResource = "courses",
                    ImnEthosPropertyName = "hours.interval",
                    ImnEthosEnumsEntity = "ST.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.CONTACT.MEASURES",
                    ImnCollEntity = "ST.VALCODES",
                    ImnCollValcodeId = "CONTACT.MEASURES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Contact Measures",
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "17",
                    ImnEthosResource = "financial-aid-fund",
                    ImnEthosPropertyName = "category.name",
                    ImnEthosEnumsEntity = "ST.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.AWARD.CATEGORY.NAMES",
                    ImnCollEntity = "AWARD.CATEGORIES",
                    ImnCollFieldName = "AC.INTG.NAME",
                    ImnCollMappingTitle = "Award Categories"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "18",
                    ImnEthosResource = "housing-assignments",
                    ImnEthosPropertyName = "status",
                    ImnEthosEnumsEntity = "ST.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.ROOM.ASSIGN.STATUSES",
                    ImnCollEntity = "ST.VALCODES",
                    ImnCollValcodeId = "ROOM.ASSIGN.STATUSES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Room Assignment Statuses"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "19",
                    ImnEthosResource = "housing-requests",
                    ImnEthosPropertyName = "status",
                    ImnEthosEnumsEntity = "ST.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.ROOM.REQUEST.STATUSES",
                    ImnCollEntity = "ST.VALCODES",
                    ImnCollValcodeId = "ROOM.ASSIGN.STATUSES",
                    ImnCollFieldName = "VAL.ACTION.CODE.4",
                    ImnCollMappingTitle = "Room Request Statuses"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "19",
                    ImnEthosResource = "housing-requests",
                    ImnEthosPropertyName = "status",
                    ImnEthosEnumsEntity = "ST.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.ROOM.REQUEST.STATUSES",
                    ImnCollEntity = "ST.VALCODES",
                    ImnCollValcodeId = "ROOM.ASSIGN.STATUSES",
                    ImnCollFieldName = "VAL.ACTION.CODE.4",
                    ImnCollMappingTitle = "Room Request Statuses"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "2",
                    ImnEthosResource = "phone-types",
                    ImnEthosPropertyName = "phoneType",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.PHONE.TYPES",
                    ImnCollEntity = "CORE.VALCODES",
                    ImnCollValcodeId = "PHONE.TYPES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Phone Types",
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "3",
                    ImnEthosResource = "person-hold-types",
                    ImnEthosPropertyName = "category",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.RESTR.CATEGORIES",
                    ImnCollEntity = "RESTRICTIONS",
                    ImnCollFieldName = "REST.INTG.CATEGORY",
                    ImnCollMappingTitle = "Restrictions"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "4",
                    ImnEthosResource = "address-types",
                    ImnEthosPropertyName = "addressType",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.ADREL.TYPES",
                    ImnCollEntity = "CORE.VALCODES",
                    ImnCollValcodeId = "ADREL.TYPES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Address Types",
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "5",
                    ImnEthosResource = "personal-relationship-types",
                    ImnEthosPropertyName = "relationshipType",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.PERSON.RELATION.TYPES",
                    ImnCollEntity = "RELATION.TYPES",
                    ImnCollFieldName = "RELTY.INTG.PERSON.REL.TYPE",
                    ImnCollMappingTitle = "General Relation Types"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "6",
                    ImnEthosResource = "personal-relationship-types",
                    ImnEthosPropertyName = "relationshipType",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.PERSON.RELATION.TYPES",
                    ImnCollEntity = "RELATION.TYPES",
                    ImnCollFieldName = "RELTY.INTG.FEMALE.REL.TYPE",
                    ImnCollMappingTitle = "Female Specfic Relation Types"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "7",
                    ImnEthosResource = "personal-relationship-types",
                    ImnEthosPropertyName = "relationshipType",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.PERSON.RELATION.TYPES",
                    ImnCollEntity = "RELATION.TYPES",
                    ImnCollFieldName = "RELTY.INTG.MALE.REL.TYPE",
                    ImnCollMappingTitle = "Male Specfic Relation Types"
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "8",
                    ImnEthosResource = "veteran-statuses",
                    ImnEthosPropertyName = "category",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.MIL.STATUSES",
                    ImnCollEntity = "CORE.VALCODES",
                    ImnCollValcodeId = "MIL.STATUSES",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Veteran Statuses",
                };
                mappingInfoRecords.Add(mappingInfoRecord);
                mappingInfoRecord = new IntgMappingInfo()
                {
                    Recordkey = "9",
                    ImnEthosResource = "social-mediae-types",
                    ImnEthosPropertyName = "type",
                    ImnEthosEnumsEntity = "CORE.VALCODES",
                    ImnEthosEnumsValcodeId = "INTG.SOCIAL.MEDIA.NETWORKS",
                    ImnCollEntity = "CORE.VALCODES",
                    ImnCollValcodeId = "SOCIAL.MEDIA.NETWORKS",
                    ImnCollFieldName = "VAL.ACTION.CODE.3",
                    ImnCollMappingTitle = "Social Media Types",
                };
                mappingInfoRecords.Add(mappingInfoRecord);


                var mappingInfoContracts = new Collection<IntgMappingInfo>(mappingInfoRecords);

                dataAccessorMock.Setup(ac => ac.SelectAsync("INTG.MAPPING.INFO", ""))
                    .ReturnsAsync(mappingInfoContracts.Select(ec => ec.Recordkey).ToArray());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<IntgMappingInfo>("INTG.MAPPING.INFO", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(mappingInfoContracts);


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(r => r.Select(It.IsAny<GuidLookup[]>())).Returns(new Dictionary<string, GuidLookupResult>());

                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("INTG.MAPPING.SETTINGS", new GuidLookupResult() { Entity = "INTG.MAPPING.SETTINGS", PrimaryKey = "1*PRI" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                
                dataAccessorMock.Setup(ac => ac.ReadRecordAsync<IntgMappingSettings>(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(mappingSettingContracts.FirstOrDefault());
                
                dataAccessorMock.Setup(ac => ac.ReadRecordAsync<IntgMappingInfo>("INTG.MAPPING.INFO", It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(mappingInfoContracts.FirstOrDefault());

                // email-types                                
                ApplValcodes personEmailTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "PERSON.EMAIL.TYPES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.EMAIL.TYPES", 
                    It.IsAny<bool>())).ReturnsAsync(personEmailTypesResponse);

                // phone-types
                ApplValcodes phoneTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "PHONE.TYPES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES",
                    It.IsAny<bool>())).ReturnsAsync(phoneTypesResponse);                              

                // address-types
                ApplValcodes addressTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "ADREL.TYPES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES",
                    It.IsAny<bool>())).ReturnsAsync(addressTypesResponse);

                // veteran-statuses
                ApplValcodes veteranStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "MIL.STATUSES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MIL.STATUSES",
                    It.IsAny<bool>())).ReturnsAsync(veteranStatusesResponse);

                // social-media-types
                ApplValcodes socialMediaTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "SOCIAL.MEDIA.NETWORKS"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "SOCIAL.MEDIA.NETWORKS",
                    It.IsAny<bool>())).ReturnsAsync(socialMediaTypesResponse);

                // visa-types
                ApplValcodes visaTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "VISA.TYPES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "VISA.TYPES",
                    It.IsAny<bool>())).ReturnsAsync(visaTypesResponse);

                // rehire eligibility codes for employees
                ApplValcodes rehireResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("HR.VALCODES", "REHIRE.ELIGIBILITY.CODES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "REHIRE.ELIGIBILITY.CODES",
                    It.IsAny<bool>())).ReturnsAsync(rehireResponse);

                // hr statuses for employees
                ApplValcodes hrStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("HR.VALCODES", "HR.STATUSES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "HR.STATUSES",
                    It.IsAny<bool>())).ReturnsAsync(hrStatusesResponse);

                // section-statuses
                ApplValcodes sectionStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "SECTION.STATUSES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES",
                    It.IsAny<bool>())).ReturnsAsync(sectionStatusesResponse);

                // contact measures for courses
                ApplValcodes contactMeasuresResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "CONTACT.MEASURES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "CONTACT.MEASURES",
                    It.IsAny<bool>())).ReturnsAsync(contactMeasuresResponse);

                // room assign statuses for housing-assignments and housing-requests
                ApplValcodes roomAssignStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "ROOM.ASSIGN.STATUSES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ROOM.ASSIGN.STATUSES",
                    It.IsAny<bool>())).ReturnsAsync(roomAssignStatusesResponse);

                // meal-plan-assignments
                ApplValcodes mealPlansResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                         new ApplValcodesVals("", "Primary", "", "PRI", "", "personal", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "MEAL.ASSIGN.STATUSES"))
                     .ReturnsAsync(new string[] { "PRI" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "MEAL.ASSIGN.STATUSES",
                    It.IsAny<bool>())).ReturnsAsync(mealPlansResponse);

                // RESTRICTIONS for person-hold-types
                dataAccessorMock.Setup(i => i.ReadRecordAsync<Restrictions>(It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new Restrictions() { Recordkey = "AN", RestDesc = "Session", RestIntgCategory = "2" });

                dataAccessorMock.Setup(i => i.BulkReadRecordAsync<Restrictions>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new Collection<Restrictions>(new List<Restrictions>() { new Restrictions() { Recordkey = "AN", RestDesc = "Session", RestIntgCategory = "2" } }));

                // RELATION.TYPES for relation-types
                dataAccessorMock.Setup(i => i.ReadRecordAsync<RelationTypes>(It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new RelationTypes() { Recordkey = "AN", ReltyDesc = "Session", ReltyIntgPersonRelType = "1", ReltyIntgMaleRelType = "5", ReltyIntgFemaleRelType = "3"});

                dataAccessorMock.Setup(i => i.BulkReadRecordAsync<RelationTypes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new Collection<RelationTypes>(new List<RelationTypes>() { new RelationTypes() { Recordkey = "AN", ReltyDesc = "Session", ReltyIntgPersonRelType = "1", ReltyIntgMaleRelType = "5", ReltyIntgFemaleRelType = "3" } }));

                // SESSIONS for academic-periods
                dataAccessorMock.Setup(i => i.ReadRecordAsync<SessionsBase>(It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new SessionsBase() { Recordkey = "AN", SessDesc = "Session", SessIntgCategory = "2" });

                dataAccessorMock.Setup(i => i.BulkReadRecordAsync<SessionsBase>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new Collection<SessionsBase>(new List<SessionsBase>() { new SessionsBase() { Recordkey = "AN", SessDesc = "Session", SessIntgCategory = "2" } }));

                // ROOM.TYPES for room-types
                dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.RoomTypes>(It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new DataContracts.RoomTypes() { Recordkey = "AN", RmtpDescription = "Session", RmtpIntgRoomType = "2" });

                dataAccessorMock.Setup(i => i.BulkReadRecordAsync<DataContracts.RoomTypes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new Collection<DataContracts.RoomTypes>(new List<DataContracts.RoomTypes>() { new DataContracts.RoomTypes() { Recordkey = "AN", RmtpDescription = "Session", RmtpIntgRoomType = "2" } }));

                // AWARD.CATEGORIES for financial-aid-funds
                dataAccessorMock.Setup(i => i.ReadRecordAsync<AwardCategoriesBase>(It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new AwardCategoriesBase() { Recordkey = "AN", AcDescription = "Session", AcIntgName = "2" });

                dataAccessorMock.Setup(i => i.BulkReadRecordAsync<AwardCategoriesBase>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).
                    ReturnsAsync(new Collection<AwardCategoriesBase>(new List<AwardCategoriesBase>() { new AwardCategoriesBase() { Recordkey = "AN", AcDescription = "Session", AcIntgName = "2" } }));

              
                // INTG.RESTR.CATEGORIES enumeration translation table for person-hold-types
                ApplValcodes intgRestrCategoriesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "term", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.RESTR.CATEGORIES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.RESTR.CATEGORIES", It.IsAny<bool>())).
                    ReturnsAsync(intgRestrCategoriesResponse);

                // INTG.PERSON.RELATION.TYPES enumeration translation table for person-relationship-types
                ApplValcodes intgPersonRelationTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "parent", "", "1", "", "", ""),
                        new ApplValcodesVals("", "father", "", "3", "", "", ""),
                        new ApplValcodesVals("", "mother", "", "5", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.PERSON.RELATION.TYPES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.RELATION.TYPES", It.IsAny<bool>())).
                    ReturnsAsync(intgPersonRelationTypesResponse);
                
                // INTG.SESS.CATEGORIES enumeration translation table for academic-periods
                ApplValcodes intgSessCategoriesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "term", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "INTG.SESS.CATEGORIES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SESS.CATEGORIES", It.IsAny<bool>())).
                    ReturnsAsync(intgSessCategoriesResponse);

                // INTG.ROOM.TYPES enumeration translation table for room-types
                ApplValcodes intgRoomTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "term", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.ROOM.TYPES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.ROOM.TYPES", It.IsAny<bool>())).
                    ReturnsAsync(intgRoomTypesResponse);

                // INTG.AWARD.CATEGORY.NAMES enumeration translation table for financial-aid-funds
                ApplValcodes intgAwardCategoryNamesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "term", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "INTG.AWARD.CATEGORY.NAMES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.AWARD.CATEGORY.NAMES", It.IsAny<bool>())).
                    ReturnsAsync(intgAwardCategoryNamesResponse);

                dataAccessorMock.Setup(repo => repo.SelectAsync("INTG.MAPPING.SETTINGS", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2" });

                // INTG.EMAIL.TYPES enumeration translation table for email-types
                ApplValcodes intgEmailTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.EMAIL.TYPES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.EMAIL.TYPES", It.IsAny<bool>())).
                    ReturnsAsync(intgEmailTypesResponse);

                // INTG.PHONE.TYPES enumeration translation table for phone-types
                ApplValcodes intgPhoneTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.PHONE.TYPES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PHONE.TYPES", It.IsAny<bool>())).
                    ReturnsAsync(intgPhoneTypesResponse);

                // INTG.ADREL.TYPES enumeration translation table for address-types
                ApplValcodes intgAddrTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.ADREL.TYPES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.ADREL.TYPES", It.IsAny<bool>())).
                    ReturnsAsync(intgAddrTypesResponse);

                // INTG.MIL.STATUSES enumeration translation table for veteran-statuses
                ApplValcodes intgVeteranStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.MIL.STATUSES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.MIL.STATUSES", It.IsAny<bool>())).
                    ReturnsAsync(intgVeteranStatusesResponse);

                // INTG.SOCIAL.MEDIA.NETWORKS enumeration translation table for social-media-types
                ApplValcodes intgSocialMediaTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.SOCIAL.MEDIA.NETWORKS"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.SOCIAL.MEDIA.NETWORKS", It.IsAny<bool>())).
                    ReturnsAsync(intgSocialMediaTypesResponse);

                // INTG.VISA.TYPES enumeration translation table for visa-types
                ApplValcodes intgVisaTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.VISA.TYPES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.VISA.TYPES", It.IsAny<bool>())).
                    ReturnsAsync(intgVisaTypesResponse);

                // INTG.REHIRE.ELIGIBILITY enumeration translation table for employees
                ApplValcodes intgRehireResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("HR.VALCODES", "INTG.REHIRE.ELIGIBILITY"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "INTG.REHIRE.ELIGIBILITY", It.IsAny<bool>())).
                    ReturnsAsync(intgRehireResponse);


                // INTG.HR.STATUSES enumeration translation table for employees
                ApplValcodes intgHrStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("HR.VALCODES", "INTG.HR.STATUSES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "INTG.HR.STATUSES", It.IsAny<bool>())).
                    ReturnsAsync(intgHrStatusesResponse);


                // INTG.SECTION.STATUSES enumeration translation table for section-statuses
                ApplValcodes intgSectionStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "INTG.SECTION.STATUSES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SECTION.STATUSES", It.IsAny<bool>())).
                    ReturnsAsync(intgSectionStatusesResponse);


                // INTG.CONTACT.MEASURES enumeration translation table for courses
                ApplValcodes intgContactMeasuresResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "INTG.CONTACT.MEASURES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.CONTACT.MEASURES", It.IsAny<bool>())).
                    ReturnsAsync(intgContactMeasuresResponse);


                // INTG.ROOM.ASSIGN.STATUSES enumeration translation table for housing-assigments
                ApplValcodes intgRoomAssignStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "INTG.ROOM.ASSIGN.STATUSES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.ROOM.ASSIGN.STATUSES", It.IsAny<bool>())).
                    ReturnsAsync(intgRoomAssignStatusesResponse);

                // INTG.ROOM.REQUEST.STATUSES enumeration translation table for housing-requests
                ApplValcodes intgRoomRequestStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "INTG.ROOM.REQUEST.STATUSES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.ROOM.REQUEST.STATUSES", It.IsAny<bool>())).
                    ReturnsAsync(intgRoomRequestStatusesResponse);

                // INTG.MEAL.PLAN.STATUSES enumeration translation table for meal-plan-assignments
                ApplValcodes intgMealPlanStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("", "personal", "", "2", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "INTG.MEAL.PLAN.STATUSES"))
                     .ReturnsAsync(new string[] { "2" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.MEAL.PLAN.STATUSES", It.IsAny<bool>())).
                    ReturnsAsync(intgMealPlanStatusesResponse);




                var response = new Ellucian.Colleague.Domain.Base.Transactions.GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllIntgMappingSettings:",
                    Entity = "INTG.MAPPING.SETTINGS",
                    Sublist = new List<string>() { "1*PRI", "2*PRI", "3*PRI", "4*PRI", "5*PRI", "6*PRI", "7*PRI", "8*PRI", "9*PRI", "10*PRI",
                        "11*PRI", "12*PRI", "13*PRI", "14*PRI", "15*PRI", "16*PRI", "17*PRI", "18*PRI", "19*PRI", "20*PRI" },
                    TotalCount = 20,
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

                transManagerMock.Setup(acc => acc.ExecuteAsync<GetCacheApiKeysRequest,
                    GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>())).ReturnsAsync(response);

                // Construct repository
                var apiSettings = new ApiSettings("TEST");
                mappingSettingsRepo = new MappingSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return mappingSettingsRepo;
            }
        }
    }
}
