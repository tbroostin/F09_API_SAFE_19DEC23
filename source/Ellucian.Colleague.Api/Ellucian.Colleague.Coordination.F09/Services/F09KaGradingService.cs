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
            Mapper.CreateMap<domF09KaGradingResponse, dtoF09KaGradingResponse>();
            Mapper.CreateMap<Domain.F09.Entities.GradeOptions, Dtos.F09.GradeOptions>();
            Mapper.CreateMap<Domain.F09.Entities.Questions, Dtos.F09.Questions>();
            var dtoResponse = Mapper.Map<domF09KaGradingResponse, dtoF09KaGradingResponse>(domainResponse);

            return dtoResponse;
        }


        public async Task<dtoF09KaGradingResponse> UpdateF09KaGradingAsync(dtoF09KaGradingRequest dtoRequest)
        {
            //convert dtoRequest to domainRequest
            Mapper.CreateMap<dtoF09KaGradingRequest, domF09KaGradingRequest>();
            Mapper.CreateMap<Dtos.F09.Questions, Domain.F09.Entities.Questions>();
            var domainRequest = Mapper.Map<dtoF09KaGradingRequest, domF09KaGradingRequest>(dtoRequest);

            //send domainRequest to Repository
            var domainResponse = await _F09KaGradingRepository.UpdateF09KaGradingAsync(domainRequest);

            //convert domainResponse to DtoResponse
            dtoF09KaGradingResponse dtoResponse = new dtoF09KaGradingResponse();
            dtoResponse.RespondType = domainResponse.RespondType;
            dtoResponse.Msg = domainResponse.Msg;

            return dtoResponse;
        }
    }
}