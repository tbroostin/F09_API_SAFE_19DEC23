//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAdvisorRelationshipsService : StudentCoordinationService, IStudentAdvisorRelationshipsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IStudentAdvisorRelationshipsRepository _studentAdvisorRelationshipsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IAdvisorTypesService _advisorTypesService;
        private readonly IConfigurationRepository _configurationRepository;

        private IEnumerable<AdvisorType> _advisorTypes = null;
        private IEnumerable<Domain.Student.Entities.AcademicProgram> _acadPrograms = null;

        public StudentAdvisorRelationshipsService(

            IStudentReferenceDataRepository referenceDataRepository,
            IStudentAdvisorRelationshipsRepository studentAdvisorRelationshipsRepository,
            IPersonRepository personRepository,
            IAdapterRegistry adapterRegistry,
            IAdvisorTypesService advisorTypesService,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IStudentRepository studentRepo,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepo, configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _studentAdvisorRelationshipsRepository = studentAdvisorRelationshipsRepository;
            _personRepository = personRepository;
            _advisorTypesService = advisorTypesService;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-advisor-relationships
        /// </summary>
        /// <returns>Collection of StudentAdvisorRelationships DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>> GetStudentAdvisorRelationshipsAsync(int offset, int limit, bool bypassCache = false,
            string student = "", string advisor = "", string advisorType = "", string startAcademicPeriod = "")
        {
            var studentAdvisorRelationshipsCollection = new List<Dtos.StudentAdvisorRelationships>();

            //Currently not supporting this filter.
            if (!string.IsNullOrEmpty(startAcademicPeriod))
            {
                return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
            }

            string studentId = string.Empty, advisorId = string.Empty, advisorTypeCode = string.Empty;

            if (!string.IsNullOrEmpty(student))
            {
                try
                {
                    studentId = await _personRepository.GetPersonIdFromGuidAsync(student);
                    // if null then send empty set.
                    if (string.IsNullOrWhiteSpace(studentId))
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                }
            }

            if (!string.IsNullOrEmpty(advisor))
            {
                try
                {
                    advisorId = await _personRepository.GetPersonIdFromGuidAsync(advisor);
                    // if null then send empty set.
                    if (string.IsNullOrWhiteSpace(advisorId))
                    {
                        return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                }
            }

            if (!string.IsNullOrEmpty(advisorType))
            {
                try
                {
                    var advisorTypes = await _referenceDataRepository.GetAdvisorTypesAsync(bypassCache);
                    advisorTypeCode = advisorTypes.FirstOrDefault(x => x.Guid == advisorType).Code;
                }
                catch (Exception)
                {
                    //value for advisor Type was not found so send a empty set.
                    return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                }
            }

            var studentAdvisorRelationshipsEntities = await _studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsAsync(offset, limit, bypassCache,
                studentId, advisorId, advisorTypeCode);
            int totalRecords = studentAdvisorRelationshipsEntities.Item2;

            if(studentAdvisorRelationshipsEntities != null && studentAdvisorRelationshipsEntities.Item1 != null && !studentAdvisorRelationshipsEntities.Item1.Any())
            {
                return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
            }

            List<string> personIds = new List<string>();
            var advisorIds = studentAdvisorRelationshipsEntities.Item1.Where(i => !string.IsNullOrWhiteSpace(i.Advisor)).Select(a => a.Advisor);
            var studentIds = studentAdvisorRelationshipsEntities.Item1.Where(i => !string.IsNullOrWhiteSpace(i.Student)).Select(a => a.Student);

            if (advisorIds != null && advisorIds.Any()) personIds.AddRange(advisorIds);
            if (studentIds != null && studentIds.Any()) personIds.AddRange(studentIds);

            foreach (var studentAdvisorRelationships in studentAdvisorRelationshipsEntities.Item1)
            {
                Dtos.StudentAdvisorRelationships studentAdvisorRelationshipsDto = 
                    await ConvertStudentAdvisorRelationshipsEntityToDto(studentAdvisorRelationships, await _personRepository.GetPersonGuidsCollectionAsync(personIds.Distinct().ToList()));
                if (studentAdvisorRelationshipsDto != null)
                    studentAdvisorRelationshipsCollection.Add(studentAdvisorRelationshipsDto);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentAdvisorRelationships from its GUID
        /// </summary>
        /// <returns>StudentAdvisorRelationships DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAdvisorRelationships> GetStudentAdvisorRelationshipsByGuidAsync(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException("guid", "student-advisor-relationships requires a GUID");
            }

            StudentAdvisorRelationships studentAdvisorRelationshipsDto = null;
            try
            {
                var entity = await _studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync(guid);
                List<string> personIds = new List<string>();

                if (!string.IsNullOrWhiteSpace(entity.Advisor)) personIds.Add(entity.Advisor);
                if (!string.IsNullOrWhiteSpace(entity.Student)) personIds.Add(entity.Student);
                
                studentAdvisorRelationshipsDto = await ConvertStudentAdvisorRelationshipsEntityToDto(entity, await _personRepository.GetPersonGuidsCollectionAsync(personIds.Distinct().ToList()));
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (KeyNotFoundException)
            {
                IntegrationApiExceptionAddError("student-advisor-relationships not found for GUID " + guid, "GUID.Not.Found", guid, "", System.Net.HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException)
            {
                IntegrationApiExceptionAddError("student-advisor-relationships not found for GUID " + guid, "GUID.Not.Found", guid, "", System.Net.HttpStatusCode.NotFound);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return studentAdvisorRelationshipsDto;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentAdvisorRelationships domain entity to its corresponding StudentAdvisorRelationships DTO
        /// </summary>
        /// <param name="source">StudentAdvisorRelationships domain entity</param>
        /// <returns>StudentAdvisorRelationships DTO</returns>
        private async Task<Dtos.StudentAdvisorRelationships> ConvertStudentAdvisorRelationshipsEntityToDto(Domain.Student.Entities.StudentAdvisorRelationship source, Dictionary<string, string> personIdsDictionary)
        {
            var studentAdvisorRelationships = new Dtos.StudentAdvisorRelationships();

            //validate that the source data coming in contains the required data needed for the API response
            if (string.IsNullOrWhiteSpace(source.Guid))
            {
                IntegrationApiExceptionAddError("Record does not contain a valid GUID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.Id + "'",
                    "Bad.Data", source.Guid, source.Id);
            }
            studentAdvisorRelationships.Id = source.Guid;

            if (string.IsNullOrWhiteSpace(source.Advisor))
            {
                IntegrationApiExceptionAddError("Record does not contain an advisor ID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.Id + "'",
                    "Bad.Data", source.Guid, source.Id);
            }
            
            if (string.IsNullOrWhiteSpace(source.Student))
            {
                IntegrationApiExceptionAddError("Record does not contain a student ID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.Id + "'",
                    "Bad.Data", source.Guid, source.Id);
            }

            if (source.StartOn == null)
            {
                IntegrationApiExceptionAddError("Record does not contain a start date. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.Id + "'",
                    "Bad.Data", source.Guid, source.Id);
            }

            //Get the advisor GUID
            var advisorGuid = string.Empty;
            if(!string.IsNullOrEmpty(source.Advisor) && personIdsDictionary != null && personIdsDictionary.Any() && personIdsDictionary.TryGetValue(source.Advisor, out advisorGuid))
            {
                studentAdvisorRelationships.Advisor = new GuidObject2(advisorGuid);
            }
            else
            {
                IntegrationApiExceptionAddError("Could not retrive the advisor's GUID. Entity: PERSON, Record ID: '" + source.Advisor + "'",
                    "Validation.Exception", source.Guid, source.Id);
            }

            //get the student GUID
            var studentGuid = string.Empty;
            if (!string.IsNullOrEmpty(source.Student) && personIdsDictionary != null && personIdsDictionary.Any() && personIdsDictionary.TryGetValue(source.Student, out studentGuid))
            {
                studentAdvisorRelationships.Student = new GuidObject2(studentGuid);
            }
            else
            {
                IntegrationApiExceptionAddError("Could not retrive the student's GUID. Entity: PERSON, Record ID: '" + source.Student + "'",
                    "Validation.Exception", source.Guid, source.Id);
            }

            //Get the AdvisorType GUID
            if (!string.IsNullOrWhiteSpace(source.AdvisorType))
            {
                try
                {
                    var advisorTypeGuid = await _referenceDataRepository.GetAdvisorTypeGuidAsync(source.AdvisorType);
                    if (string.IsNullOrEmpty(advisorTypeGuid))
                    {
                        IntegrationApiExceptionAddError("The field STAD.TYPE could not be matched with an advisor-types resource or the GUID is missing. Entity: ADVISOR.TYPES, Record ID: '" + source.AdvisorType + "'",
                            "Validation.Exception", source.Guid, source.Id);
                    }
                    studentAdvisorRelationships.AdvisorType = new GuidObject2(advisorTypeGuid);
                }
                catch(RepositoryException)
                {
                    IntegrationApiExceptionAddError("The field STAD.TYPE could not be matched with an advisor-types resource or the GUID is missing. Entity: ADVISOR.TYPES, Record ID: '" + source.AdvisorType + "'",
                        "Validation.Exception", source.Guid, source.Id);
                }
            }

            //Get the programs GUID
            if (!string.IsNullOrEmpty(source.Program))
            {
                try
                {
                    var programGuid = await _referenceDataRepository.GetAcademicProgramsGuidAsync(source.Program);
                    if (string.IsNullOrEmpty(programGuid))
                    {
                        IntegrationApiExceptionAddError("The field STAD.ACAD.PROGRAM could not be matched with an academic-programs resource or the GUID is missing. Entity: ACAD.PROGRAMS, Record ID: '" + source.Program + "'",
                            "Validation.Exception", source.Guid, source.Id);
                    }
                    studentAdvisorRelationships.Program = new GuidObject2(programGuid);
                }
                catch(RepositoryException)
                {
                    IntegrationApiExceptionAddError("The field STAD.ACAD.PROGRAM could not be matched with an academic-programs resource or the GUID is missing. Entity: ACAD.PROGRAMS, Record ID: '" + source.Program + "'",
                        "Validation.Exception", source.Guid, source.Id);
                }
            }

            studentAdvisorRelationships.StartOn = source.StartOn;
            studentAdvisorRelationships.EndOn = source.EndOn;

            return studentAdvisorRelationships;
        }

        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> GetAcadPrograms(bool bypassCache = false)
        {
            if (_acadPrograms == null)
            {
                _acadPrograms = await _referenceDataRepository.GetAcademicProgramsAsync(bypassCache);
            }
            return _acadPrograms;
        }

        private async Task<IEnumerable<AdvisorType>> GetAdvisorTypes(bool bypassCache = false)
        {
            if (_advisorTypes == null)
            {
                _advisorTypes = await _referenceDataRepository.GetAdvisorTypesAsync(bypassCache);
            }
            return _advisorTypes;
        }
    }
}