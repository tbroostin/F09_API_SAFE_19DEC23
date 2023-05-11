//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos.Student;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to InstructionalDeliveryMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructionalDeliveryMethodsController : BaseCompressedApiController
    {

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructionalDeliveryMethodsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public InstructionalDeliveryMethodsController(ILogger logger)
        {
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all instructional-delivery-methods
        /// </summary>
        /// <returns>All <see cref="InstructionalDeliveryMethod">InstructionalDeliveryMethods</see></returns>
        public async Task<IEnumerable<InstructionalDeliveryMethod>> GetInstructionalDeliveryMethodsAsync()
        {
            return new List<InstructionalDeliveryMethod>();
        }

        /// <summary>
        /// Retrieve (GET) an existing instructional-delivery-methods
        /// </summary>
        /// <param name="guid">GUID of the instructional-delivery-methods to get</param>
        /// <returns>A InstructionalDeliveryMethods object <see cref="InstructionalDeliveryMethod"/> in EEDM format</returns>
        [HttpGet]
        public async Task<InstructionalDeliveryMethod> GetInstructionalDeliveryMethodByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No instructional-delivery-methods was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new InstructionalDeliveryMethods
        /// </summary>
        /// <param name="InstructionalDeliveryMethod">DTO of the new InstructionalDeliveryMethods</param>
        /// <returns>A InstructionalDeliveryMethod object <see cref="InstructionalDeliveryMethod"/> in EEDM format</returns>
        [HttpPost]
        public async Task<InstructionalDeliveryMethod> PostInstructionalDeliveryMethodAsync([FromBody] InstructionalDeliveryMethod InstructionalDeliveryMethod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing InstructionalDeliveryMethods
        /// </summary>
        /// <param name="guid">GUID of the InstructionalDeliveryMethods to update</param>
        /// <param name="InstructionalDeliveryMethod">DTO of the updated InstructionalDeliveryMethod</param>
        /// <returns>A InstructionalDeliveryMethod object <see cref="InstructionalDeliveryMethod"/> in EEDM format</returns>
        [HttpPut]
        public async Task<InstructionalDeliveryMethod> PutInstructionalDeliveryMethodAsync([FromUri] string guid, [FromBody] InstructionalDeliveryMethod InstructionalDeliveryMethod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a InstructionalDeliveryMethods
        /// </summary>
        /// <param name="guid">GUID to desired InstructionalDeliveryMethod</param>
        [HttpDelete]
        public async Task DeleteInstructionalDeliveryMethodAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}