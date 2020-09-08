using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class LineItemSummaryEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemSummary, Ellucian.Colleague.Dtos.ColleagueFinance.LineItemSummary>
    {
        public LineItemSummaryEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
        public LineItemSummary MapToType(Domain.ColleagueFinance.Entities.LineItemSummary Source)
        {
            var lineItemSummaryDto = new LineItemSummary();

            lineItemSummaryDto.ItemId = Source.ItemId;
            lineItemSummaryDto.ItemName = Source.ItemName;
            lineItemSummaryDto.ItemDescription = Source.ItemDescription;
            lineItemSummaryDto.ItemQuantity = Source.ItemQuantity;
            lineItemSummaryDto.ItemUnitOfIssue = Source.ItemUnitOfIssue;
            lineItemSummaryDto.ItemMSDSFlag = Source.ItemMSDSFlag;
            return lineItemSummaryDto;
        }
    }
}
