// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    public class PersonRestrictionTypeService : BaseCoordinationService, IPersonRestrictionTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonRestrictionRepository _personRestrictionRepository;

        public PersonRestrictionTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IPersonRepository personRepository,
                                         IPersonRestrictionRepository personRestrictionRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _personRestrictionRepository = personRestrictionRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Gets all active restriction types for a person
        /// </summary>
        /// <param name="guid">GUID for the person</param>
        /// <returns>List of active RestrictionType GUIDs for the person</returns>
        public async Task<List<Dtos.GuidObject>> GetActivePersonRestrictionTypesAsync(string guid)
        {
            return await GetPersonRestrictionTypesAsync(guid, true);
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Gets restriction types for a person, either all of them or just the active ones
        /// </summary>
        /// <param name="guid">GUID for the person</param>
        /// <returns>List of RestrictionType GUIDs for the person</returns>
        private async Task<List<Dtos.GuidObject>> GetPersonRestrictionTypesAsync(string guid, bool activeOnly)
        {
            string personId = await _personRepository.GetPersonIdFromGuidAsync(guid);

            IEnumerable<string> personRestrictionIds;
            DateTime today = DateTime.Now;
            if (activeOnly)
            {
                personRestrictionIds = (await _personRestrictionRepository.GetAsync(personId)).Where(sr => (sr.StartDate.HasValue && sr.StartDate.Value <= today) && (sr.EndDate.HasValue == false || (sr.EndDate.HasValue && sr.EndDate.Value > today))).Select(r => r.RestrictionId).Distinct();
            }
            else
            {
                personRestrictionIds = (await _personRestrictionRepository.GetAsync(personId)).Select(r => r.RestrictionId).Distinct();
            }
            IEnumerable<Restriction> restrictions = await _referenceDataRepository.RestrictionsAsync();

            List<Dtos.GuidObject> restrictionTypeGuids = new List<Dtos.GuidObject>();
            foreach (var personRestrictionId in personRestrictionIds)
            {
                Restriction restriction = restrictions.Where(r => r.Code == personRestrictionId).First();
                if (restriction != null)
                {
                    restrictionTypeGuids.Add(new Dtos.GuidObject(restriction.Guid));
                }
            }
            return restrictionTypeGuids;
        }
    }
}
