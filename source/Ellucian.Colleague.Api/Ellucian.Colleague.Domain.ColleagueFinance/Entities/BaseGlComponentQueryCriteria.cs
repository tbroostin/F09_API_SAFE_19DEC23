// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Base GL component query filter criteria.
    /// </summary>
    [Serializable]
    public class BaseGlComponentQueryCriteria
    {
        /// <summary>
        /// Fiscal year.
        /// </summary>
        public string FiscalYear { get; set; }

        /// <summary>
        /// A list of cost center component filter criteria.
        /// </summary>
        public IEnumerable<CostCenterComponentQueryCriteria> ComponentCriteria { get; set; }

        /// <summary>
        /// Boolean flag to control what type of accounts are returned.
        /// </summary>
        public bool IncludeActiveAccountsWithNoActivity { get; set; }
        
        /// <summary>
        /// Constructor that initializes a cost center query criteria domain entity.
        /// </summary>
        /// <param name="componentCriteria">IEnumerable Cost center component query criteria.</param>
        public BaseGlComponentQueryCriteria(IEnumerable<CostCenterComponentQueryCriteria> componentCriteria)
        {
            if (componentCriteria == null)
            {
                throw new ArgumentNullException("componentCriteria");
            }
            ComponentCriteria = new List<CostCenterComponentQueryCriteria>(componentCriteria);            
        }
    }
}
