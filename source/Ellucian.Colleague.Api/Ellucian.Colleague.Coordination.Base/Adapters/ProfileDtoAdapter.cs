// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping Profile Dto to Entity
    /// </summary>
    public class ProfileDtoAdapter : AutoMapperAdapter<Dtos.Base.Profile, Ellucian.Colleague.Domain.Base.Entities.Profile>
    {
        /// <summary>
        /// Initializes a new instance of the ProfileDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public ProfileDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Base.Address, Domain.Base.Entities.Address>();
            AddMappingDependency<Dtos.Base.EmailAddress, Domain.Base.Entities.EmailAddress>();
            AddMappingDependency<Dtos.Base.Phone, Domain.Base.Entities.Phone>();
        }

        /// <summary>
        /// Maps a Profile Dto to the corresponding Entity
        /// </summary>
        /// <param name="Source">The Profile Dto</param>
        /// <returns>The corresponding Profile Entity</returns>
        public override Domain.Base.Entities.Profile MapToType(Dtos.Base.Profile Source)
        {
            Domain.Base.Entities.Profile profileEntity = new Domain.Base.Entities.Profile(Source.Id, Source.LastName);
            profileEntity.AddressConfirmationDateTime = Source.AddressConfirmationDateTime;
            profileEntity.BirthDate = Source.BirthDate;
            profileEntity.EmailAddressConfirmationDateTime = Source.EmailAddressConfirmationDateTime;
            profileEntity.FirstName = Source.FirstName;
            profileEntity.LastChangedDateTime = Source.LastChangedDateTime;
            profileEntity.PhoneConfirmationDateTime = Source.PhoneConfirmationDateTime;
            profileEntity.PreferredName = Source.PreferredName;
            profileEntity.ChosenFirstName = Source.ChosenFirstName;
            profileEntity.ChosenMiddleName = Source.ChosenMiddleName;
            profileEntity.ChosenLastName = Source.ChosenLastName;
            profileEntity.IsDeceased = Source.IsDeceased;
            profileEntity.Nickname = Source.Nickname;
            profileEntity.GenderIdentityCode = Source.GenderIdentityCode;
            profileEntity.PersonalPronounCode = Source.PersonalPronounCode;

            var addressDtoAdapter = adapterRegistry.GetAdapter<Dtos.Base.Address, Domain.Base.Entities.Address>();
            foreach (var addressDto in Source.Addresses)
            {
                profileEntity.AddAddress(addressDtoAdapter.MapToType(addressDto));
            }

            var emailAddressDtoAdapter = adapterRegistry.GetAdapter<Dtos.Base.EmailAddress, Domain.Base.Entities.EmailAddress>();
            foreach (var emailAddressDto in Source.EmailAddresses) {
                profileEntity.AddEmailAddress(emailAddressDtoAdapter.MapToType(emailAddressDto));
            } 

            var phoneDtoAdapter = adapterRegistry.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>();
            foreach (var phoneDto in Source.Phones) {
                profileEntity.AddPhone(phoneDtoAdapter.MapToType(phoneDto));
            }
            return profileEntity;
        }
    }
}
