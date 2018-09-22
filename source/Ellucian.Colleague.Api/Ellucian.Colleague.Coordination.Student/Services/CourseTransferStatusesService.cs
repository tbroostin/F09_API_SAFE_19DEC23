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
    public class CourseTransferStatusesService : BaseCoordinationService, ICourseTransferStatusesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public CourseTransferStatusesService(

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
        /// Gets all course-transfer-statuses
        /// </summary>
        /// <returns>Collection of CourseTransferStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseTransferStatuses>> GetCourseTransferStatusesAsync(bool bypassCache = false)
        {
            var courseTransferStatusesCollection = new List<Ellucian.Colleague.Dtos.CourseTransferStatuses>();

            var sectionRegistrationStatusItemEntities = await _referenceDataRepository.GetStudentAcademicCreditStatusesAsync(bypassCache);
                        
            if (sectionRegistrationStatusItemEntities != null && sectionRegistrationStatusItemEntities.Any())
            {
                foreach (var courseTransferStatuses in sectionRegistrationStatusItemEntities)
                {
                    if (courseTransferStatuses.Category != null && !courseTransferStatuses.Category.Equals(Ellucian.Colleague.Domain.Student.Entities.CourseTransferStatusesCategory.NotSet))
                    {
                        courseTransferStatusesCollection.Add(ConvertCourseTransferStatusesEntityToDto(courseTransferStatuses));
                    }
                }
            }
            return courseTransferStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CourseTransferStatuses from its GUID
        /// </summary>
        /// <returns>CourseTransferStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CourseTransferStatuses> GetCourseTransferStatusesByGuidAsync(string guid)
        {
            try
            {
                var courseTransferStatus = (await _referenceDataRepository.GetStudentAcademicCreditStatusesAsync(true)).Where(r => r.Guid == guid).First();

                if (courseTransferStatus.Status.SectionRegistrationStatusReason == Colleague.Domain.Student.Entities.RegistrationStatusReason.Transfer)
                {
                    return ConvertCourseTransferStatusesEntityToDto(courseTransferStatus);
                }
                else
                {
                    throw new ArgumentException("course-transfer-statuses not found for GUID " + guid);
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("course-transfer-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("course-transfer-statuses not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentAcadCredStatuses domain entity to its corresponding CourseTransferStatuses DTO
        /// </summary>
        /// <param name="source">StudentAcadCredStatuses domain entity</param>
        /// <returns>CourseTransferStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.CourseTransferStatuses ConvertCourseTransferStatusesEntityToDto(SectionRegistrationStatusItem source)
        {
            var courseTransferStatuses = new Ellucian.Colleague.Dtos.CourseTransferStatuses();

            courseTransferStatuses.Id = source.Guid;
            courseTransferStatuses.Code = source.Code;
            courseTransferStatuses.Title = source.Description;
            courseTransferStatuses.Description = null;           
                                                                          
            courseTransferStatuses.Category= ConvertCourseTransferStatusesCategoryDomainEnumToCourseTransferStatusesCategoryDtoEnum(source.Category);
                        
            return courseTransferStatuses;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CourseTransferStatusesCategory domain enumeration value to its corresponding CourseTransferStatusesCategory DTO enumeration value
        /// </summary>
        /// <param name="source">CourseTransferStatusesCategory domain enumeration value</param>
        /// <returns>CourseTransferStatusesCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.CourseTransferStatusesCategory ConvertCourseTransferStatusesCategoryDomainEnumToCourseTransferStatusesCategoryDtoEnum(Ellucian.Colleague.Domain.Student.Entities.CourseTransferStatusesCategory source)
        {
            switch (source)
            {

                case Ellucian.Colleague.Domain.Student.Entities.CourseTransferStatusesCategory.Preliminary:
                    return Dtos.EnumProperties.CourseTransferStatusesCategory.Preliminary;
                case Ellucian.Colleague.Domain.Student.Entities.CourseTransferStatusesCategory.Approved:
                    return Dtos.EnumProperties.CourseTransferStatusesCategory.Approved;
                default:
                    return Dtos.EnumProperties.CourseTransferStatusesCategory.Preliminary;
            }  
		}

	}

}