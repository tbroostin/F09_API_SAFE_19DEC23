// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public abstract class AcadResult
    {
        public string GroupId;
        public Result Result { get; set; }
        public AcadResultExplanation Explanation { get; set; }

        protected AcadResult() { }

        public abstract string GetSubject();
        public abstract IEnumerable<string> GetDepartments();
        public abstract IEnumerable<string> GetCourseLevels();
        public abstract Grade GetGrade();
        public abstract Course GetCourse();
        public abstract bool IsInstitutional();
        public abstract string GetAcadCredId();
        public abstract AcademicCredit GetAcadCred();

        //  This may need refactoring
        public abstract decimal GetCredits();
        public abstract decimal GetAdjustedCredits();
        public abstract decimal GetCompletedCredits();
        public abstract decimal GetGradePoints();
        public abstract decimal GetGpaCredit();

        public abstract string GetTermCode();
        public abstract string GetSectionId();

    }

    /// <summary>
    /// Enumerates the various result statuses possible for an academic result.
    /// </summary>
    [Serializable]
    public enum Result
    {
        Applied, 
        PlannedApplied,
        Related,

        NotInCoursesList,
        NotInFromCoursesList,

        MaxDepartments,
        MaxCourses,
        MaxSubjects,
        MaxCoursesPerSubject,
        MaxCoursesPerDepartment,
        MaxCoursesAtLevel,
        MaxCoursesPerRule,
        MaxCredits,
        MaxCreditsPerCourse,
        MaxCreditsPerSubject,
        MaxCreditsPerDepartment,
        MaxCreditsAtLevel,
        MaxCreditsPerRule,
        MinCreditsPerCourse,
        MinGrade,

        FromWrongDepartment,
        FromWrongSubject,
        FromWrongLevel,

        CourseExcluded,
        DepartmentExcluded,
        SubjectExcluded,
        LevelExcluded, 
        Untested, 
        ExcludedByOverride, 
        RuleFailed,
        MinInstitutionalCredit,
        MinGPA,

        InCoursesListButAlreadyApplied,
        
        ReplacedWithGPAValues,
        Replaced
    }
    /// <summary>
    /// This is to specify explanation associated with AcadResult. By default this will be 'None'. 
    /// 'Extra' is used to identify if AcadCredit is extra within group
    /// ExtraInGroup is used to identify AcadCredit if group that it belongs to was marked as Extra.
    /// These Explanations are only needed for Extra Course Handling
    /// </summary>
    [Serializable]
    public enum AcadResultExplanation
    {
        None,
        Extra,
        ExtraInGroup

    }
}