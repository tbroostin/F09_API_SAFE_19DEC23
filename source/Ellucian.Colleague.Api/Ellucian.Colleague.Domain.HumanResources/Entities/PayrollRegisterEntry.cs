/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PayrollRegisterEntry represents an entry in the Payroll Register, describing how/what/when
    /// a person was paid. 
    /// </summary>
    [Serializable]
    public class PayrollRegisterEntry
    {
        /// <summary>
        /// The database ID of the entry.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The Start Date of the PayPeriod that this entry paid to.
        /// </summary>
        public DateTime? PayPeriodStartDate { get; private set; }

        /// <summary>
        /// The End Date of the PayPeriod that this entry paid to.
        /// </summary>
        public DateTime PayPeriodEndDate { get; private set; }

        /// <summary>
        /// The Colleague PERSON id of the employee that this entry paid.
        /// </summary>
        public string EmployeeId { get; private set; }

        /// <summary>
        /// The Id of the PayCycle
        /// </summary>
        public string PayCycleId { get; private set; }

        /// <summary>
        /// The sequence number of the entry. Payroll can be run multiple times for the same period, and an entry
        /// is created for each run. The sequence number is incremented on each run. The entry
        /// with the highest sequence number is the entry of record.
        /// </summary>
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// The Reference Id of the Paycheck, also known as the check number.
        /// </summary>
        public string PaycheckReferenceId { get; private set; }

        /// <summary>
        /// The Reference Id of the Pay Statement, also known as the advice number.
        /// </summary>
        public string PayStatementReferenceId { get; private set; }

        /// <summary>
        /// Boolean to indicate if this record is using the 2020 W4 calculation rules.
        /// </summary>
        public bool Apply2020W4Rules { get; private set; }

        /// <summary>
        /// The ReferenceKey is a combination of the PaycheckReferenceId and the PayStatementReferenceId. It
        /// can be used to join a PayrollRegisterEntry to a Pay Statement.
        /// </summary>
        public string ReferenceKey
        {
            get
            {
                var stmtId = PayStatementReferenceId ?? string.Empty;
                var checkId = PaycheckReferenceId ?? string.Empty;
                return string.Format("{0}*{1}", stmtId, checkId);
            }
        }

        /// <summary>
        /// A list of register entries describing the earnings for the payroll period.
        /// </summary>
        public List<PayrollRegisterEarningsEntry> EarningsEntries { get; private set; }

        /// <summary>
        /// A list of register entries describing the taxes paid for the period.
        /// </summary>
        public List<PayrollRegisterTaxEntry> TaxEntries { get; private set; }

        /// <summary>
        /// A list of register entries describing the benefits and deductions that are paid into for the period.
        /// </summary>
        public List<PayrollRegisterBenefitDeductionEntry> BenefitDeductionEntries { get; private set; }

        /// <summary>
        /// A list of register entries describing leave for this payroll period.
        /// </summary>
        public List<PayrollRegisterLeaveEntry> LeaveEntries { get; private set; }

        /// <summary>
        /// List of register entries describing taxable benefits for this payroll period.
        /// </summary>
        public List<PayrollRegisterTaxableBenefitEntry> TaxableBenefitEntries { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="paycheckReferenceId"></param>
        /// <param name="payStatementReferenceId"></param>
        public PayrollRegisterEntry(string id, string employeeId, DateTime? payPeriodStartDate, DateTime payPeriodEndDate, string payCycleId, int sequenceNumber, string paycheckReferenceId, string payStatementReferenceId, bool newW4Flag)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            if (string.IsNullOrEmpty(payCycleId))
            {
                throw new ArgumentNullException("payCycleId");
            }
            if (string.IsNullOrEmpty(paycheckReferenceId) && string.IsNullOrEmpty(payStatementReferenceId))
            {
                throw new ArgumentException("paycheckReferenceId or payStatementReferenceId is required");
            }

            Id = id;
            EmployeeId = employeeId;
            PayCycleId = payCycleId;
            PaycheckReferenceId = paycheckReferenceId;
            PayStatementReferenceId = payStatementReferenceId;
            PayPeriodStartDate = payPeriodStartDate;
            PayPeriodEndDate = payPeriodEndDate;
            SequenceNumber = sequenceNumber;
            Apply2020W4Rules = newW4Flag;

            EarningsEntries = new List<PayrollRegisterEarningsEntry>();
            TaxEntries = new List<PayrollRegisterTaxEntry>();
            BenefitDeductionEntries = new List<PayrollRegisterBenefitDeductionEntry>();
            LeaveEntries = new List<PayrollRegisterLeaveEntry>();
            TaxableBenefitEntries = new List<PayrollRegisterTaxableBenefitEntry>();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            return ((PayrollRegisterEntry)obj).Id == this.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
