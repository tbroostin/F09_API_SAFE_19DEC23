// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Services for Intructional Platform objects.
    /// </summary>
    [RegisterType]
    public class InstructionalPlatformService : BaseCoordinationService, IInstructionalPlatformService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
       
        #region Constructors

        public InstructionalPlatformService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        #endregion

        #region IInstructionalPlatformService

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all instructional platforms
        /// </summary>
        /// <returns>Collection of InstructionalPlatform DTO objects</returns>
        public async Task<IEnumerable<Dtos.InstructionalPlatform>> GetInstructionalPlatformsAsync(bool bypassCache = false)
        {
            var instructionalPlatformCollection = new List<Ellucian.Colleague.Dtos.InstructionalPlatform>();

            var instructionalPlatformEntities = await  _referenceDataRepository.GetInstructionalPlatformsAsync(bypassCache);
            if (instructionalPlatformEntities != null && instructionalPlatformEntities.Any())
            {
                foreach (var instructionalPlatform in instructionalPlatformEntities)
                {
                    instructionalPlatformCollection.Add(ConvertInstructionalPlatformEntityToInstructionalPlatformDto(instructionalPlatform));
                }
            }

            return instructionalPlatformCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets an Instructional Platform by Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns>single Instructional Platform by Guid</returns>
        public async Task<Dtos.InstructionalPlatform> GetInstructionalPlatformByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return ConvertInstructionalPlatformEntityToInstructionalPlatformDto((await _referenceDataRepository.GetInstructionalPlatformsAsync(bypassCache)).First(rt => rt.Guid == guid));
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Instructional Platform not found for GUID " + guid, ex);
            }
        }

        #endregion

        #region Conversions

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Takes an InstructionalPlatform Entity and converts it into an InstructionalPlatform DTO
        /// </summary>
        /// <param name="source">Instructional Platform Entity</param>
        /// <returns>Instructional Platform DTO</returns>
        private Dtos.InstructionalPlatform ConvertInstructionalPlatformEntityToInstructionalPlatformDto(InstructionalPlatform source)
        {
            return new Dtos.InstructionalPlatform{Code = source.Code, Id = source.Guid, Description = null, Title = source.Description};
        }

        #endregion
    }
}
