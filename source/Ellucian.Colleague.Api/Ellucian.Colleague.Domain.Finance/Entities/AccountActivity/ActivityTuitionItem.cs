// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityTuitionItem : ActivityTermItem
    {
        public string Classroom { get; set; }

        public decimal? Credits { get; set; }

        public decimal? BillingCredits { get; set; }

        public decimal? Ceus { get; set; }

        //public string Days { get; set; }
        public List<DayOfWeek> Days { get; set; }

        public string Instructor { get; set; }

        public string Status { get; set; }

        //public string Times { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
