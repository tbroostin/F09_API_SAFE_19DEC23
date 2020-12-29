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
    public class PhoneTypeService : BaseCoordinationService, IPhoneTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        private const string _dataOrigin = "Colleague";

        public PhoneTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger,
                                         IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all phone types
        /// </summary>
        /// <returns>Collection of PhoneType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PhoneType2>> GetPhoneTypesAsync(bool bypassCache = false)
        {
            var phoneTypeCollection = new List<Ellucian.Colleague.Dtos.PhoneType2>();

            var phoneTypeEntities = await _referenceDataRepository.GetPhoneTypesAsync(bypassCache);
            if (phoneTypeEntities != null && phoneTypeEntities.Count() > 0)
            {
                foreach (var phoneType in phoneTypeEntities)
                {
                    phoneTypeCollection.Add(ConvertPhoneEntityToPhoneTypeDto(phoneType));
                }
            }
            return phoneTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a phone type from its GUID
        /// </summary>
        /// <returns>PhoneType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PhoneType2> GetPhoneTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertPhoneEntityToPhoneTypeDto((await _referenceDataRepository.GetPhoneTypesAsync(true)).Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Phone Type not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Gets list of base phone types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.PhoneType">PhoneType</see> items</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneType>> GetBasePhoneTypesAsync()
        {
            var phoneTypeCollection = new List<Ellucian.Colleague.Dtos.Base.PhoneType>();
            try
            {
                var phoneTypeEntities = await _referenceDataRepository.GetPhoneTypesAsync(false);
                if (phoneTypeEntities != null && phoneTypeEntities.Count() > 0)
                {
                    var adapter = new AutoMapperAdapter<PhoneType, Ellucian.Colleague.Dtos.Base.PhoneType>(_adapterRegistry, logger);
                    foreach (var phoneType in phoneTypeEntities)
                    {
                        phoneTypeCollection.Add(adapter.MapToType(phoneType));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred getting phone types " + ex.Message);
            }
            return phoneTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PhoneType domain entity to its corresponding PhoneType2 DTO
        /// </summary>
        /// <param name="source">Phone domain entity</param>
        /// <returns>PhoneType DTO</returns>
        private Dtos.PhoneType2 ConvertPhoneEntityToPhoneTypeDto(PhoneType source)
        {
            var phoneType = new Dtos.PhoneType2();
            phoneType.Id = source.Guid;
            phoneType.Code = source.Code;
            phoneType.Title = source.Description;
            phoneType.Description = null;
            phoneType.PhoneTypeList = new Dtos.PhoneTypeList();
            phoneType.PhoneTypeList = ConvertPhoneTypeDomainEnumToPhoneTypeDtoEnum(source.PhoneTypeCategory);

            return phoneType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a PhoneType domain enumeration value to its corresponding PhoneType DTO enumeration value
        /// </summary>
        /// <param name="source">PhoneType domain enumeration value</param>
        /// <returns>PhoneType DTO enumeration value</returns>
        private Dtos.PhoneTypeList ConvertPhoneTypeDomainEnumToPhoneTypeDtoEnum(PhoneTypeCategory source)
        {
            switch (source)
            {
                case Domain.Base.Entities.PhoneTypeCategory.Business:
                    return Dtos.PhoneTypeList.Business;
                case Domain.Base.Entities.PhoneTypeCategory.Fax:
                    return Dtos.PhoneTypeList.Fax;
                case Domain.Base.Entities.PhoneTypeCategory.Home:
                    return Dtos.PhoneTypeList.Home;
                case Domain.Base.Entities.PhoneTypeCategory.Mobile:
                    return Dtos.PhoneTypeList.Mobile;
                case Domain.Base.Entities.PhoneTypeCategory.Pager:
                    return Dtos.PhoneTypeList.Pager;
                case Domain.Base.Entities.PhoneTypeCategory.School:
                    return Dtos.PhoneTypeList.School;
                case Domain.Base.Entities.PhoneTypeCategory.TDD:
                    return Dtos.PhoneTypeList.TDD;
                case Domain.Base.Entities.PhoneTypeCategory.Vacation:
                    return Dtos.PhoneTypeList.Vacation;
                case Domain.Base.Entities.PhoneTypeCategory.Billing:
                    return Dtos.PhoneTypeList.Billing;
                case Domain.Base.Entities.PhoneTypeCategory.Branch:
                    return Dtos.PhoneTypeList.Branch;
                case Domain.Base.Entities.PhoneTypeCategory.Main:
                    return Dtos.PhoneTypeList.Main;
                case Domain.Base.Entities.PhoneTypeCategory.Region:
                    return Dtos.PhoneTypeList.Region;
                case Domain.Base.Entities.PhoneTypeCategory.Support:
                    return Dtos.PhoneTypeList.Support;
                case Domain.Base.Entities.PhoneTypeCategory.Parent:
                    return Dtos.PhoneTypeList.Parent;
                case Domain.Base.Entities.PhoneTypeCategory.Family:
                    return Dtos.PhoneTypeList.Family;
                case Domain.Base.Entities.PhoneTypeCategory.MatchingGifts:
                    return Dtos.PhoneTypeList.Matching;
                default:
                    return Dtos.PhoneTypeList.Other;
            }
        }
    }
}
