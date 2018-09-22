// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Configuration
{
    [Serializable]
    public class AvailablePaymentMethod
    {
        public AvailablePaymentMethod()
        {
            InternalCode = string.Empty;
            Description = string.Empty;
            Type = string.Empty;
        }

        public string InternalCode { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }
    }
}
