//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    public class CourseStatusesService : BaseCoordinationService, ICourseStatusesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public CourseStatusesService(

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
        /// Gets all course-statuses
        /// </summary>
        /// <returns>Collection of CourseStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseStatuses>> GetCourseStatusesAsync(bool bypassCache = false)
        {
            var courseStatusesCollection = new List<Ellucian.Colleague.Dtos.CourseStatuses>();

            var courseStatusesEntities = await _referenceDataRepository.GetCourseStatusesAsync(bypassCache);
            if (courseStatusesEntities != null && courseStatusesEntities.Any())
            {
                foreach (var courseStatuses in courseStatusesEntities)
                {
                    courseStatusesCollection.Add(ConvertCourseStatusesEntityToDto(courseStatuses));
                }
            }
            return courseStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CourseStatuses from its GUID
        /// </summary>
        /// <returns>CourseStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CourseStatuses> GetCourseStatusesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertCourseStatusesEntityToDto((await _referenceDataRepository.GetCourseStatusesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("course-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("course-statuses not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CourseStatuses domain entity to its corresponding CourseStatuses DTO
        /// </summary>
        /// <param name="source">CourseStatuses domain entity</param>
        /// <returns>CourseStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.CourseStatuses ConvertCourseStatusesEntityToDto(Domain.Student.Entities.CourseStatuses source)
        {
            var courseStatuses = new Ellucian.Colleague.Dtos.CourseStatuses();

            courseStatuses.Id = source.Guid;
            courseStatuses.Code = source.Code;
            courseStatuses.Title = source.Description;
            courseStatuses.Description = null;
            switch (source.Status)
            {
                case CourseStatus.Active:
                    courseStatuses.Category = CourseStatusesCategory.Active;
                    break;
                case CourseStatus.Terminated:
                    courseStatuses.Category = CourseStatusesCategory.Ended;
                    break;
            }

            return courseStatuses;
        }
    }
}
