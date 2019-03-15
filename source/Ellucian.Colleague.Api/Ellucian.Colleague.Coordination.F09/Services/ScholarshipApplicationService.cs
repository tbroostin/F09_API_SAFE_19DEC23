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
    public class ScholarshipApplicationService : BaseCoordinationService, IScholarshipApplicationService
    {
        private readonly IScholarshipApplicationRepository _ScholarshipApplicationRepository;

        public ScholarshipApplicationService(IAdapterRegistry adapterRegistry, IScholarshipApplicationRepository ScholarshipApplicationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._ScholarshipApplicationRepository = ScholarshipApplicationRepository;
        }

        public async Task<ScholarshipApplicationResponseDto> GetScholarshipApplicationAsync(string personId)
        {
            var application = await _ScholarshipApplicationRepository.GetScholarshipApplicationAsync(personId);
            var dto = this.ConvertToDTO(application);

            return dto;
        }

        public async Task<ScholarshipApplicationResponseDto> UpdateScholarshipApplicationAsync(Ellucian.Colleague.Dtos.F09.ScholarshipApplicationRequestDto request)
        {
            Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationRequest applicationRequest = new Domain.F09.Entities.ScholarshipApplicationRequest();
            applicationRequest.Id = request.Id;
            applicationRequest.RequestType = request.RequestType;
            applicationRequest.XfstId = request.XfstId;
            applicationRequest.XfstRefName = request.XfstRefName;
            applicationRequest.XfstSelfRateDesc = request.XfstSelfRateDesc;
            applicationRequest.XfstResearchInt = request.XfstResearchInt;
            applicationRequest.XfstDissTopic = request.XfstDissTopic;
            applicationRequest.XfstFinSit = request.XfstFinSit;
            applicationRequest.XfstSelfRate = request.XfstSelfRate;

            var application = await _ScholarshipApplicationRepository.UpdateScholarshipApplicationAsync(applicationRequest);
            var dto = this.ConvertToDTO(application);

            return dto;
        }

        private ScholarshipApplicationResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationResponse application)
        {
            List<ScholarshipApplicationAwardsDto> awards = new List<ScholarshipApplicationAwardsDto>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards respAward in application.Awards)
            {
                ScholarshipApplicationAwardsDto award = new ScholarshipApplicationAwardsDto();
                award.Id = respAward.Id;
                award.Title = respAward.Title;
                award.Desc = respAward.Desc;
                award.MinMax = respAward.MinMax;
                award.AddnlRequ = respAward.AddnlRequ;
                award.Checked = respAward.Checked;
                awards.Add(award);
            }

            var dto = new ScholarshipApplicationResponseDto
            (
                application.Id,
                application.RespondType,
                application.MsgHtml,
                application.StudentName,
                application.StudentEmail,
                application.StudentAddress,
                application.ApplDeadline,
                application.ApplTerm,
                application.XfstId,
                application.XfstPrevSubmit,
                application.XfstRefName,
                application.XfstSelfRateDesc,
                application.XfstResearchInt,
                application.XfstDissTopic,
                application.XfstFinSit,
                application.XfstSelfRate,
                application.Step1Html,
                application.Step2Html,
                application.Step3Html,
                application.Step4Html,
                application.ErrorMsg,
                awards
            );

            return dto;
        }
    }
}
