// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for CF Web configuration entity to Dto mapping.
    /// </summary>
    public class ColleagueFinanceWebConfigurationsEntityDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration, ColleagueFinanceWebConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CFWebDefaultsEntityDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public ColleagueFinanceWebConfigurationsEntityDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.PurchasingDefaults, PurchasingDefaults>();
            AddMappingDependency<Domain.ColleagueFinance.Entities.VoucherWebConfiguration, VoucherWebConfiguration>();
            AddMappingDependency<Domain.ColleagueFinance.Entities.ProcurementDocumentField, ProcurementDocumentField>();            
        }

        public ColleagueFinanceWebConfiguration MapToType(Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration source)
        {
            var colleagueFinanceWebConfigurationDto = new ColleagueFinanceWebConfiguration();
            // Initialize the adapter to convert the entity to DTO object.
            var adapter = adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration, Dtos.ColleagueFinance.ColleagueFinanceWebConfiguration>();
            colleagueFinanceWebConfigurationDto = adapter.MapToType(source);
            var documentFieldAdapter = adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.ProcurementDocumentField, ProcurementDocumentField>();

            var requisitionFieldRequirements = new List<ProcurementDocumentField>();
            if (source.RequisitionFieldRequirements != null)
            {
                foreach (var requisitionField in source.RequisitionFieldRequirements)
                {
                    if (requisitionField != null)
                    {
                        requisitionFieldRequirements.Add(documentFieldAdapter.MapToType(requisitionField));
                    }
                }
            }
            colleagueFinanceWebConfigurationDto.RequisitionFieldRequirements = requisitionFieldRequirements;

            var purchaseOrderFieldRequirements = new List<ProcurementDocumentField>();
            if (source.PurchaseOrderFieldRequirements != null)
            {
                foreach (var poField in source.PurchaseOrderFieldRequirements)
                {
                    if (poField != null)
                    {
                        purchaseOrderFieldRequirements.Add(documentFieldAdapter.MapToType(poField));
                    }
                }
            }
            colleagueFinanceWebConfigurationDto.PurchaseOrderFieldRequirements = purchaseOrderFieldRequirements;

            var voucherFieldRequirements = new List<ProcurementDocumentField>();
            if (source.VoucherFieldRequirements != null)
            {
                foreach (var voucherField in source.VoucherFieldRequirements)
                {
                    if (voucherField != null)
                    {
                        voucherFieldRequirements.Add(documentFieldAdapter.MapToType(voucherField));
                    }
                }
            }
            colleagueFinanceWebConfigurationDto.VoucherFieldRequirements = voucherFieldRequirements;


            return colleagueFinanceWebConfigurationDto;
        }
    }
}
