//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmissionApplicationStatus
    {

        public string ApplicationDecisionBy { get; set; }

        public string ApplicationStatus { get; set; }

        public DateTime? ApplicationStatusDate { get; set; }

        public DateTime? ApplicationStatusTime { get; set; }
    }
}
