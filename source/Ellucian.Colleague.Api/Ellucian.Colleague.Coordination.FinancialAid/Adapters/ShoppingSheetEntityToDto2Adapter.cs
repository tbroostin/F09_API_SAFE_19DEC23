//Copyright 2020 Ellucian Company L.P. and its affiliates.
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
    public class ShoppingSheetEntityToDto2Adapter : AutoMapperAdapter<Domain.FinancialAid.Entities.ShoppingSheet2, Dtos.FinancialAid.ShoppingSheet2>
    {

        public ShoppingSheetEntityToDto2Adapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.ShoppingSheetCostItem2, Dtos.FinancialAid.ShoppingSheetCostItem2>();
            AddMappingDependency<Domain.FinancialAid.Entities.ShoppingSheetAwardItem2, Dtos.FinancialAid.ShoppingSheetAwardItem2>();
        }
    }
}
