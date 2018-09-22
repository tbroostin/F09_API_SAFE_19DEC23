/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes FinancialAidOffice and FinancialAidConfiguration Data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidOfficesController : BaseCompressedApiController
    {
        private readonly IFinancialAidOfficeService financialAidOfficeService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for FinancialAidOfficesController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidOfficeService">FinancialAidOfficeService</param>
        /// <param name="logger">Logger</param>
        public FinancialAidOfficesController(IAdapterRegistry adapterRegistry, IFinancialAidOfficeService financialAidOfficeService, ILogger logger)
        {
            this.financialAidOfficeService = financialAidOfficeService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice3 objects</returns>
        public async Task<IEnumerable<FinancialAidOffice3>> GetFinancialAidOffices3Async()
        {
            try
            {
                return await financialAidOfficeService.GetFinancialAidOffices3Async();
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }


        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice2 objects</returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this API")]
        public IEnumerable<FinancialAidOffice2> GetFinancialAidOffices2()
        {
            try
            {
                return financialAidOfficeService.GetFinancialAidOffices2();
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice objects</returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this API")]
        public async Task<IEnumerable<FinancialAidOffice2>> GetFinancialAidOffices2Async()
        {
            try
            {
                return await financialAidOfficeService.GetFinancialAidOffices2Async();
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources.
        /// </accessComments>
        /// <returns>A list of FinancialAidOffice objects</returns>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        public IEnumerable<FinancialAidOffice> GetFinancialAidOffices()
        {
            try
            {
                return financialAidOfficeService.GetFinancialAidOffices();
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }

        /// <summary>
        /// Get a list of Financial Aid Offices and their year-based configurations
        /// </summary>
        /// <returns>A list of FinancialAidOffice objects</returns>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        public async Task<IEnumerable<FinancialAidOffice>> GetFinancialAidOfficesAsync()
        {
            try
            {
                return await financialAidOfficeService.GetFinancialAidOfficesAsync();
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("FinancialAidOffices", string.Empty);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting FinancialAidOffices resource. See log for details." + Environment.NewLine + e.Message);
            }
        }
    }
}