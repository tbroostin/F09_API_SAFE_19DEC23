//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class RoommateCharacteristicsService : BaseCoordinationService, IRoommateCharacteristicsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public RoommateCharacteristicsService(
            IAdapterRegistry adapterRegistry,
            IStudentReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory,
            IConfigurationRepository configurationRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

    /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
    /// <summary>
    /// Gets all roommate-characteristics
    /// </summary>
    /// <returns>Collection of RoommateCharacteristics DTO objects</returns>
    public async Task<IEnumerable<Ellucian.Colleague.Dtos.RoommateCharacteristics>> GetRoommateCharacteristicsAsync(bool bypassCache = false)
        {
            var roommateCharacteristicsCollection = new List<Ellucian.Colleague.Dtos.RoommateCharacteristics>();

            var roommateCharacteristicsEntities = await _referenceDataRepository.GetRoommateCharacteristicsAsync(bypassCache);
            if (roommateCharacteristicsEntities != null && roommateCharacteristicsEntities.Any())
            {
                foreach (var roommateCharacteristics in roommateCharacteristicsEntities)
                {
                    roommateCharacteristicsCollection.Add(ConvertRoommateCharacteristicsEntityToDto(roommateCharacteristics));
                }
            }
            return roommateCharacteristicsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a RoommateCharacteristics from its GUID
        /// </summary>
        /// <returns>RoommateCharacteristics DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RoommateCharacteristics> GetRoommateCharacteristicsByGuidAsync(string guid)
        {
            try
            {
                return ConvertRoommateCharacteristicsEntityToDto((await _referenceDataRepository.GetRoommateCharacteristicsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("roommate-characteristics not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("roommate-characteristics not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a RoommateCharacteristics domain entity to its corresponding RoommateCharacteristics DTO
        /// </summary>
        /// <param name="source">RoommateCharacteristics domain entity</param>
        /// <returns>RoommateCharacteristics DTO</returns>
        private Ellucian.Colleague.Dtos.RoommateCharacteristics ConvertRoommateCharacteristicsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.RoommateCharacteristics source)
        {
            var roommateCharacteristics = new Ellucian.Colleague.Dtos.RoommateCharacteristics();

            roommateCharacteristics.Id = source.Guid;
            roommateCharacteristics.Code = source.Code;
            roommateCharacteristics.Title = source.Description;
            roommateCharacteristics.Description = null;

            return roommateCharacteristics;
        }


    }
}
