// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonVisaResponse
    {
        public string PersonId { get; set; }
        public string StrGuid { get; set; }
    }
}
