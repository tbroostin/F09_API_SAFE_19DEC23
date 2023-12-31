﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The originating document associated with the line item, which indicates what encumbrance needs to be relieved
    /// </summary>
    [DataContract]
    public class LineItemReferenceDocumentDtoProperty
    {
        /// <summary>
        /// The originating purchase order associated with the line item.
        /// </summary>
        [DataMember(Name = "purchaseOrder", EmitDefaultValue = false)]
        public string PurchaseOrder { get; set; }

        /// <summary>
        /// The originating blanket purchase order associated with the line item.
        /// </summary>
        [DataMember(Name = "blanketPurchaseOrder", EmitDefaultValue = false)]
        public string BlanketPurchaseOrder { get; set; }

        /// <summary>
        /// The originating recurring voucher associated with the line item.
        /// </summary>
        [DataMember(Name = "recurringVoucher", EmitDefaultValue = false)]
        public string RecurringVoucher { get; set; }

        /// <summary>
        /// The originating purchasing arrangement associated with the line item.
        /// </summary>
        [DataMember(Name = "purchasingArrangement", EmitDefaultValue = false)]
        public string PurchasingArrangement { get; set; }

    }
}