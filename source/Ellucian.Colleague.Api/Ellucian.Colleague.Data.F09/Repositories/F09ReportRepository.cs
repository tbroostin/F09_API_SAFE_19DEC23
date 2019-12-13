using System;
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
    class F09ReportRepository : BaseColleagueRepository, IF09ReportRepository
    {
        public F09ReportRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<domF09ReportResponse> GetF09ReportAsync(domF09ReportRequest domainRequest)
        {
            try
            {
                //convert domainRequest to ctxRequest
                var ctxRequest = new ctxF09ReportRequest();
                ctxRequest.Id = domainRequest.Id;
                ctxRequest.Report = domainRequest.Report;
                ctxRequest.RequestType = domainRequest.RequestType;
                ctxRequest.JsonRequest = domainRequest.JsonRequest;

                //send ctxRequest to Colleague for actual response
                ctxF09ReportResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09ReportRequest, ctxF09ReportResponse>(ctxRequest);

                //convert ctxResponse to domainResponse                
                var domainResponse = new domF09ReportResponse();
                domainResponse.RespondType = ctxResponse.RespondType;
                domainResponse.Msg = ctxResponse.Msg;
                domainResponse.HtmlReport = ctxResponse.HtmlReport;
                domainResponse.JsonReportOptions = ctxResponse.JsonReportOptions;

                return domainResponse;
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09ReportAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09ReportAsync': " + String.Join("\n", ex.Message));
            }
        }

    }
}
