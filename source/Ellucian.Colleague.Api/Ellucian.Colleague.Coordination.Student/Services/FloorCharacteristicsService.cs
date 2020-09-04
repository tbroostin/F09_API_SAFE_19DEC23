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
    public class FloorCharacteristicsService : BaseCoordinationService, IFloorCharacteristicsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public FloorCharacteristicsService(

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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all floor-characteristics
        /// </summary>
        /// <returns>Collection of FloorCharacteristics DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FloorCharacteristics>> GetFloorCharacteristicsAsync(bool bypassCache = false)
        {
            var floorCharacteristicsCollection = new List<Ellucian.Colleague.Dtos.FloorCharacteristics>();

            var floorCharacteristicsEntities = await _referenceDataRepository.GetFloorCharacteristicsAsync(bypassCache);
            if (floorCharacteristicsEntities != null && floorCharacteristicsEntities.Any())
            {
                foreach (var floorCharacteristics in floorCharacteristicsEntities)
                {
                    floorCharacteristicsCollection.Add(ConvertFloorCharacteristicsEntityToDto(floorCharacteristics));
                }
            }
            return floorCharacteristicsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FloorCharacteristics from its GUID
        /// </summary>
        /// <returns>FloorCharacteristics DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FloorCharacteristics> GetFloorCharacteristicsByGuidAsync(string guid)
        {
            try
            {
                return ConvertFloorCharacteristicsEntityToDto((await _referenceDataRepository.GetFloorCharacteristicsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("floor-characteristics not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("floor-characteristics not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FloorCharacteristics domain entity to its corresponding FloorCharacteristics DTO
        /// </summary>
        /// <param name="source">FloorCharacteristics domain entity</param>
        /// <returns>FloorCharacteristics DTO</returns>
        private Ellucian.Colleague.Dtos.FloorCharacteristics ConvertFloorCharacteristicsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics source)
        {
            var floorCharacteristics = new Ellucian.Colleague.Dtos.FloorCharacteristics();

            floorCharacteristics.Id = source.Guid;
            floorCharacteristics.Code = source.Code;
            floorCharacteristics.Title = source.Description;
            floorCharacteristics.Description = null;           
                                                                        
            return floorCharacteristics;
        }

      
    }
   
}