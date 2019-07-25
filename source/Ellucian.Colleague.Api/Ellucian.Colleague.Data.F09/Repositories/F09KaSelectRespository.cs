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

namespace Ellucian.Colleague.Data.F09.Repositories
{



    [RegisterType]
    class F09KaSelectRepository : BaseColleagueRepository, IF09KaSelectRepository
    {
        public F09KaSelectRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<F09KaSelectResponse> GetF09KaSelectAsync(string personId)
        {
            //create ctx request
            var ctxRequest = new ctxF09KaSelectRequest();
            ctxRequest.FacId = personId;

            F09KaSelectResponse domainResponse = new F09KaSelectResponse();

            try
            {
                //send ctxRequest to Colleague for actual response
                ctxF09KaSelectResponse ctxResponse = await transactionInvoker.ExecuteAsync<ctxF09KaSelectRequest, ctxF09KaSelectResponse>(ctxRequest);

                //convert ctxResponse to domainResponse
                List<KaSelectTerms> termsDomain = new List<KaSelectTerms>();
                foreach (Terms term in ctxResponse.Terms)
                {
                    KaSelectTerms t = new KaSelectTerms();
                    t.TermId = term.TermId;
                    t.TermDesc = term.TermDesc;
                    termsDomain.Add(t);
                }
                domainResponse.KATerms = termsDomain;

                List<KaSelectSTC> stcDomain = new List<KaSelectSTC>();
                foreach (STC stc in ctxResponse.STC)
                {
                    KaSelectSTC t = new KaSelectSTC();
                    t.StcId = stc.StcId;
                    t.StcTerm = stc.StcTerm;
                    t.StcStuId = stc.StcStuId;
                    t.StcStuName = stc.StcStuName;
                    t.StcCourse = stc.StcCourse;
                    t.StcIDate = stc.StcIDate;
                    stcDomain.Add(t);
                }
                domainResponse.KAStc = stcDomain;

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09KaSelectAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09KaSelectAsync': " + String.Join("\n", ex.Message));
            }

            return domainResponse;
        }
    }
}
