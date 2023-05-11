// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student
{
    [Serializable]
    public static class PlanningPermissionCodes
    {
        // Advisor - advisee list
        public const string ViewAssignedAdvisees = "VIEW.ASSIGNED.ADVISEES";
        public const string ViewAnyAdvisee = "VIEW.ANY.ADVISEE";

        // THE FOLLOWING TWO PERMISSIONS ARE BEING MADE OBSOLETE IN VERSION 1.2 OF THE API.
        public const string ViewAdviseeDegreePlan = "VIEW.ADVISEE.DEGREE.PLAN";
        public const string UpdateAdviseeDegreePlan = "UPDATE.ADVISEE.DEGREE.PLAN";
        public const string UpdateAdvisorAssignments = "UPDATE.ADVISOR.ASSIGNMENTS";

        // Advisor Permissions as of Version 1.2
        // Review permissions allow advisor to approve or deny courses and add notes to a plan only.
        public const string ReviewAnyAdvisee = "REVIEW.ANY.ADVISEE";
        public const string ReviewAssignedAdvisees = "REVIEW.ASSIGNED.ADVISEES";

        // Update permissions allow the advisor to approve or deny courses, add notes or make changes to the plan
        // such as adding and removing terms, planned courses, planned sections etc.
        public const string UpdateAnyAdvisee = "UPDATE.ANY.ADVISEE";
        public const string UpdateAssignedAdvisees = "UPDATE.ASSIGNED.ADVISEES";

        // All Access permissions all the advisor to approve, deny courses, add notes, make changes to the plan
        // and do all registration functions such as register, add/drop, and waitlist courses for the student.
        public const string AllAccessAnyAdvisee = "ALL.ACCESS.ANY.ADVISEE";
        public const string AllAccessAssignedAdvisees = "ALL.ACCESS.ASSIGNED.ADVISEES";

        //permission given to an applicant to run program evalautions and what-if
        public const string ApplicantEvaluateWhatIf = "EVALUATE.WHAT.IF";
    }
}
