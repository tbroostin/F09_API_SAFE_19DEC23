//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Adapters;
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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class InstructorStaffTypesService : StudentCoordinationService, IInstructorStaffTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public InstructorStaffTypesService(

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
        /// Gets all instructor-staff-types
        /// </summary>
        /// <returns>Collection of InstructorStaffTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorStaffTypes>> GetInstructorStaffTypesAsync(bool bypassCache = false)
        {
            var instructorStaffTypesCollection = new List<Ellucian.Colleague.Dtos.InstructorStaffTypes>();

            var instructorStaffTypesEntities = await _referenceDataRepository.GetFacultyContractTypesAsync(bypassCache);
            if (instructorStaffTypesEntities != null && instructorStaffTypesEntities.Any())
            {
                foreach (var instructorStaffTypes in instructorStaffTypesEntities)
                {
                    instructorStaffTypesCollection.Add(ConvertInstructorStaffTypesEntityToDto(instructorStaffTypes));
                }
            }
            return instructorStaffTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a InstructorStaffTypes from its GUID
        /// </summary>
        /// <returns>InstructorStaffTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.InstructorStaffTypes> GetInstructorStaffTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertInstructorStaffTypesEntityToDto((await _referenceDataRepository.GetFacultyContractTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("instructor-staff-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("instructor-staff-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstructorStaffTypes domain entity to its corresponding InstructorStaffTypes DTO
        /// </summary>
        /// <param name="source">InstructorStaffTypes domain entity</param>
        /// <returns>FacultyContractTypes DTO</returns>
        private Ellucian.Colleague.Dtos.InstructorStaffTypes ConvertInstructorStaffTypesEntityToDto(FacultyContractTypes source)
        {
            var instructorStaffTypes = new Ellucian.Colleague.Dtos.InstructorStaffTypes();

            instructorStaffTypes.Id = source.Guid;
            instructorStaffTypes.Code = source.Code;
            instructorStaffTypes.Title = source.Description;
            instructorStaffTypes.Description = null;

            return instructorStaffTypes;
        }
    }
}