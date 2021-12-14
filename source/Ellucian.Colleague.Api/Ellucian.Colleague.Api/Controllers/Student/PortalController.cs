﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.Portal;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Portal Controller is introduced to replace Portal Web Part of WebAdvisor. 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PortalController : BaseCompressedApiController
    {
        private readonly IPortalService _portalService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PortalWebController class.
        /// </summary>
        /// <param name="service">Service of type <see cref="IPortalService">IPortalService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PortalController(IPortalService service, ILogger logger)
        {
            _portalService = service;
            _logger = logger;
        }

        /// <summary>
        /// This returns the total courses and the list of course ids  that are applicable for deletion from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of courses that are applicable for deletion from Portal. </accessComments>
        public async Task<PortalDeletedCoursesResult> GetCoursesForDeletionAsync()
        {
            try
            {
                PortalDeletedCoursesResult portalDeletedCoursesDto = await _portalService.GetCoursesForDeletionAsync();
                return portalDeletedCoursesDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user  does not have appropriate permissions to access deleted courses for Portal";
                _logger.Error(pex, error);
                throw CreateHttpResponseException("The user does not have appropriate permissions to access deleted courses for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository level exception occured while retrieving deleted courses for Portal");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving deleted courses for Portal";
                _logger.Error(e, error);
                throw CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This returns the list of sections that are applicable for updating from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of sections that are applicable for updated from Portal.</accessComments>
        public async Task<PortalUpdatedSectionsResult> GetSectionsForUpdateAsync()
        {
            try
            {
                var portalUpdatedSectionsResultDto = await _portalService.GetSectionsForUpdateAsync();
                return portalUpdatedSectionsResultDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user does not have appropriate permissions to access updated sections for Portal";
                _logger.Error(pex, error);
                throw CreateHttpResponseException("The user does not have appropriate permissions to access updated sections for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository level exception occured while retrieving updated sections for Portal");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving updated sections for Portal";
                _logger.Error(e, error);
                throw CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This returns the total sections and the list of section ids that are applicable for deletion from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of sections that are applicable for deletion from Portal.</accessComments>
        public async Task<PortalDeletedSectionsResult> GetSectionsForDeletionAsync()
        {
            try
            {
                PortalDeletedSectionsResult portalDeletedSectionsResultDto = await _portalService.GetSectionsForDeletionAsync();
                return portalDeletedSectionsResultDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user  does not have appropriate permissions to access deleted sections for Portal";
                _logger.Error(pex, error);
                throw CreateHttpResponseException("The user does not have appropriate permissions to access deleted course sections for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository level exception occured while retrieving deleted sections for Portal");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving deleted sections for Portal";
                _logger.Error(e, error);
                throw CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns event and reminders to be displayed in the Portal for the authenticated user.
        /// </summary>
        /// <param name="criteria">Event and reminder selection criteria</param>
        /// <accessComments>Any authenticated user may retrieve events and reminders to be displayed in the Portal for themselves</accessComments>
        public async Task<Dtos.Student.Portal.PortalEventsAndReminders> QueryEventsAndRemindersAsync([FromBody]PortalEventsAndRemindersQueryCriteria criteria)
        {
            try
            {
                PortalEventsAndReminders portalEventsAndRemindersDto = await _portalService.GetEventsAndRemindersAsync(criteria);
                return portalEventsAndRemindersDto;
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository level exception occured while retrieving Portal events and reminders.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving Portal events and reminders.";
                _logger.Error(e, error);
                throw CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }

        }


        /// <summary>
        /// This returns the courses that are applicable for updating from Portal.
        /// </summary>
        /// <accessComments>Any authenticated user with PORTAL.CATALOG.ADMIN permissions is allowed to get the list of courses that are applicable for updated from Portal.</accessComments>
        public async Task<PortalUpdatedCoursesResult> GetCoursesForUpdateAsync()
        {
            try
            {
                var portalUpdatedCoursesResultDto = await _portalService.GetCoursesForUpdateAsync();
                return portalUpdatedCoursesResultDto;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user does not have appropriate permissions to access updated courses for Portal";
                _logger.Error(pex, error);
                throw CreateHttpResponseException("The user does not have appropriate permissions to access updated courses for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository level exception occured while retrieving updated courses for Portal");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving updated courses for Portal";
                _logger.Error(e, error);
                throw CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a student's list of preferred course sections
        /// </summary>
        /// <param name="studentId">ID of the student whose list of preferred course sections is being updated</param>
        /// <param name="courseSectionIds">IDs of the course sections to be added to the student's list of preferred course sections</param>
        /// <returns>Collection of <see cref="PortalStudentPreferredCourseSectionUpdateResult"/></returns>
        /// <accessComments>Authenticated users may update their own preferred course sections.</accessComments>
        public async Task<IEnumerable<PortalStudentPreferredCourseSectionUpdateResult>> UpdateStudentPreferredCourseSectionsAsync([FromUri] string studentId, [FromBody]IEnumerable<string> courseSectionIds)
        {
            try
            {
                var result = await _portalService.UpdateStudentPreferredCourseSectionsAsync(studentId, courseSectionIds);
                return result;
            }
            catch (PermissionsException pex)
            {
                string error = "The current user does not have appropriate permissions to access updated courses for Portal";
                _logger.Error(pex, error);
                throw CreateHttpResponseException("The user does not have appropriate permissions to access updated courses for Portal", HttpStatusCode.Forbidden);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository level exception occured while retrieving updated courses for Portal");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                string error = "An exception occured while retrieving updated courses for Portal";
                _logger.Error(e, error);
                throw CreateHttpResponseException(error, HttpStatusCode.BadRequest);
            }
        }
    }
}