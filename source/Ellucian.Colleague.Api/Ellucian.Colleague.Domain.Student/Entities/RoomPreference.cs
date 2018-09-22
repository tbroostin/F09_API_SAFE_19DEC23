// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RoomPreference
    {
        public string Site { get; set; }
        public string SiteReqdFlag { get; set; }
        public string Building { get; set; }
        public string BuildingReqdFlag { get; set; }
        public string Wing { get; set; }
        public string WingReqdFlag { get; set; }
        public string Floor { get; set; }
        public string FloorReqd { get; set; }
        public string Room { get; set; }
        public string RoomReqdFlag { get; set; }
    }
}
