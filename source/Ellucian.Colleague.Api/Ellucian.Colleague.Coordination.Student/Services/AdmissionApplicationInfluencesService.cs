//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionApplicationInfluencesService : BaseCoordinationService, IAdmissionApplicationInfluencesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public AdmissionApplicationInfluencesService(

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
        /// Gets all admission-application-influences
        /// </summary>
        /// <returns>Collection of AdmissionApplicationInfluences DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationInfluences>> GetAdmissionApplicationInfluencesAsync(bool bypassCache = false)
        {
            var admissionApplicationInfluencesCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplicationInfluences>();

            var admissionApplicationInfluencesEntities = await _referenceDataRepository.GetApplicationInfluencesAsync(bypassCache);
            if (admissionApplicationInfluencesEntities != null && admissionApplicationInfluencesEntities.Any())
            {
                foreach (var admissionApplicationInfluences in admissionApplicationInfluencesEntities)
                {
                    admissionApplicationInfluencesCollection.Add(ConvertAdmissionApplicationInfluencesEntityToDto(admissionApplicationInfluences));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return admissionApplicationInfluencesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AdmissionApplicationInfluences from its GUID
        /// </summary>
        /// <returns>AdmissionApplicationInfluences DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationInfluences> GetAdmissionApplicationInfluencesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertAdmissionApplicationInfluencesEntityToDto((await _referenceDataRepository.GetApplicationInfluencesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission application influence was found for GUID '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No admission application influence was found for GUID '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ApplInfluences domain entity to its corresponding AdmissionApplicationInfluences DTO
        /// </summary>
        /// <param name="source">ApplInfluences domain entity</param>
        /// <returns>AdmissionApplicationInfluences DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionApplicationInfluences ConvertAdmissionApplicationInfluencesEntityToDto(ApplicationInfluence source)
        {
            var admissionApplicationInfluences = new Ellucian.Colleague.Dtos.AdmissionApplicationInfluences();

            admissionApplicationInfluences.Id = source.Guid;
            admissionApplicationInfluences.Code = source.Code;
            admissionApplicationInfluences.Title = source.Description;
            admissionApplicationInfluences.Description = null;

            return admissionApplicationInfluences;
        }
    }
}