using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
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

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Controller exposes actions to interact with EarningsTypeGroups
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EarningsTypeGroupsController : BaseCompressedApiController
    {
        private ILogger logger;
        private IAdapterRegistry adapterRegistry;
        private IHumanResourcesReferenceDataRepository referenceDataRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        public EarningsTypeGroupsController(ILogger logger, IAdapterRegistry adapterRegistry, IHumanResourcesReferenceDataRepository referenceDataRepository)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Get all EarningsTypeGroups. This endpoint is used in SelfService
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can view EarningsTypeGroups
        /// </accessComments>
        /// <returns>A list of all EarningsTypeGroups</returns>
        public async Task<IEnumerable<EarningsTypeGroup>> GetEarningsTypeGroupsAsync()
        {
            var earningsTypeGroupDictionary = await referenceDataRepository.GetEarningsTypesGroupsAsync();
            if (earningsTypeGroupDictionary == null || !earningsTypeGroupDictionary.Any())
            {
                return new List<EarningsTypeGroup>();
            }

            var adapter = adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EarningsTypeGroup, EarningsTypeGroup>();
            var dtos = earningsTypeGroupDictionary.Values.Select(etg => adapter.MapToType(etg));
            return dtos;
        }

    }
}