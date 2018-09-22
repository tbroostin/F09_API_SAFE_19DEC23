// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class SponsorInvoice : ReceivablePayment
    {
        private readonly string _invoiceId;
        public string InvoiceId { get { return _invoiceId; } }

        private readonly string _sponsorId;
        public string SponsorId { get { return _sponsorId; } }

        private readonly string _sponsorshipId;
        public string SponsorshipId { get { return _sponsorshipId; } }

        private readonly List<string> _termIds = new List<string>();
        public ReadOnlyCollection<string> TermIds { get; private set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public SponsorInvoice(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, decimal amount, 
            string invoiceId, string sponsorId, string sponsorshipId)
            : base(id, referenceNumber, personId, receivableType, termId, date, amount)
        {
            _invoiceId = invoiceId;
            _sponsorId = sponsorId;
            _sponsorshipId = sponsorshipId;

            TermIds = _termIds.AsReadOnly();
        }

        public void AddTerm(string termId)
        {
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId");
            }
            if (!_termIds.Contains(termId))
            {
                _termIds.Add(termId);
            }
        }
    }
}
