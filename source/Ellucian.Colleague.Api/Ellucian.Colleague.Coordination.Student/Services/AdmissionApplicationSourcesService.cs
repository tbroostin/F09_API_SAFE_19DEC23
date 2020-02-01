//Copyright 2019 Ellucian Company L.P. and its affiliates.
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
    public class AdmissionApplicationSourcesService : BaseCoordinationService, IAdmissionApplicationSourcesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public AdmissionApplicationSourcesService(

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
        /// Gets all admission-application-sources
        /// </summary>
        /// <returns>Collection of AdmissionApplicationSources DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSources>> GetAdmissionApplicationSourcesAsync(bool bypassCache = false)
        {
            var admissionApplicationSourcesCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplicationSources>();

            var admissionApplicationSourcesEntities = await _referenceDataRepository.GetApplicationSourcesAsync(bypassCache);
            if (admissionApplicationSourcesEntities != null && admissionApplicationSourcesEntities.Any())
            {
                foreach (var admissionApplicationSources in admissionApplicationSourcesEntities)
                {
                    admissionApplicationSourcesCollection.Add(ConvertAdmissionApplicationSourcesEntityToDto(admissionApplicationSources));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return admissionApplicationSourcesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AdmissionApplicationSources from its GUID
        /// </summary>
        /// <returns>AdmissionApplicationSources DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationSources> GetAdmissionApplicationSourcesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertAdmissionApplicationSourcesEntityToDto((await _referenceDataRepository.GetApplicationSourcesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission-application-sources was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission-application-sources was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ApplicationSources domain entity to its corresponding AdmissionApplicationSources DTO
        /// </summary>
        /// <param name="source">ApplicationSources domain entity</param>
        /// <returns>AdmissionApplicationSources DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionApplicationSources ConvertAdmissionApplicationSourcesEntityToDto(ApplicationSource source)
        {
            var admissionApplicationSources = new Ellucian.Colleague.Dtos.AdmissionApplicationSources();

            admissionApplicationSources.Id = source.Guid;
            admissionApplicationSources.Code = source.Code;
            admissionApplicationSources.Title = source.Description;
            admissionApplicationSources.Description = null;

            return admissionApplicationSources;
        }


    }
}
