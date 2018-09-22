/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom FinancialAidOfficeEntityToDtoAdapter extends AutoMapperAdapter and adds a mapping dependency on:
    /// FinancialAidConfiguration
    /// ShoppingSheetConfiguration
    /// </summary>
    public class FinancialAidOffice2EntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice2>
    {
        /// <summary>
        /// Constructor for FinancialAidOfficeEntityToDtoAdapter adds mapping dependencies to FinancialAidConfiguration and ShoppingSheetConfiguration
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public FinancialAidOffice2EntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.FinancialAidConfiguration, Dtos.FinancialAid.FinancialAidConfiguration2>();
            AddMappingDependency<Domain.FinancialAid.Entities.ShoppingSheetConfiguration, Dtos.FinancialAid.ShoppingSheetConfiguration>();
            AddMappingDependency<Domain.FinancialAid.Entities.AcademicProgressConfiguration, Dtos.FinancialAid.AcademicProgressConfiguration>();
            AddMappingDependency<Domain.FinancialAid.Entities.AcademicProgressPropertyConfiguration, Dtos.FinancialAid.AcademicProgressPropertyConfiguration>();
        }
    }
}

