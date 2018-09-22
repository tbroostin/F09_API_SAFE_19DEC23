// Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    public class GenderIdentityTypeService : BaseCoordinationService, IGenderIdentityTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;

        public GenderIdentityTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        public async Task<IEnumerable<Dtos.Base.GenderIdentityType>> GetBaseGenderIdentityTypesAsync(bool ignoreCache = false)
        {
            try
            {
                var genderIdentityTypesEntityCollection = await _referenceDataRepository.GetGenderIdentityTypesAsync(ignoreCache);
                var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.GenderIdentityType, Dtos.Base.GenderIdentityType>();
                var genderIdentityTypesDtoCollection = new List<Dtos.Base.GenderIdentityType>();
                foreach (var genderIdentityTypesEntity in genderIdentityTypesEntityCollection)
                {
                    var genderIdentityTypesDto = adapter.MapToType(genderIdentityTypesEntity);
                    genderIdentityTypesDtoCollection.Add(genderIdentityTypesDto);
                }
                return genderIdentityTypesDtoCollection;
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to retrieve gendery identities ", ex.Message);
                logger.Info(ex, message);
                throw;
            }
        }
    }
}
