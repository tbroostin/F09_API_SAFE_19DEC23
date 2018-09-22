// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.using System;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provide access to faculty Consent and student petition data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionPermissionsController : BaseCompressedApiController
    {
         private readonly ILogger _logger;
         private readonly ISectionPermissionService _service;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="logger"></param>
         public SectionPermissionsController(ISectionPermissionService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }
        /// <summary>
        /// Returns the student petitions and faculty consents found for the specified section. Requestor must be an instructor of the section to access the information.
        /// </summary>
        /// <param name="sectionId">The section Id to use to retrieve student petitions and faculty consents.</param>
        /// <returns>The <see cref="Dtos.Student.SectionPermission">SectionPermission</see> object</returns>
        /// <accessComments>Only the faculty of a section can retrieve the permissions for the section.</accessComments>
        public async Task<SectionPermission> GetSectionPermissionAsync(string sectionId)
         {
             try 
             {
                 var sectionPermission= await _service.GetAsync(sectionId);
                 return sectionPermission;
             }
             catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Section Permission is forbidden.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Invalid section specified.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred retrieving permissions for section." , System.Net.HttpStatusCode.BadRequest);
            }
         }
    }

}
