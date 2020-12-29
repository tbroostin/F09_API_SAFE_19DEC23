//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonExternalEducationCredentialsService : BaseCoordinationService, IPersonExternalEducationCredentialsService
    {
        private readonly IPersonExternalEducationCredentialsRepository _personExternalEducationCredentialsRepository;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        public PersonExternalEducationCredentialsService(
            IPersonExternalEducationCredentialsRepository personExternalEducationCredentialsRepository,
            IInstitutionRepository institutionRepository,
            IReferenceDataRepository referenceDataRepository,
            IPersonRepository personRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _personExternalEducationCredentialsRepository = personExternalEducationCredentialsRepository;
            _institutionRepository = institutionRepository;
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
        }

        #region Reference Methods

        /// <summary>
        /// OtherHonors
        /// </summary>
        private IEnumerable<Domain.Base.Entities.OtherHonor> _academicHonors;
        private async Task<IEnumerable<Domain.Base.Entities.OtherHonor>> AcademicHonorsAsync(bool bypassCache)
        {
            if (_academicHonors == null)
            {
                _academicHonors = await _referenceDataRepository.GetOtherHonorsAsync(bypassCache);
            }
            return _academicHonors;
        }

        /// <summary>
        /// OtherCcds
        /// </summary>
        private IEnumerable<Domain.Base.Entities.OtherCcd> _otherCertifications;
        private async Task<IEnumerable<Domain.Base.Entities.OtherCcd>> OtherCcdsAsync(bool bypassCache)
        {
            if (_otherCertifications == null)
            {
                _otherCertifications = await _referenceDataRepository.GetOtherCcdsAsync(bypassCache);
            }
            return _otherCertifications;
        }

        /// <summary>
        /// CcdTypes
        /// </summary>
        private IEnumerable<Domain.Base.Entities.CcdType> _ccdTypes;
        private async Task<IEnumerable<Domain.Base.Entities.CcdType>> CcdTypesAsync(bool bypassCache)
        {
            if (_ccdTypes == null)
            {
                _ccdTypes = await _referenceDataRepository.GetCcdTypeAsync(bypassCache);
            }
            return _ccdTypes;
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
        /// AcademicCredentials
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

        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all person-external-education-credentials
        /// </summary>
        /// <returns>Collection of PersonExternalEducationCredentials DTO objects</returns>
        public async Task<Tuple<IEnumerable<PersonExternalEducationCredentials>, int>> GetPersonExternalEducationCredentialsAsync(int offset, int limit, string personFilter,
            Dtos.PersonExternalEducationCredentials personExternalEducationCredentialsFilter = null,
            string personGuid = "", bool bypassCache = false)
        {
            var personExternEducationCredentialsCollection = new List<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials>();
            int totalCount = 0;
            List<PersonExternalEducationCredentials> dtos = new List<PersonExternalEducationCredentials>();

            ViewPersonExternalEducationCredentialsPermission();

            string[] filterPersonIds = null;
            var personId = string.Empty;
            var externalEducationId = string.Empty;
            try
            {
                #region person filter named query.
          
                if (!string.IsNullOrEmpty(personFilter))
                {
                    var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                    if (personFilterKeys != null && personFilterKeys.Any())
                    {
                        filterPersonIds = personFilterKeys;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Dtos.PersonExternalEducationCredentials>, int>(new List<Dtos.PersonExternalEducationCredentials>(), 0);
                    }
                }

                #endregion

                #region person GUID filter
               
                if (!string.IsNullOrEmpty(personGuid))
                {
                    personId = await _personRepository.GetPersonIdForNonCorpOnly(personGuid);
                    if (string.IsNullOrEmpty(personId))
                    {
                        return new Tuple<IEnumerable<Dtos.PersonExternalEducationCredentials>, int>(new List<Dtos.PersonExternalEducationCredentials>(), 0);
                    }
                }

                #endregion

                #region external Education filter

                if (personExternalEducationCredentialsFilter != null)
                {
                    if (personExternalEducationCredentialsFilter.ExternalEducation != null && !string.IsNullOrEmpty(personExternalEducationCredentialsFilter.ExternalEducation.Id))
                    {
                        externalEducationId = await _personExternalEducationCredentialsRepository.GetExternalEducationIdFromGuidAsync(personExternalEducationCredentialsFilter.ExternalEducation.Id);
                        if (string.IsNullOrEmpty(externalEducationId))
                        {
                            return new Tuple<IEnumerable<Dtos.PersonExternalEducationCredentials>, int>(new List<Dtos.PersonExternalEducationCredentials>(), 0);
                        }
                    }
                }

                #endregion
            }
            catch (Exception)
            {
                return new Tuple<IEnumerable<Dtos.PersonExternalEducationCredentials>, int>(new List<Dtos.PersonExternalEducationCredentials>(), 0);
            }

            var entities = new Tuple<IEnumerable<Domain.Base.Entities.ExternalEducation>, int>(null, 0);
            try
            {
                entities = await _personExternalEducationCredentialsRepository.GetExternalEducationCredentialsAsync(offset, limit, filterPersonIds, personFilter, 
                   personId, externalEducationId, bypassCache);
                if (entities == null || !entities.Item1.Any())
                {
                    return new Tuple<IEnumerable<Dtos.PersonExternalEducationCredentials>, int>(new List<Dtos.PersonExternalEducationCredentials>(), 0);
                }
            }
            catch (RepositoryException ex)
            {
                foreach (var error in ex.Errors)
                {
                    IntegrationApiExceptionAddError(error.Message, error.Code, error.Id, error.SourceId);
                }
                throw IntegrationApiException;
            }

            totalCount = entities.Item2;
            foreach (var entity in entities.Item1)
            {
                try
                {
                    dtos.Add(await ConvertPersonExternalEducationCredentialsEntityToDtoAsync(entity, bypassCache));
                }
                catch (RepositoryException ex)
                {
                    foreach (var error in ex.Errors)
                    {
                        IntegrationApiExceptionAddError(error.Message, error.Code, error.Id, error.SourceId);
                    }
                }
                catch (IntegrationApiException)
                {
                    // Continue processing each record and return error collection
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Concat(ex.Message, ": ", ex.InnerException), "Validation.Exception", entity.Guid, entity.Id);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return dtos.Any() ? new Tuple<IEnumerable<PersonExternalEducationCredentials>, int>(dtos, totalCount) :
                new Tuple<IEnumerable<PersonExternalEducationCredentials>, int>(new List<PersonExternalEducationCredentials>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonExternalEducationCredentials from its GUID
        /// </summary>
        /// <returns>PersonExternalEducationCredentials DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials> GetPersonExternalEducationCredentialsByGuidAsync(string guid, bool bypassCache = false)
        {

            ViewPersonExternalEducationCredentialsPermission();

            try
            {
                var externalEducationEntity = await _personExternalEducationCredentialsRepository.GetExternalEducationCredentialsByGuidAsync(guid);

                return await ConvertPersonExternalEducationCredentialsEntityToDtoAsync(externalEducationEntity, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No person-external-education-credentials was found for guid '{0}'.",  guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No person-external-education-credentials was found for guid '{0}'.",  guid), ex);
            }
            catch (RepositoryException ex)
            {
                foreach (var error in ex.Errors)
                {
                    IntegrationApiExceptionAddError(error.Message, error.Code, error.Id, error.SourceId);
                }
                throw IntegrationApiException;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Concat(ex.Message, ": ", ex.InnerException));
                throw IntegrationApiException;
            }
        }

                /// <summary>
        /// Update a PersonExternalEducationCredentials.
        /// </summary>
        /// <param name="PersonExternalEducationCredentials">The <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see></returns>
        public async Task<PersonExternalEducationCredentials> UpdatePersonExternalEducationCredentialsAsync(PersonExternalEducationCredentials personExternalEducationCredentials)
        {
            if (personExternalEducationCredentials == null)
                throw new ArgumentNullException("PersonExternalEducationCredentials", "Must provide a PersonExternalEducationCredentials for update.");
            if (string.IsNullOrEmpty(personExternalEducationCredentials.Id))
                throw new ArgumentNullException("PersonExternalEducationCredentials", "Must provide a guid for PersonExternalEducationCredentials update.");

            string entityId = string.Empty;
            try
            {
                // get the ID associated with the incoming guid
                entityId = await _personExternalEducationCredentialsRepository.GetExternalEducationCredentialsIdFromGuidAsync(personExternalEducationCredentials.Id);
            }
            catch (KeyNotFoundException)
            {
                // Continue creating a new record using the GUID
            }
            catch (RepositoryException ex)
            {
                foreach (var error in ex.Errors)
                {
                    IntegrationApiExceptionAddError(error.Message, error.Code, error.Id, error.SourceId, httpStatusCode: System.Net.HttpStatusCode.NotFound);
                }
                throw IntegrationApiException;
            }
            if (!string.IsNullOrEmpty(entityId))
            {
                // verify the user has the permission to update a personExternalEducationCredentials
                UpdateCreatePersonExternalEducationCredentialsPermission();

                _personExternalEducationCredentialsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                
                try
                {
                    // map the DTO to entities
                    var personExternalEducationCredentialsEntity
                    = await ConvertPersonExternalEducationCredentialsDtoToEntityAsync(entityId, personExternalEducationCredentials);

                    // update the entity in the database
                    var updatedPersonExternalEducationCredentialsEntity =
                        await _personExternalEducationCredentialsRepository.UpdateExternalEducationCredentialsAsync(personExternalEducationCredentialsEntity);

                    return await this.ConvertPersonExternalEducationCredentialsEntityToDtoAsync(updatedPersonExternalEducationCredentialsEntity, true);
                }
                catch (RepositoryException ex)
                {
                    foreach (var error in ex.Errors)
                    {
                        IntegrationApiExceptionAddError(error.Message, error.Code, error.Id, error.SourceId);
                    }
                    throw IntegrationApiException;
                }
                catch (IntegrationApiException ex)
                {
                    throw ex;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Concat(ex.Message, ": ", ex.InnerException));
                    throw IntegrationApiException;
                }
            }
            // perform a create instead
            return await CreatePersonExternalEducationCredentialsAsync(personExternalEducationCredentials);
        }

        /// <summary>
        /// Create a PersonExternalEducationCredentials.
        /// </summary>
        /// <param name="personExternalEducationCredentials">The <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see></returns>
        public async Task<PersonExternalEducationCredentials> CreatePersonExternalEducationCredentialsAsync(PersonExternalEducationCredentials personExternalEducationCredentials)
        {
            if (personExternalEducationCredentials == null)
                throw new ArgumentNullException("PersonExternalEducationCredentials", "Must provide a PersonExternalEducationCredentials for update.");
            if (string.IsNullOrEmpty(personExternalEducationCredentials.Id))
                throw new ArgumentNullException("PersonExternalEducationCredentials", "Must provide a guid for PersonExternalEducationCredentials update.");

            // verify the user has the permission to create a personExternalEducationCredentials
            UpdateCreatePersonExternalEducationCredentialsPermission();

            _personExternalEducationCredentialsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                var personExternalEducationCredentialsEntity
                         = await ConvertPersonExternalEducationCredentialsDtoToEntityAsync(string.Empty, personExternalEducationCredentials);

                // create a personExternalEducationCredentials entity in the database
                var createdPersonExternalEducationCredentials =
                    await _personExternalEducationCredentialsRepository.CreateExternalEducationCredentialsAsync(personExternalEducationCredentialsEntity);
                // return the newly created personExternalEducationCredentials
                return await this.ConvertPersonExternalEducationCredentialsEntityToDtoAsync(createdPersonExternalEducationCredentials, true);
            }
            catch (RepositoryException ex)
            {
                foreach (var error in ex.Errors)
                {
                    IntegrationApiExceptionAddError(error.Message, error.Code, error.Id, error.SourceId);
                }
                throw IntegrationApiException;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Concat(ex.Message, ": ", ex.InnerException));
                throw IntegrationApiException;
            }
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ExternalEduction domain entity to its corresponding PersonExternalEducationCredentials DTO
        /// </summary>
        /// <param name="source">AcadCredentials domain entity</param>
        /// <returns>PersonExternalEducationCredentials DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials> ConvertPersonExternalEducationCredentialsEntityToDtoAsync(Domain.Base.Entities.ExternalEducation source, bool bypassCache = false)
        {
            var personExternalEducationCredentials = new Ellucian.Colleague.Dtos.PersonExternalEducationCredentials
            {
                Id = source.Guid,
                ExternalEducation = new GuidObject2(source.InstAttendGuid)
            };
            if (!string.IsNullOrEmpty(source.AcadDegree))
            {
                var degreeEntity = (await AcademicCredentialsAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(source.AcadDegree, StringComparison.OrdinalIgnoreCase));
                if (degreeEntity == null || string.IsNullOrEmpty(degreeEntity.Guid))
                {
                    IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for Degree code of '{0}'.", source.AcadDegree), "Validation.Exception", source.Guid, source.Id);
                }
                else
                {
                    personExternalEducationCredentials.Credential = new GuidObject2(degreeEntity.Guid);
                    if (source.AcadDegreeDate != null && source.AcadDegreeDate.HasValue)
                        personExternalEducationCredentials.EarnedOn = source.AcadDegreeDate;
                    if (source.AcadCcd != null && source.AcadCcd.Any())
                    {
                        personExternalEducationCredentials.SupplementalCredentials = new List<PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty>();
                        int index = 0;
                        foreach (var ccd in source.AcadCcd)
                        {
                            var ccdEntity = (await AcademicCredentialsAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(ccd, StringComparison.OrdinalIgnoreCase));
                            if (ccdEntity == null || string.IsNullOrEmpty(ccdEntity.Guid))
                            {
                                IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for CCD code of '{0}'.", ccd), "Validation.Exception", source.Guid, source.Id);
                            }
                            else
                            {
                                var suppliment = new PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty()
                                {
                                    Credential = new GuidObject2(ccdEntity.Guid)
                                };
                                if (source.AcadCcdDate != null && source.AcadCcdDate.Count() > index && source.AcadCcdDate.ElementAt(index).HasValue)
                                    suppliment.EarnedOn = source.AcadCcdDate.ElementAt(index);

                                personExternalEducationCredentials.SupplementalCredentials.Add(suppliment);
                            }
                            index++;
                        }
                    }
                }
            }
            else
            {
                if (source.AcadCcd == null || !source.AcadCcd.Any())
                {
                    IntegrationApiExceptionAddError("Credential must have either a Degree or CCD.", "Missing.Required.Property", source.Guid, source.Id);
                }
                else
                {
                    var ccdDictionary = await FindCredentials(source, bypassCache);
                    int index = 0;
                    foreach (var dict in ccdDictionary)
                    {
                        if (index == 0)
                        {
                            personExternalEducationCredentials.Credential = new GuidObject2(dict.Key);
                            personExternalEducationCredentials.EarnedOn = dict.Value;
                        }
                        else
                        {
                            var suppliment = new PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty()
                            {
                                Credential = new GuidObject2(dict.Key),
                                EarnedOn = dict.Value
                            };
                        }
                        index++;
                    }
                }
            }
            if ((source.AcadMajors != null && source.AcadMajors.Any()) || (source.AcadMinors != null && source.AcadMinors.Any()) || (source.AcadSpecialization != null && source.AcadSpecialization.Any()))
            {
                personExternalEducationCredentials.Disciplines = new List<GuidObject2>();
                if (source.AcadMajors != null && source.AcadMajors.Any())
                {
                    foreach (var major in source.AcadMajors)
                    {
                        var disciplineEntity = (await AcademicDisciplinesAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(major, StringComparison.OrdinalIgnoreCase) && ac.AcademicDisciplineType == Domain.Base.Entities.AcademicDisciplineType.Major);
                        if (disciplineEntity == null || string.IsNullOrEmpty(disciplineEntity.Guid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for Major code of '{0}'.", major), "Validation.Exception", source.Guid, source.Id);
                        }
                        else
                        {
                            personExternalEducationCredentials.Disciplines.Add(new GuidObject2(disciplineEntity.Guid));
                        }
                    }
                }
                if (source.AcadMinors != null && source.AcadMinors.Any())
                {
                    foreach (var minor in source.AcadMinors)
                    {
                        var disciplineEntity = (await AcademicDisciplinesAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(minor, StringComparison.OrdinalIgnoreCase) && ac.AcademicDisciplineType == Domain.Base.Entities.AcademicDisciplineType.Minor);
                        if (disciplineEntity == null || string.IsNullOrEmpty(disciplineEntity.Guid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for Minor code of '{0}'.", minor), "Validation.Exception", source.Guid, source.Id);
                        }
                        else
                        {
                            personExternalEducationCredentials.Disciplines.Add(new GuidObject2(disciplineEntity.Guid));
                        }
                    }
                }
                if (source.AcadSpecialization != null && source.AcadSpecialization.Any())
                {
                    foreach (var special in source.AcadSpecialization)
                    {
                        var disciplineEntity = (await AcademicDisciplinesAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(special, StringComparison.OrdinalIgnoreCase) && ac.AcademicDisciplineType == Domain.Base.Entities.AcademicDisciplineType.Concentration);
                        if (disciplineEntity == null || string.IsNullOrEmpty(disciplineEntity.Guid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for Specilization code of '{0}'.", special), "Validation.Exception", source.Guid, source.Id);
                        }
                        else
                        {
                            personExternalEducationCredentials.Disciplines.Add(new GuidObject2(disciplineEntity.Guid));
                        }
                    }
                }
            }
            if ((source.AcadStartDate != null && source.AcadStartDate.HasValue) || (source.AcadEndDate != null && source.AcadEndDate.HasValue))
            {
                personExternalEducationCredentials.AttendancePeriods = new List<PersonExternalEducationCredentialsAttendanceperiodsDtoProperty>()
                {
                    new PersonExternalEducationCredentialsAttendanceperiodsDtoProperty()
                    {
                        StartOn = source.AcadStartDate,
                        EndOn = source.AcadEndDate
                    }
                };
            }
            if (source.AcadHonors != null && source.AcadHonors.Any())
            {
                personExternalEducationCredentials.Recognitions = new List<GuidObject2>();
                foreach (var honor in source.AcadHonors)
                {
                    //var honorEntity = (await AcademicHonorsAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(honor, StringComparison.OrdinalIgnoreCase));
                    var honorEntityGuid = string.Empty;
                    try
                    {
                        honorEntityGuid = await _referenceDataRepository.GetOtherHonorsGuidAsync(honor);


                        if (string.IsNullOrEmpty(honorEntityGuid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for Honor code of '{0}'.", honor), "Validation.Exception", source.Guid, source.Id);
                        }
                        else
                        {
                            personExternalEducationCredentials.Recognitions.Add(new GuidObject2(honorEntityGuid));
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "Validation.Exception", source.Guid, source.Id);
                    }
                }
            }
            if (source.AcadGpa != null && source.AcadGpa.HasValue)
            {
                personExternalEducationCredentials.PerformanceMeasure = source.AcadGpa.ToString();
            }
            if (source.AcadRankDenominator != null && source.AcadRankDenominator.HasValue)
            {
                personExternalEducationCredentials.ClassSize = source.AcadRankDenominator;
            }
            if (source.AcadRankPercent != null && source.AcadRankPercent.HasValue)
            {
                personExternalEducationCredentials.ClassPercentile = source.AcadRankPercent;
            }
            if (source.AcadRankNumerator != null && source.AcadRankNumerator.HasValue)
            {
                personExternalEducationCredentials.ClassRank = source.AcadRankNumerator;
            }
            if (source.AcadThesis != null && !string.IsNullOrEmpty(source.AcadThesis))
            {
                personExternalEducationCredentials.ThesisTitle = source.AcadThesis;
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return personExternalEducationCredentials;
        }

        private async Task<Domain.Base.Entities.ExternalEducation> ConvertPersonExternalEducationCredentialsDtoToEntityAsync(string id, PersonExternalEducationCredentials personExternalEducationCredentialsDto)
        {
            string guid = personExternalEducationCredentialsDto.Id;
            Domain.Base.Entities.InstType instType = Domain.Base.Entities.InstType.Unknown;

            var externalEducationEntity = new Colleague.Domain.Base.Entities.ExternalEducation(personExternalEducationCredentialsDto.Id);
            
            // External Education ID (personId*institutionsId)
            if (personExternalEducationCredentialsDto.ExternalEducation != null && !string.IsNullOrEmpty(personExternalEducationCredentialsDto.ExternalEducation.Id))
            {
                try
                {
                    var externalEducationId = await _personExternalEducationCredentialsRepository.GetExternalEducationIdFromGuidAsync(personExternalEducationCredentialsDto.ExternalEducation.Id);

                    if (!string.IsNullOrEmpty(externalEducationId))
                    {
                        var personId = externalEducationId.Split('*')[0];
                        var institutionId = externalEducationId.Split('*')[1];
                        externalEducationEntity.AcadPersonId = personId;
                        externalEducationEntity.AcadInstitutionsId = institutionId;

                        var institution = await _institutionRepository.GetInstitutionAsync(institutionId);
                        instType = institution.InstitutionType;
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Could not determine person and institution ID from externalEducation GUID '{0}'.", personExternalEducationCredentialsDto.ExternalEducation.Id), "Validation.Exception", guid, id);
                }
            }
            else
            {
                IntegrationApiExceptionAddError("externalEducation is a required property.", "Missing.Required.Property", guid, id);
            }

            // Primary Credential
            if (personExternalEducationCredentialsDto.Credential != null && !string.IsNullOrEmpty(personExternalEducationCredentialsDto.Credential.Id))
            {
                var credentialEntity = (await AcademicCredentialsAsync(false)).FirstOrDefault(ac => ac.Guid.Equals(personExternalEducationCredentialsDto.Credential.Id, StringComparison.InvariantCultureIgnoreCase));
                if (credentialEntity != null)
                {
                    if (credentialEntity.AcademicCredentialType == Domain.Base.Entities.AcademicCredentialType.Degree)
                    {
                        externalEducationEntity.AcadDegree = credentialEntity.Code;
                        externalEducationEntity.AcadDegreeDate = personExternalEducationCredentialsDto.EarnedOn;
                    }
                    else
                    {
                        if (credentialEntity.AcademicCredentialType == Domain.Base.Entities.AcademicCredentialType.Certificate)
                        {
                            if (instType == Domain.Base.Entities.InstType.HighSchool && (personExternalEducationCredentialsDto.EarnedOn == null || !personExternalEducationCredentialsDto.EarnedOn.HasValue))
                            {
                                IntegrationApiExceptionAddError("The Earned On date is required for the High School credential.", "Missing.Required.Property", guid, id);
                            }
                            if (externalEducationEntity.AcadCcd == null)
                            {
                                externalEducationEntity.AcadCcd = new List<string>();
                            }
                            if (externalEducationEntity.AcadCcdDate == null)
                            {
                                externalEducationEntity.AcadCcdDate = new List<DateTime?>();
                            }
                            externalEducationEntity.AcadCcd.Add(credentialEntity.Code);
                            externalEducationEntity.AcadCcdDate.Add(personExternalEducationCredentialsDto.EarnedOn);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Credential Type '{0}' is not supported as a primary credential.", credentialEntity.AcademicCredentialType.ToString()), "Validation.Exception", guid, id);
                        }
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("Cannot find a credential for GUID '{0}'.", personExternalEducationCredentialsDto.Credential.Id), "Validation.Exception", guid, id);
                }
            }
            else
            {
                IntegrationApiExceptionAddError("The credential property is required.", "Missing.Required.Property", guid, id);
            }

            // Suplimental Credentials (CCDs)
            if (personExternalEducationCredentialsDto.SupplementalCredentials != null && personExternalEducationCredentialsDto.SupplementalCredentials.Any())
            {
                foreach (var supplementalCred in personExternalEducationCredentialsDto.SupplementalCredentials)
                {
                    if (supplementalCred != null && supplementalCred.Credential != null && !string.IsNullOrEmpty(supplementalCred.Credential.Id))
                    {
                        var credentialEntity = (await AcademicCredentialsAsync(false)).FirstOrDefault(ac => ac.Guid.Equals(supplementalCred.Credential.Id, StringComparison.InvariantCultureIgnoreCase));
                        if (credentialEntity != null)
                        {
                            if (credentialEntity.AcademicCredentialType == Domain.Base.Entities.AcademicCredentialType.Certificate)
                            {
                                if (instType == Domain.Base.Entities.InstType.HighSchool && (supplementalCred.EarnedOn == null || !supplementalCred.EarnedOn.HasValue))
                                {
                                    IntegrationApiExceptionAddError("Supplemental Credentials require an EarnedOn date for High Schools.", "Missing.Required.Property", guid, id);
                                }
                                else
                                {
                                    if (externalEducationEntity.AcadCcd == null)
                                    {
                                        externalEducationEntity.AcadCcd = new List<string>();
                                    }
                                    if (externalEducationEntity.AcadCcdDate == null)
                                    {
                                        externalEducationEntity.AcadCcdDate = new List<DateTime?>();
                                    }
                                    externalEducationEntity.AcadCcd.Add(credentialEntity.Code);
                                    externalEducationEntity.AcadCcdDate.Add(supplementalCred.EarnedOn);
                                }
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Format("The Credential '{0}' is not valid for Supplemental Credentials.", supplementalCred.Credential.Id), "Validation.Exception", guid, id);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot find a credential for GUID '{0}'.", supplementalCred.Credential.Id), "Validation.Exception", guid, id);
                        }
                    }
                }
            }

            // Academic Disciplines (Majors, Minors, Specializations)
            if (personExternalEducationCredentialsDto.Disciplines != null && personExternalEducationCredentialsDto.Disciplines.Any())
            {
                foreach (var discipline in personExternalEducationCredentialsDto.Disciplines)
                {
                    if (discipline != null && !string.IsNullOrEmpty(discipline.Id))
                    {
                        var disciplineEntity = (await AcademicDisciplinesAsync(false)).FirstOrDefault(ad => ad.Guid.Equals(discipline.Id, StringComparison.InvariantCultureIgnoreCase));
                        if (disciplineEntity != null)
                        {
                            switch (disciplineEntity.AcademicDisciplineType)
                            {
                                case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                                    {
                                        if (externalEducationEntity.AcadSpecialization == null)
                                        {
                                            externalEducationEntity.AcadSpecialization = new List<string>();
                                        }
                                        externalEducationEntity.AcadSpecialization.Add(disciplineEntity.Code);
                                        break;
                                    }
                                case Domain.Base.Entities.AcademicDisciplineType.Major:
                                    {
                                        if (instType == Domain.Base.Entities.InstType.HighSchool)
                                        {
                                            IntegrationApiExceptionAddError(string.Format("The Academic Discipline '{0}' is not allowed for a high school.", discipline.Id), "Validation.Exception", guid, id);
                                        }
                                        if (externalEducationEntity.AcadMajors == null)
                                        {
                                            externalEducationEntity.AcadMajors = new List<string>();
                                        }
                                        externalEducationEntity.AcadMajors.Add(disciplineEntity.Code);
                                        break;
                                    }
                                case Domain.Base.Entities.AcademicDisciplineType.Minor:
                                    {
                                        if (instType == Domain.Base.Entities.InstType.HighSchool)
                                        {
                                            IntegrationApiExceptionAddError(string.Format("The Academic Discipline '{0}' is not allowed for a high school.", discipline.Id), "Validation.Exception", guid, id);
                                        }
                                        if (externalEducationEntity.AcadMinors == null)
                                        {
                                            externalEducationEntity.AcadMinors = new List<string>();
                                        }
                                        externalEducationEntity.AcadMinors.Add(disciplineEntity.Code);
                                        break;
                                    }
                                default:
                                    {
                                        IntegrationApiExceptionAddError(string.Format("Academic discipline for GUID '{0}' has an invalid type of '{1}'.", discipline.Id, disciplineEntity.AcademicDisciplineType.ToString()), "Validation.Exception", guid, id);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot find an academic discipline for GUID '{0}'.", discipline.Id), "Validation.Exception", guid, id);
                        }
                    }
                }
            }

            // Attendance Periods
            if (personExternalEducationCredentialsDto.AttendancePeriods != null && personExternalEducationCredentialsDto.AttendancePeriods.Any())
            {
                if (personExternalEducationCredentialsDto.AttendancePeriods.Count() > 1)
                {
                    IntegrationApiExceptionAddError("Colleague does not support more than one set of Attendance Periods for a person-external-education-credentials.", "Validation.Exception", guid, id);
                }

                var attendPeriod = personExternalEducationCredentialsDto.AttendancePeriods.FirstOrDefault();
                if (attendPeriod != null)
                {
                    if (attendPeriod.StartOn != null && attendPeriod.EndOn != null && attendPeriod.StartOn > attendPeriod.EndOn)
                    {
                        IntegrationApiExceptionAddError("The Start On date cannot be greater than the End On date.", "Validation.Exception", guid, id);
                    }
                    // Start On
                    externalEducationEntity.AcadStartDate = attendPeriod.StartOn;
                    // End On
                    externalEducationEntity.AcadEndDate = attendPeriod.EndOn;
                }
            }

            // Performance Measure
            if (personExternalEducationCredentialsDto.PerformanceMeasure != null && !string.IsNullOrEmpty(personExternalEducationCredentialsDto.PerformanceMeasure))
            {
                if (instType.Equals(Domain.Base.Entities.InstType.HighSchool))
                {
                    IntegrationApiExceptionAddError("Performance Measure is maintained at the person-external-education level for High Schools.", "Validation.Exception", guid, id);
                }
                try
                {
                    if (Decimal.Parse(personExternalEducationCredentialsDto.PerformanceMeasure) > Decimal.Parse("999.999"))
                    {
                        IntegrationApiExceptionAddError("The Performance Measure is not a valid value.", "Validation.Exception", guid, id);
                    }
                    externalEducationEntity.AcadGpa = Decimal.Parse(personExternalEducationCredentialsDto.PerformanceMeasure);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError("The Performance Measure is not a valid value.", "Validation.Exception", guid, id);
                }
            }

            // Recognitions (Honors)
            if (personExternalEducationCredentialsDto.Recognitions != null && personExternalEducationCredentialsDto.Recognitions.Any())
            {
                foreach (var recognition in personExternalEducationCredentialsDto.Recognitions)
                {
                    if (recognition != null && !string.IsNullOrEmpty(recognition.Id))
                    {
                        var honorsEntity = (await AcademicHonorsAsync(false)).FirstOrDefault(ah => ah.Guid.Equals(recognition.Id, StringComparison.InvariantCultureIgnoreCase));
                        if (honorsEntity != null)
                        {
                            if (externalEducationEntity.AcadHonors == null)
                            {
                                externalEducationEntity.AcadHonors = new List<string>();
                            }
                            externalEducationEntity.AcadHonors.Add(honorsEntity.Code);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot find an academic honors recognition for GUID '{0}'.", recognition.Id), "Validation.Exception", guid, id);
                        }
                    }
                }
            }

            // Validate Class Size, Rank, and Percentage
            if ((personExternalEducationCredentialsDto.ClassSize != null && personExternalEducationCredentialsDto.ClassSize.HasValue)
                || (personExternalEducationCredentialsDto.ClassRank != null && personExternalEducationCredentialsDto.ClassRank.HasValue)
                || (personExternalEducationCredentialsDto.ClassPercentile != null && personExternalEducationCredentialsDto.ClassPercentile.HasValue))
            {
                if (instType == Domain.Base.Entities.InstType.HighSchool)
                {
                    IntegrationApiExceptionAddError("Class Size, Class Rank, and Class Percentile are maintained at the person-external-education level for High Schools.", "Validation.Exception", guid, id);
                }

                var classSize = personExternalEducationCredentialsDto.ClassSize;
                var classRank = personExternalEducationCredentialsDto.ClassRank;
                var classPercent = personExternalEducationCredentialsDto.ClassPercentile;

                if (classSize != null && classSize.HasValue && (classRank == null || !classRank.HasValue))
                {
                    IntegrationApiExceptionAddError("Class Rank is required when Class Size is provided.", "Validation.Exception", guid, id);
                }
                if (classRank != null && classRank.HasValue && (classSize == null || !classSize.HasValue))
                {
                    IntegrationApiExceptionAddError("Class Size is required when Class Rank is provided.", "Validation.Exception", guid, id);
                }
                if (classSize != null && classSize.HasValue && classRank != null && classRank.HasValue)
                {
                    if (classRank > classSize)
                    {
                        IntegrationApiExceptionAddError("Class Rank cannot be greater than Class Size.", "Validation.Exception", guid, id);
                    }
                    if (classSize > 9999999)
                    {
                        IntegrationApiExceptionAddError("The Class Size is not a valid value.", "Validation.Exception", guid, id);
                    }
                    if (classRank > 9999999)
                    {
                        IntegrationApiExceptionAddError("The Class Rank is not a valid value.", "Validation.Exception", guid, id);
                    }
                }

                // Class Size
                if (classSize != null && classSize.HasValue)
                {
                    externalEducationEntity.AcadRankDenominator = classSize;
                } 

                // Class Rank
                if (classRank != null && classRank.HasValue)
                {
                    externalEducationEntity.AcadRankNumerator = classRank;
                }

                // Class Percent
                if (classPercent != null && classPercent.HasValue && classPercent.Value > 0)
                {
                    if (classPercent > 100)
                    {
                        IntegrationApiExceptionAddError("The Class Percentile is not a valid value.", "Validation.Exception", guid, id);
                    }
                    externalEducationEntity.AcadRankPercent = classPercent;
                }
                else
                {
                    if (classRank != null && classRank > 0 && classSize != null && classSize > 0)
                    {
                        try
                        {
                            // Calculate classPercentage
                            classPercent = (Convert.ToDecimal(classRank) / Convert.ToDecimal(classSize)) * 100;
                            if (classPercent > 100)
                            {
                                IntegrationApiExceptionAddError(string.Format("The calculated Class Percentile '{0}' is not a valid value.", classPercent), "Validation.Exception", guid, id);
                            }
                            externalEducationEntity.AcadRankPercent = classPercent;
                        }
                        catch (Exception)
                        {
                            IntegrationApiExceptionAddError("The Class Percentile is not a valid value.", "Validation.Exception", guid, id);
                        }
                    }
                }
            }

            // Thesis
            if (personExternalEducationCredentialsDto.ThesisTitle != null && !string.IsNullOrEmpty(personExternalEducationCredentialsDto.ThesisTitle))
            {
                if (instType.Equals(Domain.Base.Entities.InstType.HighSchool))
                {
                    IntegrationApiExceptionAddError("Thesis title is not allowed for High Schools.", "Validation.Exception", guid, id);
                }
                externalEducationEntity.AcadThesis = personExternalEducationCredentialsDto.ThesisTitle;
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return externalEducationEntity;
        }

        private async Task<Dictionary<string, DateTime?>> FindCredentials(Domain.Base.Entities.ExternalEducation source, bool bypassCache = false)
        {
            Dictionary<string, DateTime?> ccdDictionary = new Dictionary<string, DateTime?>();

            string primaryCredential = string.Empty;
            DateTime? primaryEarnedOn = null;
            int primaryLevel = 0;

            int index = 0;
            foreach (var ccd in source.AcadCcd)
            {
                var ccdEntity = (await OtherCcdsAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(ccd, StringComparison.OrdinalIgnoreCase));
                if (ccdEntity == null || string.IsNullOrEmpty(ccdEntity.Guid))
                {
                    IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for CCD code of '{0}'.", ccd), "Validation.Exception", source.Guid, source.Id);
                }
                else
                {
                    int credentialLevel = 0;
                    if (!string.IsNullOrEmpty(ccdEntity.CredentialTypeID))
                    {
                        var ccdTypeEntity = (await CcdTypesAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(ccdEntity.CredentialTypeID, StringComparison.OrdinalIgnoreCase));
                        if (ccdTypeEntity == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("The CCD Type code of '{0}' is missing from the database.", ccdEntity.CredentialTypeID), "Validation.Exception", source.Guid, source.Id);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ccdTypeEntity.CredentialLevel))
                            {
                                try
                                {
                                    credentialLevel = int.Parse(ccdTypeEntity.CredentialLevel);
                                }
                                catch
                                {
                                    credentialLevel = 0;
                                }
                            }
                        }
                    }
                    var earnedOn = (source.AcadCcdDate != null && source.AcadCcdDate.Count() > index && source.AcadCcdDate.ElementAt(index).HasValue) ?
                        source.AcadCcdDate.ElementAt(index) : null;

                    if (credentialLevel > primaryLevel || index == 0)
                    {
                        primaryCredential = ccdEntity.Guid;
                        primaryEarnedOn = earnedOn;
                    }
                }
                index++;
            }
            if (!string.IsNullOrEmpty(primaryCredential))
            {
                ccdDictionary.Add(primaryCredential, primaryEarnedOn);
            }

            index = 0;
            foreach (var ccd in source.AcadCcd)
            {
                var ccdEntity = (await OtherCcdsAsync(bypassCache)).FirstOrDefault(ac => ac.Code.Equals(ccd, StringComparison.OrdinalIgnoreCase));
                if (ccdEntity == null || string.IsNullOrEmpty(ccdEntity.Guid))
                {
                    IntegrationApiExceptionAddError(string.Format("Cannot find a GUID for CCD code of '{0}'.", ccd), "Validation.Exception", source.Guid, source.Id);
                }
                else
                {
                    var earnedOn = (source.AcadCcdDate != null && source.AcadCcdDate.Count() > index && source.AcadCcdDate.ElementAt(index).HasValue) ?
                        source.AcadCcdDate.ElementAt(index) : null;

                    if (!ccdDictionary.ContainsKey(ccdEntity.Guid))
                    {
                       ccdDictionary.Add(ccdEntity.Guid, earnedOn);
                    }
                }
                index++;
            }

            return ccdDictionary;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update PersonExternalEducationCredentials.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void ViewPersonExternalEducationCredentialsPermission()
        {
            bool hasPermission = (HasPermission(BasePermissionCodes.ViewPersonExternalEducationCredentials) || HasPermission(BasePermissionCodes.UpdatePersonExternalEducationCredentials));

            // User is not allowed to create or update PersonExternalEducationCredentials without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view person-external-education-credentials.");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update PersonExternalEducationCredentials.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void UpdateCreatePersonExternalEducationCredentialsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.UpdatePersonExternalEducationCredentials);

            // User is not allowed to create or update PersonExternalEducationCredentials without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update person-external-education-credentials.");
            }
        }
   }
}