// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

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
                throw new ArgumentException(string.Format("GUID is required. Id: '{0}'", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(string.Format("Accounting string is required. GUID: '{0}'", guid));
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException(string.Format("Description is required. GUID: '{0}'", guid));
            }
            if (!transactionDate.HasValue)
            {
                throw new ArgumentException(string.Format("Transaction date is required. GUID: '{0}'", guid));
            }
            // If either credit or debit fields have a value of zero, then null it out
            // If they both have zero, then leave zero in the credit field and let the transaction go through
            // This is really bad data but we don't want to report on it as such because some Colleague
            // processes are causing the data to have a zero transaction and we can't remove them or do anything
            // to modify then, therefore, we don't want to issue an error response to something that they cannot fix or change.
            // SRM - 03/29/2021
            if (credit.HasValue && credit.Value == 0 && debit.HasValue && debit.Value != 0) credit = null;
            if (debit.HasValue && debit.Value == 0 && credit.HasValue && credit.Value != 0) debit = null;
            if (debit.HasValue && debit.Value == 0 && credit.HasValue && credit.Value == 0) debit = null;
            if ((!debit.HasValue && !credit.HasValue) || (debit == 0 && credit == 0))
            {
                throw new ArgumentException(string.Format("Credit/Debit value is required. GUID: '{0}'", guid));
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
