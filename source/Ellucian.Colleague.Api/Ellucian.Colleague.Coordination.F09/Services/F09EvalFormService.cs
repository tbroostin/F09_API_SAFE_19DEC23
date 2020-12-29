using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using AutoMapper;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    [RegisterType]
    public class F09EvalFormService : BaseCoordinationService, IF09EvalFormService
    {
        private readonly IF09EvalFormRepository _F09EvalFormRepository;

        public F09EvalFormService(IAdapterRegistry adapterRegistry, IF09EvalFormRepository F09EvalFormRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._F09EvalFormRepository = F09EvalFormRepository;
        }

        public async Task<dtoF09EvalFormResponse> GetF09EvalFormAsync(string stcId)
        {
            var domainResponse = await _F09EvalFormRepository.GetF09EvalFormAsync(stcId);

            //convert domainResponse to DtoResponse
            Mapper.CreateMap<domainF09EvalFormResponse, dtoF09EvalFormResponse>();
            Mapper.CreateMap<Domain.F09.Entities.Questions, Dtos.F09.Questions>();
            var dtoResponse = Mapper.Map<domainF09EvalFormResponse, dtoF09EvalFormResponse>(domainResponse);

            return dtoResponse;
        }


        public async Task<dtoF09EvalFormResponse> UpdateF09EvalFormAsync(dtoF09EvalFormRequest dtoRequest)
        {
            //convert dtoRequest to domainRequest
            Mapper.CreateMap<dtoF09EvalFormRequest, domainF09EvalFormRequest>();
            Mapper.CreateMap<Dtos.F09.Questions, Domain.F09.Entities.Questions>();
            var domainRequest = Mapper.Map<dtoF09EvalFormRequest, domainF09EvalFormRequest>(dtoRequest);

            //send domainRequest to Repository
            var domainResponse = await _F09EvalFormRepository.UpdateF09EvalFormAsync(domainRequest);

            //convert domainResponse to DtoResponse
            dtoF09EvalFormResponse dtoResponse = new dtoF09EvalFormResponse();
            dtoResponse.RespondType = domainResponse.RespondType;
            dtoResponse.Msg = domainResponse.Msg;

            return dtoResponse;
        }
    }
}