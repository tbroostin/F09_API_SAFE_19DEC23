using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.F09.Transactions;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet;
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
    class PdfStudentTrackingSheetRepository : BaseColleagueRepository, IPdfStudentTrackingSheetRepository
    {
        public PdfStudentTrackingSheetRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<PdfTrackingSheetResponse> GetPdfStudentTrackingSheetAsync(string personId)
        {
            var request = new ctxF09PdfTrackingSheetRequest();
            request.Id = personId;

            PdfTrackingSheetResponse application;

            try
            {
                ctxF09PdfTrackingSheetResponse response = await transactionInvoker.ExecuteAsync<ctxF09PdfTrackingSheetRequest, ctxF09PdfTrackingSheetResponse>(request);
                application = this.CreatePdfStudentTrackingSheetObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetPdfStudentTrackingSheetAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetPdfStudentTrackingSheetAsync': " + String.Join("\n", ex.Message));
            }

            return application;
        }

        private PdfTrackingSheetResponse CreatePdfStudentTrackingSheetObject(ctxF09PdfTrackingSheetResponse response)
        {
            PdfTrackingSheetResponse application = new PdfTrackingSheetResponse();
            application.StuName = response.StuName;
            application.StuAddr = response.StuAddr;
            application.BusAddr = response.BusAddr;
            application.FamiliarName = response.FamiliarName;
            application.GradProgAdvisor = response.GradProgAdvisor;
            application.TranEquivText = response.TranEquivText;
            application.TkResdyHours = response.TkResdyHours;
            application.TkReshrHours = response.TkReshrHours;
            application.ADisChair = response.ADisChair;
            application.DisAd = response.DisAd;
            application.DisFacRdr = response.DisFacRdr;
            application.DisStuRdr = response.DisStuRdr;
            application.DisConFac = response.DisConFac;
            application.DisExtExam = response.DisExtExam;
            application.AdLabel = response.AdLabel;
            application.Degrees = response.Degrees;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones> phones = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones>();
            foreach (Transactions.Phones respOptions in response.Phones)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones();
                option.PhoneNo = respOptions.PhoneNo;
                option.PhoneExt = respOptions.PhoneExt;
                option.PhoneType = respOptions.PhoneType;
                phones.Add(option);
            }
            application.Phones = phones;

            return application;
        }
    }
}
