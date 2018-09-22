// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Custom adapter for cost center component query filter.
    /// </summary>
    public class CostCenterQueryCriteriaDtoToEntityAdapter : 
        AutoMapperAdapter<Dtos.ColleagueFinance.CostCenterQueryCriteria, Domain.ColleagueFinance.Entities.CostCenterQueryCriteria>
    {
        public CostCenterQueryCriteriaDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Domain.ColleagueFinance.Entities.CostCenterQueryCriteria MapToType(Dtos.ColleagueFinance.CostCenterQueryCriteria source)
        {
            List<Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria> criteria = 
                new List<Domain.ColleagueFinance.Entities.CostCenterComponentQueryCriteria>();
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

            var costCenterEntity = new Domain.ColleagueFinance.Entities.CostCenterQueryCriteria(criteria);
            costCenterEntity.FiscalYear = source.FiscalYear;
            costCenterEntity.IncludeActiveAccountsWithNoActivity = source.IncludeActiveAccountsWithNoActivity;

            if ((source.FinancialThresholds != null) && (source.FinancialThresholds.Any()))
            {
                foreach (var ftDto in source.FinancialThresholds)
                {
                    Domain.ColleagueFinance.Entities.FinancialThreshold financialHealthIndicator = Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinancialThreshold.NearThreshold;
                    switch (ftDto)
                    {
                        case FinancialThreshold.NearThreshold:
                            financialHealthIndicator = Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinancialThreshold.NearThreshold;
                            break;
                        case FinancialThreshold.OverThreshold:
                            financialHealthIndicator = Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinancialThreshold.OverThreshold;
                            break;
                        case FinancialThreshold.UnderThreshold:
                            financialHealthIndicator = Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinancialThreshold.UnderThreshold;
                            break;

                    }
                    costCenterEntity.FinancialThresholds.Add(financialHealthIndicator);
                }
            }

            return costCenterEntity;
        }
    }
}