// Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Clear API cache keys for Ethos integration
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EedmCacheKeysController : BaseCompressedApiController
    {
        private readonly IEedmCacheKeysService _eedmCacheKeysService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EedmCacheKeysController class.
        /// </summary>
        /// <param name="eedmCacheKeysService">Service of type <see cref="IEedmCacheKeysService">IEedmCacheKeysService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EedmCacheKeysController(IEedmCacheKeysService eedmCacheKeysService, ILogger logger)
        {
            _eedmCacheKeysService = eedmCacheKeysService;
            _logger = logger;
        }

        /// <summary>
        /// POST - Clear EEDM cache keys
        /// </summary>
        public void ClearEedmCacheKeys()
        {
            _eedmCacheKeysService.ClearEedmCacheKeys();
        }
    }
}