using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a line item for an Procurement Receiveing Summary.
    /// </summary>
    [Serializable]
    public class LineItemSummary
    {
        /// <summary>
        /// Private system-generated line item id.
        /// </summary>
        private readonly string itemId;
        /// <summary>
        /// The purchase order item id
        /// </summary>
        public string ItemId { get { return itemId; } }

        /// <summary>
        /// Private line item name.
        /// </summary>
        private readonly string itemName;

        /// <summary>
        /// The purchase order item name
        /// </summary>
        public string ItemName { get { return itemName; } }

        /// <summary>
        /// The purchase order item description
        /// </summary>
        public string ItemDescription { get; set; }

        /// <summary>
        /// Private system-generated line item id.
        /// </summary>
        private readonly decimal? itemQuantity;

        /// <summary>
        /// The purchase order item quantity
        /// </summary>        
        public decimal? ItemQuantity { get { return itemQuantity; } }

        /// <summary>
        /// The purchase order item units of issue
        /// </summary>
        public string ItemUnitOfIssue { get; set; }
        /// <summary>
        /// Items MSDS flag
        /// </summary>
        public bool ItemMSDSFlag { get; set; }

        /// <summary>
        /// Parameterized contructor to instantiate a LineItemSummary object.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemName"></param>
        /// <param name="itemQuantity"></param>
        public LineItemSummary(string itemId, string itemName, decimal? itemQuantity) {

            if (string.IsNullOrEmpty(itemId))
            {
                throw new ArgumentNullException("itemId");
            }
            //if (string.IsNullOrEmpty(itemName))
            //{
            //    throw new ArgumentNullException("itemName");
            //}

            if (itemQuantity.Equals(null))
            {
                throw new ArgumentNullException("itemQuantity");
            }
            if (itemQuantity <= 0)
            {
                throw new ArgumentNullException("itemQuantity");
            }

            this.itemId = itemId;
            this.itemName = itemName;
            this.itemQuantity = itemQuantity;
        }

    }
}
