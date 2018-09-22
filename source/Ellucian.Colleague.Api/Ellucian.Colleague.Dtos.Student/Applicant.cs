// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Applicant DTO
    /// </summary>
    public class Applicant : Person
    {
        /// <summary>
        /// Id of the student's Financial Aid counselor. If empty or null, the applicant has not been
        /// assigned a counselor yet.
        /// </summary>
        public string FinancialAidCounselorId { get; set; }

        /// <summary>
        /// Preferred email address of student
        /// </summary>
        public string PreferredEmailAddress { get; set; }
    }
}
