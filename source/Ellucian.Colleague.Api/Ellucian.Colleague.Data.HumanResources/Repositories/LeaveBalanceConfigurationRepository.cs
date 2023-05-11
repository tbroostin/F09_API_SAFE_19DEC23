/*Copyright 2018-2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LeaveBalanceConfigurationRepository : BaseColleagueRepository, ILeaveBalanceConfigurationRepository
    {
        public LeaveBalanceConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }       
      
        /// <summary>
        /// Gets the leave management configuration from HRSS.DEFAULTS File
        /// </summary>
        /// <returns>Cached LeaveBalanceConfiguration object</returns>
        public async Task<LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync()
        {
            return await GetOrAddToCacheAsync("LeaveBalanceConfiguration", async () => await BuildLeaveBalanceConfigurationAsync(), Level1CacheTimeoutValue);
        }        

        /// <summary>
        /// Builds the leave management configuration from HRSS.DEFAULTS File
        /// </summary>
        /// <returns>LeaveBalanceConfiguration object</returns>
        private async Task<LeaveBalanceConfiguration> BuildLeaveBalanceConfigurationAsync()
        {
            LeaveRequestActionType leaveRequestActionType;

            var leaveManagementConfiguration = new LeaveBalanceConfiguration();
            var hrssDefaults = await DataReader.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS");
            if (hrssDefaults != null)
            {
                leaveManagementConfiguration.ExcludedLeavePlanIds = hrssDefaults.HrssExcludeLeavePlanIds;
                //Set the threshold 
                leaveManagementConfiguration.LeaveRequestLookbackDays = hrssDefaults.HrssLeaveLkbk;

                if (!Enum.TryParse(hrssDefaults.HrssLrUnsubmitWdrw, true, out leaveRequestActionType))
                {
                    // In case the leaveRequestActionType returns empty string after parsing, then set the leaveRequestActionType as Reject Type (the default value in LVSS form for Unsubmit/Withdraw field).
                    leaveRequestActionType = LeaveRequestActionType.R;
                }
                leaveManagementConfiguration.LeaveRequestActionType = leaveRequestActionType;
                leaveManagementConfiguration.AllowSupervisorToEditLeaveRequests = hrssDefaults.HrssLrAllowSuprvsrEdit == "Y" ? true : false;
            }
            else
            {
                logger.Info("Null HrssDefaults record returned from database");
            }

            return leaveManagementConfiguration;
        }
    }
}
