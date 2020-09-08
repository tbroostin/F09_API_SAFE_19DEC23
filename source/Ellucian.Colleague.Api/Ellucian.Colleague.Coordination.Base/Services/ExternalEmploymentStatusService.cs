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
    public class ExternalEmploymentStatusesService : BaseCoordinationService, IExternalEmploymentStatusesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public ExternalEmploymentStatusesService(

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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all external-employment-statuses
        /// </summary>
        /// <returns>Collection of ExternalEmploymentStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmploymentStatuses>> GetExternalEmploymentStatusesAsync(bool bypassCache = false)
        {
            var externalEmploymentStatusesCollection = new List<Ellucian.Colleague.Dtos.ExternalEmploymentStatuses>();

            var externalEmploymentStatusesEntities = await _referenceDataRepository.GetExternalEmploymentStatusesAsync(bypassCache);
            if (externalEmploymentStatusesEntities != null && externalEmploymentStatusesEntities.Any())
            {
                foreach (var externalEmploymentStatuses in externalEmploymentStatusesEntities)
                {
                    externalEmploymentStatusesCollection.Add(ConvertExternalEmploymentStatusesEntityToDto(externalEmploymentStatuses));
                }
            }
            return externalEmploymentStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a ExternalEmploymentStatuses from its GUID
        /// </summary>
        /// <returns>ExternalEmploymentStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ExternalEmploymentStatuses> GetExternalEmploymentStatusesByGuidAsync(string guid)
        {
            try
            {
                return ConvertExternalEmploymentStatusesEntityToDto((await _referenceDataRepository.GetExternalEmploymentStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("external-employment-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("external-employment-statuses not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a ExternalEmploymentStatuses domain entity to its corresponding ExternalEmploymentStatuses DTO
        /// </summary>
        /// <param name="source">ExternalEmploymentStatuses domain entity</param>
        /// <returns>ExternalEmploymentStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.ExternalEmploymentStatuses ConvertExternalEmploymentStatusesEntityToDto(ExternalEmploymentStatus source)
        {
            var externalEmploymentStatuses = new Ellucian.Colleague.Dtos.ExternalEmploymentStatuses();

            externalEmploymentStatuses.Id = source.Guid;
            externalEmploymentStatuses.Code = source.Code;
            externalEmploymentStatuses.Title = source.Description;
            externalEmploymentStatuses.Description = null;           
                                                                        
            return externalEmploymentStatuses;
        }

      
    }

}