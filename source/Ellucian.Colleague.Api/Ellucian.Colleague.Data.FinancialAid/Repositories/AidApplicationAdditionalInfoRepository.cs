/*Copyright 2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
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
    public class AidApplicationAdditionalInfoRepository : BaseColleagueRepository, IAidApplicationAdditionalInfoRepository, IEthosExtended
    {
        protected const int AllAidApplicationInfoFilterTimeout = 20; // Clear from cache every 20 minutes
        protected const string AllAidApplicationInfoFilterCache = "AllAidApplicationInfoFilter";
        public AidApplicationAdditionalInfoRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>> GetAidApplicationAdditionalInfoAsync(int offset, int limit, string appDemoId = "", string personId = "",
            string aidApplicationType = "", string aidYear = "", string applicantAssignedId = "")
        {
            try
            {
                int totalCount = 0;
                string[] subList = null;
                var repositoryException = new RepositoryException();
                string aidApplicationInfoCacheKey = CacheSupport.BuildCacheKey(AllAidApplicationInfoFilterCache, appDemoId, personId, aidApplicationType, aidYear, applicantAssignedId);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       aidApplicationInfoCacheKey,
                       "FAAPP.ADDL",
                       offset,
                       limit,
                       AllAidApplicationInfoFilterTimeout,
                       () =>
                       {
                           return GetAidApplicationInfoFilterCriteria(appDemoId, personId, aidApplicationType, aidYear, applicantAssignedId);
                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(new List<AidApplicationAdditionalInfo>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;
                var exception = new RepositoryException();
                if (subList != null && subList.Any())
                {
                    var faappAddlDataContracts = await DataReader.BulkReadRecordAsync<FaappAddl>(subList);
                    var faappDemoDataContracts = await DataReader.BulkReadRecordAsync<FaappDemo>(subList);
                    Dictionary<string, FaappDemo> faapDemoDictionary = new Dictionary<string, FaappDemo>();
                    if (faappDemoDataContracts != null && faappDemoDataContracts.Any())
                    {
                        faapDemoDictionary = faappDemoDataContracts.ToDictionary(x => x.Recordkey);
                    }
                    var aidApplicationAdditionalInfoEntities = new List<AidApplicationAdditionalInfo>();
                    if (faappAddlDataContracts != null && faappAddlDataContracts.Count > 0)
                    {
                        var key = "";
                        foreach (var item in faappAddlDataContracts)
                        {
                            key = item.Recordkey;
                            if (!faapDemoDictionary.ContainsKey(key))
                            {
                                logger.Error("GetAidApplicationAdditionalInfoAsync: Aid application demographics record is missing for record- " + item.Recordkey);
                                exception.AddError(new RepositoryError("FAAPP.DEMO.Record.Not.Found", string.Format("Aid application demographics record is missing for record- " + item.Recordkey)));
                                continue;
                            }

                            var applicationDemoEntity = ConvertToAidApplicationAdditionalInfo(item, faapDemoDictionary[key]);
                            aidApplicationAdditionalInfoEntities.Add(applicationDemoEntity);
                        }
                        if(exception.Errors != null && exception.Errors.Any())
                        {            
                              throw exception;            
                        }
                    }
                    return new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(aidApplicationAdditionalInfoEntities, totalCount);
                }
                else
                {
                    return new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(new List<AidApplicationAdditionalInfo>(), totalCount);
                }

            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (RepositoryException)
            {
                throw;
            }
        }

        public async Task<AidApplicationAdditionalInfo> GetAidApplicationAdditionalInfoByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "aid-application-additional-info ID is required for record retrieval.");
            }

            FaappAddl faappAddl = await DataReader.ReadRecordAsync<FaappAddl>(id);
            if (faappAddl == null)
            {
                throw new KeyNotFoundException("No aid-application-additional-info was found for ID: " + id);
            }

            FaappDemo faappDemo = await DataReader.ReadRecordAsync<FaappDemo>(id);            
            return ConvertToAidApplicationAdditionalInfo(faappAddl, faappDemo);
        }

        /// <summary>
        /// Get criteria and limiting list.
        /// </summary>
        /// <returns></returns>
        private async Task<CacheSupport.KeyCacheRequirements> GetAidApplicationInfoFilterCriteria(string appDemoId, string personId, string aidApplicationType, string aidYear, string applicantAssignedId)
        {
            string criteria = string.Empty;
            var criteriaBuilder = new StringBuilder();
            List<string> aidApplicationInfoLimitingKeys = new List<string>();
            if (!string.IsNullOrEmpty(appDemoId))
            {
                criteriaBuilder.AppendFormat("WITH FAAPP.ADDL.ID = '{0}'", appDemoId);

            }
            if (!string.IsNullOrEmpty(personId))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FADL.STUDENT.ID = '{0}'", personId);
            }

            if (!string.IsNullOrEmpty(aidApplicationType))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FADL.TYPE = '{0}'", aidApplicationType);
            }

            if (!string.IsNullOrEmpty(aidYear))
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                criteriaBuilder.AppendFormat("WITH FADL.YEAR = '{0}'", aidYear);
            }

            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }

            aidApplicationInfoLimitingKeys = (await DataReader.SelectAsync("FAAPP.ADDL", criteria)).ToList();

            if (!string.IsNullOrEmpty(applicantAssignedId) && aidApplicationInfoLimitingKeys != null && aidApplicationInfoLimitingKeys.Any())
            {
                string assignedIdCriteria = string.Format("WITH FAAD.ASSIGNED.ID = '{0}'", applicantAssignedId);
                aidApplicationInfoLimitingKeys = (await DataReader.SelectAsync("FAAPP.DEMO", aidApplicationInfoLimitingKeys.ToArray(), assignedIdCriteria)).ToList();
            }

            if (aidApplicationInfoLimitingKeys == null || !aidApplicationInfoLimitingKeys.Any())
            {
                return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
            }
            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }
            return new CacheSupport.KeyCacheRequirements()
            {
                limitingKeys = aidApplicationInfoLimitingKeys,
                criteria = criteria
            };

        }


        public async Task<AidApplicationAdditionalInfo> CreateAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity)
        {
            if (aidApplicationAdditionalInfoEntity == null)
                throw new ArgumentNullException("aidApplicationAdditionalInfoEntity", "Must provide a aidApplicationAdditionalInfoEntity to create.");
            var repositoryException = new RepositoryException();
            AidApplicationAdditionalInfo createdEntity = null;
            UpdateAidApplAdditionalRequest createRequest;
            try
            {
                createRequest = await BuildUpdateAidApplAdditionalInfoRequestAsync(aidApplicationAdditionalInfoEntity);
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationAdditionalInfoEntity.Id,
                        SourceId = aidApplicationAdditionalInfoEntity.Id
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

                var createResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplAdditionalRequest, UpdateAidApplAdditionalResponse>(createRequest);

                if (createResponse != null && createResponse.UpdateStudentInformationErrors.Any())
                {
                    foreach (var error in createResponse.UpdateStudentInformationErrors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMessage))
                        {
                            SourceId = createRequest.IdemId,
                            Id = createRequest.IdemId

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        createdEntity = await GetAidApplicationAdditionalInfoByIdAsync(createRequest.IdemId);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationAdditionalInfoEntity.Id,
                           SourceId = aidApplicationAdditionalInfoEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }

            // get the newly created record from the database
            return createdEntity;
        }


        /// <summary>
        /// Update an existing AidApplicationAdditionalInfo domain entity
        /// </summary>
        /// <param name="aidApplicationAdditionalInfoEntity">AidApplicationAdditionalInfo domain entity</param>
        /// <returns>AidApplicationAdditionalInfo domain entity</returns>
        public async Task<AidApplicationAdditionalInfo> UpdateAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity)
        {
            if (aidApplicationAdditionalInfoEntity == null)
                throw new ArgumentNullException("aidApplicationAdditionalInfoEntity", "Must provide an aidApplicationAdditionalInfoEntity to update.");
            if (string.IsNullOrWhiteSpace(aidApplicationAdditionalInfoEntity.Id))
                throw new ArgumentNullException("aidApplicationDemographicsEntity", "Must provide the id of the aidApplicationAdditionalInfoEntity to update.");

            var repositoryException = new RepositoryException();
            AidApplicationAdditionalInfo updatedEntity = null;
            UpdateAidApplAdditionalRequest updateRequest;
            try
            {
                updateRequest = await BuildUpdateAidApplAdditionalInfoRequestAsync(aidApplicationAdditionalInfoEntity);
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = aidApplicationAdditionalInfoEntity.Id,
                        SourceId = aidApplicationAdditionalInfoEntity.Id
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

                //write the data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAidApplAdditionalRequest, UpdateAidApplAdditionalResponse>(updateRequest);

                if (updateResponse != null && updateResponse.UpdateStudentInformationErrors.Any())
                {
                    foreach (var error in updateResponse.UpdateStudentInformationErrors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCode, " - ", error.ErrorMessage))
                        {
                            SourceId = updateRequest.IdemId,
                            Id = updateRequest.IdemId

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        updatedEntity = await GetAidApplicationAdditionalInfoByIdAsync(updateRequest.IdemId);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = aidApplicationAdditionalInfoEntity.Id,
                           SourceId = aidApplicationAdditionalInfoEntity.Id
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
        private async Task<UpdateAidApplAdditionalRequest> BuildUpdateAidApplAdditionalInfoRequestAsync(AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity)
        {
            var request = new UpdateAidApplAdditionalRequest();
            if (string.IsNullOrWhiteSpace(aidApplicationAdditionalInfoEntity.PersonId))
                throw new ArgumentNullException("aidApplicationAdditionalInfoEntity", "Must provide the person id.");

            var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>(aidApplicationAdditionalInfoEntity.PersonId);
            if (personRecord == null)
            {
                throw new ArgumentNullException(string.Format("person record not found with id {0}.", aidApplicationAdditionalInfoEntity.PersonId));
            }
            if (aidApplicationAdditionalInfoEntity.Id != "new")
                request.AidAddId = aidApplicationAdditionalInfoEntity.Id;
            request.IdemId = aidApplicationAdditionalInfoEntity.AppDemoId;
            request.Year = aidApplicationAdditionalInfoEntity.AidYear;
            request.Type = aidApplicationAdditionalInfoEntity.ApplicationType;
            request.StudentId = aidApplicationAdditionalInfoEntity.PersonId;
            request.Ssid = aidApplicationAdditionalInfoEntity.StudentStateId;   
            if (aidApplicationAdditionalInfoEntity.FosterCare.HasValue)
            {
                request.FosterCare = aidApplicationAdditionalInfoEntity.FosterCare.Value ? "Y" : "N";
            }            
            request.County = aidApplicationAdditionalInfoEntity.ApplicationCounty;
            request.WardshipState = aidApplicationAdditionalInfoEntity.WardshipState;
            if (aidApplicationAdditionalInfoEntity.ChafeeConsideration.HasValue)
            {
                request.ChafeeConsider = aidApplicationAdditionalInfoEntity.ChafeeConsideration.Value ? "Y" : "N";
            }   
            if (aidApplicationAdditionalInfoEntity.CreateCcpgRecord.HasValue && aidApplicationAdditionalInfoEntity.CreateCcpgRecord == true)
            {
                request.CcpgActive = "Y";
            }
            request.User1 = aidApplicationAdditionalInfoEntity.User1;
            request.User2 = aidApplicationAdditionalInfoEntity.User2;
            request.User3 = aidApplicationAdditionalInfoEntity.User3;
            request.User4 = aidApplicationAdditionalInfoEntity.User4;
            request.User5 = aidApplicationAdditionalInfoEntity.User5;
            request.User6 = aidApplicationAdditionalInfoEntity.User6;
            request.User7 = aidApplicationAdditionalInfoEntity.User7;
            request.User8 = aidApplicationAdditionalInfoEntity.User8;
            request.User9 = aidApplicationAdditionalInfoEntity.User9;
            request.User10 = aidApplicationAdditionalInfoEntity.User10;
            request.User11 = aidApplicationAdditionalInfoEntity.User11;
            request.User12 = aidApplicationAdditionalInfoEntity.User12;
            request.User13 = aidApplicationAdditionalInfoEntity.User13;
            request.User14 = aidApplicationAdditionalInfoEntity.User14;
            request.User15 = aidApplicationAdditionalInfoEntity.User15;
            request.User16 = aidApplicationAdditionalInfoEntity.User16;
            request.User17 = aidApplicationAdditionalInfoEntity.User17;
            request.User18 = aidApplicationAdditionalInfoEntity.User18;
            request.User19 = aidApplicationAdditionalInfoEntity.User19;
            request.User21 = aidApplicationAdditionalInfoEntity.User21;
            return request;
        }
        private static string CheckAndAssignValue(string inputValue)
        {
            return !string.IsNullOrEmpty(inputValue) ? inputValue : null;
        }

        private static AidApplicationAdditionalInfo ConvertToAidApplicationAdditionalInfo(FaappAddl faappAddlDataContract, FaappDemo faappDemo)
        {
            if (faappAddlDataContract == null || faappDemo == null)
            {
                throw new ArgumentNullException("faappAddlDataContract & faappDemo is required.");
            }

            AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity = new AidApplicationAdditionalInfo(faappAddlDataContract.Recordkey, faappAddlDataContract.Recordkey);
            aidApplicationAdditionalInfoEntity.AidYear = CheckAndAssignValue(faappAddlDataContract.FadlYear);
            aidApplicationAdditionalInfoEntity.PersonId = CheckAndAssignValue(faappAddlDataContract.FadlStudentId);
            aidApplicationAdditionalInfoEntity.ApplicationType = CheckAndAssignValue(faappAddlDataContract.FadlType);

            //AssignedId from FaappDemo datacontract
            aidApplicationAdditionalInfoEntity.ApplicantAssignedId = CheckAndAssignValue(faappDemo.FaadAssignedId);

            aidApplicationAdditionalInfoEntity.StudentStateId = CheckAndAssignValue(faappAddlDataContract.FadlSsid);
            if(!string.IsNullOrEmpty(faappAddlDataContract.FadlFosterCare))
            {
                aidApplicationAdditionalInfoEntity.FosterCare = faappAddlDataContract.FadlFosterCare.ToUpper() == "Y";
            }            
            aidApplicationAdditionalInfoEntity.ApplicationCounty = CheckAndAssignValue(faappAddlDataContract.FadlCounty);
            aidApplicationAdditionalInfoEntity.WardshipState = CheckAndAssignValue(faappAddlDataContract.FadlWardshipState);
            if (!string.IsNullOrEmpty(faappAddlDataContract.FadlChafeeConsider))
            {
                aidApplicationAdditionalInfoEntity.ChafeeConsideration = faappAddlDataContract.FadlChafeeConsider.ToUpper() == "Y";
            }            
            if(!string.IsNullOrEmpty(faappAddlDataContract.FadlCcpgActive))
            {
                aidApplicationAdditionalInfoEntity.CreateCcpgRecord = faappAddlDataContract.FadlCcpgActive.ToUpper() == "Y";
            }
            aidApplicationAdditionalInfoEntity.User1 = CheckAndAssignValue(faappAddlDataContract.FadlUser1);
            aidApplicationAdditionalInfoEntity.User2 = CheckAndAssignValue(faappAddlDataContract.FadlUser2);
            aidApplicationAdditionalInfoEntity.User3 = CheckAndAssignValue(faappAddlDataContract.FadlUser3);
            aidApplicationAdditionalInfoEntity.User4 = CheckAndAssignValue(faappAddlDataContract.FadlUser4);
            aidApplicationAdditionalInfoEntity.User5 = CheckAndAssignValue(faappAddlDataContract.FadlUser5);
            aidApplicationAdditionalInfoEntity.User6 = CheckAndAssignValue(faappAddlDataContract.FadlUser6);
            aidApplicationAdditionalInfoEntity.User7 = CheckAndAssignValue(faappAddlDataContract.FadlUser7);
            aidApplicationAdditionalInfoEntity.User8 = CheckAndAssignValue(faappAddlDataContract.FadlUser8);
            aidApplicationAdditionalInfoEntity.User9 = CheckAndAssignValue(faappAddlDataContract.FadlUser9);
            aidApplicationAdditionalInfoEntity.User10 = CheckAndAssignValue(faappAddlDataContract.FadlUser10);
            aidApplicationAdditionalInfoEntity.User11 = CheckAndAssignValue(faappAddlDataContract.FadlUser11);
            aidApplicationAdditionalInfoEntity.User12 = CheckAndAssignValue(faappAddlDataContract.FadlUser12);
            aidApplicationAdditionalInfoEntity.User13 = CheckAndAssignValue(faappAddlDataContract.FadlUser13);
            aidApplicationAdditionalInfoEntity.User14 = CheckAndAssignValue(faappAddlDataContract.FadlUser14);
            aidApplicationAdditionalInfoEntity.User15 = faappAddlDataContract.FadlUser15;
            aidApplicationAdditionalInfoEntity.User16 = faappAddlDataContract.FadlUser16;
            aidApplicationAdditionalInfoEntity.User17 = faappAddlDataContract.FadlUser17;
            aidApplicationAdditionalInfoEntity.User18 = faappAddlDataContract.FadlUser18;
            aidApplicationAdditionalInfoEntity.User19 = faappAddlDataContract.FadlUser19;
            aidApplicationAdditionalInfoEntity.User21 = faappAddlDataContract.FadlUser21;

            return aidApplicationAdditionalInfoEntity;
        }

        }
}
