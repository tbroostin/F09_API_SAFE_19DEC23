using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Purchase Order line item informations
    /// </summary>
    public class LineItemSummary
    {
        /// <summary>
        /// The purchase order item id
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// The purchase order item name
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// The purchase order item description
        /// </summary>
        public string ItemDescription { get; set; }

        /// <summary>
        /// The purchase order item quantity
        /// </summary>        
        public decimal? ItemQuantity { get; set; }

        /// <summary>
        /// The purchase order item units of issue
        /// </summary>
        public string ItemUnitOfIssue { get; set; }
        /// <summary>
        /// Item MSDS falg
        /// </summary>
        public bool ItemMSDSFlag { get; set; }
    }
}
