//Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.Filters;
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
    public class StudentAcademicCredentialsService : BaseCoordinationService, IStudentAcademicCredentialsService
    {
        private readonly IStudentAcademicCredentialsRepository _studentAcademicCredentialsRepository;
        private readonly IStudentAcademicProgramRepository _studentAcademicProgramRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository _configurationRepository;


        /// <summary>
        /// ...ctor
        /// </summary>
        /// <param name="studentAcademicCredentialsRepository"></param>
        /// <param name="studentAcademicProgramRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="studentReferenceDataRepository"></param>
        /// <param name="termRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="logger"></param>
        public StudentAcademicCredentialsService(
            IStudentAcademicCredentialsRepository studentAcademicCredentialsRepository,
            IStudentAcademicProgramRepository studentAcademicProgramRepository,
            IPersonRepository personRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            ITermRepository termRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger) :
            base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _studentAcademicCredentialsRepository = studentAcademicCredentialsRepository;
            _studentAcademicProgramRepository = studentAcademicProgramRepository;
            _personRepository = personRepository;
            _termRepository = termRepository;
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

      

        #region GET, GET By ID
        /// <summary>
        /// Gets all student academic credentials.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteria"></param>
        /// <param name="personFilterFilter"></param>
        /// <param name="academicProgramFilter"></param>
        /// <param name="filterQualifiers"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentAcademicCredentials>, int>> GetStudentAcademicCredentialsAsync(int offset, int limit,
               StudentAcademicCredentials criteria = null, PersonFilterFilter2 personFilterFilter = null, AcademicProgramsFilter academicProgramFilter = null,
               Dictionary<string, string> filterQualifiers = null, bool bypassCache = false)
        {

        
            #region Filters

            Domain.Student.Entities.StudentAcademicCredential criteriaEntity = new Domain.Student.Entities.StudentAcademicCredential();

            List<AcadCredential> acadCreds = new List<AcadCredential>();

            //credentials
            if (criteria != null && criteria.Credentials != null && criteria.Credentials.Any())
            {
                var creds = (await GetAcadCredentialsAsync(bypassCache));
                if (creds == null || !creds.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }

                //Checks every guid is for credential.
                foreach (var credential in criteria.Credentials)
                {
                    var cred = creds.FirstOrDefault(i => i.Guid.Equals(credential.Credential.Id, StringComparison.OrdinalIgnoreCase));
                    if (cred == null)
                    {
                        return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                    }
                    var x = cred.AcademicCredentialType;
                    acadCreds.Add(cred);
                }

                var degreeCred = acadCreds.Where(c => c != null && c.AcademicCredentialType == Domain.Base.Entities.AcademicCredentialType.Degree).ToList();
                var certCred = acadCreds.Where(c => c != null && c.AcademicCredentialType == Domain.Base.Entities.AcademicCredentialType.Certificate).ToList();

                if (degreeCred != null && degreeCred.Any())
                {
                    List<Tuple<string, DateTime?>> degreeTuple = new List<Tuple<string, DateTime?>>();
                    var credCodes = degreeCred.Select(d => d.Code).ToList();
                    foreach (var code in credCodes)
                    {
                        Tuple<string, DateTime?> tuple = new Tuple<string, DateTime?>(code, default(DateTime?));
                        degreeTuple.Add(tuple);
                    }
                    criteriaEntity.Degrees = degreeTuple;
                }

                if (certCred != null && certCred.Any())
                {
                    List<Tuple<string, DateTime?>> ccdTuple = new List<Tuple<string, DateTime?>>();
                    var certCodes = certCred.Select(d => d.Code).ToList();
                    foreach (var code in certCodes)
                    {
                        Tuple<string, DateTime?> tuple = new Tuple<string, DateTime?>(code, default(DateTime?));
                        ccdTuple.Add(tuple);
                    }
                    criteriaEntity.Ccds = ccdTuple;
                }
            }

            //graduatedOn
            if (criteria != null && criteria.GraduatedOn.HasValue)
            {
                criteriaEntity.GraduatedOn = criteria.GraduatedOn.Value;
            }
            //student
            if (criteria != null && criteria.Student != null && !string.IsNullOrWhiteSpace(criteria.Student.Id))
            {
                try
                {
                    var studentKey = await _personRepository.GetPersonIdFromGuidAsync(criteria.Student.Id);
                    if (string.IsNullOrEmpty(studentKey))
                    {
                        return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                    }
                    else
                    {
                        criteriaEntity.StudentId = studentKey;
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }
            }

            //academicLevel
            if (criteria != null && criteria.AcademicLevel != null && !string.IsNullOrWhiteSpace(criteria.AcademicLevel.Id))
            {
                var acadLevels = await GetAcademicLevelsAsync(bypassCache);
                if (acadLevels == null || !acadLevels.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }
                var level = acadLevels.FirstOrDefault(l => l.Guid.Equals(criteria.AcademicLevel.Id, StringComparison.OrdinalIgnoreCase));
                if (level == null)
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }
                criteriaEntity.AcademicLevel = level.Code;
            }

            //studentProgram
            if (criteria != null && criteria.StudentProgram != null && !string.IsNullOrWhiteSpace(criteria.StudentProgram.Id))
            {
                try
                {
                    /*WITH ACAD.PERSON.ID = the student id AND ACAD.ACAD.PROGRAM = the program*/
                    var studentProgramKey = await _studentAcademicProgramRepository.GetStudentAcademicProgramIdFromGuidAsync(criteria.StudentProgram.Id);
                    if (string.IsNullOrEmpty(studentProgramKey))
                    {
                        return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                    }
                    string[] keys = studentProgramKey.Split(new[] { "*" }, StringSplitOptions.None);
                    /*
                        looks like student filter & first item in the key array for program points to same property in entity ACAD.PERSON.ID 
                    */
                    if (!string.IsNullOrEmpty(criteriaEntity.StudentId) && !string.IsNullOrEmpty(keys[0]) &&
                       !criteriaEntity.StudentId.Equals(keys[0], StringComparison.OrdinalIgnoreCase))
                    {
                        return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                    }
                    criteriaEntity.StudentId = keys[0];
                    criteriaEntity.AcadAcadProgramId = keys[1];
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }
            }

            //graduationAcademicPeriod
            if (criteria != null && criteria.GraduationAcademicPeriod != null && !string.IsNullOrWhiteSpace(criteria.GraduationAcademicPeriod.Id))
            {
                var acadPeriods = await GetAcademicPeriods(bypassCache);
                if (acadPeriods == null || !acadPeriods.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }
                var acadPeriod = acadPeriods.FirstOrDefault(p => p.Guid.Equals(criteria.GraduationAcademicPeriod.Id));
                if (acadPeriod == null)
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }
                criteriaEntity.AcademicPeriod = acadPeriod.Code;
            }

            #endregion Filters

            #region Named queries

            //personFilter
            string[] filterPersonIds = new string[] { };
            if (personFilterFilter != null && personFilterFilter.personFilter != null && !string.IsNullOrWhiteSpace(personFilterFilter.personFilter.Id))
            {
                if (!string.IsNullOrWhiteSpace(personFilterFilter.personFilter.Id))
                {
                    try
                    {
                        var personFilterKeys = await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilterFilter.personFilter.Id);
                        if (personFilterKeys != null)
                        {
                            filterPersonIds = personFilterKeys;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                        }
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                    }
                }
            }
            //academicPrograms
            string acadProgramFilter = null;
            if (academicProgramFilter != null && academicProgramFilter.AcademicPrograms != null && !string.IsNullOrEmpty(academicProgramFilter.AcademicPrograms.Id))
            {
                var acadPrograms = await GetAcademicProgramsAsync(bypassCache);
                if (acadPrograms == null || !acadPrograms.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }

                var acadProgram = acadPrograms.FirstOrDefault(ap => ap.Guid.Equals(academicProgramFilter.AcademicPrograms.Id, StringComparison.OrdinalIgnoreCase));
                if (acadProgram == null)
                {
                    return new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
                }
                acadProgramFilter = acadProgram.Code;
            }
            #endregion

            var entities = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialsAsync(offset, limit,
                criteriaEntity, filterPersonIds, acadProgramFilter, filterQualifiers);

            var studentIds = entities.Item1.Where(i => !string.IsNullOrEmpty(i.AcadPersonId)).Select(s => s.AcadPersonId).ToList();
            Dictionary<string, string> personDict = new Dictionary<string, string>();

            personDict = await _personRepository.GetPersonGuidsCollectionAsync(studentIds);

            List<StudentAcademicCredentials> dtos = new List<StudentAcademicCredentials>();

            if (entities != null && entities.Item1.Any())
            {
                foreach (var entity in entities.Item1)
                {
                    var dto = await ConvertEntityToDtoAsync(entity, personDict, bypassCache);
                    dtos.Add(dto);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return dtos.Any() ? new Tuple<IEnumerable<StudentAcademicCredentials>, int>(dtos, entities.Item2) :
                new Tuple<IEnumerable<StudentAcademicCredentials>, int>(new List<StudentAcademicCredentials>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentAcademicCredentials from its GUID
        /// </summary>
        /// <returns>StudentAcademicCredentials DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicCredentials> GetStudentAcademicCredentialsByGuidAsync(string guid, bool bypassCache = true)
        {
        
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required.");
            }

            var acadCredTuple = await _studentAcademicCredentialsRepository.GetAcadCredentialKeyAsync(guid);
            if (acadCredTuple == null || !acadCredTuple.Item1.Equals("ACAD.CREDENTIALS", StringComparison.OrdinalIgnoreCase))
            {
                IntegrationApiExceptionAddError(string.Format("Academic credentials not found for guid '{0}'.", guid));
                throw IntegrationApiException;
            }

            if (acadCredTuple == null || string.IsNullOrEmpty(acadCredTuple.Item2) || string.IsNullOrEmpty(acadCredTuple.Item3))
            {
                IntegrationApiExceptionAddError(string.Format("Academic credentials not found for guid '{0}'.", guid));
                throw IntegrationApiException;
            }

            Ellucian.Colleague.Dtos.StudentAcademicCredentials dto = new StudentAcademicCredentials();

            var entity = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync(acadCredTuple.Item2);
            if(entity == null)
            {
                IntegrationApiExceptionAddError(string.Format("Academic credentials not found for guid '{0}'.", guid));
                throw IntegrationApiException;
            }

            Dictionary<string, string> personDict = new Dictionary<string, string>();
            personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { entity.AcadPersonId });
            if (personDict == null || !personDict.Any())
            {
                IntegrationApiExceptionAddError(string.Format("Person guid not found, PersonId: '{0}', Record ID:'{1}", entity.AcadPersonId, entity.RecordKey),
                                        "student.id", entity.RecordGuid, entity.RecordKey);
                throw IntegrationApiException;
            }


            List<StudentAcademicCredentials> dtos = new List<StudentAcademicCredentials>();

            dto = await ConvertEntityToDtoAsync(entity, personDict, bypassCache);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return dto;
        }

        #endregion

        #region Helper Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AcadCredentials domain entity to its corresponding StudentAcademicCredentials DTO
        /// </summary>
        /// <param name="source">AcadCredentials domain entity</param>
        /// <returns>StudentAcademicCredentials DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.StudentAcademicCredentials> ConvertEntityToDtoAsync(StudentAcademicCredential source, 
            Dictionary<string, string> personDict, bool bypassCache = false)
        {
            var dto = new Ellucian.Colleague.Dtos.StudentAcademicCredentials();

            //id
            if (string.IsNullOrEmpty(source.RecordGuid))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for student-acadeimic-credentials entity.",
                    "student-acadeimic-credentials.id", id: source.RecordKey);
            }
            else
            {
                dto.Id = source.RecordGuid;
            }

            //student.id
            var studentGuid = string.Empty;
            if (personDict != null && !personDict.TryGetValue(source.AcadPersonId, out studentGuid) || string.IsNullOrEmpty(source.AcadPersonId))
            {
                IntegrationApiExceptionAddError(string.Format("Person guid not found, PersonId: '{0}', Record ID:'{1}", source.AcadPersonId, source.RecordKey),
                                        "student.id", source.RecordGuid, source.RecordKey);
            }
            else
            {
                dto.Student = new GuidObject2(studentGuid);
            }

            //studentProgram.id
            if (!string.IsNullOrEmpty(source.StudentProgramGuid))
            {
                dto.StudentProgram = new GuidObject2(source.StudentProgramGuid);
            }

            //academicLevel.id
            if (!string.IsNullOrEmpty(source.AcademicLevel))
            {
                string acadLevelGuid = string.Empty;
                try
                {
                    acadLevelGuid = await _studentReferenceDataRepository.GetAcademicLevelsGuidAsync(source.AcademicLevel);
                    if (string.IsNullOrEmpty(acadLevelGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic level guid not found for code '{0}', Record ID:'{1}", source.AcademicLevel, source.RecordKey),
                        "academicLevel.id", source.RecordGuid, source.RecordKey);
                    }
                    dto.AcademicLevel = new GuidObject2(acadLevelGuid);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Academic level guid not found for code '{0}', Record ID:'{1}", source.AcademicLevel, source.RecordKey),
                                            "academicLevel.id", source.RecordGuid, source.RecordKey);
                }
            }

            //credentials
            //Degrees
            List<StudentAcademicCredentialsCredentials> credentials = new List<StudentAcademicCredentialsCredentials>();
            if (source.Degrees != null && source.Degrees.Any())
            {
                foreach (var degree in source.Degrees)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(degree.Item1))
                        {
                            string credGuid = await _referenceDataRepository.GetAcadCredentialsGuidAsync(degree.Item1);
                            if (string.IsNullOrEmpty(credGuid))
                            {
                                IntegrationApiExceptionAddError(string.Format("Academic credential guid not found for code '{0}', Record ID:'{1}", degree.Item1, source.RecordKey),
                                                        "credentials.credential.id", source.RecordGuid, source.RecordKey);
                            }
                            else
                            {

                                StudentAcademicCredentialsCredentials cred = new StudentAcademicCredentialsCredentials()
                                {
                                    Credential = new GuidObject2(credGuid),
                                    EarnedOn = degree.Item2 ?? default(DateTime?)
                                };
                                credentials.Add(cred);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic credential guid not found for code '{0}', Record ID:'{1}", degree.Item1, source.RecordKey),
                        "credentials.credential.id", source.RecordGuid, source.RecordKey);
                    }
                }
            }

            //Ccds
            if (source.Ccds != null && source.Ccds.Any())
            {
                foreach (var ccd in source.Ccds)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ccd.Item1))
                        {
                            string credGuid = await _referenceDataRepository.GetAcadCredentialsGuidAsync(ccd.Item1);
                            if (string.IsNullOrEmpty(credGuid))
                            {
                                IntegrationApiExceptionAddError(string.Format("Academic credential guid not found for code '{0}', Record ID:'{1}", ccd.Item1, source.RecordKey),
                                                        "credentials.credential.id", source.RecordGuid, source.RecordKey);
                            }
                            else
                            {

                                StudentAcademicCredentialsCredentials cred = new StudentAcademicCredentialsCredentials()
                                {
                                    Credential = new GuidObject2(credGuid),
                                    EarnedOn = ccd.Item2 ?? default(DateTime)
                                };
                                credentials.Add(cred);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic credential guid not found for code '{0}', Record ID:'{1}", ccd.Item1, source.RecordKey),
                                            "credentials.credential.id", source.RecordGuid, source.RecordKey);
                    }
                }
            }
            if (credentials.Any())
            {
                dto.Credentials = credentials;
            }        

            //disciplines Majors
            List<GuidObject2> disciplines = new List<GuidObject2>();
            
            if (source.AcadMajors != null && source.AcadMajors.Any())
            {
                var majors = await GetAcademicMajors(source, bypassCache);
                if ((majors != null) && (majors.Any()))
                {
                    foreach (var acadMajor in source.AcadMajors)
                    {

                        var major = majors.FirstOrDefault(x => x.Code.Equals(acadMajor, StringComparison.OrdinalIgnoreCase));

                        if ((major == null) || (string.IsNullOrEmpty(major.Guid)))
                        {
                            IntegrationApiExceptionAddError(string.Format("Academic discipline guid not found for major code '{0}', Record ID:'{1}", acadMajor, source.RecordKey),
                                                "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                        }
                        else
                        {
                            disciplines.Add(new GuidObject2(major.Guid));
                        }
                    }
                }
            }

            if (source.AcadMinors != null && source.AcadMinors.Any())
            {
                var minors = await GetAcademicMinors(source, bypassCache);
                if ((minors != null) && (minors.Any()))
                {
                    foreach (var acadMinor in source.AcadMinors)
                    {
                        var minor = minors.FirstOrDefault(x => x.Code.Equals(acadMinor, StringComparison.OrdinalIgnoreCase));

                        if ((minor == null) || (string.IsNullOrEmpty(minor.Guid)))
                        {
                            IntegrationApiExceptionAddError(string.Format("Academic discipline guid not found for minor code '{0}', Record ID:'{1}", acadMinor, source.RecordKey),
                                                "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                        }
                        else
                        {
                            disciplines.Add(new GuidObject2(minor.Guid));
                        }
                    }
                }
            }

            if (source.AcadSpecializations != null && source.AcadSpecializations.Any())
            {
                var specializations = await GetAcademicSpecializations(source, bypassCache);
                if ((specializations != null) && (specializations.Any()))
                {
                    foreach (var acadSpecialization in source.AcadSpecializations)
                    {

                        var specialization = specializations.FirstOrDefault(x => x.Code.Equals(acadSpecialization, StringComparison.OrdinalIgnoreCase));

                        if ((specialization == null) || (string.IsNullOrEmpty(specialization.Guid)))
                        {
                            IntegrationApiExceptionAddError(string.Format("Academic discipline guid not found for specialization code '{0}', Record ID:'{1}", acadSpecialization, source.RecordKey),
                                                "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                        }
                        else
                        {
                            disciplines.Add(new GuidObject2(specialization.Guid));
                        }
                    }
                }
            }

            if (disciplines.Any())
            {
                dto.Disciplines = disciplines;
            }

            //recognitions
            List<GuidObject2> recognitions = new List<GuidObject2>();
            if (source.AcadHonors != null && source.AcadHonors.Any())
            {
                foreach (var acadHonor in source.AcadHonors)
                {
                    try
                    {
                        var guid = await _referenceDataRepository.GetOtherHonorsGuidAsync(acadHonor);
                        if (string.IsNullOrEmpty(guid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Academic honor guid not found for code '{0}', Record ID:'{1}", acadHonor, source.RecordKey),
                                                "recognitions.id", source.RecordGuid, source.RecordKey);
                        }
                        else
                        {
                            GuidObject2 guidObject2 = new GuidObject2(guid);
                            recognitions.Add(guidObject2);
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic honor guid not found for code '{0}', Record ID:'{1}", acadHonor, source.RecordKey),
                                            "recognitions.id", source.RecordGuid, source.RecordKey);
                    }
                }
            }
            if (recognitions.Any())
            {
                dto.Recognitions = recognitions;
            }

            //graduatedOn
            if (source.GraduatedOn.HasValue)
            {
                dto.GraduatedOn = source.GraduatedOn.Value;
                dto.GraduationYear = source.GraduatedOn.Value.Year.ToString();
            }


            //thesisTitle
            dto.ThesisTitle = string.IsNullOrEmpty(source.AcadThesis) ? null : source.AcadThesis;

            //graduationAcademicPeriod.id
            if (!string.IsNullOrEmpty(source.AcadTerm))
            {
                //GetAcademicPeriods
                try
                {
                    var acadPeriodGuid = await _termRepository.GetAcademicPeriodsGuidAsync(source.AcadTerm);
                    if (string.IsNullOrEmpty(acadPeriodGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Graduation academic period guid not found for code '{0}', Record ID:'{1}", source.AcadTerm, source.RecordKey),
                                            "graduationAcademicPeriod.id", source.RecordGuid, source.RecordKey);
                    }
                    else
                    {
                        dto.GraduationAcademicPeriod = new GuidObject2(acadPeriodGuid);
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Graduation academic period guid not found for code '{0}', Record ID:'{1}", source.AcadTerm, source.RecordKey),
                                        "graduationAcademicPeriod.id", source.RecordGuid, source.RecordKey);
                }

            }

            return dto;
        }

        private async Task<IEnumerable<Domain.Base.Entities.AcademicDiscipline>> GetAcademicMajors(StudentAcademicCredential source, bool bypassCache = false)
        {
            var acadDisciplines = await this.GetAcademicDisciplinesAsync(bypassCache);
            if (acadDisciplines == null)
            {
                IntegrationApiExceptionAddError("Academic major records not found.", "GUID.Not.Found", source.RecordGuid, source.RecordKey);

            }
            var majors = acadDisciplines.Where(a => a.AcademicDisciplineType == Domain.Base.Entities.AcademicDisciplineType.Major);
            if ((majors == null) || (!majors.Any()))
            {
                IntegrationApiExceptionAddError("Academic major records not found.", "GUID.Not.Found", source.RecordGuid, source.RecordKey);
            }

            return majors;
        }


        private async Task<IEnumerable<Domain.Base.Entities.AcademicDiscipline>> GetAcademicMinors(StudentAcademicCredential source, bool bypassCache = false)
        {
            var acadDisciplines = await this.GetAcademicDisciplinesAsync(bypassCache);
            if (acadDisciplines == null)
            {
                IntegrationApiExceptionAddError("Academic minor records not found.", "GUID.Not.Found", source.RecordGuid, source.RecordKey);

            }
            var minors = acadDisciplines.Where(a => a.AcademicDisciplineType == Domain.Base.Entities.AcademicDisciplineType.Minor);
            if ((minors == null) || (!minors.Any()))
            {
                IntegrationApiExceptionAddError("Academic minor records not found.", "GUID.Not.Found", source.RecordGuid, source.RecordKey);
            }

            return minors;
        }


        private async Task<IEnumerable<Domain.Base.Entities.AcademicDiscipline>> GetAcademicSpecializations(StudentAcademicCredential source, bool bypassCache = false)
        {
            var acadDisciplines = await this.GetAcademicDisciplinesAsync(bypassCache);
            if (acadDisciplines == null)
            {
                IntegrationApiExceptionAddError("Academic specialization records not found.", "GUID.Not.Found", source.RecordGuid, source.RecordKey);

            }
            var concentrations = acadDisciplines.Where(a => a.AcademicDisciplineType == Domain.Base.Entities.AcademicDisciplineType.Concentration);
            if ((concentrations == null) || (!concentrations.Any()))
            {
                IntegrationApiExceptionAddError("Academic specialization records not found.", "GUID.Not.Found", source.RecordGuid, source.RecordKey);
            }

            return concentrations;
        }
        //Academic credentials
        private IEnumerable<AcadCredential> _acadCredentials;
        private async Task<IEnumerable<AcadCredential>> GetAcadCredentialsAsync(bool bypassCache)
        {
            return _acadCredentials ?? (_acadCredentials = await _referenceDataRepository.GetAcadCredentialsAsync(bypassCache));
        }

        //Academic Levels
        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> GetAcademicLevelsAsync(bool bypassCache)
        {
            return _academicLevels ?? (_academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache));
        }

        //Academic Periods
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> GetAcademicPeriods(bool bypassCache)
        {
            var termEntities = await GetTermsAsync(bypassCache);
            return _academicPeriods ?? (_academicPeriods = _termRepository.GetAcademicPeriods(termEntities));
        }

        //Terms
        private IEnumerable<Domain.Student.Entities.Term> _terms = null;
        private async Task<IEnumerable<Domain.Student.Entities.Term>> GetTermsAsync(bool bypassCache)
        {
            return _terms ?? (_terms = await _termRepository.GetAsync(bypassCache));
        }

        //Academic Programs
        private IEnumerable<Domain.Student.Entities.AcademicProgram> _academicPrograms = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> GetAcademicProgramsAsync(bool bypassCache)
        {
            return _academicPrograms ?? (_academicPrograms = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache));
        }

        /// <summary>
        /// Academic Disciplines
        /// </summary>
        private IEnumerable<Domain.Base.Entities.AcademicDiscipline> _academicDisciplines;
        private async Task<IEnumerable<Domain.Base.Entities.AcademicDiscipline>> GetAcademicDisciplinesAsync(bool bypassCache)
        {
            if (_academicDisciplines == null)
            {
                _academicDisciplines = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache);
            }
            return _academicDisciplines;
        }
        #endregion

      
    }
}