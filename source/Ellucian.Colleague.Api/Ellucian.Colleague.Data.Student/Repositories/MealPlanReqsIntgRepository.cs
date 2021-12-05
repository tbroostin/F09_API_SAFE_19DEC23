// Copyright 2017-2021 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IMealPlanReqsIntgRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class MealPlanReqsIntgRepository : BaseColleagueRepository, IMealPlanReqsIntgRepository
    {
        readonly int readSize;
        const string AllMealPlanRequestsRecordsCache = "AllMealPlanRequestsRecordKeys";
        const int AllMealPlanRequestsRecordsCacheTimeout = 20;
        private RepositoryException exception = new RepositoryException();

        /// <summary>
        /// Constructor to instantiate a Meal Plan Request repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public MealPlanReqsIntgRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get the Meal Plan Request requested
        /// </summary>
        /// <param name="id">Meal Plan Request GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Domain.Student.Entities.MealPlanReqsIntg> GetByIdAsync(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid");
                }

                // Read the MEAL.PLAN.REQS.INTG record
                var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
                if (idDict == null || idDict.Count == 0)
                {
                    throw new KeyNotFoundException(string.Format("No meal-plan-requests was found for GUID '{0}'. ", guid));
                }

                var foundEntry = idDict.FirstOrDefault();
                if (foundEntry.Value == null)
                {
                    throw new KeyNotFoundException(string.Format("No meal-plan-requests was found for GUID '{0}'. ", guid));
                }

                if (foundEntry.Value.Entity != "MEAL.PLAN.REQS.INTG")
                {
                    exception.AddError(new RepositoryError("Bad.Data", "GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, MEAL.PLAN.REQS.INTG")
                    {
                        Id = guid
                    });
                    throw exception;
                }

                var MealPlanReqsIntgs = await DataReader.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(foundEntry.Value.PrimaryKey);
                {
                    if (MealPlanReqsIntgs == null)
                    {
                        throw new KeyNotFoundException(string.Format("No meal-plan-requests was found for GUID '{0}'. ", guid));
                    }
                }

                return BuildMealPlanReqsIntg(MealPlanReqsIntgs);

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get Meal Plan Request
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>

        /// <returns>A list of MealPlanReqsIntg domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.MealPlanReqsIntg>,int>> GetAsync(int offset, int limit, bool bypassCache)
        {
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllMealPlanRequestsRecordsCache);
            List<MealPlanReqsIntg> mealPlanRequests = new List<MealPlanReqsIntg>();

            if (limit == 0) limit = readSize;
            int totalCount = 0;
            var selectionCriteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "MEAL.PLAN.REQS.INTG",
                offset,
                limit,
                AllMealPlanRequestsRecordsCacheTimeout,
                async () =>
                {
                    return new CacheSupport.KeyCacheRequirements();
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<MealPlanReqsIntg>, int>(new List<MealPlanReqsIntg>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            var subList = keyCacheObject.Sublist.ToArray();

            if (subList == null || !subList.Any())
            {
                return new Tuple<IEnumerable<MealPlanReqsIntg>, int>(new List<MealPlanReqsIntg>(), 0);
            }

            var mealPlanReqsIntgs = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.MealPlanReqsIntg>("MEAL.PLAN.REQS.INTG", subList);
            if ((mealPlanReqsIntgs.InvalidKeys != null && mealPlanReqsIntgs.InvalidKeys.Any()) ||
    mealPlanReqsIntgs.InvalidRecords != null && mealPlanReqsIntgs.InvalidRecords.Any())
            {
                if (mealPlanReqsIntgs.InvalidKeys.Any())
                {
                    exception.AddErrors(mealPlanReqsIntgs.InvalidKeys
                        .Select(key => new RepositoryError("Bad.Data",
                        string.Format("Unable to locate the following MEAL.PLAN.REQS.INTG key '{0}'.", key.ToString()))));
                }
                if (mealPlanReqsIntgs.InvalidRecords.Any())
                {
                    exception.AddErrors(mealPlanReqsIntgs.InvalidRecords
                       .Select(r => new RepositoryError("Bad.Data",
                       string.Format("Error: '{0}' ", r.Value))
                       { SourceId = r.Key }));
                }
            }

            foreach (var mealPlanReqIntg in mealPlanReqsIntgs.BulkRecordsRead)
            {
                if (mealPlanReqIntg != null)
                {

                    try
                    {
                        mealPlanRequests.Add(BuildMealPlanReqsIntg(mealPlanReqIntg));
                    }
                    catch (Exception e)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", e.Message)
                        {
                            SourceId = mealPlanReqIntg.Recordkey,
                            Id = mealPlanReqIntg.RecordGuid
                        });
                    }
                }
            }
            
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return mealPlanRequests.Any() ?
               new Tuple<IEnumerable<Domain.Student.Entities.MealPlanReqsIntg>, int>(mealPlanRequests, totalCount) :
               new Tuple<IEnumerable<Domain.Student.Entities.MealPlanReqsIntg>, int>(new List<Domain.Student.Entities.MealPlanReqsIntg>(), 0);
        }

        /// <summary>
        /// Get Meal Plan Request data contract to entity
        /// </summary>
        /// <param name="source">Meal plan request data contract</param>
        /// <returns>MealPlanReqsIntg domain entitiy</returns>
        private Domain.Student.Entities.MealPlanReqsIntg BuildMealPlanReqsIntg(DataContracts.MealPlanReqsIntg source)
        {
            var statusAssociation = source.MpriStatusesEntityAssociation;
            string crntStatus = string.Empty;
            DateTime? crntStatusDate =  new DateTime();
            if ((statusAssociation != null) && (statusAssociation.Any()))
            {
                //get the most current status only
                var currentStatus = statusAssociation
                     .Where(i => i.MpriStatusDateAssocMember != null)
                        .OrderByDescending(dt => dt.MpriStatusDateAssocMember)
                        .FirstOrDefault();
                if (currentStatus != null)
                {
                   crntStatus = currentStatus.MpriStatusAssocMember;
                   crntStatusDate = currentStatus.MpriStatusDateAssocMember;
                }
            }

            var mealPlanReqsIntg = new Domain.Student.Entities.MealPlanReqsIntg(source.RecordGuid, source.Recordkey, source.MpriPerson, source.MpriMealPlan);
            mealPlanReqsIntg.EndDate = source.MpriEndDate;
            mealPlanReqsIntg.StartDate = source.MpriStartDate;
            mealPlanReqsIntg.SubmittedDate = source.MpriSubmittedDate;
            mealPlanReqsIntg.Term = source.MpriTerm;
            mealPlanReqsIntg.Status = crntStatus;
            mealPlanReqsIntg.StatusDate = crntStatusDate;
            
            return mealPlanReqsIntg;
        }

        /// <summary>
        /// Update an MealPlanReqsIntg domain entity
        /// </summary>
        /// <param name="MealPlanReqsIntgEntity">The MealPlanReqsIntg domain entity to update</param>
        /// <returns>The updated MealPlanReqsIntg domain entity</returns>
        public async Task<MealPlanReqsIntg> UpdateMealPlanReqsIntgAsync(MealPlanReqsIntg MealPlanReqsIntgEntity)
        {
            if (MealPlanReqsIntgEntity == null)
                throw new ArgumentNullException("MealPlanReqsIntgEntity", "Must provide a MealPlanReqsIntgEntity to update.");
            if (string.IsNullOrEmpty(MealPlanReqsIntgEntity.Guid))
                throw new ArgumentNullException("MealPlanReqsIntgEntity", "Must provide the guid of the MealPlanReqsIntgEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var MealPlanReqsIntgId = await GetRecordKeyFromGuidAsync(MealPlanReqsIntgEntity.Guid);

            if (!string.IsNullOrEmpty(MealPlanReqsIntgId))
            {

                var extendedDataTuple = GetEthosExtendedDataLists();
                var updateRequest = BuildMealPlanReqsIntgUpdateRequest(MealPlanReqsIntgEntity);
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)

                {

                    updateRequest.ExtendedNames = extendedDataTuple.Item1;

                    updateRequest.ExtendedValues = extendedDataTuple.Item2;

                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(updateRequest);

                if (updateResponse.CreateUpdateMealPlanReqRequestErrors.Any())
                {
                    var exception = new RepositoryException(string.Format("Error(s) occurred updating MealPlanReqsIntg '{0}':", MealPlanReqsIntgEntity.Guid));
                    foreach (var error in updateResponse.CreateUpdateMealPlanReqRequestErrors)
                    {
                        if (!string.IsNullOrEmpty(error.ErrorCodes))
                        {
                            exception.AddError(new RepositoryError(error.ErrorCodes, error.ErrorMessages));
                            logger.Error(error.ErrorMessages);
                        }
                        else
                        {
                            exception.AddError(new RepositoryError(error.ErrorMessages));
                            logger.Error(error.ErrorMessages);
                        }

                    }
                    throw exception;
                }

                // get the updated entity from the database
                return await GetByIdAsync(updateResponse.Guid);
            }

            // perform a create instead
            return await CreateMealPlanReqsIntgAsync(MealPlanReqsIntgEntity);
        }

        /// <summary>
        /// Create an MealPlanReqsIntg domain entity
        /// </summary>
        /// <param name="MealPlanReqsIntgEntity">The MealPlanReqsIntg domain entity to create</param>
        /// <returns>The created MealPlanReqsIntg domain entity</returns>       
        public async Task<MealPlanReqsIntg> CreateMealPlanReqsIntgAsync(MealPlanReqsIntg MealPlanReqsIntgEntity)
        {
            if (MealPlanReqsIntgEntity == null)
                throw new ArgumentNullException("MealPlanReqsIntgEntity", "Must provide a MealPlanReqsIntgEntity to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();
            var createRequest = BuildMealPlanReqsIntgUpdateRequest(MealPlanReqsIntgEntity);
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)

            {

                createRequest.ExtendedNames = extendedDataTuple.Item1;

                createRequest.ExtendedValues = extendedDataTuple.Item2;

            }
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(createRequest);

            if (createResponse.CreateUpdateMealPlanReqRequestErrors.Any())
            {
                var exception = new RepositoryException(string.Format("Error(s) occurred updating MealPlanReqsIntg '{0}':", MealPlanReqsIntgEntity.Guid));
                foreach (var error in createResponse.CreateUpdateMealPlanReqRequestErrors)
                {
                    if (!string.IsNullOrEmpty(error.ErrorCodes))
                    {
                        exception.AddError(new RepositoryError(error.ErrorCodes, error.ErrorMessages));
                        logger.Error(error.ErrorMessages);
                    }
                    else
                    {
                        exception.AddError(new RepositoryError(error.ErrorMessages));
                        logger.Error(error.ErrorMessages);
                    }

                }
                throw exception;
            }

            // get the newly created  from the database
            return await GetByIdAsync(createResponse.Guid);
        }

        /// <summary>
        /// Create an UpdateMealPlanAssignIntgRequest from an MealPlanReqsIntg domain entity
        /// </summary>
        /// <param name="MealPlanReqsIntgEntity">MealPlanReqsIntg domain entity</param>
        /// <returns>UpdateMealPlanAssignIntgRequest transaction object</returns>
        private CreateUpdateMealPlanReqRequest BuildMealPlanReqsIntgUpdateRequest(MealPlanReqsIntg MealPlanReqsIntg)
        {
            var request = new CreateUpdateMealPlanReqRequest();
            request.Guid = MealPlanReqsIntg.Guid;
            request.MealPlanId = MealPlanReqsIntg.MealPlan;
            request.PersonId= MealPlanReqsIntg.PersonId;
            request.AcademicPeriodId = MealPlanReqsIntg.Term;
            request.MealPlanReqId = MealPlanReqsIntg.Id;
            request.StartOn = MealPlanReqsIntg.StartDate;
            request.EndOn = MealPlanReqsIntg.EndDate;
            request.SubmittedOn = MealPlanReqsIntg.SubmittedDate;
            request.Status = MealPlanReqsIntg.Status;
            request.StatusDate = MealPlanReqsIntg.StatusDate;
            return request;
        }

    }
}
