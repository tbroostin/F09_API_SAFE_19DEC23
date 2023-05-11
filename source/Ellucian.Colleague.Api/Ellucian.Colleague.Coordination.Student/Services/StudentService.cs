// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Http.Exceptions;
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
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using Ellucian.Colleague.Coordination.Base.Reports;
using System.Xml;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

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
        private readonly IPlanningStudentRepository _planningStudentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IProgramRepository _programRepository;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly ITranscriptGroupingRepository _transcriptGroupingRepository;
        private ILogger _logger;

        public StudentService(IAdapterRegistry adapterRegistry, IStudentRepository studentRepository, IPersonRepository personRepository,
            IAcademicCreditRepository academicCreditRepository, IAcademicHistoryService academicHistoryService,
            ITermRepository termRepository, IRegistrationPriorityRepository priorityRepository, IStudentConfigurationRepository
            studentConfigurationRepository, IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository, IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IStaffRepository staffRepository, ILogger logger,
            IPlanningStudentRepository planningStudentRepository, ISectionRepository sectionRepository, IProgramRepository programRepository,
            IStudentProgramRepository studentProgramRepository, ITranscriptGroupingRepository transcriptGroupingRepository)
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
            _planningStudentRepository = planningStudentRepository;
            _sectionRepository = sectionRepository;
            _programRepository = programRepository;
            _studentProgramRepository = studentProgramRepository;
            _transcriptGroupingRepository = transcriptGroupingRepository;
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
        /// Get StudentBatch3 DTO which returns Students without Address and Name Hierarchies and with Academic Level information
        /// Allows a null marital status and filters student advisements/advisors by incoming term
        /// Allows an alien status that indicates citizenship.
        /// Filter out student advisements which ended today or earlier.
        /// </summary>
        /// <param name="studentIds">List of Student Ids</param>
        /// <param name="inheritFromPerson">Flag to inherit Name/Address Hierarchy from Person (Default to false)</param>
        /// <param name="getDegreePlan">Flag to get a Degree Plan Id (Default to false)</param>
        /// <returns>List of StudentBatch3 objects</returns>
        public async Task<PrivacyWrapper<IEnumerable<Dtos.Student.StudentBatch3>>> QueryStudentsById4Async(IEnumerable<string> studentIds, bool inheritFromPerson = false, bool getDegreePlan = false, string term = null)
        {
            List<Dtos.Student.StudentBatch3> students = new List<Dtos.Student.StudentBatch3>();
            CheckGetPersonAndStudentInformationViewPermission();
            Domain.Student.Entities.Term termData = null;
            if (!string.IsNullOrEmpty(term))
            {
                termData = _termRepository.Get(term);
            }
            bool filterAdvisorsByTerm = true;
            bool filterEndedAdvisements = true;
            bool hasPrivacyRestriction = false;
            var citizenshipStatusData = await _referenceDataRepository.GetCitizenshipStatusesAsync(false);
            var studentEntities = await _studentRepository.GetStudentsByIdAsync(studentIds, termData, citizenshipStatusData, inheritFromPerson, getDegreePlan, filterAdvisorsByTerm, filterEndedAdvisements);
            if (studentEntities != null && studentEntities.Count() > 0)
            {
                var studentDtoAdapter = new StudentEntityToStudentBatch3DtoAdapter(_adapterRegistry, _logger);
                foreach (var student in studentEntities)
                {
                    try
                    {
                        // Before doing anything, check the current advisor's privacy code settings (on their staff record)
                        // against any privacy code on the student's record
                        var studentHasPrivacyRestriction = string.IsNullOrEmpty(student.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(student.PrivacyStatusCode);

                        Dtos.Student.StudentBatch3 studentDto;

                        // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                        // then blank out the record, except for name, id, and privacy code
                        if (studentHasPrivacyRestriction)
                        {
                            hasPrivacyRestriction = true;
                            studentDto = new Dtos.Student.StudentBatch3()
                            {
                                LastName = student.LastName,
                                FirstName = student.FirstName,
                                MiddleName = student.MiddleName,
                                Id = student.Id,
                                PrivacyStatusCode = student.PrivacyStatusCode
                            };
                        }
                        else
                        {
                            studentDto = studentDtoAdapter.MapToType(student);
                        }
                        students.Add(studentDto);
                    }
                    catch (ColleagueSessionExpiredException ce)
                    {
                        string message = string.Format("Colleague session expired while building StudentBatch3 DTO for student {0}", student.Id);
                        logger.Error(ce, message);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, string.Format("Failed to build StudentBatch3 DTO for student {0}", student.Id));
                        logger.Error(ex, ex.Message);
                    }
                }
            }
            return new PrivacyWrapper<IEnumerable<Dtos.Student.StudentBatch3>>(students, hasPrivacyRestriction);
        }

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

            var messageEntities = (await _studentRepository.CheckRegistrationEligibilityAsync(id)).Messages;

            var messages = new List<Dtos.Student.RegistrationMessage>();
            foreach (var message in messageEntities)
            {
                messages.Add(new Dtos.Student.RegistrationMessage { Message = message.Message, SectionId = message.SectionId });
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
        [Obsolete("OBSOLETE as of API 1.34. Please use CheckRegistrationEligibility3Async")]
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

            var registrationEligibilityDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>();
            Dtos.Student.RegistrationEligibility registrationEligibilityDto = registrationEligibilityDtoAdapter.MapToType(registrationEligibility);

            return registrationEligibilityDto;
        }

        /// <summary>
        /// Check if student is eligible for registration. Returns messages if student fails eligibility checks. Also indicates if current user
        /// has override permissions, enabling them to register the student even if ineligible.
        /// </summary>
        /// <param name="studentId">student Id</param>
        /// <returns><see cref="RegistrationEligibility">RegistrationEligibility</see> object, consisting of status information and  messages indicating eligibility or lack of it.
        /// In addition booleans indicating if the current user has override permissions.</returns>
        public async Task<Dtos.Student.RegistrationEligibility> CheckRegistrationEligibility3Async(string studentId)
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
            // CR-000148260 The registration priority messages need to be returned even if the user has override permissions
            // CR-000148260 Do not change the status for users with override permissions so that they are still able to register
            registrationEligibility.UpdateRegistrationPrioritiesIncludeMessagesForUsersWithOverrides(studentRegistrationPriorities, allTerms);

            var registrationEligibilityDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>();
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
            var studentDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>();

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
        public async Task<IEnumerable<Domain.Student.Entities.TranscriptRestriction>> GetTranscriptRestrictionsAsync(string studentId)
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
            try
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
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while getting transcript restrictions";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
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
                logger.Error(message);
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
            var termAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Term, Dtos.Student.Term>();

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

            // Make sure user has access to any student
            if (!HasPermission(PlanningPermissionCodes.ViewAnyAdvisee))
            {
                throw new PermissionsException("User does not have permissions to access to this function");
            }

            var response = await _studentRepository.OrderTranscriptAsync(requestEntity);
            return response;
        }

        public async Task<string> CheckTranscriptStatusAsync(string orderId, string currentStatusCode)
        {
            // Make sure user has access to any student
            if (!HasPermission(PlanningPermissionCodes.ViewAnyAdvisee))
            {
                throw new PermissionsException("User does not have permissions to access to this function");
            }

            var response = await _studentRepository.CheckTranscriptStatusAsync(orderId, currentStatusCode);
            return response;
        }

        /// <summary>
        /// Given a student ID, transcript grouping, and the path to a report spec, generate the student unofficial transcript
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="path">The path to the report spec</param>
        /// <param name="transcriptGrouping"></param>
        /// <param name="reportWatermarkPath"></param>
        /// <param name="deviceInfoPath"></param>
        /// <remarks>The PdfSharp reading and re-writing of the .NET assembled PDF is used to ensure the final, returned PDF uses PDF specification 1.4. </remarks>
        /// <returns>The unofficial transcript</returns>
        public async Task<Tuple<byte[], string>> GetUnofficialTranscriptAsync(string studentId, string path, string transcriptGrouping, string reportWatermarkPath, string deviceInfoPath)
        {
            try
            {
                // Make sure the current user has permission to view the student's transcript information
                var student = await _studentRepository.GetAsync(studentId);
                if (student == null)
                {
                    throw new KeyNotFoundException("Student with ID " + studentId + " not found in the repository.");
                }
                await CheckStudentAdvisorUserAccessAsync(studentId, student.ConvertToStudentAccess());

                // When request is made by a student (not an advisor)
                if (UserIsSelf(studentId))
                {
                    // Make sure the student does not have any transcript grouping restrictions when enforced - method throws permissions exception
                    await CheckStudentTranscriptRestrictionsAsync(studentId: studentId);
                }

                // Make sure the transcript grouping is valid and exists for the student's program(s) - method throws key not found exception
                await CheckStudentProgramTranscriptGroupingsAsync(studentId: studentId, transcriptGrouping: transcriptGrouping);

                var transcriptText = await _studentRepository.GetTranscriptAsync(studentId, transcriptGrouping);
                var transcriptConfiguration = await _studentConfigurationRepository.GetUnofficialTranscriptConfigurationAsync();

                byte[] renderedBytes = null;
                // Create the report object, set it's path, and set permissions for the sandboxed app domain in which the report runs to unrestricted
                using (LocalReport report = new LocalReport())
                {
                    report.ReportPath = path;
                    report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                    report.EnableExternalImages = true;

                    string fontSize = "9pt"; // Default font size for the report
                    double defaultTextboxWidth = 7.48959;
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
                        // Check if the device info should be set form TRFM parameters
                        if (transcriptConfiguration != null && transcriptConfiguration.IsUseTanscriptFormat)
                        {
                            fontSize = transcriptConfiguration.FontSize + "pt";
                            string pageWidth = transcriptConfiguration.PageWidth;
                            string pageHeight = transcriptConfiguration.PageHeight;

                            string marginTop = transcriptConfiguration.TopMargin;
                            string marginLeft = transcriptConfiguration.LeftMargin;
                            string marginRight = transcriptConfiguration.RightMargin;
                            string marginBottom = transcriptConfiguration.BottomMargin;

                            #region  Calculate Report Textbox width
                            double dPageWidth = 0;
                            Double.TryParse(pageWidth, out dPageWidth);

                            double dMarginLeft = 0;
                            Double.TryParse(marginLeft, out dMarginLeft);

                            double dMarginRight = 0;
                            Double.TryParse(marginRight, out dMarginRight);

                            // Report Textbox Width = (PageWidth - LeftMargin - RightMargin)
                            defaultTextboxWidth = (dPageWidth - dMarginLeft - dMarginRight);
                            #endregion

                            DeviceInfo = "<DeviceInfo>" +
                            " <OutputFormat>PDF</OutputFormat>" +
                            " <PageWidth>" + pageWidth + "in</PageWidth>" +
                            " <PageHeight>" + pageHeight + "in</PageHeight>" +
                            " <MarginTop>" + marginTop + "in</MarginTop>" +
                            " <MarginLeft>" + marginLeft + "in</MarginLeft>" +
                            " <MarginRight>" + marginRight + "in</MarginRight>" +
                            " <MarginBottom>" + marginBottom + "in</MarginBottom>" +
                            " <HumanReadablePDF>True</HumanReadablePDF>" +
                            "</DeviceInfo>";
                        }
                        else
                        {
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            //read the device info path in xml document
                            XmlDocument deviceInfoDoc = new XmlDocument();
                            deviceInfoDoc.Load(deviceInfoPath);
                            XmlNode deviceInfoRootNode = deviceInfoDoc.FirstChild;//this is to reach to DeviceInfo tag
                                                                                  // read the default settings in xml document
                            XmlDocument defaultDoc = new XmlDocument();
                            defaultDoc.LoadXml(PdfReportConstants.DeviceInfo);
                            XmlNode defaultDeviceInfoRootNode = defaultDoc.FirstChild; // this is to reach to DeviceInfo tag
                            if (defaultDeviceInfoRootNode.HasChildNodes)
                            {
                                //loop through all the default settings and if it is not in device info file (UnofficialTranscriptDeviceInfo.txt) then append it to the XMlDocumcent
                                //otherwise if it is already there then don't worry because default settings or tags could have been overridden by configurable device info file.
                                foreach (XmlNode childNode in defaultDeviceInfoRootNode.ChildNodes)
                                {
                                    var n = childNode.Name;
                                    XmlNodeList elemList = deviceInfoDoc.GetElementsByTagName(n);
                                    if (elemList == null || elemList.Count == 0)
                                    {
                                        deviceInfoRootNode.AppendChild(deviceInfoDoc.ImportNode(childNode, true));
                                    }
                                }
                            }

                            #region  Calculate Report Textbox width
                            // Get Page Width value from the deviceInfoDoc
                            double dPageWidth = 0;
                            XmlNodeList pageWidthNode = deviceInfoDoc.GetElementsByTagName("PageWidth");
                            if (pageWidthNode != null && pageWidthNode.Count > 0)
                            {
                                string pageWidth = pageWidthNode[0].InnerText.Replace("in", "");
                                Double.TryParse(pageWidth, out dPageWidth);
                            }

                            double dMarginLeft = 0;
                            XmlNodeList marginLeftNode = deviceInfoDoc.GetElementsByTagName("MarginLeft");
                            if (marginLeftNode != null && marginLeftNode.Count > 0)
                            {
                                string marginLeft = marginLeftNode[0].InnerText.Replace("in", "");
                                Double.TryParse(marginLeft, out dMarginLeft);
                            }

                            double dMarginRight = 0;
                            XmlNodeList marginRightNode = deviceInfoDoc.GetElementsByTagName("MarginRight");
                            if (marginRightNode != null && marginRightNode.Count > 0)
                            {
                                string marginRight = marginRightNode[0].InnerText.Replace("in", "");
                                Double.TryParse(marginRight, out dMarginRight);
                            }

                            defaultTextboxWidth = (dPageWidth - dMarginLeft - dMarginRight);

                            #endregion

                            DeviceInfo = deviceInfoDoc.InnerXml;
                            sw.Stop();
                            if (_logger.IsInfoEnabled)
                            {
                                _logger.Info(string.Format("Elapsed time to read device info and match with defaults by reading XML tags= {0}", sw.ElapsedMilliseconds));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Unable to read txt file UnofficialTranscriptDeviceInfo.txt. Using defaults instead.");
                        logger.Error(e, e.Message);
                        DeviceInfo = "<DeviceInfo>" +
                        " <OutputFormat>PDF</OutputFormat>" +
                        " <PageWidth>8.5in</PageWidth>" +
                        " <PageHeight>11in</PageHeight>" +
                        " <MarginTop>0.5in</MarginTop>" +
                        " <MarginLeft>0.5in</MarginLeft>" +
                        " <MarginRight>0.5in</MarginRight>" +
                        " <MarginBottom>0.5in</MarginBottom>" +
                        " <HumanReadablePDF>True</HumanReadablePDF>" +
                        "</DeviceInfo>";
                    }

                    try
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        //read the device info path in xml document
                        XmlDocument reportXml = new XmlDocument();
                        reportXml.Load(path);
                        XmlNodeList reportXmlNode = reportXml.ChildNodes;
                        XmlNode bodyXmlNode = reportXml.ChildNodes.Item(1).FirstChild;
                        XmlNode reportItemsXmlNode = bodyXmlNode.FirstChild;
                        XmlNode textboxXmlNode = reportItemsXmlNode.FirstChild;
                        XmlNode widthXmlNode = textboxXmlNode.ChildNodes.Item(4);
                        XmlNode revisedWidthXmlNode = textboxXmlNode.ChildNodes.Item(4);
                        revisedWidthXmlNode.FirstChild.Value = defaultTextboxWidth.ToString() + "in";
                        XmlNode savedWidthXmlNode = textboxXmlNode.ReplaceChild(revisedWidthXmlNode, widthXmlNode);
                        reportXml.Save(path);
                        sw.Stop();
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.Info(string.Format("Elapsed time to read RDLC as XML= {0}", sw.ElapsedMilliseconds));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, ex.Message);
                    }

                    var parameters = new List<ReportParameter>();
                    parameters.Add(new ReportParameter("WatermarkPath", reportWatermarkPath));
                    parameters.Add(new ReportParameter("TranscriptText", transcriptText));
                    parameters.Add(new ReportParameter("ReportFontSize", fontSize));
                    report.SetParameters(parameters);

                    // Render the report as a byte array
                    string ReportType = "PDF";
                    renderedBytes = report.Render(
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
                }
                byte[] reportByteArray = null;
                using (var pdfStream = PdfReader.Open(new MemoryStream(renderedBytes), PdfDocumentOpenMode.Import))
                {
                    var outputDocument = new PdfDocument();
                    foreach (PdfPage page in pdfStream.Pages)
                    {
                        outputDocument.AddPage(page);
                    }

                    var outputStream = new MemoryStream();
                    outputDocument.Save(outputStream);

                    reportByteArray = outputStream.ToArray();
                    outputStream.Close();
                }

                // Now, since we have the student entity here go ahead and build the file name to use and return it as well.
                var filenameToUse = Regex.Replace(
                                (student.LastName +
                                " " + student.FirstName +
                                " " + student.Id +
                                " " + DateTime.Now.ToShortDateString()),
                                "[^a-zA-Z0-9_]", "_")
                                + ".pdf";

                return new Tuple<byte[], string>(reportByteArray, filenameToUse);
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while getting student's unofficial transcript";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// when the student has enforced transcript restrictions throw a permissions exception.
        /// </summary>
        /// <param name="student"></param>
        private async Task CheckStudentTranscriptRestrictionsAsync(string studentId)
        {
            var transcriptAccess = await GetTranscriptRestrictions2Async(studentId);
            if (transcriptAccess != null && transcriptAccess.EnforceTranscriptRestriction &&
                transcriptAccess.TranscriptRestrictions != null && transcriptAccess.TranscriptRestrictions.Count > 0)
            {
                var message = "Student has enforced transcript restrictions and cannot access transcripts.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// when the student's program(s) do not include the given transcript grouping or the transcript grouping is not selectable, throw a key not found exception.
        /// </summary>
        /// <param name="studentId">Unique id of the student</param>
        /// <param name="transcriptGrouping">id of the transcript grouping</param>
        /// <returns></returns>
        private async Task CheckStudentProgramTranscriptGroupingsAsync(string studentId, string transcriptGrouping)
        {
            //get all transcript groupings
            var transcriptGroupings = await _transcriptGroupingRepository.GetAsync();
            if (transcriptGroupings == null || !transcriptGroupings.Any())
            {
                var message = "No Transcript Groupings have been defined.";
                _logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // filter to include only ones where IsUserSelectable is true
            var selectableGroupings = transcriptGroupings.Where(x => x != null && x.IsUserSelectable == true);

            // No transcript groupings have been defined as selectable for unofficial transcript printing
            if (selectableGroupings == null || !selectableGroupings.Any())
            {
                var message = "No Transcript Groupings have been defined as selectable.";
                _logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // check trascript grouping parameter is a valid selectable grouping for the student
            var programs = await _programRepository.GetAsync();
            var studentPrograms = await _studentProgramRepository.GetAsync(studentId);

            bool groupingFound = false;
            foreach (var studentProgram in studentPrograms)
            {
                if (studentProgram != null)
                {
                    //get the transcript grouping for the student's program from the program record
                    var program = programs.Where(p => p != null && p.Code == studentProgram.ProgramCode).FirstOrDefault();
                    if (program != null)
                    {
                        var programGrouping = !string.IsNullOrEmpty(program.UnofficialTranscriptGrouping) ?
                            program.UnofficialTranscriptGrouping : program.TranscriptGrouping;

                        //skip the program when the program transcript grouping is not the same as the passed transcript grouping or
                        //when the student's program grouping is not a selectable transcript grouping
                        if (programGrouping == transcriptGrouping && selectableGroupings.Any(stg => stg != null && stg.Id == programGrouping))
                        {
                            groupingFound = true;
                            break;
                        }
                    }
                }
            }

            if (groupingFound == false)
            {
                var message = "The transcript grouping '" + transcriptGrouping + "' does not match the student's program(s) or has not been defined as selectable.";
                _logger.Error(message);
                throw new KeyNotFoundException(message);
            }
        }

        /// <summary>
        /// Process registration requests for a student
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="sectionRegistrations">Section registrations to be processed</param>
        /// <returns>A Registration Response containing any messages returned by registration</returns>
        public async Task<Dtos.Student.RegistrationResponse> RegisterAsync(string studentId, IEnumerable<Dtos.Student.SectionRegistration> sectionRegistrations)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "You must supply a studentId");
            }

            if (sectionRegistrations == null || sectionRegistrations.Count() == 0)
            {
                throw new ArgumentNullException("sectionsRegistrations", "You must supply at least one Section Registration to be processed.");
            }

            var messages = new List<Dtos.Student.RegistrationMessage>();

            // Prevent action without proper permissions - If user is self continue - otherwise check permissions.
            if (!UserIsSelf(studentId))
            {
                // Make sure user has permissions to update this degree plan. 
                // If not, an PermissionsException will be thrown.
                await CheckRegisterPermissionsAsync(studentId);
            }

            var sectionRegistrationEntities = new List<Domain.Student.Entities.SectionRegistration>();
            foreach (var sectionReg in sectionRegistrations)
            {
                sectionRegistrationEntities.Add(new Domain.Student.Entities.SectionRegistration()
                {
                    Action = (Domain.Student.Entities.RegistrationAction)sectionReg.Action,
                    Credits = sectionReg.Credits,
                    SectionId = sectionReg.SectionId,
                    DropReasonCode = sectionReg.DropReasonCode,
                    IntentToWithdrawId = sectionReg.IntentToWithdrawId
                });
            }

            var request = new RegistrationRequest(studentId, sectionRegistrationEntities);
            var responseEntity = await _studentRepository.RegisterAsync(request);
            var responseDto = new Dtos.Student.RegistrationResponse();
            responseDto.Messages = new List<Dtos.Student.RegistrationMessage>();
            responseDto.PaymentControlId = responseEntity.PaymentControlId;

            foreach (var message in responseEntity.Messages)
            {
                responseDto.Messages.Add(new Dtos.Student.RegistrationMessage { Message = message.Message, SectionId = message.SectionId });
            }

            responseDto.RegisteredSectionIds = responseEntity.RegisteredSectionIds;

            return responseDto;
        }

        /// <summary>
        /// Process registration requests for a student
        /// </summary>
        /// <param name="studentId">Id of student</param>
        /// <param name="sectionDropRegistration">Section Drop Registration request to process</param>
        /// <returns>A Registration Response containing any messages returned by drop registration</returns>
        public async Task<Dtos.Student.RegistrationResponse> DropRegistrationAsync(string studentId, Dtos.Student.SectionDropRegistration sectionDropRegistration)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException("studentId", "You must supply a studentId");
            }

            if (sectionDropRegistration == null)
            {
                throw new ArgumentNullException("sectionDropRegistration", "You must supply a Section Drop Registration to be processed.");
            }

            if (string.IsNullOrWhiteSpace(sectionDropRegistration.SectionId))
            {
                throw new ArgumentNullException("sectionId", "You must supply a sectionId.");
            }

            // Prevent action without proper permissions 
            // Make sure user has permissions to drop the section from this degree plan. 
            // If not, an PermissionsException will be thrown.
            await CheckDropRegisterPermissionsAsync();

            // Get the specified section from the repository
            Domain.Student.Entities.Section section = await GetSectionAsync(sectionDropRegistration.SectionId);
            var userPermissions = await GetUserPermissionCodesAsync();
            var allDepartments = await _referenceDataRepository.DepartmentsAsync();

            // Ensure that the current user is a faculty of the given section or a departmental oversight member with the required permissions. 
            if (!(IsSectionFaculty(section) && userPermissions.Contains(StudentPermissionCodes.CanDropStudent))
                && !(CheckDepartmentalOversightAccessForSection(section, allDepartments) && userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionDropRoster)))
            {
                var message = "Current user is neither a faculty nor a departmental oversight of requested section " + sectionDropRegistration.SectionId + "with the required permissions and therefore cannot drop section";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // Determine if we need to check for Last Date Attended/Never Attended and, if so, get that data
            var facultyGradingConfig = await _studentConfigurationRepository.GetFacultyGradingConfiguration2Async();
            if (facultyGradingConfig.RequireLastDateAttendedOrNeverAttendedFlagBeforeFacultyDrop)
            {
                // Get academic credits for the section, including crosslisted sections if appropriate
                var querySectionIds = new List<string>() { section.Id };
                if (facultyGradingConfig.IncludeCrosslistedStudents)
                {
                    var crossListedSectionIds = section.CrossListedSections.Select(x => x.Id);
                    querySectionIds.AddRange(crossListedSectionIds);
                }
                var academicCreditsWithInvalidKeys = await _academicCreditRepository.GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(querySectionIds.Distinct().ToList());
                if (academicCreditsWithInvalidKeys != null && academicCreditsWithInvalidKeys.AcademicCredits != null)
                {
                    // Identify academic credits for the section for this student
                    var studentAcadCredsForSection = academicCreditsWithInvalidKeys.AcademicCredits.Where(ac => ac != null && ac.StudentId == studentId && querySectionIds.Contains(ac.SectionId)).ToList();
                    // If the student has any academic credits for this section/crosslisting with no Last Date Attended, and the student is not flagged as Never Attended, do not allow the drop
                    if (studentAcadCredsForSection.Any(sac => !sac.LastAttendanceDate.HasValue && sac.NeverAttended == false))
                    {
                        var message = "Student " + studentId + " does not have a Last Date Attended and was not flagged as Never Attended for course section " + sectionDropRegistration.SectionId + "; faculty may not drop a student from a course sections if the student is not flagged as Never Attended and does not have a Last Date Attended.";
                        logger.Error(message);
                        throw new PermissionsException(message);
                    }
                }
            }

            var sectionRegistrationEntities = new List<Domain.Student.Entities.SectionRegistration>();
            sectionRegistrationEntities.Add(new Domain.Student.Entities.SectionRegistration()
            {
                Action = Domain.Student.Entities.RegistrationAction.Drop,
                Credits = null,
                SectionId = sectionDropRegistration.SectionId,
                DropReasonCode = sectionDropRegistration.DropReasonCode
            });

            var request = new RegistrationRequest(studentId, sectionRegistrationEntities);
            var responseEntity = await _studentRepository.RegisterAsync(request);
            var responseDto = new Dtos.Student.RegistrationResponse();
            responseDto.Messages = new List<Dtos.Student.RegistrationMessage>();
            responseDto.PaymentControlId = responseEntity.PaymentControlId;

            foreach (var message in responseEntity.Messages)
            {
                responseDto.Messages.Add(new Dtos.Student.RegistrationMessage { Message = message.Message, SectionId = message.SectionId });
            }

            responseDto.RegisteredSectionIds = responseEntity.RegisteredSectionIds;
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
            Dtos.Student.Student student = null;

            try
            {
                // Get student entity 
                var studentEntity = await _studentRepository.GetAsync(id);
                // Throw error if student not found
                if (studentEntity == null)
                {
                    throw new KeyNotFoundException("Student not found in repository");
                }
                // Make sure user has access to this student--If not, method throws exception
                await CheckUserAccessAndViewPersonPermissionAsync(id, studentEntity.ConvertToStudentAccess());
                if (!UserIsSelf(id) && !HasProxyAccessForPerson(id))
                {
                    //if appropriate permissions exists then check if student have a privacy code and logged-in user have a staff record with same privacy code.
                    hasPrivacyRestriction = string.IsNullOrEmpty(studentEntity.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(studentEntity.PrivacyStatusCode);
                }
                if (hasPrivacyRestriction)
                {
                    student = new Dtos.Student.Student()
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
                    var studentDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>();
                    student = studentDtoAdapter.MapToType(studentEntity);
                }
                return new PrivacyWrapper<Dtos.Student.Student>(student, hasPrivacyRestriction);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Session has expired while retrieving student information for student {0}", id);
                logger.Error(tex, message);
                throw;
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while retrieving student information for student: " + id;
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Search Students either by student id or student name
        /// </summary>
        /// <param name="criteria">Id or name of student</param>
        /// <param name="pageSize">Number of records to retrieve</param>
        /// <param name="pageIndex">Current page number</param>
        /// <returns>List of students whose search creteria mathces</returns>
        public async Task<PrivacyWrapper<List<Dtos.Student.Student>>> Search3Async(StudentSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Student search criteria must be specified when searching for students.");
            }

            // Only users with VIEW.PERSON.INFORMATION and VIEW.STUDENT.INFORMATION can search for students by name or ID
            CheckGetPersonAndStudentInformationViewPermission();

            var watch = new Stopwatch();
            watch.Start();
            logger.Info("Begin Search3Async for students...");

            var hasPrivacyRestriction = false; // Default to false, will be set properly when retrieving students

            List<Domain.Student.Entities.Student> students = new List<Domain.Student.Entities.Student>();

            string searchString = criteria.StudentKeyword;

            List<Dtos.Student.Student> studentDtos = new List<Dtos.Student.Student>();
            var studentAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>();

            // Remove extra blank spaces
            var tempString = searchString.Trim();
            Regex regEx = new Regex(@"\s+");
            searchString = regEx.Replace(tempString, @" ");

            // If search string is a numeric ID and it is for a particular student, return only that
            double personId;
            bool isId = double.TryParse(searchString, out personId);
            if (isId && !string.IsNullOrEmpty(criteria.StudentKeyword))
            {
                // Validate the ID - if invalid, error will be thrown
                var matchingStudents = (await _studentRepository.GetStudentsSearchAsync(new List<string>() { searchString }));
                // If valid, return the ID. If not found, return null
                if (matchingStudents == null)
                {
                    logger.Debug("Call to StudentRepository.GetStudentsSearchAsync with query '" + searchString + "' returned null.");
                    return new PrivacyWrapper<List<Dtos.Student.Student>>(new List<Dtos.Student.Student>(), hasPrivacyRestriction);
                }
                else
                {
                    students = matchingStudents.ToList();
                    // loop through each student id
                    foreach (var stud in students)
                    {
                        try
                        {
                            // Before doing anything, check the current advisor's privacy code settings (on their staff record)
                            // against any privacy code on the student's record
                            var studentHasPrivacyRestriction = string.IsNullOrEmpty(stud.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(stud.PrivacyStatusCode);

                            Dtos.Student.Student studentDtoe;

                            // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                            // then blank out the record, except for name, id, and privacy code
                            if (studentHasPrivacyRestriction)
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
                        catch (ColleagueSessionExpiredException ce)
                        {
                            string message = string.Format("Colleague session expired while building Student DTO for student {0}", stud.Id);
                            logger.Error(ce, message);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, string.Format("Failed to build Student DTO for student {0}", stud.Id));
                            logger.Error(ex, ex.Message);
                        }
                    }

                    return new PrivacyWrapper<List<Dtos.Student.Student>>(studentDtos, hasPrivacyRestriction);

                }
            }

            // Otherwise, we are doing a name search of students - parse the search string into name parts
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
                var matchingStudents = await _studentRepository.GetStudentSearchByNameAsync(lastName, firstName, middleName, pageSize, pageIndex);
                if (matchingStudents == null)
                {
                    logger.Debug("Call to StudentRepository.GetStudentSearchByNameAsync with lastName'" + lastName + "', firstName '" + firstName + "', middleName '" + middleName + " returned null.");
                }
                else
                {
                    students = matchingStudents.ToList();
                }
            }

            // loop through each student
            foreach (var student in students)
            {
                try
                {
                    // Before doing anything, check the current advisor's privacy code settings (on their staff record)
                    // against any privacy code on the student's record
                    var studentHasPrivacyRestriction = string.IsNullOrEmpty(student.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(student.PrivacyStatusCode);

                    Dtos.Student.Student studentDtoe;

                    // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                    // then blank out the record, except for name, id, and privacy code
                    if (studentHasPrivacyRestriction)
                    {
                        hasPrivacyRestriction = true;
                        studentDtoe = new Dtos.Student.Student()
                        {
                            LastName = student.LastName,
                            FirstName = student.FirstName,
                            MiddleName = student.MiddleName,
                            Id = student.Id,
                            PrivacyStatusCode = student.PrivacyStatusCode
                        };
                    }
                    else
                    {
                        studentDtoe = studentAdapter.MapToType(student);
                        studentDtoe.DegreePlanId = null;
                        if (student.PreferredEmailAddress != null)
                        {
                            studentDtoe.PreferredEmailAddress = student.PreferredEmailAddress.Value;
                        }
                    }
                    studentDtos.Add(studentDtoe);
                }
                catch (ColleagueSessionExpiredException ce)
                {
                    string message = string.Format("Colleague session expired while building DTO for student {0}", student.Id);
                    logger.Error(ce, message);
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, string.Format("Failed to build Student DTO for student {0}", student.Id));
                }
            }
            logger.Info(string.Format("Search3Async for students completed in {0}", watch.ElapsedMilliseconds.ToString()));

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

        /// <summary>
        /// Attempts to get a section from the repository.
        /// </summary>
        /// <param name="sectionId">Id of section to retrieve</param>
        /// <returns><see cref="Section">Section</see> domain object</returns>
        private async Task<Domain.Student.Entities.Section> GetSectionAsync(string sectionId)
        {
            Domain.Student.Entities.Section section;

            try
            {
                section = await _sectionRepository.GetSectionAsync(sectionId);
                if (section == null)
                {
                    var message = "Repository returned a null section for Id " + sectionId;
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                }
                return section;
            }
            catch (ArgumentNullException aex)
            {
                var message = "Section ID must be specified.";
                logger.Error(message);
                throw aex;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "sectionId " + sectionId + " not found in repository. Exception message: " + kex.Message;
                logger.Error(message);
                throw kex;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read section from repository using id " + sectionId + "Exception message: " + ex.ToString();
                logger.Error(message);
                throw new ColleagueWebApiException(ex.Message);
            }
        }

        /// <summary>
        /// Returns boolean indicating if the current user is a faculty for the given section.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        private bool IsSectionFaculty(Domain.Student.Entities.Section section)
        {
            if (section == null || section.FacultyIds == null || section.FacultyIds.Count() == 0)
            {
                return false;
            }
            return section.FacultyIds.Where(f => f == CurrentUser.PersonId).FirstOrDefault() == null ? false : true;
        }

        #region Student Cohort
        /// <summary>
        /// Gets all student cohorts
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.StudentCohort>> GetAllStudentCohortsAsync(Dtos.Filters.CodeItemFilter criteria = null, bool bypassCache = false)
        {
            IEnumerable<Domain.Student.Entities.StudentCohort> entities = await _studentReferenceDataRepository.GetAllStudentCohortAsync(bypassCache);
            IEnumerable<Domain.Student.Entities.StudentCohort> filteredList = null;

            List<Dtos.StudentCohort> studentCohortsDtos = new List<Dtos.StudentCohort>();

            if (criteria != null && !string.IsNullOrEmpty(criteria.Code))
            {
                filteredList = entities.Where(sc => !string.IsNullOrWhiteSpace(sc.Code) && sc.Code.Equals(criteria.Code, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (filteredList == null || !filteredList.Any())
                {
                    return new List<Dtos.StudentCohort>();
                }
                entities = filteredList;
            }

            foreach (var studentCohortEntity in entities)
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
        public async Task<Dtos.StudentCohort> GetStudentCohortByGuidAsync(string id, bool bypassCache = false)
        {
            Domain.Student.Entities.StudentCohort studentCohortEntity = (await _studentReferenceDataRepository.GetAllStudentCohortAsync(bypassCache))
                                                                        .FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (studentCohortEntity == null)
            {
                throw new KeyNotFoundException("student-cohorts not found for GUID : " + id);
            }
            return BuildStudentCohort(studentCohortEntity);
        }

        /// <summary>
        /// Builds student cohort dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.StudentCohort BuildStudentCohort(StudentCohort source)
        {
            Dtos.StudentCohort dto = new Dtos.StudentCohort();
            dto.Id = source.Guid;
            dto.Code = source.Code;
            dto.Description = source.Description;
            dto.Title = source.Description;
            dto.StudentCohortType = ConvertEntityToCohortTypeDto(source.CohortType);

            return dto;
        }

        /// <summary>
        /// Converts entity cohort types to dto cohort type.
        /// </summary>
        /// <param name="cohortType"></param>
        /// <returns></returns>
        private CohortType ConvertEntityToCohortTypeDto(string cohortType)
        {
            switch (cohortType)
            {
                case "FED":
                    return CohortType.Federal;
                default:
                    return CohortType.NotSet;
            }
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
        public async Task<IEnumerable<Dtos.ResidentType>> GetResidentTypesAsync(bool bypassCache = false)
        {
            var residentTypeCollection = new List<Dtos.ResidentType>();

            var residentTypeEntities = await _studentReferenceDataRepository.GetAdmissionResidencyTypesAsync(bypassCache);
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
        public async Task<Dtos.ResidentType> GetResidentTypeByIdAsync(string id)
        {
            try
            {
                return ConvertResidentTypeEntityToDto((await _studentReferenceDataRepository.GetAdmissionResidencyTypesAsync(true)).Where(st => st.Guid == id).First());
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
        private Dtos.ResidentType ConvertResidentTypeEntityToDto(Domain.Student.Entities.AdmissionResidencyType source)
        {
            var residentType = new Dtos.ResidentType();

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
        /// <returns>A Student DTO <see cref="Dtos.Students">object</see></returns>
        public async Task<Dtos.Students> GetStudentsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Student.");
            }

            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                logger.Info("StudentService Timing: (GetStudentsByGuidAsync) GetStudentFromGuidAsync " +
                            "repo lookup started");
                watch.Restart();
            }

            Domain.Student.Entities.Student studentEntity = null;

            try
            {
                studentEntity = await _studentRepository.GetDataModelStudentFromGuidAsync(guid);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Concat("Student not found for GUID '", guid, "'"));
            }
            catch (RepositoryException)
            {
                throw new KeyNotFoundException(string.Concat("Student not found for GUID '", guid, "'"));
            }

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
            var retval = (await ConvertStudentsEntityToStudentsDto(studentEntity, personGuidCollection, bypassCache));

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("StudentService Timing: (GetStudentsByGuidAsync) service conversion in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            return retval;
        }

        /// <summary>
        /// Get an Student from its GUID
        /// </summary>
        /// <returns>A Student DTO <see cref="Dtos.Students">object</see></returns>
        public async Task<Dtos.Students2> GetStudentsByGuid2Async(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Student.");
            }

            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                logger.Info("StudentService Timing: (GetStudentsByGuidAsync) GetStudentFromGuidAsync " +
                            "repo lookup started");
                watch.Restart();
            }
            Domain.Student.Entities.Student studentEntity = null;
            try
            {
                studentEntity = await _studentRepository.GetDataModelStudentFromGuid2Async(guid);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Concat("Student not found for GUID '", guid, "'"));
            }
            catch (RepositoryException)
            {
                throw new KeyNotFoundException(string.Concat("Student not found for GUID '", guid, "'"));
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
            var retval = (await ConvertStudentsEntityToStudents2Dto(studentEntity, personGuidCollection, bypassCache));

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

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
        /// <returns>A Student DTO <see cref="Dtos.Students">object</see></returns>     
        public async Task<Tuple<IEnumerable<Dtos.Students>, int>> GetStudentsAsync(int offset,
             int limit, bool bypassCache = false, string person = "", string type = "", string cohorts = "", string residency = "")
        {
            try
            {
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
                }

                var newCohort = string.Empty;
                if (!string.IsNullOrEmpty(cohorts))
                {
                    try
                    {
                        var allStudentCohorts = (await GetAllStudentCohortsAsync(null, bypassCache)).ToList();
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
        /// Get a list of Students
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaFilter">critera filter for Students2</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <param name="bypassCache"></param>
        /// <returns>A Student DTO <see cref="Dtos.Students">object</see></returns>     
        public async Task<Tuple<IEnumerable<Dtos.Students2>, int>> GetStudents2Async(int offset,
             int limit, Dtos.Students2 criteriaFilter, string personFilter, bool bypassCache = false)
        {
            string person = string.Empty, newPerson = string.Empty;
            List<string> types = new List<string>(), residencies = new List<string>(),
                newTypes = new List<string>(), newResidencies = new List<string>();
            string[] filterPersonIds = new List<string>().ToArray();

            if (!string.IsNullOrEmpty(personFilter))
            {
                var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                if (personFilterKeys != null)
                {
                    filterPersonIds = personFilterKeys;
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.Students2>, int>(new List<Dtos.Students2>(), 0);
                }
            }

            if (criteriaFilter != null)
            {
                // person criteria filter
                person = criteriaFilter.Person != null ? criteriaFilter.Person.Id : string.Empty;
                if (!string.IsNullOrEmpty(person))
                {
                    try
                    {
                        newPerson = await _personRepository.GetPersonIdFromGuidAsync(person);
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.Students2>, int>(new List<Dtos.Students2>(), 0);
                    }
                }

                // types criteria filter
                if ((criteriaFilter.Types != null) && (criteriaFilter.Types.Any()))
                {
                    var studentTypes = (await GetStudentTypesAsync(bypassCache)).ToList();
                    if (studentTypes.Any())
                    {
                        // extract GUID object 2 from DTO property
                        var typeObjects = new List<GuidObject2>();
                        foreach (var typeObject in criteriaFilter.Types)
                        {
                            typeObjects.Add(typeObject.Type);
                        }
                        // convert from GUID object2 to string list of GUIDs.
                        types = ConvertGuidObject2ListToStringList(typeObjects);
                        foreach (var type in types)
                        {
                            // translate list of GUIDs to list of codes                               
                            var studentType = studentTypes.FirstOrDefault(st => st.Guid == type);
                            if (studentType == null)
                            {
                                return new Tuple<IEnumerable<Dtos.Students2>, int>(new List<Dtos.Students2>(), 0);
                            }
                            else
                            {
                                var newType = studentType != null ? studentType.Code : string.Empty;
                                newTypes.Add(newType);
                            }
                        }
                    }
                }

                //residencies criteria filter
                if ((criteriaFilter.Residencies != null) && (criteriaFilter.Residencies.Any()))
                {
                    var residencyTypes = (await GetResidentTypesAsync(bypassCache)).ToList();
                    if (residencyTypes.Any())
                    {
                        // extract GUID object 2 from DTO property
                        var residencyObjects = new List<GuidObject2>();
                        foreach (var residencyObject in criteriaFilter.Residencies)
                        {
                            residencyObjects.Add(residencyObject.Residency);
                        }
                        // convert from GUID object2 to string list of GUIDs.
                        residencies = ConvertGuidObject2ListToStringList(residencyObjects);
                        foreach (var residency in residencies)
                        {
                            // translate list of GUIDs to list of codes
                            var residencyType = residencyTypes.FirstOrDefault(st => st.Id == residency);

                            if (residencyType == null)
                            {
                                return new Tuple<IEnumerable<Dtos.Students2>, int>(new List<Dtos.Students2>(), 0);
                            }
                            else
                            {
                                var newResidency = residencyType != null ? residencyType.Code : string.Empty;
                                newResidencies.Add(newResidency);
                            }
                        }
                    }
                }
            }

            var studentsEntitiesTuple = await _studentRepository.GetDataModelStudents2Async(offset, limit, bypassCache, filterPersonIds, newPerson, newTypes, newResidencies);

            if (studentsEntitiesTuple != null)
            {
                var studentsEntities = studentsEntitiesTuple.Item1.ToList();
                var totalCount = studentsEntitiesTuple.Item2;

                if (studentsEntities.Any())
                {
                    var students = new List<Colleague.Dtos.Students2>();

                    var ids = studentsEntities.Where(x => (!string.IsNullOrEmpty(x.Id))).Select(x => x.Id).Distinct().ToList();
                    //
                    var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);


                    foreach (var student in studentsEntities)
                    {
                        students.Add(await ConvertStudentsEntityToStudents2Dto(student, personGuidCollection, bypassCache));
                    }
                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }
                    return new Tuple<IEnumerable<Dtos.Students2>, int>(students, totalCount);

                }
                // no results
                return new Tuple<IEnumerable<Dtos.Students2>, int>(new List<Dtos.Students2>(), totalCount);
            }
            //no results
            return new Tuple<IEnumerable<Dtos.Students2>, int>(new List<Dtos.Students2>(), 0);
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
                    var allStudentCohorts = (await GetAllStudentCohortsAsync(null, bypassCache)).ToList();
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
                throw new ColleagueWebApiException("Student exception occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Converts a Student domain entity to its corresponding Student  DTO
        /// </summary>
        /// <param name="student">A list of <see cref="Dtos.Student">Student</see> domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>A <see cref="Dtos.Students">Students</see> DTO</returns>
        private async Task<Students2> ConvertStudentsEntityToStudents2Dto(Domain.Student.Entities.Student student,
            Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            var studentDto = new Colleague.Dtos.Students2();

            if (student == null)
            {
                IntegrationApiExceptionAddError(string.Concat("Student is required"));
                return studentDto;
            }
            if (string.IsNullOrEmpty(student.StudentGuid))
            {
                IntegrationApiExceptionAddError(string.Concat("Guid for student is required"), id: student.Id);
                return studentDto;
            }
            if (string.IsNullOrEmpty(student.Id))
            {
                IntegrationApiExceptionAddError(string.Concat("Id for student is required"), guid: student.StudentGuid);
                return studentDto;
            }

            studentDto.Id = student.StudentGuid;

            if (personGuidCollection != null && personGuidCollection.Any())
            {
                var studentGuid = string.Empty;
                personGuidCollection.TryGetValue(student.Id, out studentGuid);
                if (!string.IsNullOrEmpty(studentGuid))
                {
                    studentDto.Person = new Dtos.GuidObject2(studentGuid);
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("Student GUID not found in Person collection"), guid: student.StudentGuid, id: student.Id);
                    return studentDto;
                }
            }


            if ((student.StudentTypeInfo != null) && (student.StudentTypeInfo.Any()))
            {
                var studentTypesCollection = new List<StudentTypesDtoProperty>();
                var studentTypes = student.StudentTypeInfo.OrderByDescending(st => st.TypeDate < DateTime.Now);
                if (studentTypes != null)
                {

                    var allTypes = await GetStudentTypesAsync(bypassCache);
                    if (allTypes == null || !allTypes.Any())
                    {
                        // make sure this is captured/reported
                        throw new InvalidOperationException("Student Types are not defined.");
                    }
                    if ((allTypes != null) && allTypes.Any())
                    {
                        bool validTypes = true;
                        foreach (var stuType in studentTypes)
                        {
                            var typeObject = allTypes.FirstOrDefault(st => st.Code == stuType.Type);
                            if (typeObject == null || string.IsNullOrEmpty(typeObject.Guid))
                            {
                                IntegrationApiExceptionAddError(string.Concat("No GUID found for student type: ", stuType.Type), guid: student.StudentGuid, id: student.Id);
                                validTypes = false;
                            }
                            else
                            {
                                var studentTypeObject = new StudentTypesDtoProperty();
                                studentTypeObject.Type = new GuidObject2(typeObject.Guid);
                                studentTypeObject.StartOn = stuType.TypeDate;
                                studentTypesCollection.Add(studentTypeObject);
                            }
                        }
                        if (validTypes == true)
                        {
                            studentDto.Types = studentTypesCollection;
                        }
                    }
                }
            }

            if ((student.StudentResidencies != null) && (student.StudentResidencies.Any()))
            {
                var studentResidenciesCollection = new List<StudentResidenciesDtoProperty>();
                if (student.StudentResidencies != null)
                {
                    var allResidencies = await GetResidentTypesAsync(bypassCache);
                    if (allResidencies == null || !allResidencies.Any())
                    {
                        throw new InvalidOperationException("Student residencies are not defined.");
                    }
                    if ((allResidencies != null) && (allResidencies.Any()))
                    {
                        bool validResidencies = true;
                        foreach (var stuResidency in student.StudentResidencies)
                        {
                            var residencyObject = allResidencies.FirstOrDefault(st => st.Code == stuResidency.Residency);
                            if (residencyObject == null || string.IsNullOrEmpty(residencyObject.Id))
                            {
                                IntegrationApiExceptionAddError(string.Concat("No GUID found for student residency ", stuResidency.Residency), guid: student.StudentGuid, id: student.Id);
                                validResidencies = false;
                            }
                            else
                            {
                                var studentResidencyObject = new StudentResidenciesDtoProperty();
                                studentResidencyObject.Residency = new GuidObject2(residencyObject.Id);
                                studentResidencyObject.StartOn = stuResidency.Date;
                                studentResidenciesCollection.Add(studentResidencyObject);
                            }
                        }
                        if (validResidencies == true)
                        {
                            studentDto.Residencies = studentResidenciesCollection;
                        }
                    }
                }
            }


            if (student.StudentAcademicLevels != null && student.StudentAcademicLevels.Any())
            {
                var levelClassificationsCollection = new List<StudentLevelClassificationsDtoProperty>();
                var allLevels = (await GetAcademicLevelsAsync(bypassCache)).ToList();
                if (allLevels == null || !allLevels.Any())
                {
                    throw new InvalidOperationException("Academic levels are not defined.");
                }
                var allClassifications = (await GetAllStudentClassificationAsync(bypassCache)).ToList();
                if ((allClassifications != null) && (allClassifications.Any()) && (allLevels != null) && (allLevels.Any()))
                {
                    foreach (var academicLevel in student.StudentAcademicLevels)
                    {
                        if (!string.IsNullOrEmpty(academicLevel.ClassLevel))
                        {
                            var leaveClassification = new StudentLevelClassificationsDtoProperty();
                            bool validLeaveClassification = true; var levelObject = allLevels.FirstOrDefault(al => al.Code == academicLevel.AcademicLevel);
                            if (levelObject == null || string.IsNullOrEmpty(levelObject.Guid))
                            {
                                IntegrationApiExceptionAddError(string.Concat("No GUID found academic level ", academicLevel.AcademicLevel), guid: student.StudentGuid, id: student.Id);
                                validLeaveClassification = false;
                            }
                            else
                            {
                                leaveClassification.Level = new GuidObject2(levelObject.Guid);
                            }
                            var classificationObject = allClassifications.FirstOrDefault(cl => cl.Code == academicLevel.ClassLevel);
                            if (classificationObject == null || string.IsNullOrEmpty(classificationObject.Guid))
                            {
                                IntegrationApiExceptionAddError(string.Concat("No GUID found for classification ", academicLevel.ClassLevel), guid: student.StudentGuid, id: student.Id);
                                validLeaveClassification = false;
                            }
                            else
                            {
                                leaveClassification.LatestClassification = new GuidObject2(classificationObject.Guid);
                            }
                            if (validLeaveClassification == true)
                            {
                                levelClassificationsCollection.Add(leaveClassification);
                                studentDto.LevelClassifications = levelClassificationsCollection;
                            }
                        }
                    }
                }
            }
            return studentDto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view persons.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetPersonAndStudentInformationViewPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewPersonInformation) && HasPermission(StudentPermissionCodes.ViewStudentInformation);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view person information.");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view persons.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetPersonViewPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewPersonInformation);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view person information.");
            }
        }

        /// <summary>
        /// Convert list of GUID object Ids to list of strings
        /// </summary>
        /// <param name="guidObjectList">Guid Object of Ids</param>
        /// <returns>List of strings</returns>

        public List<string> ConvertGuidObject2ListToStringList(List<GuidObject2> guidObjectList)
        {
            var retval = new List<string>();
            if (guidObjectList != null & guidObjectList.Any())
            {
                foreach (var guidObject in guidObjectList)
                {
                    if (!string.IsNullOrEmpty(guidObject.Id))
                    {
                        retval.Add(guidObject.Id);
                    }

                }

            }
            return retval;
        }
        #endregion

        #region Planning Student

        /// <summary>
        /// Get the PlanningStudent information for the given student ID. Throws exception if user is not authorized to
        /// access this student's information.
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public async Task<PrivacyWrapper<Dtos.Student.PlanningStudent>> GetPlanningStudentAsync(string studentId)
        {
            var hasPrivacyRestriction = false;
            Dtos.Student.PlanningStudent planningStudent = null;
            var planningStudentEntity = await _planningStudentRepository.GetAsync(studentId);

            // Determine if this user has the rights to access this student 


            // If permissions not found, this will throw a permissions exception. 
            await CheckUserAccessAndViewPersonPermissionAsync(studentId, planningStudentEntity.ConvertToStudentAccess());
            if (!UserIsSelf(studentId) && !HasProxyAccessForPerson(studentId))
            {
                //if appropriate permissions exists then check if student have privacy code and logged-in user have a staff record with same privacy code.
                hasPrivacyRestriction = string.IsNullOrEmpty(planningStudentEntity.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(planningStudentEntity.PrivacyStatusCode);
            }
            if (hasPrivacyRestriction)
            {
                planningStudent = new Dtos.Student.PlanningStudent()
                {
                    LastName = planningStudentEntity.LastName,
                    FirstName = planningStudentEntity.FirstName,
                    MiddleName = planningStudentEntity.MiddleName,
                    Id = planningStudentEntity.Id,
                    PrivacyStatusCode = planningStudentEntity.PrivacyStatusCode
                };
            }
            else
            {
                var adapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Student.PlanningStudent>();
                planningStudent = adapter.MapToType(planningStudentEntity);
            }
            return new PrivacyWrapper<Dtos.Student.PlanningStudent>(planningStudent, hasPrivacyRestriction);
        }

        #endregion Planning Student
        /// <summary>
        /// Get Student Academic Levels
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>List of Student Academic Level</returns>
        public async Task<IEnumerable<Dtos.Student.StudentAcademicLevel>> GetStudentAcademicLevelsAsync(string studentId)
        {
            List<Dtos.Student.StudentAcademicLevel> studentAcademicLevelsDto = new List<Dtos.Student.StudentAcademicLevel>();
            if (string.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id must be provided in order to retrieve student's academic levels");
            }

            if (!UserIsSelf(studentId) && !(await UserIsAdvisorAsync(studentId)) && !(HasPermission(StudentPermissionCodes.ViewStudentInformation)))
            {
                var message = "Current user is not the student to request academic levels or current user is advisor or faculty but doesn't have appropriate permissions and therefore cannot access it.";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentAcademicLevel> studentAcademicLevelsEntity = await _studentRepository.GetStudentAcademicLevelsAsync(studentId);
            if (studentAcademicLevelsEntity != null && studentAcademicLevelsEntity.Any())
            {
                var studentAcademicLevelEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentAcademicLevel, Dtos.Student.StudentAcademicLevel>();
                foreach (var studentAcadLevelEntity in studentAcademicLevelsEntity)
                {
                    studentAcademicLevelsDto.Add(studentAcademicLevelEntityToDtoAdapter.MapToType(studentAcadLevelEntity));
                }
            }
            else
            {
                logger.Warn("Repository call to retrieve student's academic levels returns null or empty entity");
            }
            return studentAcademicLevelsDto;
        }
    }
}
