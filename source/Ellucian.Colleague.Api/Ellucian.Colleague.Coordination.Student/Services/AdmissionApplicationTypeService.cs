// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Coordination.Student.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionApplicationTypeService : StudentCoordinationService, IAdmissionApplicationTypesService
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public AdmissionApplicationTypeService(
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
        /// Gets all admission application types
        /// </summary>
        /// <returns>Collection of AdmissionApplicationTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationTypes>> GetAdmissionApplicationTypesAsync(bool bypassCache = false)
        {
            var admissionApplicationTypesCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplicationTypes>();

            var admissionApplicationTypesEntities = await _referenceDataRepository.GetAdmissionApplicationTypesAsync(bypassCache);
            if (admissionApplicationTypesEntities != null && admissionApplicationTypesEntities.Any())
            {
                foreach (var admissionApplicationTypes in admissionApplicationTypesEntities)
                {
                    admissionApplicationTypesCollection.Add(ConvertAdmissionApplicationTypesEntityToDto(admissionApplicationTypes));
                }
            }
            return admissionApplicationTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6.0</remarks>
        /// <summary>
        /// Get a AdmissionApplicationTypes from its GUID
        /// </summary>
        /// <returns>AdmissionApplicationTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationTypes> GetAdmissionApplicationTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertAdmissionApplicationTypesEntityToDto((await _referenceDataRepository.GetAdmissionApplicationTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("admission-application-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionApplicationTypes domain entity to its corresponding AdmissionApplicationTypes DTO
        /// </summary>
        /// <param name="source">AdmissionApplicationTypes domain entity</param>
        /// <returns>AdmissionApplicationTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionApplicationTypes ConvertAdmissionApplicationTypesEntityToDto(Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationType source)
        {
            var admissionApplicationTypes = new Ellucian.Colleague.Dtos.AdmissionApplicationTypes();

            admissionApplicationTypes.Id = source.Guid;
            admissionApplicationTypes.Code = source.Code;
            admissionApplicationTypes.Title = source.Description;
            admissionApplicationTypes.Description = null;

            return admissionApplicationTypes;
        }


    }
}
