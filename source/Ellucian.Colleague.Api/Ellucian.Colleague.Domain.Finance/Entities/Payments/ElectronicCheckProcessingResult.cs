﻿// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class ElectronicCheckProcessingResult
    {
        public string CashReceiptsId { get; set; }

        public string ErrorMessage { get; set; }
    }
}
