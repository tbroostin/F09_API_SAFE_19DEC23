//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AssessmentPercentileTypesService : IAssessmentPercentileTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public AssessmentPercentileTypesService(
            IStudentReferenceDataRepository referenceDataRepository,
            ILogger logger)
        {

            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all assessment-percentile-types
        /// </summary>
        /// <returns>Collection of AssessmentPercentileTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentPercentileTypes>> GetAssessmentPercentileTypesAsync(bool bypassCache = false)
        {
            var assessmentPercentileTypesCollection = new List<Ellucian.Colleague.Dtos.AssessmentPercentileTypes>();

            var assessmentPercentileTypesEntities = await _referenceDataRepository.GetIntgTestPercentileTypesAsync(bypassCache);
            if (assessmentPercentileTypesEntities != null && assessmentPercentileTypesEntities.Any())
            {
                foreach (var assessmentPercentileTypes in assessmentPercentileTypesEntities)
                {
                    assessmentPercentileTypesCollection.Add(ConvertIntgTestPercentileTypeEntityToDto(assessmentPercentileTypes));
                }
            }
            return assessmentPercentileTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get a AssessmentPercentileTypes from its GUID
        /// </summary>
        /// <returns>AssessmentPercentileTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AssessmentPercentileTypes> GetAssessmentPercentileTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertIntgTestPercentileTypeEntityToDto((await _referenceDataRepository.GetIntgTestPercentileTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("assessment-percentile-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("assessment-percentile-types not found for GUID " + guid, ex);
            }
        }

 
        /// <summary>
        /// Converts a IntgTestPercentileType domain entity to its corresponding AssessmentPercentileTypes DTO
        /// </summary>
        /// <param name="source">IntgTestPercentileType domain entity</param>
        /// <returns>AssessmentPercentileTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AssessmentPercentileTypes ConvertIntgTestPercentileTypeEntityToDto(IntgTestPercentileType source)
        {
            var assessmentPercentileTypes = new Ellucian.Colleague.Dtos.AssessmentPercentileTypes();

            assessmentPercentileTypes.Id = source.Guid;
            assessmentPercentileTypes.Code = source.Code;
            assessmentPercentileTypes.Title = source.Description;
            assessmentPercentileTypes.Description = null;

            return assessmentPercentileTypes;
        }


    }
}