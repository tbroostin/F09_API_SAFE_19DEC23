// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base;
using System.Web;

namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// AdvisementsCompleteController
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class AdvisementsCompleteController : BaseCompressedApiController
    {
        private readonly IAdvisorService _advisorService;
        private readonly ILogger _logger;

        /// <summary>
        /// AdvisementsCompleteController constructor
        /// </summary>
        /// <param name="advisorService">Service of type <see cref="IAdvisorService">IAdvisorService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AdvisementsCompleteController(IAdvisorService advisorService, ILogger logger)
        {
            _advisorService = advisorService;
            _logger = logger;
        }

        /// <summary>
        /// Posts a <see cref="Dtos.Planning.CompletedAdvisement">completed advisement</see>
        /// </summary>
        /// <param name="studentId">ID of the student whose advisement is being marked complete</param>
        /// <param name="completeAdvisement">A <see cref="Dtos.Planning.CompletedAdvisement">completed advisement</see></param>
        /// <returns>An <see cref="Dtos.Planning.Advisee">advisee</see></returns>
        [HttpPost]
        public async Task<Advisee> PostCompletedAdvisementAsync(string studentId, [FromBody]CompletedAdvisement completeAdvisement)
        {
            try
            {
                var privacyWrapper = await _advisorService.PostCompletedAdvisementAsync(studentId, completeAdvisement);
                var advisee = privacyWrapper.Dto as Advisee;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return advisee;
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
