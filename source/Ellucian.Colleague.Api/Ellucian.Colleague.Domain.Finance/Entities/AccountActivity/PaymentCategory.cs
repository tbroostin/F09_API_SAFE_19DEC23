// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// This entire class is no longer needed since all of the information contained within
    /// is now being stored directly in the AccountPeriod class.
    /// </summary>
    [Serializable]
    public partial class PaymentCategory
    {
        /*
         * Remove the Applied Deposits property below because it's
         * now being used directly in the AccountPeriod class as the 'Deposit'
         * property.
         *
        [DataMember]
        public List<DepositItem> AppliedDeposits { get; set; }

        [DataMember]
        public List<DateTermItem> FinancialAid { get; set; }
        
        [DataMember]
        public List<SponsorPaymentItem> SponsorPayments { get; set; }

        [DataMember]
        public List<PaymentPaidItem> StudentPayments { get; set; }
         */
    }
}