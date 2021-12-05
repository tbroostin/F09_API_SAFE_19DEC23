//Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentUnverifiedGradesService : BaseCoordinationService, IStudentUnverifiedGradesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentUnverifiedGradesRepository _studentUnverifiedGradesRepository;
        private readonly ISectionRegistrationRepository _sectionRegistrationRepository;
        private IEnumerable<Domain.Student.Entities.Grade> _grades = null;
        private IEnumerable<Domain.Student.Entities.SectionGradeType> _gradeTypes = null;
        private IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;

        public StudentUnverifiedGradesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IPersonRepository personRepository,
            IGradeRepository gradeRepository,
            ISectionRepository sectionRepository,
            IStudentUnverifiedGradesRepository studentUnverifiedGradesRepository,
            ISectionRegistrationRepository sectionRegistrationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _gradeRepository = gradeRepository;
            _sectionRepository = sectionRepository;
            _studentUnverifiedGradesRepository = studentUnverifiedGradesRepository;
            _sectionRegistrationRepository = sectionRegistrationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-unverified-grades
        /// </summary>
        /// <returns>Collection of StudentUnverifiedGrades DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentUnverifiedGrades>, int>> GetStudentUnverifiedGradesAsync(int offset, int limit,
            string student = "", string sectionRegistration = "", string section = "", bool bypassCache = false)
        {
          
            string newStudent = string.Empty;
            if (!string.IsNullOrEmpty(student))
            {
                try
                {
                    newStudent = await _personRepository.GetPersonIdFromGuidAsync(student);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(new List<Dtos.StudentUnverifiedGrades>(), 0);
                }
                if (string.IsNullOrEmpty(newStudent))
                    return new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(new List<Dtos.StudentUnverifiedGrades>(), 0);
            }

            string newSectionRegistration = string.Empty;
            if (!string.IsNullOrEmpty(sectionRegistration))
            {
                try
                {
                    newSectionRegistration = (sectionRegistration == string.Empty ? string.Empty : await _sectionRegistrationRepository.GetSectionRegistrationIdFromGuidAsync(sectionRegistration));
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(new List<Dtos.StudentUnverifiedGrades>(), 0);
                }
                if (!string.IsNullOrEmpty(sectionRegistration) && string.IsNullOrEmpty(newSectionRegistration))
                {
                    return new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(new List<Dtos.StudentUnverifiedGrades>(), 0);
                }
            }

            string newSection = string.Empty;
            if (!string.IsNullOrEmpty(section))
            {
                try
                {
                    newSection = (section == string.Empty ? string.Empty : await _sectionRepository.GetSectionIdFromGuidAsync(section));
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(new List<Dtos.StudentUnverifiedGrades>(), 0);
                }
                if (!string.IsNullOrEmpty(section) && string.IsNullOrEmpty(newSection))
                {
                    return new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(new List<Dtos.StudentUnverifiedGrades>(), 0);
                }
            }

            var studentUnverifiedGradesDtos = new List<Dtos.StudentUnverifiedGrades>();
            Tuple<IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades>, int> studentUnverifiedGradesEntities = null;
            try
            {
                studentUnverifiedGradesEntities = await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradesAsync(offset, limit, bypassCache, newStudent, newSectionRegistration, newSection);

                if (studentUnverifiedGradesEntities != null && studentUnverifiedGradesEntities.Item1 != null && studentUnverifiedGradesEntities.Item1.Any())
                {
                    studentUnverifiedGradesDtos = (await ConvertStudentUnverifiedGradesEntityToDto(studentUnverifiedGradesEntities.Item1, bypassCache)).ToList();
                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (IntegrationApiException)
            {
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(studentUnverifiedGradesDtos, studentUnverifiedGradesEntities.Item2);            
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentUnverifiedGrades from its GUID
        /// </summary>
        /// <returns>StudentUnverifiedGrades DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentUnverifiedGrades> GetStudentUnverifiedGradesByGuidAsync(string guid, bool bypassCache = true)
        {
            Domain.Student.Entities.StudentUnverifiedGrades studentUnverifiedGradesEntity = null;

            try
            {
                studentUnverifiedGradesEntity = await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradeByGuidAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", guid);
                throw IntegrationApiException;
            }

            //HED-23209/24644
            var allGrades = string.Concat(studentUnverifiedGradesEntity.FinalGrade, studentUnverifiedGradesEntity.MidtermGrade1, studentUnverifiedGradesEntity.MidtermGrade2,
                studentUnverifiedGradesEntity.MidtermGrade3, studentUnverifiedGradesEntity.MidtermGrade4, studentUnverifiedGradesEntity.MidtermGrade5,
                studentUnverifiedGradesEntity.MidtermGrade6);
            if ((string.IsNullOrEmpty(allGrades) && !studentUnverifiedGradesEntity.HasNeverAttended && !studentUnverifiedGradesEntity.LastAttendDate.HasValue))
            {             
                IntegrationApiExceptionAddError("Record has no midterm grades, final grade, last attendance date, or never attended flag", "Global.Internal.Error",
                    guid, id: (studentUnverifiedGradesEntity != null) ? "" : studentUnverifiedGradesEntity.StudentCourseSecId);
                throw IntegrationApiException;
            }

            IEnumerable<StudentUnverifiedGrades> studentUnverifiedGrades = null;
            try
            {
                studentUnverifiedGrades = (await ConvertStudentUnverifiedGradesEntityToDto(new List<Domain.Student.Entities.StudentUnverifiedGrades>()
                {studentUnverifiedGradesEntity }, bypassCache));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return studentUnverifiedGrades != null ? studentUnverifiedGrades.FirstOrDefault() : null;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentUnverifiedGradesSubmissions from its GUID
        /// </summary>
        /// <returns>StudentUnverifiedGradesSubmissions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentUnverifiedGradesSubmissions> GetStudentUnverifiedGradesSubmissionsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return await this.ConvertStudentUnverifiedGradesEntityToDto(await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradeByGuidAsync(guid), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-unverified-grades-submissions not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-unverified-grades-submissions not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Update a StudentUnverifiedGradesSubmissions.
        /// </summary>
        /// <param name="StudentUnverifiedGradesSubmissions">The <see cref="StudentUnverifiedGradesSubmissions">studentUnverifiedGradesSubmissions</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentUnverifiedGrades">studentUnverifiedGrades</see></returns>
        public async Task<Dtos.StudentUnverifiedGrades> UpdateStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions)
        {
            ValidateStudentUnverifiedGradesSubmissions(studentUnverifiedGradesSubmissions);

            _studentUnverifiedGradesRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // get the ID associated with the incoming guid
            string studentUnverifiedGradesSubmissionsEntityId = string.Empty;
            try
            {
                studentUnverifiedGradesSubmissionsEntityId = await _studentUnverifiedGradesRepository.GetStudentUnverifiedGradesIdFromGuidAsync(studentUnverifiedGradesSubmissions.Id);
            }
            catch (Exception)
            { // if the guid is not found then attempt to create
            }

            if (!string.IsNullOrEmpty(studentUnverifiedGradesSubmissionsEntityId))
            {

                try
                {
                    // map the DTO to entities
                    var studentUnverifiedGradesSubmissionsEntity
                    = await ConvertStudentUnverifiedGradesSubmissionsDtoToEntityAsync(studentUnverifiedGradesSubmissionsEntityId, studentUnverifiedGradesSubmissions);

                    // update the entity in the database
                    var updatedStudentUnverifiedGradesSubmissionsEntity =
                        await _studentUnverifiedGradesRepository.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesSubmissionsEntity);

                    var studentUnverifiedGrades = await this.ConvertStudentUnverifiedGradesEntityToDto(new List<Domain.Student.Entities.StudentUnverifiedGrades>()
                            {updatedStudentUnverifiedGradesSubmissionsEntity }, true);

                    return studentUnverifiedGrades != null ? studentUnverifiedGrades.FirstOrDefault() : null;
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                catch (IntegrationApiException)
                {
                    throw IntegrationApiException;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Create.Update.Exception", studentUnverifiedGradesSubmissions.Id, studentUnverifiedGradesSubmissionsEntityId);
                    throw IntegrationApiException;
                }
            }
            // perform a create instead
            return await CreateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesSubmissions);
        }

        /// <summary>
        /// Create a StudentUnverifiedGradesSubmissions.
        /// </summary>
        /// <param name="studentUnverifiedGradesSubmissions">The <see cref="StudentUnverifiedGradesSubmissions">studentUnverifiedGradesSubmissions</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="StudentUnverifiedGrades">studentUnverifiedGrades</see></returns>
        public async Task<Dtos.StudentUnverifiedGrades> CreateStudentUnverifiedGradesSubmissionsAsync(Dtos.StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions)
        {
            //Validate the request 
            ValidateStudentUnverifiedGradesSubmissions(studentUnverifiedGradesSubmissions);

            _studentUnverifiedGradesRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                
                var studentUnverifiedGradesSubmissionsEntity
                         = await ConvertStudentUnverifiedGradesSubmissionsDtoToEntityAsync(studentUnverifiedGradesSubmissions.Id, studentUnverifiedGradesSubmissions);

                // create a studentUnverifiedGrades entity in the database
                var createdStudentUnverifiedGradesSubmissions =
                    await _studentUnverifiedGradesRepository.CreateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesSubmissionsEntity);
                // return the newly created studentUnverifiedGrades
                var studentUnverifiedGrades = await this.ConvertStudentUnverifiedGradesEntityToDto(new List<Domain.Student.Entities.StudentUnverifiedGrades>()
                           {createdStudentUnverifiedGradesSubmissions }, true);

                return studentUnverifiedGrades != null ? studentUnverifiedGrades.FirstOrDefault() : null;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (IntegrationApiException)
            {
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Create.Update.Exception", studentUnverifiedGradesSubmissions.Id);
                throw IntegrationApiException;
            }
        }


        /// <summary>
        /// Convert a StudentUnverifiedGradesSubmissions Dto to an StudentUnverifiedGrades domain entity
        /// </summary>
        /// <param name="studentUnverifiedGradesSubmissionsEntityId"></param>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.StudentUnverifiedGrades> ConvertStudentUnverifiedGradesSubmissionsDtoToEntityAsync(string studentUnverifiedGradesSubmissionsEntityId, StudentUnverifiedGradesSubmissions source, bool bypassCache = false)
        {
            if (source == null)
            {
                return null;
            }

            var studentId = string.Empty;
            var studentAcadCredId = string.Empty;
            var studentCourseSecId = string.Empty;
            var sectionGradeScheme = string.Empty;

            if ((source.SectionRegistration != null) && (!string.IsNullOrEmpty(source.SectionRegistration.Id)))
            {
                studentAcadCredId = await _studentUnverifiedGradesRepository.GetStudentAcademicCredIdFromGuidAsync(source.SectionRegistration.Id);
                if (string.IsNullOrEmpty(studentAcadCredId))
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to obtain id for SectionRegistration:  ", source.SectionRegistration.Id), "Validation.Exception", source.Id);
                    throw IntegrationApiException;
                }
                var studentAcadCredData = await _studentUnverifiedGradesRepository.GetStudentAcadCredDataFromIdAsync(studentAcadCredId);
                studentId = studentAcadCredData.Item1;
                studentCourseSecId = studentAcadCredData.Item2;
                sectionGradeScheme = studentAcadCredData.Item3;
            }
            var studentUnverifiedGradesSubmission = new Domain.Student.Entities.StudentUnverifiedGrades(source.Id, studentCourseSecId)
            {
                StudentAcadaCredId = studentAcadCredId,
                StudentId = studentId,
                GradeScheme = sectionGradeScheme
            };

            if (source.LastAttendance != null)
            {
                //Both lastAttendance date and status included in payload
                if ((source.LastAttendance.Status == StudentUnverifiedGradesStatus.Neverattended)
                    && (source.LastAttendance.Date != null))
                {
                    IntegrationApiExceptionAddError("Never attended status and last attendance date may not both be set.", "Validation.Exception", source.Id, studentCourseSecId);
                }

                if (source.LastAttendance.Status == StudentUnverifiedGradesStatus.Neverattended)
                    studentUnverifiedGradesSubmission.HasNeverAttended = true;
                if (source.LastAttendance.Date != null)
                    studentUnverifiedGradesSubmission.LastAttendDate = source.LastAttendance.Date;
            }

            if (source.Grade == null)
            {
                return studentUnverifiedGradesSubmission;
            }

            var grade = source.Grade;
            var gradeId = string.Empty;
            var incompleteGrade = string.Empty;

            if ((source.Grade.Grade != null) && (!string.IsNullOrEmpty(source.Grade.Grade.Id)))
            {
                var gradeEntity = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Guid == grade.Grade.Id);
                if (gradeEntity == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to retrieve grade definition for id: '{0}'.", grade.Grade.Id), "Validation.Exception", source.Id, studentCourseSecId);
                }
                else
                {
                    if ((!string.IsNullOrEmpty(sectionGradeScheme)) && (sectionGradeScheme != gradeEntity.GradeSchemeCode))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Section grade-scheme code '", sectionGradeScheme, "' does not match grade grade-scheme '", gradeEntity.GradeSchemeCode, "'"), "Validation.Exception", source.Id, studentCourseSecId);
                    }
                    else
                    {
                        gradeId = gradeEntity.Id;
                        incompleteGrade = gradeEntity.IncompleteGrade;
                        studentUnverifiedGradesSubmission.GradeScheme = gradeEntity.GradeSchemeCode;
                        studentUnverifiedGradesSubmission.GradeId = gradeId;
                    }
                }
            }

            if ((grade.Type != null) && (!string.IsNullOrEmpty(grade.Type.Id)))
            {
                var gradeType = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Guid == grade.Type.Id);
                if ((gradeType == null) || (string.IsNullOrEmpty(gradeType.Code)))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to retrieve grade types for id: '{0}'.", grade.Type.Id), "Validation.Exception", source.Id, studentCourseSecId);
                }
                else
                {
                    studentUnverifiedGradesSubmission.GradeType = gradeType.Code;
                    switch (gradeType.Code)
                    {
                        case "FINAL":
                            studentUnverifiedGradesSubmission.FinalGrade = gradeId;
                            break;
                        case "MID1":
                            studentUnverifiedGradesSubmission.MidtermGrade1 = gradeId;
                            break;
                        case "MID2":
                            studentUnverifiedGradesSubmission.MidtermGrade2 = gradeId;
                            break;
                        case "MID3":
                            studentUnverifiedGradesSubmission.MidtermGrade3 = gradeId;
                            break;
                        case "MID4":
                            studentUnverifiedGradesSubmission.MidtermGrade4 = gradeId;
                            break;
                        case "MID5":
                            studentUnverifiedGradesSubmission.MidtermGrade5 = gradeId;
                            break;
                        case "MID6":
                            studentUnverifiedGradesSubmission.MidtermGrade6 = gradeId;
                            break;
                        default: break;
                    }
                }

                //The final grade is not defined as an incomplete grade (GRD.INCOMPLETE.GRADE is not null) 
                //and expiration date included in payload
                if ((string.IsNullOrEmpty(incompleteGrade)) && (grade.IncompleteGrade != null)
                    && (grade.IncompleteGrade.ExtensionDate != null) && (grade.IncompleteGrade.ExtensionDate.HasValue))
                {
                    if (gradeType.Code == "FINAL")
                    {
                        IntegrationApiExceptionAddError(string.Format("Final grade does not allow an expiration date."), "Validation.Exception", source.Id, studentCourseSecId);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Incomplete grade details only apply to final grades."), "Validation.Exception", source.Id, studentCourseSecId);
                    }
                }
                else
                {
                    if (grade.IncompleteGrade != null)
                    {
                        //If the object is included in the payload and the grade type is not "FINAL" issue an error that incomplete grade details only apply to final grades 
                        if (gradeType.Code != "FINAL")
                        {
                            IntegrationApiExceptionAddError("Incomplete grade details only apply to final grades.", "Validation.Exception", source.Id, studentCourseSecId);
                        }
                        else
                        {
                            if (((grade.IncompleteGrade.FinalGrade != null) || (grade.IncompleteGrade.ExtensionDate.HasValue))
                                && (string.IsNullOrEmpty(incompleteGrade)))
                            {
                                IntegrationApiExceptionAddError("Grade does not have an associated incomplete grade.", "Validation.Exception", source.Id, studentCourseSecId);
                            }
                            else
                            {
                                if ((grade.IncompleteGrade.ExtensionDate != null) && (grade.IncompleteGrade.ExtensionDate.HasValue))
                                {
                                    studentUnverifiedGradesSubmission.FinalGradeDate = grade.IncompleteGrade.ExtensionDate;
                                }
                                if ((grade.IncompleteGrade.FinalGrade != null) && (!string.IsNullOrEmpty(grade.IncompleteGrade.FinalGrade.Id)))
                                {

                                    var incompleteGradeEntity = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Guid == grade.IncompleteGrade.FinalGrade.Id);
                                    if (incompleteGradeEntity == null)
                                    {
                                        IntegrationApiExceptionAddError(string.Format("Unable to retrieve grade definition for grade.IncompleteGrade.FinalGrade.Id: '{0}'.", grade.IncompleteGrade.FinalGrade.Id), "Validation.Exception", source.Id, studentCourseSecId);
                                    }
                                    else
                                    {
                                        //If GRD.INCOMPLETE.GRADE is blank then issue an error
                                        if (incompleteGradeEntity.IncompleteGrade == null)
                                        {
                                            IntegrationApiExceptionAddError("Grade submitted is not an incomplete grade.", "Validation.Exception", source.Id, studentCourseSecId);
                                        }
                                        //The grade.incompleteGradeDetails.defaultFinalGrade does not match the GRD.INCOMPLETE.GRADE
                                        if (incompleteGradeEntity.Id != incompleteGrade)
                                        {
                                            IntegrationApiExceptionAddError(string.Format("Incomplete final grade is not valid for grade '{0}'.", incompleteGradeEntity.Id), "Validation.Exception", source.Id, studentCourseSecId);
                                        }

                                        //if ((grade.IncompleteGrade.ExtensionDate != null) && (grade.IncompleteGrade.ExtensionDate.HasValue))
                                        //{
                                        //    studentUnverifiedGradesSubmission.FinalGradeDate = grade.IncompleteGrade.ExtensionDate;
                                        //}
                                        studentUnverifiedGradesSubmission.FinalGrade = gradeId;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return studentUnverifiedGradesSubmission;
        }

        /// <summary>
        /// Validate StudentUnverifiedGradesSubmissions DTO
        /// </summary>
        /// <param name="source"></param>
        private void ValidateStudentUnverifiedGradesSubmissions(StudentUnverifiedGradesSubmissions source)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("StudentUnverifiedGradesSubmissions body is required.");
                throw IntegrationApiException;
            }

            if (source.Id == null)
            {
                IntegrationApiExceptionAddError("StudentUnverifiedGradesSubmissions.Id is required.");
            }
            if ((source.SectionRegistration == null) || (string.IsNullOrEmpty(source.SectionRegistration.Id)))
            {
                IntegrationApiExceptionAddError("StudentUnverifiedGradesSubmissions.SectionRegistration.Id is required.");
            }
            if (source.Grade != null)
            {
                var grade = source.Grade;

                if ((grade.Type == null) || (string.IsNullOrEmpty(grade.Type.Id)))
                {
                    IntegrationApiExceptionAddError("StudentUnverifiedGradesSubmissions.Grade.Type.Id is required.");
                }
                
                if ((grade.IncompleteGrade != null) && (grade.IncompleteGrade.FinalGrade != null) && (string.IsNullOrEmpty(grade.IncompleteGrade.FinalGrade.Id)))
                {
                    IntegrationApiExceptionAddError("StudentUnverifiedGradesSubmissions.Grade.IncompleteGrade.FinalGrade.Id is required if providing an IncompleteGrade.FinalGrade.");
                }
            }

            if (source.LastAttendance != null)
            {
                var lastAttendance = source.LastAttendance;
                if ((lastAttendance.Date == null) && (lastAttendance.Status == StudentUnverifiedGradesStatus.NotSet))
                {
                    IntegrationApiExceptionAddError("If providing a StudentUnverifiedGradesSubmissions.LastAttendance then either the Date or Status must be populated.");
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts StudentUnverifiedGrades domain entities to their corresponding StudentUnverifiedGradesSubmissions DTOs
        /// </summary>
        /// <param name="source">AcadCredentials domain entities</param>
        /// <param name="source">bypass cache boolean</param>
        /// <returns>StudentUnverifiedGradesSubmissions DTOs</returns>
        private async Task<StudentUnverifiedGradesSubmissions> ConvertStudentUnverifiedGradesEntityToDto(Domain.Student.Entities.StudentUnverifiedGrades source, bool bypassCache)
        {
            if (source == null)
            {
                return null;
            }

            var studentUnverifiedGradesSubmission = new Dtos.StudentUnverifiedGradesSubmissions();
            studentUnverifiedGradesSubmission.Id = source.Guid;

            if (string.IsNullOrEmpty(source.StudentAcadaCredId))
            {
                throw new ArgumentNullException("SectionRegistration is required. ");
            }

            var studentAcadCredGuid = await this._studentUnverifiedGradesRepository.GetStudentAcadCredGuidFromIdAsync(source.StudentAcadaCredId);
            if (string.IsNullOrEmpty(studentAcadCredGuid))
            {
                throw new ArgumentNullException(string.Concat("Unable to obtain guid for StudentAcadCredId:  ", source.StudentAcadaCredId));
            }
            studentUnverifiedGradesSubmission.SectionRegistration = new GuidObject2(studentAcadCredGuid);


            StudentUnverifiedGradesGradeDtoProperty studentUnverifiedGradesGradeDtoProperty = null;
            if (!string.IsNullOrEmpty(source.FinalGrade))
            {
               
                var allGrades = await GetGradesAsync(bypassCache);
                if (allGrades == null)
                {
                    throw new Exception("Unable to retrieve grades");
                }

                var finalGrade = allGrades.FirstOrDefault(g => g.Id == source.FinalGrade);
                if (finalGrade == null)
                {
                    throw new Exception(string.Format("Invalid final grade '{0}' for Student Course Sec {1}.", source.FinalGrade, source.Guid));
                }
                if (studentUnverifiedGradesGradeDtoProperty == null)
                    studentUnverifiedGradesGradeDtoProperty = new StudentUnverifiedGradesGradeDtoProperty();

                studentUnverifiedGradesGradeDtoProperty.Grade = new GuidObject2(finalGrade.Guid);
                var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "FINAL");
                studentUnverifiedGradesGradeDtoProperty.Type = new GuidObject2(type.Guid);

                // Report incomplete grade info if final grade is an incomplete one.
                if (!string.IsNullOrEmpty(finalGrade.IncompleteGrade))
                {
                    var studentUnverifiedGradesIncompleteGradeDtoProperty = new StudentUnverifiedGradesIncompleteGradeDtoProperty();
                    var incompleteGradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == finalGrade.IncompleteGrade);
                    if (finalGrade == null)
                    {
                        throw new Exception(string.Format("Invalid incomplete grade '{0}' for Student Course Sec {1}.", source.FinalGrade, source.Guid));
                    }
                    studentUnverifiedGradesIncompleteGradeDtoProperty.FinalGrade = new GuidObject2(incompleteGradeGuid.Guid);
                    studentUnverifiedGradesIncompleteGradeDtoProperty.ExtensionDate = source.IncompleteGradeExtensionDate;
                    studentUnverifiedGradesGradeDtoProperty.IncompleteGrade = studentUnverifiedGradesIncompleteGradeDtoProperty;
                }
            }
            else if (!string.IsNullOrEmpty(source.MidtermGrade1))
            {
                var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade1);
                if (gradeGuid == null)
                {
                    throw new Exception(string.Format("Invalid midterm grade 1 '{0}' for Student Course Sec {1}.", source.MidtermGrade1, source.Guid));
                }
                if (studentUnverifiedGradesGradeDtoProperty == null)
                    studentUnverifiedGradesGradeDtoProperty = new StudentUnverifiedGradesGradeDtoProperty();
                studentUnverifiedGradesGradeDtoProperty.Grade = new GuidObject2(gradeGuid.Guid);
                var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID1");
                studentUnverifiedGradesGradeDtoProperty.Type = new GuidObject2(type.Guid);
            }
            else if (!string.IsNullOrEmpty(source.MidtermGrade2))
            {
                var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade2);
                if (gradeGuid == null)
                {
                    throw new Exception(string.Format("Invalid midterm grade 2 '{0}' for Student Course Sec {1}.", source.MidtermGrade2, source.Guid));
                }
                if (studentUnverifiedGradesGradeDtoProperty == null)
                    studentUnverifiedGradesGradeDtoProperty = new StudentUnverifiedGradesGradeDtoProperty();
                studentUnverifiedGradesGradeDtoProperty.Grade = new GuidObject2(gradeGuid.Guid);
                var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID2");
                studentUnverifiedGradesGradeDtoProperty.Type = new GuidObject2(type.Guid);
            }
            else if (!string.IsNullOrEmpty(source.MidtermGrade3))
            {
                var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade3);
                if (gradeGuid == null)
                {
                    throw new Exception(string.Format("Invalid midterm grade 3 '{0}' for Student Course Sec {1}.", source.MidtermGrade3, source.Guid));
                }
                if (studentUnverifiedGradesGradeDtoProperty == null)
                    studentUnverifiedGradesGradeDtoProperty = new StudentUnverifiedGradesGradeDtoProperty();
                studentUnverifiedGradesGradeDtoProperty.Grade = new GuidObject2(gradeGuid.Guid);
                var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID3");
                studentUnverifiedGradesGradeDtoProperty.Type = new GuidObject2(type.Guid);
            }
            else if (!string.IsNullOrEmpty(source.MidtermGrade4))
            {
                var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade4);
                if (gradeGuid == null)
                {
                    throw new Exception(string.Format("Invalid midterm grade 4 '{0}' for Student Course Sec {1}.", source.MidtermGrade4, source.Guid));
                }
                if (studentUnverifiedGradesGradeDtoProperty == null)
                    studentUnverifiedGradesGradeDtoProperty = new StudentUnverifiedGradesGradeDtoProperty();
                studentUnverifiedGradesGradeDtoProperty.Grade = new GuidObject2(gradeGuid.Guid);
                var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID4");
                studentUnverifiedGradesGradeDtoProperty.Type = new GuidObject2(type.Guid);
            }
            else if (!string.IsNullOrEmpty(source.MidtermGrade5))
            {
                var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade5);
                if (gradeGuid == null)
                {
                    throw new Exception(string.Format("Invalid midterm grade 5 '{0}' for Student Course Sec {1}.", source.MidtermGrade5, source.Guid));
                }
                if (studentUnverifiedGradesGradeDtoProperty == null)
                    studentUnverifiedGradesGradeDtoProperty = new StudentUnverifiedGradesGradeDtoProperty();
                studentUnverifiedGradesGradeDtoProperty.Grade = new GuidObject2(gradeGuid.Guid);
                var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID5");
                studentUnverifiedGradesGradeDtoProperty.Type = new GuidObject2(type.Guid);
            }
            else if (!string.IsNullOrEmpty(source.MidtermGrade6))
            {
                var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade6);
                if (gradeGuid == null)
                {
                    throw new Exception(string.Format("Invalid midterm grade 6 '{0}' for Student Course Sec {1}.", source.MidtermGrade6, source.Guid));
                }
                if (studentUnverifiedGradesGradeDtoProperty == null)
                    studentUnverifiedGradesGradeDtoProperty = new StudentUnverifiedGradesGradeDtoProperty();
                studentUnverifiedGradesGradeDtoProperty.Grade = new GuidObject2(gradeGuid.Guid);
                var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID6");
                studentUnverifiedGradesGradeDtoProperty.Type = new GuidObject2(type.Guid);
            }

            if (studentUnverifiedGradesGradeDtoProperty != null)
            {
                studentUnverifiedGradesSubmission.Grade = studentUnverifiedGradesGradeDtoProperty;
            }
            if ((source.LastAttendDate != null && source.LastAttendDate.HasValue) || (source.HasNeverAttended == true))
            {
                var lastAttendance = new StudentUnverifiedGradesLastAttendanceDtoProperty();

                if (source.LastAttendDate != null && source.LastAttendDate.HasValue)
                    lastAttendance.Date = source.LastAttendDate.Value;
                if (source.HasNeverAttended == true)
                    lastAttendance.Status = StudentUnverifiedGradesStatus.Neverattended;

                studentUnverifiedGradesSubmission.LastAttendance = lastAttendance;
            }
            return studentUnverifiedGradesSubmission;
        }

        /// <summary>
        /// IntegrationApiExceptionAddError
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        private void IntegrationApiExceptionAddError(string message, string code = null)
        {
            if (IntegrationApiException == null)
                IntegrationApiException = new IntegrationApiException();

            if (string.IsNullOrEmpty(code))
                code = "Global.Internal.Error";

            IntegrationApiException.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
            {
                Code = code,
                Message = message,
            });
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts StudentUnverifiedGrades domain entities to their corresponding StudentUnverifiedGrades DTOs
        /// </summary>
        /// <param name="source">AcadCredentials domain entities</param>
        /// <param name="source">bypass cache boolean</param>
        /// <returns>StudentUnverifiedGrades DTOs</returns>
        private async Task<IEnumerable<Dtos.StudentUnverifiedGrades>> ConvertStudentUnverifiedGradesEntityToDto(IEnumerable<Domain.Student.Entities.StudentUnverifiedGrades> sources, bool bypassCache)
        {
            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }

            var studentUnverifiedGrades = new List<Dtos.StudentUnverifiedGrades>();
            Dictionary<string, string> personGuidCollection = null;
            Dictionary<string, string> studentAcadCredGuidCollection = null;

            try
            {
                var personIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.StudentId)))
                     .Select(x => x.StudentId).Distinct().ToList();
                personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);
                IsGuidCollectionValid(personIds, personGuidCollection, "PERSONS");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            try
            {
                var studentAcadCredIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.StudentAcadaCredId)))
                     .Select(x => x.StudentAcadaCredId).Distinct().ToList();
                studentAcadCredGuidCollection = await _studentUnverifiedGradesRepository.GetGuidsCollectionAsync(studentAcadCredIds, "STUDENT.ACAD.CRED");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            foreach (var source in sources)
            {
                var studentUnverifiedGrade = new Dtos.StudentUnverifiedGrades();

                studentUnverifiedGrade.Id = source.Guid;

                bool validSource = true;

                // student
                if (string.IsNullOrEmpty(source.StudentId))
                {
                    IntegrationApiExceptionAddError("Student is required.", guid: source.Guid, id: source.StudentCourseSecId);
                    validSource = false;
                }
                else
                {
                    if (personGuidCollection == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to locate guid for student ID : {0}.", source.StudentId), guid: source.Guid, id: source.StudentCourseSecId);
                        validSource = false;
                    }
                    else
                    {
                        var personGuid = string.Empty;
                        personGuidCollection.TryGetValue(source.StudentId, out personGuid);
                        if (string.IsNullOrEmpty(personGuid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for student ID : {0}.", source.StudentId), guid: source.Guid, id: source.StudentCourseSecId);
                            validSource = false;
                        }
                        else
                        {
                            studentUnverifiedGrade.Student = new GuidObject2(personGuid);
                        }
                    }
                }

                // sectionRegistration
                if (validSource == true)
                {
                    if (string.IsNullOrEmpty(source.StudentAcadaCredId))
                    {
                        IntegrationApiExceptionAddError("SectionRegistration is required.", guid: source.Guid, id: source.StudentCourseSecId);
                        validSource = false;
                    }
                    else
                    {
                        if (studentAcadCredGuidCollection == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate student acad cred guid for studentAcadCred ID : {0}.", source.StudentAcadaCredId), guid: source.Guid,
                                id: source.StudentCourseSecId);
                            validSource = false;
                        }
                        else
                        {
                            var studentAcadCredGuid = string.Empty;
                            studentAcadCredGuidCollection.TryGetValue(source.StudentAcadaCredId, out studentAcadCredGuid);
                            if (string.IsNullOrEmpty(studentAcadCredGuid))
                            {
                                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for studentAcadCred ID : {0}.", source.StudentAcadaCredId), guid: source.Guid, 
                                    id: source.StudentCourseSecId);
                                validSource = false;
                            }
                            studentUnverifiedGrade.SectionRegistration = new GuidObject2(studentAcadCredGuid);
                        }
                    }
                }

                // awardGradeScheme
                if (validSource == true)
                {
                    if (!string.IsNullOrEmpty(source.GradeScheme))
                    {
                        var gradeSchemes = await GetGradeSchemesAsync(bypassCache);
                        if (gradeSchemes != null)
                        {
                            var gradeScheme = gradeSchemes.FirstOrDefault(t => t.Code == source.GradeScheme);
                            if (gradeScheme == null || string.IsNullOrEmpty(gradeScheme.Guid))
                            {
                                IntegrationApiExceptionAddError(string.Format("Invalid grade scheme or missing GUID for grade scheme code'{0}'.", source.GradeScheme), guid: source.Guid, id: source.StudentCourseSecId);
                            }
                            else
                            {
                                studentUnverifiedGrade.AwardGradeScheme = new GuidObject2(gradeScheme.Guid);
                            }
                        }
                    }
                }

                // details
                if (validSource == true)
                {
                    var gradeObjects = new List<Dtos.StudentUnverifiedGradesGrades>();
                    if (!string.IsNullOrEmpty(source.FinalGrade))
                    {
                        var gradeObject = new StudentUnverifiedGradesGrades();
                        var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.FinalGrade);
                        if (gradeGuid == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid final grade '{0}'.", source.FinalGrade), guid: source.Guid, id: source.StudentCourseSecId);
                        }
                        else
                        {
                            gradeObject.Grade = new GuidObject2(gradeGuid.Guid);
                            var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "FINAL");
                            gradeObject.Type = new GuidObject2(type.Guid);
                            gradeObject.SubmittedOn = source.FinalGradeDate;
                            gradeObjects.Add(gradeObject);

                            // incompleteGrade
                            // Report incomplete grade info if final grade is an incomplete one.
                            if (!string.IsNullOrEmpty(gradeGuid.IncompleteGrade))
                            {
                                var incompleteGradeObject = new StudentUnverifiedGradesIncompleteGrade();
                                var incompleteGradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == gradeGuid.IncompleteGrade);
                                if (gradeGuid == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Invalid incomplete grade '{0}'.", source.FinalGrade), guid: source.Guid, id: source.StudentCourseSecId);
                                }
                                incompleteGradeObject.FinalGrade = new GuidObject2(incompleteGradeGuid.Guid);
                                incompleteGradeObject.ExtensionDate = source.IncompleteGradeExtensionDate;
                                studentUnverifiedGrade.IncompleteGrade = incompleteGradeObject;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(source.MidtermGrade1))
                    {
                        var gradeObject = new StudentUnverifiedGradesGrades();
                        var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade1);
                        if (gradeGuid == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid midterm grade 1 '{0}'.", source.MidtermGrade1), guid: source.Guid, id: source.StudentCourseSecId);
                        }
                        else
                        {
                            gradeObject.Grade = new GuidObject2(gradeGuid.Guid);
                            var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID1");
                            gradeObject.Type = new GuidObject2(type.Guid);
                            gradeObject.SubmittedOn = source.MidtermGradeDate1;
                            gradeObjects.Add(gradeObject);
                        }
                    }

                    if (!string.IsNullOrEmpty(source.MidtermGrade2))
                    {
                        var gradeObject = new StudentUnverifiedGradesGrades();
                        var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade2);
                        if (gradeGuid == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid midterm grade 2 '{0}'.", source.MidtermGrade2), guid: source.Guid, id: source.StudentCourseSecId);
                        }
                        else
                        {
                            gradeObject.Grade = new GuidObject2(gradeGuid.Guid);
                            var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID2");
                            gradeObject.Type = new GuidObject2(type.Guid);
                            gradeObject.SubmittedOn = source.MidtermGradeDate2;
                            gradeObjects.Add(gradeObject);
                        }
                    }

                    if (!string.IsNullOrEmpty(source.MidtermGrade3))
                    {
                        var gradeObject = new StudentUnverifiedGradesGrades();
                        var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade3);
                        if (gradeGuid == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid midterm grade 3 '{0}'.", source.MidtermGrade3), guid: source.Guid, id: source.StudentCourseSecId);
                        }
                        else
                        {
                            gradeObject.Grade = new GuidObject2(gradeGuid.Guid);
                            var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID3");
                            gradeObject.Type = new GuidObject2(type.Guid);
                            gradeObject.SubmittedOn = source.MidtermGradeDate3;
                            gradeObjects.Add(gradeObject);
                        }
                    }

                    if (!string.IsNullOrEmpty(source.MidtermGrade4))
                    {
                        var gradeObject = new StudentUnverifiedGradesGrades();
                        var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade4);
                        if (gradeGuid == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid midterm grade 4 '{0}'.", source.MidtermGrade4), guid: source.Guid, id: source.StudentCourseSecId);
                        }
                        else
                        {
                            gradeObject.Grade = new GuidObject2(gradeGuid.Guid);
                            var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID4");
                            gradeObject.Type = new GuidObject2(type.Guid);
                            gradeObject.SubmittedOn = source.MidtermGradeDate4;
                            gradeObjects.Add(gradeObject);
                        }
                    }

                    if (!string.IsNullOrEmpty(source.MidtermGrade5))
                    {
                        var gradeObject = new StudentUnverifiedGradesGrades();
                        var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade5);
                        if (gradeGuid == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid midterm grade 5 '{0}'.", source.MidtermGrade5), guid: source.Guid, id: source.StudentCourseSecId);
                        }
                        else
                        {
                            gradeObject.Grade = new GuidObject2(gradeGuid.Guid);
                            var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID5");
                            gradeObject.Type = new GuidObject2(type.Guid);
                            gradeObject.SubmittedOn = source.MidtermGradeDate5;
                            gradeObjects.Add(gradeObject);
                        }
                    }

                    if (!string.IsNullOrEmpty(source.MidtermGrade6))
                    {
                        var gradeObject = new StudentUnverifiedGradesGrades();
                        var gradeGuid = (await GetGradesAsync(bypassCache)).FirstOrDefault(g => g.Id == source.MidtermGrade6);
                        if (gradeGuid == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Invalid midterm grade 6 '{0}'.", source.MidtermGrade6), guid: source.Guid, id: source.StudentCourseSecId);
                        }
                        else
                        {
                            gradeObject.Grade = new GuidObject2(gradeGuid.Guid);
                            var type = (await GetGradeTypesAsync(bypassCache)).FirstOrDefault(t => t.Code == "MID6");
                            gradeObject.Type = new GuidObject2(type.Guid);
                            gradeObject.SubmittedOn = source.MidtermGradeDate6;
                            gradeObjects.Add(gradeObject);
                        }
                    }

                    var gradeDetails = new Dtos.StudentUnverifiedGradesDetails();

                    if (gradeObjects != null && gradeObjects.Any())
                    {
                        gradeDetails.Grades = gradeObjects;
                    }

                    if (source.LastAttendDate != null)
                    {
                        var LastAttendObject = new StudentUnverifiedGradesLastAttendance();
                        LastAttendObject.Date = source.LastAttendDate;
                        gradeDetails.LastAttendance = LastAttendObject;
                    }
                    else
                    {
                        if (source.HasNeverAttended == true)
                        {
                            var LastAttendObject = new StudentUnverifiedGradesLastAttendance();
                            LastAttendObject.Status = Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended;
                            gradeDetails.LastAttendance = LastAttendObject;
                        }
                    }

                    studentUnverifiedGrade.Details = gradeDetails;

                    studentUnverifiedGrades.Add(studentUnverifiedGrade);
                }

            }

            return studentUnverifiedGrades;
        }
        
        /// <summary>
        /// ValidateGuidCollection.
        /// </summary>
        /// <param name="ids">Collection of string ids</param>
        /// <param name="guidCollection">Dictionary containing ids (key) and guids (value)</param>
        /// <param name="entityName">Entity name used for building the error message</param>
        /// <returns>bool representing if an error occured.  Errors are written to IntegrationApiException</returns>
        private bool IsGuidCollectionValid(List<string> ids, Dictionary<string, string> guidCollection, string entityName)
        {
            var retval = true;

            if (ids == null && guidCollection != null && guidCollection.Any())
            {
                guidCollection.Values.ToList().ForEach(i =>
                        IntegrationApiExceptionAddError(string.Format("Unable to locate {0} id for guid '{1}.", entityName, i)));
                retval = false;
            }
            else if (guidCollection == null && ids != null)
            {
                ids.ToList().ForEach(i =>
                        IntegrationApiExceptionAddError(string.Format("Unable to locate {0} guid for id '{1}'.", entityName, i)));
                retval = false;
            }
            else if (ids.Count() != guidCollection.Count())
            {
                var missingIds = ids.Except(guidCollection.Keys);
                if (missingIds != null && missingIds.Any())
                {
                    missingIds.ToList().ForEach(i =>
                        IntegrationApiExceptionAddError(string.Format("Unable to locate {0} guid for id '{1}'.", entityName, i)));
                    retval = false;
                }
            }
            return retval;
        }

        /// <summary>
        /// Get all Grade Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="Grade"> Grade entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.Grade>> GetGradesAsync(bool bypassCache)
        {
            if (_grades == null)
            {
                _grades = await _gradeRepository.GetHedmAsync(bypassCache);
            }
            return _grades;
        }

        /// <summary>
        /// Get all Grade Scheme Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="Grade"> Grade scheme entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GetGradeSchemesAsync(bool bypassCache)
        {
            if (_gradeSchemes == null)
            {
                _gradeSchemes = await _referenceDataRepository.GetGradeSchemesAsync(bypassCache);
            }
            return _gradeSchemes;
        }

        /// <summary>
        /// Get all Section Grades Types Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="Grade"> Section Grade Types entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.SectionGradeType>> GetGradeTypesAsync(bool bypassCache)
        {
            if (_gradeTypes == null)
            {
                _gradeTypes = await _referenceDataRepository.GetSectionGradeTypesAsync(bypassCache);
            }
            return _gradeTypes;
        }
    }
}
