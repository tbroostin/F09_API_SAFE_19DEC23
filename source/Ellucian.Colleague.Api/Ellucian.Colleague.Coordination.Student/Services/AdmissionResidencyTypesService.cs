//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
    public class AdmissionResidencyTypesService : BaseCoordinationService, IAdmissionResidencyTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public AdmissionResidencyTypesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IStaffRepository staffRepository,
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all admission-residency-types
        /// </summary>
        /// <returns>Collection of AdmissionResidencyTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionResidencyTypes>> GetAdmissionResidencyTypesAsync(bool bypassCache = false)
        {
            var admissionResidencyTypesCollection = new List<Ellucian.Colleague.Dtos.AdmissionResidencyTypes>();

            var admissionResidencyTypesEntities = await _referenceDataRepository.GetAdmissionResidencyTypesAsync(bypassCache);
            if (admissionResidencyTypesEntities != null && admissionResidencyTypesEntities.Any())
            {
                foreach (var admissionResidencyTypes in admissionResidencyTypesEntities)
                {
                    admissionResidencyTypesCollection.Add(ConvertAdmissionResidencyTypesEntityToDto(admissionResidencyTypes));
                }
            }
            return admissionResidencyTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a AdmissionResidencyTypes from its GUID
        /// </summary>
        /// <returns>AdmissionResidencyTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionResidencyTypes> GetAdmissionResidencyTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertAdmissionResidencyTypesEntityToDto((await _referenceDataRepository.GetAdmissionResidencyTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("admission-residency-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("admission-residency-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionResidencyTypes domain entity to its corresponding AdmissionResidencyTypes DTO
        /// </summary>
        /// <param name="source">AdmissionResidencyTypes domain entity</param>
        /// <returns>AdmissionResidencyTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionResidencyTypes ConvertAdmissionResidencyTypesEntityToDto(AdmissionResidencyType source)
        {
            var admissionResidencyTypes = new Ellucian.Colleague.Dtos.AdmissionResidencyTypes();

            admissionResidencyTypes.Id = source.Guid;
            admissionResidencyTypes.Code = null;
            admissionResidencyTypes.Title = source.Description;
            admissionResidencyTypes.Description = null;           
                                                            
            return admissionResidencyTypes;
        }

      
    }
}