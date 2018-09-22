// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class CreditUsage
    {

        public Usage Usage { get; set; } // Relation type
        public string GroupId { get; set; } // Group to which this credit has been related

        public CreditUsage(Usage usage)
        {
            Usage = usage;
        }
        public override string ToString()
        {
            return GroupId + " " + Usage.ToString();
        }
    }

    [Serializable]
    public enum Usage
    {
        Unapplied,
        Applied,
        Related
    }
}
