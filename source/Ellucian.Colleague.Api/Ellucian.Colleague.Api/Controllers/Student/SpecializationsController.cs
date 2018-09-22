// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.License;
using Ellucian.Colleague.Configuration.Licensing;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Specialization data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SpecializationsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the MajorsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        public SpecializationsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        // GET /api/specialization
        /// <summary>
        /// Retrieves all Specializations.
        /// </summary>
        /// <returns>All <see cref="Specialization">Specialization</see> codes and descriptions.</returns>
        public async Task<IEnumerable<Specialization>> GetAsync()
        {
            var specializationCollection = await _referenceDataRepository.GetSpecializationsAsync();

            // Get the right adapter for the type mapping
            var specializationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Specialization, Specialization>();

            // Map the degree plan entity to the degree plan DTO
            var specializationDtoCollection = new List<Specialization>();
            foreach (var specialization in specializationCollection)
            {
                specializationDtoCollection.Add(specializationDtoAdapter.MapToType(specialization));
            }

            return specializationDtoCollection;
        }
    }
}

