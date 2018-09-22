//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Coordination.Student.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CourseTitleTypeService : BaseCoordinationService, ICourseTitleTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public CourseTitleTypeService(

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
        /// Gets all course-title-types
        /// </summary>
        /// <returns>Collection of CourseTitleType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseTitleType>> GetCourseTitleTypesAsync(bool bypassCache = false)
        {
            var CourseTitleTypeCollection = new List<Ellucian.Colleague.Dtos.CourseTitleType>();

            var CourseTitleTypeEntities = await _referenceDataRepository.GetCourseTitleTypesAsync(bypassCache);
            if (CourseTitleTypeEntities != null && CourseTitleTypeEntities.Any())
            {
                foreach (var courseTitleType in CourseTitleTypeEntities)
                {
                    CourseTitleTypeCollection.Add(ConvertCourseTitleTypeEntityToDto(courseTitleType));
                }
            }
            return CourseTitleTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CourseTitleType from its GUID
        /// </summary>
        /// <returns>CourseTitleType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CourseTitleType> GetCourseTitleTypeByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertCourseTitleTypeEntityToDto((await _referenceDataRepository.GetCourseTitleTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("course-title-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("course-title-types not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgCrsTitleTypes domain entity to its corresponding CourseTitleType DTO
        /// </summary>
        /// <param name="source">IntgCrsTitleTypes domain entity</param>
        /// <returns>CourseTitleType DTO</returns>
        private Ellucian.Colleague.Dtos.CourseTitleType ConvertCourseTitleTypeEntityToDto(Domain.Student.Entities.CourseTitleType source)
        {
            var CourseTitleType = new Ellucian.Colleague.Dtos.CourseTitleType();

            CourseTitleType.Id = source.Guid;
            CourseTitleType.Code = source.Code;
            CourseTitleType.Title = source.Title;
            CourseTitleType.Description = source.Description;

            return CourseTitleType;
        }


    }
}
