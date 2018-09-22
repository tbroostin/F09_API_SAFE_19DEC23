// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class SelfservicePreferencesService : BaseCoordinationService, ISelfservicePreferencesService
    {
        private ISelfservicePreferencesRepository _selfservicePreferencesRepository;

        public SelfservicePreferencesService(
            ISelfservicePreferencesRepository selfservicePreferencesRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _selfservicePreferencesRepository = selfservicePreferencesRepository;
        }

        public async Task<Dtos.Base.SelfservicePreference> GetPreferenceAsync(string personId, string preferenceType)
        {
            if (CurrentUser != null && CurrentUser.IsPerson(personId))
            {
                var preferenceEntity = await _selfservicePreferencesRepository.GetPreferenceAsync(CurrentUser.PersonId, preferenceType);
                var preferenceDto = _adapterRegistry.GetAdapter<Domain.Base.Entities.SelfservicePreference, Dtos.Base.SelfservicePreference>().MapToType(preferenceEntity);

                return preferenceDto;
            }
            else
            {
                throw new PermissionsException("Cannot retrieve preferences when not logged in.");
            }
        }


        public async Task<Dtos.Base.SelfservicePreference> UpdatePreferenceAsync(string id, string personId, string preferenceType, IDictionary<string, dynamic> preferences)
        {
            if (CurrentUser != null && CurrentUser.IsPerson(personId))
            {
                var preferenceEntity = await _selfservicePreferencesRepository.UpdatePreferenceAsync(id, CurrentUser.PersonId, preferenceType, preferences);
                var preferenceDto = _adapterRegistry.GetAdapter<Domain.Base.Entities.SelfservicePreference, Dtos.Base.SelfservicePreference>().MapToType(preferenceEntity);

                return preferenceDto;
            }
            else
            {
                throw new PermissionsException("Cannot update preferences when not logged in.");
            }
        }

        public async Task DeletePreferenceAsync(string personId, string preferenceType)
        {
            if (CurrentUser != null && CurrentUser.IsPerson(personId))
            {
                await _selfservicePreferencesRepository.DeletePreferenceAsync(personId, preferenceType);
            }
            else
            {
                throw new PermissionsException("Cannot delete preferences when not logged in.");
            }
        }
    }
}
