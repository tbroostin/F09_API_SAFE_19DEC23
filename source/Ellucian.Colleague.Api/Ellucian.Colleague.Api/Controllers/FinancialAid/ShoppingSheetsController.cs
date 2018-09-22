/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes access to Student-specific Financial Aid Shopping Sheet data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ShoppingSheetsController : BaseCompressedApiController
    {
        private readonly IShoppingSheetService shoppingSheetService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Dependency Injection constructor for ShoppingSheetsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="shoppingSheetService">ShoppingSheetService</param>
        /// <param name="logger">Logger</param>
        public ShoppingSheetsController(IAdapterRegistry adapterRegistry, IShoppingSheetService shoppingSheetService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.shoppingSheetService = shoppingSheetService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all shopping sheet resources for the given student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get shopping sheets</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of all shopping sheets for the given student</returns>
        [HttpGet]
        public async Task<IEnumerable<ShoppingSheet>> GetShoppingSheetsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }

            try
            {
                return await shoppingSheetService.GetShoppingSheetsAsync(studentId, getActiveYearsOnly);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("User does not have access rights to student {0}", studentId);
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred getting shopping sheet resources");
                throw CreateHttpResponseException("Unknown error occurred getting shopping sheet resources");
            }
        }


    }
}