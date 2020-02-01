// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.BudgetManagement.Tests
{
    public class TestBudgetDevelopmentConfigurationRepository : IBudgetDevelopmentConfigurationRepository
    {
        BudgetManagement.Entities.BudgetConfiguration budgetConfiguration = new BudgetManagement.Entities.BudgetConfiguration("FY2021")
        {
            BudgetTitle = "Working Budget",
            BudgetYear = "FY2021",
            BudgetStatus = BudgetManagement.Entities.BudgetStatus.Working
        };

        BudgetManagement.Entities.BudgetConfigurationComparable budgetConfigurationComparable1 = new BudgetManagement.Entities.BudgetConfigurationComparable()
        {
            SequenceNumber = 1,
            ComparableId = "C1",
            ComparableYear = "2015",
            ComparableHeader = "Actuals"
        };

        BudgetManagement.Entities.BudgetConfigurationComparable budgetConfigurationComparable2 = new BudgetManagement.Entities.BudgetConfigurationComparable()
        {
            SequenceNumber = 2,
            ComparableId = "C2",
            ComparableYear = "2016",
            ComparableHeader = "Original Budget"
        };

        BudgetManagement.Entities.BudgetConfigurationComparable budgetConfigurationComparable3 = new BudgetManagement.Entities.BudgetConfigurationComparable()
        {
            SequenceNumber = 3,
            ComparableId = "C3",
            ComparableYear = "2017",
            ComparableHeader = "Adjusted Budget"
        };

        BudgetManagement.Entities.BudgetConfigurationComparable budgetConfigurationComparable4 = new BudgetManagement.Entities.BudgetConfigurationComparable()
        {
            SequenceNumber = 4,
            ComparableId = "C4",
            ComparableYear = "2018",
            ComparableHeader = "Encumbered Actuals"
        };

        BudgetManagement.Entities.BudgetConfigurationComparable budgetConfigurationComparable5 = new BudgetManagement.Entities.BudgetConfigurationComparable()
        {
            SequenceNumber = 5,
            ComparableId = "C5",
            ComparableYear = "2019",
            ComparableHeader = "Allocated Budget"
        };

        // Create the parameter and budget records.
        public BudgetDevDefaults BudgetDevDefaultsContract = new BudgetDevDefaults()
        {
            Recordkey = "BUDGET.DEV.DEFAULTS",
            BudDevBudget = "FY2021"
        };

        public Budget BudgetContract = new Budget()
        {
            Recordkey = "FY2021",
            BuTitle = "Working Budget",
            BuBaseYear = "2021",
            BuStatus = "W",
            BuComp1Year = 2015,
            BuComp1Heading = "2015 Actual",
            BuComp2Year = 2016,
            BuComp2Heading = "2016 Org Bu",
            BuComp3Year = 2017,
            BuComp3Heading = "2017 Adj Bu",
            BuComp4Year = 2018,
            BuComp4Heading = "2018 Enc Ac",
            BuComp5Year = 2019,
            BuComp5Heading = "2019 All Bu"

        };

        public async Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationAsync()
        {
            return await Task.Run(() => {
                budgetConfiguration.RemoveBudgetConfigurationComparables();
                budgetConfiguration.AddBudgetConfigurationComparable(budgetConfigurationComparable1);
                budgetConfiguration.AddBudgetConfigurationComparable(budgetConfigurationComparable2);
                budgetConfiguration.AddBudgetConfigurationComparable(budgetConfigurationComparable3);
                budgetConfiguration.AddBudgetConfigurationComparable(budgetConfigurationComparable4);
                budgetConfiguration.AddBudgetConfigurationComparable(budgetConfigurationComparable5);

                return budgetConfiguration;
            });
        }

        public async Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationNoComparablesAsync()
        {
            return await Task.Run(() => {
                return budgetConfiguration;
            });
        }
    }
}