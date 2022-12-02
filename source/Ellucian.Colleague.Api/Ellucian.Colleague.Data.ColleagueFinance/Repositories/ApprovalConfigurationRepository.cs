// Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    [RegisterType]
    public class ApprovalConfigurationRepository : BaseColleagueRepository, IApprovalConfigurationRepository
    {
        public ApprovalConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Returns the approval configuration settings.
        /// </summary>
        public async Task<ApprovalConfiguration> GetApprovalConfigurationAsync()
        {
            var approvalConfiguration = await GetOrAddToCacheAsync<ApprovalConfiguration>("ApprovalConfiguration",
               async () => await BuildApprovalConfiguration());

            return approvalConfiguration;
        }

        private async Task<ApprovalConfiguration> BuildApprovalConfiguration()
        {
            ApprovalConfiguration approvalConfigurationSettings = new ApprovalConfiguration();
            var purchasingDefaultsDataContract = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");

            if ((purchasingDefaultsDataContract == null) || (purchasingDefaultsDataContract.PurApprGlrolesDocTypes == null))
            {
                throw new ConfigurationException("Approval settings are not set up.");
            }

            if (purchasingDefaultsDataContract.PurApprGlrolesDocTypes.Any())
            {
                foreach (var docType in purchasingDefaultsDataContract.PurApprGlrolesDocTypes)
                {
                    switch (docType.ToUpperInvariant())
                    {
                        case "A":
                            approvalConfigurationSettings.ArVouchersUseApprovalRoles = true;
                            break;
                        case "B":
                            approvalConfigurationSettings.BudgetEntriesUseApprovalRoles = true;
                            break;
                        case "C":
                            approvalConfigurationSettings.RecurringVouchersUseApprovalRoles = true;
                            break;
                        case "J":
                            approvalConfigurationSettings.JournalEntriesUseApprovalRoles = true;
                            break;
                        case "O":
                            approvalConfigurationSettings.BlanketPurchaseOrdersUseApprovalRoles = true;
                            break;
                        case "P":
                            approvalConfigurationSettings.PurchaseOrdersUseApprovalRoles = true;
                            break;
                        case "R":
                            approvalConfigurationSettings.RequisitionsUseApprovalRoles = true;
                            break;
                        case "V":
                            approvalConfigurationSettings.VouchersUseApprovalRoles = true;
                            break;
                        default:
                            var message = string.Format("Invalid Tax Year {0}", purchasingDefaultsDataContract.PurApprGlrolesDocTypes);
                            logger.Error(message);
                            throw new ApplicationException(message);
                    }
                }
            }

            return approvalConfigurationSettings;
        }
    }
}