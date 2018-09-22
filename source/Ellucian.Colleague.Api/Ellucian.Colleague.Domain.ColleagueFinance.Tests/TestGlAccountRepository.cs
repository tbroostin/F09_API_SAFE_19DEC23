// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestGlAccountRepository
    {
        private readonly List<string> glNumbers = new List<string>();
        private List<GeneralLedgerComponent> glComponents;
        private GeneralLedgerAccountStructure accountStructure;
        private CostCenterStructure costCenterStructure;
        private string fundValue = "";
        private string sourceValue = "";
        private string locationValue = "";
        private string functionValue = "";
        private string locationSubclassValue = "";
        private string unitValue = "";
        private string unitSubclassValue = "";
        private string glSubclassValue = "";
        private string objectValue = "";
        private string glNumberValue = "";

        public static string FUND_CODE = "FUND";
        public static string SOURCE_CODE = "SOURCE";
        public static string LOCATION_CODE = "LOCATION";
        public static string LOCATION_SUBCLASS_CODE = "LOCATION_SUBCLASS";
        public static string FUNCTION_CODE = "FUNCTION";
        public static string UNIT_CODE = "UNIT";
        public static string UNIT_SUBCLASS_CODE = "UNIT_SUBCLASS";
        public static string OBJECT_CODE = "OBJECT";
        public static string GL_SUBSCLASS_CODE = "GL_SUBCLASS";

        public List<string> AllGlNumbers { get { return glNumbers.Distinct().ToList(); } }

        public List<GeneralLedgerComponent> GlComponents { get { return glComponents; } }

        public GeneralLedgerAccountStructure AccountStructure { get { return accountStructure; } }

        public CostCenterStructure CostCenterStructure { get { return costCenterStructure; } }

        public TestGlAccountRepository()
        {
            glComponents = new List<GeneralLedgerComponent>();
            //accountStructure.SetMajorComponentStartPositions(new List<string>() { "1", "4", "7", "10", "13", "19" });
            glComponents.Add(new GeneralLedgerComponent(FUND_CODE, true, GeneralLedgerComponentType.Fund, "1", "2"));
            glComponents.Add(new GeneralLedgerComponent(SOURCE_CODE, true, GeneralLedgerComponentType.Source, "4", "2"));
            glComponents.Add(new GeneralLedgerComponent(LOCATION_CODE, true, GeneralLedgerComponentType.Location, "7", "2"));
            glComponents.Add(new GeneralLedgerComponent(LOCATION_SUBCLASS_CODE, true, GeneralLedgerComponentType.Location, "7", "1"));
            glComponents.Add(new GeneralLedgerComponent(FUNCTION_CODE, true, GeneralLedgerComponentType.Function, "10", "2"));
            glComponents.Add(new GeneralLedgerComponent(UNIT_CODE, true, GeneralLedgerComponentType.Unit, "13", "5"));
            glComponents.Add(new GeneralLedgerComponent(UNIT_SUBCLASS_CODE, true, GeneralLedgerComponentType.Unit, "13", "3"));
            glComponents.Add(new GeneralLedgerComponent(OBJECT_CODE, true, GeneralLedgerComponentType.Object, "19", "5"));
            glComponents.Add(new GeneralLedgerComponent(GL_SUBSCLASS_CODE, true, GeneralLedgerComponentType.Object, "19", "2"));

            accountStructure = new GeneralLedgerAccountStructure();
            accountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUND_CODE));
            accountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == SOURCE_CODE));
            accountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == LOCATION_CODE));
            accountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUNCTION_CODE));
            accountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == UNIT_CODE));
            accountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == OBJECT_CODE));

            costCenterStructure = new CostCenterStructure();
            costCenterStructure.AddCostCenterComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUND_CODE));
            costCenterStructure.AddCostCenterComponent(glComponents.FirstOrDefault(x => x.ComponentName == SOURCE_CODE));
            costCenterStructure.AddCostCenterComponent(glComponents.FirstOrDefault(x => x.ComponentName == UNIT_CODE));

            InitializeGlNumbers();
        }

        #region Setters
        public void ResetGlNumbers()
        {
            glNumbers.Clear();
            InitializeGlNumbers();
        }

        public TestGlAccountRepository SetGlNumbers(IEnumerable<string> seedGlAccountNumbers)
        {
            glNumbers.Clear();
            glNumbers.AddRange(seedGlAccountNumbers);

            return this;
        }
        #endregion

        private void InitializeGlNumbers()
        {
            glNumbers.Add("10_00_01_01_33333_51001");
            glNumbers.Add("10_00_01_02_33333_51001");
            glNumbers.Add("10_00_01_03_33333_51001");
            glNumbers.Add("10_00_01_04_33333_51001");
            glNumbers.Add("10_00_01_05_33333_51001");
            glNumbers.Add("10_00_01_06_33333_51001");

            glNumbers.Add("10_00_01_01_44444_51001");
            glNumbers.Add("10_00_01_02_44444_51001");
            glNumbers.Add("10_00_01_03_44444_51001");
            glNumbers.Add("10_00_01_04_44444_51001");
            glNumbers.Add("10_00_01_05_44444_51001");

            glNumbers.Add("10_00_05_07_33333_51001");
            glNumbers.Add("11_00_05_00_33333_51002");
            glNumbers.Add("12_01_05_00_33333_51003");
            glNumbers.Add("13_01_04_00_44444_51001");

            #region Cost Center "1000AJK55" - AJK
            // Umbrella accounts
            glNumbers.Add("10_00_UM_U1_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_UM_U2_AJK55_51001"); // Subtotal: 01

            // Poolee accounts
            glNumbers.Add("10_00_P1_U1_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_P2_U1_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_P3_U1_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_P4_U2_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_P5_U2_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_P6_U2_AJK55_51001"); // Subtotal: 01

            // Non-pooled accounts
            glNumbers.Add("10_00_NP_00_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_NP_01_AJK55_51001"); // Subtotal: 01
            glNumbers.Add("10_00_NP_02_AJK55_51001"); // Subtotal: 01

            glNumbers.Add("10_00_NP_03_AJK55_51002"); // Subtotal: 02
            glNumbers.Add("10_00_NP_04_AJK55_51002"); // Subtotal: 02
            #endregion

            #region Cost Center "1000AJK66" - AJK
            // Non-pooled accounts
            glNumbers.Add("10_00_NP_00_AJK66_51001"); // Subtotal: 01
            glNumbers.Add("10_00_NP_01_AJK66_51001"); // Subtotal: 01
            glNumbers.Add("10_00_NP_02_AJK66_51001"); // Subtotal: 01

            glNumbers.Add("10_00_NP_03_AJK66_51002"); // Subtotal: 02
            glNumbers.Add("10_00_NP_04_AJK66_51002"); // Subtotal: 02
            #endregion

            #region Cost Center "1000EJK77" - AJK (only revenue accounts)
            // Non-pooled accounts
            glNumbers.Add("10_00_01_00_EJK77_41001"); // Subtotal: 41
            glNumbers.Add("10_00_01_01_EJK77_41001"); // Subtotal: 41
            glNumbers.Add("10_00_01_02_EJK77_41001"); // Subtotal: 41
            #endregion

            #region Cost Center "1000EJK88" - AJK (revenue accounts + one liability)
            // Non-pooled accounts
            glNumbers.Add("10_00_01_00_EJK88_41001"); // Subtotal: 41
            glNumbers.Add("10_00_01_01_EJK88_41001"); // Subtotal: 41
            glNumbers.Add("10_00_01_02_EJK88_11001"); // Subtotal: 11
            #endregion

            #region Cost Center "1000NMK67" - AJK
            // Umbrella accounts
            glNumbers.Add("10_00_UM_U1_NMK67_51001"); // Subtotal: 01

            // Poolee accounts
            glNumbers.Add("10_00_P1_U1_NMK67_51001"); // Subtotal: 01
            glNumbers.Add("10_00_P2_U1_NMK67_51001"); // Subtotal: 01
            glNumbers.Add("10_00_P3_U1_NMK67_51001"); // Subtotal: 01

            // Non-pooled accounts
            glNumbers.Add("10_00_NP_00_NMK67_51001"); // Subtotal: 01
            glNumbers.Add("10_00_NP_01_NMK67_51001"); // Subtotal: 01
            glNumbers.Add("10_00_NP_02_NMK67_51001"); // Subtotal: 01
            #endregion
        }

        #region Filters
        public TestGlAccountRepository WithFunction(string functionValue)
        {
            this.functionValue = functionValue;
            return this;
        }

        public TestGlAccountRepository WithFund(string fundValue)
        {
            this.fundValue = fundValue;
            return this;
        }

        public TestGlAccountRepository WithLocation(string locationValue)
        {
            this.locationValue = locationValue;
            return this;
        }

        public TestGlAccountRepository WithLocationSubclass(string locationSubclassValue)
        {
            this.locationSubclassValue = locationSubclassValue;
            return this;
        }

        public TestGlAccountRepository WithObject(string objectValue)
        {
            this.objectValue = objectValue;
            return this;
        }

        public TestGlAccountRepository WithSource(string sourceValue)
        {
            this.sourceValue = sourceValue;
            return this;
        }

        public TestGlAccountRepository WithUnit(string unitValue)
        {
            this.unitValue = unitValue;
            return this;
        }

        public TestGlAccountRepository WithUnitSubclass(string unitSubclassValue)
        {
            this.unitSubclassValue = unitSubclassValue;
            return this;
        }

        public TestGlAccountRepository WithGlSubclass(string glSubclassValue)
        {
            this.glSubclassValue = glSubclassValue;
            return this;
        }

        public TestGlAccountRepository WithGlNumber(string glNumber)
        {
            this.glNumberValue = glNumber;
            return this;
        }
        #endregion

        #region Getters
        public List<string> GetGlNumbersForOneCostCenter()
        {
            this.fundValue = "10";
            this.sourceValue = "00";
            this.unitValue = "AJK55";

            return GetFilteredGlNumbers();
        }

        public List<string> GetFilteredGlNumbers()
        {
            var accountsToReturn = glNumbers;
            if (!string.IsNullOrEmpty(fundValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == FUND_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == fundValue).ToList();
            }

            if (!string.IsNullOrEmpty(sourceValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == SOURCE_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == sourceValue).ToList();
            }

            if (!string.IsNullOrEmpty(locationValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == LOCATION_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == locationValue).ToList();
            }

            if (!string.IsNullOrEmpty(locationSubclassValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == LOCATION_SUBCLASS_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == locationSubclassValue).ToList();
            }

            if (!string.IsNullOrEmpty(functionValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == FUNCTION_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == functionValue).ToList();
            }

            if (!string.IsNullOrEmpty(unitValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == UNIT_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == unitValue).ToList();
            }

            if (!string.IsNullOrEmpty(unitSubclassValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == UNIT_SUBCLASS_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == unitSubclassValue).ToList();
            }

            if (!string.IsNullOrEmpty(glSubclassValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == GL_SUBSCLASS_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == glSubclassValue).ToList();
            }

            if (!string.IsNullOrEmpty(objectValue))
            {
                var component = glComponents.FirstOrDefault(x => x.ComponentName == OBJECT_CODE);
                accountsToReturn = accountsToReturn.Where(x => x.Substring(component.StartPosition, component.ComponentLength) == objectValue).ToList();
            }

            // Clear the component values so subsequent filters can be applied
            fundValue = "";
            sourceValue = "";
            locationValue = "";
            locationSubclassValue = "";
            functionValue = "";
            unitValue = "";
            unitSubclassValue = "";
            objectValue = "";

            return accountsToReturn.Distinct().ToList();
        }
        #endregion
    }
}