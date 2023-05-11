/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// CourseCreditAssociation entity contains each course credit for a specific student/award year/award period 
    /// </summary>
    [Serializable]
    public class CourseCreditAssociation
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom this credit belongs
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// The Award Year this credit belongs to
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// The Award Period that this credit belongs to
        /// </summary>
        public string AwardPeriod { get { return _AwardPeriod; } }
        private readonly string _AwardPeriod;

        /// <summary>
        /// The Course Name for this credit
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// The Course Title for this credit
        /// </summary>
        public string CourseTitle { get; set; }

        /// <summary>
        /// The Course Section number for this course
        /// </summary>
        public string CourseSection { get; set; }

        /// <summary>
        /// The Term that this course belongs to (since there can be many terms to one award period)
        /// </summary>
        public string CourseTerm { get; set; }

        /// <summary>
        /// The number of credits that this course is for (non-FA)
        /// </summary>
        public string CourseCred { get; set; }

        /// <summary>
        /// The number of Inst credits that this course is for
        /// </summary>
        public string CourseInstCred { get; set; }

        /// <summary>
        /// The number of Title IV credits that this course is for
        /// </summary>
        public string CourseTivCred { get; set; }

        /// <summary>
        /// The number of Pell credits that this course is for
        /// </summary>
        public string CoursePellCred { get; set; }

        /// <summary>
        /// The number of Direct Loan credits that this course is for
        /// </summary>
        public string CourseDlCred { get; set; }
        /// <summary>
        /// Flag indicating if the course is included in the student's program
        /// </summary>
        public bool CourseProgFlag { get; set; }
        /// <summary>
        /// Flag indicating if Degree Audit is active for this course's award period
        /// </summary>
        public bool DegreeAuditActiveFlag { get; set; }

        /// <summary>
        /// Create a new CourseCreditAssocation
        /// </summary>
        /// <param name="awardYear">Required: The awardYear to which this credit applies</param>
        /// <param name="studentId">Required: The Colleague PERSON id of the student to whom this credit applies </param>
        /// <param name="awardPeriodId">Required: The award period to which this credit applies</param>
        /// <param name="courseName">The name of this course credit</param>
        /// <param name="courseTitle">The title of this course credit</param>
        /// <param name="courseSection">The section of this course credit</param>
        /// <param name="courseTerm">The term that this course credit belongs to (can/will be different than the award period)</param>
        /// <param name="courseCred">The credit amount that this course is for</param>
        /// <param name="courseInstCred">The Inst credit amount that this course is for</param>
        /// <param name="courseTivCred">The Title IV credit amount that this course is for</param>
        /// <param name="coursePellCred">The Pell credit amount that this course is for</param>
        /// <param name="courseDlCred">The Direct Loan credit amount that this course is for</param>
        /// <param name="courseProgFlag">Y/N flag indicating if the course is active in the student's program</param>
        /// <param name="degreeAuditActiveFlag">Y/N flag indicating if this course's award period is active for Degree Audit</param>
        /// <exception cref="ArgumentNullException">Thrown if any required arguments are null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the campusBasedOriginalAmount argument is less than zero.</exception>
        public CourseCreditAssociation(
            string awardYear,
            string studentId,
            string awardPeriodId,
            string courseName,
            string courseTitle,
            string courseSection,
            string courseTerm,
            string courseCred,
            string courseInstCred,
            string courseTivCred,
            string coursePellCred,
            string courseDlCred,
            string courseProgFlag,
            string degreeAuditActiveFlag)
        {
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardPeriodId))
            {
                throw new ArgumentNullException("awardPeriodId");
            }

            _AwardYear = awardYear;
            _StudentId = studentId;
            _AwardPeriod = awardPeriodId;

            if (!string.IsNullOrEmpty(courseName) && !string.IsNullOrWhiteSpace(courseName))
            {
                CourseName = courseName;
            }
            if (!string.IsNullOrEmpty(courseTitle) && !string.IsNullOrWhiteSpace(courseTitle))
            {
                CourseTitle = courseTitle;
            }
            if (!string.IsNullOrEmpty(courseSection) && !string.IsNullOrWhiteSpace(courseSection))
            {
                CourseSection = courseSection;
            }
            if (!string.IsNullOrEmpty(courseTerm) && !string.IsNullOrWhiteSpace(courseTerm))
            {
                CourseTerm = courseTerm;
            }
            if (!string.IsNullOrEmpty(courseCred) && !string.IsNullOrWhiteSpace(courseCred))
            {
                CourseCred = courseCred;
            }
            if (!string.IsNullOrEmpty(courseInstCred) && !string.IsNullOrWhiteSpace(courseInstCred))
            {
                CourseInstCred = courseInstCred;
            }
            if (!string.IsNullOrEmpty(courseTivCred) && !string.IsNullOrWhiteSpace(courseTivCred))
            {
                CourseTivCred = courseTivCred;
            }
            if (!string.IsNullOrEmpty(coursePellCred) && !string.IsNullOrWhiteSpace(coursePellCred))
            {
                CoursePellCred = coursePellCred;
            }
            if (!string.IsNullOrEmpty(courseDlCred) && !string.IsNullOrWhiteSpace(courseDlCred))
            {
                CourseDlCred = courseDlCred;
            }   
            if (!string.IsNullOrEmpty(courseProgFlag) && !string.IsNullOrWhiteSpace(courseProgFlag))
            {
                CourseProgFlag = (courseProgFlag.ToUpper() == "Y");
            }
            if (!string.IsNullOrEmpty(degreeAuditActiveFlag) && !string.IsNullOrWhiteSpace(degreeAuditActiveFlag))
            {
                DegreeAuditActiveFlag = (degreeAuditActiveFlag.ToUpper() == "Y");
            }
        }
    }
}
