// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.HumanResources.Base.Tests;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class HumanResourcesReferenceDataRepositoryTests
    {

        /// <summary>
        /// Test class for Instructional Methods
        /// </summary>
        [TestClass]
        public class AssignmentContractTypeUnitsTests : BaseRepositorySetup
        {
            HumanResourcesReferenceDataRepository hRRDR;
            Collection<Ellucian.Colleague.Data.HumanResources.DataContracts.AsgmtContractTypes> _assignmentContractTypesCollection;
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                _assignmentContractTypesCollection = new Collection<Ellucian.Colleague.Data.HumanResources.DataContracts.AsgmtContractTypes>()
                {
                    new Ellucian.Colleague.Data.HumanResources.DataContracts.AsgmtContractTypes()
                    {
                        Recordkey = "FPA",
                        ActypDesc = "Fixed by Assignment w/ Subr"
                    }

                };
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Ellucian.Colleague.Data.HumanResources.DataContracts.AsgmtContractTypes>("ASGMT.CONTRACT.TYPES", "", true)).ReturnsAsync(_assignmentContractTypesCollection);
                hRRDR = new HumanResourcesReferenceDataRepository(cacheProvider, transFactory, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }


            [TestMethod]
            public async Task GetAssignmentContractTypesAsync()
            {
                var result = await hRRDR.GetAssignmentContractTypesAsync(false);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(_assignmentContractTypesCollection.First().ActypDesc, result.First().Description);
            }
        }

        /// <summary>
        /// Test class for Bargaining Units codes
        /// </summary>
        /// 
        [TestClass]
        public class BargainingUnitsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<BargainingUnit> allBargainingUnits;
            string codeItemName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allBargainingUnits = new TestBargainingUnitsRepository().GetBargainingUnits();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllBargainingUnits");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allBargainingUnits = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsBargainingUnitsCacheAsync()
            {
                var bargainingunits = await referenceDataRepo.GetBargainingUnitsAsync(false);

                for (int i = 0; i < allBargainingUnits.Count(); i++)
                {
                    Assert.AreEqual(allBargainingUnits.ElementAt(i).Guid, bargainingunits.ElementAt(i).Guid);
                    Assert.AreEqual(allBargainingUnits.ElementAt(i).Code, bargainingunits.ElementAt(i).Code);
                    Assert.AreEqual(allBargainingUnits.ElementAt(i).Description, bargainingunits.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsBargainingUnitsNonCacheAsync()
            {
                var bargainingUnits = await referenceDataRepo.GetBargainingUnitsAsync(true);

                for (int i = 0; i < allBargainingUnits.Count(); i++)
                {
                    Assert.AreEqual(allBargainingUnits.ElementAt(i).Guid, bargainingUnits.ElementAt(i).Guid);
                    Assert.AreEqual(allBargainingUnits.ElementAt(i).Code, bargainingUnits.ElementAt(i).Code);
                    Assert.AreEqual(allBargainingUnits.ElementAt(i).Description, bargainingUnits.ElementAt(i).Description);
                }
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to BargainingUnits read
                var denominationsCollection = new Collection<BargUnits>(allBargainingUnits.Select(record =>
                    new Data.HumanResources.DataContracts.BargUnits()
                    {
                        Recordkey = record.Code,
                        BgnDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<BargUnits>("BARG.UNITS", "", true))
                    .ReturnsAsync(denominationsCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var denomination = allBargainingUnits.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "BARG.UNITS", denomination.Code }),
                            new RecordKeyLookupResult() { Guid = denomination.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Beneficiary Types
        /// </summary>
        [TestClass]
        public class BeneficiaryTypesTests
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<BeneficiaryTypes> _allBeneficiaryTypes;
            ApplValcodes _beneficiaryTypeValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build beneficiary types responses used for mocking
                _allBeneficiaryTypes = new TestBeneficiaryTypeRepository().GetBeneficiaryTypes();
                _beneficiaryTypeValcodeResponse = BuildValcodeResponse(_allBeneficiaryTypes);

                // Build HR reference data repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("HR_BENEFICIARY.TYPES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _beneficiaryTypeValcodeResponse = null;
                _allBeneficiaryTypes = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsBeneficiaryTypesCacheAsync()
            {
                var beneficiaryTypes = await _referenceDataRepo.GetBeneficiaryTypesAsync(false);
                for (int i = 0; i < beneficiaryTypes.Count(); i++)
                {
                    Assert.AreEqual(_allBeneficiaryTypes.ElementAt(i).Code, beneficiaryTypes.ElementAt(i).Code);
                    Assert.AreEqual(_allBeneficiaryTypes.ElementAt(i).Description, beneficiaryTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsBeneficiaryTypesNonCacheAsync()
            {
                var beneficiaryTypes = await _referenceDataRepo.GetBeneficiaryTypesAsync(true);
                for (int i = 0; i < beneficiaryTypes.Count(); i++)
                {
                    Assert.AreEqual(_allBeneficiaryTypes.ElementAt(i).Code, beneficiaryTypes.ElementAt(i).Code);
                    Assert.AreEqual(_allBeneficiaryTypes.ElementAt(i).Description, beneficiaryTypes.ElementAt(i).Description);
                }
            }

            //[TestMethod]
            //public async Task GetBeneficiaryTypes_WritesToCacheAsync()
            //{

            //    // Set up local cache mock to respond to cache request:
            //    //  -to "Contains" request, return "false" to indicate item is not in cache
            //    //  -to cache "Get" request, return null so we know it's reading from the "repository"
            //    _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
            //    _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

            //    // return a valid response to the data accessor request
            //    _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_beneficiaryTypeValcodeResponse);

            //    _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            //     x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            //     .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            //    // But after data accessor read, set up mocking so we can verify the list of beneficiary types was written to the cache
            //    _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<BeneficiaryTypes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

            //    _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("HR_BENEFICIARY.TYPES"), null)).Returns(true);
            //    var beneficiaryTypes = await _referenceDataRepo.GetBeneficiaryTypesAsync(false);
            //    _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("HR_BENEFICIARY.TYPES"), null)).Returns(beneficiaryTypes);
            //    // Verify that beneficiary types were returned, which means they came from the "repository".
            //    Assert.IsTrue(beneficiaryTypes.Count() == 6);

            //    // Verify that the beneficiary type item was added to the cache after it was read from the repository
            //    _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<BeneficiaryTypes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            //}

            [TestMethod]
            public async Task GetBeneficiaryTypes_GetsCachedBeneficiaryTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "BENEFICIARY.TYPES" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allBeneficiaryTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BENEFICIARY.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the beneficiary types are returned
                Assert.IsTrue((await _referenceDataRepo.GetBeneficiaryTypesAsync(false)).Count() == 6);
                // Verify that the beneficiary types were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to beneficiary type valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BENEFICIARY.TYPES", It.IsAny<bool>())).ReturnsAsync(_beneficiaryTypeValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var beneficiaryType = _allBeneficiaryTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "BD.CALC.METHODS", beneficiaryType.Code }),
                            new RecordKeyLookupResult() { Guid = beneficiaryType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<BeneficiaryTypes> beneficiaryTypes)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in beneficiaryTypes)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Beneficiary category
        /// </summary>
        [TestClass]
        public class BeneficiaryCategoryTests
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<BeneficiaryCategory> _allBeneficiaryCategories;
            ApplValcodes _beneficiaryTypeValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build beneficiary types responses used for mocking
                _allBeneficiaryCategories = new TestBeneficiaryCategoryRepository().GetBeneficiaryCategoriesAsync();
                _beneficiaryTypeValcodeResponse = BuildValcodeResponse(_allBeneficiaryCategories);

                // Build HR reference data repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("BENEFICIARY.TYPES");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _beneficiaryTypeValcodeResponse = null;
                _allBeneficiaryCategories = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsBeneficiaryCategoryAsync()
            {
                var beneficiaryCategories = await _referenceDataRepo.GetBeneficiaryCategoriesAsync();
                for (int i = 0; i < beneficiaryCategories.Count(); i++)
                {
                    Assert.AreEqual(_allBeneficiaryCategories.ElementAt(i).Code, beneficiaryCategories.ElementAt(i).Code);
                    Assert.AreEqual(_allBeneficiaryCategories.ElementAt(i).Description, beneficiaryCategories.ElementAt(i).Description);
                }
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to beneficiary type valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODE", "BENEFICIARY.TYPES", It.IsAny<bool>())).ReturnsAsync(_beneficiaryTypeValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<BeneficiaryCategory> beneficiaryCategories)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in beneficiaryCategories)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, item.ProcessingCode, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }

        }

        /// <summary>
        /// Test class for Cost Calculation Methods
        /// </summary>
        [TestClass]
        public class CostCalculationMethods
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<CostCalculationMethod> _allCostCalculationMethods;
            ApplValcodes _costCalculationMethodValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build cost calculation methods responses used for mocking
                _allCostCalculationMethods = new TestCostCalculationMethodRepository().GetCostCalculationMethods();
                _costCalculationMethodValcodeResponse = BuildValcodeResponse(_allCostCalculationMethods);

                // Build HR reference data repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("HR_BD.CALC.METHODS_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _costCalculationMethodValcodeResponse = null;
                _allCostCalculationMethods = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsCostCalculationMethodsCacheAsync()
            {
                var costCalculationMethods = await _referenceDataRepo.GetCostCalculationMethodsAsync(false);
                for (int i = 0; i < costCalculationMethods.Count(); i++)
                {
                    Assert.AreEqual(_allCostCalculationMethods.ElementAt(i).Code, costCalculationMethods.ElementAt(i).Code);
                    Assert.AreEqual(_allCostCalculationMethods.ElementAt(i).Description, costCalculationMethods.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsCostCalculationMethodsNonCacheAsync()
            {
                var costCalculationMethods = await _referenceDataRepo.GetCostCalculationMethodsAsync(true);
                for (int i = 0; i < costCalculationMethods.Count(); i++)
                {
                    Assert.AreEqual(_allCostCalculationMethods.ElementAt(i).Code, costCalculationMethods.ElementAt(i).Code);
                    Assert.AreEqual(_allCostCalculationMethods.ElementAt(i).Description, costCalculationMethods.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetCostCalculationMethods_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_costCalculationMethodValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of cost calculation methods was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<CostCalculationMethod>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("HR_BD.CALC.METHODS"), null)).Returns(true);
                var costCalculationMethods = await _referenceDataRepo.GetCostCalculationMethodsAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("HR_BD.CALC.METHODS"), null)).Returns(costCalculationMethods);
                // Verify that cost calculation methods were returned, which means they came from the "repository".
                Assert.IsTrue(costCalculationMethods.Count() == 6);

                // Verify that the cost calculation method item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<CostCalculationMethod>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetCostCalculationMethods_GetsCachedCostCalculationMethodsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "CLASSIFICATIONS" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allCostCalculationMethods).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BD.CALC.METHODS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the cost calculation methods are returned
                Assert.IsTrue((await _referenceDataRepo.GetCostCalculationMethodsAsync(false)).Count() == 6);
                // Verify that the cost calculation methods were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to cost calculation method valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BD.CALC.METHODS", It.IsAny<bool>())).ReturnsAsync(_costCalculationMethodValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var costCalculationMethod = _allCostCalculationMethods.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "BD.CALC.METHODS", costCalculationMethod.Code }),
                            new RecordKeyLookupResult() { Guid = costCalculationMethod.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<CostCalculationMethod> costCalculationMethods)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in costCalculationMethods)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }


        /// <summary>
        /// Test class for Deduction Categories
        /// </summary>
        [TestClass]
        public class DeductionCategories
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<DeductionCategory> _allDeductionCategories;
            ApplValcodes _deductionCategoryValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build deduction categories responses used for mocking
                _allDeductionCategories = new TestDeductionCategoryRepository().GetDeductionCategories();
                _deductionCategoryValcodeResponse = BuildValcodeResponse(_allDeductionCategories);

                // Build privacy statuses repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("HR_BENDED.TYPES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _deductionCategoryValcodeResponse = null;
                _allDeductionCategories = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsDeductionCategoriesCacheAsync()
            {
                var deductionCategories = await _referenceDataRepo.GetDeductionCategoriesAsync(false);
                for (int i = 0; i < deductionCategories.Count(); i++)
                {
                    Assert.AreEqual(_allDeductionCategories.ElementAt(i).Code, deductionCategories.ElementAt(i).Code);
                    Assert.AreEqual(_allDeductionCategories.ElementAt(i).Description, deductionCategories.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsDeductionCategoriesNonCacheAsync()
            {
                var deductionCategories = await _referenceDataRepo.GetDeductionCategoriesAsync(true);
                for (int i = 0; i < deductionCategories.Count(); i++)
                {
                    Assert.AreEqual(_allDeductionCategories.ElementAt(i).Code, deductionCategories.ElementAt(i).Code);
                    Assert.AreEqual(_allDeductionCategories.ElementAt(i).Description, deductionCategories.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetDeductionCategories_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_deductionCategoryValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of deduction categories was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<DeductionCategory>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("HR_BENDED.TYPES"), null)).Returns(true);
                var deductionCategories = await _referenceDataRepo.GetDeductionCategoriesAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("HR_BENDED.TYPES"), null)).Returns(deductionCategories);
                // Verify that deduction categories were returned, which means they came from the "repository".
                Assert.IsTrue(deductionCategories.Count() == 6);

                // Verify that the deduction category item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<DeductionCategory>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetDeductionCategories_GetsCachedDeductionCategoriesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "CLASSIFICATIONS" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allDeductionCategories).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BENDED.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the deduction categories are returned
                Assert.IsTrue((await _referenceDataRepo.GetDeductionCategoriesAsync(false)).Count() == 6);
                // Verify that the deduction categories were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BENDED.TYPES", It.IsAny<bool>())).ReturnsAsync(_deductionCategoryValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var deductionCategory = _allDeductionCategories.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "BENDED.TYPES", deductionCategory.Code }),
                            new RecordKeyLookupResult() { Guid = deductionCategory.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<DeductionCategory> deductionCategories)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in deductionCategories)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Earning Types codes
        /// </summary>
        [TestClass]
        public class EarningTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<EarningType2> allEarningTypes;
            string codeItemName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allEarningTypes = new TestEarningType2Repository().GetEarningTypes();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllHREarningTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allEarningTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEarningTypesCacheAsync()
            {
                var earningTypes = await referenceDataRepo.GetEarningTypesAsync(false);

                for (int i = 0; i < allEarningTypes.Count(); i++)
                {
                    Assert.AreEqual(allEarningTypes.ElementAt(i).Guid, earningTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allEarningTypes.ElementAt(i).Code, earningTypes.ElementAt(i).Code);
                    Assert.AreEqual(allEarningTypes.ElementAt(i).Description, earningTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEarningTypesNonCacheAsync()
            {
                var earningTypes = await referenceDataRepo.GetEarningTypesAsync(true);

                for (int i = 0; i < allEarningTypes.Count(); i++)
                {
                    Assert.AreEqual(allEarningTypes.ElementAt(i).Guid, earningTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allEarningTypes.ElementAt(i).Code, earningTypes.ElementAt(i).Code);
                    Assert.AreEqual(allEarningTypes.ElementAt(i).Description, earningTypes.ElementAt(i).Description);
                }
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to EarningTypes read
                var earningTypesCollection = new Collection<Earntype>(allEarningTypes.Select(record =>
                    new Data.HumanResources.DataContracts.Earntype()
                    {
                        Recordkey = record.Code,
                        EtpDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Earntype>("EARNTYPE", "", true))
                    .ReturnsAsync(earningTypesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var denomination = allEarningTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "EARNTYPE", denomination.Code }),
                            new RecordKeyLookupResult() { Guid = denomination.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Employment Classifications
        /// </summary>
        [TestClass]
        public class EmploymentClassifications
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<EmploymentClassification> _allEmploymentClassifications;
            ApplValcodes _employmentClassificationValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build employment classifications responses used for mocking
                _allEmploymentClassifications = new TestEmploymentClassRepository().GetEmploymentClassifications();
                _employmentClassificationValcodeResponse = BuildValcodeResponse(_allEmploymentClassifications);

                // Build privacy statuses repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("HR_CLASSIFICATIONS_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _employmentClassificationValcodeResponse = null;
                _allEmploymentClassifications = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEmploymentClassificationsCacheAsync()
            {
                var employmentClassifications = await _referenceDataRepo.GetEmploymentClassificationsAsync(false);
                for (int i = 0; i < employmentClassifications.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentClassifications.ElementAt(i).Code, employmentClassifications.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentClassifications.ElementAt(i).Description, employmentClassifications.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEmploymentClassificationsNonCacheAsync()
            {
                var employmentClassifications = await _referenceDataRepo.GetEmploymentClassificationsAsync(true);
                for (int i = 0; i < employmentClassifications.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentClassifications.ElementAt(i).Code, employmentClassifications.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentClassifications.ElementAt(i).Description, employmentClassifications.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetEmploymentClassifications_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_employmentClassificationValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of employment classifications was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentClassification>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("HR_CLASSIFICATIONS"), null)).Returns(true);
                var employmentClassifications = await _referenceDataRepo.GetEmploymentClassificationsAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("HR_CLASSIFICATIONS"), null)).Returns(employmentClassifications);
                // Verify that employment classifications were returned, which means they came from the "repository".
                Assert.IsTrue(employmentClassifications.Count() == 4);

                // Verify that the employment classification item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentClassification>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetEmploymentClassifications_GetsCachedEmploymentClassificationsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "CLASSIFICATIONS" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allEmploymentClassifications).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "CLASSIFICATIONS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the employment classifications are returned
                Assert.IsTrue((await _referenceDataRepo.GetEmploymentClassificationsAsync(false)).Count() == 4);
                // Verify that the employment classifications were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "CLASSIFICATIONS", It.IsAny<bool>())).ReturnsAsync(_employmentClassificationValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var employmentClassification = _allEmploymentClassifications.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "CLASSIFICATIONS", employmentClassification.Code }),
                            new RecordKeyLookupResult() { Guid = employmentClassification.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<EmploymentClassification> employmentClassifications)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in employmentClassifications)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        [TestClass]
        public class EmploymentDepartmentsTests : BaseRepositorySetup
        {
            public TestEmploymentDepartmentRepository testDataRepository;

            public HumanResourcesReferenceDataRepository repositoryUnderTest;

            public void HumanResourcesReferenceDataRepositoryTestsInitialize()
            {
                MockInitialize();
                testDataRepository = new TestEmploymentDepartmentRepository();

                repositoryUnderTest = BuildRepository();
            }

            [TestInitialize]
            public void Initialize()
            {
                HumanResourcesReferenceDataRepositoryTestsInitialize();
            }

            public HumanResourcesReferenceDataRepository BuildRepository()
            {
                string id, guid, id2, guid2, id3, guid3, sid, sid2, sid3;
                GuidLookup guidLookup;
                GuidLookupResult guidLookupResult;
                Dictionary<string, GuidLookupResult> guidLookupDict;
                RecordKeyLookup recordLookup;
                RecordKeyLookupResult recordLookupResult;
                Dictionary<string, RecordKeyLookupResult> recordLookupDict;

                var empDept = testDataRepository.GetEmploymentDepartments();

                // Set up for GUID lookups
                id = empDept.ElementAt(0).Code; // "1";
                id2 = empDept.ElementAt(1).Code; //"2";
                id3 = empDept.ElementAt(2).Code; //"3";

                // Secondary keys for GUID lookups
                sid = "11";
                sid2 = "22";
                sid3 = "33";

                guid = empDept.ElementAt(0).Guid.ToLowerInvariant(); //"F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = empDept.ElementAt(1).Guid.ToLowerInvariant(); //"5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = empDept.ElementAt(2).Guid.ToLowerInvariant(); //"246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "DEPTS", PrimaryKey = id, SecondaryKey = sid };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("DEPTS", id, "DEPT.INTG.KEY.IDX", sid, false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                var bulkRecordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult> {
                    { "DEPTS+" + id, new RecordKeyLookupResult() {Guid = guid } },
                    { "DEPTS+" + id2, new RecordKeyLookupResult() {Guid = guid2 } },
                    { "DEPTS+" + id3, new RecordKeyLookupResult() {Guid = guid3 } }
                };
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(bulkRecordKeyLookupResults);

                var bulkResult = new Dictionary<string, GuidLookupResult>();
                bulkResult[guid] = new GuidLookupResult() { Entity = "DEPTS", PrimaryKey = id, SecondaryKey = sid };
                bulkResult[guid2] = new GuidLookupResult() { Entity = "DEPTS", PrimaryKey = id2, SecondaryKey = sid2 };
                bulkResult[guid3] = new GuidLookupResult() { Entity = "DEPTS", PrimaryKey = id3, SecondaryKey = sid3 };

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(bulkResult);

                dataReaderMock.Setup(d => d.SelectAsync("DEPTS", ""))
                    .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.GetEmploymentDepartments() == null ? null :
                        testDataRepository.GetEmploymentDepartments().Select(dtype => dtype.Guid).ToArray()));

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Depts>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, b) =>
                        Task.FromResult(testDataRepository.GetEmploymentDepartments() == null ? null :
                            new Collection<Ellucian.Colleague.Data.Base.DataContracts.Depts>(testDataRepository.GetEmploymentDepartments()
                                .Where(record => ids.Contains(record.Guid))
                                .Select(record =>
                                    (record == null) ? null : new Ellucian.Colleague.Data.Base.DataContracts.Depts()
                                    {
                                        Recordkey = record.Code,
                                        DeptsDesc = record.Description,
                                        RecordGuid = record.Guid,
                                        DeptIntgKeyIdx = sid
                                    }).ToList())
                        ));

                apiSettings.BulkReadSize = 1;

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                return new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            public async Task<IEnumerable<EmploymentDepartment>> getExpectedEmploymentDepartments()
            {
                return testDataRepository.GetEmploymentDepartments();
            }

            public async Task<IEnumerable<EmploymentDepartment>> getActualEmploymentDepartments(bool ignoreCache = false)
            {
                return await repositoryUnderTest.GetEmploymentDepartmentsAsync(ignoreCache);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await getExpectedEmploymentDepartments()).ToList();
                var actual = (await getActualEmploymentDepartments()).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = (await getExpectedEmploymentDepartments()).ToArray();
                var actual = (await getActualEmploymentDepartments()).ToArray();
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected[i].Code, actual[i].Code);
                    Assert.AreEqual(expected[i].Guid, actual[i].Guid);
                    Assert.AreEqual(expected[i].Description, actual[i].Description);
                }
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest_Cached()
            {
                var expected = (await getExpectedEmploymentDepartments()).ToList();
                var actual = (await getActualEmploymentDepartments(true)).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

        }

        /// <summary>
        /// Test class for Employment Frequencies
        /// </summary>
        [TestClass]
        public class EmploymentFrequencies
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<EmploymentFrequency> _allEmploymentFrequencies;
            ApplValcodes _employmentFrequencyValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build employment frequencies responses used for mocking
                _allEmploymentFrequencies = new TestEmploymentFrequencyRepository().GetEmploymentFrequencies();
                _employmentFrequencyValcodeResponse = BuildValcodeResponse(_allEmploymentFrequencies);

                // Build privacy statuses repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("HR_TIME.FREQUENCIES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _employmentFrequencyValcodeResponse = null;
                _allEmploymentFrequencies = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEmploymentFrequenciesCacheAsync()
            {
                var employmentFrequencies = await _referenceDataRepo.GetEmploymentFrequenciesAsync(false);
                for (int i = 0; i < employmentFrequencies.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentFrequencies.ElementAt(i).Code, employmentFrequencies.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentFrequencies.ElementAt(i).Description, employmentFrequencies.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEmploymentFrequenciesNonCacheAsync()
            {
                var employmentFrequencies = await _referenceDataRepo.GetEmploymentFrequenciesAsync(true);
                for (int i = 0; i < employmentFrequencies.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentFrequencies.ElementAt(i).Code, employmentFrequencies.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentFrequencies.ElementAt(i).Description, employmentFrequencies.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetEmploymentFrequencies_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_employmentFrequencyValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of employment frequencies was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentFrequency>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("HR_TIME.FREQUENCIES"), null)).Returns(true);
                var employmentFrequencies = await _referenceDataRepo.GetEmploymentFrequenciesAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("HR_TIME.FREQUENCIES"), null)).Returns(employmentFrequencies);
                // Verify that employment frequencies were returned, which means they came from the "repository".
                Assert.IsTrue(employmentFrequencies.Count() == 6);

                // Verify that the employment frequency item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentFrequency>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetEmploymentFrequencies_GetsCachedEmploymentFrequenciesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "TIME.FREQUENCIES" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allEmploymentFrequencies).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "TIME.FREQUENCIES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the employment frequencies are returned
                Assert.IsTrue((await _referenceDataRepo.GetEmploymentFrequenciesAsync(false)).Count() == 6);
                // Verify that the employment frequencies were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "TIME.FREQUENCIES", It.IsAny<bool>())).ReturnsAsync(_employmentFrequencyValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var employmentFrequency = _allEmploymentFrequencies.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "TIME.FREQUENCIES", employmentFrequency.Code }),
                            new RecordKeyLookupResult() { Guid = employmentFrequency.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<EmploymentFrequency> employmentFrequencies)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in employmentFrequencies)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Employment Performance Review Ratings
        /// </summary>
        [TestClass]
        public class EmploymentPerformanceReviewRatings
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<EmploymentPerformanceReviewRating> _allEmploymentPerformanceReviewRatings;
            ApplValcodes _employmentPerformanceReviewRatingValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build employment performance review ratings responses used for mocking
                _allEmploymentPerformanceReviewRatings = new TestEmploymentPerformanceReviewRatingRepository().GetEmploymentPerformanceReviewRatings();
                _employmentPerformanceReviewRatingValcodeResponse = BuildValcodeResponse(_allEmploymentPerformanceReviewRatings);

                // Build privacy statuses repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("HR_PERFORMANCE.EVAL.RATINGS_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _employmentPerformanceReviewRatingValcodeResponse = null;
                _allEmploymentPerformanceReviewRatings = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEmploymentPerformanceReviewRatingsCacheAsync()
            {
                var employmentPerformanceReviewRatings = await _referenceDataRepo.GetEmploymentPerformanceReviewRatingsAsync(false);
                for (int i = 0; i < employmentPerformanceReviewRatings.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentPerformanceReviewRatings.ElementAt(i).Code, employmentPerformanceReviewRatings.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentPerformanceReviewRatings.ElementAt(i).Description, employmentPerformanceReviewRatings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEmploymentPerformanceReviewRatingsNonCacheAsync()
            {
                var employmentPerformanceReviewRatings = await _referenceDataRepo.GetEmploymentPerformanceReviewRatingsAsync(true);
                for (int i = 0; i < employmentPerformanceReviewRatings.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentPerformanceReviewRatings.ElementAt(i).Code, employmentPerformanceReviewRatings.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentPerformanceReviewRatings.ElementAt(i).Description, employmentPerformanceReviewRatings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetEmploymentPerformanceReviewRatings_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_employmentPerformanceReviewRatingValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of employment performance review ratings was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentPerformanceReviewRating>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("HR_PERFORMANCE.EVAL.RATINGS"), null)).Returns(true);
                var employmentPerformanceReviewRatings = await _referenceDataRepo.GetEmploymentPerformanceReviewRatingsAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("HR_PERFORMANCE.EVAL.RATINGS"), null)).Returns(employmentPerformanceReviewRatings);
                // Verify that employment performance review ratings were returned, which means they came from the "repository".
                Assert.IsTrue(employmentPerformanceReviewRatings.Count() == 4);

                // Verify that the employment performance review rating item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentPerformanceReviewRating>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetEmploymentPerformanceReviewRatings_GetsCachedEmploymentPerformanceReviewRatingsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "CLASSIFICATIONS" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allEmploymentPerformanceReviewRatings).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "PERFORMANCE.EVAL.RATINGS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the employment performance review ratings are returned
                Assert.IsTrue((await _referenceDataRepo.GetEmploymentPerformanceReviewRatingsAsync(false)).Count() == 4);
                // Verify that the employment performance review ratings were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "PERFORMANCE.EVAL.RATINGS", It.IsAny<bool>())).ReturnsAsync(_employmentPerformanceReviewRatingValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var employmentPerformanceReviewRating = _allEmploymentPerformanceReviewRatings.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "PERFORMANCE.EVAL.RATINGS", employmentPerformanceReviewRating.Code }),
                            new RecordKeyLookupResult() { Guid = employmentPerformanceReviewRating.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<EmploymentPerformanceReviewRating> employmentPerformanceReviewRatings)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in employmentPerformanceReviewRatings)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Employment Performance Review Types
        /// </summary>
        [TestClass]
        public class EmploymentPerformanceReviewTypes
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<EmploymentPerformanceReviewType> _allEmploymentPerformanceReviewTypes;
            ApplValcodes _employmentPerformanceReviewTypeValcodeResponse;
            string _valcodeName;

            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build employment performance review types responses used for mocking
                _allEmploymentPerformanceReviewTypes = new TestEmploymentPerformanceReviewTypeRepository().GetEmploymentPerformanceReviewTypes();
                _employmentPerformanceReviewTypeValcodeResponse = BuildValcodeResponse(_allEmploymentPerformanceReviewTypes);

                // Build privacy statuses repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("HR_EVALUATION.CYCLES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _employmentPerformanceReviewTypeValcodeResponse = null;
                _allEmploymentPerformanceReviewTypes = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEmploymentPerformanceReviewTypesCacheAsync()
            {
                var employmentPerformanceReviewTypes = await _referenceDataRepo.GetEmploymentPerformanceReviewTypesAsync(false);
                for (int i = 0; i < employmentPerformanceReviewTypes.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentPerformanceReviewTypes.ElementAt(i).Code, employmentPerformanceReviewTypes.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentPerformanceReviewTypes.ElementAt(i).Description, employmentPerformanceReviewTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEmploymentPerformanceReviewTypesNonCacheAsync()
            {
                var employmentPerformanceReviewTypes = await _referenceDataRepo.GetEmploymentPerformanceReviewTypesAsync(true);
                for (int i = 0; i < employmentPerformanceReviewTypes.Count(); i++)
                {
                    Assert.AreEqual(_allEmploymentPerformanceReviewTypes.ElementAt(i).Code, employmentPerformanceReviewTypes.ElementAt(i).Code);
                    Assert.AreEqual(_allEmploymentPerformanceReviewTypes.ElementAt(i).Description, employmentPerformanceReviewTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetEmploymentPerformanceReviewTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_employmentPerformanceReviewTypeValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of employment performance review types was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentPerformanceReviewType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("HR_EVALUATION.CYCLES"), null)).Returns(true);
                var employmentPerformanceReviewTypes = await _referenceDataRepo.GetEmploymentPerformanceReviewTypesAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("HR_EVALUATION.CYCLES"), null)).Returns(employmentPerformanceReviewTypes);
                // Verify that employment classifications were returned, which means they came from the "repository".
                Assert.IsTrue(employmentPerformanceReviewTypes.Count() == 4);

                // Verify that the employment classification item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentPerformanceReviewType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetEmploymentPerformanceReviewTypes_GetsCachedEmploymentPerformanceReviewTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "CLASSIFICATIONS" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allEmploymentPerformanceReviewTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "EVALUATION.CYCLES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the employment classifications are returned
                Assert.IsTrue((await _referenceDataRepo.GetEmploymentPerformanceReviewTypesAsync(false)).Count() == 4);
                // Verify that the employment classifications were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "EVALUATION.CYCLES", It.IsAny<bool>())).ReturnsAsync(_employmentPerformanceReviewTypeValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var employmentPerformanceReviewType = _allEmploymentPerformanceReviewTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "EVALUATION.CYCLES", employmentPerformanceReviewType.Code }),
                            new RecordKeyLookupResult() { Guid = employmentPerformanceReviewType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new HumanResourcesReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<EmploymentPerformanceReviewType> employmentPerformanceReviewTypes)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in employmentPerformanceReviewTypes)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Employment Proficiencies codes
        /// </summary>
        [TestClass]
        public class EmploymentProficienciesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<EmploymentProficiency> allEmploymentProficiencies;
            string codeItemName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allEmploymentProficiencies = new TestEmploymentProficiencyRepository().GetEmploymentProficiencies();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllEmploymentProficiencies");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allEmploymentProficiencies = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEmploymentProficienciesCacheAsync()
            {
                var employmentProficiencies = await referenceDataRepo.GetEmploymentProficienciesAsync(false);

                for (int i = 0; i < allEmploymentProficiencies.Count(); i++)
                {
                    Assert.AreEqual(allEmploymentProficiencies.ElementAt(i).Guid, employmentProficiencies.ElementAt(i).Guid);
                    Assert.AreEqual(allEmploymentProficiencies.ElementAt(i).Code, employmentProficiencies.ElementAt(i).Code);
                    Assert.AreEqual(allEmploymentProficiencies.ElementAt(i).Description, employmentProficiencies.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEmploymentProficienciesNonCacheAsync()
            {
                var employmentProficiencies = await referenceDataRepo.GetEmploymentProficienciesAsync(true);

                for (int i = 0; i < allEmploymentProficiencies.Count(); i++)
                {
                    Assert.AreEqual(allEmploymentProficiencies.ElementAt(i).Guid, employmentProficiencies.ElementAt(i).Guid);
                    Assert.AreEqual(allEmploymentProficiencies.ElementAt(i).Code, employmentProficiencies.ElementAt(i).Code);
                    Assert.AreEqual(allEmploymentProficiencies.ElementAt(i).Description, employmentProficiencies.ElementAt(i).Description);
                }
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to EmploymentProficiencies read
                var employmentProficienciesCollection = new Collection<Jobskills>(allEmploymentProficiencies.Select(record =>
                    new Data.HumanResources.DataContracts.Jobskills()
                    {
                        Recordkey = record.Code,
                        JskDesc = record.Description,
                        RecordGuid = record.Guid,
                        JskLicenseCert = "Y",
                        JskAuthority = "AUTH",
                        JskComment = "COMMENT"
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Jobskills>("JOBSKILLS", "", It.IsAny<bool>()))
                    .ReturnsAsync(employmentProficienciesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var employmentProficiency = allEmploymentProficiencies.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "JOBSKILLS", employmentProficiency.Code }),
                            new RecordKeyLookupResult() { Guid = employmentProficiency.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class DeductionTypesTests : BaseRepositorySetup
        {
            public TestDeductionTypeRepository testDataRepository;

            public HumanResourcesReferenceDataRepository repositoryUnderTest;

            public void HumanResourcesReferenceDataRepositoryTestsInitialize()
            {
                MockInitialize();
                testDataRepository = new TestDeductionTypeRepository();

                repositoryUnderTest = BuildRepository();
            }

            [TestInitialize]
            public void Initialize()
            {
                HumanResourcesReferenceDataRepositoryTestsInitialize();
            }

            public HumanResourcesReferenceDataRepository BuildRepository()
            {
                dataReaderMock.Setup(d => d.SelectAsync("BENDED", "WITH BD.PAYERS = 'E''S'"))
                    .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.GetDeductionTypes() == null ? null :
                        testDataRepository.GetDeductionTypes().Select(dtype => dtype.Guid).ToArray()));

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Bended>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, b) =>
                        Task.FromResult(testDataRepository.GetDeductionTypes() == null ? null :
                            new Collection<Bended>(testDataRepository.GetDeductionTypes()
                                .Where(record => ids.Contains(record.Guid))
                                .Select(record =>
                                    (record == null) ? null : new Bended()
                                    {
                                        Recordkey = record.Code,
                                        BdDesc = record.Description,
                                        RecordGuid = record.Guid
                                    }).ToList())
                        ));

                apiSettings.BulkReadSize = 1;

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                return new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            public async Task<IEnumerable<DeductionType>> getExpectedDeductionTypes()
            {
                return testDataRepository.GetDeductionTypes();
            }

            public async Task<IEnumerable<DeductionType>> getActualDeductionTypes(bool ignoreCache = false)
            {
                return await repositoryUnderTest.GetDeductionTypesAsync(ignoreCache);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await getExpectedDeductionTypes()).ToList();
                var actual = (await getActualDeductionTypes()).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = (await getExpectedDeductionTypes()).ToArray();
                var actual = (await getActualDeductionTypes()).ToArray();
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected[i].Code, actual[i].Code);
                    Assert.AreEqual(expected[i].Guid, actual[i].Guid);
                    Assert.AreEqual(expected[i].Description, actual[i].Description);
                }
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest_Cached()
            {
                var expected = (await getExpectedDeductionTypes()).ToList();
                var actual = (await getActualDeductionTypes(true)).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

        }

        [TestClass]
        public class DeductionTypes2Tests : BaseRepositorySetup
        {
            public TestDeductionTypeRepository testDataRepository;

            public HumanResourcesReferenceDataRepository repositoryUnderTest;

            public void HumanResourcesReferenceDataRepositoryTestsInitialize()
            {
                MockInitialize();
                testDataRepository = new TestDeductionTypeRepository();

                repositoryUnderTest = BuildRepository();
            }

            [TestInitialize]
            public void Initialize()
            {
                HumanResourcesReferenceDataRepositoryTestsInitialize();
            }

            public HumanResourcesReferenceDataRepository BuildRepository()
            {
                dataReaderMock.Setup(d => d.SelectAsync("BENDED", "WITH BD.PAYERS = 'E''S'"))
                    .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.GetDeductionTypes() == null ? null :
                        testDataRepository.GetDeductionTypes().Select(dtype => dtype.Guid).ToArray()));

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Bended>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, b) =>
                        Task.FromResult(testDataRepository.GetDeductionTypes() == null ? null :
                            new Collection<Bended>(testDataRepository.GetDeductionTypes()
                                .Where(record => ids.Contains(record.Guid))
                                .Select(record =>
                                    (record == null) ? null : new Bended()
                                    {
                                        Recordkey = record.Code,
                                        BdDesc = record.Description,
                                        RecordGuid = record.Guid
                                    }).ToList())
                        ));

                apiSettings.BulkReadSize = 1;

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                return new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            public async Task<IEnumerable<DeductionType>> getExpectedDeductionTypes()
            {
                return testDataRepository.GetDeductionTypes();
            }

            public async Task<IEnumerable<DeductionType>> getActualDeductionTypes(bool ignoreCache = false)
            {
                return await repositoryUnderTest.GetDeductionTypes2Async(ignoreCache);
            }

            //[TestMethod]
            //public async Task ExpectedEqualsActualTest()
            //{
            //    var expected = (await getExpectedDeductionTypes()).ToList();
            //    var actual = (await getActualDeductionTypes()).ToList();
            //    CollectionAssert.AreEqual(expected, actual);
            //}

            //[TestMethod]
            //public async Task AttributesTest()
            //{
            //    var expected = (await getExpectedDeductionTypes()).ToArray();
            //    var actual = (await getActualDeductionTypes()).ToArray();
            //    for (int i = 0; i < expected.Count(); i++)
            //    {
            //        Assert.AreEqual(expected[i].Code, actual[i].Code);
            //        Assert.AreEqual(expected[i].Guid, actual[i].Guid);
            //        Assert.AreEqual(expected[i].Description, actual[i].Description);
            //    }
            //}

            //[TestMethod]
            //public async Task ExpectedEqualsActualTest_Cached()
            //{
            //    var expected = (await getExpectedDeductionTypes()).ToList();
            //    var actual = (await getActualDeductionTypes(true)).ToList();
            //    CollectionAssert.AreEqual(expected, actual);
            //}

        }

        [TestClass]
        public class EmploymentStatusEndingReason_GETAll
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<EmploymentStatusEndingReason> allEmploymentStatusEndingReason;
            ApplValcodes employmentStatusEndingReasonValcodeResponse;
            string valcodeName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build EmploymentStatusEndingReason responses used for mocking
                allEmploymentStatusEndingReason = new TestEmploymentStatusEndingReasonRepository().GetEmploymentStatusEndingReasons();
                employmentStatusEndingReasonValcodeResponse = BuildValcodeResponse(allEmploymentStatusEndingReason);

                // Build privacy statuses repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("HR_STATUS_ENDING_REASONS_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                employmentStatusEndingReasonValcodeResponse = null;
                allEmploymentStatusEndingReason = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEmploymentStatusEndingReasonsCacheAsync()
            {
                var EmploymentStatusEndingReasons = await referenceDataRepo.GetEmploymentStatusEndingReasonsAsync(false);
                for (int i = 0; i < EmploymentStatusEndingReasons.Count(); i++)
                {
                    Assert.AreEqual(allEmploymentStatusEndingReason.ElementAt(i).Code, EmploymentStatusEndingReasons.ElementAt(i).Code);
                    Assert.AreEqual(allEmploymentStatusEndingReason.ElementAt(i).Description, EmploymentStatusEndingReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEmploymentStatusEndingReasonsGuidAsync()
            {
                var employmentStatusEndingReasonsGuid = await referenceDataRepo.GetEmploymentStatusEndingReasonsGuidAsync(allEmploymentStatusEndingReason.ElementAt(0).Code);

                Assert.IsNotNull(employmentStatusEndingReasonsGuid);
                Assert.AreEqual(allEmploymentStatusEndingReason.ElementAt(0).Guid, employmentStatusEndingReasonsGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetsEmploymentStatusEndingReasonsGuidAsync_Invalid()
            {
                await referenceDataRepo.GetEmploymentStatusEndingReasonsGuidAsync("invalid");
            }


            [TestMethod]
            public async Task GetsEmploymentStatusEndingReasonsNonCacheAsync()
            {
                var EmploymentStatusEndingReasons = await referenceDataRepo.GetEmploymentStatusEndingReasonsAsync(true);
                for (int i = 0; i < EmploymentStatusEndingReasons.Count(); i++)
                {
                    Assert.AreEqual(allEmploymentStatusEndingReason.ElementAt(i).Code, EmploymentStatusEndingReasons.ElementAt(i).Code);
                    Assert.AreEqual(allEmploymentStatusEndingReason.ElementAt(i).Description, EmploymentStatusEndingReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetEmploymentStatusEndingReasons_WritesToCacheAsync()
            {
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(employmentStatusEndingReasonValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of employment classifications was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentStatusEndingReason>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("HR_TERMINATION_REASONS"), null)).Returns(true);
                var employmentClassifications = await referenceDataRepo.GetEmploymentStatusEndingReasonsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("HR_TERMINATION_REASONS"), null)).Returns(employmentClassifications);
                // Verify that employment termination reasons were returned, which means they came from the "repository".
                Assert.IsTrue(employmentClassifications.Count() == 6);

                // Verify that the employment classification item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EmploymentStatusEndingReason>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "STATUS.ENDING.REASONS", It.IsAny<bool>())).ReturnsAsync(employmentStatusEndingReasonValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var employmentStatusEndingReasons = allEmploymentStatusEndingReason.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "STATUS.ENDING.REASONS", employmentStatusEndingReasons.Code }),
                            new RecordKeyLookupResult() { Guid = employmentStatusEndingReasons.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason> employmentStatusEndingReasons)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in employmentStatusEndingReasons)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Pay classes codes
        /// </summary>
        [TestClass]
        public class PayClassesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<PayClass> allPayClasses;
            string codeItemName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allPayClasses = new TestPayClassRepository().GetPayClasses();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllPayClasses");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allPayClasses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsPayClassesCacheAsync()
            {
                var payClasses = await referenceDataRepo.GetPayClassesAsync(false);

                for (int i = 0; i < allPayClasses.Count(); i++)
                {
                    Assert.AreEqual(allPayClasses.ElementAt(i).Guid, payClasses.ElementAt(i).Guid);
                    Assert.AreEqual(allPayClasses.ElementAt(i).Code, payClasses.ElementAt(i).Code);
                    Assert.AreEqual(allPayClasses.ElementAt(i).Description, payClasses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsPayClassesNonCacheAsync()
            {
                var payClasses = await referenceDataRepo.GetPayClassesAsync(true);

                for (int i = 0; i < allPayClasses.Count(); i++)
                {
                    Assert.AreEqual(allPayClasses.ElementAt(i).Guid, payClasses.ElementAt(i).Guid);
                    Assert.AreEqual(allPayClasses.ElementAt(i).Code, payClasses.ElementAt(i).Code);
                    Assert.AreEqual(allPayClasses.ElementAt(i).Description, payClasses.ElementAt(i).Description);
                }
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to PayClasses read
                var payClassesCollection = new Collection<Payclass>(allPayClasses.Select(record =>
                    new Data.HumanResources.DataContracts.Payclass()
                    {
                        Recordkey = record.Code,
                        PclsDesc = record.Description,
                        RecordGuid = record.Guid,
                        PclsCyclesPerYear = 4,
                        PclsCycleFreq = "FREQ",
                        PclsCycleWorkTimeAmt = 40,
                        PclsCycleWorkTimeUnits = "HRS",
                        PclsActiveFlag = "Y",
                        PclsHrlyOrSlry = "H",
                        PclsYearWorkTimeAmt = 4,
                        PclsYearWorkTimeUnits = "YR"
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Payclass>("PAYCLASS", "", It.IsAny<bool>()))
                    .ReturnsAsync(payClassesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var payClass = allPayClasses.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "PAYCLASS", payClass.Code }),
                            new RecordKeyLookupResult() { Guid = payClass.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Pay Cycles codes
        /// </summary>
        [TestClass]
        public class PayCycles2Tests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            string _cacheKey;
            IEnumerable<PayCycle2> _allPayCycles;
            HumanResourcesReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                // Build responses used for mocking
                _allPayCycles = new TestPayCycles2Repository().GetPayCycles();

                // Build repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _cacheKey = _referenceDataRepo.BuildFullCacheKey("AllPayCycles");
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                _allPayCycles = null;
                _referenceDataRepo = null;
                _cacheKey = null;

            }

            [TestMethod]
            public async Task HrReferenceDataRepository_GetPayCyclesAsync_False()
            {
                var results = await _referenceDataRepo.GetPayCyclesAsync(false);
                Assert.AreEqual(_allPayCycles.Count(), results.Count());

                foreach (var payCycle in _allPayCycles)
                {
                    var result = results.FirstOrDefault(i => i.Guid == payCycle.Guid);

                    Assert.AreEqual(payCycle.Code, result.Code);
                    Assert.AreEqual(payCycle.Description, result.Description);
                    Assert.AreEqual(payCycle.Guid, result.Guid);
                }

            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var ldmGuidPaycyle = new List<string>();
                var records = new Collection<Paycycle>();

                foreach (var item in _allPayCycles)
                {
                    ldmGuidPaycyle.Add(item.Guid);
                    var record = new Paycycle()
                    {
                        Recordkey = item.Code,
                        PcyDesc = item.Description,
                        RecordGuid = item.Guid
                    };
                    records.Add(record);

                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Paycycle>("PAYCYCLE", It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).ReturnsAsync(records);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Paycycle>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(ldmGuidPaycyle.ToArray());

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = _allPayCycles.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(record.Guid,
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = _allPayCycles.Where(e => e.Guid == recordKeyLookup.Guid).FirstOrDefault();
                        result.Add(record.Guid,
                          new GuidLookupResult() { PrimaryKey = record.Code });
                    }
                    return Task.FromResult(result);
                });

                return new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object,
                     loggerMock.Object);

            }
        }

        [TestClass]
        public class PayrollDeductionArrangementChangeReason_GETAll
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<PayrollDeductionArrangementChangeReason> allPayrollDeductionArrangementChangeReason;
            ApplValcodes payrollDeductionArrangementChangeReasonValcodeResponse;
            string valcodeName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build PayrollDeductionArrangementChangeReason used for mocking
                allPayrollDeductionArrangementChangeReason = new TestPayrollDeductionArrangementChangeReasonRepository().GetPayrollDeductionArrangementChangeReasons();
                payrollDeductionArrangementChangeReasonValcodeResponse = BuildValcodeResponse(allPayrollDeductionArrangementChangeReason);

                // Build privacy statuses repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("HR_BENDED_CHANGE_REASONS_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                payrollDeductionArrangementChangeReasonValcodeResponse = null;
                allPayrollDeductionArrangementChangeReason = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsPayrollDeductionArrangementChangeReasonsCacheAsync()
            {
                var payrollDeductionArrangementChangeReasons = await referenceDataRepo.GetPayrollDeductionArrangementChangeReasonsAsync(false);
                for (int i = 0; i < payrollDeductionArrangementChangeReasons.Count(); i++)
                {
                    Assert.AreEqual(allPayrollDeductionArrangementChangeReason.ElementAt(i).Code, payrollDeductionArrangementChangeReasons.ElementAt(i).Code);
                    Assert.AreEqual(allPayrollDeductionArrangementChangeReason.ElementAt(i).Description, payrollDeductionArrangementChangeReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsPayrollDeductionArrangementChangeReasonsNonCacheAsync()
            {
                var payrollDeductionArrangementChangeReasons = await referenceDataRepo.GetPayrollDeductionArrangementChangeReasonsAsync(true);
                for (int i = 0; i < payrollDeductionArrangementChangeReasons.Count(); i++)
                {
                    Assert.AreEqual(allPayrollDeductionArrangementChangeReason.ElementAt(i).Code, payrollDeductionArrangementChangeReasons.ElementAt(i).Code);
                    Assert.AreEqual(allPayrollDeductionArrangementChangeReason.ElementAt(i).Description, payrollDeductionArrangementChangeReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetPayrollDeductionArrangementChangeReasons_WritesToCacheAsync()
            {
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of employment classifications was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<PayrollDeductionArrangementChangeReason>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("HR_BENDED_CHANGE_REASONS"), null)).Returns(true);
                var payrollDeductionArrangementChangeReasons = await referenceDataRepo.GetPayrollDeductionArrangementChangeReasonsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("HR_BENDED_CHANGE_REASONS"), null)).Returns(payrollDeductionArrangementChangeReasons);
                // Verify that employment termination reasons were returned, which means they came from the "repository".
                Assert.IsTrue(payrollDeductionArrangementChangeReasons.Count() == 6);

                // Verify that the employment classification item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<PayrollDeductionArrangementChangeReason>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BENDED.CHANGE.REASONS", It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var payrollDeductionArrangementChangeReasons = allPayrollDeductionArrangementChangeReason.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "BENDED.CHANGE.REASONS", payrollDeductionArrangementChangeReasons.Code }),
                            new RecordKeyLookupResult() { Guid = payrollDeductionArrangementChangeReasons.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason> payrollDeductionArrangementChangeReasons)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in payrollDeductionArrangementChangeReasons)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Job Change Reasons
        /// </summary>
        [TestClass]
        public class JobChangeReasons
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<JobChangeReason> allJobChangeReasons;
            ApplValcodes jobChangeReasonValcodeResponse;
            string valcodeName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build job change reasons responses used for mocking
                allJobChangeReasons = new TestJobChangeReasonRepository().GetJobChangeReasons();
                jobChangeReasonValcodeResponse = BuildValcodeResponse(allJobChangeReasons);

                // Build job change reasons repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("HR_POSITION_ENDING_REASONS_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                jobChangeReasonValcodeResponse = null;
                allJobChangeReasons = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsJobChangeReasonsCacheAsync()
            {
                var jobChangeReasons = await referenceDataRepo.GetJobChangeReasonsAsync(false);
                for (int i = 0; i < jobChangeReasons.Count(); i++)
                {
                    Assert.AreEqual(allJobChangeReasons.ElementAt(i).Code, jobChangeReasons.ElementAt(i).Code);
                    Assert.AreEqual(allJobChangeReasons.ElementAt(i).Description, jobChangeReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsJobChangeReasonsNonCacheAsync()
            {
                var jobChangeReasons = await referenceDataRepo.GetJobChangeReasonsAsync(true);
                for (int i = 0; i < jobChangeReasons.Count(); i++)
                {
                    Assert.AreEqual(allJobChangeReasons.ElementAt(i).Code, jobChangeReasons.ElementAt(i).Code);
                    Assert.AreEqual(allJobChangeReasons.ElementAt(i).Description, jobChangeReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetJobChangeReasons_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(jobChangeReasonValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of job change reasons was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<JobChangeReason>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("HR_POSITION_ENDING_REASONS"), null)).Returns(true);
                var jobChangeReasons = await referenceDataRepo.GetJobChangeReasonsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("HR_POSITION_ENDING_REASONS"), null)).Returns(jobChangeReasons);
                // Verify that job change reasons were returned, which means they came from the "repository".
                Assert.IsTrue(jobChangeReasons.Count() == 4);

                // Verify that the job change reason item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<JobChangeReason>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetJobChangeReasons_GetsCachedJobChangeReasonsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "REHIRE.ELIGIBILITY.CODES" cache item)
                //cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                //cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allJobChangeReasons).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                //dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "REHIRE.ELIGIBILITY.CODES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the job change reasons are returned
                Assert.IsTrue((await referenceDataRepo.GetJobChangeReasonsAsync(false)).Count() == 4);
                // Verify that the rehire types were retrieved from cache
                //cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to job change reason valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "POSITION.ENDING.REASONS", It.IsAny<bool>())).ReturnsAsync(jobChangeReasonValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var jobChangeReason = allJobChangeReasons.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "POSITION.ENDING.REASONS", jobChangeReason.Code }),
                            new RecordKeyLookupResult() { Guid = jobChangeReason.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<JobChangeReason> jobChangeReasons)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in jobChangeReasons)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        //[TestClass]
        //public class LeaveTypesTests
        //{
        //    Mock<IColleagueTransactionFactory> transFactoryMock;
        //    Mock<ICacheProvider> cacheProviderMock;
        //    Mock<IColleagueDataReader> dataAccessorMock;
        //    Mock<ILogger> loggerMock;
        //    IEnumerable<LeaveType> allLeaveTypes;
        //    ApplValcodes leaveTypesValcodeResponse;
        //    string domainEntityNameName;
        //    ApiSettings apiSettings;

        //    Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
        //    IHumanResourcesReferenceDataRepository referenceDataRepository;
        //    HumanResourcesReferenceDataRepository referenceDataRepo;

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        loggerMock = new Mock<ILogger>();
        //        apiSettings = new ApiSettings("TEST");

        //        allLeaveTypes = new TestLeaveTypeRepository().GetLeaveTypes();
        //        leaveTypesValcodeResponse = BuildValcodeResponse(allLeaveTypes);
        //        var leaveTypesValResponse = new List<string>() { "2" };
        //        leaveTypesValcodeResponse.ValActionCode1 = leaveTypesValResponse;

        //        referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
        //        referenceDataRepository = referenceDataRepositoryMock.Object;

        //        referenceDataRepo = BuildValidReferenceDataRepository();
        //        domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_LEAVE.TYPES_GUID");

        //        cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
        //           x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //           .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        transFactoryMock = null;
        //        dataAccessorMock = null;
        //        cacheProviderMock = null;
        //        leaveTypesValcodeResponse = null;
        //        allLeaveTypes = null;
        //        referenceDataRepo = null;
        //    }


        //    [TestMethod]
        //    public async Task HumanResourcesReferenceDataRepo_GetsLeaveTypesCacheAsync()
        //    {
        //        var leaveTypes = await referenceDataRepo.GetLeaveTypesAsync(false);

        //        for (int i = 0; i < allLeaveTypes.Count(); i++)
        //        {
        //            Assert.AreEqual(allLeaveTypes.ElementAt(i).Code, leaveTypes.ElementAt(i).Code);
        //            Assert.AreEqual(allLeaveTypes.ElementAt(i).Description, leaveTypes.ElementAt(i).Description);
        //        }
        //    }

        //    [TestMethod]
        //    public async Task HumanResourcesReferenceDataRepo_GetsLeaveTypesNonCacheAsync()
        //    {
        //        var statuses = await referenceDataRepo.GetLeaveTypesAsync(true);

        //        for (int i = 0; i < allLeaveTypes.Count(); i++)
        //        {
        //            Assert.AreEqual(allLeaveTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
        //            Assert.AreEqual(allLeaveTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
        //        }
        //    }

        //    [TestMethod]
        //    public async Task HumanResourcesReferenceDataRepo_GetLeaveTypes_WritesToCacheAsync()
        //    {

        //        // Set up local cache mock to respond to cache request:
        //        //  -to "Contains" request, return "false" to indicate item is not in cache
        //        //  -to cache "Get" request, return null so we know it's reading from the "repository"
        //        cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
        //        cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

        //        // return a valid response to the data accessor request
        //        dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "LEAVE.TYPES", It.IsAny<bool>())).ReturnsAsync(leaveTypesValcodeResponse);

        //        cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
        //         x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //         .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

        //        // But after data accessor read, set up mocking so we can verify the list of leaveTypes was written to the cache
        //        cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<LeaveType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

        //        cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_LEAVE.TYPES"), null)).Returns(true);
        //        var leaveTypes = await referenceDataRepo.GetLeaveTypesAsync(false);
        //        cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_LEAVE.TYPES"), null)).Returns(leaveTypes);
        //        // Verify that leaveTypes were returned, which means they came from the "repository".
        //        Assert.IsTrue(leaveTypes.Count() == 3);

        //        // Verify that the leaveTypes item was added to the cache after it was read from the repository
        //        cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<LeaveType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

        //    }

        //    [TestMethod]
        //    public async Task HumanResourcesReferenceDataRepo_GetLeaveTypes_GetsCachedLeaveTypesAsync()
        //    {
        //        // Set up local cache mock to respond to cache request:
        //        //  -to "Contains" request, return "true" to indicate item is in cache
        //        //  -to "Get" request, return the cache item (in this case the "LEAVE.TYPES" cache item)
        //        cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
        //        cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allLeaveTypes).Verifiable();

        //        // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
        //        dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "LEAVE.TYPES", true)).ReturnsAsync(new ApplValcodes());

        //        // Assert the leaveTypes are returned
        //        Assert.IsTrue((await referenceDataRepo.GetLeaveTypesAsync(false)).Count() == 3);
        //        // Verify that the sleaveTypes were retrieved from cache
        //        cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
        //    }

        //    private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
        //    {
        //        // transaction factory mock
        //        transFactoryMock = new Mock<IColleagueTransactionFactory>();
        //        // Cache Provider Mock
        //        cacheProviderMock = new Mock<ICacheProvider>();
        //        // Set up data accessor for mocking 
        //        dataAccessorMock = new Mock<IColleagueDataReader>();

        //        // Set up dataAccessorMock as the object for the DataAccessor
        //        transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

        //        // Setup response to leaveTypes domainEntityName read
        //        dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "LEAVE.TYPES", It.IsAny<bool>())).ReturnsAsync(leaveTypesValcodeResponse);
        //        cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //        .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

        //        dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
        //        {
        //            var result = new Dictionary<string, RecordKeyLookupResult>();
        //            foreach (var recordKeyLookup in recordKeyLookups)
        //            {
        //                var leaveTypes = allLeaveTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
        //                result.Add(string.Join("+", new string[] { "ST.LEAVE.TYPES", "LEAVE.TYPES", leaveTypes.Code }),
        //                    new RecordKeyLookupResult() { Guid = leaveTypes.Guid });
        //            }
        //            return Task.FromResult(result);
        //        });

        //        // Construct repository
        //        referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

        //        return referenceDataRepo;
        //    }

        //    private ApplValcodes BuildValcodeResponse(IEnumerable<LeaveType> leaveTypes)
        //    {
        //        ApplValcodes leaveTypesResponse = new ApplValcodes();
        //        leaveTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
        //        foreach (var item in leaveTypes)
        //        {
        //            leaveTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
        //        }
        //        return leaveTypesResponse;
        //    }
        //}

        /// <summary>
        /// Test class for Rehire Types
        /// </summary>
        [TestClass]
        public class RehireTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<RehireType> allRehireTypes;
            ApplValcodes rehireTypeValcodeResponse;
            string valcodeName;

            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build rehire types responses used for mocking
                allRehireTypes = new TestRehireTypeRepository().GetRehireTypes();
                rehireTypeValcodeResponse = BuildValcodeResponse(allRehireTypes);

                // Build rehire types repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("HR_REHIRE_ELIGIBILITY_CODES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                rehireTypeValcodeResponse = null;
                allRehireTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsRehireTypesCacheAsync()
            {
                var rehireTypes = await referenceDataRepo.GetRehireTypesAsync(false);
                for (int i = 0; i < rehireTypes.Count(); i++)
                {
                    Assert.AreEqual(allRehireTypes.ElementAt(i).Code, rehireTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRehireTypes.ElementAt(i).Description, rehireTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsRehireTypesNonCacheAsync()
            {
                var rehireTypes = await referenceDataRepo.GetRehireTypesAsync(true);
                for (int i = 0; i < rehireTypes.Count(); i++)
                {
                    Assert.AreEqual(allRehireTypes.ElementAt(i).Code, rehireTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRehireTypes.ElementAt(i).Description, rehireTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetRehireTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(rehireTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of rehire types was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<RehireType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("HR_REHIRE_ELIGIBILITY_CODES"), null)).Returns(true);
                var rehireTypes = await referenceDataRepo.GetRehireTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("HR_REHIRE_ELIGIBILITY_CODES"), null)).Returns(rehireTypes);
                // Verify that rehire types were returned, which means they came from the "repository".
                Assert.IsTrue(rehireTypes.Count() == 4);

                // Verify that the rehire type item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<RehireType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetRehireTypes_GetsCachedRehireTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "REHIRE.ELIGIBILITY.CODES" cache item)
                //cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                //cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allRehireTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                //dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "REHIRE.ELIGIBILITY.CODES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the rehire types are returned
                Assert.IsTrue((await referenceDataRepo.GetRehireTypesAsync(false)).Count() == 4);
                // Verify that the rehire types were retrieved from cache
                //cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to rehire type valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "REHIRE.ELIGIBILITY.CODES", It.IsAny<bool>())).ReturnsAsync(rehireTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var rehireType = allRehireTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "REHIRE.ELIGIBILITY.CODES", rehireType.Code }),
                            new RecordKeyLookupResult() { Guid = rehireType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<RehireType> rehireTypes)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in rehireTypes)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        [TestClass]
        public class HrStatusesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<HrStatuses> allHrStatuses;
            ApplValcodes contractTypesValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
            IHumanResourcesReferenceDataRepository referenceDataRepository;
            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allHrStatuses = new TestContractTypesReferenceDataRepository().GetHrStatusesAsync();
                contractTypesValcodeResponse = BuildValcodeResponse(allHrStatuses);
                var contractTypesValResponse = new List<string>() { "2" };
                contractTypesValcodeResponse.ValActionCode1 = contractTypesValResponse;

                referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_HR.STATUSES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                contractTypesValcodeResponse = null;
                allHrStatuses = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetsHrStatusesCacheAsync()
            {
                var contractTypes = await referenceDataRepo.GetHrStatusesAsync(false);

                for (int i = 0; i < allHrStatuses.Count(); i++)
                {
                    Assert.AreEqual(allHrStatuses.ElementAt(i).Code, contractTypes.ElementAt(i).Code);
                    Assert.AreEqual(allHrStatuses.ElementAt(i).Description, contractTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetsHrStatusesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetHrStatusesAsync(true);

                for (int i = 0; i < allHrStatuses.Count(); i++)
                {
                    Assert.AreEqual(allHrStatuses.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allHrStatuses.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetHrStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "HR.STATUSES", It.IsAny<bool>())).ReturnsAsync(contractTypesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of contractTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<HrStatuses>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("HR.STATUSES"), null)).Returns(true);
                var contractTypes = await referenceDataRepo.GetHrStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("HR.STATUSES"), null)).Returns(contractTypes);
                // Verify that contractTypes were returned, which means they came from the "repository".
                Assert.IsTrue(contractTypes.Count() == 4);

                // Verify that the contractTypes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<HrStatuses>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetHrStatuses_GetsCachedHrStatusesAsync()
            {
                // Assert the employment classifications are returned
                Assert.IsTrue((await referenceDataRepo.GetHrStatusesAsync(false)).Count() == 4);
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to contractTypes domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "HR.STATUSES", It.IsAny<bool>())).ReturnsAsync(contractTypesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var contractTypes = allHrStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "HR.STATUSES", contractTypes.Code }),
                            new RecordKeyLookupResult() { Guid = contractTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<HrStatuses> contractTypes)
            {
                ApplValcodes contractTypesResponse = new ApplValcodes();
                contractTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in contractTypes)
                {
                    contractTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return contractTypesResponse;
            }
        }

        [TestClass]
        public class TenureTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            public IEnumerable<TenureTypes> allTenureTypes;
            ApplValcodes instructorTenureTypesValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
            IHumanResourcesReferenceDataRepository referenceDataRepository;
            HumanResourcesReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allTenureTypes = new List<TenureTypes>()
                {
                    new TenureTypes("547b8331-a96b-4c06-8427-899e18c7acea","AA", "AA Tenure Type"),
                    new TenureTypes("547b8331-a96b-4c06-8427-899e18c7aceb","BB", "BB Tenure Type")
                };


                instructorTenureTypesValcodeResponse = BuildValcodeResponse(allTenureTypes);
                var instructorTenureTypesValResponse = new List<string>() { "2" };
                instructorTenureTypesValcodeResponse.ValActionCode1 = instructorTenureTypesValResponse;

                referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("HR_TENURE.TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                instructorTenureTypesValcodeResponse = null;
                allTenureTypes = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetsTenureTypesCacheAsync()
            {
                var instructorTenureTypes = await referenceDataRepo.GetTenureTypesAsync(false);

                for (int i = 0; i < allTenureTypes.Count(); i++)
                {
                    Assert.AreEqual(allTenureTypes.ElementAt(i).Code, instructorTenureTypes.ElementAt(i).Code);
                    Assert.AreEqual(allTenureTypes.ElementAt(i).Description, instructorTenureTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetsTenureTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetTenureTypesAsync(true);

                for (int i = 0; i < allTenureTypes.Count(); i++)
                {
                    Assert.AreEqual(allTenureTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allTenureTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetTenureTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "TENURE.TYPES", It.IsAny<bool>())).ReturnsAsync(instructorTenureTypesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of instructorTenureTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<TenureTypes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("HR_TENURE.TYPES"), null)).Returns(true);
                var instructorTenureTypes = await referenceDataRepo.GetTenureTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("HR_TENURE.TYPES"), null)).Returns(instructorTenureTypes);
                // Verify that instructorTenureTypes were returned, which means they came from the "repository".
                Assert.IsTrue(instructorTenureTypes.Count() == 2);

                // Verify that the instructorTenureTypes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<TenureTypes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task HumanResourcesReferenceDataRepo_GetTenureTypes_GetsCachedTenureTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "TENURE.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allTenureTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "TENURE.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the instructorTenureTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetTenureTypesAsync(false)).Count() == 2);
                // Verify that the sinstructorTenureTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private HumanResourcesReferenceDataRepository BuildValidReferenceDataRepository()
            {

                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to instructorTenureTypes domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "TENURE.TYPES", It.IsAny<bool>())).ReturnsAsync(instructorTenureTypesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var instructorTenureTypes = allTenureTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "HR.VALCODES", "TENURE.TYPES", instructorTenureTypes.Code }),
                            new RecordKeyLookupResult() { Guid = instructorTenureTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<TenureTypes> instructorTenureTypes)
            {
                ApplValcodes instructorTenureTypesResponse = new ApplValcodes();
                instructorTenureTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();

                foreach (var item in instructorTenureTypes)
                {
                    var x = new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", "");

                    instructorTenureTypesResponse.ValsEntityAssociation.Add(x);
                }
                return instructorTenureTypesResponse;
            }
        }



        /// <summary>
        /// Test class for PayStatementConfiguration
        /// </summary>
        [TestClass]
        public class PayStatementConfigurationTests : BaseRepositorySetup
        {
            public HumanResourcesReferenceDataRepository repositoryUnderTest;
            public HrwebDefaults hrwebDefaults;
            public Paymstr payMaster;


            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                hrwebDefaults = new HrwebDefaults()
                {
                    HrwebPayAdvicePriorYears = "5",
                    HrwebPayAdviceCheckDate = "-5",
                    HrwebPayAdviceFilingStat = "N",
                    HrwebPayAdviceDisplaySsn = "S"
                };
                payMaster = new Paymstr()
                {
                    PmZeroBendedOnStub = "Y",
                    PmInstitutionName = "Ellucian University",
                    PmInstitutionAddress = new List<string>() { "2003 Edmund Halley Dr.", "Ste 500" },
                    PmInstitutionCity = "Reston",
                    PmInstitutionState = "VA",
                    PmInstitutionZipcode = "20191"
                };

                dataReaderMock.Setup(r => r.ReadRecordAsync<HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS", It.IsAny<bool>()))
                    .Returns<string, string, bool>((f, r, b) => Task.FromResult(hrwebDefaults));

                dataReaderMock.Setup(r => r.ReadRecordAsync<Paymstr>("ACCOUNT.PARAMETERS", "PAYROLL.MASTER", It.IsAny<bool>()))
                    .Returns<string, string, bool>((f, r, b) => Task.FromResult(payMaster));

                // create a new instance of the repo...
                repositoryUnderTest = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ActualsEqualExpectedTest()
            {
                var actuals = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(-5, actuals.OffsetDaysCount);
                Assert.AreEqual(5, actuals.PreviousYearsCount);
                Assert.AreEqual(true, actuals.DisplayZeroAmountBenefitDeductions);
                Assert.AreEqual(false, actuals.DisplayWithholdingStatusFlag);
                Assert.AreEqual(SSNDisplay.Full, actuals.SocialSecurityNumberDisplay);
            }

            [TestMethod]
            public async Task NullHrwebDefaultsRecord_UseDefaultsTest()
            {
                var defaults = new PayStatementConfiguration();

                hrwebDefaults = null;

                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();

                Assert.AreEqual(defaults.OffsetDaysCount, actual.OffsetDaysCount);
                Assert.AreEqual(defaults.PreviousYearsCount, actual.PreviousYearsCount);
                Assert.AreEqual(defaults.DisplayWithholdingStatusFlag, actual.DisplayWithholdingStatusFlag);
                Assert.AreEqual(defaults.SocialSecurityNumberDisplay, actual.SocialSecurityNumberDisplay);
            }

            [TestMethod]
            public async Task UnableToParseOffsetDaysCountTest()
            {
                var defaults = new PayStatementConfiguration();

                hrwebDefaults.HrwebPayAdviceCheckDate = "foobar";

                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();

                Assert.AreEqual(defaults.OffsetDaysCount, actual.OffsetDaysCount);
            }

            [TestMethod]
            public async Task UnableToParsePriorYearsTest()
            {
                var defaults = new PayStatementConfiguration();

                hrwebDefaults.HrwebPayAdvicePriorYears = "foobar";

                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();

                Assert.AreEqual(defaults.PreviousYearsCount, actual.PreviousYearsCount);
            }

            [TestMethod]
            public async Task NullPaymasterRecord_UseDefaultsTest()
            {
                var defaults = new PayStatementConfiguration();

                payMaster = null;

                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();

                Assert.AreEqual(defaults.DisplayZeroAmountBenefitDeductions, actual.DisplayZeroAmountBenefitDeductions);
                Assert.AreEqual(defaults.InstitutionName, actual.InstitutionName);
                CollectionAssert.AreEqual(defaults.InstitutionMailingLabel, actual.InstitutionMailingLabel);
            }

            [TestMethod]
            public async Task ZeroBendedOnStubIsNullTest()
            {
                payMaster.PmZeroBendedOnStub = null;
                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();

                Assert.IsFalse(actual.DisplayZeroAmountBenefitDeductions);
            }

            [TestMethod]
            public async Task ZeroBendedOnStubIsNoTest()
            {
                payMaster.PmZeroBendedOnStub = "N";
                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();

                Assert.IsFalse(actual.DisplayZeroAmountBenefitDeductions);
            }

            [TestMethod]
            public async Task ZeroBendedOnStub_IgnoreCaseTest()
            {
                payMaster.PmZeroBendedOnStub = "y";
                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();

                Assert.IsTrue(actual.DisplayZeroAmountBenefitDeductions);
            }

            [TestMethod]
            public async Task AddressLinesTest()
            {
                var expectLinesCount = payMaster.PmInstitutionAddress.Count + 1; //street address lines + csz

                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(expectLinesCount, actual.InstitutionMailingLabel.Count);
            }

            [TestMethod]
            public async Task NullAddressLinesTest()
            {
                payMaster.PmInstitutionName = null;
                payMaster.PmInstitutionAddress = null;
                payMaster.PmInstitutionCity = null;
                payMaster.PmInstitutionState = null;
                payMaster.PmInstitutionZipcode = null;

                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.IsFalse(actual.InstitutionMailingLabel.Any());
            }

            [TestMethod]
            public async Task NullSocialSecurityNumberDisplayUsesLastFourTest()
            {
                hrwebDefaults.HrwebPayAdviceDisplaySsn = null;
                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(SSNDisplay.LastFour, actual.SocialSecurityNumberDisplay);
            }

            [TestMethod]
            public async Task TranslateDisplaySsnCodesTest()
            {
                hrwebDefaults.HrwebPayAdviceDisplaySsn = "S";
                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(SSNDisplay.Full, actual.SocialSecurityNumberDisplay);

                hrwebDefaults.HrwebPayAdviceDisplaySsn = "L";
                actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(SSNDisplay.LastFour, actual.SocialSecurityNumberDisplay);

                hrwebDefaults.HrwebPayAdviceDisplaySsn = "N";
                actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(SSNDisplay.Hidden, actual.SocialSecurityNumberDisplay);

                hrwebDefaults.HrwebPayAdviceDisplaySsn = "X";
                actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(SSNDisplay.LastFour, actual.SocialSecurityNumberDisplay);
            }

            [TestMethod]
            public async Task NullDisplayWithholdingStatusUsesTrueTest()
            {
                hrwebDefaults.HrwebPayAdviceFilingStat = null;
                var actual = await repositoryUnderTest.GetPayStatementConfigurationAsync();
                Assert.AreEqual(true, actual.DisplayWithholdingStatusFlag);
            }
        }

        [TestClass]
        public class TaxCodesTests : BaseRepositorySetup
        {

            public TestTaxCodeRepository testData;

            async Task<IEnumerable<TaxCode>> getActualTaxCodesAsync()
            {
                return await humanResourcesReferenceDataRepository.GetTaxCodesAsync();
            }

            HumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                testData = new TestTaxCodeRepository();

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Taxcodes>("", It.IsAny<bool>()))
                    .Returns<string, bool>((a, b) =>
                     Task.FromResult(testData.taxCodeRecords == null ? null :
                         new Collection<Taxcodes>(testData.taxCodeRecords.Select(td => new Taxcodes()
                         {
                             Recordkey = td.code,
                             TaxDesc = td.description,
                             TaxFilingStatus = td.filingStatus,
                             TaxTypeCode = td.type,
                         }).ToList()
                     )));

                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "TAXCODE.FILING.STATUSES", It.IsAny<bool>()))
                    .Returns<string, string, bool>((f, r, b) => Task.FromResult(new ApplValcodes()
                    {
                        Recordkey = "TAX.CODE.FILING.STATUSES",
                        ValsEntityAssociation = testData.filingStatusRecords.Select(fs =>
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = fs.code,
                                ValExternalRepresentationAssocMember = fs.description
                            }).ToList()
                    }));

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                humanResourcesReferenceDataRepository = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedDataIsReturned()
            {
                var expected = await testData.GetTaxCodesAsync();
                var taxcodes = await getActualTaxCodesAsync();
                CollectionAssert.AreEqual(expected.ToList(), taxcodes.ToList());
            }

            [TestMethod]
            public async Task TaxCodeFICATypeTest()
            {
                testData.taxCodeRecords.ForEach(tc => tc.type = "FICA");

                Assert.IsTrue((await getActualTaxCodesAsync()).All(tc => tc.Type == TaxCodeType.FicaWithholding));
            }

            [TestMethod]
            public async Task FilingStatusTest()
            {
                var filingStatus = testData.filingStatusRecords[0];

                testData.taxCodeRecords.ForEach(tc => tc.filingStatus = filingStatus.code);
                Assert.IsTrue((await getActualTaxCodesAsync()).All(tc => tc.FilingStatus.Code == filingStatus.code && tc.FilingStatus.Description == filingStatus.description));

            }

            [TestMethod]
            public async Task NoFilingStatusTest()
            {
                testData.taxCodeRecords.ForEach(tc => tc.filingStatus = null);
                Assert.IsTrue((await getActualTaxCodesAsync()).All(tc => tc.FilingStatus == null));
            }

            [TestMethod]
            public async Task CacheProviderIsCalled()
            {
                cacheProviderMock.Setup(x => x.Contains(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()))
                    .Returns(true);
                await getActualTaxCodesAsync();
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()));
            }
            [TestMethod]
            public async Task ErrorCreatingRecordIsLogged()
            {
                testData.taxCodeRecords.ForEach(td => td.description = "");
                var data = await getActualTaxCodesAsync();
                loggerMock.Verify(e => e.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                Assert.IsFalse(data.Any());

            }
        }

        [TestClass]
        public class EarningsDifferentialsTests : BaseRepositorySetup
        {

            public HumanResourcesReferenceDataRepository repositoryUnderTest;
            public TestEarningsDifferentialRepository testData;


            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                testData = new TestEarningsDifferentialRepository();

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Earndiff>("", true))
                    .Returns<string, bool>((s, b) => Task.FromResult(testData.earnDiffRecords == null ? null :
                       new Collection<Earndiff>(testData.earnDiffRecords.Select(diff => new Earndiff()
                       {
                           Recordkey = diff.code,
                           EdfDesc = diff.description
                       }).ToList())));

                repositoryUnderTest = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task GetEarningsDifferentialTest()
            {
                var actual = await repositoryUnderTest.GetEarningsDifferentialsAsync();
                var expected = await testData.GetEarningsDifferentialsAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray());
            }

            [TestMethod]
            public async Task NullRecordsReturnedByBulkReadTest()
            {
                testData.earnDiffRecords = null;
                var actual = await repositoryUnderTest.GetEarningsDifferentialsAsync();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NoRecordsReturnByBulkReadTest()
            {
                testData.earnDiffRecords = new List<TestEarningsDifferentialRepository.EarnDiffRecord>();
                var actual = await repositoryUnderTest.GetEarningsDifferentialsAsync();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task InvalidEarndiffRecord_NotAddedToListTest()
            {
                testData.earnDiffRecords.ForEach(diff => diff.description = null);
                var actual = await repositoryUnderTest.GetEarningsDifferentialsAsync();

                Assert.IsFalse(actual.Any());
            }
        }

        [TestClass]
        public class BenefitDeductionTypeTests : BaseRepositorySetup
        {
            public HumanResourcesReferenceDataRepository repositoryUnderTest;

            public TestBenefitDeductionTypeRepository testData;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                testData = new TestBenefitDeductionTypeRepository();

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Bended>(string.Empty, true))
                    .Returns<string, bool>((s, b) => Task.FromResult(testData.bendedRecords == null ? null :
                        new Collection<Bended>(testData.bendedRecords.Select(bd => new Bended()
                        {
                            Recordkey = bd.code,
                            BdDesc = bd.description,
                            BdInstitutionType = bd.type
                        }).ToList())));

                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BENDED.TYPES", true))
                    .Returns<string, string, bool>((s1, s2, b) =>
                    Task.FromResult(testData.bendedTypes == null ? null : new ApplValcodes()
                    {
                        Recordkey = "BENDED.TYPES",
                        ValsEntityAssociation = testData.bendedTypes.Select(t => new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = t.code,
                            ValExternalRepresentationAssocMember = t.description,
                            ValActionCode2AssocMember = t.specialCode2
                        }).ToList()
                    }));

                repositoryUnderTest = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task GetBenefitDeductionTypesTest()
            {
                var expected = await testData.GetBenefitDeductionTypesAsync();
                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray());
            }

            [TestMethod]
            public async Task NullBendedRecordsReturnedByBulkReadTest()
            {
                testData.bendedRecords = null;

                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task InvalidBenefitDeductionRecord_NoAddTest()
            {
                testData.bendedRecords.ForEach(br => br.description = null);

                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NullTypePointerDefaultsToBenefitTypeTest()
            {
                testData.bendedRecords.ForEach(br => br.type = null);
                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsTrue(actual.All(b => b.Category == BenefitDeductionTypeCategory.Benefit));
            }

            [TestMethod]
            public async Task InvalidTypePointerDefaultsToBenefitTypeTest()
            {
                testData.bendedRecords.ForEach(br => br.type = "foobar");

                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsTrue(actual.All(b => b.Category == BenefitDeductionTypeCategory.Benefit));
            }

            [TestMethod]
            public async Task InvalidBendedTypesValcode_NoRecordsReturnedTest()
            {
                testData.bendedTypes = null;
                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task TranslateCodeToDeductionTest()
            {
                testData.bendedTypes.ForEach(b => b.specialCode2 = "D");

                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsTrue(actual.All(b => b.Category == BenefitDeductionTypeCategory.Deduction));

            }

            [TestMethod]
            public async Task TranslateCodeToBenefitTest()
            {
                testData.bendedTypes.ForEach(b => b.specialCode2 = "B");

                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsTrue(actual.All(b => b.Category == BenefitDeductionTypeCategory.Benefit));

            }

            [TestMethod]
            public async Task TranslateInvalidCodeToDefaultBenefitTest()
            {
                testData.bendedTypes.ForEach(b => b.specialCode2 = "foo");

                var actual = await repositoryUnderTest.GetBenefitDeductionTypesAsync();

                Assert.IsTrue(actual.All(b => b.Category == BenefitDeductionTypeCategory.Benefit));

            }



        }

        [TestClass]
        public class EarningsTypeGroupTests : BaseRepositorySetup
        {
            public HumanResourcesReferenceDataRepository repositoryUnderTest;

            public TestEarningsTypeGroupRepository testData;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                testData = new TestEarningsTypeGroupRepository();

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<EarntypeGroupings>("", true))
                    .Returns<string, bool>((s, b) => Task.FromResult(new Collection<EarntypeGroupings>(testData.earningsTypeGroupDataContracts)));

                repositoryUnderTest = new HumanResourcesReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task EarningsTypeGroupsTest()
            {
                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();

                Assert.IsNotNull(actual);
                Assert.IsTrue(actual.Any());
            }

            [TestMethod]
            public async Task NoEarnTypeGroupingsRecordsTest()
            {
                testData.earningsTypeGroupDataContracts = new List<EarntypeGroupings>();

                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task IsEnabledForTimeManagementTest()
            {
                testData.earningsTypeGroupDataContracts[0].EtpgUseInSelfService = "Y";
                testData.earningsTypeGroupDataContracts[1].EtpgUseInSelfService = "y";

                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();

                Assert.IsTrue(actual[testData.earningsTypeGroupDataContracts[0].Recordkey].IsEnabledForTimeManagement);

                Assert.IsTrue(actual[testData.earningsTypeGroupDataContracts[1].Recordkey].IsEnabledForTimeManagement);
            }

            [TestMethod]
            public async Task IsDisabledForTimeManagementTest()
            {
                testData.earningsTypeGroupDataContracts[0].EtpgUseInSelfService = "n";
                testData.earningsTypeGroupDataContracts[1].EtpgUseInSelfService = null;
                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();

                Assert.IsFalse(actual[testData.earningsTypeGroupDataContracts[0].Recordkey].IsEnabledForTimeManagement);

                Assert.IsFalse(actual[testData.earningsTypeGroupDataContracts[1].Recordkey].IsEnabledForTimeManagement);

            }

            [TestMethod]
            public async Task InvalidRecordsNotAddedTest()
            {
                testData.earningsTypeGroupDataContracts[0].Recordkey = null;
                testData.earningsTypeGroupDataContracts[1].Recordkey = "";
                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();

                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task DuplicateEarningsTypeGroupRecordNotAddedTest()
            {
                testData.earningsTypeGroupDataContracts.Add(new EarntypeGroupings()
                {
                    Recordkey = testData.earningsTypeGroupDataContracts[0].Recordkey,
                    EtpgDesc = "foo"
                });

                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();

                var countOfDuplicatedItems = actual.Values
                    .Where(v => v.EarningsTypeGroupId == testData.earningsTypeGroupDataContracts[0].Recordkey).Count();
                Assert.AreEqual(1, countOfDuplicatedItems);
            }


            [TestMethod]
            public async Task InvalidEarningsTypeGroupItemNotAddedTest()
            {
                testData.earningsTypeGroupDataContracts[0].EtpgEarntype = new List<string>() { null, "", " " };
                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();

                Assert.IsFalse(actual[testData.earningsTypeGroupDataContracts[0].Recordkey].EarningsTypeGroupItems.Any());
            }

            [TestMethod]
            public async Task DuplicateEarningsTypeGroupItemNotAddedTest()
            {
                var duplicateItemId = testData.earningsTypeGroupDataContracts[0].EtpgEarntype[0];
                testData.earningsTypeGroupDataContracts[0].EtpgEarntype.Add(duplicateItemId);
                testData.earningsTypeGroupDataContracts[0].EtpgEarntypeDesc.Add("duplicate description");

                var actual = await repositoryUnderTest.GetEarningsTypesGroupsAsync();
                var actualDuplicateItemsCount = actual[testData.earningsTypeGroupDataContracts[0].Recordkey].EarningsTypeGroupItems
                    .Where(item => item.EarningsTypeId == duplicateItemId).Count();
                Assert.AreEqual(1, actualDuplicateItemsCount);
            }

        }
    }
}