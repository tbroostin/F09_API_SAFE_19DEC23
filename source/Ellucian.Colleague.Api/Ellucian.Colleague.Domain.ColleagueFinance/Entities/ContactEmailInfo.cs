// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class ContactEmailInfo
    {
        public string EmailType { get; set; }
        public string EmailAddress { get; set; }
    }
}