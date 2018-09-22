/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IBankingInformationConfigurationRepository
    {
        /// <summary>
        /// Get an institutions configuration information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BankingInformationConfiguration> GetBankingInformationConfigurationAsync();

    }
}
