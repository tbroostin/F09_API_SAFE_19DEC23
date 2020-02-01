// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Utilities
{
    [TestClass]
    public class GlAccountUtilityTests
    {
        #region Initialize and Cleanup
        private GeneralLedgerClassConfiguration glClassConfiguration = null;
        private GeneralLedgerClassConfiguration shortAccountConfiguration = null;
        string glAccount = null;

        private string glClassName = "GL.CLASS";
        private List<string> glExpenseValues = new List<string>() { "5", "7" };
        private List<string> glRevenueValues = new List<string>() { "4", "6" };
        private List<string> glAssetValues = new List<string>() { "1" };
        private List<string> glLiabilityValues = new List<string>() { "2" };
        private List<string> glFundBalValues = new List<string>() { "3" };
        private GlClass GlClass = GlClass.Asset;

        private TestGlAccountRepository testGlAccountRepository;
        private CostCenterStructure costCenterStructure = new CostCenterStructure();
        private CostCenterStructure shortCostCenterStructure = new CostCenterStructure();
        private IList<string> majorComponentStartPositionLong = new List<string>() { "1", "4", "7", "10", "13", "19" };
        private IList<string> majorComponentStartPositionShort = new List<string>() { "1", "2", "3", "4", "10" };

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the GL Class configuration
            glClassConfiguration = new GeneralLedgerClassConfiguration(glClassName, glExpenseValues, glRevenueValues, glAssetValues, glLiabilityValues, glFundBalValues);
            glClassConfiguration.GlClassStartPosition = 18;
            glClassConfiguration.GlClassLength = 1;

            shortAccountConfiguration = new GeneralLedgerClassConfiguration(
                "GL.CLASS",
                new List<string>() { "6", "7" },
                new List<string>() { "4", "5" },
                new List<string>() { "1" },
                new List<string>() { "2" },
                new List<string>() { "3" });
            shortAccountConfiguration.GlClassStartPosition = 9;
            shortAccountConfiguration.GlClassLength = 1;

            testGlAccountRepository = new TestGlAccountRepository();
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));

            shortCostCenterStructure.AddCostCenterComponent(new GeneralLedgerComponent("DEPARTMENT", true, GeneralLedgerComponentType.Unit, "4", "6"));
            shortCostCenterStructure.AddCostCenterComponent(new GeneralLedgerComponent("OBJECT", true, GeneralLedgerComponentType.Object, "10", "5"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            glClassConfiguration = null;
            testGlAccountRepository = null;
        }
        #endregion

        #region GetGlAccountGlClass

        [TestMethod]
        public void GlClassIs_Asset()
        {
            glAccount = "11_01_01_00_00000_10000";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong), GlClass.Asset);
        }

        [TestMethod]
        public void GlClassIs_Liability()
        {
            glAccount = "11_01_01_00_00000_20000";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong), GlClass.Liability);
        }

        [TestMethod]
        public void GlClassIs_FundBalance()
        {
            glAccount = "11_01_01_00_00000_30000";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong), GlClass.FundBalance);
        }

        [TestMethod]
        public void GlClassIs_Revenue()
        {
            glAccount = "11_01_01_00_00000_40000";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong), GlClass.Revenue);
        }

        [TestMethod]
        public void GlClassIs_Expense()
        {
            glAccount = "11_01_01_00_00000_50000";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong), GlClass.Expense);
        }

        [TestMethod]
        public void LongGlAccount_GlClassIs_WithDelimiters()
        {
            glAccount = "11-01-01-00-00000-50000";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong), GlClass.Expense);
        }

        [TestMethod]
        public void LongGlAccount_GlClassIs_NoDelimiters()
        {
            glAccount = "110101000000050000";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong), GlClass.Expense);
        }

        [TestMethod]
        public void ShortGlAccount_GlClassIs_Expense()
        {
            glAccount = "9-7-0-970000-62006";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, shortAccountConfiguration, majorComponentStartPositionShort), GlClass.Expense);
        }

        [TestMethod]
        public void ShortGlAccount_GlClass_NoDelimiters()
        {
            glAccount = "97097000062006";
            Assert.AreEqual(GlAccountUtility.GetGlAccountGlClass(glAccount, shortAccountConfiguration, majorComponentStartPositionShort), GlClass.Expense);
        }

        [TestMethod]
        public void GetGlAccountGlClass_NullGlAccount()
        {
            var expectedParam = "glAccount";
            var actualParam = "";
            try
            {
                GlAccountUtility.GetGlAccountGlClass(null, shortAccountConfiguration, majorComponentStartPositionShort);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void GetGlAccountGlClass_EmptyGlAccount()
        {
            var expectedParam = "glAccount";
            var actualParam = "";
            try
            {
                GlAccountUtility.GetGlAccountGlClass("", shortAccountConfiguration, majorComponentStartPositionShort);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void GetGlAccountGlClass_NullConfiguration()
        {
            var expectedParam = "glClassConfiguration";
            var actualParam = "";
            try
            {
                glAccount = "9-7-0-970000-62006";
                GlAccountUtility.GetGlAccountGlClass(glAccount, null, majorComponentStartPositionShort);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void GetGlAccountGlClass_NullAccountStructure()
        {
            var expectedParam = "majorComponentStartPosition";
            var actualParam = "";
            try
            {
                glAccount = "9-7-0-970000-62006";
                GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void GlClassIs_Missing()
        {
            glAccount = "11_01_01_00_00000_10000";
            glClassConfiguration.GlClassStartPosition = 0;
            glClassConfiguration.GlClassLength = 0;
            GlClass glClass = GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong);
        }

        [TestMethod]
        public void GlClassIs_Invalid()
        {
            glAccount = "11_01_01_00_00000_90000";
            var expectedMessage = "Invalid glClass for GL account: " + glAccount;
            var actualMessage = "";
            try
            {
                GlClass glClass = GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, majorComponentStartPositionLong);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region GetCostCenterId

        [TestMethod]
        public void GetCostCenterId_Success()
        {
            // Cost center is composed of location, unit, and object.
            var costCenterId = GlAccountUtility.GetCostCenterId("10_11_12_13_33333_51001", this.costCenterStructure, majorComponentStartPositionLong);
            Assert.AreEqual("123333351001", costCenterId);
        }

        [TestMethod]
        public void GetCostCenterId_LongGlAccount_WithDelimiters_Success()
        {
            glAccount = "10-11-12-13-33333-51001";
            var costCenterId = GlAccountUtility.GetCostCenterId(glAccount, this.costCenterStructure, majorComponentStartPositionLong);
            Assert.AreEqual("123333351001", costCenterId);
        }

        [TestMethod]
        public void GetCostCenterId_LongGlAccount_NoDelimiters_Success()
        {
            glAccount = "101112133333351001";
            var costCenterId = GlAccountUtility.GetCostCenterId(glAccount, this.costCenterStructure, majorComponentStartPositionLong);
            Assert.AreEqual("123333351001", costCenterId);
        }

        [TestMethod]
        public void GetCostCenterId_ShortGlAccount_Success()
        {
            glAccount = "9-7-0-970000-62006";
            Assert.AreEqual("97000062006", GlAccountUtility.GetCostCenterId(glAccount, shortCostCenterStructure, majorComponentStartPositionShort));
        }

        [TestMethod]
        public void GetCostCenterId_ShortGlAccount_NoDelimiters_Success()
        {
            glAccount = "97097000062006";
            Assert.AreEqual("97000062006", GlAccountUtility.GetCostCenterId(glAccount, shortCostCenterStructure, majorComponentStartPositionShort));
        }

        [TestMethod]
        public void GetCostCenterId_NullGlNumber()
        {
            var costCenterId = GlAccountUtility.GetCostCenterId(null, this.costCenterStructure, majorComponentStartPositionLong);
            Assert.AreEqual("", costCenterId);
        }

        [TestMethod]
        public void GetCostCenterId_EmptyGlNumber()
        {
            var costCenterId = GlAccountUtility.GetCostCenterId("", this.costCenterStructure, majorComponentStartPositionLong);
            Assert.AreEqual("", costCenterId);
        }

        [TestMethod]
        public void GetCostCenterId_NullStructure()
        {
            var costCenterId = GlAccountUtility.GetCostCenterId("10_11_12_13_33333_51001", null, majorComponentStartPositionLong);
            Assert.AreEqual("", costCenterId);
        }

        [TestMethod]
        public void GetCostCenterId_NullAccountStructure()
        {
            var expectedParam = "majorComponentStartPosition";
            var actualParam = "";
            try
            {
                glAccount = "10_11_12_13_33333_51001";
                GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region EvaluateExclusionsForBudgetAdjustment
        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ShortGlAccount_OneError()
        {
            var glAccounts = new List<string>()
            {
                "9-7-0-970000-62006",
                "9-7-0-970000-62007"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "10", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("62007", "62007")
                },
            };

            var messages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionShort);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Object:62007: may not be used in budget adjustments.", messages.First());
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_SingleObjectViolation()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51001",
                "11_00_01_01_33333_51002"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51001", "51001")
                },
            };

            var messages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Object:51001: may not be used in budget adjustments.", messages.First());
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ObjectRangeViolations()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51001",
                "11_00_01_01_33333_51002"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51001", "51001")
                },
            };

            var expectedMessages = new List<string>()
            {
                "Object:51001: may not be used in budget adjustments.",
            };
            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(expectedMessages.Count, actualMessages.Count);
            foreach (var expectedMessage in expectedMessages)
            {
                var actualMessage = actualMessages.FirstOrDefault(x => x == expectedMessage);
                Assert.AreEqual(expectedMessage, actualMessage);
            }
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_DuplicateMessages()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51001",
                "11_00_01_01_33333_51003"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51001", "51001")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51000", "51002")
                },
            };

            var expectedMessages = new List<string>()
            {
                "Object:51001: may not be used in budget adjustments.",
            };
            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(expectedMessages.Count, actualMessages.Count);
            foreach (var expectedMessage in expectedMessages)
            {
                var actualMessage = actualMessages.FirstOrDefault(x => x == expectedMessage);
                Assert.AreEqual(expectedMessage, actualMessage);
            }
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ComplexExclusions()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "10_00_01_01_33333_51002",
                "10_00_01_01_33333_51003",
                "10_00_01_01_33333_51004",
                "10_00_01_01_33333_75000",
                "11_00_01_01_33333_75001",
                "11_00_01_01_33333_75A01",
                "11_00_01_01_33333_75a47",
                "11_00_01_01_33333_75N02",
                "11_00_01_01_33333_75N10"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51000", "51002")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75A10", "75N05")
                }
            };

            var expectedMessages = new List<string>()
            {
                "Object:51000: may not be used in budget adjustments.",
                "Object:51002: may not be used in budget adjustments.",
                "Object:75000: may not be used in budget adjustments.",
                "Object:75A47: may not be used in budget adjustments.",
                "Object:75N02: may not be used in budget adjustments."
            };
            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(expectedMessages.Count, actualMessages.Count);
            foreach (var expectedMessage in expectedMessages)
            {
                var actualMessage = actualMessages.FirstOrDefault(x => x == expectedMessage);
                Assert.AreEqual(expectedMessage, actualMessage);
            }
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ComplexExclusions_NoMessages()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "10_00_01_01_33333_51002",
                "10_00_01_01_33333_51003",
                "10_00_01_01_33333_51004",
                "10_00_01_01_33333_75000",
                "11_00_01_01_33333_75001",
                "11_00_01_01_33333_75A01",
                "11_00_01_01_33333_75a47",
                "11_00_01_01_33333_75N02",
                "11_00_01_01_33333_75N10"
            };
            IList<string> majorComponentStartPosition = new List<string>() { "1", "4", "7", "10", "13", "19" };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("50500", "50ZZZ")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51500", "51BBB")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75500", "75900")
                },
                 new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75P00", "75ZZZ")
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPosition);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_GlAccountsIsNull()
        {
            List<string> glAccounts = null;
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_GlAccountsContainsNull()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                null,
                "10_00_01_01_33333_51001"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ExclusionsObjectIsNull()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "10_00_01_01_33333_51001"
            };
            BudgetAdjustmentAccountExclusions exclusions = null;

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ExclusionComponentIsNull()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "10_00_01_01_33333_51001"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = null,
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ExclusionRangeListIsNull()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "10_00_01_01_33333_51001"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = null
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ExclusionRangeListContainsNull()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "10_00_01_01_33333_51001"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = null
                },
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_ExclusionRangeListHasValidEntriesButContainsANullEntry()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "10_00_01_01_33333_51002",
                "10_00_01_01_33333_51003",
                "10_00_01_01_33333_51004",
                "10_00_01_01_33333_75000",
                "11_00_01_01_33333_75001",
                "11_00_01_01_33333_75A01",
                "11_00_01_01_33333_75a47",
                "11_00_01_01_33333_75N02",
                "11_00_01_01_33333_75N10"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51000", "51000")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("51001", "51003")
                },
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = null
                },
            };

            var expectedMessages = new List<string>()
            {
                "Object:51000: may not be used in budget adjustments.",
                "Object:51002: may not be used in budget adjustments.",
                "Object:51003: may not be used in budget adjustments.",
            };
            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(expectedMessages.Count, actualMessages.Count);
            foreach (var expectedMessage in expectedMessages)
            {
                var actualMessage = actualMessages.FirstOrDefault(x => x == expectedMessage);
                Assert.AreEqual(expectedMessage, actualMessage);
            }
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_GlAccountsContainsEmpty()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51000",
                "",
                "10_00_01_01_33333_51001"
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(0, actualMessages.Count);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_GlNumberTooShortForStart()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_3",
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(1, actualMessages.Count);
            Assert.AreEqual(glAccounts[0].Replace("_", "") + ": is not a valid GL account.", actualMessages[0]);
        }

        [TestMethod]
        public void EvaluateExclusionsForBudgetAdjustment_GlNumberTooShortForLength()
        {
            var glAccounts = new List<string>()
            {
                "10_00_01_01_33333_51",
            };
            var exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>()
            {
                new BudgetAdjustmentExcludedElement
                {
                    ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5"),
                    ExclusionRange = new GeneralLedgerComponentRange("75000", "75000")
                }
            };

            var actualMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(glAccounts, exclusions, majorComponentStartPositionLong);
            Assert.AreEqual(1, actualMessages.Count);
            Assert.AreEqual(glAccounts[0].Replace("_", "") + ": is not a valid GL account.", actualMessages[0]);
        }
        #endregion

        #region ConvertGlAccountToInternalFormat

        [TestMethod]
        public void ConvertGlAccountToInternalFormat_NullGlAccount()
        {
            var expectedParam = "glAccount";
            var actualParam = "";
            try
            {
                GlAccountUtility.ConvertGlAccountToInternalFormat(null, majorComponentStartPositionLong);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void ConvertGlAccountToInternalFormat_NullAccountStructure()
        {
            var expectedParam = "majorComponentStartPosition";
            var actualParam = "";
            try
            {
                glAccount = "10_11_12_13_33333_51001";
                GlAccountUtility.ConvertGlAccountToInternalFormat(glAccount, null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        #endregion

        #region ConvertGlAccountToExternalFormat

        [TestMethod]
        public void ConvertGlAccountToExternalFormat_NullGlAccount()
        {
            var expectedParam = "glAccount";
            var actualParam = "";
            try
            {
                GlAccountUtility.ConvertGlAccountToExternalFormat(null, majorComponentStartPositionLong);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void ConvertGlAccountToExternalFormat_NullAccountStructure()
        {
            var expectedParam = "majorComponentStartPosition";
            var actualParam = "";
            try
            {
                glAccount = "10_11_12_13_33333_51001";
                GlAccountUtility.ConvertGlAccountToExternalFormat(glAccount, null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void ConvertGlAccountToExternalFormat_ShortGlAccount()
        {
            glAccount = "97097000062006";
            var formattedGlAccount = "9-7-0-970000-62006";
            Assert.AreEqual(GlAccountUtility.ConvertGlAccountToExternalFormat(glAccount, majorComponentStartPositionShort), formattedGlAccount);
        }

        [TestMethod]
        public void ConvertGlAccountToExternalFormat_LongGlAccount()
        {
            glAccount = "11_01_01_00_00000_50000";
            var formattedGlAccount = "11-01-01-00-00000-50000";
            Assert.AreEqual(GlAccountUtility.ConvertGlAccountToExternalFormat(glAccount, majorComponentStartPositionLong), formattedGlAccount);
        }

        #endregion
    }
}