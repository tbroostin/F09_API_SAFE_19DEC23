// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for course placeholder data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CoursePlaceholdersController : BaseCompressedApiController
    {
        private readonly ICoursePlaceholderService _coursePlaceholderService;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="CoursePlaceholdersController"/> class.
        /// </summary>
        /// <param name="coursePlaceholderService">Interface to course placeholder coordination service</param>
        /// <param name="logger">Interface to logger</param>
        public CoursePlaceholdersController(ICoursePlaceholderService coursePlaceholderService, ILogger logger)
        {
            _coursePlaceholderService = coursePlaceholderService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve a collection of course placeholders by ID
        /// </summary>
        /// <param name="coursePlaceholderIds">Unique identifiers for course placeholders to retrieve</param>
        /// <returns>Collection of <see cref="CoursePlaceholder"/></returns>
        /// <accessComments>Any authenticated user can retrieve course placeholder information.</accessComments>
        public async Task<IEnumerable<CoursePlaceholder>> QueryCoursePlaceholdersByIdsAsync([FromBody] IEnumerable<string> coursePlaceholderIds)
        {
            if (coursePlaceholderIds == null || !coursePlaceholderIds.Any())
            {
                throw CreateHttpResponseException("At least one course placeholder ID is required when retrieving course placeholders by ID.");
            }
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                var coursePlaceholderDtos = await _coursePlaceholderService.GetCoursePlaceholdersByIdsAsync(coursePlaceholderIds, bypassCache);
                return coursePlaceholderDtos;
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Information for one or more course placeholders could not be retrieved.";
                _logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var message = string.Format("An error occurred while trying to retrieve course placeholder data for IDs {0}.", string.Join(",", coursePlaceholderIds));
                _logger.Error(ex, message);
                throw CreateHttpResponseException(message);
            }
        }
    }
}