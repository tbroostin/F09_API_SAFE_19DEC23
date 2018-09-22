// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class RegistrationBilling
    {
        private readonly string _Id;
        private readonly string _PersonId;
        private readonly string _AccountTypeCode;
        private readonly DateTime _BillingStart;
        private readonly DateTime _BillingEnd;
        private readonly string _InvoiceId;
        private readonly List<RegistrationBillingItem> _Items = new List<RegistrationBillingItem>();

        public string Id { get { return _Id; } }
        public string PersonId { get { return _PersonId; } }
        public string AccountTypeCode { get { return _AccountTypeCode; } }
        public DateTime BillingStart { get { return _BillingStart; } }
        public DateTime BillingEnd { get { return _BillingEnd; } }
        public string InvoiceId { get { return _InvoiceId; } }
        public List<RegistrationBillingItem> Items { get { return _Items; } }

        public string TermId { get; set; }
        public string AdjustmentId { get; set; }

        public RegistrationBilling(string id, string personId, string accountType, DateTime? start, DateTime? end, string invoiceId, IEnumerable<RegistrationBillingItem> items)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id","ID cannot be null");
            }

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID cannot be null");
            }

            if (string.IsNullOrEmpty(accountType))
            {
                throw new ArgumentNullException("accountType", "Account Type cannot be null");
            }

            if (start == null)
            {
                throw new ArgumentNullException("start","Billing Start Date cannot be null");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end", "Billing End Date cannot be null");
            }

            if (start.Value > end.Value)
            {
                throw new ArgumentOutOfRangeException("start", "Billing start date cannot be after the billing end date.");
            }
            
            if (string.IsNullOrEmpty(invoiceId))
            {
                throw new ArgumentNullException("invoiceId", "Invoice ID cannot be null");
            }

            if (items == null || items.Count() == 0)
            {
                throw new ArgumentNullException("items", "At least one billing item must be specified.");
            }

            _Id = id;
            _PersonId = personId;
            _AccountTypeCode = accountType;
            _BillingStart = start.Value;
            _BillingEnd = end.Value;
            _InvoiceId = invoiceId;
            _Items.AddRange(items);
        }
    }
}
