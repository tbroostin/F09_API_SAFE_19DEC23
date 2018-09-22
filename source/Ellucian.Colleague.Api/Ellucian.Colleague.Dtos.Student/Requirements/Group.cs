// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Requirement group
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Id of this group
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Meaningful code for this group
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Text describing this requirement group
        /// </summary>
        public string DisplayText { get; set; }
        /// <summary>
        /// List of courses to select from
        /// </summary>
        public ICollection<string> FromCourses { get; set; }
        /// <summary>
        /// Specific courses
        /// </summary>
        public ICollection<string> Courses { get; set; }
        /// <summary>
        /// Allowed CourseLevels
        /// </summary>
        public ICollection<string> FromLevels { get; set; }  
        /// <summary>
        /// Allowed Departments
        /// </summary>
        public ICollection<string> FromDepartments { get; set; }
        /// <summary>
        /// Allowed Subjects
        /// </summary>
        public ICollection<string> FromSubjects { get; set; }
        /// <summary>
        /// Minimum number of courses per Department
        /// </summary>
        public int? MinCoursesPerDepartment { get; set; }
        /// <summary>
        /// Minimum number of courses per Subject
        /// </summary>
        public int? MinCoursesPerSubject { get; set; }
        /// <summary>
        /// Minimum number of credits
        /// </summary>
        public decimal? MinCredits { get; set; }
        /// <summary>
        /// Minimum credits per Course
        /// </summary>
        public decimal? MinCreditsPerCourse { get; set; }
        /// <summary>
        /// Minimum credits per Department
        /// </summary>
        public decimal? MinCreditsPerDepartment { get; set; }
        /// <summary>
        /// Minimum credits per Subject
        /// </summary>
        public decimal? MinCreditsPerSubject { get; set; }
        /// <summary>
        /// Minimum number of Courses required
        /// </summary>
        public int? MinCourses { get; set; }
        /// <summary>
        /// Minimum number of Departments required
        /// </summary>
        public int? MinDepartments { get; set; }
        /// <summary>
        /// Minimum number of Subjects required
        /// </summary>
        public int? MinSubjects { get; set; }
        /// <summary>
        /// Unallowable Courses
        /// </summary>
        public List<string> ButNotCourses { get; set; }
        /// <summary>
        /// Unallowable CourseLevels
        /// </summary>
        public List<string> ButNotCourseLevels { get; set; }
        /// <summary>
        /// Unallowable Departments
        /// </summary>
        public List<string> ButNotDepartments { get; set; }
        /// <summary>
        /// Unallowable Subjects
        /// </summary>
        public List<string> ButNotSubjects { get; set; }
        /// <summary>
        /// Maximum number of Courses
        /// </summary>
        public int? MaxCourses { get; set; }
        /// <summary>
        /// Maximum number of courses per Department
        /// </summary>
        public int? MaxCoursesPerDepartment { get; set; }
        /// <summary>
        /// Maximum number of courses per Subject
        /// </summary>
        public int? MaxCoursesPerSubject { get; set; }
        /// <summary>
        /// Maximum credits
        /// </summary>
        public decimal? MaxCredits { get; set; }
        /// <summary>
        /// Maximum credits per Course
        /// </summary>
        public decimal? MaxCreditsPerCourse { get; set; }
        /// <summary>
        /// Maximum credits per Department
        /// </summary>
        public decimal? MaxCreditsPerDepartment { get; set; }
        /// <summary>
        /// Maximum credits per Subject
        /// </summary>
        public decimal? MaxCreditsPerSubject { get; set; }
        /// <summary>
        /// Maximum number of Departments
        /// </summary>
        public int? MaxDepartments { get; set; }
        /// <summary>
        /// Maximum number of Subjects
        /// </summary>
        public int? MaxSubjects { get; set; }
        /// <summary>
        /// Maximum Courses at specified CourseLevels
        /// <see cref="MaxCoursesAtLevels"/>
        /// </summary>
        public MaxCoursesAtLevels MaxCoursesAtLevels { get; set; }
        /// <summary>
        /// Maximum Credits at specified CourseLevels
        /// </summary>
        public MaxCreditAtLevels MaxCreditsAtLevels { get; set; }
        /// <summary>
        /// Rules that each AcademicCredit must satisfy
        /// </summary>
        public ICollection<string> AcademicCreditRules { get; set; }
        /// <summary>
        /// Maximum number of courses that must satisfy the MaxCoursesRule
        /// </summary>
        public int? MaxCoursesPerRule { get; set; }
        /// <summary>
        /// Id of the MaxCoursesRule
        /// </summary>
        public string MaxCoursesRule { get; set; }
        /// <summary>
        /// Maximum credits that must satisfy the MaxCreditsRule
        /// </summary>
        public decimal? MaxCreditsPerRule { get; set; }
        /// <summary>
        /// Id of the MaxCreditsRule
        /// </summary>
        public string MaxCreditsRule { get; set; }
        /// <summary>
        /// Boolean is True if any of the rules in AcademicCreditRules collection are defined against AcademicCredit (as opposed to Course)
        /// </summary>
        public bool HasAcademicCreditBasedRules { get; set; }
        /// <summary>
        /// Minimum GPA required to consider this group complete
        /// </summary>
        public decimal? MinGpa { get; set; }
        /// <summary>
        /// Minimum number of institutional credits required to consider this group complete
        /// </summary>
        public decimal? MinInstitutionalCredits { get; set; }
        /// <summary>
        /// Flag indicating whether or not this block is used exclusively to convey print text
        /// </summary>
        public bool OnlyConveysPrintText { get; set; }
    }
}
