/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Colleague.Domain.Student;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// FinancialAidPersonService class
    /// </summary>
    [RegisterType]
    public class FinancialAidPersonService : FinancialAidCoordinationService, IFinancialAidPersonService
    {
        private readonly IFinancialAidPersonRepository financialAidPersonRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        /// <param name="financialAidPersonRepository"></param>
        public FinancialAidPersonService(IAdapterRegistry adapterRegistry,
           ICurrentUserFactory currentUserFactory,
           IRoleRepository roleRepository,
           ILogger logger,
           IFinancialAidPersonRepository financialAidPersonRepository,
           IConfigurationRepository configurationRepository)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.financialAidPersonRepository = financialAidPersonRepository;
        }

        /// <summary>
        /// Searches for financial aid persons for the specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Set of Person DTOs</returns>
        public async Task<PrivacyWrapper<IEnumerable<Dtos.Base.Person>>> SearchFinancialAidPersonsAsync(FinancialAidPersonQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if ((criteria.FinancialAidPersonIds == null || !criteria.FinancialAidPersonIds.Any())
                && string.IsNullOrEmpty(criteria.FinancialAidPersonQueryKeyword))
            {
                throw new ArgumentException("either a list of person ids or a query keyword must be present");
            }

            if (!HasPermission(StudentPermissionCodes.ViewFinancialAidInformation))
            {
                string message = CurrentUser.PersonId + " does not have permission code " + StudentPermissionCodes.ViewFinancialAidInformation;
                logger.Error(message);
                throw new PermissionsException(message);
            }

            bool hasPrivacyRestriction = false;
            IEnumerable<Domain.Base.Entities.PersonBase> financialAidPersons = new List<Domain.Base.Entities.PersonBase>();
            try
            {
                if (criteria.FinancialAidPersonIds != null && criteria.FinancialAidPersonIds.Any())
                {
                    financialAidPersons = await financialAidPersonRepository.SearchFinancialAidPersonsByIdsAsync(criteria.FinancialAidPersonIds);
                }
                else if (!string.IsNullOrEmpty(criteria.FinancialAidPersonQueryKeyword))
                {
                    financialAidPersons = await financialAidPersonRepository.SearchFinancialAidPersonsByKeywordAsync(criteria.FinancialAidPersonQueryKeyword);
                }
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            var financialAidPersonDtoToEntityAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.Base.Person>();
            var personDtos = new List<Dtos.Base.Person>();
            foreach (var personEntity in financialAidPersons)
            {
                var person = personEntity;
                if (faPersonHasPrivacyRestriction(personEntity))
                {
                    hasPrivacyRestriction = true;
                    person = new PersonBase(personEntity.Id, personEntity.LastName, personEntity.PrivacyStatusCode)
                    {
                        FirstName = personEntity.FirstName,
                        MiddleName = personEntity.MiddleName,
                        PreferredName = personEntity.PreferredName
                    };
                }
                personDtos.Add(financialAidPersonDtoToEntityAdapter.MapToType(person));
            }
            return new PrivacyWrapper<IEnumerable<Dtos.Base.Person>>(personDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// Returns true/false to indicate whether the passed in person has privacy restriction
        /// </summary>
        /// <param name="person">person base entity</param>
        /// <returns>true/false</returns>
        private bool faPersonHasPrivacyRestriction(PersonBase person)
        {
            if (string.IsNullOrEmpty(person.PrivacyStatusCode) || CurrentUser.IsPerson(person.Id) || HasProxyAccessForPerson(person.Id))
            {
                return false;
            }
            else
            {
                return !HasPrivacyCodeAccess(person.PrivacyStatusCode);
            }
        }
    }
}
