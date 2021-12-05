/// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonVisaRepository :BaseColleagueRepository, IPersonVisaRepository
    {

        const string AllPersonVisasCache = "AllPersonVisas";
        const int AllPersonVisasCacheTimeout = 20; // Clear from cache every 20 minutes
        RepositoryException exception = null;

        #region ..ctor
        public PersonVisaRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 Cache Timeout Value for data that changes rarely.
            CacheTimeout = Level1CacheTimeoutValue;
        }
        #endregion

        #region Get

        /// <summary>
        /// Gets all PersonVisa entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PersonVisa>, int>> GetAllPersonVisasAsync(int offset, int limit, string person, bool bypassCache)
        {
            //Criteria to filter out the records which has visa type specified in VISA.TYPE column
            string criteria = "WITH VISA.TYPE NE ''";

            if (!string.IsNullOrEmpty(person))
            {
                var entity = await this.GetRecordInfoFromGuidAsync(person);
                if (entity == null || entity.Entity != "PERSON")
                {
                    //throw new KeyNotFoundException(string.Format("No person visa information for id {0} Key not found.", person));
                    return null;
                }
                var personCriteria = string.Format(" AND WITH @ID EQ '{0}'", entity.PrimaryKey);
                criteria = string.Concat(criteria, personCriteria);
            }
            var personIds = await DataReader.SelectAsync("PERSON", criteria);
            var foreignPersonIds = await DataReader.SelectAsync("FOREIGN.PERSON", personIds, "");

            int totalCount = foreignPersonIds.Count();

            Array.Sort(foreignPersonIds);

            var subList = foreignPersonIds.Skip(offset).Take(limit).ToArray();

            var personContracts = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);

            var foreignPersonContracts = await DataReader.BulkReadRecordAsync<ForeignPerson>("FOREIGN.PERSON", subList);            

            List<PersonVisa> personVisas = new List<PersonVisa>();

            foreach (var personContract in personContracts)
            {
                var foreignPersonContract = foreignPersonContracts.FirstOrDefault(i => i.Recordkey.Equals(personContract.Recordkey));
                PersonVisa personVisa = BuildPersonVisa(foreignPersonContract, personContract);
                personVisas.Add(personVisa);
            }

            return personVisas.Any() ? new Tuple<IEnumerable<PersonVisa>, int>(personVisas, totalCount) : null;
        }

        /// <summary>
        /// Gets a collection of PersonVisa domain entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="person"></param>
        /// <param name="visaTypeCategories"></param>
        /// <param name="visaTypeDetail"></param>
        /// <param name="bypassCache"></param>
        /// <returns>ollection of PersonVisa domain entities</returns>
        public async Task<Tuple<IEnumerable<PersonVisa>, int>> GetAllPersonVisas2Async(int offset, int limit, string person, List<string> visaTypeCategories, string visaTypeDetail, bool bypassCache)
        {
            var personVisas = new List<PersonVisa>();
            int totalCount = 0;
            string[] subList = null;
            var studentAcademicPeriodsCacheKey = CacheSupport.BuildCacheKey(AllPersonVisasCache, person, visaTypeCategories, visaTypeDetail);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
               this,
               ContainsKey,
               GetOrAddToCacheAsync,
               AddOrUpdateCacheAsync,
               transactionInvoker,
               studentAcademicPeriodsCacheKey,
               "FOREIGN.PERSON",
               offset,
               limit,
               AllPersonVisasCacheTimeout,
               async () =>
               {
                   //Criteria to filter out the records which has visa type specified in VISA.TYPE column
                   string criteria = "WITH VISA.TYPE NE ''";

                   if (!string.IsNullOrEmpty(person))
                   {
                       var entity = await this.GetRecordInfoFromGuidAsync(person);

                       if (entity != null && entity.Entity == "PERSON")
                       {
                           var personCriteria = string.Format(" AND WITH ID EQ '{0}'", entity.PrimaryKey);
                           criteria = string.Concat(criteria, personCriteria);
                       }
                       else
                       {
                           return new CacheSupport.KeyCacheRequirements()
                           {
                               NoQualifyingRecords = true
                           };
                       }
                   }

                   if (!string.IsNullOrEmpty(visaTypeDetail))
                   {
                       var entity = await this.GetRecordInfoFromGuidAsync(visaTypeDetail);

                       if (entity != null && entity.PrimaryKey == "VISA.TYPES")
                       {
                           var personCriteria = string.Format(" AND WITH VISA.TYPE EQ '{0}'", entity.SecondaryKey);
                           criteria = string.Concat(criteria, personCriteria);
                       }
                       else
                       {
                           return new CacheSupport.KeyCacheRequirements()
                           {
                               NoQualifyingRecords = true
                           };
                       }
                   }

                   if (visaTypeCategories != null && visaTypeCategories.Any() && visaTypeCategories.Count > 0)
                   {
                       criteria = string.Concat(criteria, " AND WITH VISA.TYPE EQ '?'");
                   }

                   var personIds = await DataReader.SelectAsync("PERSON", criteria, visaTypeCategories != null ? visaTypeCategories.ToArray() : null);
                   if (!personIds.Any() && personIds.Count() == 0)
                   {                    
                       return new CacheSupport.KeyCacheRequirements()
                       {
                           NoQualifyingRecords = true
                       }; 
                   }

                   var requirements = new CacheSupport.KeyCacheRequirements()
                   {
                       limitingKeys = personIds.Distinct().ToList(),
                   };

                   return requirements;

               });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<PersonVisa>, int>(personVisas, totalCount);
            }

            subList = keyCache.Sublist.ToArray();

            totalCount = keyCache.TotalCount.Value;
            
            var personContracts = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);

            if (personContracts == null)
            {
                return new Tuple<IEnumerable<PersonVisa>, int>(personVisas, totalCount);
            }

            var foreignPersonContracts = await DataReader.BulkReadRecordAsync<ForeignPerson>("FOREIGN.PERSON", subList);


            foreach (var personContract in personContracts)
            {
                try
                {
                    ForeignPerson foreignPersonContract = null;
                    if (foreignPersonContracts != null)
                    {
                        foreignPersonContract = foreignPersonContracts.FirstOrDefault(i => i.Recordkey.Equals(personContract.Recordkey));
                    }
                    personVisas.Add(BuildPersonVisa(foreignPersonContract, personContract));
                }
                catch (Exception ex)
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                    {
                        SourceId = personContract.Recordkey,
                        Id = personContract.RecordGuid
                    });
                }
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return personVisas.Any() ? new Tuple<IEnumerable<PersonVisa>, int>(personVisas, totalCount) : null;
        }

        /// <summary>
        /// Gets PersonVisa domain entity record by Id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>PersonVisa</returns>
        public async Task<PersonVisa> GetPersonVisaByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide a GUID to get the person visa.");
            }

            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null || entity.Entity != "FOREIGN.PERSON")             
            {
                throw new KeyNotFoundException("No person visa information for GUID " + id + ".");
            }

            var personKey = entity.PrimaryKey;

            var foreignPersonContract = await DataReader.ReadRecordAsync<ForeignPerson>(personKey, false);
            if (foreignPersonContract == null)
            {
                throw new RepositoryException("No FOREIGN.PERSON information found for PERSON id '" + personKey + "'.");
            }

            var personContract = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personKey, false);
            if (personContract == null)
            {
                throw new RepositoryException("No PERSON information for FOREIGN.PERSON id '" + personKey + "'.");
            }

            var personVisa = BuildPersonVisa(foreignPersonContract, personContract);

            return personVisa;
        }
        #endregion

        #region PUT & POST

        /// <summary>
        /// Creates/Updates person visa information
        /// </summary>
        /// <param name="personVisaRequest">Domain.Base.Entities.PersonVisaRequest doamin entity to be created/updated</param>
        /// <returns>Domain.Base.Entities.PersonVisaResponse domain entity</returns>
      
        public async Task<Domain.Base.Entities.PersonVisa> UpdatePersonVisaAsync(Domain.Base.Entities.PersonVisaRequest personVisaRequest)
        {
            if (personVisaRequest == null)
            {
                throw new ArgumentNullException("personVisaRequest", "personVisaRequest is a required parameter.");
            }

            var updateRequest = new UpdatePersonVisaRequest
            {
                EntryDate = personVisaRequest.EntryDate,
                ExpireDate = personVisaRequest.ExpireDate,
                IssueDate = personVisaRequest.IssueDate,
                PersonId = personVisaRequest.PersonId,
                RequestDate = personVisaRequest.RequestDate,
                Status = personVisaRequest.Status,
                StrGuid = personVisaRequest.StrGuid,
                VisaNo = personVisaRequest.VisaNo,
                VisaType = personVisaRequest.VisaType
            };

            var extendedDataTuple = GetEthosExtendedDataLists();

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<UpdatePersonVisaRequest, UpdatePersonVisaResponse>(updateRequest);

            if (updateResponse == null)
            {               
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Create.Update.Exception", "An unexpected error occurred creating/updating person visa")
                {
                    SourceId = personVisaRequest.PersonId,
                    Id = personVisaRequest.StrGuid
                });

                throw exception;
            }

            if (updateResponse.VisaErrorMessages.Any())
            {               
                var exception = new RepositoryException();
                foreach (var error in updateResponse.VisaErrorMessages)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMsg))
                    {
                        SourceId = personVisaRequest.PersonId,
                        Id = personVisaRequest.StrGuid
                    });
                }
                throw exception;
            }    

            var foreignPersonContract = await DataReader.ReadRecordAsync<ForeignPerson>(updateResponse.PersonId, false);
            if (foreignPersonContract == null)
            {
                throw new RepositoryException("No FOREIGN.PERSON information found for PERSON id '" + updateResponse.PersonId + "'.");
            }

            var personContract = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(updateResponse.PersonId, false);
            if (personContract == null)
            {
                throw new RepositoryException("No PERSON information for FOREIGN.PERSON id '" + updateResponse.PersonId + "'.");
            }

            var personVisa = BuildPersonVisa(foreignPersonContract, personContract);

            return personVisa;

        }

        #endregion

        #region DELETE
        /// <summary>
        /// Erases visa related infor from the respective columns, its not a hard delete rather soft delete
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="personId">personId</param>
        public async Task DeletePersonVisaAsync(string id, string personId)
        {
            var request = new DeletePersonVisaRequest()
            {
                StrGuid = id,
                PersonId = personId
            };

            //Delete
            var response = await transactionInvoker.ExecuteAsync<DeletePersonVisaRequest, DeletePersonVisaResponse>(request);

            //if there are any errors throw
            if (response.DeleteVisaErrors.Any())
            {
                var exception = new RepositoryException("Errors encountered while deleting person visa: " + id);
                response.DeleteVisaErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCode) ? "" : e.ErrorCode, e.ErrorMsg)));
                throw exception;
            }
        }
        #endregion

        #region Other methods

        /// <summary>
        /// Builds PersonVisa domain entity
        /// </summary>
        /// <param name="foreignPersonContract">ForeignPerson</param>
        /// <param name="personContract">Ellucian.Colleague.Data.Base.DataContracts.Person</param>
        /// <returns>PersonVisa</returns>
        private PersonVisa BuildPersonVisa(ForeignPerson foreignPersonContract, Ellucian.Colleague.Data.Base.DataContracts.Person personContract)
        {
            if (foreignPersonContract == null)
            {
                throw new RepositoryException("No FOREIGN.PERSON information found for PERSON id '" + personContract != null ? personContract.Recordkey : "" + "'.");
            }
            if (personContract == null)
            {
                throw new RepositoryException("No PERSON information for FOREIGN.PERSON id '" + foreignPersonContract != null ? foreignPersonContract.Recordkey : "" + "'.");
            }
            if (foreignPersonContract != null && string.IsNullOrEmpty(foreignPersonContract.RecordGuid))
            {
                throw new RepositoryException("No GUID found for FOREIGN.PERSON id '" + foreignPersonContract.Recordkey + "'.");
            }
            try
            {
                return new PersonVisa(personContract.Recordkey, personContract.VisaType)
                {
                    Guid = foreignPersonContract.RecordGuid,
                    PersonGuid = personContract.RecordGuid,
                    VisaNumber = foreignPersonContract.FperVisaNo,
                    RequestDate = foreignPersonContract.FperVisaRequestDate,
                    IssueDate = personContract.VisaIssuedDate,
                    ExpireDate = personContract.VisaExpDate,
                    EntryDate = personContract.PersonCountryEntryDate
                };             
            }
            catch
            {
                throw new RepositoryException(string.Concat("Error retieving person visa information for id '", 
                    personContract != null ? personContract.Recordkey  : "", "'.  Details: " , exception.Message));
            }
        }
        #endregion
    }
}