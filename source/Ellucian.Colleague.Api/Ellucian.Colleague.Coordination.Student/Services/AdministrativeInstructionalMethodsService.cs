//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdministrativeInstructionalMethodsService : BaseCoordinationService, IAdministrativeInstructionalMethodsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public AdministrativeInstructionalMethodsService(

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
        /// Gets all administrative-instructional-methods
        /// </summary>
        /// <returns>Collection of AdministrativeInstructionalMethods DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods>> GetAdministrativeInstructionalMethodsAsync(bool bypassCache = false)
        {
            var administrativeInstructionalMethodsCollection = new List<Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods>();

            var administrativeInstructionalMethodsEntities = await _referenceDataRepository.GetAdministrativeInstructionalMethodsAsync(bypassCache);
            if (administrativeInstructionalMethodsEntities != null && administrativeInstructionalMethodsEntities.Any())
            {
                foreach (var administrativeInstructionalMethods in administrativeInstructionalMethodsEntities)
                {
                    administrativeInstructionalMethodsCollection.Add(ConvertAdministrativeInstructionalMethodsEntityToDto(administrativeInstructionalMethods));
                }
            }
            return administrativeInstructionalMethodsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AdministrativeInstructionalMethods from its GUID
        /// </summary>
        /// <returns>AdministrativeInstructionalMethods DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods> GetAdministrativeInstructionalMethodsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertAdministrativeInstructionalMethodsEntityToDto((await _referenceDataRepository.GetAdministrativeInstructionalMethodsAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("administrative-instructional-methods not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("administrative-instructional-methods not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Saptype domain entity to its corresponding AdministrativeInstructionalMethods DTO
        /// </summary>
        /// <param name="source">Saptype domain entity</param>
        /// <returns>AdministrativeInstructionalMethods DTO</returns>
        private Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods ConvertAdministrativeInstructionalMethodsEntityToDto(Domain.Student.Entities.AdministrativeInstructionalMethod source)
        {
            var administrativeInstructionalMethods = new Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods();

            administrativeInstructionalMethods.Id = source.Guid;
            administrativeInstructionalMethods.InstructionalMethod = new Dtos.GuidObject2(source.InstructionalMethodGuid);
            administrativeInstructionalMethods.Code = source.Code;
            administrativeInstructionalMethods.Title = source.Description;
            administrativeInstructionalMethods.Description = null;

            return administrativeInstructionalMethods;
        }
    }
}