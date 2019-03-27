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

            List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards> awards = new List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards>();
            foreach (ScholarshipApplicationAwardsDto reqAward in request.Awards)
            {
                Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards award = new Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards();
                award.Id = reqAward.Id;
                award.Title = reqAward.Title;
                award.Desc = reqAward.Desc;
                award.MinMax = reqAward.MinMax;
                award.AddnlRequ = reqAward.AddnlRequ;
                award.LorEmailRequ = reqAward.LorEmailRequ;
                award.LorEmail = reqAward.LorEmail;
                award.Checked = reqAward.Checked;
                awards.Add(award);
            }

            applicationRequest.Awards = awards;

            List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ> softQs = new List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ>();
            foreach (ScholarshipApplicationSoftQDto reqSoftQ in request.SoftQs)
            {
                Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ softQ = new Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ();
                softQ.Code = reqSoftQ.Code;
                softQ.Desc = reqSoftQ.Desc;
                softQ.Checked = reqSoftQ.Checked;
                softQs.Add(softQ);
            }

            applicationRequest.SoftQs = softQs;

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
                award.LorEmailRequ = respAward.LorEmailRequ;
                award.LorEmail = respAward.LorEmail;
                award.Checked = respAward.Checked;
                awards.Add(award);
            }

            List<ScholarshipApplicationSoftQDto> softQs = new List<ScholarshipApplicationSoftQDto>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ respSoftQ in application.SoftQs)
            {
                ScholarshipApplicationSoftQDto softQ = new ScholarshipApplicationSoftQDto();
                softQ.Code = respSoftQ.Code;
                softQ.Desc = respSoftQ.Desc;
                softQ.Checked = respSoftQ.Checked;
                softQs.Add(softQ);
            }

            var dto = new ScholarshipApplicationResponseDto
            (
                application.Id,
                application.RespondType,
                application.MsgHtml,
                application.SoftQHtml,
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
                awards,
                softQs
            );

            return dto;
        }
    }
}
