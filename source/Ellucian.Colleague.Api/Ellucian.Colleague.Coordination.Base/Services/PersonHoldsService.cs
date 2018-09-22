// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base;

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
        public async Task<Tuple<IEnumerable<Dtos.PersonHold>, int>> GetPersonHoldsAsync(int offset, int limit)
        {
            CheckUserPersonHoldsViewPermissions();

            var dtoPersonHolds = new List<Dtos.PersonHold>();
            var personHoldsEntities = await _personHoldsRepository.GetPersonHoldsAsync(offset, limit);
           
            //Get all hold types with category
            var personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(true) as List<Domain.Base.Entities.Restriction>;
            if (personHoldTypes != null)
            {
                foreach (var holdEntity in personHoldsEntities.Item1)
                {
                    var restriction = personHoldTypes.FirstOrDefault(r => r.Code == holdEntity.RestrictionId);

                    if (restriction != null)
                    {
                        var personId = await _personRepository.GetPersonGuidFromIdAsync(holdEntity.StudentId);
                        var personHoldId = await _personHoldsRepository.GetStudentHoldGuidFromIdAsync(holdEntity.Id);

                        var dtosPersonHold = BuildPersonHold(personHoldId, holdEntity, personHoldTypes, personId,
                            restriction);
                        dtoPersonHolds.Add(dtosPersonHold);
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
            return new Tuple<IEnumerable<Dtos.PersonHold>, int>(dtoPersonHolds, personHoldsEntities.Item2);
        }

        /// <summary>
        /// Retrieves all active person holds for hold id
        /// </summary>
        /// <param name="id">id for the person hold</param>
        /// <returns>List of PersonHold for the person</returns>
        public async Task<Dtos.PersonHold> GetPersonHoldAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Person hold id is required");
            }

            CheckUserPersonHoldsViewPermissions();

            Dtos.PersonHold dtoPersonHolds = null;

            PersonRestriction personHoldEntity = await _personHoldsRepository.GetPersonHoldByIdAsync(id);
            if (personHoldEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Did not find any person hold entity with id: {0}", id));
            }
            else
            {
                //Get all hold types with category
                List<Domain.Base.Entities.Restriction> personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(true) as List<Domain.Base.Entities.Restriction>;

                string personId = await _personRepository.GetPersonGuidFromIdAsync(personHoldEntity.StudentId);
                if (string.IsNullOrEmpty(personId))
                {
                    throw new KeyNotFoundException(string.Format("Did not find any person with id: {0}", personHoldEntity.StudentId));
                }

                //Get the person hold type(restriction)
                Restriction restriction = personHoldTypes.Where(r => r.Code == personHoldEntity.RestrictionId).FirstOrDefault();
                if (restriction == null)
                {
                    throw new KeyNotFoundException(string.Format("Did not find any person hold type with id: {0}", personHoldEntity.RestrictionId));
                }

                //Get the guid for the entity, just to make sure guid is generated, get the guid based on entity id
                string personHoldId = await _personHoldsRepository.GetStudentHoldGuidFromIdAsync(personHoldEntity.Id);
                if (string.IsNullOrEmpty(personHoldId))
                {
                    throw new KeyNotFoundException(string.Format("Did not find guid for id: {0}", personHoldEntity.Id));
                }

                dtoPersonHolds = BuildPersonHold(personHoldId, personHoldEntity, personHoldTypes, personId, restriction);
            }

            return dtoPersonHolds;
        }

        /// <summary>
        /// Gets person holds based on personId
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.PersonHold>> GetPersonHoldsAsync(string personId)
        {
            CheckUserPersonHoldsViewPermissions();

            if (string.IsNullOrEmpty(personId))
            {
                return new List<Dtos.PersonHold>();
                //throw new ArgumentNullException("Person id is required");
            }
            List<Domain.Base.Entities.PersonRestriction> personHoldsEntities = await _personHoldsRepository.GetPersonHoldsByPersonIdAsync(personId) as List<Domain.Base.Entities.PersonRestriction>;

            List<Dtos.PersonHold> dtoPersonHolds = new List<Dtos.PersonHold>();

            //Get all hold types with category
            List<Domain.Base.Entities.Restriction> personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(true) as List<Domain.Base.Entities.Restriction>;

            foreach (var holdEntity in personHoldsEntities)
            {
                Dtos.PersonHold dtosPersonHold = null;
                string tempPersonId = await _personRepository.GetPersonGuidFromIdAsync(holdEntity.StudentId);
                Restriction restriction = personHoldTypes.Where(r => r.Code == holdEntity.RestrictionId).FirstOrDefault();
                string personHoldId = await _personHoldsRepository.GetStudentHoldGuidFromIdAsync(holdEntity.Id);
                if (restriction != null)
                {
                    dtosPersonHold = BuildPersonHold(personHoldId, holdEntity, personHoldTypes, tempPersonId, restriction);
                    dtoPersonHolds.Add(dtosPersonHold);
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
            // get user permissions
            CheckUserPersonHoldsDeletePermissions();

            string personHoldId = await _personHoldsRepository.GetStudentHoldIdFromGuidAsync(personHoldsId);

            if (string.IsNullOrEmpty(personHoldId))
            {
                throw new ArgumentNullException(string.Format("Did not find any person hold id with id: {0}", personHoldsId));
            }
            //Log all the warnings
            var warnings = await _personHoldsRepository.DeletePersonHoldsAsync(personHoldId);
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

        /// <summary>
        /// Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague.
        /// </summary>
        private void CheckUserPersonHoldsViewPermissions()
        {
            // access is ok if the current user has the view person-holds permission
            if (!HasPermission(PersonHoldsPermissionCodes.ViewPersonHold))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view person-holds.");
                throw new PermissionsException("User is not authorized to view person-holds.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague.
        /// </summary>
        private void CheckUserPersonHoldsCreateUpdatePermissions()
        {
            // access is ok if the current user has the create/update person-holds permission
            if (!HasPermission(PersonHoldsPermissionCodes.CreateUpdatePersonHold))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create/update person-holds.");
                throw new PermissionsException("User is not authorized to create/update person-holds.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to delete a hold (a.k.a. a record from STUDENT.RESTRICTIONS) in Colleague.
        /// </summary>
        private void CheckUserPersonHoldsDeletePermissions()
        {
            // access is ok if the current user has the delete person-holds permission
            if (!HasPermission(PersonHoldsPermissionCodes.DeletePersonHold))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to delete person-holds.");
                throw new PermissionsException("User is not authorized to delete person-holds.");
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
            CheckUserPersonHoldsCreateUpdatePermissions();

            _personHoldsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            string personId = string.Empty;
            Domain.Base.Entities.Restriction holdType = null;
            string notificationIndicator = string.Empty;
            string personHoldEnityId = string.Empty;            

            //Check id's match, For POST id will be blank & will come in the payload
            if (string.IsNullOrEmpty(id))
            {
                id = personHoldDto.Id;
            }

            //Do required fields & empty or null id check
            CheckForRequiredFields(personHoldDto);

            //For POST this will return null
            personHoldEnityId = await _personHoldsRepository.GetStudentHoldIdFromGuidAsync(personHoldDto.Id);

            //Check start date is before end date
            if (personHoldDto.StartOn > personHoldDto.EndOn)
            {
                throw new InvalidOperationException("The hold start date must be on or before the hold end date.");
            }            

            // get the person ID associated with the incoming guid            
            if (personHoldDto.Person != null && !string.IsNullOrEmpty(personHoldDto.Person.Id))
            {
                var person = await _personRepository.GetPersonByGuidNonCachedAsync(personHoldDto.Person.Id);
                if (person == null)
                {
                    throw new KeyNotFoundException("Person ID associated to id '" + personHoldDto.Person.Id + "' not found");
                }

                if(person != null && !string.IsNullOrEmpty(person.PersonCorpIndicator) && person.PersonCorpIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("The person specified is an organization, not a person.");
                }
                personId = person.Id;
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
                    throw new KeyNotFoundException("Person hold type associated with id '" + personHoldDto.PersonHoldTypeType.Detail.Id + "' was not found");
                }
            }

            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.PersonHoldCategory != null && 
                (personHoldDto.PersonHoldTypeType.Detail == null || string.IsNullOrEmpty(personHoldDto.PersonHoldTypeType.Detail.Id)))
            {
                List<Domain.Base.Entities.Restriction> personHoldTypes = await _referenceDataRepository.GetRestrictionsWithCategoryAsync(true) as List<Domain.Base.Entities.Restriction>;
                holdType = personHoldTypes.FirstOrDefault(h => h.RestIntgCategory.ToString().Equals(personHoldDto.PersonHoldTypeType.PersonHoldCategory.ToString(), StringComparison.InvariantCultureIgnoreCase));
                
                if (holdType == null)
                {
                    throw new KeyNotFoundException("Person hold type associated with category '" + personHoldDto.PersonHoldTypeType.PersonHoldCategory.ToString() + "' was not found");
                }
            }

            //On a POST or PUT, if the enumeration here is "notify" we can store "Y" to STR.PRTL.DISPLAY.FLAG. Otherwise, we can leave STR.PRTL.DISPLAY.FLAG blank.
            notificationIndicator = (personHoldDto.NotificationIndicator == Dtos.NotificationIndicatorType.Notify) ? "Y" : string.Empty;

            //Construct the request for PUT or POST
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

            Dtos.PersonHold personHoldDtos = await this.GetPersonHoldAsync(response.PersonHoldGuid);

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
            dtoPersonHolds.Comment = personHoldEntity.Comment;
            dtoPersonHolds.EndOn = personHoldEntity.EndDate;
            dtoPersonHolds.Person = new Dtos.GuidObject2(personId);
            dtoPersonHolds.PersonHoldTypeType = ConvertCategoryToPersonHoldTypeType(personHoldType, personHoldTypes);
            if (personHoldEntity.StartDate != null) dtoPersonHolds.StartOn = personHoldEntity.StartDate.Value;
            dtoPersonHolds.NotificationIndicator = ConvertDisplayFlagToNotificationIndicatorType(personHoldEntity.NotificationIndicator);
            return dtoPersonHolds;
        }
        #endregion

        #region Convert  and Helper Methods
        /// <summary>
        /// Check for required fields and empty id fields
        /// </summary>
        /// <param name="personHoldDto"></param>
        private static void CheckForRequiredFields(Dtos.PersonHold personHoldDto)
        {
            if (personHoldDto.Person == null)
            {
                throw new ArgumentNullException("personHoldDto.person", "Must provide person for person-holds");
            }

            if (personHoldDto.Person != null && string.IsNullOrEmpty(personHoldDto.Person.Id))
            {
                throw new ArgumentNullException("personHoldDto.person.id", "Must provide person id for person-holds");
            }

            if (personHoldDto.PersonHoldTypeType == null)
            {
                throw new ArgumentNullException("personHoldDto.personHoldTypeType", "Must provide person hold type for person-holds");
            }
            
            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.PersonHoldCategory == null &&
               personHoldDto.PersonHoldTypeType.Detail != null && !string.IsNullOrEmpty(personHoldDto.PersonHoldTypeType.Detail.Id))
            {
                throw new ArgumentNullException("The category is required if the type is included in the payload.");
            }

            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.PersonHoldCategory == null)
            {
                throw new ArgumentNullException("personHoldDto.personHoldTypeType", "Must provide person hold category for person-holds");
            }

            if (personHoldDto.PersonHoldTypeType != null && personHoldDto.PersonHoldTypeType.Detail != null && string.IsNullOrEmpty(personHoldDto.PersonHoldTypeType.Detail.Id))
            {
                throw new ArgumentNullException("personHoldDto.personHoldTypeType.detail.id", "Must provide id if person hold type details is included for person-holds");
            }          

            if (personHoldDto.NotificationIndicator != null && string.IsNullOrEmpty(personHoldDto.NotificationIndicator.Value.ToString()))
            {
                throw new ArgumentNullException("personHoldDto.notificationIndicator", "Must provide value for notification indicator if notification indicator is included for person-holds");
            }

            if (personHoldDto.StartOn == null || (personHoldDto.StartOn != null && string.IsNullOrEmpty(personHoldDto.StartOn.ToString())))
            {
                throw new ArgumentNullException("personHoldDto.startOn", "Must provide start on for person-holds");
            }

            DateTimeOffset startOn;
            DateTimeOffset endOn;            

            if (personHoldDto.StartOn != null && !DateTimeOffset.TryParse(personHoldDto.StartOn.ToString(), out startOn))
            {
                throw new FormatException("Provided startOn date has invalid format");
            }

            if (personHoldDto.EndOn != null && personHoldDto.EndOn.HasValue && !DateTimeOffset.TryParse(personHoldDto.EndOn.Value.ToString(), out endOn) )
            {
                throw new FormatException("Provided endOn date has invalid format");
            }            
        }

        /// <summary>
        /// Converts string displayFlag to Dtos.NotificationIndicatorType
        /// </summary>
        /// <param name="displayFlag">displayFlag</param>
        /// <returns></returns>
        private Dtos.NotificationIndicatorType ConvertDisplayFlagToNotificationIndicatorType(string displayFlag)
        {
            switch (displayFlag)
            {
                case "Y":
                return Dtos.NotificationIndicatorType.Notify;
                default:
                return Dtos.NotificationIndicatorType.Watch;
            }
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

            tempRestriction = personHoldTypes.FirstOrDefault(ht => ht.Code == restriction.Code);
            if (tempRestriction == null)
                throw new KeyNotFoundException("Person hold type was not found.");

            personHoldTypeType.Detail = new Dtos.GuidObject2(tempRestriction.Guid);

            return personHoldTypeType;
        }
        #endregion       
    }
}
