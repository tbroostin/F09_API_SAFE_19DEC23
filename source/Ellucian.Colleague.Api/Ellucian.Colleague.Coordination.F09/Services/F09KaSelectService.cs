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
    public class F09KaSelectService : BaseCoordinationService, IF09KaSelectService
    {
        private readonly IF09KaSelectRepository _F09KaSelectRepository;

        public F09KaSelectService(IAdapterRegistry adapterRegistry, IF09KaSelectRepository F09KaSelectRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._F09KaSelectRepository = F09KaSelectRepository;
        }

        public async Task<F09KaSelectResponseDto> GetF09KaSelectAsync(string personId)
        {
            var domainResponse = await _F09KaSelectRepository.GetF09KaSelectAsync(personId);

            //convert domainResponse to DtoResponse
            List<KaSelectTermsDto> termsDto = new List<KaSelectTermsDto>();
            List<KaSelectSTCDto> stcDto = new List<KaSelectSTCDto>();

            foreach (Ellucian.Colleague.Domain.F09.Entities.KaSelectTerms term in domainResponse.KATerms)
            {
                KaSelectTermsDto t = new KaSelectTermsDto();
                t.TermId = term.TermId;
                t.TermDesc = term.TermDesc;
                termsDto.Add(t);
            }

            foreach (Ellucian.Colleague.Domain.F09.Entities.KaSelectSTC stc in domainResponse.KAStc)
            {
                KaSelectSTCDto t = new KaSelectSTCDto();
                t.StcId = stc.StcId;
                t.StcTerm = stc.StcTerm;
                t.StcStuId = stc.StcStuId;
                t.StcStuName = stc.StcStuName;
                t.StcCourse = stc.StcCourse;
                t.StcIDate = stc.StcIDate;
                stcDto.Add(t);
            }


            var dtoResponse = new F09KaSelectResponseDto(
                termsDto,
                stcDto
            );

            return dtoResponse;

        }

    }
}