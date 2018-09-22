/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// OutsideAwards Repository. Provides access to the stored outside awards as well as 
    /// ability to create new outside awards
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class OutsideAwardsRepository : BaseColleagueRepository, IOutsideAwardsRepository
    {
        /// <summary>
        /// Instntiate a new OutsideAwardRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public OutsideAwardsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Create outside award method
        /// </summary>
        /// <param name="outsideAward">input outside award entity</param>
        /// <returns>created outside award</returns>
        public async Task<OutsideAward> CreateOutsideAwardAsync(OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw new ArgumentNullException("outsideAward");
            }

            CreateOutsideAwardRequest request = new CreateOutsideAwardRequest()
            {
                StudentId = outsideAward.StudentId,
                OutsideAwardName = outsideAward.AwardName,
                OutsideAwardAmount = outsideAward.AwardAmount,
                OutsideAwardYear = outsideAward.AwardYearCode,
                OutsideAwardFundingSource = outsideAward.AwardFundingSource,
                OutsideAwardType = outsideAward.AwardType.ToString()
            };

            CreateOutsideAwardResponse response = await transactionInvoker.ExecuteAsync<CreateOutsideAwardRequest, CreateOutsideAwardResponse>(request);

            if (string.IsNullOrEmpty(response.OutOutsideAwardId))
            {
                throw new Exception("Unknown error occurred while trying to create a new outside award record");
            }

            return await GetOutsideAwardAsync(response.OutOutsideAwardId);
        }

        /// <summary>
        /// Gets outside award by id
        /// </summary>
        /// <param name="outsideAwardRecordId">outside award record id</param>
        /// <returns>outside award entity</returns>
        private async Task<OutsideAward> GetOutsideAwardAsync(string outsideAwardRecordId)
        {
            if (string.IsNullOrEmpty(outsideAwardRecordId))
            {
                throw new ArgumentNullException("outsideAwardRecord");
            }

            FaOutsideAwards outsideAwardRecord = await DataReader.ReadRecordAsync<FaOutsideAwards>(outsideAwardRecordId);

            if (outsideAwardRecord == null)
            {
                throw new KeyNotFoundException(string.Format("Outside award record {0} was not found", outsideAwardRecordId));
            }

            try
            {
                return new OutsideAward(outsideAwardRecord.Recordkey, outsideAwardRecord.FoaStudentId,
                    outsideAwardRecord.FoaYear, outsideAwardRecord.FoaAwardName, outsideAwardRecord.FoaAwardType, 
                    outsideAwardRecord.FoaAmount.Value, outsideAwardRecord.FoaFundingSource);
            }
            catch (Exception e)
            {
                string message = string.Format("Unable to retrieve outside award record for {0}", outsideAwardRecordId);
                logger.Error(e, message);
                throw new ApplicationException(message, e);
            }
        }

        /// <summary>
        /// Gets outside award entities for the specified student id and award year code
        /// </summary>
        /// <param name="studentId">student id to retrieve outside awards for</param>
        /// <param name="awardYearCode">award year code to retrieve outside awards for</param>
        /// <returns>List of OutsideAward entities</returns>
        public async Task<IEnumerable<OutsideAward>> GetOutsideAwardsAsync(string studentId, string awardYearCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }

            string criteria = "WITH FOA.STUDENT.ID EQ '" + studentId + "' WITH FOA.YEAR EQ '" + awardYearCode + "'"; ;
            Collection<FaOutsideAwards> outsideAwardsForYear = await DataReader.BulkReadRecordAsync<FaOutsideAwards>(criteria);

            List<OutsideAward> outsideAwardEntities = new List<OutsideAward>();
            if (outsideAwardsForYear != null)
            {
                foreach (var awardRecord in outsideAwardsForYear)
                {
                    outsideAwardEntities.Add(new OutsideAward(awardRecord.Recordkey, awardRecord.FoaStudentId, awardRecord.FoaYear,
                        awardRecord.FoaAwardName, awardRecord.FoaAwardType, awardRecord.FoaAmount.HasValue ? awardRecord.FoaAmount.Value : 0,
                        awardRecord.FoaFundingSource));
                }
            }
            return outsideAwardEntities;
        }

        /// <summary>
        /// Deletes the outside award record with the specified id
        /// </summary>
        /// <param name="outsideAwardId">outside award record id</param>
        /// <returns></returns>
        public async Task DeleteOutsideAwardAsync(string outsideAwardId)
        {
            if (string.IsNullOrEmpty(outsideAwardId))
            {
                throw new ArgumentNullException("outsideAwardId");
            }

            DeleteOutsideAwardRequest request = new DeleteOutsideAwardRequest() { OutsideAwardId = outsideAwardId };

            DeleteOutsideAwardResponse response = await transactionInvoker.ExecuteAsync<DeleteOutsideAwardRequest, DeleteOutsideAwardResponse>(request);

            if (response.ErrorCode == "OutsideAward.MissingRecord")
            {
                logger.Error(response.ErrorMessage);
                throw new KeyNotFoundException(string.Format("Unable to delete record: no record was found with the specified id: {0}", outsideAwardId));
            }
            if (!string.IsNullOrEmpty(response.ErrorCode))
            {
                logger.Error(response.ErrorMessage);
                throw new ApplicationException(string.Format("Unable to delete record with the specified id: {0}. See log for more details.", outsideAwardId));
            }
        }

        /// <summary>
        /// Updates an existing outside award record with the passed in information.
        /// </summary>
        /// <param name="outsideAward">outside award record</param>
        /// <returns></returns>
        public async Task<OutsideAward> UpdateOutsideAwardAsync(OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw new ArgumentNullException("outsideAward");
            }
            
            UpdateOutsideAwardRequest request = new UpdateOutsideAwardRequest()
            {
                StudentId = outsideAward.StudentId,
                OutsideAwardName = outsideAward.AwardName,
                OutsideAwardAmount = outsideAward.AwardAmount,
                OutsideAwardYear = outsideAward.AwardYearCode,
                OutsideAwardFundingSource = outsideAward.AwardFundingSource,
                OutsideAwardType = outsideAward.AwardType.ToString(),
                OutsideAwardId = outsideAward.Id
            };

            UpdateOutsideAwardResponse response = await transactionInvoker.ExecuteAsync<UpdateOutsideAwardRequest, UpdateOutsideAwardResponse>(request);

            if (response.ErrorCode == "OutsideAward.MissingRecord")
            {
                logger.Error(response.ErrorMessage);
                throw new KeyNotFoundException(string.Format("Unable to update record: no record was found with the specified id: {0}", outsideAward.StudentId));
            }
            if (!string.IsNullOrEmpty(response.ErrorCode))
            {
                logger.Error(response.ErrorMessage);
                throw new ApplicationException(string.Format("Unable to update record with the specified id: {0}. See log for more details.", outsideAward.StudentId));
            }
            return await GetOutsideAwardAsync(response.OutsideAwardId);

        }

    }
}
