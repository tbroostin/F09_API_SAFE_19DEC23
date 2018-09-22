// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to GenderIdentityType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GenderIdentityTypesController : BaseCompressedApiController
    {
        private readonly IGenderIdentityTypeService _genderIdentityTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GenderIdentityTypesController class.
        /// </summary>
        /// <param name="genderIdentityTypeService">Service of type <see cref="IGenderIdentityTypeService">IGenderIdentityTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public GenderIdentityTypesController(IGenderIdentityTypeService genderIdentityTypeService, ILogger logger)
        {
            _genderIdentityTypeService = genderIdentityTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves gender identity types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.GenderIdentityType">GenderIdentityType</see> objects></returns>
        [HttpGet]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>> GetAsync()
        {
            try
            {
                bool ignoreCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        ignoreCache = true;
                    }
                }
                return await _genderIdentityTypeService.GetBaseGenderIdentityTypesAsync(ignoreCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }
    }
}
