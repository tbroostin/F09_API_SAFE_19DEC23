// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    [RegisterType]
    public class FinancialAidRepository : BaseColleagueRepository, IFinancialAidRepository
    {
        public FinancialAidRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Returns potential untransmitted D7 financial aid
        /// </summary>
        /// <param name="criteria">A <see cref="PotentialD7FinancialAidCriteria"/> object containing the query criteria</param>
        /// <returns>Enumeration of <see cref="Domain.Finance.Entities.AccountActivity.PotentialD7FinancialAid"/>potential financial aid</returns>
        public async Task<IEnumerable<Domain.Finance.Entities.AccountActivity.PotentialD7FinancialAid>> GetPotentialD7FinancialAidAsync(PotentialD7FinancialAidCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            var awards = criteria.AwardsToEvaluate.Select(x => x.AwardPeriodAward).ToList();
            var xmitInds = criteria.AwardsToEvaluate.Select(x => x.TransmitExcessIndicator).ToList();

            return await ExecuteGetPotentialD7FinancialAid(criteria.StudentId, criteria.TermId, awards, xmitInds);
        }

        private async Task<IEnumerable<Domain.Finance.Entities.AccountActivity.PotentialD7FinancialAid>> ExecuteGetPotentialD7FinancialAid(string studentId, string termId, IEnumerable<string> awardPeriodAwardsToEvaluate, IEnumerable<bool> awardPeriodAwardTransmitExcess)
        {
            var ctxRequest = new GetPotentialD7FinancialAidRequest
            {
                StudentId = studentId,
                TermId = termId,
                AwdPrdAwardsToEvaluate = awardPeriodAwardsToEvaluate.ToList(),
                AwdPrdAwardsXmitExcessInd = awardPeriodAwardTransmitExcess.ToList()
            };

            try
            {
                var ctxResponse = await transactionInvoker.ExecuteAsync<GetPotentialD7FinancialAidRequest, GetPotentialD7FinancialAidResponse>(ctxRequest);
                // response is not null
                if (ctxResponse == null)
                {
                    throw new ApplicationException("null response when invoking transaction GetPotentialD7FinancialAid");
                }

                if (string.IsNullOrEmpty(ctxResponse.AbortMessage))
                {
                    // list of potential aid is not null
                    if (ctxResponse.PotentialD7FinancialAid == null)
                    {
                        throw new ApplicationException("null list of potential aid returned by transaction GetPotentialD7FinancialAid");
                    }
                    // award id is not null
                    if (ctxResponse.PotentialD7FinancialAid.Any(x => string.IsNullOrEmpty(x.PotentialAwdPrdAwards)))
                    {
                        throw new ApplicationException("Empty award period*award was returned.");
                    }
                    // return list of potential aid entities
                    return ctxResponse.PotentialD7FinancialAid
                        .Where(x => x != null)
                        .Select(
                        x => new Domain.Finance.Entities.AccountActivity.PotentialD7FinancialAid
                            (x.PotentialAwdPrdAwards,
                             x.PotentialAwdPrdAwardDescriptions,
                             x.PotentialAwdPrdAwardAmounts.HasValue ? (decimal)x.PotentialAwdPrdAwardAmounts : 0)
                        );
                }
                else
                {
                    throw new ApplicationException(ctxResponse.AbortMessage);
                }
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae.Message);
                throw ae;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }
        }
    }
}
