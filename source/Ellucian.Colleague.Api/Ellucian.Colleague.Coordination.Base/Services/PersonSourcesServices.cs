//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonSourcesService : BaseCoordinationService, IPersonSourcesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public PersonSourcesService(

            IReferenceDataRepository referenceDataRepository,
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
        /// Gets all person-sources
        /// </summary>
        /// <returns>Collection of PersonSources DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonSources>> GetPersonSourcesAsync(bool bypassCache = false)
        {
            var personSourcesCollection = new List<Ellucian.Colleague.Dtos.PersonSources>();

            var personSourcesEntities = await _referenceDataRepository.GetPersonOriginCodesAsync(bypassCache);
            if (personSourcesEntities != null && personSourcesEntities.Any())
            {
                foreach (var personSources in personSourcesEntities)
                {
                    personSourcesCollection.Add(ConvertPersonSourcesEntityToDto(personSources));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return personSourcesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonSources from its GUID
        /// </summary>
        /// <returns>PersonSources DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonSources> GetPersonSourcesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertPersonSourcesEntityToDto((await _referenceDataRepository.GetPersonOriginCodesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No person-sources was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No person-sources was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PersonOriginCodes domain entity to its corresponding PersonSources DTO
        /// </summary>
        /// <param name="source">PersonOriginCodes domain entity</param>
        /// <returns>PersonSources DTO</returns>
        private Ellucian.Colleague.Dtos.PersonSources ConvertPersonSourcesEntityToDto(PersonOriginCodes source)
        {
            var personSources = new Ellucian.Colleague.Dtos.PersonSources();

            personSources.Id = source.Guid;
            personSources.Code = source.Code;
            personSources.Title = source.Description;
            personSources.Description = null;

            return personSources;
        }
    }
}
