//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for LeaveBalanceConfiguration services
    /// </summary>
    public interface ILeaveBalanceConfigurationService
    {
        /// <summary>
        /// Gets the configurations for leave balance
        /// </summary>
        /// <returns>LeaveBalanceConfiguration</returns>
        Task<Dtos.HumanResources.LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync();
    }
}
