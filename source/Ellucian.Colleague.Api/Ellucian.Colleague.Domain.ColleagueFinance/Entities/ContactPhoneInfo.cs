// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class ContactPhoneInfo
    {
        public string PhoneType { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneExtension { get; set; }
    }
}