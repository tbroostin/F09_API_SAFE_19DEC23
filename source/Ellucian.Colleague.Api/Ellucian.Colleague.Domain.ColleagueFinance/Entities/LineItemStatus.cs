using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This enumeration contains the available statuses for a Line Item
    /// </summary>
    [Serializable]
    public enum LineItemStatus
    {
        /// <summary>
        /// The Line Item has an item with an accepted status
        /// </summary>
        Accepted,
        /// <summary>
        /// The Line Item has an item with a backordered status
        /// </summary>
        Backordered,
        /// <summary>
        /// The Line Item has an item with a closed 
        /// and from now on it can only be purged
        /// </summary>
        Closed,
        /// <summary>
        /// The Line Item has an item with an invoiced status
        /// </summary>
        Invoiced,
        /// <summary>
        /// The Line Item has an item with an outstanding status
        /// </summary>
        Outstanding,
        /// <summary>
        /// The Line Item has an item with a paid status
        /// </summary>
        Paid,
        /// <summary>
        /// The Line Item has an item with a reconciled 
        /// status and from now on it can only be purged
        /// </summary>
        Reconciled,
        /// <summary>
        /// The Line Item has an item with a voided
        /// status and from now on it can only be purged
        /// </summary>
        Voided,
        /// <summary>
        /// The Line Item has an item with a Hold status
        /// </summary>
        Hold,
        /// <summary>
        /// The Line Item has no status present and 
        /// status will be defaulted to None
        /// </summary>
        None
    }
}
