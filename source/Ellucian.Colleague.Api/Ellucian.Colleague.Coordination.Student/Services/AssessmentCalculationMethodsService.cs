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
    public class AssessmentCalculationMethodsService : IAssessmentCalculationMethodsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public AssessmentCalculationMethodsService(


            IStudentReferenceDataRepository referenceDataRepository,
            ILogger logger)
        {

            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all assessment-calculation-methods
        /// </summary>
        /// <returns>Collection of AssessmentCalculationMethods DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentCalculationMethods>> GetAssessmentCalculationMethodsAsync(bool bypassCache = false)
        {
            var assessmentCalculationMethodsCollection = new List<Ellucian.Colleague.Dtos.AssessmentCalculationMethods>();

            var assessmentCalculationMethodsEntities = await _referenceDataRepository.GetNonCourseGradeUsesAsync(bypassCache);
            if (assessmentCalculationMethodsEntities != null && assessmentCalculationMethodsEntities.Any())
            {
                foreach (var assessmentCalculationMethods in assessmentCalculationMethodsEntities)
                {
                    assessmentCalculationMethodsCollection.Add(ConvertNonCourseGradeUsesEntityToDto(assessmentCalculationMethods));
                }
            }
            return assessmentCalculationMethodsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get a AssessmentCalculationMethods from its GUID
        /// </summary>
        /// <returns>AssessmentCalculationMethods DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AssessmentCalculationMethods> GetAssessmentCalculationMethodsByGuidAsync(string guid)
        {
            try
            {
                return ConvertNonCourseGradeUsesEntityToDto((await _referenceDataRepository.GetNonCourseGradeUsesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("assessment-calculation-methods not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("assessment-calculation-methods not found for GUID " + guid, ex);
            }
        }


        /// <summary>
        /// Converts a NonCourseGradeUses domain entity to its corresponding AssessmentCalculationMethods DTO
        /// </summary>
        /// <param name="source">NonCourseGradeUses domain entity</param>
        /// <returns>AssessmentCalculationMethods DTO</returns>
        private Ellucian.Colleague.Dtos.AssessmentCalculationMethods ConvertNonCourseGradeUsesEntityToDto(NonCourseGradeUses source)
        {
            var assessmentCalculationMethods = new Ellucian.Colleague.Dtos.AssessmentCalculationMethods();

            assessmentCalculationMethods.Id = source.Guid;
            assessmentCalculationMethods.Code = source.Code;
            assessmentCalculationMethods.Title = source.Description;
            assessmentCalculationMethods.Description = null;

            return assessmentCalculationMethods;
        }
    }
}