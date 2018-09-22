// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonalPronounType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalPronounTypesController : BaseCompressedApiController
    {
        private readonly IPersonalPronounTypeService _personalPronounTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonalPronounTypesController class.
        /// </summary>
        /// <param name="personalPronounTypeService">Service of type <see cref="IPersonalPronounTypeService">IPersonalPronounTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PersonalPronounTypesController(IPersonalPronounTypeService personalPronounTypeService, ILogger logger)
        {
            _personalPronounTypeService = personalPronounTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves personal pronoun types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.PersonalPronounType">PersonalPronounType</see> objects></returns>
        [HttpGet]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>> GetAsync()
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
                return await _personalPronounTypeService.GetBasePersonalPronounTypesAsync(ignoreCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }
    }
}