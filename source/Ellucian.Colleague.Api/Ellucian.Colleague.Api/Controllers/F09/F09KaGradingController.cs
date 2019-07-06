using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Web;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Configuration;
using System.Text.RegularExpressions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Coordination.Finance;
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
    public class F09KaGradingController : BaseCompressedApiController
    {
        private readonly IF09KaGradingService _UpdateF09KaGradingService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateF09KaGradingService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public F09KaGradingController(IF09KaGradingService updateF09KaGradingService, ILogger logger, ApiSettings apiSettings)
        {
            if (updateF09KaGradingService == null) throw new ArgumentNullException(nameof(F09KaGradingService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._UpdateF09KaGradingService = updateF09KaGradingService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<dtoF09KaGradingResponse> GetF09KaGradingAsync(string stcId)
        {
            dtoF09KaGradingResponse dtoResponse;

            try
            {
                if (string.IsNullOrEmpty(stcId))
                {
                    _logger.Error("F09KaGradingController-GetF09KaGradingAsync: Must provide a stc id in the request uri");
                    throw new Exception();
                }

                dtoResponse = await _UpdateF09KaGradingService.GetF09KaGradingAsync(stcId);

                return dtoResponse;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09KaGrading: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="dtoRequest"><see cref="Dtos.Base.Profile">KaGrading</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">KaGrading</see></returns>
        [HttpPut]
        public async Task<dtoF09KaGradingResponse> PutF09KaGradingAsync([FromBody] dtoF09KaGradingRequest dtoRequest)
        {
            try
            {
                if (dtoRequest == null)
                {
                    _logger.Error("F09KaGradingController-PutF09KaGradingAsync: Must provide a dtoRequest in the request body");
                    throw new Exception();
                }
                if (string.IsNullOrEmpty(dtoRequest.StcId))
                {
                    _logger.Error("F09KaGradingController-PutF09KaGradingAsync: Must provide a stc Id in the request body");
                    throw new Exception();
                }

                var dtoResponse = await _UpdateF09KaGradingService.UpdateF09KaGradingAsync(dtoRequest);

                return dtoResponse;
            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09KaGrading: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }

}