// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class Vendors
    {

        private string _guid;
        private string _id;

        /// <summary>
        /// Lists the types of terms available from the vendor
        /// </summary>       
        public List<string> Terms { get; set; }

        /// <summary>
        /// Vendor type codes identify types, or special characteristics, of vendors
        /// </summary>
        public List<string> Types { get; set; }

        /// <summary>
        /// Used to stop a check from being issued to this vendor
        /// </summary>
        public string StopPaymentFlag { get; set; }

        /// <summary>
        /// A value of N will result in a warning when this   vendor ID is used
        /// </summary>
        public string ApprovalFlag { get; set; }

        /// <summary>
        /// If this flag is N(o), this vendor cannot be entered on   
        /// requisition/p.o./blanket p.o/voucher/recurring voucher   creation.
        /// </summary>        
        public string ActiveFlag { get; set; }

        /// <summary>
        /// Displayed but not used by system.
        /// </summary>
        public List<string> Misc { get; set; }

        /// <summary>
        /// Used to issue a warning if an AP type is entered for a   
        /// vendor and that type is not in this list
        /// </summary>       
        public List<string> ApTypes { get; set; }

        /// <summary>
        /// Vendors Add Date
        /// </summary>      
        public DateTime? AddDate { get; set; }

        /// <summary>
        /// This field is used as the default currency code on   
        /// requisitions, purchase orders and vouchers when a vendor   
        /// is specified.
        /// </summary>       
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Returns true if Vendor is an organization
        /// </summary>
        public bool IsOrganization { get; set; }

        /// <summary>
        /// Corp Parent
        /// </summary>
        public List<string> CorpParent{ get; set; }

        /// <summary>
        /// The reasons the vendor has been placed on hold
        /// </summary>
         public List<string> IntgHoldReasons { get; set; }

        /// <summary>
        /// The vendor types eProcurment, Travel
        /// </summary>
        public List<string> Categories { get; set; }

        /// <summary>
		/// Tax Id
		/// </summary>
		public string TaxId { get; set; }

        /// <summary>
        /// Tax Form
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// List of address Id and Type
        /// </summary>
        public List<Dictionary<string, string>> AddressInfo { get; set; }

        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException("Vendors Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// GUID for the remark; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _guid; }
            set
            {
                if (string.IsNullOrEmpty(_guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        public Vendors(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Vendors guid can not be null or empty");
            }
            _guid = guid;
        }
    }
}