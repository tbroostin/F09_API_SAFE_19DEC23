/// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
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
        /// Gets all PersonVisa entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PersonVisa>, int>> GetAllPersonVisas2Async(int offset, int limit, string person, List<string> visaTypeCategories, string visaTypeDetail, bool bypassCache)
        {
            //Criteria to filter out the records which has visa type specified in VISA.TYPE column
            string criteria = "WITH VISA.TYPE NE ''";

            if (!string.IsNullOrEmpty(person))
            {
                var entity = await this.GetRecordInfoFromGuidAsync(person);
                //if (entity == null || entity.Entity != "PERSON")
                //{
                //    throw new KeyNotFoundException(string.Format("No person visa information for id {0} Key not found.", person));
                //}
                //var personCriteria = string.Format(" AND WITH @ID EQ '{0}'", entity.PrimaryKey);
                //criteria = string.Concat(criteria, personCriteria);

                if (entity != null && entity.Entity == "PERSON")
                {
                    var personCriteria = string.Format(" AND WITH ID EQ '{0}'", entity.PrimaryKey);
                    criteria = string.Concat(criteria, personCriteria);
                }
                else
                {
                    return null;
                }
                
            }
            
            if (!string.IsNullOrEmpty(visaTypeDetail))
            {
                var entity = await this.GetRecordInfoFromGuidAsync(visaTypeDetail);
                //if (entity == null || entity.PrimaryKey != "VISA.TYPES")
                //{
                //    throw new KeyNotFoundException(string.Format("No person visa information for id {0} Key not found.", visaTypeDetail));
                //}
                //var personCriteria = string.Format(" AND WITH VISA.TYPE EQ '{0}'", entity.SecondaryKey);
                //criteria = string.Concat(criteria, personCriteria);

                if (entity != null && entity.PrimaryKey == "VISA.TYPES")
                {
                    var personCriteria = string.Format(" AND WITH VISA.TYPE EQ '{0}'", entity.SecondaryKey);
                    criteria = string.Concat(criteria, personCriteria);
                }
                else
                {
                    return null;
                }
            }
            if (visaTypeCategories != null && visaTypeCategories.Any() && visaTypeCategories.Count > 0)
            {
                criteria = string.Concat(criteria, " AND WITH VISA.TYPE EQ '?'");
            }

            var personIds = await DataReader.SelectAsync("PERSON", criteria, visaTypeCategories != null ? visaTypeCategories.ToArray() : null);
            if (!personIds.Any() && personIds.Count() == 0)
            {
                return null;
            }
            var foreignPersonIds = await DataReader.SelectAsync("FOREIGN.PERSON", personIds, "");
            //var personIds = new List<string>();

            //if (!string.IsNullOrEmpty(visaTypeDetail))
            //{
            //    personIds = await DataReader.SelectAsync("PERSON", criteria);
            //}
            //else if (visaTypeCategories.Any() && visaTypeCategories.Count > 0)
            //{
            //    personIds = await DataReader.SelectAsync("PERSON", criteria, visaTypeCategories.ToArray());
            //}
            //else
            //{
            //    personIds = await DataReader.SelectAsync("PERSON", criteria);
            //}
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
        /// Gets person visa record by Id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>PersonVisa</returns>
        public async Task<PersonVisa> GetPersonVisaByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide an id to get the person visa.");
            }

            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null || entity.Entity != "FOREIGN.PERSON")             
            {
                throw new KeyNotFoundException("No person visa information for id " + id + ".  Key not found.");
            }

            string personKey = entity.PrimaryKey;

            ForeignPerson foreignPersonContract = await DataReader.ReadRecordAsync<ForeignPerson>(personKey, false);
            if (foreignPersonContract == null)
            {
                throw new KeyNotFoundException("No person visa information for id " + personKey + ". FOREIGN.PERSON not found.");
            }

            Ellucian.Colleague.Data.Base.DataContracts.Person personContract = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(personKey, false);
            if (personContract == null)
            {
                throw new KeyNotFoundException("No person visa information for id " + personKey + ". PERSON not found.");
            }

            PersonVisa personVisa = BuildPersonVisa(foreignPersonContract, personContract);
            return personVisa;
        }
        #endregion

        #region PUT & POST
        /// <summary>
        /// Updates person visa information
        /// </summary>
        /// <param name="personVisaRequest">Domain.Base.Entities.PersonVisaRequest</param>
        /// <returns>Domain.Base.Entities.PersonVisaResponse</returns>
        public async Task<Domain.Base.Entities.PersonVisaResponse> UpdatePersonVisaAsync(Domain.Base.Entities.PersonVisaRequest personVisaRequest)
        {
            UpdatePersonVisaRequest updateRequest = new UpdatePersonVisaRequest();
            updateRequest.EntryDate = personVisaRequest.EntryDate;
            updateRequest.ExpireDate = personVisaRequest.ExpireDate;
            updateRequest.IssueDate = personVisaRequest.IssueDate;
            updateRequest.PersonId = personVisaRequest.PersonId;
            updateRequest.RequestDate = personVisaRequest.RequestDate;
            updateRequest.Status = personVisaRequest.Status;
            updateRequest.StrGuid = personVisaRequest.StrGuid;
            updateRequest.VisaNo = personVisaRequest.VisaNo;
            updateRequest.VisaType = personVisaRequest.VisaType;

            var extendedDataTuple = GetEthosExtendedDataLists();

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            UpdatePersonVisaResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdatePersonVisaRequest, UpdatePersonVisaResponse>(updateRequest);

            if (updateResponse.VisaErrorMessages.Any())
            {
                var errorMessage = string.Empty;
                foreach (var message in updateResponse.VisaErrorMessages)
                {
                    errorMessage = string.Format("Error occurred updating person visa '{0} {1}'", personVisaRequest.StrGuid, personVisaRequest.PersonId);
                    errorMessage += string.Join(Environment.NewLine, message.ErrorMsg);
                    logger.Error(errorMessage.ToString());
                }
                throw new InvalidOperationException(errorMessage);
            }

            Domain.Base.Entities.PersonVisaResponse personVisaResponse = new Domain.Base.Entities.PersonVisaResponse();
            personVisaResponse.PersonId = updateResponse.PersonId;
            personVisaResponse.StrGuid = updateResponse.StrGuid;

            return personVisaResponse;

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
                response.DeleteVisaErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCode, e.ErrorMsg)));
                throw exception;
            }
        }
        #endregion

        #region Other methods
        /// <summary>
        /// Builds PersonVisa entity
        /// </summary>
        /// <param name="foreignPersonContract">ForeignPerson</param>
        /// <param name="personContract">Ellucian.Colleague.Data.Base.DataContracts.Person</param>
        /// <returns>PersonVisa</returns>
        private PersonVisa BuildPersonVisa(ForeignPerson foreignPersonContract, Ellucian.Colleague.Data.Base.DataContracts.Person personContract)
        {
            try
            {
                PersonVisa personVisa = new PersonVisa(personContract.Recordkey, personContract.VisaType);

                personVisa.Guid = foreignPersonContract.RecordGuid;
                personVisa.PersonGuid = personContract.RecordGuid;
                personVisa.VisaNumber = foreignPersonContract.FperVisaNo;
                personVisa.RequestDate = foreignPersonContract.FperVisaRequestDate;
                personVisa.IssueDate = personContract.VisaIssuedDate;
                personVisa.ExpireDate = personContract.VisaExpDate;
                personVisa.EntryDate = personContract.PersonCountryEntryDate;
                return personVisa;
            }
            catch
            {
                throw new KeyNotFoundException("No person visa information for id " + personContract.Recordkey + ". Visa Type not defined.");
            }
        }
        #endregion
    }
}