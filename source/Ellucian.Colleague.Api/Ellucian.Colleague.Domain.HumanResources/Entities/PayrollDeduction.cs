/* Copyright 2016 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PayrollDeduction
    {
        /// <summary>
        /// The database Guid
        /// </summary>
        public string Guid { get; set; }
       
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The arrangement database Guid
        /// </summary>
        public string ArrangementGuid { get; set; }
        
        /// <summary>
        /// The arrangement database Id
        /// </summary>
        public string ArrangementId { get { return arrangementId; } }
        private readonly string arrangementId;

        /// <summary>
        /// The deduction date
        /// </summary>
        public DateTime DeductionDate { get { return deductionDate; } }
        private readonly DateTime deductionDate;

        /// <summary>
        /// The amount currency type
        /// </summary>
        public string AmountCountry { get { return amountCountry; } }
        private readonly string amountCountry;

        /// <summary>
        /// The deduction amount
        /// </summary>
        public decimal Amount { get { return amount; } }
        private readonly decimal amount;

        /// <summary>
        /// Constructor for PayrollDeduciton
        /// </summary>
        /// <param name="id">id of the record</param>
        /// <param name="guid">guid for the record</param>
        /// <param name="arrangementId">id for the arrangment</param>
        /// <param name="arrangementGuid">guid for the arrangement</param>
        /// <param name="deductionDate">deduction date of the contribution</param>
        /// <param name="currencyCountry">currency country</param>
        /// <param name="amount">amount of the deduction</param>
        public PayrollDeduction(string guid, string id, string arrangementId, string arrangementGuid, DateTime deductionDate, string currencyCountry, decimal amount)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException("Guid is required to create a new PayrollDeduction");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id is required to create a new PayrollDeduction");
            }

            if (string.IsNullOrEmpty(arrangementId))
            {
                throw new ArgumentException("Arrangement Id is required to create a new PayrollDeduction");
            }

            if (string.IsNullOrEmpty(arrangementGuid))
            {
                throw new ArgumentException("Arrangement Guid is required to create a new PayrollDeduction");
            }

            if (string.IsNullOrEmpty(currencyCountry))
            {
                throw new ArgumentException("Country Currency is required to create a new PayrollDeduction");
            }

            if (deductionDate == default(DateTime))
            {
                throw new ArgumentException("Deduction Date is required to create a new PayrollDeduction");
            }

            Guid = guid;
            this.id = id;
            this.arrangementId = arrangementId;
            ArrangementGuid = arrangementGuid;
            this.deductionDate = deductionDate;
            this.amountCountry = currencyCountry;
            this.amount = amount;
        }
    }
}
