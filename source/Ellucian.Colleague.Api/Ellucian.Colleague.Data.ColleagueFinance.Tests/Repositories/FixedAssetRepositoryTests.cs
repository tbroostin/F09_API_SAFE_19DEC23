using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class FixedAssetsRepositoryTests
    {
        [TestClass]
        public class FixedAssetsRepositoryTests_V12
        {
            [TestClass]
            public class FixedAssetsRepositoryTests_GETALL_GETBYID : BaseRepositorySetup
            {
                #region DECLARATIONS
                
                private FixedAssetsRepository fixedAssetsRepository;
                private Collection<DataContracts.FixedAssets> fixedAssets;
                private Collection<Insurance> insurances;
                private Collection<CalcMethods> calculationMethods;
                private Collection<DataContracts.GlAccts> generalLedgerAccounts;                
                private Dictionary<string, GuidLookupResult> lookUpResult;
                
                private string guid = "adcbf49c-f129-470c-aa31-272493846751";

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    MockInitialize();

                    InitializeTestData();

                    InitializeTestMock();
                    
                    fixedAssetsRepository = new FixedAssetsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                }

                [TestCleanup]
                public void Cleanup()
                {
                    MockCleanup();

                    fixedAssetsRepository = null;

                }

                private void InitializeTestData()
                {                    
                    lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "FIXED.ASSETS", PrimaryKey = "1" } } };
                   
                    fixedAssets = new Collection<DataContracts.FixedAssets>()
                    {
                        new DataContracts.FixedAssets()
                        {
                            Recordkey = "CED",
                            RecordGuid = "88bf5fe1-a655-4ee6-97b7-6e9e1236b928",
                            FixAssetType = "BLG",
                            FixCapitalize = "C",
                            FixAssetCategory = "AC",
                            FixDesc = "Continuing Ed building",
                            FixAcquisMethod = "PR",
                            FixAcquisCost = 100000,
                            FixSalvageValue = 90000,
                            FixUsefulLife = 50,
                            FixCalcMethod = "1",
                            FixInsuranceId = "1",
                            FixPropertyTag = "CED",
                            FixCalcAcct = "11_00_01_00_00000_10110"
                        },
                        new DataContracts.FixedAssets()
                        {
                            Recordkey = "CHP",
                            RecordGuid = "4a5801ba-fabd-4619-af75-6ca0adc580b2",
                            FixAssetType = "BLG",
                            FixCapitalize = "C",
                            FixAssetCategory = "RL",
                            FixDesc = "Chapel",
                            FixAcquisMethod = "PR",
                            FixAcquisCost = 100000,
                            FixSalvageValue = 90000,
                            FixUsefulLife = 50,
                            FixCalcMethod = "2",
                            FixInsuranceId = "2",
                            FixPropertyTag = "CHP",
                            FixCalcAcct = "22_00_01_00_00000_10110"
                        },
                        new DataContracts.FixedAssets()
                        {
                            Recordkey = "DIN",
                            RecordGuid = "efeb7bc4-0a17-4b5c-b44f-a09f70bc9fdb",
                            FixAssetType = "BLG",
                            FixCapitalize = "C",
                            FixAssetCategory = "SS",
                            FixDesc = "Dining Hall",
                            FixAcquisMethod = "PR",
                            FixAcquisCost = 100000,
                            FixSalvageValue = 90000,
                            FixUsefulLife = 50,
                            FixCalcMethod = "3",
                            FixInsuranceId = "3",
                            FixPropertyTag = "DIN",
                            FixCalcAcct = "33_00_01_00_00000_10110"
                        }
                    };
                    
                    insurances = new Collection<Insurance>()
                    {
                        new Insurance() { Recordkey = "1", InsAmtCoverage = 5000 },
                        new Insurance() { Recordkey = "2", InsAmtCoverage = 15000 },
                        new Insurance() { Recordkey = "3", InsAmtCoverage = 30000 }
                    };

                    calculationMethods = new Collection<CalcMethods>()
                    {
                        new CalcMethods() { Recordkey = "1", CalcDesc = "Calculation method 1" },
                        new CalcMethods() { Recordkey = "2", CalcDesc = "Calculation method 2" },
                        new CalcMethods() { Recordkey = "3", CalcDesc = "Calculation method 3" }
                    };

                    generalLedgerAccounts = new Collection<DataContracts.GlAccts>()
                    {
                        new DataContracts.GlAccts() { Recordkey = "11_00_01_00_00000_10110", RecordGuid = "37a4f999-0e08-4156-be77-0407378f3fe0" },
                        new DataContracts.GlAccts() { Recordkey = "22_00_01_00_00000_10110", RecordGuid = "9bd60526-a914-404d-bc49-9e4ca77b57c1" },
                        new DataContracts.GlAccts() { Recordkey = "33_00_01_00_00000_10110", RecordGuid = "a80278ec-5230-4289-976f-ef6d6b42389b" }
                    };
                }

                private void InitializeTestMock()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.FixedAssets>(It.IsAny<string>(), true)).ReturnsAsync(fixedAssets.FirstOrDefault());
                    dataReaderMock.Setup(d => d.SelectAsync("FIXED.ASSETS", It.IsAny<string>())).ReturnsAsync(new List<string>() { "CED", "CHP", "DIN" }.ToArray<string>());                    
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.FixedAssets>("FIXED.ASSETS", It.IsAny<string[]>(), true)).ReturnsAsync(fixedAssets);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Insurance>("INSURANCE", It.IsAny<string[]>(), true)).ReturnsAsync(insurances);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<CalcMethods>("CALC.METHODS", string.Empty, true)).ReturnsAsync(calculationMethods);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", It.IsAny<string[]>(), true)).ReturnsAsync(generalLedgerAccounts);                    
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                }

                #endregion

                #region GETALL

                [TestMethod]
                public async Task FixedAssetsRepository_GetFixedAssetstsAsync()
                {
                    var result = await fixedAssetsRepository.GetFixedAssetsAsync(0, 10, false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 3);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task FixedAssetsRepository_GetFixedAssetstsAsync_KeyNotFound_Insurance()
                {
                    var insurances = new Collection<Insurance>()
                    {
                        new Insurance() { Recordkey = "2", InsAmtCoverage = 15000 }
                    };
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Insurance>("INSURANCE", It.IsAny<string[]>(), true)).ReturnsAsync(insurances);

                    var result = await fixedAssetsRepository.GetFixedAssetsAsync(0, 10, false);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task FixedAssetsRepository_GetFixedAssetstsAsync_KeyNotFound_CalculationMethod()
                {
                    calculationMethods = new Collection<CalcMethods>()
                    {
                        new CalcMethods() { Recordkey = "2", CalcDesc = "Calculation method 2" }
                    };
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<CalcMethods>("CALC.METHODS", string.Empty, true)).ReturnsAsync(calculationMethods);

                    var result = await fixedAssetsRepository.GetFixedAssetsAsync(0, 10, false);
                }


                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task FixedAssetsRepository_GetFixedAssetstsAsync_KeyNotFound_GeneralLedgerAccount()
                {
                    generalLedgerAccounts = new Collection<DataContracts.GlAccts>()
                    {
                        new DataContracts.GlAccts() { Recordkey = "22_00_01_00_00000_10110", RecordGuid = "9bd60526-a914-404d-bc49-9e4ca77b57c1" }
                    };
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", It.IsAny<string[]>(), true)).ReturnsAsync(generalLedgerAccounts);

                    var result = await fixedAssetsRepository.GetFixedAssetsAsync(0, 10, false);
                }

                #endregion

                #region GETBYID

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task FixedAssetsRepository_GetFixedAssetById_Guid_Null()
                {
                    await fixedAssetsRepository.GetFixedAssetByIdAsync(null);
                }


                [TestMethod]
                public async Task FixedAssetsRepository_GetFixedAssetByIdAsync()
                {
                    var result = await fixedAssetsRepository.GetFixedAssetByIdAsync(guid);
                    Assert.IsNotNull(result);
                }

                #endregion
            }
        }
    }
    
}



