//Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
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
    public class StudentCohortAssignmentsService : BaseCoordinationService, IStudentCohortAssignmentsService
    {
        private readonly IStudentCohortAssignmentsRepository _studentCohortAssignmentsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private Dictionary<string, string> personGuidDictionary = new Dictionary<string, string>();

        public StudentCohortAssignmentsService(
            IStudentCohortAssignmentsRepository studentCohortAssignmentsRepository,
            IPersonRepository personRepository,
            IStudentReferenceDataRepository studentRreferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _studentCohortAssignmentsRepository = studentCohortAssignmentsRepository;
            _personRepository = personRepository;
            _studentReferenceDataRepository = studentRreferenceDataRepository;
        }

        #region Reference Data
        /// <summary>
        /// Student Cohorts
        /// </summary>
        private IEnumerable<Domain.Student.Entities.StudentCohort> _studentCohort = null;
        private async Task<IEnumerable<Domain.Student.Entities.StudentCohort>> StudentCohortsAsync(bool bypassCache = false)
        {
            return _studentCohort ?? await _studentReferenceDataRepository.GetAllStudentCohortAsync(bypassCache);
        }

        #endregion 

        #region All GET Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-cohort-assignments
        /// </summary>
        /// <returns>Collection of StudentCohortAssignments DTO objects</returns>
        public async Task<Tuple<IEnumerable<StudentCohortAssignments>, int>> GetStudentCohortAssignmentsAsync(int offset, int limit, 
            StudentCohortAssignments criteria = null, Dictionary<string, string> filterQualifiers = null, bool bypassCache = false)
        {
          
            StudentCohortAssignment criteriaObj = null;
            if(criteria != null)
            {
                criteriaObj = new StudentCohortAssignment();
                if (criteria.Person != null && !string.IsNullOrWhiteSpace(criteria.Person.Id))
                {
                    var personId = await _personRepository.GetPersonIdFromGuidAsync(criteria.Person.Id);
                    if (string.IsNullOrWhiteSpace(personId))
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignments>, int>(new List<StudentCohortAssignments>(), 0);
                    }
                    criteriaObj.PersonId = personId;
                }

                if(criteria.Cohort != null && !string.IsNullOrWhiteSpace(criteria.Cohort.Id))
                {
                    var cohorts = await StudentCohortsAsync(bypassCache);
                    if(cohorts == null)
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignments>, int>(new List<StudentCohortAssignments>(), 0);
                    }
                    var cohort = cohorts.FirstOrDefault(c => c.Guid.Equals(criteria.Cohort.Id, StringComparison.OrdinalIgnoreCase));
                    if (cohort == null)
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignments>, int>(new List<StudentCohortAssignments>(), 0);
                    }
                    criteriaObj.CohortId = cohort.Code;
                    criteriaObj.CohortType = cohort.CohortType;
                }

                if (criteria.StartOn.HasValue)
                {
                    criteriaObj.StartOn = criteria.StartOn.Value;
                }

                if (criteria.EndOn.HasValue)
                {
                    criteriaObj.EndOn = criteria.EndOn.Value;
                }
            }
            var studentCohortAssignmentsCollection = new List<Ellucian.Colleague.Dtos.StudentCohortAssignments>();


            var studentCohortAssignmentsEntities = 
                await _studentCohortAssignmentsRepository.GetStudentCohortAssignmentsAsync(offset, limit, criteriaObj, filterQualifiers);
            var totalCount = 0;

            if (studentCohortAssignmentsEntities != null && studentCohortAssignmentsEntities.Item1.Any())
            {
                totalCount = studentCohortAssignmentsEntities.Item2;
                var personIds = studentCohortAssignmentsEntities.Item1.Select(p => p.PersonId).Distinct();
                personGuidDictionary = await _personRepository.GetPersonGuidsCollectionAsync(personIds);

                foreach (var entity in studentCohortAssignmentsEntities.Item1)
                {
                    studentCohortAssignmentsCollection.Add(await ConvertStudentCohortAssignmentsEntityToDtoAsync(entity, personGuidDictionary, bypassCache));
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return studentCohortAssignmentsCollection.Any()? new Tuple<IEnumerable<StudentCohortAssignments>, int>(studentCohortAssignmentsCollection, totalCount) :
                new Tuple<IEnumerable<StudentCohortAssignments>, int>(new List<StudentCohortAssignments>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentCohortAssignments from its GUID
        /// </summary>
        /// <returns>StudentCohortAssignments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentCohortAssignments> GetStudentCohortAssignmentsByGuidAsync(string guid, bool bypassCache = true)
        {
           
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid");
                }
                StudentCohortAssignment entity = await _studentCohortAssignmentsRepository.GetStudentCohortAssignmentByIdAsync(guid);
                if (entity == null)
                {
                    throw new KeyNotFoundException(string.Format("No student-cohort-assignments was found for guid '{0}'", guid));
                }
                personGuidDictionary = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { entity.PersonId });

                var dto = await ConvertStudentCohortAssignmentsEntityToDtoAsync(entity, personGuidDictionary, bypassCache);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dto;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No student-cohort-assignments was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No student-cohort-assignments was found for guid '{0}'", guid), ex);
            }
        }

        #endregion All GET Methods

        #region All Convert Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentAcadLevels domain entity to its corresponding StudentCohortAssignments DTO
        /// </summary>
        /// <param name="source">StudentAcadLevels domain entity</param>
        /// <returns>StudentCohortAssignments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentCohortAssignments> ConvertStudentCohortAssignmentsEntityToDtoAsync(StudentCohortAssignment source, 
            Dictionary<string, string> personGuids,  bool bypassCache = false)
        {
            var dto = new Ellucian.Colleague.Dtos.StudentCohortAssignments();

            dto.Id = source.RecordGuid;
            var personGuid = string.Empty;
            if(personGuids != null && personGuids.Any() && !personGuids.TryGetValue(source.PersonId, out personGuid))
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID '{0}'", source.PersonId), "Bad.Data", source.RecordGuid, source.RecordKey);
            }
            dto.Person = new GuidObject2(personGuid);

            try
            {
                var cohortGuid = await _studentReferenceDataRepository.GetStudentCohortGuidAsync(source.CohortId);
                if (cohortGuid == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for cohort ID '{0}'", source.CohortId), "Bad.Data", source.RecordGuid, source.RecordKey);
                }
                dto.Cohort = new GuidObject2(cohortGuid);
            }
            catch (RepositoryException)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for cohort ID '{0}'", source.CohortId), "Bad.Data", source.RecordGuid, source.RecordKey);
            }

            try
            {
                var levelGuid = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(source.AcadLevel);
                if (string.IsNullOrEmpty(levelGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic level ID '{0}'", source.AcadLevel), "Bad.Data", source.RecordGuid, source.RecordKey);
                }
                dto.AcademicLevel = new GuidObject2(levelGuid);
            }
            catch (RepositoryException)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic level ID '{0}'", source.AcadLevel), "Bad.Data", source.RecordGuid, source.RecordKey);
            }
            dto.StartOn = source.StartOn.HasValue ? source.StartOn.Value : default(DateTime?);
            dto.EndOn = source.EndOn.HasValue ? source.EndOn.Value : default(DateTime?);

            return dto;
        }

        #endregion All Convert Methods
    
    }
}