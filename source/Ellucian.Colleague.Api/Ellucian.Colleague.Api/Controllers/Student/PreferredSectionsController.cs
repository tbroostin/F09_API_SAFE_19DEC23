// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Security;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Preferred Section data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PreferredSectionsController : BaseCompressedApiController
    {
        private readonly IPreferredSectionService _preferredSectionService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentPreferredSectionsRepository class.
        /// </summary>
        /// <param name="preferredSectionService"></param>
        /// <param name="logger"></param>
        public PreferredSectionsController(IPreferredSectionService preferredSectionService, ILogger logger)
        {
            _preferredSectionService = preferredSectionService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the student's list of preferred sections.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <returns>List of student's current Preferred Sections and applicable messages.</returns>
        public async Task<Dtos.Student.PreferredSectionsResponse> GetPreferredSectionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Invalid studentId");
                throw CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _preferredSectionService.GetAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                _logger.Info(pex.ToString());
                throw CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update (create new and/or update existing) the student's list of preferred sections.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="preferredSections">List of preferred sections to create and/or update.</param>
        /// <returns>List of student's current Preferred Sections and applicable messages.</returns>
        [HttpPost]
        public async Task<IEnumerable<Dtos.Student.PreferredSectionMessage>> UpdatePreferredSectionsAsync(string studentId, [FromBody] IEnumerable<Dtos.Student.PreferredSection> preferredSections)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Invalid studentId");
                throw CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (preferredSections == null || preferredSections.Count() <= 0)
            {
                _logger.Error("Invalid preferredSections");
                throw CreateHttpResponseException("Invalid preferredSections. Must provide at least one.", System.Net.HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<Dtos.Student.PreferredSectionMessage> response = await _preferredSectionService.UpdateAsync(studentId, preferredSections);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.Info(pex.ToString());
                throw CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Delete the indicated section from the student's preferred sections list.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="sectionId">The Section Id to delete.</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IEnumerable<Dtos.Student.PreferredSectionMessage>> DeletePreferredSectionsAsync(string studentId, string sectionId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Invalid studentId");
                throw CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                _logger.Error("Invalid sectionId");
                throw CreateHttpResponseException("Invalid sectionId", HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<Dtos.Student.PreferredSectionMessage> response = await _preferredSectionService.DeleteAsync(studentId, sectionId);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.Info(pex.ToString());
                throw CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }


/*        /// <summary>
        /// Deletes the indicated sections from the student's preferred sections list.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="sectionIds">List of Preferred Section IDs to delete.</param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<Dtos.Student.PreferredSectionMessage> DeletePreferredSections(string studentId, [FromBody]List<string> sectionIds)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.Error("Invalid studentId");
                throw CreateHttpResponseException("Invalid studentId", HttpStatusCode.BadRequest);
            }
            if (sectionIds == null || sectionIds.Count() == 0)
            {
                _logger.Error("Invalid sectionIds");
                throw CreateHttpResponseException("Invalid sectionIds. Must provide at least one.", System.Net.HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<Dtos.Student.PreferredSectionMessage> response = _preferredSectionService.Delete(studentId, sectionIds);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.Info(pex.ToString());
                throw CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }
*/
    }
}