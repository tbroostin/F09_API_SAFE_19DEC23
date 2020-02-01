// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Provides read-only access to the fundamental data necessary for the Student Self Service system to function.
    /// </summary>
    public interface IStudentReferenceDataRepository
    {
        /// <summary>
        /// Academic departments
        /// </summary>
        Task<IEnumerable<AcademicDepartment>> GetAcademicDepartmentsAsync();

        /// <summary>
        /// Get a collection of academic departments
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of academic departments</returns>
        Task<IEnumerable<AcademicDepartment>> GetAcademicDepartmentsAsync(bool ignoreCache);

        /// <summary>
        /// Graduate, undergraduate, etc.
        /// </summary>
        Task<IEnumerable<AcademicLevel>> GetAcademicLevelsAsync();

        /// <summary>
        /// Get guid for AcademicLevels code
        /// </summary>
        /// <param name="code">AcademicLevels code</param>
        /// <returns>Guid</returns>
        Task<string> GetAcademicLevelsGuidAsync(string code);

        /// <summary>
        /// BA History, MA Engineering, etc.
        /// </summary>
        Task<IEnumerable<AcademicProgram>> GetAcademicProgramsAsync(bool ignoreCache = false);

        /// <summary>
        /// Get guid for AcademicPrograms code
        /// </summary>
        /// <param name="code">AcademicPrograms code</param>
        /// <returns>Guid</returns>
        Task<string> GetAcademicProgramsGuidAsync(string code);

        /// <summary>
        /// BA History, MA Engineering, etc.
        /// </summary>
        Task<AcademicProgram> GetAcademicProgramByGuidAsync(string guid);

        /// <summary>
        /// Get a collection of admission residency type
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of admission residency types</returns>
        Task<IEnumerable<AdmissionResidencyType>> GetAdmissionResidencyTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for AdmissionResidencyTypes code
        /// </summary>
        /// <param name="code">AdmissionResidencyTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetAdmissionResidencyTypesGuidAsync(string code);

        /// <summary>
        /// Advisor Types such as Major, Minor, General, Academic, etc.
        /// </summary>
        Task<IEnumerable<AdvisorType>> GetAdvisorTypesAsync(bool ignoreCache = false);

        /// <summary>
        /// Get guid for AdvisorTypesGuid code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<string> GetAdvisorTypeGuidAsync(string code);

        ///// <summary>
        ///// Get a collection of AdvisorTypes
        ///// </summary>
        ///// <param name="ignoreCache">Bypass cache flag</param>
        ///// <returns>Collection of AdvisorTypes</returns>
        //Task<IEnumerable<AdvisorType2>> GetAdvisorTypesAsync(bool ignoreCache);

        /// <summary>
        /// Academic Standings such as Good, Honors, Probation, etc.
        /// </summary>
        Task<IEnumerable<AcademicStanding>> GetAcademicStandingsAsync();

        /// <summary>
        /// Academic Standings such as Good, Honors, Probation, etc.
        /// </summary>
        Task<IEnumerable<AcademicStanding2>> GetAcademicStandings2Async(bool ignoreCache);

        /// <summary>
        /// Get a collection of academic levels
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of academic levels</returns>
        Task<IEnumerable<AcademicLevel>> GetAcademicLevelsAsync(bool ignoreCache);

        /// <summary>
        /// Task to return AccountReceivableType
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AccountReceivableType>> GetAccountReceivableTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for AccountReceivableTypes code
        /// </summary>
        /// <param name="code">AccountReceivableTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetAccountReceivableTypesGuidAsync(string code);

        /// <summary>
        /// Get code for AccountReceivableType guid
        /// </summary>
        /// <param name="code">AccountReceivableType guid</param>
        /// <returns>code</returns>
        Task<string> GetAccountReceivableTypesCodeFromGuidAsync(string guid);

        /// <summary>
        /// Admmission Application Types (Standard (only current entry in INTG.APPLICATION.TYPES))
        /// </summary>
        Task<IEnumerable<AdmissionApplicationType>> GetAdmissionApplicationTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for AdmissionApplicationTypes code
        /// </summary>
        /// <param name="code">AdmissionApplicationTypess code</param>
        /// <returns>Guid</returns>
        Task<string> GetAdmissionApplicationTypesGuidAsync(string code);

        /// <summary>
        /// Gets the list of valid admission application status types
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<AdmissionApplicationStatusType>> GetAdmissionApplicationStatusTypesAsync(bool bypassCache);

        /// <summary>
        /// Get guid for AdmissionApplicationStatusTypes code
        /// </summary>
        /// <param name="code">AdmissionApplicationStatusTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetAdmissionApplicationStatusTypesGuidAsync(string code);

        /// <summary>
        /// Get guid for AdmissionPopulations code
        /// </summary>
        /// <param name="code">AdmissionPopulations code</param>
        /// <returns>Guid</returns>
        Task<string> GetAdmissionPopulationsGuidAsync(string code);


        /// <summary>
        /// Admmission Populations (Colleague admit statuses: first time college student, transfer students, etc)
        /// </summary>
        Task<IEnumerable<AdmissionPopulation>> GetAdmissionPopulationsAsync(bool ignoreCache);

        /// <summary>
        /// Admitted statuses (first time college student, transfer students, etc)
        /// </summary>
        Task<IEnumerable<AdmittedStatus>> GetAdmittedStatusesAsync();

        /// <summary>
        /// Affiliations from Campus Organizations
        /// </summary>
        Task<IEnumerable<Affiliation>> GetAffiliationsAsync();

        /// <summary>
        /// Application influences (campus tour, brochure, etc)
        /// </summary>
        Task<IEnumerable<ApplicationInfluence>> GetApplicationInfluencesAsync();

        /// <summary>
        /// Application Sources
        /// </summary>
        Task<IEnumerable<ApplicationSource>> GetApplicationSourcesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for ApplicationSource code
        /// </summary>
        /// <param name="code">ApplicationSource code</param>
        /// <returns>Guid</returns>
        Task<string> GetApplicationSourcesGuidAsync(string code);

        /// <summary>
        /// Application statuses (Accepted, Early Accept, Provisional Accept, Applied, Deferred, etc)
        /// </summary>
        Task<IEnumerable<ApplicationStatus>> GetApplicationStatusesAsync();

        /// <summary>
        /// Application status categories (Accepted, Applied, Waitlisted, etc.)
        /// </summary>
        Task<IEnumerable<ApplicationStatusCategory>> GetApplicationStatusCategoriesAsync();

        /// <summary>
        /// Get a collection of assessment special circumstances
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of assessment special circumstances</returns>
        Task<IEnumerable<AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstancesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for AssessmentSpecialCircumstance code
        /// </summary>
        /// <param name="code">AssessmentSpecialCircumstance code</param>
        /// <returns>Guid</returns>
        Task<string> GetAssessmentSpecialCircumstancesGuidAsync(string code);

        /// <summary>
        /// Get a collection of BookOptions
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<BookOption>> GetBookOptionsAsync();

        /// <summary>
        /// Task to return campus involvement roles
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<CampusInvRole>> GetCampusInvolvementRolesAsync(bool ignoreCache);

        /// <summary>
        /// Task to return campus organization types
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<CampusOrganizationType>> GetCampusOrganizationTypesAsync(bool ignoreCache);

        /// <summary>
        /// Career Goals
        /// </summary>
        Task<IEnumerable<CareerGoal>> GetCareerGoalsAsync();

        /// <summary>
        /// Certificates, Credentials, Degrees
        /// </summary>
        Task<IEnumerable<Ccd>> GetCcdsAsync();

        /// <summary>
        /// Get a collection of ChargeAssessmentMethod
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ChargeAssessmentMethod</returns>
        Task<IEnumerable<ChargeAssessmentMethod>> GetChargeAssessmentMethodsAsync(bool ignoreCache);

        /// <summary>
        /// Class Levels such as FR - Freshman, SO - Sophomore, etc.
        /// </summary>
        Task<IEnumerable<ClassLevel>> GetClassLevelsAsync();

        /// <summary>
        /// First year, second year, third year, etc.
        /// </summary>
        Task<IEnumerable<CourseLevel>> GetCourseLevelsAsync();

        /// <summary>
        /// Get a collection of course levels
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of course levels</returns>
        Task<IEnumerable<CourseLevel>> GetCourseLevelsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for CourseLevel code
        /// </summary>
        /// <param name="code">CourseLevel code</param>
        /// <returns>Guid</returns>
        Task<string> GetCourseLevelGuidAsync(string code);

        /// <summary>
        /// Course statuses
        /// </summary>
        Task<IEnumerable<CourseStatuses>> GetCourseStatusesAsync();

        /// <summary>
        /// Get a collection of course statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of course statuses</returns>
        Task<IEnumerable<CourseStatuses>> GetCourseStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of CourseTitleTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CourseTitleType</returns>
        Task<IEnumerable<CourseTitleType>> GetCourseTitleTypesAsync(bool ignoreCache = false);

        /// <summary>
        /// Get guid for CourseTitleType code
        /// </summary>
        /// <param name="code">CourseTitleType code</param>
        /// <returns>Guid</returns>
        Task<string> GetCourseTitleTypeGuidAsync(string code);

        /// <summary>
        /// Get a collection of CourseTopic
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CourseTopic</returns>
        Task<IEnumerable<CourseTopic>> GetCourseTopicsAsync(bool ignoreCache);
        
        /// <summary>
        /// Get a collection of CourseType domain entities
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CourseType</returns>
        Task<IEnumerable<CourseType>> GetCourseTypesAsync(bool ignoreCache = false);

        /// <summary>
        /// Get guid for CourseType code
        /// </summary>
        /// <param name="code">CourseTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetCourseTypeGuidAsync(string code);

        /// <summary>
        /// Specific types of credit such as Institutional, Transfer, etc.
        /// </summary>
        Task<IEnumerable<CredType>> GetCreditTypesAsync();

        /// <summary>
        /// Credit Categories
        /// </summary>
        Task<IEnumerable<CreditCategory>> GetCreditCategoriesAsync();

        /// <summary>
        /// Get guid for CreditCategories code
        /// </summary>
        /// <param name="code">CreditCategories code</param>
        /// <returns>Guid</returns>
        Task<string> GetCreditCategoriesGuidAsync(string code);

        /// <summary>
        /// Get a collection of credit categories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of credit categories</returns>
        Task<IEnumerable<CreditCategory>> GetCreditCategoriesAsync(bool ignoreCache);

        /// <summary>
        /// Degrees
        /// </summary>
        Task<IEnumerable<Degree>> GetDegreesAsync();

        /// <summary>
        /// Get a collection of enrollment statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of enrollment statuses</returns>
        Task<IEnumerable<EnrollmentStatus>> GetEnrollmentStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Federal Course Classifications.
        /// </summary>
        Task<IEnumerable<FederalCourseClassification>> GetFederalCourseClassificationsAsync();

        /// <summary>
        /// Local Course Classifications
        /// </summary>
        Task<IEnumerable<LocalCourseClassification>> GetLocalCourseClassificationsAsync();

        /// <summary>
        /// External transcript status (repeated, withdrawn, etc)
        /// </summary>
        Task<IEnumerable<ExternalTranscriptStatus>> GetExternalTranscriptStatusesAsync();

        /// <summary>
        /// Grade schemes.
        /// </summary>
        Task<IEnumerable<GradeScheme>> GetGradeSchemesAsync();

        /// <summary>
        /// Get a collection of grade schemes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of grade schemes</returns>
        Task<IEnumerable<GradeScheme>> GetGradeSchemesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for GradeScheme code
        /// </summary>
        /// <param name="code">GradeScheme code</param>
        /// <returns>Guid</returns>
        Task<string> GetGradeSchemeGuidAsync(string code);

        /// <summary>
        /// Grade subschemes.
        /// </summary>
        Task<IEnumerable<GradeSubscheme>> GetGradeSubschemesAsync();

        /// <summary>
        /// Instructional methods.
        /// </summary>
        Task<IEnumerable<InstructionalMethod>> GetInstructionalMethodsAsync();
        
        /// <summary>
        /// Get a collection of instructional methods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of instructional methods</returns>
        Task<IEnumerable<InstructionalMethod>> GetInstructionalMethodsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for InstructionalMethod code
        /// </summary>
        /// <param name="code">InstructionalMethod code</param>
        /// <returns>Guid</returns>
        Task<string> GetInstructionalMethodGuidAsync(string code);

        /// <summary>
        /// Get a collection of administrative instructional methods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of administrative instructional methods</returns>
        Task<IEnumerable<AdministrativeInstructionalMethod>> GetAdministrativeInstructionalMethodsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for AdministrativeInstructionalMethod code
        /// </summary>
        /// <param name="code">AdministrativeInstructionalMethod code</param>
        /// <returns>Guid</returns>
        Task<string> GetAdministrativeInstructionalMethodGuidAsync(string code);

        /// <summary>
        /// Get a collection of IntgTestPercentileTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgTestPercentileTypes</returns>
        Task<IEnumerable<IntgTestPercentileType>> GetIntgTestPercentileTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for IntgTestPercentileTypes code
        /// </summary>
        /// <param name="code">IntgTestPercentileTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetIntgTestPercentileTypesGuidAsync(string code);

        /// <summary>
        /// Primary fields of study.
        /// </summary>
        Task<IEnumerable<Major>> GetMajorsAsync(bool ignoreCache = false);


        /// <summary>
        /// Get a collection of MealPlan
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MealPlan</returns>
        Task<IEnumerable<MealPlan>> GetMealPlansAsync(bool ignoreCache = false);

        /// <summary>
        /// Get guid for MealPlan code
        /// </summary>
        /// <param name="code">MealPlan code</param>
        /// <returns>Guid</returns>
        Task<string> GetMealPlanGuidAsync(string code);

        /// <summary>
        /// Get a collection of MealType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MealType</returns>
        Task<IEnumerable<MealType>> GetMealTypesAsync(bool ignoreCache = false);

        /// <summary>
        /// Get a collection of MealPlanRates
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MealPlanRates</returns>
        Task<IEnumerable<MealPlanRates>> GetMealPlanRatesAsync(bool ignoreCache);


        /// <summary>
        /// Secondary fields of study.
        /// </summary>
        Task<IEnumerable<Minor>> GetMinorsAsync(bool ignoreCache = false);

        /// <summary>
        /// Gets a collection of <see cref="NonAcademicAttendanceEventType">nonacademic attendance event types</see>
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of <see cref="NonAcademicAttendanceEventType">nonacademic attendance event types</see></returns>
        Task<IEnumerable<NonAcademicAttendanceEventType>> GetNonAcademicAttendanceEventTypesAsync(bool ignoreCache = false);

        /// <summary>
        /// Get a collection of NonCourseCategories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of NonCourseCategories</returns>
        Task<IEnumerable<NonCourseCategories>> GetNonCourseCategoriesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of NonCourseGradeUses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of NonCourseGradeUses</returns>
        Task<IEnumerable<NonCourseGradeUses>> GetNonCourseGradeUsesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of HousingResidentType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of HousingResidentType</returns>
        Task<IEnumerable<HousingResidentType>> GetHousingResidentTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of RoomRate
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RoomRate</returns>
        Task<IEnumerable<RoomRate>> GetRoomRatesAsync(bool ignoreCache);

        /// <summary>
        /// Section grade type.
        /// </summary>
        Task<IEnumerable<SectionGradeType>> GetSectionGradeTypesAsync();

        /// <summary>
        /// Get a collection of section grade types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of section grade types</returns>
        Task<IEnumerable<SectionGradeType>> GetSectionGradeTypesAsync(bool ignoreCache);

        /// <summary>
        /// Section Registration Statuses
        /// </summary>
        Task<IEnumerable<SectionRegistrationStatusItem>> SectionRegistrationStatusesAsync();

        /// <summary>
        /// Get a collection of student academic credit statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of student academic credit statuses</returns>
        Task<IEnumerable<SectionRegistrationStatusItem>> GetStudentAcademicCreditStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Section status codes
        /// </summary>
        Task<IEnumerable<SectionStatusCode>> GetSectionStatusCodesAsync();

        /// <summary>
        /// Get a collection of SectionStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of SectionStatuses</returns>
        Task<IEnumerable<SectionStatuses>> GetSectionStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Course of study that results in a specialized degree.
        /// </summary>
        Task<IEnumerable<Specialization>> GetSpecializationsAsync();

        /// <summary>
        /// Student Status to identify student attributes.
        /// </summary>
        Task<IEnumerable<StudentStatus>> GetStudentStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Student Statuses code
        /// </summary>
        /// <param name="code">Student Statuses code</param>
        /// <returns>Guid</returns>
        Task<string> GetStudentStatusesGuidAsync(string code);

        /// <summary>
        /// Student Type to identify student attributes.
        /// </summary>
        Task<IEnumerable<StudentType>> GetStudentTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Student Types code
        /// </summary>
        /// <param name="code">Student Types code</param>
        /// <returns>Guid</returns>
        Task<string> GetStudentTypesGuidAsync(string code);

        /// <summary>
        /// Student load (full time, part time, etc)
        /// </summary>
        Task<IEnumerable<StudentLoad>> GetStudentLoadsAsync();

        /// <summary>
        /// Academic subjects.
        /// </summary>
        Task<IEnumerable<Subject>> GetSubjectsAsync();

        /// <summary>
        /// Get a collection of subjects
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of subjects</returns>
        Task<IEnumerable<Subject>> GetSubjectsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Subjects code
        /// </summary>
        /// <param name="code">Subjects code</param>
        /// <returns>Guid</returns>
        Task<string> GetSubjectGuidAsync(string code);

        /// <summary>
        /// Topic codes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of topic codes</returns>
        Task<IEnumerable<TopicCode>> GetTopicCodesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for TopicCode code
        /// </summary>
        /// <param name="code">TopicCode code</param>
        /// <returns>Guid</returns>
        Task<string> GetTopicCodeGuidAsync(string code);

        /// <summary>
        /// Transcript category (major course, college prep, etc)
        /// </summary>
        Task<IEnumerable<TranscriptCategory>> GetTranscriptCategoriesAsync();

        /// <summary>
        /// Tests such as ACT, SAT, etc.
        /// </summary>
        Task<IEnumerable<Test>> GetTestsAsync();

        /// <summary>
        /// Test Sources
        /// </summary>
        Task<IEnumerable<TestSource>> GetTestSourcesAsync(bool ignoreCache);


        /// <summary>
        /// Get guid for TestSources code
        /// </summary>
        /// <param name="code">TestSources code</param>
        /// <returns>Guid</returns>
        Task<string> GetTestSourcesGuidAsync(string code);

        /// <summary>
        /// Statuses of non-courses
        /// </summary>
        Task<IEnumerable<NoncourseStatus>> GetNoncourseStatusesAsync();

        /// <summary>
        /// Get guid for CourseStatus code
        /// </summary>
        /// <param name="code">CourseStatus code</param>
        /// <returns>Guid</returns>
        Task<string> GetCourseStatusGuidAsync(string code);

        /// <summary>
        /// Waitlist statuses
        /// </summary>
        Task<IEnumerable<WaitlistStatusCode>> GetWaitlistStatusCodesAsync();

        /// <summary>
        /// Section transfer status information
        /// </summary>
        Task<IEnumerable<SectionTransferStatus>> GetSectionTransferStatusesAsync();

        /// <summary>
        /// Student Waiver reasons and descriptions
        /// </summary>
        Task<IEnumerable<StudentWaiverReason>> GetStudentWaiverReasonsAsync();

        /// <summary>
        /// Petition Statuses and descriptions
        /// </summary>
        Task<IEnumerable<PetitionStatus>> GetPetitionStatusesAsync();

        /// <summary>
        /// Student Petition reasons and descriptions
        /// </summary>
        Task<IEnumerable<StudentPetitionReason>> GetStudentPetitionReasonsAsync();

        /// <summary>
        ///  Task of the Cap Size options for Graduation
        /// </summary>
        Task<IEnumerable<CapSize>> GetCapSizesAsync();

        /// <summary>
        /// Task to Returns Student Gown Sizes and Descriptions
        /// </summary>
        Task<IEnumerable<GownSize>> GetGownSizesAsync();

         /// <summary>
         /// Task to return Session Cycles and Descriptions
         /// </summary>
         /// <returns></returns>
        Task<IEnumerable<SessionCycle>> GetSessionCyclesAsync();

         /// <summary>
         /// Task to return Yearly Cycles and Descriptions
         /// </summary>
         /// <returns></returns>
        Task<IEnumerable<YearlyCycle>> GetYearlyCyclesAsync();

        /// <summary>
        /// Task to return Hold Request Types and Descriptions
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<HoldRequestType>> GetHoldRequestTypesAsync();

        /// <summary>
        /// Task to return Host Country
        /// </summary>
        /// <returns></returns>
        Task<string> GetHostCountryAsync();

        /// <summary>
        /// Task to return accounting codes
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AccountingCode>> GetAccountingCodesAsync(bool ignoreCache);

        /// <summary>
        /// Task to return DistributionMethod
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<DistributionMethod>> GetDistrMethodCodesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for DistributionMethod code
        /// </summary>
        /// <param name="code">DistributionMethod code</param>
        /// <returns>Guid</returns>
        Task<string> GetDistrMethodGuidAsync(string code);

        /// <summary>
        /// Get code for DistributionMethod guid
        /// </summary>
        /// <param name="code">DistributionMethod guid</param>
        /// <returns>code</returns>
        Task<string> GetDistrMethodCodeFromGuidAsync(string guid);

        /// <summary>
        /// Gets all student cohorts
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<StudentCohort>> GetAllStudentCohortAsync(bool bypassCache);

        /// <summary>
        /// Gets student cohort guid based the code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<string> GetStudentCohortGuidAsync(string code);
        /// <summary>
        /// Gets all student classifications
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<StudentClassification>> GetAllStudentClassificationAsync(bool bypassCache);

        /// <summary>
        /// Get guid for Student Classification code
        /// </summary>
        /// <param name="code">Student Classification code</param>
        /// <returns>Guid</returns>
        Task<string> GetStudentClassificationGuidAsync(string code);

        /// <summary>
        /// Gets all schedule terms
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<ScheduleTerm>> GetAllScheduleTermsAsync(bool bypassCache);

        /// <summary>
        /// Get a collection of WithdrawReason domain entities
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of WithdrawReason domain entitities</returns>
        Task<IEnumerable<WithdrawReason>> GetWithdrawReasonsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for WithdrawReasons code
        /// </summary>
        /// <param name="code">WithdrawReasons code</param>
        /// <returns>Guid</returns>
        Task<string> GetWithdrawReasonsGuidAsync(string code);

        /// <summary>
        /// Gets the list of valid admission decision types
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<AdmissionDecisionType>> GetAdmissionDecisionTypesAsync(bool bypassCache);

        /// <summary>
        /// Get an admission decision type by Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<AdmissionDecisionType> GetAdmissionDecisionTypeByGuidAsync(string guid);

        /// <summary>
        /// Get guid for AdmissionDecisionTypes
        /// </summary>
        /// <param name="code">AdmissionDecisionTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetAdmissionDecisionTypesGuidAsync(string code);

        /// <summary>
        /// Get special processing code for AdmissionDecisionTypes
        /// </summary>
        /// <param name="code">AdmissionDecisionTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetAdmissionDecisionTypesSPCodeAsync(string code);

        /// <summary>
        /// Get a collection of FacultySpecialStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FacultySpecialStatuses</returns>
        Task<IEnumerable<FacultySpecialStatuses>> GetFacultySpecialStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of FacultyContractTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FacultyContractTypes</returns>
        Task<IEnumerable<FacultyContractTypes>> GetFacultyContractTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of BillingOverrideReasons
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of BillingOverrideReasons</returns>
        Task<IEnumerable<BillingOverrideReasons>> GetBillingOverrideReasonsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of FloorPreferences
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FloorPreferences</returns>
        Task<IEnumerable<FloorCharacteristics>> GetFloorCharacteristicsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of RoommateCharacteristics
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RoommateCharacteristics</returns>
        Task<IEnumerable<RoommateCharacteristics>> GetRoommateCharacteristicsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of StudentResidentialCategories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of StudentResidentialCategories</returns>
        Task<IEnumerable<StudentResidentialCategories>> GetStudentResidentialCategoriesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of AttendanceTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AttendanceTypes</returns>
        Task<IEnumerable<AttendanceTypes>> GetAttendanceTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of ArCategories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ArCategories</returns>
        Task<IEnumerable<ArCategory>> GetArCategoriesAsync(bool ignoreCache);

        /// <summary>
        /// Gets a collection of AccountReceivableDepositType
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<AccountReceivableDepositType>> GetAccountReceivableDepositTypesAsync(bool ignoreCache);

        /// <summary>
        /// Gets a collection of Distribution2
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Distribution2>> GetDistributionsAsync(bool ignoreCache);
        /// <summary>
        /// Returns all of the campus org roles
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<CampusOrgRole>> CampusOrgRolesAsync();

        /// <summary>
        /// Returns Contact Measures valcode table Asynchronously (ContactHoursPeriod in service layer)
        /// </summary>
        Task<IEnumerable<ContactMeasure>> GetContactMeasuresAsync(bool ignoreCache);

        /// <summary>
        /// Task to Returns Drop Reasons
        /// </summary>
        Task<IEnumerable<DropReason>> GetDropReasonsAsync();

        /// <summary>
        /// Gets financial aid academic progress types
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Domain.Student.Entities.SapType>> GetSapTypesAsync(bool ignoreCache = false);

        /// <summary>
        /// Gets sap statuses
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<SapStatuses>> GetSapStatusesAsync(string restrictedVisibilityValue = "", bool ignoreCache = false);

        /// <summary>
        /// Get a collection of SectionTitleType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of SectionTitleType</returns>
        Task<IEnumerable<SectionTitleType>> GetSectionTitleTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a GUID for SectionTitleType.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<string> GetSectionTitleTypesGuidAsync(string code);

        /// <summary>
        /// Get a collection of SectionDescriptionType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of SectionDescriptionType</returns>
        Task<IEnumerable<SectionDescriptionType>> GetSectionDescriptionTypesAsync(bool ignoreCache);

        /// <summary>
        /// Gets a GUID for SectionDescriptionType.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<string> GetSectionDescriptionTypesGuidAsync(string code);

        /// <summary>
        /// Get a collection of financial aid fund categories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund categories</returns>
        Task<IEnumerable<FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool ignoreCache = false);

        /// <summary>
        /// Get a collection of financial aid fund classifications
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund classifications</returns>
        Task<IEnumerable<FinancialAidFundClassification>> GetFinancialAidFundClassificationsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of financial aid years
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid years</returns>
        Task<IEnumerable<FinancialAidYear>> GetFinancialAidYearsAsync(bool ignoreCache = false);

        /// <summary>
        /// Get a collection of financial aid award periods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid award periods</returns>
        Task<IEnumerable<FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool ignoreCache = false);

        /// <summary>
        /// Public Accessor for Financial Aid AwardStatuses. Retrieves and caches all award statuses 
        /// defined in Colleague.
        /// Each status category in Colleague maps to one of three categories in the API. There are 5:
        /// Colleague Accepted = Accepted
        /// Colleague Pending = Pending
        /// Colleague Estimated = Pending
        /// Colleague Rejected = Rejected
        /// Colleague Denied = Rejected
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<AwardStatus></returns>
        Task<IEnumerable<AwardStatus>> AwardStatusesAsync(bool bypassCache = false);

        /// <summary>
        /// Gets collection of grading terms.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<GradingTerm>> GetGradingTermsAsync(bool bypassCache = false);

        /// <summary>
        /// Get Unidata formatted date for filters.
        /// </summary>   
        /// <param name="date">date </param>
        /// <returns>date in undiata format</returns>
        Task<string> GetUnidataFormattedDate(string date);

        /// <summary>
        /// Get the LDM.DEFAULTS value for LDMD.INCLUDE.ENRL.HEADCOUNTS
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns>List of section-registration-statuses to be included in headcounts</returns>
        Task<List<string>> GetHeadcountInclusionListAsync(bool ignoreCache = false);
    }
}