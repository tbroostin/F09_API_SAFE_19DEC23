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
    class GetF09StuTrackingSheetRepository : BaseColleagueRepository, IGetF09StuTrackingSheetRepository
    {
        public GetF09StuTrackingSheetRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<GetF09StuTrackingSheetResponse> GetF09StuTrackingSheetAsync(string personId)
        {
            var request = new CtxF09StuTrackingSheetRequest();
            request.Id = personId;

            GetF09StuTrackingSheetResponse profile;

            try
            {
                CtxF09StuTrackingSheetResponse response = await transactionInvoker.ExecuteAsync<CtxF09StuTrackingSheetRequest, CtxF09StuTrackingSheetResponse>(request);
                profile = this.CreateStuTrackingSheetObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09StuTrackingSheetAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09StuTrackingSheetAsync': " + String.Join("\n", ex.Message));
            }

            return profile;
        }

        private GetF09StuTrackingSheetResponse CreateStuTrackingSheetObject(CtxF09StuTrackingSheetResponse response)
        {
            GetF09StuTrackingSheetResponse studentTrackingSheet = new GetF09StuTrackingSheetResponse();
            studentTrackingSheet.Html = response.Html;

            return studentTrackingSheet;
        }
    }
}

