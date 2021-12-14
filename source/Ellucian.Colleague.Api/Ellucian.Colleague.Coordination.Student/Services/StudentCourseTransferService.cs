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
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentCourseTransfersService : BaseCoordinationService, IStudentCourseTransferService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
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
            try
            {
                var studentCourseTransferEntities = new List<Domain.Student.Entities.StudentCourseTransfer>();
                var studentCourseTransferEntity = await _academicCreditRepository.GetStudentCourseTransferByGuidAsync(guid, bypassCache);
                studentCourseTransferEntities.Add(studentCourseTransferEntity);
                return ((await ConvertStudentCourseTransfersEntityToDto(studentCourseTransferEntities, bypassCache)).ToList()).FirstOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No student course transfer was found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No student course transfer was found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-course-transfers
        /// </summary>
        /// <returns>Collection of StudentCourseTransfers DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>> GetStudentCourseTransfers2Async(int offset, int limit, bool bypassCache)
        {


            var studentCourseTransfersCollection = new List<Dtos.StudentCourseTransfer>();
            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentCourseTransfer>, int> studentCourseTransfersData = null;
            try
            {
                studentCourseTransfersData = await _academicCreditRepository.GetStudentCourseTransfersAsync(offset, limit, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            var studentCourseTransfersEntities = studentCourseTransfersData.Item1;

            if (studentCourseTransfersEntities != null && studentCourseTransfersEntities.Any())
            {
                studentCourseTransfersCollection = (await ConvertStudentCourseTransfers2EntityToDto(studentCourseTransfersEntities, bypassCache)).ToList();
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
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
            Dtos.StudentCourseTransfer studentCourseTransferDto;
            try
            {
                var studentCourseTransferEntities = new List<Domain.Student.Entities.StudentCourseTransfer>();
                var studentCourseTransferEntity = await _academicCreditRepository.GetStudentCourseTransferByGuidAsync(guid, bypassCache);
                studentCourseTransferEntities.Add(studentCourseTransferEntity);
                studentCourseTransferDto = ((await ConvertStudentCourseTransfers2EntityToDto(studentCourseTransferEntities, bypassCache)).ToList()).FirstOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No student course transfer was found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No student course transfer was found for GUID " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return studentCourseTransferDto;
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
                    try
                    {
                        var acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(source.AcademicLevel);
                        if (!string.IsNullOrEmpty(acadLevel))
                        {
                            sct.AcademicLevel = new Dtos.GuidObject2(acadLevel);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            source.Guid, source.Id);
                    }
                }

                // AcademicPeriod
                if (!string.IsNullOrEmpty(source.AcademicPeriod))
                {
                    try
                    {
                        var acadPeriod = await _termRepository.GetAcademicPeriodsGuidAsync(source.AcademicPeriod);
                        if (!string.IsNullOrEmpty(acadPeriod))
                        {
                            sct.AcademicPeriod = new Dtos.GuidObject2(acadPeriod);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            source.Guid, source.Id);
                    }
                }

                // AcademicPrograms
                if (source.AcademicPrograms != null && source.AcademicPrograms.Count() > 0)
                {
                    sct.AcademicPrograms = new List<GuidObject2>();
                    foreach (var prog in source.AcademicPrograms)
                    {
                        try
                        {
                            var academicProgram = await _studentReferenceDataRepository.GetAcademicProgramsGuidAsync(prog);
                            if (!string.IsNullOrEmpty(academicProgram))
                            {
                                sct.AcademicPrograms.Add(new Dtos.GuidObject2(academicProgram));
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                source.Guid, source.Id);
                        }
                    }
                }

                // AwardGradeScheme
                if (!string.IsNullOrEmpty(source.GradeScheme))
                {
                    try
                    {
                        var gradeScheme = await _studentReferenceDataRepository.GetGradeSchemeGuidAsync(source.GradeScheme);
                        if (!string.IsNullOrEmpty(gradeScheme))
                        {
                            sct.AwardGradeScheme = new Dtos.GuidObject2(gradeScheme);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            source.Guid, source.Id);
                    }
                }

                // Grade
                if (!string.IsNullOrEmpty(source.Grade))
                {
                    try
                    {
                        var grade = await _gradeRepository.GetGradesGuidAsync(source.Grade);
                        if (!string.IsNullOrEmpty(grade))
                        {
                            sct.Grade = new Dtos.GuidObject2(grade);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            source.Guid, source.Id);
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
                //sct.Credit.CreditCategory.CreditType = (StudentCourseTransferCreditType)Enum.Parse(typeof(StudentCourseTransferCreditType),
                // (await _academicCreditRepository.GetCreditTypeAsync(source.CreditType)).ToString());

                // Credit.CreditCategory.Detail;
                if (!string.IsNullOrEmpty(source.CreditType))
                {
                    var creditCategories = await GetCreditCategoriesAsync(bypassCache);
                    if (creditCategories != null)
                    {
                        var creditCategory = creditCategories.FirstOrDefault(t => t.Code == source.CreditType);
                        if (creditCategory != null)
                        {
                            if (!string.IsNullOrEmpty(creditCategory.Guid))
                            {
                                sct.Credit.CreditCategory.Detail = new GuidObject2(creditCategory.Guid);
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the credit category '{0}'", source.CreditType), "GUID.Not.Found", source.Guid, source.Id);
                            }
                            sct.Credit.CreditCategory.CreditType = (StudentCourseTransferCreditType)Enum.Parse(typeof(StudentCourseTransferCreditType), creditCategory.CreditType.ToString());
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to find the credit category '{0}'", source.CreditType), "Bad.Data", source.Guid, source.Id);
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
                    IntegrationApiExceptionAddError(string.Format("Equivalent course is required."), "Bad.Data", source.Guid, source.Id);
                }
                else if (courseGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the course '{0}'", source.Course), "GUID.Not.Found", source.Guid, source.Id);
                }
                else
                {
                    var courseGuid = string.Empty;
                    courseGuidCollection.TryGetValue(source.Course, out courseGuid);
                    if (string.IsNullOrEmpty(courseGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the course '{0}'", source.Course), "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        sct.EquivalentCourse = new GuidObject2(courseGuid);
                    }
                }

                // QualityPoints
                if (source.QualityPoints != null) sct.QualityPoints = source.QualityPoints;

                // Student
                if (string.IsNullOrEmpty(source.Student))
                {
                    IntegrationApiExceptionAddError(string.Format("Student is required."), "Bad.Data", source.Guid, source.Id);
                }
                else if (studentGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the student '{0}'", source.Student), "GUID.Not.Found", source.Guid, source.Id);
                }
                else
                {
                    var studentGuid = string.Empty;
                    studentGuidCollection.TryGetValue(source.Student, out studentGuid);
                    if (string.IsNullOrEmpty(studentGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the student '{0}'", source.Student), "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        sct.Student = new GuidObject2(studentGuid);
                    }
                }

                // TransferredFrom (institution)
                if (string.IsNullOrEmpty(source.TransferredFromInstitution))
                {
                    IntegrationApiExceptionAddError(string.Format("TransferredFrom institution is required."), "Bad.Data", source.Guid, source.Id);
                }
                else if (institutionGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the institution '{0}'", source.TransferredFromInstitution), "GUID.Not.Found", source.Guid, source.Id);
                }
                else
                {
                    var institutionGuid = string.Empty;
                    institutionGuidCollection.TryGetValue(source.TransferredFromInstitution, out institutionGuid);
                    if (string.IsNullOrEmpty(institutionGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the institution '{0}'", source.TransferredFromInstitution), "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        sct.TransferredFrom = new GuidObject2(institutionGuid);
                    }
                }

                // Status
                if (!string.IsNullOrEmpty(source.Status))
                {
                    try
                    {
                        var status = await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesGuidAsync(source.Status);
                        if (!string.IsNullOrEmpty(status))
                        {
                            sct.Status = new Dtos.GuidObject2(status);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            source.Guid, source.Id);
                    }
                }
                scts.Add(sct);
            }
            return scts;
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
