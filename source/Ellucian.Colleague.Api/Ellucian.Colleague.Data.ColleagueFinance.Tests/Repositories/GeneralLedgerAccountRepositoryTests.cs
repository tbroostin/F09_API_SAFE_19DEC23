// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class GeneralLedgerAccountRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private GeneralLedgerAccountRepository actualRepository;
        private GetGlAccountDescriptionResponse glAccountsDescriptionResponse;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
        private TestGlAccountRepository testGlAccountRepository;
        private TestGeneralLedgerAccountRepository testGeneralLedgerAccountRepository;
        private List<string> majorComponentStartPositions = new List<string>() { "1", "4", "7", "10", "13", "19" };
        private Glnodisp glNoDispontract = new Glnodisp()
        {
            Recordkey = "SS",
            DisplayPieces = new List<string>()
        };

        private GlAccts glAccountRecord;
        private GlAccountValidationResponse glAccountValidationResponse;
        private string glAccount = "11_00_01_02_ACTIV_50000";
        private string fiscalYear = DateTime.Now.Year.ToString();

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            testGlAccountRepository = new TestGlAccountRepository();
            testGeneralLedgerAccountRepository = new TestGeneralLedgerAccountRepository();
            actualRepository = new GeneralLedgerAccountRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            glAccountRecord = new GlAccts();
            glAccountsDescriptionResponse = new GetGlAccountDescriptionResponse();
            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
            glAccountRecord = null;
            glAccountValidationResponse = null;
        }
        #endregion

        #region GetGlAccountDescriptionsAsync
        #region Success cases
        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_FundOnly()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "FD" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var fundComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Fund);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var fdDescsContract in testGlConfigurationRepository.FdDescs)
            {
                var glAccountsWithFundCode = glAccountDescriptions.Where(x =>
                    x.Key.Substring(fundComponent.StartPosition, fundComponent.ComponentLength) == fdDescsContract.Recordkey).ToList();

                foreach (var glAccountDescription in glAccountsWithFundCode)
                {
                    Assert.AreEqual(fdDescsContract.FdDescription, glAccountDescription.Value);
                }
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_SourceOnly()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "SO" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var sourceComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Source);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var soDescsContract in testGlConfigurationRepository.SoDescs)
            {
                var glAccountsWithSourceCode = glAccountDescriptions.Where(x =>
                    x.Key.Substring(sourceComponent.StartPosition, sourceComponent.ComponentLength) == soDescsContract.Recordkey).ToList();

                foreach (var glAccountDescription in glAccountsWithSourceCode)
                {
                    Assert.AreEqual(soDescsContract.SoDescription, glAccountDescription.Value);
                }
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_LocationOnly()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "LO" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var locationComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Location);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var loDescsContract in testGlConfigurationRepository.LoDescs)
            {
                var glAccountsWithLocationCode = glAccountDescriptions.Where(x =>
                    x.Key.Substring(locationComponent.StartPosition, locationComponent.ComponentLength) == loDescsContract.Recordkey).ToList();

                foreach (var glAccountDescription in glAccountsWithLocationCode)
                {
                    Assert.AreEqual(loDescsContract.LoDescription, glAccountDescription.Value);
                }
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_FunctionOnly()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "FC" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var functionComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Function);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var fcDescsContract in testGlConfigurationRepository.FcDescs)
            {
                var glAccountsWithFunctionCode = glAccountDescriptions.Where(x =>
                    x.Key.Substring(functionComponent.StartPosition, functionComponent.ComponentLength) == fcDescsContract.Recordkey).ToList();

                foreach (var glAccountDescription in glAccountsWithFunctionCode)
                {
                    Assert.AreEqual(fcDescsContract.FcDescription, glAccountDescription.Value);
                }
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_UnitOnly()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "UN" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var unitComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Unit);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var unDescsContract in testGlConfigurationRepository.UnDescs)
            {
                var glAccountsWithUnitCode = glAccountDescriptions.Where(x =>
                    x.Key.Substring(unitComponent.StartPosition, unitComponent.ComponentLength) == unDescsContract.Recordkey).ToList();

                foreach (var glAccountDescription in glAccountsWithUnitCode)
                {
                    Assert.AreEqual(unDescsContract.UnDescription, glAccountDescription.Value);
                }
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_ObjectOnly()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "OB" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var objectComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var obDescsContract in testGlConfigurationRepository.ObDescs)
            {
                var glAccountsWithObjectCode = glAccountDescriptions.Where(x =>
                    x.Key.Substring(objectComponent.StartPosition, objectComponent.ComponentLength) == obDescsContract.Recordkey).ToList();

                foreach (var glAccountDescription in glAccountsWithObjectCode)
                {
                    Assert.AreEqual(obDescsContract.ObDescription, glAccountDescription.Value);
                }
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_FundAndLocation()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "FD", "LO" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var fundComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Fund);
            var locationComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Location);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var fundId = glAccountDescription.Key.Substring(fundComponent.StartPosition, fundComponent.ComponentLength);
                var locationId = glAccountDescription.Key.Substring(locationComponent.StartPosition, locationComponent.ComponentLength);

                var fundDescription = testGlConfigurationRepository.FdDescs.FirstOrDefault(x => x.Recordkey == fundId).FdDescription;
                var locationDescription = testGlConfigurationRepository.LoDescs.FirstOrDefault(x => x.Recordkey == locationId).LoDescription;

                Assert.AreEqual(fundDescription + " : " + locationDescription, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_UnitAndObject()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "UN", "OB" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var unitComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Unit);
            var objectComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var unitId = glAccountDescription.Key.Substring(unitComponent.StartPosition, unitComponent.ComponentLength);
                var objectId = glAccountDescription.Key.Substring(objectComponent.StartPosition, objectComponent.ComponentLength);

                var unitDescription = testGlConfigurationRepository.UnDescs.FirstOrDefault(x => x.Recordkey == unitId).UnDescription;
                var objectDescription = testGlConfigurationRepository.ObDescs.FirstOrDefault(x => x.Recordkey == objectId).ObDescription;

                Assert.AreEqual(unitDescription + " : " + objectDescription, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_AllMajorComponents()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "FD", "SO", "LO", "FC", "UN", "OB" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var fundComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Fund);
            var sourceComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Source);
            var locationComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Location);
            var functionComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Function);
            var unitComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Unit);
            var objectComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var fundId = glAccountDescription.Key.Substring(fundComponent.StartPosition, fundComponent.ComponentLength);
                var sourceId = glAccountDescription.Key.Substring(sourceComponent.StartPosition, sourceComponent.ComponentLength);
                var locationId = glAccountDescription.Key.Substring(locationComponent.StartPosition, locationComponent.ComponentLength);
                var functionId = glAccountDescription.Key.Substring(functionComponent.StartPosition, functionComponent.ComponentLength);
                var unitId = glAccountDescription.Key.Substring(unitComponent.StartPosition, unitComponent.ComponentLength);
                var objectId = glAccountDescription.Key.Substring(objectComponent.StartPosition, objectComponent.ComponentLength);

                var fundDescription = testGlConfigurationRepository.FdDescs.FirstOrDefault(x => x.Recordkey == fundId).FdDescription;
                var sourceDescription = testGlConfigurationRepository.SoDescs.FirstOrDefault(x => x.Recordkey == sourceId).SoDescription;
                var locationDescription = testGlConfigurationRepository.LoDescs.FirstOrDefault(x => x.Recordkey == locationId).LoDescription;
                var functionDescription = testGlConfigurationRepository.FcDescs.FirstOrDefault(x => x.Recordkey == functionId).FcDescription;
                var unitDescription = testGlConfigurationRepository.UnDescs.FirstOrDefault(x => x.Recordkey == unitId).UnDescription;
                var objectDescription = testGlConfigurationRepository.ObDescs.FirstOrDefault(x => x.Recordkey == objectId).ObDescription;

                var expecteDescription = fundDescription + " : " + sourceDescription + " : " + locationDescription + " : "
                    + functionDescription + " : " + unitDescription + " : " + objectDescription;
                Assert.AreEqual(expecteDescription, glAccountDescription.Value);
            }
        }
        #endregion

        #region Error cases
        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_GlnodispContractIsNull_DefaultComponentsAreUnitAndObject()
        {
            glNoDispontract = null;
            var glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));

            var defaultComponent1 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Unit);
            var defaultComponent2 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);

            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var defaultComponentId1 = glAccountDescription.Key.Substring(defaultComponent1.StartPosition, defaultComponent1.ComponentLength);
                var defaultComponentId2 = glAccountDescription.Key.Substring(defaultComponent2.StartPosition, defaultComponent2.ComponentLength);

                var defaultComponentDescription1 = testGlConfigurationRepository.UnDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId1).UnDescription;
                var defaultComponentDescription2 = testGlConfigurationRepository.ObDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId2).ObDescription;

                Assert.AreEqual(defaultComponentDescription1 + " : " + defaultComponentDescription2, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_GlnodispContractIsNull_DefaultComponentsAreFundAndSource()
        {
            glNoDispontract = null;
            var glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE));

            var defaultComponent1 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Fund);
            var defaultComponent2 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Source);

            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var defaultComponentId1 = glAccountDescription.Key.Substring(defaultComponent1.StartPosition, defaultComponent1.ComponentLength);
                var defaultComponentId2 = glAccountDescription.Key.Substring(defaultComponent2.StartPosition, defaultComponent2.ComponentLength);

                var defaultComponentDescription1 = testGlConfigurationRepository.FdDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId1).FdDescription;
                var defaultComponentDescription2 = testGlConfigurationRepository.SoDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId2).SoDescription;

                Assert.AreEqual(defaultComponentDescription1 + " : " + defaultComponentDescription2, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_GlnodispContractIsNull_DefaultComponentsAreFunctionAndLocation()
        {
            glNoDispontract = null;
            var glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));

            var defaultComponent1 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Function);
            var defaultComponent2 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Location);

            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var defaultComponentId1 = glAccountDescription.Key.Substring(defaultComponent1.StartPosition, defaultComponent1.ComponentLength);
                var defaultComponentId2 = glAccountDescription.Key.Substring(defaultComponent2.StartPosition, defaultComponent2.ComponentLength);

                var defaultComponentDescription1 = testGlConfigurationRepository.FcDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId1).FcDescription;
                var defaultComponentDescription2 = testGlConfigurationRepository.LoDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId2).LoDescription;

                Assert.AreEqual(defaultComponentDescription1 + " : " + defaultComponentDescription2, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_GlnodispDisplayPiecesIsNull_DefaultComponentsAreFunctionAndLocation()
        {
            glNoDispontract.DisplayPieces = null;
            var glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));

            var defaultComponent1 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Function);
            var defaultComponent2 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Location);

            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var defaultComponentId1 = glAccountDescription.Key.Substring(defaultComponent1.StartPosition, defaultComponent1.ComponentLength);
                var defaultComponentId2 = glAccountDescription.Key.Substring(defaultComponent2.StartPosition, defaultComponent2.ComponentLength);

                var defaultComponentDescription1 = testGlConfigurationRepository.FcDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId1).FcDescription;
                var defaultComponentDescription2 = testGlConfigurationRepository.LoDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId2).LoDescription;

                Assert.AreEqual(defaultComponentDescription1 + " : " + defaultComponentDescription2, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_GlnodispDisplayPiecesIsEmpty_DefaultComponentsAreFunctionAndLocation()
        {
            glNoDispontract.DisplayPieces = new List<string>();
            var glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));

            var defaultComponent1 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Function);
            var defaultComponent2 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Location);

            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var defaultComponentId1 = glAccountDescription.Key.Substring(defaultComponent1.StartPosition, defaultComponent1.ComponentLength);
                var defaultComponentId2 = glAccountDescription.Key.Substring(defaultComponent2.StartPosition, defaultComponent2.ComponentLength);

                var defaultComponentDescription1 = testGlConfigurationRepository.FcDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId1).FcDescription;
                var defaultComponentDescription2 = testGlConfigurationRepository.LoDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId2).LoDescription;

                Assert.AreEqual(defaultComponentDescription1 + " : " + defaultComponentDescription2, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_GlAccountStructureIsNull()
        {
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, null);
            Assert.AreEqual(0, glAccountDescriptions.Count);
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_GlAccountStructureMajorComponentsListIsEmpty()
        {
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, new GeneralLedgerAccountStructure());
            Assert.AreEqual(0, glAccountDescriptions.Count);
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_FundDescsBulkReadReturnsNull()
        {
            testGlConfigurationRepository.FdDescs = null;
            glNoDispontract.DisplayPieces = new List<string>() { "FD" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            // None of the GL numbers should have a description since no descriptions were returned from the bulk read.
            Assert.AreEqual(0, glAccountDescriptions.Where(x => !string.IsNullOrEmpty(x.Value)).Count());
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_FunctionDescsBulkReadReturnsNull()
        {
            testGlConfigurationRepository.FcDescs = null;
            glNoDispontract.DisplayPieces = new List<string>() { "FC" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            // None of the GL numbers should have a description since no descriptions were returned from the bulk read.
            Assert.AreEqual(0, glAccountDescriptions.Where(x => !string.IsNullOrEmpty(x.Value)).Count());
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_ObjectDescsBulkReadReturnsNull()
        {
            testGlConfigurationRepository.ObDescs = null;
            glNoDispontract.DisplayPieces = new List<string>() { "OB" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            // None of the GL numbers should have a description since no descriptions were returned from the bulk read.
            Assert.AreEqual(0, glAccountDescriptions.Where(x => !string.IsNullOrEmpty(x.Value)).Count());
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_UnitDescsBulkReadReturnsNull()
        {
            testGlConfigurationRepository.UnDescs = null;
            glNoDispontract.DisplayPieces = new List<string>() { "UN" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            // None of the GL numbers should have a description since no descriptions were returned from the bulk read.
            Assert.AreEqual(0, glAccountDescriptions.Where(x => !string.IsNullOrEmpty(x.Value)).Count());
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_SourceDescsBulkReadReturnsNull()
        {
            testGlConfigurationRepository.SoDescs = null;
            glNoDispontract.DisplayPieces = new List<string>() { "SO" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            // None of the GL numbers should have a description since no descriptions were returned from the bulk read.
            Assert.AreEqual(0, glAccountDescriptions.Where(x => !string.IsNullOrEmpty(x.Value)).Count());
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_LocationDescsBulkReadReturnsNull()
        {
            testGlConfigurationRepository.LoDescs = null;
            glNoDispontract.DisplayPieces = new List<string>() { "LO" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            // None of the GL numbers should have a description since no descriptions were returned from the bulk read.
            Assert.AreEqual(0, glAccountDescriptions.Where(x => !string.IsNullOrEmpty(x.Value)).Count());
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_InvalidMajorComponentId()
        {
            glNoDispontract.DisplayPieces = new List<string>() { "ZZ" };
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            // None of the GL numbers should have a description since no descriptions were returned from the bulk read.
            Assert.AreEqual(0, glAccountDescriptions.Where(x => !string.IsNullOrEmpty(x.Value)).Count());
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_OnlyTwoMajorComponentsInAccountStructure()
        {
            var glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));

            var defaultComponent1 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Unit);
            var defaultComponent2 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);

            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var defaultComponentId1 = glAccountDescription.Key.Substring(defaultComponent1.StartPosition, defaultComponent1.ComponentLength);
                var defaultComponentId2 = glAccountDescription.Key.Substring(defaultComponent2.StartPosition, defaultComponent2.ComponentLength);

                var defaultComponentDescription1 = testGlConfigurationRepository.UnDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId1).UnDescription;
                var defaultComponentDescription2 = testGlConfigurationRepository.ObDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId2).ObDescription;

                Assert.AreEqual(defaultComponentDescription1 + " : " + defaultComponentDescription2, glAccountDescription.Value);
            }
        }

        [TestMethod]
        public async Task GetGlAccountDescriptionsAsync_OnlyOneMajorComponentInAccountStructure()
        {
            var glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            var defaultComponent2 = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);

            var glAccountDescriptions = await actualRepository.GetGlAccountDescriptionsAsync(testGlAccountRepository.AllGlNumbers, glAccountStructure);

            foreach (var glAccountDescription in glAccountDescriptions)
            {
                var defaultComponentId = glAccountDescription.Key.Substring(defaultComponent2.StartPosition, defaultComponent2.ComponentLength);
                var defaultComponentDescription = testGlConfigurationRepository.ObDescs.FirstOrDefault(x => x.Recordkey == defaultComponentId).ObDescription;

                Assert.AreEqual(defaultComponentDescription, glAccountDescription.Value);
            }
        }
        #endregion
        #endregion

        #region GetAsync
        [TestMethod]
        public async Task GetAsync_HappyPath()
        {
            glAccountsDescriptionResponse.GlAccountIds.Add("11_00_01_00_20601_51000");
            glAccountsDescriptionResponse.GlDescriptions.Add("Operating Fund : South Campus");
            var glAccountEntity = await actualRepository.GetAsync(glAccountsDescriptionResponse.GlAccountIds.First(), new List<string>());

            Assert.AreEqual(glAccountsDescriptionResponse.GlAccountIds.First(), glAccountEntity.Id);
            Assert.AreEqual(glAccountsDescriptionResponse.GlDescriptions.First(), glAccountEntity.Description);
        }

        [TestMethod]
        public async Task GetAsync_NullGlAccountId()
        {
            var expectedParam = "generalledgeraccountid";
            var actualParam = "";
            try
            {
                string glAccountId = null;
                await actualRepository.GetAsync(glAccountId, majorComponentStartPositions);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_EmptyGlAccountId()
        {
            var expectedParam = "generalledgeraccountid";
            var actualParam = "";
            try
            {
                await actualRepository.GetAsync("", majorComponentStartPositions);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_CtxResponseIsNull()
        {
            var inputGlAccountId = "11_00_01_00_20601_55555";
            glAccountsDescriptionResponse = null;
            var glAccountEntity = await actualRepository.GetAsync(inputGlAccountId, majorComponentStartPositions);

            Assert.AreEqual(inputGlAccountId, glAccountEntity.Id);
            Assert.AreEqual("", glAccountEntity.Description);
        }

        [TestMethod]
        public async Task GetAsync_ResponseIdsIsNull()
        {
            var inputGlAccountId = "11_00_01_00_20601_55555";
            glAccountsDescriptionResponse.GlAccountIds = null;
            var glAccountEntity = await actualRepository.GetAsync(inputGlAccountId, majorComponentStartPositions);

            Assert.AreEqual(inputGlAccountId, glAccountEntity.Id);
            Assert.AreEqual("", glAccountEntity.Description);
        }

        [TestMethod]
        public async Task GetAsync_ResponseDescriptionAreNull()
        {
            var inputGlAccountId = "11_00_01_00_20601_55555";
            glAccountsDescriptionResponse.GlAccountIds.Add(inputGlAccountId);
            glAccountsDescriptionResponse.GlDescriptions = null;
            var glAccountEntity = await actualRepository.GetAsync(inputGlAccountId, majorComponentStartPositions);

            Assert.AreEqual(inputGlAccountId, glAccountEntity.Id);
            Assert.AreEqual("", glAccountEntity.Description);
        }

        [TestMethod]
        public async Task GetAsync_ResponseIdsAndDescriptionAreNull()
        {
            var inputGlAccountId = "11_00_01_00_20601_55555";
            glAccountsDescriptionResponse.GlAccountIds = null;
            glAccountsDescriptionResponse.GlDescriptions = null;
            var glAccountEntity = await actualRepository.GetAsync(inputGlAccountId, majorComponentStartPositions);

            Assert.AreEqual(inputGlAccountId, glAccountEntity.Id);
            Assert.AreEqual("", glAccountEntity.Description);
        }

        [TestMethod]
        public async Task GetAsync_GlAccountIdNotIsResponse()
        {
            var inputGlAccountId = "11_00_01_00_20601_55555";
            glAccountsDescriptionResponse.GlAccountIds.Add("11_00_01_00_20601_51000");
            glAccountsDescriptionResponse.GlDescriptions.Add("Operating Fund : South Campus");
            var glAccountEntity = await actualRepository.GetAsync(inputGlAccountId, majorComponentStartPositions);

            Assert.AreEqual(inputGlAccountId, glAccountEntity.Id);
            Assert.AreEqual("", glAccountEntity.Description);
        }

        [TestMethod]
        public async Task GetAsync_GlAccountIdHasNoCorrespondingDescription()
        {
            var inputGlAccountId = "11_00_01_00_20601_55555";
            glAccountsDescriptionResponse.GlAccountIds.Add("11_00_01_00_20601_51000");
            glAccountsDescriptionResponse.GlAccountIds.Add(inputGlAccountId);
            glAccountsDescriptionResponse.GlDescriptions.Add("Operating Fund : South Campus");
            var glAccountEntity = await actualRepository.GetAsync(inputGlAccountId, majorComponentStartPositions);

            Assert.AreEqual(inputGlAccountId, glAccountEntity.Id);
            Assert.AreEqual("", glAccountEntity.Description);
        }

        [TestMethod]
        public async Task GetAsync_GlAccountIdsHasCorrespondingDescriptionButFewerDescriptionsThanIds()
        {
            var inputGlAccountId = "11_00_01_00_20601_55555";
            var inputGlDescription = "Operating Fund : South Campus";
            glAccountsDescriptionResponse.GlAccountIds.Add(inputGlAccountId);
            glAccountsDescriptionResponse.GlAccountIds.Add("11_00_01_00_20601_51000");
            glAccountsDescriptionResponse.GlDescriptions.Add(inputGlDescription);
            var glAccountEntity = await actualRepository.GetAsync(inputGlAccountId, majorComponentStartPositions);

            Assert.AreEqual(inputGlAccountId, glAccountEntity.Id);
            Assert.AreEqual(inputGlDescription, glAccountEntity.Description);
        }
        #endregion

        #region Validate Gl Account

        [TestMethod]
        public async Task ValidateGlAccountAsync_NonPool_Success_FutureFiscalYear()
        {
            glAccount = "11_00_01_02_ACTIV_50000";
            fiscalYear = DateTime.Now.AddYears(2).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("success", glAccountValidationResponse.Status);
            Assert.AreEqual(null, glAccountValidationResponse.ErrorMessage);
            Assert.AreEqual(50000, glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear).GlBudgetPostedAssocMember);

            var memosAssociation = glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
            var expectedRemainBalance = memosAssociation.GlBudgetPostedAssocMember.HasValue ? memosAssociation.GlBudgetPostedAssocMember.Value : 0m;
            expectedRemainBalance += memosAssociation.GlBudgetMemosAssocMember.HasValue ? memosAssociation.GlBudgetMemosAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlEncumbrancePostedAssocMember.HasValue ? memosAssociation.GlEncumbrancePostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlEncumbranceMemosAssocMember.HasValue ? memosAssociation.GlEncumbranceMemosAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlRequisitionMemosAssocMember.HasValue ? memosAssociation.GlRequisitionMemosAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlActualPostedAssocMember.HasValue ? memosAssociation.GlActualPostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlActualMemosAssocMember.HasValue ? memosAssociation.GlActualMemosAssocMember.Value : 0m;
            Assert.AreEqual(expectedRemainBalance, glAccountValidationResponse.RemainingBalance);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_NonPool_Success_CurrentFiscalYear()
        {
            glAccount = "11_00_01_02_ACTIV_50000";
            fiscalYear = DateTime.Now.Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("success", glAccountValidationResponse.Status);
            Assert.AreEqual(null, glAccountValidationResponse.ErrorMessage);
            Assert.AreEqual(null, glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear).GlBudgetPostedAssocMember);

            var memosAssociation = glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
            var expectedRemainBalance = memosAssociation.GlBudgetPostedAssocMember.HasValue ? memosAssociation.GlBudgetPostedAssocMember.Value : 0m;
            expectedRemainBalance += memosAssociation.GlBudgetMemosAssocMember.HasValue ? memosAssociation.GlBudgetMemosAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlEncumbrancePostedAssocMember.HasValue ? memosAssociation.GlEncumbrancePostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlEncumbranceMemosAssocMember.HasValue ? memosAssociation.GlEncumbranceMemosAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlRequisitionMemosAssocMember.HasValue ? memosAssociation.GlRequisitionMemosAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlActualPostedAssocMember.HasValue ? memosAssociation.GlActualPostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.GlActualMemosAssocMember.HasValue ? memosAssociation.GlActualMemosAssocMember.Value : 0m;
            Assert.AreEqual(expectedRemainBalance, glAccountValidationResponse.RemainingBalance);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_Umbrella_Success_FutureFiscalYear()
        {
            glAccount = "11_00_01_02_UMBRL_50000";
            fiscalYear = DateTime.Now.AddYears(2).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("success", glAccountValidationResponse.Status);
            Assert.AreEqual(null, glAccountValidationResponse.ErrorMessage);
            Assert.AreEqual(66666, glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear).FaBudgetPostedAssocMember);

            var memosAssociation = glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
            var expectedRemainBalance = memosAssociation.FaBudgetPostedAssocMember.HasValue ? memosAssociation.FaBudgetPostedAssocMember.Value : 0m;
            expectedRemainBalance += memosAssociation.FaBudgetMemoAssocMember.HasValue ? memosAssociation.FaBudgetMemoAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaEncumbrancePostedAssocMember.HasValue ? memosAssociation.FaEncumbrancePostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaEncumbranceMemoAssocMember.HasValue ? memosAssociation.FaEncumbranceMemoAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaRequisitionMemoAssocMember.HasValue ? memosAssociation.FaRequisitionMemoAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaActualPostedAssocMember.HasValue ? memosAssociation.FaActualPostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaActualMemoAssocMember.HasValue ? memosAssociation.FaActualMemoAssocMember.Value : 0m;
            Assert.AreEqual(expectedRemainBalance, glAccountValidationResponse.RemainingBalance);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_Umbrella_Successs_CurrentFiscalYear()
        {
            glAccount = "11_00_01_02_UMBRL_50000";
            fiscalYear = DateTime.Now.Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("success", glAccountValidationResponse.Status);
            Assert.AreEqual(null, glAccountValidationResponse.ErrorMessage);
            Assert.AreEqual(null, glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear).FaBudgetPostedAssocMember);

            var memosAssociation = glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
            var expectedRemainBalance = memosAssociation.FaBudgetPostedAssocMember.HasValue ? memosAssociation.FaBudgetPostedAssocMember.Value : 0m;
            expectedRemainBalance += memosAssociation.FaBudgetMemoAssocMember.HasValue ? memosAssociation.FaBudgetMemoAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaEncumbrancePostedAssocMember.HasValue ? memosAssociation.FaEncumbrancePostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaEncumbranceMemoAssocMember.HasValue ? memosAssociation.FaEncumbranceMemoAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaRequisitionMemoAssocMember.HasValue ? memosAssociation.FaRequisitionMemoAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaActualPostedAssocMember.HasValue ? memosAssociation.FaActualPostedAssocMember.Value : 0m;
            expectedRemainBalance -= memosAssociation.FaActualMemoAssocMember.HasValue ? memosAssociation.FaActualMemoAssocMember.Value : 0m;
            Assert.AreEqual(expectedRemainBalance, glAccountValidationResponse.RemainingBalance);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_Poolee_Success()
        {
            glAccount = "11_00_01_02_POOL1_50001";
            fiscalYear = DateTime.Now.AddYears(2).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("A poolee type GL account is not allowed in budget adjustments.", glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateGlAccountAsync_NullGlAccount()
        {
            glAccount = null;
            fiscalYear = DateTime.Now.Year.ToString();

            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateGlAccountAsync_EmptyGlAccount()
        {
            glAccount = "";
            fiscalYear = DateTime.Now.Year.ToString();

            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_NullDataRecord()
        {
            glAccount = "11_00_01_02_ACTIV_50000";
            fiscalYear = DateTime.Now.Year.ToString();

            glAccountRecord = null;
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("The GL account does not exist.", glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_Inactive_GlAccount()
        {
            glAccount = "11_00_01_02_INCTV_59999";
            fiscalYear = DateTime.Now.AddYears(1).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("The GL account is not active.", glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_NullFiscalYear()
        {
            glAccount = "11_00_01_02_ACTIV_50000";
            fiscalYear = null;

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("success", glAccountValidationResponse.Status);
            Assert.AreEqual(null, glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_NullMemosAssociationForFiscalYear()
        {
            glAccount = "11_00_01_02_ACTIV_50000";
            fiscalYear = DateTime.Now.AddYears(8).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("The GL account is not available for the fiscal year.", glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_NullMemosAssociation()
        {
            glAccount = "11_00_01_02_NOMEM_58888";
            fiscalYear = DateTime.Now.Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("The GL account is not available for the fiscal year.", glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_Frozen()
        {
            glAccount = "11_00_01_02_CLOSD_57777";
            fiscalYear = DateTime.Now.AddYears(2).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("The GL account is frozen.", glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_YearEnd()
        {
            glAccount = "11_00_01_02_CLOSD_57777";
            fiscalYear = DateTime.Now.AddYears(1).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("The GL account is in year-end status.", glAccountValidationResponse.ErrorMessage);

        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_Closed()
        {
            glAccount = "11_00_01_02_CLOSD_57777";
            fiscalYear = DateTime.Now.Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("The GL account is closed.", glAccountValidationResponse.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateGlAccountAsync_Poolee()
        {
            glAccount = "11_00_01_02_POOL1_50001";
            fiscalYear = DateTime.Now.AddYears(2).Year.ToString();

            glAccountRecord = testGeneralLedgerAccountRepository.glAccountRecords.FirstOrDefault(x => x.Recordkey == glAccount);
            glAccountValidationResponse = await actualRepository.ValidateGlAccountAsync(glAccount, fiscalYear);

            Assert.AreEqual(glAccountRecord.Recordkey, glAccountValidationResponse.Id);
            Assert.AreEqual("failure", glAccountValidationResponse.Status);
            Assert.AreEqual("A poolee type GL account is not allowed in budget adjustments.", glAccountValidationResponse.ErrorMessage);
        }

        #endregion

        #region Private methods

        private void InitializeMockStatements()
        {
            transManagerMock.Setup(tio => tio.ExecuteAsync<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(It.IsAny<GetGlAccountDescriptionRequest>())).Returns(() =>
            {
                return Task.FromResult(glAccountsDescriptionResponse);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<Glnodisp>("SS", true)).Returns(() =>
            {
                return Task.FromResult(glNoDispontract);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FcDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.FcDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FdDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.FdDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<LoDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.LoDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<ObDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.ObDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<SoDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.SoDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<UnDescs>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(testGlConfigurationRepository.UnDescs);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<GlAccts>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(glAccountRecord);
            });
        }
        #endregion
    }
}