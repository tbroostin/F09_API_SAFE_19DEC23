// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CampusOrgType : CodeItem
    {
        public bool PilotFlag { get; set; }

        public CampusOrgType(string code, string desc, bool pilotFlag)
            : base(code, desc)
        {
            PilotFlag = pilotFlag;
        }
    }
}
