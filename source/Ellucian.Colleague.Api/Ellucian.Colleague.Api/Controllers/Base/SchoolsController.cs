// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Schools data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SchoolsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _schoolRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SchoolsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="schoolRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public SchoolsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository schoolRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _schoolRepository = schoolRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets information for all Schools codes
        /// </summary>
        /// <returns>List of <see cref="School"/>Schools</returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public IEnumerable<Ellucian.Colleague.Dtos.Base.School> Get()
        {
            var schoolDtoCollection = new List<Ellucian.Colleague.Dtos.Base.School>();
            var schoolCollection = _schoolRepository.Schools;
            // Get the right adapter for the type mapping
            var schoolDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.School, Ellucian.Colleague.Dtos.Base.School>();
            // Map the grade entity to the grade DTO
            foreach (var school in schoolCollection)
            {
                schoolDtoCollection.Add(schoolDtoAdapter.MapToType(school));
            }

            return schoolDtoCollection;

        }
    }
}
