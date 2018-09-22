//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AptitudeAssessmentsService : BaseCoordinationService, IAptitudeAssessmentsService
    {

        private readonly IAptitudeAssessmentsRepository _aptitudeAssessmentsRepository;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private IEnumerable<string> _aptitudeAssesmentKeys;

        /// <summary>
        /// ...ctor
        /// </summary>
        /// <param name="aptitudeAssessmentsRepository"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public AptitudeAssessmentsService(

            IAptitudeAssessmentsRepository aptitudeAssessmentsRepository,
            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IConfigurationRepository configurationRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger,null, configurationRepository)
        {

            _aptitudeAssessmentsRepository = aptitudeAssessmentsRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        #region GET methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all aptitude-assessments
        /// </summary>
        /// <returns>Collection of AptitudeAssessments DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessment>> GetAptitudeAssessmentsAsync(bool bypassCache = false)
        {
            var aptitudeAssessmentsCollection = new List<Ellucian.Colleague.Dtos.AptitudeAssessment>();

            var aptitudeAssessmentsEntities = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsAsync(bypassCache);
            if (aptitudeAssessmentsEntities != null && aptitudeAssessmentsEntities.Any())
            {
                _aptitudeAssesmentKeys = aptitudeAssessmentsEntities.Where(key => !string.IsNullOrEmpty(key.ParentAssessmentId)).Select(k => k.ParentAssessmentId).Distinct();

                foreach (var aptitudeAssessment in aptitudeAssessmentsEntities)
                {
                    aptitudeAssessmentsCollection.Add(await ConvertAptitudeAssessmentsEntityToDto(aptitudeAssessment));
                }
            }
            return aptitudeAssessmentsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a AptitudeAssessments from its GUID
        /// </summary>
        /// <returns>AptitudeAssessments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AptitudeAssessment> GetAptitudeAssessmentsByGuidAsync(string guid)
        {
            try
            {
                var aptitudeAssesmentEntity = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentByIdAsync(guid);
                _aptitudeAssesmentKeys = new List<string>() { aptitudeAssesmentEntity.ParentAssessmentId };
                return await ConvertAptitudeAssessmentsEntityToDto(aptitudeAssesmentEntity);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("aptitude-assessments not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("aptitude-assessments not found for GUID " + guid, ex);
            }
        }
        #endregion

        #region Convert methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AptitudeAssessments domain entity to its corresponding AptitudeAssessments DTO
        /// </summary>
        /// <param name="source">AptitudeAssessments domain entity</param>
        /// <returns>AptitudeAssessments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.AptitudeAssessment> ConvertAptitudeAssessmentsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.NonCourse source)
        {
            var aptitudeAssessments = new Ellucian.Colleague.Dtos.AptitudeAssessment();

            aptitudeAssessments.Id = source.Guid;
            //The NCRS.SHORT.TITLE is not required in Colleague, so we need special logic in the API to publish the code found in NON.COURSES.ID as the title.
            aptitudeAssessments.Title = source.Title;
            aptitudeAssessments.Code = source.Code;            
            aptitudeAssessments.Description = string.IsNullOrEmpty(source.Description)? null : source.Description;
            aptitudeAssessments.ParentAssessment = string.IsNullOrEmpty(source.ParentAssessmentId)? null : await ConvertEntityToParentAssessmentDto(source.ParentAssessmentId);
            aptitudeAssessments.ValidScores = ConvertEntityToValidScoresDto(source.ScoreMin, source.ScoreMax);
            aptitudeAssessments.AssessmentType = await ConvertEntityToAssessmentTypeDto(source.AssessmentTypeId);//Convert id to guid
            aptitudeAssessments.Calculation = await ConvertEntityToAssessmentCalculationDto(source.CalculationMethod);                                                 
            return aptitudeAssessments;
        }

        /// <summary>
        /// Gets guid object for the ParentAssessment
        /// </summary>
        /// <param name="parentAssessmentKey"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToParentAssessmentDto(string parentAssessmentKey)
        {
            string value = string.Empty;
            var assessmentGuid = (await AptitudeAssesmentGuids(_aptitudeAssesmentKeys)).TryGetValue(parentAssessmentKey, out value);
            if (!(await AptitudeAssesmentGuids(_aptitudeAssesmentKeys)).TryGetValue(parentAssessmentKey, out value))
            {
                return null;
            }
            return string.IsNullOrEmpty(value)? null : new GuidObject2(value);
        }        

        /// <summary>
        /// Valid Score
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private Dtos.DtoProperties.ValidScoreProperty ConvertEntityToValidScoresDto(int? min, int? max)
        {
            Dtos.DtoProperties.ValidScoreProperty validScore = new Dtos.DtoProperties.ValidScoreProperty()
            {
                ValidScoreRange = new Dtos.DtoProperties.ValidScoreRangeProperty()
                {
                    ValidScoreMinimum = min,
                    ValidScoreMaximum = max
                },
            };
            return (!validScore.ValidScoreRange.ValidScoreMinimum.HasValue && !validScore.ValidScoreRange.ValidScoreMaximum.HasValue) ? null : validScore;
        }

        /// <summary>
        /// Returns guid object for assessment type
        /// </summary>
        /// <param name="assessmentTypeId"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToAssessmentTypeDto(string assessmentTypeId)
        {
            if (string.IsNullOrEmpty(assessmentTypeId))
            {
                return null;
            }
            var assessmentType = (await AssessmentTypes()).FirstOrDefault(i => i.Code.Equals(assessmentTypeId, StringComparison.OrdinalIgnoreCase));
            if (assessmentType == null)
            {
                throw new KeyNotFoundException("aptitude-assessment-type not found for id " + assessmentTypeId);
            }
            return new GuidObject2(assessmentType.Guid);
        }
        
        /// <summary>
        /// Assessment Calculation
        /// </summary>
        /// <param name="calculationMethod"></param>
        /// <returns></returns>
        private async Task<Dtos.DtoProperties.AssessmentCalculationProperty> ConvertEntityToAssessmentCalculationDto(string calculationMethod)
        {
            if(string.IsNullOrEmpty(calculationMethod))
            {
                return new Dtos.DtoProperties.AssessmentCalculationProperty() 
                {
                    CalculationType = CalculationType.Raw,
                    CalculationMethod = null
                };
            }
            var calcMethod = (await CalculationMethods()).FirstOrDefault(i => i.Code.Equals(calculationMethod, StringComparison.OrdinalIgnoreCase));
            if (calcMethod == null)
            {
                throw new KeyNotFoundException("assessment-calculation-method not found for code " + calculationMethod);
            }
            return new Dtos.DtoProperties.AssessmentCalculationProperty() 
            {
                CalculationType = CalculationType.Calculated,
                CalculationMethod = new GuidObject2(calcMethod.Guid)
            };
        }
        #endregion

        #region Other helper methods
        /// <summary>
        /// Assessment Types
        /// </summary>
        private IDictionary<string, string> _aptitudeAssesmentGuids;
        private async Task<IDictionary<string, string>> AptitudeAssesmentGuids(IEnumerable<string> aptitudeAssessmentKeys)
        {
            if (_aptitudeAssesmentGuids == null)
            {
                _aptitudeAssesmentGuids = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentGuidsAsync(aptitudeAssessmentKeys);
            }
            return _aptitudeAssesmentGuids;
        }

        /// <summary>
        /// Assessment Types
        /// </summary>
        private IEnumerable<NonCourseCategories> _assesmentTypes;
        private async Task<IEnumerable<NonCourseCategories>> AssessmentTypes()
        {
            if (_assesmentTypes == null)
            {
                _assesmentTypes = await _referenceDataRepository.GetNonCourseCategoriesAsync(false);
            }
            return _assesmentTypes;
        }

        /// <summary>
        /// Calculation Methods
        /// </summary>
        private IEnumerable<NonCourseGradeUses> _calculationMethods;
        private async Task<IEnumerable<NonCourseGradeUses>> CalculationMethods()
        {
            if (_calculationMethods == null)
            {
                _calculationMethods = await _referenceDataRepository.GetNonCourseGradeUsesAsync(false);
            }
            return _calculationMethods;
        }
        #endregion
    }   
}