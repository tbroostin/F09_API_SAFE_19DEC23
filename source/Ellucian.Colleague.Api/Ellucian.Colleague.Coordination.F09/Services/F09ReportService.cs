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
    public class F09ReportService : BaseCoordinationService, IF09ReportService
    {
        private readonly IF09ReportRepository _F09ReportRepository;

        public F09ReportService(IAdapterRegistry adapterRegistry, IF09ReportRepository F09ReportRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._F09ReportRepository = F09ReportRepository;
        }

        public async Task<dtoF09ReportResponse> GetF09ReportAsync(dtoF09ReportRequest dtoRequest)        
        {
            //convert dtoRequest to domainRequest
            var domainRequest = new Domain.F09.Entities.domF09ReportRequest();
            domainRequest.Id = dtoRequest.Id;
            domainRequest.Report = dtoRequest.Report;
            domainRequest.JsonRequest = dtoRequest.JsonRequest;

            //send domainRequest to Repository
            var domainResponse = await _F09ReportRepository.GetF09ReportAsync(domainRequest);

            //convert domainResponse to DtoResponse
            var dtoResponse = new dtoF09ReportResponse();
            dtoResponse.HtmlReport = domainResponse.HtmlReport;
            return dtoResponse;
        }
    }
}
