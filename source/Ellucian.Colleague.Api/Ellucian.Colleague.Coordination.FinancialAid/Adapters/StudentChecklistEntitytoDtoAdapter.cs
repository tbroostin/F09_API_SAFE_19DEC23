/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
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
    /// Adapter class to add mapping dependency from StudentChecklistItem entity to StudentChecklistItem DTO
    /// </summary>
    public class StudentChecklistEntitytoDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.StudentFinancialAidChecklist, Dtos.FinancialAid.StudentFinancialAidChecklist>
    {
        /// <summary>
        /// Constructor for the custom StudentChecklistEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public StudentChecklistEntitytoDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.StudentChecklistItem, Dtos.FinancialAid.StudentChecklistItem>();
        }
    }
}
