/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LeaveBalanceConfigurationRepository : BaseColleagueRepository, ILeaveBalanceConfigurationRepository
    {
        public LeaveBalanceConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }
        /// <summary>
        /// Builds the leave management configuration from HRSS.DEFAULTS File
        /// </summary>
        /// <returns>LeaveBalanceConfiguration object</returns>
        public async Task<LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync()
        {
            var leaveManagementConfiguration = new LeaveBalanceConfiguration();
            var hrssDefaults = await DataReader.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS");
            if (hrssDefaults != null)
            {
                leaveManagementConfiguration.ExcludedLeavePlanIds = hrssDefaults.HrssExcludeLeavePlanIds;
            }
            else
            {
                logger.Info("Null HrssDefaults record returned from database");
            }

            return leaveManagementConfiguration;
        }
    }
}
