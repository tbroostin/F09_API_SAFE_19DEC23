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
    public class F09EvalFormController : BaseCompressedApiController
    {
        private readonly IF09EvalFormService _UpdateF09EvalFormService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateF09EvalFormService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public F09EvalFormController(IF09EvalFormService updateF09EvalFormService, ILogger logger, ApiSettings apiSettings)
        {
            if (updateF09EvalFormService == null) throw new ArgumentNullException(nameof(F09EvalFormService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._UpdateF09EvalFormService = updateF09EvalFormService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<dtoF09EvalFormResponse> GetF09EvalFormAsync(string key)
        {
            dtoF09EvalFormResponse dtoResponse;

            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    _logger.Error("F09EvalFormController-GetF09EvalFormAsync: Must provide a stc id in the request uri");
                    throw new Exception();
                }

                dtoResponse = await _UpdateF09EvalFormService.GetF09EvalFormAsync(key);
                return dtoResponse;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09EvalForm: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="dtoRequest"><see cref="Dtos.Base.Profile">KaGrading</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">KaGrading</see></returns>
        [HttpPut]
        public async Task<dtoF09EvalFormResponse> PutF09EvalFormAsync([FromBody] dtoF09EvalFormRequest dtoRequest)
        {
            try
            {
                if (dtoRequest == null)
                {
                    _logger.Error("F09EvalFormController-PutF09EvalFormAsync: Must provide a dtoRequest in the request body");
                    throw new Exception();
                }
                if (string.IsNullOrEmpty(dtoRequest.EvalKey))
                {
                    _logger.Error("F09EvalFormController-PutF09EvalFormAsync: Must provide a eval key in the request body");
                    throw new Exception();
                }

                var dtoResponse = await _UpdateF09EvalFormService.UpdateF09EvalFormAsync(dtoRequest);
                return dtoResponse;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09EvalForm: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }

}