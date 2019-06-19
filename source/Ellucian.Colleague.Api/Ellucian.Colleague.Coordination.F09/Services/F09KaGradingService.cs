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
    public class F09KaGradingService : BaseCoordinationService, IF09KaGradingService
    {
        private readonly IF09KaGradingRepository _F09KaGradingRepository;

        public F09KaGradingService(IAdapterRegistry adapterRegistry, IF09KaGradingRepository F09KaGradingRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._F09KaGradingRepository = F09KaGradingRepository;
        }

        public async Task<dtoF09KaGradingResponse> GetF09KaGradingAsync(string stcId)
        {
            var domainResponse = await _F09KaGradingRepository.GetF09KaGradingAsync(stcId);

            //convert domainResponse to DtoResponse
            List<GradeOptions> dtoGs = new List<GradeOptions>();

            foreach (Domain.F09.Entities.GradeOptions domG in domainResponse.GradeOptions)
            {
                GradeOptions dtoG = new GradeOptions();
                dtoG.GradeCode = domG.GradeCode;
                dtoG.GradeDesc = domG.GradeDesc;
                dtoGs.Add(dtoG);
            }

            var dtoResponse = new dtoF09KaGradingResponse(
                domainResponse.FacId,
                domainResponse.StcId,
                domainResponse.RespondType,
                domainResponse.ErrorMsg,
                domainResponse.KaHeaderHtml,
                dtoGs
            );

            return dtoResponse;

        }


        public async Task<dtoF09KaGradingResponse> UpdateF09KaGradingAsync(dtoF09KaGradingRequest dtoRequest)
        {
            //convert dtoRequest to domainRequest
            Domain.F09.Entities.domF09KaGradingRequest domainRequest = new Domain.F09.Entities.domF09KaGradingRequest();
            domainRequest.StcId = dtoRequest.StcId;
            domainRequest.KaComments = dtoRequest.KaComments;
            domainRequest.GradeSelected = dtoRequest.GradeSelected;
            domainRequest.RequestType = dtoRequest.RequestType;

            //send domainRequest to Repository
            var domainResponse = await _F09KaGradingRepository.UpdateF09KaGradingAsync(domainRequest);

            //convert domainResponse to DtoResponse
            dtoF09KaGradingResponse dtoResponse = new dtoF09KaGradingResponse();
            dtoResponse.ErrorMsg = domainResponse.ErrorMsg;

            return dtoResponse;
        }


    }
}