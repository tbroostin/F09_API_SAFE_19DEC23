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
    public class F09EvalSelectController : BaseCompressedApiController
    {
        private readonly IF09EvalSelectService _UpdateF09EvalSelectService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateF09EvalSelectService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public F09EvalSelectController(IF09EvalSelectService updateF09EvalSelectService, ILogger logger, ApiSettings apiSettings)
        {
            if (updateF09EvalSelectService == null) throw new ArgumentNullException(nameof(F09EvalSelectService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._UpdateF09EvalSelectService = updateF09EvalSelectService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<dtoF09EvalSelectResponse> GetF09EvalSelectAsync(string personId)
        {
            dtoF09EvalSelectResponse dtoResponse;

            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.Error("F09EvalSelectController-GetF09EvalSelectAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                dtoResponse = await _UpdateF09EvalSelectService.GetF09EvalSelectAsync(personId);

                return dtoResponse;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09EvalSelect: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }


    }

}