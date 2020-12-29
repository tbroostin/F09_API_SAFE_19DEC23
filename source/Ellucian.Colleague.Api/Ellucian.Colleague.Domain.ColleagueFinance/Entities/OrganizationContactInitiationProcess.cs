// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class OrganizationContactInitiationProcess
    {
        public string VendorId { get; set; }
        public string PersonId { get; set; }
        public string RelationshipType { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public List<ContactPhoneInfo> PhoneInfos { get; set; }
        public ContactEmailInfo EmailInfo { get; set; }
        public string RequestType { get; set; }
    }
}