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
    public class GetF09StuTrackingSheetService : BaseCoordinationService, IGetF09StuTrackingSheetService
    {
        private readonly IGetF09StuTrackingSheetRepository _GetF09StuTrackingSheetRepository;

        public GetF09StuTrackingSheetService(IAdapterRegistry adapterRegistry, IGetF09StuTrackingSheetRepository GetF09StuTrackingSheetRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._GetF09StuTrackingSheetRepository = GetF09StuTrackingSheetRepository;
        }

        public async Task<GetF09StuTrackingSheetResponseDto> GetF09StuTrackingSheetAsync(string Id)
        {
            var profile = await _GetF09StuTrackingSheetRepository.GetF09StuTrackingSheetAsync(Id);
            var dto = this.ConvertToDTO(profile);

            return dto;
        }


        private GetF09StuTrackingSheetResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.GetF09StuTrackingSheetResponse student)
        {
            var dto = new GetF09StuTrackingSheetResponseDto
            (
                student.Html
            );

            return dto;
        }
    }
}

