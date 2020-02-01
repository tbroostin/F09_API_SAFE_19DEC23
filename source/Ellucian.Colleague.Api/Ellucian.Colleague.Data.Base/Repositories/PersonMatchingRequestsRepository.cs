// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonMatchingRequestsRepository : BaseColleagueRepository, IPersonMatchingRequestsRepository
    {
        RepositoryException exception = new RepositoryException();
        private string colleagueTimeZone;

        #region Constructor

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public PersonMatchingRequestsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using 24 hours for the cache timeout.
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        #endregion

        #region GET Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaObj"></param>
        /// <param name="personFilter"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>> GetPersonMatchRequestsAsync(int offset, int limit, Domain.Base.Entities.PersonMatchRequest criteriaObj = null,
            string[] filterPersonIds = null, bool bypassCache = false)
        {
            List<Domain.Base.Entities.PersonMatchRequest> entities = new List<Domain.Base.Entities.PersonMatchRequest>();
            Collection<DataContracts.PersonMatchRequest> personMatchRequests = new Collection<DataContracts.PersonMatchRequest>();

            string criteria = string.Empty;
            string[] limitingKeys = new string[] { };
            int totalCount = 0;
            var personLimitingKeys = new List<string>();

            #region  Named query person filter
            if (filterPersonIds != null && filterPersonIds.Any())
            {
                // Select all match requests with matched person IDs and then, if they are
                // in the list from the SAVE.LIST.PARMS saved list, build limitingKeys.
                var personIds = (await DataReader.SelectAsync("PERSON.MATCH.REQUEST", "WITH PMR.PERSON.ID NE '' BY.EXP PMR.PERSON.ID SAVING PMR.PERSON.ID")).Distinct();
                if (personIds != null && personIds.Any())
                {
                    foreach (var personId in personIds)
                    {
                        if (filterPersonIds.Contains(personId))
                        {
                            if (!personLimitingKeys.Contains(personId))
                            {
                                personLimitingKeys.Add(personId);
                            }
                        }
                    }
                }
                if (personLimitingKeys == null || !personLimitingKeys.Any())
                {
                    return new Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>(new List<Domain.Base.Entities.PersonMatchRequest>(), 0);
                }
                limitingKeys = await DataReader.SelectAsync("PERSON.MATCH.REQUEST", "WITH PMR.PERSON.ID = '?'", personLimitingKeys.ToArray());
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>(new List<Domain.Base.Entities.PersonMatchRequest>(), 0);
                }
            }

            #endregion

            #region criteria filter

            if (criteriaObj != null)
            {
                // Originator Filter
                if (!string.IsNullOrWhiteSpace(criteriaObj.Originator))
                {
                    criteria = string.Format("WITH PMR.ORIGINATOR EQ '{0}'", criteriaObj.Originator);
                }

                // Outcomes Type and Status
                if (criteriaObj.Outcomes != null && criteriaObj.Outcomes.Any())
                {
                    foreach (var outcome in criteriaObj.Outcomes)
                    {
                        if (outcome.Type != PersonMatchRequestType.NotSet)
                        {
                            if (!string.IsNullOrEmpty(criteria))
                            {
                                criteria += " AND ";
                            }
                            if (outcome.Type == PersonMatchRequestType.Initial)
                            {
                                if (outcome.Status != PersonMatchRequestStatus.NotSet)
                                {
                                    criteria += string.Format("WITH PMR.INITIAL.STATUS EQ '{0}'", ConvertMatchStatusString(outcome.Status));
                                }
                                else
                                {
                                    criteria += "WITH PMR.INITIAL.STATUS NE ''";
                                }
                            }
                            else
                            {
                                if (outcome.Status != PersonMatchRequestStatus.NotSet)
                                {
                                    criteria += string.Format("WITH PMR.FINAL.STATUS EQ '{0}'", ConvertMatchStatusString(outcome.Status));
                                }
                                else
                                {
                                    criteria += "WITH PMR.FINAL.STATUS NE ''";
                                }
                            }
                        }
                        else
                        {
                            if (outcome.Status != PersonMatchRequestStatus.NotSet)
                            {
                                if (!string.IsNullOrEmpty(criteria))
                                {
                                    criteria += " AND ";
                                }
                                criteria += string.Format("WITH PMR.INITIAL.STATUS EQ '{0}' OR WITH PMR.FINAL.STATUS EQ '{0}'", ConvertMatchStatusString(outcome.Status));
                            }
                        }
                    }
                }
            }

            #endregion

            limitingKeys = await DataReader.SelectAsync("PERSON.MATCH.REQUEST", limitingKeys, criteria);

            if (limitingKeys == null || !limitingKeys.Any())
            {
                return new Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>(new List<Domain.Base.Entities.PersonMatchRequest>(), 0);
            }

            totalCount = limitingKeys.Count();
            var sublist = limitingKeys.Skip(offset).Take(limit).Distinct().ToArray();

            if(sublist == null || !sublist.Any())
            {
                return new Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>(new List<Domain.Base.Entities.PersonMatchRequest>(), 0);
            }

            personMatchRequests = await DataReader.BulkReadRecordAsync<DataContracts.PersonMatchRequest>(sublist);
            if (personMatchRequests == null)
            {
                return new Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>(new List<Domain.Base.Entities.PersonMatchRequest>(), 0);
            }

            foreach (var matchRequest in personMatchRequests)
            {
                Domain.Base.Entities.PersonMatchRequest entity = BuildEntity(matchRequest);
                entities.Add(entity);
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return entities.Any() ? new Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>(entities, totalCount) :
                new Tuple<IEnumerable<Domain.Base.Entities.PersonMatchRequest>, int>(new List<Domain.Base.Entities.PersonMatchRequest>(), 0);
        }

        /// <summary>
        /// Gets person-match-requests by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.PersonMatchRequest> GetPersonMatchRequestsByIdAsync(string id, bool bypassCache = false)
        {
            if(string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "person-match-requests guid is required.");
            }
            var personMatchRequestId = new GuidLookup(id);
            var entity = await DataReader.ReadRecordAsync<DataContracts.PersonMatchRequest>("PERSON.MATCH.REQUEST", personMatchRequestId);
            if (entity == null)
            {
                throw new KeyNotFoundException("No person-matching-requests was found for guid '" + id + "'.");
            }

            var entities = BuildEntity(entity);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return entities;
        }

        #endregion

        #region POST methods for initiations prospects

        /// <summary>
        ///  CreatePersonMatchingRequestsInitiationsProspectsAsync
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.PersonMatchRequest> CreatePersonMatchingRequestsInitiationsProspectsAsync(PersonMatchRequestInitiation entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("PersonMatchRequestInitiation", "Must provide a Person Match Request Initiation Entity to create.");
            }

            var extendedDataTuple = GetEthosExtendedDataLists();

            CreatePersonMatchRequestRequest createPersonMatchRequest = BuildPersonMatchRequestInitiationRequest(entity);
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createPersonMatchRequest.ExtendedNames = extendedDataTuple.Item1;
                createPersonMatchRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            CreatePersonMatchRequestResponse createPersonMatchResponse = await transactionInvoker.ExecuteAsync<CreatePersonMatchRequestRequest, CreatePersonMatchRequestResponse>(createPersonMatchRequest);

            // If there is any error message - throw an exception
            if (createPersonMatchResponse.CreatePersonMatchErrors != null && createPersonMatchResponse.CreatePersonMatchErrors.Any())
            {
                var errorMessage = "Error occurred updating person match request";
                logger.Error(errorMessage);
                foreach (var message in createPersonMatchResponse.CreatePersonMatchErrors)
                {
                    //collect all the failure messages
                    if (message != null && !string.IsNullOrEmpty(message.ErrorMessages))
                    {
                        exception.AddError(new RepositoryError(message.ErrorCodes, message.ErrorMessages) { Id = createPersonMatchResponse.Guid, SourceId =createPersonMatchResponse.PersonMatchRequestId });
                    }
                }
                throw exception;
            }

            var guid = createPersonMatchResponse.Guid;
            return await GetPersonMatchRequestsByIdAsync(guid);
        }

        /// <summary>
        /// Builds an  update admApplication request.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private CreatePersonMatchRequestRequest BuildPersonMatchRequestInitiationRequest(PersonMatchRequestInitiation entity)
        {
            try
            {
                var request = new CreatePersonMatchRequestRequest()
                {
                    Guid = entity.Guid,
                    PersonMatchRequestId = entity.RecordKey,
                    RequestType = "PROSPECT",
                    FirstName = entity.FirstName,
                    MiddleName = entity.MiddleName,
                    LastName = entity.LastName,
                    FormerFirstName = entity.BirthFirstName,
                    FormerMiddleName = entity.BirthMiddleName,
                    FormerLastName = entity.BirthLastName,
                    ChosenFirstName = entity.ChosenFirstName,
                    ChosenMiddleName = entity.ChosenMiddleName,
                    ChosenLastName = entity.ChosenLastName,
                    Gender = entity.Gender,
                    BirthDate = entity.BirthDate,
                    TaxId = entity.TaxId,
                    AddressLines = entity.AddressLines,
                    AddressType = entity.AddressType,
                    City = entity.City,
                    State = entity.State,
                    Zip = entity.Zip,
                    Locality = entity.Locality,
                    Region = entity.Region,
                    SubRegion = entity.SubRegion,
                    PostalCode = entity.PostalCode,
                    Country = entity.Country,
                    County = entity.County,
                    DeliveryPoint = entity.DeliveryPoint,
                    CorrectionDigit = entity.CorrectionDigit,
                    CarrierRoute = entity.CarrierRoute
                };

                // Alternative IDs
                if (entity.AlternateIds != null)
                {
                    request.CreatePersonMatchAltIds = new List<CreatePersonMatchAltIds>();
                    foreach (var alternateId in entity.AlternateIds)
                    {
                        request.CreatePersonMatchAltIds.Add(new CreatePersonMatchAltIds()
                        {
                            AltIds = alternateId.Id,
                            AltIdTypes = alternateId.Type
                        });
                    }
                }

                // Emails
                if (!string.IsNullOrEmpty(entity.Email))
                {
                    request.CreatePersonMatchEmails = new List<CreatePersonMatchEmails>()
                    {
                        new CreatePersonMatchEmails()
                        {
                            EmailAddresses = entity.Email,
                            EmailTypes = entity.EmailType
                        }
                    };
                }

                // Phones
                if (!string.IsNullOrEmpty(entity.Phone))
                {
                    request.CreatePersonMatchPhones = new List<CreatePersonMatchPhones>()
                    {
                        new CreatePersonMatchPhones()
                        {
                            PhoneNumbers = entity.Phone,
                            PhoneTypes = entity.PhoneType
                        }
                    };
                }

                return request;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("Bad.Data", e.Message) { Id = entity.Guid, SourceId = entity.RecordKey });
                throw exception;
            }
        }

        #endregion

        #region Build Method

        /// <summary>
        /// Builds PersonMatchRequest entity.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Domain.Base.Entities.PersonMatchRequest BuildEntity(DataContracts.PersonMatchRequest source)
        {
            Domain.Base.Entities.PersonMatchRequest entity = null;
            string guid = source.RecordGuid;
            if (string.IsNullOrEmpty(guid))
            {
                exception.AddError(new RepositoryError("Bad.Data", "Invalid Data contract returned from bulk read. Missing GUID")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }
            if (string.IsNullOrEmpty(source.Recordkey))
            {
                exception.AddError(new RepositoryError("Bad.Data", "Invalid Data contract returned from bulk read. Missing Record Key")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }
            if (string.IsNullOrEmpty(source.PmrInitialStatus) && string.IsNullOrEmpty(source.PmrFinalStatus))
            {
                exception.AddError(new RepositoryError("Bad.Data", "Invalid Data contract returned from bulk read. Missing both initial and final statuses.")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }

            entity = new Domain.Base.Entities.PersonMatchRequest()
            {
                Guid = guid,
                RecordKey = source.Recordkey,
                PersonId = source.PmrPersonId,
                Originator = source.PmrOriginator
            };
            if (!string.IsNullOrEmpty(source.PmrInitialStatus))
            {
                PersonMatchRequestType type = PersonMatchRequestType.Initial;
                PersonMatchRequestStatus status = ConvertStringMatchStatus(source.PmrInitialStatus);
                DateTimeOffset date = ConvertMatchDate(source.PmrInitialStatusDate, source.PmrInitialStatusTime);
                entity.AddPersonMatchRequestOutcomes(new PersonMatchRequestOutcomes(type, status, date));
            }
            if (!string.IsNullOrEmpty(source.PmrFinalStatus))
            {
                PersonMatchRequestType type = PersonMatchRequestType.Final;
                PersonMatchRequestStatus status = ConvertStringMatchStatus(source.PmrFinalStatus);
                DateTimeOffset date = ConvertMatchDate(source.PmrFinalStatusDate, source.PmrFinalStatusTime);
                entity.AddPersonMatchRequestOutcomes(new PersonMatchRequestOutcomes(type, status, date));
            }

            return entity;
        }

        private PersonMatchRequestStatus ConvertStringMatchStatus(string status)
        {
            switch (status)
            {
                case ("D"):
                    {
                        return PersonMatchRequestStatus.ExistingPerson;
                    }
                case ("N"):
                    {
                        return PersonMatchRequestStatus.NewPerson;
                    }
                case ("R"):
                    {
                        return PersonMatchRequestStatus.ReviewRequired;
                    }
                default:
                    {
                        return PersonMatchRequestStatus.NotSet;
                    }
            }
        }

        private string ConvertMatchStatusString(PersonMatchRequestStatus status)
        {
            switch (status)
            {
                case (PersonMatchRequestStatus.ExistingPerson):
                    {
                        return "D";
                    }
                case (PersonMatchRequestStatus.NewPerson):
                    {
                        return "N";
                    }
                case (PersonMatchRequestStatus.ReviewRequired):
                    {
                        return "R";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        private DateTimeOffset ConvertMatchDate(DateTime? statusDate, DateTime? statusTime)
        {
            var statusDateAndTime = new DateTimeOffset();
            if (statusDate != null && statusTime != null && statusDate.HasValue && statusTime.HasValue)
            {
                var time = statusTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone);

                statusDateAndTime = statusDate.GetValueOrDefault().Date + time.GetValueOrDefault().TimeOfDay;
            }
            return statusDateAndTime;
        }

        #endregion
    }
}