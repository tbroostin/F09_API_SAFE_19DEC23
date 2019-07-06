using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //create ctx request
            var ctxRequest = new ctxF09KaGradingRequest();
            ctxRequest.StcId = stcId;
            ctxRequest.RequestType = "GetKaGradingForm";

            domF09KaGradingResponse domResponse = new domF09KaGradingResponse();

            try
            {
                //send ctxRequest to Colleague for actual response
                ctxF09KaGradingResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09KaGradingRequest, ctxF09KaGradingResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                Mapper.CreateMap<ctxF09KaGradingResponse, domF09KaGradingResponse>();
                Mapper.CreateMap<Transactions.GradeOptions, Domain.F09.Entities.GradeOptions > ();
                Mapper.CreateMap<Transactions.Questions, Domain.F09.Entities.Questions>();
                
                domResponse = Mapper.Map<ctxF09KaGradingResponse, domF09KaGradingResponse>(ctxResponse);


                //List<Domain.F09.Entities.GradeOptions> domGs = new List<Domain.F09.Entities.GradeOptions>();
                //foreach (Transactions.GradeOptions ctxG in ctxResponse.GradeOptions)
                //{
                //    Domain.F09.Entities.GradeOptions domG = new Domain.F09.Entities.GradeOptions();
                //    domG.GradeCode = ctxG.GradeCode;
                //    domG.GradeDesc = ctxG.GradeDesc;
                //    domGs.Add(domG);
                //}
                //domResponse.FacId = ctxResponse.FacId;
                //domResponse.StcId = ctxResponse.StcId;
                //domResponse.RespondType = ctxResponse.RespondType;
                //domResponse.ErrorMsg = ctxResponse.ErrorMsg;
                //domResponse.KaHeaderHtml = ctxResponse.KaHeaderHtml;
                //domResponse.GradeOptions = domGs;

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09KaGradingAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09KaGradingAsync': " + String.Join("\n", ex.Message));
            }

            return domResponse;
        }

        public async Task<domF09KaGradingResponse> UpdateF09KaGradingAsync(domF09KaGradingRequest domainRequest)
        {
            ////create ctx request
            var ctxRequest = new ctxF09KaGradingRequest();
            //ctxRequest.StcId = domainRequest.StcId;
            //ctxRequest.GradeSelected = domainRequest.GradeSelected;
            //ctxRequest.KaComments = domainRequest.KaComments;
            //ctxRequest.RequestType = domainRequest.RequestType;

            //convert domainRequest to ctxRequest
            Mapper.CreateMap<domF09KaGradingRequest, ctxF09KaGradingRequest>();
            Mapper.CreateMap<Domain.F09.Entities.Questions, Transactions.Questions>();
            ctxRequest = Mapper.Map<domF09KaGradingRequest, ctxF09KaGradingRequest>(domainRequest);


            domF09KaGradingResponse domainResponse = new domF09KaGradingResponse();

            try
            {
                //send ctxRequest to Colleague for actual response
                ctxF09KaGradingResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09KaGradingRequest, ctxF09KaGradingResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                domainResponse.RespondType = ctxResponse.RespondType;
                domainResponse.Msg = ctxResponse.Msg;
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-UpdateF09KaGradingAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-UpdateF09KaGradingAsync': " + String.Join("\n", ex.Message));
            }

            return domainResponse;
        }

    }
}
