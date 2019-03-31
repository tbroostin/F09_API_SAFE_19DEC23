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
    public class F09StuTrackingSheetController : BaseCompressedApiController
    {
        private readonly IGetF09StuTrackingSheetService _GetF09StuTrackingSheetService;
        private readonly ILogger _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getF09StuTrackingSheetService"></param>
        /// <param name="logger"></param>
        public F09StuTrackingSheetController(IGetF09StuTrackingSheetService getF09StuTrackingSheetService, ILogger logger)
        {
            if (getF09StuTrackingSheetService == null) throw new ArgumentNullException(nameof(getF09StuTrackingSheetService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._GetF09StuTrackingSheetService = getF09StuTrackingSheetService;
            this._logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<GetF09StuTrackingSheetResponseDto> GetF09StuTrackingSheetAsync(string Id)
        {
            GetF09StuTrackingSheetResponseDto stuTrackingSheet;
            try
            {
                if (string.IsNullOrEmpty(Id))
                {
                    _logger.Error("F09StuTrackingSheetController-GetF09StuTrackingSheetAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                stuTrackingSheet = await _GetF09StuTrackingSheetService.GetF09StuTrackingSheetAsync(Id);

                return stuTrackingSheet;

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
