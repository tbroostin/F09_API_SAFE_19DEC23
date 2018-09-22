// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a <see cref="Domain.Base.Entities.PersonProxyUser"/> from a <see cref="Dtos.Base.PersonProxyUser"/>
    /// </summary>
    public class PersonProxyUserDtoAdapter : AutoMapperAdapter<Dtos.Base.PersonProxyUser, Domain.Base.Entities.PersonProxyUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonProxyUserDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public PersonProxyUserDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Base.Phone, Domain.Base.Entities.Phone>();
            AddMappingDependency<Dtos.Base.EmailAddress, Domain.Base.Entities.EmailAddress>();
            AddMappingDependency<Dtos.Base.PersonName, Domain.Base.Entities.PersonName>();
        }

        /// <summary>
        /// Maps a <see cref="Domain.Base.Entities.PersonProxyUser"/> from a <see cref="Dtos.Base.PersonProxyUser"/>
        /// </summary>
        /// <param name="Source">The <see cref="Dtos.Base.PersonProxyUser"/> to map</param>
        /// <returns>The mapped <see cref="Domain.Base.Entities.PersonProxyUser"/></returns>
        public override Domain.Base.Entities.PersonProxyUser MapToType(Dtos.Base.PersonProxyUser Source)
        {
            if (Source == null)
            {
                throw new ArgumentNullException("Source");
            }
            var email = new List<Domain.Base.Entities.EmailAddress>();
            if (Source.EmailAddresses != null && Source.EmailAddresses.Any())
            {
                var emailAdapter = adapterRegistry.GetAdapter<Dtos.Base.EmailAddress, Domain.Base.Entities.EmailAddress>();
                email.AddRange(Source.EmailAddresses.Select(e => emailAdapter.MapToType(e)).ToList());
            }

            var phone = new List<Domain.Base.Entities.Phone>();
            if (Source.Phones != null && Source.Phones.Any())
            {
                var phoneAdapter = adapterRegistry.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>();
                phone.AddRange(Source.Phones.Select(p => phoneAdapter.MapToType(p)).ToList());
            }

            var name = new List<Domain.Base.Entities.PersonName>();
            if (Source.FormerNames != null && Source.FormerNames.Any())
            {
                var nameAdapter = adapterRegistry.GetAdapter<Dtos.Base.PersonName, Domain.Base.Entities.PersonName>();
                name.AddRange(Source.FormerNames.Select(n => nameAdapter.MapToType(n)).ToList());
            }

            return new Domain.Base.Entities.PersonProxyUser(Source.Id, Source.FirstName, Source.LastName, email, phone, name, Source.PrivacyStatusCode)
            {
                BirthDate = Source.BirthDate,
                Gender = string.IsNullOrEmpty(Source.Gender) ? string.Empty : Source.Gender,
                GovernmentId = string.IsNullOrEmpty(Source.GovernmentId) ? string.Empty : Source.GovernmentId,
                MiddleName = string.IsNullOrEmpty(Source.MiddleName) ? string.Empty : Source.MiddleName,
                Prefix = string.IsNullOrEmpty(Source.Prefix) ? string.Empty : Source.Prefix,
                Suffix = string.IsNullOrEmpty(Source.Suffix) ? string.Empty : Source.Suffix,
                
            };
        }
    }}
