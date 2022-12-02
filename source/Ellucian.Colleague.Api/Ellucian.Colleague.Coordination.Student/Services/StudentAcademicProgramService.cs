// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAcademicProgramService : StudentCoordinationService, IStudentAcademicProgramService
    {
        private readonly IStudentAcademicProgramRepository _studentAcademicProgramRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly ITermRepository _termRepository;
        private readonly IPersonRepository _personRepository;
        private ILogger _logger;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private string _defaultInstitutionId;

        public StudentAcademicProgramService(IAdapterRegistry adapterRegistry,
            IStudentRepository studentRepository,
            IStudentAcademicProgramRepository studentAcademicProgramRepository,
            ITermRepository termRepository, IStudentReferenceDataRepository studentReferenceDataRepository,
            ICatalogRepository catalogRepository, IPersonRepository personRepository, IReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _studentAcademicProgramRepository = studentAcademicProgramRepository;
            _studentRepository = studentRepository;
            _logger = logger;
            _termRepository = termRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _catalogRepository = catalogRepository;
            _personRepository = personRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;

        }

        #region Get all codefiles/valcodes


        private IEnumerable<Domain.Base.Entities.OtherHonor> _otherHonors = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherHonor>> GetOtherHonorsAsync(bool bypassCache)
        {
            if (_otherHonors == null)
            {
                _otherHonors = await _referenceDataRepository.GetOtherHonorsAsync(bypassCache);
            }
            return _otherHonors;
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

        private IEnumerable<Domain.Student.Entities.EnrollmentStatus> _enrollmentStatuses = null;
        private async Task<IEnumerable<Domain.Student.Entities.EnrollmentStatus>> GetEnrollmentStatusesAsync(bool bypassCache)
        {
            if (_enrollmentStatuses == null)
            {
                _enrollmentStatuses = await _studentReferenceDataRepository.GetEnrollmentStatusesAsync(bypassCache);
            }
            return _enrollmentStatuses;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Catalog> _catalogs = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Catalog>> GetCatalogAsync(bool bypassCache)
        {
            if (_catalogs == null)
            {
                _catalogs = await _catalogRepository.GetAsync(bypassCache);
            }
            return _catalogs;
        }

        private IEnumerable<Domain.Base.Entities.Location> _location = null;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetLocationsAsync(bool bypassCache)
        {
            if (_location == null)
            {
                _location = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _location;
        }

        private IEnumerable<Domain.Base.Entities.Department> _otherDepartments = null;
        private async Task<IEnumerable<Domain.Base.Entities.Department>> GetDepartmentsAsync(bool bypassCache)
        {
            if (_otherDepartments == null)
            {
                _otherDepartments = await _referenceDataRepository.GetDepartments2Async(bypassCache);
            }
            return _otherDepartments;
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

        private IEnumerable<Domain.Student.Entities.Term> _terms = null;
        private async Task<IEnumerable<Domain.Student.Entities.Term>> GetTermsAsync(bool bypassCache)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        //Academic Periods
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> GetAcademicPeriods(bool bypassCache)
        {
            if (_academicPeriods == null)
            {
                var termEntities = await GetTermsAsync(bypassCache);
                _academicPeriods = _termRepository.GetAcademicPeriods(termEntities);
            }
            return _academicPeriods;
        }

        //get academic credentials
        private IEnumerable<AcadCredential> _acadCredentials;
        private async Task<IEnumerable<AcadCredential>> GetAcadCredentialsAsync(bool bypassCache)
        {
            if (_acadCredentials == null)
            {
                _acadCredentials = await _referenceDataRepository.GetAcadCredentialsAsync(bypassCache);

            }
            return _acadCredentials;
        }

        //get academic disciplines
        private IEnumerable<Domain.Base.Entities.AcademicDiscipline> _acadDisciplines;
        private async Task<IEnumerable<Domain.Base.Entities.AcademicDiscipline>> GetAcademicDisciplinesAsync(bool bypassCache)
        {
            if (_acadDisciplines == null)
            {
                _acadDisciplines = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache); ;

            }
            return _acadDisciplines;
        }

        private IEnumerable<AdmissionPopulation> _admissionPopulations;
        private async Task<IEnumerable<AdmissionPopulation>> GetAdmissionPopulationsAsync(bool bypassCache)
        {
            if (_admissionPopulations == null)
            {
                _admissionPopulations = await _studentReferenceDataRepository.GetAdmissionPopulationsAsync(bypassCache);
            }
            return _admissionPopulations;
        }
        #endregion

        /// <summary>
        /// Get an Student Academic Program from its GUID
        /// </summary>
        /// <returns>A Student Academic Program DTO <see cref="Ellucian.Colleague.Dtos.StudentAcademicProgram">object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms> GetStudentAcademicProgramByGuidAsync(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid", "GUID is required to get an Student Academic Program.");
                }
                
                var programEntity = new List<StudentAcademicProgram>();
                var inst = GetDefaultInstitutionId();
                programEntity.Add(await _studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, inst));
                return (await ConvertStudentAcademicProgramEntityToDto(programEntity, true)).FirstOrDefault();
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("Student Academic Program not found for GUID " + guid);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Get an Student Academic Program from its GUID
        /// </summary>
        /// <returns>A Student Academic Program DTO <see cref="Ellucian.Colleague.Dtos.StudentAcademicProgram2">object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms2> GetStudentAcademicProgramByGuid2Async(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid", "GUID is required to get an Student Academic Program.");
                }
               
                var programEntity = new List<StudentAcademicProgram>();
                var inst = GetDefaultInstitutionId();
                programEntity.Add(await _studentAcademicProgramRepository.GetStudentAcademicProgramByGuidAsync(guid, inst));
                return (await ConvertStudentAcademicProgramEntityToDto2Async(programEntity, true)).FirstOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException( ex.Message);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get an Student Academic Program from its GUID
        /// </summary>
        /// <returns>A Student Academic Program DTO <see cref="Ellucian.Colleague.Dtos.StudentAcademicProgram3">object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms3> GetStudentAcademicProgramByGuid3Async(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    //throw new ArgumentNullException("guid", "GUID is required to get an Student Academic Program.");
                    IntegrationApiExceptionAddError("GUID is required to get an Student Academic Program.", "Missing.Required.Property");
                    throw IntegrationApiException;
                }
                
                var programEntity = new List<StudentAcademicProgram>();
                var inst = string.Empty;
                try
                {
                    inst = GetDefaultInstitutionId();
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                    throw IntegrationApiException;
                }
                try
                {
                    programEntity.Add(await _studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(guid, inst));
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                return (await ConvertStudentAcademicProgramEntityToDto3Async(programEntity, false)).FirstOrDefault();
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get an Student Academic Program from its GUID
        /// </summary>
        /// <returns>A Student Academic Program DTO <see cref="Ellucian.Colleague.Dtos.StudentAcademicProgram4">object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms4> GetStudentAcademicProgramByGuid4Async(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    //throw new ArgumentNullException("guid", "GUID is required to get an Student Academic Program.");
                    IntegrationApiExceptionAddError("GUID is required to get an Student Academic Program.", "Missing.Required.Property");
                    throw IntegrationApiException;
                }
              
                var programEntity = new List<StudentAcademicProgram>();
                var inst = string.Empty;
                try
                {
                    inst = GetDefaultInstitutionId();
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                    throw IntegrationApiException;
                }
                try
                {
                    programEntity.Add(await _studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(guid, inst, false));
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                return (await ConvertStudentAcademicProgramEntityToDto4Async(programEntity, false)).FirstOrDefault();
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get an Student Academic Program from its GUID
        /// </summary>
        /// <returns>A Student Academic Program DTO <see cref="Ellucian.Colleague.Dtos.StudentAcademicProgram3">object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicProgramsSubmissions> GetStudentAcademicProgramSubmissionByGuidAsync(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    IntegrationApiExceptionAddError("GUID is required to get an Student Academic Program.", "Missing.Required.Property");
                    throw IntegrationApiException;
                }
                
                var programEntity = new List<StudentAcademicProgram>();
                var inst = string.Empty;
                try
                {
                    inst = GetDefaultInstitutionId();
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                    throw IntegrationApiException;
                }
                try
                {
                    programEntity.Add(await _studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(guid, inst));
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                return (await ConvertStudentAcademicProgramEntityToSubmissionDtoAsync(programEntity, false)).FirstOrDefault();
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Converts a Student Academic Programs domain entity to its corresponding Student Academic Program DTO
        /// </summary>
        /// <param name="source">A list of <see cref="StudentAcademicProgram">StudentAcademicProgram</see> domain entity</param>
        /// <returns>A list of <see cref="StudentAcademicProgram">StudentAcademicProgram</see> DTO</returns>
        private async Task<IEnumerable<Colleague.Dtos.StudentAcademicPrograms>> ConvertStudentAcademicProgramEntityToDto(List<Colleague.Domain.Student.Entities.StudentAcademicProgram> StuPrograms, bool bypassCache)
        {
            var studentAcadProgramDtos = new List<Colleague.Dtos.StudentAcademicPrograms>();

            try
            {
                foreach (var StuProgram in StuPrograms)
                {
                    var studentProgramDto = new Colleague.Dtos.StudentAcademicPrograms();
                    studentProgramDto.Id = StuProgram.Guid;
                    if (!string.IsNullOrEmpty(StuProgram.ProgramCode))
                    {
                        var programCode = await _studentReferenceDataRepository.GetAcademicProgramsGuidAsync(StuProgram.ProgramCode);
                        if (!string.IsNullOrEmpty(programCode))
                        {
                            studentProgramDto.Program = new Dtos.GuidObject2(programCode);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.CatalogCode))
                    {
                        var catalogCode = await _catalogRepository.GetCatalogGuidAsync(StuProgram.CatalogCode);
                        if (!string.IsNullOrEmpty(catalogCode))
                        {
                            studentProgramDto.Catalog = new Dtos.GuidObject2(catalogCode);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.StudentId))
                    {
                        var studentGuid = await _personRepository.GetPersonGuidFromIdAsync(StuProgram.StudentId);
                        if (!string.IsNullOrEmpty(studentGuid))
                        {
                            studentProgramDto.Student = new Dtos.GuidObject2(studentGuid);
                        }
                    }
                    //process location
                    if (!string.IsNullOrEmpty(StuProgram.Location))
                    {
                        var location = await _referenceDataRepository.GetLocationsGuidAsync(StuProgram.Location);
                        if (!string.IsNullOrEmpty(location))
                        {
                            studentProgramDto.Site = new Dtos.GuidObject2(location);
                        }
                    }
                    //process stpr.dept for disciplines, administeringInstitutionUnit, programOwner
                    Dtos.GuidObject2 deptGuid = null;
                    if (!string.IsNullOrEmpty(StuProgram.DepartmentCode))
                    {
                        var dept = await _referenceDataRepository.GetDepartments2GuidAsync(StuProgram.DepartmentCode);
                        if (!string.IsNullOrEmpty(dept))
                        {
                            deptGuid = new Dtos.GuidObject2(dept);
                            studentProgramDto.ProgramOwner = deptGuid;
                        }
                    }
                    //process start term
                    if (!string.IsNullOrEmpty(StuProgram.StartTerm))
                    {
                        var term = await _termRepository.GetAcademicPeriodsGuidAsync(StuProgram.StartTerm);
                        if (!string.IsNullOrEmpty(term))
                        {
                            studentProgramDto.StartTerm = new Dtos.GuidObject2(term);
                        }
                    }
                    //process academic level
                    if (!string.IsNullOrEmpty(StuProgram.AcademicLevelCode))
                    {
                        var acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(StuProgram.AcademicLevelCode);
                        if (!string.IsNullOrEmpty(acadLevel))
                        {
                            studentProgramDto.AcademicLevel = new Dtos.GuidObject2(acadLevel);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.DegreeCode) || StuProgram.StudentProgramCcds.Any())
                    {
                        var credentials = await ConvertCredentialCodeToGuidAsync(StuProgram.DegreeCode, StuProgram.StudentProgramCcds, bypassCache);
                        if (credentials != null)
                        {
                            studentProgramDto.Credentials = credentials;
                        }
                    }
                    if (StuProgram.StudentProgramMajors.Any() || StuProgram.StudentProgramMinors.Any() || StuProgram.StudentProgramSpecializations.Any())
                    {
                        var disciplines = await ConvertDisciplineCodeToGuidAsync(StuProgram.StudentProgramMajors, StuProgram.StudentProgramMinors, StuProgram.StudentProgramSpecializations, deptGuid, bypassCache);
                        if (disciplines != null)
                        {
                            studentProgramDto.Disciplines = disciplines;
                        }
                    }
                    studentProgramDto.StartDate = StuProgram.StartDate;
                    studentProgramDto.EndDate = StuProgram.EndDate;
                    var enroll = new Ellucian.Colleague.Dtos.EnrollmentStatusDetail();
                    // Determine the enrollment status
                    if (!string.IsNullOrEmpty(StuProgram.Status))
                    {
                        var enrollStatus = (await this.GetEnrollmentStatusesAsync(bypassCache)).FirstOrDefault(ct => ct.Code == StuProgram.Status);
                        if (enrollStatus != null)
                        {
                            enroll.Detail = new Dtos.GuidObject2() { Id = enrollStatus.Guid };
                            switch (enrollStatus.EnrollmentStatusType)
                            {
                                case Domain.Student.Entities.EnrollmentStatusType.active:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.complete:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.inactive:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                                    //if the status is inactive and end date is missing then display today's date
                                    break;
                            }
                        }
                    }
                    studentProgramDto.EnrollmentStatus = enroll;
                    //get acad credentials
                    studentProgramDto.PerformanceMeasure = StuProgram.GradGPA.HasValue ? StuProgram.GradGPA.Value.ToString() : null;
                    if (StuProgram.StudentProgramHonors.Any())
                    {
                        var recognitions = await ConvertRecognitionsCodeToGuid(StuProgram.StudentProgramHonors, bypassCache);
                        if (recognitions != null)
                        {
                            studentProgramDto.Recognitions = recognitions;
                        }
                    }
                    studentProgramDto.GraduatedOn = StuProgram.GraduationDate;
                    studentProgramDto.CredentialsDate = StuProgram.CredentialsDate;
                    studentProgramDto.ThesisTitle = !string.IsNullOrEmpty(StuProgram.ThesisTitle) ? StuProgram.ThesisTitle : null;
                    studentProgramDto.CreditsEarned = StuProgram.CreditsEarned == (decimal)0 ? null : StuProgram.CreditsEarned;
                    studentAcadProgramDtos.Add(studentProgramDto);
                }


                return studentAcadProgramDtos;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Student Academic Program not found.");
            }
        }

        /// <summary>
        /// Converts a Student Academic Programs domain entity to its corresponding Student Academic Program DTO
        /// </summary>
        /// <param name="source">A list of <see cref="StudentAcademicProgram">StudentAcademicProgram</see> domain entity</param>
        /// <returns>A list of <see cref="StudentAcademicProgram2">StudentAcademicProgram</see> DTO</returns>
        private async Task<IEnumerable<Colleague.Dtos.StudentAcademicPrograms2>> ConvertStudentAcademicProgramEntityToDto2Async(List<Colleague.Domain.Student.Entities.StudentAcademicProgram> StuPrograms, bool bypassCache)
        {
            try
            {
                var studentAcadProgramDtos = new List<Colleague.Dtos.StudentAcademicPrograms2>();

                var ids = new List<string>();


                ids.AddRange(StuPrograms.Where(p => (!string.IsNullOrEmpty(p.StudentId)))
                    .Select(p => p.StudentId).Distinct().ToList());

                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

                foreach (var StuProgram in StuPrograms)
                {
                    var studentProgramDto = new Colleague.Dtos.StudentAcademicPrograms2();
                    studentProgramDto.Id = StuProgram.Guid;
                    if (!string.IsNullOrEmpty(StuProgram.ProgramCode))
                    {
                        var programCode = await _studentReferenceDataRepository.GetAcademicProgramsGuidAsync(StuProgram.ProgramCode);
                        if (!string.IsNullOrEmpty(programCode))
                        {
                            studentProgramDto.AcademicProgram = new Dtos.GuidObject2(programCode);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.CatalogCode))
                    {
                        var catalogCode = await _catalogRepository.GetCatalogGuidAsync(StuProgram.CatalogCode);
                        if (catalogCode != null)
                        {
                            studentProgramDto.AcademicCatalog = new Dtos.GuidObject2(catalogCode);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.StudentId))
                    {
                        var studentGuid = string.Empty;
                        personGuidCollection.TryGetValue(StuProgram.StudentId, out studentGuid);
                        if (string.IsNullOrEmpty(studentGuid))
                        {
                            throw new KeyNotFoundException(string.Concat("Student guid not found, StudentId: '", StuProgram.StudentId, "', Record ID: '", StuProgram.Guid, "'"));
                        }
                        studentProgramDto.Student = new Dtos.GuidObject2(studentGuid);
                    }

                    //process circulum objective
                    if (StuProgram.CredentialsDate != null)
                    {
                        studentProgramDto.CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective.Outcome;
                    }
                    else
                    {
                        studentProgramDto.CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective.Matriculated;
                    }
                    //process preference
                    if (StuProgram.IsPrimary)
                        studentProgramDto.Preference = Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary;
                    //process location
                    if (!string.IsNullOrEmpty(StuProgram.Location))
                    {
                        var location = await _referenceDataRepository.GetLocationsGuidAsync(StuProgram.Location);
                        if (location != null)
                        {
                            studentProgramDto.Site = new Dtos.GuidObject2(location);
                        }
                    }
                    //process stpr.dept for disciplines, administeringInstitutionUnit, programOwner
                    Dtos.GuidObject2 deptGuid = null;
                    if (!string.IsNullOrEmpty(StuProgram.DepartmentCode))
                    {
                        var dept = await _referenceDataRepository.GetDepartments2GuidAsync(StuProgram.DepartmentCode);
                        if (dept != null)
                        {
                            deptGuid = new Dtos.GuidObject2(dept);
                            studentProgramDto.ProgramOwner = deptGuid;
                        }
                    }
                    //process academic periods 
                    if (!string.IsNullOrEmpty(StuProgram.StartTerm) || !string.IsNullOrEmpty(StuProgram.AnticipatedCompletionTerm) || !string.IsNullOrEmpty(StuProgram.GradTerm))
                    {
                        var acadPeriods = new Colleague.Dtos.StudentAcademicProgramsAcademicPeriods();
                        //process start term
                        if (!string.IsNullOrEmpty(StuProgram.StartTerm))
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(StuProgram.StartTerm);
                            if (term != null)
                            {
                                acadPeriods.Starting = new Dtos.GuidObject2(term);
                            }
                        }
                        //process expected graduation term
                        if (!string.IsNullOrEmpty(StuProgram.AnticipatedCompletionTerm))
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(StuProgram.AnticipatedCompletionTerm);
                            if (term != null)
                            {
                                acadPeriods.ExpectedGraduation = new Dtos.GuidObject2(term);
                            }
                        }
                        //process actual graduation term
                        if (!string.IsNullOrEmpty(StuProgram.GradTerm))
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(StuProgram.GradTerm);
                            if (term != null)
                            {
                                acadPeriods.ActualGraduation = new Dtos.GuidObject2(term);
                            }
                        }
                        studentProgramDto.AcademicPeriods = acadPeriods;
                    }
                    //process academic level
                    if (!string.IsNullOrEmpty(StuProgram.AcademicLevelCode))
                    {
                        var acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(StuProgram.AcademicLevelCode);
                        {
                            studentProgramDto.AcademicLevel = new Dtos.GuidObject2(acadLevel);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.DegreeCode) || StuProgram.StudentProgramCcds.Any())
                    {
                        var credentials = await ConvertCredentialCodeToGuidAsync(StuProgram.DegreeCode, StuProgram.StudentProgramCcds, bypassCache);
                        if (credentials != null)
                        {
                            studentProgramDto.Credentials = credentials;
                        }
                    }
                    if (StuProgram.StudentProgramMajors.Any() || StuProgram.StudentProgramMinors.Any() || StuProgram.StudentProgramSpecializations.Any())
                    {
                        var disciplines = await ConvertDisciplineCodeToGuidAsync(StuProgram.StudentProgramMajors, StuProgram.StudentProgramMinors, StuProgram.StudentProgramSpecializations, deptGuid, bypassCache);
                        if (disciplines != null)
                        {
                            studentProgramDto.Disciplines = disciplines;
                        }
                    }
                    studentProgramDto.StartDate = StuProgram.StartDate;
                    studentProgramDto.EndDate = StuProgram.EndDate;
                    studentProgramDto.ExpectedGraduationDate = StuProgram.AnticipatedCompletionDate;
                    var enroll = new Ellucian.Colleague.Dtos.EnrollmentStatusDetail();
                    // Determine the enrollment status
                    if (!string.IsNullOrEmpty(StuProgram.Status))
                    {
                        var enrollStatus = (await this.GetEnrollmentStatusesAsync(bypassCache)).FirstOrDefault(ct => ct.Code == StuProgram.Status);
                        if (enrollStatus != null)
                        {
                            enroll.Detail = new Dtos.GuidObject2() { Id = enrollStatus.Guid };
                            switch (enrollStatus.EnrollmentStatusType)
                            {
                                case Domain.Student.Entities.EnrollmentStatusType.active:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.complete:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.inactive:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                                    //if the status is inactive and end date is missing then display today's date
                                    break;
                            }
                        }
                    }
                    studentProgramDto.EnrollmentStatus = enroll;
                    //get acad credentials
                    studentProgramDto.PerformanceMeasure = StuProgram.GradGPA.HasValue ? StuProgram.GradGPA.Value.ToString() : null;
                    if (StuProgram.StudentProgramHonors.Any())
                    {
                        var recognitions = await ConvertRecognitionsCodeToGuid(StuProgram.StudentProgramHonors, bypassCache);
                        if (recognitions != null)
                        {
                            studentProgramDto.Recognitions = recognitions;
                        }
                    }
                    studentProgramDto.GraduatedOn = StuProgram.GraduationDate;
                    studentProgramDto.CredentialsDate = StuProgram.CredentialsDate;
                    studentProgramDto.ThesisTitle = !string.IsNullOrEmpty(StuProgram.ThesisTitle) ? StuProgram.ThesisTitle : null;
                    studentProgramDto.CreditsEarned = StuProgram.CreditsEarned == (decimal)0 ? null : StuProgram.CreditsEarned;
                    studentAcadProgramDtos.Add(studentProgramDto);
                }

                return studentAcadProgramDtos;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Student Academic Program not found.");
            }
        }

        /// <summary>
        /// Converts a Student Academic Programs domain entity to its corresponding Student Academic Program DTO
        /// </summary>
        /// <param name="source">A list of <see cref="StudentAcademicProgram">StudentAcademicProgram</see> domain entity</param>
        /// <returns>A IEnumerable of <see cref="StudentAcademicProgram3">StudentAcademicProgram</see> DTO</returns>
        private async Task<IEnumerable<Colleague.Dtos.StudentAcademicPrograms3>> ConvertStudentAcademicProgramEntityToDto3Async(List<Colleague.Domain.Student.Entities.StudentAcademicProgram> stuPrograms, bool bypassCache = false)
        {
            var studentAcadProgramDtos = new List<Colleague.Dtos.StudentAcademicPrograms3>();

            var ids = new List<string>();

            ids.AddRange(stuPrograms.Where(p => (!string.IsNullOrEmpty(p.StudentId)))
                .Select(p => p.StudentId).Distinct().ToList());

            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

            foreach (var stuProgram in stuPrograms)
            {
                var studentProgramDto = new Colleague.Dtos.StudentAcademicPrograms3();
                var id = string.Concat(stuProgram.StudentId, "*", stuProgram.ProgramCode);

                // Make sure whe have a valid GUID for the record we are dealing with
                if (string.IsNullOrEmpty(stuProgram.Guid))
                {
                    IntegrationApiExceptionAddError("Could not find a GUID for student-academic-programs entity.", "GUID.Not.Found", id: id);
                }

                studentProgramDto.Id = stuProgram.Guid;
                if (!string.IsNullOrEmpty(stuProgram.ProgramCode))
                {
                    try
                    {
                        var programCode = await _studentReferenceDataRepository.GetAcademicProgramsGuidAsync(stuProgram.ProgramCode);
                        if (!string.IsNullOrEmpty(programCode))
                        {
                            studentProgramDto.AcademicProgram = new Dtos.GuidObject2(programCode);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            id: id, guid: stuProgram.Guid);
                    }
                }
                if (!string.IsNullOrEmpty(stuProgram.CatalogCode))
                {
                    try
                    {
                        var catalogCode = await _catalogRepository.GetCatalogGuidAsync(stuProgram.CatalogCode);
                        if (catalogCode != null)
                        {
                            studentProgramDto.AcademicCatalog = new Dtos.GuidObject2(catalogCode);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            id: id, guid: stuProgram.Guid);
                    }
                }
                if (!string.IsNullOrEmpty(stuProgram.StudentId))
                {
                    var studentGuid = string.Empty;
                    personGuidCollection.TryGetValue(stuProgram.StudentId, out studentGuid);
                    if (string.IsNullOrEmpty(studentGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Person guid not found for Person Id: '", stuProgram.StudentId, "'"), "GUID.Not.Found"
                            , stuProgram.Guid, id);
                    }
                    else
                    {
                        studentProgramDto.Student = new Dtos.GuidObject2(studentGuid);
                    }
                }

                studentProgramDto.CurriculumObjective = this.ConvertCurriculumObjCategoryToCurriculumObjDtoEnum(stuProgram.CurriculumObjective);
                //process preference
                if (stuProgram.IsPrimary)
                    studentProgramDto.Preference = Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary;
                //process location
                if (!string.IsNullOrEmpty(stuProgram.Location))
                {
                    try
                    {
                        var location = await _referenceDataRepository.GetLocationsGuidAsync(stuProgram.Location);
                        if (location != null)
                        {
                            studentProgramDto.Site = new Dtos.GuidObject2(location);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                           stuProgram.Guid, id);
                    }
                }
                //process stpr.dept for disciplines, administeringInstitutionUnit, programOwner
                Dtos.GuidObject2 deptGuid = null;
                if (!string.IsNullOrEmpty(stuProgram.DepartmentCode))
                {
                    try
                    {
                        var dept = await _referenceDataRepository.GetDepartments2GuidAsync(stuProgram.DepartmentCode);
                        if (dept != null)
                        {
                            deptGuid = new Dtos.GuidObject2(dept);
                            studentProgramDto.ProgramOwner = deptGuid;
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            stuProgram.Guid, id);
                    }
                }
                //process academic periods 
                if (!string.IsNullOrEmpty(stuProgram.StartTerm) || !string.IsNullOrEmpty(stuProgram.AnticipatedCompletionTerm) || !string.IsNullOrEmpty(stuProgram.GradTerm))
                {
                    var acadPeriods = new Colleague.Dtos.StudentAcademicProgramsAcademicPeriods();
                    //process start term
                    if (!string.IsNullOrEmpty(stuProgram.StartTerm))
                    {
                        try
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(stuProgram.StartTerm);
                            if (term != null)
                            {
                                acadPeriods.Starting = new Dtos.GuidObject2(term);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                stuProgram.Guid, id);
                        }
                    }
                    //process expected graduation term
                    if (!string.IsNullOrEmpty(stuProgram.AnticipatedCompletionTerm))
                    {
                        try
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(stuProgram.AnticipatedCompletionTerm);
                            if (term != null)
                            {
                                acadPeriods.ExpectedGraduation = new Dtos.GuidObject2(term);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                stuProgram.Guid, id);
                        }
                    }
                    //process actual graduation term
                    if (!string.IsNullOrEmpty(stuProgram.GradTerm))
                    {
                        try
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(stuProgram.GradTerm);
                            if (term != null)
                            {
                                acadPeriods.ActualGraduation = new Dtos.GuidObject2(term);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                stuProgram.Guid, id);
                        }
                    }
                    studentProgramDto.AcademicPeriods = acadPeriods;
                }
                //process academic level
                if (!string.IsNullOrEmpty(stuProgram.AcademicLevelCode))
                {
                    try
                    {
                        var acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(stuProgram.AcademicLevelCode);
                        if (acadLevel != null)
                        {
                            studentProgramDto.AcademicLevel = new Dtos.GuidObject2(acadLevel);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            stuProgram.Guid, id);
                    }
                }

                if (!string.IsNullOrEmpty(stuProgram.DegreeCode) || stuProgram.StudentProgramCcds.Any())
                {
                    var credentials = await ConvertCredentialCodeToGuid2Async(stuProgram.DegreeCode, stuProgram.StudentProgramCcds, id, stuProgram.Guid, bypassCache);
                    if (credentials != null)
                    {
                        studentProgramDto.Credentials = credentials;
                    }
                }

                var disciplines = await ConvertDisciplineCodeToGuid2Async(
                        stuProgram.StudentProgramMajors,
                        stuProgram.StudentProgramMajorsTuple,
                        stuProgram.StudentProgramMinors,
                        stuProgram.StudentProgramMinorsTuple,
                        stuProgram.StudentProgramSpecializations,
                        stuProgram.StudentProgramSpecializationsTuple,
                        deptGuid, id, stuProgram.Guid, bypassCache);
                if (disciplines != null)
                {
                    studentProgramDto.Disciplines = disciplines;
                }

                studentProgramDto.StartOn = (stuProgram.StartDate == new DateTime()) ? null : stuProgram.StartDate;
                studentProgramDto.EndOn = stuProgram.EndDate;
                studentProgramDto.ExpectedGraduationDate = stuProgram.AnticipatedCompletionDate;
                var enrollmentStatusDetail = new Ellucian.Colleague.Dtos.EnrollmentStatusDetail();
                // Determine the enrollment status
                if (!string.IsNullOrEmpty(stuProgram.Status))
                {

                    var allEnrollmentStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);
                    if (allEnrollmentStatuses != null && allEnrollmentStatuses.Any())
                    {
                        var enrollStatus = allEnrollmentStatuses.FirstOrDefault(ct => ct.Code == stuProgram.Status);
                        if (enrollStatus != null)
                        {
                            enrollmentStatusDetail.Detail = new Dtos.GuidObject2() { Id = enrollStatus.Guid };
                            switch (enrollStatus.EnrollmentStatusType)
                            {
                                case Domain.Student.Entities.EnrollmentStatusType.active:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.complete:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.inactive:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                                    //if the status is inactive and end date is missing then display today's date
                                    break;
                            }
                        }
                    }
                }
                studentProgramDto.EnrollmentStatus = enrollmentStatusDetail;
                //get acad credentials
                if (stuProgram.StudentProgramHonors!= null && stuProgram.StudentProgramHonors.Any())
                {
                    var recognitions = await ConvertRecognitionsCodeToGuid2(stuProgram.StudentProgramHonors, id, stuProgram.Guid, bypassCache);
                    if (recognitions != null)
                    {
                        studentProgramDto.Recognitions = recognitions;
                    }
                }
                studentProgramDto.GraduatedOn = stuProgram.GraduationDate;
                studentProgramDto.CredentialsDate = stuProgram.CredentialsDate;
                studentProgramDto.ThesisTitle = !string.IsNullOrEmpty(stuProgram.ThesisTitle)
                    ? stuProgram.ThesisTitle : null;
                if (!(string.IsNullOrEmpty(stuProgram.AdmitStatus)))
                {
                    studentProgramDto.AdmissionClassification = new Dtos.DtoProperties.AdmissionClassificationDtoProperty()
                    {
                        AdmissionCategory = await ConvertAdmitStatusCodeToGuidAsync(stuProgram.AdmitStatus, bypassCache)
                    };
                }

                studentAcadProgramDtos.Add(studentProgramDto);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return studentAcadProgramDtos;
        }

        /// <summary>
        /// Converts a Student Academic Programs domain entity to its corresponding Student Academic Program DTO
        /// </summary>
        /// <param name="source">A list of <see cref="StudentAcademicProgram">StudentAcademicProgram</see> domain entity</param>
        /// <returns>A IEnumerable of <see cref="StudentAcademicPrograms4">StudentAcademicProgram</see> DTO</returns>
        private async Task<IEnumerable<Colleague.Dtos.StudentAcademicPrograms4>> ConvertStudentAcademicProgramEntityToDto4Async(List<Colleague.Domain.Student.Entities.StudentAcademicProgram> stuPrograms, bool bypassCache = false)
        {
            var studentAcadProgramDtos = new List<Colleague.Dtos.StudentAcademicPrograms4>();

            var ids = new List<string>();

            ids.AddRange(stuPrograms.Where(p => (!string.IsNullOrEmpty(p.StudentId)))
                .Select(p => p.StudentId).Distinct().ToList());

            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

            foreach (var stuProgram in stuPrograms)
            {
                var studentProgramDto = new Colleague.Dtos.StudentAcademicPrograms4();
                var id = string.Concat(stuProgram.StudentId, "*", stuProgram.ProgramCode);

                // Make sure whe have a valid GUID for the record we are dealing with
                if (string.IsNullOrEmpty(stuProgram.Guid))
                {
                    IntegrationApiExceptionAddError("Could not find a GUID for student-academic-programs entity.", "GUID.Not.Found", id: id);
                }

                studentProgramDto.Id = stuProgram.Guid;
                if (!string.IsNullOrEmpty(stuProgram.ProgramCode))
                {
                    try
                    {
                        var programCode = await _studentReferenceDataRepository.GetAcademicProgramsGuidAsync(stuProgram.ProgramCode);
                        if (!string.IsNullOrEmpty(programCode))
                        {
                            studentProgramDto.AcademicProgram = new Dtos.GuidObject2(programCode);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            id: id, guid: stuProgram.Guid);
                    }
                }
                if (!string.IsNullOrEmpty(stuProgram.CatalogCode))
                {
                    try
                    {
                        var catalogCode = await _catalogRepository.GetCatalogGuidAsync(stuProgram.CatalogCode);
                        if (catalogCode != null)
                        {
                            studentProgramDto.AcademicCatalog = new Dtos.GuidObject2(catalogCode);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            id: id, guid: stuProgram.Guid);
                    }
                }
                if (!string.IsNullOrEmpty(stuProgram.StudentId))
                {
                    var studentGuid = string.Empty;
                    personGuidCollection.TryGetValue(stuProgram.StudentId, out studentGuid);
                    if (string.IsNullOrEmpty(studentGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Person guid not found for Person Id: '", stuProgram.StudentId, "'"), "GUID.Not.Found"
                            , stuProgram.Guid, id);
                    }
                    else
                    {
                        studentProgramDto.Student = new Dtos.GuidObject2(studentGuid);
                    }
                }

                studentProgramDto.CurriculumObjective = this.ConvertCurriculumObjCategoryToCurriculumObjDtoEnum(stuProgram.CurriculumObjective);
                //process preference
                if (stuProgram.IsPrimary)
                    studentProgramDto.Preference = Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary;
                //process location
                if (!string.IsNullOrEmpty(stuProgram.Location))
                {
                    try
                    {
                        var location = await _referenceDataRepository.GetLocationsGuidAsync(stuProgram.Location);
                        if (location != null)
                        {
                            studentProgramDto.Site = new Dtos.GuidObject2(location);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                           stuProgram.Guid, id);
                    }
                }
                //process stpr.dept for disciplines, administeringInstitutionUnit, programOwner
                Dtos.GuidObject2 deptGuid = null;
                if (!string.IsNullOrEmpty(stuProgram.DepartmentCode))
                {
                    try
                    {
                        var dept = await _referenceDataRepository.GetDepartments2GuidAsync(stuProgram.DepartmentCode);
                        if (dept != null)
                        {
                            deptGuid = new Dtos.GuidObject2(dept);
                            studentProgramDto.ProgramOwner = deptGuid;
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            stuProgram.Guid, id);
                    }
                }
                //process academic periods 
                if (!string.IsNullOrEmpty(stuProgram.StartTerm) || !string.IsNullOrEmpty(stuProgram.AnticipatedCompletionTerm) || !string.IsNullOrEmpty(stuProgram.GradTerm))
                {
                    var acadPeriods = new Colleague.Dtos.StudentAcademicProgramsAcademicPeriods2();
                    //process start term
                    if (!string.IsNullOrEmpty(stuProgram.StartTerm))
                    {
                        try
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(stuProgram.StartTerm);
                            if (term != null)
                            {
                                acadPeriods.Starting = new Dtos.GuidObject2(term);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                stuProgram.Guid, id);
                        }
                    }
                    //process expected graduation term
                    if (!string.IsNullOrEmpty(stuProgram.AnticipatedCompletionTerm))
                    {
                        try
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(stuProgram.AnticipatedCompletionTerm);
                            if (term != null)
                            {
                                acadPeriods.ExpectedGraduation = new Dtos.GuidObject2(term);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                stuProgram.Guid, id);
                        }
                    }
                    
                    studentProgramDto.AcademicPeriods = acadPeriods;
                }
                //process academic level
                if (!string.IsNullOrEmpty(stuProgram.AcademicLevelCode))
                {
                    try
                    {
                        var acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(stuProgram.AcademicLevelCode);
                        if (acadLevel != null)
                        {
                            studentProgramDto.AcademicLevel = new Dtos.GuidObject2(acadLevel);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            stuProgram.Guid, id);
                    }
                }

                if (!string.IsNullOrEmpty(stuProgram.DegreeCode) || stuProgram.StudentProgramCcds.Any())
                {
                    var credentials = await ConvertCredentialCodeToGuid2Async(stuProgram.DegreeCode, stuProgram.StudentProgramCcds, id, stuProgram.Guid, bypassCache);
                    if (credentials != null)
                    {
                        studentProgramDto.Credentials = credentials;
                    }
                }

                var disciplines = await ConvertDisciplineCodeToGuid2Async(
                        stuProgram.StudentProgramMajors,
                        stuProgram.StudentProgramMajorsTuple,
                        stuProgram.StudentProgramMinors,
                        stuProgram.StudentProgramMinorsTuple,
                        stuProgram.StudentProgramSpecializations,
                        stuProgram.StudentProgramSpecializationsTuple,
                        deptGuid, id, stuProgram.Guid, bypassCache);
                if (disciplines != null)
                {
                    studentProgramDto.Disciplines = disciplines;
                }

                studentProgramDto.StartOn = (stuProgram.StartDate == new DateTime()) ? null : stuProgram.StartDate;
                studentProgramDto.EndOn = stuProgram.EndDate;
                studentProgramDto.ExpectedGraduationDate = stuProgram.AnticipatedCompletionDate;
                var enrollmentStatusDetail = new Ellucian.Colleague.Dtos.EnrollmentStatusDetail();
                // Determine the enrollment status
                if (!string.IsNullOrEmpty(stuProgram.Status))
                {

                    var allEnrollmentStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);
                    if (allEnrollmentStatuses != null && allEnrollmentStatuses.Any())
                    {
                        var enrollStatus = allEnrollmentStatuses.FirstOrDefault(ct => ct.Code == stuProgram.Status);
                        if (enrollStatus != null)
                        {
                            enrollmentStatusDetail.Detail = new Dtos.GuidObject2() { Id = enrollStatus.Guid };
                            switch (enrollStatus.EnrollmentStatusType)
                            {
                                case Domain.Student.Entities.EnrollmentStatusType.active:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.complete:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.inactive:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                                    //if the status is inactive and end date is missing then display today's date
                                    break;
                            }
                        }
                    }
                }
                studentProgramDto.EnrollmentStatus = enrollmentStatusDetail;
                //get acad credentials
                //if (stuProgram.StudentProgramHonors != null && stuProgram.StudentProgramHonors.Any())
                //{
                //    var recognitions = await ConvertRecognitionsCodeToGuid2(stuProgram.StudentProgramHonors, id, stuProgram.Guid, bypassCache);
                //    if (recognitions != null)
                //    {
                //        studentProgramDto.Recognitions = recognitions;
                //    }
                //}
                //studentProgramDto.GraduatedOn = stuProgram.GraduationDate;
                //studentProgramDto.CredentialsDate = stuProgram.CredentialsDate;
                //studentProgramDto.ThesisTitle = !string.IsNullOrEmpty(stuProgram.ThesisTitle)
                //    ? stuProgram.ThesisTitle : null;
                if (!(string.IsNullOrEmpty(stuProgram.AdmitStatus)))
                {
                    studentProgramDto.AdmissionClassification = new Dtos.DtoProperties.AdmissionClassificationDtoProperty()
                    {
                        AdmissionCategory = await ConvertAdmitStatusCodeToGuidAsync(stuProgram.AdmitStatus, bypassCache)
                    };
                }

                studentAcadProgramDtos.Add(studentProgramDto);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return studentAcadProgramDtos;
        }

        /// <summary>
        /// Converts a Student Academic Programs domain entity to its corresponding Student Academic Program Submission DTO
        /// </summary>
        /// <param name="source">A list of <see cref="StudentAcademicProgram">StudentAcademicProgram</see> domain entity</param>
        /// <returns>A IEnumerable of <see cref="StudentAcademicPrograms4">StudentAcademicProgram</see> DTO</returns>
        private async Task<IEnumerable<Colleague.Dtos.StudentAcademicProgramsSubmissions>> ConvertStudentAcademicProgramEntityToSubmissionDtoAsync(List<Colleague.Domain.Student.Entities.StudentAcademicProgram> stuPrograms, bool bypassCache = false)
        {
            var studentAcadProgramDtos = new List<Colleague.Dtos.StudentAcademicProgramsSubmissions>();

            var ids = new List<string>();

            ids.AddRange(stuPrograms.Where(p => (!string.IsNullOrEmpty(p.StudentId)))
                .Select(p => p.StudentId).Distinct().ToList());

            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

            foreach (var stuProgram in stuPrograms)
            {
                var studentProgramDto = new Colleague.Dtos.StudentAcademicProgramsSubmissions();
                var id = string.Concat(stuProgram.StudentId, "*", stuProgram.ProgramCode);

                // Make sure whe have a valid GUID for the record we are dealing with
                if (string.IsNullOrEmpty(stuProgram.Guid))
                {
                    IntegrationApiExceptionAddError("Could not find a GUID for student-academic-programs entity.", "GUID.Not.Found", id: id);
                }

                studentProgramDto.Id = stuProgram.Guid;
                if (!string.IsNullOrEmpty(stuProgram.ProgramCode))
                {
                    try
                    {
                        var programCode = await _studentReferenceDataRepository.GetAcademicProgramsGuidAsync(stuProgram.ProgramCode);
                        if (!string.IsNullOrEmpty(programCode))
                        {
                            studentProgramDto.AcademicProgram = new Dtos.GuidObject2(programCode);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            id: id, guid: stuProgram.Guid);
                    }
                }
                if (!string.IsNullOrEmpty(stuProgram.CatalogCode))
                {
                    try
                    {
                        var catalogCode = await _catalogRepository.GetCatalogGuidAsync(stuProgram.CatalogCode);
                        if (catalogCode != null)
                        {
                            studentProgramDto.AcademicCatalog = new Dtos.GuidObject2(catalogCode);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            id: id, guid: stuProgram.Guid);
                    }
                }
                if (!string.IsNullOrEmpty(stuProgram.StudentId))
                {
                    var studentGuid = string.Empty;
                    personGuidCollection.TryGetValue(stuProgram.StudentId, out studentGuid);
                    if (string.IsNullOrEmpty(studentGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Person guid not found for Person Id: '", stuProgram.StudentId, "'"), "GUID.Not.Found"
                            , stuProgram.Guid, id);
                    }
                    else
                    {
                        studentProgramDto.Student = new Dtos.GuidObject2(studentGuid);
                    }
                }

                studentProgramDto.CurriculumObjective = this.ConvertCurriculumObjCategoryToCurriculumObjDtoEnum(stuProgram.CurriculumObjective);
                //process preference
                if (stuProgram.IsPrimary)
                    studentProgramDto.Preference = Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary;
                //process location
                if (!string.IsNullOrEmpty(stuProgram.Location))
                {
                    try
                    {
                        var location = await _referenceDataRepository.GetLocationsGuidAsync(stuProgram.Location);
                        if (location != null)
                        {
                            studentProgramDto.Site = new Dtos.GuidObject2(location);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                           stuProgram.Guid, id);
                    }
                }
                //process stpr.dept for disciplines, administeringInstitutionUnit, programOwner
                Dtos.GuidObject2 deptGuid = null;
                if (!string.IsNullOrEmpty(stuProgram.DepartmentCode))
                {
                    try
                    {
                        var dept = await _referenceDataRepository.GetDepartments2GuidAsync(stuProgram.DepartmentCode);
                        if (dept != null)
                        {
                            deptGuid = new Dtos.GuidObject2(dept);
                            studentProgramDto.ProgramOwner = deptGuid;
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            stuProgram.Guid, id);
                    }
                }
                //process academic periods 
                if (!string.IsNullOrEmpty(stuProgram.StartTerm) || !string.IsNullOrEmpty(stuProgram.AnticipatedCompletionTerm) || !string.IsNullOrEmpty(stuProgram.GradTerm))
                {
                    var acadPeriods = new Colleague.Dtos.StudentAcademicProgramsAcademicPeriods2();
                    //process start term
                    if (!string.IsNullOrEmpty(stuProgram.StartTerm))
                    {
                        try
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(stuProgram.StartTerm);
                            if (term != null)
                            {
                                acadPeriods.Starting = new Dtos.GuidObject2(term);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                stuProgram.Guid, id);
                        }
                    }
                    //process expected graduation term
                    if (!string.IsNullOrEmpty(stuProgram.AnticipatedCompletionTerm))
                    {
                        try
                        {
                            var term = await _termRepository.GetAcademicPeriodsGuidAsync(stuProgram.AnticipatedCompletionTerm);
                            if (term != null)
                            {
                                acadPeriods.ExpectedGraduation = new Dtos.GuidObject2(term);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                stuProgram.Guid, id);
                        }
                    }
                    studentProgramDto.AcademicPeriods = acadPeriods;
                }
                //process academic level
                if (!string.IsNullOrEmpty(stuProgram.AcademicLevelCode))
                {
                    try
                    {
                        var acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(stuProgram.AcademicLevelCode);
                        if (acadLevel != null)
                        {
                            studentProgramDto.AcademicLevel = new Dtos.GuidObject2(acadLevel);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            stuProgram.Guid, id);
                    }
                }

                if (!string.IsNullOrEmpty(stuProgram.DegreeCode) || stuProgram.StudentProgramCcds.Any())
                {
                    var credentials = await ConvertCredentialCodeToGuid2Async(stuProgram.DegreeCode, stuProgram.StudentProgramCcds, id, stuProgram.Guid, bypassCache);
                    if (credentials != null)
                    {
                        studentProgramDto.Credentials = credentials;
                    }
                }

                var disciplines = await ConvertDisciplineCodeToGuid2Async(
                        stuProgram.StudentProgramMajors,
                        stuProgram.StudentProgramMajorsTuple,
                        stuProgram.StudentProgramMinors,
                        stuProgram.StudentProgramMinorsTuple,
                        stuProgram.StudentProgramSpecializations,
                        stuProgram.StudentProgramSpecializationsTuple,
                        deptGuid, id, stuProgram.Guid, bypassCache);
                if (disciplines != null)
                {
                    studentProgramDto.Disciplines = disciplines;
                }

                studentProgramDto.StartOn = (stuProgram.StartDate == new DateTime()) ? null : stuProgram.StartDate;
                studentProgramDto.EndOn = stuProgram.EndDate;
                studentProgramDto.ExpectedGraduationDate = stuProgram.AnticipatedCompletionDate;
                var enrollmentStatusDetail = new Ellucian.Colleague.Dtos.EnrollmentStatusDetail();
                // Determine the enrollment status
                if (!string.IsNullOrEmpty(stuProgram.Status))
                {

                    var allEnrollmentStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);
                    if (allEnrollmentStatuses != null && allEnrollmentStatuses.Any())
                    {
                        var enrollStatus = allEnrollmentStatuses.FirstOrDefault(ct => ct.Code == stuProgram.Status);
                        if (enrollStatus != null)
                        {
                            enrollmentStatusDetail.Detail = new Dtos.GuidObject2() { Id = enrollStatus.Guid };
                            switch (enrollStatus.EnrollmentStatusType)
                            {
                                case Domain.Student.Entities.EnrollmentStatusType.active:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.complete:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                                    break;
                                case Domain.Student.Entities.EnrollmentStatusType.inactive:
                                    enrollmentStatusDetail.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                                    //if the status is inactive and end date is missing then display today's date
                                    break;
                            }
                        }
                    }
                }
                studentProgramDto.EnrollmentStatus = enrollmentStatusDetail;
                if (!(string.IsNullOrEmpty(stuProgram.AdmitStatus)))
                {
                    studentProgramDto.AdmissionClassification = new Dtos.DtoProperties.AdmissionClassificationDtoProperty()
                    {
                        AdmissionCategory = await ConvertAdmitStatusCodeToGuid2Async(stuProgram.AdmitStatus, stuProgram.Guid, id)
                    };
                }

                studentAcadProgramDtos.Add(studentProgramDto);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return studentAcadProgramDtos;
        }

        /// <summary>
        /// Converts an admitStatus to a GuidObject
        /// </summary>
        /// <param name="admitStatus"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Ellucian.Colleague.Dtos.GuidObject2> ConvertAdmitStatusCodeToGuidAsync(string admitStatus, bool bypassCache = false)
        {
            Ellucian.Colleague.Dtos.GuidObject2 guidObject = null;

            if (!string.IsNullOrEmpty(admitStatus))
            {
                var admissionPopulations = await this.GetAdmissionPopulationsAsync(bypassCache);

                if (admissionPopulations != null && admissionPopulations.Any())
                {
                    var admissionPopulation = admissionPopulations.FirstOrDefault(a => a.Code == admitStatus);
                    if ((admissionPopulation != null) && (!string.IsNullOrEmpty(admissionPopulation.Guid)))
                    {
                        guidObject = new Ellucian.Colleague.Dtos.GuidObject2(admissionPopulation.Guid);
                    }
                }
            }
            return guidObject;
        }

        /// <summary>
        /// Converts an admitStatus to a GuidObject
        /// </summary>
        /// <param name="admitStatus"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Ellucian.Colleague.Dtos.GuidObject2> ConvertAdmitStatusCodeToGuid2Async(string admitStatus, string guid = "", string sourceId = "")
        {
            Ellucian.Colleague.Dtos.GuidObject2 guidObject = null;

            if (!string.IsNullOrEmpty(admitStatus))
            {
                try { 
               var admissionPopulationGuid = await _studentReferenceDataRepository.GetAdmissionPopulationsGuidAsync(admitStatus);

                if (!string.IsNullOrEmpty(admissionPopulationGuid))
                {
                    guidObject = new Ellucian.Colleague.Dtos.GuidObject2(admissionPopulationGuid);

                }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found", guid, sourceId);
                }
            }
            return guidObject;
        }

        /// <summary>
        /// Convert Academic honors to GUID. 
        /// </summary>
        /// <param name="honors">honors Code</param>
        /// <param name="bypassCache"></param>
        /// <returns>Academic Honors GUID</returns>
        private async Task<List<Dtos.GuidObject2>> ConvertRecognitionsCodeToGuid(List<string> honors, bool bypassCache = false)
        {
            List<Dtos.GuidObject2> guidObjects = null;
            if (honors != null && honors.Any())
            {
                foreach (var hnr in honors)
                {
                    if (!string.IsNullOrEmpty(hnr))
                    {
                        var honor = await _referenceDataRepository.GetOtherHonorsGuidAsync(hnr);

                        if (guidObjects == null)
                        {
                            guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                        }
                        guidObjects.Add(new Dtos.GuidObject2(honor));
                    }
                }
            }

            return guidObjects;
        }

        /// <summary>
        /// Convert Academic honors to GUID. 
        /// </summary>
        /// <param name="honors">honors Code</param>
        /// <param name="bypassCache"></param>
        /// <returns>Academic Honors GUID</returns>
        private async Task<List<Dtos.GuidObject2>> ConvertRecognitionsCodeToGuid2(List<string> honors,
            string id, string guid, bool bypassCache = false)
        {
            List<Dtos.GuidObject2> guidObjects = null;
            if (honors != null && honors.Any())
            {
                foreach (var hnr in honors)
                {
                    if (!string.IsNullOrEmpty(hnr))
                    {
                        try
                        {
                            var honor = await _referenceDataRepository.GetOtherHonorsGuidAsync(hnr);

                            if (guidObjects == null)
                            {
                                guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                            }
                            guidObjects.Add(new Dtos.GuidObject2(honor));
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                guid, id);
                        }
                    }
                }
            }

            return guidObjects;
        }
        /// <summary>
        /// Convert Academic Credentials to GUID. Credential includes degree. 
        /// </summary>
        /// <param name="degreeCode">Degree Code</param>
        /// <param name="certificates">list of certificates</param>
        /// <param name="bypassCache"></param>
        /// <returns>Academic Credentials GUID</returns>
        private async Task<List<Dtos.GuidObject2>> ConvertCredentialCodeToGuidAsync(string degreeCode, List<string> certificates, bool bypassCache = false)
        {

            List<Dtos.GuidObject2> guidObjects = null;
            if ((certificates != null) && (certificates.Any()))
            {
                foreach (var ccd in certificates)
                {
                    if (!string.IsNullOrEmpty(ccd))
                    {
                        var cert = await _referenceDataRepository.GetOtherCcdsGuidAsync(ccd);
                        if (guidObjects == null)
                        {
                            guidObjects = new List<Dtos.GuidObject2>();
                        }
                        guidObjects.Add(new Dtos.GuidObject2(cert));
                    }
                }
            }

            if (!string.IsNullOrEmpty(degreeCode))
            {
                if (!string.IsNullOrEmpty(degreeCode))
                {
                    var degree = await _referenceDataRepository.GetOtherDegreeGuidAsync(degreeCode);
                    if (guidObjects == null)
                    {
                        guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                    }
                    guidObjects.Add(new Dtos.GuidObject2(degree));
                }
            }
            return guidObjects;
        }

        /// <summary>
        /// Convert Academic Credentials to GUID. Credential includes degree. 
        /// </summary>
        /// <param name="degreeCode">Degree Code</param>
        /// <param name="certificates">list of certificates</param>
        /// <param name="bypassCache"></param>
        /// <returns>Academic Credentials GUID</returns>
        private async Task<List<Dtos.GuidObject2>> ConvertCredentialCodeToGuid2Async(string degreeCode, List<string> certificates, string id, string guid, bool bypassCache = false)
        {
            List<Dtos.GuidObject2> guidObjects = null;
            if ((certificates != null) && (certificates.Any()))
            {
                foreach (var ccd in certificates)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ccd))
                        {
                            var cert = await _referenceDataRepository.GetOtherCcdsGuidAsync(ccd);
                            if (guidObjects == null)
                            {
                                guidObjects = new List<Dtos.GuidObject2>();
                            }
                            guidObjects.Add(new Dtos.GuidObject2(cert));
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                            id: id, guid: guid);
                    }
                }
            }

            if (!string.IsNullOrEmpty(degreeCode))
            {
                if (!string.IsNullOrEmpty(degreeCode))
                {
                    try
                    {
                        var degree = await _referenceDataRepository.GetOtherDegreeGuidAsync(degreeCode);
                        if (guidObjects == null)
                        {
                            guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                        }
                        guidObjects.Add(new Dtos.GuidObject2(degree));
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                            id: id, guid: guid);
                    }
                }
            }
            return guidObjects;
        }

        /// <summary>
        /// Convert Academic Discipline to GUID. Displine include majors, minors & specializations from academic program and additional majors, minors & specializations from STUDENT.PROGRAMS.
        /// credential include degree & credentials from academic program and additional credentials from STUDENT.PROGRAMS.
        /// </summary>
        /// <param name="majors">list of majors</param>
        /// <param name="minors">list of minors</param>
        /// <param name="specializations">list of specializations</param>
        /// <returns>Academic Discipline GUIDs</returns>
        private async Task<List<Dtos.StudentAcademicProgramDisciplines>> ConvertDisciplineCodeToGuidAsync(List<string> majors, List<string> minors, List<string> specializations, Dtos.GuidObject2 deptGuid, bool bypassCache = false)
        {
            List<Dtos.StudentAcademicProgramDisciplines> disciplineObjects = null;

            if ((majors != null) && (majors.Any()))
            {
                foreach (var major in majors)
                {
                    if (!string.IsNullOrEmpty(major))
                    {
                        var maj = await _referenceDataRepository.GetOtherMajorsGuidAsync(major);

                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines() { Discipline = new Dtos.GuidObject2(maj), AdministeringInstitutionUnit = deptGuid });
                    }
                }
            }

            if ((minors != null) && (minors.Any()))
            {
                foreach (var minor in minors)
                {
                    if (!string.IsNullOrEmpty(minor))
                    {
                        var min = await _referenceDataRepository.GetOtherMinorsGuidAsync(minor);

                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines() { Discipline = new Dtos.GuidObject2(min), AdministeringInstitutionUnit = deptGuid });

                    }
                }
            }

            if ((specializations != null) && (specializations.Any()))
            {
                foreach (var specialization in specializations)
                {
                    if (!string.IsNullOrEmpty(specialization))
                    {
                        var special = await _referenceDataRepository.GetOtherSpecialsGuidAsync(specialization);

                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines() { Discipline = new Dtos.GuidObject2(special), AdministeringInstitutionUnit = deptGuid });
                    }
                }
            }

            return disciplineObjects;
        }

        /// <summary>
        /// Convert Academic Discipline to GUID. Displine include majors, minors & specializations from academic program and additional majors, minors & specializations from STUDENT.PROGRAMS.
        /// credential include degree & credentials from academic program and additional credentials from STUDENT.PROGRAMS.
        /// </summary>
        /// <param name="majors">list of majors</param>
        /// <param name="minors">list of minors</param>
        /// <param name="specializations">list of specializations</param>
        /// <returns>Academic Discipline GUIDs</returns>
        private async Task<List<Dtos.StudentAcademicProgramDisciplines2>> ConvertDisciplineCodeToGuid2Async(
           List<string> majors, List<Tuple<string, DateTime?, DateTime?>> majorsTuple,
           List<string> minors, List<Tuple<string, DateTime?, DateTime?>> minorsTuple,
           List<string> specializations, List<Tuple<string, DateTime?, DateTime?>> specializationsTuple,
           Dtos.GuidObject2 deptGuid, string id, string guid, bool bypassCache = false)
        {

            List<Dtos.StudentAcademicProgramDisciplines2> disciplineObjects = null;

            if ((majors != null) && (majors.Any()))
            {
                foreach (var major in majors)
                {
                    if (!string.IsNullOrEmpty(major))
                    {
                        try
                        {
                            var maj = await _referenceDataRepository.GetOtherMajorsGuidAsync(major);
                            if (disciplineObjects == null)
                            {
                                disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines2>();
                            }
                            disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines2()
                            {
                                Discipline = new Dtos.GuidObject2(maj),
                                AdministeringInstitutionUnit = deptGuid
                            });
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                                id: id, guid: guid);
                        }
                    }
                }
            }

            if ((majorsTuple != null) && (majorsTuple.Any()))
            {
                foreach (var major in majorsTuple)
                {
                    if (!string.IsNullOrEmpty(major.Item1))
                    {
                        try
                        {
                            var maj = await _referenceDataRepository.GetOtherMajorsGuidAsync(major.Item1);
                            if (disciplineObjects == null)
                            {
                                disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines2>();
                            }
                            disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines2()
                            {
                                Discipline = new Dtos.GuidObject2(maj),
                                AdministeringInstitutionUnit = deptGuid,
                                StartOn = major.Item2,
                                EndOn = major.Item3
                            });
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                                id: id, guid: guid);
                        }
                    }
                }
            }

            if ((minors != null) && (minors.Any()))
            {
                foreach (var minor in minors)
                {
                    if (!string.IsNullOrEmpty(minor))
                    {
                        try
                        {
                            var min = await _referenceDataRepository.GetOtherMinorsGuidAsync(minor);

                            if (disciplineObjects == null)
                            {
                                disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines2>();
                            }
                            disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines2()
                            {
                                Discipline = new Dtos.GuidObject2(min),
                                AdministeringInstitutionUnit = deptGuid
                            });
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                                id: id, guid: guid);
                        }
                    }
                }
            }

            if ((minorsTuple != null) && (minorsTuple.Any()))
            {
                foreach (var minor in minorsTuple)
                {
                    if (!string.IsNullOrEmpty(minor.Item1))
                    {
                        try
                        {
                            var min = await _referenceDataRepository.GetOtherMinorsGuidAsync(minor.Item1);
                            if (disciplineObjects == null)
                            {
                                disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines2>();
                            }
                            disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines2()
                            {
                                Discipline = new Dtos.GuidObject2(min),
                                AdministeringInstitutionUnit = deptGuid,
                                StartOn = minor.Item2,
                                EndOn = minor.Item3
                            });
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                                id: id, guid: guid);
                        }
                    }
                }
            }

            if ((specializations != null) && (specializations.Any()))
            {
                foreach (var specialization in specializations)
                {
                    if (!string.IsNullOrEmpty(specialization))
                    {
                        try
                        {
                            var special = await _referenceDataRepository.GetOtherSpecialsGuidAsync(specialization);
                            if (disciplineObjects == null)
                            {
                                disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines2>();
                            }
                            disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines2()
                            {
                                Discipline = new Dtos.GuidObject2(special),
                                AdministeringInstitutionUnit = deptGuid
                            });
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                                id: id, guid: guid);
                        }
                    }
                }
            }

            if ((specializationsTuple != null) && (specializationsTuple.Any()))
            {
                foreach (var specialization in specializationsTuple)
                {

                    if (!string.IsNullOrEmpty(specialization.Item1))
                    {
                        try
                        {
                            var special = await _referenceDataRepository.GetOtherSpecialsGuidAsync(specialization.Item1);
                            if (disciplineObjects == null)
                            {
                                disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines2>();
                            }
                            disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines2()
                            {
                                Discipline = new Dtos.GuidObject2(special),
                                AdministeringInstitutionUnit = deptGuid,
                                StartOn = specialization.Item2,
                                EndOn = specialization.Item3
                            });
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found",
                                id: id, guid: guid);
                        }
                    }
                }
            }

            return disciplineObjects;
        }

        /// <summary>
        /// Creates a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgramDto">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the created student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms> CreateStudentAcademicProgramAsync(Dtos.StudentAcademicPrograms studentAcadProgramDto, bool bypassCache = false)
        {
          

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgramEntity = await ConvertStudentAcademicProgramDtoToEntityAsync(studentAcadProgramDto, bypassCache);
            var createdStudentAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStudentAcadProgEntity.Add(await _studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(studentAcadProgramEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto(createdStudentAcadProgEntity, bypassCache)).FirstOrDefault();
        }

        /// <summary>
        /// Creates a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgramDto2">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the created student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms2> CreateStudentAcademicProgram2Async(Dtos.StudentAcademicPrograms2 studentAcadProgramDto, bool bypassCache = false)
        {
            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgramEntity = await ConvertStudentAcademicProgramDtoToEntity2Async(studentAcadProgramDto, bypassCache);
            var createdStudentAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStudentAcadProgEntity.Add(await _studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(studentAcadProgramEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto2Async(createdStudentAcadProgEntity, bypassCache)).FirstOrDefault();
        }


        /// <summary>
        /// Creates a Student Academic Program Submission
        /// </summary>
        /// <param name="StudentAcademicProgramsSubmissions">An StudentAcademicProgramsSubmissions domain object</param>
        /// <returns>An Student Academic Program DTO object for the created student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms4> CreateStudentAcademicProgramSubmissionAsync(Dtos.StudentAcademicProgramsSubmissions studentAcadProgramDto, bool bypassCache = false)
        {
             ValidateStudentAcademicProgramsSubmissions(string.Empty, string.Empty, studentAcadProgramDto);

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgramEntity = await ConvertStudentAcademicProgramSubmissionDtoToEntityAsync(studentAcadProgramDto, string.Empty, bypassCache);
            // Throw errors
            var createdStudentAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStudentAcadProgEntity.Add(await _studentAcademicProgramRepository.CreateStudentAcademicProgram2Async(studentAcadProgramEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto4Async(createdStudentAcadProgEntity, bypassCache)).FirstOrDefault();
        }

        /// <summary>
        /// Replaces student academic program.
        /// </summary>
        /// <param name="studentAcadProgramDto"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<StudentAcademicPrograms4> CreateStudentAcademicProgramReplacementsAsync(StudentAcademicProgramReplacements studentAcadProgramDto, bool bypassCache)
        {
           

            ValidateStudentAcademicProgramReplacements(string.Empty, string.Empty, studentAcadProgramDto);

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var inst = GetDefaultInstitutionId();
            Domain.Student.Entities.StudentAcademicProgram studentAcadProgramEntity = await ConvertStudentAcademicProgramReplacementsDtoToEntityAsync(studentAcadProgramDto, string.Empty, inst, bypassCache);
            // Throw errors
            var createdStudentAcadProgEntity = new List<StudentAcademicProgram>();
            createdStudentAcadProgEntity.Add(await _studentAcademicProgramRepository.CreateStudentAcademicProgram2Async(studentAcadProgramEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto4Async(createdStudentAcadProgEntity, bypassCache)).FirstOrDefault();
        }

        /// <summary>
        /// Validates the data in the StudentAcademicPrograms object
        /// </summary>
        /// <param name="stuAcadProg">StudentAcademicPrograms from the request</param>
        private void ValidateStudentAcademicProgramReplacements(string guid, string stprKey, Dtos.StudentAcademicProgramReplacements stuAcadProg)
        {
            if (stuAcadProg == null)
            {

                IntegrationApiExceptionAddError("Must provide a StudentAcademicProgramsReplacements object for update", "Missing.Required.Property", guid, stprKey);
                throw IntegrationApiException;
            }

            if (stuAcadProg.Student == null || string.IsNullOrEmpty(stuAcadProg.Student.Id))
            {
                IntegrationApiExceptionAddError("Student.id is a required property", "Missing.Required.Property", guid, stprKey);
            }

            if (stuAcadProg.ProgramToReplace == null || string.IsNullOrEmpty(stuAcadProg.ProgramToReplace.Id))
            {
                IntegrationApiExceptionAddError("ProgramToReplace.id is a required property", "Missing.Required.Property", guid, stprKey);
            }

            if (stuAcadProg.NewProgram == null || string.IsNullOrEmpty(stuAcadProg.NewProgram.Detail.Id))
            {
                IntegrationApiExceptionAddError("NewProgram.id is a required property", "Missing.Required.Property", guid, stprKey);
            }

            if(stuAcadProg.ProgramToReplace != null && stuAcadProg.NewProgram != null && !string.IsNullOrEmpty(stuAcadProg.ProgramToReplace.Id) && 
                !string.IsNullOrEmpty(stuAcadProg.NewProgram.Detail.Id) && stuAcadProg.ProgramToReplace.Id.Equals(stuAcadProg.NewProgram.Detail.Id, StringComparison.OrdinalIgnoreCase))
            {
                IntegrationApiExceptionAddError(string.Format("The program to replace: {0} can not be the same as the new program: {1}.", stuAcadProg.ProgramToReplace.Id, stuAcadProg.NewProgram.Detail.Id), 
                    "Missing.Required.Property", guid, stprKey);
            }

            //validate curriculumObjective
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.CurriculumObjective == null || stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.CurriculumObjective == Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.NotSet)
            {
                IntegrationApiExceptionAddError("The curriculumObjective must be set to 'matriculated' for any new student programs.", "Missing.Required.Property", guid, stprKey);
            }
            else
            {
                if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.CurriculumObjective != Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated)
                {
                    IntegrationApiExceptionAddError("CurriculumObjective must be set to 'matriculated' for any new or updated student programs.", "Validation.Exception", guid, stprKey);
                }
            }

            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.StartOn == null)
            {
                IntegrationApiExceptionAddError("StartOn is required to create or update a matriculated student program.", "Missing.Required.Property", guid, stprKey);

            }
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.EnrollmentStatus == null || stuAcadProg.NewProgram != null && (stuAcadProg.NewProgram.EnrollmentStatus != null && 
                string.IsNullOrEmpty(stuAcadProg.NewProgram.EnrollmentStatus.EnrollStatus.ToString())))
            {
                IntegrationApiExceptionAddError("EnrollmentStatus is a required property", "Missing.Required.Property", guid, stprKey);
            }
            //check end date is not before start date
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.EndOn != null && stuAcadProg.NewProgram.EndOn < stuAcadProg.NewProgram.StartOn)
            {
                IntegrationApiExceptionAddError("EndOn cannot be before startOn.", "Validation.Exception", guid, stprKey);

            }

            //check end date is not before start date
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.EndOn != null && stuAcadProg.NewProgram.EndOn < stuAcadProg.NewProgram.StartOn)
            {
                IntegrationApiExceptionAddError("EndOn cannot be before startOn.", "Validation.Exception", guid, stprKey);

            }
            //check exptected graduation date is not before start date
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.ExpectedGraduationDate != null && stuAcadProg.NewProgram.ExpectedGraduationDate < stuAcadProg.NewProgram.StartOn)
            {
                IntegrationApiExceptionAddError("ExpectedGraduationDate cannot be before startOn.", "Validation.Exception", guid, stprKey);
            }
            
            //if the enrollment status is inactive, then the end date is required.
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.EndOn == null && stuAcadProg.NewProgram.EnrollmentStatus != null && stuAcadProg.NewProgram.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Inactive)
            {
                IntegrationApiExceptionAddError("EndOn is required for the enrollment status of inactive.", "Validation.Exception", guid, stprKey);
            }
            // the status of complete is not valid for PUT/POST
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.EnrollmentStatus != null && stuAcadProg.NewProgram.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Complete)
            {
                IntegrationApiExceptionAddError("Enrollment status of complete is not supported. Graduation processing can only be invoked directly in Colleague.", "Validation.Exception", guid, stprKey);
            }

            //the status of active cannot have end date
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.EndOn != null && stuAcadProg.NewProgram.EnrollmentStatus != null && stuAcadProg.NewProgram.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Active)
            {
                IntegrationApiExceptionAddError("EndOn is not valid for the enrollment status of active.", "Validation.Exception", guid, stprKey);
            }
            //check the credentials body is good.
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.Credentials != null && stuAcadProg.NewProgram.Credentials.Count > 0)
            {
                foreach (var cred in stuAcadProg.NewProgram.Credentials)
                {
                    if (cred == null || string.IsNullOrEmpty(cred.Id))
                    {
                        IntegrationApiExceptionAddError("Credential id is a required field when credentials are in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                }
            }

            //check displines body is good.
            if (stuAcadProg.NewProgram != null && stuAcadProg.NewProgram.Disciplines != null && stuAcadProg.NewProgram.Disciplines.Count > 0)
            {
                foreach (var dis in stuAcadProg.NewProgram.Disciplines)
                {
                    if (dis.Discipline == null)
                    {
                        IntegrationApiExceptionAddError("Discipline is a required property when disciplines are in the message body.", "Missing.Required.Property", guid, stprKey);

                    }
                    else if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        IntegrationApiExceptionAddError("Discipline id is a required property when discipline is in the message body.", "Missing.Required.Property", guid, stprKey);

                    }

                }
            }
            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Convert DTO to Entity.
        /// </summary>
        /// <param name="stuAcadProgramsDto"></param>
        /// <param name="empty"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<StudentAcademicProgram> ConvertStudentAcademicProgramReplacementsDtoToEntityAsync(StudentAcademicProgramReplacements stuAcadProgramsDto, string stprKey, string inst, bool bypassCache)
        {
            // handle empty guid
            var guid = string.Empty;
            var depts = await this.GetDepartmentsAsync(bypassCache);
            if (!string.Equals(stuAcadProgramsDto.Id, Guid.Empty.ToString()))
            {
                guid = stuAcadProgramsDto.Id;
            }

            var startDate = new DateTime();
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.StartOn != null)
            {
                startDate = Convert.ToDateTime(stuAcadProgramsDto.NewProgram.StartOn.Value.ToString("yyyy-MM-dd"));
            }

            var studentId = string.Empty;
            try
            {
                studentId = await _personRepository.GetPersonIdFromGuidAsync(stuAcadProgramsDto.Student.Id);
                if (string.IsNullOrEmpty(studentId))
                {
                    IntegrationApiExceptionAddError("Student.id is not a valid GUID for persons.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Student.id is not a valid GUID for persons.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }

            if ((!string.IsNullOrEmpty(studentId)) && (await _personRepository.IsCorpAsync(studentId)))
            {
                IntegrationApiExceptionAddError("Student.id is not a valid GUID for persons.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }

            #region program to replace
            string programToRepl = string.Empty;
            try
            {
                var progToRepl = await _studentAcademicProgramRepository.GetStudentAcademicProgramByGuid2Async(stuAcadProgramsDto.ProgramToReplace.Id, inst);
                if(progToRepl == null)
                {
                    IntegrationApiExceptionAddError("ProgramToReplace.Id does not match the program ID for the program to replace.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    throw IntegrationApiException;
                }
                programToRepl = progToRepl.ProgramCode;

                if (!string.IsNullOrEmpty(progToRepl.Status))
                {
                    var enrollStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);

                    if(enrollStatuses != null)
                    {
                        var status = enrollStatuses.FirstOrDefault(st => st.Code.Equals(progToRepl.Status, StringComparison.OrdinalIgnoreCase) && 
                                     st.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                        if(status == null && !progToRepl.StartDate.HasValue)
                        {
                            IntegrationApiExceptionAddError(string.Format("The student's program {0} is not in an active state and may not be replaced.", stuAcadProgramsDto.ProgramToReplace.Id), 
                                "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                        }
                    }
                }


            }
            catch (Exception)
            {
                IntegrationApiExceptionAddError("ProgramToReplace.Id does not match the program ID for the programToReplace.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                throw IntegrationApiException;
            }
            #endregion program to replace

            //get program code
            var programCode = "";
            try
            {
                programCode = ConvertGuidToCode((await this.GetAcademicProgramsAsync(bypassCache)), stuAcadProgramsDto.NewProgram.Detail.Id);
                //make sure the programToReplace and new program is not the same.
                if (!string.IsNullOrEmpty(programCode) && !string.IsNullOrEmpty(programToRepl) && programCode == programToRepl)
                {
                    IntegrationApiExceptionAddError(string.Format("The programToReplace and newProgram refer to the same academic program - {0}. Replacement not permitted.", programCode),  "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                }
            }
            catch (Exception)
            {
                IntegrationApiExceptionAddError("Program.Id is not a valid GUID for academic-programs.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }
            if (string.IsNullOrEmpty(programCode))
            {
                IntegrationApiExceptionAddError("Program.Id is not a valid GUID for academic-programs.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }

            //get catalog code
            var catalogCode = string.Empty;
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.AcademicCatalog != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.AcademicCatalog.Id))
                {
                    IntegrationApiExceptionAddError("Catalog.id is a required property when catalog is in the message body.", "Missing.Required.Property", guid, stprKey);

                }
                try
                {
                    catalogCode = await ConvertCatalogGuidToCode(stuAcadProgramsDto.NewProgram.AcademicCatalog.Id, bypassCache);
                }
                catch (Exception e)
                {
                    IntegrationApiExceptionAddError("Catalog.Id is not a valid GUID for academic-catalogs.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                }

            }

            ////check the status
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.EnrollmentStatus != null && stuAcadProgramsDto.NewProgram.EnrollmentStatus.Detail != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.EnrollmentStatus.Detail.Id))
                {
                    IntegrationApiExceptionAddError("EnrollmentStatus.Detail.Id is a required field when detail is in the message body", "Missing.Required.Property", guid, stprKey);
                }
            }

            //get enrollment status
            string enrollStat = string.Empty;
            try
            {
                var enrollStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);

                if (enrollStatuses != null)
                {
                    //if there is detail. id is required.
                    if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.EnrollmentStatus != null)
                    {

                        if (stuAcadProgramsDto.NewProgram.EnrollmentStatus.Detail != null && !(string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.EnrollmentStatus.Detail.Id)))
                        {
                            Domain.Student.Entities.EnrollmentStatus enrollStatus = enrollStatuses.FirstOrDefault(ct => ct.Guid == stuAcadProgramsDto.NewProgram.EnrollmentStatus.Detail.Id);

                            if (enrollStatus != null)
                            {
                                //check if the detail id and the enumerable match
                                if (!enrollStatus.EnrollmentStatusType.ToString().ToUpperInvariant().Equals(stuAcadProgramsDto.NewProgram.EnrollmentStatus.EnrollStatus.ToString().ToUpperInvariant()))
                                {
                                    IntegrationApiExceptionAddError(string.Concat(" The enrollment Status of '", enrollStatus.EnrollmentStatusType.ToString(), "' referred by the detail ID '", 
                                        stuAcadProgramsDto.NewProgram.EnrollmentStatus.Detail.Id.ToString(), "' is different from that in the payload."), "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                }
                                enrollStat = enrollStatus.Code;
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("EnrollmentStatus.Detail.Id not a valid GUID for enrollment-statuses.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);

                            }
                        }
                        else //if the detail is not there, we will just pass the status to the transaction where we will figure out the status
                        {
                            switch (stuAcadProgramsDto.NewProgram.EnrollmentStatus.EnrollStatus)
                            {
                                case Dtos.EnrollmentStatusType.Active:
                                    enrollStat = stuAcadProgramsDto.NewProgram.EnrollmentStatus.EnrollStatus.ToString();
                                    break;
                                case Dtos.EnrollmentStatusType.Complete:
                                    IntegrationApiExceptionAddError("EnrollmentStatus.status of complete is not supported.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                    break;
                                case Dtos.EnrollmentStatusType.Inactive:
                                    enrollStat = stuAcadProgramsDto.NewProgram.EnrollmentStatus.EnrollStatus.ToString();
                                    break;

                            }
                        }
                    }
                }
            }
            catch
            {
                IntegrationApiExceptionAddError("Unable to retrieve enrollment-statuses.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            //create entity
            Ellucian.Colleague.Domain.Student.Entities.StudentAcademicProgram studentProgEntity = null;
            try
            {
                studentProgEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentAcademicProgram(studentId, programCode, catalogCode, guid, startDate, enrollStat);
                studentProgEntity.StudentProgramToReplace = string.Concat(studentId, "*", programToRepl);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Missing.Required.Property", guid, stprKey);
                throw IntegrationApiException;
            }
            //get Program Owner
            if (stuAcadProgramsDto.NewProgram != null &&  stuAcadProgramsDto.NewProgram.ProgramOwner != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.ProgramOwner.Id))
                {
                    IntegrationApiExceptionAddError("ProgramOwner.id is a required property when programOwner is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    var department = (depts).FirstOrDefault(s => s.Guid == stuAcadProgramsDto.NewProgram.ProgramOwner.Id);
                    if (department == null)
                    {
                        IntegrationApiExceptionAddError("ProgramOwner.id is not a valid GUID for educational-institution-units.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        studentProgEntity.DepartmentCode = department.Code;
                    }
                }
            }

            //get location code
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.Site != null)
            {
                var locationCode = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.Site.Id))
                {
                    IntegrationApiExceptionAddError("Site.id is a required property when site is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    try
                    {
                        locationCode = ConvertGuidToCode((await this.GetLocationsAsync(bypassCache)), stuAcadProgramsDto.NewProgram.Site.Id);
                    }
                    catch (Exception ex)
                    {
                        //we will catch this in next statement.
                        logger.Error(ex, "Unable to get location code.");
                    }
                    if (string.IsNullOrEmpty(locationCode) && stuAcadProgramsDto.NewProgram.Site.Id != null)
                    {
                        IntegrationApiExceptionAddError("Site.id is not a valid GUID for sites.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        studentProgEntity.Location = locationCode;
                    }
                }
            }

            //get academic level code

            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.AcademicLevel != null)
            {
                var academicLevel = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.AcademicLevel.Id))
                {
                    IntegrationApiExceptionAddError("AcademicLevel.id is a required property when site is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    try
                    {
                        academicLevel = await ConvertAcademicLevelGuidToCode(stuAcadProgramsDto.NewProgram.AcademicLevel.Id, bypassCache);
                    }
                    catch (Exception ex)
                    {
                        //we will issue exception in the next statement.
                        logger.Error(ex, "Unable to get academic level.");
                    }
                    if (string.IsNullOrEmpty(academicLevel) && stuAcadProgramsDto.NewProgram.AcademicLevel.Id != null)
                    {
                        IntegrationApiExceptionAddError("AcademicLevel.id is not a valid GUID for academic-levels.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        studentProgEntity.AcademicLevelCode = academicLevel;
                    }
                }
            }

            //get various academic periods
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.AcademicPeriods != null)
            {
                var termEntities = await this.GetTermsAsync(bypassCache);
                if (stuAcadProgramsDto.NewProgram.AcademicPeriods.Starting != null)
                {
                    var termCode = string.Empty;
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.AcademicPeriods.Starting.Id))
                    {
                        IntegrationApiExceptionAddError("AcademicPeriods.starting.id is a required property when academicPeriods.starting is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    else
                    {
                        try
                        {
                            termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.NewProgram.AcademicPeriods.Starting.Id);
                        }
                        catch (Exception ex)
                        {
                            //we will throw exception below.
                            logger.Error(ex, "Unable to get term code.");
                        }
                        if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.NewProgram.AcademicPeriods.Starting.Id != null)
                        {
                            IntegrationApiExceptionAddError("AcademicPeriods.starting.id is not a valid GUID for academic-periods.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                        }
                        else
                        {
                            studentProgEntity.StartTerm = termCode;
                        }
                    }
                }

                if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.AcademicPeriods != null && stuAcadProgramsDto.NewProgram.AcademicPeriods.ExpectedGraduation != null)
                {
                    var termCode = string.Empty;
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.AcademicPeriods.ExpectedGraduation.Id))
                    {
                        IntegrationApiExceptionAddError("AcademicPeriods.expectedGraduation.id is a required property when academicPeriods.expectedGraduation is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    else
                    {
                        try
                        {
                            termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.NewProgram.AcademicPeriods.ExpectedGraduation.Id);
                        }
                        catch (Exception ex)
                        {
                            //we will catch it in next statement
                            logger.Error(ex, "Unable to get term code.");
                        }
                        if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.NewProgram.AcademicPeriods.ExpectedGraduation.Id != null)
                        {
                            IntegrationApiExceptionAddError("AcademicPeriods.expectedGraduation.id is not a valid GUID for academic-periods.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                        }
                        else
                        {
                            //If the expectedGraduationDate is present in the payload validate that the date fits the term start/end date.
                            if (stuAcadProgramsDto.NewProgram.ExpectedGraduationDate.HasValue)
                            {
                                var termInfo = termEntities.FirstOrDefault(term => term.Code == termCode);
                                if (termInfo != null)
                                {
                                    if ((termInfo.StartDate != null && stuAcadProgramsDto.NewProgram.ExpectedGraduationDate < termInfo.StartDate) || (termInfo.EndDate != null && 
                                        stuAcadProgramsDto.NewProgram.ExpectedGraduationDate > termInfo.EndDate))
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Expected graduation academicPeriod ID '", stuAcadProgramsDto.NewProgram.AcademicPeriods.ExpectedGraduation.Id.ToString(), "' " +
                                            "is not valid for the Expected Graduation date of ", Convert.ToDateTime(stuAcadProgramsDto.NewProgram.ExpectedGraduationDate).ToString("yyyy-MM-dd")), "Validation.Exception", 
                                            stuAcadProgramsDto.Id, stprKey);
                                    }
                                }
                            }
                            studentProgEntity.AnticipatedCompletionTerm = termCode;
                        }
                    }
                }

            }

            // get degrees and certificate from the credentials in the DTO
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.Credentials != null && stuAcadProgramsDto.NewProgram.Credentials.Any())
            {
                var degrees = new List<string>();
                var credentials = await GetAcadCredentialsAsync(bypassCache);
                foreach (var cred in stuAcadProgramsDto.NewProgram.Credentials)
                {
                    var credential = credentials.FirstOrDefault(d => d.Guid == cred.Id);
                    if (credential == null)
                    {
                        //throw new ArgumentException(string.Concat(" Credential ID '", cred.Id.ToString(), "' was not found. Valid Credential is required."));
                        IntegrationApiExceptionAddError("Credentials.id is not a valid GUID for academic-credentials.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        var type = credential.AcademicCredentialType;
                        switch (type)
                        {
                            case Domain.Base.Entities.AcademicCredentialType.Certificate:
                                studentProgEntity.AddCcds(credential.Code);
                                break;
                            case Domain.Base.Entities.AcademicCredentialType.Degree:
                                degrees.Add(credential.Code);
                                break;
                            //produce error if honor codes are included
                            case Domain.Base.Entities.AcademicCredentialType.Honorary:
                                IntegrationApiExceptionAddError("Credentials.id of type honor is not supported.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                break;

                            case Domain.Base.Entities.AcademicCredentialType.Diploma:
                                IntegrationApiExceptionAddError("Credentials.id of type diploma is not supported.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                break;
                        }
                    }

                }

                //if there is more than one degree in the payload, produce an error
                if (degrees.Count > 1)
                {
                    IntegrationApiExceptionAddError("Credentials array cannot have more than one degree.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                }
                else
                {
                    studentProgEntity.DegreeCode = degrees.FirstOrDefault();
                }
            }

            //get the displicines which included majors, minors, specializations
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.Disciplines != null && stuAcadProgramsDto.NewProgram.Disciplines.Any())
            {
                var disciplines = await GetAcademicDisciplinesAsync(bypassCache);
                foreach (var dis in stuAcadProgramsDto.NewProgram.Disciplines)
                {

                    if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        IntegrationApiExceptionAddError("Disciplines.discipline.id is a required property when disciplines.discipline is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    var discipline = disciplines.FirstOrDefault(d => d.Guid == dis.Discipline.Id);
                    if (discipline == null)
                    {
                        IntegrationApiExceptionAddError("Disciplines.discipline.id is not a valid GUID for academic-disciplines.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        switch (discipline.AcademicDisciplineType)
                        {
                            //getting majors
                            case Domain.Base.Entities.AcademicDisciplineType.Major:
                                studentProgEntity.AddMajors(discipline.Code, dis.StartOn, dis.EndOn);
                                break;
                            //getting minors
                            case Domain.Base.Entities.AcademicDisciplineType.Minor:
                                studentProgEntity.AddMinors(discipline.Code, dis.StartOn, dis.EndOn);
                                break;
                            //getting specializations
                            case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                                studentProgEntity.AddSpecializations(discipline.Code, dis.StartOn, dis.EndOn);
                                break;
                        }
                    }
                    //check end date is not before start date
                    if (dis.EndOn != null && dis.EndOn < dis.StartOn)
                    {
                        IntegrationApiExceptionAddError("The requested discipline " + dis.Discipline.Id + " endOn must be on or after the discipline startOn.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);

                    }

                }

            }
            //process End date
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.EndOn != null)
            {
                studentProgEntity.EndDate = Convert.ToDateTime(stuAcadProgramsDto.NewProgram.EndOn.Value.ToString("yyyy-MM-dd"));
            }

            //process expected graduation date
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.ExpectedGraduationDate.HasValue)
            {
                studentProgEntity.AnticipatedCompletionDate = Convert.ToDateTime(stuAcadProgramsDto.NewProgram.ExpectedGraduationDate.Value.ToString("yyyy-MM-dd"));
            }

            // process admit status
            if (stuAcadProgramsDto.NewProgram != null && stuAcadProgramsDto.NewProgram.AdmissionClassification != null)
            {
                var admitStatus = string.Empty;
                if (stuAcadProgramsDto.NewProgram.AdmissionClassification.AdmissionCategory == null)
                {
                    IntegrationApiExceptionAddError("AdmissionClassification.admissionCategory is a required property when admissionClassification is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.NewProgram.AdmissionClassification.AdmissionCategory.Id))
                    {
                        IntegrationApiExceptionAddError("AdmissionClassification.admissionCategory.id is a required property when admissionClassification.admissionCategory is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    else
                    {
                        try
                        {
                            admitStatus = await ConvertAdmissionClassificationGuidToCode(stuAcadProgramsDto.NewProgram.AdmissionClassification.AdmissionCategory.Id, bypassCache);
                        }
                        catch (Exception ex)
                        {
                            //we will throw the exception in the next statement
                            logger.Error(ex, "Unable to get admit status.");
                        }
                        if (string.IsNullOrEmpty(admitStatus) && stuAcadProgramsDto.NewProgram.AdmissionClassification.AdmissionCategory.Id != null)
                        {
                            IntegrationApiExceptionAddError("AdmissionClassification.admissionCategory.id is not a valid GUID for admission-populations.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                        }
                        else
                        {
                            studentProgEntity.AdmitStatus = admitStatus;
                        }
                    }
                }

            }
            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return studentProgEntity;
        }

        /// <summary>
        /// Update a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgDto">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the updated student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms> UpdateStudentAcademicProgramAsync(Dtos.StudentAcademicPrograms studentAcadProgDto, bool bypassCache = false)
        {
            
            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgEntity = await ConvertStudentAcademicProgramDtoToEntityAsync(studentAcadProgDto, bypassCache);
            var createdStuAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStuAcadProgEntity.Add(await _studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcadProgEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto(createdStuAcadProgEntity, bypassCache)).FirstOrDefault();
        }

        /// <summary>
        /// Update a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgDto2">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the updated student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms2> UpdateStudentAcademicProgram2Async(Dtos.StudentAcademicPrograms2 studentAcadProgDto, bool bypassCache = false)
        {
            
            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgEntity = await ConvertStudentAcademicProgramDtoToEntity2Async(studentAcadProgDto, bypassCache);
            var createdStuAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStuAcadProgEntity.Add(await _studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcadProgEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto2Async(createdStuAcadProgEntity, bypassCache)).FirstOrDefault();
        }

        /// <summary>
        /// Update a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgDto2">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the updated student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms4> UpdateStudentAcademicProgramSubmissionAsync(Dtos.StudentAcademicProgramsSubmissions studentAcadProgDto, bool bypassCache = false)
        {
           
            //check for errors
            string stprKey = string.Empty;
            string guid = studentAcadProgDto.Id;
            if (!string.IsNullOrEmpty(guid))
            {
                guid = guid.ToLowerInvariant();
                try
                {
                    stprKey = await _studentAcademicProgramRepository.GetStudentAcademicProgramIdFromGuidAsync(guid);
                }
                catch (Exception ex)
                {
                    // Fall through with a null stcKey in case we are doing PUT with a new GUID.
                    logger.Error(ex, "Unable to get academic program for stprKey.");
                }
            }

            ValidateStudentAcademicProgramsSubmissions(guid, stprKey, studentAcadProgDto);

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgEntity = await ConvertStudentAcademicProgramSubmissionDtoToEntityAsync(studentAcadProgDto, stprKey, bypassCache);

            var createdStuAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStuAcadProgEntity.Add(await _studentAcademicProgramRepository.UpdateStudentAcademicProgram2Async(studentAcadProgEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto4Async(createdStuAcadProgEntity, bypassCache)).FirstOrDefault();
        }


        /// <summary>
        /// Validates the data in the StudentAcademicPrograms object
        /// </summary>
        /// <param name="stuAcadProg">StudentAcademicPrograms from the request</param>
        private void ValidateStudentAcademicProgramsSubmissions(string guid, string stprKey, Dtos.StudentAcademicProgramsSubmissions stuAcadProg)
        {
            if (stuAcadProg == null)
            {

                IntegrationApiExceptionAddError("Must provide a StudentAcademicProgramsSubmissions object for update", "Missing.Required.Property", guid, stprKey);
                throw IntegrationApiException;
            }

            if (stuAcadProg.AcademicProgram == null || string.IsNullOrEmpty(stuAcadProg.AcademicProgram.Id))
            {
                IntegrationApiExceptionAddError("Program.id is a required property", "Missing.Required.Property", guid, stprKey);
            }
            if (stuAcadProg.Student == null || string.IsNullOrEmpty(stuAcadProg.Student.Id))
            {
               IntegrationApiExceptionAddError("Student.id is a required property", "Missing.Required.Property", guid, stprKey);
            }
            //validate curriculumObjective
            if (stuAcadProg.CurriculumObjective == null || stuAcadProg.CurriculumObjective == Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.NotSet)
            {
                IntegrationApiExceptionAddError("CurriculumObjective is a required property", "Missing.Required.Property", guid, stprKey);
            }
            else
            {
                if (stuAcadProg.CurriculumObjective != Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated)
                {
                    IntegrationApiExceptionAddError("CurriculumObjective must be set to 'matriculated' for any new or updated student programs.", "Validation.Exception", guid, stprKey);
                }
            }
            //validate preference
            if (stuAcadProg.StartOn == null)
            {
                IntegrationApiExceptionAddError("StartOn is required to create or update a matriculated student program.", "Missing.Required.Property", guid, stprKey);

            }
            if (stuAcadProg.EnrollmentStatus == null  || (stuAcadProg.EnrollmentStatus != null && string.IsNullOrEmpty(stuAcadProg.EnrollmentStatus.EnrollStatus.ToString())))
            {
                IntegrationApiExceptionAddError("EnrollmentStatus is a required property", "Missing.Required.Property", guid, stprKey);
            }
            //check end date is not before start date
            if (stuAcadProg.EndOn != null && stuAcadProg.EndOn < stuAcadProg.StartOn)
            {
                IntegrationApiExceptionAddError("EndOn cannot be before startOn.", "Validation.Exception", guid, stprKey);

            }
            //check exptected graduation date is not before start date
            if (stuAcadProg.ExpectedGraduationDate != null && stuAcadProg.ExpectedGraduationDate < stuAcadProg.StartOn)
            {
                IntegrationApiExceptionAddError("ExpectedGraduationDate cannot be before startOn.", "Validation.Exception", guid, stprKey);
            }

            //if the enrollment status is inactive, then the end date is required.
            if (stuAcadProg.EndOn == null && stuAcadProg.EnrollmentStatus != null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Inactive)
            {
                IntegrationApiExceptionAddError("EndOn is required for the enrollment status of inactive.", "Validation.Exception", guid, stprKey);
            }
            // the status of complete is not valid for PUT/POST
            if (stuAcadProg.EnrollmentStatus != null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Complete)
            {
                IntegrationApiExceptionAddError("Enrollment status of complete is not supported. Graduation processing can only be invoked directly in Colleague.", "Validation.Exception", guid, stprKey);
            }

            // the preference of primary is not supported for PUT/POST
            if (stuAcadProg.Preference == Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary)
            {
                IntegrationApiExceptionAddError("Preference may not be set for a student's program.", "Validation.Exception", guid, stprKey);
            }

            //the status of active cannot have end date
            if (stuAcadProg.EndOn != null && stuAcadProg.EnrollmentStatus != null && stuAcadProg.EnrollmentStatus.EnrollStatus == Dtos.EnrollmentStatusType.Active)
            {
                IntegrationApiExceptionAddError("EndOn is not valid for the enrollment status of active.", "Validation.Exception", guid, stprKey);
            }
            //check the credentials body is good.
            if (stuAcadProg.Credentials != null && stuAcadProg.Credentials.Count > 0)
            {
                foreach (var cred in stuAcadProg.Credentials)
                {
                    if (cred == null || string.IsNullOrEmpty(cred.Id))
                    {
                       IntegrationApiExceptionAddError("Credential id is a required field when credentials are in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                }
            }

            //check displines body is good.
            if (stuAcadProg.Disciplines != null && stuAcadProg.Disciplines.Count > 0)
            {
                foreach (var dis in stuAcadProg.Disciplines)
                {
                    if (dis.Discipline == null)
                    {
                        IntegrationApiExceptionAddError("Discipline is a required property when disciplines are in the message body.", "Missing.Required.Property", guid, stprKey);

                    }
                    else if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        IntegrationApiExceptionAddError("Discipline id is a required property when discipline is in the message body.", "Missing.Required.Property", guid, stprKey);

                    }

                }
            }
            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
        }

       
        /// <summary>
        /// Converts a Student Academic Program DTO to its corresponding Student Programs domain entity
        /// </summary>
        /// <param name="stuAcadProgramsDto">List of student academic program DTOs</param>
        /// <returns>A<see cref="Domain.Student.Entities.StudentAcademicProgram">Student Program</see> domain entity</returns>
        private async Task<Domain.Student.Entities.StudentAcademicProgram> ConvertStudentAcademicProgramDtoToEntityAsync(Ellucian.Colleague.Dtos.StudentAcademicPrograms stuAcadProgramsDto, bool bypassCache = false)
        {
            // handle empty guid
            var guid = string.Empty;
            var depts = await this.GetDepartmentsAsync(bypassCache);
            if (!string.Equals(stuAcadProgramsDto.Id, Guid.Empty.ToString()))
            {
                guid = stuAcadProgramsDto.Id;
            }
            var startDate = new DateTime();
            if (stuAcadProgramsDto.StartDate != null)
            {
                startDate = Convert.ToDateTime(stuAcadProgramsDto.StartDate.Value.ToString("yyyy-MM-dd"));
            }
            var personId = await _personRepository.GetPersonIdFromGuidAsync(stuAcadProgramsDto.Student.Id);
            if ((string.IsNullOrEmpty(personId)) || (await _personRepository.IsCorpAsync(personId)))
            {
                throw new ArgumentException(string.Concat(" Student ID '", stuAcadProgramsDto.Student.Id.ToString(), "'  is not a valid persons guid."));
            }
            //get program code
            var programCode = "";

            try
            {
                programCode = ConvertGuidToCode((await this.GetAcademicProgramsAsync(bypassCache)), stuAcadProgramsDto.Program.Id);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(string.Concat(" Program ID '", stuAcadProgramsDto.Program.Id.ToString(), "' was not found. Valid Program is required."));
            }
            //get catalog code
            var catalogCode = string.Empty;
            if (stuAcadProgramsDto.Catalog != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.Catalog.Id))
                {
                    throw new ArgumentException("Catalog id is a required field when Catalog is in the message body.");
                }
                try
                {
                    catalogCode = await ConvertCatalogGuidToCode(stuAcadProgramsDto.Catalog.Id, bypassCache);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(string.Concat(" Catalog ID '", stuAcadProgramsDto.Catalog.Id.ToString(), "' was not found. Valid Catalog is required."));
                }

            }

            ////check the status
            if (stuAcadProgramsDto.EnrollmentStatus.Detail != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.EnrollmentStatus.Detail.Id))
                {
                    throw new ArgumentException("Enrollment status detail id is a required field when detail is in the message body.");
                }
            }

            //get enrollment status
            string enrollStat = string.Empty;

            var enrollStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);
            if (enrollStatuses != null)
            {
                //if there is detail. id is required.

                if (stuAcadProgramsDto.EnrollmentStatus.Detail != null && !(string.IsNullOrEmpty(stuAcadProgramsDto.EnrollmentStatus.Detail.Id)))
                {
                    Domain.Student.Entities.EnrollmentStatus enrollStatus = enrollStatuses.FirstOrDefault(ct => ct.Guid == stuAcadProgramsDto.EnrollmentStatus.Detail.Id);

                    if (enrollStatus != null)
                    {
                        //check if the detail id and the enumerable match
                        if (!enrollStatus.EnrollmentStatusType.ToString().ToUpperInvariant().Equals(stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString().ToUpperInvariant()))
                        {
                            throw new ArgumentException(string.Concat(" The enrollment Status of '", enrollStatus.EnrollmentStatusType.ToString(), "' referred by the detail ID '", stuAcadProgramsDto.EnrollmentStatus.Detail.Id.ToString(), "' is different from that in the payload. "));
                        }
                        enrollStat = enrollStatus.Code;
                    }
                    else
                    {
                        throw new ArgumentException(string.Concat(" Enrollment Status Detail ID '", stuAcadProgramsDto.EnrollmentStatus.Detail.Id.ToString(), "' was not found. Valid detail ID is required."));

                    }
                }


                //if the detail is not there, we will just pass the status to the transaction where we will figure out the status
                else
                {
                    switch (stuAcadProgramsDto.EnrollmentStatus.EnrollStatus)
                    {
                        case Dtos.EnrollmentStatusType.Active:
                            enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                            break;
                        case Dtos.EnrollmentStatusType.Complete:
                            enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                            break;
                        case Dtos.EnrollmentStatusType.Inactive:
                            enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                            break;
                            //default:
                            //throw new ArgumentException(string.Concat(" Enrollment Status '", acadProgEnroll.EnrollmentStatus.EnrollStatus.ToString(), "' was not found. Valid enrollment status type is required."));
                    }
                }
            }

            //create entity
            var studentProgEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentAcademicProgram(personId, programCode, catalogCode, guid, startDate, enrollStat);
            //get Program Owner
            if (stuAcadProgramsDto.ProgramOwner != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.ProgramOwner.Id))
                {
                    throw new ArgumentException("Program Owner ID is required when ProgramOwner is in the message body.");
                }
                var department = (depts).FirstOrDefault(s => s.Guid == stuAcadProgramsDto.ProgramOwner.Id);
                if (department == null)
                {
                    throw new ArgumentException(string.Concat(" Program Owner ID '", stuAcadProgramsDto.ProgramOwner.Id.ToString(), "' was not found. Valid Program Owner is required."));
                }
                else
                {
                    studentProgEntity.DepartmentCode = department.Code;
                }
            }


            //get location code
            if (stuAcadProgramsDto.Site != null)
            {
                var locationCode = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.Site.Id))
                {
                    throw new ArgumentException("Site id is a required field when Site is in the message body.");
                }
                locationCode = ConvertGuidToCode((await this.GetLocationsAsync(bypassCache)), stuAcadProgramsDto.Site.Id);
                if (string.IsNullOrEmpty(locationCode) && stuAcadProgramsDto.Site.Id != null)
                {
                    throw new ArgumentException(string.Concat(" Location ID '", stuAcadProgramsDto.Site.Id.ToString(), "' was not found. Valid Location is required."));
                }
                else
                {
                    studentProgEntity.Location = locationCode;
                }
            }

            //get academic level code

            if (stuAcadProgramsDto.AcademicLevel != null)
            {
                var academicLevel = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicLevel.Id))
                {
                    throw new ArgumentException("Academic Level Id is a required field when Academic Level is in the message body.");
                }
                academicLevel = await ConvertAcademicLevelGuidToCode(stuAcadProgramsDto.AcademicLevel.Id, bypassCache);
                if (string.IsNullOrEmpty(academicLevel) && stuAcadProgramsDto.AcademicLevel.Id != null)
                {
                    throw new ArgumentException(string.Concat(" Academic Level Id '", stuAcadProgramsDto.AcademicLevel.Id.ToString(), "' was not found. Valid Academic Level is required."));
                }
                else
                {
                    studentProgEntity.AcademicLevelCode = academicLevel;
                }
            }

            //get start term code

            if (stuAcadProgramsDto.StartTerm != null)
            {
                var termCode = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.StartTerm.Id))
                {
                    throw new ArgumentException("academicPeriod id is a required field when academicPeriod is in the message body.");
                }
                //var termEntities = await this.GetTermsAsync(bypassCache);
                termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.StartTerm.Id);
                if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.StartTerm.Id != null)
                {
                    throw new ArgumentException(string.Concat(" academicPeriod ID '", stuAcadProgramsDto.StartTerm.Id.ToString(), "' was not found. Valid academicPeriod is required."));
                }
                else
                {
                    studentProgEntity.StartTerm = termCode;
                }
            }

            // get degrees and certificate from the credentials in the DTO
            if (stuAcadProgramsDto.Credentials != null && stuAcadProgramsDto.Credentials.Any())
            {
                var degrees = new List<string>();
                var credentials = await GetAcadCredentialsAsync(bypassCache);
                foreach (var cred in stuAcadProgramsDto.Credentials)
                {
                    var credential = credentials.FirstOrDefault(d => d.Guid == cred.Id);
                    if (credential == null)
                    {
                        throw new ArgumentException(string.Concat(" Credential ID '", cred.Id.ToString(), "' was not found. Valid Credential is required."));
                    }
                    var type = credential.AcademicCredentialType;
                    switch (type)
                    {
                        case Domain.Base.Entities.AcademicCredentialType.Certificate:
                            studentProgEntity.AddCcds(credential.Code);
                            break;
                        case Domain.Base.Entities.AcademicCredentialType.Degree:
                            degrees.Add(credential.Code);
                            break;
                        //produce error if honor codes are included
                        case Domain.Base.Entities.AcademicCredentialType.Honorary:
                            throw new ArgumentException(credential.Guid + " is an Honor code. Honor code is not allowed during Student Academic Program.");

                        case Domain.Base.Entities.AcademicCredentialType.Diploma:
                            throw new ArgumentException(credential.Guid + " is a Diploma. Diploma is not allowed during Student Academic Program.");
                    }

                }

                //if there is more than one degree in the payload, produce an error
                if (degrees.Count > 1)
                {
                    throw new ArgumentException("The payload cannot have more than one degree under credentials.");
                }
                else
                {
                    studentProgEntity.DegreeCode = degrees.FirstOrDefault();
                }
            }

            //get the displicines which included majors, minors, specializations
            if (stuAcadProgramsDto.Disciplines != null && stuAcadProgramsDto.Disciplines.Any())
            {
                var disciplines = await GetAcademicDisciplinesAsync(bypassCache);
                var administerDepts = new List<string>();
                foreach (var dis in stuAcadProgramsDto.Disciplines)
                {

                    if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        throw new ArgumentException("discipline id is a required field when discipline is in the message body.");
                    }
                    var discipline = disciplines.FirstOrDefault(d => d.Guid == dis.Discipline.Id);
                    if (discipline == null)
                    {
                        throw new ArgumentException(string.Concat(" Discipline ID '", dis.Discipline.Id.ToString(), "' was not found. Valid Discipline ID is required."));
                    }
                    else
                    {
                        switch (discipline.AcademicDisciplineType)
                        {
                            //getting majors
                            case Domain.Base.Entities.AcademicDisciplineType.Major:
                                studentProgEntity.AddMajors(discipline.Code);
                                break;
                            //getting minors
                            case Domain.Base.Entities.AcademicDisciplineType.Minor:
                                studentProgEntity.AddMinors(discipline.Code);
                                break;
                            //getting specializations
                            case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                                studentProgEntity.AddSpecializations(discipline.Code);
                                break;
                        }
                    }

                    if (dis.AdministeringInstitutionUnit != null)
                    {
                        if (string.IsNullOrEmpty(dis.AdministeringInstitutionUnit.Id))
                        {
                            throw new ArgumentException("Administering Institution Unit Id is a required field when Administering Institution Unit is in the message body.");
                        }
                        var administerUnit = (depts).FirstOrDefault(s => s.Guid == dis.AdministeringInstitutionUnit.Id);
                        if (administerUnit == null && dis.AdministeringInstitutionUnit.Id != null)
                        {
                            throw new ArgumentException(string.Concat(" Administering Institution Unit Id '", dis.AdministeringInstitutionUnit.Id, "' was not found. Valid Administering Institution Unit is required."));
                        }
                        else
                        {
                            if (!administerDepts.Contains(administerUnit.Code))
                            {
                                administerDepts.Add(administerUnit.Code);
                            }
                        }

                    }

                }
                //we can have just one administering unit 
                if (administerDepts.Count > 1)
                {
                    throw new ArgumentException("Only one administering Institution Unit is supported.");
                }
                else
                {
                    var dept = administerDepts.FirstOrDefault();
                    //this department needs to be same as ProgramOwner
                    if (!string.Equals(dept, studentProgEntity.DepartmentCode))
                        throw new ArgumentException("ProgramOwner and Administering Institution Unit needs to be same.");
                }
            }
            //process End date
            if (stuAcadProgramsDto.EndDate != null)
            {
                studentProgEntity.EndDate = Convert.ToDateTime(stuAcadProgramsDto.EndDate.Value.ToString("yyyy-MM-dd"));
            }
            //process fields from acad.credentials
            if (!string.IsNullOrEmpty(stuAcadProgramsDto.PerformanceMeasure))
            {
                decimal perfParse;
                if (decimal.TryParse(stuAcadProgramsDto.PerformanceMeasure, out perfParse))
                {
                    studentProgEntity.GradGPA = perfParse;
                }
                else
                {
                    throw new ArgumentException("PerformanceMeasure needs to be a decimal.");
                }
            }
            //process recognitions
            if (stuAcadProgramsDto.Recognitions != null && stuAcadProgramsDto.Recognitions.Any())
            {
                var honors = await GetOtherHonorsAsync(bypassCache);
                foreach (var honor in stuAcadProgramsDto.Recognitions)
                {
                    if (string.IsNullOrEmpty(honor.Id))
                    {
                        throw new ArgumentException("Recognitions ID is a required field when recognition is in the message body.");
                    }

                    var honor_ = honors.FirstOrDefault(d => d.Guid == honor.Id);
                    if (honor_ == null)
                    {
                        throw new ArgumentException(string.Concat(" Recognition ID '", honor.Id.ToString(), "' was not found. Valid recognition is required."));
                    }
                    else
                    {
                        studentProgEntity.AddHonors(honor_.Code);
                    }

                }
            }
            //process graduation date
            if (stuAcadProgramsDto.GraduatedOn != null)
            {
                studentProgEntity.GraduationDate = Convert.ToDateTime(stuAcadProgramsDto.GraduatedOn.Value.ToString("yyyy-MM-dd"));
            }
            //process credentials date
            if (stuAcadProgramsDto.CredentialsDate != null)
            {
                studentProgEntity.CredentialsDate = Convert.ToDateTime(stuAcadProgramsDto.CredentialsDate.Value.ToString("yyyy-MM-dd"));
            }
            //process thesis title
            studentProgEntity.ThesisTitle = stuAcadProgramsDto.ThesisTitle;
            studentProgEntity.CreditsEarned = stuAcadProgramsDto.CreditsEarned;
            return studentProgEntity;

        }

        /// <summary>
        /// Converts a Student Academic Program DTO to its corresponding Student Programs domain entity
        /// </summary>
        /// <param name="stuAcadProgramsDto">List of student academic program DTOs</param>
        /// <returns>A<see cref="Domain.Student.Entities.StudentAcademicProgram2">Student Program</see> domain entity</returns>
        private async Task<Domain.Student.Entities.StudentAcademicProgram> ConvertStudentAcademicProgramDtoToEntity2Async(Ellucian.Colleague.Dtos.StudentAcademicPrograms2 stuAcadProgramsDto, bool bypassCache = false)
        {
            // handle empty guid
            var guid = string.Empty;
            var depts = await this.GetDepartmentsAsync(bypassCache);
            if (!string.Equals(stuAcadProgramsDto.Id, Guid.Empty.ToString()))
            {
                guid = stuAcadProgramsDto.Id;
            }
            var startDate = new DateTime();
            if (stuAcadProgramsDto.StartDate != null)
            {
                startDate = Convert.ToDateTime(stuAcadProgramsDto.StartDate.Value.ToString("yyyy-MM-dd"));
            }
            var personId = await _personRepository.GetPersonIdFromGuidAsync(stuAcadProgramsDto.Student.Id);
            if ((string.IsNullOrEmpty(personId)) || (await _personRepository.IsCorpAsync(personId)))
            {
                throw new ArgumentException(string.Concat(" Student ID '", stuAcadProgramsDto.Student.Id.ToString(), "' is not a valid persons guid."));
            }
            //get program code
            var programCode = "";
            try
            {
                programCode = ConvertGuidToCode((await this.GetAcademicProgramsAsync(bypassCache)), stuAcadProgramsDto.AcademicProgram.Id);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(string.Concat(" Program ID '", stuAcadProgramsDto.AcademicProgram.Id.ToString(), "' was not found. Valid Program is required."));
            }

            //get catalog code
            var catalogCode = string.Empty;
            if (stuAcadProgramsDto.AcademicCatalog != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicCatalog.Id))
                {
                    throw new ArgumentException("Catalog id is a required field when Catalog is in the message body.");
                }
                try
                {
                    catalogCode = await ConvertCatalogGuidToCode(stuAcadProgramsDto.AcademicCatalog.Id, bypassCache);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(string.Concat(" Catalog ID '", stuAcadProgramsDto.AcademicCatalog.Id.ToString(), "' was not found. Valid Catalog is required."));
                }
                //catalogCode = await ConvertCatalogGuidToCode(stuAcadProgramsDto.Catalog.Id, bypassCache);
                //if (string.IsNullOrEmpty(catalogCode) && stuAcadProgramsDto.Catalog.Id != null)
                //{
                //    throw new ArgumentException(string.Concat(" Catalog ID '", stuAcadProgramsDto.Catalog.Id.ToString(), "' was not found. Valid Catalog is required."));
                //}
            }

            ////check the status
            if (stuAcadProgramsDto.EnrollmentStatus != null && stuAcadProgramsDto.EnrollmentStatus.Detail != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.EnrollmentStatus.Detail.Id))
                {
                    throw new ArgumentException("Enrollment status detail id is a required field when detail is in the message body.");
                }
            }

            //get enrollment status
            string enrollStat = string.Empty;

            var enrollStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);
            if (enrollStatuses != null)
            {
                //if there is detail. id is required.
                if (stuAcadProgramsDto.EnrollmentStatus != null)
                {

                    if (stuAcadProgramsDto.EnrollmentStatus.Detail != null && !(string.IsNullOrEmpty(stuAcadProgramsDto.EnrollmentStatus.Detail.Id)))
                    {
                        Domain.Student.Entities.EnrollmentStatus enrollStatus = enrollStatuses.FirstOrDefault(ct => ct.Guid == stuAcadProgramsDto.EnrollmentStatus.Detail.Id);

                        if (enrollStatus != null)
                        {
                            //check if the detail id and the enumerable match
                            if (!enrollStatus.EnrollmentStatusType.ToString().ToUpperInvariant().Equals(stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString().ToUpperInvariant()))
                            {
                                throw new ArgumentException(string.Concat(" The enrollment Status of '", enrollStatus.EnrollmentStatusType.ToString(), "' referred by the detail ID '", stuAcadProgramsDto.EnrollmentStatus.Detail.Id.ToString(), "' is different from that in the payload. "));
                            }
                            enrollStat = enrollStatus.Code;
                        }
                        else
                        {
                            throw new ArgumentException(string.Concat(" Enrollment Status Detail ID '", stuAcadProgramsDto.EnrollmentStatus.Detail.Id.ToString(), "' was not found. Valid detail ID is required."));

                        }
                    }


                    //if the detail is not there, we will just pass the status to the transaction where we will figure out the status
                    else
                    {
                        switch (stuAcadProgramsDto.EnrollmentStatus.EnrollStatus)
                        {
                            case Dtos.EnrollmentStatusType.Active:
                                enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                                break;
                            case Dtos.EnrollmentStatusType.Complete:
                                enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                                break;
                            case Dtos.EnrollmentStatusType.Inactive:
                                enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                                break;
                                //default:
                                //throw new ArgumentException(string.Concat(" Enrollment Status '", acadProgEnroll.EnrollmentStatus.EnrollStatus.ToString(), "' was not found. Valid enrollment status type is required."));
                        }
                    }
                }
            }

            //create entity
            var studentProgEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentAcademicProgram(personId, programCode, catalogCode, guid, startDate, enrollStat);
            //get Program Owner
            if (stuAcadProgramsDto.ProgramOwner != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.ProgramOwner.Id))
                {
                    throw new ArgumentException("Program Owner ID is required when ProgramOwner is in the message body.");
                }
                var department = (depts).FirstOrDefault(s => s.Guid == stuAcadProgramsDto.ProgramOwner.Id);
                if (department == null)
                {
                    throw new ArgumentException(string.Concat(" Program Owner ID '", stuAcadProgramsDto.ProgramOwner.Id.ToString(), "' was not found. Valid Program Owner is required."));
                }
                else
                {
                    studentProgEntity.DepartmentCode = department.Code;
                }
            }

            //process preference data
            if (stuAcadProgramsDto.Preference == Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary)
            {
                studentProgEntity.IsPrimary = true;
            }

            //get location code
            if (stuAcadProgramsDto.Site != null)
            {
                var locationCode = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.Site.Id))
                {
                    throw new ArgumentException("Site id is a required field when Site is in the message body.");
                }
                locationCode = ConvertGuidToCode((await this.GetLocationsAsync(bypassCache)), stuAcadProgramsDto.Site.Id);
                if (string.IsNullOrEmpty(locationCode) && stuAcadProgramsDto.Site.Id != null)
                {
                    throw new ArgumentException(string.Concat(" Location ID '", stuAcadProgramsDto.Site.Id.ToString(), "' was not found. Valid Location is required."));
                }
                else
                {
                    studentProgEntity.Location = locationCode;
                }
            }

            //get academic level code

            if (stuAcadProgramsDto.AcademicLevel != null)
            {
                var academicLevel = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicLevel.Id))
                {
                    throw new ArgumentException("Academic Level Id is a required field when Academic Level is in the message body.");
                }
                academicLevel = await ConvertAcademicLevelGuidToCode(stuAcadProgramsDto.AcademicLevel.Id, bypassCache);
                if (string.IsNullOrEmpty(academicLevel) && stuAcadProgramsDto.AcademicLevel.Id != null)
                {
                    throw new ArgumentException(string.Concat(" Academic Level Id '", stuAcadProgramsDto.AcademicLevel.Id.ToString(), "' was not found. Valid Academic Level is required."));
                }
                else
                {
                    studentProgEntity.AcademicLevelCode = academicLevel;
                }
            }

            //get various academic periods
            if (stuAcadProgramsDto.AcademicPeriods != null)
            {
                var termEntities = await this.GetTermsAsync(bypassCache);
                if (stuAcadProgramsDto.AcademicPeriods.Starting != null)
                {
                    var termCode = string.Empty;
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicPeriods.Starting.Id))
                    {
                        throw new ArgumentException("Starting academicPeriod id is a required field when starting academicPeriod is in the message body.");
                    }
                    termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.AcademicPeriods.Starting.Id);
                    if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.AcademicPeriods.Starting.Id != null)
                    {
                        throw new ArgumentException(string.Concat(" Starting academicPeriod ID '", stuAcadProgramsDto.AcademicPeriods.Starting.Id.ToString(), "' was not found. Valid academicPeriod is required."));
                    }
                    else
                    {
                        studentProgEntity.StartTerm = termCode;
                    }
                }

                if (stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation != null)
                {
                    var termCode = string.Empty;
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id))
                    {
                        throw new ArgumentException("Expected graduation academicPeriod id is a required field when expected graduation academicPeriod is in the message body.");
                    }
                    termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id);
                    if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id != null)
                    {
                        throw new ArgumentException(string.Concat(" Expected graduation academicPeriod ID '", stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id.ToString(), "' was not found. Valid academicPeriod is required."));
                    }
                    else
                    {
                        //If the expectedGraduationDate is present in the payload validate that the date fits the term start/end date.
                        if (stuAcadProgramsDto.ExpectedGraduationDate.HasValue)
                        {
                            var termInfo = termEntities.FirstOrDefault(term => term.Code == termCode);
                            if (termInfo != null)
                            {
                                if ((termInfo.StartDate != null && stuAcadProgramsDto.ExpectedGraduationDate < termInfo.StartDate) || (termInfo.EndDate != null && stuAcadProgramsDto.ExpectedGraduationDate > termInfo.EndDate))
                                {
                                    throw new ArgumentException(string.Concat(" Expected graduation academicPeriod ID '", stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id.ToString(), "' is not valid for the Expected Graduation date of ", Convert.ToDateTime(stuAcadProgramsDto.ExpectedGraduationDate).ToString("yyyy-MM-dd")));
                                }
                            }
                        }
                        studentProgEntity.AnticipatedCompletionTerm = termCode;
                    }
                }
                if (stuAcadProgramsDto.AcademicPeriods.ActualGraduation != null)
                {
                    var termCode = string.Empty;
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicPeriods.ActualGraduation.Id))
                    {
                        throw new ArgumentException("Actual Graduation academicPeriod id is a required field when Actual Graduation academicPeriod is in the message body.");
                    }
                    termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.AcademicPeriods.ActualGraduation.Id);
                    if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.AcademicPeriods.ActualGraduation.Id != null)
                    {
                        throw new ArgumentException(string.Concat(" Actual Graduation academicPeriod ID '", stuAcadProgramsDto.AcademicPeriods.ActualGraduation.Id.ToString(), "' was not found. Valid academicPeriod is required."));
                    }
                    else
                    {
                        studentProgEntity.GradTerm = termCode;
                    }
                }
            }

            // get degrees and certificate from the credentials in the DTO
            if (stuAcadProgramsDto.Credentials != null && stuAcadProgramsDto.Credentials.Any())
            {
                var degrees = new List<string>();
                var credentials = await GetAcadCredentialsAsync(bypassCache);
                foreach (var cred in stuAcadProgramsDto.Credentials)
                {
                    var credential = credentials.FirstOrDefault(d => d.Guid == cred.Id);
                    if (credential == null)
                    {
                        throw new ArgumentException(string.Concat(" Credential ID '", cred.Id.ToString(), "' was not found. Valid Credential is required."));
                    }
                    var type = credential.AcademicCredentialType;
                    switch (type)
                    {
                        case Domain.Base.Entities.AcademicCredentialType.Certificate:
                            studentProgEntity.AddCcds(credential.Code);
                            break;
                        case Domain.Base.Entities.AcademicCredentialType.Degree:
                            degrees.Add(credential.Code);
                            break;
                        //produce error if honor codes are included
                        case Domain.Base.Entities.AcademicCredentialType.Honorary:
                            throw new ArgumentException(credential.Guid + " is an Honor code. Honor code is not allowed during Student Academic Program.");

                        case Domain.Base.Entities.AcademicCredentialType.Diploma:
                            throw new ArgumentException(credential.Guid + " is a Diploma. Diploma is not allowed during Student Academic Program.");
                    }

                }

                //if there is more than one degree in the payload, produce an error
                if (degrees.Count > 1)
                {
                    throw new ArgumentException("The payload cannot have more than one degree under credentials.");
                }
                else
                {
                    studentProgEntity.DegreeCode = degrees.FirstOrDefault();
                }
            }

            //get the displicines which included majors, minors, specializations
            if (stuAcadProgramsDto.Disciplines != null && stuAcadProgramsDto.Disciplines.Any())
            {
                var disciplines = await GetAcademicDisciplinesAsync(bypassCache);
                var administerDepts = new List<string>();
                foreach (var dis in stuAcadProgramsDto.Disciplines)
                {

                    if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        throw new ArgumentException("discipline id is a required field when discipline is in the message body.");
                    }
                    var discipline = disciplines.FirstOrDefault(d => d.Guid == dis.Discipline.Id);
                    if (discipline == null)
                    {
                        throw new ArgumentException(string.Concat(" Discipline ID '", dis.Discipline.Id.ToString(), "' was not found. Valid Discipline ID is required."));
                    }
                    else
                    {
                        switch (discipline.AcademicDisciplineType)
                        {
                            //getting majors
                            case Domain.Base.Entities.AcademicDisciplineType.Major:
                                studentProgEntity.AddMajors(discipline.Code);
                                break;
                            //getting minors
                            case Domain.Base.Entities.AcademicDisciplineType.Minor:
                                studentProgEntity.AddMinors(discipline.Code);
                                break;
                            //getting specializations
                            case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                                studentProgEntity.AddSpecializations(discipline.Code);
                                break;
                        }
                    }

                    if (dis.AdministeringInstitutionUnit != null)
                    {
                        if (string.IsNullOrEmpty(dis.AdministeringInstitutionUnit.Id))
                        {
                            throw new ArgumentException("Administering Institution Unit Id is a required field when Administering Institution Unit is in the message body.");
                        }
                        var administerUnit = (depts).FirstOrDefault(s => s.Guid == dis.AdministeringInstitutionUnit.Id);
                        if (administerUnit == null && dis.AdministeringInstitutionUnit.Id != null)
                        {
                            throw new ArgumentException(string.Concat(" Administering Institution Unit Id '", dis.AdministeringInstitutionUnit.Id, "' was not found. Valid Administering Institution Unit is required."));
                        }
                        else
                        {
                            if (!administerDepts.Contains(administerUnit.Code))
                            {
                                administerDepts.Add(administerUnit.Code);
                            }
                        }

                    }

                }
                //we can have just one administering unit 
                if (administerDepts.Count > 1)
                {
                    throw new ArgumentException("Only one administering Institution Unit is supported.");
                }
                else
                {
                    var dept = administerDepts.FirstOrDefault();
                    //this department needs to be same as ProgramOwner
                    if (!string.Equals(dept, studentProgEntity.DepartmentCode))
                        throw new ArgumentException("ProgramOwner and Administering Institution Unit needs to be same.");
                }
            }
            //process End date
            if (stuAcadProgramsDto.EndDate != null)
            {
                studentProgEntity.EndDate = Convert.ToDateTime(stuAcadProgramsDto.EndDate.Value.ToString("yyyy-MM-dd"));
            }

            //process expected graduation date
            if (stuAcadProgramsDto.ExpectedGraduationDate.HasValue)
            {
                studentProgEntity.AnticipatedCompletionDate = Convert.ToDateTime(stuAcadProgramsDto.ExpectedGraduationDate.Value.ToString("yyyy-MM-dd"));
            }
            //process fields from acad.credentials
            if (!string.IsNullOrEmpty(stuAcadProgramsDto.PerformanceMeasure))
            {
                decimal perfParse;
                if (decimal.TryParse(stuAcadProgramsDto.PerformanceMeasure, out perfParse))
                {
                    studentProgEntity.GradGPA = perfParse;
                }
                else
                {
                    throw new ArgumentException("PerformanceMeasure needs to be a decimal.");
                }
            }
            //process recognitions
            if (stuAcadProgramsDto.Recognitions != null && stuAcadProgramsDto.Recognitions.Any())
            {
                var honors = await GetOtherHonorsAsync(bypassCache);
                foreach (var honor in stuAcadProgramsDto.Recognitions)
                {
                    if (string.IsNullOrEmpty(honor.Id))
                    {
                        throw new ArgumentException("Recognitions ID is a required field when recognition is in the message body.");
                    }

                    var honor_ = honors.FirstOrDefault(d => d.Guid == honor.Id);
                    if (honor_ == null)
                    {
                        throw new ArgumentException(string.Concat(" Recognition ID '", honor.Id.ToString(), "' was not found. Valid recognition is required."));
                    }
                    else
                    {
                        studentProgEntity.AddHonors(honor_.Code);
                    }

                }
            }
            //process graduation date
            if (stuAcadProgramsDto.GraduatedOn != null)
            {
                studentProgEntity.GraduationDate = Convert.ToDateTime(stuAcadProgramsDto.GraduatedOn.Value.ToString("yyyy-MM-dd"));
            }
            //process credentials date
            if (stuAcadProgramsDto.CredentialsDate != null)
            {
                studentProgEntity.CredentialsDate = Convert.ToDateTime(stuAcadProgramsDto.CredentialsDate.Value.ToString("yyyy-MM-dd"));
            }
            //process thesis title
            studentProgEntity.ThesisTitle = stuAcadProgramsDto.ThesisTitle;
            studentProgEntity.CreditsEarned = stuAcadProgramsDto.CreditsEarned;
            return studentProgEntity;

        }

        /// <summary>
        /// Converts a Student Academic Program DTO to its corresponding Student Programs domain entity
        /// </summary>
        /// <param name="stuAcadProgramsDto">List of student academic program DTOs</param>
        /// <returns>A<see cref="Domain.Student.Entities.StudentAcademicProgram2">Student Program</see> domain entity</returns>
        private async Task<Domain.Student.Entities.StudentAcademicProgram> ConvertStudentAcademicProgramSubmissionDtoToEntityAsync(Ellucian.Colleague.Dtos.StudentAcademicProgramsSubmissions stuAcadProgramsDto, string stprKey, bool bypassCache = false)
        {
            // handle empty guid
            var guid = string.Empty;
            var depts = await this.GetDepartmentsAsync(bypassCache);
            if (!string.Equals(stuAcadProgramsDto.Id, Guid.Empty.ToString()))
            {
                guid = stuAcadProgramsDto.Id;
            }
            var startDate = new DateTime();
            if (stuAcadProgramsDto.StartOn != null)
            {
                startDate = Convert.ToDateTime(stuAcadProgramsDto.StartOn.Value.ToString("yyyy-MM-dd"));
            }
            var personId = string.Empty;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(stuAcadProgramsDto.Student.Id);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Student.id is not a valid GUID for persons.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }
            if (string.IsNullOrEmpty(personId))
            {
                IntegrationApiExceptionAddError("Student.id is not a valid GUID for persons.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }
            if ((!string.IsNullOrEmpty(personId)) && (await _personRepository.IsCorpAsync(personId)))
            {
                IntegrationApiExceptionAddError("Student.id is not a valid GUID for persons.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }
            //get program code
            var programCode = "";
            try
            {
                programCode = ConvertGuidToCode((await this.GetAcademicProgramsAsync(bypassCache)), stuAcadProgramsDto.AcademicProgram.Id);
            }
            catch (Exception e)
            {
                IntegrationApiExceptionAddError("Program.Id is not a valid GUID for academic-programs.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }
            if (string.IsNullOrEmpty(programCode))
            {
                IntegrationApiExceptionAddError("Program.Id is not a valid GUID for academic-programs.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }

            //get catalog code
            var catalogCode = string.Empty;
            if (stuAcadProgramsDto.AcademicCatalog != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicCatalog.Id))
                {
                    IntegrationApiExceptionAddError("Catalog.id is a required property when catalog is in the message body.", "Missing.Required.Property", guid, stprKey);

                }
                try
                {
                    catalogCode = await ConvertCatalogGuidToCode(stuAcadProgramsDto.AcademicCatalog.Id, bypassCache);
                }
                catch (Exception e)
                {
                    IntegrationApiExceptionAddError("Catalog.Id is not a valid GUID for academic-catalogs.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                }

            }

            ////check the status
            if (stuAcadProgramsDto.EnrollmentStatus != null && stuAcadProgramsDto.EnrollmentStatus.Detail != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.EnrollmentStatus.Detail.Id))
                {
                    IntegrationApiExceptionAddError("EnrollmentStatus.Detail.Id is a required field when detail is in the message body", "Missing.Required.Property", guid, stprKey);
                }
            }

            //get enrollment status
            string enrollStat = string.Empty;
            try
            {
                var enrollStatuses = await this.GetEnrollmentStatusesAsync(bypassCache);

                if (enrollStatuses != null)
                {
                    //if there is detail. id is required.
                    if (stuAcadProgramsDto.EnrollmentStatus != null)
                    {

                        if (stuAcadProgramsDto.EnrollmentStatus.Detail != null && !(string.IsNullOrEmpty(stuAcadProgramsDto.EnrollmentStatus.Detail.Id)))
                        {
                            Domain.Student.Entities.EnrollmentStatus enrollStatus = enrollStatuses.FirstOrDefault(ct => ct.Guid == stuAcadProgramsDto.EnrollmentStatus.Detail.Id);

                            if (enrollStatus != null)
                            {
                                //check if the detail id and the enumerable match
                                if (!enrollStatus.EnrollmentStatusType.ToString().ToUpperInvariant().Equals(stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString().ToUpperInvariant()))
                                {
                                    IntegrationApiExceptionAddError(string.Concat(" The enrollment Status of '", enrollStatus.EnrollmentStatusType.ToString(), "' referred by the detail ID '", stuAcadProgramsDto.EnrollmentStatus.Detail.Id.ToString(), "' is different from that in the payload."), "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                }
                                enrollStat = enrollStatus.Code;
                            }
                            else
                            {
                                IntegrationApiExceptionAddError("EnrollmentStatus.Detail.Id not a valid GUID for enrollment-statuses.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);

                            }
                        }


                        //if the detail is not there, we will just pass the status to the transaction where we will figure out the status
                        else
                        {
                            switch (stuAcadProgramsDto.EnrollmentStatus.EnrollStatus)
                            {
                                case Dtos.EnrollmentStatusType.Active:
                                    enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                                    break;
                                case Dtos.EnrollmentStatusType.Complete:
                                    IntegrationApiExceptionAddError("EnrollmentStatus.status of complete is not supported.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                    break;
                                case Dtos.EnrollmentStatusType.Inactive:
                                    enrollStat = stuAcadProgramsDto.EnrollmentStatus.EnrollStatus.ToString();
                                    break;

                            }
                        }
                    }
                }
            }
            catch
            {
                IntegrationApiExceptionAddError("Unable to retrieve enrollment-statuses.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
            }

            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            //create entity
            Ellucian.Colleague.Domain.Student.Entities.StudentAcademicProgram studentProgEntity = null;
            try
            {
                studentProgEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentAcademicProgram(personId, programCode, catalogCode, guid, startDate, enrollStat);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Missing.Required.Property", guid, stprKey);
                throw IntegrationApiException;
            }
            //get Program Owner
            if (stuAcadProgramsDto.ProgramOwner != null)
            {
                if (string.IsNullOrEmpty(stuAcadProgramsDto.ProgramOwner.Id))
                {
                    IntegrationApiExceptionAddError("ProgramOwner.id is a required property when programOwner is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    var department = (depts).FirstOrDefault(s => s.Guid == stuAcadProgramsDto.ProgramOwner.Id);
                    if (department == null)
                    {
                        IntegrationApiExceptionAddError("ProgramOwner.id is not a valid GUID for educational-institution-units.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        studentProgEntity.DepartmentCode = department.Code;
                    }
                }
            }

            //get location code
            if (stuAcadProgramsDto.Site != null)
            {
                var locationCode = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.Site.Id))
                {
                    IntegrationApiExceptionAddError("Site.id is a required property when site is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    try
                    {
                        locationCode = ConvertGuidToCode((await this.GetLocationsAsync(bypassCache)), stuAcadProgramsDto.Site.Id);
                    }
                    catch (Exception ex)
                    {
                        //we will catch this in next statement.
                        logger.Error(ex, "Unable to get location code.");
                    }
                    if (string.IsNullOrEmpty(locationCode) && stuAcadProgramsDto.Site.Id != null)
                    {
                        IntegrationApiExceptionAddError("Site.id is not a valid GUID for sites.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        studentProgEntity.Location = locationCode;
                    }
                }
            }

            //get academic level code

            if (stuAcadProgramsDto.AcademicLevel != null)
            {
                var academicLevel = string.Empty;
                if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicLevel.Id))
                {
                    IntegrationApiExceptionAddError("AcademicLevel.id is a required property when site is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    try
                    {
                        academicLevel = await ConvertAcademicLevelGuidToCode(stuAcadProgramsDto.AcademicLevel.Id, bypassCache);
                    }
                    catch (Exception ex)
                    {
                        //we will issue exception in the next statement.
                        logger.Error(ex, "Unable to get academic level.");
                    }
                    if (string.IsNullOrEmpty(academicLevel) && stuAcadProgramsDto.AcademicLevel.Id != null)
                    {
                        IntegrationApiExceptionAddError("AcademicLevel.id is not a valid GUID for academic-levels.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        studentProgEntity.AcademicLevelCode = academicLevel;
                    }
                }
            }

            //get various academic periods
            if (stuAcadProgramsDto.AcademicPeriods != null)
            {
                var termEntities = await this.GetTermsAsync(bypassCache);
                if (stuAcadProgramsDto.AcademicPeriods.Starting != null)
                {
                    var termCode = string.Empty;
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicPeriods.Starting.Id))
                    {
                        IntegrationApiExceptionAddError("AcademicPeriods.starting.id is a required property when academicPeriods.starting is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    else
                    {
                        try
                        {
                            termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.AcademicPeriods.Starting.Id);
                        }
                        catch (Exception ex)
                        {
                            //we will throw exception below.
                            logger.Error(ex, "Unable to get term code.");
                        }
                        if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.AcademicPeriods.Starting.Id != null)
                        {
                            IntegrationApiExceptionAddError("AcademicPeriods.starting.id is not a valid GUID for academic-periods.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                        }
                        else
                        {
                            studentProgEntity.StartTerm = termCode;
                        }
                    }
                }

                if (stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation != null)
                {
                    var termCode = string.Empty;
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id))
                    {
                        IntegrationApiExceptionAddError("AcademicPeriods.expectedGraduation.id is a required property when academicPeriods.expectedGraduation is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    else
                    {
                        try
                        {
                            termCode = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id);
                        }
                        catch (Exception ex)
                        {
                            //we will catch it in next statement
                            logger.Error(ex, "Unable to get term code.");
                        }
                        if (string.IsNullOrEmpty(termCode) && stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id != null)
                        {
                            IntegrationApiExceptionAddError("AcademicPeriods.expectedGraduation.id is not a valid GUID for academic-periods.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                        }
                        else
                        {
                            //If the expectedGraduationDate is present in the payload validate that the date fits the term start/end date.
                            if (stuAcadProgramsDto.ExpectedGraduationDate.HasValue)
                            {
                                var termInfo = termEntities.FirstOrDefault(term => term.Code == termCode);
                                if (termInfo != null)
                                {
                                    if ((termInfo.StartDate != null && stuAcadProgramsDto.ExpectedGraduationDate < termInfo.StartDate) || (termInfo.EndDate != null && stuAcadProgramsDto.ExpectedGraduationDate > termInfo.EndDate))
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Expected graduation academicPeriod ID '", stuAcadProgramsDto.AcademicPeriods.ExpectedGraduation.Id.ToString(), "' is not valid for the Expected Graduation date of ", Convert.ToDateTime(stuAcadProgramsDto.ExpectedGraduationDate).ToString("yyyy-MM-dd")), "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                    }
                                }
                            }
                            studentProgEntity.AnticipatedCompletionTerm = termCode;
                        }
                    }
                }

            }

            // get degrees and certificate from the credentials in the DTO
            if (stuAcadProgramsDto.Credentials != null && stuAcadProgramsDto.Credentials.Any())
            {
                var degrees = new List<string>();
                var credentials = await GetAcadCredentialsAsync(bypassCache);
                foreach (var cred in stuAcadProgramsDto.Credentials)
                {
                    var credential = credentials.FirstOrDefault(d => d.Guid == cred.Id);
                    if (credential == null)
                    {
                        //throw new ArgumentException(string.Concat(" Credential ID '", cred.Id.ToString(), "' was not found. Valid Credential is required."));
                        IntegrationApiExceptionAddError("Credentials.id is not a valid GUID for academic-credentials.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        var type = credential.AcademicCredentialType;
                        switch (type)
                        {
                            case Domain.Base.Entities.AcademicCredentialType.Certificate:
                                studentProgEntity.AddCcds(credential.Code);
                                break;
                            case Domain.Base.Entities.AcademicCredentialType.Degree:
                                degrees.Add(credential.Code);
                                break;
                            //produce error if honor codes are included
                            case Domain.Base.Entities.AcademicCredentialType.Honorary:
                                IntegrationApiExceptionAddError("Credentials.id of type honor is not supported.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                break;

                            case Domain.Base.Entities.AcademicCredentialType.Diploma:
                                IntegrationApiExceptionAddError("Credentials.id of type diploma is not supported.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                                break;
                        }
                    }

                }

                //if there is more than one degree in the payload, produce an error
                if (degrees.Count > 1)
                {
                    IntegrationApiExceptionAddError("Credentials array cannot have more than one degree.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);
                }
                else
                {
                    studentProgEntity.DegreeCode = degrees.FirstOrDefault();
                }
            }

            //get the displicines which included majors, minors, specializations
            if (stuAcadProgramsDto.Disciplines != null && stuAcadProgramsDto.Disciplines.Any())
            {
                var disciplines = await GetAcademicDisciplinesAsync(bypassCache);
                foreach (var dis in stuAcadProgramsDto.Disciplines)
                {

                    if (string.IsNullOrEmpty(dis.Discipline.Id))
                    {
                        IntegrationApiExceptionAddError("Disciplines.discipline.id is a required property when disciplines.discipline is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    var discipline = disciplines.FirstOrDefault(d => d.Guid == dis.Discipline.Id);
                    if (discipline == null)
                    {
                        IntegrationApiExceptionAddError("Disciplines.discipline.id is not a valid GUID for academic-disciplines.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                    }
                    else
                    {
                        switch (discipline.AcademicDisciplineType)
                        {
                            //getting majors
                            case Domain.Base.Entities.AcademicDisciplineType.Major:
                                studentProgEntity.AddMajors(discipline.Code, dis.StartOn, dis.EndOn);
                                break;
                            //getting minors
                            case Domain.Base.Entities.AcademicDisciplineType.Minor:
                                studentProgEntity.AddMinors(discipline.Code, dis.StartOn, dis.EndOn);
                                break;
                            //getting specializations
                            case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                                studentProgEntity.AddSpecializations(discipline.Code, dis.StartOn, dis.EndOn);
                                break;
                        }
                    }
                    //check end date is not before start date
                    if (dis.EndOn != null && dis.EndOn < dis.StartOn)
                    {
                        IntegrationApiExceptionAddError("The requested discipline " + dis.Discipline.Id + " endOn must be on or after the discipline startOn.", "Validation.Exception", stuAcadProgramsDto.Id, stprKey);

                    }

                }

            }
            //process End date
            if (stuAcadProgramsDto.EndOn != null)
            {
                studentProgEntity.EndDate = Convert.ToDateTime(stuAcadProgramsDto.EndOn.Value.ToString("yyyy-MM-dd"));
            }

            //process expected graduation date
            if (stuAcadProgramsDto.ExpectedGraduationDate.HasValue)
            {
                studentProgEntity.AnticipatedCompletionDate = Convert.ToDateTime(stuAcadProgramsDto.ExpectedGraduationDate.Value.ToString("yyyy-MM-dd"));
            }

            // process admit status
            if (stuAcadProgramsDto.AdmissionClassification != null)
            {
                var admitStatus = string.Empty;
                if (stuAcadProgramsDto.AdmissionClassification.AdmissionCategory == null)
                {
                    IntegrationApiExceptionAddError("AdmissionClassification.admissionCategory is a required property when admissionClassification is in the message body.", "Missing.Required.Property", guid, stprKey);
                }
                else
                {
                    if (string.IsNullOrEmpty(stuAcadProgramsDto.AdmissionClassification.AdmissionCategory.Id))
                    {
                        IntegrationApiExceptionAddError("AdmissionClassification.admissionCategory.id is a required property when admissionClassification.admissionCategory is in the message body.", "Missing.Required.Property", guid, stprKey);
                    }
                    else
                    {
                        try
                        {
                            admitStatus = await ConvertAdmissionClassificationGuidToCode(stuAcadProgramsDto.AdmissionClassification.AdmissionCategory.Id, bypassCache);
                        }
                        catch (Exception ex)
                        {
                            //we will throw the exception in tne next statement
                            logger.Error(ex, "Unable to get admit status.");
                        }
                        if (string.IsNullOrEmpty(admitStatus) && stuAcadProgramsDto.AdmissionClassification.AdmissionCategory.Id != null)
                        {
                            IntegrationApiExceptionAddError("AdmissionClassification.admissionCategory.id is not a valid GUID for admission-populations.", "GUID.Not.Found", stuAcadProgramsDto.Id, stprKey);
                        }
                        else
                        {
                            studentProgEntity.AdmitStatus = admitStatus;
                        }
                    }
                }

            }
            // Throw errors
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return studentProgEntity;

        }

        /// <summary>
        /// Return a list of StudentAcademicProgram objects based on selection criteria.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="student">Id of the student enrolled on the academic program</param>
        /// <param name="startOn">Student Academic Program starts on or after this date</param>
        /// <param name="endOn">Student Academic Program ends on or before this date</param>
        /// <param name="program">academic program Name Contains ...program...</param>
        /// <param name="catalog">Student Academic Program catalog  equal to</param>
        /// <param name="status">Student Academic Program status equals to </param>
        /// <param name="programOwner">The owner of the academic program. This property represents the global identifier for the Program Owner.</param>
        /// <param name="site">	The site (campus) the student enrolls for the program at</param>
        /// <param name="graduatedOn">The date the student graduate from the program.</param>
        /// <param name="credential">The academic credentials that can be awarded for completing an academic program</param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns>List of StudentAcademicProgram <see cref="Dtos.StudentAcademicPrograms"/> objects representing matching Student Academic Programs</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms>, int>> GetStudentAcademicProgramsAsync(int offset, int limit, bool bypassCache = false, string student = "",
            string startOn = "", string endOn = "", string program = "", string catalog = "", string status = "", string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "",
            string credential = "", string graduatedAcademicPeriod = "", string completeStatus = "")
        {
            try
            {
                
                string newStudent = string.Empty, newStartOn = string.Empty, newEndOn = string.Empty, newProgram = string.Empty, newCatalog = string.Empty, newStatus = string.Empty, newProgramOwner = string.Empty, newSite = string.Empty,
                    newAcademicLevel = string.Empty, newGraduatedOn = string.Empty, newgraduatedAcademicPeriod = string.Empty;

                List<string> ccdCredential = new List<string>();
                List<string> degreeCredential = new List<string>();
                // Convert and validate all input parameters
                newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
                newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
                if (student != string.Empty)
                {
                    newStudent = await _personRepository.GetPersonIdFromGuidAsync(student);
                    if (string.IsNullOrEmpty(newStudent))
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms>, int>(new List<Dtos.StudentAcademicPrograms>(), 0);
                }
                newProgram = (program == string.Empty ? string.Empty : ConvertGuidToCode(await GetAcademicProgramsAsync(bypassCache), program));
                newCatalog = (catalog == string.Empty ? string.Empty : await ConvertCatalogGuidToCode(catalog));
                newStatus = (status == string.Empty ? string.Empty : await ConvertEnrollmentStatusToCode(status));
                newProgramOwner = (programOwner == string.Empty ? string.Empty : ConvertGuidToCode(await GetDepartmentsAsync(bypassCache), programOwner));
                newSite = (site == string.Empty ? string.Empty : ConvertGuidToCode(await GetLocationsAsync(bypassCache), site));
                newAcademicLevel = (academicLevel == string.Empty ? string.Empty : await ConvertAcademicLevelGuidToCode(academicLevel, bypassCache));
                newGraduatedOn = (graduatedOn == string.Empty ? string.Empty : await ConvertDateArgument(graduatedOn));
                var termEntities = await this.GetTermsAsync(bypassCache);
                newgraduatedAcademicPeriod = (graduatedAcademicPeriod == string.Empty ? string.Empty : ConvertGuidToCode(await GetAcademicPeriods(bypassCache), graduatedAcademicPeriod));

                if (!string.IsNullOrEmpty(credential))
                {
                    var credentialTuple = await ConvertCredentialFilterToCode(credential);
                    if (!string.IsNullOrEmpty(credentialTuple.Item1))
                        ccdCredential.Add(credentialTuple.Item1);
                    if (!string.IsNullOrEmpty(credentialTuple.Item2))
                        degreeCredential.Add(credentialTuple.Item2);
                    completeStatus = await ConvertEnrollmentStatusToCode(Dtos.EnrollmentStatusType.Complete.ToString());
                }

                var studentAcadProgDtos = new List<Dtos.StudentAcademicPrograms>();
                var defaultInstitutionId = GetDefaultInstitutionId();
                var studentAcadProgEntitiesTuple = await _studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, offset, limit, bypassCache, newProgram,
                    newStartOn, newEndOn, newStudent, newCatalog, newStatus, newProgramOwner, newSite, newAcademicLevel, newGraduatedOn, ccdCredential , degreeCredential , newgraduatedAcademicPeriod, completeStatus);
                if (studentAcadProgEntitiesTuple != null)
                {
                    var studentAcadProgEntities = studentAcadProgEntitiesTuple.Item1;
                    var totalCount = studentAcadProgEntitiesTuple.Item2;
                    if (studentAcadProgEntities != null && studentAcadProgEntities.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms>, int>
                            (await ConvertStudentAcademicProgramEntityToDto(studentAcadProgEntities.ToList(), bypassCache), totalCount);
                    }
                    else
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms>, int>(new List<Dtos.StudentAcademicPrograms>(), totalCount);
                    }
                }
                else
                {
                    //no results
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms>, int>(new List<Dtos.StudentAcademicPrograms>(), 0);
                }
            }
            catch(InvalidOperationException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                // One or more of the arguments failed to match up to a guid.  Return empty set.
                return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms>, int>(new List<Dtos.StudentAcademicPrograms>(), 0);

            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }

        }

        /// <summary>
        /// Return a list of StudentAcademicProgram objects based on selection criteria.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="student">Id of the student enrolled on the academic program</param>
        /// <param name="startOn">Student Academic Program starts on or after this date</param>
        /// <param name="endOn">Student Academic Program ends on or before this date</param>
        /// <param name="program">academic program Name Contains ...program...</param>
        /// <param name="status">Student Academic Program status equals to </param>
        /// <param name="programOwner">The owner of the academic program. This property represents the global identifier for the Program Owner.</param>
        /// <param name="site">	The site (campus) the student enrolls for the program at</param>
        /// <param name="graduatedOn">The date the student graduate from the program.</param>
        /// <param name="credential">The academic credentials that can be awarded for completing an academic program</param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns>List of StudentAcademicProgram <see cref="Dtos.StudentAcademicPrograms"/> objects representing matching Student Academic Programs</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms2>, int>> GetStudentAcademicPrograms2Async(int offset, int limit, bool bypassCache = false, string student = "",
            string startOn = "", string endOn = "", string program = "", string status = "", string site = "", string academicLevel = "", string graduatedOn = "",
            List<string> credential = null, string graduatedAcademicPeriod = "")
        {
            try
            {
                
                string newStartOn = string.Empty, newEndOn = string.Empty, newStudent = string.Empty, newProgram = string.Empty, newStatus = string.Empty, newSite = string.Empty,
                    newAcademicLevel = string.Empty, newGraduatedOn = string.Empty, newgraduatedAcademicPeriod = string.Empty, completeStatus = string.Empty;
                List<string> ccdCredential = new List<string>();
                List<string> degreeCredential = new List<string>();

                newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
                newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
                newGraduatedOn = (graduatedOn == string.Empty ? string.Empty : await ConvertDateArgument(graduatedOn));

                try
                {
                    // Convert and validate all input parameters

                    if (student != string.Empty)
                    {
                        newStudent = await _personRepository.GetPersonIdFromGuidAsync(student);
                        if (string.IsNullOrEmpty(newStudent))
                            throw new ArgumentException("Invalid student " + student + " in the arguments");
                    }
                    newProgram = (program == string.Empty ? string.Empty : ConvertGuidToCode(await GetAcademicProgramsAsync(bypassCache), program));
                    newStatus = (status == string.Empty ? string.Empty : await ConvertEnrollmentStatusToCode(status));
                    newSite = (site == string.Empty ? string.Empty : ConvertGuidToCode(await GetLocationsAsync(bypassCache), site));
                    newAcademicLevel = (academicLevel == string.Empty ? string.Empty : await ConvertAcademicLevelGuidToCode(academicLevel, bypassCache));
                    var termEntities = await this.GetTermsAsync(bypassCache);
                    newgraduatedAcademicPeriod = (graduatedAcademicPeriod == string.Empty ? string.Empty : ConvertGuidToCode(await GetAcademicPeriods(bypassCache), graduatedAcademicPeriod));
                    completeStatus = string.Empty;

                    if (credential != null && credential.Any())
                    {
                        var credentialTuple = await ConvertCredentialFilterToCode2(credential);
                        if (credentialTuple != null && credentialTuple.Any())
                        {
                            credentialTuple.ForEach(i =>
                            {
                                if (!string.IsNullOrEmpty(i.Item1))
                                {
                                    ccdCredential.Add(i.Item1);
                                }
                                if (!string.IsNullOrEmpty(i.Item2))
                                {
                                    degreeCredential.Add(i.Item2);
                                }
                            });
                        }
                        completeStatus = await ConvertEnrollmentStatusToCode(Dtos.EnrollmentStatusType.Complete.ToString());
                    }
                }
                catch(Exception)
                {
                    //no results or invalid filters
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms2>, int>(new List<Dtos.StudentAcademicPrograms2>(), 0);
                }

                var studentAcadProgDtos = new List<Dtos.StudentAcademicPrograms2>();
                var defaultInstitutionId = GetDefaultInstitutionId();
                var studentAcadProgEntitiesTuple = await _studentAcademicProgramRepository.GetStudentAcademicPrograms2Async(defaultInstitutionId, offset, limit, bypassCache, newProgram,
                    newStartOn, newEndOn, newStudent, string.Empty, newStatus, string.Empty, newSite, newAcademicLevel, newGraduatedOn, ccdCredential, degreeCredential, newgraduatedAcademicPeriod, completeStatus);
                if (studentAcadProgEntitiesTuple != null)
                {
                    var studentAcadProgEntities = studentAcadProgEntitiesTuple.Item1;
                    var totalCount = studentAcadProgEntitiesTuple.Item2;
                    if (studentAcadProgEntities != null && studentAcadProgEntities.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms2>, int>
                            (await ConvertStudentAcademicProgramEntityToDto2Async(studentAcadProgEntities.ToList(), bypassCache), totalCount);
                    }
                    else
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms2>, int>(new List<Dtos.StudentAcademicPrograms2>(), totalCount);
                    }
                }
                else
                {
                    //no results
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms2>, int>(new List<Dtos.StudentAcademicPrograms2>(), 0);
                }
            }
            catch(InvalidOperationException)
            {
                throw;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }

        }

        /// <summary>
        /// Return a list of StudentAcademicProgram objects based on selection criteria.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="criteriaObj"></param>
        /// <returns>List of StudentAcademicProgram <see cref="Dtos.StudentAcademicPrograms3"/> objects representing matching Student Academic Programs</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms3>, int>> GetStudentAcademicPrograms3Async(int offset, int limit,
            Dtos.StudentAcademicPrograms3 criteriaObj, bool bypassCache = false)
        {
            try
            {
               
                string newStartOn = string.Empty, newEndOn = string.Empty, newStudent = string.Empty, newProgram = string.Empty, newStatus = string.Empty, newSite = string.Empty,
                    newAcademicLevel = string.Empty, newGraduatedOn = string.Empty, newgraduatedAcademicPeriod = string.Empty, completeStatus = string.Empty;
                List<string> ccdCredential = new List<string>();
                List<string> degreeCredential = new List<string>();
                var curriculumObjective = CurriculumObjectiveCategory.NotSet;

                try
                {
                    newStartOn = criteriaObj.StartOn.HasValue ? await ConvertDateArgument(criteriaObj.StartOn.ToString()) : string.Empty;
                    newEndOn = criteriaObj.EndOn.HasValue ? await ConvertDateArgument(criteriaObj.EndOn.ToString()) : string.Empty;
                    newGraduatedOn = criteriaObj.GraduatedOn.HasValue ? await ConvertDateArgument(criteriaObj.GraduatedOn.ToString()) : string.Empty;

                    if (criteriaObj.Student != null && !string.IsNullOrEmpty(criteriaObj.Student.Id))
                    {
                        newStudent = await _personRepository.GetPersonIdFromGuidAsync(criteriaObj.Student.Id);
                        if (string.IsNullOrEmpty(newStudent))
                            throw new ArgumentException("Invalid student " + criteriaObj.Student.Id + " in the arguments");
                    }
                    if ((criteriaObj.AcademicProgram != null && !string.IsNullOrEmpty(criteriaObj.AcademicProgram.Id)))
                    {
                        newProgram = ConvertGuidToCode(await GetAcademicProgramsAsync(bypassCache), criteriaObj.AcademicProgram.Id);
                    }
                    if (criteriaObj.EnrollmentStatus != null)
                    {
                        newStatus = await ConvertEnrollmentStatusToCode(criteriaObj.EnrollmentStatus.EnrollStatus.ToString());
                    }
                    if ((criteriaObj.Site != null && !string.IsNullOrEmpty(criteriaObj.Site.Id)))
                    {
                        newSite = ConvertGuidToCode(await GetLocationsAsync(bypassCache), criteriaObj.Site.Id);
                    }
                    if ((criteriaObj.AcademicLevel != null) && (!string.IsNullOrEmpty(criteriaObj.AcademicLevel.Id)))
                    {
                        newAcademicLevel = await ConvertAcademicLevelGuidToCode(criteriaObj.AcademicLevel.Id, bypassCache);
                    }

                    if (criteriaObj.AcademicPeriods != null && criteriaObj.AcademicPeriods.ActualGraduation != null && !string.IsNullOrEmpty(criteriaObj.AcademicPeriods.ActualGraduation.Id))
                    {
                        newgraduatedAcademicPeriod = ConvertGuidToCode(await GetAcademicPeriods(bypassCache), criteriaObj.AcademicPeriods.ActualGraduation.Id);
                    }

                    if (criteriaObj.Credentials != null && criteriaObj.Credentials.Any())
                    {
                        var credentials = new List<string>();
                        criteriaObj.Credentials.ToList().ForEach(i =>
                        {
                            credentials.Add(i.Id);
                        });

                        foreach (var credential in criteriaObj.Credentials)
                        {
                            var credentialTuple = await ConvertCredentialFilterToCode2(credentials);
                            if (credentialTuple != null && credentialTuple.Any())
                            {
                                credentialTuple.ForEach(i =>
                                {
                                    if (!string.IsNullOrEmpty(i.Item1))
                                    {
                                        ccdCredential.Add(i.Item1);
                                    }
                                    if (!string.IsNullOrEmpty(i.Item2))
                                    {
                                        degreeCredential.Add(i.Item2);
                                    }
                                });
                            }
                        }
                    }
                    completeStatus = await ConvertEnrollmentStatusToCode(Dtos.EnrollmentStatusType.Complete.ToString());

                    if (criteriaObj.CurriculumObjective != null)
                    {
                        curriculumObjective = ConvertCurriculumObjDtoToCurriculumObjCategoryEnum(criteriaObj.CurriculumObjective);
                    }

                }
                catch (Exception ex)
                {
                    //no results or invalid filters
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms3>, int>(new List<Dtos.StudentAcademicPrograms3>(), 0);
                }

                var defaultInstitutionId = string.Empty;
                try
                {
                    defaultInstitutionId = GetDefaultInstitutionId();
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                    throw IntegrationApiException;
                }
                Tuple<IEnumerable<StudentAcademicProgram>, int> studentAcadProgEntitiesTuple = null;
                try
                {
                    studentAcadProgEntitiesTuple = await _studentAcademicProgramRepository.GetStudentAcademicPrograms3Async(defaultInstitutionId, offset, limit, bypassCache, newProgram,
                        newStartOn, newEndOn, newStudent, string.Empty, newStatus, string.Empty, newSite, newAcademicLevel, newGraduatedOn, ccdCredential, degreeCredential, newgraduatedAcademicPeriod, completeStatus, curriculumObjective);
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                if (studentAcadProgEntitiesTuple != null)
                {
                    var studentAcadProgEntities = studentAcadProgEntitiesTuple.Item1;
                    var totalCount = studentAcadProgEntitiesTuple.Item2;
                    if (studentAcadProgEntities != null && studentAcadProgEntities.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms3>, int>
                            (await ConvertStudentAcademicProgramEntityToDto3Async(studentAcadProgEntities.ToList(), bypassCache), totalCount);
                    }
                    else
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms3>, int>(new List<Dtos.StudentAcademicPrograms3>(), totalCount);
                    }
                }
                else
                {
                    //no results
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms3>, int>(new List<Dtos.StudentAcademicPrograms3>(), 0);
                }
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw new ColleagueWebApiException(e.Message);
            }

        }

        /// <summary>
        /// Return a list of StudentAcademicProgram objects based on selection criteria.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="criteriaObj"></param>
        /// <param name="personFilterValue"></param>
        /// <returns>List of StudentAcademicProgram <see cref="Dtos.StudentAcademicPrograms4"/> objects representing matching Student Academic Programs</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms4>, int>> GetStudentAcademicPrograms4Async(int offset, int limit,
            Dtos.StudentAcademicPrograms4 criteriaObj, string personFilterValue, bool bypassCache = false)
        {
            try
            {
                

                string newStartOn = string.Empty, newEndOn = string.Empty, newStudent = string.Empty, newProgram = string.Empty, newStatus = string.Empty, newSite = string.Empty,
                    newAcademicLevel = string.Empty, newGraduatedOn = string.Empty, newgraduatedAcademicPeriod = string.Empty, completeStatus = string.Empty;
                List<string> ccdCredential = new List<string>();
                List<string> degreeCredential = new List<string>();
                var curriculumObjective = CurriculumObjectiveCategory.NotSet;
                string[] filterPersonIds = new List<string>().ToArray();

                try
                {
                    if (!string.IsNullOrEmpty(personFilterValue))
                    {
                        var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilterValue));
                        if (personFilterKeys != null)
                        {
                            filterPersonIds = personFilterKeys;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms4>, int>(new List<Dtos.StudentAcademicPrograms4>(), 0);
                        }


                    }
                    else
                    {
                        newStartOn = criteriaObj.StartOn.HasValue ? await ConvertDateArgument(criteriaObj.StartOn.ToString()) : string.Empty;
                        newEndOn = criteriaObj.EndOn.HasValue ? await ConvertDateArgument(criteriaObj.EndOn.ToString()) : string.Empty;
                        //newGraduatedOn = criteriaObj.GraduatedOn.HasValue ? await ConvertDateArgument(criteriaObj.GraduatedOn.ToString()) : string.Empty;

                        if (criteriaObj.Student != null && !string.IsNullOrEmpty(criteriaObj.Student.Id))
                        {
                            newStudent = await _personRepository.GetPersonIdFromGuidAsync(criteriaObj.Student.Id);
                            if (string.IsNullOrEmpty(newStudent))
                                throw new ArgumentException("Invalid student " + criteriaObj.Student.Id + " in the arguments");
                        }
                        if ((criteriaObj.AcademicProgram != null && !string.IsNullOrEmpty(criteriaObj.AcademicProgram.Id)))
                        {
                            newProgram = ConvertGuidToCode(await GetAcademicProgramsAsync(bypassCache), criteriaObj.AcademicProgram.Id);
                        }
                        if (criteriaObj.EnrollmentStatus != null)
                        {
                            newStatus = await ConvertEnrollmentStatusToCode(criteriaObj.EnrollmentStatus.EnrollStatus.ToString());
                        }
                        if ((criteriaObj.Site != null && !string.IsNullOrEmpty(criteriaObj.Site.Id)))
                        {
                            newSite = ConvertGuidToCode(await GetLocationsAsync(bypassCache), criteriaObj.Site.Id);
                        }
                        if ((criteriaObj.AcademicLevel != null) && (!string.IsNullOrEmpty(criteriaObj.AcademicLevel.Id)))
                        {
                            newAcademicLevel = await ConvertAcademicLevelGuidToCode(criteriaObj.AcademicLevel.Id, bypassCache);
                        }

                        if (criteriaObj.Credentials != null && criteriaObj.Credentials.Any())
                        {
                            var credentials = new List<string>();
                            criteriaObj.Credentials.ToList().ForEach(i =>
                            {
                                credentials.Add(i.Id);
                            });

                            foreach (var credential in criteriaObj.Credentials)
                            {
                                var credentialTuple = await ConvertCredentialFilterToCode2(credentials);
                                if (credentialTuple != null && credentialTuple.Any())
                                {
                                    credentialTuple.ForEach(i =>
                                    {
                                        if (!string.IsNullOrEmpty(i.Item1))
                                        {
                                            ccdCredential.Add(i.Item1);
                                        }
                                        if (!string.IsNullOrEmpty(i.Item2))
                                        {
                                            degreeCredential.Add(i.Item2);
                                        }
                                    });
                                }
                            }
                        }
                        completeStatus = await ConvertEnrollmentStatusToCode(Dtos.EnrollmentStatusType.Complete.ToString());

                        if (criteriaObj.CurriculumObjective != null)
                        {
                            curriculumObjective = ConvertCurriculumObjDtoToCurriculumObjCategoryEnum(criteriaObj.CurriculumObjective);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //no results or invalid filters
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms4>, int>(new List<Dtos.StudentAcademicPrograms4>(), 0);
                }

                
                Tuple<IEnumerable<StudentAcademicProgram>, int> studentAcadProgEntitiesTuple = null;
                try
                {
                    if (string.IsNullOrEmpty(personFilterValue))
                    {
                        
                       var defaultInstitutionId = GetDefaultInstitutionId();
                        
                        studentAcadProgEntitiesTuple = await _studentAcademicProgramRepository.GetStudentAcademicPrograms4Async(defaultInstitutionId, offset, limit, bypassCache, newProgram,
                            newStartOn, newEndOn, newStudent, string.Empty, newStatus, string.Empty, newSite, newAcademicLevel, string.Empty, ccdCredential, degreeCredential, string.Empty,
                            completeStatus, curriculumObjective, false);
                    }
                    else
                        studentAcadProgEntitiesTuple = await _studentAcademicProgramRepository.GetStudentAcademicProgramsPersonFilterAsync(offset, limit,
                            filterPersonIds, personFilterValue, bypassCache);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                if (studentAcadProgEntitiesTuple != null)
                {
                    var studentAcadProgEntities = studentAcadProgEntitiesTuple.Item1;
                    var totalCount = studentAcadProgEntitiesTuple.Item2;
                    if (studentAcadProgEntities != null && studentAcadProgEntities.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms4>, int>
                            (await ConvertStudentAcademicProgramEntityToDto4Async(studentAcadProgEntities.ToList(), bypassCache), totalCount);
                    }
                    else
                    {
                        // no results
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms4>, int>(new List<Dtos.StudentAcademicPrograms4>(), totalCount);
                    }
                }
                else
                {
                    //no results
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPrograms4>, int>(new List<Dtos.StudentAcademicPrograms4>(), 0);
                }
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw new ColleagueWebApiException(e.Message);
            }

        }

        /// <summary>
        /// Convert a StudentAcademicProgramsCurriculumObjective2 DTO enumeration to a CurriculumObjectiveCategory domain object enumeration
        /// </summary>
        /// <param name="curriculumObjective"></param>
        /// <returns>CurriculumObjectiveCategory enumeration</returns>
        private CurriculumObjectiveCategory ConvertCurriculumObjDtoToCurriculumObjCategoryEnum(StudentAcademicProgramsCurriculumObjective2? curriculumObjective)
        {
            if (curriculumObjective == null)
                return CurriculumObjectiveCategory.NotSet;
            switch (curriculumObjective)
            {
                case StudentAcademicProgramsCurriculumObjective2.Applied:
                    return CurriculumObjectiveCategory.Applied;
                case StudentAcademicProgramsCurriculumObjective2.Matriculated:
                    return CurriculumObjectiveCategory.Matriculated;
                case StudentAcademicProgramsCurriculumObjective2.Outcome:
                    return CurriculumObjectiveCategory.Outcome;
                case StudentAcademicProgramsCurriculumObjective2.Recruited:
                    return CurriculumObjectiveCategory.Recruited;
                default:
                    return CurriculumObjectiveCategory.NotSet;
            }
        }

        /// <summary>
        /// Convert a CurriculumObjectiveCategory domain object enumeration to a StudentAcademicProgramsCurriculumObjective2 DTO enumeration
        /// </summary>
        /// <param name="curriculumObjective"></param>
        /// <returns></returns>
        private StudentAcademicProgramsCurriculumObjective2? ConvertCurriculumObjCategoryToCurriculumObjDtoEnum(CurriculumObjectiveCategory? curriculumObjective)
        {
            if (curriculumObjective == null)
                return StudentAcademicProgramsCurriculumObjective2.NotSet;
            switch (curriculumObjective)
            {
                case CurriculumObjectiveCategory.Applied:
                    return StudentAcademicProgramsCurriculumObjective2.Applied;
                case CurriculumObjectiveCategory.Matriculated:
                    return StudentAcademicProgramsCurriculumObjective2.Matriculated;
                case CurriculumObjectiveCategory.Outcome:
                    return StudentAcademicProgramsCurriculumObjective2.Outcome;
                case CurriculumObjectiveCategory.Recruited:
                    return StudentAcademicProgramsCurriculumObjective2.Recruited;
                default:
                    return StudentAcademicProgramsCurriculumObjective2.NotSet;
            }
        }

        /// <summary>
        ///  Get default organization id from PID2
        /// </summary>
        /// <returns>default institution id</returns>
        private string GetDefaultInstitutionId()
        {
            if (!string.IsNullOrEmpty(_defaultInstitutionId)) return _defaultInstitutionId;
            var hostId = string.Empty;
            var defaultsConfiguration = _configurationRepository.GetDefaultsConfiguration();
            if (defaultsConfiguration != null)
            {
                hostId = defaultsConfiguration.HostInstitutionCodeId;

            }
            if (string.IsNullOrEmpty(hostId))
                throw new KeyNotFoundException("Unable to determine default institution from PID2.");
            _defaultInstitutionId = hostId;

            return _defaultInstitutionId;
        }

        /// <summary>
        /// Converts date to unidata Date
        /// </summary>
        /// <param name="date">UTC datetime</param>
        /// <returns>Unidata Date</returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _studentAcademicProgramRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Invalid Date format in arguments");
            }
        }

        /// <summary>
        /// Convert a GUID to a code in a code file
        /// </summary>
        /// <param name="codeList">Source list of codes, must inherit GuidCodeItem</param>
        /// <param name="guid">GUID corresponding to a code</param>
        /// <returns>The code corresponding to the GUID</returns>
        protected static new string ConvertGuidToCode(IEnumerable<Domain.Entities.GuidCodeItem> codeList, string guid)
        {
            if (codeList == null || !codeList.Any())
            {
                throw new ArgumentNullException("codeList");
            }
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var entity = codeList.FirstOrDefault(c => c.Guid == guid);
            if (entity != null)
                return entity.Code;
            else
                throw new ArgumentException("Invalid guid " + guid + " in the arguments.");
        }

        /// <summary>
        /// Credential Filter can be one of two items in Colleague, CCD or DEGREE. This method checks to see which one the guid references and returns the appropriate code. 
        /// Item1 will be the CCD code, Item2 will be the DEGREE code.
        /// </summary>
        /// <param name="credential"></param>
        /// <returns>Item1 will be the CCD code, Item2 will be the DEGREE code.</returns>
        private async Task<Tuple<string, string>> ConvertCredentialFilterToCode(string credential)
        {
            var credentials = await GetAcadCredentialsAsync(false);
            var returnTuple = new Tuple<string, string>(string.Empty, string.Empty);

            var credentialObject = credentials.FirstOrDefault(d => d.Guid == credential);
            if (credentialObject == null)
            {
                throw new ArgumentException(string.Concat(" Credential ID '", credential, "' was not found. Valid Credential is required."));
            }
            var type = credentialObject.AcademicCredentialType;
            switch (type)
            {
                //this is the CCD return
                case Domain.Base.Entities.AcademicCredentialType.Certificate:
                    returnTuple = new Tuple<string, string>(credentialObject.Code, string.Empty);
                    break;
                //this is the DEGREE return
                case Domain.Base.Entities.AcademicCredentialType.Degree:
                    returnTuple = new Tuple<string, string>(string.Empty, credentialObject.Code);
                    break;
                case Domain.Base.Entities.AcademicCredentialType.Honorary:
                    throw new ArgumentException(credential + " is an Honor code. Honor code is not allowed to use as a credential filter Student Academic Program.");
                case Domain.Base.Entities.AcademicCredentialType.Diploma:
                    throw new ArgumentException(credential + " is a Diploma. Diploma is not allowed to use as a credential filter on Student Academic Program.");
                default:
                    throw new ArgumentException(credential + " was not found, invalid filter criteria for credential on Student Academic Program");
            }

            return returnTuple;
        }

        /// <summary>
        /// Credential Filter can be one of two items in Colleague, CCD or DEGREE. This method checks to see which one the guid references and returns the appropriate code. 
        /// Item1 will be the CCD code, Item2 will be the DEGREE code.
        /// </summary>
        /// <param name="sources"></param>
        /// <returns>Item1 will be the CCD code, Item2 will be the DEGREE code.</returns>
        private async Task<List<Tuple<string, string>>> ConvertCredentialFilterToCode2(List<string> sources)
        {
            if (sources == null || !sources.Any())
            {
                return null;
            }
            var credentials = await GetAcadCredentialsAsync(false);
            var returnTuples = new List<Tuple<string, string>>();

            sources.ForEach(credential =>
            {
                var credentialObject = credentials.FirstOrDefault(d => d.Guid == credential);
                if (credentialObject == null)
                {
                    throw new ArgumentException(string.Concat(" Credential ID '", sources, "' was not found. Valid Credential is required."));
                }
                var type = credentialObject.AcademicCredentialType;
                switch (type)
                {
                    //this is the CCD return
                    case Domain.Base.Entities.AcademicCredentialType.Certificate:
                        returnTuples.Add(new Tuple<string, string>(credentialObject.Code, string.Empty));
                        break;
                    //this is the DEGREE return
                    case Domain.Base.Entities.AcademicCredentialType.Degree:
                        returnTuples.Add(new Tuple<string, string>(string.Empty, credentialObject.Code));
                        break;
                    case Domain.Base.Entities.AcademicCredentialType.Honorary:
                        throw new ArgumentException(sources + " is an Honor code. Honor code is not allowed to use as a credential filter Student Academic Program.");
                    case Domain.Base.Entities.AcademicCredentialType.Diploma:
                        throw new ArgumentException(sources + " is a Diploma. Diploma is not allowed to use as a credential filter on Student Academic Program.");
                    default:
                        throw new ArgumentException(sources + " was not found, invalid filter criteria for credential on Student Academic Program");
                }
            });

            return returnTuples;
        }

        /// <summary>
        /// Converts Catalog Guid to code
        /// </summary>
        /// <param name="guid">Catalog GUID</param>
        /// <param name="bypassCache"></param>
        /// <returns>Catalog Code</returns>
        private async Task<string> ConvertCatalogGuidToCode(string guid, bool bypassCache = false)
        {
            var catalogCode = string.Empty;
            if (!string.IsNullOrEmpty(guid))
            {
                var catalog = (await this.GetCatalogAsync(bypassCache)).FirstOrDefault(cat => cat.Guid == guid);
                if (catalog != null)
                {
                    catalogCode = catalog.Code;
                }
                else
                    throw new ArgumentException("Invalid catalog guid " + guid + " in the arguments.");

            }
            return catalogCode;
        }

        /// <summary>
        /// Converts Academic Level Guid to code
        /// </summary>
        /// <param name="guid">Academic Level GUID</param>
        /// <param name="bypassCache"></param>
        /// <returns>Academic Level Code</returns>
        private async Task<string> ConvertAcademicLevelGuidToCode(string guid, bool bypassCache = false)
        {
            var acadLevelCode = string.Empty;

            if (!string.IsNullOrEmpty(guid))
            {
                var acad = (await GetAcademicLevelsAsync(bypassCache)).FirstOrDefault(cat => cat.Guid == guid);
                if (acad != null)
                {
                    acadLevelCode = acad.Code;
                }
                else
                    throw new ArgumentException("Invalid academicLevel " + guid + " in the arguments.");
            }
            return acadLevelCode;
        }

        /// <summary>
        /// Converts Admission Classification Guid to code
        /// </summary>
        /// <param name="guid">Admission Classification GUID</param>
        /// <param name="bypassCache"></param>
        /// <returnsAdmission Classification Code</returns>
        private async Task<string> ConvertAdmissionClassificationGuidToCode(string guid, bool bypassCache = false)
        {
            var admitStatusCode = string.Empty;

            if (!string.IsNullOrEmpty(guid))
            {
                var acad = (await GetAdmissionPopulationsAsync(bypassCache)).FirstOrDefault(cat => cat.Guid == guid);
                if (acad != null)
                {
                    admitStatusCode = acad.Code;
                }
                else
                    throw new ArgumentException("Invalid admission-classification " + guid + " in the arguments.");
            }
            return admitStatusCode;
        }

        /// <summary>
        /// Converts Enrollment Status to code
        /// </summary>
        /// <param name="status">Status </param>
        /// <param name="bypassCache"></param>
        /// <returns>Status Code</returns>
        private async Task<string> ConvertEnrollmentStatusToCode(string status, bool bypassCache = false)
        {
            var statusCode = string.Empty;
            if (!string.IsNullOrEmpty(status))
            {
                var enrollment = (await GetEnrollmentStatusesAsync(bypassCache)).Where(es => es.EnrollmentStatusType.ToString().Equals(status, StringComparison.OrdinalIgnoreCase));
                if (enrollment.Any())
                {
                    foreach (var stat in enrollment)
                    {
                        statusCode += "'" + stat.Code + "' ";
                    }
                }
                else
                    throw new ArgumentException("Invalid enrollmentStatus in the arguments.");

            }
            return statusCode;
        }
    }
}