//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CourseCategoriesService : BaseCoordinationService, ICourseCategoriesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public CourseCategoriesService(

            IStudentReferenceDataRepository referenceDataRepository,
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
        /// Gets all course-categories
        /// </summary>
        /// <returns>Collection of CourseCategories DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseCategories>> GetCourseCategoriesAsync(bool bypassCache = false)
        {
            var courseCategoriesCollection = new List<Ellucian.Colleague.Dtos.CourseCategories>();

            var courseCategoriesEntities = await _referenceDataRepository.GetCourseTypesAsync(bypassCache);
            if (courseCategoriesEntities != null && courseCategoriesEntities.Any())
            {
                foreach (var courseCategories in courseCategoriesEntities)
                {
                    courseCategoriesCollection.Add(ConvertCourseTypeEntityToDto(courseCategories));
                }
            }
            return courseCategoriesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CourseCategories from its GUID
        /// </summary>
        /// <returns>CourseCategories DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CourseCategories> GetCourseCategoriesByGuidAsync(string guid)
        {
            try
            {
                return ConvertCourseTypeEntityToDto((await _referenceDataRepository.GetCourseTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("course-categories not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("course-categories not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CourseTypes domain entity to its corresponding CourseCategories DTO
        /// </summary>
        /// <param name="source">CourseTypes domain entity</param>
        /// <returns>CourseCategories DTO</returns>
        private Ellucian.Colleague.Dtos.CourseCategories ConvertCourseTypeEntityToDto(CourseType source)
        {
            var courseCategories = new Ellucian.Colleague.Dtos.CourseCategories();

            courseCategories.Id = source.Guid;
            courseCategories.Code = source.Code;
            courseCategories.Title = source.Description;
            courseCategories.Description = null;

            return courseCategories;
        }
    }
}