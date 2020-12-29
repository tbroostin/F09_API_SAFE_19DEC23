// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert RetentionAlertCaseCategoryOrgRole entity to RetentionAlertCaseCategoryOrgRole DTO
    /// </summary>
    public class RetentionAlertCaseCategoryOrgRolesEntityToDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Dtos.Student.RetentionAlertCaseCategoryOrgRole>
    {
        public RetentionAlertCaseCategoryOrgRolesEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.RetentionAlertCaseCategoryOrgRole, Dtos.Student.RetentionAlertCaseCategoryOrgRole>();
        }
    }
}
