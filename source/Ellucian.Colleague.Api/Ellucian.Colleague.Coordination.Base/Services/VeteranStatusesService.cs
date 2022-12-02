//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class VeteranStatusesService : BaseCoordinationService, IVeteranStatusesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public VeteranStatusesService(

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
        /// Gets all veteran-statuses
        /// </summary>
        /// <returns>Collection of VeteranStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.VeteranStatuses>> GetVeteranStatusesAsync(bool bypassCache = false)
        {
            var veteranStatusesCollection = new List<Ellucian.Colleague.Dtos.VeteranStatuses>();

            var veteranStatusesEntities = await _referenceDataRepository.GetMilStatusesAsync(bypassCache);
            if (veteranStatusesEntities != null && veteranStatusesEntities.Any())
            {
                foreach (var veteranStatuses in veteranStatusesEntities)
                {
                    var veteranStatus = ConvertMilStatusesEntityToVeteranStatusesDto(veteranStatuses);
                    if (veteranStatus != null)
                    {
                        veteranStatusesCollection.Add(veteranStatus);
                    }
                }
            }
            if ((veteranStatusesEntities != null) && (veteranStatusesEntities.Any()) && (!veteranStatusesCollection.Any()))
            {
                throw new ColleagueWebApiException("An error has occurred.  Veteran Status categories must be mapped on CDHP ");
            }
            return veteranStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a VeteranStatuses from its GUID
        /// </summary>
        /// <returns>VeteranStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.VeteranStatuses> GetVeteranStatusesByGuidAsync(string guid)
        {
            try
            {
                return ConvertMilStatusesEntityToVeteranStatusesDto((await _referenceDataRepository.GetMilStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("veteran-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("veteran-statuses not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MilStatuses domain entity to its corresponding VeteranStatuses DTO
        /// </summary>
        /// <param name="source">MilStatuses domain entity</param>
        /// <returns>VeteranStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.VeteranStatuses ConvertMilStatusesEntityToVeteranStatusesDto(MilStatuses source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("MilStatuses is a required field");
            }

            if (source.Category == null)
            {
                throw new ColleagueWebApiException("An error has occurred.  Veteran Status categories must be mapped on CDHP ");
            }
            var veteranStatuses = new Ellucian.Colleague.Dtos.VeteranStatuses();

            veteranStatuses.Id = source.Guid;
            veteranStatuses.Code = source.Code;
            veteranStatuses.Title = source.Description;
            veteranStatuses.Description = null;
           veteranStatuses.Category = ConvertVeteranStatusCategoryDomainEnumToDtoEnum(source.Category);
            
            return veteranStatuses;
        }



        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a VeteranStatusCategory domain enumeration value to its corresponding VeteranStatusesCategory DTO enumeration value
        /// </summary>
        /// <param name="source">VeteranStatusCategory domain enumeration value</param>
        /// <returns>VeteranStatusesCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.VeteranStatusesCategory ConvertVeteranStatusCategoryDomainEnumToDtoEnum(Ellucian.Colleague.Domain.Base.Entities.VeteranStatusCategory? source)
        {
            switch (source)
            {
                case Ellucian.Colleague.Domain.Base.Entities.VeteranStatusCategory.Nonveteran:
                    return Dtos.EnumProperties.VeteranStatusesCategory.Nonveteran;
                case Ellucian.Colleague.Domain.Base.Entities.VeteranStatusCategory.Activeduty:
                    return Dtos.EnumProperties.VeteranStatusesCategory.Activeduty;
                case Ellucian.Colleague.Domain.Base.Entities.VeteranStatusCategory.Protectedveteran:
                    return Dtos.EnumProperties.VeteranStatusesCategory.Protectedveteran;
                case Ellucian.Colleague.Domain.Base.Entities.VeteranStatusCategory.Nonprotectedveteran:
                    return Dtos.EnumProperties.VeteranStatusesCategory.Nonprotectedveteran;
                default:
                    return Dtos.EnumProperties.VeteranStatusesCategory.NotSet;
            }
        }
    }
}