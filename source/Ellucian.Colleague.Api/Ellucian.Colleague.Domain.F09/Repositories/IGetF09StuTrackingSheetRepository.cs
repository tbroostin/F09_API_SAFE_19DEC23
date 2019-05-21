using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IGetF09StuTrackingSheetRepository
    {
        Task<GetF09StuTrackingSheetResponse> GetF09StuTrackingSheetAsync(string Id);

        Task<PdfTrackingSheetResponse> GetPdfStudentTrackingSheetAsync(string personId);
    }
}


