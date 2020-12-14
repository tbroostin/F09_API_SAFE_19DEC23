// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.BudgetManagement.Repositories
{
    [RegisterType]
    public class BudgetConfigurationRepository : BaseColleagueRepository, IBudgetDevelopmentConfigurationRepository
    {
        public BudgetConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get the working budget configuration information.
        /// </summary>
        /// <returns>BudgeConfiguration entity</returns>
        public async Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationAsync()
        {
            var buDevDataContract = await DataReader.ReadRecordAsync<BudgetDevDefaults>("CF.PARMS", "BUDGET.DEV.DEFAULTS");
            if (buDevDataContract == null)
            {
                throw new ConfigurationException("BUDGET.DEV.DEFAULTS record does not exist.");
            }

            // Validate that there is a working budget for SS.
            if (buDevDataContract.BudDevBudget == null)
            {
                throw new ApplicationException("The available working budget for Self-Service does not exist in Colleague.");
            }

            var budgetDataContract = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", buDevDataContract.BudDevBudget);
            if (budgetDataContract == null)
            {
                throw new KeyNotFoundException(string.Format("No BUDGET record was found for ID {0}.", buDevDataContract.BudDevBudget));
            }

            var id = buDevDataContract.BudDevBudget;

            // Initialize the budget configuration domain entity.
            var buConfigurationEntity = new BudgetConfiguration(id);
            var budComparablesEntity = new List<BudgetConfigurationComparable>();

            // Assign the title if there is one.
            buConfigurationEntity.BudgetTitle = string.Empty;
            if (!string.IsNullOrEmpty(budgetDataContract.BuTitle))
            {
                buConfigurationEntity.BudgetTitle = budgetDataContract.BuTitle;
            }

            // Assign the year if there is one.
            buConfigurationEntity.BudgetYear = string.Empty;
            if (!string.IsNullOrEmpty(budgetDataContract.BuBaseYear))
            {
                buConfigurationEntity.BudgetYear = budgetDataContract.BuBaseYear;
            }

            BudgetStatus buStatus = new BudgetStatus();

            if (!string.IsNullOrEmpty(budgetDataContract.BuStatus))
            {
                switch (budgetDataContract.BuStatus.ToUpperInvariant())
                {
                    case "N":
                        buStatus = BudgetStatus.New;
                        break;
                    case "G":
                        buStatus = BudgetStatus.Generated;
                        break;
                    case "P":
                        buStatus = BudgetStatus.Posted;
                        break;
                    case "W":
                        buStatus = BudgetStatus.Working;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid status for budget: " + budgetDataContract.Recordkey);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for budget: " + budgetDataContract.Recordkey);
            }

            buConfigurationEntity.BudgetStatus = buStatus;

            // Assign the information for the comparables column headers.
            // Right now there are only five comparables.

            int seq = 0;
            for (int i = 0; i < 5; i++)
            {
                var comparable = new BudgetConfigurationComparable();
                comparable.ComparableHeader = string.Empty;
                comparable.ComparableId = string.Empty;
                comparable.ComparableYear = string.Empty;
                comparable.SequenceNumber = 0;

                int? year = null;
                string header = string.Empty;
                switch (i)
                {
                    case 0:
                        year = budgetDataContract.BuComp1Year;
                        header = budgetDataContract.BuComp1Heading;
                        break;
                    case 1:
                        year = budgetDataContract.BuComp2Year;
                        header = budgetDataContract.BuComp2Heading;
                        break;
                    case 2:
                        year = budgetDataContract.BuComp3Year;
                        header = budgetDataContract.BuComp3Heading;
                        break;
                    case 3:
                        year = budgetDataContract.BuComp4Year;
                        header = budgetDataContract.BuComp4Heading;
                        break;
                    case 4:
                        year = budgetDataContract.BuComp5Year;
                        header = budgetDataContract.BuComp5Heading;
                        break;
                }

                // Only add a comparable if there is a header for it.
                if (!string.IsNullOrWhiteSpace(header))
                {
                    comparable.ComparableHeader = header;
                    if (year != null)
                    {
                        comparable.ComparableYear = year.ToString();
                    }
                    seq = seq + 1;
                    comparable.ComparableId = "C" + seq;
                    comparable.SequenceNumber = seq;

                    // Add the comparable object to the list in the budget configuration entity.
                    buConfigurationEntity.AddBudgetConfigurationComparable(comparable);
                }
            }

            return buConfigurationEntity;
        }
    }
}



