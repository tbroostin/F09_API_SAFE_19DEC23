// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Service for accessing banking information configuration
    /// </summary>
    public interface IBankingInformationConfigurationService
    {
        /// <summary>
        /// Get the Configuration object for Colleague Self Service Banking Information
        /// </summary>
        /// <returns>Returns a single banking information configuration object</returns>
        Task<Dtos.Base.BankingInformationConfiguration> GetBankingInformationConfigurationAsync();
    }
}
