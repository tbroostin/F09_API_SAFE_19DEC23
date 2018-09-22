// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CampusOrgRole : CodeItem
    {
        public string PilotPriority { get; set; }

        public CampusOrgRole(string code, string desc, string pilotPriority)
            :   base(code, desc)
        {
            PilotPriority = pilotPriority;
        }
    }
}
