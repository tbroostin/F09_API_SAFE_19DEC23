using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class F09EvalSelectService : BaseCoordinationService, IF09EvalSelectService
    {
        private readonly IF09EvalSelectRepository _F09EvalSelectRepository;

        public F09EvalSelectService(IAdapterRegistry adapterRegistry, IF09EvalSelectRepository F09EvalSelectRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._F09EvalSelectRepository = F09EvalSelectRepository;
        }

        public async Task<dtoF09EvalSelectResponse> GetF09EvalSelectAsync(string personId)
        {
            var domainResponse = await _F09EvalSelectRepository.GetF09EvalSelectAsync(personId);

            //convert domainResponse to DtoResponse
            var dtoTypes = new List<EType>();            
            foreach (Ellucian.Colleague.Domain.F09.Entities.EType domainType in domainResponse.EType)
            {
                var dtoType = new EType();
                dtoType.Type = domainType.Type;
                dtoType.TypeDesc = domainType.TypeDesc;
                dtoTypes.Add(dtoType);
            }

            var dtoEvals = new List<QEval>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.QEval domainEval in domainResponse.QEval)
            {
                var dtoEval = new QEval();
                dtoEval.EvalKey = domainEval.EvalKey;
                dtoEval.EvalType = domainEval.EvalType;
                dtoEval.EvalDesc1 = domainEval.EvalDesc1;
                dtoEval.EvalDesc2 = domainEval.EvalDesc2;
                dtoEvals.Add(dtoEval);
            }

            var dtoResponse = new dtoF09EvalSelectResponse(
                domainResponse.Id,
                domainResponse.RespondType,
                domainResponse.Msg,
                dtoEvals,
                dtoTypes
            );

            return dtoResponse;
        }

    }
}
