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
    public class F09SsnController : BaseCompressedApiController
    {
        private readonly IF09SsnService _UpdateF09SsnService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateF09SsnService"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public F09SsnController(IF09SsnService updateF09SsnService, ILogger logger, ApiSettings apiSettings)
        {
            if (updateF09SsnService == null) throw new ArgumentNullException(nameof(F09SsnService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._UpdateF09SsnService = updateF09SsnService;
            this._logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<F09SsnResponseDto> GetF09SsnAsync(string personId)
        {
            F09SsnResponseDto dtoResponse;

            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.Error("F09SsnController-GetF09SsnAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                dtoResponse = await _UpdateF09SsnService.GetF09SsnAsync(personId);

                return dtoResponse;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to F09Ssn: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="dtoRequest"><see cref="Dtos.Base.Profile">Profile</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Profile</see></returns>
        [HttpPut]
        public async Task<F09SsnResponseDto> PutF09SsnAsync([FromBody] F09SsnRequestDto dtoRequest)
        {
            try
            {
                if (dtoRequest == null)
                {
                    _logger.Error("F09SsnController-PutF09SsnAsync: Must provide a dtoRequest in the request body");
                    throw new Exception();
                }
                if (string.IsNullOrEmpty(dtoRequest.Id))
                {
                    _logger.Error("F09SsnController-PutF09SsnAsync: Must provide a person Id in the request body");
                    throw new Exception();
                }

                var dtoResponse = await _UpdateF09SsnService.UpdateF09SsnAsync(dtoRequest);

                return dtoResponse;
            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to update Scholarship Application information: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }
}
