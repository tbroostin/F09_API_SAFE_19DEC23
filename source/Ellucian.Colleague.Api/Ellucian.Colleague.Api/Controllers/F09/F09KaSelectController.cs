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
    public class F09KaSelectController : BaseCompressedApiController
    {
        private readonly IF09KaSelectService _UpdateF09KaSelectService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateF09KaSelectService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public F09KaSelectController(IF09KaSelectService updateF09KaSelectService, ILogger logger, ApiSettings apiSettings)
        {
            if (updateF09KaSelectService == null) throw new ArgumentNullException(nameof(F09KaSelectService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._UpdateF09KaSelectService = updateF09KaSelectService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<F09KaSelectResponseDto> GetF09KaSelectAsync(string personId)
        {
            F09KaSelectResponseDto dtoResponse;

            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.Error("F09KaSelectController-GetF09KaSelectAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                dtoResponse = await _UpdateF09KaSelectService.GetF09KaSelectAsync(personId);

                return dtoResponse;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09KaSelect: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }


    }

}