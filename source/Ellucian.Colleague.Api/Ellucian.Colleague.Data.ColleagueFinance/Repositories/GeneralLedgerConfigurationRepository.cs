// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    [RegisterType]
    public class GeneralLedgerConfigurationRepository : BaseColleagueRepository, IGeneralLedgerConfigurationRepository
    {
        private List<GeneralLedgerComponentDescription> componentDescriptions;

        public GeneralLedgerConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            componentDescriptions = new List<GeneralLedgerComponentDescription>();
        }

        #region Fiscal Year Configuration
        /// <summary>
        /// Returns the GL fiscal year information.
        /// </summary>
        public async Task<GeneralLedgerFiscalYearConfiguration> GetFiscalYearConfigurationAsync()
        {
            var fiscalYearConfiguration = await GetOrAddToCacheAsync<GeneralLedgerFiscalYearConfiguration>("GeneralLedgerFiscalYearConfiguration",
               async () => await BuildGlFiscalYearConfiguration());

            return fiscalYearConfiguration;

        }

        private async Task<GeneralLedgerFiscalYearConfiguration> BuildGlFiscalYearConfiguration()
        {
            var dataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);

            if (dataContract == null)
            {
                throw new ConfigurationException("Fiscal year data is not set up.");
            }

            if (!dataContract.FiscalStartMonth.HasValue)
            {
                throw new ConfigurationException("Fiscal year start month must have a value.");
            }

            if (dataContract.FiscalStartMonth < 1 || dataContract.FiscalStartMonth > 12)
            {
                throw new ConfigurationException("Fiscal year start month must be in between 1 and 12.");
            }

            if (string.IsNullOrEmpty(dataContract.CfCurrentFiscalYear))
            {
                throw new ConfigurationException("Current fiscal year must have a value.");
            }

            if (!dataContract.CurrentFiscalMonth.HasValue)
            {
                throw new ConfigurationException("Current fiscal month must have a value.");
            }

            if (dataContract.CurrentFiscalMonth < 1 || dataContract.CurrentFiscalMonth > 12)
            {
                throw new ConfigurationException("Current fiscal month must be in between 1 and 12.");
            }

            var accountStructure = await GetGlstructAsync();
            int numberOfFuturePeriods = 0;
            string accountFuturePeriods = accountStructure.AcctFuturePeriods;
            // If the number of future periods is not set, default it to 0.
            if (string.IsNullOrEmpty(accountFuturePeriods))
            {
                numberOfFuturePeriods = 0;
            }
            else
            {
                numberOfFuturePeriods = Int32.Parse(accountFuturePeriods);
            }

            string accountFDateOvrdFyear = accountStructure.AcctFdateOvrdFyear;
            // If the future date overrides future year is not set, default it to "N"o.
            if ((string.IsNullOrEmpty(accountFDateOvrdFyear)) || (accountFDateOvrdFyear.ToUpperInvariant() != "Y"))
            {
                accountFDateOvrdFyear = "N";
            }

            return new GeneralLedgerFiscalYearConfiguration(dataContract.FiscalStartMonth.Value, dataContract.CfCurrentFiscalYear, dataContract.CurrentFiscalMonth.Value, numberOfFuturePeriods, accountFDateOvrdFyear.ToUpperInvariant());
        }
        #endregion

        #region Account Structure
        /// <summary>
        /// Get the General Ledger Account structure for Colleague Financials.
        /// </summary>
        /// <returns>General Ledger account configuration.</returns>
        public async Task<GeneralLedgerAccountStructure> GetAccountStructureAsync()
        {
            var accountStructure = await GetOrAddToCacheAsync<GeneralLedgerAccountStructure>("GeneralLedgerAccountStructure",
                async () => await BuildAccountStructure());

            return accountStructure;
        }

        private async Task<GeneralLedgerAccountStructure> BuildAccountStructure()
        {
            var glAccountStructure = new GeneralLedgerAccountStructure();

            var glStruct = await GetGlstructAsync();

            // The length of the GL account without delimiters or underscores is the first value in the list.
            glAccountStructure.GlAccountLength = glStruct.AcctSize[0];

            glAccountStructure.SetMajorComponentStartPositions(glStruct.AcctStart);
            glAccountStructure.FullAccessRole = glStruct.GlFullAccessRole;
            glAccountStructure.CheckAvailableFunds = glStruct.AcctCheckAvailFunds;

            // Get the major components and subcomponents.
            for (int i = 0; i < glStruct.AcctNames.Count; i++)
            {
                var componentName = glStruct.AcctNames[i];
                bool isPartOfDescription = false;

                var majorComponent = BuildGeneralLedgerComponent(componentName, isPartOfDescription, glStruct);

                glAccountStructure.AddMajorComponent(majorComponent);
            }

            // Make sure the subcomponent data has been stored properly in the domain entity.
            for (int i = 0; i < glStruct.AcctNames.Count; i++)
            {
                #region null checks
                if (glStruct.AcctSubName == null || !glStruct.AcctSubName.Any())
                {
                    throw new ConfigurationException("GL account structure is missing subcomponent name definitions.");
                }

                if (glStruct.AcctSubStart == null || !glStruct.AcctSubStart.Any())
                {
                    throw new ConfigurationException("GL account structure is missing subcomponent start position definitions.");
                }

                if (glStruct.AcctSubLgth == null || !glStruct.AcctSubLgth.Any())
                {
                    throw new ConfigurationException("GL account structure is missing subcomponent length definitions.");
                }
                #endregion

                // Get the subcomponent names, start positions, and lengths for the given major component.
                var majorComponentType = DetermineComponentType(glStruct.AcctComponentType[i]);
                var subcomponentNames = glStruct.AcctSubName[i].Split(DmiString._SM);
                var subcomponentStartPositions = glStruct.AcctSubStart[i].Split(DmiString._SM);
                var subcomponentLengths = glStruct.AcctSubLgth[i].Split(DmiString._SM);

                // Grab the individual subcomponent name, start position, and length and make sure that subcomponent is represented in the entity.
                for (int j = 0; j < subcomponentNames.Length; j++)
                {
                    if (string.IsNullOrEmpty(subcomponentNames[j]))
                    {
                        throw new ConfigurationException("GL account structure is missing subcomponent name definitions.");
                    }

                    var subcomponentName = subcomponentNames[j].ToString();
                    var subcomponentStartPosition = subcomponentStartPositions[j];
                    var subcomponentLength = subcomponentLengths[j];

                    glAccountStructure.AddSubcomponent(new GeneralLedgerComponent(subcomponentName, false, majorComponentType, subcomponentStartPosition, subcomponentLength));
                }
            }

            glAccountStructure.glDelimiter = glStruct.AcctDlm;
            glAccountStructure.AccountOverrideTokens = glStruct.AcctOverrideTokens;

            return glAccountStructure;
        }

        #endregion

        #region Cost Center Structure

        /// <summary>
        /// Get the GL Cost Center configuration for Colleague Financials.
        /// </summary>
        /// <returns>General Ledger configuration</returns>
        public async Task<CostCenterStructure> GetCostCenterStructureAsync()
        {
            var costCenterStructure = await GetOrAddToCacheAsync<CostCenterStructure>("GeneralLedgerCostCenterStructure",
                async () => await BuildCostCenterStructure());

            return costCenterStructure;
        }

        private async Task<CostCenterStructure> BuildCostCenterStructure()
        {
            var costCenterStructure = new CostCenterStructure();

            var glStruct = await GetGlstructAsync();

            // Get the cost center and object components.
            var componentInfoDataContract = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");
            if (componentInfoDataContract == null)
                throw new ConfigurationException("GL component information is not defined.");

            // Add the cost center components
            for (int i = 0; i < componentInfoDataContract.CfwebCkrCostCenterComps.Count; i++)
            {
                var componentName = componentInfoDataContract.CfwebCkrCostCenterComps[i];
                bool isPartOfDescription = false;

                // Only look at the index in the the descriptions list if we're still within the bounds of the array.
                if (i < componentInfoDataContract.CfwebCkrCostCenterDescs.Count)
                    isPartOfDescription = componentInfoDataContract.CfwebCkrCostCenterDescs[i].ToUpper() == "Y";

                var costCenterComponent = BuildGeneralLedgerComponent(componentName, isPartOfDescription, glStruct);

                costCenterStructure.AddCostCenterComponent(costCenterComponent);
            }

            // Add the object components
            for (int i = 0; i < componentInfoDataContract.CfwebCkrObjectCodeComps.Count; i++)
            {
                var componentName = componentInfoDataContract.CfwebCkrObjectCodeComps[i];
                bool isPartOfDescription = false;

                // Only look at the index in the the descriptions list if we're still within the bounds of the array.
                if (i < componentInfoDataContract.CfwebCkrObjectCodeDescs.Count)
                    isPartOfDescription = componentInfoDataContract.CfwebCkrObjectCodeDescs[i].ToUpper() == "Y";

                var objectComponent = BuildGeneralLedgerComponent(componentName, isPartOfDescription, glStruct);

                costCenterStructure.AddObjectComponent(objectComponent);
            }

            // Obtain the information for the cost center subtotals. If there is no value 
            // for the subtotal, use the object major component to subtotal the cost center.

            // AcctSubName is a list of strings but each string contains each major component's subcomponents separated by subvalue marks.
            // Example glStruct.AcctSubName[0] contains "FUND.GROUP":@SV:"FUND"
            var subcomponentList = new List<string>();
            if (glStruct.AcctSubName != null && glStruct.AcctSubName.Any())
            {
                foreach (var subName in glStruct.AcctSubName)
                {
                    string[] subvalues = subName.Split(DmiString._SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentList.Add(sub);
                    }
                }
            }

            // Same with the list of subcomponent start positions.
            var subcomponentStartList = new List<string>();
            if (glStruct.AcctSubStart != null && glStruct.AcctSubStart.Any())
            {
                foreach (var subStart in glStruct.AcctSubStart)
                {
                    string[] subvalues = subStart.Split(DmiString._SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentStartList.Add(sub);
                    }
                }
            }

            // Same with the list of subcomponent lengths.
            var subcomponentLengthList = new List<string>();
            if (glStruct.AcctSubLgth != null && glStruct.AcctSubLgth.Any())
            {
                foreach (var subLgth in glStruct.AcctSubLgth)
                {
                    string[] subvalues = subLgth.Split(DmiString._SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentLengthList.Add(sub);
                    }
                }
            }

            // Determine if the cost center subtotal component is defined in Colleague.
            // If it is defined, determine the information for the subtotal component.
            // If the subtotal component is not defined, default it to the GL class value
            // and set the correspondent property to false.
            bool subtotalDescription = false;

            if (componentInfoDataContract.CfwebCostCenterSubtotals != null && componentInfoDataContract.CfwebCostCenterSubtotals.Any())
            {
                var costCenterSubtotalComponent = componentInfoDataContract.CfwebCostCenterSubtotals.FirstOrDefault();
                var subtotalPosition = subcomponentList.FindIndex(x => x.Equals(costCenterSubtotalComponent));
                var subtotalStartPosition = subcomponentStartList[subtotalPosition];
                var subtotalLength = subcomponentLengthList[subtotalPosition];

                if (string.IsNullOrEmpty(costCenterSubtotalComponent))
                    throw new ArgumentNullException("costCenterSubtotalComponent", "costCenterSubtotalComponent must have a value.");

                if (string.IsNullOrEmpty(subtotalLength))
                    throw new ArgumentNullException("subtotalLength", "subtotalLength must have a value.");

                if (string.IsNullOrEmpty(subtotalStartPosition))
                    throw new ArgumentNullException("subtotalStartPosition", "subtotalStartPosition must have a value.");

                int requestedStartPosition;
                if (Int32.TryParse(subtotalStartPosition, out requestedStartPosition))
                {
                    if ((requestedStartPosition - 1) < 0)
                    {
                        throw new ApplicationException("The component start position cannot be negative.");
                    }
                }
                else
                {
                    throw new ApplicationException("The component start is not an integer.");
                }

                int requestedLength;
                bool result = Int32.TryParse(subtotalLength, out requestedLength);
                if (result)
                {
                    if ((requestedLength - 1) < 0)
                    {
                        throw new ApplicationException("Invalid length specified for GL component.");
                    }
                }
                else
                {
                    throw new ApplicationException("The component length is not an integer.");
                }

                try
                {
                    costCenterStructure.CostCenterSubtotal = new GeneralLedgerComponent(costCenterSubtotalComponent, subtotalDescription, GeneralLedgerComponentType.Object, subtotalStartPosition, subtotalLength);
                    // Set the property to indicate the user defined a cost center subtotal.
                    costCenterStructure.IsCostCenterSubtotalDefined = true;
                }
                catch (ArgumentNullException anex)
                {
                    logger.Info(anex.Message);
                }
                catch (ApplicationException apex)
                {
                    logger.Info(apex.Message);
                }
            }
            else
            {
                // If there is not a value for the cost center subtotal, cost centers will not be broken down by subtotal.
                // Default the value for the GL class as the subtotal id so we can still group the cost center GL accounts
                // by GL class type within the cost center.

                // Obtain the information for the GL Class.
                var glClassConfiguration = await GetClassConfigurationAsync();

                // Assign the GL Class as the default cost center subtotal.
                var costCenterSubtotalComponent = glClassConfiguration.ClassificationName;
                var subtotalPosition = subcomponentList.FindIndex(x => x.Equals(costCenterSubtotalComponent));
                var subtotalStartPosition = subcomponentStartList[subtotalPosition];
                var subtotalLength = subcomponentLengthList[subtotalPosition];

                if (string.IsNullOrEmpty(costCenterSubtotalComponent))
                    throw new ArgumentNullException("costCenterSubtotalComponent", "costCenterSubtotalComponent must have a value.");

                if (string.IsNullOrEmpty(subtotalLength))
                    throw new ArgumentNullException("subtotalLength", "subtotalLength must have a value.");

                if (string.IsNullOrEmpty(subtotalStartPosition))
                    throw new ArgumentNullException("subtotalStartPosition", "subtotalStartPosition must have a value.");

                int requestedStartPosition;
                if (Int32.TryParse(subtotalStartPosition, out requestedStartPosition))
                {
                    if ((requestedStartPosition - 1) < 0)
                    {
                        throw new ApplicationException("The component start position cannot be negative.");
                    }
                }
                else
                {
                    throw new ApplicationException("The component start is not an integer.");
                }

                int requestedLength;
                if (Int32.TryParse(subtotalLength, out requestedLength))
                {
                    if ((requestedLength - 1) < 0)
                    {
                        throw new ApplicationException("Invalid length specified for GL component.");
                    }
                }
                else
                {
                    throw new ApplicationException("The component length is not an integer.");
                }

                try
                {
                    // Create the default cost center subtotal.
                    costCenterStructure.CostCenterSubtotal = new GeneralLedgerComponent(costCenterSubtotalComponent, subtotalDescription, GeneralLedgerComponentType.Object, subtotalStartPosition, subtotalLength);

                    // Set the property to differentiate a defaulted subtotal by GL CLASS (will not be displayed to the user)
                    // vs the case where the user has actually defined the GL Class as the subtotal (will be displayed to the user).
                    costCenterStructure.IsCostCenterSubtotalDefined = false;
                }
                catch (ArgumentNullException anex)
                {
                    logger.Info(anex.Message);
                }
                catch (ApplicationException apex)
                {
                    logger.Info(apex.Message);
                }
            }

            // Determine the information for the UN component
            bool unDescription = false;
            foreach (var glMajorAssoc in glStruct.GlmajorEntityAssociation)
            {
                if (glMajorAssoc.AcctComponentTypeAssocMember == "UN")
                {
                    var unComponent = glMajorAssoc.AcctNamesAssocMember;
                    var unStartPosition = glMajorAssoc.AcctStartAssocMember;
                    var unLength = glMajorAssoc.AcctLengthAssocMember.ToString();

                    if (string.IsNullOrEmpty(unComponent))
                        throw new ArgumentNullException("costCenterSubtotalComponent", "costCenterSubtotalComponent must have a value.");

                    if (string.IsNullOrEmpty(unLength))
                        throw new ArgumentNullException("unLength", "unLength must have a value.");

                    if (string.IsNullOrEmpty(unStartPosition))
                        throw new ArgumentNullException("unStartPosition", "unStartPosition must have a value.");

                    int requestedStartPosition;
                    if (Int32.TryParse(unStartPosition, out requestedStartPosition))
                    {
                        if ((requestedStartPosition - 1) < 0)
                        {
                            throw new ApplicationException("The component start position cannot be negative.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException("The component start is not an integer.");
                    }

                    int requestedLength;
                    if (Int32.TryParse(unLength, out requestedLength))
                    {
                        if ((requestedLength - 1) < 0)
                        {
                            throw new ApplicationException("Invalid length specified for GL component.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException("The component length is not an integer.");
                    }

                    try
                    {
                        costCenterStructure.Unit = new GeneralLedgerComponent(unComponent, unDescription, GeneralLedgerComponentType.Unit, unStartPosition, unLength);
                    }
                    catch (ArgumentNullException anex)
                    {
                        logger.Info(anex.Message);
                    }
                    catch (ApplicationException apex)
                    {
                        logger.Info(apex.Message);
                    }
                }
            }

            return costCenterStructure;
        }
        #endregion

        #region GL Class Configuration
        public async Task<GeneralLedgerClassConfiguration> GetClassConfigurationAsync()
        {
            var classConfiguration = await GetOrAddToCacheAsync<GeneralLedgerClassConfiguration>("GeneralLedgerClassConfiguration",
                async () => await BuildGlClassConfiguration());

            return classConfiguration;
        }

        private async Task<GeneralLedgerClassConfiguration> BuildGlClassConfiguration()
        {
            var dataContract = await DataReader.ReadRecordAsync<Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
            if (dataContract == null)
            {
                throw new ConfigurationException("GL class definition is not defined.");
            }

            if (string.IsNullOrEmpty(dataContract.GlClassDict))
            {
                throw new ConfigurationException("GL class name is not defined.");
            }

            if (dataContract.GlClassExpenseValues == null)
            {
                throw new ConfigurationException("GL class expense values are not defined.");
            }

            if (dataContract.GlClassRevenueValues == null)
            {
                throw new ConfigurationException("GL class revenue values are not defined.");
            }

            if (dataContract.GlClassAssetValues == null)
            {
                throw new ConfigurationException("GL class asset values are not defined.");
            }

            if (dataContract.GlClassLiabilityValues == null)
            {
                throw new ConfigurationException("GL class liability values are not defined.");
            }

            if (dataContract.GlClassFundBalValues == null)
            {
                throw new ConfigurationException("GL class fund balance values are not defined.");
            }

            var glClassConfiguration = new GeneralLedgerClassConfiguration(dataContract.GlClassDict,
                dataContract.GlClassExpenseValues,
                dataContract.GlClassRevenueValues,
                dataContract.GlClassAssetValues,
                dataContract.GlClassLiabilityValues,
                dataContract.GlClassFundBalValues);

            var glStruct = await GetGlstructAsync();

            // Locate the GL Class name, dataContract.GlGlClassDict, in the list of subcomponents and get the start position and length.

            var subcomponentList = BuildSubcomponentList(glStruct);
            var subcomponentStartList = BuildSubcomponentStartList(glStruct);
            var subcomponentLengthList = BuildSubcomponentLengthtList(glStruct);

            var glClassPosition = subcomponentList.FindIndex(x => x.Equals(dataContract.GlClassDict));
            var glClassStartPosition = subcomponentStartList[glClassPosition];
            var glClassLength = subcomponentLengthList[glClassPosition];

            if (string.IsNullOrEmpty(glClassStartPosition))
                throw new ArgumentNullException("glClassStartPosition", "glClassStartPosition must have a value.");

            if (string.IsNullOrEmpty(glClassLength))
                throw new ArgumentNullException("glClassLength", "glClassLength must have a value.");

            int requestedStartPosition;
            if (Int32.TryParse(glClassStartPosition, out requestedStartPosition))
            {
                if ((requestedStartPosition - 1) < 0)
                {
                    throw new ApplicationException("The GL class subcomponent start position cannot be negative.");
                }
            }
            else
            {
                throw new ApplicationException("The GL class subcomponent start position is not an integer.");
            }

            int requestedLength;
            bool result = Int32.TryParse(glClassLength, out requestedLength);
            if (result)
            {
                if ((requestedLength - 1) < 0)
                {
                    throw new ApplicationException("The GL class subcomponent has an invalid length specified.");
                }
            }
            else
            {
                throw new ApplicationException("The GL class subcomponent length is not an integer.");
            }

            // Adjust the start position since C# index starts at zero.
            glClassConfiguration.GlClassStartPosition = requestedStartPosition - 1;
            glClassConfiguration.GlClassLength = requestedLength;

            return glClassConfiguration;
        }
        #endregion

        #region Fiscal years
        /// <summary>
        /// Return a set of fiscal years; the current year, up to five previous years and one future year.
        /// </summary>
        /// <param name="currentFiscalYear"></param>
        /// <returns>Set of fiscal years.</returns>
        public async Task<IEnumerable<string>> GetAllFiscalYearsAsync(int currentFiscalYear)
        {
            // It will return up to 7 fiscal years.
            var fiscalYears = new List<string>();

            // Calculate which one is a future fiscal year.
            int futureYear = currentFiscalYear + 1;
            fiscalYears.Add(futureYear.ToString());

            // Add the current fiscal year and calculate which ones are the previous five fiscal years.
            while (fiscalYears.Count < 7)
            {
                fiscalYears.Add(currentFiscalYear.ToString());
                currentFiscalYear--;
            }

            var genLdgrDataContracts = await DataReader.BulkReadRecordAsync<GenLdgr>(fiscalYears.ToArray());

            if (genLdgrDataContracts == null || genLdgrDataContracts.Count <= 0)
                throw new ConfigurationException("No fiscal years have been set up.");

            // Return those fiscal years for which there is a GEN.LDGR record created.
            return genLdgrDataContracts.Where(x => x != null && fiscalYears.Contains(x.Recordkey))
                .Select(x => x.Recordkey).ToList();
        }

        /// <summary>
        /// Get all of the open fiscal years.
        /// </summary>
        /// <returns>Set of fiscal years.</returns>
        public async Task<IEnumerable<string>> GetAllOpenFiscalYears()
        {
            var fiscalYearIds = await DataReader.SelectAsync("GEN.LDGR", "GEN.LDGR.STATUS EQ 'O'");
            if (fiscalYearIds == null)
            {
                throw new ConfigurationException("Error selecting open fiscal years.");
            }

            if (!fiscalYearIds.Any())
            {
                throw new ConfigurationException("There are no open fiscal years.");
            }

            return fiscalYearIds.ToList();
        }
        #endregion

        #region Budget adjustments

        /// <summary>
        /// Get indicator that determines whether budget adjustments are turned on or off.
        /// </summary>
        /// <returns>BudgetAdjustmentsEnabled entity</returns>
        public async Task<BudgetAdjustmentsEnabled> GetBudgetAdjustmentEnabledAsync()
        {
            var dataContract = await DataReader.ReadRecordAsync<BudgetWebDefaults>("CF.PARMS", "BUDGET.WEB.DEFAULTS");
            var enabled = false;
            if (dataContract != null && dataContract.BudWebBudAdjAllowed != null && dataContract.BudWebBudAdjAllowed.ToUpperInvariant() == "Y")
            {
                enabled = true;
            }

            return new BudgetAdjustmentsEnabled(enabled);
        }

        /// <summary>
        /// Get parameters that control allowed/required business rules for budget adjustments.
        /// </summary>
        /// <returns>BudgetAdjustmentParameters entity.</returns>
        public async Task<BudgetAdjustmentParameters> GetBudgetAdjustmentParametersAsync()
        {
            var dataContract = await DataReader.ReadRecordAsync<BudgetWebDefaults>("CF.PARMS", "BUDGET.WEB.DEFAULTS");
            var sameCostCenterRequired = true;
            var approvalRequired = false;
            var sameCostCenterApprovalRequired = false;



            if (dataContract != null)
            {
                // Only when the parameter is specifically "N" are same cost centers not required.
                if (!string.IsNullOrWhiteSpace(dataContract.BudAdjSameCstCntrRequrd) && dataContract.BudAdjSameCstCntrRequrd.ToUpperInvariant() == "N")
                {
                    sameCostCenterRequired = false;
                }
                // Only when the parameter is specifically "Y" are approvals required.
                if (!string.IsNullOrWhiteSpace(dataContract.BudAdjApprovalNeededFlag) && dataContract.BudAdjApprovalNeededFlag.ToUpperInvariant() == "Y")
                {
                    approvalRequired = true;
                }
                // Only when the parameter is specifically "Y" are approvals required for budget adjustments in the same cost center.
                if (!string.IsNullOrWhiteSpace(dataContract.BudAdjSameCcApprReq) && dataContract.BudAdjSameCcApprReq.ToUpperInvariant() == "Y")
                {
                    sameCostCenterApprovalRequired = true;
                }
            }

            return new BudgetAdjustmentParameters(sameCostCenterRequired, approvalRequired, sameCostCenterApprovalRequired);
        }

        /// <summary>
        /// Get the exclusion data for evaluating which GL accounts may be used in a budget adjustment.
        /// </summary>
        /// <returns>BudgetAdjustmentAccountExclusions entity</returns>
        public async Task<BudgetAdjustmentAccountExclusions> GetBudgetAdjustmentAccountExclusionsAsync()
        {
            var exclusions = new BudgetAdjustmentAccountExclusions();
            var dataContract = await DataReader.ReadRecordAsync<BudgetWebDefaults>("CF.PARMS", "BUDGET.WEB.DEFAULTS");
            var accountStructure = await GetAccountStructureAsync();

            if (dataContract == null)
            {
                throw new ConfigurationException("Budget web defaults not configured.");
            }

            /*
             * The data in this contract is conditionally required and the API must respond in the following ways:
             *  - If no exclusion data is defined then return an empty (and valid) exclusion object.
             *  - If a exclusion component is NOT defined but its range values were defined then return an error message.
             *  - If a exclusion component IS defined and the range values are NOT defined properly then return an error message.
             */

            if (dataContract.BudAdjExcludeCriteriaEntityAssociation != null && dataContract.BudAdjExcludeCriteriaEntityAssociation.Any())
            {
                foreach (var element in dataContract.BudAdjExcludeCriteriaEntityAssociation)
                {
                    if (element != null)
                    {
                        // Initialize a GL component to exclude.
                        var excludedElement = new BudgetAdjustmentExcludedElement();

                        // If the GL component is null or empty but there is a from or to value, throw an error.
                        if (string.IsNullOrWhiteSpace(element.BudWebExclComponentAssocMember))
                        {
                            if (!string.IsNullOrWhiteSpace(element.BudWebExclFromValuesAssocMember) || !string.IsNullOrWhiteSpace(element.BudWebExclToValuesAssocMember))
                            {
                                throw new ConfigurationException("Invalid budget adjustment GL account exclusions web parameters.");
                            }
                        }
                        // If there is a component but both from and to values are not filled, throw a value.
                        else
                        {
                            if (string.IsNullOrWhiteSpace(element.BudWebExclFromValuesAssocMember) || string.IsNullOrWhiteSpace(element.BudWebExclToValuesAssocMember))
                            {
                                throw new ConfigurationException("Invalid budget adjustment GL account exclusions web parameters.");
                            }
                        }

                        if (!string.IsNullOrEmpty(element.BudWebExclComponentAssocMember) && !string.IsNullOrEmpty(element.BudWebExclFromValuesAssocMember) && !string.IsNullOrEmpty(element.BudWebExclToValuesAssocMember))
                        {
                            // The user can select to use the full GL account number as part of the criteria.
                            if (element.BudWebExclComponentAssocMember.ToUpperInvariant() == "FULLACCOUNT")
                            {
                                var glAccountLength = accountStructure.GlAccountLength;
                                var fullGlAccountComponent = new GeneralLedgerComponent("FULLACCOUNT", false, GeneralLedgerComponentType.FullAccount, "1", glAccountLength);
                                // Populate the excluded GL account component.
                                excludedElement.ExclusionComponent = fullGlAccountComponent;
                            }
                            else
                            {
                                // It is one of the GL account structure components.  Validate the GL component name against
                                // the ones in the GL account structure and get the General Ledger component object built.
                                var glComponent = accountStructure.Subcomponents.FirstOrDefault(x => x.ComponentName == element.BudWebExclComponentAssocMember);
                                if (glComponent == null)
                                {
                                    throw new ConfigurationException("Invalid budget adjustment GL account exclusions web parameters.");
                                }
                                // Populate the excluded GL account component.
                                excludedElement.ExclusionComponent = glComponent;
                            }

                            try
                            {
                                // Populate the range values.
                                // For FULLACCOUNT, the values do not contain delimiters or underscores.
                                excludedElement.ExclusionRange = new GeneralLedgerComponentRange(element.BudWebExclFromValuesAssocMember, element.BudWebExclToValuesAssocMember);
                                exclusions.ExcludedElements.Add(excludedElement);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                                throw new ConfigurationException("Invalid budget adjustment GL account exclusions web parameters.");
                            }
                        }
                    }
                }
            }

            return exclusions;
        }
        #endregion

        #region private methods

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

        private GeneralLedgerComponent BuildGeneralLedgerComponent(string componentName, bool isPartOfDescription, Glstruct glStruct)
        {
            GeneralLedgerComponent glComponent = null;

            if (string.IsNullOrEmpty(componentName))
                throw new ConfigurationException("Component name for GL structure is not defined.");

            // Locate the GLSTRUCT entry for this component.
            var glStructEntry = glStruct.GlmajorEntityAssociation.Where(x => x.AcctNamesAssocMember == componentName).FirstOrDefault();
            if (glStructEntry == null)
                throw new ConfigurationException("GL structure information is not defined.");

            if (string.IsNullOrEmpty(glStructEntry.AcctStartAssocMember))
                throw new ConfigurationException("Start position for GL component not defined.");

            if (glStructEntry.AcctLengthAssocMember < 1)
                throw new ConfigurationException("Invalid length specified for GL component.");

            if (string.IsNullOrEmpty(glStructEntry.AcctLengthAssocMember.ToString()))
                throw new ConfigurationException("Length for GL component not defined.");

            int requestedStartPosition;
            if (Int32.TryParse(glStructEntry.AcctStartAssocMember, out requestedStartPosition))
            {
                if ((requestedStartPosition - 1) < 0)
                {
                    throw new ApplicationException("The component start position cannot be negative.");
                }
            }
            else
            {
                throw new ApplicationException("The component start is not an integer.");
            }

            var componentType = DetermineComponentType(glStructEntry.AcctComponentTypeAssocMember);

            try
            {
                glComponent = new GeneralLedgerComponent(componentName, isPartOfDescription, componentType, glStructEntry.AcctStartAssocMember, glStructEntry.AcctLengthAssocMember.ToString());
            }
            catch (ArgumentNullException anex)
            {
                logger.Info(anex.Message);
            }
            catch (ApplicationException apex)
            {
                logger.Info(apex.Message);
            }

            return glComponent;
        }

        /// <summary>
        /// Obtain the ACCT.STRUCTURE record from Colleague.
        ///  This record contains General Ledger setup parameters.
        /// </summary>
        /// <returns>The Glstruct data record.</returns>
        private async Task<Glstruct> GetGlstructAsync()
        {
            // Get General Ledger parameters from the ACCT.STRUCTURE record in ACCOUNT.PARAMETERS.
            var glStruct = new Glstruct();

            glStruct = await DataReader.ReadRecordAsync<Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE");
            if (glStruct == null)
                // GLSTRUCT must exist for Colleague Financials to function properly
                throw new ConfigurationException("GL account structure is not defined.");

            return glStruct;
        }

        private List<string> BuildSubcomponentList(Glstruct glStruct)
        {
            // AcctSubName is a list of strings but each string contains each major component's subcomponents separated by subvalue marks.
            // Example glStruct.AcctSubName[0] contains "FUND.GROUP":@SV:"FUND"
            var subcomponentList = new List<string>();
            if (glStruct.AcctSubName != null && glStruct.AcctSubName.Any())
            {
                foreach (var subName in glStruct.AcctSubName)
                {
                    string[] subvalues = subName.Split(DmiString._SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentList.Add(sub);
                    }
                }
            }
            return subcomponentList;
        }

        // AcctSubStart is a list of strings but each string contains each subcomponent's start position separated by subvalue marks.
        // Example glStruct.AcctSubStart[0] contains "1":@SV:"3"
        private List<string> BuildSubcomponentStartList(Glstruct glStruct)
        {
            var subcomponentStartList = new List<string>();
            if (glStruct.AcctSubStart != null && glStruct.AcctSubStart.Any())
            {
                foreach (var subStart in glStruct.AcctSubStart)
                {
                    string[] subvalues = subStart.Split(DmiString._SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentStartList.Add(sub);
                    }
                }
            }
            return subcomponentStartList;
        }

        // AcctSubLgth is a list of strings but each string contains each subcomponent' length separated by subvalue marks.
        // Example glStruct.AcctSubLgth[0] contains "1":@SV:"2"
        private List<string> BuildSubcomponentLengthtList(Glstruct glStruct)
        {
            var subcomponentLengthList = new List<string>();
            if (glStruct.AcctSubLgth != null && glStruct.AcctSubLgth.Any())
            {
                foreach (var subLgth in glStruct.AcctSubLgth)
                {
                    string[] subvalues = subLgth.Split(DmiString._SM);
                    foreach (var sub in subvalues)
                    {
                        subcomponentLengthList.Add(sub);
                    }
                }
            }
            return subcomponentLengthList;
        }

        #endregion
    }
}