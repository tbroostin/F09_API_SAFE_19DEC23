using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
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
    public class F09SsnService : BaseCoordinationService, IF09SsnService
    {
        private readonly IF09SsnRepository _F09SsnRepository;

        public F09SsnService(IAdapterRegistry adapterRegistry, IF09SsnRepository F09SsnRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._F09SsnRepository = F09SsnRepository;
        }

        public async Task<F09SsnResponseDto> GetF09SsnAsync(string personId)
        {
            var domainResponse = await _F09SsnRepository.GetF09SsnAsync(personId);
            
            //convert domainResponse to DtoResponse
            F09SsnResponseDto dtoResponse = new F09SsnResponseDto();
            dtoResponse.RespondType = domainResponse.RespondType;
            dtoResponse.Ssn = domainResponse.Ssn;
            dtoResponse.ErrorMsg = domainResponse.ErrorMsg;

            return dtoResponse;
        }

        public async Task<F09SsnResponseDto> UpdateF09SsnAsync(Ellucian.Colleague.Dtos.F09.F09SsnRequestDto dtoRequest)
        {
            //convert dtoRequest to domainRequest
            Ellucian.Colleague.Domain.F09.Entities.F09SsnRequest domainRequest = new Domain.F09.Entities.F09SsnRequest();
            domainRequest.Id = dtoRequest.Id;
            domainRequest.RequestType = dtoRequest.RequestType;
            domainRequest.Ssn = dtoRequest.Ssn;

            //send domainRequest to Repository
            var domainResponse = await _F09SsnRepository.UpdateF09SsnAsync(domainRequest);

            //convert domainResponse to DtoResponse
            F09SsnResponseDto dtoResponse = new F09SsnResponseDto();
            dtoResponse.RespondType = domainResponse.RespondType;
            dtoResponse.Ssn = domainResponse.Ssn;
            dtoResponse.ErrorMsg = domainResponse.ErrorMsg;

            return dtoResponse;
        }
    }
}
