//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmploymentDepartmentsService : BaseCoordinationService, IEmploymentDepartmentsService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public EmploymentDepartmentsService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all employment-departments
        /// </summary>
        /// <returns>Collection of EmploymentDepartments DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentDepartments>> GetEmploymentDepartmentsAsync(bool bypassCache = false)
        {
            var employmentDepartmentsCollection = new List<Ellucian.Colleague.Dtos.EmploymentDepartments>();

            var employmentDepartmentsEntities = await _referenceDataRepository.GetEmploymentDepartmentsAsync(bypassCache);
            if (employmentDepartmentsEntities != null && employmentDepartmentsEntities.Any())
            {
                foreach (var employmentDepartments in employmentDepartmentsEntities)
                {
                    employmentDepartmentsCollection.Add(ConvertEmploymentDepartmentsEntityToDto(employmentDepartments));
                }
            }
            return employmentDepartmentsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmploymentDepartments from its GUID
        /// </summary>
        /// <returns>EmploymentDepartments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentDepartments> GetEmploymentDepartmentsByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmploymentDepartmentsEntityToDto((await _referenceDataRepository.GetEmploymentDepartmentsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("employment-departments not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("employment-departments not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Depts domain entity to its corresponding EmploymentDepartments DTO
        /// </summary>
        /// <param name="source">Depts domain entity</param>
        /// <returns>EmploymentDepartments DTO</returns>
        private Ellucian.Colleague.Dtos.EmploymentDepartments ConvertEmploymentDepartmentsEntityToDto(EmploymentDepartment source)
        {
            var employmentDepartments = new Ellucian.Colleague.Dtos.EmploymentDepartments();

            employmentDepartments.Id = source.Guid;
            employmentDepartments.Code = source.Code;
            employmentDepartments.Title = source.Description;
            employmentDepartments.Description = null;           
                                                                        
            return employmentDepartments;
        }

      
    }
   
}