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
    class F09SsnRepository : BaseColleagueRepository, IF09SsnRepository
    {
        public F09SsnRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<F09SsnResponse> GetF09SsnAsync(string personId)
        {
            //create ctx request
            var ctxRequest = new ctxF09SsnRequest();
            ctxRequest.Id = personId;
            ctxRequest.RequestType = "Get";

            F09SsnResponse domainResponse = new F09SsnResponse();

            try
            {
                //send ctxRequest to Colleague for actual response
                ctxF09SsnResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09SsnRequest, ctxF09SsnResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                domainResponse.RespondType = ctxResponse.RespondType;
                domainResponse.Ssn = ctxResponse.Ssn;
                domainResponse.ErrorMsg = ctxResponse.ErrorMsg;

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09SsnAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09SsnAsync': " + String.Join("\n", ex.Message));
            }

            return domainResponse;
        }

        public async Task<F09SsnResponse> UpdateF09SsnAsync(F09SsnRequest domainRequest)
        {
            //create ctx request
            var ctxRequest = new ctxF09SsnRequest();
            ctxRequest.Id = domainRequest.Id;
            ctxRequest.Ssn = domainRequest.Ssn;
            ctxRequest.RequestType = domainRequest.RequestType;

            F09SsnResponse domainResponse = new F09SsnResponse();

            try
            {
                //send ctxRequest to Colleague for actual response
                ctxF09SsnResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09SsnRequest, ctxF09SsnResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                domainResponse.RespondType = ctxResponse.RespondType;
                domainResponse.Ssn = ctxResponse.Ssn;
                domainResponse.ErrorMsg = ctxResponse.ErrorMsg;
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-UpdateF09SsnAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-UpdateF09SsnAsync': " + String.Join("\n", ex.Message));
            }

            return domainResponse;
        }

    }

}
