// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// TestScores is a grouping of data to import a test score from Recruiter.
    /// </summary>
    [Serializable]
    public class TestScore
    {
        public string ErpProspectId;
        public string TestType { get; set; }
        public string TestDate { get; set; }
        public string Source { get; set; }
        public string SubtestType { get; set; }
        public string Score { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public string RecruiterOrganizationName;
        public string RecruiterOrganizationId;
    }
}
