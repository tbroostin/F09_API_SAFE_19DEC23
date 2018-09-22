//Copyright 2017 Ellucian Company L.P. and its affiliates.

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

            CheckGetViewStudentAdvisorRelationshipPermission();

            var studentAdvisorRelationshipsCollection = new List<Dtos.StudentAdvisorRelationships>();

            //Currently not supporting this filter.
            if (!string.IsNullOrEmpty(startAcademicPeriod))
            {
                return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
            }

            string studentId = string.Empty, advisorId = string.Empty, advisorTypeCode = string.Empty;

            if (!string.IsNullOrEmpty(student))
            {
                studentId = await _personRepository.GetPersonIdFromGuidAsync(student); 
                // if null then send empty set.
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                }
            }

            if (!string.IsNullOrEmpty(advisor))
            {
                advisorId = await _personRepository.GetPersonIdFromGuidAsync(advisor);
                // if null then send empty set.
                if (string.IsNullOrWhiteSpace(advisorId))
                {
                    return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                }
            }

            if (!string.IsNullOrEmpty(advisorType))
            {
                var advisorTypes = await _referenceDataRepository.GetAdvisorTypesAsync(false);
                try
                {
                    advisorTypeCode = advisorTypes.FirstOrDefault(x=> x.Guid == advisorType).Code;
                }
                catch (Exception e)
                {
                    //value for advisor Type was not found so send a empty set.
                    return new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 0);
                }
            }

            var studentAdvisorRelationshipsEntities = await _studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsAsync(offset, limit,bypassCache,
                studentId, advisorId, advisorTypeCode);
            int totalRecords = studentAdvisorRelationshipsEntities.Item2;

            foreach (var studentAdvisorRelationships in studentAdvisorRelationshipsEntities.Item1)
            {
                Dtos.StudentAdvisorRelationships studentAdvisorRelationshipsDto = await ConvertStudentAdvisorRelationshipsEntityToDto(studentAdvisorRelationships);
                studentAdvisorRelationshipsCollection.Add(studentAdvisorRelationshipsDto);
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
            CheckGetViewStudentAdvisorRelationshipPermission();
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException("guid", "student-advisor-relationships requires a GUID");
            }
            try
            {
                return await ConvertStudentAdvisorRelationshipsEntityToDto((await _studentAdvisorRelationshipsRepository.GetStudentAdvisorRelationshipsByGuidAsync(guid)));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-advisor-relationships not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-advisor-relationships not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentAdvisorRelationships domain entity to its corresponding StudentAdvisorRelationships DTO
        /// </summary>
        /// <param name="source">StudentAdvisorRelationships domain entity</param>
        /// <returns>StudentAdvisorRelationships DTO</returns>
        private async Task<Dtos.StudentAdvisorRelationships> ConvertStudentAdvisorRelationshipsEntityToDto(Domain.Student.Entities.StudentAdvisorRelationship source)
        {
            var studentAdvisorRelationships = new Dtos.StudentAdvisorRelationships();

            //validate that the source data coming in contains the required data needed for the API response
            if (string.IsNullOrWhiteSpace(source.guid))
            {
                throw new Exception("Record does not contain a valid GUID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.guid + "'");
            }
            studentAdvisorRelationships.Id = source.guid;

            if (string.IsNullOrWhiteSpace(source.advisor))
            {
                throw new Exception("Record does not contain a advisor ID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.guid + "'");
            }
            
            if (string.IsNullOrWhiteSpace(source.student))
            {
                throw new Exception("Record does not contain a student ID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.guid + "'");
            }

            if (source.startOn == null)
            {
                throw new Exception("Record does not contain a start date. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.guid + "'");
            }

            //Get the advisor GUID
            var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.advisor);
            if (!string.IsNullOrEmpty(personGuid))
            {
                studentAdvisorRelationships.Advisor = new GuidObject2(personGuid);
            } else
            {
                throw new Exception("We could not retrive the advisor's GUID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.guid + "'");
            }

            //get the student GUID
            personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.student);
            if (!string.IsNullOrEmpty(personGuid))
            {
                studentAdvisorRelationships.Student = new GuidObject2(personGuid);
            }
            else
            {
                throw new Exception("We could not retrive the student's GUID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.guid + "'");
            }

            //Get the AdvisorType GUID
            if (!string.IsNullOrWhiteSpace(source.advisorType)){
                var advisorTypes = await this.GetAdvisorTypes();
                if (advisorTypes == null)
                {
                    throw new Exception("Advisor types could not be loaded.");
                }
                var advisorTypeGuid = advisorTypes.FirstOrDefault(x => x.Code == source.advisorType);
                if (advisorTypeGuid == null || (advisorTypeGuid != null && string.IsNullOrEmpty(advisorTypeGuid.Guid)))
                {
                    throw new Exception("The record's field STAD.TYPE could not be matched with Advisor types or missing a GUID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.id + "'");
                }
                studentAdvisorRelationships.AdvisorType = new GuidObject2(advisorTypeGuid.Guid);
            }

            //Get the programs GUID
            if (!string.IsNullOrEmpty(source.program))
            {
                var acadPrograms = await this.GetAcadPrograms();
                if (acadPrograms == null)
                {
                    throw new Exception("Acad programs could not be retrieved");
                }
                var programGuid = acadPrograms.FirstOrDefault(x => x.Code == source.program);
                if (programGuid == null)
                {
                    throw new Exception("The record's field STAD.ACAD.PROGRAM could not be matched with Acad programs list or it is missing a GUID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.id + "'");
                }
                if (string.IsNullOrEmpty(programGuid.Guid))
                {
                    throw new Exception("The record's field STAD.ACAD.PROGRAM could not be matched with Acad programs list or it is missing a GUID. Entity: STUDENT.ADVISEMENT, Record ID: '" + source.id + "'");
                }
                studentAdvisorRelationships.Program = new GuidObject2(programGuid.Guid);
            }

            studentAdvisorRelationships.StartOn = source.startOn;
            studentAdvisorRelationships.EndOn = source.endOn;


            return studentAdvisorRelationships;
        }

        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> GetAcadPrograms(bool bypassCache = false)
        {
            if (_acadPrograms == null)
            {
                _acadPrograms = await _referenceDataRepository.GetAcademicProgramsAsync(false);
            }
            return _acadPrograms;
        }

        private async Task<IEnumerable<AdvisorType>> GetAdvisorTypes(bool bypassCache = false)
        {
            if (_advisorTypes == null)
            {
                _advisorTypes = await _referenceDataRepository.GetAdvisorTypesAsync(false);
            }
            return _advisorTypes;
        }

        private void CheckGetViewStudentAdvisorRelationshipPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewStudentAdivsorRelationships);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view student advisor relationships.");
            }
        }


    }
}