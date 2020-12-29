// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using slf4net;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    /// <summary>
    /// Adapt a PotentialD7FinancialAidCriteria entity from a PotentialD7FinancialAidCriteria DTO
    /// </summary>
    public class PotentialD7FinancialAidCriteriaToEntityAdapter : BaseAdapter<Ellucian.Colleague.Dtos.Finance.PotentialD7FinancialAidCriteria, Ellucian.Colleague.Domain.Finance.Entities.PotentialD7FinancialAidCriteria>
    {
        /// <summary>
        /// Constructor for PotentialD7FinancialAidCriteriaToEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry">Base interface for adapter registries</param>
        /// <param name="logger">Interface for logging mechanisms</param>
        public PotentialD7FinancialAidCriteriaToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Map a PotentialD7FinancialAidCriteria DTO to its corresponding entity type
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public override Domain.Finance.Entities.PotentialD7FinancialAidCriteria MapToType(PotentialD7FinancialAidCriteria criteria)
        {
            var awards = criteria.AwardPeriodAwardsToEvaluate
                .Select(x => new Domain.Finance.Entities.AwardPeriodAwardTransmitExcessStatus(x.AwardPeriodAward, x.TransmitExcessIndicator))
                .ToList();
            return new Domain.Finance.Entities.PotentialD7FinancialAidCriteria(criteria.StudentId, criteria.TermId, awards.ToList());
        }
    }
}
