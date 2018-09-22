// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonHoldResponse
    {
        public string WarningCode { get; set; }
        public string WarningMessage { get; set; }
        public string PersonHoldId { get; set; }
        public string PersonHoldGuid { get; set; }
    }
}
