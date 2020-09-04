//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class ExternalEmploymentPositionsService : BaseCoordinationService, IExternalEmploymentPositionsService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public ExternalEmploymentPositionsService(

            IReferenceDataRepository referenceDataRepository,
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
        /// Gets all external-employment-positions
        /// </summary>
        /// <returns>Collection of ExternalEmploymentPositions DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmploymentPositions>> GetExternalEmploymentPositionsAsync(bool bypassCache = false)
        {
            var externalEmploymentPositionsCollection = new List<Ellucian.Colleague.Dtos.ExternalEmploymentPositions>();

            var externalEmploymentPositionsEntities = await _referenceDataRepository.GetPositionsAsync(bypassCache);
            if (externalEmploymentPositionsEntities != null && externalEmploymentPositionsEntities.Any())
            {
                foreach (var externalEmploymentPositions in externalEmploymentPositionsEntities)
                {
                    externalEmploymentPositionsCollection.Add(ConvertPositionsEntityToDto(externalEmploymentPositions));
                }
            }
            return externalEmploymentPositionsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ExternalEmploymentPositions from its GUID
        /// </summary>
        /// <returns>ExternalEmploymentPositions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ExternalEmploymentPositions> GetExternalEmploymentPositionsByGuidAsync(string guid)
        {
            try
            {
                return ConvertPositionsEntityToDto((await _referenceDataRepository.GetPositionsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("external-employment-positions not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("external-employment-positions not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ExternalEmploymentPositions domain entity to its corresponding ExternalEmploymentPositions DTO
        /// </summary>
        /// <param name="source">ExternalEmploymentPositions domain entity</param>
        /// <returns>ExternalEmploymentPositions DTO</returns>
        private Ellucian.Colleague.Dtos.ExternalEmploymentPositions ConvertPositionsEntityToDto(Positions source)
        {
            var externalEmploymentPositions = new Ellucian.Colleague.Dtos.ExternalEmploymentPositions();

            externalEmploymentPositions.Id = source.Guid;
            externalEmploymentPositions.Code = source.Code;
            externalEmploymentPositions.Title = source.Description;
            externalEmploymentPositions.Description = null;

            return externalEmploymentPositions;
        }
    }
}