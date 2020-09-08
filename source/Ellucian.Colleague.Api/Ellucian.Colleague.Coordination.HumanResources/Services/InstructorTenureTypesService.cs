
//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class InstructorTenureTypesService : BaseCoordinationService, IInstructorTenureTypesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public InstructorTenureTypesService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IConfigurationRepository configurationRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all instructor-tenure-types
        /// </summary>
        /// <returns>Collection of InstructorTenureTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructorTenureTypes>> GetInstructorTenureTypesAsync(bool bypassCache = false)
        {
            var instructorTenureTypesCollection = new List<Ellucian.Colleague.Dtos.InstructorTenureTypes>();

            var instructorTenureTypesEntities = await _referenceDataRepository.GetTenureTypesAsync(bypassCache);
            if (instructorTenureTypesEntities != null && instructorTenureTypesEntities.Any())
            {
                foreach (var instructorTenureTypes in instructorTenureTypesEntities)
                {
                    instructorTenureTypesCollection.Add(ConvertInstructorTenureTypesEntityToDto(instructorTenureTypes));
                }
            }
            return instructorTenureTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a InstructorTenureTypes from its GUID
        /// </summary>
        /// <returns>InstructorTenureTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.InstructorTenureTypes> GetInstructorTenureTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertInstructorTenureTypesEntityToDto((await _referenceDataRepository.GetTenureTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("instructor-tenure-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("instructor-tenure-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InstructorTenureTypes domain entity to its corresponding InstructorTenureTypes DTO
        /// </summary>
        /// <param name="source">InstructorTenureTypes domain entity</param>
        /// <returns>InstructorTenureTypes DTO</returns>
        private Ellucian.Colleague.Dtos.InstructorTenureTypes ConvertInstructorTenureTypesEntityToDto(TenureTypes source)
        {
            var instructorTenureTypes = new Ellucian.Colleague.Dtos.InstructorTenureTypes();

            instructorTenureTypes.Id = source.Guid;
            instructorTenureTypes.Code = source.Code;
            instructorTenureTypes.Title = source.Description;
            instructorTenureTypes.Description = null;

            return instructorTenureTypes;
        }


    }
}
