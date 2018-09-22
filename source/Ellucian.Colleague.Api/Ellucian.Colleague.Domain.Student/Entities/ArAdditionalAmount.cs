// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ArAdditionalAmount
    {
        public string Recordkey { get; set; }
        public string AraaArCode { get; set; }
        public Decimal? AraaChargeAmt { get; set; }
        public Decimal? AraaCrAmt { get; set; }
        public string AraaRoomAssignmentId { get; set; }
    }
}
