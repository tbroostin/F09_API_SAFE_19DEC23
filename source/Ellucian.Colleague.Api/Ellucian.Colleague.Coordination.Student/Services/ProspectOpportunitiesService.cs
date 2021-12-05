// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class ProspectOpportunitiesService : BaseCoordinationService, IProspectOpportunitiesService
    {

        private readonly IProspectOpportunitiesRepository _prospectOpportunitiesRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentAcademicProgramRepository _studentAcademicProgramRepository;
        private readonly ITermRepository _termRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;

        #region ..ctor

        public ProspectOpportunitiesService(
            IProspectOpportunitiesRepository prospectOpportunitiesRepository,
            IPersonRepository personRepository,
            IStudentAcademicProgramRepository studentAcademicProgramRepository,
            ITermRepository termRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _prospectOpportunitiesRepository = prospectOpportunitiesRepository;
            _personRepository = personRepository;
            _studentAcademicProgramRepository = studentAcademicProgramRepository;
            _termRepository = termRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

        #endregion

        #region GET Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all prospect-opportunities
        /// </summary>
        /// <returns>Collection of ProspectOpportunities DTO objects</returns>
        public async Task<Tuple<IEnumerable<ProspectOpportunities>, int>> GetProspectOpportunitiesAsync(int offset, int limit, Dtos.ProspectOpportunities criteria,
             string personFilter, bool bypassCache)
        {
            var prospectOpportunitiesCollection = new List<Ellucian.Colleague.Dtos.ProspectOpportunities>();
            string newProspectId = string.Empty;
            string newEntryAcadPeriod = string.Empty;
            int totalCount = 0;
            ProspectOpportunity prospectOpportunity = new ProspectOpportunity();
            List<ProspectOpportunities> dtos = new List<ProspectOpportunities>();

            try
            {
                // access is ok if the current user has the view, or create, permission
                //if ((!await CheckViewProspectOpportunitiesPermissionAsync()) && (!await CheckUpdateProspectOpportunitiesPermissionAsync()))
                //{
                //    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view prospect-opportunities.");
                //    throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to view prospect-opportunities.");
                //}

                //convert criteria values.
                if (criteria != null)
                {
                    if (criteria.Prospect != null && !string.IsNullOrWhiteSpace(criteria.Prospect.Id))
                    {
                        try
                        {
                            var prospectId = await _personRepository.GetPersonIdFromGuidAsync(criteria.Prospect.Id);
                            prospectOpportunity.ProspectId = prospectId;
                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<ProspectOpportunities>, int>(new List<ProspectOpportunities>(), 0);
                        }
                    }

                    if (criteria.EntryAcademicPeriod != null && !string.IsNullOrWhiteSpace(criteria.EntryAcademicPeriod.Id))
                    {
                        try
                        {
                            var acadperiod = await _termRepository.GetAcademicPeriodsCodeFromGuidAsync(criteria.EntryAcademicPeriod.Id);
                            prospectOpportunity.EntryAcademicPeriod = acadperiod;

                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<ProspectOpportunities>, int>(new List<ProspectOpportunities>(), 0);
                        }
                    }
                }

                //convert person filter named query.
                string[] filterPersonIds = new List<string>().ToArray();

                if (!string.IsNullOrEmpty(personFilter))
                {
                    try
                    {
                        var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                        if (personFilterKeys != null && personFilterKeys.Any())
                        {
                            filterPersonIds = personFilterKeys;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<Dtos.ProspectOpportunities>, int>(new List<Dtos.ProspectOpportunities>(), 0);
                        }
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<Dtos.ProspectOpportunities>, int>(new List<Dtos.ProspectOpportunities>(), 0);
                    }
                }
                Tuple<IEnumerable<ProspectOpportunity>, int> entities = null;
                try
                {
                    entities = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(offset, limit, prospectOpportunity, filterPersonIds);
                    if (entities == null || !entities.Item1.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.ProspectOpportunities>, int>(new List<Dtos.ProspectOpportunities>(), 0);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }

                totalCount = entities.Item2;
                //Build applicant guids dictionary
                var applApplicantids = entities.Item1.Select(a => a.ProspectId).Distinct().ToList();
                Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(applApplicantids);

                //Build student acad program guids dictionary
                var strAcadProgIds = entities.Item1.Select(a => a.StudentAcadProgId).Distinct().ToList();
                Dictionary<string, string> stAcadPrgDict = await _studentAcademicProgramRepository.GetStudentAcademicProgramGuidsCollectionAsync(strAcadProgIds);

                foreach (var entity in entities.Item1)
                {
                    dtos.Add(await ConvertEntityToDtoAsync(entity, personDict, stAcadPrgDict));
                }

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }

            return dtos.Any() ? new Tuple<IEnumerable<ProspectOpportunities>, int>(dtos, totalCount) :
                new Tuple<IEnumerable<ProspectOpportunities>, int>(new List<ProspectOpportunities>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ProspectOpportunities from its GUID
        /// </summary>
        /// <returns>ProspectOpportunities DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ProspectOpportunities> GetProspectOpportunitiesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid");
                }

                // access is ok if the current user has the view, or create, permission
                //if ((!await CheckViewProspectOpportunitiesPermissionAsync()) && (!await CheckUpdateProspectOpportunitiesPermissionAsync()))
                //{
                //    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view prospect-opportunities.");
                //    throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to view prospect-opportunities.");
                //}

                var entity = await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync(guid);

                Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { entity.ProspectId });
                Dictionary<string, string> stAcadPrgDict = await _studentAcademicProgramRepository.GetStudentAcademicProgramGuidsCollectionAsync(new List<string>()
                                                           { entity.StudentAcadProgId });

                var dto = await ConvertEntityToDtoAsync(entity, personDict, stAcadPrgDict);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return dto;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No prospect-opportunities was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No prospect-opportunities was found for guid '{0}'", guid), ex);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ProspectOpportunities from its GUID
        /// </summary>
        /// <returns>ProspectOpportunities DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ProspectOpportunitiesSubmissions> GetProspectOpportunitiesSubmissionsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid is a required argument.");
                }

                // access is ok if the current user has the view, or create, permission
                if ((!await CheckViewProspectOpportunitiesPermissionAsync()) && (!await CheckUpdateProspectOpportunitiesPermissionAsync()))
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view prospect-opportunities.");
                    throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to view prospect-opportunities.");
                }

                Domain.Student.Entities.AdmissionApplication entity = await _prospectOpportunitiesRepository.GetProspectOpportunitiesSubmissionsByGuidAsync(guid);

                Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { entity.ApplicantPersonId });
                Dictionary<string, string> stAcadPrgDict = await _studentAcademicProgramRepository.GetStudentAcademicProgramGuidsCollectionAsync(new List<string>()
                 {
                    string.Concat(entity.ApplicantPersonId, "*", entity.ApplicationAcadProgram)
                });

                ProspectOpportunitiesSubmissions dto = await ConvertProspectOpportunitiesSubmissionsEntityToDtoAsync(entity, personDict, stAcadPrgDict);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return dto ?? null;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No prospect-opportunities was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No prospect-opportunities was found for guid '{0}'", guid), ex);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region PUT/POST submissions

        /// <summary>
        /// Update a ProspectOpportunitiesSubmissions.
        /// </summary>
        /// <param name="ProspectOpportunitiesSubmissions">The <see cref="ProspectOpportunitiesSubmissions">prospectOpportunitiesSubmissions</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="ProspectOpportunities">ProspectOpportunities</see></returns>
        public async Task<Dtos.ProspectOpportunities> UpdateProspectOpportunitiesSubmissionsAsync(ProspectOpportunitiesSubmissions prospectOpportunitiesSubmissions, bool bypassCache)
        {
            ValidateProspectOpportunitiesSubmissions(prospectOpportunitiesSubmissions);

            // verify the user has the permission to update a prospectOpportunitiesSubmissions
            if (!await CheckUpdateProspectOpportunitiesPermissionAsync())
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update prospect-opportunities.");
                throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to update prospect-opportunities.");
            }

            _prospectOpportunitiesRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // get the ID associated with the incoming guid
            string prospectOpportunitiesSubmissionsEntityId = string.Empty;
            try
            {
                prospectOpportunitiesSubmissionsEntityId = await _prospectOpportunitiesRepository.GetProspectOpportunityIdFromGuidAsync(prospectOpportunitiesSubmissions.Id);
            }
            catch (Exception)
            { // if the guid is not found then attempt to create
            }

            if (!string.IsNullOrEmpty(prospectOpportunitiesSubmissionsEntityId))
            {

                try
                {
                    // map the DTO to entities
                    Domain.Student.Entities.AdmissionApplication admissionApplicationEntity
                    = await ConvertProspectOpportunitiesSubmissionsDtoToEntityAsync(prospectOpportunitiesSubmissions.Id, prospectOpportunitiesSubmissions, bypassCache);

                    // update the entity in the database
                    var updatedProspectOpportunitiesSubmissions =
                        await _prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(admissionApplicationEntity);

                    Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { updatedProspectOpportunitiesSubmissions.ProspectId });
                    Dictionary<string, string> stAcadPrgDict = await _studentAcademicProgramRepository.GetStudentAcademicProgramGuidsCollectionAsync(new List<string>()
                    {
                        string.Concat(admissionApplicationEntity.ApplicantPersonId, "*", admissionApplicationEntity.ApplicationAcadProgram)
                    });

                    ProspectOpportunities dto = await ConvertEntityToDtoAsync(updatedProspectOpportunitiesSubmissions, personDict, stAcadPrgDict);

                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }

                    return dto ?? null;
                }
                catch (IntegrationApiException ex)
                {
                    throw ex;
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await CreateProspectOpportunitiesSubmissionsAsync(prospectOpportunitiesSubmissions, bypassCache);
        }

        /// <summary>
        /// Create a ProspectOpportunitiesSubmissions.
        /// </summary>
        /// <param name="prospectOpportunitiesSubmissions">The <see cref="ProspectOpportunitiesSubmissions">prospectOpportunitiesSubmissions</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="ProspectOpportunities">ProspectOpportunities</see></returns>
        public async Task<Dtos.ProspectOpportunities> CreateProspectOpportunitiesSubmissionsAsync(Dtos.ProspectOpportunitiesSubmissions prospectOpportunitiesSubmissions, bool bypassCache)
        {
            //Validate the request 
            ValidateProspectOpportunitiesSubmissions(prospectOpportunitiesSubmissions);

            // verify the user has the permission to create a prospectOpportunitiesSubmissions
            if (!await CheckUpdateProspectOpportunitiesPermissionAsync())
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create prospect-opportunities.");
                throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to create prospect-opportunities.");
            }

            _prospectOpportunitiesRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {

                var admissionApplicationEntity
                         = await ConvertProspectOpportunitiesSubmissionsDtoToEntityAsync(prospectOpportunitiesSubmissions.Id, prospectOpportunitiesSubmissions, bypassCache);

                // create a ProspectOpportunities entity in the database
                var createdProspectOpportunitiesSubmissions =
                    await _prospectOpportunitiesRepository.CreateProspectOpportunitiesSubmissionsAsync(admissionApplicationEntity);
                // return the newly created ProspectOpportunities
                Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { createdProspectOpportunitiesSubmissions.ProspectId });
                Dictionary<string, string> stAcadPrgDict = await _studentAcademicProgramRepository.GetStudentAcademicProgramGuidsCollectionAsync(new List<string>()
                {
                    string.Concat(admissionApplicationEntity.ApplicantPersonId, "*", admissionApplicationEntity.ApplicationAcadProgram)
                });

                var dto = await ConvertEntityToDtoAsync(createdProspectOpportunitiesSubmissions, personDict, stAcadPrgDict);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dto ?? null;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        #endregion

        #region Reference Methods

        /// <summary>
        /// Terms
        /// </summary>
        private IEnumerable<Term> _terms;
        private async Task<IEnumerable<Term>> Terms(bool bypassCache)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        /// <summary>
        /// AdmissionPopulation
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AdmissionPopulation> _admissionPopulations;
        private async Task<IEnumerable<Domain.Student.Entities.AdmissionPopulation>> AdmissionPopulationsAsync(bool bypassCache)
        {
            if (_admissionPopulations == null)
            {
                _admissionPopulations = await _studentReferenceDataRepository.GetAdmissionPopulationsAsync(bypassCache);
            }
            return _admissionPopulations;
        }

        /// <summary>
        /// Sites
        /// </summary>
        private IEnumerable<Domain.Base.Entities.Location> _locations;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> SitesAsync(bool bypassCache)
        {
            if (_locations == null)
            {
                _locations = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _locations;
        }

        /// <summary>
        /// Person Origin Codes for Person Source
        /// </summary>
        private IEnumerable<Domain.Base.Entities.PersonOriginCodes> _personOriginCodes;
        private async Task<IEnumerable<Domain.Base.Entities.PersonOriginCodes>> PersonOriginCodesAsync(bool bypassCache)
        {
            if (_personOriginCodes == null)
            {
                _personOriginCodes = await _referenceDataRepository.GetPersonOriginCodesAsync(bypassCache);
            }
            return _personOriginCodes;
        }

        /// <summary>
        /// AcademicLevels
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> AcademicLevelsAsync(bool bypassCache)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            }
            return _academicLevels;
        }

        /// <summary>
        /// AcademicDisciplines
        /// </summary>
        private IEnumerable<Domain.Base.Entities.AcademicDiscipline> _academicDisciplines;
        private async Task<IEnumerable<Domain.Base.Entities.AcademicDiscipline>> AcademicDisciplinesAsync(bool bypassCache)
        {
            if (_academicDisciplines == null)
            {
                _academicDisciplines = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache);
            }
            return _academicDisciplines;
        }

        /// <summary>
        /// AcademicDisciplines
        /// </summary>
        private IEnumerable<Domain.Base.Entities.AcadCredential> _academicCredentials;
        private async Task<IEnumerable<Domain.Base.Entities.AcadCredential>> AcademicCredentialsAsync(bool bypassCache)
        {
            if (_academicCredentials == null)
            {
                _academicCredentials = await _referenceDataRepository.GetAcadCredentialsAsync(bypassCache);
            }
            return _academicCredentials;
        }

        /// <summary>
        /// AcademicProgram
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicProgram> _academicPrograms;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicProgram>> AcademicProgramsAsync(bool bypassCache)
        {
            if (_academicPrograms == null)
            {
                _academicPrograms = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);
            }
            return _academicPrograms;
        }

        /// <summary>
        /// AcademicProgram
        /// </summary>
        private IEnumerable<Domain.Student.Entities.AcademicDepartment> _academicDepartments;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicDepartment>> AcademicDepartmentsAsync(bool bypassCache)
        {
            if (_academicDepartments == null)
            {
                _academicDepartments = await _studentReferenceDataRepository.GetAcademicDepartmentsAsync(bypassCache);
            }
            return _academicDepartments;
        }

        /// <summary>
        /// Educational goal
        /// </summary>
        private IEnumerable<Domain.Student.Entities.EducationGoals> _educationalGoals;
        private async Task<IEnumerable<Domain.Student.Entities.EducationGoals>> EducationalGoalsAsync(bool bypassCache)
        {
            if (_educationalGoals == null)
            {
                _educationalGoals = await _studentReferenceDataRepository.GetEducationGoalsAsync(bypassCache);
            }
            return _educationalGoals;
        }

        /// <summary>
        /// Career goal
        /// </summary>
        private IEnumerable<Domain.Student.Entities.CareerGoal> _careerGoals;
        private async Task<IEnumerable<Domain.Student.Entities.CareerGoal>> CareerGoalsAsync(bool bypassCache)
        {
            if (_careerGoals == null)
            {
                _careerGoals = await _studentReferenceDataRepository.GetCareerGoalsAsync(bypassCache);
            }
            return _careerGoals;
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="personDict"></param>
        /// <param name="stAcadPrgDict"></param>
        /// <returns></returns>
        private async Task<ProspectOpportunities> ConvertEntityToDtoAsync(ProspectOpportunity source, Dictionary<string, string> personDict, Dictionary<string, string> stAcadPrgDict)
        {
            ProspectOpportunities dto = new ProspectOpportunities();

            //id
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for prospect-opportunities entity.", "Bad.Data", id: source.RecordKey);
            }
            dto.Id = source.Guid;

            //prospect.id
            string prospectId;
            if(!personDict.TryGetValue(source.ProspectId, out prospectId))
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for prospect ID '{0}'", source.RecordKey), "Bad.Data", source.Guid, source.RecordKey);
            }
            dto.Prospect = new GuidObject2(prospectId);

            //recruitAcademicPrograms.id
            string studentAcadProgId;
            if (!stAcadPrgDict.TryGetValue(source.StudentAcadProgId, out studentAcadProgId))
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for recruit academic programs ID '{0}'", source.StudentAcadProgId), "Bad.Data", source.Guid, 
                                                source.RecordKey);
            }
            dto.RecruitAcademicPrograms = new List<GuidObject2>()
            {
                new GuidObject2(studentAcadProgId)
            };

            //entryAcademicPeriod.id optional field
            if (!string.IsNullOrEmpty(source.EntryAcademicPeriod))
            {
                var termGuid = string.Empty;
                try
                {
                    termGuid = await _termRepository.GetAcademicPeriodsGuidAsync(source.EntryAcademicPeriod);
                }
                catch
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for entry academic period ID '{0}'", source.EntryAcademicPeriod), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                if (string.IsNullOrEmpty(termGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for entry academic period ID '{0}'", source.EntryAcademicPeriod), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                dto.EntryAcademicPeriod = new GuidObject2(termGuid);
            }

            //admissionPopulation.id optional field
            if (!string.IsNullOrEmpty(source.AdmissionPopulation))
            {
                var admissionPopulationGuid = string.Empty;
                try
                {
                    admissionPopulationGuid = await _studentReferenceDataRepository.GetAdmissionPopulationsGuidAsync(source.AdmissionPopulation);
                }
                catch
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for admission population ID '{0}'", source.AdmissionPopulation), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                if (string.IsNullOrEmpty(admissionPopulationGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for admission population ID '{0}'", source.AdmissionPopulation), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                dto.AdmissionPopulation = new GuidObject2(admissionPopulationGuid);
            }

            //site.id optional field
            if (!string.IsNullOrEmpty(source.Site))
            {
                var siteId = string.Empty;
                try
                {
                    siteId = await _referenceDataRepository.GetLocationsGuidAsync(source.Site);
                }
                catch
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for site ID '{0}'", source.Site), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                if (string.IsNullOrEmpty(siteId))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for site ID '{0}'", source.Site), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                dto.Site = new GuidObject2(siteId);
            }

            //educationalGoal.id optional field
            if (!string.IsNullOrEmpty(source.EducationalGoal))
            {
                var educationalGoalId = string.Empty;
                try
                {
                    educationalGoalId = await _studentReferenceDataRepository.GetEducationGoalGuidAsync(source.EducationalGoal);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for educational goal ID '{0}'", source.EducationalGoal), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                if (string.IsNullOrEmpty(educationalGoalId))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for educational goal ID '{0}'", source.EducationalGoal), "Bad.Data", source.Guid,
                                    source.RecordKey);
                }
                dto.EducationalGoal = new GuidObject2(educationalGoalId);
            }

            //careerGoals.id optional field
            if (source.CareerGoals != null && source.CareerGoals.Any())
            {
                var careerGoalsObj = new List<GuidObject2>();
                foreach (var careerGoal in source.CareerGoals)
                {
                    if (!string.IsNullOrEmpty(careerGoal))
                    {
                        var careerGoalId = string.Empty;
                        try
                        {
                            careerGoalId = await _studentReferenceDataRepository.GetCareerGoalGuidAsync(careerGoal);
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for career goal ID '{0}'", careerGoal), "Bad.Data", source.Guid,
                                            source.RecordKey);
                        }
                        if (string.IsNullOrEmpty(careerGoalId))
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for career goal ID '{0}'", careerGoal), "Bad.Data", source.Guid,
                                            source.RecordKey);
                        }
                        careerGoalsObj.Add(new GuidObject2(careerGoalId));
                    }
                }
                dto.CareerGoals = careerGoalsObj;
            }

            return dto;
        }

        /// </summary>
        /// <param name="source"></param>
        /// <param name="personDict"></param>
        /// <param name="stAcadPrgDict"></param>
        /// <returns></returns>
        private async Task<ProspectOpportunitiesSubmissions> ConvertProspectOpportunitiesSubmissionsEntityToDtoAsync(Domain.Student.Entities.AdmissionApplication source, Dictionary<string, string> personDict, Dictionary<string, string> stAcadPrgDict)
        {
            ProspectOpportunitiesSubmissions dto = new ProspectOpportunitiesSubmissions();

            //id
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for prospect-opportunities entity.",
                    "Bad.Data", id: source.ApplicationRecordKey);
            }
            dto.Id = source.Guid;

            //prospect.id
            string prospectId = string.Empty;
            if (!personDict.TryGetValue(source.ApplicantPersonId, out prospectId))
            {
                IntegrationApiExceptionAddError("Unable to locate guid for prospect ID.",
                    "Bad.Data", source.Guid, source.ApplicationRecordKey);
            }
            dto.Prospect = new GuidObject2(prospectId);

            //recruitAcademicPrograms.program.id
            var recruitAcademicPrograms = new List<RecruitAcademicProgram>();
            if (string.IsNullOrEmpty(source.ApplicationAcadProgram))
            {
                IntegrationApiExceptionAddError("Unable to locate guid for academic program.",
                    "Bad.Data", source.Guid, source.ApplicationRecordKey);
            }
            string programGuid = "";
            string academicLevelGuid = "";
            var programEntity = (await AcademicProgramsAsync(false)).FirstOrDefault(i => i.Code.Equals(source.ApplicationAcadProgram, StringComparison.OrdinalIgnoreCase));
            if (programEntity == null)
            {
                IntegrationApiExceptionAddError("Unable to locate guid for academic program.",
                    "Bad.Data", source.Guid, source.ApplicationRecordKey);
            }
            else
            {
                programGuid = programEntity.Guid;
                var academicLevel = programEntity.AcadLevelCode;
                if (!string.IsNullOrEmpty(academicLevel))
                {
                    var acadLevelEntity = (await AcademicLevelsAsync(false)).FirstOrDefault(i => i.Code.Equals(academicLevel, StringComparison.OrdinalIgnoreCase));
                    if (acadLevelEntity == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic level not found for code {0}. ", academicLevel),
                            "Bad.Data", source.Guid, source.ApplicationRecordKey);
                    }
                    else
                    {
                        academicLevelGuid = acadLevelEntity.Guid;
                    }
                }
            }
            var recruitAcademicProgram = new RecruitAcademicProgram()
            {
                Program = new GuidObject2(programGuid),
                AcademicLevel = new GuidObject2(academicLevelGuid)
            };
            // recruitAcademicPrograms.disciplines
            if (source.ApplicationDisciplines != null && source.ApplicationDisciplines.Any())
            {
                foreach (var discipline in source.ApplicationDisciplines)
                {
                    var disciplineEntity = (await AcademicDisciplinesAsync(false)).FirstOrDefault(i => i.Code.Equals(discipline.Code, StringComparison.OrdinalIgnoreCase));
                    if (disciplineEntity == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic discipline '{0}'.", discipline),
                            "Bad.Data", source.Guid, source.ApplicationRecordKey);
                    }
                    else
                    {
                        string deptGuid = "";
                        if (!string.IsNullOrEmpty(discipline.AdministeringInstitutionUnit))
                        {
                            var acadDeptEntity = (await AcademicDepartmentsAsync(false)).FirstOrDefault(i => i.Code.Equals(discipline.AdministeringInstitutionUnit, StringComparison.OrdinalIgnoreCase));
                            if (acadDeptEntity != null && !string.IsNullOrEmpty(acadDeptEntity.Guid))
                            {
                                deptGuid = acadDeptEntity.Guid;
                            }
                        }
                        if (recruitAcademicProgram.Disciplines == null)
                        {
                            recruitAcademicProgram.Disciplines = new List<RecruitAcademicProgramDiscipline>();
                        }
                        recruitAcademicProgram.Disciplines.Add(new RecruitAcademicProgramDiscipline()
                        {
                            Discipline = new GuidObject2(disciplineEntity.Guid),
                            StartOn = discipline.StartOn,
                            AdministeringInstitutionUnit = new GuidObject2(deptGuid)
                        });
                    }
                }
            }
            //recruitAcademicPrograms.credentials
            if (source.ApplicationCredentials != null && source.ApplicationCredentials.Any())
            {
                foreach (var credential in source.ApplicationCredentials)
                {
                    var credentialEntity = (await AcademicCredentialsAsync(false)).FirstOrDefault(i => i.Code.Equals(credential, StringComparison.OrdinalIgnoreCase));
                    if (credentialEntity == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic credential '{0}'.", credential),
                            "Bad.Data", source.Guid, source.ApplicationRecordKey);
                    }
                    else
                    {
                        if (recruitAcademicProgram.Credentials == null)
                        {
                            recruitAcademicProgram.Credentials = new List<GuidObject2>();
                        }
                        recruitAcademicProgram.Credentials.Add(new GuidObject2(credentialEntity.Guid));
                    }
                }
            }
            //recruitAcademicPrograms.programOwner.id
            if (!string.IsNullOrEmpty(source.ApplicationProgramOwner))
            {
                var departmentEntity = (await AcademicDepartmentsAsync(false)).FirstOrDefault(i => i.Code.Equals(source.ApplicationProgramOwner, StringComparison.OrdinalIgnoreCase));
                if (departmentEntity == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for academic department '{0}'.", source.ApplicationOwnerId),
                            "Bad.Data", source.Guid, source.ApplicationRecordKey);
                }
                else
                {
                    recruitAcademicProgram.ProgramOwner = new GuidObject2(departmentEntity.Guid);
                }
            }
            if (recruitAcademicProgram != null)
            {
                recruitAcademicPrograms.Add(recruitAcademicProgram);
                dto.RecruitAcademicPrograms = recruitAcademicPrograms;
            }

            //entryAcademicPeriod.id optional field
            if (!string.IsNullOrEmpty(source.ApplicationStartTerm))
            {
                var termGuid = await _termRepository.GetAcademicPeriodsGuidAsync(source.ApplicationStartTerm);
                if (string.IsNullOrEmpty(termGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for entry academic period ID '{0}'", source.ApplicationStartTerm),
                        "Bad.Data", source.Guid, source.ApplicationRecordKey);
                }
                dto.EntryAcademicPeriod = new GuidObject2(termGuid);
            }

            //admissionPopulation.id optional field
            if (!string.IsNullOrEmpty(source.ApplicationAdmitStatus))
            {
                var admissionPopulationGuid = await _studentReferenceDataRepository.GetAdmissionPopulationsGuidAsync(source.ApplicationAdmitStatus);
                if (string.IsNullOrEmpty(admissionPopulationGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for admission population ID '{0}'", source.ApplicationAdmitStatus),
                        "Bad.Data", source.Guid, source.ApplicationRecordKey);
                }
                dto.AdmissionPopulation = new GuidObject2(admissionPopulationGuid);
            }

            //site.id optional field
            string location = source.ApplicationLocations.FirstOrDefault();
            if (!string.IsNullOrEmpty(location))
            {
                var siteId = await _referenceDataRepository.GetLocationsGuidAsync(location);
                if (string.IsNullOrEmpty(siteId))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for site ID '{0}'", location),
                        "Bad.Data", source.Guid, source.ApplicationRecordKey);
                }
                dto.Site = new GuidObject2(siteId);
            }

            // Person Source
            if (!string.IsNullOrEmpty(source.PersonSource))
            {
                var sourceId = await _referenceDataRepository.GetPersonOriginCodesGuidAsync(source.PersonSource);
                if (string.IsNullOrEmpty(sourceId))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for personSource ID '{0}'", source.PersonSource),
                        "Bad.Data", source.Guid, source.ApplicationRecordKey);
                }
                dto.PersonSource = new GuidObject2(sourceId);
            }

            return dto;
        }

        /// <summary>
        /// Converts submissions dto to entity.
        /// </summary>
        /// <returns></returns>
        private async Task<Domain.Student.Entities.AdmissionApplication> ConvertProspectOpportunitiesSubmissionsDtoToEntityAsync(string guid, ProspectOpportunitiesSubmissions dto, bool bypassCache)
        {
            //id
            if (string.IsNullOrEmpty(dto.Id))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for prospect-opportunities-submissions.",
                    "Validation.Exception", guid: guid);
            }
            string applicationKey = string.Empty;
            Domain.Student.Entities.AdmissionApplication entity = new Domain.Student.Entities.AdmissionApplication(guid);
            if (guid.Equals(Guid.Empty.ToString()))
            {
                entity = new Domain.Student.Entities.AdmissionApplication(guid);
            }
            else
            {
                try
                {
                    applicationKey = await _prospectOpportunitiesRepository.GetProspectOpportunityIdFromGuidAsync(guid);
                    entity = new Domain.Student.Entities.AdmissionApplication(guid, applicationKey);
                }
                catch
                {
                    IntegrationApiExceptionAddError(string.Format("No prospect opportunty was found for guid {0}.", guid),
                        "GUID.Not.Found", guid: guid);
                }
            }

            //applicant.id persons.json
            string applicantKey = "";
            if (dto.Prospect != null && !string.IsNullOrEmpty(dto.Prospect.Id))
            {
                applicantKey = await _personRepository.GetPersonIdFromGuidAsync(dto.Prospect.Id);
                if (string.IsNullOrEmpty(applicantKey))
                {
                    IntegrationApiExceptionAddError(string.Format("Prospect not found for guid {0}. ", dto.Prospect.Id),
                        "GUID.Not.Found", guid, applicationKey);
                }
                else
                {
                    entity.ApplicantPersonId = applicantKey;
                }
            }

            //academicPeriod.id
            if (dto.EntryAcademicPeriod != null && !string.IsNullOrEmpty(dto.EntryAcademicPeriod.Id))
            {
                var termsEntity = (await Terms(bypassCache)).FirstOrDefault(i => i.RecordGuid.Equals(dto.EntryAcademicPeriod.Id, StringComparison.OrdinalIgnoreCase));
                if (termsEntity == null || string.IsNullOrEmpty(termsEntity.Code))
                {
                    var error = string.Format("Academic period not found for guid  {0}. ", dto.EntryAcademicPeriod.Id);
                    IntegrationApiExceptionAddError(error, "GUID.Not.Found", guid, applicationKey);
                }
                else
                {
                    if (string.IsNullOrEmpty(termsEntity.Code))
                    {
                        IntegrationApiExceptionAddError(string.Format("Academic period not found for guid {0}. ", dto.EntryAcademicPeriod.Id),
                            "GUID.Not.Found", guid, applicationKey);
                    }
                    else
                    {
                        entity.ApplicationStartTerm = termsEntity.Code;
                    }
                }
            }

            if (dto.RecruitAcademicPrograms != null && dto.RecruitAcademicPrograms.Any())
            {
                Domain.Student.Entities.AcademicProgram programEntity = null;
                var program = dto.RecruitAcademicPrograms.FirstOrDefault();
                if (program != null)
                {
                    //program.id
                    if (program.Program != null && !string.IsNullOrEmpty(program.Program.Id))
                    {
                        programEntity = (await AcademicProgramsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(program.Program.Id, StringComparison.OrdinalIgnoreCase));
                        if (programEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Academic program not found for guid {0}. ", program.Program.Id),
                                "GUID.Not.Found", guid, applicationKey);
                        }
                        else
                        {
                            entity.ApplicationAcadProgram = programEntity.Code;
                        }
                    }

                    //program.credentials
                    if (program.Credentials != null && program.Credentials.Any())
                    {
                        foreach (var credential in program.Credentials)
                        {
                            if (credential != null && !string.IsNullOrEmpty(credential.Id))
                            {
                                var credentialEntity = (await AcademicCredentialsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(credential.Id, StringComparison.OrdinalIgnoreCase));
                                if (credentialEntity == null || string.IsNullOrEmpty(credentialEntity.Code))
                                {
                                    IntegrationApiExceptionAddError(string.Format("Academic Credentials not found for guid {0}. ", credential.Id),
                                        "GUID.Not.Found", guid, applicationKey);
                                }
                                else
                                {
                                    if (entity.ApplicationCredentials == null)
                                    {
                                        entity.ApplicationCredentials = new List<string>();
                                    }
                                    switch (credentialEntity.AcademicCredentialType)
                                    {
                                        case Domain.Base.Entities.AcademicCredentialType.Degree:
                                            {
                                                if (programEntity == null || programEntity.DegreeCode != credentialEntity.Code)
                                                {
                                                    IntegrationApiExceptionAddError(string.Format("The requested degree '{0}' is not permitted for the academic program '{1}'.", credentialEntity.Code, entity.ApplicationAcadProgram),
                                                        "Validation.Exception", guid, applicationKey);
                                                }
                                                break;
                                            }
                                        case Domain.Base.Entities.AcademicCredentialType.Certificate:
                                            {

                                                entity.ApplicationCredentials.Add(credentialEntity.Code);
                                                break;
                                            }
                                        default:
                                            IntegrationApiExceptionAddError(string.Format("Academic Credentials of type '{0}' is not allowed. ", credentialEntity.AcademicCredentialType.ToString()),
                                                "Validation.Exception", guid, applicationKey);
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    //program.programOwner
                    if (program.ProgramOwner != null && !string.IsNullOrEmpty(program.ProgramOwner.Id))
                    {
                        var ownerEntity = (await AcademicDepartmentsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(program.ProgramOwner.Id, StringComparison.OrdinalIgnoreCase));
                        if (ownerEntity == null || string.IsNullOrEmpty(ownerEntity.Code))
                        {
                            IntegrationApiExceptionAddError(string.Format("Application program owner not found for guid {0}. ", program.ProgramOwner.Id),
                                "GUID.Not.Found", guid, applicationKey);
                        }
                        else
                        {
                            entity.ApplicationOwnerId = ownerEntity.Code;
                        }
                    }

                    //academicLevel.id
                    if (program.AcademicLevel != null && !string.IsNullOrEmpty(program.AcademicLevel.Id))
                    {
                        //This property really is not used for PUT/POST since it is derived from Acad program in GET
                        var acadLevelEntity = (await AcademicLevelsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(program.AcademicLevel.Id, StringComparison.OrdinalIgnoreCase));
                        if (acadLevelEntity == null || string.IsNullOrEmpty(acadLevelEntity.Code))
                        {
                            IntegrationApiExceptionAddError(string.Format("Academic level not found for guid {0}. ", program.AcademicLevel.Id),
                                "GUID.Not.Found", guid, applicationKey);
                        }
                        else
                        {
                            entity.ApplicationAcadLevel = acadLevelEntity.Code;
                        }
                    }

                    //disciplines
                    if (program.Disciplines != null && program.Disciplines.Any())
                    {
                        foreach (var discipline in program.Disciplines)
                        {
                            if (discipline.Discipline != null && !string.IsNullOrEmpty(discipline.Discipline.Id))
                            {
                                var disciplineEntity = (await AcademicDisciplinesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(discipline.Discipline.Id, StringComparison.OrdinalIgnoreCase));
                                if (disciplineEntity == null || string.IsNullOrEmpty(disciplineEntity.Code))
                                {
                                    IntegrationApiExceptionAddError(string.Format("Academic Discipline not found for guid {0}. ", discipline.Discipline.Id),
                                        "GUID.Not.Found", guid, applicationKey);
                                }
                                else
                                {
                                    string deptCode = "";
                                    if (discipline.AdministeringInstitutionUnit != null && !string.IsNullOrEmpty(discipline.AdministeringInstitutionUnit.Id))
                                    {
                                        var acadDeptEntity = (await AcademicDepartmentsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(discipline.AdministeringInstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                                        if (acadDeptEntity == null || string.IsNullOrEmpty(acadDeptEntity.Code))
                                        {
                                            IntegrationApiExceptionAddError(string.Format("Administering Institution Unit not found for guid {0}. ", discipline.AdministeringInstitutionUnit.Id),
                                                "GUID.Not.Found", guid, applicationKey);
                                        }
                                        else
                                        {
                                            deptCode = acadDeptEntity.Code;
                                        }
                                    }
                                    if (entity.ApplicationStprAcadPrograms == null)
                                    {
                                        entity.ApplicationStprAcadPrograms = new List<string>();
                                    }
                                    if (entity.ApplicationDisciplines == null)
                                    {
                                        entity.ApplicationDisciplines = new List<ApplicationDiscipline>();
                                    }
                                    entity.ApplicationStprAcadPrograms.Add(disciplineEntity.Code);
                                    entity.ApplicationDisciplines.Add(new ApplicationDiscipline()
                                    {
                                        Code = disciplineEntity.Code,
                                        AdministeringInstitutionUnit = deptCode,
                                        DisciplineType = disciplineEntity.AcademicDisciplineType,
                                        StartOn = discipline.StartOn
                                    });
                                }
                            }
                        }
                    }
                }
            }

            //admissionPopulation.id
            if (dto.AdmissionPopulation != null && !string.IsNullOrEmpty(dto.AdmissionPopulation.Id))
            {
                var admPopulation = (await AdmissionPopulationsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.AdmissionPopulation.Id, StringComparison.OrdinalIgnoreCase));
                if (admPopulation == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Application admit status not found for guid {0}. ", dto.AdmissionPopulation.Id),
                        "GUID.Not.Found", guid, applicationKey);
                }
                else
                {
                    entity.ApplicationAdmitStatus = admPopulation.Code;
                }
            }

            //site.id
            if (dto.Site != null && !string.IsNullOrEmpty(dto.Site.Id))
            {
                var site = (await SitesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.Site.Id, StringComparison.OrdinalIgnoreCase));
                if (site == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Site not found for guid {0}.", dto.Site.Id),
                        "GUID.Not.Found", guid, applicationKey);
                }
                else
                {
                    entity.ApplicationLocations = new List<string>() { site.Code };
                }
            }

            // opportunitySource.id (Not supported in Colleague)
            if (dto.OpportunitySource != null && !string.IsNullOrEmpty(dto.OpportunitySource.Id))
            {
                IntegrationApiExceptionAddError(string.Format("Opportunity Source not found for guid {0}", dto.OpportunitySource.Id),
                        "GUID.Not.Found", guid, applicantKey);
            }

            // personSource.id
            if (dto.PersonSource != null && !string.IsNullOrEmpty(dto.PersonSource.Id))
            {
                var source = (await PersonOriginCodesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.PersonSource.Id, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Person Source not found for guid {0}", dto.PersonSource.Id),
                        "GUID.Not.Found", guid, applicantKey);
                }
                else
                {
                    entity.PersonSource = source.Code;
                }
            }

            //educational goal
            if (dto.EducationalGoal != null && !string.IsNullOrEmpty(dto.EducationalGoal.Id))
            {
                try
                {
                    var educationalGoal = (await EducationalGoalsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(dto.EducationalGoal.Id, StringComparison.OrdinalIgnoreCase));
                    if (educationalGoal == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Educational goal not found for guid '{0}'.", dto.EducationalGoal.Id), "Bad.Data", guid, applicantKey);
                    }
                    else
                    {
                        entity.EducationalGoal = educationalGoal.Code;
                    }
                }
                catch
                {
                    IntegrationApiExceptionAddError(string.Format("Educational goal not found for guid '{0}'.", dto.EducationalGoal.Id), "Bad.Data", guid, applicantKey);
                }
            }

            //career goals
            if (dto.CareerGoals != null && dto.CareerGoals.Any())
            {
                var careerGoals = new List<string>();
                foreach (var careerGoal in dto.CareerGoals)
                {
                    if (careerGoal != null && !string.IsNullOrEmpty(careerGoal.Id))
                    {
                        try
                        {
                            var careerGoalObject = (await CareerGoalsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(careerGoal.Id, StringComparison.OrdinalIgnoreCase));
                            if (careerGoalObject == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Career goal not found for guid '{0}'.", careerGoal.Id), "Bad.Data", guid, applicantKey);
                            }
                            else
                            {
                                careerGoals.Add(careerGoalObject.Code);
                            }
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError(string.Format("Career goal not found for guid '{0}'.", careerGoal.Id), "Bad.Data", guid, applicantKey);
                        }
                    }
                }
                entity.CareerGoals = careerGoals;
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return entity;
        }

        /// <summary>
        /// Validate ProspectOpportunitiesSubmissions DTO
        /// </summary>
        /// <param name="source"></param>
        private void ValidateProspectOpportunitiesSubmissions(ProspectOpportunitiesSubmissions source)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("ProspectOpportunitiesSubmissions body is required.", "Validation.Exception");
                throw IntegrationApiException;
            }

            if (string.IsNullOrWhiteSpace(source.Id))
            {
                IntegrationApiExceptionAddError("id is required.", "Missing.Required.Property");
            }
            if ((source.Prospect == null) || (string.IsNullOrWhiteSpace(source.Prospect.Id)))
            {
                IntegrationApiExceptionAddError("prospect.id is required.", "Missing.Required.Property", source.Id);
            }

            if (source.RecruitAcademicPrograms == null || !source.RecruitAcademicPrograms.Any() || source.RecruitAcademicPrograms.Count() > 1)
            {
                if (source.RecruitAcademicPrograms == null || !source.RecruitAcademicPrograms.Any())
                {
                    IntegrationApiExceptionAddError("The property recruitAcademicProgram is required.", "Validation.Exception", source.Id);
                }
                else
                {
                    IntegrationApiExceptionAddError("Only one recruitAcademicProgram per application can be submitted.", "Validation.Exception", source.Id);
                }
            }
            else
            {
                var recruitAcademicProgram = source.RecruitAcademicPrograms.FirstOrDefault();
                if (recruitAcademicProgram == null || recruitAcademicProgram.Program == null || string.IsNullOrEmpty(recruitAcademicProgram.Program.Id))
                {
                    IntegrationApiExceptionAddError("recruitAcademicPrograms[0].program.id is required.", "Missing.Required.Property", source.Id);
                }

                if (recruitAcademicProgram != null)
                {
                    if (recruitAcademicProgram.AcademicLevel != null && string.IsNullOrEmpty(recruitAcademicProgram.AcademicLevel.Id))
                    {
                        IntegrationApiExceptionAddError("recruitAcademicProgram.acadLevel.id is required when submitting academicLevel.", "Missing.Required.Property", source.Id);
                    }
                    if (recruitAcademicProgram.Credentials != null && recruitAcademicProgram.Credentials.Any())
                    {
                        foreach (var credential in recruitAcademicProgram.Credentials)
                        {
                            if (credential != null && string.IsNullOrEmpty(credential.Id))
                            {
                                IntegrationApiExceptionAddError("recruitAcademicProgram.credentials.id is required when submitting credentials.", "Missing.Required.Property", source.Id);
                            }
                        }
                    }
                    if (recruitAcademicProgram.ProgramOwner != null && string.IsNullOrEmpty(recruitAcademicProgram.ProgramOwner.Id))
                    {
                        IntegrationApiExceptionAddError("recruitAcademicProgram.programOwner.id is required when submitting programOwner.", "Missing.Required.Property", source.Id);
                    }
                    if (recruitAcademicProgram.Disciplines != null && recruitAcademicProgram.Disciplines.Any())
                    {
                        foreach (var discipline in recruitAcademicProgram.Disciplines)
                        {
                            if (discipline == null || discipline.Discipline == null || string.IsNullOrEmpty(discipline.Discipline.Id))
                            {
                                IntegrationApiExceptionAddError("recruitAcademicProgram.discipline.discipline.id is required when submitting disciplines.", "Missing.Required.Property", source.Id);
                            }
                            if (discipline != null && discipline.AdministeringInstitutionUnit != null && string.IsNullOrEmpty(discipline.AdministeringInstitutionUnit.Id))
                            {
                                IntegrationApiExceptionAddError("recruitAcademicProgram.discipline.administeringInstitutionUnit.id is required when submitting administeringInstitionUnit.", "Missing.Required.Property", source.Id);
                            }
                        }
                    }
                }
            }

            if (source.EntryAcademicPeriod != null)
            {
                var entryAcademicPeriod = source.EntryAcademicPeriod;
                if (string.IsNullOrEmpty(entryAcademicPeriod.Id))
                {
                    IntegrationApiExceptionAddError("If providing an entryAcademicPeriod then entryAcademicPeriod.id is required.", "Missing.Required.Property", source.Id);
                }
            }

            if (source.AdmissionPopulation != null)
            {
                var admissionPopulation = source.AdmissionPopulation;
                if (string.IsNullOrEmpty(admissionPopulation.Id))
                {
                    IntegrationApiExceptionAddError("If providing an admissionPopulation then admissionPopulation.id is required.", "Missing.Required.Property", source.Id);
                }
            }

            if (source.Site != null)
            {
                var site = source.Site;
                if (string.IsNullOrEmpty(site.Id))
                {
                    IntegrationApiExceptionAddError("If providing a site then site.id is required.", "Missing.Required.Property", source.Id);
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;
        }

        /// <summary>
        /// Permissions code that allows an external system to perform the READ operation.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task<bool> CheckViewProspectOpportunitiesPermissionAsync()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.ViewProspectOpportunity))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Permissions code that allows an external system to perform the CREATE and UPDATE operations using prospect-opportunities-submissions and therefore also provides 
        /// the same permissions as VIEW.PROSPECT.OPPORTUNITY.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckUpdateProspectOpportunitiesPermissionAsync()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.UpdateProspectOpportunity))
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}