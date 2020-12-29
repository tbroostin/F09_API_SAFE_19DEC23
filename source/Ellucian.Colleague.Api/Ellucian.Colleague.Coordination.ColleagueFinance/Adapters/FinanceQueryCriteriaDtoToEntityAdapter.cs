// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Custom adapter for finance query filter criteria.
    /// </summary>
    public class FinanceQueryCriteriaDtoToEntityAdapter :
        AutoMapperAdapter<Dtos.ColleagueFinance.FinanceQueryCriteria, Domain.ColleagueFinance.Entities.FinanceQueryCriteria>
    {
        public FinanceQueryCriteriaDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Domain.ColleagueFinance.Entities.FinanceQueryCriteria MapToType(Dtos.ColleagueFinance.FinanceQueryCriteria source)
        {
            List<Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria> criteria =
                new List<Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria>();
            List<Domain.ColleagueFinance.Entities.FinanceQueryComponentSortCriteria> sortCriteria =
              new List<Domain.ColleagueFinance.Entities.FinanceQueryComponentSortCriteria>();
            if (source.ComponentCriteria != null)
            {
                // loop through the cost center component query criteria on the cost center query criteria DTO, 
                // and build a list of cost center component query criteria domain entities.
                foreach (var criteriaDto in source.ComponentCriteria)
                {
                    Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria compCriteria =
                        new Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria(criteriaDto.ComponentName);
                    compCriteria.IndividualComponentValues = criteriaDto.IndividualComponentValues;

                    // for each cost center component query criteria, if there is a list of cost center component
                    // range query criteria, loop through the list of range criteria and build a list of cost center
                    // component query criteria domain entities. Then, add them to the cost center component query 
                    // criteria domain entity.
                    if (criteriaDto.RangeComponentValues != null)
                    {
                        List<Domain.ColleagueFinance.Entities.CostCenterComponentRangeQueryCriteria> rangeCriteria =
                            new List<Domain.ColleagueFinance.Entities.CostCenterComponentRangeQueryCriteria>();
                        foreach (var rangeDto in criteriaDto.RangeComponentValues)
                        {
                            Domain.ColleagueFinance.Entities.CostCenterComponentRangeQueryCriteria componentRange =
                                new Domain.ColleagueFinance.Entities.CostCenterComponentRangeQueryCriteria(rangeDto.StartValue, rangeDto.EndValue);
                            rangeCriteria.Add(componentRange);
                        }
                        compCriteria.RangeComponentValues = rangeCriteria;
                    }
                    criteria.Add(compCriteria);
                }
            }
            if(source.ComponentSortCriteria!=null && source.ComponentSortCriteria.Count > 0)
            {
                // loop through the cost center component query criteria on the cost center query criteria DTO, 
                // and build a list of cost center component query criteria domain entities.
                foreach (var sortCriteriaDto in source.ComponentSortCriteria)
                {
                    if (sortCriteriaDto != null)
                    {
                        sortCriteria.Add(new Domain.ColleagueFinance.Entities.FinanceQueryComponentSortCriteria(sortCriteriaDto.ComponentName, sortCriteriaDto.Order,sortCriteriaDto.IsDisplaySubTotal));
                    }
                }                   
            }

            var entity = new Domain.ColleagueFinance.Entities.FinanceQueryCriteria(criteria, sortCriteria);
            entity.FiscalYear = source.FiscalYear;
            entity.IncludeActiveAccountsWithNoActivity = source.IncludeActiveAccountsWithNoActivity;
            entity.ProjectReferenceNos = source.ProjectReferenceNos;
            return entity;
        }
    }
}
