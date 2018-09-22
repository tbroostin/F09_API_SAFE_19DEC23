// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Access to Department data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class DepartmentsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// DepartmentsController Constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Reference data repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        public DepartmentsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Departments
        /// </summary>
        /// <returns>All <see cref="Department">Department codes and descriptions.</see></returns>
        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            var departmentCollection = await _referenceDataRepository.DepartmentsAsync();

            // Get the right adapter for the type mapping
            var departmentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Department, Department>();

            // Map the degree plan entity to the degree plan DTO
            var departmentDtoCollection = new List<Department>();
            foreach (var department in departmentCollection)
            {
                departmentDtoCollection.Add(departmentDtoAdapter.MapToType(department));
            }

            return departmentDtoCollection;
        }

        /// <summary>
        /// Retrieves all active Departments.
        /// </summary>
        /// <returns>All <see cref="Department">active Department codes and descriptions.</see></returns>
        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
        {
            var departmentCollection = await _referenceDataRepository.DepartmentsAsync();

            // Get the right adapter for the type mapping
            var departmentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Department, Department>();

            // Map the degree plan entity to the degree plan DTO
            var departmentDtoCollection = new List<Department>();
            foreach (var department in departmentCollection)
            {
                if (department.IsActive)
                {
                    departmentDtoCollection.Add(departmentDtoAdapter.MapToType(department));
                }
            }

            return departmentDtoCollection;
        }
    }
}
