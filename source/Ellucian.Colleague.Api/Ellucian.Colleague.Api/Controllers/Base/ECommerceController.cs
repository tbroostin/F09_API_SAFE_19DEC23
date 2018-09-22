// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to e-Commerce data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ECommerceController : BaseCompressedApiController
    {
        private readonly IECommerceService _ecommerceService;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// ECommerceController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="ecommerceService">Service of type <see cref="IECommerceService">IECommerceService</see></param>
        public ECommerceController(IAdapterRegistry adapterRegistry, IECommerceService ecommerceService)
        {
            _adapterRegistry = adapterRegistry;
            _ecommerceService = ecommerceService;
        }

        /// <summary>
        /// Retrieves all Convenience Fees.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>All <see cref="ConvenienceFee">Convenience Fee codes and descriptions.</see></returns>
        public IEnumerable<ConvenienceFee> GetConvenienceFees()
        {
            return _ecommerceService.GetConvenienceFees();
        }
    }
}