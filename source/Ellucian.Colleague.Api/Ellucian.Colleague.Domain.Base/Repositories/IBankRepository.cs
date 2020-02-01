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
    public interface IBankRepository
    {
        /// <summary>
        /// Get a bank's routing information from a US routing number or CA institution number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Bank> GetBankAsync(string id);
                
        Task<Dictionary<string, Bank>> GetAllBanksAsync();
    }
}
