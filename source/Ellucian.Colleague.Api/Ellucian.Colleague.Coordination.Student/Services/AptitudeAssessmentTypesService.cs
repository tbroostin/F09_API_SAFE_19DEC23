//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AptitudeAssessmentTypesService : BaseCoordinationService, IAptitudeAssessmentTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        
        public AptitudeAssessmentTypesService(

            IStudentReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all aptitude-assessment-types
        /// </summary>
        /// <returns>Collection of AptitudeAssessmentTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessmentTypes>> GetAptitudeAssessmentTypesAsync(bool bypassCache = false)
        {
            var aptitudeAssessmentTypesCollection = new List<Ellucian.Colleague.Dtos.AptitudeAssessmentTypes>();

            var specialProcesssingCodesToInclude = new List<string>(new string[] { "A", "P", "T" });

            var aptitudeAssessmentTypesEntities = await _referenceDataRepository.GetNonCourseCategoriesAsync(bypassCache);
            if (aptitudeAssessmentTypesEntities != null && aptitudeAssessmentTypesEntities.Any())
            {
                foreach (var aptitudeAssessmentTypes in aptitudeAssessmentTypesEntities)
                {
                    if (specialProcesssingCodesToInclude.Contains(aptitudeAssessmentTypes.SpecialProcessingCode))
                    {
                        aptitudeAssessmentTypesCollection.Add(ConvertNonCourseCategoriesEntityToDto(aptitudeAssessmentTypes));
                    }
                }
            }
            return aptitudeAssessmentTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get an AptitudeAssessmentTypes from its GUID
        /// </summary>
        /// <returns>AptitudeAssessmentTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AptitudeAssessmentTypes> GetAptitudeAssessmentTypesByGuidAsync(string guid)
        {
            try
            {
                var nonCourseCategory = (await _referenceDataRepository.GetNonCourseCategoriesAsync(true)).First(r => r.Guid == guid);
                if (nonCourseCategory == null)
                {
                    throw new KeyNotFoundException(string.Format("aptitude-assessment-types not found for GUID '{0}'", guid));
                }
                var specialProcesssingCodesToInclude = new List<string>(new string[] {"A", "P", "T"});
                if (specialProcesssingCodesToInclude.Contains(nonCourseCategory.SpecialProcessingCode))
                {
                    return ConvertNonCourseCategoriesEntityToDto(nonCourseCategory);
                }
                return null;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("aptitude-assessment-types not found for GUID '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("aptitude-assessment-types not found for GUID '{0}'", guid), ex);
            }
        }

        /// <summary>
        /// Converts a NonCourseCategories domain entity to its corresponding AptitudeAssessmentTypes DTO
        /// </summary>
        /// <param name="source">NonCourseCategories domain entity</param>
        /// <returns>AptitudeAssessmentTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AptitudeAssessmentTypes ConvertNonCourseCategoriesEntityToDto(NonCourseCategories source)
        {
            var aptitudeAssessmentTypes = new Ellucian.Colleague.Dtos.AptitudeAssessmentTypes();

            var specialProcesssingCodesToInclude = new List<string>(new string[] {"A", "P", "T"});
            if (!specialProcesssingCodesToInclude.Contains(source.SpecialProcessingCode))
                return null;

            aptitudeAssessmentTypes.Id = source.Guid;
            aptitudeAssessmentTypes.Code = source.Code;
            aptitudeAssessmentTypes.Title = source.Description;
            aptitudeAssessmentTypes.Description = null;

            return aptitudeAssessmentTypes;
        }
    }
}