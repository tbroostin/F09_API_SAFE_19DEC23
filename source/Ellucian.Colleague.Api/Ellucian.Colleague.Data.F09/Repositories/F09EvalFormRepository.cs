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
using AutoMapper;

namespace Ellucian.Colleague.Data.F09.Repositories
{
    // f09 teresa@toad-code.com 07/30/30
    // Important note: after generating the ctxF09EvalForm transaction
    // delete "public class Questions"
    // because KA grading already defines that class, we simply need to reuse it


    [RegisterType]
    class F09EvalFormRepository : BaseColleagueRepository, IF09EvalFormRepository
    {
        public F09EvalFormRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<domainF09EvalFormResponse> GetF09EvalFormAsync(string key)
        {
            try
            {
                //create ctx request
                var ctxRequest = new ctxF09EvalFormRequest();
                ctxRequest.EvalKey = key;
                ctxRequest.RequestType = "GetEvalForm";

                //send ctxRequest to Colleague for actual response
                ctxF09EvalFormResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09EvalFormRequest, ctxF09EvalFormResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                Mapper.CreateMap<ctxF09EvalFormResponse, domainF09EvalFormResponse>();
                Mapper.CreateMap<Transactions.Questions, Domain.F09.Entities.Questions>();
                var domainResponse = Mapper.Map<ctxF09EvalFormResponse, domainF09EvalFormResponse>(ctxResponse);

                return domainResponse;
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09EvalFormAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09EvalFormAsync': " + String.Join("\n", ex.Message));
            }
        }

        public async Task<domainF09EvalFormResponse> UpdateF09EvalFormAsync(domainF09EvalFormRequest domainRequest)
        {

            try
            {
                //create ctx request
                var ctxRequest = new ctxF09EvalFormRequest();

                //convert domainRequest to ctxRequest
                Mapper.CreateMap<domainF09EvalFormRequest, ctxF09EvalFormRequest>();
                Mapper.CreateMap<Domain.F09.Entities.Questions, Transactions.Questions>();
                ctxRequest = Mapper.Map<domainF09EvalFormRequest, ctxF09EvalFormRequest>(domainRequest);

                //send ctxRequest to Colleague for actual response
                ctxF09EvalFormResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09EvalFormRequest, ctxF09EvalFormResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                var domainResponse = new domainF09EvalFormResponse();
                domainResponse.RespondType = ctxResponse.RespondType;
                domainResponse.Msg = ctxResponse.Msg;

                return domainResponse;
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-UpdateF09EvalFormAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-UpdateF09EvalFormAsync': " + String.Join("\n", ex.Message));
            }

        }

    }
}
