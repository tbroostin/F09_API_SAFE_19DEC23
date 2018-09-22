/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// FinancialAidPersonRepository interface
    /// </summary>
    public interface IFinancialAidPersonRepository
    {
        /// <summary>
        /// Searches for persons by keyword (last, first last, first middle last name 
        /// combination or person id)
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        Task<IEnumerable<PersonBase>> SearchFinancialAidPersonsByKeywordAsync(string criteria);
        /// <summary>
        /// Searches for persons for the specified person ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<PersonBase>> SearchFinancialAidPersonsByIdsAsync(IEnumerable<string> ids);
    }
}
