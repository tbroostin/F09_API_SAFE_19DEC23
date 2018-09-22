// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class CheckPayment
    {
        public string AbaRoutingNumber { get; set; }

        public string BankAccountNumber { get; set; }

        public string CheckNumber { get; set; }

        public string DriversLicenseNumber { get; set; }

        public string DriversLicenseState { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string BillingAddress1 { get; set; }

        public string BillingAddress2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string EmailAddress { get; set; }
    }
}
