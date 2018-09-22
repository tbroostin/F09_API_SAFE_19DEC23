//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom Adapter to transform ShoppingSheet Domain Entities to ShoppingSheet DTOs
    /// </summary>
    public class ShoppingSheetEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.ShoppingSheet, Dtos.FinancialAid.ShoppingSheet>
    {

        public ShoppingSheetEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.ShoppingSheetCostItem, Dtos.FinancialAid.ShoppingSheetCostItem>();
            AddMappingDependency<Domain.FinancialAid.Entities.ShoppingSheetAwardItem, Dtos.FinancialAid.ShoppingSheetAwardItem>();
        }
    }
}
