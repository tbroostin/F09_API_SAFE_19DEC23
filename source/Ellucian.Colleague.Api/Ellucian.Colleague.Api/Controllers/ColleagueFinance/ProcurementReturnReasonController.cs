using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{   /// <summary>
    /// Provides Procurement Return Reason information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ProcurementReturnReasonController: BaseCompressedApiController
    {
        private readonly IProcurementReturnReasonService procurementReturnReasonService;
        private readonly ILogger logger;
        /// <summary>
        /// Constructor to initialize ProcurementReturnReasonController object.
        /// </summary>
        public ProcurementReturnReasonController(IProcurementReturnReasonService procurementReturnReasonService, ILogger logger)
        {
            this.procurementReturnReasonService = procurementReturnReasonService;
            this.logger = logger;
        }

        /// <summary>
        /// Get all of the Return Reason codes.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ProcurementReturnReason>> GetProcurementReturnReasonsAsync()
        {
             try
            {
                var procurementReturnReasonCodes = await procurementReturnReasonService.GetProcurementReturnReasonsAsync();
                return procurementReturnReasonCodes;
            }
            // Application exceptions will be caught below.
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the return reason codes.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, knfex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException("Unable to get Return Reason Codes.", HttpStatusCode.BadRequest);
            }

        }
    }
}