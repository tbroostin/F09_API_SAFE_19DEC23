using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.F09.Transactions;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.F09.Repositories
{
    [RegisterType]
    class ScholarshipApplicationRepository : BaseColleagueRepository, IScholarshipApplicationRepository
    {
        public ScholarshipApplicationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<ScholarshipApplicationResponse> GetScholarshipApplicationAsync(string personId)
        {
            var request = new F09_ScholarshipApplicationRequest();
            request.Id = personId;
            request.RequestType = "Get";

            ScholarshipApplicationResponse application;

            try
            {
                F09_ScholarshipApplicationResponse response = await transactionInvoker.ExecuteAsync<F09_ScholarshipApplicationRequest, F09_ScholarshipApplicationResponse>(request);
                application = this.CreateScholarshipApplicationObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetScholarshipApplicationAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetScholarshipApplicationAsync': " + String.Join("\n", ex.Message));
            }

            return application;
        }

        public async Task<ScholarshipApplicationResponse> UpdateScholarshipApplicationAsync(ScholarshipApplicationRequest applicationRequest)
        {
            var request = new F09_ScholarshipApplicationRequest();
            request.Id = applicationRequest.Id;
            request.RequestType = applicationRequest.RequestType;
            request.XfstId = applicationRequest.XfstId;
            request.XfstRefName = applicationRequest.XfstRefName;
            request.XfstSelfRateDesc = applicationRequest.XfstSelfRateDesc;
            request.XfstResearchInt = applicationRequest.XfstResearchInt;
            request.XfstDissTopic = applicationRequest.XfstDissTopic;
            request.XfstFinSit = applicationRequest.XfstFinSit;
            request.XfstSelfRate = applicationRequest.XfstSelfRate;

            List<Awards> awards = new List<Awards>();
            foreach (ScholarshipApplicationAwards reqAward in applicationRequest.Awards)
            {
                Awards award = new Awards();
                award.AwId = reqAward.Id;
                award.AwTitle = reqAward.Title;
                award.AwDesc = reqAward.Desc;
                award.AwMinMax = reqAward.MinMax;
                award.AwAddnlRequ = reqAward.AddnlRequ;
                award.AwChecked = reqAward.Checked;
                awards.Add(award);
            }
            request.Awards = awards;

            ScholarshipApplicationResponse application;

            try
            {
                F09_ScholarshipApplicationResponse response = await transactionInvoker.ExecuteAsync<F09_ScholarshipApplicationRequest, F09_ScholarshipApplicationResponse>(request);
                application = this.CreateScholarshipApplicationObject(response);
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-UpdateScholarshipApplicationAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-UpdateScholarshipApplicationAsync': " + String.Join("\n", ex.Message));
            }

            return application;
        }

        private ScholarshipApplicationResponse CreateScholarshipApplicationObject(F09_ScholarshipApplicationResponse response)
        {
            ScholarshipApplicationResponse application = new ScholarshipApplicationResponse();
            application.Id = response.Id;
            application.RespondType = response.RespondType;
            application.MsgHtml = response.MsgHtml;
            application.StudentName = response.StudentName;
            application.StudentEmail = response.StudentEmail;
            application.StudentAddress = response.StudentAddress;
            application.ApplDeadline = response.ApplDeadline;
            application.ApplTerm = response.ApplTerm;
            application.XfstId = response.XfstId;
            application.XfstPrevSubmit = response.XfstPrevSubmit;
            application.XfstRefName = response.XfstRefName;
            application.XfstSelfRateDesc = response.XfstSelfRateDesc;
            application.XfstResearchInt = response.XfstResearchInt;
            application.XfstDissTopic = response.XfstDissTopic;
            application.XfstFinSit = response.XfstFinSit;
            application.XfstSelfRate = response.XfstSelfRate;
            application.Step1Html = response.Step1Html;
            application.Step2Html = response.Step2Html;
            application.Step3Html = response.Step3Html;
            application.Step4Html = response.Step4Html;
            application.ErrorMsg = response.ErrorMsg;

            List<ScholarshipApplicationAwards> awards = new List<ScholarshipApplicationAwards>();
            foreach (Awards respAward in response.Awards)
            {
                ScholarshipApplicationAwards award = new ScholarshipApplicationAwards();
                award.Id = respAward.AwId;
                award.Title = respAward.AwTitle;
                award.Desc = respAward.AwDesc;
                award.MinMax = respAward.AwMinMax;
                award.AddnlRequ = respAward.AwAddnlRequ;
                award.Checked = respAward.AwChecked;
                awards.Add(award);
            }
            application.Awards = awards;

            return application;
        }
    }
}
