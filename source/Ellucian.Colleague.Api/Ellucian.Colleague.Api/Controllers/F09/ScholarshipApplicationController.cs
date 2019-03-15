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
    public class ScholarshipApplicationController : BaseCompressedApiController
    {
        private readonly IScholarshipApplicationService _UpdateScholarshipApplicationService;
        private readonly ILogger _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateScholarshipApplicationService"></param>
        /// <param name="logger"></param>
        public ScholarshipApplicationController(IScholarshipApplicationService updateScholarshipApplicationService, ILogger logger)
        {
            if (updateScholarshipApplicationService == null) throw new ArgumentNullException(nameof(ScholarshipApplicationService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this._UpdateScholarshipApplicationService = updateScholarshipApplicationService;
            this._logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ScholarshipApplicationResponseDto> GetScholarshipApplicationAsync(string personId)
        {
            ScholarshipApplicationResponseDto profile;
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.Error("F09-ScholarshipApplicationController-GetScholarshipApplicationAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }

                profile = await _UpdateScholarshipApplicationService.GetScholarshipApplicationAsync(personId);

                return profile;

            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException("Unable to get Scholarship Application information: " + ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates certain person profile information: AddressConfirmationDateTime, EmailAddressConfirmationDateTime,
        /// PhoneConfirmationDateTime, EmailAddresses, Personal Phones and Addresses. LastChangedDateTime must match the last changed timestamp on the database
        /// Person record to ensure updates not occurring from two different sources at the same time. If no changes are found, a NotModified Http status code
        /// is returned. If required by configuration, users must be set up with permissions to perform these updates: UPDATE.OWN.EMAIL, UPDATE.OWN.PHONE, and 
        /// UPDATE.OWN.ADDRESS. 
        /// </summary>
        /// <param name="request"><see cref="Dtos.Base.Profile">Profile</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Profile</see></returns>
        [HttpPut]
        public async Task<ScholarshipApplicationResponseDto> PutScholarshipApplicationAsync([FromBody] ScholarshipApplicationRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    _logger.Error("F09-ScholarshipApplicationController-PutScholarshipApplicationAsync: Must provide a profile in the request body");
                    throw new Exception();
                }
                if (string.IsNullOrEmpty(request.Id))
                {
                    _logger.Error("F09-ScholarshipApplicationController-PutScholarshipApplicationAsync: Must provide a person Id in the request body");
                    throw new Exception();
                }

                var profile = await _UpdateScholarshipApplicationService.UpdateScholarshipApplicationAsync(request);

                return profile;
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
