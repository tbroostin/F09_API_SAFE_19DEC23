// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    public class PersonalPronounTypeService : BaseCoordinationService, IPersonalPronounTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;

        public PersonalPronounTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IPersonRepository personRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        public async Task<IEnumerable<Dtos.Base.PersonalPronounType>> GetBasePersonalPronounTypesAsync(bool ignoreCache=false)
        {
            var personalPronounTypesEntityCollection = await _referenceDataRepository.GetPersonalPronounTypesAsync(ignoreCache);
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonalPronounType, Dtos.Base.PersonalPronounType>();
            var personalPronounTypesDtoCollection = new List<Dtos.Base.PersonalPronounType>();
            foreach (var personalPronounTypesEntity in personalPronounTypesEntityCollection)
            {
                var personalPronounTypesDto = adapter.MapToType(personalPronounTypesEntity);
                personalPronounTypesDtoCollection.Add(personalPronounTypesDto);
            }
            return personalPronounTypesDtoCollection;
        }
    }
}
