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
    class F09AdminTrackingSheetRepository : BaseColleagueRepository, IF09AdminTrackingSheetRepository
    {
        public F09AdminTrackingSheetRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<F09AdminTrackingSheetResponse> GetF09AdminTrackingSheetAsync(string personId)
        {
            var request = new ctxF09AdminTrackingSheetRequest();
            request.Id = personId;            

            F09AdminTrackingSheetResponse at;

            try
            {
                ctxF09AdminTrackingSheetResponse response = await transactionInvoker.ExecuteAsync<ctxF09AdminTrackingSheetRequest, ctxF09AdminTrackingSheetResponse>(request);
                at = this.CreateStuTrackingSheetObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09AdminTrackingSheetAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09AdminTrackingSheetAsync': " + String.Join("\n", ex.Message));
            }

            return at;
        }

        private F09AdminTrackingSheetResponse CreateStuTrackingSheetObject(ctxF09AdminTrackingSheetResponse response)
        {
            F09AdminTrackingSheetResponse adminTrackingSheet = new F09AdminTrackingSheetResponse();
            
            List<F09AdminTrackingSheet> adminTrackingSheets = new List<F09AdminTrackingSheet>();
            foreach (StuTracking respStuTracking in response.StuTracking)
            {
                F09AdminTrackingSheet t = new F09AdminTrackingSheet();
                t.StuId = respStuTracking.StuId;
                t.StuName = respStuTracking.StuName;
                t.StadType = respStuTracking.StadType;
                t.ReviewTerms = respStuTracking.ReviewTerms;
                t.Prog = respStuTracking.Prog;
                adminTrackingSheets.Add(t);
            }
            adminTrackingSheet.AdminTrackingSheets = adminTrackingSheets;
            return adminTrackingSheet;
        }
    }
}
