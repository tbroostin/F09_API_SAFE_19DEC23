//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;


namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentCourseTransfersService : BaseCoordinationService, IStudentCourseTransferService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private IEnumerable<Dtos.Student.AcademicLevel> allAcadLevels;
        private readonly IGradeRepository _gradeRepository;
        private readonly ITermRepository _termRepository;
        private readonly ICourseRepository _courseRepository;

        private IEnumerable<Domain.Student.Entities.Grade> _grades = null;
        private IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;
        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;
        private IEnumerable<Domain.Student.Entities.AcademicProgram> _academicPrograms = null;
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods = null;
        private IEnumerable<Domain.Student.Entities.CreditCategory> _creditCategories = null;

        public StudentCourseTransfersService(

            IPersonRepository personRepository,
            IAcademicCreditRepository academicCreditRepository,
            ICourseRepository courseRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IGradeRepository gradeRepository,
            ITermRepository termRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _personRepository = personRepository;
            _academicCreditRepository = academicCreditRepository;
            _courseRepository = courseRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _gradeRepository = gradeRepository;
            _termRepository = termRepository;            
        }

        private IEnumerable<Domain.Student.Entities.SectionRegistrationStatusItem> _transferStatuses = null;

        private async Task<IEnumerable<Domain.Student.Entities.SectionRegistrationStatusItem>> GetStudentAcademicCreditStatusesAsync(bool bypassCache)
        {
            if (_transferStatuses == null)
            {
                _transferStatuses = await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesAsync(bypassCache);
            }
            return _transferStatuses;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-course-transfers
        /// </summary>
        /// <returns>Collection of StudentCourseTransfers DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>> GetStudentCourseTransfersAsync(int offset, int limit, bool bypassCache)
        {
            await CheckViewPermission();

            var studentCourseTransfersCollection = new List<Dtos.StudentCourseTransfer>();
            var studentCourseTransfersData = await _academicCreditRepository.GetStudentCourseTransfersAsync(offset, limit, bypassCache);
            var studentCourseTransfersEntities = studentCourseTransfersData.Item1;

            if (studentCourseTransfersEntities != null && studentCourseTransfersEntities.Any())
            {
                studentCourseTransfersCollection = (await ConvertStudentCourseTransfersEntityToDto(studentCourseTransfersEntities, bypassCache)).ToList();
                //studentCourseTransfersCollection = (await ConvertStudentCourseTransfers2EntityToDto(studentCourseTransfersEntities, bypassCache)).ToList();
            }
            return new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, studentCourseTransfersData.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentCourseTransfers from its GUID
        /// </summary>
        /// <returns>StudentCourseTransfers DTO object</returns>
        public async Task<Dtos.StudentCourseTransfer> GetStudentCourseTransferByGuidAsync(string guid, bool bypassCache = false)
        {

            await CheckViewPermission();

            try
            {
                var studentCourseTransferEntities = new List<Domain.Student.Entities.StudentCourseTransfer>();
                var studentCourseTransferEntity = await _academicCreditRepository.GetStudentCourseTransferByGuidAsync(guid, bypassCache);
                studentCourseTransferEntities.Add(studentCourseTransferEntity);
                return ((await ConvertStudentCourseTransfersEntityToDto(studentCourseTransferEntities, bypassCache)).ToList()).FirstOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-course-transfers not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("student-course-transfers not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-course-transfers
        /// </summary>
        /// <returns>Collection of StudentCourseTransfers DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>> GetStudentCourseTransfers2Async(int offset, int limit, bool bypassCache)
        {
            await CheckViewPermission();

            var studentCourseTransfersCollection = new List<Dtos.StudentCourseTransfer>();
            var studentCourseTransfersData = await _academicCreditRepository.GetStudentCourseTransfersAsync(offset, limit, bypassCache);
            var studentCourseTransfersEntities = studentCourseTransfersData.Item1;
            
            if (studentCourseTransfersEntities != null && studentCourseTransfersEntities.Any())
            {
                studentCourseTransfersCollection = (await ConvertStudentCourseTransfers2EntityToDto(studentCourseTransfersEntities, bypassCache)).ToList();
            }
            return new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, studentCourseTransfersData.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentCourseTransfers from its GUID
        /// </summary>
        /// <returns>StudentCourseTransfers DTO object</returns>
        public async Task<Dtos.StudentCourseTransfer> GetStudentCourseTransfer2ByGuidAsync(string guid, bool bypassCache = false)
        {

            await CheckViewPermission();

            try
            {
                var studentCourseTransferEntities = new List<Domain.Student.Entities.StudentCourseTransfer>();
                var studentCourseTransferEntity = await _academicCreditRepository.GetStudentCourseTransferByGuidAsync(guid, bypassCache);
                studentCourseTransferEntities.Add(studentCourseTransferEntity);
                return ((await ConvertStudentCourseTransfers2EntityToDto(studentCourseTransferEntities, bypassCache)).ToList()).FirstOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-course-transfers not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("student-course-transfers not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentAcadCred domain entity to its corresponding StudentCourseTransfers DTO
        /// </summary>
        /// <param name="source">StudentAcadCred domain entity</param>
        /// <returns>StudentCourseTransfers DTO</returns>
        private async Task<IEnumerable<Dtos.StudentCourseTransfer>> ConvertStudentCourseTransfersEntityToDto(IEnumerable<Domain.Student.Entities.StudentCourseTransfer> sources, bool bypassCache)
        {
            // Perform bulk reads of GUIDs that will be needed
            var studentIds = sources
                 .Where(x => (!string.IsNullOrEmpty(x.Student)))
                 .Select(x => x.Student).Distinct().ToList();
            var studentGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(studentIds);

            var institutionIds = sources
                 .Where(x => (!string.IsNullOrEmpty(x.TransferredFromInstitution)))
                 .Select(x => x.TransferredFromInstitution).Distinct().ToList();
            var institutionGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(institutionIds);

            var courseIds = sources
                 .Where(x => (!string.IsNullOrEmpty(x.Course)))
                 .Select(x => x.Course).Distinct().ToList();
            var courseGuidCollection = await this._courseRepository.GetGuidsCollectionAsync(courseIds, "COURSES");

            var scts = new List<Dtos.StudentCourseTransfer>();

            foreach (var source in sources)
            {
                var sct = new Dtos.StudentCourseTransfer();

                // Id
                sct.Id = source.Guid;

                // AcademicLevel
                if (!string.IsNullOrEmpty(source.AcademicLevel))
                {
                    var academicLevels = await GetAcademicLevelsAsync(bypassCache);
                    if (academicLevels != null)
                    {
                        var academicLevel = academicLevels.FirstOrDefault(t => t.Code == source.AcademicLevel);
                        if (academicLevel != null)
                        {
                            sct.AcademicLevel = new GuidObject2(academicLevel.Guid);
                        }
                    }
                }

                // AcademicPeriod
                if (!string.IsNullOrEmpty(source.AcademicPeriod))
                {
                    var academicPeriods = await GetAcademicPeriodsAsync(bypassCache);
                    if (academicPeriods != null)
                    {
                        var academicPeriod = academicPeriods.FirstOrDefault(t => t.Code == source.AcademicPeriod);
                        if (academicPeriod != null)
                        {
                            sct.AcademicPeriod = new GuidObject2(academicPeriod.Guid);
                        }
                    }
                }

                // AcademicPrograms
                if (source.AcademicPrograms != null && source.AcademicPrograms.Count() > 0)
                {
                    sct.AcademicPrograms = new List<GuidObject2>();
                    var academicPrograms = await GetAcademicProgramsAsync(bypassCache);
                    if (academicPrograms != null)
                    {
                        foreach (var prog in source.AcademicPrograms)
                        {
                            var academicProgram = academicPrograms.FirstOrDefault(t => t.Code == prog);
                            if (academicProgram != null)
                            {
                                sct.AcademicPrograms.Add(new GuidObject2(academicProgram.Guid));
                            }
                        }
                    }
                }

                // AwardGradeScheme
                if (!string.IsNullOrEmpty(source.GradeScheme))
                {
                    var gradeSchemes = await GetGradeSchemesAsync(bypassCache);
                    if (gradeSchemes != null)
                    {
                        var gradeScheme = gradeSchemes.FirstOrDefault(t => t.Code == source.GradeScheme);
                        if (gradeScheme != null)
                        {
                            sct.AwardGradeScheme = new GuidObject2(gradeScheme.Guid);
                        }
                    }
                }

                // Grade
                if (!string.IsNullOrEmpty(source.Grade))
                {
                    var grades = await GetGradesAsync(bypassCache);
                    if (grades != null)
                    {
                        var grade = (await GetGradesAsync(bypassCache)).FirstOrDefault(t => t.Id == source.Grade);
                        if (grade != null)
                        {
                            sct.Grade = new GuidObject2(grade.Guid);
                        }
                    }
                }

                sct.Credit = new Dtos.DtoProperties.StudentCourseTransferCreditDtoProperty();

                // Credit.AwardedCredit
                sct.Credit.AwardedCredit = source.AwardedCredit;

                sct.Credit.CreditCategory = new Dtos.DtoProperties.StudentCourseTransferCreditCategoryDtoProperty();

                // Credit.Measure
                // Per spec sheet:
                // In Colleague, credits that are awarded for a course equivalency are always stored 
                // in STC.CRED and are assumed to be credits(i.e.not CEUs).As a result, when publishing
                // the STC.CRED.TYPE, always publish the enumeration "credit" here.
                sct.Credit.Measure = StudentCourseTransferMeasure.Credit;
                // sct.Credit.Measure = (StudentCourseTransferMeasure) Enum.Parse(typeof(StudentCourseTransferMeasure),source.studentCourseTransferMeasure);

                // Credit.CreditCategory.CreditType
                sct.Credit.CreditCategory.CreditType = (StudentCourseTransferCreditType)Enum.Parse(typeof(StudentCourseTransferCreditType),
                    (await _academicCreditRepository.GetCreditTypeAsync(source.CreditType)).ToString());

                // Credit.CreditCategory.Detail;
                if (!string.IsNullOrEmpty(source.CreditType))
                {
                    var creditCategories = await GetCreditCategoriesAsync(bypassCache);
                    if (creditCategories != null)
                    {
                        var creditCategory = creditCategories.FirstOrDefault(t => t.Code == source.CreditType);
                        if (creditCategory != null)
                        {
                            sct.Credit.CreditCategory.Detail = new GuidObject2(creditCategory.Guid);
                        }
                    }
                }                

                // EquivalencyAppliedOn
                if (source.EquivalencyAppliedOn != null)
                {
                    sct.EquivalencyAppliedOn = (DateTime)source.EquivalencyAppliedOn;
                }
                
                // EquivalentCourse
                if (string.IsNullOrEmpty(source.Course))
                {
                    throw new ArgumentNullException("EquivalentCourse is required. ");
                }
                if (courseGuidCollection == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for course ID : {0}", source.Course));
                }
                var courseGuid = string.Empty;
                courseGuidCollection.TryGetValue(source.Course, out courseGuid);
                if (string.IsNullOrEmpty(courseGuid))
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for studentAcadCred ID : {0}", source.Course));
                }
                sct.EquivalentCourse = new GuidObject2(courseGuid);

                // QualityPoints
                if (source.QualityPoints != null) sct.QualityPoints = source.QualityPoints;

                // Student
                if (string.IsNullOrEmpty(source.Student))
                {
                    throw new ArgumentNullException("Student is required. ");
                }
                if (studentGuidCollection == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for student ID : {0}", source.Student));
                }
                var studentGuid = string.Empty;
                studentGuidCollection.TryGetValue(source.Student, out studentGuid);
                if (string.IsNullOrEmpty(studentGuid))
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for student ID : {0}", source.Student));
                }
                sct.Student = new GuidObject2(studentGuid);

                // TransferredFrom (institution)
                if (string.IsNullOrEmpty(source.TransferredFromInstitution))
                {
                    throw new ArgumentNullException("TransferredFrom is required. ");
                }
                if (institutionGuidCollection == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for TransferredFrom Institution ID : {0}", source.TransferredFromInstitution));
                }
                var institutionGuid = string.Empty;
                institutionGuidCollection.TryGetValue(source.TransferredFromInstitution, out institutionGuid);
                if (string.IsNullOrEmpty(institutionGuid))
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for TransferredFrom Institution ID : {0}", source.TransferredFromInstitution));
                }
                sct.TransferredFrom = new GuidObject2(institutionGuid);

                scts.Add(sct);
            }            
            return scts;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentAcadCred domain entity to its corresponding StudentCourseTransfers DTO
        /// </summary>
        /// <param name="source">StudentAcadCred domain entity</param>
        /// <returns>StudentCourseTransfers DTO</returns>
        private async Task<IEnumerable<Dtos.StudentCourseTransfer>> ConvertStudentCourseTransfers2EntityToDto(IEnumerable<Domain.Student.Entities.StudentCourseTransfer> sources, bool bypassCache = false)
        {
            // Perform bulk reads of GUIDs that will be needed
            var studentIds = sources
                 .Where(x => (!string.IsNullOrEmpty(x.Student)))
                 .Select(x => x.Student).Distinct().ToList();
            var studentGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(studentIds);

            var institutionIds = sources
                 .Where(x => (!string.IsNullOrEmpty(x.TransferredFromInstitution)))
                 .Select(x => x.TransferredFromInstitution).Distinct().ToList();
            var institutionGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(institutionIds);

            var courseIds = sources
                 .Where(x => (!string.IsNullOrEmpty(x.Course)))
                 .Select(x => x.Course).Distinct().ToList();
            var courseGuidCollection = await this._courseRepository.GetGuidsCollectionAsync(courseIds, "COURSES");

            var scts = new List<Dtos.StudentCourseTransfer>();                        

            foreach (var source in sources)
            {
                var sct = new Dtos.StudentCourseTransfer();

                // Id
                sct.Id = source.Guid;

                // AcademicLevel
                if (!string.IsNullOrEmpty(source.AcademicLevel))
                {
                    var academicLevels = await GetAcademicLevelsAsync(bypassCache);
                    if (academicLevels != null)
                    {
                        var academicLevel = academicLevels.FirstOrDefault(t => t.Code == source.AcademicLevel);
                        if (academicLevel != null)
                        {
                            sct.AcademicLevel = new GuidObject2(academicLevel.Guid);
                        }
                    }
                }

                // AcademicPeriod
                if (!string.IsNullOrEmpty(source.AcademicPeriod))
                {
                    var academicPeriods = await GetAcademicPeriodsAsync(bypassCache);
                    if (academicPeriods != null)
                    {
                        var academicPeriod = academicPeriods.FirstOrDefault(t => t.Code == source.AcademicPeriod);
                        if (academicPeriod != null)
                        {
                            sct.AcademicPeriod = new GuidObject2(academicPeriod.Guid);
                        }
                    }
                }

                // AcademicPrograms
                if (source.AcademicPrograms != null && source.AcademicPrograms.Count() > 0)
                {
                    sct.AcademicPrograms = new List<GuidObject2>();
                    var academicPrograms = await GetAcademicProgramsAsync(bypassCache);
                    if (academicPrograms != null)
                    {
                        foreach (var prog in source.AcademicPrograms)
                        {
                            var academicProgram = academicPrograms.FirstOrDefault(t => t.Code == prog);
                            if (academicProgram != null)
                            {
                                sct.AcademicPrograms.Add(new GuidObject2(academicProgram.Guid));
                            }
                        }
                    }
                }

                // AwardGradeScheme
                if (!string.IsNullOrEmpty(source.GradeScheme))
                {
                    var gradeSchemes = await GetGradeSchemesAsync(bypassCache);
                    if (gradeSchemes != null)
                    {
                        var gradeScheme = gradeSchemes.FirstOrDefault(t => t.Code == source.GradeScheme);
                        if (gradeScheme != null)
                        {
                            sct.AwardGradeScheme = new GuidObject2(gradeScheme.Guid);
                        }
                    }
                }

                // Grade
                if (!string.IsNullOrEmpty(source.Grade))
                {
                    var grades = await GetGradesAsync(bypassCache);
                    if (grades != null)
                    {
                        var grade = (await GetGradesAsync(bypassCache)).FirstOrDefault(t => t.Id == source.Grade);
                        if (grade != null)
                        {
                            sct.Grade = new GuidObject2(grade.Guid);
                        }
                    }
                }
                                
                sct.Credit = new Dtos.DtoProperties.StudentCourseTransferCreditDtoProperty();

                // Credit.AwardedCredit
                sct.Credit.AwardedCredit = source.AwardedCredit;
                                
                // Credit.Measure                
                // Per spec sheet:
                // In Colleague, credits that are awarded for a course equivalency are always stored 
                // in STC.CRED and are assumed to be credits(i.e.not CEUs).As a result, when publishing
                // the STC.CRED.TYPE, always publish the enumeration "credit" here.
                sct.Credit.Measure = StudentCourseTransferMeasure.Credit;
                // sct.Credit.Measure = (StudentCourseTransferMeasure) Enum.Parse(typeof(StudentCourseTransferMeasure),source.studentCourseTransferMeasure);

                sct.Credit.CreditCategory = new Dtos.DtoProperties.StudentCourseTransferCreditCategoryDtoProperty();

                // Credit.CreditCategory.CreditType
                sct.Credit.CreditCategory.CreditType = (StudentCourseTransferCreditType)Enum.Parse(typeof(StudentCourseTransferCreditType),
                    (await _academicCreditRepository.GetCreditTypeAsync(source.CreditType)).ToString());

                // Credit.CreditCategory.Detail
                if (!string.IsNullOrEmpty(source.CreditType))
                {
                    var creditCategories = await GetCreditCategoriesAsync(bypassCache);
                    if (creditCategories != null)
                    {
                        var creditCategory = creditCategories.FirstOrDefault(t => t.Code == source.CreditType);
                        if (creditCategory != null)
                        {
                            sct.Credit.CreditCategory.Detail = new GuidObject2(creditCategory.Guid);
                        }
                    }
                }        

                // EquivalencyAppliedOn
                if (source.EquivalencyAppliedOn != null)
                {
                    sct.EquivalencyAppliedOn = (DateTime)source.EquivalencyAppliedOn;
                }
                
                // EquivalentCourse
                if (string.IsNullOrEmpty(source.Course))
                {
                    throw new ArgumentNullException("EquivalentCourse is required. ");
                }
                if (courseGuidCollection == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for course ID : {0}", source.Course));
                }
                var courseGuid = string.Empty;
                courseGuidCollection.TryGetValue(source.Course, out courseGuid);
                if (string.IsNullOrEmpty(courseGuid))
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for studentAcadCred ID : {0}", source.Course));
                }
                sct.EquivalentCourse = new GuidObject2(courseGuid);

                // QualityPoints
                if (source.QualityPoints != null) sct.QualityPoints = source.QualityPoints;

                // Student
                if (string.IsNullOrEmpty(source.Student))
                {
                    throw new ArgumentNullException("Student is required. ");
                }
                if (studentGuidCollection == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for student ID : {0}", source.Student));
                }
                var studentGuid = string.Empty;
                studentGuidCollection.TryGetValue(source.Student, out studentGuid);
                if (string.IsNullOrEmpty(studentGuid))
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for student ID : {0}", source.Student));
                }
                sct.Student = new GuidObject2(studentGuid);

                // TransferredFrom (institution)
                if (string.IsNullOrEmpty(source.TransferredFromInstitution))
                {
                    throw new ArgumentNullException("TransferredFrom is required. ");
                }
                if (institutionGuidCollection == null)
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for TransferredFrom Institution ID : {0}", source.TransferredFromInstitution));
                }
                var institutionGuid = string.Empty;
                institutionGuidCollection.TryGetValue(source.TransferredFromInstitution, out institutionGuid);
                if (string.IsNullOrEmpty(institutionGuid))
                {
                    throw new KeyNotFoundException(string.Format("Unable to locate guid for TransferredFrom Institution ID : {0}", source.TransferredFromInstitution));
                }
                sct.TransferredFrom = new GuidObject2(institutionGuid);

                // Status
                if (!string.IsNullOrEmpty(source.Status))
                {
                    var status = (await GetStudentAcademicCreditStatusesAsync(false)).FirstOrDefault(s => s.Code.Equals(source.Status, StringComparison.OrdinalIgnoreCase));
                    sct.Status = status != null && !status.Category.Equals(Domain.Student.Entities.CourseTransferStatusesCategory.NotSet) ? new GuidObject2(status.Guid) : null;
                }
                scts.Add(sct);
            }
            return scts;
        }

        /// <summary>
        /// Check view permission
        /// </summary>
        /// <returns></returns>
        private async Task CheckViewPermission()
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            if (!userPermissions.Contains(StudentPermissionCodes.ViewStudentCourseTransfers))
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view student-course-transfers");
            }
        }

        /// <summary>
        /// Get all Grades Entity Objects
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
        /// Get all GradeScheme Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="GradeScheme"> GradeScheme entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GetGradeSchemesAsync(bool bypassCache)
        {
            if (_gradeSchemes == null)
            {
                _gradeSchemes = await _studentReferenceDataRepository.GetGradeSchemesAsync(bypassCache);
            }
            return _gradeSchemes;
        }

        /// <summary>
        /// Get all AcademicLevel Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="AcademicLevel"> AcademicLevel entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> GetAcademicLevelsAsync(bool bypassCache)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            }
            return _academicLevels;
        }

        /// <summary>
        /// Get all AcademicProgram Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="AcademicProgram"> AcademicProgram entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> GetAcademicProgramsAsync(bool bypassCache)
        {
            if (_academicPrograms == null)
            {
                _academicPrograms = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);
            }
            return _academicPrograms;
        }

        /// <summary>
        /// Get all AcademicPeriod Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="AcademicPeriod"> AcademicPeriod entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> GetAcademicPeriodsAsync(bool bypassCache)
        {
            if (_academicPeriods == null)
            {
                var termEntities = await _termRepository.GetAsync(bypassCache);
                if (termEntities != null && termEntities.Any())
                {
                    _academicPeriods = _termRepository.GetAcademicPeriods(termEntities);
                }
            }
            return _academicPeriods;
        }

        /// <summary>
        /// Get all CreditCategory Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.</param>
        /// <returns>A collection of <see cref="CreditCategory"> CreditCategory entity objects</returns>
        private async Task<IEnumerable<Domain.Student.Entities.CreditCategory>> GetCreditCategoriesAsync(bool bypassCache)
        {
            if (_creditCategories == null)
            {
                _creditCategories = await _studentReferenceDataRepository.GetCreditCategoriesAsync();
            }
            return _creditCategories;
        }

    }
}
