// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Similar to a CreditResult, holds result of an evaluation of a planned course against a group
    /// </summary>
    [Serializable]
    public class CourseResult : AcadResult
    {
        private readonly PlannedCredit _PlannedCourse;
        public PlannedCredit PlannedCourse { get { return _PlannedCourse; } }

        public CourseResult(PlannedCredit plannedCourse)
        {
            if (plannedCourse == null)
            {
                throw new ArgumentNullException("plannedCourse");
            }

            _PlannedCourse = plannedCourse;
            Result = Result.Untested;
        }

        public override AcademicCredit GetAcadCred()
        {
            return null;
        }
        public override string GetSubject()
        {
            return PlannedCourse.Course.SubjectCode;
        }
        public override IEnumerable<string> GetDepartments()
        {
            return PlannedCourse.Course.Departments.Select(d => d.AcademicDepartmentCode);
        }
        public override IEnumerable<string> GetCourseLevels()
        {
            return PlannedCourse.Course.CourseLevelCodes;
        }
        public override Grade GetGrade()
        {
            return null;
        }
        public override Course GetCourse()
        {
            return PlannedCourse.Course;
        }
        public override bool IsInstitutional()
        {
            return false; //  Planned courses are never going to be institutional credit.
        }

        public override decimal GetCredits()
        {
            if (PlannedCourse.Credits.HasValue) { return (decimal)PlannedCourse.Credits; }
            if (PlannedCourse.Course.MinimumCredits != null) { return (decimal)PlannedCourse.Course.MinimumCredits; }
            if (PlannedCourse.Course.Ceus != null) { return (decimal)PlannedCourse.Course.Ceus; }
            return 0m;
        }

        public override decimal GetAdjustedCredits()
        {
            return 0m;
        }
        public override decimal GetCompletedCredits()
        {
            return 0m;
        }
        public override decimal GetGradePoints()
        {
            return 0m;
        }
        public override decimal GetGpaCredit()
        {
            return 0m;
        }



        public override string ToString()
        {
            return "PlnCrs " + PlannedCourse.Course.SubjectCode + "*" + PlannedCourse.Course.Number;
        }
        public override string GetAcadCredId()
        {
            return null;
        }

        public override string GetTermCode()
        {
            return PlannedCourse.TermCode;
        }

        public override string GetSectionId()
        {
            return PlannedCourse.SectionId;
        }

        /// <summary>
        /// Determines whether the specified CourseResult object is equal to this object
        /// by comparing the academic credit id.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is CreditResult || (obj as CourseResult).PlannedCourse.Course == null || PlannedCourse.Course == null)
            {
                return false;
            }
            else
            {
                return (obj as CourseResult).GetHashCode().Equals(this.GetHashCode());
            }
        }

        /// <summary>
        /// GetHashCode intentionally allowed to default object GetHashCode()
        /// </summary>
        /// <returns></returns>

    }
}

