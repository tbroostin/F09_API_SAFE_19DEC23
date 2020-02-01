// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CurriculumService : BaseCoordinationService, ICurriculumService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger logger;
        private const string _dataOrigin = "Colleague";

        public CurriculumService(IStudentReferenceDataRepository studentReferenceDataRepository, 
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Gets all academic levels
        /// </summary>
        /// <returns>Collection of AcademicLevel2 DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicLevel2>> GetAcademicLevels2Async(bool bypassCache = false)
        {
            var academicLevelCollection = new List<Ellucian.Colleague.Dtos.AcademicLevel2>();

            var academicLevelEntities = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            if (academicLevelEntities != null && academicLevelEntities.Count() > 0)
            {
                foreach (var academicLevel in academicLevelEntities)
                {
                    academicLevelCollection.Add(ConvertAcademicLevelEntityToDto2(academicLevel));
                }
            }
            return academicLevelCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Get an academic level from its ID
        /// </summary>
        /// <returns>AcademicLevel2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicLevel2> GetAcademicLevelById2Async(string id)
        {
            try
            {
                return ConvertAcademicLevelEntityToDto2((await _studentReferenceDataRepository.GetAcademicLevelsAsync(true)).Where(al => al.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Level not found for GUID " + id, ex);
            }
        }

        /// <summary>
        /// Returns all account receivable types
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountReceivableType>> GetAccountReceivableTypesAsync(bool bypassCache)
        {
            var accountReceivableTypeCollection = new List<Ellucian.Colleague.Dtos.AccountReceivableType>();

            var accountReceivableTypes = await _studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache);
            if (accountReceivableTypes != null && accountReceivableTypes.Any())
            {
                foreach (var accountReceivableType in accountReceivableTypes)
                {
                    accountReceivableTypeCollection.Add(ConvertAccountReceivableTypeEntityToDto(accountReceivableType));
                }
            }
            return accountReceivableTypeCollection;
        }

        /// <summary>
        /// Returns an account receivable type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.AccountReceivableType> GetAccountReceivableTypeByIdAsync(string id)
        {
            try
            {
                return ConvertAccountReceivableTypeEntityToDto((await _studentReferenceDataRepository.GetAccountReceivableTypesAsync(true)).Where(art => art.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Account Receivable Type not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all assessment special circumstances
        /// </summary>
        /// <returns>Collection of AssessmentSpecialCircumstance DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstancesAsync(bool bypassCache = false)
        {
            var assessmentSpecialCircumstanceCollection = new List<Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance>();

            var assessmentSpecialCircumstanceEntities = await _studentReferenceDataRepository.GetAssessmentSpecialCircumstancesAsync(bypassCache);
            if (assessmentSpecialCircumstanceEntities != null && assessmentSpecialCircumstanceEntities.Count() > 0)
            {
                foreach (var assessmentSpecialCircumstance in assessmentSpecialCircumstanceEntities)
                {
                    assessmentSpecialCircumstanceCollection.Add(ConvertAssessmentSpecialCircumstanceEntityToDto(assessmentSpecialCircumstance));
                }
            }
            return assessmentSpecialCircumstanceCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an assessment special circumstance from its GUID
        /// </summary>
        /// <returns>AssessmentSpecialCircumstance DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance> GetAssessmentSpecialCircumstanceByGuidAsync(string guid)
        {
            try
            {
                return ConvertAssessmentSpecialCircumstanceEntityToDto((await _studentReferenceDataRepository.GetAssessmentSpecialCircumstancesAsync(true)).Where(fa => fa.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Assessment special circumstance not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Gets all course levels
        /// </summary>
        /// <returns>Collection of CourseLevel2 DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseLevel2>> GetCourseLevels2Async(bool bypassCache = false)
        {
            var courseLevelCollection = new List<Ellucian.Colleague.Dtos.CourseLevel2>();

            var courseLevelEntities = await _studentReferenceDataRepository.GetCourseLevelsAsync(bypassCache);
            if (courseLevelEntities != null && courseLevelEntities.Count() > 0)
            {
                foreach (var courseLevel in courseLevelEntities)
                {
                    courseLevelCollection.Add(ConvertCourseLevelEntityToDto2(courseLevel));
                }
            }
            return courseLevelCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Get a course level from its ID
        /// </summary>
        /// <returns>CourseLevel2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CourseLevel2> GetCourseLevelById2Async(string id)
        {
            try
            {
                return ConvertCourseLevelEntityToDto2((await _studentReferenceDataRepository.GetCourseLevelsAsync(true)).Where(cl => cl.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Course Level not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Gets all enrollment statuses
        /// </summary>
        /// <returns>Collection of EnrollmentStatus DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EnrollmentStatus>> GetEnrollmentStatusesAsync(bool bypassCache = false)
        {
            var enrollmentStatusCollection = new List<Ellucian.Colleague.Dtos.EnrollmentStatus>();

            var enrollmentStatusEntities = await _studentReferenceDataRepository.GetEnrollmentStatusesAsync(bypassCache);
            if (enrollmentStatusEntities != null && enrollmentStatusEntities.Count() > 0)
            {
                foreach (var enrollmentStatus in enrollmentStatusEntities)
                {
                    enrollmentStatusCollection.Add(ConvertEnrollmentStatusEntityToDto(enrollmentStatus));
                }
            }
            return enrollmentStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Gets all academic period enrollment statuses
        /// </summary>
        /// <returns>Collection of AcademicPeriodEnrollmentStatus DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus>> GetAcademicPeriodEnrollmentStatusesAsync(bool bypassCache = false)
        {
            var enrollmentStatusCollection = new List<Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus>();

            var enrollmentStatusEntities = await _studentReferenceDataRepository.GetEnrollmentStatusesAsync(bypassCache);
            if (enrollmentStatusEntities != null && enrollmentStatusEntities.Count() > 0)
            {
                foreach (var enrollmentStatus in enrollmentStatusEntities)
                {
                    enrollmentStatusCollection.Add(ConvertAcademicPeriodEnrollmentStatusEntityToDto(enrollmentStatus));
                }
            }
            return enrollmentStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Get a enrollment status from its ID
        /// </summary>
        /// <returns>EnrollmentStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EnrollmentStatus> GetEnrollmentStatusByGuidAsync(string id)
        {
            try
            {
                return ConvertEnrollmentStatusEntityToDto((await _studentReferenceDataRepository.GetEnrollmentStatusesAsync(true)).Where(srs => srs.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Enrollment Status not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Get a academic period enrollment status from its ID
        /// </summary>
        /// <returns>AcademicPeriodEnrollmentStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus> GetAcademicPeriodEnrollmentStatusByGuidAsync(string id)
        {
            try
            {
                return ConvertAcademicPeriodEnrollmentStatusEntityToDto((await _studentReferenceDataRepository.GetEnrollmentStatusesAsync(true)).Where(srs => srs.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Period Enrollment Status not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM version 4</remarks>
        /// <summary>
        /// Gets all instructional methods
        /// </summary>
        /// <returns>Collection of InstructionalMethod DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructionalMethod2>> GetInstructionalMethods2Async(bool bypassCache = false)
        {
            var instructionalMethodCollection = new List<Ellucian.Colleague.Dtos.InstructionalMethod2>();

            var instructionalMethodEntities = await _studentReferenceDataRepository.GetInstructionalMethodsAsync(bypassCache);
            if (instructionalMethodEntities != null && instructionalMethodEntities.Count() > 0)
            {
                foreach (var instructionalMethod in instructionalMethodEntities)
                {
                    instructionalMethodCollection.Add(ConvertInstructionalMethodEntityToDto2(instructionalMethod));
                }
            }
            return instructionalMethodCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM version 4</remarks>
        /// <summary>
        /// Get an instructional method from its GUID
        /// </summary>
        /// <returns>InstructionalMethod DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.InstructionalMethod2> GetInstructionalMethodById2Async(string guid)
        {
            try
            {
                return ConvertInstructionalMethodEntityToDto2((await _studentReferenceDataRepository.GetInstructionalMethodsAsync(true)).Where(im => im.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Instructional method not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Gets all section grade types
        /// </summary>
        /// <returns>Collection of SectionGradeType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionGradeType>> GetSectionGradeTypesAsync(bool bypassCache = false)
        {
            var sectionGradeTypeCollection = new List<Ellucian.Colleague.Dtos.SectionGradeType>();

            var sectionGradeTypeEntities = await _studentReferenceDataRepository.GetSectionGradeTypesAsync(bypassCache);
            if (sectionGradeTypeEntities != null && sectionGradeTypeEntities.Count() > 0)
            {
                foreach (var sectionGradeType in sectionGradeTypeEntities)
                {
                    sectionGradeTypeCollection.Add(ConvertSectionGradeTypeEntityToDto(sectionGradeType));
                }
            }
            return sectionGradeTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Get a section grade type from its GUID
        /// </summary>
        /// <returns>SectionGradeType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.SectionGradeType> GetSectionGradeTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertSectionGradeTypeEntityToDto((await _studentReferenceDataRepository.GetSectionGradeTypesAsync(true)).Where(cl => cl.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Section Grade Type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Gets all section registration statuses
        /// </summary>
        /// <returns>Collection of SectionRegistrationStatusItem DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2>> GetSectionRegistrationStatuses2Async(bool bypassCache = false)
        {
            var sectionRegistrationStatusCollection = new List<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2>();

            var sectionRegistrationStatusEntities = await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesAsync(bypassCache);
            if (sectionRegistrationStatusEntities != null && sectionRegistrationStatusEntities.Count() > 0)
            {
                foreach (var registrationStatus in sectionRegistrationStatusEntities)            
                {
                    if (registrationStatus != null && registrationStatus.Status.SectionRegistrationStatusReason != Colleague.Domain.Student.Entities.RegistrationStatusReason.Transfer)
                    {
                        sectionRegistrationStatusCollection.Add(ConvertSectionRegistrationStatusEntityToDto2(registrationStatus));
                    }
                }
            }
            return sectionRegistrationStatusCollection;
        }

        /// <remarks>FOR USE WITH EEDM Version 8</remarks>
        /// <summary>
        /// Gets all section registration statuses
        /// </summary>
        /// <returns>Collection of SectionRegistrationStatusItem3 DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3>> GetSectionRegistrationStatuses3Async(bool bypassCache = false)
        {
            var sectionRegistrationStatusCollection = new List<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3>();
            IEnumerable<Domain.Student.Entities.SectionRegistrationStatusItem> sectionRegistrationStatusEntities = new List<Domain.Student.Entities.SectionRegistrationStatusItem>();
            List<string> headcountInclusionList = await _studentReferenceDataRepository.GetHeadcountInclusionListAsync(bypassCache);
            try
            {
                sectionRegistrationStatusEntities = await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesAsync(bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (sectionRegistrationStatusEntities != null && sectionRegistrationStatusEntities.Count() > 0)
            {
                foreach (var registrationStatus in sectionRegistrationStatusEntities)
                {    
                    if (registrationStatus != null && registrationStatus.Status.SectionRegistrationStatusReason != Colleague.Domain.Student.Entities.RegistrationStatusReason.Transfer)
                    {
                        sectionRegistrationStatusCollection.Add(ConvertSectionRegistrationStatusEntityToDto3(registrationStatus, headcountInclusionList));
                    }
                }
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return sectionRegistrationStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Get a section registration status from its ID
        /// </summary>
        /// <returns>SectionRegistrationStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2> GetSectionRegistrationStatusById2Async(string id)
        {
            try
            {                
                var statusItem = (await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesAsync(true)).Where(srs => srs.Guid == id).First();
                if (statusItem != null && statusItem.Status.SectionRegistrationStatusReason != Colleague.Domain.Student.Entities.RegistrationStatusReason.Transfer)
                {
                    return ConvertSectionRegistrationStatusEntityToDto2(statusItem);
                }
                else
                {
                    throw new KeyNotFoundException("section-registration-statuses not found for GUID " + id);
                }
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("section-registration-statuses not found for GUID " + id);
            }
        }

        /// <remarks>FOR USE WITH EEDM Version 8</remarks>
        /// <summary>
        /// Get a section registration status from its ID
        /// </summary>
        /// <returns>SectionRegistrationStatusItem3 DTO object</returns>
        public async Task<Dtos.SectionRegistrationStatusItem3> GetSectionRegistrationStatusById3Async(string id)
        {
            Dtos.SectionRegistrationStatusItem3 sectionRegistrationStatus = null;
            try
            {
                var statusItem = (await _studentReferenceDataRepository.GetStudentAcademicCreditStatusesAsync(true)).Where(srs => srs.Guid == id).First();
                List<string> headcountInclusionList = await _studentReferenceDataRepository.GetHeadcountInclusionListAsync(true);
                if (statusItem != null && statusItem.Status.SectionRegistrationStatusReason != Colleague.Domain.Student.Entities.RegistrationStatusReason.Transfer)
                {
                    sectionRegistrationStatus = ConvertSectionRegistrationStatusEntityToDto3(statusItem, headcountInclusionList);
                }
                else
                {
                    throw new KeyNotFoundException("section-registration-statuses not found for GUID " + id);
                }

            }
            catch (Exception)
            {
                throw new KeyNotFoundException("section-registration-statuses not found for GUID " + id);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return sectionRegistrationStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Gets all student statuses
        /// </summary>
        /// <returns>Collection of StudentStatus DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentStatus>> GetStudentStatusesAsync(bool bypassCache = false)
        {
            var studentStatusCollection = new List<Ellucian.Colleague.Dtos.StudentStatus>();

            var studentStatusEntities = await _studentReferenceDataRepository.GetStudentStatusesAsync(bypassCache);
            if (studentStatusEntities != null && studentStatusEntities.Count() > 0)
            {
                foreach (var studentStatus in studentStatusEntities)
                {
                    studentStatusCollection.Add(ConvertStudentStatusEntityToDto(studentStatus));
                }
            }
            return studentStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Get an student status from its ID
        /// </summary>
        /// <returns>StudentStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentStatus> GetStudentStatusByIdAsync(string id)
        {
            try
            {
                return ConvertStudentStatusEntityToDto((await _studentReferenceDataRepository.GetStudentStatusesAsync(true)).Where(st => st.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Student Status not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Gets all student types
        /// </summary>
        /// <returns>Collection of StudentType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentType>> GetStudentTypesAsync(bool bypassCache = false)
        {
            var studentTypeCollection = new List<Ellucian.Colleague.Dtos.StudentType>();

            var studentTypeEntities = await _studentReferenceDataRepository.GetStudentTypesAsync(bypassCache);
            if (studentTypeEntities != null && studentTypeEntities.Count() > 0)
            {
                foreach (var studentType in studentTypeEntities)
                {
                    studentTypeCollection.Add(ConvertStudentTypeEntityToDto(studentType));
                }
            }
            return studentTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Get an student type from its ID
        /// </summary>
        /// <returns>StudentType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentType> GetStudentTypeByIdAsync(string id)
        {
            try
            {
                return ConvertStudentTypeEntityToDto((await _studentReferenceDataRepository.GetStudentTypesAsync(true)).Where(st => st.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Student Type not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Gets all subjects
        /// </summary>
        /// <returns>Collection of Subject DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Subject2>> GetSubjects2Async(bool bypassCache)
        {
            var subjectCollection = new List<Ellucian.Colleague.Dtos.Subject2>();

            var subjectEntities = (await _studentReferenceDataRepository.GetSubjectsAsync(bypassCache));
            if (subjectEntities != null && subjectEntities.Count() > 0)
            {
                foreach (var subject in subjectEntities)
                {
                    subjectCollection.Add(ConvertSubjectEntityToDto2(subject));
                }
            }
            return subjectCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Get a subject from its GUID
        /// </summary>
        /// <returns>Subject DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Subject2> GetSubjectByGuid2Async(string guid)
        {
            try
            {
                return ConvertSubjectEntityToDto2((await _studentReferenceDataRepository.GetSubjectsAsync(true)).Where(s => s.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Subject not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Converts an AcademicLevel domain entity to its corresponding AcademicLevel2 DTO
        /// </summary>
        /// <param name="source">AcademicLevel domain entity</param>
        /// <returns>AcademicLevel2 DTO</returns>
        private Ellucian.Colleague.Dtos.AcademicLevel2 ConvertAcademicLevelEntityToDto2(Ellucian.Colleague.Domain.Student.Entities.AcademicLevel source)
        {
            var academicLevel = new Ellucian.Colleague.Dtos.AcademicLevel2();

            //academicLevel.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            academicLevel.Id = source.Guid;
            academicLevel.Code = source.Code;
            academicLevel.Title = source.Description;
            academicLevel.Description = null;
            if (source.Category)
            {
                academicLevel.Category = new List<Dtos.AcademicLevelCategory?>() { Dtos.AcademicLevelCategory.ContinuingEducation };
            }

            return academicLevel;
        }

        /// <summary>
        /// Converts from AccountReceivableType entity to AccountReceivableType dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.AccountReceivableType ConvertAccountReceivableTypeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.AccountReceivableType source)
        {
            Dtos.AccountReceivableType accountReceivableType = new Dtos.AccountReceivableType();
            accountReceivableType.Id = source.Guid;
            accountReceivableType.Code = source.Code;
            accountReceivableType.Title = source.Description;
            accountReceivableType.Description = string.Empty;
            return accountReceivableType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an Assessment Special Circumstance domain entity to its corresponding AssessmentSpecialCircumstance DTO
        /// </summary>
        /// <param name="source">AssessmentSpecialCircumstance domain entity</param>
        /// <returns>AssessmentSpecialCircumstance DTO</returns>
        private Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance ConvertAssessmentSpecialCircumstanceEntityToDto(Ellucian.Colleague.Domain.Student.Entities.AssessmentSpecialCircumstance source)
        {
            var assessmentSpecialCircumstance = new Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance();

            assessmentSpecialCircumstance.Id = source.Guid;
            assessmentSpecialCircumstance.Code = source.Code;
            assessmentSpecialCircumstance.Title = source.Description;
            assessmentSpecialCircumstance.Description = null;

            return assessmentSpecialCircumstance;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Versioin 4</remarks>
        /// <summary>
        /// Converts a CourseLevel domain entity to its corresponding CourseLevel2 DTO
        /// </summary>
        /// <param name="source">CourseLevel domain entity</param>
        /// <returns>CourseLevel2 DTO</returns>
        private Ellucian.Colleague.Dtos.CourseLevel2 ConvertCourseLevelEntityToDto2(Ellucian.Colleague.Domain.Student.Entities.CourseLevel source)
        {
            var courseLevel = new Ellucian.Colleague.Dtos.CourseLevel2();

            //courseLevel.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            courseLevel.Id = source.Guid;
            courseLevel.Code = source.Code;
            courseLevel.Title = source.Description;
            courseLevel.Description = null;

            return courseLevel;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Converts a EnrollmentStatus domain entity to its corresponding EnrollmentStatus DTO
        /// </summary>
        /// <param name="source">EnrollmentStatus domain entity</param>
        /// <returns>EnrollmentStatus DTO</returns>
        private Ellucian.Colleague.Dtos.EnrollmentStatus ConvertEnrollmentStatusEntityToDto(Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus source)
        {
            var enrollmentStatus = new Ellucian.Colleague.Dtos.EnrollmentStatus();

            enrollmentStatus.Id = source.Guid;
            enrollmentStatus.Code = source.Code;
            enrollmentStatus.Title = source.Description;
            enrollmentStatus.Description = source.Description;
            enrollmentStatus.enrollmentStatusType = new Dtos.EnrollmentStatusType();

            switch (source.EnrollmentStatusType)
            {
                case Domain.Student.Entities.EnrollmentStatusType.inactive:
                    {
                        enrollmentStatus.enrollmentStatusType = Dtos.EnrollmentStatusType.Inactive;
                        break;
                    }
                case Domain.Student.Entities.EnrollmentStatusType.active:
                    {
                        enrollmentStatus.enrollmentStatusType = Dtos.EnrollmentStatusType.Active;
                        break;
                    }
                case Domain.Student.Entities.EnrollmentStatusType.complete:
                    {
                        enrollmentStatus.enrollmentStatusType = Dtos.EnrollmentStatusType.Complete;
                        break;
                    }
            }

            return enrollmentStatus;
        }

        ///// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        ///// <summary>
        ///// Converts an Financial Aid Award Period domain entity to its corresponding FinancialAidAwardPeriod DTO
        ///// </summary>
        ///// <param name="source">FinancialAidAwardPeriod domain entity</param>
        ///// <returns>FinancialAidAwardPeriod DTO</returns>
        //private Ellucian.Colleague.Dtos.FinancialAidAwardPeriod ConvertFinancialAidAwardPeriodEntityToDto(Ellucian.Colleague.Domain.Student.Entities.FinancialAidAwardPeriod source)
        //{
        //    var financialAidAwardPeriod = new Ellucian.Colleague.Dtos.FinancialAidAwardPeriod();

        //    financialAidAwardPeriod.Id = source.Guid;
        //    financialAidAwardPeriod.Code = source.Code;
        //    financialAidAwardPeriod.Title = source.Description;
        //    financialAidAwardPeriod.Description = null;
        //    financialAidAwardPeriod.Start = source.StartDate;
        //    financialAidAwardPeriod.End = source.EndDate;
        //    financialAidAwardPeriod.Status = source.status;

        //    return financialAidAwardPeriod;
        //}

        ///// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        ///// <summary>
        ///// Converts an Financial Aid Fund Category domain entity to its corresponding FinancialAidFundCategory DTO
        ///// </summary>
        ///// <param name="source">FinancialAidFundCategory domain entity</param>
        ///// <returns>FinancialAidFundCategory DTO</returns>
        //private Ellucian.Colleague.Dtos.FinancialAidFundCategory ConvertFinancialAidFundCategoryEntityToDto(Ellucian.Colleague.Domain.Student.Entities.FinancialAidFundCategory source)
        //{
        //    var financialAidFundCategory = new Ellucian.Colleague.Dtos.FinancialAidFundCategory();

        //    financialAidFundCategory.Id = source.Guid;
        //    financialAidFundCategory.Code = source.Code;
        //    financialAidFundCategory.Title = source.Description;
        //    financialAidFundCategory.Description = null;

        //    return financialAidFundCategory;
        //}

        ///// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        ///// <summary>
        ///// Converts an Financial Aid Office domain entity to its corresponding FinancialAidOffice DTO
        ///// </summary>
        ///// <param name="source">FinancialAidOffice domain entity</param>
        ///// <returns>FinancialAidOffice DTO</returns>
        //private Ellucian.Colleague.Dtos.FinancialAidOffice ConvertFinancialAidOfficeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.FinancialAidOffice source)
        //{
        //    var financialAidOffice = new Ellucian.Colleague.Dtos.FinancialAidOffice();

        //    financialAidOffice.Id = source.Guid;
        //    financialAidOffice.Code = source.Code;
        //    financialAidOffice.Title = source.Description;
        //    financialAidOffice.Description = null;
        //    financialAidOffice.AidAdministrator = source.aidAdministrator;
        //    financialAidOffice.AddressLines = source.addressLines;
        //    financialAidOffice.City = source.city;
        //    financialAidOffice.State = financialAidOffice.State;
        //    financialAidOffice.PostalCode = source.postalCode;
        //    financialAidOffice.PhoneNumber = source.phoneNumber;
        //    financialAidOffice.faxNumber = source.faxNumber;
        //    financialAidOffice.EmailAddress = source.emailAddress;
        //    financialAidOffice.Name = source.name;

        //    return financialAidOffice;
        //}

        ///// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        ///// <summary>
        ///// Converts an Financial Aid Year domain entity to its corresponding FinancialAidYear DTO
        ///// </summary>
        ///// <param name="source">FinancialAidYear domain entity</param>
        ///// <returns>FinancialAidYear DTO</returns>
        //private Ellucian.Colleague.Dtos.FinancialAidYear ConvertFinancialAidYearEntityToDto(Ellucian.Colleague.Domain.Student.Entities.FinancialAidYear source)
        //{
        //    var financialAidYear = new Ellucian.Colleague.Dtos.FinancialAidYear();

        //    financialAidYear.Id = source.Guid;
        //    financialAidYear.Code = source.Code;
        //    financialAidYear.Title = source.Description;
        //    if (!string.IsNullOrEmpty(source.Code))
        //    {
        //        try
        //        {
        //            var hostCountry = source.HostCountry;
        //            switch (hostCountry.ToString())
        //            {
        //                case "USA":
        //                    financialAidYear.Start = new DateTime(Int32.Parse(source.Code), 07, 01);
        //                    financialAidYear.End = new DateTime(Int32.Parse(source.Code) + 1, 06, 30);
        //                    break;
        //                default:
        //                    financialAidYear.Start = null;
        //                    financialAidYear.End = null;
        //                    break;
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            throw new ArgumentException("Code not defined for financial aid year for guid " + source.Guid + " with title " + source.Description);
        //        }
        //    }
        //    financialAidYear.Description = null;
        //    switch (source.status)
        //    {
        //        case "D" :
        //            financialAidYear.Status = FinancialAidYearStatus.Inactive;
        //            break;
        //        default :
        //            financialAidYear.Status = FinancialAidYearStatus.Active;
        //            break;
        //    }

        //    return financialAidYear;
        //}

        /// <summary>
        /// Converts entity to academic period enrollment statuses
        /// </summary>
        /// <param name="source">Domain.Student.Entities.EnrollmentStatus</param>
        /// <returns>Dtos.AcademicPeriodEnrollmentStatus</returns>
        private Dtos.AcademicPeriodEnrollmentStatus ConvertAcademicPeriodEnrollmentStatusEntityToDto(Domain.Student.Entities.EnrollmentStatus source)
        {
            var enrollmentStatus = new Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus();

            enrollmentStatus.Id = source.Guid;
            enrollmentStatus.Code = source.Code;
            enrollmentStatus.Title = source.Description;
            enrollmentStatus.Description = source.Description;

            return enrollmentStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Converts an InstructionalMethod domain entity to its corresponding InstructionalMethod DTO
        /// </summary>
        /// <param name="source">InstructionalMethod domain entity</param>
        /// <returns>InstructionalMethod DTO</returns>
        private Ellucian.Colleague.Dtos.InstructionalMethod ConvertInstructionalMethodEntityToDto(Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod source)
        {
            var instructionalMethod = new Ellucian.Colleague.Dtos.InstructionalMethod();
            instructionalMethod.Guid = source.Guid;
            instructionalMethod.Abbreviation = source.Code;
            instructionalMethod.Title = source.Description;
            instructionalMethod.Description = null;
            return instructionalMethod;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM version 4</remarks>
        /// <summary>
        /// Converts an InstructionalMethod domain entity to its corresponding InstructionalMethod DTO
        /// </summary>
        /// <param name="source">InstructionalMethod domain entity</param>
        /// <returns>InstructionalMethod2 DTO</returns>
        private Ellucian.Colleague.Dtos.InstructionalMethod2 ConvertInstructionalMethodEntityToDto2(Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod source)
        {
            var instructionalMethod = new Ellucian.Colleague.Dtos.InstructionalMethod2();
            instructionalMethod.Id = source.Guid;
            instructionalMethod.Code = source.Code;
            instructionalMethod.Title = source.Description;
            instructionalMethod.Description = null;
            return instructionalMethod;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Converts a SectionGradeType domain entity to its corresponding SectionGradeType DTO
        /// </summary>
        /// <param name="source">SectionGradeType domain entity</param>
        /// <returns>SectionGradeType DTO</returns>
        private Ellucian.Colleague.Dtos.SectionGradeType ConvertSectionGradeTypeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.SectionGradeType source)
        {
            var sectionGradeType = new Ellucian.Colleague.Dtos.SectionGradeType();
            sectionGradeType.Id = source.Guid;
            sectionGradeType.Code = source.Code;
            sectionGradeType.Title = source.Description;
            sectionGradeType.Description = null;

            return sectionGradeType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Converts a SectionRegistrationStatusItem domain entity to its corresponding SectionRegistrationStatusItem DTO
        /// </summary>
        /// <param name="source">SectionRegistrationStatusItem domain entity</param>
        /// <returns>SectionRegistrationStatusItem DTO</returns>
        private Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2 ConvertSectionRegistrationStatusEntityToDto2(Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem source)
        {
            var sectionRegistrationStatus = new Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2();

            sectionRegistrationStatus.Id = source.Guid;
            sectionRegistrationStatus.Code = source.Code;
            sectionRegistrationStatus.Title = source.Description;
            sectionRegistrationStatus.Description = source.Description;
            sectionRegistrationStatus.Status = new Dtos.SectionRegistrationStatusItemStatus() { RegistrationStatus = new Dtos.RegistrationStatus2(), SectionRegistrationStatusReason = new Dtos.RegistrationStatusReason2() };

            switch (source.Status.RegistrationStatus)
            {
                case Domain.Student.Entities.RegistrationStatus.Registered:
                    {
                        sectionRegistrationStatus.Status.RegistrationStatus = Dtos.RegistrationStatus2.Registered;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatus.NotRegistered:
                    {
                        sectionRegistrationStatus.Status.RegistrationStatus = Dtos.RegistrationStatus2.NotRegistered;
                        break;
                    }
            }

            switch (source.Status.SectionRegistrationStatusReason)
            {
                case Domain.Student.Entities.RegistrationStatusReason.Registered:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Registered;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Dropped:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Dropped;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Withdrawn:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Withdrawn;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Canceled:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Canceled;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Pending:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Pending;
                        break;
                    }

            }
            return sectionRegistrationStatus;
        }

        /// <remarks>FOR USE WITH EEDM Version 8</remarks>
        /// <summary>
        /// Converts a SectionRegistrationStatusItem domain entity to its corresponding SectionRegistrationStatusItem DTO
        /// </summary>
        /// <param name="source">SectionRegistrationStatusItem domain entity</param>
        /// <returns>SectionRegistrationStatusItem3 DTO</returns>
        private Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3 ConvertSectionRegistrationStatusEntityToDto3(Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem source, List<string> headcountInclusionList)
        {
            var sectionRegistrationStatus = new Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3();

            sectionRegistrationStatus.Id = source.Guid;
            sectionRegistrationStatus.Code = source.Code;
            sectionRegistrationStatus.Title = source.Description;
            sectionRegistrationStatus.Description = source.Description;
            sectionRegistrationStatus.Status = new Dtos.SectionRegistrationStatusItemStatus()
            {
                RegistrationStatus = new Dtos.RegistrationStatus2(),
                SectionRegistrationStatusReason = new Dtos.RegistrationStatusReason2()
            };

            switch (source.Status.RegistrationStatus)
            {
                case Domain.Student.Entities.RegistrationStatus.Registered:
                    {
                        sectionRegistrationStatus.Status.RegistrationStatus = Dtos.RegistrationStatus2.Registered;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatus.NotRegistered:
                    {
                        sectionRegistrationStatus.Status.RegistrationStatus = Dtos.RegistrationStatus2.NotRegistered;
                        break;
                    }
            }

            sectionRegistrationStatus.HeadCountStatus = Dtos.HeadCountStatus.Exclude;
            if (headcountInclusionList != null && headcountInclusionList.Any() && headcountInclusionList.Contains(source.Code))
            {
                sectionRegistrationStatus.HeadCountStatus = Dtos.HeadCountStatus.Include;
            }

            switch (source.Status.SectionRegistrationStatusReason)
            {
                case Domain.Student.Entities.RegistrationStatusReason.Registered:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Registered;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Dropped:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Dropped;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Withdrawn:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Withdrawn;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Canceled:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Canceled;
                        break;
                    }
                case Domain.Student.Entities.RegistrationStatusReason.Pending:
                    {
                        sectionRegistrationStatus.Status.SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Pending;
                        break;
                    }

            }
            return sectionRegistrationStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Converts an StudentStatus domain entity to its corresponding StudentStatus DTO
        /// </summary>
        /// <param name="source">StudentStatus domain entity</param>
        /// <returns>StudentStatus DTO</returns>
        private Ellucian.Colleague.Dtos.StudentStatus ConvertStudentStatusEntityToDto(Ellucian.Colleague.Domain.Student.Entities.StudentStatus source)
        {
            var studentStatus = new Ellucian.Colleague.Dtos.StudentStatus();

            studentStatus.Id = source.Guid;
            studentStatus.Code = source.Code;
            studentStatus.Title = source.Description;
            studentStatus.Description = null;

            return studentStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Converts an StudentType domain entity to its corresponding StudentType DTO
        /// </summary>
        /// <param name="source">StudentType domain entity</param>
        /// <returns>StudentType DTO</returns>
        private Ellucian.Colleague.Dtos.StudentType ConvertStudentTypeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.StudentType source)
        {
            var studentType = new Ellucian.Colleague.Dtos.StudentType();

            studentType.Id = source.Guid;
            studentType.Code = source.Code;
            studentType.Title = source.Description;
            studentType.Description = null;

            return studentType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Converts a Subject domain entity to its corresponding Subject2 DTO
        /// </summary>
        /// <param name="source">Subject domain entity</param>
        /// <returns>Subject2 DTO</returns>
        private Ellucian.Colleague.Dtos.Subject2 ConvertSubjectEntityToDto2(Ellucian.Colleague.Domain.Student.Entities.Subject source)
        {
            var subject = new Ellucian.Colleague.Dtos.Subject2();
            subject.Id= source.Guid;
            subject.Abbreviation = source.Code;
            subject.Title = source.Description;
            subject.Description = null;
            return subject;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a CreditType domain entity to its corresponding CreditCategoryType DTO
        /// </summary>
        /// <param name="source">CreditType domain entity</param>
        /// <returns>CreditCategoryType DTO</returns>
        private Ellucian.Colleague.Dtos.CreditCategoryType ConvertCreditTypeEntityToParentCategoryDto(Ellucian.Colleague.Domain.Student.Entities.CreditType source)
        {
            switch (source)
            {
                case Domain.Student.Entities.CreditType.ContinuingEducation:
                    return Dtos.CreditCategoryType.ContinuingEducation;
                case Domain.Student.Entities.CreditType.Exchange:
                    return Dtos.CreditCategoryType.Exchange;
                case Domain.Student.Entities.CreditType.Institutional:
                    return Dtos.CreditCategoryType.Institutional;
                case Domain.Student.Entities.CreditType.Other:
                    return Dtos.CreditCategoryType.Other;
                case Domain.Student.Entities.CreditType.Transfer:
                    return Dtos.CreditCategoryType.Transfer;
                default:
                    return Dtos.CreditCategoryType.None;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a CreditType domain entity to its corresponding CreditCategoryType2 DTO
        /// </summary>
        /// <param name="source">CreditType domain entity</param>
        /// <returns>CreditCategoryType2 DTO</returns>
        private Ellucian.Colleague.Dtos.CreditCategoryType2? ConvertCreditType2EntityToParentCategoryDto(Ellucian.Colleague.Domain.Student.Entities.CreditType source)
        {
            switch (source)
            {
                case Domain.Student.Entities.CreditType.ContinuingEducation:
                    return Dtos.CreditCategoryType2.ContinuingEducation;
                case Domain.Student.Entities.CreditType.Institutional:
                    return Dtos.CreditCategoryType2.Institutional;
                case Domain.Student.Entities.CreditType.Transfer:
                    return Dtos.CreditCategoryType2.Transfer;
                default:
                    return Dtos.CreditCategoryType2.ContinuingEducation;
            }
        }

        #region V6 Changes

        /// <summary>
        /// Gets all credit categories
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<Dtos.CreditCategory3></returns>
        public async Task<IEnumerable<Dtos.CreditCategory3>> GetCreditCategories3Async(bool bypassCache)
        {
            var creditTypeCollection = new List<Ellucian.Colleague.Dtos.CreditCategory3>();

            var creditTypeEntities = await _studentReferenceDataRepository.GetCreditCategoriesAsync(bypassCache);
            if (creditTypeEntities != null && creditTypeEntities.Count() > 0)
            {
                foreach (var creditType in creditTypeEntities)
                {
                    creditTypeCollection.Add(ConvertCourseCreditTypeEntityToCreditCategory3Dto(creditType));
                }
            }
            return creditTypeCollection;
        }

        /// <summary>
        /// Gets credit category by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.CreditCategory3</returns>
        public async Task<Dtos.CreditCategory3> GetCreditCategoryByGuid3Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Credit category id is required.");
            }

            try
            {
                return ConvertCourseCreditTypeEntityToCreditCategory3Dto((await _studentReferenceDataRepository.GetCreditCategoriesAsync(true)).Where(cc => cc.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Credit categories not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CourseCreditType domain entity to its corresponding CreditCategory DTO
        /// </summary>
        /// <param name="source">CourseCreditType domain entity</param>
        /// <returns>CreditCategory DTO</returns>
        private Ellucian.Colleague.Dtos.CreditCategory3 ConvertCourseCreditTypeEntityToCreditCategory3Dto(Ellucian.Colleague.Domain.Student.Entities.CreditCategory source)
        {
            var creditType = new Ellucian.Colleague.Dtos.CreditCategory3();
            creditType.Id = source.Guid;
            creditType.Code = source.Code;
            creditType.Title = source.Description;
            creditType.Description = null;
            creditType.CreditType = ConvertCreditType3EntityToParentCategoryDto(source.CreditType);
            return creditType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CreditType domain entity to its corresponding CreditCategoryType3 DTO
        /// </summary>
        /// <param name="source">CreditType domain entity</param>
        /// <returns>CreditCategoryType3 DTO</returns>
        private CreditCategoryType3? ConvertCreditType3EntityToParentCategoryDto(Ellucian.Colleague.Domain.Student.Entities.CreditType source)
        {
            switch (source)
            {
                case Domain.Student.Entities.CreditType.ContinuingEducation:
                    return CreditCategoryType3.ContinuingEducation;
                case Domain.Student.Entities.CreditType.Institutional:
                    return CreditCategoryType3.Institutional;
                case Domain.Student.Entities.CreditType.Transfer:
                    return CreditCategoryType3.Transfer;
                case Domain.Student.Entities.CreditType.Exchange:
                    return CreditCategoryType3.Exchange;
                case Domain.Student.Entities.CreditType.None:
                    return CreditCategoryType3.NoCredit;
                case Domain.Student.Entities.CreditType.Other:
                    return CreditCategoryType3.Other;
                default:
                    return null;
            }
        }

        #endregion
    }
}
