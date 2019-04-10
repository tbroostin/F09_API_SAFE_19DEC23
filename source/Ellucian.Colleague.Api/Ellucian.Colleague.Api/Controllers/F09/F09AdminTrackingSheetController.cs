using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Resources;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Data.F09;
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
    public class F09AdminTrackingSheetController : BaseCompressedApiController
    {
        private readonly IF09AdminTrackingSheetService _F09AdminTrackingSheetService;
        private readonly ILogger _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="F09AdminTrackingSheetService"></param>
        /// <param name="logger"></param>
        public F09AdminTrackingSheetController(IF09AdminTrackingSheetService F09AdminTrackingSheetService, ILogger logger)
        {
            if (F09AdminTrackingSheetService == null) throw new ArgumentNullException(nameof(F09AdminTrackingSheetService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._F09AdminTrackingSheetService = F09AdminTrackingSheetService;
            this._logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<F09AdminTrackingSheetResponseDto> GetF09AdminTrackingSheetAsync(string Id)
        {
            F09AdminTrackingSheetResponseDto AdminTrackingSheet;
            try
            {
                if (string.IsNullOrEmpty(Id))
                {
                    _logger.Error("F09AdminTrackingSheetController-GetF09AdminTrackingSheetAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                AdminTrackingSheet = await _F09AdminTrackingSheetService.GetF09AdminTrackingSheetAsync(Id);

                return AdminTrackingSheet;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to get Tracking Sheet information: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }
}
