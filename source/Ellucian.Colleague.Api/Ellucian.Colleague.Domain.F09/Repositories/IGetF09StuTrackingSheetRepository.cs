using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IGetF09StuTrackingSheetRepository
    {
        Task<GetF09StuTrackingSheetResponse> GetF09StuTrackingSheetAsync(string Id);
    }
}


