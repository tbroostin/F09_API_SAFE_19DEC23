using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using System.Linq;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EmploymentPerformanceReviewsRepository : BaseColleagueRepository, IEmploymentPerformanceReviewsRepository
    {
        public static char _VM = Convert.ToChar(DynamicArray.VM);

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
            var totalCount = 0;
            var employmentPerformanceReviewEntities = new List<EmploymentPerformanceReview>();
            var criteria = "";

            criteria = string.Concat(criteria, "WITH PERPOS.EVAL.RATINGS.DATE NE '' BY.EXP PERPOS.EVAL.RATINGS.DATE");

            var perposIds = await DataReader.SelectAsync("PERPOS", criteria.ToString());

            var perposIds2 = new List<string>();
            foreach (var perposId in perposIds)
            {
                var perposKey = perposId.Split(_VM)[0];
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

            totalCount = keys.Count();
            keys.Sort();
            
            var keysSubList = keys.Skip(offset).Take(limit).ToArray();
            
            if (keysSubList.Any())
            {
                var subList = new List<string>();

                foreach (var key in keysSubList)
                {
                    var perposKey = key.Split('|')[0];
                    subList.Add(perposKey);
                }
                var perposCollection = await DataReader.BulkReadRecordAsync<Perpos>("PERPOS", subList.ToArray());

                foreach (var key in keysSubList)
                {

                    var perposKey = key.Split('|');
                    var perpos = perposCollection.FirstOrDefault(x => x.Recordkey == perposKey[0]);
                    //var perpos = perposCollection.FirstOrDefault(x => x.Recordkey == perposKey[0] && x.PerposEvalRatingsDate.Contains(perposKey[1]));
                    //employmentPerformanceReviewEntities.Add(await BuildEmploymentPerformanceReviewsAsync(perpos, perbens, perposKey[1]));
                    
                    try
                    {
                        //var perposRecords = perpos.PerposEvalsEntityAssociation.Where(i => i.PerposEvalRatingsDateAssocMember.Equals(perposKey[1]));
                        foreach (var perposRecord in perpos.PerposEvalsEntityAssociation)
                        {
                            var effectiveDate = Convert.ToDateTime(perposRecord.PerposEvalRatingsDateAssocMember);
                            ////convert a datetime to a unidata internal value 
                            var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                            if (offsetDate.ToString().Equals(perposKey[1]))
                            {
                                var employmentPerformanceReviewGuidInfo = await GetGuidFromRecordInfoAsync("PERPOS", perpos.Recordkey, "PERPOS.EVAL.RATINGS.DATE", perposKey[1]);

                                employmentPerformanceReviewEntities.Add(new EmploymentPerformanceReview(employmentPerformanceReviewGuidInfo, 
                                    perpos.PerposHrpId, perpos.Recordkey, perposRecord.PerposEvalRatingsDateAssocMember, perposRecord.PerposEvalRatingsCycleAssocMember, perposRecord.PerposEvalRatingsAssocMember)
                                {
                                    ReviewedById = perposRecord.PerposEvalRatingsHrpidAssocMember,
                                    Comment = perposRecord.PerposEvalRatingsCommentAssocMember
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
            return new Tuple<IEnumerable<EmploymentPerformanceReview>, int>(employmentPerformanceReviewEntities, totalCount);

        }

        /// <summary>
        /// Returns a review for a specified Employment Performance Reviews key.
        /// </summary>
        /// <param name="ids">Key to Employment Performance Reviews to be returned</param>
        /// <returns>EmploymentPerformanceReview Objects</returns>
        public async Task<EmploymentPerformanceReview> GetEmploymentPerformanceReviewByIdAsync(string id)
        {
            var employmentPerformanceReviewId = await GetRecordInfoFromGuidAsync(id);

            if (employmentPerformanceReviewId == null)
                throw new KeyNotFoundException();

            var perpos = await DataReader.ReadRecordAsync<Perpos>("PERPOS", employmentPerformanceReviewId.PrimaryKey);
            foreach (var perposRecord in perpos.PerposEvalsEntityAssociation)
            {
                var effectiveDate = Convert.ToDateTime(perposRecord.PerposEvalRatingsDateAssocMember);
                ////convert a datetime to a unidata internal value 
                var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                if (offsetDate.ToString().Equals(employmentPerformanceReviewId.SecondaryKey))
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
            catch
            {
                // Guid does not already exist so we are okay.
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
                createResponse.CreateReviewErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
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
            var employmentPerformanceReviewsEntityId = await GetIdFromGuidAsync(employmentPerformanceReviewsEntity.Guid);

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
                    updateResponse.CreateReviewErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
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
                response.DeleteReviewErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
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
                        throw new Exception(string.Concat(ex.Message, ", effectiveDate: ", effectiveDate != DateTime.MinValue ? effectiveDate.ToShortDateString() : ""),
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
        /// Get the GUID for a entity using its ID
        /// </summary>
        /// <param name="id">entity ID</param>
        /// <param name="entity">entity</param>
        /// <returns>entity GUID</returns>
        public async Task<string> GetJobGuidFromIdAsync(string id, string entity)
        {
            try
            {
                var guid = await GetGuidFromRecordInfoAsync(entity, id);
                var criteria = string.Format("WITH LDM.GUID.PRIMARY.KEY EQ '{0}' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.ENTITY EQ 'PERPOS' AND LDM.GUID.REPLACED.BY EQ ''", id);
                var guidRecords = await DataReader.SelectAsync("LDM.GUID", criteria);

                if (guid != guidRecords[0])
                    guid = guidRecords[0];
                return guid;
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
        public async Task<string> GetIdFromGuidAsync(string id)
        {
            try
            {
                return await GetRecordKeyFromGuidAsync(id);
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

    }

}
