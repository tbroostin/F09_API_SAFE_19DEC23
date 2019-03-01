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
    class GetActiveRestrictionsRepository : BaseColleagueRepository, IGetActiveRestrictionsRepository
    {
        public GetActiveRestrictionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<GetActiveRestrictionsResponse> GetActiveRestrictionsAsync(string personId)
        {
            var request = new ctxGetActiveRestrictionsRequest();
            request.Id = personId;

            GetActiveRestrictionsResponse profile;

            try
            {
                ctxGetActiveRestrictionsResponse response = await transactionInvoker.ExecuteAsync<ctxGetActiveRestrictionsRequest, ctxGetActiveRestrictionsResponse>(request);
                profile = this.CreateActiveRestrictionsObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetActiveRestrictionsAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetActiveRestrictionsAsync': " + String.Join("\n", ex.Message));
            }

            return profile;
        }

        private GetActiveRestrictionsResponse CreateActiveRestrictionsObject(ctxGetActiveRestrictionsResponse response)
        {
            GetActiveRestrictionsResponse student = new GetActiveRestrictionsResponse();
            student.ActiveRestrictions = response.ActiveRestrictions;

            return student;
        }
    }
}
