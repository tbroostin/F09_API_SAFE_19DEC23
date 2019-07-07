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

    [RegisterType]
    class F09KaGradingRepository : BaseColleagueRepository, IF09KaGradingRepository
    {
        public F09KaGradingRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<domF09KaGradingResponse> GetF09KaGradingAsync(string stcId)
        {
            try
            {
                //create ctx request
                var ctxRequest = new ctxF09KaGradingRequest();
                ctxRequest.StcId = stcId;
                ctxRequest.RequestType = "GetKaGradingForm";

                //send ctxRequest to Colleague for actual response
                ctxF09KaGradingResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09KaGradingRequest, ctxF09KaGradingResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                Mapper.CreateMap<ctxF09KaGradingResponse, domF09KaGradingResponse>();
                Mapper.CreateMap<Transactions.GradeOptions, Domain.F09.Entities.GradeOptions > ();
                Mapper.CreateMap<Transactions.Questions, Domain.F09.Entities.Questions>();               
                var domResponse = Mapper.Map<ctxF09KaGradingResponse, domF09KaGradingResponse>(ctxResponse);

                return domResponse;
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09KaGradingAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09KaGradingAsync': " + String.Join("\n", ex.Message));
            }            
        }

        public async Task<domF09KaGradingResponse> UpdateF09KaGradingAsync(domF09KaGradingRequest domainRequest)
        {

            try
            {
                //create ctx request
                var ctxRequest = new ctxF09KaGradingRequest();

                //convert domainRequest to ctxRequest
                Mapper.CreateMap<domF09KaGradingRequest, ctxF09KaGradingRequest>();
                Mapper.CreateMap<Domain.F09.Entities.Questions, Transactions.Questions>();
                ctxRequest = Mapper.Map<domF09KaGradingRequest, ctxF09KaGradingRequest>(domainRequest);

                //send ctxRequest to Colleague for actual response
                ctxF09KaGradingResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09KaGradingRequest, ctxF09KaGradingResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                var domainResponse = new domF09KaGradingResponse();
                domainResponse.RespondType = ctxResponse.RespondType;
                domainResponse.Msg = ctxResponse.Msg;

                return domainResponse;
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-UpdateF09KaGradingAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-UpdateF09KaGradingAsync': " + String.Join("\n", ex.Message));
            }
            
        }

    }
}
