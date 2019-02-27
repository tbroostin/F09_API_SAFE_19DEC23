//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using System.Net;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentTranscriptGradesService : BaseCoordinationService, IStudentTranscriptGradesService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentTranscriptGradesRepository _studentTranscriptGradesRepository;

        public StudentTranscriptGradesService(
            IStudentTranscriptGradesRepository studentTranscriptGradesRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IPersonRepository personRepository,
            IGradeRepository gradeRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _studentTranscriptGradesRepository = studentTranscriptGradesRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _personRepository = personRepository;
            _gradeRepository = gradeRepository;
        }

        #region GET Methods

        /// <summary>
        /// Gets all StudentTranscriptGrade
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentTranscriptGrades">StudentTranscriptGrade</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentTranscriptGrades>, int>> GetStudentTranscriptGradesAsync(int offset, int limit,
            Dtos.StudentTranscriptGrades criteriaFilter, bool bypassCache = false)
        {
            string studentGuid = string.Empty;
            if (criteriaFilter != null)
            {
                studentGuid = criteriaFilter.Student != null ? criteriaFilter.Student.Id : string.Empty;
            }

            string studentId = string.Empty;
            if (!string.IsNullOrEmpty(studentGuid))
            {
                try
                {
                    studentId = await _personRepository.GetPersonIdFromGuidAsync(studentGuid);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentTranscriptGrades>, int>(new List<Dtos.StudentTranscriptGrades>(), 0);
                }
                if (string.IsNullOrEmpty(studentId))
                    return new Tuple<IEnumerable<Dtos.StudentTranscriptGrades>, int>(new List<Dtos.StudentTranscriptGrades>(), 0);
            }

            var studentTranscriptGrades = new List<Dtos.StudentTranscriptGrades>();
            try
            {
                if (!await CheckViewStudentTranscriptGradesPermission())
                {
                    throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view student transcript grades.");
                }

                var studentTranscriptGradesEntities = await _studentTranscriptGradesRepository.GetStudentTranscriptGradesAsync(offset, limit, studentId, bypassCache);
                if (studentTranscriptGradesEntities != null && studentTranscriptGradesEntities.Item1.Any())
                {
                    studentTranscriptGrades = (await BuildStudentTranscriptGradesDtoAsync(studentTranscriptGradesEntities.Item1, bypassCache)).ToList();
                }
                return studentTranscriptGrades.Any() ? new Tuple<IEnumerable<Dtos.StudentTranscriptGrades>, int>(studentTranscriptGrades, studentTranscriptGradesEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.StudentTranscriptGrades>, int>(new List<Dtos.StudentTranscriptGrades>(), 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get a StudentTranscriptGrade by guid.
        /// </summary>
        /// <param name="guid">Guid of the StudentTranscriptGrade in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentTranscriptGrades">StudentTranscriptGrade</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentTranscriptGrades> GetStudentTranscriptGradesByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a student transcript grade.");
            }
            try
            {
                if (!await CheckViewStudentTranscriptGradesPermission())
                {
                    throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view student transcript grades. ");
                }

                var studentTranscriptGradesEntity = await _studentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);
                var studentTranscriptGrades = (await BuildStudentTranscriptGradesDtoAsync(new List<Domain.Student.Entities.StudentTranscriptGrades>()
                { studentTranscriptGradesEntity }, bypassCache));

                return studentTranscriptGrades != null ? studentTranscriptGrades.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get a StudentTranscriptGrade by guid.
        /// </summary>
        /// <param name="guid">Guid of the StudentTranscriptGrade in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentTranscriptGradesAdjustments">StudentTranscriptGrade</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentTranscriptGradesAdjustments> GetStudentTranscriptGradesAdjustmentsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a student transcript grade.");
            }

            if (!await CheckViewStudentTranscriptGradesPermission())
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view student transcript grades. ");
            }

            var studentTranscriptGradesEntity = await _studentTranscriptGradesRepository.GetStudentTranscriptGradesByGuidAsync(guid);

            StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments = null;
            try
            {
                studentTranscriptGradesAdjustments = (await BuildStudentTranscriptGradesAdjustmentsDtoAsync(studentTranscriptGradesEntity, bypassCache));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return studentTranscriptGradesAdjustments;

        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Update a StudentTranscriptGradesAdjustments.
        /// </summary>
        /// <param name="StudentTranscriptGradesAdjustments">The <see cref="StudentTranscriptGradesAdjustments">studentTranscriptGradesAdjustments</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentTranscriptGrades">studentTranscriptGrades</see></returns>
        public async Task<Dtos.StudentTranscriptGrades> UpdateStudentTranscriptGradesAdjustmentsAsync(StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments, bool bypassCache = true)
        {
            if (studentTranscriptGradesAdjustments == null)
                throw new ArgumentNullException("StudentTranscriptGradesAdjustments", "Must provide a StudentTranscriptGradesAdjustments for update");
            if (string.IsNullOrEmpty(studentTranscriptGradesAdjustments.Id))
                throw new ArgumentNullException("StudentTranscriptGradesAdjustments", "Must provide a guid for StudentTranscriptGradesAdjustments update");

            // get the ID associated with the incoming guid
            var studentTranscriptGradesAdjustmentsEntityId = await _studentTranscriptGradesRepository.GetStudentTranscriptGradesIdFromGuidAsync(studentTranscriptGradesAdjustments.Id);
            if (string.IsNullOrEmpty(studentTranscriptGradesAdjustmentsEntityId))
            {
                throw new ArgumentException(string.Format("Invalid GUID '{0}' provided for update to StudentTranscriptGradesAdjustments. ", "StudentTranscriptGradesAdjustments.Id"));
            }
            var detail = studentTranscriptGradesAdjustments.Detail;
            if (detail == null)
            {
                throw new ArgumentException("The detail object is missing from StudentTranscriptGradesAdjustment. ", "StudentTranscriptGradesAdjustments.Detail");
            }
            if (detail == null || detail.Grade == null || string.IsNullOrEmpty(detail.Grade.Id))
            {
                throw new ArgumentNullException("StudentTranscriptGradesAdjustments.Detail.Grade.Id", "Must provide a grade for StudentTranscriptGradesAdjustments update");
            }
            // verify the user has the permission to update a studentTranscriptGradesAdjustments
            CheckCreateStudentTranscriptGradesAdjustmentsPermission();

            _studentTranscriptGradesRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // map the DTO to entities
            var studentTranscriptGradesEntity
            = await ConvertStudentTranscriptGradesAdjustmentsDtoToEntityAsync(studentTranscriptGradesAdjustmentsEntityId, studentTranscriptGradesAdjustments, bypassCache);

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            Domain.Student.Entities.StudentTranscriptGrades updatedStudentTranscriptGradesEntity = null;
            try
            {
                // update the entity in the database
                updatedStudentTranscriptGradesEntity =
                      await _studentTranscriptGradesRepository.UpdateStudentTranscriptGradesAdjustmentsAsync(studentTranscriptGradesEntity);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            var studentTranscriptGrades = (await BuildStudentTranscriptGradesDtoAsync(new List<Domain.Student.Entities.StudentTranscriptGrades>()
                { updatedStudentTranscriptGradesEntity }, bypassCache));

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return studentTranscriptGrades != null ? studentTranscriptGrades.FirstOrDefault() : null;

        }
        #endregion

        #region Private Methods

        /// <summary>
        /// BuildStudentTranscriptGradesDtoAsync
        /// </summary>
        /// <param name="sources">Collection of StudentTranscriptGrades domain entities</param>
        /// <param name="bypassCache">bypassCache flag.  Defaulted to false</param>
        /// <returns>Collection of StudentTranscriptGrades DTO objects </returns>
        private async Task<IEnumerable<Dtos.StudentTranscriptGrades>> BuildStudentTranscriptGradesDtoAsync(IEnumerable<Domain.Student.Entities.StudentTranscriptGrades> sources,
            bool bypassCache = false)
        {

            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }
            var studentTranscriptGrade = new List<Dtos.StudentTranscriptGrades>();
            Dictionary<string, string> personGuidCollection = null;
            Dictionary<string, string> sectionGuidCollection = null;
            Dictionary<string, string> courseGuidCollection = null;
            Dictionary<string, string> studentCourseSectionGuidCollection = null;

            try
            {
                var personIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.StudentId)))
                     .Select(x => x.StudentId).Distinct().ToList();
                personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            try
            {
                var sectionIds = sources
                    .Where(x => (!string.IsNullOrEmpty(x.CourseSection)))
                    .Select(x => x.CourseSection).Distinct().ToList();
                sectionGuidCollection = await _studentTranscriptGradesRepository.GetGuidsCollectionAsync(sectionIds, "COURSE.SECTIONS");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            try
            {
                var courseIds = sources
                           .Where(x => (!string.IsNullOrEmpty(x.Course)))
                           .Select(x => x.Course).Distinct().ToList();
                courseGuidCollection = await _studentTranscriptGradesRepository.GetGuidsCollectionAsync(courseIds, "COURSES");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            try
            {
                var studentCourseSectionIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.StudentCourseSectionId)))
                     .Select(x => x.StudentCourseSectionId).Distinct().ToList();
                studentCourseSectionGuidCollection = await _studentTranscriptGradesRepository.GetGuidsCollectionAsync(studentCourseSectionIds, "STUDENT.COURSE.SEC");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }        

            foreach (var source in sources)
            {
                try
                {
                    studentTranscriptGrade.Add(await ConvertStudentTranscriptGradesEntityToDtoAsync(source,
                        personGuidCollection, sectionGuidCollection, courseGuidCollection, studentCourseSectionGuidCollection, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, id: source.Id, guid: source.Guid);
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return studentTranscriptGrade;
        }

      
        private async Task<Dtos.StudentTranscriptGradesAdjustments> BuildStudentTranscriptGradesAdjustmentsDtoAsync(Domain.Student.Entities.StudentTranscriptGrades source,
                  bool bypassCache = false)
        {

            if (source == null)
            {
                return null;
            }
            var studentTranscriptGrade = new Dtos.StudentTranscriptGradesAdjustments();
            studentTranscriptGrade.Id = source.Guid;
            studentTranscriptGrade.Detail = new StudentTranscriptGradesAdjustmentsDetail();
            studentTranscriptGrade.Detail.Grade = await ConvertVerifiedGradeToGuidObjectAsync(source.VerifiedGrade, source.Guid, source.Id, bypassCache);
            // Don't build the incomplete grade object because on partial put, the data is not compatible
            // with a grade submission that is not defined with an incomplete grade.
            // studentTranscriptGrade.Detail.IncompleteGrade = await ConvertEntityToIncompleteGradeGuidObjectAsync(source, bypassCache);
            //if (source.FinalGradeExpirationDate != null && studentTranscriptGrade.Detail.IncompleteGrade != null)
            //    studentTranscriptGrade.Detail.IncompleteGrade.ExtensionDate = source.FinalGradeExpirationDate;

            return studentTranscriptGrade;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentTranscriptGrades domain entity to its corresponding StudentTranscriptGrades DTO
        /// </summary>
        /// <param name="source">StudentTranscriptGrades domain entity</param>
        /// <returns>StudentTranscriptGrades DTO</returns>
        private async Task<StudentTranscriptGrades> ConvertStudentTranscriptGradesEntityToDtoAsync(Domain.Student.Entities.StudentTranscriptGrades source,
            Dictionary<string, string> personGuidCollection,
            Dictionary<string, string> sectionGuidCollection,
            Dictionary<string, string> courseGuidCollection,
            Dictionary<string, string> studentCourseSecGuidCollection,
            bool bypassCache = false)
        {

            if (source == null)
                return null;

            var studentTranscriptGrade = new Ellucian.Colleague.Dtos.StudentTranscriptGrades();

            studentTranscriptGrade.Id = source.Guid;
            studentTranscriptGrade.Student = ConvertStudentIdToGuidObject(source.StudentId, personGuidCollection, source.Guid, source.Id);
            studentTranscriptGrade.Course = ConvertCourseToStudentTranscriptGradesCourse(source, sectionGuidCollection, courseGuidCollection);
            studentTranscriptGrade.Grade = await ConvertVerifiedGradeToGuidObjectAsync(source.VerifiedGrade, source.Guid, source.Id, bypassCache);
            studentTranscriptGrade.IncompleteGrade = await ConvertEntityToIncompleteGradeGuidObjectAsync(source, bypassCache);
            studentTranscriptGrade.AwardGradeScheme = await ConvertGradeSchemesCodeToGuidObjectAsync(source.GradeSchemeCode, source.Guid, source.Id, bypassCache);
            studentTranscriptGrade.CreditCategory = await ConvertEntityToCreditCategoryGuidObjectAsync(source, bypassCache);
            studentTranscriptGrade.Credit = ConvertEntityToGradesCredit(source);
            studentTranscriptGrade.UnverifiedGrade = ConvertStudentCourseSecToUnverifiedGradeGuidObject(source.StudentCourseSectionId, studentCourseSecGuidCollection, source.Guid, source.Id);
            if (source.VerifiedGradeDate.HasValue)
                studentTranscriptGrade.RecordedOn = source.VerifiedGradeDate.Value;
            studentTranscriptGrade.ChangeDetails = await ConvertEntityToStudentTranscriptGradesChangeDetailsCollectionAsync(source.StudentTranscriptGradesHistory,
                source.Guid, source.Id, bypassCache);

            return studentTranscriptGrade;
        }

        #endregion

        #region Convert Methods

        /// <summary>
        /// Convert the StudentTranscriptGradesAdjustments DTO to its associated StudentTranscriptGradesAdjustments domain entity
        /// </summary>
        /// <param name="studentTranscriptGradesAdjustmentsId">Id for StudentTranscriptGrades</param>
        /// <param name="studentTranscriptGradesAdjustmentsDto">DTO for StudentTranscriptGradesAdjustments</param>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.StudentTranscriptGradesAdjustments> ConvertStudentTranscriptGradesAdjustmentsDtoToEntityAsync(string studentTranscriptGradesAdjustmentsId, StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustmentsDto, bool bypassCache)
        {
            var studentTranscriptGradesAdjustment = new Domain.Student.Entities.StudentTranscriptGradesAdjustments(studentTranscriptGradesAdjustmentsId, studentTranscriptGradesAdjustmentsDto.Id);
            var detail = studentTranscriptGradesAdjustmentsDto.Detail;
            if (detail != null)
            {
                var gradeDefinitions = await GetGradeDefinitionsAsync(bypassCache);
                if (gradeDefinitions == null)
                {
                    IntegrationApiExceptionAddError("Grade definitions table not found. Grades cannot be validated",
                        guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId);
                }
                string incompleteGradeAllowed = string.Empty;
                if (detail.Grade != null && !string.IsNullOrEmpty(detail.Grade.Id))
                {

                    var gradeCodeEntity = gradeDefinitions.FirstOrDefault(gd => gd.Guid.Equals(detail.Grade.Id, StringComparison.OrdinalIgnoreCase));
                    if (gradeCodeEntity == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Grade definition not found for Guid: {0}", detail.Grade.Id),
                           guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId, httpStatusCode: HttpStatusCode.NotFound);

                    }
                    else
                    {
                        studentTranscriptGradesAdjustment.VerifiedGrade = gradeCodeEntity.Id;
                        incompleteGradeAllowed = gradeCodeEntity.IncompleteGrade;

                        // If an incomplete grade is defined for this grade then extension date is required
                        if (!string.IsNullOrEmpty(incompleteGradeAllowed) && (detail.IncompleteGrade == null || detail.IncompleteGrade.ExtensionDate == null))
                        {
                            IntegrationApiExceptionAddError("The grade is defined as an incomplete grade and requires an extension date.",
                                "studentTranscriptGradesAdjustments.detail.incompleteGrade.extensionDate", studentTranscriptGradesAdjustmentsDto.Id, studentTranscriptGradesAdjustmentsId);
                        }
                    }
                }
                if (detail.IncompleteGrade != null && detail.IncompleteGrade.FinalGrade != null && !string.IsNullOrEmpty(detail.IncompleteGrade.FinalGrade.Id))
                {
                    var gradeCodeEntity = gradeDefinitions.FirstOrDefault(gd => gd.Guid.Equals(detail.IncompleteGrade.FinalGrade.Id, StringComparison.OrdinalIgnoreCase));
                    if (gradeCodeEntity == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Grade definition not found for Guid: {0}", detail.IncompleteGrade.FinalGrade.Id),
                            detail.IncompleteGrade.FinalGrade.Id, studentTranscriptGradesAdjustmentsDto.Id, studentTranscriptGradesAdjustmentsId, HttpStatusCode.NotFound);

                    }
                    else if (gradeCodeEntity.Id != incompleteGradeAllowed)
                    {
                        if (string.IsNullOrEmpty(incompleteGradeAllowed))
                        {
                             IntegrationApiExceptionAddError("Incomplete final grade is only allowed for incomplete grades.",
                                 guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId);
                        }
                        else
                        {
                           IntegrationApiExceptionAddError("Incomplete Final grade does not match the grades definition for the incomplete grade being assigned.",
                                 guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId);
                        }
                    }
                    studentTranscriptGradesAdjustment.IncompleteGrade = (gradeCodeEntity != null) ? gradeCodeEntity.Id : null;
                }
                if (detail.IncompleteGrade != null)
                {
                    if (string.IsNullOrEmpty(incompleteGradeAllowed) && detail.IncompleteGrade.ExtensionDate != null)
                    {
                        IntegrationApiExceptionAddError("Extension date is only allowed for incomplete grades.",
                                 guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId);
                    }
                    if (string.IsNullOrEmpty(incompleteGradeAllowed) && detail.IncompleteGrade.FinalGrade != null && !string.IsNullOrEmpty(detail.IncompleteGrade.FinalGrade.Id))
                    {
                        IntegrationApiExceptionAddError("Incomplete final grade is only allowed for incomplete grades.",
                                 guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId);
                    }
                    if (detail.IncompleteGrade != null && detail.IncompleteGrade.ExtensionDate != null)
                        studentTranscriptGradesAdjustment.ExtensionDate = detail.IncompleteGrade.ExtensionDate;
                }
                if (detail.ChangeReason != null && !string.IsNullOrEmpty(detail.ChangeReason.Id))
                {
                    var changeReasons = await GetGradeChangeReasonsAsync(bypassCache);
                    if (changeReasons == null)
                    {
                        IntegrationApiExceptionAddError("Grade Change Reasons table not found. Grades change reason cannot be validated.",
                                guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId, httpStatusCode: HttpStatusCode.NotFound);
                    }
                    else
                    {
                        var changeReasonEntity = changeReasons.FirstOrDefault(cr => cr.Guid.Equals(detail.ChangeReason.Id, StringComparison.OrdinalIgnoreCase));
                        if (changeReasonEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Grade change reason not found for Guid: {0}", detail.ChangeReason.Id),
                                guid: studentTranscriptGradesAdjustmentsDto.Id, id: studentTranscriptGradesAdjustmentsId, httpStatusCode: HttpStatusCode.NotFound);
                        }
                        else
                        {
                            studentTranscriptGradesAdjustment.ChangeReason = changeReasonEntity.Code;
                        }
                    }
                }
                
                if (IntegrationApiException != null)
                    throw IntegrationApiException;
            }

            return studentTranscriptGradesAdjustment;
        }

        /// <summary>
        /// Converts a StudentTranscriptGrades domain entity to a collection of StudentTranscriptGradesChangeDetails DTO objects
        /// </summary>
        /// <param name="source">StudentTranscriptGrades domain entity</param>
        /// <param name="bypassCache">bypassCache flag. Defaulted to false</param>
        /// <returns></returns>
        private async Task<List<StudentTranscriptGradesChangeDetails>> ConvertEntityToStudentTranscriptGradesChangeDetailsCollectionAsync
            (List<Domain.Student.Entities.StudentTranscriptGradesHistory> studentTranscriptGradesHistory, string guid, string id, bool bypassCache = false)
        {
            if ((studentTranscriptGradesHistory == null) || (!studentTranscriptGradesHistory.Any()))
            {
                return null;
            }

            var changeDetails = new List<StudentTranscriptGradesChangeDetails>();

            foreach (var history in studentTranscriptGradesHistory)
            {
                var gradeSchemeCode = await GetGradeSchemeCodeAsync(history.PreviousVerifiedGradeValue, guid, id, bypassCache);

                changeDetails.Add(new StudentTranscriptGradesChangeDetails()
                {
                    RecordedOn = history.GradeChangeDate,
                    AwardGradeScheme = await ConvertGradeSchemesCodeToGuidObjectAsync(gradeSchemeCode, guid, id, bypassCache),
                    ChangeReason = await ConvertGradeChangeReasonsToGuidObjectAsync(history.GradeChangeReason, guid, id, bypassCache),
                    Grade = await ConvertVerifiedGradeToGuidObjectAsync(history.PreviousVerifiedGradeValue, guid, id, bypassCache)
                });
            }

            return changeDetails;
        }

        /// <summary>
        /// Converts a StudentTranscriptGrades domain entity to a StudentTranscriptGradesCreditDtoProperty
        /// </summary>
        /// <param name="source">StudentTranscriptGrades domain entity</param>
        /// <returns></returns>
        private Dtos.StudentTranscriptGradesCreditDtoProperty ConvertEntityToGradesCredit
         (Domain.Student.Entities.StudentTranscriptGrades source)
        {
            var studentTranscriptGradesCreditDtoProperty = new Dtos.StudentTranscriptGradesCreditDtoProperty();

            var attemptedCredit = source.AttemptedCredit.HasValue && source.AttemptedCredit.Value > 0
                ? source.AttemptedCredit.Value
                : (source.AttemptedCeus.HasValue && source.AttemptedCeus.Value > 0 ? source.AttemptedCeus.Value : 0);
            var completedCredit = source.CompletedCredit.HasValue && source.CompletedCredit.Value > 0
                ? source.CompletedCredit.Value
                : (source.CompletedCeus.HasValue && source.CompletedCeus.Value > 0 ? source.CompletedCeus.Value : 0);

            if (attemptedCredit > 0)
                studentTranscriptGradesCreditDtoProperty.AttemptedCredit = attemptedCredit;

            if (completedCredit > 0)
                studentTranscriptGradesCreditDtoProperty.EarnedCredit = completedCredit;

            var qualityPoint = new Dtos.DtoProperties.StudentTranscriptGradesQualityPointDtoProperty();

            decimal? gpa = null;
            if (source.StwebTranAltcumFlag)
                gpa = source.AltcumContribGradePts.HasValue ? source.AltcumContribGradePts : 0;
            else
                gpa = source.ContribGradePoints.HasValue ? source.ContribGradePoints : 0;

            var nonWeighted = source.GradePoints.HasValue ? source.GradePoints : 0;
            if ((gpa != null && gpa > 0) || nonWeighted > 0)
            {
                if (gpa > 0)
                    qualityPoint.Gpa = gpa;
                if (nonWeighted > 0)
                    qualityPoint.NonWeighted = nonWeighted;
                studentTranscriptGradesCreditDtoProperty.QualityPoint = qualityPoint;
            }

            studentTranscriptGradesCreditDtoProperty.RepeatedSection = GetStudentTranscriptGradesRepeatedSection(source);

            if (!studentTranscriptGradesCreditDtoProperty.AttemptedCredit.HasValue
                && !studentTranscriptGradesCreditDtoProperty.EarnedCredit.HasValue
                && studentTranscriptGradesCreditDtoProperty.QualityPoint == null
                && studentTranscriptGradesCreditDtoProperty.RepeatedSection == Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.NotSet)
            {
                return null;
            }

            return studentTranscriptGradesCreditDtoProperty;
        }

        /// <summary>
        ///  Get a StudentTranscriptGradesRepeatedSection enumeration from the StudentTranscriptGrades domain entity
        /// </summary>
        /// <param name="source">StudentTranscriptGrades domain entity</param>
        /// <returns>StudentTranscriptGradesRepeatedSection enumeration property</returns>
        private Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection GetStudentTranscriptGradesRepeatedSection
            (Domain.Student.Entities.StudentTranscriptGrades source)
        {
            if (source == null)
            {
                return Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.NotSet;
            }

            if (string.IsNullOrEmpty(source.ReplCode)
                && ((source.RepeatAcademicCreditIds == null) || (!source.RepeatAcademicCreditIds.Any())))
                return Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.NotRepeated;

            var retVal = Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.NotSet;

            decimal? cmplCredits = null;
            decimal? gpaCredits = null;

            if (source.StwebTranAltcumFlag)
            {
                if (source.AltcumContribCmplCredits.HasValue)
                    cmplCredits = source.AltcumContribCmplCredits.Value;
                if (source.AltcumContribGpaCredits.HasValue)
                    gpaCredits = source.AltcumContribGpaCredits.Value;
            }
            else
            {
                if (source.ContribCmplCredits.HasValue)
                    cmplCredits = source.ContribCmplCredits.Value;
                if (source.ContribGradePoints.HasValue)
                    gpaCredits = source.ContribGradePoints.Value;
            }

            if ((!string.IsNullOrEmpty(source.ReplCode))
                && (cmplCredits.HasValue) && (gpaCredits.HasValue))
            {
                if ((cmplCredits == 0) && (gpaCredits == 0))
                {
                    return Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.RepeatedIncludeNeither;
                }
                if ((cmplCredits != 0) && (gpaCredits != 0))
                {
                    return Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.RepeatedIncludeBoth;
                }
                if ((cmplCredits != 0) && (gpaCredits == 0))
                {
                    return Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.RepeatedIncludeCredit;
                }
                if ((cmplCredits == 0) && (gpaCredits != 0))
                {
                    return Dtos.EnumProperties.StudentTranscriptGradesRepeatedSection.RepeatedIncludeQualityPoints;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Convert a StudentTranscriptGrades domain entity to a StudentTranscriptGradesIncompleteGradeDtoProperty
        /// </summary>
        /// <param name="source">StudentTranscriptGrades domain entity</param>
        /// <param name="bypassCache">bypassCache flag.  Defaulted to false</param>
        /// <returns>StudentTranscriptGradesIncompleteGradeDtoProperty</returns>
        private async Task<Dtos.StudentTranscriptGradesIncompleteGradeDtoProperty> ConvertEntityToIncompleteGradeGuidObjectAsync
           (Domain.Student.Entities.StudentTranscriptGrades source, bool bypassCache = false)
        {
            if ((source != null) && (!source.FinalGradeExpirationDate.HasValue))
            {
                return null;
            }

            var studentTranscriptGradesIncompleteGrade = new Dtos.StudentTranscriptGradesIncompleteGradeDtoProperty();

            studentTranscriptGradesIncompleteGrade.ExtensionDate = source.FinalGradeExpirationDate.Value;

            //If the GRADES record for STC.VERIFIED.GRADE has a value in GRD.INCOMPLETE.GRADE then publish the incomplete grade.
            var incompleteGrade = string.Empty;

            if (!string.IsNullOrEmpty(source.VerifiedGrade))
            {

                var entity = await this.GetGradeDefinitionsAsync(bypassCache);
                if (entity == null || !entity.Any())
                {
                    IntegrationApiExceptionAddError("Grade definitions are not defined.",
                                 guid: source.Guid, id: source.Id);
                }
                else
                {
                    var gradeDefinitions = entity.FirstOrDefault(i => i.Id.Equals(source.VerifiedGrade, StringComparison.OrdinalIgnoreCase));
                    if (gradeDefinitions == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Grade definition not found for key: '{0}'.", source.VerifiedGrade),
                                    guid: source.Guid, id: source.Id, httpStatusCode: HttpStatusCode.NotFound);
                    }

                    if (gradeDefinitions != null && !string.IsNullOrEmpty(gradeDefinitions.IncompleteGrade))
                    {
                        // Now get the Incomplete Final Grade if they don't meet the extension date.
                        var incompleteGradeDefiniton = entity.FirstOrDefault(i => i.Id.Equals(gradeDefinitions.IncompleteGrade, StringComparison.OrdinalIgnoreCase));
                        if (incompleteGradeDefiniton == null)
                        {
                             IntegrationApiExceptionAddError(string.Format("Grade definition not found for key: '{0}'.", gradeDefinitions.IncompleteGrade),
                                   guid: source.Guid, id: source.Id, httpStatusCode: HttpStatusCode.NotFound);
                        }
                        studentTranscriptGradesIncompleteGrade.FinalGrade = new GuidObject2(incompleteGradeDefiniton.Guid);
                    }
                }
            }

            return studentTranscriptGradesIncompleteGrade;
        }

        /// <summary>
        /// Convert course to studentTranscriptGradesCourseDtoProperty
        /// The course object within the student-transcript-grades schema structure is a oneOf where we should take a 
        ///      hierarchical approach:
        /// If the section is available to reference, use the course.section.id option.
        ///     else, if the course is available use the course.detail.id option.
        ///     else use the course.name option.
        /// </summary>
        /// <param name="source">StudentTranscriptGrades domain object</param>
        /// <param name="sectionGuidCollection">dictionary of associated sections contaning association of ids and guids</param>
        /// <param name="courseGuidCollection">dictionary of associated cources contaning association of ids and guids</param>
        /// <returns></returns>
        private Dtos.StudentTranscriptGradesCourseDtoProperty ConvertCourseToStudentTranscriptGradesCourse
            (Domain.Student.Entities.StudentTranscriptGrades source,
            Dictionary<string, string> sectionGuidCollection,
            Dictionary<string, string> courseGuidCollection)
        {
            if ((source == null) || (sectionGuidCollection == null) || (courseGuidCollection == null))
            {
                return null;
            }

            var sectionGuid = string.Empty;
            var courseGuid = string.Empty;

            if (!string.IsNullOrEmpty(source.CourseSection))
            {
                sectionGuidCollection.TryGetValue(source.CourseSection, out sectionGuid);
                if (string.IsNullOrEmpty(sectionGuid))
                {
                     IntegrationApiExceptionAddError(string.Format("Unable to locate guid for student course section '{0}'", source.CourseSection),
                             guid: source.Guid, id: source.Id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            else if (!string.IsNullOrEmpty(source.Course))
            {
                courseGuidCollection.TryGetValue(source.Course, out courseGuid);
                if (string.IsNullOrEmpty(courseGuid))
                {
                     IntegrationApiExceptionAddError(string.Format("Unable to locate guid for course '{0}'", source.Course),
                             guid: source.Guid, id: source.Id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            var studentTranscriptGradesCourse = new Dtos.StudentTranscriptGradesCourseDtoProperty();

            if (!string.IsNullOrEmpty(sectionGuid))
            {
                studentTranscriptGradesCourse.Section = new GuidObject2(sectionGuid);
            }
            else if (!string.IsNullOrEmpty(courseGuid))
            {
                studentTranscriptGradesCourse.Detail = new GuidObject2(courseGuid);
            }
            else
            {
                studentTranscriptGradesCourse.Name = !string.IsNullOrEmpty(source.CourseName) ? source.CourseName : source.Title;
            }
            return studentTranscriptGradesCourse;
        }

        /// <summary>
        /// Convert a studentID to a GuidObject
        /// </summary>
        /// <param name="studentId">a studentID</param>
        /// <param name="personGuidCollection">a dictionary of associated person guids and ids</param>
        /// <returns>guidObject</returns>
        private GuidObject2 ConvertStudentIdToGuidObject(string studentId, Dictionary<string, string> personGuidCollection, string guid, string id)
        {
            if (studentId == null)
            {
                return null;
            }
            var personGuid = string.Empty;

            if (personGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for student ID : '{0}'", studentId),
                           guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
            }
            else
            {
               
                personGuidCollection.TryGetValue(studentId, out personGuid);
                if (string.IsNullOrEmpty(personGuid))
                {
                     IntegrationApiExceptionAddError(string.Format("Unable to locate guid for student ID : '{0}'", studentId),
                               guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (string.IsNullOrEmpty(personGuid)) ? null : new GuidObject2(personGuid);
        }

        /// <summary>
        /// Convert a gradeSchemeCode to a guidObject
        /// </summary>
        /// <param name="gradeSchemeCode">gradeSchemeCode</param>
        /// <param name="guid">associated entities guid. Used to build error message if necessary</param>
        /// <param name="bypassCache">bypassCache flag.  Defaulted to false</param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertGradeSchemesCodeToGuidObjectAsync(string gradeSchemeCode, string guid = "", string id = "",
            bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(gradeSchemeCode))
            {
                return null;
            }
            Domain.Student.Entities.GradeScheme gradeScheme = null;
            var entity = await this.GetGradeSchemesAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                IntegrationApiExceptionAddError("Grade schemes are not defined",
                            guid: guid, id: id);
            }
            else
            {
                gradeScheme = entity.FirstOrDefault(i => i.Code.Equals(gradeSchemeCode, StringComparison.OrdinalIgnoreCase));
                if (gradeScheme == null)
                {
                   IntegrationApiExceptionAddError(string.Format("Grade Scheme not found for key: '{0}'.", gradeSchemeCode),
                               guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (gradeScheme == null) ? null : new GuidObject2(gradeScheme.Guid);
        }

        private GuidObject2 ConvertStudentCourseSecToUnverifiedGradeGuidObject(string studentCourseSecId,
            Dictionary<string, string> studentCourseSecGuidCollection, string guid, string id)
        {
            if (string.IsNullOrEmpty(studentCourseSecId))
            {
                return null;
            }
            var unverifiedGradeGuid = string.Empty;
            if (studentCourseSecGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for studentCourseSec id : '{0}'", studentCourseSecId),
                            guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
            }
            else
            {
                
                studentCourseSecGuidCollection.TryGetValue(studentCourseSecId, out unverifiedGradeGuid);
                if (string.IsNullOrEmpty(unverifiedGradeGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for unverified grade id : '{0}'", studentCourseSecId),
                                guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (string.IsNullOrEmpty(unverifiedGradeGuid)) ? null : new GuidObject2(unverifiedGradeGuid);
        }

        private async Task<GuidObject2> ConvertVerifiedGradeToGuidObjectAsync(string verifiedGrade, string guid = "", string id = "",
            bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(verifiedGrade))
            {
                return null;
            }
            Domain.Student.Entities.Grade gradeDefinitions = null;
            var entity = await this.GetGradeDefinitionsAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                IntegrationApiExceptionAddError("Grade definitions are not defined.",
                              guid: guid, id: id);

            }
            else
            {
                gradeDefinitions = entity.FirstOrDefault(i => i.Id.Equals(verifiedGrade, StringComparison.OrdinalIgnoreCase));
                if (gradeDefinitions == null)
                {
                     IntegrationApiExceptionAddError(string.Format("Grade definition not found for key: '{0}'.", verifiedGrade),
                                guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (gradeDefinitions == null) ? null : new GuidObject2(gradeDefinitions.Guid);
        }

        private async Task<GuidObject2> ConvertGradeChangeReasonsToGuidObjectAsync(string changeReason, string guid = "", string id = "",
           bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(changeReason))
                return null;

            Domain.Base.Entities.GradeChangeReason gradeChangeReason = null;
            var gradeChangeReasons = await GetGradeChangeReasonsAsync(bypassCache);
            if (gradeChangeReasons == null)
            {
                IntegrationApiExceptionAddError("Grade Change Reasons table not found. Grades change reason cannot be validated.",
                           guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
            }
            else
            {
                gradeChangeReason = gradeChangeReasons.FirstOrDefault(i => i.Code.Equals(changeReason, StringComparison.OrdinalIgnoreCase));
                if (gradeChangeReason == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Grade change reason not found for key: '{0}'.", changeReason),
                               guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (gradeChangeReason == null) ? null : new GuidObject2(gradeChangeReason.Guid);
        }

        private async Task<string> GetGradeSchemeCodeAsync(string grade, string guid = "", string id = "",
            bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(grade))
            {
                return null;
            }
            Domain.Student.Entities.Grade gradeDefinitions = null;
            var entity = await this.GetGradeDefinitionsAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                 IntegrationApiExceptionAddError("Grade definitions are not defined.",
                           guid: guid, id: id);
            }
            else
            {
                gradeDefinitions = entity.FirstOrDefault(i => i.Id.Equals(grade, StringComparison.OrdinalIgnoreCase));
                if (gradeDefinitions == null)
                {
                     IntegrationApiExceptionAddError(string.Format("Grade definition not found for key: '{0}'.", grade),
                               guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (gradeDefinitions == null) ? null : gradeDefinitions.GradeSchemeCode;
        }

        private async Task<GuidObject2> ConvertEntityToCreditCategoryGuidObjectAsync(Domain.Student.Entities.StudentTranscriptGrades source,
            bool bypassCache = false)
        {
            if (source != null && string.IsNullOrEmpty(source.CreditType))
            {
                return null;
            }
            Domain.Student.Entities.CreditCategory creditCategories = null;
            var entity = await this.GetCreditCategoriesAsync(bypassCache);
            if (entity == null || !entity.Any())
            {
                IntegrationApiExceptionAddError("Credit Categories are not defined.",
                            guid: source.Guid, id: source.Id);
            }
            else
            {
                creditCategories = entity.FirstOrDefault(i => i.Code.Equals(source.CreditType, StringComparison.OrdinalIgnoreCase));
                if (creditCategories == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Credit Categories not found for key: '{0}'.", source.CreditType),
                              guid: source.Guid, id: source.Id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (creditCategories == null) ?  null : new GuidObject2(creditCategories.Guid);
        }

        #endregion

        #region All Reference Data

        /// <summary>
        /// Grade definitions
        /// </summary>
        IEnumerable<Domain.Student.Entities.Grade> _gradeDefinitions = null;
        private async Task<IEnumerable<Domain.Student.Entities.Grade>> GetGradeDefinitionsAsync(bool bypassCache)
        {
            return _gradeDefinitions ?? (_gradeDefinitions = await _gradeRepository.GetHedmAsync(bypassCache));
        }

        /// <summary>
        /// Grade change reasons
        /// </summary>
        IEnumerable<Domain.Base.Entities.GradeChangeReason> _gradeChangeReasons = null;
        private async Task<IEnumerable<Domain.Base.Entities.GradeChangeReason>> GetGradeChangeReasonsAsync(bool bypassCache)
        {
            return _gradeChangeReasons ?? (_gradeChangeReasons = await _referenceDataRepository.GetGradeChangeReasonAsync(bypassCache));
        }

        /// <summary>
        /// Grade schemes
        /// </summary>
        IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GetGradeSchemesAsync(bool bypassCache)
        {
            return _gradeSchemes ?? (_gradeSchemes = await _studentReferenceDataRepository.GetGradeSchemesAsync(bypassCache));
        }

        /// <summary>
        /// Credit Categories
        /// </summary>
        IEnumerable<Domain.Student.Entities.CreditCategory> _creditCategories = null;
        private async Task<IEnumerable<Domain.Student.Entities.CreditCategory>> GetCreditCategoriesAsync(bool bypassCache)
        {
            return _creditCategories ?? (_creditCategories = await _studentReferenceDataRepository.GetCreditCategoriesAsync(bypassCache));
        }

        #endregion

        #region Permission Check

        /// <summary>
        /// Permissions code that allows an external system to perform the READ operation.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckViewStudentTranscriptGradesPermission()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.ViewStudentTranscriptGrades) || userPermissions.Contains(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update StudentTranscriptGradesAdjustments.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateStudentTranscriptGradesAdjustmentsPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments);

            // User is not allowed to create or update StudentTranscriptGradesAdjustments without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update StudentTranscriptGradesAdjustments.");
            }
        }

        #endregion
    }
}