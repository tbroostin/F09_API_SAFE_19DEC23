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
    public class EmploymentFrequenciesService : BaseCoordinationService, IEmploymentFrequenciesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public EmploymentFrequenciesService(

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
        /// Gets all employment-frequencies
        /// </summary>
        /// <returns>Collection of EmploymentFrequencies DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentFrequencies>> GetEmploymentFrequenciesAsync(bool bypassCache = false)
        {
            var employmentFrequenciesCollection = new List<Ellucian.Colleague.Dtos.EmploymentFrequencies>();

            var employmentFrequenciesEntities = await _referenceDataRepository.GetEmploymentFrequenciesAsync(bypassCache);
            if (employmentFrequenciesEntities != null && employmentFrequenciesEntities.Any())
            {
                foreach (var employmentFrequencies in employmentFrequenciesEntities)
                {
                    employmentFrequenciesCollection.Add(ConvertEmploymentFrequenciesEntityToDto(employmentFrequencies));
                }
            }
            return employmentFrequenciesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmploymentFrequencies from its GUID
        /// </summary>
        /// <returns>EmploymentFrequencies DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentFrequencies> GetEmploymentFrequenciesByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmploymentFrequenciesEntityToDto((await _referenceDataRepository.GetEmploymentFrequenciesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("employment-frequencies not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("employment-frequencies not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmploymentFrequency domain entity to its corresponding EmploymentFrequencies DTO
        /// </summary>
        /// <param name="source">EmploymentFrequency domain entity</param>
        /// <returns>EmploymentFrequencies DTO</returns>
        private Ellucian.Colleague.Dtos.EmploymentFrequencies ConvertEmploymentFrequenciesEntityToDto(EmploymentFrequency source)
        {
            var employmentFrequencies = new Ellucian.Colleague.Dtos.EmploymentFrequencies();

            employmentFrequencies.Id = source.Guid;
            employmentFrequencies.Code = source.Code;
            employmentFrequencies.Title = source.Description;
            employmentFrequencies.Description = null;           
                                                                          
            employmentFrequencies.Type= ConvertEmploymentFrequenciesTypeDomainEnumToEmploymentFrequenciesTypeDtoEnum(source.EmploymentFrequenciesType);
                        
            return employmentFrequencies;
        }

      
   
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmploymentFrequenciesType domain enumeration value to its corresponding EmploymentFrequenciesType DTO enumeration value
        /// </summary>
        /// <param name="source">EmploymentFrequenciesType domain enumeration value</param>
        /// <returns>EmploymentFrequenciesType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.EmploymentFrequenciesType ConvertEmploymentFrequenciesTypeDomainEnumToEmploymentFrequenciesTypeDtoEnum(Domain.HumanResources.Entities.EmploymentFrequenciesType source)
        {
            switch (source)
            {

                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Daily:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Daily;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Weekly:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Weekly;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Biweekly:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Biweekly;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Monthly:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Monthly;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Quarterly:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Quarterly;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Semiannually:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Semiannually;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Annually:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Annually;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Semimonthly:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Semimonthly;
                case Domain.HumanResources.Entities.EmploymentFrequenciesType.Contractual:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Contractual;
                default:
                    return Dtos.EnumProperties.EmploymentFrequenciesType.Contractual;
            }
        }
   }
}