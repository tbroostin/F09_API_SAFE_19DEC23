// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using StudentClassification = Ellucian.Colleague.Domain.Student.Entities.StudentClassification;
using StudentCohort = Ellucian.Colleague.Domain.Student.Entities.StudentCohort;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentService : StudentCoordinationService, IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IAcademicHistoryService _acadHistService;
        private readonly ITermRepository _termRepository;
        private readonly IRegistrationPriorityRepository _priorityRepository;
        private readonly IStudentConfigurationRepository _studentConfigurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStaffRepository _staffRepository;
        private ILogger _logger;

        public StudentService(IAdapterRegistry adapterRegistry, IStudentRepository studentRepository, IPersonRepository personRepository, IAcademicCreditRepository academicCreditRepository, IAcademicHistoryService academicHistoryService, 
            ITermRepository termRepository, IRegistrationPriorityRepository priorityRepository, IStudentConfigurationRepository studentConfigurationRepository, IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,IConfigurationRepository configurationRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IStaffRepository staffRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository, staffRepository)
        {
            _studentRepository = studentRepository;
            _personRepository = personRepository;
            _academicCreditRepository = academicCreditRepository;
            _acadHistService = academicHistoryService;
            _termRepository = termRepository;
            _priorityRepository = priorityRepository;
            _studentConfigurationRepository = studentConfigurationRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _staffRepository = staffRepository;
            _logger = logger;
        }


        #region private methods

        private IEnumerable<StudentClassification> _studentClassification = null;

        private async Task<IEnumerable<StudentClassification>> GetAllStudentClassificationAsync(bool bypassCache)
        {
            if (_studentClassification == null)
            {
                _studentClassification = await _studentReferenceDataRepository.GetAllStudentClassificationAsync(bypassCache);
            }
            return _studentClassification;
        }

        private IEnumerable<Domain.Student.Entities.StudentType> _studentTypes = null;

        private async Task<IEnumerable<Domain.Student.Entities.StudentType>> GetStudentTypesAsync(bool bypassCache)
        {
            if (_studentTypes == null)
            {
                _studentTypes = await _studentReferenceDataRepository.GetStudentTypesAsync(bypassCache);
            }
            return _studentTypes;
        }

        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;

        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> GetAcademicLevelsAsync(bool bypassCache)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            }
            return _academicLevels;
        }

        private IEnumerable<Domain.Student.Entities.AcademicProgram> _academicPrograms = null;

        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> GetAcademicProgramsAsync(bool bypassCache)
        {
            if (_academicPrograms == null)
            {
                _academicPrograms = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);
            }
            return _academicPrograms;
        }
        #endregion

        /// <summary>
        /// get resources via list of IDs
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="inheritFromPerson">Flag to inherit Name/Address hierarchy from Person (Default to true)</param>
        /// <param name="getDegreePlan">Flag to get a Degree Plan ID (Default to true)</param>
        /// <returns>List of Student objects</returns>
        public async Task<IEnumerable<Dtos.Student.Student>> GetStudentsByIdAsync(IEnumerable<string> studentIds, bool inheritFromPerson = true, bool getDegreePlan = true)
        {
            List<Dtos.Student.Student> students = new List<Dtos.Student.Student>();
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            var studentEntities = await _studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, inheritFromPerson, getDegreePlan);
            if (studentEntities != null && studentEntities.Count() > 0)
            {
                var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.Student>();
                foreach (var student in studentEntities)
                {
                    // Make sure user has access to this student--If not, method throws exception
                    await CheckUserAccessAsync(student.Id, student.ConvertToStudentAccess());

                    students.Add(studentDtoAdapter.MapToType(student));
                }
            }
            return students;

        }

        /// <summary>
        /// Get StudentBatch DTO which returns Students without Address and Name Hierarchies
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="inheritFromPerson">Flag to inherit Name/Address Hierarchy from Person (Default to false)</param>
        /// <param name="getDegreePlan">Flag to get a Degree Plan Id (Default to false)</param>
        /// <returns>List of StudentBatch objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentBatch>> QueryStudentsByIdAsync(IEnumerable<string> studentIds, bool inheritFromPerson=false, bool getDegreePlan=false)
        {
            List<Dtos.Student.StudentBatch> students = new List<Dtos.Student.StudentBatch>();
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                throw new PermissionsException("User does not have permissions to query students");
            }
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            var studentEntities = await _studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, inheritFromPerson, getDegreePlan);
            if (studentEntities != null && studentEntities.Count() > 0)
            {
                var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.StudentBatch>();
                foreach (var student in studentEntities)
                {
                    students.Add(studentDtoAdapter.MapToType(student));
                }
            }
            return students;
        }

        /// <summary>
        /// Get StudentBatch2 DTO which returns Students without Address and Name Hierarchies and with Academic Level information.
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="inheritFromPerson">Flag to inherit Name/Address Hierarchy from Person (Default to false)</param>
        /// <param name="getDegreePlan">Flag to get a Degree Plan Id (Default to false)</param>
        /// <returns>List of StudentBatch2 objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentBatch2>> QueryStudentsByIdAsync2(IEnumerable<string> studentIds, bool inheritFromPerson = false, bool getDegreePlan = false, string term = null)
        {
            List<Dtos.Student.StudentBatch2> students = new List<Dtos.Student.StudentBatch2>();
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                throw new PermissionsException("User does not have permissions to query students");
            }
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            if (!string.IsNullOrEmpty(term))
            {
                termData = _termRepository.Get(term);
            }
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
            var studentEntities = await _studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData,inheritFromPerson, getDegreePlan);
            if (studentEntities != null && studentEntities.Count() > 0)
            {
                var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.StudentBatch2>();
                foreach (var student in studentEntities)
                {
                    students.Add(studentDtoAdapter.MapToType(student));
                }
            }
            return students;
        }

        /// <summary>
        /// Get StudentBatch3 DTO which returns Students without Address and Name Hierarchies and with Academic Level information
        /// Allows a null marital status and filters student advisements/advisors by incoming term
        /// Allows an alien status that indicates citizenship.
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="inheritFromPerson">Flag to inherit Name/Address Hierarchy from Person (Default to false)</param>
        /// <param name="getDegreePlan">Flag to get a Degree Plan Id (Default to false)</param>
        /// <returns>List of StudentBatch3 objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentBatch3>> QueryStudentsByIdAsync3(IEnumerable<string> studentIds, bool inheritFromPerson = false, bool getDegreePlan = false, string term = null)
        {
            List<Dtos.Student.StudentBatch3> students = new List<Dtos.Student.StudentBatch3>();
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                throw new PermissionsException("User does not have permissions to query students");
            }
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            if (!string.IsNullOrEmpty(term))
            {
                termData = _termRepository.Get(term);
            }
            bool filterAdvisorsByTerm = true;
            var citizenshipStatusData = await _referenceDataRepository.GetCitizenshipStatusesAsync(false);
            var studentEntities = await _studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, inheritFromPerson, getDegreePlan, filterAdvisorsByTerm);
            if (studentEntities != null && studentEntities.Count() > 0)
            {
                var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.StudentBatch3>();
                foreach (var student in studentEntities)
                {
                    students.Add(studentDtoAdapter.MapToType(student));
                }
            }
            return students;
        }

        /// <summary>
        /// Get StudentBatch3 DTO which returns Students without Address and Name Hierarchies and with Academic Level information
        /// Allows a null marital status and filters student advisements/advisors by incoming term
        /// Allows an alien status that indicates citizenship.
        /// Filter out student advisements which ended today or earlier.
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="inheritFromPerson">Flag to inherit Name/Address Hierarchy from Person (Default to false)</param>
        /// <param name="getDegreePlan">Flag to get a Degree Plan Id (Default to false)</param>
        /// <returns>List of StudentBatch3 objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentBatch3>> QueryStudentsById4Async(IEnumerable<string> studentIds, bool inheritFromPerson = false, bool getDegreePlan = false, string term = null)
        {
            List<Dtos.Student.StudentBatch3> students = new List<Dtos.Student.StudentBatch3>();
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                throw new PermissionsException("User does not have permissions to query students");
            }
            Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
            if (!string.IsNullOrEmpty(term))
            {
                termData = _termRepository.Get(term);
            }
            bool filterAdvisorsByTerm = true;
            bool filterEndedAdvisements = true;
            var citizenshipStatusData = await _referenceDataRepository.GetCitizenshipStatusesAsync(false);
            var studentEntities = await _studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, inheritFromPerson, getDegreePlan, filterAdvisorsByTerm, filterEndedAdvisements);
            if (studentEntities != null && studentEntities.Count() > 0)
            {
                var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.StudentBatch3>();
                foreach (var student in studentEntities)
                {
                    students.Add(studentDtoAdapter.MapToType(student));
                }
            }
            return students;
        }

        //public IEnumerable<string> GetRegisteredStudentIdsByTerm(string termId)
        //{     
        //    var studentEntity = _studentRepository.Get(id);
        //    var studentEntity = _studentRepository.GetStudentsByTerm(termId);
        //    // Throw error if student not found
        //    if (studentEntity == null)
        //    {
        //        throw new ApplicationException("Student not found in repository");
        //    }
        //    // Make sure user has access to this student--If not, method throws exception
        //    CheckUserAccess(studentEntity);
        //    // Build and return the student dto
        //    var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.Student>();
        //    Dtos.Student.Student student = studentDtoAdapter.MapToType(studentEntity);
        //    return student;
        //}

        /// <summary>
        /// OBSOLETE as of API 1.3. Please use CheckRegistrationEligibility2
        /// Returns result of checking student eligibility. 
        /// </summary>
        /// <param name="id">Id of student</param>
        /// <returns>List of <see cref="RegistrationMessage">RegistrationMessages</see>. Presence of registration messages implies ineligibility.</returns>
        [Obsolete("OBSOLETE as of API 1.3. Please use CheckRegistrationEligibility2Async")]
        public async Task<IEnumerable<Dtos.Student.RegistrationMessage>> CheckRegistrationEligibilityAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Invalid id");
            }

            // Make sure user has access to this student--If not, method throws exception
            await CheckUserAccessAsync(id);

            var messageEntities =( await _studentRepository.CheckRegistrationEligibilityAsync(id)).Messages;

            var messages = new List<Ellucian.Colleague.Dtos.Student.RegistrationMessage>();
            foreach (var message in messageEntities)
            {
                messages.Add(new Ellucian.Colleague.Dtos.Student.RegistrationMessage { Message = message.Message, SectionId = message.SectionId });
            }

            return messages;
        }

        /// <summary>
        /// Check if student is eligible for registration. Returns messages if student fails eligibility checks. Also indicates if current user
        /// has override permissions, enabling them to register the student even if ineligible.
        /// </summary>
        /// <param name="studentId">student Id</param>
        /// <returns><see cref="RegistrationEligibility">RegistrationEligibility</see> object, consisting of status information and  messages indicating eligibility or lack of it.
        /// In addition booleans indicating if the current user has override permissions.</returns>
        public async Task<Dtos.Student.RegistrationEligibility> CheckRegistrationEligibility2Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Invalid Student Id");
            }

            // Make sure user has access to this student--If not, method throws exception
            await CheckStudentAdvisorUserAccessAsync(studentId);

            var registrationEligibility = await _studentRepository.CheckRegistrationEligibilityAsync(studentId);

            // Next determine if the student has any registration priority (or is missing one where required).
            // Registration priorities can affect the registration eligibility status for a term and the anticipated add date.
            IEnumerable<RegistrationPriority> studentRegistrationPriorities = await _priorityRepository.GetAsync(studentId);

            // Retrieve all terms for the priority checking (cannot just pull the registration ones due to the reporting term check).
            var allTerms = await _termRepository.GetAsync();
            // Next deal with any registration priorities - these may override the information above.
            // Even if the student has no priorities you still need to do this update because if the term
            // requires priorities and they don't have any then it changes their registration status.
            registrationEligibility.UpdateRegistrationPriorities(studentRegistrationPriorities, allTerms);

            var registrationEligibilityDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>();
            Dtos.Student.RegistrationEligibility registrationEligibilityDto = registrationEligibilityDtoAdapter.MapToType(registrationEligibility);

            return registrationEligibilityDto;
        }

        public async Task<IEnumerable<Dtos.Student.Student>> SearchAsync(string lastName, DateTime? dateOfBirth, string firstName = "", string formerName = "", string studentId = "", string governmentId = "")
        {
            // Get student entity
            var studentEntities = await _studentRepository.SearchAsync(lastName, firstName, dateOfBirth, formerName, studentId, governmentId);
            // Throw error if student not found  
            if (studentEntities == null || studentEntities.Count() == 0)
            {
                throw new KeyNotFoundException("No students found matching supplied criteria.");
            }

            // Make sure user has access to any student
            if (!HasPermission(PlanningPermissionCodes.ViewAnyAdvisee))
            {
                throw new PermissionsException("User does not have permissions to access to this function");
            }


            // Build and return the student dtos
            var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.Student>();

            // This should be one line but SelectMany is fussy
            var students = new List<Dtos.Student.Student>();
            foreach (var stu in studentEntities)
            {
                students.Add(studentDtoAdapter.MapToType(stu));
            }
            return students.AsEnumerable();
        }

        public async Task<IEnumerable<string>> SearchIdsAsync(string termId)
        {
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                var studentIds = await _studentRepository.SearchIdsAsync(termId);
                return studentIds;
            }
            else
            {
                throw new PermissionsException("User does not have permissions to access this function");
            }
        }

        // <summary>
        /// Gets the transcript restrictions for the indicated student.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>The list of <see cref="TranscriptRestriction">TranscriptRestrictions</see> found for this student</returns>
        [Obsolete("OBSOLETE as of API 1.9. Please use GetTranscriptRestrictions2Async")]
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TranscriptRestriction>> GetTranscriptRestrictionsAsync(string studentId)
        {
            return await _studentRepository.GetTranscriptRestrictionsAsync(studentId);
        }

        // <summary>
        /// Gets the transcript restrictions for the indicated student and the flag to determine if they restrictions should be enforced.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>a <see cref="TranscriptAccess">TranscriptAccess</see> object, consisting of Web Trascript Restiction paramentes and a list of transcript restrictions for this student.</returns>
        public async Task<Dtos.Student.TranscriptAccess> GetTranscriptRestrictions2Async(string studentId)
        {
            // Make sure user has access to this student--If not, method throws exception
            var student = await _studentRepository.GetAsync(studentId);
            if (student == null)
            {
                throw new KeyNotFoundException("Student with ID " + studentId + " not found in the repository.");
            }


            // If not correct permissions, throw a permissions exception. 
            await CheckStudentAdvisorUserAccessAsync(studentId, student.ConvertToStudentAccess());


            var transcriptAccess = new Dtos.Student.TranscriptAccess();
            var studentConfiguration = await _studentConfigurationRepository.GetStudentConfigurationAsync();
            var transcriptRestrictions = await _studentRepository.GetTranscriptRestrictionsAsync(studentId);

            transcriptAccess.EnforceTranscriptRestriction = studentConfiguration.EnforceTranscriptRestriction;

            var transcriptRestrictionDto = new List<Dtos.Student.TranscriptRestriction>();
            if (transcriptRestrictions.Count() > 0)
            {
                var restrictionAdapter = new AutoMapperAdapter<Domain.Student.Entities.TranscriptRestriction, Dtos.Student.TranscriptRestriction>(_adapterRegistry, _logger);
                foreach (var transcriptRestriction in transcriptRestrictions)
                {
                    transcriptAccess.TranscriptRestrictions.Add(restrictionAdapter.MapToType(transcriptRestriction));
                }
            }

            return transcriptAccess;
        }

        /// <summary>
        /// Gets a list of terms for which the student has credits without verified grades.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns>List of ungraded <see cref="Dtos.Student.Term">Terms</see> found for this student</returns>
        public async Task<IEnumerable<Dtos.Student.Term>> GetUngradedTermsAsync(string studentId)
        {
            if (!UserIsSelf(studentId) && !(await UserIsAdvisorAsync(studentId)) && !HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                var message = "Current user is not the student to request ungraded terms or current user is advisor or faculty but doesn't have appropriate permissions and therefore cannot access it.";
                logger.Info(message);
                throw new PermissionsException(message);
            }
            var history = await _acadHistService.GetAcademicHistory2Async(studentId, false, true, null);
            List<string> ungradedTermIds = new List<string>();

            if (history != null)
            {
                foreach (var acTerm in history.AcademicTerms)
                {
                    var creditsInTerm = acTerm.AcademicCredits.Count();
                    var vfdCredits = acTerm.AcademicCredits.Where(ac => ac.HasVerifiedGrade).Count();

                    if (acTerm.AcademicCredits.Count() > acTerm.AcademicCredits.Where(ac => ac.HasVerifiedGrade).Count())
                    {
                        ungradedTermIds.Add(acTerm.TermId);
                    }
                }
            }

            var ungradedTermEntities = await _termRepository.GetAsync(ungradedTermIds);

            // Build and return the term dtos
            var termAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Dtos.Student.Term>();

            var ungradedTerms = new List<Dtos.Student.Term>();

            if (ungradedTermEntities != null)
            {
                foreach (var term in ungradedTermEntities)
                {
                    ungradedTerms.Add(termAdapter.MapToType(term));
                }
            }
            ungradedTerms.RemoveAll(ug => ug.EndDate < DateTime.Today.AddDays(-60));  // if the term ended more than two months ago 
            // and you still have unverified grades, then something
            // else is going on but chances are you don't want to wait 
            // for that term to end for your transcript.
            return ungradedTerms.AsEnumerable();

        }

        /// <summary>
        /// Accepts a transcript order and returns the status of the order.
        /// </summary>
        /// <param name="order">Data transfer object mapped to the PESC XML Transcript Request standard.</param>
        /// <returns>The status of the order, in PESC XML, encoded BASE-64 to protect it from being munged. </see></returns>
        public async Task<string> OrderTranscriptAsync(TranscriptRequest order)
        {
            var requestAdapter = _adapterRegistry.GetAdapter<Dtos.Student.Transcripts.TranscriptRequest, Domain.Student.Entities.Transcripts.TranscriptRequest>();
            var requestEntity = requestAdapter.MapToType(order);
            var response =await _studentRepository.OrderTranscriptAsync(requestEntity);
            return response;
        }


        public async Task<string> CheckTranscriptStatusAsync(string orderId, string currentStatusCode)
        {
            var response = await _studentRepository.CheckTranscriptStatusAsync(orderId, currentStatusCode);
            return response;

        }

        /// <summary>
        /// Given a student ID and the path to a report spec, generate the student unofficial transcript
        /// </summary>
        /// <param name="id">The student's ID</param>
        /// <param name="path">The path to the report spec</param>
        /// <returns>The unofficial transcript</returns>
        public async Task<Tuple<byte[],string>> GetUnofficialTranscriptAsync(string studentId, string path, string transcriptGrouping, string reportWatermarkPath, string deviceInfoPath)
        {
            // Make sure the current user has permission to view the student's transcript information
            var student = await _studentRepository.GetAsync(studentId);
            await CheckStudentAdvisorUserAccessAsync(studentId, student.ConvertToStudentAccess());

            var transcriptText = await _studentRepository.GetTranscriptAsync(studentId, transcriptGrouping);

            // Create the report object, set it's path, and set permissions for the sandboxed app domain in which the report runs to unrestricted
            LocalReport report = new LocalReport();
            report.ReportPath = path;
            report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
            report.EnableExternalImages = true;
            var parameters = new List<ReportParameter>();
            parameters.Add(new ReportParameter("WatermarkPath", reportWatermarkPath));
            parameters.Add(new ReportParameter("TranscriptText", transcriptText));
            report.SetParameters(parameters);

            // Set up some options for the report
            string mimeType = string.Empty;
            string encoding;
            string fileNameExtension;
            Warning[] warnings;
            string[] streams;

            // See if we can get device info from txt file. If not use defaults.
            string DeviceInfo;
            try
            {
                using (StreamReader deviceInfoTxt = new StreamReader(deviceInfoPath))
                {
                    DeviceInfo = deviceInfoTxt.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                logger.Info("Unable to read txt file UnofficialTranscriptDeviceInfo.txt. Using defaults instead.");
                logger.Info(e.Message);
                DeviceInfo = "<DeviceInfo>" +
                " <OutputFormat>PDF</OutputFormat>" +
                " <PageWidth>8.5in</PageWidth>" +
                " <PageHeight>11in</PageHeight>" +
                " <MarginTop>0.5in</MarginTop>" +
                " <MarginLeft>0.5in</MarginLeft>" +
                " <MarginRight>0.5in</MarginRight>" +
                " <MarginBottom>0.5in</MarginBottom>" +
                "</DeviceInfo>";
            }

            // Render the report as a byte array
            string ReportType = "PDF";
            var renderedBytes = report.Render(
                ReportType,
                DeviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            // Recover and free up memory
            report.DataSources.Clear();
            report.ReleaseSandboxAppDomain();
            report.Dispose();

            // Now, since we have the student entity here go ahead and build the file name to use and return it as well.
            var filenameToUse = Regex.Replace(
                            (student.LastName +
                            " " + student.FirstName +
                            " " + student.Id +
                            " " + DateTime.Now.ToShortDateString()),
                            "[^a-zA-Z0-9_]", "_")
                            + ".pdf";
            return new Tuple<byte[], string>(renderedBytes, filenameToUse) ;
        }

        /// <summary>
        /// Process registration requests for a student
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="sectionRegistrations">Section registrations to be processed</param>
        /// <returns>A Registration Response containing any messages returned by registration</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.RegistrationResponse> RegisterAsync(string studentId, IEnumerable<Ellucian.Colleague.Dtos.Student.SectionRegistration> sectionRegistrations)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must supply a studentId");
            }

            if (sectionRegistrations == null || sectionRegistrations.Count() == 0)
            {
                throw new ArgumentNullException("sectionsRegistrations", "You must supply at least one Section Registration to be processed.");
            }

            var messages = new List<Ellucian.Colleague.Dtos.Student.RegistrationMessage>();

            // Prevent action without proper permissions - If user is self continue - otherwise check permissions.
            if (!UserIsSelf(studentId))
            {
                // Make sure user has permissions to update this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckRegisterPermissionsAsync(studentId);
            }

            var sectionRegistrationEntities = new List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistration>();
            foreach (var sectionReg in sectionRegistrations)
            {
                sectionRegistrationEntities.Add(new Ellucian.Colleague.Domain.Student.Entities.SectionRegistration()
                {
                    Action = (Ellucian.Colleague.Domain.Student.Entities.RegistrationAction)sectionReg.Action,
                    Credits = sectionReg.Credits,
                    SectionId = sectionReg.SectionId,
                    DropReasonCode = sectionReg.DropReasonCode                    
                });
            }

            var request = new RegistrationRequest(studentId, sectionRegistrationEntities);
            var responseEntity = await _studentRepository.RegisterAsync(request);
            var responseDto = new Ellucian.Colleague.Dtos.Student.RegistrationResponse();
            responseDto.Messages = new List<Ellucian.Colleague.Dtos.Student.RegistrationMessage>();
            responseDto.PaymentControlId = responseEntity.PaymentControlId;

            foreach (var message in responseEntity.Messages)
            {
                responseDto.Messages.Add(new Ellucian.Colleague.Dtos.Student.RegistrationMessage { Message = message.Message, SectionId = message.SectionId });
            }

            return responseDto;
        }

        /// <summary>
        /// Throws an exception if the current user is not authorized to access the student's data. Same as CheckUserAccess, but
        /// does not read the student if needed. If the calling controller method already needs to read student, then it should use 
        /// Get(studentId) which will throw an exception if the user does not have access.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        public async Task CheckStudentAccessAsync(string studentId)
        {
            if (!UserIsSelf(studentId))
            {
                // This will throw an exception if the user does not have access to this student's data
                await CheckUserAccessAsync(studentId);
            }
        }
      
        public async Task<PrivacyWrapper<Dtos.Student.Student>> GetAsync(string id)
        {
            var hasPrivacyRestriction = false;
            Dtos.Student.Student student =null;

            // Get student entity 
            var studentEntity = await _studentRepository.GetAsync(id);
            // Throw error if student not found
            if (studentEntity == null)
            {
                throw new KeyNotFoundException("Student not found in repository");
            }
            // Make sure user has access to this student--If not, method throws exception
            await CheckUserAccessAsync(id, studentEntity.ConvertToStudentAccess());
            if (!UserIsSelf(id) && !HasProxyAccessForPerson(id))
            {
                //if appropriate permissions exists then check if student have a privacy code and logged-in user have a staff record with same privacy code.
                 hasPrivacyRestriction = string.IsNullOrEmpty(studentEntity.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(studentEntity.PrivacyStatusCode);
            }
            if (hasPrivacyRestriction)
            {
                student = new  Dtos.Student.Student()
                {
                    LastName = studentEntity.LastName,
                    FirstName = studentEntity.FirstName,
                    MiddleName = studentEntity.MiddleName,
                    Id = studentEntity.Id,
                    PrivacyStatusCode = studentEntity.PrivacyStatusCode
                };
            }
            else
            {
                // Build and return the student dto
                var studentDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Dtos.Student.Student>();
                 student = studentDtoAdapter.MapToType(studentEntity);
            }
            return new PrivacyWrapper<Dtos.Student.Student>(student, hasPrivacyRestriction);
        }

        /// <summary>
        /// Search Students either by student id or student name
        /// </summary>
        /// <param name="criteria">Id or name of student</param>
        /// <param name="pageSize">Number of records to retrieve</param>
        /// <param name="pageIndex">Current page number</param>
        /// <returns>List of students whose search creteria mathces</returns>
        public async Task<PrivacyWrapper<List<Ellucian.Colleague.Dtos.Student.Student>>> Search3Async(StudentSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            var hasPrivacyRestriction = false; // Default to false, will be set properly when retreiving advisees

            List<Domain.Student.Entities.Student> students = null;

            string searchString = criteria.StudentKeyword;

            List<Dtos.Student.Student> studentDtos = new List<Dtos.Student.Student>();
            var studentAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>();

            // Remove extra blank spaces
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");

            // If search string is a numeric ID and it is for a particular advisee, return only that
            double personId;
            bool isId = double.TryParse(searchString, out personId);
            if (isId && !string.IsNullOrEmpty(criteria.StudentKeyword))
            {
                await CheckUserAccessAsync(CurrentUser.UserId, null);

                // If the requested ID is not an assigned advisee but the advisor can only view assigned advisees, then return an empty list

                //if (!planningStudents.Select(x => x.Id).Contains(searchString))
                //{
                //    return new PrivacyWrapper<List<Dtos.Student.Student>>(new List<Dtos.Student.Student>(), hasPrivacyRestriction);
                //}

                // Validate the ID - if invalid, error will be thrown
                //var student1 = await _adviseeRepository.GetAsync(searchString);
                students = (await _studentRepository.GetStudentsSearchAsync(new List<string>() { searchString })).ToList();
                // If valid, return the ID. If not found, return null
                if (students == null)
                {
                    return new PrivacyWrapper<List<Dtos.Student.Student>>(new List<Dtos.Student.Student>(), hasPrivacyRestriction);
                }
                else
                {

                    //var studentList = new List<Domain.Student.Entities.Student>() { student };
                    //var adviseeDegreePlansList = await _degreePlanRepository.GetAsync(students.Select(s => s.Id));

                    // loop through each advisee id
                    foreach (var stud in students)
                    {
                        try
                        {
                            // Before doing anything, check the current advisor's privacy code settings (on their staff record)
                            // against any privacy code on the student's record
                            var adviseeHasPrivacyRestriction = string.IsNullOrEmpty(stud.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(stud.PrivacyStatusCode);

                            Dtos.Student.Student studentDtoe;

                            // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                            // then blank out the record, except for name, id, and privacy code
                            if (adviseeHasPrivacyRestriction)
                            {
                                hasPrivacyRestriction = true;
                                studentDtoe = new Dtos.Student.Student()
                                {
                                    LastName = stud.LastName,
                                    FirstName = stud.FirstName,
                                    MiddleName = stud.MiddleName,
                                    Id = stud.Id,
                                    PrivacyStatusCode = stud.PrivacyStatusCode
                                };
                            }
                            else
                            {
                                studentDtoe = studentAdapter.MapToType(stud);
                                studentDtoe.DegreePlanId = null;
                                if (stud.PreferredEmailAddress != null)
                                {
                                    studentDtoe.PreferredEmailAddress = stud.PreferredEmailAddress.Value;
                                }
                            }
                            studentDtos.Add(studentDtoe);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Failed to build advisee dto for advisee - " + stud.Id);
                            logger.Error(ex, ex.Message);
                        }
                    }

                    return new PrivacyWrapper<List<Dtos.Student.Student>>(studentDtos, hasPrivacyRestriction);

                }
            }
            //IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.DegreePlan> adviseeDegreePlans = null;
            PrivacyWrapper<List<Dtos.Student.Student>> privacyWrapper = null;

            // Otherwise, we are doing a name search of advisees or the advisors - parse the search string into name parts
            string lastName = null;
            string firstName = null;
            string middleName = null;

            ParseNames(searchString, ref lastName, ref firstName, ref middleName);
            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentException("Either an id or a first and last name must be supplied.");
            }


            // Regular expression for all punctuation and numbers to remove from name string
            Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
            Regex regexNotSpace = new Regex(@"\s");

            var nameStrings = searchString.Split(',');
            // If there was a comma, set the first item to last name
            if (nameStrings.Count() > 1)
            {
                lastName = nameStrings.ElementAt(0).Trim();
                if (nameStrings.Count() >= 2)
                {
                    // parse the two items after the comma using a space. Ignore anything else
                    var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                    if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                    if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                }
            }
            else
            {
                // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                // Blank values don't hurt anything.
                nameStrings = searchString.Split(' ');
                switch (nameStrings.Count())
                {
                    case 1:
                        lastName = nameStrings.ElementAt(0).Trim();
                        break;
                    case 2:
                        firstName = nameStrings.ElementAt(0).Trim();
                        lastName = nameStrings.ElementAt(1).Trim();
                        break;
                    default:
                        firstName = nameStrings.ElementAt(0).Trim();
                        middleName = nameStrings.ElementAt(1).Trim();
                        lastName = nameStrings.ElementAt(2).Trim();
                        break;
                }
            }
            // Remove characters that won't make sense for each name part, including all punctuation and numbers 
            if (lastName != null)
            {
                lastName = regexNotPunc.Replace(lastName, "");
                lastName = regexNotSpace.Replace(lastName, "");
            }
            if (firstName != null)
            {
                firstName = regexNotPunc.Replace(firstName, "");
                firstName = regexNotSpace.Replace(firstName, "");
            }
            if (middleName != null)
            {
                middleName = regexNotPunc.Replace(middleName, "");
                middleName = regexNotSpace.Replace(middleName, "");
            }

            // Now that the search string is parsed into the appropriate pieces, do the appropriate search based on which keyword was supplied

            if (!string.IsNullOrEmpty(criteria.StudentKeyword))
            {
                await CheckUserAccessAsync(CurrentUser.UserId, null);
                //// Advisee Name Search: Call repository method to query students based on name strings
                //// Do the search based on the advisor's permissions
                students = (await _studentRepository.GetStudentSearchByNameAsync(lastName, firstName, middleName, pageSize, pageIndex)).ToList();

            }

            var watch = new Stopwatch();
            watch.Start();

            //adviseeDegreePlans = await _degreePlanRepository.GetAsync(students.Select(s => s.Id));

            //===================================================
            watch.Stop();
            logger.Info("Get Advisee Plans... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();
            //var studentNamesList = new List<Domain.Student.Entities.Student>() { students };
            // loop through each advisee id
            foreach (var studente in students)
            {
                try
                {
                    // Before doing anything, check the current advisor's privacy code settings (on their staff record)
                    // against any privacy code on the student's record
                    var adviseeHasPrivacyRestriction = string.IsNullOrEmpty(studente.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(studente.PrivacyStatusCode);

                    Dtos.Student.Student studentDtoe;

                    // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                    // then blank out the record, except for name, id, and privacy code
                    if (adviseeHasPrivacyRestriction)
                    {
                        hasPrivacyRestriction = true;
                        studentDtoe = new Dtos.Student.Student()
                        {
                            LastName = studente.LastName,
                            FirstName = studente.FirstName,
                            MiddleName = studente.MiddleName,
                            Id = studente.Id,
                            PrivacyStatusCode = studente.PrivacyStatusCode
                        };
                    }
                    else
                    {
                        studentDtoe = studentAdapter.MapToType(studente);
                        studentDtoe.DegreePlanId = null;
                        if (studente.PreferredEmailAddress != null)
                        {
                            studentDtoe.PreferredEmailAddress = studente.PreferredEmailAddress.Value;
                        }
                    }
                    studentDtos.Add(studentDtoe);
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to build advisee dto for advisee - " + studente.Id);
                    logger.Error(ex, ex.Message);
                }
            }

            //===================================================
            watch.Stop();
            logger.Info("Build Advisee DTOs... completed in " + watch.ElapsedMilliseconds.ToString());

            return new PrivacyWrapper<List<Dtos.Student.Student>>(studentDtos, hasPrivacyRestriction);

        }


        private void ParseNames(string criteria, ref string lastName, ref string firstName, ref string middleName)
        {
            // Regular expression for all punctuation and numbers to remove from name string
            Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
            Regex regexNotSpace = new Regex(@"\s");

            var nameStrings = criteria.Split(',');
            // If there was a comma, set the first item to last name
            if (nameStrings.Count() > 1)
            {
                lastName = nameStrings.ElementAt(0).Trim();
                if (nameStrings.Count() >= 2)
                {
                    // parse the two items after the comma using a space. Ignore anything else
                    var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                    if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                    if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                }
            }
            else
            {
                // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                // Blank values don't hurt anything.
                nameStrings = criteria.Split(' ');
                switch (nameStrings.Count())
                {
                    case 1:
                        lastName = nameStrings.ElementAt(0).Trim();
                        break;
                    case 2:
                        firstName = nameStrings.ElementAt(0).Trim();
                        lastName = nameStrings.ElementAt(1).Trim();
                        break;
                    default:
                        firstName = nameStrings.ElementAt(0).Trim();
                        middleName = nameStrings.ElementAt(1).Trim();
                        lastName = nameStrings.ElementAt(2).Trim();
                        break;
                }
            }
            // Remove characters that won't make sense for each name part, including all punctuation and numbers 
            if (lastName != null)
            {
                lastName = regexNotPunc.Replace(lastName, "");
                lastName = regexNotSpace.Replace(lastName, "");
            }
            if (firstName != null)
            {
                firstName = regexNotPunc.Replace(firstName, "");
                firstName = regexNotSpace.Replace(firstName, "");
            }
            if (middleName != null)
            {
                middleName = regexNotPunc.Replace(middleName, "");
                middleName = regexNotSpace.Replace(middleName, "");
            }

        }

        #region Student Cohort
        /// <summary>
        /// Gets all student cohorts
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.StudentCohort>> GetAllStudentCohortsAsync(bool bypassCache)
        {
            IEnumerable<Domain.Student.Entities.StudentCohort> studentCohortEntities = await _studentReferenceDataRepository.GetAllStudentCohortAsync(bypassCache);
            List<Dtos.StudentCohort> studentCohortsDtos = new List<Dtos.StudentCohort>();

            foreach (var studentCohortEntity in studentCohortEntities)
            {
                Dtos.StudentCohort studentCohortDto = BuildStudentCohort(studentCohortEntity);
                studentCohortsDtos.Add(studentCohortDto);
            }
            return studentCohortsDtos;
        }       

        /// <summary>
        /// Gets student cohort by guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.StudentCohort> GetStudentCohortByGuidAsync(string id)
        {
            Domain.Student.Entities.StudentCohort studentCohortEntity = (await _studentReferenceDataRepository.GetAllStudentCohortAsync(true))
                                                                        .FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (studentCohortEntity == null)
            {
                throw new KeyNotFoundException("student-cohorts not found for GUID : " + id);
            }
            return  BuildStudentCohort(studentCohortEntity);
        }

        /// <summary>
        /// Builds student cohort dto
        /// </summary>
        /// <param name="studentCohortEntity"></param>
        /// <returns></returns>
        private Dtos.StudentCohort BuildStudentCohort(StudentCohort studentCohortEntity)
        {
            Dtos.StudentCohort studentCohortDto = new Dtos.StudentCohort();
            studentCohortDto.Id = studentCohortEntity.Guid;
            studentCohortDto.Code = studentCohortEntity.Code;
            studentCohortDto.Description = studentCohortEntity.Description;
            studentCohortDto.Title = studentCohortEntity.Description;

            return studentCohortDto;
        }

        #endregion

        #region StudentClassification

        /// <summary>
        /// Gets all student classifications
        /// </summary>
        /// <param name="bypassCache">bypassCache</param>
        /// <returns>IEnumerable<Dtos.StudentClassification></returns>
        public async Task<IEnumerable<Dtos.StudentClassification>> GetAllStudentClassificationsAsync(bool bypassCache)
        {
            IEnumerable<Domain.Student.Entities.StudentClassification> studentClassificationEntities = await _studentReferenceDataRepository.GetAllStudentClassificationAsync(bypassCache);
            List<Dtos.StudentClassification> studentClassificationsDtos = new List<Dtos.StudentClassification>();

            foreach (var studentClassificationEntity in studentClassificationEntities)
            {
                Dtos.StudentClassification studentClassificationDto = BuildStudentClassificationDto(studentClassificationEntity);
                studentClassificationsDtos.Add(studentClassificationDto);
            }
            return studentClassificationsDtos;
        }

        /// <summary>
        /// Gets student classification by guid
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Dtos.StudentClassification</returns>
        public async Task<Dtos.StudentClassification> GetStudentClassificationByGuidAsync(string id)
        {
            Domain.Student.Entities.StudentClassification studentClassificationEntity = (await _studentReferenceDataRepository.GetAllStudentClassificationAsync(true))
                                                                                            .FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (studentClassificationEntity == null)
            {
                throw new KeyNotFoundException("No student classificaion found with id: " + id);
            }
            Dtos.StudentClassification studentClassificationDto = BuildStudentClassificationDto(studentClassificationEntity);
            return studentClassificationDto;
        }

        /// <summary>
        /// Converts entity to dto
        /// </summary>
        /// <param name="studentClassificationEntity">studentClassificationEntity</param>
        /// <returns>Dtos.StudentClassification</returns>
        private Dtos.StudentClassification BuildStudentClassificationDto(StudentClassification studentClassificationEntity)
        {
            Dtos.StudentClassification studentClassificationDto = new Dtos.StudentClassification() 
            {
                Code = studentClassificationEntity.Code,
                Description = null,
                Id = studentClassificationEntity.Guid,
                Title = studentClassificationEntity.Description
            };
            return studentClassificationDto;
        }

        #endregion

        #region Resident Types
        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Gets all resident types
        /// </summary>
        /// <returns>Collection of ResidentType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ResidentType>> GetResidentTypesAsync(bool bypassCache = false)
        {
            var residentTypeCollection = new List<Ellucian.Colleague.Dtos.ResidentType>();

            var residentTypeEntities = await _studentRepository.GetResidencyStatusesAsync(bypassCache);
            if (residentTypeEntities != null && residentTypeEntities.Count() > 0)
            {
                foreach (var residentType in residentTypeEntities)
                {
                    residentTypeCollection.Add(ConvertResidentTypeEntityToDto(residentType));
                }
            }
            return residentTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Get an resident type from its ID
        /// </summary>
        /// <returns>ResidentType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ResidentType> GetResidentTypeByIdAsync(string id)
        {
            try
            {
                return ConvertResidentTypeEntityToDto((await _studentRepository.GetResidencyStatusesAsync(true)).Where(st => st.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Resident Type not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Converts an ResidentType domain entity to its corresponding ResidentType DTO
        /// </summary>
        /// <param name="source">ResidentType domain entity</param>
        /// <returns>ResidentType DTO</returns>
        private Ellucian.Colleague.Dtos.ResidentType ConvertResidentTypeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.ResidencyStatus source)
        {
            var residentType = new Ellucian.Colleague.Dtos.ResidentType();

            residentType.Id = source.Guid;
            residentType.Code = source.Code;
            residentType.Title = source.Description;
            residentType.Description = null;

            return residentType;
        }

        #endregion

        #region Ellucian Data Model Students
        /// <summary>
        /// Get an Student from its GUID
        /// </summary>
        /// <returns>A Student DTO <see cref="Ellucian.Colleague.Dtos.Students">object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.Students> GetStudentsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Student.");
            }

            CheckGetStudentViewPermission();
            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                logger.Info("StudentService Timing: (GetStudentsByGuidAsync) GetStudentFromGuidAsync " +
                            "repo lookup started");
                watch.Restart();
            }

            var studentEntity = await _studentRepository.GetDataModelStudentFromGuidAsync(guid);
            if (studentEntity == null)
            {
                throw new KeyNotFoundException("Student not found for GUID " + guid);

            }

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("StudentService Timing: (GetStudentsByGuidAsync) repo lookup in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("StudentService Timing: (GetStudentsByGuidAsync) service conversion started");
                watch.Restart();
            }

            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync
                (new List<string>() { studentEntity.Id });
            var retval =  (await ConvertStudentsEntityToStudentsDto(studentEntity, personGuidCollection, bypassCache));

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("StudentService Timing: (GetStudentsByGuidAsync) service conversion in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            return retval;
        }

        /// <summary>
        /// Get a list of Students
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="person">GUID for a reference to link a student to the common HEDM persons entity.</param>
        /// <param name="type">GUID for the type of the student.</param>
        /// <param name="cohorts">GUID for the groupings of students for reporting/tracking purposes (cohorts) to which the student is associated.</param>
        /// <param name="residency">GUID for the residency type for selecting students.</param>
        /// <returns>A Student DTO <see cref="Ellucian.Colleague.Dtos.Students">object</see></returns>     
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Students>, int>> GetStudentsAsync(int offset, 
             int limit, bool bypassCache = false, string person = "", string type = "", string cohorts ="", string residency = "")
        {
            try
            {
                CheckGetStudentViewPermission();

                var newPerson = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {
                    try
                    {
                        newPerson = await _personRepository.GetPersonIdFromGuidAsync(person);
                        if (string.IsNullOrEmpty(newPerson))
                        {
                            throw new KeyNotFoundException(string.Concat("Person not found for guid:", person));
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        return new Tuple<IEnumerable<Dtos.Students>, int>(new List<Dtos.Students>(), 0);
                    }
                    //newPerson = await _personRepository.GetPersonIdFromGuidAsync(person);
                    //if (string.IsNullOrEmpty(newPerson))
                    //{
                    //    throw new KeyNotFoundException(string.Concat("Person not found for guid:", person));
                    //}
                }

                var newCohort = string.Empty;
                if (!string.IsNullOrEmpty(cohorts))
                {
                    try
                    {
                        var allStudentCohorts = (await GetAllStudentCohortsAsync(bypassCache)).ToList();
                        if (allStudentCohorts.Any())
                        {
                            var studentCohort = allStudentCohorts.FirstOrDefault(st => st.Id == cohorts);
                            newCohort = studentCohort != null ? studentCohort.Code : string.Empty;
                        }
                        if (string.IsNullOrEmpty(newCohort))
                        {
                            throw new KeyNotFoundException(string.Concat("Cohort not found for guid:", cohorts));
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        return new Tuple<IEnumerable<Dtos.Students>, int>(new List<Dtos.Students>(), 0);
                    }
                    //var allStudentCohorts = (await GetAllStudentCohortsAsync(bypassCache)).ToList();
                    //if (allStudentCohorts.Any())
                    //{
                    //    var studentCohort = allStudentCohorts.FirstOrDefault(st => st.Id == cohorts);
                    //    newCohort = studentCohort != null ? studentCohort.Code : string.Empty;
                    //}
                    //if (string.IsNullOrEmpty(newCohort))
                    //{
                    //    throw new KeyNotFoundException(string.Concat("Cohort not found for guid:", cohorts));
                    //}
                }

                var newType = string.Empty;
                if (!string.IsNullOrEmpty(type))
                {
                    try
                    {
                        var studentTypes = (await GetStudentTypesAsync(bypassCache)).ToList();
                        if (studentTypes.Any())
                        {
                            var studentType = studentTypes.FirstOrDefault(st => st.Guid == type);
                            newType = studentType != null ? studentType.Code : string.Empty;
                        }
                        if (string.IsNullOrEmpty(newType))
                        {
                            throw new KeyNotFoundException(string.Concat("Type not found for guid:", type));
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        return new Tuple<IEnumerable<Dtos.Students>, int>(new List<Dtos.Students>(), 0);
                    }
                    //var studentTypes = (await GetStudentTypesAsync(bypassCache)).ToList();
                    //if (studentTypes.Any())
                    //{
                    //    var studentType = studentTypes.FirstOrDefault(st => st.Guid == type);
                    //    newType = studentType != null ? studentType.Code : string.Empty;
                    //}
                    //if (string.IsNullOrEmpty(newType))
                    //{
                    //    throw new KeyNotFoundException(string.Concat("Type not found for guid:", type));
                    //}
                }

                var newResidency = string.Empty;
                if (!string.IsNullOrEmpty(residency))
                {
                    try
                    {
                        var residencyTypes = (await GetResidentTypesAsync(bypassCache)).ToList();
                        if (residencyTypes.Any())
                        {
                            var residencyType = residencyTypes.FirstOrDefault(st => st.Id == residency);
                            newResidency = residencyType != null ? residencyType.Code : string.Empty;
                        }
                        if (string.IsNullOrEmpty(newResidency))
                        {
                            throw new KeyNotFoundException(string.Concat("Residency type filter GUID not found for:", residency));
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        return new Tuple<IEnumerable<Dtos.Students>, int>(new List<Dtos.Students>(), 0);
                    }
                    //var residencyTypes = (await GetResidentTypesAsync(bypassCache)).ToList();
                    //if (residencyTypes.Any())
                    //{
                    //    var residencyType = residencyTypes.FirstOrDefault(st => st.Id == residency);
                    //    newResidency = residencyType != null ? residencyType.Code : string.Empty;
                    //}
                    //if (string.IsNullOrEmpty(newResidency))
                    //{
                    //    throw new KeyNotFoundException(string.Concat("Residency type filter GUID not found for:", residency));
                    //}
                }
                var studentsEntitiesTuple = await _studentRepository.GetDataModelStudentsAsync(offset, limit, bypassCache, newPerson, newType, newCohort, newResidency);

                if (studentsEntitiesTuple != null)
                {
                    var studentsEntities = studentsEntitiesTuple.Item1.ToList();
                    var totalCount = studentsEntitiesTuple.Item2;

                    if (studentsEntities.Any())
                    {
                        var students = new List<Colleague.Dtos.Students>();

                        var ids = studentsEntities.Where(x => (!string.IsNullOrEmpty(x.Id))).Select(x => x.Id).Distinct().ToList();
                        var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

                        foreach (var student in studentsEntities)
                        {
                            students.Add(await ConvertStudentsEntityToStudentsDto(student, personGuidCollection, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.Students>, int>(students, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.Students>, int>(new List<Dtos.Students>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.Students>, int>(new List<Dtos.Students>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Converts a Student domain entity to its corresponding Student  DTO
        /// </summary>
        /// <param name="student">A list of <see cref="Dtos.Student">Student</see> domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>A <see cref="Dtos.Students">Students</see> DTO</returns>
        private async Task<Students> ConvertStudentsEntityToStudentsDto(Domain.Student.Entities.Student student,
            Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            if (student == null)
            {
                throw new ArgumentNullException("Student is required.");
            }
            if (string.IsNullOrEmpty(student.StudentGuid))
            {
                throw new ArgumentNullException("Guid for student is required.");
            }
            if (string.IsNullOrEmpty(student.Id))
            {
                throw new ArgumentNullException("Id for student is required.");
            }

            var studentDto = new Colleague.Dtos.Students();

            try
            {
                studentDto.Id = student.StudentGuid;

                if (personGuidCollection != null && personGuidCollection.Any())
                {
                    //var studentGuid = await _personRepository.GetPersonGuidFromIdAsync(student.Id);
                    var studentGuid = string.Empty;
                    personGuidCollection.TryGetValue(student.Id, out studentGuid);
                    if (!string.IsNullOrEmpty(studentGuid))
                    {
                        studentDto.Person = new Dtos.GuidObject2(studentGuid);
                    }
                }

                if ((student.StudentTypeInfo != null) && (student.StudentTypeInfo.Any()))
                {
                    var type = student.StudentTypeInfo.OrderByDescending(st => st.TypeDate < DateTime.Now).FirstOrDefault();
                    if (type != null)
                    {
                        var studentTypes = await GetStudentTypesAsync(bypassCache);
                        if (studentTypes != null)
                        {
                            var studentType = studentTypes.FirstOrDefault(st => st.Code == type.Type);
                            studentDto.Type = studentType == null ? null : new GuidObject2(studentType.Guid);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(student.ResidencyStatus))
                {
                    var residencyTypes = await GetResidentTypesAsync(bypassCache);
                    if (residencyTypes != null)
                    {
                        var residentType = residencyTypes.FirstOrDefault(rt => rt.Code == student.ResidencyStatus);
                        studentDto.Residency = residentType == null ? null : new GuidObject2(residentType.Id);
                    }
                }


                if (student.StudentAcademicLevels != null)
                {
                    var cohorts = new List<GuidObject2>();
                    var allStudentCohorts = (await GetAllStudentCohortsAsync(bypassCache)).ToList();
                    if (allStudentCohorts.Any())
                    {
                        foreach (var academicLevel in student.StudentAcademicLevels)
                        {
                            if ((academicLevel.StudentAcademicLevelCohorts != null) && (academicLevel.StudentAcademicLevelCohorts.Any()))
                            {
                                foreach (var academicLevelCohorts in academicLevel.StudentAcademicLevelCohorts)
                                {
                                    var studentCohort = allStudentCohorts.FirstOrDefault(sc => sc.Code == academicLevelCohorts.OtherCohortGroup);
                                    if (studentCohort == null) continue;
                                    cohorts.Add(new GuidObject2(studentCohort.Id));
                                }
                            }
                        }
                    }
                    if (cohorts.Any())
                    {
                        studentDto.Cohorts = cohorts;
                    }
                }

                var measures = new List<PerformanceMeasureDtoProperty>();
                foreach (var acadLevel in student.StudentAcademicLevels)
                {
                    var measure = new PerformanceMeasureDtoProperty();

                    if (!string.IsNullOrEmpty(acadLevel.AcademicLevel))
                    {
                        var academicLevels = (await GetAcademicLevelsAsync(bypassCache)).ToList();
                        if (academicLevels.Any())
                        {
                            var level = academicLevels.FirstOrDefault(al => al.Code == acadLevel.AcademicLevel);
                            if (level != null)
                            {
                                measure.Level = new GuidObject2(level.Guid);
                            }
                        }
                        if (student.PerformanceMeasures != null && student.PerformanceMeasures.Any())
                        {
                            var performanceMeasure = string.Empty;
                            student.PerformanceMeasures.TryGetValue(acadLevel.AcademicLevel, out performanceMeasure);
                            measure.PerformanceMeasure = !(string.IsNullOrEmpty(performanceMeasure))
                                    ? performanceMeasure : null;
                        }
                    }
                    if (!string.IsNullOrEmpty(acadLevel.ClassLevel))
                    {
                        var allStudentClassification = (await GetAllStudentClassificationAsync(bypassCache)).ToList();
                        if (allStudentClassification.Any())
                        {
                            var studentClassification = allStudentClassification.FirstOrDefault(sc => sc.Code == acadLevel.ClassLevel);

                            if (studentClassification != null)
                            {
                                measure.Classification = new GuidObject2(studentClassification.Guid);
                            }
                        }
                    }
                    measures.Add(measure);
                }
                if (measures.Any())
                {
                    studentDto.Measures = measures;
                }

                return studentDto;
            }
            catch (Exception ex)
            {
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(ex, "Student exception occurred: ");
                }
                throw new Exception("Student exception occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Students.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetStudentViewPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewStudentInformation);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Student.");
            }
        }
        #endregion


    }
}
