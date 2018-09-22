//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CourseTopicsService : BaseCoordinationService, ICourseTopicsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public CourseTopicsService(

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
        /// Gets all course-topics
        /// </summary>
        /// <returns>Collection of CourseTopics DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseTopics>> GetCourseTopicsAsync(bool bypassCache = false)
        {
            var courseTopicsCollection = new List<Ellucian.Colleague.Dtos.CourseTopics>();

            var courseTopicsEntities = await _referenceDataRepository.GetCourseTopicsAsync(bypassCache);
            if (courseTopicsEntities != null && courseTopicsEntities.Any())
            {
                foreach (var courseTopics in courseTopicsEntities)
                {
                    courseTopicsCollection.Add(ConvertCourseTopicsEntityToDto(courseTopics));
                }
            }
            return courseTopicsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CourseTopics from its GUID
        /// </summary>
        /// <returns>CourseTopics DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CourseTopics> GetCourseTopicsByGuidAsync(string guid)
        {
            try
            {
                return ConvertCourseTopicsEntityToDto((await _referenceDataRepository.GetCourseTopicsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("course-topics not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("course-topics not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CourseTopic domain entity to its corresponding CourseTopics DTO
        /// </summary>
        /// <param name="source">CourseTopic domain entity</param>
        /// <returns>CourseTopics DTO</returns>
        private Ellucian.Colleague.Dtos.CourseTopics ConvertCourseTopicsEntityToDto(CourseTopic source)
        {
            var courseTopics = new Ellucian.Colleague.Dtos.CourseTopics();

            courseTopics.Id = source.Guid;
            courseTopics.Code = source.Code;
            courseTopics.Title = source.Description;
            courseTopics.Description = null;           
                                                                        
            return courseTopics;
        }
 
    }
   
}