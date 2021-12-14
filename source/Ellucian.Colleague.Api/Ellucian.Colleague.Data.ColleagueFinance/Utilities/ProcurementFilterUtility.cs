// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Utilities
{
    /// <summary>
    /// Provides various methods to build criteria for procurement filter criteria.
    /// </summary>
    public class ProcurementFilterUtility
    {
        /// <summary>
        /// returns unibasic query string to perform range criteria
        /// </summary>
        /// <param name="criteria">procurement filter criteria</param>
        /// <param name="entityAttribute">attribute of entity ex."VOU.TOTAL.AMT" of VOUCHERS entity is attribute to query against amount range criteria.</param>
        /// <returns>query criteria string</returns>
        public static string BuildAmountRangeQuery(ProcurementDocumentFilterCriteria criteria, string entityAttribute)
        {
            string minMaxAmountQuery = string.Empty;
            if (criteria.MinAmount != null || criteria.MaxAmount != null)
            {
                var minAmount = criteria.MinAmount;
                var maxAmount = criteria.MaxAmount;
                
                //when MaxAmount & MinAmount has a value
                if (minAmount.HasValue && maxAmount.HasValue)
                {
                    minMaxAmountQuery = string.Format("AND WITH ({0} GE '{1}' AND {0} LE '{2}') ", entityAttribute, minAmount.Value, maxAmount.Value);                    
                }
                //when MinAmount has value but MaxAmount is null
                else if (minAmount.HasValue && !maxAmount.HasValue)
                {
                    minMaxAmountQuery = string.Format("AND WITH {0} GE '{1}' ", entityAttribute, minAmount.Value);                    
                }
                //when MinAmount is null but MaxAmount has value
                else if (!minAmount.HasValue && maxAmount.HasValue)
                {
                    minMaxAmountQuery = string.Format("AND WITH {0} LE '{1}' ", entityAttribute, maxAmount.Value);                    
                }
            }
            return minMaxAmountQuery;
        }

        /// <summary>
        /// returns unibasic query string to perform selection on list of values / id's
        /// </summary>
        /// <param name="listCriteria">list of values / ids</param>
        /// <param name="entityAttribute">attribute of entity ex."VOU.CURRENT.STATUS of VOUCHERS entity" is attribute to query against list of statuses passed.</param>
        /// <returns></returns>
        public static string BuildListQuery(List<string> listCriteria, string entityAttribute)
        {
            string listQuery = null;            
            if (listCriteria != null && listCriteria.Any())
            {
                var vendorIdsVal = string.Join(" ", listCriteria.Select(x => string.Format("'{0}'", x)));
                listQuery = string.Format("AND WITH {0} EQ " + vendorIdsVal + " ", entityAttribute);                
            }
            return listQuery;
        }
    }
}
