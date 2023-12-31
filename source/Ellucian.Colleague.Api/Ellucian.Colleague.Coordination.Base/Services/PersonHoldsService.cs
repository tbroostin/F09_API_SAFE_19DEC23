﻿// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Holds on persons records.
    /// </summary>
    [RegisterType]
    public class PersonHoldsService : BaseCoordinationService, IPersonHoldsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonHoldsRepository _personHoldsRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public PersonHoldsService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IPersonRepository personRepository,
                                         IPersonHoldsRepository personHoldsRepository,
                                         IConfigurationRepository configurationRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _personHoldsRepository = personHoldsRepository;
            _configurationRepository = configurationRepository;
        }

        #region GET Methods
        /// <summary>
        ///  Get all person-holds
        /// </summary>
        /// <returns>Tuple containing a Collection of Dtos.PersonHold, along with total record count</returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonHold>, int>> GetPersonHoldsAsync(int offset, int limit, bool bypassCache = false)
        {           
            var dtoPersonHolds = new List<Dtos.PersonHold>();
            Tuple<IEnumerable<PersonRestriction>, int> personHoldsEntities = new Tuple<IEnumerable<PersonRestriction>, int>(new List<PersonRestriction>(), 0);
            try
            {
                personHoldsEntities = await _personHoldsRepository.GetPersonHoldsAsync(offset, limit);

                //Get all hold types with category
                var personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(bypassCache) as List<Domain.Base.Entities.Restriction>;

                if (personHoldTypes != null)
                {
                    var personIds = personHoldsEntities.Item1.Where(p => !string.IsNullOrWhiteSpace(p.StudentId)).Select(s => s.StudentId).ToList();
                    var personIdDict = await _personRepository.GetPersonGuidsCollectionAsync(personIds.Distinct());

                    foreach (var holdEntity in personHoldsEntities.Item1)
                    {
                        var restriction = personHoldTypes.FirstOrDefault(r => r != null && r.Code == holdEntity.RestrictionId);

                        if (restriction != null)
                        {
                            string personId;
                            if (personIdDict != null && personIdDict.Any() && personIdDict.TryGetValue(holdEntity.StudentId, out personId))
                            {
                                var dtosPersonHold = BuildPersonHold(holdEntity.Guid, holdEntity, personHoldTypes, personId,
                               restriction);
                                dtoPersonHolds.Add(dtosPersonHold);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Person hold type '{0}' is missing the GUID or is invalid.", holdEntity.RestrictionId), "Bad.Data", holdEntity.Guid, holdEntity.Id);
                        }
                    }
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.PersonHold>, int>(dtoPersonHolds, personHoldsEntities.Item2);
        }

        /// <summary>
        /// Retrieves all active person holds for hold id
        /// </summary>
        /// <param name="id">guid for the person hold</param>
        /// <returns>List of PersonHold for the person</returns>
        public async Task<Dtos.PersonHold> GetPersonHoldAsync(string id, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Person hold GUID is required.");
            }
            
            PersonRestriction personHoldEntity = null;
            try
            {
                personHoldEntity = await _personHoldsRepository.GetPersonHoldByIdAsync(id);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("No person-holds was found for GUID '{0}'", id));

            }
            if (personHoldEntity == null)
            {
                throw new KeyNotFoundException(string.Format("No person-holds was found for GUID '{0}'", id));
            }

            Dtos.PersonHold dtoPersonHolds = null;

            try {
                var personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(bypassCache) as List<Domain.Base.Entities.Restriction>;

                string personId = await _personRepository.GetPersonGuidFromIdAsync(personHoldEntity.StudentId);
                if (string.IsNullOrEmpty(personId))
                {
                    throw new KeyNotFoundException(string.Format("Did not find any person with id: {0}", personHoldEntity.StudentId));
                }

                //Get the person hold type(restriction)
                Restriction restriction = personHoldTypes.FirstOrDefault(r => r != null && r.Code.Equals(personHoldEntity.RestrictionId, StringComparison.OrdinalIgnoreCase));
                if (restriction == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Person hold type '{0}' is missing the GUID or is invalid.", personHoldEntity.RestrictionId), "Bad.Data", personHoldEntity.Guid, personHoldEntity.Id);
                    throw IntegrationApiException;
                }

                //Get the guid for the entity, just to make sure guid is generated, get the guid based on entity id
                string personHoldId = await _personHoldsRepository.GetStudentHoldGuidFromIdAsync(personHoldEntity.Id);
                if (string.IsNullOrEmpty(personHoldId))
                {
                    throw new KeyNotFoundException(string.Format("Did not find guid for id: {0}", personHoldEntity.Id));
                }

                dtoPersonHolds = BuildPersonHold(personHoldEntity.Guid, personHoldEntity, personHoldTypes, personId, restriction);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("No person-holds was found for GUID '{0}'", id));

            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return dtoPersonHolds;
        }

        /// <summary>
        /// Gets person holds based on personId
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.PersonHold>> GetPersonHoldsAsync(string personId, bool bypassCache = false)
        {

            if (string.IsNullOrEmpty(personId))
            {
                return new List<Dtos.PersonHold>();
            }

            List<Dtos.PersonHold> dtoPersonHolds = new List<Dtos.PersonHold>();

            try
            {
                List<Domain.Base.Entities.PersonRestriction> personHoldsEntities = await _personHoldsRepository.GetPersonHoldsByPersonIdAsync(personId) as List<Domain.Base.Entities.PersonRestriction>;

                //Get all hold types with category
                List<Domain.Base.Entities.Restriction> personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(bypassCache) as List<Domain.Base.Entities.Restriction>;

                var personIds = personHoldsEntities.Where(p => !string.IsNullOrWhiteSpace(p.StudentId)).Select(s => s.StudentId).ToList();
                var personIdDict = await _personRepository.GetPersonGuidsCollectionAsync(personIds.Distinct());

                foreach (var holdEntity in personHoldsEntities)
                {
                    Dtos.PersonHold dtosPersonHold = null;

                    Restriction restriction = personHoldTypes.Where(r => r.Code == holdEntity.RestrictionId).FirstOrDefault();
                    if (restriction != null)
                    {
                        string tempPersonId;
                        if (personIdDict != null && personIdDict.Any() && personIdDict.TryGetValue(holdEntity.StudentId, out tempPersonId))
                        {
                            dtosPersonHold = BuildPersonHold(holdEntity.Guid, holdEntity, personHoldTypes, tempPersonId, restriction);
                            dtoPersonHolds.Add(dtosPersonHold);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(holdEntity.RestrictionId))
                        {
                            logger.Info("The '" + holdEntity.RestrictionId +
                                        "' restriction code is not defined.  Ignoring during person-holds Get.");
                        }
                    }
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return dtoPersonHolds;
        }
       
        #endregion

        #region DELETE Methods
        /// <summary>
        /// Delete person hold based on person hold id
        /// </summary>
        /// <param name="personHoldsId"></param>
        /// <returns></returns>
        public async Task DeletePersonHoldAsync(string personHoldsId)
        {
            if (string.IsNullOrEmpty(personHoldsId))
            {
                throw new ArgumentNullException("guid", "GUID is required to delete a person-holds.");
            }

            try
            {
                var personHoldEntity = await _personHoldsRepository.GetPersonHoldByIdAsync(personHoldsId);
                if (personHoldEntity == null)
                {
                    throw new KeyNotFoundException();
                }
                //Log all the warnings
                var warnings = await _personHoldsRepository.DeletePersonHoldsAsync(personHoldsId);
                if (warnings.Any())
                {
                    string warningMessage = string.Empty;
                    foreach (var warning in warnings)
                    {
                        warningMessage += string.Format(Environment.NewLine + "'{0}' : '{1}'", warning.WarningCode, warning.WarningMessage);
                    }
                    logger.Info(warningMessage);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                IntegrationApiExceptionAddError(string.Format("Person-holds not found for guid: '{0}'.", personHoldsId), "GUID.Not.Found", personHoldsId);
                throw IntegrationApiException;
            }
        }

        #endregion

        #region PUT Method

        /// <summary>
        /// Updates person hold
        /// </summary>
        /// <param name="id">person hold id</param>
        /// <param name="personHoldDto">personHold object</param>
        /// <returns></returns>
        public async Task<Dtos.PersonHold> UpdatePersonHoldAsync(string id, Dtos.PersonHold personHoldDto)
        {

            _personHoldsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            string personId = string.Empty;
            Domain.Base.Entities.Restriction holdType = null;
            string personHoldEnityId = string.Empty;            

            //Check id's match, For POST id will be blank & will come in the payload
            if (string.IsNullOrEmpty(id))
            {
                id = personHoldDto.Id;
            }

            //Do required fields & empty or null id check
            CheckForRequiredFields(personHoldDto);
            
            //Check start date is before end date
            if (personHoldDto.StartOn > personHoldDto.EndOn)
            {
                IntegrationApiExceptionAddError("The hold start date must be on or before the hold end date.", "Validation.Exception", id);
            }            

            // get the person ID associated with the incoming guid            
            if (personHoldDto.Person != null && !string.IsNullOrEmpty(personHoldDto.Person.Id))
            {
                var person = await _personRepository.GetPersonByGuidNonCachedAsync(personHoldDto.Person.Id);
                if (person == null)
                {
                    IntegrationApiExceptionAddError("Person ID associated to id '" + personHoldDto.Person.Id + "' not found", "Validation.Exception", id);
                }
                else
                {
                    if (!string.IsNullOrEmpty(person.PersonCorpIndicator) && person.PersonCorpIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                    {
                        IntegrationApiExceptionAddError("The person specified is an organization, not a person.", "Validation.Exception", id);
                    }
                    personId = person.Id;
                }
            }

            /* 
                Check PersonHoldTypeType
                On a POST or PUT operation, we can use this guid to determine exactly which RESTRICTIONS code (found in RESTRICTIONS.ID) to use to assign the person.  Ultimately, 
                this will be a new record in the STUDENT.RESTRICTIONS file. 
            */
            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.Detail != null && !string.IsNullOrEmpty(personHoldDto.PersonHoldTypeType.Detail.Id))
            {
                List<Domain.Base.Entities.Restriction> personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(true) as List<Domain.Base.Entities.Restriction>;
                holdType = personHoldTypes.FirstOrDefault(h => h.Guid == personHoldDto.PersonHoldTypeType.Detail.Id);

                if (holdType == null)
                {
                    IntegrationApiExceptionAddError("Person hold type associated with id '" + personHoldDto.PersonHoldTypeType.Detail.Id + "' was not found", "Validation.Exception", id);
                }
            }

            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.PersonHoldCategory != null && 
                (personHoldDto.PersonHoldTypeType.Detail == null || string.IsNullOrEmpty(personHoldDto.PersonHoldTypeType.Detail.Id)))
            {
                List<Domain.Base.Entities.Restriction> personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(true) as List<Domain.Base.Entities.Restriction>;
                holdType = personHoldTypes.FirstOrDefault(h => h.RestIntgCategory.ToString().Equals(personHoldDto.PersonHoldTypeType.PersonHoldCategory.ToString(), StringComparison.InvariantCultureIgnoreCase));
                
                if (holdType == null)
                {
                    IntegrationApiExceptionAddError("Person hold type associated with category '" + personHoldDto.PersonHoldTypeType.PersonHoldCategory.ToString() + "' was not found", "Validation.Exception", id);
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            //On a POST or PUT, if the enumeration here is "notify" we can store "Y" to STR.PRTL.DISPLAY.FLAG. Otherwise, we can leave STR.PRTL.DISPLAY.FLAG blank.
            string notificationIndicator = string.Empty;
            switch (personHoldDto.NotificationIndicator)
            {
                case Dtos.NotificationIndicatorType.Notify:
                    notificationIndicator = "Y";
                    break;
                case Dtos.NotificationIndicatorType.Watch:
                    notificationIndicator = "N";
                    break;
                default: break;
            }

            Dtos.PersonHold personHoldDtos = null;
            //Construct the request for PUT or POST
            try
            {
                var request = new PersonHoldRequest(personHoldEnityId, personId, holdType.Code, personHoldDto.StartOn, personHoldDto.EndOn, notificationIndicator);
                if (personHoldDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    request.PersonHoldGuid = string.Empty;
                }
                else
                {
                    request.PersonHoldGuid = personHoldDto.Id;
                }
                request.Comments = personHoldDto.Comment;

                //Update the person hold
                var response = await _personHoldsRepository.UpdatePersonHoldAsync(request);

                personHoldDtos = await this.GetPersonHoldAsync(response.PersonHoldGuid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return personHoldDtos;
        }

        #endregion

        #region POST Method
        /// <summary>
        /// POST's the person hold
        /// </summary>
        /// <param name="personHold"></param>
        /// <returns></returns>
        public async Task<Dtos.PersonHold> CreatePersonHoldAsync(Dtos.PersonHold personHold)
        {
            string id = string.Empty;
            return await UpdatePersonHoldAsync(id, personHold);
        }
        #endregion

        #region Build methods
        private Dtos.PersonHold BuildPersonHold(string personHoldId, PersonRestriction personHoldEntity, List<Domain.Base.Entities.Restriction> personHoldTypes,
                                                string personId, Domain.Base.Entities.Restriction personHoldType)
        {
            Dtos.PersonHold dtoPersonHolds = new Dtos.PersonHold();
            dtoPersonHolds.Id = personHoldId;
            if (!String.IsNullOrEmpty(personHoldEntity.Comment))
            {
                dtoPersonHolds.Comment = personHoldEntity.Comment;
            }
            dtoPersonHolds.EndOn = personHoldEntity.EndDate;
            dtoPersonHolds.Person = new Dtos.GuidObject2(personId);
            try
            {
                dtoPersonHolds.PersonHoldTypeType = ConvertCategoryToPersonHoldTypeType(personHoldType, personHoldTypes);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Format("Person hold type '{0}' is missing the GUID or is invalid. {1}", personHoldEntity.RestrictionId, ex.Message), "Bad.Data", personHoldEntity.Guid, personHoldEntity.Id);
            }
            if (personHoldEntity.StartDate != null) dtoPersonHolds.StartOn = personHoldEntity.StartDate.Value;
            
      
            dtoPersonHolds.NotificationIndicator = ConvertDisplayFlagToNotificationIndicatorType( personHoldType.RestPrtlDisplayFlag, personHoldEntity.NotificationIndicator);

            return dtoPersonHolds;
        }
        #endregion                                         

        #region Convert  and Helper Methods
        /// <summary>
        /// Check for required fields and empty id fields
        /// </summary>
        /// <param name="personHoldDto"></param>
        private void CheckForRequiredFields(Dtos.PersonHold personHoldDto)
        {
            if (personHoldDto.Person == null)
            {
                IntegrationApiExceptionAddError("Must provide person for person-holds", "Validation.Exception", personHoldDto.Id);
            }

            if (personHoldDto.Person != null && string.IsNullOrEmpty(personHoldDto.Person.Id))
            {
                IntegrationApiExceptionAddError("Must provide person id for person-holds", "Validation.Exception", personHoldDto.Id);
            }

            if (personHoldDto.PersonHoldTypeType == null)
            {
                IntegrationApiExceptionAddError("Must provide person hold type for person-holds", "Validation.Exception", personHoldDto.Id);
            }
            
            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.PersonHoldCategory == null &&
               personHoldDto.PersonHoldTypeType.Detail != null && !string.IsNullOrEmpty(personHoldDto.PersonHoldTypeType.Detail.Id))
            {
                IntegrationApiExceptionAddError("The category is required if the type is included in the payload.", "Validation.Exception", personHoldDto.Id);
            }

            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.PersonHoldCategory == null)
            {
                IntegrationApiExceptionAddError("Must provide person hold category for person-holds", "Validation.Exception", personHoldDto.Id);
            }

            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.Detail != null && string.IsNullOrEmpty(personHoldDto.PersonHoldTypeType.Detail.Id))
            {
                IntegrationApiExceptionAddError("Must provide id if person hold type details is included for person-holds", "Validation.Exception", personHoldDto.Id);
            }          

            if (personHoldDto.NotificationIndicator != null && string.IsNullOrEmpty(personHoldDto.NotificationIndicator.Value.ToString()))
            {
                IntegrationApiExceptionAddError("Must provide value for notification indicator if notification indicator is included for person-holds", "Validation.Exception", personHoldDto.Id);
            }

            if (personHoldDto.StartOn == null || (personHoldDto.StartOn != null && string.IsNullOrEmpty(personHoldDto.StartOn.ToString())))
            {
                IntegrationApiExceptionAddError("Must provide start on for person-holds", "Validation.Exception", personHoldDto.Id);
            }

            DateTimeOffset startOn;
            DateTimeOffset endOn;            

            if (personHoldDto.StartOn != null && !DateTimeOffset.TryParse(personHoldDto.StartOn.ToString(), out startOn))
            {
                IntegrationApiExceptionAddError("Provided startOn date has invalid format", "Validation.Exception", personHoldDto.Id);
            }

            if (personHoldDto.EndOn != null && personHoldDto.EndOn.HasValue && !DateTimeOffset.TryParse(personHoldDto.EndOn.Value.ToString(), out endOn) )
            {
                IntegrationApiExceptionAddError("Provided endOn date has invalid format", "Validation.Exception", personHoldDto.Id);
            }            
        }

        /// <summary>
        /// Converts STR.PRTL.DISPLAY.FLAG or REST.PRTL.DISPLAY.FLAG to Dtos.NotificationIndicatorType
        ///     If the STR.PRTL.DISPLAY.FLAG is set to "Y" then return "notify". 
        ///     If STR.PRTL.DISPLAY.FLAG is set to "N" then return "watch".
        ///     Otherwise, if STR.PRTL.DISPLAY.FLAG is null then check the REST.PRTL.DISPLAY.FLAG attribute 
        ///      from the corresponding RESTRICTIONS record.  If populated with a "Y" then return "notify". 
        ///     If REST.PRTL.DISPLAY.FLAG is blank or null then return "watch".
        /// </summary>
        /// <param name="restPrtlDisplayFlag">REST.PRTL.DISPLAY.FLAG</param>
        /// <param name="strPrtlDisplayFlag">STR.PRTL.DISPLAY.FLAG</param>
        /// <returns>Dtos.NotificationIndicatorType</returns>
        private Dtos.NotificationIndicatorType ConvertDisplayFlagToNotificationIndicatorType(string restPrtlDisplayFlag, string strPrtlDisplayFlag)
        {
            if (!string.IsNullOrEmpty(strPrtlDisplayFlag))
            {
                if (strPrtlDisplayFlag == "Y")
                    return Dtos.NotificationIndicatorType.Notify;
                else if (strPrtlDisplayFlag == "N")
                    return Dtos.NotificationIndicatorType.Watch;
            }
            if ((!string.IsNullOrEmpty(restPrtlDisplayFlag)) && (restPrtlDisplayFlag == "Y"))      
            {
                return Dtos.NotificationIndicatorType.Notify;
            }
            return Dtos.NotificationIndicatorType.Watch;
        }

        /// <summary>
        /// Converts dto person hold types
        /// </summary>
        /// <param name="restriction"></param>
        /// <param name="personHoldTypes"></param>
        /// <returns></returns>
        private Dtos.PersonHoldTypeType ConvertCategoryToPersonHoldTypeType(Restriction restriction, List<Restriction> personHoldTypes)
        {
            Dtos.PersonHoldTypeType personHoldTypeType = new Dtos.PersonHoldTypeType();
            Restriction tempRestriction = null;
            switch (restriction.RestIntgCategory)
            {
                case RestrictionCategoryType.Academic:
                    personHoldTypeType.PersonHoldCategory = Dtos.PersonHoldCategoryTypes.Academic;
                    break;
                case RestrictionCategoryType.Administrative:
                    personHoldTypeType.PersonHoldCategory = Dtos.PersonHoldCategoryTypes.Administrative;
                    break;
                case RestrictionCategoryType.Disciplinary:
                    personHoldTypeType.PersonHoldCategory = Dtos.PersonHoldCategoryTypes.Disciplinary;
                    break;
                case RestrictionCategoryType.Financial:
                    personHoldTypeType.PersonHoldCategory = Dtos.PersonHoldCategoryTypes.Financial;
                    break;
                case RestrictionCategoryType.Health:
                    personHoldTypeType.PersonHoldCategory = Dtos.PersonHoldCategoryTypes.Health;
                    break;
                default:
                    personHoldTypeType.PersonHoldCategory = Dtos.PersonHoldCategoryTypes.Academic;
                    break;
            }

            tempRestriction = personHoldTypes.FirstOrDefault(ht => ht != null && ht.Code == restriction.Code);
            if (tempRestriction == null)
                throw new KeyNotFoundException("Person hold type was not found.");

            personHoldTypeType.Detail = new Dtos.GuidObject2(tempRestriction.Guid);

            return personHoldTypeType;
        }
        #endregion       
    }
}
