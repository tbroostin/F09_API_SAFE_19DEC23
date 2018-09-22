//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class EmploymentVocationService : BaseCoordinationService, IEmploymentVocationService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public EmploymentVocationService(

            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository, 
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all external-employment
        /// </summary>
        /// <returns>Collection of EmploymentVocations DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentVocation>> GetEmploymentVocationsAsync(bool bypassCache = false)
        {
            var employmentVocationsCollection = new List<Ellucian.Colleague.Dtos.EmploymentVocation>();

            var employmentVocationsEntities = await _referenceDataRepository.GetVocationsAsync(bypassCache);
            if (employmentVocationsEntities != null && employmentVocationsEntities.Any())
            {
                foreach (var employmentVocation in employmentVocationsEntities)
                {
                    employmentVocationsCollection.Add(ConvertEmploymentVocationEntityToDto(employmentVocation));
                }
            }
            return employmentVocationsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmploymentVocation from its GUID
        /// </summary>
        /// <returns>EmploymentVocation DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentVocation> GetEmploymentVocationByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmploymentVocationEntityToDto((await _referenceDataRepository.GetVocationsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No employment vocation was found for guid " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No employment vocation was found for guid " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmploymentVocation domain entity to its corresponding EmploymentVocation DTO
        /// </summary>
        /// <param name="source">EmploymentVocation domain entity</param>
        /// <returns>EmploymentVocation DTO</returns>
        private Ellucian.Colleague.Dtos.EmploymentVocation ConvertEmploymentVocationEntityToDto(Ellucian.Colleague.Domain.Base.Entities.Vocation source)
        {
            var employmentVocation = new Ellucian.Colleague.Dtos.EmploymentVocation();

            employmentVocation.Id = source.Guid;
            employmentVocation.Code = source.Code;
            employmentVocation.Title = string.IsNullOrEmpty(source.Description) ? source.Code : source.Description;
            employmentVocation.Description = null;

            return employmentVocation;
        }
    }
}