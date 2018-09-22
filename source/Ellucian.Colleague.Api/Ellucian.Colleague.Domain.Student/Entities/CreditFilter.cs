using Ellucian.Colleague.Domain.Base.Entities;
// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Filters what credits can apply to a Program.  Equivalent to a Colleague Transcript Grouping.
    /// </summary>
    [Serializable]
    public class CreditFilter
    {
        /// <summary>
        /// List of academic levels to be included. If empty, all academic levels allowed
        /// </summary>
        public List<string> AcademicLevels { get; set; }
        /// <summary>
        /// List of departments to be included. If empty, all departments allowed
        /// </summary>
        public List<string> Departments { get; set; }
        /// <summary>
        /// List of divisions to be included , If emty, all divisions allowed
        /// </summary>
        public List<string> Divisions { get; set; }
        /// <summary>
        /// List of schools to be included , If emty, all divisions allowed
        /// </summary>
        public List<string> Schools { get; set; }
        /// <summary>
        /// List of locations to be included , If emty, all divisions allowed
        /// </summary>
        public List<string> Locations { get; set; }
        /// <summary>
        /// List of course levels to be included. If empty, all course levels allowed
        /// </summary>
        public List<string> CourseLevels { get; set; }
        /// <summary>
        /// The list of credits types (not credit type categories) to be included. If empty, all credit types allowed
        /// </summary>
        public List<string> CreditTypes { get; set; }
        /// <summary>
        /// The list of subjects to be included. If empty, all subjects allowed
        /// </summary>
        public List<string> Subjects { get; set; }
        /// <summary>
        /// The list of marks to be included. If empty, all marks allowed
        /// </summary>
        public List<string> Marks { get; set; }
        /// <summary>
        /// Indicates whether to include items that have no grade scheme, therefore are never graded.
        /// </summary>
        public bool IncludeNeverGradedCredits { get; set; }


        public CreditFilter()
        {
            AcademicLevels = new List<string>();
            Departments = new List<string>();
            Divisions = new List<string>();
            Schools = new List<string>();
            Locations = new List<string>();
            CourseLevels = new List<string>();
            CreditTypes = new List<string>();
            Subjects = new List<string>();
            Marks = new List<string>();
            IncludeNeverGradedCredits = true;
        }


        public bool Passes(Course plannedcourse)
        {

            if (AcademicLevels.Count > 0)
            {
                if (!AcademicLevels.Contains(plannedcourse.AcademicLevelCode))
                {
                    return false;
                }
            }

            if (Departments.Count > 0)
            {
                if ((plannedcourse.DepartmentCodes.Count() > 0) &&
                    (plannedcourse.DepartmentCodes.Intersect(this.Departments).Count() == 0))
                {
                    return false;
                }
            }
            if (CourseLevels.Count > 0 && plannedcourse.CourseLevelCodes.Count() > 0)
            {
                if (CourseLevels.Intersect(plannedcourse.CourseLevelCodes).Count() == 0)
                {
                    return false;
                }
            }

            if (Subjects.Count > 0)
            {
                if (!string.IsNullOrEmpty(plannedcourse.SubjectCode) && (!Subjects.Contains(plannedcourse.SubjectCode)))
                {
                    return false;
                }
            }

            if (CreditTypes.Count > 0)
            {
                if (!CreditTypes.Contains(plannedcourse.LocalCreditType))
                {
                    return false;
                }
            }
            //filter out locations
            if (Locations != null && Locations.Count > 0 )
            {
                if (plannedcourse.LocationCodes==null || plannedcourse.LocationCodes.Count == 0 || !plannedcourse.LocationCodes.Intersect(this.Locations).Any())
                {
                    return false;
                }
               
            }
            return true;
        }

        /// <summary>
        /// Filters academic credits being used in a student Program evaluation based on the program's transcript grouping filters
        /// along with any other factors that should exclude the academic credit.
        /// </summary>
        /// <param name="academiccredit">Academic Credit being filtered</param>
        /// <returns></returns>
        public bool Passes(AcademicCredit academiccredit)
        {
            if (AcademicLevels.Count > 0)
            {
                if (academiccredit.AcademicLevelCode != null)
                {
                    if (!AcademicLevels.Contains(academiccredit.AcademicLevelCode))
                    {
                        return false;
                    }
                }
            }
            if (Departments.Count > 0)
            {
                if ((academiccredit.DepartmentCodes.Count() > 0) &&
                    (academiccredit.DepartmentCodes.Intersect(this.Departments).Count() == 0))
                {
                    return false;
                }
            }

            //Filter on location , divisions, schools
            //logic is to pick only those credits that have exact match for settings in TRGR.
            //if location,divisions,schools are  empty or do not exist in TRGR, do not pick.

            if (Locations != null && Locations.Count > 0)
            {
                if (string.IsNullOrEmpty(academiccredit.Location) || !Locations.Contains(academiccredit.Location))
                {
                    return false;

                }
            }
           
            if (Divisions != null && Divisions.Count > 0)
            {
                if (academiccredit.Divisions==null || academiccredit.Divisions.Count == 0 || !academiccredit.Divisions.Intersect(this.Divisions).Any())
                {
                    return false;
                }
            }
            if (Schools != null && Schools.Count > 0)
            {
                if (academiccredit.Schools==null || academiccredit.Schools.Count == 0 || !academiccredit.Schools.Intersect(this.Schools).Any())
                {
                    return false;
                }
            }

            if (CourseLevels.Count > 0)
            {
                if (!string.IsNullOrEmpty(academiccredit.CourseLevelCode) && (!CourseLevels.Contains(academiccredit.CourseLevelCode)))
                {
                    return false;
                }
            }
            if (CreditTypes.Count > 0)
            {
                // Academic credit "LocalType" carries the credit type code. 
                if (!CreditTypes.Contains(academiccredit.LocalType))
                {
                    return false;
                }
            }
            if (Subjects.Count > 0)
            {
                if (!string.IsNullOrEmpty(academiccredit.SubjectCode) && (!Subjects.Contains(academiccredit.SubjectCode)))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(academiccredit.Mark) && (!Marks.Contains(academiccredit.Mark)))
            {
                return false;
            }

            // If credit filter says that never graded credits are to be excluded, check to see if the academic credit has a grade scheme and if
            // it doesn't do not include it. 
            if (!IncludeNeverGradedCredits && string.IsNullOrEmpty(academiccredit.GradeSchemeCode))
            {
                return false;
            }

            // We never want to include withdrawn or dropped credits that are ungraded to the evaluation.  It is as if they never took them.
            // Only the graded withdrawals and drops will be used in evaluation (and ultimately they will likely just be listed as other)
            if ((academiccredit.Status == CreditStatus.Withdrawn || academiccredit.Status == CreditStatus.Dropped) && academiccredit.VerifiedGrade == null)
            {
                return false;
            }


            return true;
        }

        public bool Passes(Tuple<IEnumerable<Department>,IEnumerable<string>> deptAndSchools)
        {
            if (deptAndSchools == null)
            {
                return false;
            }
            IEnumerable<Department> departments = deptAndSchools.Item1;
            IEnumerable<string> deptSchools = deptAndSchools.Item2;
         
            if (Divisions != null && Divisions.Count > 0)
            {
                if (departments == null)
                {
                    return false;
                }
                IEnumerable<string> divisions = departments.Where(ad => !string.IsNullOrEmpty(ad.Division)).Select(a => a.Division);
                if (divisions == null || !divisions.Any() || !Divisions.Intersect(divisions).Any())
                {
                    return false;
                }
            }
            if (Schools != null && Schools.Count > 0)
            {
                if (deptSchools.Any() && deptSchools != null)
                {
                    IEnumerable<string> schools = deptSchools.Where(ad => !string.IsNullOrEmpty(ad));


                    if (schools == null || !schools.Any() || !Schools.Intersect(schools).Any())
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
