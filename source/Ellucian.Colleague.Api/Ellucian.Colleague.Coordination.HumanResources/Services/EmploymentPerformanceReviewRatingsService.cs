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
    public class EmploymentPerformanceReviewRatingsService : BaseCoordinationService, IEmploymentPerformanceReviewRatingsService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public EmploymentPerformanceReviewRatingsService(

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
        /// Gets all employment-performance-review-ratings
        /// </summary>
        /// <returns>Collection of EmploymentPerformanceReviewRatings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings>> GetEmploymentPerformanceReviewRatingsAsync(bool bypassCache = false)
        {
            var employmentPerformanceReviewRatingsCollection = new List<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings>();

            var employmentPerformanceReviewRatingsEntities = await _referenceDataRepository.GetEmploymentPerformanceReviewRatingsAsync(bypassCache);
            if (employmentPerformanceReviewRatingsEntities != null && employmentPerformanceReviewRatingsEntities.Any())
            {
                foreach (var employmentPerformanceReviewRatings in employmentPerformanceReviewRatingsEntities)
                {
                    employmentPerformanceReviewRatingsCollection.Add(ConvertEmploymentPerformanceReviewRatingsEntityToDto(employmentPerformanceReviewRatings));
                }
            }
            return employmentPerformanceReviewRatingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmploymentPerformanceReviewRatings from its GUID
        /// </summary>
        /// <returns>EmploymentPerformanceReviewRatings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings> GetEmploymentPerformanceReviewRatingsByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmploymentPerformanceReviewRatingsEntityToDto((await _referenceDataRepository.GetEmploymentPerformanceReviewRatingsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("employment-performance-review-ratings not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("employment-performance-review-ratings not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmploymentPerformanceReviewRatings domain entity to its corresponding EmploymentPerformanceReviewRatings DTO
        /// </summary>
        /// <param name="source">EmploymentPerformanceReviewRatings domain entity</param>
        /// <returns>EmploymentPerformanceReviewRatings DTO</returns>
        private Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings ConvertEmploymentPerformanceReviewRatingsEntityToDto(EmploymentPerformanceReviewRating source)
        {
            var employmentPerformanceReviewRatings = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviewRatings();

            employmentPerformanceReviewRatings.Id = source.Guid;
            employmentPerformanceReviewRatings.Code = source.Code;
            employmentPerformanceReviewRatings.Title = source.Description;
            employmentPerformanceReviewRatings.Description = null;           
                                                                        
            return employmentPerformanceReviewRatings;
        }
  
    }

}