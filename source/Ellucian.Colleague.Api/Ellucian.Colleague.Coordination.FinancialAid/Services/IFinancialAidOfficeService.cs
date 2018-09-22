//Copyright 2014-2016 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    public interface IFinancialAidOfficeService : IBaseService
    {
        /// <summary>
        /// Gets all fa offices asynchronously
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FinancialAidOffice3>> GetFinancialAidOffices3Async();

        #region Obsolete Methods

        /// <summary>
        /// Gets all fa offices
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this method")]
        IEnumerable<FinancialAidOffice2> GetFinancialAidOffices2();

        /// <summary>
        /// Gets all fa offices asynchronously
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this method")]
        Task<IEnumerable<FinancialAidOffice2>> GetFinancialAidOffices2Async();
        
        /// <summary>
        /// Gets all fa offices
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        IEnumerable<FinancialAidOffice> GetFinancialAidOffices();

        /// <summary>
        /// Gets all fa offices asynchronously
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice>> GetFinancialAidOfficesAsync();

        #endregion
    }
}
