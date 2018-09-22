// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class ElectronicCheckPayer
    {
        
        public string FirstName { get; set; }

        
        public string MiddleName { get; set; }

        
        public string LastName { get; set; }

        
        public string Street { get; set; }

        
        public string City { get; set; }

        
        public string State { get; set; }

        
        public string PostalCode { get; set; }

        
        public string Country { get; set; }

        
        public string Email { get; set; }

        
        public string Telephone { get; set; }
    }
}
