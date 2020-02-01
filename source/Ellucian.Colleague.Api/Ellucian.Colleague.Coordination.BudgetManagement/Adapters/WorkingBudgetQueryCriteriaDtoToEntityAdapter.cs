// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos.BudgetManagement;


namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{
    /// <summary>
    /// Custom adapter for the working budget component query filter.
    /// </summary>
    public class WorkingBudgetQueryCriteriaDtoToEntityAdapter :
        AutoMapperAdapter<Dtos.BudgetManagement.WorkingBudgetQueryCriteria, Domain.BudgetManagement.Entities.WorkingBudgetQueryCriteria>
    {
        public WorkingBudgetQueryCriteriaDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Domain.BudgetManagement.Entities.WorkingBudgetQueryCriteria MapToType(Dtos.BudgetManagement.WorkingBudgetQueryCriteria source)
        {
            List<Domain.BudgetManagement.Entities.ComponentQueryCriteria> componentCriteria = new List<Domain.BudgetManagement.Entities.ComponentQueryCriteria>();

            if (source.ComponentCriteria != null)
            {
                // Loop through the working budget component query criteria on the working budget query criteria DTO, 
                // and build a list of component query criteria domain entities.
                foreach (var criteriaDto in source.ComponentCriteria)
                {
                    Domain.BudgetManagement.Entities.ComponentQueryCriteria compCriteria =
                        new Domain.BudgetManagement.Entities.ComponentQueryCriteria(criteriaDto.ComponentName);
                    compCriteria.IndividualComponentValues = criteriaDto.IndividualComponentValues;

                    // for each component query criteria, if there is a list of component range query criteria, loop
                    // through the list of range criteria and build a list of component range query criteria domain entities.
                    // Then, add them to the component query criteria domain entity.
                    if (criteriaDto.RangeComponentValues != null)
                    {
                        List<Domain.BudgetManagement.Entities.ComponentRangeQueryCriteria> rangeCriteria =
                            new List<Domain.BudgetManagement.Entities.ComponentRangeQueryCriteria>();
                        foreach (var rangeDto in criteriaDto.RangeComponentValues)
                        {
                            Domain.BudgetManagement.Entities.ComponentRangeQueryCriteria componentRange =
                                new Domain.BudgetManagement.Entities.ComponentRangeQueryCriteria(rangeDto.StartValue, rangeDto.EndValue);
                            rangeCriteria.Add(componentRange);
                        }
                        compCriteria.RangeComponentValues = rangeCriteria;
                    }
                    componentCriteria.Add(compCriteria);
                }
            }

            var workingBudgetQueryCriteriaEntity = new Domain.BudgetManagement.Entities.WorkingBudgetQueryCriteria(componentCriteria);

            workingBudgetQueryCriteriaEntity.StartLineItem = source.StartLineItem;
            workingBudgetQueryCriteriaEntity.LineItemCount = source.LineItemCount;

            if ((source.BudgetOfficerIds != null) && (source.BudgetOfficerIds.Any()))
            {
                workingBudgetQueryCriteriaEntity.BudgetOfficerIds = new List<string>();
                foreach (var boId in source.BudgetOfficerIds)
                {
                    workingBudgetQueryCriteriaEntity.BudgetOfficerIds.Add(boId);
                }
            }

            if ((source.BudgetReportingUnitIds != null) && (source.BudgetReportingUnitIds.Any()))
            {
                workingBudgetQueryCriteriaEntity.BudgetReportingUnitIds = new List<string>();
                foreach (var unitId in source.BudgetReportingUnitIds)
                {
                    // The reporting unit IDs are always uppercased in Colleague.
                    workingBudgetQueryCriteriaEntity.BudgetReportingUnitIds.Add(unitId.ToUpperInvariant());
                }
            }

            workingBudgetQueryCriteriaEntity.IncludeBudgetReportingUnitsChildren = source.IncludeBudgetReportingUnitsChildren;

            List<Domain.BudgetManagement.Entities.SortSubtotalComponentQueryCriteria> sortSubtotalCriteria = new List<Domain.BudgetManagement.Entities.SortSubtotalComponentQueryCriteria>();

            if (source.SortSubtotalComponentQueryCriteria != null && source.SortSubtotalComponentQueryCriteria.Any())
            {
                // Loop through the working budget sort/subtotal query criteria on the working budget query criteria DTO, 
                // and build a list of sort/subtotal query criteria domain entities.
                foreach (var sortSubtotalDto in source.SortSubtotalComponentQueryCriteria)
                {
                    Domain.BudgetManagement.Entities.SortSubtotalComponentQueryCriteria sortSubtotalDomain = new Domain.BudgetManagement.Entities.SortSubtotalComponentQueryCriteria();

                    sortSubtotalDomain.SubtotalName = sortSubtotalDto.SubtotalName;
                    sortSubtotalDomain.SubtotalType = sortSubtotalDto.SubtotalType;
                    sortSubtotalDomain.Order = sortSubtotalDto.Order;
                    sortSubtotalDomain.IsDisplaySubTotal = sortSubtotalDto.IsDisplaySubTotal;

                    sortSubtotalCriteria.Add(sortSubtotalDomain);
                }
                workingBudgetQueryCriteriaEntity.SortSubtotalComponentQueryCriteria = sortSubtotalCriteria;
            }

            return workingBudgetQueryCriteriaEntity;
        }
    }
}