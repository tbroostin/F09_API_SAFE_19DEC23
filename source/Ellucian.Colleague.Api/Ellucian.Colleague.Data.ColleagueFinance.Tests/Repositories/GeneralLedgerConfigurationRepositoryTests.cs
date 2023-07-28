// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Dmi.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class GeneralLedgerConfigurationRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private GeneralLedgerConfigurationRepository actualRepository;
        private TestGeneralLedgerConfigurationRepository expectedRepository;
        private BudgetWebDefaults budgetWebDefaultsDataContract;
        private int testFiscalYear = 0;

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();

            // Initialize the test GL configuration repository.
            expectedRepository = new TestGeneralLedgerConfigurationRepository();
            budgetWebDefaultsDataContract = expectedRepository.BudgetWebDefaultsContract;

            this.actualRepository = BuildRepository();

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<Fiscalyr>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.FiscalYearDataContract);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<Glstruct>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                if (this.expectedRepository.GlStructDataContract != null)
                {
                    this.expectedRepository.GlStructDataContract.buildAssociations();
                }

                return Task.FromResult(this.expectedRepository.GlStructDataContract);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.CfWebDefaultsDataContract);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FcDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.FcDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FdDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.FdDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<LoDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.LoDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<ObDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.ObDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<SoDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.SoDescs);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<UnDescs>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.UnDescs);
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<Glclsdef>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.GlClassDefDataContract);
            });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<GenLdgr>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.GenLdgrDataContracts);
            });

            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
            {
                if (this.expectedRepository.GenLdgrDataContracts == null)
                {
                    string[] nullArray = null;
                    return Task.FromResult(nullArray);
                }
                return Task.FromResult(this.expectedRepository.GenLdgrDataContracts.Where(x => x.GenLdgrStatus == "O").Select(x => x.Recordkey).ToArray());
            });

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetWebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(budgetWebDefaultsDataContract);
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            budgetWebDefaultsDataContract = null;
            expectedRepository = null;
            testFiscalYear = 0;
        }
        #endregion

        #region Fiscal Year Configuration

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task FiscalYearConfiguration_DataReaderReturnsNull()
        {
            this.expectedRepository.FiscalYearDataContract = null;
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task FiscalYearConfiguration_DataContractContainsNullStartMonth()
        {
            this.expectedRepository.FiscalYearDataContract.FiscalStartMonth = null;
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task FiscalYearConfiguration_DataContractContainsOutOfRangeStartMonth_0()
        {
            this.expectedRepository.FiscalYearDataContract.FiscalStartMonth = 0;
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task FiscalYearConfiguration_DataContractContainsOutOfRangeStartMonth_13()
        {
            this.expectedRepository.FiscalYearDataContract.FiscalStartMonth = 13;
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
        }

        [TestMethod]
        public async Task FiscalYearConfiguration_StartMonthIs1_Success()
        {
            this.expectedRepository.FiscalYearDataContract.FiscalStartMonth = 1;
            var expectedConfiguration = await this.expectedRepository.GetFiscalYearConfigurationAsync();
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();

            Assert.AreEqual(expectedConfiguration.StartMonth, actualConfiguration.StartMonth);
            Assert.AreEqual(expectedConfiguration.CurrentFiscalYear, actualConfiguration.CurrentFiscalYear);
        }

        [TestMethod]
        public async Task FiscalYearConfiguration_StartMonthIs12_Success()
        {
            this.expectedRepository.FiscalYearDataContract.FiscalStartMonth = 12;
            var expectedConfiguration = await this.expectedRepository.GetFiscalYearConfigurationAsync();
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();

            Assert.AreEqual(expectedConfiguration.StartMonth, actualConfiguration.StartMonth);
            Assert.AreEqual(expectedConfiguration.CurrentFiscalYear, actualConfiguration.CurrentFiscalYear);
        }

        [TestMethod]
        public async Task FiscalYearConfiguration_CurrentFiscalMonthIs1_Success()
        {
            this.expectedRepository.FiscalYearDataContract.CurrentFiscalMonth = 1;
            var expectedConfiguration = await this.expectedRepository.GetFiscalYearConfigurationAsync();
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();

            Assert.AreEqual(expectedConfiguration.CurrentFiscalMonth, actualConfiguration.CurrentFiscalMonth);
        }

        [TestMethod]
        public async Task FiscalYearConfiguration_CurrentFiscalMonthIsNull()
        {
            var expectedMessage = "Current fiscal month must have a value.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.FiscalYearDataContract.CurrentFiscalMonth = null;
                var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task FiscalYearConfiguration_CurrentFiscalMonthIs0()
        {
            var expectedMessage = "Current fiscal month must be in between 1 and 12.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.FiscalYearDataContract.CurrentFiscalMonth = 0;
                var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task FiscalYearConfiguration_CurrentFiscalMonthIs13()
        {
            var expectedMessage = "Current fiscal month must be in between 1 and 12.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.FiscalYearDataContract.CurrentFiscalMonth = 13;
                var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task FiscalYearConfiguration_DataContractContainsNullCurrentYear()
        {
            this.expectedRepository.FiscalYearDataContract.CfCurrentFiscalYear = null;
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task FiscalYearConfiguration_DataContractContainsEmptyCurrentYear()
        {
            this.expectedRepository.FiscalYearDataContract.CfCurrentFiscalYear = "";
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();
        }

        [TestMethod]
        public async Task FiscalYearConfiguration_Success()
        {
            var expectedConfiguration = await this.expectedRepository.GetFiscalYearConfigurationAsync();
            var actualConfiguration = await actualRepository.GetFiscalYearConfigurationAsync();

            Assert.AreEqual(expectedConfiguration.StartMonth, actualConfiguration.StartMonth);
            Assert.AreEqual(expectedConfiguration.CurrentFiscalYear, actualConfiguration.CurrentFiscalYear);
        }
        #endregion

        #region Account structure
        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task AccountStructure_GlstructDataContractIsNull()
        {
            expectedRepository.GlStructDataContract = null;
            var accountStructure = await actualRepository.GetAccountStructureAsync();
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubNameIsNull()
        {
            var expectedMessage = "GL account structure is missing subcomponent name definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubName = null;
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubNameContainsNull()
        {
            var expectedMessage = "GL account structure is missing subcomponent name definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubName = new List<string>();
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubNameContainsEnpty()
        {
            var expectedMessage = "GL account structure is missing subcomponent name definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubName[0] = "";
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubStartIsNull()
        {
            var expectedMessage = "GL account structure is missing subcomponent start position definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubStart = null;
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubStartContainsNull()
        {
            var expectedMessage = "GL account structure is missing subcomponent start position definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubStart = new List<string>();
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubStartContainsEmpty()
        {
            var expectedMessage = "GL account structure is missing subcomponent start position definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubStart = new List<string>();
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubLgthIsNull()
        {
            var expectedMessage = "GL account structure is missing subcomponent length definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubLgth = null;
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubLgthContainsNull()
        {
            var expectedMessage = "GL account structure is missing subcomponent length definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubLgth = new List<string>();
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubLgthContainsEmpty()
        {
            var expectedMessage = "GL account structure is missing subcomponent length definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubLgth = new List<string>();
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAccountStructureAsync_AcctSubNameContainsNullSubvalue()
        {
            var expectedMessage = "GL account structure is missing subcomponent name definitions.";
            var actualMessage = "";
            try
            {
                this.expectedRepository.GlStructDataContract.AcctSubName = new List<string>() { "FUND" + DmiString._SM + null, "PROGRAM", "LOCATION", "ACTIVITY", "FUNCTION" + DmiString._SM + "DIVISION" + DmiString._SM + "SUBDIVISION" + DmiString._SM + "DEPARTMENT", "GL.CLASS" + DmiString._SM + "GL.SUBCLASS" + DmiString._SM + "CATEGORY" + DmiString._SM + "SUBCATEGORY" + DmiString._SM + "OBJECT" };
                await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task AccountStructure_CostCenterEmptyStartPosition()
        {
            expectedRepository.GlStructDataContract.AcctStart[0] = "";

            string message = "";
            try
            {
                var accountStructure = await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("Start position for GL component not defined.", message);
        }

        [TestMethod]
        public async Task AccountStructure_CostCenterNullComponentLength()
        {
            expectedRepository.GlStructDataContract.AcctLength[0] = null;

            string message = "";
            try
            {
                var accountStructure = await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("Length for GL component not defined.", message);
        }

        [TestMethod]
        public async Task AccountStructure_CostCenterInvalidComponentLength()
        {
            expectedRepository.GlStructDataContract.AcctLength[0] = 0;

            string message = "";
            try
            {
                var accountStructure = await actualRepository.GetAccountStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("Invalid length specified for GL component.", message);
        }

        [TestMethod]
        public async Task AccountStructure_Success_MajorComponents()
        {
            var accountStructure = await actualRepository.GetAccountStructureAsync();

            // Make sure the start positions and full access role match
            for (int i = 0; i < expectedRepository.GlStructDataContract.AcctStart.Count; i++)
            {
                Assert.AreEqual(expectedRepository.GlStructDataContract.AcctStart[i], accountStructure.MajorComponentStartPositions[i]);
            }
            Assert.AreEqual(expectedRepository.GlStructDataContract.GlFullAccessRole, accountStructure.FullAccessRole);


            // Make sure the major component data matches.
            Assert.AreEqual(expectedRepository.GlStructDataContract.AcctNames.Count, accountStructure.MajorComponents.Count);
            for (int i = 0; i < expectedRepository.GlStructDataContract.AcctNames.Count; i++)
            {
                // Compare to the data coming from GLSTRUCT
                var glStructData = expectedRepository.GlStructDataContract.GlmajorEntityAssociation
                    .Where(x => x.AcctNamesAssocMember == expectedRepository.GlStructDataContract.AcctNames[i]).ToList().FirstOrDefault();
                if (glStructData != null)
                {
                    Assert.AreEqual(glStructData.AcctNamesAssocMember, accountStructure.MajorComponents[i].ComponentName);
                    Assert.AreEqual(Convert.ToInt32(glStructData.AcctStartAssocMember) - 1, accountStructure.MajorComponents[i].StartPosition);
                    Assert.AreEqual(glStructData.AcctLengthAssocMember.Value, accountStructure.MajorComponents[i].ComponentLength);

                    // Confirm the component type
                    var componentType = DetermineComponentType(glStructData.AcctComponentTypeAssocMember);
                    Assert.AreEqual(componentType, accountStructure.MajorComponents[i].ComponentType);
                }
            }
        }

        [TestMethod]
        public async Task AccountStructure_Success_Subcomponents()
        {
            var accountStructureEntity = await actualRepository.GetAccountStructureAsync();

            // Make sure the subcomponent data has been stored properly in the domain entity.
            for (int i = 0; i < expectedRepository.GlStructDataContract.AcctNames.Count; i++)
            {
                // Get the subcomponent names, start positions, and lengths for the given major component.
                var majorComponentType = DetermineComponentType(expectedRepository.GlStructDataContract.AcctComponentType[i]);
                var subcomponentNames = expectedRepository.GlStructDataContract.AcctSubName[i].Split(DmiString._SM);
                var subcomponentStartPositions = expectedRepository.GlStructDataContract.AcctSubStart[i].Split(DmiString._SM);
                var subcomponentLengths = expectedRepository.GlStructDataContract.AcctSubLgth[i].Split(DmiString._SM);

                // Grab the individual subcomponent name, start position, and length and make sure that subcomponent is represented in the entity.
                for (int j = 0; j < subcomponentNames.Length; j++)
                {
                    var subcomponentName = subcomponentNames[j].ToString();
                    var subcomponentStartPosition = Convert.ToInt32(subcomponentStartPositions[j]) - 1;
                    var subcomponentLength = Convert.ToInt32(subcomponentLengths[j]);

                    // Find the corresponding entity...
                    var matchingSubcomponent = accountStructureEntity.Subcomponents.FirstOrDefault(x =>
                        x.ComponentType == majorComponentType
                        && x.ComponentName == subcomponentName
                        && x.StartPosition == subcomponentStartPosition
                        && x.ComponentLength == subcomponentLength);
                    Assert.IsNotNull(matchingSubcomponent);
                }
            }
        }
        #endregion

        #region Cost Center Structure
        [TestMethod]
        public async Task CostCenterStructure_CfWebDefaultsDataContractIsNull()
        {
            expectedRepository.CfWebDefaultsDataContract = null;

            string message = "";
            try
            {
                var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("GL component information is not defined.", message);
        }

        [TestMethod]
        public async Task CostCenterStructure_GlStructContractDoesNotContainComponentFromCFWP()
        {
            expectedRepository.GlStructDataContract.AcctNames.RemoveAt(0);

            string message = "";
            try
            {
                var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("GL structure information is not defined.", message);
        }

        [TestMethod]
        public async Task CostCenterStructure_GlStructContractHasNoAssociationData()
        {
            expectedRepository.GlStructDataContract.AcctNames = new List<string>();
            expectedRepository.GlStructDataContract.AcctStart = new List<string>();
            expectedRepository.GlStructDataContract.AcctLength = new List<int?>();
            expectedRepository.GlStructDataContract.AcctComponentType = new List<string>();

            string message = "";
            try
            {
                var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("GL structure information is not defined.", message);
        }

        [TestMethod]
        public async Task CostCenterStructure_CostCenterNullComponentName()
        {
            expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps[0] = null;

            string message = "";
            try
            {
                var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("Component name for GL structure is not defined.", message);
        }

        [TestMethod]
        public async Task CostCenterStructure_CostCenterEmptyComponentName()
        {
            expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps[0] = "";

            string message = "";
            try
            {
                var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("Component name for GL structure is not defined.", message);
        }

        [TestMethod]
        public async Task CostCenterStructure_CostCenterNullStartPosition()
        {
            expectedRepository.GlStructDataContract.AcctStart[0] = null;

            string message = "";
            try
            {
                var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();
            }
            catch (ConfigurationException cex)
            {
                message = cex.Message;
            }

            Assert.AreEqual("Start position for GL component not defined.", message);
        }

        [TestMethod]
        public async Task CostCenterStructure_Success()
        {
            var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();

            // Make sure the cost center component data matches
            Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps.Count, costCenterStructure.CostCenterComponents.Count);
            for (int i = 0; i < expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps.Count; i++)
            {
                // Compare to the data coming from CF.WEB.DEFAULTS
                Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps[i], costCenterStructure.CostCenterComponents[i].ComponentName);
                Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterDescs[i].ToUpper() == "Y", costCenterStructure.CostCenterComponents[i].IsPartOfDescription);

                // Compare to the data coming from GLSTRUCT
                var glStructData = expectedRepository.GlStructDataContract.GlmajorEntityAssociation
                    .Where(x => x.AcctNamesAssocMember == expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps[i]).ToList().FirstOrDefault();
                if (glStructData != null)
                {
                    Assert.AreEqual(glStructData.AcctNamesAssocMember, costCenterStructure.CostCenterComponents[i].ComponentName);
                    Assert.AreEqual(Convert.ToInt32(glStructData.AcctStartAssocMember) - 1, costCenterStructure.CostCenterComponents[i].StartPosition);
                    Assert.AreEqual(glStructData.AcctLengthAssocMember.Value, costCenterStructure.CostCenterComponents[i].ComponentLength);

                    // Confirm the component type
                    var componentType = DetermineComponentType(glStructData.AcctComponentTypeAssocMember);
                    Assert.AreEqual(componentType, costCenterStructure.CostCenterComponents[i].ComponentType);
                }
            }

            // Make sure the object component data matches
            Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeComps.Count, costCenterStructure.ObjectComponents.Count);
            for (int i = 0; i < expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeComps.Count; i++)
            {
                // Compare to the data coming from CF.WEB.DEFAULTS
                Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeComps[i], costCenterStructure.ObjectComponents[i].ComponentName);
                Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeDescs[i].ToUpper() == "Y", costCenterStructure.ObjectComponents[i].IsPartOfDescription);

                // Compare to the data coming from GLSTRUCT
                var glStructData = expectedRepository.GlStructDataContract.GlmajorEntityAssociation
                    .Where(x => x.AcctNamesAssocMember == expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeComps[i]).ToList().FirstOrDefault();
                if (glStructData != null)
                {
                    Assert.AreEqual(glStructData.AcctNamesAssocMember, costCenterStructure.ObjectComponents[i].ComponentName);
                    Assert.AreEqual(Convert.ToInt32(glStructData.AcctStartAssocMember) - 1, costCenterStructure.ObjectComponents[i].StartPosition);
                    Assert.AreEqual(glStructData.AcctLengthAssocMember.Value, costCenterStructure.ObjectComponents[i].ComponentLength);

                    // Confirm the component type
                    var componentType = DetermineComponentType(glStructData.AcctComponentTypeAssocMember);
                    Assert.AreEqual(componentType, costCenterStructure.ObjectComponents[i].ComponentType);
                }
            }
        }

        [TestMethod]
        public async Task CostCenterStructure_InvalidCostCenterData()
        {
            expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterDescs.RemoveAt(0);
            var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();

            // Make sure the cost center component data matches
            Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps.Count, costCenterStructure.CostCenterComponents.Count);
            for (int i = 0; i < expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps.Count; i++)
            {
                Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterComps[i], costCenterStructure.CostCenterComponents[i].ComponentName);

                // Compare the description values until we the index gets out of bounds; at that point we expect false.
                if (i < expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterDescs.Count)
                    Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrCostCenterDescs[i].ToUpper() == "Y", costCenterStructure.CostCenterComponents[i].IsPartOfDescription);
                else
                    Assert.IsFalse(costCenterStructure.CostCenterComponents[i].IsPartOfDescription);
            }
        }

        [TestMethod]
        public async Task CostCenterStructure_InvalidObjectData()
        {
            expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeDescs.RemoveAt(0);
            var costCenterStructure = await actualRepository.GetCostCenterStructureAsync();

            // Make sure the object component data matches
            Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeComps.Count, costCenterStructure.ObjectComponents.Count);
            for (int i = 0; i < expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeComps.Count; i++)
            {
                Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeComps[i], costCenterStructure.ObjectComponents[i].ComponentName);

                // Compare the description values until we the index gets out of bounds; at that point we expect false.
                if (i < expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeDescs.Count)
                    Assert.AreEqual(expectedRepository.CfWebDefaultsDataContract.CfwebCkrObjectCodeDescs[i].ToUpper() == "Y", costCenterStructure.ObjectComponents[i].IsPartOfDescription);
                else
                    Assert.IsFalse(costCenterStructure.ObjectComponents[i].IsPartOfDescription);
            }
        }
        #endregion

        #region GL Class Configuration
        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task GetClassConfiguration_DataReaderReturnsNull()
        {
            expectedRepository.GlClassDefDataContract = null;
            var classConfiguration = await actualRepository.GetClassConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task GetClassConfiguration_NullGlClassName()
        {
            expectedRepository.GlClassDefDataContract.GlClassDict = null;
            var classConfiguration = await actualRepository.GetClassConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public async Task GetClassConfiguration_NullExpenseValueList()
        {
            expectedRepository.GlClassDefDataContract.GlClassExpenseValues = null;
            var classConfiguration = await actualRepository.GetClassConfigurationAsync();
        }

        [TestMethod]
        public async Task GetClassConfiguration_NullRevenueValueList()
        {
            var expectedMessage = "GL class revenue values are not defined.";
            var actualMessage = "";
            try
            {
                expectedRepository.GlClassDefDataContract.GlClassRevenueValues = null;
                var classConfiguration = await actualRepository.GetClassConfigurationAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetClassConfiguration_Success()
        {
            var classConfiguration = await actualRepository.GetClassConfigurationAsync();
            Assert.AreEqual(expectedRepository.GlClassDefDataContract.GlClassDict, classConfiguration.ClassificationName);

            Assert.AreEqual(expectedRepository.GlClassDefDataContract.GlClassExpenseValues.Count,
                classConfiguration.ExpenseClassValues.Count);
            foreach (var expectedValue in expectedRepository.GlClassDefDataContract.GlClassExpenseValues)
            {
                Assert.IsTrue(classConfiguration.ExpenseClassValues.Contains(expectedValue));
            }

            Assert.AreEqual(expectedRepository.GlClassDefDataContract.GlClassRevenueValues.Count,
                classConfiguration.RevenueClassValues.Count);
            foreach (var expectedValue in expectedRepository.GlClassDefDataContract.GlClassRevenueValues)
            {
                Assert.IsTrue(classConfiguration.RevenueClassValues.Contains(expectedValue));
            }
        }
        #endregion

        #region Available fiscal years
        [TestMethod]
        public async Task GetAllFiscalYearsAsync_BulkReadReturnsNull()
        {
            expectedRepository.GenLdgrDataContracts = null;

            // Set up the FY data
            var fiscalYearInfo = await expectedRepository.GetFiscalYearConfigurationAsync();

            string message = "";
            try
            {
                var actualFiscalYears = await actualRepository.GetAllFiscalYearsAsync(testFiscalYear);
            }
            catch (ConfigurationException anex)
            {
                message = anex.Message;
            }
            Assert.AreEqual("No fiscal years have been set up.", message);
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_BulkReadReturnsEmptySet()
        {
            // Remove all fiscal years
            while (expectedRepository.GenLdgrDataContracts.Count > 0)
                expectedRepository.GenLdgrDataContracts.RemoveAt(0);

            // Set up the FY data
            var fiscalYearInfo = await expectedRepository.GetFiscalYearConfigurationAsync();

            string message = "";
            try
            {
                var actualFiscalYears = await actualRepository.GetAllFiscalYearsAsync(testFiscalYear);
            }
            catch (ConfigurationException anex)
            {
                message = anex.Message;
            }
            Assert.AreEqual("No fiscal years have been set up.", message);
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_OneGenLdgrContractIsNull()
        {
            // Set up the FY data
            var genLdgrDataContract = expectedRepository.GenLdgrDataContracts[2] = null;

            var fiscalYearInfo = await expectedRepository.GetFiscalYearConfigurationAsync();
            var expectedFiscalYears = await expectedRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            var actualFiscalYears = await actualRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            // Make sure the repository returns the same number of years.
            Assert.AreEqual(expectedRepository.GenLdgrDataContracts.Count - 1, actualFiscalYears.Count());

            foreach (var year in actualFiscalYears)
            {
                // Make sure the lists contain the same values.
                int yearInt = 0;
                Int32.TryParse(year, out yearInt);
                Assert.IsTrue(expectedFiscalYears.Contains(year));
            }
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_OneGenLdgrContractHasNullRecordKey()
        {
            // Set up the FY data
            var genLdgrDataContract = expectedRepository.GenLdgrDataContracts[2].Recordkey = null;

            var fiscalYearInfo = await expectedRepository.GetFiscalYearConfigurationAsync();
            var expectedFiscalYears = await expectedRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            var actualFiscalYears = await actualRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            // Make sure the repository returns the same number of years.
            Assert.AreEqual(expectedRepository.GenLdgrDataContracts.Count - 1, actualFiscalYears.Count());

            foreach (var year in actualFiscalYears)
            {
                // Make sure the lists contain the same values.
                int yearInt = 0;
                Int32.TryParse(year, out yearInt);
                Assert.IsTrue(expectedFiscalYears.Contains(year));
            }
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_MoreYearsThanRequested()
        {
            // Set up the FY data
            expectedRepository.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = "1995", GenLdgrStatus = "C" });
            expectedRepository.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = "1994", GenLdgrStatus = "C" });
            expectedRepository.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = "1993", GenLdgrStatus = "C" });

            var fiscalYearInfo = await expectedRepository.GetFiscalYearConfigurationAsync();
            var expectedFiscalYears = await expectedRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            var actualFiscalYears = await actualRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            // Make sure the repository returns the same number of years.
            Assert.AreEqual(expectedFiscalYears.Count(), actualFiscalYears.Count());

            // Make sure the first year returned is the fiscal year for the current date.
            Assert.AreEqual(fiscalYearInfo.FiscalYearForToday.ToString(), actualFiscalYears.ElementAt(1));

            foreach (var year in actualFiscalYears)
            {
                // Make sure the lists contain the same values.
                int yearInt = 0;
                Int32.TryParse(year, out yearInt);
                Assert.IsTrue(expectedFiscalYears.Contains(yearInt.ToString()));
            }
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_SameYearsAsRequested()
        {
            // Set up the FY data
            var fiscalYearInfo = await expectedRepository.GetFiscalYearConfigurationAsync();
            var expectedFiscalYears = await expectedRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            var actualFiscalYears = await actualRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            // Make sure the repository returns the same number of years.
            Assert.AreEqual(expectedFiscalYears.Count(), actualFiscalYears.Count());

            // Make sure the first year returned is the fiscal year for the current date.
            Assert.AreEqual(fiscalYearInfo.FiscalYearForToday.ToString(), actualFiscalYears.ElementAt(1));

            var previousFiscalYear = 0;
            foreach (var year in actualFiscalYears)
            {
                // Make sure the lists contain the same values.
                int yearInt = 0;
                Int32.TryParse(year, out yearInt);
                Assert.IsTrue(expectedFiscalYears.Contains(yearInt.ToString()));

                // Make sure the actual list contains decremented values.
                if (previousFiscalYear != 0)
                    Assert.AreEqual(previousFiscalYear, yearInt + 1);

                previousFiscalYear = yearInt;
            }
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_FewerYearsThanRequested()
        {
            // Delete two of the six original fiscal years.
            expectedRepository.GenLdgrDataContracts.RemoveAt(6);
            expectedRepository.GenLdgrDataContracts.RemoveAt(5);

            // Set up the FY data
            var fiscalYearInfo = await expectedRepository.GetFiscalYearConfigurationAsync();
            var expectedFiscalYears = await expectedRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            var actualFiscalYears = await actualRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            // Make sure the repository returns the same number of years.
            Assert.AreEqual(expectedRepository.GenLdgrDataContracts.Count, actualFiscalYears.Count());

            // Make sure the first year returned is the fiscal year for the current date.
            Assert.AreEqual(fiscalYearInfo.FiscalYearForToday.ToString(), actualFiscalYears.ElementAt(1));

            foreach (var year in actualFiscalYears)
            {
                // Make sure the lists contain the same values.
                int yearInt = 0;
                Int32.TryParse(year, out yearInt);
                Assert.IsTrue(expectedFiscalYears.Contains(yearInt.ToString()));
            }
        }
        #endregion

        #region Open fiscal years
        [TestMethod]
        public async Task GetAllFiscalYearsAsync_HappyPath()
        {
            var openFiscalYears = await actualRepository.GetAllFiscalYearsAsync(testFiscalYear);
            foreach (var openFiscalYear in openFiscalYears)
            {
                var expectedFiscalYear = expectedRepository.GenLdgrDataContracts.FirstOrDefault(x => x.Recordkey == openFiscalYear && x.GenLdgrStatus == "O");
                Assert.IsNotNull(expectedFiscalYear);
            }
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_SelectAsyncReturnsNull()
        {
            expectedRepository.GenLdgrDataContracts = null;

            string expectedMessage = "Error selecting open fiscal years.";
            string actualMessage = "";
            try
            {
                var actualFiscalYears = await actualRepository.GetAllOpenFiscalYears();
            }
            catch (ConfigurationException ce)
            {
                actualMessage = ce.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetAllFiscalYearsAsync_SelectAsyncReturnsEmptySet()
        {
            foreach (var genLdgrContract in expectedRepository.GenLdgrDataContracts)
            {
                genLdgrContract.GenLdgrStatus = "C";
            }

            string expectedMessage = "There are no open fiscal years.";
            string actualMessage = "";
            try
            {
                var actualFiscalYears = await actualRepository.GetAllOpenFiscalYears();
            }
            catch (ConfigurationException ce)
            {
                actualMessage = ce.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region GetBudgetAdjustmentEnabledAsync
        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_DataReaderReturnsFlagAs_Y()
        {
            this.budgetWebDefaultsDataContract.BudWebBudAdjAllowed = "Y";
            var entity = await this.actualRepository.GetBudgetAdjustmentEnabledAsync();
            Assert.IsTrue(entity.Enabled);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_DataReaderReturnsFlagAs_y()
        {
            this.budgetWebDefaultsDataContract.BudWebBudAdjAllowed = "y";
            var entity = await this.actualRepository.GetBudgetAdjustmentEnabledAsync();
            Assert.IsTrue(entity.Enabled);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_DataReaderReturnsFlagAs_N()
        {
            this.budgetWebDefaultsDataContract.BudWebBudAdjAllowed = "N";
            var entity = await this.actualRepository.GetBudgetAdjustmentEnabledAsync();
            Assert.IsFalse(entity.Enabled);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_DataReaderReturnsFlagAs_Blank()
        {
            this.budgetWebDefaultsDataContract.BudWebBudAdjAllowed = "";
            var entity = await this.actualRepository.GetBudgetAdjustmentEnabledAsync();
            Assert.IsFalse(entity.Enabled);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_DataReaderReturnsFlagAs_Null()
        {
            this.budgetWebDefaultsDataContract.BudWebBudAdjAllowed = null;
            var entity = await this.actualRepository.GetBudgetAdjustmentEnabledAsync();
            Assert.IsFalse(entity.Enabled);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentEnabledAsync_DataReaderReturns_Null()
        {
            this.budgetWebDefaultsDataContract = null;
            var entity = await this.actualRepository.GetBudgetAdjustmentEnabledAsync();
            Assert.IsFalse(entity.Enabled);
        }
        #endregion

        #region GetBudgetAdjustmentAccountExclusionsAsync

        #region Happy paths
        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentAndRangesArePopulated()
        {
            // Get the exclusion data and compare it to the expected data.
            var exclusionsEntity = await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();

            Assert.AreEqual(budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation.Count(), exclusionsEntity.ExcludedElements.Count());
            foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
            {
                var matchingEntityElement = exclusionsEntity.ExcludedElements.Where(x => x.ExclusionComponent.ComponentName == element.BudWebExclComponentAssocMember
                && x.ExclusionRange.StartValue == element.BudWebExclFromValuesAssocMember && x.ExclusionRange.EndValue == element.BudWebExclToValuesAssocMember);

                Assert.IsNotNull(matchingEntityElement);
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_DataContractIsNull()
        {
            var expectedMessage = "Budget web defaults not configured.";
            var actualMessage = "";
            try
            {
                this.budgetWebDefaultsDataContract = null;
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsNullAndRangesArePopulatedCorrectly()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[0].BudWebExclComponentAssocMember = null;
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsNullAndRangesArePopulatedIncorrectly_OnlyFromValues()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[0].BudWebExclComponentAssocMember = null;
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclToValuesAssocMember = string.Empty;
                }
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsNullAndRangesArePopulatedIncorrectly_OnlyToValues()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[0].BudWebExclComponentAssocMember = null;
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclFromValuesAssocMember = string.Empty;
                }
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsEmptyAndRangesAreEmpty()
        {
            foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
            {
                element.BudWebExclComponentAssocMember = string.Empty;
                element.BudWebExclFromValuesAssocMember = string.Empty;
                element.BudWebExclToValuesAssocMember = string.Empty;
            }

            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            try
            {
                // Get the exclusion data and compare it to the expected data.
                var exclusionsEntity = await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsNullAndRangesAreNull()
        {
            foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
            {
                element.BudWebExclComponentAssocMember = null;
                element.BudWebExclFromValuesAssocMember = null;
                element.BudWebExclToValuesAssocMember = null;
            }

            var expectedMessage = "";
            var actualMessage = "";
            var exclusionsEntity = new BudgetAdjustmentAccountExclusions();
            try
            {
                // Get the exclusion data and compare it to the expected data.
                exclusionsEntity = await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(0, exclusionsEntity.ExcludedElements.Count());
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsNullAndRangesAreEmpty()
        {
            foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
            {
                element.BudWebExclComponentAssocMember = null;
                element.BudWebExclFromValuesAssocMember = string.Empty;
                element.BudWebExclToValuesAssocMember = string.Empty;
            }

            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            try
            {
                // Get the exclusion data and compare it to the expected data.
                var exclusionsEntity = await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsEmptyAndRangesAreNull()
        {
            foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
            {
                element.BudWebExclComponentAssocMember = string.Empty;
                element.BudWebExclFromValuesAssocMember = null;
                element.BudWebExclToValuesAssocMember = null;
            }

            var expectedMessage = "";
            var actualMessage = "";
            var exclusionsEntity = new BudgetAdjustmentAccountExclusions();
            try
            {
                // Get the exclusion data and compare it to the expected data.
                exclusionsEntity = await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(0, exclusionsEntity.ExcludedElements.Count());
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsEmptyAndRangesArePopulatedCorrectly()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[0].BudWebExclComponentAssocMember = string.Empty;
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsEmptyAndRangesArePopulatedIncorrectly_OnlyFromValues()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[0].BudWebExclComponentAssocMember = string.Empty;
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclToValuesAssocMember = string.Empty;
                }
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsEmptyAndRangesArePopulatedIncorrectly_OnlyToValues()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[0].BudWebExclComponentAssocMember = string.Empty;
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclFromValuesAssocMember = string.Empty;
                }
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndRangesAreNull()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclFromValuesAssocMember = null;
                    element.BudWebExclToValuesAssocMember = null;
                }
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndRangesAreEmpty()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var exclusionsEntity = new BudgetAdjustmentAccountExclusions();
            try
            {
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclFromValuesAssocMember = string.Empty;
                    element.BudWebExclToValuesAssocMember = string.Empty;
                }
                // Get the exclusion data and compare it to the expected data.
                exclusionsEntity = await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(0, exclusionsEntity.ExcludedElements.Count());
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndRangesArePopulatedIncorrectly_OnlyFromValues()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclToValuesAssocMember = string.Empty;
                }
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndRangesArePopulatedIncorrectly_OnlyToValues()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                foreach (var element in budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    element.BudWebExclFromValuesAssocMember = string.Empty;
                }
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndFromRangeContainsNull()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[1].BudWebExclFromValuesAssocMember = null;
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndFromRangeContainsEmpty()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[1].BudWebExclFromValuesAssocMember = string.Empty;
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndToRangeContainsNull()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[1].BudWebExclToValuesAssocMember = null;
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndToRangeContainsEmpty()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[1].BudWebExclToValuesAssocMember = string.Empty;
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAccountExclusionsAsync_ComponentIsPopulatedAndRangesHaveValuesNotSortedCorrectly()
        {
            var expectedMessage = "Invalid budget adjustment GL account exclusions web parameters.";
            var actualMessage = "";
            try
            {
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[1].BudWebExclFromValuesAssocMember = "54";
                budgetWebDefaultsDataContract.BudAdjExcludeCriteriaEntityAssociation[1].BudWebExclToValuesAssocMember = "52";
                await this.actualRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            }
            catch (ConfigurationException cex)
            {
                actualMessage = cex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #endregion

        #region GetBudgetAdjustmentParametersAsync

        [TestMethod]
        public async Task GetBudgetAdjustmentParametersAsync_DataReaderReturnsFlagAs_Y()
        {
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetWebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new BudgetWebDefaults()
                {
                    BudAdjApprovalNeededFlag = "Y",
                    BudAdjSameCcApprReq = "Y",
                    BudAdjSameCstCntrRequrd = "Y"
                });
            });
            var entity = await this.actualRepository.GetBudgetAdjustmentParametersAsync();
            Assert.IsTrue(entity.ApprovalRequired);
            Assert.IsTrue(entity.SameCostCenterApprovalRequired);
            Assert.IsTrue(entity.SameCostCenterRequired);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentParametersAsync_DataReaderReturnsFlagAs_N()
        {
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetWebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new BudgetWebDefaults()
                {
                    BudAdjApprovalNeededFlag = "N",
                    BudAdjSameCcApprReq = "N",
                    BudAdjSameCstCntrRequrd = "N"
                });
            });
            var entity = await this.actualRepository.GetBudgetAdjustmentParametersAsync();
            Assert.IsFalse(entity.ApprovalRequired);
            Assert.IsFalse(entity.SameCostCenterApprovalRequired);
            Assert.IsFalse(entity.SameCostCenterRequired);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentParametersAsync_DataReaderReturnsFlagAs_Null()
        {
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetWebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new BudgetWebDefaults()
                {
                    BudAdjApprovalNeededFlag = null,
                    BudAdjSameCcApprReq = null,
                    BudAdjSameCstCntrRequrd = null
                });
            });
            var entity = await this.actualRepository.GetBudgetAdjustmentParametersAsync();
            Assert.IsFalse(entity.ApprovalRequired);
            Assert.IsFalse(entity.SameCostCenterApprovalRequired);
            Assert.IsTrue(entity.SameCostCenterRequired);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentParametersAsync_DataReaderReturnsFlagAs_EmptyString()
        {
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BudgetWebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new BudgetWebDefaults()
                {
                    BudAdjApprovalNeededFlag = string.Empty,
                    BudAdjSameCcApprReq = string.Empty,
                    BudAdjSameCstCntrRequrd = string.Empty
                });
            });
            var entity = await this.actualRepository.GetBudgetAdjustmentParametersAsync();
            Assert.IsFalse(entity.ApprovalRequired);
            Assert.IsFalse(entity.SameCostCenterApprovalRequired);
            Assert.IsTrue(entity.SameCostCenterRequired);
        }

        #endregion

        #region Private methods
        private GeneralLedgerConfigurationRepository BuildRepository()
        {
            return new GeneralLedgerConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        private GeneralLedgerComponentType DetermineComponentType(string componentTypeString)
        {
            var componentType = GeneralLedgerComponentType.Function;
            switch (componentTypeString.ToUpper())
            {
                case "FD":
                    componentType = GeneralLedgerComponentType.Fund;
                    break;
                case "FC":
                    componentType = GeneralLedgerComponentType.Function;
                    break;
                case "OB":
                    componentType = GeneralLedgerComponentType.Object;
                    break;
                case "UN":
                    componentType = GeneralLedgerComponentType.Unit;
                    break;
                case "SO":
                    componentType = GeneralLedgerComponentType.Source;
                    break;
                case "LO":
                    componentType = GeneralLedgerComponentType.Location;
                    break;
            }

            return componentType;
        }
        #endregion
    }
}