// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    public class PersonProxyUserRepository : PersonBaseRepository, IPersonProxyUserRepository
    {
        const int PersonProfileCacheTimeout = 60;

        private readonly string _colleagueTimeZone;

        /// <summary>
        /// Constructor for person profile repository.
        /// </summary>
        /// <param name="cacheProvider">The cache provider</param>
        /// <param name="transactionFactory">The Colleague Transaction factory</param>
        /// <param name="logger">The logger</param>
        public PersonProxyUserRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Creates a person for the purpose of becoming a proxy user
        /// </summary>
        /// <param name="person">The <see cref="PersonProxyUser"> person</see> to create</param>
        /// <returns>The created <see cref="PersonProxyUser"> person </see></returns>
        public async Task<PersonProxyUser> CreatePersonProxyUserAsync(PersonProxyUser person)
        {
            if (person == null)
            {
                throw new ArgumentNullException("person");
            }

            var emails = person.EmailAddresses.Select(e => new PeopleProxyEmail() {EmailAddresses = e.Value, EmailTypes = e.TypeCode}).ToList();
            var phones = person.Phones.Select(p => new PersonProxyPhone() { PhoneExtensions = p.Extension, PhoneNumbers = p.Number, PhoneTypes = p.TypeCode }).ToList();
            var names = person.FormerNames.Select(n => new PersonProxyNamehist() { FormerFamilyNames = n.FamilyName, FormerGivenNames = n.GivenName, FormerMiddleNames = n.MiddleName }).ToList();
            var request = new CreatePersonProxyUserRequest()
            {
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                MiddleName = person.MiddleName,
                BirthDate = person.BirthDate,
                Gender = person.Gender,
                Prefix = person.Prefix,
                Suffix = person.Suffix,
                Ssn = person.GovernmentId,
                PeopleProxyEmail = emails,
                PersonProxyNamehist = names,
                PersonProxyPhone = phones,
            };

            try {
                var response = await transactionInvoker.ExecuteAsync<CreatePersonProxyUserRequest, CreatePersonProxyUserResponse>(request);
                if (response == null)
                {
                    var message = "No response returned by Colleague Transaction CREATE.PERSON.PROXY.USER.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                if (response.ErrorMessages != null && response.ErrorMessages.Any())
                {
                    foreach (var msg in response.ErrorMessages)
                    {
                        logger.Error(msg);
                    }
                    var message = "Error returned by Colleague Transaction CREATE.PERSON.PROXY.USER.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                var data = await DataReader.ReadRecordAsync<Data.Base.DataContracts.PersonProxyUser>(response.PersonId);
                if (data == null )
                {
                    var message = "Could not read PERSON record.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                return BuildPersonProxyUser(data);
            }
            catch (Exception ex)
            {
                var message = "Error processing Colleague Transaction CREATE.PERSON.PROXY.USER.";
                logger.Error(ex, message);
                throw new RepositoryException(message, ex);
            }
        }

        private PersonProxyUser BuildPersonProxyUser(DataContracts.PersonProxyUser data)
        {
            var names = (data.NamehistEntityAssociation != null && data.NamehistEntityAssociation.Any())
               ? data.NamehistEntityAssociation.Select(n => new PersonName(n.NameHistoryFirstNameAssocMember, n.NameHistoryMiddleNameAssocMember, n.NameHistoryLastNameAssocMember)).ToList()
               : new List<PersonName>();
            var phones = (data.PerphoneEntityAssociation != null && data.PerphoneEntityAssociation.Any())
                ? data.PerphoneEntityAssociation.Select(p => new Phone(p.PersonalPhoneNumberAssocMember, p.PersonalPhoneTypeAssocMember, p.PersonalPhoneExtensionAssocMember)).ToList()
                : new List<Phone>();
            var emails = (data.PeopleEmailEntityAssociation != null && data.PeopleEmailEntityAssociation.Any())
                ? data.PeopleEmailEntityAssociation.Select(e => new EmailAddress(e.PersonEmailAddressesAssocMember, e.PersonEmailTypesAssocMember) { IsPreferred = (e.PersonPreferredEmailAssocMember != null) }).ToList()
                : new List<EmailAddress>();
            return new PersonProxyUser(data.Recordkey, data.FirstName, data.LastName, emails, phones, names)
            {
                BirthDate = data.BirthDate,
                Gender = data.Gender,
                GovernmentId = data.Ssn,
                MiddleName = data.MiddleName,
                Prefix = data.Prefix,
                Suffix = data.Suffix,
            };
        }
    }
}
