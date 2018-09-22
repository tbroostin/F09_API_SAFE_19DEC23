// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Domain Entity class for PurchaseOrderReceiptItem
    /// </summary>
    [Serializable]
    public class PurchaseOrderReceiptItem
    {
       private string id;

        public string Id
        {
            get { return id; }
            set
            {
                if (string.IsNullOrEmpty(id))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        id = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Id.");
                }
            }
        }

        /// <summary>
        /// The quantity received (if positive) or unreceived (if negative).
        /// </summary>
        public Decimal? ReceivedQty { get; set; }

        /// <summary>
        /// The dollar amount received (if positive) or unreceived (if negative).
        /// </summary>
        public Decimal? ReceivedAmt { get; set; }

        /// <summary>
        /// The currency of PRII.RECEIVED.AMT.
        /// </summary>
        public string ReceivedAmtCurrency { get; set; }

        /// <summary>
        /// The quantity rejected (if positive) or unreceived (if negative).
        /// </summary>
        public Decimal? RejectedQty { get; set; }

        /// <summary>
        /// The dollar amount received (if positive) or unreceived (if negative).
        /// </summary>
        public Decimal? RejectedAmt { get; set; }

        /// <summary>
        /// The currency of PRII.RECEIVED.AMT.
        /// </summary>
        public string RejectedAmtCurrency { get; set; }

        /// <summary>
        /// Item-level receiving comments.
        /// </summary>
        public string ReceivingComments { get; set; }

        /// <summary>
        /// This constructor initializes the domain entity.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public PurchaseOrderReceiptItem(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id is a required field when creating a PurchaseOrderReceiptItem");
            }

            this.id = id;
        }
    } 
}