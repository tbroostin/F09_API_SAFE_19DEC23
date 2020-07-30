using System;
using System.Collections.Generic;
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
    class F09EvalSelectRepository : BaseColleagueRepository, IF09EvalSelectRepository
    {
        public F09EvalSelectRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<domainF09EvalSelectResponse> GetF09EvalSelectAsync(string personId)
        {
            //create ctx request
            var ctxRequest = new ctxF09EvalSelectRequest();
            ctxRequest.Id = personId;

            var domainResponse = new domainF09EvalSelectResponse();

            try
            {
                //send ctxRequest to Colleague for actual response
                ctxF09EvalSelectResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09EvalSelectRequest, ctxF09EvalSelectResponse>(ctxRequest);

                //convert ctxResponse to domainResponse               
                var domainTypes = new List<Domain.F09.Entities.EType>();
                foreach (Transactions.EType ctxType in ctxResponse.EType)
                {
                    var domainType = new Domain.F09.Entities.EType();
                    domainType.Type = ctxType.Type;
                    domainType.TypeDesc = ctxType.TypeDesc;
                    domainTypes.Add(domainType);
                }

                var domainEvals = new List<Domain.F09.Entities.QEval>();
                foreach (Transactions.QEval ctxEval in ctxResponse.QEval)
                {
                    var domainEval = new Domain.F09.Entities.QEval();
                    domainEval.EvalKey = ctxEval.EvalKey;
                    domainEval.EvalType = ctxEval.EvalType;
                    domainEval.EvalDesc1 = ctxEval.EvalDesc1;
                    domainEval.EvalDesc2 = ctxEval.EvalDesc2;
                    domainEvals.Add(domainEval);
                }

                domainResponse.Id = ctxResponse.Id;
                domainResponse.RespondType = ctxResponse.RespondType;
                domainResponse.Msg = ctxResponse.Msg;
                domainResponse.QEval = domainEvals;
                domainResponse.EType = domainTypes;

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09EvalSelectAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09EvalSelectAsync': " + String.Join("\n", ex.Message));
            }

            return domainResponse;
        }
    }
}
