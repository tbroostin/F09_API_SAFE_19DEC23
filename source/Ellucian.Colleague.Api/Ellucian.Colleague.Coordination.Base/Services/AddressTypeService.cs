// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
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
    public class AddressTypeService : BaseCoordinationService, IAddressTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private const string _dataOrigin = "Colleague";

        public AddressTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all address types
        /// </summary>
        /// <returns>Collection of AddressType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AddressType2>> GetAddressTypesAsync(bool bypassCache = false)
        {
            var addressTypeCollection = new List<Ellucian.Colleague.Dtos.AddressType2>();

            var addressEntities = await _referenceDataRepository.GetAddressTypes2Async(bypassCache);
            if (addressEntities != null && addressEntities.Count() > 0)
            {
                foreach (var address in addressEntities)
                {
                    addressTypeCollection.Add(ConvertAddressTypeEntityToAddressType2Dto(address));
                }
            }
            return addressTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a address type from its GUID
        /// </summary>
        /// <returns>AddressType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AddressType2> GetAddressTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertAddressTypeEntityToAddressType2Dto((await _referenceDataRepository.GetAddressTypes2Async(true)).Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Address Type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a AddressType domain entity to its corresponding AddressType DTO
        /// </summary>
        /// <param name="source">AddressType domain entity</param>
        /// <returns>AddressType DTO</returns>
        private Dtos.AddressType2 ConvertAddressTypeEntityToAddressType2Dto(AddressType2 source)
        {
            var addressType = new Dtos.AddressType2();
            addressType.Id = source.Guid;
            addressType.Code = source.Code;
            addressType.Title = source.Description;
            addressType.Description = null;
            addressType.AddressTypeList = new Dtos.AddressTypeList();
            addressType.AddressTypeList = ConvertAddressTypeCategoryDomainEnumToAddressTypeCategoryDtoEnum(source.AddressTypeCategory);
           
            return addressType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PersonAddressType domain enumeration value to its corresponding PersonAddressType DTO enumeration value
        /// </summary>
        /// <param name="source">PersonAddressType domain enumeration value</param>
        /// <returns>PersonAddressType DTO enumeration value</returns>
        private Dtos.AddressTypeList ConvertAddressTypeCategoryDomainEnumToAddressTypeCategoryDtoEnum(AddressTypeCategory source)
        {
            switch (source)
            {
                case Domain.Base.Entities.AddressTypeCategory.Billing:
                    return Dtos.AddressTypeList.Billing;
                case Domain.Base.Entities.AddressTypeCategory.Home:
                    return Dtos.AddressTypeList.Home;
                case Domain.Base.Entities.AddressTypeCategory.Business:
                    return Dtos.AddressTypeList.Business;
                case Domain.Base.Entities.AddressTypeCategory.Mailing:
                    return Dtos.AddressTypeList.Mailing;
                case Domain.Base.Entities.AddressTypeCategory.School:
                    return Dtos.AddressTypeList.School;
                case Domain.Base.Entities.AddressTypeCategory.Shipping:
                    return Dtos.AddressTypeList.Shipping;
                case Domain.Base.Entities.AddressTypeCategory.Vacation:
                    return Dtos.AddressTypeList.Vacation;
                case Domain.Base.Entities.AddressTypeCategory.Parent:
                    return Dtos.AddressTypeList.Parent;
                case Domain.Base.Entities.AddressTypeCategory.Family:
                    return Dtos.AddressTypeList.Family;
                case Domain.Base.Entities.AddressTypeCategory.Branch:
                    return Dtos.AddressTypeList.Branch;
                case Domain.Base.Entities.AddressTypeCategory.Main:
                    return Dtos.AddressTypeList.Main;
                case Domain.Base.Entities.AddressTypeCategory.Pobox:
                    return Dtos.AddressTypeList.Pobox;
                case Domain.Base.Entities.AddressTypeCategory.Region:
                    return Dtos.AddressTypeList.Region;
                case Domain.Base.Entities.AddressTypeCategory.Support:
                    return Dtos.AddressTypeList.Support;
                case Domain.Base.Entities.AddressTypeCategory.MatchingGifts:
                    return Dtos.AddressTypeList.MatchingGifts;
                default:
                    return Dtos.AddressTypeList.Other;

            }
        }
    }
}
