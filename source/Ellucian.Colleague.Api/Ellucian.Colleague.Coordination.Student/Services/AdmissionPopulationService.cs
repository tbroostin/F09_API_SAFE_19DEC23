// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    public class AdmissionPopulationService : BaseCoordinationService, IAdmissionPopulationsService
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public AdmissionPopulationService(
            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6.0</remarks>
        /// <summary>
        /// Gets all admission populations
        /// </summary>
        /// <returns>Collection of AdmissionPopulations DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionPopulations>> GetAdmissionPopulationsAsync(bool bypassCache = false)
        {
            var admissionPopulationsCollection = new List<Ellucian.Colleague.Dtos.AdmissionPopulations>();

            var admissionPopulationsEntities = await _referenceDataRepository.GetAdmissionPopulationsAsync(bypassCache);
            if (admissionPopulationsEntities != null && admissionPopulationsEntities.Any())
            {
                foreach (var admissionPopulations in admissionPopulationsEntities)
                {
                    admissionPopulationsCollection.Add(ConvertAdmissionPopulationsEntityToDto(admissionPopulations));
                }
            }
            return admissionPopulationsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6.0</remarks>
        /// <summary>
        /// Get a AdmissionPopulations from its GUID
        /// </summary>
        /// <returns>AdmissionPopulations DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionPopulations> GetAdmissionPopulationsByGuidAsync(string guid)
        {
            try
            {
                return ConvertAdmissionPopulationsEntityToDto((await _referenceDataRepository.GetAdmissionPopulationsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("admission-populations not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionPopulations domain entity to its corresponding AdmissionPopulations DTO
        /// </summary>
        /// <param name="source">AdmissionPopulations domain entity</param>
        /// <returns>AdmissionPopulations DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionPopulations ConvertAdmissionPopulationsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.AdmissionPopulation source)
        {
            var admissionPopulations = new Ellucian.Colleague.Dtos.AdmissionPopulations();

            admissionPopulations.Id = source.Guid;
            admissionPopulations.Code = source.Code;
            admissionPopulations.Title = source.Description;
            admissionPopulations.Description = null;

            return admissionPopulations;
        }


    }
}
