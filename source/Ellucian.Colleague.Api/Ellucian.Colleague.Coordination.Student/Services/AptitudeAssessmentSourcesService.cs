//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class AptitudeAssessmentSourcesService : BaseCoordinationService, IAptitudeAssessmentSourcesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public AptitudeAssessmentSourcesService(

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
        /// Gets all aptitude-assessment-sources
        /// </summary>
        /// <returns>Collection of AptitudeAssessmentSources DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessmentSources>> GetAptitudeAssessmentSourcesAsync(bool bypassCache = false)
        {
            var aptitudeAssessmentSourcesCollection = new List<Ellucian.Colleague.Dtos.AptitudeAssessmentSources>();

            var aptitudeAssessmentSourcesEntities = await _referenceDataRepository.GetTestSourcesAsync(bypassCache);
            if (aptitudeAssessmentSourcesEntities != null && aptitudeAssessmentSourcesEntities.Any())
            {
                foreach (var aptitudeAssessmentSources in aptitudeAssessmentSourcesEntities)
                {
                    aptitudeAssessmentSourcesCollection.Add(ConvertAptitudeAssessmentSourcesEntityToDto(aptitudeAssessmentSources));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return aptitudeAssessmentSourcesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AptitudeAssessmentSources from its GUID
        /// </summary>
        /// <returns>AptitudeAssessmentSources DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AptitudeAssessmentSources> GetAptitudeAssessmentSourcesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertAptitudeAssessmentSourcesEntityToDto((await _referenceDataRepository.GetTestSourcesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No aptitude-assessment-sources was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No aptitude-assessment-sources was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ApplTestSources domain entity to its corresponding AptitudeAssessmentSources DTO
        /// </summary>
        /// <param name="source">ApplTestSources domain entity</param>
        /// <returns>AptitudeAssessmentSources DTO</returns>
        private Ellucian.Colleague.Dtos.AptitudeAssessmentSources ConvertAptitudeAssessmentSourcesEntityToDto(TestSource source)
        {
            var aptitudeAssessmentSources = new Ellucian.Colleague.Dtos.AptitudeAssessmentSources();

            aptitudeAssessmentSources.Id = source.Guid;
            aptitudeAssessmentSources.Code = source.Code;
            aptitudeAssessmentSources.Title = source.Description;
            aptitudeAssessmentSources.Description = null;

            return aptitudeAssessmentSources;
        }
    }
}