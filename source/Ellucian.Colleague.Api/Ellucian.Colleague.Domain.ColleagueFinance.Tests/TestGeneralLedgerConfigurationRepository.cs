// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Ellucian.Dmi.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestGeneralLedgerConfigurationRepository : IGeneralLedgerConfigurationRepository
    {
        private GeneralLedgerComponentDescriptionBuilder ComponentDescriptionBuilder;
        public GeneralLedgerAccountStructure accountStructure;
        private CostCenterStructure costCenterStructure;
        private static char _SM = Convert.ToChar(DynamicArray.SM);
        private TestGlAccountRepository testGlAccountRepository;

        public int StartYear { get { return startYear; } }
        private int startYear;

        public string ClosedYear
        {
            get
            {
                return this.GenLdgrDataContracts.FirstOrDefault(x => x.GenLdgrStatus == "C").Recordkey;
            }
        }

        /// <summary>
        /// Initialize the test GL configuration repository.
        /// </summary>
        /// <param name="currentDate">Optional current date. This allows us to specify any date we want so we can test the system for a variety of years.</param>
        public TestGeneralLedgerConfigurationRepository(DateTime? currentDate = null, bool includeAllComponents = false, IEnumerable<string> glNumbers = null)
        {
            this.ComponentDescriptionBuilder = new GeneralLedgerComponentDescriptionBuilder();
            this.testGlAccountRepository = new TestGlAccountRepository();

            #region Initialize

            // Initialize the start month of the fiscal year data contract
            this.FiscalYearDataContract.FiscalStartMonth = 6;

            // Initialize the current date to today if it hasn't been specified.
            if (!currentDate.HasValue)
            {
                currentDate = DateTime.Now;
            }

            // Initialize the current year using the currentDate variable, but increase it to the next year
            // if the current month falls AFTER the start month.
            this.startYear = currentDate.Value.Year;
            if (currentDate.Value.Month >= this.FiscalYearDataContract.FiscalStartMonth.Value)
            {
                this.startYear += 1;
            }

            // Now that the start year has been calculated, use it to initialize the current fiscal year.
            this.FiscalYearDataContract.CfCurrentFiscalYear = startYear.ToString();

            // Calculate and set the current fiscal month.
            int currentFiscalMonth = 1;
            int currentCalendarMonth = DateTime.Now.Month;
            if (currentCalendarMonth >= FiscalYearDataContract.FiscalStartMonth)
            {
                currentFiscalMonth = currentCalendarMonth - FiscalYearDataContract.FiscalStartMonth.Value + 1;
            }
            else
            {
                currentFiscalMonth = 12 - FiscalYearDataContract.FiscalStartMonth.Value + currentCalendarMonth + 1;
            }

            this.FiscalYearDataContract.CurrentFiscalMonth = currentFiscalMonth;

            // Initialize the GenLdgr data contracts
            this.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = (this.startYear + 1).ToString(), GenLdgrStatus = "A" });
            this.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = this.startYear.ToString(), GenLdgrStatus = "O" });
            this.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = (this.startYear - 1).ToString(), GenLdgrStatus = "O" });
            this.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = (this.startYear - 2).ToString(), GenLdgrStatus = "C" });
            this.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = (this.startYear - 3).ToString(), GenLdgrStatus = "C" });
            this.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = (this.startYear - 4).ToString(), GenLdgrStatus = "C" });
            this.GenLdgrDataContracts.Add(new GenLdgr() { Recordkey = (this.startYear - 5).ToString(), GenLdgrStatus = "C" });
            #endregion

            #region Initialize the account structure domain entity

            accountStructure = new GeneralLedgerAccountStructure();
            accountStructure.GlAccountLength = "18";
            accountStructure.SetMajorComponentStartPositions(new List<string>() { "1", "4", "7", "10", "13", "19" });
            accountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE));
            accountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE));
            accountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE));
            accountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));
            accountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            accountStructure.AddMajorComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            accountStructure.FullAccessRole = "ALLACCESS";
            accountStructure.glDelimiter = "-";

            #endregion

            #region Initialize the cost center structure domain entity
            costCenterStructure = new CostCenterStructure();
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE));
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE));
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            costCenterStructure.AddObjectComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            costCenterStructure.CostCenterSubtotal = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.GL_SUBSCLASS_CODE);
            costCenterStructure.Unit = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE);

            if (includeAllComponents)
            {
                costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));
                costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE));
            }

            #endregion

            #region Initialize the component description data contracts
            string desc = "";

            // Get all the fund codes
            var component = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUND_CODE);
            var fundCodes = testGlAccountRepository.AllGlNumbers.Select(x => x.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength))).Distinct().ToList();

            for (int i = 0; i < fundCodes.Count; i++)
            {
                desc = "Fund #" + i;
                this.FdDescs.Add(new FdDescs() { Recordkey = fundCodes[i], FdDescription = desc });
            }

            // Get all the source codes
            component = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.SOURCE_CODE);
            var sourceCodes = testGlAccountRepository.AllGlNumbers.Select(x => x.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength))).Distinct().ToList();

            for (int i = 0; i < sourceCodes.Count; i++)
            {
                desc = "Source #" + i;
                this.SoDescs.Add(new SoDescs() { Recordkey = sourceCodes[i], SoDescription = desc });
            }

            // Get all the location codes
            component = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE);
            var locationCodes = testGlAccountRepository.AllGlNumbers.Select(x => x.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength))).Distinct().ToList();

            for (int i = 0; i < locationCodes.Count; i++)
            {
                desc = "Location #" + i;
                this.LoDescs.Add(new LoDescs() { Recordkey = locationCodes[i], LoDescription = desc });
            }

            // Get all the function codes
            component = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.FUNCTION_CODE);
            var functionCodes = testGlAccountRepository.AllGlNumbers.Select(x => x.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength))).Distinct().ToList();

            for (int i = 0; i < functionCodes.Count; i++)
            {
                desc = "Function #" + i;
                this.FcDescs.Add(new FcDescs() { Recordkey = functionCodes[i], FcDescription = desc });
            }

            // Get all the unit codes
            component = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE);
            var unitCodes = testGlAccountRepository.AllGlNumbers.Select(x => x.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength))).Distinct().ToList();

            for (int i = 0; i < unitCodes.Count; i++)
            {
                desc = "Unit #" + i;
                this.UnDescs.Add(new UnDescs() { Recordkey = unitCodes[i], UnDescription = desc });
            }

            // Get all the object codes
            component = testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE);

            List<string> objectCodes = null;
            if (glNumbers != null && glNumbers.Any())
            {
                objectCodes = testGlAccountRepository.SetGlNumbers(glNumbers).AllGlNumbers.Select(x => x.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength))).Distinct().ToList();
            }
            else
            {
                objectCodes = testGlAccountRepository.AllGlNumbers.Select(x => x.Substring(component.StartPosition, Convert.ToInt32(component.ComponentLength))).Distinct().ToList();
            }

            for (int i = 0; i < objectCodes.Count; i++)
            {
                desc = "Object #" + i;
                this.ObDescs.Add(new ObDescs() { Recordkey = objectCodes[i], ObDescription = desc });
            }
            #endregion
        }

        #region Fiscal Year Configuration
        public Fiscalyr FiscalYearDataContract = new Fiscalyr();

        public async Task<GeneralLedgerFiscalYearConfiguration> GetFiscalYearConfigurationAsync()
        {
            return await Task.FromResult(new GeneralLedgerFiscalYearConfiguration(FiscalYearDataContract.FiscalStartMonth.Value,
                FiscalYearDataContract.CfCurrentFiscalYear, FiscalYearDataContract.CurrentFiscalMonth.Value, 11, "Y"));
        }
        #endregion

        #region Account Structure
        public async Task<GeneralLedgerAccountStructure> GetAccountStructureAsync()
        {
            return await Task.FromResult(accountStructure);
        }

        public Glstruct GlStructDataContract = new Glstruct()
        {
            AcctSize = new List<string>() { "18", "6"},
            AcctNames = new List<string>() { "FUND", "PROGRAM", "LOCATION", "ACTIVITY", "DEPARTMENT", "OBJECT" },
            AcctStart = new List<string>() { "1", "4", "7", "10", "13", "19" },
            AcctLength = new List<int?>() { 2, 2, 2, 2, 5, 5 },
            AcctComponentType = new List<string>() { "FD", "SO", "LO", "FC", "UN", "OB" },
            GlFullAccessRole = "GTT",
            AcctSubStart = new List<string>() { "1" + _SM + "1", "4", "7", "10", "13" + _SM + "13" + _SM + "13" + _SM + "13", "19" + _SM + "19" + _SM + "19" + _SM + "19" + _SM + "19" },
            AcctSubLgth = new List<string>() { "1" + _SM + "2", "2", "2", "2", "1" + _SM + "2" + _SM + "3" + _SM + "5", "1" + _SM + "2" + _SM + "3" + _SM + "4" + _SM + "5" },
            AcctSubName = new List<string>() { "FUND" + _SM + "FUND.GROUP", "PROGRAM", "LOCATION", "ACTIVITY", "FUNCTION" + _SM + "DIVISION" + _SM + "SUBDIVISION" + _SM + "DEPARTMENT", "GL.CLASS" + _SM + "GL.SUBCLASS" + _SM + "CATEGORY" + _SM + "SUBCATEGORY" + _SM + "OBJECT" }
        };

        public CfwebDefaults CfWebDefaultsDataContract = new CfwebDefaults()
        {
            CfwebCkrCostCenterComps = new List<string>() { "FUND", "PROGRAM", "DEPARTMENT" },
            CfwebCkrCostCenterDescs = new List<string>() { "Y", "", "y" },
            CfwebCkrObjectCodeComps = new List<string>() { "ACTIVITY", "OBJECT" },
            CfwebCkrObjectCodeDescs = new List<string>() { "Y", "" },
            CfwebCostCenterSubtotals = new List<string>() { "GL.SUBCLASS" }
        };
        #endregion

        #region Cost Center Structure
        public async Task<CostCenterStructure> GetCostCenterStructureAsync()
        {
            return await Task.FromResult(costCenterStructure);
        }

        #endregion

        #region Component Descriptions
        public Collection<FdDescs> FdDescs = new Collection<FdDescs>();

        public Collection<SoDescs> SoDescs = new Collection<SoDescs>();

        public Collection<LoDescs> LoDescs = new Collection<LoDescs>();

        public Collection<FcDescs> FcDescs = new Collection<FcDescs>();

        public Collection<UnDescs> UnDescs = new Collection<UnDescs>();

        public Collection<ObDescs> ObDescs = new Collection<ObDescs>();

        /// <summary>
        /// Returns the descriptions for all GL components.
        /// </summary>
        public IEnumerable<GeneralLedgerComponentDescription> GetComponentDescriptions()
        {
            var componentDescriptions = new List<GeneralLedgerComponentDescription>();
            foreach (var desc in FcDescs)
            {
                var componentDescription = this.ComponentDescriptionBuilder.WithId(desc.Recordkey).WithComponentType(GeneralLedgerComponentType.Function).Build();
                componentDescription.Description = desc.FcDescription;
                componentDescriptions.Add(componentDescription);
            }

            foreach (var desc in FdDescs)
            {
                var componentDescription = this.ComponentDescriptionBuilder.WithId(desc.Recordkey).WithComponentType(GeneralLedgerComponentType.Fund).Build();
                componentDescription.Description = desc.FdDescription;
                componentDescriptions.Add(componentDescription);
            }

            foreach (var desc in LoDescs)
            {
                var componentDescription = this.ComponentDescriptionBuilder.WithId(desc.Recordkey).WithComponentType(GeneralLedgerComponentType.Location).Build();
                componentDescription.Description = desc.LoDescription;
                componentDescriptions.Add(componentDescription);
            }

            foreach (var desc in ObDescs)
            {
                var componentDescription = this.ComponentDescriptionBuilder.WithId(desc.Recordkey).WithComponentType(GeneralLedgerComponentType.Object).Build();
                componentDescription.Description = desc.ObDescription;
                componentDescriptions.Add(componentDescription);
            }

            foreach (var desc in SoDescs)
            {
                var componentDescription = this.ComponentDescriptionBuilder.WithId(desc.Recordkey).WithComponentType(GeneralLedgerComponentType.Source).Build();
                componentDescription.Description = desc.SoDescription;
                componentDescriptions.Add(componentDescription);
            }

            foreach (var desc in UnDescs)
            {
                var componentDescription = this.ComponentDescriptionBuilder.WithId(desc.Recordkey).WithComponentType(GeneralLedgerComponentType.Unit).Build();
                componentDescription.Description = desc.UnDescription;
                componentDescriptions.Add(componentDescription);
            }

            return componentDescriptions;
        }
        #endregion

        #region GL Class Configuration
        public Glclsdef GlClassDefDataContract = new Glclsdef()
        {
            GlClassDict = "GL.CLASS",
            GlClassExpenseValues = new List<string>() { "5", "7" },
            GlClassRevenueValues = new List<string>() { "4", "6" },
            GlClassAssetValues = new List<string>() { "1" },
            GlClassLiabilityValues = new List<string>() { "2" },
            GlClassFundBalValues = new List<string>() { "3" }
        };

        public async Task<GeneralLedgerClassConfiguration> GetClassConfigurationAsync()
        {
            return await Task.Run(() =>
                new GeneralLedgerClassConfiguration(GlClassDefDataContract.GlClassDict, GlClassDefDataContract.GlClassExpenseValues,
                    GlClassDefDataContract.GlClassRevenueValues, GlClassDefDataContract.GlClassAssetValues, GlClassDefDataContract.GlClassLiabilityValues,
                    GlClassDefDataContract.GlClassFundBalValues));
        }
        #endregion

        #region Available fiscal years
        public Collection<GenLdgr> GenLdgrDataContracts = new Collection<GenLdgr>();

        public async Task<IEnumerable<string>> GetAllFiscalYearsAsync(int currentFiscalYear)
        {
            var fiscalYears = new List<string>();
            // Add one future fiscal year.
            int futureYear = currentFiscalYear + 1;
            fiscalYears.Add(futureYear.ToString());
            while (fiscalYears.Count < 7)
            {
                fiscalYears.Add(currentFiscalYear.ToString());
                currentFiscalYear--;
            }

            return await Task.Run(() => GenLdgrDataContracts.Where(x => x != null && fiscalYears.Contains(x.Recordkey))
                .Select(x => x.Recordkey).ToList());
        }

        public async Task<IEnumerable<string>> GetAllOpenFiscalYears()
        {
            return await Task.Run(() => GenLdgrDataContracts.Where(x => x.GenLdgrStatus == "O").Select(x => x.Recordkey).ToList());
        }
        #endregion

        #region Budget adjustments

        public async Task<BudgetAdjustmentsEnabled> GetBudgetAdjustmentEnabledAsync()
        {
            return await Task.Run(() => { return new BudgetAdjustmentsEnabled(true); });
        }


        public BudgetWebDefaults BudgetWebDefaultsContract = new BudgetWebDefaults()
        {
            BudAdjExcludeCriteriaEntityAssociation = new List<BudgetWebDefaultsBudAdjExcludeCriteria>()
            {
                new BudgetWebDefaultsBudAdjExcludeCriteria
                {
                   BudWebExclComponentAssocMember = "OBJECT",
                   BudWebExclFromValuesAssocMember = "50000",
                   BudWebExclToValuesAssocMember = "52222"
                },
                 new BudgetWebDefaultsBudAdjExcludeCriteria
                {
                   BudWebExclComponentAssocMember = "GL.SUBCLASS",
                   BudWebExclFromValuesAssocMember = "53",
                   BudWebExclToValuesAssocMember = "53"
                },
                new BudgetWebDefaultsBudAdjExcludeCriteria
                {
                   BudWebExclComponentAssocMember = "FUND",
                   BudWebExclFromValuesAssocMember = "12",
                   BudWebExclToValuesAssocMember = "14"
                }
            }
        };

        public BudgetWebDefaults GetBudgetWebDefaultsContractAsync()
        {

            return BudgetWebDefaultsContract;
        }

        public async Task<BudgetAdjustmentAccountExclusions> GetBudgetAdjustmentAccountExclusionsAsync()
        {
            return await Task.Run(() => { return new BudgetAdjustmentAccountExclusions(); });
        }

        public async Task<BudgetAdjustmentParameters> GetBudgetAdjustmentParametersAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}