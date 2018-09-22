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

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmploymentPerformanceReviewTypesService : BaseCoordinationService, IEmploymentPerformanceReviewTypesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public EmploymentPerformanceReviewTypesService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all employment-performance-review-types
        /// </summary>
        /// <returns>Collection of EmploymentPerformanceReviewTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes>> GetEmploymentPerformanceReviewTypesAsync(bool bypassCache = false)
        {
            var employmentPerformanceReviewTypesCollection = new List<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes>();

            var employmentPerformanceReviewTypesEntities = await _referenceDataRepository.GetEmploymentPerformanceReviewTypesAsync(bypassCache);
            if (employmentPerformanceReviewTypesEntities != null && employmentPerformanceReviewTypesEntities.Any())
            {
                foreach (var employmentPerformanceReviewTypes in employmentPerformanceReviewTypesEntities)
                {
                    employmentPerformanceReviewTypesCollection.Add(ConvertEmploymentPerformanceReviewTypesEntityToDto(employmentPerformanceReviewTypes));
                }
            }
            return employmentPerformanceReviewTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmploymentPerformanceReviewTypes from its GUID
        /// </summary>
        /// <returns>EmploymentPerformanceReviewTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes> GetEmploymentPerformanceReviewTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmploymentPerformanceReviewTypesEntityToDto((await _referenceDataRepository.GetEmploymentPerformanceReviewTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("employment-performance-review-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("employment-performance-review-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmploymentPerformanceReviewTypes domain entity to its corresponding EmploymentPerformanceReviewTypes DTO
        /// </summary>
        /// <param name="source">EmploymentPerformanceReviewTypes domain entity</param>
        /// <returns>EmploymentPerformanceReviewTypes DTO</returns>
        private Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes ConvertEmploymentPerformanceReviewTypesEntityToDto(EmploymentPerformanceReviewType source)
        {
            var employmentPerformanceReviewTypes = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviewTypes();

            employmentPerformanceReviewTypes.Id = source.Guid;
            employmentPerformanceReviewTypes.Code = source.Code;
            employmentPerformanceReviewTypes.Title = source.Description;
            employmentPerformanceReviewTypes.Description = null;
            employmentPerformanceReviewTypes.Frequency = new Dtos.DtoProperties.FrequencyDtoProperty() { Value = Convert.ToInt32(source.Frequency), Unit = FrequencyUnitType.Day };

            return employmentPerformanceReviewTypes;
        }

    }

}