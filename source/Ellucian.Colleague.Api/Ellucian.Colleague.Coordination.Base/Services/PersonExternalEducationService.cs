// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PersonExternalEducationService : BaseCoordinationService, IPersonExternalEducationService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IInstitutionsAttendRepository _institutionsAttendRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger _logger;


        public PersonExternalEducationService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            IInstitutionsAttendRepository institutionsAttendRepository, IReferenceDataRepository referenceDataRepository, IPersonRepository personRepository,
            IInstitutionRepository institutionRepository, IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _institutionsAttendRepository = institutionsAttendRepository;
            _personRepository = personRepository;
            _institutionRepository = institutionRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Gets all PersonExternalEducation
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="personExternalEducationFilter">filter criteria</param>
        /// <param name="personFilterGuid">person Filter - Guid for SAVE.LIST.PARMS which contains a savedlist of person IDs</param>
        /// <param name="personByInstitutionType">filter to retrieve information for a specific person at institution's of a specific type</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonExternalEducation">personExternalEducation</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>> GetPersonExternalEducationAsync(int offset, int limit,
            Ellucian.Colleague.Dtos.PersonExternalEducation personExternalEducationFilter = null, string personFilterGuid = "",
            PersonByInstitutionType personByInstitutionType = null, bool bypassCache = false)
        {

            var externalEducationsCollection = new List<Dtos.PersonExternalEducation>();
           
            #region filters
            
            var filterPersonIds = new List<string>().ToArray();
            var personId = string.Empty;
            var personByInstitutionTypePersonId = string.Empty;
            InstType? typeFilter = null;
            
            try
            {
                #region person id filter
                if (personExternalEducationFilter != null)
                {
                    if (personExternalEducationFilter.Person != null && !string.IsNullOrEmpty(personExternalEducationFilter.Person.Id))
                    {
                        personId = await _personRepository.GetPersonIdFromGuidAsync(personExternalEducationFilter.Person.Id);
                        if (string.IsNullOrEmpty(personId))
                        {
                            return new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(new List<Dtos.PersonExternalEducation>(), 0);
                        }
                    }
                }
                #endregion

                #region personByInstitutionType named query
                if (personByInstitutionType != null)
                {
                    if (((personByInstitutionType.Person != null  && !string.IsNullOrEmpty(personByInstitutionType.Person.Id))
                        && (personByInstitutionType.Type == null))
                        ||
                        ((personByInstitutionType.Person == null || string.IsNullOrEmpty(personByInstitutionType.Person.Id))
                        && (personByInstitutionType.Type != null)))
                    {
                        return new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(new List<Dtos.PersonExternalEducation>(), 0);

                    }

                    if (personByInstitutionType.Person != null && !string.IsNullOrEmpty(personByInstitutionType.Person.Id))
                    {
                        personByInstitutionTypePersonId = await _personRepository.GetPersonIdFromGuidAsync(personByInstitutionType.Person.Id);
                        if (string.IsNullOrEmpty(personByInstitutionTypePersonId))
                        {
                            return new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(new List<Dtos.PersonExternalEducation>(), 0);
                        }
                    }
                    if (personByInstitutionType.Type != null)
                    {
                        switch (personByInstitutionType.Type)
                        {
                            case EducationalInstitutionType.PostSecondarySchool:
                                typeFilter = InstType.College;
                                break;
                            case EducationalInstitutionType.SecondarySchool:
                                typeFilter = InstType.HighSchool;
                                break;
                        }
                    }
                }
                #endregion 

                #region personFilter named query
                if (!string.IsNullOrEmpty(personFilterGuid))
                {
                    var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilterGuid));
                    if (personFilterKeys != null)
                    {
                        filterPersonIds = personFilterKeys;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(new List<Dtos.PersonExternalEducation>(), 0);
                    }
                }
                #endregion
            }
            catch (Exception)
            {
                return new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(new List<Dtos.PersonExternalEducation>(), 0);
            }
            #endregion

            #region retrieve data
            Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int> institutionAttendEntitiesTuple = null;
            try
            {
                institutionAttendEntitiesTuple = await _institutionsAttendRepository.GetInstitutionsAttendAsync(offset, limit, personId, filterPersonIds, personFilterGuid,
                    personByInstitutionTypePersonId, typeFilter, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            if (institutionAttendEntitiesTuple == null)
            {
                return new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(new List<Dtos.PersonExternalEducation>(), 0);

            }
            #endregion

            #region build response
            var institutionAttendEntities = institutionAttendEntitiesTuple.Item1;
            var totalCount = institutionAttendEntitiesTuple.Item2;

            if (institutionAttendEntities != null && institutionAttendEntities.Any())
            {
                var ids = new List<string>();
                ids.AddRange(institutionAttendEntities.Where(x => (!string.IsNullOrEmpty(x.PersonId))).Select(x => x.PersonId).Distinct().ToList());
                ids.AddRange(institutionAttendEntities.Where(y => (!string.IsNullOrEmpty(y.InstitutionId))).Select(y => y.InstitutionId).Distinct().ToList());
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

                foreach (var acadCred in institutionAttendEntities)
                {
                    externalEducationsCollection.Add(ConvertInstitutionAttendEntityToDto(acadCred, personGuidCollection, bypassCache));
                }
            }
            #endregion

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(externalEducationsCollection, totalCount);
        }

        /// <summary>
        /// Get a PersonExternalEducation DTO object by guid.
        /// </summary>
        /// <param name="guid">Guid of the InstitutionsAttend in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonExternalEducation">personExternalEducation</see></returns>
        public async Task<Ellucian.Colleague.Dtos.PersonExternalEducation> GetPersonExternalEducationByGuidAsync(string guid, bool bypassCache = true)
        {

           string institutionAttendId = "";
            try
            {
                institutionAttendId = await _institutionsAttendRepository.GetInstitutionsAttendIdFromGuidAsync(guid);
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("No person external education was found for guid '" + guid + "'.");
            }

            if (string.IsNullOrEmpty(institutionAttendId))
            {
                throw new KeyNotFoundException("No person external education was found for guid '" + guid + "'.");
            }

            Domain.Base.Entities.InstitutionsAttend institutionsAttendEntity = null;
            try
            {
                institutionsAttendEntity = await _institutionsAttendRepository.GetInstitutionAttendByIdAsync(institutionAttendId);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            if (institutionsAttendEntity == null)
            {
                throw new KeyNotFoundException("No person external education was found for guid '" + guid + "'.");
            }
            var ids = new List<string>() { institutionsAttendEntity.PersonId, institutionsAttendEntity.InstitutionId };
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

            var retVal = ConvertInstitutionAttendEntityToDto(institutionsAttendEntity, personGuidCollection, bypassCache);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            if (retVal == null)
            {
                throw new KeyNotFoundException("No person external education was found for guid '" + guid + "'.");
            }
            return retVal;
        }
      
        /// <summary>
        /// CreateUpdatePersonExternalEducationAsync
        /// </summary>
        /// <param name="personExternalEducation"></param>
        /// <param name="isUpdate"></param>
        /// <returns></returns>
        public async Task<PersonExternalEducation> CreateUpdatePersonExternalEducationAsync(PersonExternalEducation personExternalEducation, bool isUpdate = true)
        {
            if (personExternalEducation == null)
            {
                IntegrationApiExceptionAddError("Request body must contain a valid personExternalEducation.");
                throw IntegrationApiException;
            }
            if (string.IsNullOrEmpty(personExternalEducation.Id))
            {
                IntegrationApiExceptionAddError("Id is a required property.");
                throw IntegrationApiException;
            }

            var personExternalEducationEntityId = string.Empty;

            // If this is an update, then get the ID associated with the incoming guid
            if (isUpdate)
            {
                try
                {
                    personExternalEducationEntityId = await _institutionsAttendRepository.GetInstitutionsAttendIdFromGuidAsync(personExternalEducation.Id);

                }
                catch (Exception ex)
                {
                    //do a create instead...
                    logger.Error(ex.Message, "Error retrieving institutions attend.");
                }
            }

            Validate(personExternalEducation);

            _institutionsAttendRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            var personExternalEducationEntity = await ConvertPersonExternalEducationDtoToEntityAsync(personExternalEducationEntityId, personExternalEducation);
            if (IntegrationApiException != null)
                throw IntegrationApiException;

            InstitutionsAttend updatedPersonExternalEducationEntity = null;
            try
            {
                updatedPersonExternalEducationEntity =
                await _institutionsAttendRepository.UpdateExternalEducationAsync(personExternalEducationEntity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            var ids = new List<string>() { updatedPersonExternalEducationEntity.PersonId, updatedPersonExternalEducationEntity.InstitutionId };
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

            var newDto = ConvertInstitutionAttendEntityToDto(updatedPersonExternalEducationEntity, personGuidCollection, true);

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return newDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an InstitutionsAttend domain entity to its corresponding PersonExternalEducation DTO
        /// </summary>
        /// <param name="institutionAttendEntity">InstitutionsAttend domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>ExternalEducation DTO</returns>
        private Dtos.PersonExternalEducation ConvertInstitutionAttendEntityToDto(InstitutionsAttend institutionAttendEntity, Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            if (institutionAttendEntity == null)
            {
                IntegrationApiExceptionAddError("InstitutionsAttend domain entity is required.");
                return null;
            }

            if (string.IsNullOrEmpty(institutionAttendEntity.Guid))
            {
                IntegrationApiExceptionAddError("InstitutionsAttend guid is null or empty.", "Missing.Request.ID",
                    id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);

                return null;
            }

            if (string.IsNullOrEmpty(institutionAttendEntity.PersonId))
            {
                IntegrationApiExceptionAddError("Person Id is null or empty.", "Missing.Required.Property", institutionAttendEntity.Guid,
                      id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);
            }

            if (string.IsNullOrEmpty(institutionAttendEntity.InstitutionId))
            {
                IntegrationApiExceptionAddError("Institution Id is null or empty.", "Missing.Required.Property", institutionAttendEntity.Guid,
                     id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);
            }

            if (personGuidCollection == null)
            {
                IntegrationApiExceptionAddError("personGuidCollection is null or empty.", "Bad.Data", institutionAttendEntity.Guid,
                     id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                return null;

            var externalEducation = new Dtos.PersonExternalEducation
            {
                Id = institutionAttendEntity.Guid,
            };

            var personGuid = string.Empty;
            personGuidCollection.TryGetValue(institutionAttendEntity.PersonId, out personGuid);

            if (string.IsNullOrEmpty(personGuid))
            {
                IntegrationApiExceptionAddError(string.Format("Person guid not found, PersonId: '{0}'", institutionAttendEntity.PersonId), "GUID.Not.Found", institutionAttendEntity.Guid,
                   id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);
            }
            externalEducation.Person = new GuidObject2(personGuid);

            var institutionGuid = string.Empty;
            personGuidCollection.TryGetValue(institutionAttendEntity.InstitutionId, out institutionGuid);

            if (string.IsNullOrEmpty(institutionGuid))
            {
                IntegrationApiExceptionAddError(string.Format("Institution guid not found, InstitutionId: '{0}", institutionAttendEntity.InstitutionId), "GUID.Not.Found", institutionAttendEntity.Guid,
                  id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);
            }

            externalEducation.Institution = new GuidObject2(institutionGuid);

            var attendancePeriods = new List<ExternalEducationAttendancePeriods>();

            // Maintain a collection of the start and end Years extracted from the dates attended.  We will use 
            // this collection to determine if the INSTA.YEAR.ATTEND.START and  INSTA.YEAR.ATTEND.END 
            // pair should be included in the payload
            var yearsCollection = new List<Tuple<int, int>>();

            if ((institutionAttendEntity.DatesAttended != null) && (institutionAttendEntity.DatesAttended.Any()))
            {
                foreach (var datesAttended in institutionAttendEntity.DatesAttended)
                {
                    var startDate = (datesAttended.Item1 != null) ? ConvertDateTimeToDateDtoProperty(datesAttended.Item1) : null;

                    var endDate = (datesAttended.Item2 != null) ? ConvertDateTimeToDateDtoProperty(datesAttended.Item2) : null;

                    if ((startDate != null) || (endDate != null))
                    {
                        int startYear = 0;
                        int endYear = 0;
                        var externalEducationAttendancePeriods = new ExternalEducationAttendancePeriods() { };
                        if (startDate != null)
                        {
                            externalEducationAttendancePeriods.StartDate = startDate;
                            startYear = startDate.Year;
                        }
                        if (endDate != null)
                        {
                            externalEducationAttendancePeriods.EndDate = endDate;
                            endYear = endDate.Year;
                        }

                        yearsCollection.Add(new Tuple<int, int>(startYear, endYear));
                        attendancePeriods.Add(externalEducationAttendancePeriods);
                    }
                }
            }
            if ((institutionAttendEntity.YearsAttended != null) && (institutionAttendEntity.YearsAttended.Any()))
            {
                foreach (var yearAttended in institutionAttendEntity.YearsAttended)
                {
                    int endYear = 0;
                    int startYear = 0;

                    if (yearAttended.Item1 != null)
                    {
                        startYear = Convert.ToInt32(yearAttended.Item1);
                    }

                    if (!string.IsNullOrEmpty(yearAttended.Item2))
                    {
                        Int32.TryParse(yearAttended.Item2, out endYear);
                    }

                    if (!yearsCollection.Contains(new Tuple<int, int>(startYear, endYear))
                            && ((yearAttended.Item1 != null) || (endYear != 0)))
                    {
                        var externalEducationAttendancePeriods = new ExternalEducationAttendancePeriods() { };

                        if (startYear != 0)
                        {
                            externalEducationAttendancePeriods.StartDate = new DateDtoProperty() { Year = startYear };
                        }
                        if (endYear != 0)
                        {
                            externalEducationAttendancePeriods.EndDate = new DateDtoProperty() { Year = endYear };
                        }
                        attendancePeriods.Add(externalEducationAttendancePeriods);
                    }
                }
            }
            if (attendancePeriods.Any())
            {
                externalEducation.AttendancePeriods = attendancePeriods;
            }

            externalEducation.PerformanceMeasure = institutionAttendEntity.ExtGpa.HasValue ? institutionAttendEntity.ExtGpa.Value.ToString() : null;
            externalEducation.ClassRank = institutionAttendEntity.RankNumerator;
            externalEducation.ClassSize = institutionAttendEntity.RankDenominator;

            if (institutionAttendEntity.ExtCredits.HasValue)
            {
                externalEducation.TotalCredits = institutionAttendEntity.ExtCredits;
            }

            if (institutionAttendEntity.RankPercent.HasValue)
            {
                try
                {
                    externalEducation.ClassPercentile = Convert.ToDecimal(institutionAttendEntity.RankPercent);
                }
                catch (InvalidCastException)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert rank percent to a decimal. Value: '{0}'", institutionAttendEntity.ExtCredits), "Validation.Exception", institutionAttendEntity.Guid,
                      id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);

                }
            }

            if ((institutionAttendEntity.InstaIntgHsGradDate.HasValue) && (institutionAttendEntity.InstaIntgHsGradDate != default(DateTime)))
            {
                DateTime? instaIntgHsGradDate = null;
                try
                {
                    instaIntgHsGradDate = Convert.ToDateTime(institutionAttendEntity.InstaIntgHsGradDate);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert InstaIntgHsGradDate to a dateTime.  Value : {0}", institutionAttendEntity.InstaIntgHsGradDate),
                        "Validation.Exception", institutionAttendEntity.Guid, id: string.IsNullOrEmpty(institutionAttendEntity.Id) ? "" : institutionAttendEntity.Id);
                }

                externalEducation.GraduationDetails = new ExternalEducationGraduationDetails()
                {
                    Source = Dtos.EnumProperties.ExternalEducationGraduationDetailsSource.SecondarySchool,
                    GraduatedOn = instaIntgHsGradDate
                };
            }
            return externalEducation;
        }

        /// <summary>
        /// ConvertPersonExternalEducationDtoToEntityAsync
        /// </summary>
        /// <param name="personExternalEducationEntityId"></param>
        /// <param name="personExternalEducation"></param>
        /// <returns></returns>
        private async Task<InstitutionsAttend> ConvertPersonExternalEducationDtoToEntityAsync(string personExternalEducationEntityId, PersonExternalEducation personExternalEducation)
        {
            if (personExternalEducation == null)
            {
                IntegrationApiExceptionAddError("personExternalEducation request body is required.", "Missing.Request.Body");
                throw IntegrationApiException;
            }

            string personId = "", institutionId = "";

            if (personExternalEducation.Person == null || string.IsNullOrEmpty(personExternalEducation.Person.Id))
            {
                IntegrationApiExceptionAddError("Person Id is null or empty.", "Missing.Required.Property", personExternalEducation.Id, personExternalEducationEntityId);

            }
            else
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(personExternalEducation.Person.Id);

                    if (string.IsNullOrEmpty(personId))
                    {
                        IntegrationApiExceptionAddError(string.Format("No person was found for guid '{0}'", personExternalEducation.Person.Id), "GUID.Not.Found", personExternalEducation.Id,
                            personExternalEducationEntityId);
                    }
                    else
                    {
                        var isCorp = await _personRepository.IsCorpAsync(personId);
                        if (isCorp)
                        {
                            IntegrationApiExceptionAddError(string.Format("External Education cannot be created or updated for a corporation. '{0}'", personExternalEducation.Person.Id), "Validation.Exception", personExternalEducation.Id,
                            personExternalEducationEntityId);
                        }
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("No person was found for guid '{0}'", personExternalEducation.Person.Id), "GUID.Not.Found", personExternalEducation.Id,
                            personExternalEducationEntityId);
                }
            }

            if (personExternalEducation.Institution == null || string.IsNullOrEmpty(personExternalEducation.Institution.Id))
            {
                IntegrationApiExceptionAddError("Institution Id is null or empty.", "Missing.Required.Property", personExternalEducation.Id, personExternalEducationEntityId);
            }
            else
            {
                try
                {
                    institutionId = await _institutionRepository.GetInstitutionFromGuidAsync(personExternalEducation.Institution.Id);

                    if (string.IsNullOrEmpty(institutionId))
                    {
                        IntegrationApiExceptionAddError(string.Format("No external institution was found for guid '{0}'", personExternalEducation.Institution.Id), "GUID.Not.Found", personExternalEducation.Id,
                            personExternalEducationEntityId);
                    }                   
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("No external institution was found for guid '{0}'", personExternalEducation.Institution.Id), "GUID.Not.Found", personExternalEducation.Id,
                           personExternalEducationEntityId);
                }
            }

            if (string.IsNullOrEmpty(personExternalEducationEntityId))
                personExternalEducationEntityId = string.Concat(personId, "*", institutionId);
            else
            {
                var splitKey = personExternalEducationEntityId.Split('*');
                if (splitKey.Count() != 2)
                {
                    IntegrationApiExceptionAddError(string.Concat("InstitutionAttend Record Key is not valid: ", personExternalEducationEntityId), "Validation.Exception", personExternalEducation.Id,
                           personExternalEducationEntityId);

                    throw IntegrationApiException;
                }

                if (personId != splitKey[0])
                {
                    IntegrationApiExceptionAddError(string.Concat("Person Id from person guid is not associated with the record key: ", personExternalEducationEntityId), "Validation.Exception", personExternalEducation.Id,
                           personExternalEducationEntityId);
                }
                if (institutionId != splitKey[1])
                {
                    IntegrationApiExceptionAddError(string.Concat("Institution Id from person guid is not associated with the record key: ", personExternalEducationEntityId), "Validation.Exception", personExternalEducation.Id,
                           personExternalEducationEntityId);
                }
            }

          
            Domain.Base.Entities.InstType instType = Domain.Base.Entities.InstType.Unknown;
            if (!string.IsNullOrEmpty(institutionId))
            {
                var institution = await _institutionRepository.GetInstitutionAsync(institutionId);
                instType = institution.InstitutionType;
            }

            var institutionsAttend = new InstitutionsAttend(personExternalEducation.Id, personExternalEducationEntityId);

            if (!string.IsNullOrEmpty(personExternalEducation.PerformanceMeasure))
            {
                try
                {
                    var convertedPerformanceMeasure = Convert.ToDecimal(personExternalEducation.PerformanceMeasure);
                    institutionsAttend.ExtGpa = convertedPerformanceMeasure;
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert performance measure to a decimal. Value: '{0}'", personExternalEducation.PerformanceMeasure), "Validation.Exception", personExternalEducation.Id,
                        personExternalEducationEntityId);
                }
            }

            if ((personExternalEducation.ClassSize != null && personExternalEducation.ClassSize.HasValue)
               || (personExternalEducation.ClassRank != null && personExternalEducation.ClassRank.HasValue)
               || (personExternalEducation.ClassPercentile != null && personExternalEducation.ClassPercentile.HasValue))
            {
                if (instType == Domain.Base.Entities.InstType.College)
                {
                    IntegrationApiExceptionAddError("Class Size, Class Rank, and Class Percentile are maintained at the person-external-education-credentials level for Colleges.", "Validation.Exception", personExternalEducation.Id,
                        personExternalEducationEntityId);
                }
                else
                {
                    if (personExternalEducation.ClassRank != null && personExternalEducation.ClassRank.HasValue)
                        institutionsAttend.RankNumerator = personExternalEducation.ClassRank;

                    if (personExternalEducation.ClassSize != null && personExternalEducation.ClassSize.HasValue)
                        institutionsAttend.RankDenominator = personExternalEducation.ClassSize;

                   // if we have a class percentile, try to use that value
                    if (personExternalEducation.ClassPercentile != null && personExternalEducation.ClassPercentile.HasValue)
                    {
                        try
                        {
                            // if the percentage is 0, but we have a class size, and rank, then calculate
                            if ((personExternalEducation.ClassPercentile == 0) && (personExternalEducation.ClassRank.HasValue) && (personExternalEducation.ClassRank != 0) 
                                && (personExternalEducation.ClassSize.HasValue) && (personExternalEducation.ClassSize != 0))
                                institutionsAttend.RankPercent = ((Convert.ToDecimal(personExternalEducation.ClassRank) / Convert.ToDecimal(personExternalEducation.ClassSize))) * 100;
                            else
                                institutionsAttend.RankPercent = Convert.ToDecimal(personExternalEducation.ClassPercentile);
                        }
                        catch (Exception)
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to convert ClassPercentile to a decimal. Value: '{0}'", personExternalEducation.ClassPercentile), "Validation.Exception", personExternalEducation.Id,
                                 personExternalEducationEntityId);
                        }
                    }
                    //else we dont have a class percentile, but maybe we can calculate from the rank/size
                    else if ((personExternalEducation.ClassRank.HasValue) && (personExternalEducation.ClassRank != 0) &&  (personExternalEducation.ClassSize.HasValue) && (personExternalEducation.ClassSize != 0))
                    {
                        try
                        {
                            institutionsAttend.RankPercent = ((Convert.ToDecimal(personExternalEducation.ClassRank) / Convert.ToDecimal(personExternalEducation.ClassSize))) * 100;
                        }
                        catch (Exception)
                        {
                            IntegrationApiExceptionAddError("Unable to calculate ClassPercentile.", "Validation.Exception", personExternalEducation.Id,
                                 personExternalEducationEntityId);
                        }
                    }
                }
            }

            if (personExternalEducation.TotalCredits != null && personExternalEducation.TotalCredits.HasValue)
            {
                institutionsAttend.ExtCredits = personExternalEducation.TotalCredits;
            }

            if (personExternalEducation.GraduationDetails != null && personExternalEducation.GraduationDetails.GraduatedOn != null && personExternalEducation.GraduationDetails.GraduatedOn.HasValue)
            {

                if (instType == Domain.Base.Entities.InstType.College)
                {
                    IntegrationApiExceptionAddError("graduationDetails is only allowed for secondarySchool institutions.", "Validation.Exception", personExternalEducation.Id, personExternalEducationEntityId);
                }
                else
                {
                    try
                    {
                        var convertedGraduatedOn = Convert.ToDateTime(personExternalEducation.GraduationDetails.GraduatedOn);
                        if (convertedGraduatedOn != default(DateTime))
                            institutionsAttend.InstaIntgHsGradDate = convertedGraduatedOn;
                    }
                    catch (InvalidCastException)
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to convert GraduationDetails.GraduatedOn to a datetime. Value: '{0}'", personExternalEducation.GraduationDetails.GraduatedOn),
                            "Validation.Exception", personExternalEducation.Id, personExternalEducationEntityId);
                    }
                }
            }

            if (personExternalEducation.AttendancePeriods != null && personExternalEducation.AttendancePeriods.Any())
            {
                var datesAttened = new List<Tuple<DateTime?, DateTime?>>();
                var datesAttendedYears = new List<Tuple<int, int>>();
                var yearsAttended = new List<Tuple<int?, string>>();

                foreach (var attendancePeriod in personExternalEducation.AttendancePeriods)
                {
                    // if we have the full date/time, then use that value
                    DateTime? startDate = this.ConvertDateDtoPropertyToDateTime(attendancePeriod.StartDate, personExternalEducation.Id, personExternalEducationEntityId);
                    DateTime? endDate = this.ConvertDateDtoPropertyToDateTime(attendancePeriod.EndDate, personExternalEducation.Id, personExternalEducationEntityId);

                    // avoid creating a record where the  INSTA.START.DATES year is the same as the INSTA.YEAR.ATTEND.START
                    // AND  the INSTA.END.DATES  year is the same as the INSTA.YEAR.ATTEND.END
                    int startYear = 0;
                    int endYear = 0;

                    //create a collection consisting of the INSTA.START.DATES year AND INSTA.END.DATES year
                    if ((startDate != null) || (endDate != null))
                    {
                        datesAttened.Add(new Tuple<DateTime?, DateTime?>(startDate, endDate));

                        if (startDate != null)
                        {

                            startYear = Convert.ToDateTime(startDate).Year;
                        }
                        if (endDate != null)
                        {

                            endYear = Convert.ToDateTime(endDate).Year;
                        }

                        datesAttendedYears.Add(new Tuple<int, int>(startYear, endYear));
                    }
                    // 
                    if ((attendancePeriod.StartDate != null || attendancePeriod.EndDate != null))
                    {
                        var startYearVal = attendancePeriod.StartDate != null ? attendancePeriod.StartDate.Year : 0;
                        var endYearVal = attendancePeriod.EndDate != null ? attendancePeriod.EndDate.Year : 0;

                        if (!datesAttendedYears.Contains(new Tuple<int, int>(startYearVal, endYearVal)))
                            yearsAttended.Add(new Tuple<int?, string>(startYearVal, endYearVal.ToString()));
                    }
                }

                institutionsAttend.DatesAttended = datesAttened;
                institutionsAttend.YearsAttended = yearsAttended;
            }

            return institutionsAttend;
        }

        /// <summary>
        /// ConvertDateDtoPropertyToDateTime
        /// </summary>
        /// <param name="dateDtoProperty"></param>
        /// <param name="guid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private DateTime? ConvertDateDtoPropertyToDateTime(DateDtoProperty dateDtoProperty, string guid = "", string id = "")
        {
            if (dateDtoProperty == null)
                return null;

            // Must have Year and Month.
            if ((dateDtoProperty.Year != 0) && ((dateDtoProperty.Month == null && dateDtoProperty.Day == null)))
            {
                return null;
            }

            // Return for missing month, default 1 for missing day.
            if (dateDtoProperty.Month == null)
                return null;
            if (dateDtoProperty.Day == null)
                dateDtoProperty.Day = 1;

            DateTime? retVal = null;
            try
            {

                var month = Convert.ToInt32(dateDtoProperty.Month);
                var day = Convert.ToInt32(dateDtoProperty.Day);
                var year = Convert.ToInt32(dateDtoProperty.Year);

                retVal = new DateTime(year, month, day);
            }
            catch (InvalidCastException)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to convert DateDtoProperty to a datetime. Month: '{0}', Day: '{1}', Year {2}'",
                    dateDtoProperty.Month, dateDtoProperty.Day, dateDtoProperty.Year), "Validation.Exception", guid, id);
            }
            return retVal;
        }

        /// <summary>
        /// ConvertDateTimeToDateDtoProperty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private DateDtoProperty ConvertDateTimeToDateDtoProperty(DateTime? value)
        {
            DateDtoProperty dateDtoProperty = null;

            if ((value.HasValue) && (value != default(DateTime)))
            {
                var convertedDateTime = Convert.ToDateTime(value);

                dateDtoProperty = new DateDtoProperty
                {
                    Month = convertedDateTime.Month,
                    Day = convertedDateTime.Day,
                    Year = convertedDateTime.Year
                };
            }
            return dateDtoProperty;
        }

        /// <summary>
        /// Validate the PersonExternalEducation request body on create and update.
        /// </summary>
        /// <param name="personExternalEducation"></param>
        private void Validate(PersonExternalEducation personExternalEducation)
        {
            if (personExternalEducation == null)
            {
                IntegrationApiExceptionAddError("The request body is required.", "Missing.Request.Body");
            }

            if (string.IsNullOrEmpty(personExternalEducation.Id))
            {
                IntegrationApiExceptionAddError("The id is a required property on the schema.", "Missing.Request.ID");
            }

            if (personExternalEducation.Person == null || string.IsNullOrEmpty(personExternalEducation.Person.Id))
            {
                IntegrationApiExceptionAddError("The person id is a required property on the schema.", "Missing.Required.Property");
            }

            if (personExternalEducation.Institution == null || string.IsNullOrEmpty(personExternalEducation.Institution.Id))
            {
                IntegrationApiExceptionAddError("The institution id is a required property on the schema.", "Missing.Required.Property");
            }

            if (personExternalEducation.AttendancePeriods != null && personExternalEducation.AttendancePeriods.Any())
            {
                HashSet<DateTime?> startDateCollection = new HashSet<DateTime?>();
                HashSet<int> startYearCollection = new HashSet<int>();

                foreach (var attendancePeriod in personExternalEducation.AttendancePeriods)
                {
                    DateTime? startDate = null;
                    DateTime? endDate = null;

                    if (attendancePeriod.StartDate != null)
                    {
                        if (attendancePeriod.StartDate.Year == 0)
                        {
                            IntegrationApiExceptionAddError("If providing a attendancePeriod.StartDate, the attendancePeriod.StartDate.Year is a required property on the schema.", "Validation.Exception");
                        }

                        //startOn month required when year and day provided
                        if (attendancePeriod.StartDate.Year != 0 && attendancePeriod.StartDate.Day.HasValue && !attendancePeriod.StartDate.Month.HasValue)
                        {
                            IntegrationApiExceptionAddError("Start Month is required when Start Year and Start Day are provided.", "Validation.Exception");//
                        }

                        //The specified Start Date is not a valid value.
                        try
                        {
                            startDate = ConvertDateDtoPropertyToDateTime(attendancePeriod.StartDate);
                        }
                        catch (Exception)
                        {
                            IntegrationApiExceptionAddError("The specified Start Date is not a valid value.", "Validation.Exception"); //
                        }

                        //A duplicate start date or start year is provided in the startOn array
                        if (startDate != null)
                        {
                            bool added = startDateCollection.Add(startDate);
                            if (!added)
                            {
                                IntegrationApiExceptionAddError("A duplicate Start On is not allowed.", "Validation.Exception"); //
                            }
                        }

                        //A duplicate start date or start year is provided in the startOn array
                        if ((startDate != null) && (attendancePeriod.StartDate.Year != 0))
                        {
                            bool added = startYearCollection.Add(attendancePeriod.StartDate.Year);
                            if (!added)
                            {
                                IntegrationApiExceptionAddError("A duplicate Start On is not allowed.", "Validation.Exception"); //
                            }
                        }
                    }


                    if (attendancePeriod.EndDate != null)
                    {
                        if (attendancePeriod.EndDate.Year == 0)
                        {
                            IntegrationApiExceptionAddError("If providing a attendancePeriod.EndDate, the attendancePeriod.EndDate.Year is a required property on the schema.", "Missing.Required.Property");
                        }

                        //endOn month required when year and day provided
                        if (attendancePeriod.EndDate.Year != 0 && attendancePeriod.EndDate.Day.HasValue && !attendancePeriod.EndDate.Month.HasValue)
                        {
                            IntegrationApiExceptionAddError("End Month is required when End Year and End Day are provided.", "Validation.Exception");//
                        }

                        //Invalid end date
                        try
                        {
                            endDate = ConvertDateDtoPropertyToDateTime(attendancePeriod.EndDate);
                        }
                        catch (Exception)
                        {
                            IntegrationApiExceptionAddError("The specified End Date is not a valid value.", "Validation.Exception");
                        }
                    }

                    if ((attendancePeriod.StartDate != null) && (attendancePeriod.EndDate != null))
                    {
                        //start year cannot be greater than end year
                        if (attendancePeriod.StartDate.Year != 0 && attendancePeriod.EndDate.Year != 0 && attendancePeriod.StartDate.Year > attendancePeriod.EndDate.Year)
                        {
                            IntegrationApiExceptionAddError("The Start Year cannot be greater than the End Year.", "Validation.Exception");//
                        }

                        //endOn includes a full date and startOn only has a year
                        if (endDate != null && startDate == null && attendancePeriod.StartDate.Year != 0 && !attendancePeriod.StartDate.Day.HasValue && !attendancePeriod.StartDate.Month.HasValue)
                        {
                            IntegrationApiExceptionAddError("Cannot provide an end date when only a start year was provided.", "Validation.Exception"); //
                        }

                        //endOn only includes a year and startOn has a full date
                        if (endDate == null && startDate != null && attendancePeriod.EndDate.Year != 0 && !attendancePeriod.EndDate.Day.HasValue && !attendancePeriod.EndDate.Month.HasValue)
                        {
                            IntegrationApiExceptionAddError("Cannot provide only an end year when a start date was provided.", "Validation.Exception"); //
                        }
                    }

                    //start date cannot be greater than end date
                    if (endDate != null && startDate != null && (DateTime.Compare(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate)) > 0))
                    {
                        IntegrationApiExceptionAddError("The Start Date cannot be greater than the End Date.", "Validation.Exception");//
                    }

                    //endOn is provided in the payload and startOn is not.Start date or start year is required in Colleague.
                    if (attendancePeriod.EndDate != null && attendancePeriod.StartDate == null)
                    {
                        IntegrationApiExceptionAddError("Start On is required when End On is included in the payload.", "Missing.Required.Property");//
                    }
                }
            }

            if (personExternalEducation.GraduationDetails != null)
            {
                if (personExternalEducation.GraduationDetails.Source == Dtos.EnumProperties.ExternalEducationGraduationDetailsSource.NotSet)
                {
                    IntegrationApiExceptionAddError("If providing a GraduationDetails object, the GraduationDetails.Source is a required property on the schema.", "Missing.Required.Property");
                }
                if (personExternalEducation.GraduationDetails.GraduatedOn == null && !personExternalEducation.GraduationDetails.GraduatedOn.HasValue)
                {
                    IntegrationApiExceptionAddError("If providing a GraduationDetails object, the GraduationDetails.GraduatedOn is a required property on the schema.", "Missing.Required.Property");
                }
            }

            //If classSize is provided, classRank cannot be null
            if (personExternalEducation.ClassSize.HasValue && !personExternalEducation.ClassRank.HasValue)
            {
                IntegrationApiExceptionAddError("Class Rank is required when Class Size is provided.", "Validation.Exception");
            }
            //classRank cannot be greater than classSize
            if (personExternalEducation.ClassSize.HasValue && personExternalEducation.ClassRank.HasValue && personExternalEducation.ClassRank > personExternalEducation.ClassSize)
            {
                IntegrationApiExceptionAddError("Class Rank is required when Class Size is provided.", "Validation.Exception");
            }

            //If classRank is provided, classSize must be provided
            if (personExternalEducation.ClassRank.HasValue && !personExternalEducation.ClassSize.HasValue)
            {
                IntegrationApiExceptionAddError("Class Size is required when Class Rank is provided.", "Validation.Exception");
            }

            //performanceMeasure not between 0 and 999.999
            if (!string.IsNullOrEmpty(personExternalEducation.PerformanceMeasure))
            {
                try
                {
                    var convertedPerformanceMeasure = Convert.ToDecimal(personExternalEducation.PerformanceMeasure);
                    if ((convertedPerformanceMeasure.CompareTo(0.00m) < 0) || (convertedPerformanceMeasure.CompareTo(999.999m) > 0))
                    {
                        IntegrationApiExceptionAddError("The Performance Measure is not a valid value.", "Validation.Exception");
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert performance measure to a decimal. Value: '{0}'", personExternalEducation.PerformanceMeasure), "Validation.Exception");
                }
            }

            //classSize not between 0 and 9,999,999
            if (personExternalEducation.ClassSize.HasValue)
            {
                try
                {
                    var convertedClassSize = Convert.ToInt32(personExternalEducation.ClassSize);
                    if ((convertedClassSize.CompareTo(0) < 0) || (convertedClassSize.CompareTo(9999999) > 0))
                    {
                        IntegrationApiExceptionAddError("The Class Size is not a valid value.", "Validation.Exception");
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert class size to an int. Value: '{0}'", personExternalEducation.ClassSize), "Validation.Exception");
                }
            }

            //classRank not between 0 and 9,999,999
            if (personExternalEducation.ClassRank.HasValue)
            {
                try
                {
                    var convertedClassRank = Convert.ToInt32(personExternalEducation.ClassRank);
                    if ((convertedClassRank.CompareTo(0) < 0) || (convertedClassRank.CompareTo(9999999) > 0))
                    {
                        IntegrationApiExceptionAddError("The Class Rank is not a valid value.", "Validation.Exception");
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert class rank to an int. Value: '{0}'", personExternalEducation.ClassRank), "Validation.Exception");
                }
            }

            //classPercentile not between 0 and 100
            if (personExternalEducation.ClassPercentile.HasValue)
            {
                try
                {
                    var convertedClassPercent = Convert.ToDecimal(personExternalEducation.ClassPercentile);
                    if ((convertedClassPercent.CompareTo(0.00m) < 0) || (convertedClassPercent.CompareTo(100.00m) > 0))
                    {
                        IntegrationApiExceptionAddError("The Class Percentile is not a valid value.", "Validation.Exception");
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert class percentile to an decimal.  Value: '{0}'", personExternalEducation.ClassPercentile), "Validation.Exception");
                }
            }

            //totalCredits not between 0 and 999.99
            if (personExternalEducation.TotalCredits.HasValue)
            {
                try
                {
                    var totalCredits = Convert.ToDecimal(personExternalEducation.TotalCredits);
                    if ((totalCredits.CompareTo(0.00m) < 0) || (totalCredits.CompareTo(999.99m) > 0))
                    {
                        IntegrationApiExceptionAddError("The Total Credits is not a valid value.", "Validation.Exception");
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to convert totalCredits to a decimal for validation.  Value: '{0}'", personExternalEducation.TotalCredits), "Validation.Exception");
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;
        }    
    }
}