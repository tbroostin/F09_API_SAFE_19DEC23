using System;
using System.ComponentModel;
using System.Web.Http;
using System.Net;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Web.Http.Configuration;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Coordination.F09.Services;

namespace Ellucian.Colleague.Api.Controllers.F09
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class F09ReportController : BaseCompressedApiController
    {
        private readonly IF09ReportService _GetF09ReportService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getF09ReportService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public F09ReportController(IF09ReportService getF09ReportService, ILogger logger, ApiSettings apiSettings)
        {
            if (getF09ReportService == null) throw new ArgumentNullException(nameof(F09ReportService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._GetF09ReportService = getF09ReportService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }


        /// <summary>
        /// </summary>
        /// <param name="dtoRequest"><see cref="Dtos.Base.Profile">Report</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Report</see></returns>
        [HttpPut]
        public async Task<dtoF09ReportResponse> GetF09ReportAsync([FromBody] dtoF09ReportRequest dtoRequest)
        {
            try
            {
                var dtoResponse = await _GetF09ReportService.GetF09ReportAsync(dtoRequest);
                return dtoResponse;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09Report: " + ex.Message, HttpStatusCode.BadRequest);
            }

        }
    }
}