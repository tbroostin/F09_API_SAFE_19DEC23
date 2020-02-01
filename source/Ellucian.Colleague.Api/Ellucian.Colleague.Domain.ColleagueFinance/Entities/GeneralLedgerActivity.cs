// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This class expose the transactional activities stored within GLA.FYR (GL activity/actuals).
    /// </summary>
    [Serializable]
    public class GeneralLedgerActivity
    {
        public string RecordGuid { get; private set; }
        public string RecordKey { get; private set; }
        public string Description { get; private set; }
        public DateTime? EnteredOn { get; private set; }
        public string AccountingString { get; private set; }
        public decimal? Debit { get; private set; }
        public decimal? Credit { get; private set; }
        public DateTime? TransactionDate { get; set; }
        public string GlaSource { get; set; }
        public string GlaRefNumber { get; set; }
        public string ReportingSegment { get; set; }
        public string GlaAccountId { get; set; }
        public string GlaCorpFlag { get; set; }
        public string GlaInstFlag { get; set; }
        public string HostCountry { get; set; }
        public string ProjectId { get; set; }
        public string ProjectRefNo { get; set; }
        public string ProjectGuid { get; set; }
        public string GrantsGuid { get; set; }
        public string AccountingStringGuid { get; set; }
        public bool IsPooleeAcct { get; set; }

        public GeneralLedgerActivity(string guid, string id, string description, DateTime? enteredOn, DateTime? transactionDate, decimal? debit, decimal? credit)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Format("Guid is required. Id: {0}", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Format("Accounting string is required. Guid: {0}", guid));
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException(string.Format("Description is required. Guid: {0}", guid));
            }
            if (!transactionDate.HasValue)
            {
                throw new ArgumentNullException(string.Format("Transaction date is required. Guid: {0}", guid));
            }
            //if (debit == 0) debit = null;
            //if (credit == 0) credit = null;
            if ((!debit.HasValue && !credit.HasValue) || (debit == 0 && credit == 0))
            {
                throw new ArgumentNullException("Credit/Debit value is required.");
            }

            RecordGuid = guid;
            RecordKey = id;
            Description = description;
            EnteredOn = enteredOn.HasValue? enteredOn : transactionDate.Value;
            TransactionDate = transactionDate;
            AccountingString = id;
            Debit = debit;
            Credit = credit;
        }
    }
}
