// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAcademicPeriodProfilesService : StudentCoordinationService, IStudentAcademicPeriodProfilesService
    {
        private readonly IStudentTermRepository _studentTermRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly ITermRepository _termRepository;
        private readonly IPersonRepository _personRepository;
        private ILogger _logger;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;

        public StudentAcademicPeriodProfilesService(IAdapterRegistry adapterRegistry,
            IStudentRepository studentRepository,
            IStudentProgramRepository studentProgramRepository,
            IStudentTermRepository studentTermRepository,
            IAcademicCreditRepository academicCreditRepository,
            ITermRepository termRepository, IStudentReferenceDataRepository studentReferenceDataRepository,
            ICatalogRepository catalogRepository, IPersonRepository personRepository, IReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _studentTermRepository = studentTermRepository;
            _studentRepository = studentRepository;
            _logger = logger;
            _termRepository = termRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _catalogRepository = catalogRepository;
            _personRepository = personRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
            _studentProgramRepository = studentProgramRepository;
            _academicCreditRepository = academicCreditRepository;

        }

        #region private properties and methods


        private IEnumerable<Domain.Student.Entities.StudentLoad> _studentLoad = null;

        private async Task<IEnumerable<Domain.Student.Entities.StudentLoad>> GetStudentLoadsAsync()
        {
            if (_studentLoad == null)
            {
                //always reading from cache as it is the default way. Since we need to read the whole valcode, it make sense to read it all, not just one. 
                _studentLoad = await _studentReferenceDataRepository.GetStudentLoadsAsync();
            }
            return _studentLoad;
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

        private IEnumerable<Domain.Student.Entities.Term> _terms = null;

        private async Task<IEnumerable<Domain.Student.Entities.Term>> GetTermsAsync(bool bypassCache)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods = null;

        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> GetAcademicPeriodsAsync(bool bypassCache)
        {
            if (_academicPeriods == null)
            {
                _academicPeriods = _termRepository.GetAcademicPeriods((await GetTermsAsync(bypassCache)));
            }
            return _academicPeriods;
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
        /// Get an Student Academic Period Profiles from its GUID
        /// </summary>
        /// <returns>A Student Academic Period Profiles DTO <see cref="Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles">object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles> GetStudentAcademicPeriodProfileByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Student Academic Period Profiles.");
            }
            CheckGetStudentAcademicPeriodProfilesPermission();
            try
            {
                var studentTermEntity = await _studentTermRepository.GetStudentTermByGuidAsync(guid);

                if (studentTermEntity == null)
                {
                    throw new KeyNotFoundException("Student Academic Period Profiles not found for GUID " + guid);
                }
                return (await ConvertStudentTermEntityToStudentAcademicPeriodProfileDto(new List<StudentTerm>() { studentTermEntity }, true)).FirstOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Concat("Student Academic Period Profiles not found for GUID '", guid, "'. ", ex.Message));
            }
        }

        /// <summary>
        /// Return a list of StudentAcademicPeriodProfiles objects based on selection criteria.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="person">Id of the student enrolled on the academic program</param>
        /// <param name="academicPeriod">Student Academic Period Profiles starts on or after this date</param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns>List of StudentAcademicPeriodProfiles <see cref="Dtos.StudentAcademicPeriodProfiles"/> objects representing matching Student Academic Period Profiles</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodProfiles>, int>> GetStudentAcademicPeriodProfilesAsync(int offset, int limit, bool bypassCache = false,
            string person = "", string academicPeriod = "")
        {
            try
            {
                //check permissions
                CheckGetStudentAcademicPeriodProfilesPermission();
                // Convert and validate all input parameters
                var newPerson = string.Empty;
                if (!(string.IsNullOrEmpty(person)))
                {
                    newPerson = await _personRepository.GetPersonIdFromGuidAsync(person);
                    if (string.IsNullOrEmpty(newPerson))
                    {
                        //no results
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(new List<Dtos.StudentAcademicPeriodProfiles>(), 0);
                    }
                }
                var newAcademicPeriod = string.Empty;
                if (!string.IsNullOrEmpty(academicPeriod))
                {
                    var terms = _termRepository.GetAcademicPeriods((await this.GetTermsAsync(false)));
                    newAcademicPeriod = ConvertGuidToCode(terms, academicPeriod);
                    if (string.IsNullOrEmpty(newAcademicPeriod))
                    {
                        //no results
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(new List<Dtos.StudentAcademicPeriodProfiles>(), 0);
                    }

                }

                var studentAcadProgEntitiesTuple = await _studentTermRepository.GetStudentTermsAsync(offset, limit, bypassCache, newPerson, newAcademicPeriod);
                if (studentAcadProgEntitiesTuple != null)
                {
                    var studentAcadProgEntities = studentAcadProgEntitiesTuple.Item1;
                    var totalCount = studentAcadProgEntitiesTuple.Item2;

                    if (studentAcadProgEntities != null && studentAcadProgEntities.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(await ConvertStudentTermEntityToStudentAcademicPeriodProfileDto(studentAcadProgEntities.ToList(), bypassCache), totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(new List<Dtos.StudentAcademicPeriodProfiles>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>(new List<Dtos.StudentAcademicPeriodProfiles>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Converts a StudentTerm domain entity to a Student Academic Period Profiles DTO
        /// </summary>
        /// <param name="studentTerm">A list of <see cref="StudentTerm">StudentTerm</see> domain entity</param>
        /// <param name="students"></param>
        /// <param name="bypassCache"></param>
        /// <returns>A <see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see> DTO</returns>
        private async Task<IEnumerable<StudentAcademicPeriodProfiles>> ConvertStudentTermEntityToStudentAcademicPeriodProfileDto(List<StudentTerm> studentTerms, bool bypassCache)
        {
            try
            {
                var studentAcadPeriodProfileDtos = new List<Colleague.Dtos.StudentAcademicPeriodProfiles>();
                //get student Ids
                var ids = new List<string>();
                ids.AddRange(studentTerms.Where(p => (!string.IsNullOrEmpty(p.StudentId)))
                    .Select(p => p.StudentId).Distinct().ToList());
                //get student acad cred Ids
                var stAcadCreds = studentTerms
                             .Where(i => i.StudentAcademicCredentials != null && i.StudentAcademicCredentials.Any())
                             .SelectMany(cr => cr.StudentAcademicCredentials);
                //get academic credits 
                var _academicCredits = new List<AcademicCreditMinimum>();
                if (stAcadCreds != null && stAcadCreds.Any())
                {
                    _academicCredits = (await _academicCreditRepository.GetAcademicCreditMinimumAsync(stAcadCreds.Distinct().ToList())).ToList();
                }
                //get person guids collection
                var _personGuids = await _personRepository.GetPersonGuidsCollectionAsync(ids);
                // get students collections
                var students = await _studentRepository.GetStudentAcademicPeriodProfileStudentInfoAsync(ids);
                //get student programs collection for the selected stuents
                var studentPrograms = new List<StudentProgram>();
                if (students != null && students.Any())
                {
                    var stuProgIds = new List<string>();
                    foreach (var stu in students)
                    {
                        if (stu.ProgramIds != null && stu.ProgramIds.Any())
                        {
                            foreach (var prog in stu.ProgramIds)
                            {
                                if (!string.IsNullOrEmpty(stu.Id) && !string.IsNullOrEmpty(prog))
                                {
                                    stuProgIds.Add(string.Concat(stu.Id, "*", prog));
                                }

                            }
                        }
                    }
                    if (stuProgIds != null && stuProgIds.Any())
                        studentPrograms = await _studentProgramRepository.GetStudentAcademicPeriodProfileStudentProgramInfoAsync(stuProgIds);
                }

                foreach (var studentTerm in studentTerms)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(studentTerm.StudentId))
                        {
                            throw new ArgumentNullException(string.Concat("Student ID is required, Entity:'STUDENT.TERMS', Record ID:'", studentTerm.Guid, "'"));
                        }
                        //we are looking for studentTerm.Term here so calling the guid suber to check if it is in cache and refresh it if it is not so that other calls are always going to be from cache
                        if (!string.IsNullOrEmpty(studentTerm.Term))
                        {
                            var acadPeriodGuid = await _termRepository.GetAcademicPeriodsGuidAsync(studentTerm.Term);
                        }
                        var academicPeriods = (await GetAcademicPeriodsAsync(false)).ToList();

                        var studentAcadPeriodProfileDto = new Colleague.Dtos.StudentAcademicPeriodProfiles();
                        Domain.Student.Entities.Student student = null;
                        if (!string.IsNullOrEmpty(studentTerm.Guid))
                            studentAcadPeriodProfileDto.Id = studentTerm.Guid;
                        else
                            throw new ArgumentNullException(string.Concat("No Guid found, Entity:'STUDENT.TERMS', Record ID:'", studentTerm.StudentId,"*",studentTerm.Term, "*", studentTerm.AcademicLevel,  "'"));

                        if (!string.IsNullOrEmpty(studentTerm.StudentId))
                        {
                            var guid = _personGuids.FirstOrDefault(i => i.Key.Equals(studentTerm.StudentId, StringComparison.OrdinalIgnoreCase));

                            if (!guid.Equals(default(KeyValuePair<string, string>)) && !string.IsNullOrEmpty(guid.Value))
                            {
                                studentAcadPeriodProfileDto.Person = new Dtos.GuidObject2(guid.Value);
                            }
                            else
                            {
                                throw new ArgumentNullException(string.Concat("No Guid for Person found, Entity:'PERSONS', Record ID:'", studentTerm.StudentId, "'"));
                            }
                            // get student entities
                            if (students != null && students.Any())
                            {
                                student = students.FirstOrDefault(stu => stu.Id == studentTerm.StudentId);
                            }

                            if (student == null)
                            {
                                throw new ArgumentNullException(string.Concat("Student record is not found, Entity:'STUDENT', Record ID:'", studentTerm.StudentId, "'"));
                            }
                            //get type info
                            if ((student.StudentTypeInfo != null) && (student.StudentTypeInfo.Any()))
                            {
                                var term = academicPeriods.FirstOrDefault(t => t.Code == studentTerm.Term);
                                var termEndDate = (term == null) ? DateTime.Now : term.EndDate;
                                var type = student.StudentTypeInfo
                                    .Where(st => st.TypeDate <= termEndDate)
                                    .OrderByDescending(x => x.TypeDate)
                                    .FirstOrDefault();
                                if (type != null && !string.IsNullOrEmpty(type.Type))
                                {
                                    var studentType = await _studentReferenceDataRepository.GetStudentTypesGuidAsync(type.Type);
                                    if (!string.IsNullOrEmpty(studentType))
                                        studentAcadPeriodProfileDto.Type = studentType == null ? null : new GuidObject2(studentType);
                                }
                            }
                        }

                        //process start term
                        if (!string.IsNullOrEmpty(studentTerm.Term))
                        {
                            if (academicPeriods != null && academicPeriods.Any())
                            {
                                var term = academicPeriods.FirstOrDefault(t => t.Code == studentTerm.Term);
                                studentAcadPeriodProfileDto.AcademicPeriod = term == null ? null : new Dtos.GuidObject2(term.Guid);
                                if (term != null)
                                {
                                    StudentProgram studentProgram = null;
                                    var enrollmentStatuses = await GetEnrollmentStatusesAsync(bypassCache);

                                    foreach (var program in student.ProgramIds)
                                    {
                                        var acadProgram = (await GetAcademicProgramsAsync(false)).FirstOrDefault(x => x.Code == program);
                                        if (acadProgram != null && acadProgram.AcadLevelCode == studentTerm.AcademicLevel)
                                        {
                                            //studentProgram = await _studentProgramRepository.GetAsync(studentTerm.StudentId, program);
                                            if (studentPrograms != null && studentPrograms.Any())
                                                studentProgram = studentPrograms.FirstOrDefault(sp => sp.StudentId == studentTerm.StudentId && sp.ProgramCode == program);
                                            //break;

                                            var studentProgramsStatus = new List<StudentProgramStatus>();

                                            if (studentProgram != null)
                                                foreach (var status in studentProgram.StudentProgramStatuses)
                                                {
                                                    if ((status.StatusDate >= term.StartDate) && (status.StatusDate <= term.EndDate))
                                                        studentProgramsStatus.Add(status);

                                                }

                                            // from the qualifying studentPrograms, get the most recent active

                                            var myStudentProgStatuses = studentProgramsStatus.OrderByDescending(sps => sps.StatusDate);

                                            if (myStudentProgStatuses != null && myStudentProgStatuses.Any())
                                            {
                                                foreach (var myStudentProgStatus in myStudentProgStatuses)
                                                {

                                                    if (myStudentProgStatus != null)
                                                    {
                                                        // using the most recent studentProgram from that term, lookup the enrollmentstatus guid
                                                        var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.Code == myStudentProgStatus.Status);
                                                        if (enrollmentStatus != null && enrollmentStatus.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active)
                                                        {
                                                            studentAcadPeriodProfileDto.AcademicPeriodEnrollmentStatus = new GuidObject2(enrollmentStatus.Guid);
                                                            break;
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                        // If we got a legit enrollment status code from this program, no need to check further

                                        if (studentAcadPeriodProfileDto.AcademicPeriodEnrollmentStatus != null)
                                        {
                                            break;
                                        }
                                    }

                                    // Determine valid residency for this academic period.
                                    string residencyCode = string.Empty;

                                    if (student != null)
                                    {
                                        // For student's residencies (in descending date order with null dates on top) find first residency before/on term end date. 
                                        var res = student.StudentResidencies.FirstOrDefault(sr => sr.Date <= term.EndDate || sr.Date == null);
                                        if (res != null)
                                        {
                                            residencyCode = res.Residency;
                                            // lookup the residency guid
                                            if (!string.IsNullOrEmpty(residencyCode))
                                            {
                                                var residency = await _studentRepository.GetResidencyStatusGuidAsync(residencyCode);

                                                if (!string.IsNullOrEmpty(residency))
                                                {
                                                    studentAcadPeriodProfileDto.Residency = new GuidObject2(residency);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ((studentTerm.StudentTermStatuses != null) && (studentTerm.StudentTermStatuses.Any()))
                        {
                            var currentStatus = studentTerm.StudentTermStatuses.OrderByDescending(sts => sts.StatusDate).FirstOrDefault();
                            if (currentStatus != null && currentStatus.StatusDate != DateTime.MinValue && !string.IsNullOrEmpty(currentStatus.Status))
                            {
                                var status = await _studentReferenceDataRepository.GetStudentStatusesGuidAsync(currentStatus.Status);
                                if (status != null)
                                {
                                    studentAcadPeriodProfileDto.StudentStatus = new Dtos.GuidObject2(status);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(studentTerm.StudentLoad))
                        {
                            //STUDENT.LOADS
                            var studentLoads = (await GetStudentLoadsAsync()).ToList();
                            if (studentLoads.Any())
                            {
                                var studentLoad = studentLoads.FirstOrDefault(sl => sl.Code == studentTerm.StudentLoad);
                                if ((studentLoad != null) && !(string.IsNullOrEmpty(studentLoad.Sp1)) &&
                                    (new[] { "1", "2", "3" }.Any(c => studentLoad.Sp1.Contains(c))))
                                {
                                    switch (studentLoad.Code)
                                    {
                                        case "P":
                                            studentAcadPeriodProfileDto.AcademicLoad = AcademicLoad.PartTime;
                                            break;
                                        case "F":
                                            studentAcadPeriodProfileDto.AcademicLoad = AcademicLoad.FullTime;
                                            break;
                                        case "O":
                                            studentAcadPeriodProfileDto.AcademicLoad = AcademicLoad.OverLoad;
                                            break;

                                    }
                                }
                            }
                        }

                        var measures = new List<PerformanceMeasureDtoProperty>();
                        var measure = new PerformanceMeasureDtoProperty();

                        if (!string.IsNullOrEmpty(studentTerm.AcademicLevel))
                        {
                            var acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(studentTerm.AcademicLevel);
                            if (!string.IsNullOrEmpty(acadLevel))
                            {
                                measure.Level = new GuidObject2(acadLevel);
                            }
                        }

                        var classLevel = string.Empty;
                        if ((student.StudentAcademicLevels != null) && (student.StudentAcademicLevels.Any()) && (!string.IsNullOrEmpty(studentTerm.AcademicLevel)))
                        {
                            var studentAcademicLevel = student.StudentAcademicLevels.FirstOrDefault(sal => sal.AcademicLevel == studentTerm.AcademicLevel);
                            if (studentAcademicLevel != null)
                                classLevel = studentAcademicLevel.ClassLevel;
                        }

                        if (!string.IsNullOrEmpty(classLevel))
                        {
                            var studentClassification = await _studentReferenceDataRepository.GetStudentClassificationGuidAsync(classLevel);
                            if (!string.IsNullOrEmpty(studentClassification))
                            {
                                measure.Classification = new GuidObject2(studentClassification);
                            }
                        }

                        decimal totalGradePoints = 0;
                        decimal totalGpaCredit = 0;

                        //var studentAcademicCredits = (await _academicCreditRepository.GetAsync(studentTerm.StudentAcademicCredentials)).ToList();
                        if (_academicCredits != null && _academicCredits.Any())
                        {
                            var acadCreds = _academicCredits.Where(i => studentTerm.StudentAcademicCredentials.Contains(i.Id));
                            foreach (var academicCredential in acadCreds)
                            {
                                totalGradePoints += academicCredential.GradePoints;
                                totalGpaCredit += academicCredential.GpaCredit ?? 0m;
                            }

                            if (!(totalGradePoints == 0 || totalGpaCredit == 0))
                            {
                                var gpa = totalGradePoints / totalGpaCredit;
                                if (gpa != 0)
                                {
                                    measure.PerformanceMeasure = gpa.ToString("#.##");
                                }

                            }
                        }
                        measures.Add(measure);
                        studentAcadPeriodProfileDto.Measures = measures;
                        studentAcadPeriodProfileDtos.Add(studentAcadPeriodProfileDto);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentNullException(string.Concat(ex.Message, "Student terms is not found, Entity:'STUDENT.TERMS', Record ID:'", studentTerm.Guid, "'"));
                    }
                }

                return studentAcadPeriodProfileDtos;

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                if (_logger.IsErrorEnabled)
                {
                    _logger.Error(ex, "Student Academic Period Profiles exception occurred:");
                }
                throw new KeyNotFoundException("Student Academic Period Profiles exception occurred. " + ex.Message);
            }
            
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student Academic Period Profiles.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetStudentAcademicPeriodProfilesPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewStudentAcademicPeriodProfile);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Student Academic Period Profiles.");
            }
        }

    }
}