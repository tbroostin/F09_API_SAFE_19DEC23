// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Restrictions data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RestrictionTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _restrictionRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RestrictionsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="restrictionRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public RestrictionTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository restrictionRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _restrictionRepository = restrictionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets information for all Restriction Types
        /// </summary>
        /// <returns>List of <see cref="Restriction">Restrictions</see></returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.RestrictionType>> GetAsync()
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }     
            var restrictionDtoCollection = new List<Ellucian.Colleague.Dtos.Student.RestrictionType>();
            var restrictionCollection = await _restrictionRepository.GetRestrictionsAsync(bypassCache);
            // Get the right adapter for the type mapping
            var restrictionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, Ellucian.Colleague.Dtos.Student.RestrictionType>();
            // Map the grade entity to the grade DTO
            foreach (var restriction in restrictionCollection)
            {
                restrictionDtoCollection.Add(restrictionDtoAdapter.MapToType(restriction));
            }

            return restrictionDtoCollection;

        }
    }
}
