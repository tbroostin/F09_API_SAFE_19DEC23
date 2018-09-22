// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to application influence data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ApplicationInfluencesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the ApplicationInfluencesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        public ApplicationInfluencesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Application Influences.
        /// </summary>
        /// <returns>All <see cref="ApplicationInfluence">Application Influence</see> codes and descriptions.</returns>
        public async Task<IEnumerable<ApplicationInfluence>> GetAsync()
        {
            var applicationInfluenceCollection = await _referenceDataRepository.GetApplicationInfluencesAsync();

            // Get the right adapter for the type mapping
            var applicationInfluenceDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ApplicationInfluence, ApplicationInfluence>();

            // Map the ApplicationInfluence entity to the program DTO
            var applicationInfluenceDtoCollection = new List<ApplicationInfluence>();
            foreach (var applicationInfluence in applicationInfluenceCollection)
            {
                applicationInfluenceDtoCollection.Add(applicationInfluenceDtoAdapter.MapToType(applicationInfluence));
            }

            return applicationInfluenceDtoCollection;
        }
    }
}