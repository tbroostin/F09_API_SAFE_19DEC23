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
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class InstructorCategoriesService : StudentCoordinationService, IInstructorCategoriesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public InstructorCategoriesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all instructor-categories
        /// </summary>
        /// <returns>Collection of InstructorCategories DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorCategories>> GetInstructorCategoriesAsync(bool bypassCache = false)
        {
            var instructorCategoriesCollection = new List<Ellucian.Colleague.Dtos.InstructorCategories>();

            var instructorCategoriesEntities = await _referenceDataRepository.GetFacultySpecialStatusesAsync(bypassCache);
            if (instructorCategoriesEntities != null && instructorCategoriesEntities.Any())
            {
                foreach (var instructorCategories in instructorCategoriesEntities)
                {
                    instructorCategoriesCollection.Add(ConvertInstructorCategoriesEntityToDto(instructorCategories));
                }
            }
            return instructorCategoriesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a InstructorCategories from its GUID
        /// </summary>
        /// <returns>InstructorCategories DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.InstructorCategories> GetInstructorCategoriesByGuidAsync(string guid)
        {
            try
            {
                return ConvertInstructorCategoriesEntityToDto((await _referenceDataRepository.GetFacultySpecialStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("instructor-categories not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("instructor-categories not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FacultySpecialStatuses domain entity to its corresponding InstructorCategories DTO
        /// </summary>
        /// <param name="source">InstructorCategories domain entity</param>
        /// <returns>InstructorCategories DTO</returns>
        private Ellucian.Colleague.Dtos.InstructorCategories ConvertInstructorCategoriesEntityToDto(FacultySpecialStatuses source)
        {
            var instructorCategories = new Ellucian.Colleague.Dtos.InstructorCategories();

            instructorCategories.Id = source.Guid;
            instructorCategories.Code = source.Code;
            instructorCategories.Title = source.Description;
            instructorCategories.Description = null;

            return instructorCategories;
        }


    }
}
