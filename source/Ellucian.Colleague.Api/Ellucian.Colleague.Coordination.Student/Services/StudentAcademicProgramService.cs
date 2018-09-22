// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
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


        private IEnumerable<Domain.Base.Entities.OtherDegree> _otherDegrees = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherDegree>> GetOtherDegreesAsync(bool bypassCache)
        {
            if (_otherDegrees == null)
            {
                _otherDegrees = await _referenceDataRepository.GetOtherDegreesAsync(bypassCache);
            }
            return _otherDegrees;
        }

        private IEnumerable<Domain.Base.Entities.OtherCcd> _otherCcds = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherCcd>> GetOtherCcdAsync(bool bypassCache)
        {
            if (_otherCcds == null)
            {
                _otherCcds = await _referenceDataRepository.GetOtherCcdsAsync(bypassCache);
            }
            return _otherCcds;
        }

        private IEnumerable<Domain.Base.Entities.OtherHonor> _otherHonors = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherHonor>> GetOtherHonorsAsync(bool bypassCache)
        {
            if (_otherHonors == null)
            {
                _otherHonors = await _referenceDataRepository.GetOtherHonorsAsync(bypassCache);
            }
            return _otherHonors;
        }

        private IEnumerable<Domain.Base.Entities.OtherMajor> _otherMajor = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherMajor>> GetOtherMajorsAsync(bool bypassCache)
        {
            if (_otherMajor == null)
            {
                _otherMajor = await _referenceDataRepository.GetOtherMajorsAsync(bypassCache);
            }
            return _otherMajor;
        }

        private IEnumerable<Domain.Base.Entities.OtherMinor> _otherMinor = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherMinor>> GetOtherMinorsAsync(bool bypassCache)
        {
            if (_otherMinor == null)
            {
                _otherMinor = await _referenceDataRepository.GetOtherMinorsAsync(bypassCache);
            }
            return _otherMinor;
        }

        private IEnumerable<Domain.Base.Entities.OtherSpecial> _otherSpecials = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherSpecial>> GetOtherSpecialsAsync(bool bypassCache)
        {
            if (_otherSpecials == null)
            {
                _otherSpecials = await _referenceDataRepository.GetOtherSpecialsAsync(bypassCache);
            }
            return _otherSpecials;
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

        private IEnumerable<AcademicDiscipline> _acadDisciplines;
        private async Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesAsync(bool bypassCache)
        {
            if (_acadDisciplines == null)
            {
                _acadDisciplines = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache); ;

            }
            return _acadDisciplines;
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
                CheckGetStudentAcademicProgramPermission();
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
                CheckGetStudentAcademicProgramPermission();
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
                        var programCode = ConvertCodeToGuid((await this.GetAcademicProgramsAsync(bypassCache)), StuProgram.ProgramCode);
                        if (!string.IsNullOrEmpty(programCode))
                        {
                            studentProgramDto.Program = new Dtos.GuidObject2(programCode);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.CatalogCode))
                    {
                        var catalogCode = await ConvertAcademicCatalogCodeToGuid((await this.GetCatalogAsync(bypassCache)), StuProgram.CatalogCode);
                        if (catalogCode != null)
                        {
                            studentProgramDto.Catalog = catalogCode;
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
                        var location = ConvertCodeToGuid(await this.GetLocationsAsync(bypassCache), StuProgram.Location);
                        if (location != null)
                        {
                            studentProgramDto.Site = new Dtos.GuidObject2(location);
                        }
                    }
                    //process stpr.dept for disciplines, administeringInstitutionUnit, programOwner
                    Dtos.GuidObject2 deptGuid = null;
                   if (!string.IsNullOrEmpty(StuProgram.DepartmentCode))
                    {
                        var dept = ConvertCodeToGuid((await this.GetDepartmentsAsync(bypassCache)), StuProgram.DepartmentCode);
                        if (dept != null)
                        {
                            deptGuid = new Dtos.GuidObject2(dept);
                            studentProgramDto.ProgramOwner = deptGuid;
                        }
                    }
                    //process start term
                    if (!string.IsNullOrEmpty(StuProgram.StartTerm))
                    {
                        var terms =  await GetAcademicPeriods(bypassCache);
                        var term = ConvertCodeToGuid(terms, StuProgram.StartTerm);
                        if (term != null)
                        {
                            studentProgramDto.StartTerm = new Dtos.GuidObject2(term);
                        }
                    }
                    //process academic level
                    if (!string.IsNullOrEmpty(StuProgram.AcademicLevelCode))
                    {
                        var acadLevel = ConvertCodeToGuid((await this.GetAcademicLevelsAsync(bypassCache)), StuProgram.AcademicLevelCode);
                        if (acadLevel != null)
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
                            enroll.Detail = new Dtos.GuidObject2() {Id = enrollStatus.Guid};
                            switch (enrollStatus.EnrollmentStatusType)
                            {
                                case EnrollmentStatusType.active:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                                    break;
                                case EnrollmentStatusType.complete:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                                    break;
                                case EnrollmentStatusType.inactive:
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
                    studentProgramDto.CreditsEarned = StuProgram.CreditsEarned == (decimal) 0 ? null : StuProgram.CreditsEarned;
                    studentAcadProgramDtos.Add(studentProgramDto);
                }


                return studentAcadProgramDtos;
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
                        var programCode = ConvertCodeToGuid((await this.GetAcademicProgramsAsync(bypassCache)), StuProgram.ProgramCode);
                        if (!string.IsNullOrEmpty(programCode))
                        {
                            studentProgramDto.AcademicProgram = new Dtos.GuidObject2(programCode);
                        }
                    }
                    if (!string.IsNullOrEmpty(StuProgram.CatalogCode))
                    {
                        var catalogCode = await ConvertAcademicCatalogCodeToGuid((await this.GetCatalogAsync(bypassCache)), StuProgram.CatalogCode);
                        if (catalogCode != null)
                        {
                            studentProgramDto.AcademicCatalog = catalogCode;
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
                        var location = ConvertCodeToGuid(await this.GetLocationsAsync(bypassCache), StuProgram.Location);
                        if (location != null)
                        {
                            studentProgramDto.Site = new Dtos.GuidObject2(location);
                        }
                    }
                    //process stpr.dept for disciplines, administeringInstitutionUnit, programOwner
                    Dtos.GuidObject2 deptGuid = null;
                    if (!string.IsNullOrEmpty(StuProgram.DepartmentCode))
                    {
                        var dept = ConvertCodeToGuid((await this.GetDepartmentsAsync(bypassCache)), StuProgram.DepartmentCode);
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
                        var terms = await this.GetAcademicPeriods(bypassCache);
                        //process start term
                        if (!string.IsNullOrEmpty(StuProgram.StartTerm))
                        {
                            var term = ConvertCodeToGuid(terms, StuProgram.StartTerm);
                            if (term != null)
                            {
                                acadPeriods.Starting = new Dtos.GuidObject2(term);
                            }
                        }
                        //process expected graduation term
                        if (!string.IsNullOrEmpty(StuProgram.AnticipatedCompletionTerm))
                        {
                            var term = ConvertCodeToGuid(terms, StuProgram.AnticipatedCompletionTerm);
                            if (term != null)
                            {
                                acadPeriods.ExpectedGraduation = new Dtos.GuidObject2(term);
                            }
                        }
                        //process actual graduation term
                        if (!string.IsNullOrEmpty(StuProgram.GradTerm))
                        {
                            var term = ConvertCodeToGuid(terms, StuProgram.GradTerm);
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
                        var acadLevel = ConvertCodeToGuid((await this.GetAcademicLevelsAsync(bypassCache)), StuProgram.AcademicLevelCode);
                        if (acadLevel != null)
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
                                case EnrollmentStatusType.active:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                                    break;
                                case EnrollmentStatusType.complete:
                                    enroll.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                                    break;
                                case EnrollmentStatusType.inactive:
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
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Student Academic Program not found.");
            }
        }

        /// <summary>
        /// Convert Academic Catalog code to GUID
        /// </summary>
        /// <param name="academicCatalogs">List of Catalogs Entities</param>
        /// <param name="academicCatalogCode">Catalog Code</param>
        /// <returns>Academic Catalog GUID</returns>
        private async Task<Ellucian.Colleague.Dtos.GuidObject2> ConvertAcademicCatalogCodeToGuid(IEnumerable<Catalog> academicCatalogs, string academicCatalogCode)
        {
            Ellucian.Colleague.Dtos.GuidObject2 guidObject = null;
            
            if (!string.IsNullOrEmpty(academicCatalogCode))
            {
                if (academicCatalogs != null && academicCatalogs.Any())
                {
                    var acadCatalog = academicCatalogs.FirstOrDefault(a => a.Code == academicCatalogCode);
                    if (acadCatalog != null)
                    {
                        var acadGuid = acadCatalog.Guid;
                        if (!string.IsNullOrEmpty(acadGuid))
                        {
                            guidObject = new Ellucian.Colleague.Dtos.GuidObject2(acadGuid);
                        }
                    }
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
                    var honor = (await this.GetOtherHonorsAsync(bypassCache)).FirstOrDefault(a => a.Code == hnr);
                    if (honor != null)
                    {
                        if (guidObjects == null)
                        {
                            guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                        }
                        guidObjects.Add(new Dtos.GuidObject2(honor.Guid));
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
                    var cert = (await this.GetOtherCcdAsync(bypassCache)).FirstOrDefault(a => a.Code == ccd);
                    if (cert != null)
                    {
                        if (guidObjects == null)
                        {
                            guidObjects = new List<Dtos.GuidObject2>();
                        }
                        guidObjects.Add(new Dtos.GuidObject2(cert.Guid));
                    }
                }
            }

            if (!string.IsNullOrEmpty(degreeCode))
            {
                var degree = (await GetOtherDegreesAsync(bypassCache)).FirstOrDefault(a => a.Code == degreeCode);
                if (degree != null)
                {
                    if (guidObjects == null)
                    {
                        guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                    }
                    guidObjects.Add(new Dtos.GuidObject2(degree.Guid));
                }
            }

            return guidObjects;
        }


        /// <summary>
        /// Convert Academic Discipline to GUID. Displine include majors, minors & specializations from academic program and additional majors, minors & specializations from STUDENT.PROGRAMS.
        /// </summary>
        /// <param name="majors">list of majors</param>
        /// <param name="minors">list of minors</param>
        /// <param name="specializations">list of specializations</param>
        /// <returns>Academic Discipline GUIDs</returns>
        //credential include degree & credentials from academic program and additional credentials from STUDENT.PROGRAMS.

        private async Task<List<Dtos.StudentAcademicProgramDisciplines>> ConvertDisciplineCodeToGuidAsync(List<string> majors, List<string> minors, List<string> specializations, Dtos.GuidObject2 deptGuid, bool bypassCache = false)
        {
            List<Dtos.StudentAcademicProgramDisciplines> disciplineObjects = null;

            if ((majors != null) && (majors.Any()))
            {
                foreach (var major in majors)
                {
                    var maj = (await this.GetOtherMajorsAsync(bypassCache)).FirstOrDefault(a => a.Code == major);
                    if (maj != null)
                    {
                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines() {Discipline = new Dtos.GuidObject2(maj.Guid), AdministeringInstitutionUnit = deptGuid});

                    }
                }
            }

            if ((minors != null) && (minors.Any()))
            {
                foreach (var minor in minors)
                {
                    var min = (await this.GetOtherMinorsAsync(bypassCache)).FirstOrDefault(a => a.Code == minor);
                    if (min != null)
                    {
                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines() {Discipline = new Dtos.GuidObject2(min.Guid), AdministeringInstitutionUnit = deptGuid});

                    }
                }
            }

            if ((specializations != null) && (specializations.Any()))
            {
                foreach (var specialization in specializations)
                {
                    var special = (await this.GetOtherSpecialsAsync(bypassCache)).FirstOrDefault(a => a.Code == specialization);
                    if (special != null)
                    {
                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.StudentAcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.StudentAcademicProgramDisciplines() {Discipline = new Dtos.GuidObject2(special.Guid), AdministeringInstitutionUnit = deptGuid});

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
        public async Task<Dtos.StudentAcademicPrograms> CreateStudentAcademicProgramAsync(Dtos.StudentAcademicPrograms studentAcadProgramDto)
        {
            // Confirm that user has permissions to create Student Academic Program
            CheckCreateStudentAcademicProgramPermission();

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgramEntity = await ConvertStudentAcademicProgramDtoToEntityAsync(studentAcadProgramDto, true);
            var createdStudentAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStudentAcadProgEntity.Add(await _studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(studentAcadProgramEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto(createdStudentAcadProgEntity, false)).FirstOrDefault();
        }

        /// <summary>
        /// Creates a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgramDto2">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the created student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms2> CreateStudentAcademicProgram2Async(Dtos.StudentAcademicPrograms2 studentAcadProgramDto)
        {
            // Confirm that user has permissions to create Student Academic Program
            CheckCreateStudentAcademicProgramPermission();

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgramEntity = await ConvertStudentAcademicProgramDtoToEntity2Async(studentAcadProgramDto, true);
            var createdStudentAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStudentAcadProgEntity.Add(await _studentAcademicProgramRepository.CreateStudentAcademicProgramAsync(studentAcadProgramEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto2Async(createdStudentAcadProgEntity, false)).FirstOrDefault();
        }

        /// <summary>
        /// Update a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgDto">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the updated student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms> UpdateStudentAcademicProgramAsync(Dtos.StudentAcademicPrograms studentAcadProgDto)
        {
            // Confirm that user has permissions to create/update Student Academic Program
            CheckCreateStudentAcademicProgramPermission();

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgEntity = await ConvertStudentAcademicProgramDtoToEntityAsync(studentAcadProgDto, true);
            var createdStuAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStuAcadProgEntity.Add(await _studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcadProgEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto(createdStuAcadProgEntity, true)).FirstOrDefault();
        }

        /// <summary>
        /// Update a Student Academic Program
        /// </summary>
        /// <param name="studentAcadProgDto2">An Student Academic Program domain object</param>
        /// <returns>An Student Academic Program DTO object for the updated student programs</returns>
        public async Task<Dtos.StudentAcademicPrograms2> UpdateStudentAcademicProgram2Async(Dtos.StudentAcademicPrograms2 studentAcadProgDto)
        {
            // Confirm that user has permissions to create/update Student Academic Program
            CheckCreateStudentAcademicProgramPermission();

            //Extensibility
            _studentAcademicProgramRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the Student Academic Program, convert the resulting entity back to a DTO, and return it
            var studentAcadProgEntity = await ConvertStudentAcademicProgramDtoToEntity2Async(studentAcadProgDto, true);
            var createdStuAcadProgEntity = new List<StudentAcademicProgram>();
            var inst = GetDefaultInstitutionId();
            createdStuAcadProgEntity.Add(await _studentAcademicProgramRepository.UpdateStudentAcademicProgramAsync(studentAcadProgEntity, inst));
            return (await ConvertStudentAcademicProgramEntityToDto2Async(createdStuAcadProgEntity, true)).FirstOrDefault();
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create and update Student Academic Programs.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateStudentAcademicProgramPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

            // User is not allowed to create or update courses without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create or update Student Academic Programs.");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student Academic Programs.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetStudentAcademicProgramPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentAcademicProgramConsent);

            // User is not allowed to create or update Student Academic Program without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Student Academic Programs.");
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
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentException(string.Concat(" Student ID '", stuAcadProgramsDto.Student.Id.ToString(), "' was not found. Valid Student is required."));
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
                    EnrollmentStatus enrollStatus = enrollStatuses.FirstOrDefault(ct => ct.Guid == stuAcadProgramsDto.EnrollmentStatus.Detail.Id);

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
                        case AcademicCredentialType.Certificate:
                            studentProgEntity.AddCcds(credential.Code);
                            break;
                        case AcademicCredentialType.Degree:
                            degrees.Add(credential.Code);
                            break;
                        //produce error if honor codes are included
                        case AcademicCredentialType.Honorary:
                            throw new ArgumentException(credential.Guid + " is an Honor code. Honor code is not allowed during Student Academic Program.");

                        case AcademicCredentialType.Diploma:
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
                            case AcademicDisciplineType.Major:
                                studentProgEntity.AddMajors(discipline.Code);
                                break;
                            //getting minors
                            case AcademicDisciplineType.Minor:
                                studentProgEntity.AddMinors(discipline.Code);
                                break;
                            //getting specializations
                            case AcademicDisciplineType.Concentration:
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
            var depts = await this.GetDepartmentsAsync(bypassCache); //await _referenceDataRepository.GetDepartmentsAsync(bypassCache);
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
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentException(string.Concat(" Student ID '", stuAcadProgramsDto.Student.Id.ToString(), "' was not found. Valid Student is required."));
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
                        EnrollmentStatus enrollStatus = enrollStatuses.FirstOrDefault(ct => ct.Guid == stuAcadProgramsDto.EnrollmentStatus.Detail.Id);

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
                        case AcademicCredentialType.Certificate:
                            studentProgEntity.AddCcds(credential.Code);
                            break;
                        case AcademicCredentialType.Degree:
                            degrees.Add(credential.Code);
                            break;
                        //produce error if honor codes are included
                        case AcademicCredentialType.Honorary:
                            throw new ArgumentException(credential.Guid + " is an Honor code. Honor code is not allowed during Student Academic Program.");

                        case AcademicCredentialType.Diploma:
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
                            case AcademicDisciplineType.Major:
                                studentProgEntity.AddMajors(discipline.Code);
                                break;
                            //getting minors
                            case AcademicDisciplineType.Minor:
                                studentProgEntity.AddMinors(discipline.Code);
                                break;
                            //getting specializations
                            case AcademicDisciplineType.Concentration:
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
                //check permissions
                CheckGetStudentAcademicProgramPermission();
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
                //check permissions
                CheckGetStudentAcademicProgramPermission();
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
                case AcademicCredentialType.Certificate:
                    returnTuple = new Tuple<string, string>(credentialObject.Code, string.Empty);
                    break;
                //this is the DEGREE return
                case AcademicCredentialType.Degree:
                    returnTuple = new Tuple<string, string>(string.Empty, credentialObject.Code);
                    break;
                case AcademicCredentialType.Honorary:
                    throw new ArgumentException(credential + " is an Honor code. Honor code is not allowed to use as a credential filter Student Academic Program.");
                case AcademicCredentialType.Diploma:
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
                    case AcademicCredentialType.Certificate:
                        returnTuples.Add(new Tuple<string, string>(credentialObject.Code, string.Empty));
                        break;
                    //this is the DEGREE return
                    case AcademicCredentialType.Degree:
                        returnTuples.Add(new Tuple<string, string>(string.Empty, credentialObject.Code));
                        break;
                    case AcademicCredentialType.Honorary:
                        throw new ArgumentException(sources + " is an Honor code. Honor code is not allowed to use as a credential filter Student Academic Program.");
                    case AcademicCredentialType.Diploma:
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
            if ( !string.IsNullOrEmpty(guid))
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