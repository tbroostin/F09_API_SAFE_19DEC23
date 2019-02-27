// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentRepository
    {
        Task<Entities.Student> GetAsync(string id);
        /// <summary>
        /// Wrapper around async, used by FinancialAid services
        /// </summary>
        /// <param name="program"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        Entities.Student Get(string id);
        Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetAsync(IEnumerable<string> ids);
        Task<Entities.GradeRestriction> GetGradeRestrictionsAsync(string id);
        Task<RegistrationResponse> RegisterAsync(RegistrationRequest request);
        Task<RegistrationEligibility> CheckRegistrationEligibilityAsync(string id);
        Task<RegistrationEligibility> CheckRegistrationEligibilityEthosAsync(string id, List<string> termCodes);
        Task<IEnumerable<Entities.RosterStudent>> GetRosterStudentsAsync(IEnumerable<string> ids);
        Task<IEnumerable<TranscriptRestriction>> GetTranscriptRestrictionsAsync(string id);
        Task<IEnumerable<Entities.Student>> SearchAsync(string lastName, string firstName, DateTime? dateOfBirth, string formerName, string studentId, string governmentId);
        Task<IEnumerable<string>> SearchIdsAsync(string termId);
        Task<string> OrderTranscriptAsync(Student.Entities.Transcripts.TranscriptRequest order);
        Task<string> CheckTranscriptStatusAsync(string orderId, string currentStatusCode);
        Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetStudentsByIdAsync(IEnumerable<string> studentIds, Term termData, IEnumerable<CitizenshipStatus> citizenshipStatus, bool inheritFromPerson = true, bool getDegreePlan = true, bool filterAdvisorsByTerm = false, bool filterEndedAdvisements = false);
        Task<string> GetTranscriptAsync(string studentId, string transcriptGrouping);
        Task<IEnumerable<Domain.Student.Entities.StudentAccess>> GetStudentAccessAsync(IEnumerable<string> ids);
        Task<IEnumerable<StudentCohort>> GetAllStudentCohortAsync(bool bypassCache);
        /// <summary>
        /// Resident Type to identify resident type of student.
        /// </summary>
        Task<IEnumerable<ResidencyStatus>> GetResidencyStatusesAsync(bool ignoreCache);

        Task<Entities.Student> GetDataModelStudentFromGuidAsync(string guid);
        Task<Entities.Student> GetDataModelStudentFromGuid2Async(string guid);
        Task<Tuple<IEnumerable<Entities.Student>, int>> GetDataModelStudentsAsync(int offset, int limit, bool bypassCache, string person = "", string type = "", string cohort = "", string residency = "");
        Task<Tuple<IEnumerable<Entities.Student>, int>> GetDataModelStudents2Async(int offset, int limit, bool bypassCache, string personFilter = "", string person = "", List<string> types = null, List<string> cohorts = null, List<string> residencies = null);
        /// <summary>
        /// Returns detailed students information for search by ID or Name
        /// </summary>
        Task<IEnumerable<Entities.Student>> GetStudentsSearchAsync(IEnumerable<string> ids);
        Task<IEnumerable<Entities.Student>> GetStudentSearchByNameAsync(string lastName, string firstName = null, string middleName = null, int pageSize = int.MaxValue, int pageIndex = 1);
        /// <summary>
        /// Reads the student information from Colleague and returns an IEnumerable of Students Entity models with type and residency statuses information to be used by student-academic-period-profiles API
        /// </summary>
        /// <param name="ids">Required to include at least 1 Id. These are Colleague Person (student) ids.</param>
        /// <returns>An IEnumerable list of Student Entities found in Colleague, or an empty list if none are found.</returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetStudentAcademicPeriodProfileStudentInfoAsync(IEnumerable<string> ids);

        // <summary>
        /// Get guid for ResidencyStatus code
        /// </summary>
        /// <param name="code">ResidencyStatus code</param>
        /// <returns>Guid</returns>
        Task<string> GetResidencyStatusGuidAsync(string code);
    }
}
