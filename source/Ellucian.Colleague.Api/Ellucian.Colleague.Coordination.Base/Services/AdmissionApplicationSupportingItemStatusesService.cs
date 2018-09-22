//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
    public class AdmissionApplicationSupportingItemStatusesService : BaseCoordinationService, IAdmissionApplicationSupportingItemStatusesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public AdmissionApplicationSupportingItemStatusesService(

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
        /// Gets all admission-application-supporting-item-statuses
        /// </summary>
        /// <returns>Collection of AdmissionApplicationSupportingItemStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus>> GetAdmissionApplicationSupportingItemStatusesAsync(bool bypassCache = false)
        {
            var admissionApplicationSupportingItemStatusesCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus>();

            var admissionApplicationSupportingItemStatusesEntities = await _referenceDataRepository.GetCorrStatusesAsync(bypassCache);
            if (admissionApplicationSupportingItemStatusesEntities != null && admissionApplicationSupportingItemStatusesEntities.Any())
            {
                foreach (var admissionApplicationSupportingItemStatuses in admissionApplicationSupportingItemStatusesEntities)
                {
                    admissionApplicationSupportingItemStatusesCollection.Add(ConvertAdmissionApplicationSupportingItemStatusesEntityToDto(admissionApplicationSupportingItemStatuses));
                }
            }
            return admissionApplicationSupportingItemStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AdmissionApplicationSupportingItemStatuses from its GUID
        /// </summary>
        /// <returns>AdmissionApplicationSupportingItemStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus> GetAdmissionApplicationSupportingItemStatusByGuidAsync(string guid)
        {
            try
            {
                return ConvertAdmissionApplicationSupportingItemStatusesEntityToDto((await _referenceDataRepository.GetCorrStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("admission-application-supporting-item-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("admission-application-supporting-item-statuses not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CorrStatuses domain entity to its corresponding AdmissionApplicationSupportingItemStatuses DTO
        /// </summary>
        /// <param name="source">CorrStatuses domain entity</param>
        /// <returns>AdmissionApplicationSupportingItemStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus ConvertAdmissionApplicationSupportingItemStatusesEntityToDto(CorrStatus source)
        {
            var admissionApplicationSupportingItemStatuses = new Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus();

            admissionApplicationSupportingItemStatuses.Id = source.Guid;
            admissionApplicationSupportingItemStatuses.Code = source.Code;
            admissionApplicationSupportingItemStatuses.Title = source.Description;
            admissionApplicationSupportingItemStatuses.Description = null;

            admissionApplicationSupportingItemStatuses.Type = ConvertAdmissionApplicationSupportingItemStatusesTypeDomainEnumToAdmissionApplicationSupportingItemStatusesTypeDtoEnum(source.Description);

            return admissionApplicationSupportingItemStatuses;
        }



        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdmissionApplicationSupportingItemStatusesType domain enumeration value to its corresponding AdmissionApplicationSupportingItemStatusesType DTO enumeration value
        /// </summary>
        /// <param name="source">AdmissionApplicationSupportingItemStatusesType domain enumeration value</param>
        /// <returns>AdmissionApplicationSupportingItemStatusesType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.AdmissionApplicationSupportingItemStatusType ConvertAdmissionApplicationSupportingItemStatusesTypeDomainEnumToAdmissionApplicationSupportingItemStatusesTypeDtoEnum(string source)
        {
            switch (source)
            {

                case "Incomplete":
                    return Dtos.EnumProperties.AdmissionApplicationSupportingItemStatusType.Incomplete;
                case "Received":
                    return Dtos.EnumProperties.AdmissionApplicationSupportingItemStatusType.Received;
                case "Waived":
                    return Dtos.EnumProperties.AdmissionApplicationSupportingItemStatusType.Waived;
                default:
                    return Dtos.EnumProperties.AdmissionApplicationSupportingItemStatusType.Incomplete;
            }
        }
    }
}