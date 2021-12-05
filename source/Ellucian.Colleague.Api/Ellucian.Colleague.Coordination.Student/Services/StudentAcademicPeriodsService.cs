// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAcademicPeriodsService : StudentCoordinationService, IStudentAcademicPeriodsService
    {
        private readonly IStudentAcademicPeriodRepository _studentAcademicPeriodRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ITermRepository _termRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private ILogger _logger;


        public StudentAcademicPeriodsService(IAdapterRegistry adapterRegistry,
            IStudentRepository studentRepository,
            IStudentProgramRepository studentProgramRepository,
            IStudentAcademicPeriodRepository studentAcademicPeriodRepository,
            ITermRepository termRepository, IStudentReferenceDataRepository studentReferenceDataRepository,
            IPersonRepository personRepository, IReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _studentAcademicPeriodRepository = studentAcademicPeriodRepository;
            _studentRepository = studentRepository;
            _logger = logger;
            _termRepository = termRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _personRepository = personRepository;
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
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

        private IEnumerable<Domain.Student.Entities.Term> _terms = null;

        private async Task<IEnumerable<Domain.Student.Entities.Term>> GetTermsAsync(bool bypassCache)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        #endregion

        /// <summary>
        /// Get a studentAcademicPeriods by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAcademicPeriods in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAcademicPeriods">studentAcademicPeriods</see></returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicPeriods> GetStudentAcademicPeriodsByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a student-academic-periods.");
            }

            try
            {
                StudentAcademicPeriod studentTermEntity = null;
                try
                {
                    studentTermEntity = await _studentAcademicPeriodRepository.GetStudentAcademicPeriodByGuidAsync(guid);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                if (studentTermEntity == null)
                {
                    throw new KeyNotFoundException("student-academic-periods not found for GUID " + guid);
                }
                return (await ConvertStudentTermEntityToStudentAcademicPeriodsDto(new List<StudentAcademicPeriod>() { studentTermEntity }, true)).FirstOrDefault();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-academic-periods not found for GUID " + guid);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Concat("student-academic-periods exception occurred. ", ex.Message), "Bad.Data", guid);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Return a list of StudentAcademicPeriods objects based on selection criteria.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="person">personFilter</param>
        /// <param name="filter">criteriaFilter</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>List of StudentAcademicPeriods <see cref="Dtos.StudentAcademicPeriods"/> objects representing matching student-academic-periods</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriods>, int>> GetStudentAcademicPeriodsAsync(int offset, int limit,
            string personFilter = "", StudentAcademicPeriods filter = null, bool bypassCache = false)
        {
            try
            {
                #region convert filters
                // Convert and validate all input parameters
                var newPerson = string.Empty;
                var newAcademicPeriod = string.Empty;
                List<string> newStudentStatuses = null;
                string[] filterPersonIds = new List<string>().ToArray();

                if (!string.IsNullOrEmpty(personFilter))
                {
                    if (!string.IsNullOrEmpty(personFilter))
                    {
                        var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                        if (personFilterKeys != null)
                        {
                            filterPersonIds = personFilterKeys;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                        }
                    }
                }

                if (filter != null)
                {
                    if (filter.Person != null && !string.IsNullOrEmpty(filter.Person.Id))
                    {
                        try
                        {
                            newPerson = await _personRepository.GetPersonIdFromGuidAsync(filter.Person.Id);
                            if (string.IsNullOrEmpty(newPerson))
                            {
                                return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                            }
                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                        }
                    }

                    if (filter.AcademicStatuses != null && filter.AcademicStatuses.Any())
                    {
                        try
                        {
                            var allStudentStatuses = await _studentReferenceDataRepository.GetStudentStatusesAsync(bypassCache);
                            if (allStudentStatuses == null)
                            {
                                return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                            }

                            newStudentStatuses = new List<string>();

                            foreach (var academicStatus in filter.AcademicStatuses)
                            {
                                if (academicStatus.AcademicPeriodStatus != null && !string.IsNullOrEmpty(academicStatus.AcademicPeriodStatus.Id))
                                {
                                    var newStatus = allStudentStatuses.FirstOrDefault(x => x.Guid.Equals(academicStatus.AcademicPeriodStatus.Id,
                                        StringComparison.OrdinalIgnoreCase));
                                    if (newStatus == null)
                                    {
                                        return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                                    }

                                    newStudentStatuses.Add(newStatus.Code);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                        }
                    }

                    if (filter.AcademicPeriod != null && !string.IsNullOrEmpty(filter.AcademicPeriod.Id))
                    {
                        try
                        {
                            newAcademicPeriod = await _termRepository.GetAcademicPeriodsCodeFromGuidAsync(filter.AcademicPeriod.Id);
                            if (string.IsNullOrEmpty(newAcademicPeriod))
                            {
                                return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                            }
                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
                        }
                    }
                }
                #endregion

                Tuple<IEnumerable<StudentAcademicPeriod>, int> studentAcadProgEntitiesTuple = null;
                try
                {
                    studentAcadProgEntitiesTuple = await _studentAcademicPeriodRepository
                        .GetStudentAcademicPeriodsAsync(offset, limit, bypassCache, newPerson, newAcademicPeriod, filterPersonIds, newStudentStatuses);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data");
                    throw IntegrationApiException;
                }
                if (studentAcadProgEntitiesTuple != null)
                {
                    var studentAcadProgEntities = studentAcadProgEntitiesTuple.Item1;
                    var totalCount = studentAcadProgEntitiesTuple.Item2;

                    if (studentAcadProgEntities != null && studentAcadProgEntities.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(await ConvertStudentTermEntityToStudentAcademicPeriodsDto(studentAcadProgEntities.ToList(), bypassCache), totalCount);
                    }

                    return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), totalCount);
                }

                return new Tuple<IEnumerable<Dtos.StudentAcademicPeriods>, int>(new List<Dtos.StudentAcademicPeriods>(), 0);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Concat("student-academic-periods exception occurred. ", ex.Message), "Bad.Data");
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Converts a StudentAcademicPeriod domain entity to a student-academic-periods DTO
        /// </summary>
        /// <param name="studentTerm">A list of <see cref="StudentAcademicPeriod">StudentAcademicPeriod</see> domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>A <see cref="StudentAcademicPeriods">StudentAcademicPeriods</see> DTO</returns>
        private async Task<IEnumerable<StudentAcademicPeriods>> ConvertStudentTermEntityToStudentAcademicPeriodsDto(List<StudentAcademicPeriod> studentAcademicPeriods, bool bypassCache)
        {
            var studentAcadPeriodProfileDtos = new List<Colleague.Dtos.StudentAcademicPeriods>();

            try
            {
                var ids = new List<string>();
                ids.AddRange(studentAcademicPeriods.Where(p => (!string.IsNullOrEmpty(p.StudentId)))
                    .Select(p => p.StudentId).Distinct().ToList());
                var _personGuids = await _personRepository.GetPersonGuidsCollectionAsync(ids);

                foreach (var studentAcademicPeriod in studentAcademicPeriods)
                {

                    var studentAcadPeriodProfileDto = new Colleague.Dtos.StudentAcademicPeriods();

                    if (string.IsNullOrEmpty(studentAcademicPeriod.Guid))
                        IntegrationApiExceptionAddError(string.Format("No student-academic-periods guid found for student id '{0}, term id '{1}'.",
                            studentAcademicPeriod.StudentId, studentAcademicPeriod.Term),
                                "Validation.Exception", studentAcademicPeriod.Guid);
                    else
                        studentAcadPeriodProfileDto.Id = studentAcademicPeriod.Guid;

                    if (!string.IsNullOrEmpty(studentAcademicPeriod.Term))
                    {
                        var acadPeriodGuid = await _termRepository.GetAcademicPeriodsGuidAsync(studentAcademicPeriod.Term);

                        if (string.IsNullOrEmpty(acadPeriodGuid))
                        {
                            IntegrationApiExceptionAddError(string.Format("No guid found for term id '{0}'.", studentAcademicPeriod.Term),
                                 "Validation.Exception", studentAcademicPeriod.Guid);
                        }
                        else
                        {
                            studentAcadPeriodProfileDto.AcademicPeriod = new GuidObject2(acadPeriodGuid);
                        }
                    }


                    #region person
                    if (string.IsNullOrEmpty(studentAcademicPeriod.StudentId))
                    {
                        IntegrationApiExceptionAddError("Student ID is required, Entity:'STUDENT.TERMS'", "Validation.Exception", studentAcademicPeriod.Guid);
                    }
                    else
                    {
                        var guid = _personGuids.FirstOrDefault(i => i.Key.Equals(studentAcademicPeriod.StudentId, StringComparison.OrdinalIgnoreCase));

                        if (guid.Equals(default(KeyValuePair<string, string>)) && string.IsNullOrEmpty(guid.Value))
                        {
                            IntegrationApiExceptionAddError(string.Format("No guid found for person id '{0}'.", studentAcademicPeriod.StudentId),
                                 "Validation.Exception", studentAcademicPeriod.Guid);
                        }
                        else
                        {
                            studentAcadPeriodProfileDto.Person = new Dtos.GuidObject2(guid.Value); ;
                        }

                    }
                    #endregion

                    if ((studentAcademicPeriod.StudentTerms != null) && (studentAcademicPeriod.StudentTerms.Any()))
                    {
                        var academicLevels = new List<StudentAcademicPeriodsAcademicLevels>();
                        var academicStatuses = new List<StudentAcademicPeriodsAcademicStatuses>();
                        var studentLoads = new List<StudentAcademicPeriodsAcademicLoads>();

                        foreach (var studentTerm in studentAcademicPeriod.StudentTerms)
                        {
                            var acadLevel = string.Empty;

                            acadLevel = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(studentTerm.AcademicLevel);
                            if (string.IsNullOrEmpty(acadLevel))
                            {
                                IntegrationApiExceptionAddError(string.Format("No guid found for academic level id '{0}'.", studentTerm.AcademicLevel),
                                 "Validation.Exception", studentAcademicPeriod.Guid);
                            }

                            var studentAcademicPeriodsAcademicLevel = new StudentAcademicPeriodsAcademicLevels();
                            studentAcademicPeriodsAcademicLevel.AcademicLevel = new GuidObject2(acadLevel);
                            academicLevels.Add(studentAcademicPeriodsAcademicLevel);

                            if (studentTerm.StudentTermStatuses != null && studentTerm.StudentTermStatuses.Any())
                            {
                                var studentTermStatus = studentTerm.StudentTermStatuses.FirstOrDefault();
                                var studentAcademicPeriodsAcademicStatus = new StudentAcademicPeriodsAcademicStatuses();
                                studentAcademicPeriodsAcademicStatus.AcademicLevel = new GuidObject2(acadLevel);

                                var status = await _studentReferenceDataRepository.GetStudentStatusesGuidAsync(studentTermStatus.Status);
                                if (status == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("No guid found for status id '{0}'.", studentTermStatus.Status),
                                        "Validation.Exception", studentAcademicPeriod.Guid);
                                }
                                studentAcademicPeriodsAcademicStatus.AcademicPeriodStatus = new GuidObject2(status);
                                studentAcademicPeriodsAcademicStatus.Basis = StudentAcademicPeriodsBasis.ByLevel;
                                academicStatuses.Add(studentAcademicPeriodsAcademicStatus);
                            }

                            if (!string.IsNullOrEmpty(studentTerm.StudentLoad))
                            {
                                var allStudentLoads = (await GetStudentLoadsAsync()).ToList();
                                if (allStudentLoads.Any())
                                {
                                    var studentLoad = allStudentLoads.FirstOrDefault(sl => sl.Code == studentTerm.StudentLoad);
                                    if (studentLoad == null)
                                    {
                                        IntegrationApiExceptionAddError(string.Format("No guid found for studentLoad id '{0}'.", studentTerm.StudentLoad),
                                       "Validation.Exception", studentAcademicPeriod.Guid);

                                    }
                                    if ((!string.IsNullOrEmpty(studentLoad.Sp1)) &&
                                        (new[] { "1", "2", "3" }.Any(c => studentLoad.Sp1.Contains(c))))
                                    {

                                        var studentAcadPeriodLoad = new StudentAcademicPeriodsAcademicLoads();

                                        studentAcadPeriodLoad.AcademicLevel = new GuidObject2(acadLevel);
                                        studentAcadPeriodLoad.Basis = StudentAcademicPeriodsBasis.ByLevel;

                                        switch (studentLoad.Sp1)
                                        {
                                            case "1":
                                                studentAcadPeriodLoad.AcademicLoad = AcademicLoad2.PartTime;
                                                break;
                                            case "2":
                                                studentAcadPeriodLoad.AcademicLoad = AcademicLoad2.FullTime;
                                                break;
                                            case "3":
                                                studentAcadPeriodLoad.AcademicLoad = AcademicLoad2.OverLoad;
                                                break;
                                            default:
                                                studentAcadPeriodLoad.AcademicLoad = AcademicLoad2.NotSet;
                                                break;
                                        }

                                        if (studentAcadPeriodLoad.AcademicLoad != AcademicLoad2.NotSet)
                                            studentLoads.Add(studentAcadPeriodLoad);
                                    }
                                }
                            }
                        }

                        if (studentLoads.Any())
                        {
                            studentAcadPeriodProfileDto.AcademicLoads = studentLoads;
                        }
                        if (academicLevels.Any())
                        {
                            studentAcadPeriodProfileDto.AcademicLevels = academicLevels;
                        }
                        if (academicStatuses.Any())
                        {
                            studentAcadPeriodProfileDto.AcademicStatuses = academicStatuses;
                        }
                    }
                    studentAcadPeriodProfileDtos.Add(studentAcadPeriodProfileDto);
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
                throw IntegrationApiException;
            }

            return studentAcadPeriodProfileDtos;

        }
    }
}