// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student.Transcripts;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentService : IBaseService
    {
        Task<PrivacyWrapper<Dtos.Student.Student>> GetAsync(string id);
        Task<IEnumerable<Dtos.Student.Student>> SearchAsync(string lastName, DateTime? dateOfBirth, string firstName, string formerName, string studentId, string governmentId);
        Task<IEnumerable<string>> SearchIdsAsync(string termId);
        Task<IEnumerable<Dtos.Student.RegistrationMessage>> CheckRegistrationEligibilityAsync(string id);
        Task<Dtos.Student.RegistrationEligibility> CheckRegistrationEligibility2Async(string studentId);
        Task<Dtos.Student.RegistrationEligibility> CheckRegistrationEligibility3Async(string studentId);
        Task<IEnumerable<TranscriptRestriction>> GetTranscriptRestrictionsAsync(string studentid);
        Task<Dtos.Student.TranscriptAccess> GetTranscriptRestrictions2Async(string studentId);
        Task<IEnumerable<Dtos.Student.Term>> GetUngradedTermsAsync(string studentId);
        Task<string> OrderTranscriptAsync(TranscriptRequest order);
        Task<string> CheckTranscriptStatusAsync(string orderId, string currentStatusCode);

        Task<PrivacyWrapper<IEnumerable<Dtos.Student.StudentBatch3>>> QueryStudentsById4Async(IEnumerable<string> studentIds, bool inheritFromPerson = false, bool getDegreePlan = false, string term = null);
        Task<Tuple<byte[], string>> GetUnofficialTranscriptAsync(string studentId, string path, string transcriptGrouping, string reportWatermarkPath, string deviceInfoPath);
        Task<Dtos.Student.RegistrationResponse> RegisterAsync(string studentId, IEnumerable<Dtos.Student.SectionRegistration> sectionRegistrations);
        Task<Dtos.Student.RegistrationResponse> DropRegistrationAsync(string studentId, Dtos.Student.SectionDropRegistration sectionDropRegistration);
        Task CheckStudentAccessAsync(string studentId);

        //StudentCohort
        Task<IEnumerable<Dtos.StudentCohort>> GetAllStudentCohortsAsync( Dtos.Filters.CodeItemFilter criteria = null, bool bypassCache = false);
        Task<Dtos.StudentCohort> GetStudentCohortByGuidAsync(string id, bool bypassCache = false);

        //ResidentType
        Task<IEnumerable<Ellucian.Colleague.Dtos.ResidentType>> GetResidentTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.ResidentType> GetResidentTypeByIdAsync(string id);
        //StudentClassification
        Task<IEnumerable<Dtos.StudentClassification>> GetAllStudentClassificationsAsync(bool bypassCache);
        Task<Dtos.StudentClassification> GetStudentClassificationByGuidAsync(string id);

        Task<Dtos.Students> GetStudentsByGuidAsync(string guid, bool bypassCache = true);
        Task<Dtos.Students2> GetStudentsByGuid2Async(string guid, bool bypassCache = true);
        Task<Tuple<IEnumerable<Dtos.Students>, int>> GetStudentsAsync(int offset, int limit, bool bypassCache = false, string person = "", string type = "", string cohorts = "", string residency = "");
        Task<Tuple<IEnumerable<Dtos.Students2>, int>> GetStudents2Async(int offset, int limit, Dtos.Students2 criteriaFilter, string personFilter, bool bypassCache = false);
        Task<PrivacyWrapper<List<Dtos.Student.Student>>> Search3Async(Dtos.Student.StudentSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1);
        /// <summary>
        /// Retreive Student's academic levels
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>List of all the academic levels of the student</returns>
        Task<IEnumerable<Dtos.Student.StudentAcademicLevel>> GetStudentAcademicLevelsAsync(string studentId);

        #region Planning Student
        Task<PrivacyWrapper<Dtos.Student.PlanningStudent>> GetPlanningStudentAsync(string studentId);
        #endregion Planning Student
    }
}
