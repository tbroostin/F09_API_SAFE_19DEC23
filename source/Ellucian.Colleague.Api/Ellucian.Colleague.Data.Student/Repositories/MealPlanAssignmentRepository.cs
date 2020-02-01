// Copyright 2017-2019 Ellucian Company L.P. and its affiliates

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

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IMealPlanAssignmentRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class MealPlanAssignmentRepository : BaseColleagueRepository, IMealPlanAssignmentRepository
    {
        /// <summary>
        /// Constructor to instantiate a Meal Plan Assignment repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public MealPlanAssignmentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get the Meal Plan Assignment requested
        /// </summary>
        /// <param name="id">Meal Plan Assignment GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Domain.Student.Entities.MealPlanAssignment> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the INTG.GL.POSTINGS record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "MEAL.PLAN.ASSIGNMENT")
            {
                throw new KeyNotFoundException(string.Format("No meal plan assignment was found for guid '{0}'. ", id));
            }
            var mealPlanAssignments = await DataReader.ReadRecordAsync<DataContracts.MealPlanAssignment>(recordInfo.PrimaryKey);
            {
                if (mealPlanAssignments == null)
                {
                    throw new KeyNotFoundException(string.Format("No meal plan assignment was found for guid '{0}'. ", id));
                }
            }

            return BuildMealPlanAssignment(mealPlanAssignments);
        }

        /// <summary>
        /// Get Meal Plan Assignment
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>A list of MealPlanAssignment domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.MealPlanAssignment>,int>> GetAsync(int offset, int limit, string person = "", string term = "", string mealplan="", string status="", string startDate="", string endDate="")
        {
            var mealPlanAssignmentEntities = new List<Domain.Student.Entities.MealPlanAssignment>();
            var criteria = new StringBuilder();
            String select = string.Empty;
            if (!string.IsNullOrEmpty(person))
                criteria.AppendFormat("WITH MPAS.PERSON.ID EQ '{0}'", person);
            if (!string.IsNullOrEmpty(term))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH MPAS.TERM EQ '{0}'", term);
            }
            if (!string.IsNullOrEmpty(mealplan))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH MPAS.MEAL.PLAN EQ '{0}'", mealplan);
            }
            if (!string.IsNullOrEmpty(status))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH MPAS.CURRENT.STATUS EQ '{0}'", status);
            }
            if (!string.IsNullOrEmpty(startDate))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH MPAS.START.DATE GE '{0}'", startDate);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH MPAS.END.DATE NE '' AND MPAS.END.DATE LE '{0}'", endDate);
            }

            if (criteria.Length > 0)
                select = criteria.ToString();
            string[] mealPlanAssignmentIds = await DataReader.SelectAsync("MEAL.PLAN.ASSIGNMENT", select);
            var totalCount = mealPlanAssignmentIds.Count();

            Array.Sort(mealPlanAssignmentIds);

            var subList = mealPlanAssignmentIds.Skip(offset).Take(limit).ToArray();
            var mealPlanAssignments = await DataReader.BulkReadRecordAsync<DataContracts.MealPlanAssignment>("MEAL.PLAN.ASSIGNMENT", subList);
            {
                if (mealPlanAssignments == null)
                {
                    throw new KeyNotFoundException("No records selected from MEAL.PLAN.ASSIGNMENT in Colleague.");
                }
            }

            foreach (var intgStudentMealPlansEntity in mealPlanAssignments)
            {
                mealPlanAssignmentEntities.Add(BuildMealPlanAssignment(intgStudentMealPlansEntity));
            }
            return new Tuple<IEnumerable<Domain.Student.Entities.MealPlanAssignment>, int>(mealPlanAssignmentEntities, totalCount);
        }

      
        private Domain.Student.Entities.MealPlanAssignment BuildMealPlanAssignment(DataContracts.MealPlanAssignment source)
        {
            var statusAssociation = source.MpasStatusesEntityAssociation;
            string crntStatus = string.Empty;
            DateTime? crntStatusDate =  new DateTime();           
            if ((statusAssociation != null) && (statusAssociation.Any()))
            {
                //get the first row for status
                var currentStatus = statusAssociation.FirstOrDefault();
                if (currentStatus != null)
                {
                    crntStatus = currentStatus.MpasStatusAssocMember;
                    crntStatusDate = currentStatus.MpasStatusDateAssocMember;
                }
            }

            var mealPlanAssignment = new Domain.Student.Entities.MealPlanAssignment(source.RecordGuid, source.Recordkey, source.MpasPersonId, source.MpasMealPlan, source.MpasStartDate, source.MpasNoRatePeriods, crntStatus, crntStatusDate);
            mealPlanAssignment.EndDate = source.MpasEndDate;
            mealPlanAssignment.OverrideArCode = source.MpasOverrideArCode;
            mealPlanAssignment.OverrideRate = source.MpasOverrideRate;
            mealPlanAssignment.RateOverrideReason = source.MpasRateOverrideReason;
            mealPlanAssignment.Term = source.MpasTerm;
            mealPlanAssignment.MealCard = source.MpasMealCard;
            mealPlanAssignment.UsedRatePeriods = source.MpasUsedRatePeriods;
            mealPlanAssignment.PercentUsed = source.MpasUsedPct;
            mealPlanAssignment.MealComments = source.MpasComments;

            return mealPlanAssignment;
        }

        /// <summary>
        /// Update an MealPlanAssignment domain entity
        /// </summary>
        /// <param name="MealPlanAssignmentEntity">The MealPlanAssignment domain entity to update</param>
        /// <returns>The updated MealPlanAssignment domain entity</returns>
        public async Task<MealPlanAssignment> UpdateMealPlanAssignmentAsync(MealPlanAssignment MealPlanAssignmentEntity)
        {
            if (MealPlanAssignmentEntity == null)
                throw new ArgumentNullException("MealPlanAssignmentEntity", "Must provide a MealPlanAssignmentEntity to update.");
            if (string.IsNullOrEmpty(MealPlanAssignmentEntity.Guid))
                throw new ArgumentNullException("MealPlanAssignmentEntity", "Must provide the guid of the MealPlanAssignmentEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var MealPlanAssignmentId = await GetRecordKeyFromGuidAsync(MealPlanAssignmentEntity.Guid);

            if (!string.IsNullOrEmpty(MealPlanAssignmentId))
            {

                var updateRequest = BuildMealPlanAssignmentUpdateRequest(MealPlanAssignmentEntity);

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateMealPlanAssignIntgRequest, UpdateMealPlanAssignIntgResponse>(updateRequest);

                if (updateResponse.AError)
                {
                    var exception = new RepositoryException(string.Format("Error(s) occurred updating MealPlanAssignment '{0}':", MealPlanAssignmentEntity.Guid));
                    foreach (var error in updateResponse.UpdateMealPlanAssignErrors)
                    {
                        if (!string.IsNullOrEmpty(error.AlErrorCode))
                        {
                            exception.AddError(new RepositoryError(error.AlErrorCode, error.AlErrorMsg));
                            logger.Error(error.AlErrorMsg);
                        }
                        else
                        {
                            exception.AddError(new RepositoryError(error.AlErrorMsg));
                            logger.Error(error.AlErrorMsg);
                        }

                    }
                    throw exception;
                }

                // get the updated entity from the database
                return await GetByIdAsync(MealPlanAssignmentEntity.Guid);
            }

            // perform a create instead
            return await CreateMealPlanAssignmentAsync(MealPlanAssignmentEntity);
        }

        /// <summary>
        /// Create an MealPlanAssignment domain entity
        /// </summary>
        /// <param name="MealPlanAssignmentEntity">The MealPlanAssignment domain entity to create</param>
        /// <returns>The created MealPlanAssignment domain entity</returns>       
        public async Task<MealPlanAssignment> CreateMealPlanAssignmentAsync(MealPlanAssignment MealPlanAssignmentEntity)
        {
            if (MealPlanAssignmentEntity == null)
                throw new ArgumentNullException("MealPlanAssignmentEntity", "Must provide a MealPlanAssignmentEntity to create.");

            var createRequest = BuildMealPlanAssignmentUpdateRequest(MealPlanAssignmentEntity);
            createRequest.AId = null;

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateMealPlanAssignIntgRequest, UpdateMealPlanAssignIntgResponse>(createRequest);

            if (createResponse.AError)
            {
                var exception = new RepositoryException(string.Format("Error(s) occurred updating MealPlanAssignment '{0}':", MealPlanAssignmentEntity.Guid));
                foreach (var error in createResponse.UpdateMealPlanAssignErrors)
                {
                    if (!string.IsNullOrEmpty(error.AlErrorCode))
                    {
                        exception.AddError(new RepositoryError(error.AlErrorCode, error.AlErrorMsg));
                        logger.Error(error.AlErrorMsg);
                    }
                    else
                    {
                        exception.AddError(new RepositoryError(error.AlErrorMsg));
                        logger.Error(error.AlErrorMsg);
                    }

                }
                throw exception;
            }

            // get the newly created key from the database
            return await GetByIdAsync(createResponse.AGuid);
        }

        /// <summary>
        /// Create an UpdateMealPlanAssignIntgRequest from an MealPlanAssignment domain entity
        /// </summary>
        /// <param name="MealPlanAssignmentEntity">MealPlanAssignment domain entity</param>
        /// <returns>UpdateMealPlanAssignIntgRequest transaction object</returns>
        private UpdateMealPlanAssignIntgRequest BuildMealPlanAssignmentUpdateRequest(MealPlanAssignment mealPlanAssignment)
        {
            var request = new UpdateMealPlanAssignIntgRequest();
            request.AGuid = mealPlanAssignment.Guid;
            request.ACurrentStatus = mealPlanAssignment.Status;
            request.ACurrentStatusDate = mealPlanAssignment.StatusDate;
            request.AEndDate = mealPlanAssignment.EndDate;
            request.AId = mealPlanAssignment.Id;
            request.AMealPlanId = mealPlanAssignment.MealPlan;
            request.ANoRatePeriods = mealPlanAssignment.NoRatePeriods;
            request.AOverrideArCode = mealPlanAssignment.OverrideArCode;
            //request.AOverrideArType = mealPlanAssignment.OverrideReceivableType;
            request.AOverrideRate = mealPlanAssignment.OverrideRate;
            request.AOverrideRateReason = mealPlanAssignment.RateOverrideReason;
            request.APersonId = mealPlanAssignment.PersonId;
            request.AStartDate = mealPlanAssignment.StartDate;
            request.ATerm = mealPlanAssignment.Term;
            request.AUsedPct = mealPlanAssignment.PercentUsed;
            request.AUsedRatePeriods = mealPlanAssignment.UsedRatePeriods;
            request.AComments = mealPlanAssignment.MealComments;
            request.AMealCard = mealPlanAssignment.MealCard;
            
            return request;
        }
   }
}
