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
    public class F09AdminTrackingSheetService : BaseCoordinationService, IF09AdminTrackingSheetService
    {
        private readonly IF09AdminTrackingSheetRepository _F09AdminTrackingSheetRepository;

        public F09AdminTrackingSheetService(IAdapterRegistry adapterRegistry, IF09AdminTrackingSheetRepository F09AdminTrackingSheetRepository,
        ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
        ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._F09AdminTrackingSheetRepository = F09AdminTrackingSheetRepository;
        }

        public async Task<F09AdminTrackingSheetResponseDto> GetF09AdminTrackingSheetAsync(string personId)
        {
            var at = await _F09AdminTrackingSheetRepository.GetF09AdminTrackingSheetAsync(personId);
            var dto = this.ConvertToDTO(at);

            return dto;
        }

        private F09AdminTrackingSheetResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.F09AdminTrackingSheetResponse at)
        {

            List<F09AdminTrackingSheetDto> adminTracking = new List<F09AdminTrackingSheetDto>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.F09AdminTrackingSheet tracking in at.AdminTrackingSheets)
            {
                F09AdminTrackingSheetDto t = new F09AdminTrackingSheetDto();
                t.StuId = tracking.StuId;
                t.StuName = tracking.StuName;
                t.StadType = tracking.StadType;
                t.ReviewTerms = tracking.ReviewTerms;
                t.Prog = tracking.Prog;
                adminTracking.Add(t);
            }

            var dto = new F09AdminTrackingSheetResponseDto(
                adminTracking
            );
            return dto;
        }

    }
}
