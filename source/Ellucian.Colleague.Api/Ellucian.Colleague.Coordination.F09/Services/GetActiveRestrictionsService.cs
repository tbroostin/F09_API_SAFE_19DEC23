using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    [RegisterType]
    public class GetActiveRestrictionsService : BaseCoordinationService, IGetActiveRestrictionsService
    {
        private readonly IGetActiveRestrictionsRepository _GetActiveRestrictionsRepository;

        public GetActiveRestrictionsService(IAdapterRegistry adapterRegistry, IGetActiveRestrictionsRepository GetActiveRestrictionsRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._GetActiveRestrictionsRepository = GetActiveRestrictionsRepository;
        }

        public async Task<GetActiveRestrictionsResponseDto> GetActiveRestrictionsAsync(string personId)
        {
            var profile = await _GetActiveRestrictionsRepository.GetActiveRestrictionsAsync(personId);
            var dto = this.ConvertToDTO(profile);

            return dto;
        }

        private GetActiveRestrictionsResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.GetActiveRestrictionsResponse student)
        {
            var dto = new GetActiveRestrictionsResponseDto
            (
                student.ActiveRestrictions
            );

            return dto;
        }
    }
}
