/*Copyright 2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AidApplicationDemographicsRepository : BaseColleagueRepository, IAidApplicationDemographicsRepository, IEthosExtended
    {
        protected const int AllAidApplicationDemoFilterTimeout = 20; // Clear from cache every 20 minutes
        protected const string AllAidApplicationDemoFilterCache = "AllAidApplicationDemoFilter";

        public AidApplicationDemographicsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<AidApplicationDemographics> GetAidApplicationDemographicsByIdAsync(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "aid-application-demographics ID is required for record retrieval.");
            }

            FaappDemo faappDemoDataContract = await DataReader.ReadRecordAsync<FaappDemo>(id);
            if (faappDemoDataContract == null)
            {
                throw new KeyNotFoundException("No aid-application-demographics was found for ID: " + id);
            }

            return ConvertToAidApplicationDemographics(faappDemoDataContract);
        }

        public async Task<Tuple<IEnumerable<AidApplicationDemographics>, int>> GetAidApplicationDemographicsAsync(int offset, int limit, string personId = "",
            string aidApplicationType = "", string aidYear = "", string applicantAssignedId = "")
        {
            try
            {
                int totalCount = 0;
                string[] subList = null;
                var repositoryException = new RepositoryException();
                string aidApplicationDemoCacheKey = CacheSupport.BuildCacheKey(AllAidApplicationDemoFilterCache, personId, aidApplicationType, aidYear, applicantAssignedId);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       aidApplicationDemoCacheKey,
                       "FAAPP.DEMO",
                       offset,
                       limit,
                       AllAidApplicationDemoFilterTimeout,
                       () =>
                       {
                           return GetAidApplicationDemoFilterCriteria(personId, aidApplicationType, aidYear, applicantAssignedId);
                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<AidApplicationDemographics>, int>(new List<AidApplicationDemographics>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;
                if (subList != null && subList.Any())
                {
                    var faappDemoDataContracts = await DataReader.BulkReadRecordAsync<FaappDemo>(subList);
                    var aidApplicationDemographicsEntities = new List<AidApplicationDemographics>();
                    if (faappDemoDataContracts != null && faappDemoDataContracts.Count > 0)
                    {
                        foreach (var item in faappDemoDataContracts)
                        {
                            aidApplicationDemographicsEntities.Add(ConvertToAidApplicationDemographics(item));
                        }
                    }
                    return new Tuple<IEnumerable<AidApplicationDemographics>, int>(aidApplicationDemographicsEntities, totalCount);
                }
                else
                {
                    return new Tuple<IEnumerable<AidApplicationDemographics>, int>(new List<AidApplicationDemographics>(), totalCount);
                }

            }
            catch (ArgumentException e)
            {
                throw;
            }
            catch (RepositoryException e)
            {
                throw;
            }
        }


        /// <summary>
        /// Get criteria and limiting list.
        /// </summary>
        /// <returns></returns>
        private async Task<CacheSupport.KeyCacheRequirements> GetAidApplicationDemoFilterCriteria(string personId, string aidApplicationType, string aidYear, string applicantAssignedId)
        {
            string criteria = string.Empty;
            var criteriaBuilder = new StringBuilder();
            List<string> aidApplicationDemoLimitingKeys = new List<string>();
            if (!string.IsNullOrEmpty(personId))
            {
                criteriaBuilder.AppendFormat("WITH FAAD.STUDENT.ID = '{0}'", personId);
            }

            if (!string.IsNullOrEmpty(aidApplicationType))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAAD.TYPE = '{0}'", aidApplicationType);
            }

            if (!string.IsNullOrEmpty(aidYear))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAAD.YEAR = '{0}'", aidYear);
            }

            if (!string.IsNullOrEmpty(applicantAssignedId))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FAAD.ASSIGNED.ID = '{0}'", applicantAssignedId);
            }

            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }

            aidApplicationDemoLimitingKeys = (await DataReader.SelectAsync("FAAPP.DEMO", criteria)).ToList();
            if (aidApplicationDemoLimitingKeys == null || !aidApplicationDemoLimitingKeys.Any())
            {
                return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
            }
            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }
            return new CacheSupport.KeyCacheRequirements()
            {
                limitingKeys = aidApplicationDemoLimitingKeys,
                criteria = criteria
            };

        }

        public async Task<AidApplicationDemographics> CreateAidApplicationDemographicsAsync(AidApplicationDemographics aidApplicationDemographicsEntity)
        {
            if (aidApplicationDemographicsEntity == null)
                throw new ArgumentNullException("aidApplicationDemographicsEntity", "Must provide a aidApplicationDemographicsEntity to create.");
            var repositoryException = new RepositoryException();
            AidApplicationDemographics createdEntity = null;
            UpdateAidApplDemoRequest createRequest;
            try
            {
                createRequest = await BuildUpdateAidApplDemoRequest(aidApplicationDemographicsEntity, false);
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationDemographicsEntity.Id,
                        SourceId = aidApplicationDemographicsEntity.Id
                    });
                throw repositoryException;
            }
            if (createRequest != null)
            {

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    createRequest.ExtendedNames = extendedDataTuple.Item1;
                    createRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                var createResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplDemoRequest, UpdateAidApplDemoResponse>(createRequest);

                if (createResponse != null && createResponse.UpdateFaappDemoErrors.Any())
                {
                    foreach (var error in createResponse.UpdateFaappDemoErrors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMessage))
                        {
                            SourceId = createResponse.DemoRecordKey,
                            Id = createResponse.DemoRecordKey

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        createdEntity = await GetAidApplicationDemographicsByIdAsync(createResponse.DemoRecordKey);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationDemographicsEntity.Id,
                           SourceId = aidApplicationDemographicsEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }

            // get the newly created record from the database
            return createdEntity;
        }

        /// <summary>
        /// Update an existing AidApplicationDemographics domain entity
        /// </summary>
        /// <param name="aidApplicationDemographicsEntity">AidApplicationDemographics domain entity</param>
        /// <returns>AidApplicationDemographics domain entity</returns>
        public async Task<AidApplicationDemographics> UpdateAidApplicationDemographicsAsync(AidApplicationDemographics aidApplicationDemographicsEntity)
        {
            if (aidApplicationDemographicsEntity == null)
                throw new ArgumentNullException("aidApplicationDemographicsEntity", "Must provide a aidApplicationDemographicsEntity to update.");
            if (string.IsNullOrWhiteSpace(aidApplicationDemographicsEntity.Id))
                throw new ArgumentNullException("aidApplicationDemographicsEntity", "Must provide the id of the aidApplicationDemographicsEntity to update.");

            var repositoryException = new RepositoryException();
            AidApplicationDemographics updatedEntity = null;
            UpdateAidApplDemoRequest updateRequest;
            try
            {
                updateRequest = await BuildUpdateAidApplDemoRequest(aidApplicationDemographicsEntity, true);
                updateRequest.DemoRecordKey = aidApplicationDemographicsEntity.Id;
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationDemographicsEntity.Id,
                        SourceId = aidApplicationDemographicsEntity.Id
                    });
                throw repositoryException;
            }
            if (updateRequest != null)
            {
                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplDemoRequest, UpdateAidApplDemoResponse>(updateRequest);

                if (updateResponse != null && updateResponse.UpdateFaappDemoErrors.Any())
                {
                    foreach (var error in updateResponse.UpdateFaappDemoErrors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMessage))
                        {
                            SourceId = updateResponse.DemoRecordKey,
                            Id = updateResponse.DemoRecordKey

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        updatedEntity = await GetAidApplicationDemographicsByIdAsync(updateResponse.DemoRecordKey);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationDemographicsEntity.Id,
                           SourceId = aidApplicationDemographicsEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }
            // get the updated entity from the database
            return updatedEntity;
        }

        /// <summary>
        /// Create an UpdateAidApplDemoRequest from a AidApplicationDemographics domain entity
        /// </summary>
        /// <param name="aidApplicationDemographicsEntity">AidApplicationDemographics domain entity</param>
        /// <returns>UpdateAidApplDemoRequest transaction object</returns>
        private async Task<UpdateAidApplDemoRequest> BuildUpdateAidApplDemoRequest(AidApplicationDemographics aidApplicationDemographicsEntity, bool isUpdate)
        {
            var request = new UpdateAidApplDemoRequest();

            if (string.IsNullOrWhiteSpace(aidApplicationDemographicsEntity.PersonId))
                throw new ArgumentNullException("aidApplicationDemographicsEntity", "Must provide the person id.");

            var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>(aidApplicationDemographicsEntity.PersonId);
            if (personRecord == null)
            {
                throw new ArgumentNullException(string.Format("person record not found with id {0}.", aidApplicationDemographicsEntity.PersonId));
            }
            if (isUpdate)
            {
                request.DemoRecordKey = aidApplicationDemographicsEntity.Id;
            }
            request.CollStudentId = aidApplicationDemographicsEntity.PersonId;
            request.Year = aidApplicationDemographicsEntity.AidYear;
            request.Type = aidApplicationDemographicsEntity.ApplicationType;
            request.AssignedId = aidApplicationDemographicsEntity.ApplicantAssignedId;
            request.LastName = aidApplicationDemographicsEntity.LastName;
            request.OriginalName = aidApplicationDemographicsEntity.OrigName;
            request.FirstName = aidApplicationDemographicsEntity.FirstName;
            request.MiddleInitial = aidApplicationDemographicsEntity.MiddleInitial;

            if (aidApplicationDemographicsEntity.Address != null)
            {
                var address = aidApplicationDemographicsEntity.Address;
                request.Address = CheckAndAssignValue(address.AddressLine);
                request.City = CheckAndAssignValue(address.City);
                request.State = CheckAndAssignValue(address.State);
                request.Country = CheckAndAssignValue(address.Country);
                request.ZipCode = CheckAndAssignValue(address.ZipCode);
            }

            if (aidApplicationDemographicsEntity.BirthDate.HasValue)
            {
                request.Birthdate = DateTime.SpecifyKind(aidApplicationDemographicsEntity.BirthDate.Value, DateTimeKind.Unspecified);
            }
            request.Phone = CheckAndAssignValue(aidApplicationDemographicsEntity.PhoneNumber);
            request.StudentEmailAddr = CheckAndAssignValue(aidApplicationDemographicsEntity.EmailAddress);

            int? citizenShipStatusType = null;
            if (aidApplicationDemographicsEntity.CitizenshipStatusType.HasValue)
            {
                citizenShipStatusType = (int)aidApplicationDemographicsEntity.CitizenshipStatusType.Value;
            }
            request.Citizenship = citizenShipStatusType.HasValue ? citizenShipStatusType.ToString() : null;
            request.AlternativeNumber = CheckAndAssignValue(aidApplicationDemographicsEntity.AlternatePhoneNumber);
            request.Itin = CheckAndAssignValue(aidApplicationDemographicsEntity.StudentTaxIdNumber);
            return request;
        }

        private static AidApplicationDemographics ConvertToAidApplicationDemographics(FaappDemo faappDemoDataContract)
        {
            AidApplicationDemographics aidApplicationDemographicsEntity = new AidApplicationDemographics(faappDemoDataContract.Recordkey, faappDemoDataContract.FaadStudentId, faappDemoDataContract.FaadYear, faappDemoDataContract.FaadType);
            aidApplicationDemographicsEntity.ApplicantAssignedId = CheckAndAssignValue(faappDemoDataContract.FaadAssignedId);
            aidApplicationDemographicsEntity.LastName = CheckAndAssignValue(faappDemoDataContract.FaadNameLast);
            aidApplicationDemographicsEntity.OrigName = CheckAndAssignValue(faappDemoDataContract.FaadOrigName);
            aidApplicationDemographicsEntity.FirstName = CheckAndAssignValue(faappDemoDataContract.FaadNameFirst);
            aidApplicationDemographicsEntity.MiddleInitial = CheckAndAssignValue(faappDemoDataContract.FaadNameMi);

            if (!string.IsNullOrEmpty(faappDemoDataContract.FaadAddr) || !string.IsNullOrEmpty(faappDemoDataContract.FaadCity)
                || !string.IsNullOrEmpty(faappDemoDataContract.FaadState) || !string.IsNullOrEmpty(faappDemoDataContract.FaadCountry)
                || !string.IsNullOrEmpty(faappDemoDataContract.FaadZip))
            {
                aidApplicationDemographicsEntity.Address = new Domain.FinancialAid.Entities.Address();
                aidApplicationDemographicsEntity.Address.AddressLine = CheckAndAssignValue(faappDemoDataContract.FaadAddr);
                aidApplicationDemographicsEntity.Address.City = CheckAndAssignValue(faappDemoDataContract.FaadCity);
                aidApplicationDemographicsEntity.Address.State = CheckAndAssignValue(faappDemoDataContract.FaadState);
                aidApplicationDemographicsEntity.Address.Country = CheckAndAssignValue(faappDemoDataContract.FaadCountry);
                aidApplicationDemographicsEntity.Address.ZipCode = CheckAndAssignValue(faappDemoDataContract.FaadZip);
            }

            aidApplicationDemographicsEntity.BirthDate = faappDemoDataContract.FaadBirthdate;
            aidApplicationDemographicsEntity.PhoneNumber = CheckAndAssignValue(faappDemoDataContract.FaadPhone);
            aidApplicationDemographicsEntity.EmailAddress = CheckAndAssignValue(faappDemoDataContract.FaadStudentEmailAddr);
            aidApplicationDemographicsEntity.CitizenshipStatusType = GetCitizenshipStatus(faappDemoDataContract.FaadCitizenship);
            aidApplicationDemographicsEntity.AlternatePhoneNumber = CheckAndAssignValue(faappDemoDataContract.FaadAlternateNumber);
            aidApplicationDemographicsEntity.StudentTaxIdNumber = CheckAndAssignValue(faappDemoDataContract.FaadItin);

            return aidApplicationDemographicsEntity;
        }

        private static AidApplicationCitizenshipStatus? GetCitizenshipStatus(string citizenshipStatus)
        {
            if (string.IsNullOrEmpty(citizenshipStatus))
            {
                return null;
            }
            AidApplicationCitizenshipStatus citizenshipStatusType;
            switch (citizenshipStatus)
            {
                case "1":
                    citizenshipStatusType = AidApplicationCitizenshipStatus.Citizen;
                    break;
                case "2":
                    citizenshipStatusType = AidApplicationCitizenshipStatus.NonCitizen;
                    break;
                case "3":
                    citizenshipStatusType = AidApplicationCitizenshipStatus.NotEligible;
                    break;
                default:
                    throw new ApplicationException("Invalid Citizenship status: " + citizenshipStatus.ToString());
            }
            return citizenshipStatusType;
        }

        private static string CheckAndAssignValue(string inputValue)
        {
            return !string.IsNullOrEmpty(inputValue) ? inputValue : null;
        }
    }
}