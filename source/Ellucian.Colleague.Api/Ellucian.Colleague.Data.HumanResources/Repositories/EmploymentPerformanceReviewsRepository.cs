//Copyright 2017-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EmploymentPerformanceReviewsRepository : BaseColleagueRepository, IEmploymentPerformanceReviewsRepository
    {
        const string AllEmploymentPerformanceReviewsRecordsCache = "AllEmploymentPerformanceReviewsRecordKeys";
        const int AllEmploymentPerformanceReviewsRecordsCacheTimeout = 20;
        private RepositoryException exception = new RepositoryException();

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public EmploymentPerformanceReviewsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region GET Method
        /// <summary>
        ///  Get a collection of EmploymentPerformanceReview domain entity objects
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>collection of EmploymentPerformanceReview domain entity objects</returns>
        public async Task<Tuple<IEnumerable<EmploymentPerformanceReview>, int>> GetEmploymentPerformanceReviewsAsync(int offset, int limit, bool bypassCache = false)
        {
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllEmploymentPerformanceReviewsRecordsCache);
            List<EmploymentPerformanceReview> employmentPerformanceReviewEntities = new List<EmploymentPerformanceReview>();

            int totalCount = 0;
            var selectionCriteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "",
                offset,
                limit,
                AllEmploymentPerformanceReviewsRecordsCacheTimeout,
                async () =>
                {
                    var criteria = "WITH PERPOS.EVAL.RATINGS.DATE NE '' BY.EXP PERPOS.EVAL.RATINGS.DATE";
                    var perposIds = await DataReader.SelectAsync("PERPOS", criteria.ToString());

                    var perposIds2 = new List<string>();
                    foreach (var perposId in perposIds)
                    {
                        var perposKey = perposId.Split(DmiString._VM)[0];
                        perposIds2.Add(perposKey);
                    }

                    var criteria2 = "";

                    criteria2 = string.Concat(criteria2, "WITH PERPOS.EVAL.RATINGS.DATE NE '' BY.EXP PERPOS.EVAL.RATINGS.DATE SAVING PERPOS.EVAL.RATINGS.DATE");

                    var perposDates = await DataReader.SelectAsync("PERPOS", criteria2.ToString());

                    var keys = new List<string>();
                    var idx = 0;
                    foreach (var perposId in perposIds2)
                    {
                        keys.Add(String.Concat(perposId, "|", perposDates.ElementAt(idx)));
                        idx++;
                    }
                    keys.Sort();
                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = keys,
                        criteria = ""
                    };
                    return requirements;
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<EmploymentPerformanceReview>, int>(new List<EmploymentPerformanceReview>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;
            var subList = keyCacheObject.Sublist.ToArray();
            if (subList == null || !subList.Any())
            {
                return new Tuple<IEnumerable<EmploymentPerformanceReview>, int>(new List<EmploymentPerformanceReview>(), 0);
            }

            var thisSubList = new List<string>();

            // build list of all perpos keys and bulk read them
            var allPerposKeys = new List<string>();
            foreach (var key in subList)
            {
                var perposKey = key.Split('|')[0];
                allPerposKeys.Add(perposKey);
            }
            var perposContracts = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<Perpos>("PERPOS", allPerposKeys.ToArray());
            if ((perposContracts.InvalidKeys != null && perposContracts.InvalidKeys.Any()) ||
               perposContracts.InvalidRecords != null && perposContracts.InvalidRecords.Any())
            {
                if (perposContracts.InvalidKeys.Any())
                {
                    exception.AddErrors(perposContracts.InvalidKeys
                        .Select(key => new RepositoryError("Bad.Data",
                        string.Format("Unable to locate the following PERPOS key '{0}'.", key.ToString()))));
                }
                if (perposContracts.InvalidRecords.Any())
                {
                    exception.AddErrors(perposContracts.InvalidRecords
                       .Select(r => new RepositoryError("Bad.Data",
                       string.Format("Error: '{0}' ", r.Value))
                       { SourceId = r.Key }));
                }
            }

            Dictionary<string, string> dict = null;
            try
            {
                dict = await GetGuidsCollectionAsync(subList);
            }
            catch (Exception ex)
            {
                // Suppress any possible exception with missing primary GUIDs.  We will report any missing GUIDs in a collection as
                // we process the list of employee performance reviews 
                logger.Error(ex, "Unable to get perpos contracts by guid.");
            }

            // loop through list of concatenated perpos key and PERPOS.EVAL.RATINGS.DATE
            foreach (var key in subList)
            {
                var perposKey = key.Split('|');
                var perpos = perposContracts.BulkRecordsRead.FirstOrDefault(x => x.Recordkey == perposKey[0]);

                if (perpos != null)
                {
                    foreach (var perposRecord in perpos.PerposEvalsEntityAssociation)
                    {
                        var effectiveDate = Convert.ToDateTime(perposRecord.PerposEvalRatingsDateAssocMember);
                        ////convert a datetime to a unidata internal value 
                        var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                        if (offsetDate.ToString().Equals(perposKey[1]))
                        {
                            string guid = string.Empty;
                            dict.TryGetValue(key, out guid);
                            if (string.IsNullOrEmpty(guid))
                            {
                                exception.AddError(new RepositoryError("Bad.Data", string.Format("Guid not found for perpos '{0}' for PERPOS.EVAL.RATINGS.DATE '{1}'",
                                    perpos.Recordkey, perposRecord.PerposEvalRatingsDateAssocMember))
                                {
                                    SourceId = perpos.Recordkey
                                });
                            }
                            else
                            {
                                try
                                {
                                    employmentPerformanceReviewEntities.Add(new EmploymentPerformanceReview(guid,
                                        perpos.PerposHrpId, perpos.Recordkey, perposRecord.PerposEvalRatingsDateAssocMember, perposRecord.PerposEvalRatingsCycleAssocMember, perposRecord.PerposEvalRatingsAssocMember)
                                    {
                                        ReviewedById = perposRecord.PerposEvalRatingsHrpidAssocMember,
                                        Comment = perposRecord.PerposEvalRatingsCommentAssocMember
                                    });
                                }
                                catch (Exception ex)
                                {
                                    exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                                    {
                                        Id = guid,
                                        SourceId = string.Concat(perpos.Recordkey + ", " + perposRecord.PerposEvalRatingsDateAssocMember.ToString())
                                    });
                                }
                            }
                        }
                    }
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            //}
            return new Tuple<IEnumerable<EmploymentPerformanceReview>, int>(employmentPerformanceReviewEntities, totalCount);

        }

        /// <summary>
        /// Using a collection of concatenated PERPOS and PERPOS.EVAL.RATINGS.DATE
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a perpos with guids from secondary key</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(key => new RecordKeyLookup("PERPOS", key.Split('|')[0],
                   "PERPOS.EVAL.RATINGS.DATE", key.Split('|')[1], false))
                   .ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(string.Concat(splitKeys[1], "|", splitKeys[2]), recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", "PERPOS"), ex);
            }

            return guidCollection;
        }

        /// <summary>
        /// Returns a review for a specified Employment Performance Reviews key.
        /// </summary>
        /// <param name="ids">Key to Employment Performance Reviews to be returned</param>
        /// <returns>EmploymentPerformanceReview Objects</returns>
        public async Task<EmploymentPerformanceReview> GetEmploymentPerformanceReviewByIdAsync(string id)
        {
            var guidInfo = await GetRecordInfoFromGuidAsync(id);

            if (guidInfo == null)
                throw new KeyNotFoundException();

            if (guidInfo.Entity != "PERPOS")
            {
                throw new RepositoryException("GUID " + id + " has different entity, " + guidInfo.Entity + ", than expected, PERPOS");
            }
            else
            {
                if (string.IsNullOrEmpty(guidInfo.SecondaryKey))
                {
                    throw new RepositoryException("GUID " + id + " for PERPOS has no secondary key for PERPOS.EVAL.RATINGS.DATE");
                }
            }

            var perpos = await DataReader.ReadRecordAsync<Perpos>("PERPOS", guidInfo.PrimaryKey);
            if (perpos == null && !string.IsNullOrEmpty(guidInfo.PrimaryKey))
            {
                throw new KeyNotFoundException("Invalid PERPOS ID: " + guidInfo.PrimaryKey);
            }
            foreach (var perposRecord in perpos.PerposEvalsEntityAssociation)
            {
                var effectiveDate = Convert.ToDateTime(perposRecord.PerposEvalRatingsDateAssocMember);
                ////convert a datetime to a unidata internal value 
                var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                if (offsetDate.ToString().Equals(guidInfo.SecondaryKey))
                {
                    //var employmentPerformanceReviewGuidInfo = await GetGuidFromRecordInfoAsync("PERPOS", perpos.Recordkey, "PERPOS.EVAL.RATINGS.DATE", employmentPerformanceReviewId.SecondaryKey);

                    return new EmploymentPerformanceReview(id, perpos.PerposHrpId, perpos.Recordkey, perposRecord.PerposEvalRatingsDateAssocMember, perposRecord.PerposEvalRatingsCycleAssocMember, perposRecord.PerposEvalRatingsAssocMember)
                    {
                        ReviewedById = perposRecord.PerposEvalRatingsHrpidAssocMember,
                        Comment = perposRecord.PerposEvalRatingsCommentAssocMember
                    };
                }
            }

            return null;
        }

        #endregion

        #region POST Method

        /// <summary>
        /// Create an EmploymentPerformanceReviews domain entity
        /// </summary>
        /// <param name="employmentPerformanceReviewsEntity"><see cref="EmploymentPerformanceReviews">The EmploymentPerformanceReviews domain entity to create</param>
        /// <returns><see cref="EmploymentPerformanceReviews">The created EmploymentPerformanceReviews domain entity</returns>
        public async Task<EmploymentPerformanceReview> CreateEmploymentPerformanceReviewsAsync(EmploymentPerformanceReview employmentPerformanceReviewsEntity)
        {
            if (employmentPerformanceReviewsEntity == null)
                throw new ArgumentNullException("employmentPerformanceReviewsEntity", "Must provide an employmentPerformanceReviewsEntity to create.");
            var recordGuid = string.Empty;
            try
            {
                recordGuid = await GetGuidFromRecordInfoAsync("PERPOS", employmentPerformanceReviewsEntity.PerposId, "PERPOS.EVAL.RATINGS.DATE", DmiString.DateTimeToPickDate((DateTime)employmentPerformanceReviewsEntity.CompletedDate).ToString());
            }
            catch (Exception ex)
            {
                // Guid does not already exist so we are okay.
                logger.Error(ex, "Unable to get perpos by guid.");
            }
            if (!string.IsNullOrEmpty(recordGuid))
            {
                var errorMessage = new RepositoryException("Repository error.");
                errorMessage.AddError(new RepositoryError("employmentPerformanceReview.completedOn", "The completedOn date already exists in colleague."));
                throw errorMessage;
            }
            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = this.BuildEmploymentPerformanceReviewsUpdateRequest(employmentPerformanceReviewsEntity);

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            //createRequest.InstJobId = null;
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<CreatePerposReviewRequest, CreatePerposReviewResponse>(createRequest);

            if (createResponse == null || string.IsNullOrEmpty(createResponse.PerfEvalGuid))
            {
                var errorMessage = string.Format("Error(s) occurred creating employmentPerformanceReviewsEntity '{0}':", employmentPerformanceReviewsEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("EmploymentPerformanceReview", errorMessage));
                logger.Error(errorMessage);
                throw exception;
            }

            if (createResponse.CreateReviewErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating employmentPerformanceReviewsEntity '{0}':", employmentPerformanceReviewsEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.CreateReviewErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created  from the database
            return await GetEmploymentPerformanceReviewByIdAsync(createResponse.PerfEvalGuid);
        }

        #endregion

        #region PUT Method
        /// <summary>
        /// Update an EmploymentPerformanceReviews domain entity
        /// </summary>
        /// <param name="employmentPerformanceReviewsEntity"><see cref="EmploymentPerformanceReview">The EmploymentPerformanceReviews domain entity to update</param>
        /// <returns><see cref="EmploymentPerformanceReview">The updated EmploymentPerformanceReview domain entity</returns>
        public async Task<EmploymentPerformanceReview> UpdateEmploymentPerformanceReviewsAsync(EmploymentPerformanceReview employmentPerformanceReviewsEntity)
        {

            if (employmentPerformanceReviewsEntity == null)
                throw new ArgumentNullException("employmentPerformanceReviewsEntity", "Must provide an employmentPerformanceReviewsEntity to update.");
            if (string.IsNullOrEmpty(employmentPerformanceReviewsEntity.Guid))
                throw new ArgumentNullException("employmentPerformanceReviewsEntity", "Must provide the guid of the employmentPerformanceReviewsEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var employmentPerformanceReviewsEntityId = await GetIdFromGuidAsync(employmentPerformanceReviewsEntity.Guid, "PERPOS");

            if (!string.IsNullOrEmpty(employmentPerformanceReviewsEntityId))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();

                var updateRequest = BuildEmploymentPerformanceReviewsUpdateRequest(employmentPerformanceReviewsEntity);

                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<CreatePerposReviewRequest, CreatePerposReviewResponse>(updateRequest);

                if (updateResponse.CreateReviewErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating employmentPerformanceReviewsEntity '{0}':", employmentPerformanceReviewsEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.CreateReviewErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated entity from the database
                return await GetEmploymentPerformanceReviewByIdAsync(employmentPerformanceReviewsEntity.Guid);
            }
            // perform a create instead
            return await CreateEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsEntity);
        }
        #endregion

        #region DELETE Method
        /// <summary>
        /// Delete a employment performance review based on review id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task DeleteEmploymentPerformanceReviewsAsync(string guid)
        {
            var request = new DeletePerposReviewRequest()
            {
                PerfEvalGuid = guid
            };

            //Delete
            var response = await transactionInvoker.ExecuteAsync<DeletePerposReviewRequest, DeletePerposReviewResponse>(request);

            //if there are any errors throw
            if (response.DeleteReviewErrors.Any())
            {
                var exception = new RepositoryException("Errors encountered while deleting employment performance review: " + guid);
                response.DeleteReviewErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                throw exception;
            }
        }

        #endregion

        private async Task<IEnumerable<EmploymentPerformanceReview>> BuildAllEmploymentPerformanceReviews()
        {
            var employmentPerformanceReviewEntities = new List<EmploymentPerformanceReview>();
            var employmentPerformanceReviewIds = await DataReader.SelectAsync("PERPOS", "");
            var employmentPerformanceReviewRecords = await DataReader.BulkReadRecordAsync<DataContracts.Perpos>(employmentPerformanceReviewIds);

            foreach (var employmentPerformanceReviewRecord in employmentPerformanceReviewRecords)
            {
                if (employmentPerformanceReviewRecord.PerposEvalsEntityAssociation != null && employmentPerformanceReviewRecord.PerposEvalsEntityAssociation.Any())
                {
                    var effectiveDate = DateTime.MinValue;
                    try
                    {
                        string employmentPerformanceReviewGuidInfo = string.Empty;

                        foreach (var employmentPerformanceReview in employmentPerformanceReviewRecord.PerposEvalsEntityAssociation)
                        {

                            effectiveDate = Convert.ToDateTime(employmentPerformanceReview.PerposEvalRatingsDateAssocMember);
                            //convert a datetime to a unidata internal value 
                            var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                            employmentPerformanceReviewGuidInfo = await GetGuidFromRecordInfoAsync("PERPOS", employmentPerformanceReviewRecord.Recordkey, "PERPOS.EVAL.RATINGS.DATE", offsetDate.ToString());

                            employmentPerformanceReviewEntities.Add(new EmploymentPerformanceReview(employmentPerformanceReviewGuidInfo, employmentPerformanceReviewRecord.PerposHrpId, employmentPerformanceReviewRecord.Recordkey, employmentPerformanceReview.PerposEvalRatingsDateAssocMember, employmentPerformanceReview.PerposEvalRatingsCycleAssocMember, employmentPerformanceReview.PerposEvalRatingsAssocMember)
                            {
                                ReviewedById = employmentPerformanceReview.PerposEvalRatingsHrpidAssocMember,
                                Comment = employmentPerformanceReview.PerposEvalRatingsCommentAssocMember
                            });
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        throw new ColleagueWebApiException(string.Concat(ex.Message, ", effectiveDate: ", effectiveDate != DateTime.MinValue ? effectiveDate.ToShortDateString() : ""),
                            ex.InnerException);
                    }
                }
            }

            return employmentPerformanceReviewEntities;
        }

        /// <summary>
        /// Create an UpdateVouchersIntegrationRequest from an EmploymentPerformanceReviews domain entity
        /// </summary>
        /// <param name="employmentPerformanceReviewsEntity">EmploymentPerformanceReviews domain entity</param>
        /// <returns>UpdateVouchersIntegrationRequest transaction object</returns>
        private CreatePerposReviewRequest BuildEmploymentPerformanceReviewsUpdateRequest(EmploymentPerformanceReview employmentPerformanceReviewsEntity)
        {

            var request = new CreatePerposReviewRequest
            {

                PerfEvalGuid = employmentPerformanceReviewsEntity.Guid,

                PerfEvalPersonId = employmentPerformanceReviewsEntity.PersonId,
                PerfEvalPerposId = employmentPerformanceReviewsEntity.PerposId,
                PerfEvalRatingsDate = employmentPerformanceReviewsEntity.CompletedDate,
                PerfEvalRatings = employmentPerformanceReviewsEntity.RatingCode,
                PerfEvalRatingsHrpid = employmentPerformanceReviewsEntity.ReviewedById,
                PerfEvalRatingsCycle = employmentPerformanceReviewsEntity.RatingCycleCode,
                PerfEvalRatingsComment = employmentPerformanceReviewsEntity.Comment,

            };

            return request;
        }

        /// <summary>
        /// Get the GUID for an Entity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> GetGuidFromID(string key, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, key);
            }
            catch (RepositoryException REX)
            {
                REX.AddError(new RepositoryError(entity + ".guid.NotFound", "GUID not found for " + entity + "id " + key));
                throw REX;
            }

        }

        /// <summary>
        /// Get the GUID for a entity using its ID
        /// </summary>
        /// <param name="id">entity ID</param>
        /// <param name="entity">entity</param>
        /// <returns>entity GUID</returns>
        public async Task<string> GetGuidFromIdAsync(string id, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("perpos.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }


        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetIdFromGuidAsync(string guid, string filename)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(filename + " GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(filename + " GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != filename)
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, " + filename);
            }

            return foundEntry.Value.PrimaryKey;

        }

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GuidLookupResult> GetInfoFromGuidAsync(string id)
        {
            try
            {
                return await GetRecordInfoFromGuidAsync(id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("review.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Using a collection of  ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup(filename, p, false)).ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }
    }
}
