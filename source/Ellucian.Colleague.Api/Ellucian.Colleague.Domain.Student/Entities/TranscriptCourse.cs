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
    /// TranscriptCourse is a grouping of data to import a transcript course from Recruiter
    /// </summary>
    [Serializable]
    public class TranscriptCourse
    {
        public string ErpProspectId { get; set; }
        public string ErpInstitutionId { get; set; }
        public string Title { get; set; }
        public string Course { get; set; }
        public string Term { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Grade { get; set; }
        public string InterimGradeFlag { get; set; }
        public string Credits { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string CreatedOn { get; set; }
        public string Source { get; set; }
        public string Comments { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public string RecruiterOrganizationName { get; set; }
        public string RecruiterOrganizationId { get; set; }
    }
}
