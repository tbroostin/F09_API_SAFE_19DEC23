// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A receipt session entity
    /// </summary>
    [Serializable]
    public class ReceiptSession
    {
        private readonly string _id;
        /// <summary>
        /// Identifier of the receipt session
        /// </summary>
        public string Id { get { return _id; } }

        private readonly SessionStatus _status;
        /// <summary>
        /// Current status of the receipt session
        /// </summary>
        public SessionStatus Status { get { return _status; } }

        private readonly string _cashierId;
        /// <summary>
        /// Identifier of the session's cashier
        /// </summary>
        public string CashierId { get { return _cashierId; } }

        private readonly bool _isECommerceEnabled;
        /// <summary>
        /// Can the session process eCommerce transactions?
        /// </summary>
        public bool IsECommerceEnabled { get { return _isECommerceEnabled; } }

        private readonly string _locationId;
        /// <summary>
        /// Location of the session
        /// </summary>
        public string LocationId { get { return _locationId; } }

        private readonly DateTime _receiptDate;
        /// <summary>
        /// Default date of receipts in this session
        /// </summary>
        public DateTime ReceiptDate { get { return _receiptDate; } }

        private readonly DateTimeOffset _start;
        /// <summary>
        /// The starting date and time of the session
        /// </summary>
        public DateTimeOffset Start { get { return _start; } }

        private DateTimeOffset? _end;
        /// <summary>
        /// The ending date and time of the session
        /// </summary>
        public DateTimeOffset? End
        {
            get { return _end; }
            set
            {
                if (_end.HasValue)
                {
                    throw new InvalidOperationException("End date/time already defined for receipt session.");
                }
                _end = value;
            }
        }

        /// <summary>
        /// Public constructor for a session
        /// </summary>
        /// <param name="id">The identifier of the session</param>
        /// <param name="status">Current status of the receipt session</param>
        /// <param name="cashierId">The person ID of the cashier starting the session</param>
        /// <param name="receiptDate">The date stamped on issued receipts</param>
        /// <param name="start">The starting date and time of the session</param>
        /// <param name="isEComm">Indicates if the session is e-commerce enabled</param>
        /// <param name="locationId">The location of the session</param>
        public ReceiptSession(string id, SessionStatus status, string cashierId, DateTime receiptDate, DateTimeOffset start, bool isEComm, string locationId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Receipt session id cannot be null.");
            }
            if (string.IsNullOrEmpty(cashierId))
            {
                throw new ArgumentNullException("cashierId", "Cashier id cannot be null.");
            }
            if (string.IsNullOrEmpty(locationId) && isEComm)
            {
                throw new ArgumentNullException("locationId", "Location cannot be null if e-Commerce is enabled.");
            }
            if (receiptDate == default(DateTime))
            {
                throw new ArgumentNullException("receiptDate", "Receipt date cannot be null.");
            }
            if (start == default(DateTimeOffset))
            {
                throw new ArgumentNullException("start", "Start date/time cannot be null.");
            }

            _id = id;
            _status = status;
            _cashierId = cashierId;
            _isECommerceEnabled = isEComm;
            _locationId = locationId;
            _receiptDate = receiptDate;
            _start = start;
        }
    }
}
