// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to CreditTypes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CreditTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the CredTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        public CreditTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        // GET /api/credit-types
        /// <summary>
        /// Return all Credit Types Code and Description
        /// </summary>
        /// <returns>List of <see cref="CredType">CredType</see></returns>
        public async Task<IEnumerable<CredType>> GetAsync()
        {
            var creditTypeCollection = await _referenceDataRepository.GetCreditTypesAsync();

            // Get the right adapter for the type mapping
            var creditTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CredType, CredType>();

            // Map the degree plan entity to the degree plan DTO
            var creditTypeDtoCollection = new List<CredType>();
            foreach (var credtype in creditTypeCollection)
            {
                creditTypeDtoCollection.Add(creditTypeDtoAdapter.MapToType(credtype));
            }

            return creditTypeDtoCollection;
        }
    }
}
