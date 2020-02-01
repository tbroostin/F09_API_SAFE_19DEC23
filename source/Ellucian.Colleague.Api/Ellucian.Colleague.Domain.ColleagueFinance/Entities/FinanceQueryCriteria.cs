// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Finance query filter criteria.
    /// </summary>
    [Serializable]
    public class FinanceQueryCriteria : BaseGlComponentQueryCriteria
    {
        /// <summary>
        /// Constructor that initializes a finance query criteria domain entity.
        /// </summary>
        /// <param name="componentCriteria">IEnumerable component query criteria.</param>
        public FinanceQueryCriteria(IEnumerable<CostCenterComponentQueryCriteria> componentCriteria, IEnumerable<FinanceQueryComponentSortCriteria> sortCriteria)
            :base(componentCriteria)
        {
            if (componentCriteria == null)
            {
                throw new ArgumentNullException("componentCriteria");
            }
            ComponentCriteria = new List<CostCenterComponentQueryCriteria>(componentCriteria);
            SortCriteria = new List<FinanceQueryComponentSortCriteria>(sortCriteria);
        }
        public List<FinanceQueryComponentSortCriteria> SortCriteria { get; set; }

        /// <summary>
        /// List of project reference no's to filter gl accounts.
        /// </summary>
        public List<string> ProjectReferenceNos { get; set; }       
    }
}

