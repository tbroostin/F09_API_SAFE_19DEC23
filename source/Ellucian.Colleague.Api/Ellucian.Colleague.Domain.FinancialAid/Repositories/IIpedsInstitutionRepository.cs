// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface to an IpedsInstitutionRepository
    /// </summary>
    public interface IIpedsInstitutionRepository
    {
        /// <summary>
        /// Get IpedsInstitution objects based on a list of OPE (Office of Post Secondary Education) ids
        /// </summary>
        /// <param name="opeIdList">List of OPE ids of IpedsInstitution objects to be returned</param>
        /// <returns>A list of IpedsInstitution objects limited to the ones with an OPE Id listed in the opeIdList argument. If an
        /// OPE id from the input list is invalid, no IpedsInstitution object is returned for that OPE Id. </returns>
        Task<IEnumerable<IpedsInstitution>> GetIpedsInstitutionsAsync(IEnumerable<string> opeIdList);

    }
}
