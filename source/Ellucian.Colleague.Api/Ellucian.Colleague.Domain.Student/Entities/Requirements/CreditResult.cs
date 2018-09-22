// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class CreditResult : AcadResult
    {
        private readonly AcademicCredit _Credit;
        public AcademicCredit Credit { get { return _Credit; } }

        public CreditResult(AcademicCredit credit)
        {

            if (credit == null)
            {
                throw new ArgumentNullException("credit");
            }
            _Credit = credit;
            Result = Result.Untested;
            
        }

        public override AcademicCredit GetAcadCred()
        {
            return _Credit;
        }
        public override string GetSubject()
        {
            return Credit.SubjectCode;
        }
        public override IEnumerable<string> GetDepartments()
        {
            return Credit.DepartmentCodes;
        }
        public override IEnumerable<string> GetCourseLevels()
        {
            return new List<string>() { Credit.CourseLevelCode };
        }
        public override Grade GetGrade()
        {
            return Credit.VerifiedGrade;
        }
        public override Course GetCourse()
        {
            return Credit.Course;
        }
        public override bool IsInstitutional()
        {
            return Credit.IsInstitutional();
        }

        public override decimal GetCredits()
        {
            return Credit.Credit;
        }
        public override decimal GetAdjustedCredits()
        {
            if (Credit.IsCompletedCredit)
            {
                return Credit.AdjustedCredit ?? 0m;
            }
            return Credit.Credit;
        }
        public override decimal GetCompletedCredits()
        {
            return Credit.CompletedCredit??0m;
        }
        public override decimal GetGradePoints()
        {
            return Credit.AdjustedGradePoints;
        }
        public override decimal GetGpaCredit()
        {
            return Credit.AdjustedGpaCredit;
        }


    
        public override string ToString()
        {
            return "AcadCred " + Credit.CourseName;
        }
        public override string GetAcadCredId()
        {
            return Credit.Id;
        }

        public override string GetTermCode()
        {
            return Credit.TermCode;
        }

        public override string GetSectionId()
        {
            return Credit.SectionId;
        }

        /// <summary>
        /// Determines whether the specified CreditResult object is equal to this object
        /// by comparing the academic credit id.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is CourseResult || (obj as CreditResult).GetAcadCred() == null || this.GetAcadCred() == null)
            {
                return false;
            }
            return (obj as CreditResult).GetAcadCredId().Equals(this.GetAcadCredId());
        }

        public override int GetHashCode()
        {
            return GetAcadCredId().GetHashCode();
        }
    }
}
