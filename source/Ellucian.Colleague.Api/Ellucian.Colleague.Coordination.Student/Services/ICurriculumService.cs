// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Curriculum services
    /// </summary>
    public interface ICurriculumService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicLevel2>> GetAcademicLevels2Async(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AcademicLevel2> GetAcademicLevelById2Async(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.AccountReceivableType>> GetAccountReceivableTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AccountReceivableType> GetAccountReceivableTypeByIdAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstancesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance> GetAssessmentSpecialCircumstanceByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.CourseLevel2>> GetCourseLevels2Async(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.CourseLevel2> GetCourseLevelById2Async(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.EnrollmentStatus>> GetEnrollmentStatusesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.EnrollmentStatus> GetEnrollmentStatusByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.InstructionalMethod2>> GetInstructionalMethods2Async(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.InstructionalMethod2> GetInstructionalMethodById2Async(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.SectionGradeType>> GetSectionGradeTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.SectionGradeType> GetSectionGradeTypeByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2>> GetSectionRegistrationStatuses2Async(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2> GetSectionRegistrationStatusById2Async(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.StudentStatus>> GetStudentStatusesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.StudentStatus> GetStudentStatusByIdAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.StudentType>> GetStudentTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.StudentType> GetStudentTypeByIdAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.Subject2>> GetSubjects2Async(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.Subject2> GetSubjectByGuid2Async(string guid);

        Task<IEnumerable<Dtos.AcademicPeriodEnrollmentStatus>> GetAcademicPeriodEnrollmentStatusesAsync(bool bypassCache);
        Task<Dtos.AcademicPeriodEnrollmentStatus> GetAcademicPeriodEnrollmentStatusByGuidAsync(string id);

        //V6 Changes
        Task<IEnumerable<Dtos.CreditCategory3>> GetCreditCategories3Async(bool bypassCache);
        Task<Dtos.CreditCategory3> GetCreditCategoryByGuid3Async(string id);

        //V8 changes
        Task<IEnumerable<Dtos.SectionRegistrationStatusItem3>> GetSectionRegistrationStatuses3Async(bool bypassCache);
        Task<Dtos.SectionRegistrationStatusItem3> GetSectionRegistrationStatusById3Async(string id);
    }
}
