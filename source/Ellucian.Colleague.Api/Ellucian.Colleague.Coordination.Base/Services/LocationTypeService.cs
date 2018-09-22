// Copyright 2015 Ellucian Company L.P. and its affiliates.
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


namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class LocationTypeService : BaseCoordinationService, ILocationTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private const string _dataOrigin = "Colleague";

        public LocationTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all location types
        /// </summary>
        /// <returns>Collection of LocationType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.LocationTypeItem>> GetLocationTypesAsync(bool bypassCache = false)
        {
            var locationTypeCollection = new List<Ellucian.Colleague.Dtos.LocationTypeItem>();

            var locationEntities = await _referenceDataRepository.GetLocationTypesAsync(bypassCache);
            if (locationEntities != null && locationEntities.Count() > 0)
            {
                foreach (var location in locationEntities)
                {
                    locationTypeCollection.Add(ConvertLocationTypeEntityToLocationTypeDto(location));
                }
            }
            return locationTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a location type from its GUID
        /// </summary>
        /// <returns>LocationType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.LocationTypeItem> GetLocationTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertLocationTypeEntityToLocationTypeDto((await _referenceDataRepository.GetLocationTypesAsync(true)).Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Location Type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a LocationType domain entity to its corresponding LocationType DTO
        /// </summary>
        /// <param name="source">LocationType domain entity</param>
        /// <returns>LocationType DTO</returns>
        private Dtos.LocationTypeItem ConvertLocationTypeEntityToLocationTypeDto(LocationTypeItem source)
        {
            var locationType = new Dtos.LocationTypeItem();
            locationType.Id = source.Guid;
            locationType.Code = source.Code;
            locationType.Title = source.Description;
            locationType.Description = null;
            locationType.Type = new Dtos.LocationType();

            if (source.Type.EntityType == EntityType.Person)
            {
                
                locationType.Type.PersonLocationType = ConvertPersonLocationTypeDomainEnumToPersonLocationTypeDtoEnum(source.Type.PersonLocationType);
            }
            else
            {
                locationType.Type.OrganizationLocationType = ConvertOrganizationLocationTypeDomainEnumToOrganizationLocationTypeDtoEnum(source.Type.OrganizationLocationType);
            }
            return locationType;
        }



        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonLocationType domain enumeration value to its corresponding PersonLocationType DTO enumeration value
        /// </summary>
        /// <param name="source">PersonLocationType domain enumeration value</param>
        /// <returns>PersonLocationType DTO enumeration value</returns>
        private Dtos.PersonLocationType ConvertPersonLocationTypeDomainEnumToPersonLocationTypeDtoEnum(PersonLocationType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.PersonLocationType.Billing:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Billing };
                case Domain.Base.Entities.PersonLocationType.Home:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Home};
                case Domain.Base.Entities.PersonLocationType.Business:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Business };
                case Domain.Base.Entities.PersonLocationType.Mailing:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Mailing };
                case Domain.Base.Entities.PersonLocationType.School:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.School};
                case Domain.Base.Entities.PersonLocationType.Shipping:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Shipping};
                case Domain.Base.Entities.PersonLocationType.Vacation:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Vacation};
                default:
                    return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Other };
               
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a OrganizationLocationType domain enumeration value to its corresponding OrganizationLocationType DTO enumeration value
        /// </summary>
        /// <param name="source">OrganizationLocationType domain enumeration value</param>
        /// <returns>OrganizationLocationType DTO enumeration value</returns>
        private Dtos.OrganizationLocationType ConvertOrganizationLocationTypeDomainEnumToOrganizationLocationTypeDtoEnum(OrganizationLocationType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.OrganizationLocationType.Branch:
                    return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Branch};
                case Domain.Base.Entities.OrganizationLocationType.Business:
                    return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Business };
                case Domain.Base.Entities.OrganizationLocationType.Main:
                    return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Main };
                case Domain.Base.Entities.OrganizationLocationType.Pobox:
                    return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Pobox};
                case Domain.Base.Entities.OrganizationLocationType.Region:
                    return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Region };
                case Domain.Base.Entities.OrganizationLocationType.Support:
                    return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Support };
                default:
                    return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Other };
            }
        }
    }
}
