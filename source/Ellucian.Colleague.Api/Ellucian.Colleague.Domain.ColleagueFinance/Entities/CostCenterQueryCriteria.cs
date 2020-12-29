// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Cost Center query filter criteria.
    /// </summary>
    [Serializable]
    public class CostCenterQueryCriteria : BaseGlComponentQueryCriteria
    {
      
        /// <summary>
        /// Financial health threshold filter criteria.
        /// </summary>
        public List<FinancialThreshold> FinancialThresholds { get; set; }

        /// <summary>
        /// Constructor that initializes a cost center query criteria domain entity.
        /// </summary>
        /// <param name="componentCriteria">IEnumerable Cost center component query criteria.</param>
        public CostCenterQueryCriteria(IEnumerable<CostCenterComponentQueryCriteria> componentCriteria)
        :base(componentCriteria)
        {
            if (componentCriteria == null)
            {
                throw new ArgumentNullException("componentCriteria");
            }
            ComponentCriteria = new List<CostCenterComponentQueryCriteria>(componentCriteria);
            FinancialThresholds = new List<FinancialThreshold>();
        }
    }
}
