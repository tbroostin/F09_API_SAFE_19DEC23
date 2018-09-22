//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PersonEmploymentReferences
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonEmploymentReferencesController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonEmploymentReferencesController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public PersonEmploymentReferencesController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all person-employment-references
        /// </summary>
        /// <returns>All <see cref="PersonEmploymentReference">PersonEmploymentReferences</see></returns>
        public async Task<IEnumerable<PersonEmploymentReference>> GetPersonEmploymentReferencesAsync()
        {
            return new List<PersonEmploymentReference>();
        }

        /// <summary>
        /// Retrieve (GET) an existing person-employment-references
        /// </summary>
        /// <param name="guid">GUID of the person-employment-references to get</param>
        /// <returns>A personEmploymentReference object <see cref="PersonEmploymentReference"/> in EEDM format</returns>
        [HttpGet]
        public async Task<PersonEmploymentReference> GetPersonEmploymentReferencesByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new Exception(string.Format("No person-employment-references was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new personEmploymentReference
        /// </summary>
        /// <param name="personEmploymentReference">DTO of the new personEmploymentReference</param>
        /// <returns>A personEmploymentReference object <see cref="PersonEmploymentReference"/> in EEDM format</returns>
        [HttpPost]
        public async Task<PersonEmploymentReference> PostPersonEmploymentReferenceAsync([FromBody] PersonEmploymentReference personEmploymentReference)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personEmploymentReference
        /// </summary>
        /// <param name="guid">GUID of the personEmploymentReference to update</param>
        /// <param name="personEmploymentReference">DTO of the updated personEmploymentReference</param>
        /// <returns>A personEmploymentReference object <see cref="PersonEmploymentReference"/> in EEDM format</returns>
        [HttpPut]
        public async Task<PersonEmploymentReference> PutPersonEmploymentReferenceAsync([FromUri] string guid, [FromBody] PersonEmploymentReference personEmploymentReference)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personEmploymentReference
        /// </summary>
        /// <param name="guid">GUID to desired personEmploymentReference</param>
        [HttpDelete]
        public async Task DeletePersonEmploymentReferenceAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}