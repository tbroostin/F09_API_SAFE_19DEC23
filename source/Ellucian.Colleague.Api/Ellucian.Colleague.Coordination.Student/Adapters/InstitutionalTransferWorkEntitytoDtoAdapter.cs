/// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert InstitutionalTransferWork entity to InstitutionalTransferWork DTO
    /// </summary>
    public class InstitutionalTransferWorkEntitytoDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.TransferWork.TransferEquivalencies, Dtos.Student.TransferWork.TransferEquivalencies>
    {
        public InstitutionalTransferWorkEntitytoDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.TransferWork.Equivalency, Dtos.Student.TransferWork.Equivalency>();
            AddMappingDependency<Domain.Student.Entities.TransferWork.ExternalCourseWork, Dtos.Student.TransferWork.ExternalCourseWork>();
            AddMappingDependency<Domain.Student.Entities.TransferWork.ExternalNonCourseWork, Dtos.Student.TransferWork.ExternalNonCourseWork>();
            AddMappingDependency<Domain.Student.Entities.TransferWork.EquivalentCoursCredit, Dtos.Student.TransferWork.EquivalentCourseCredit>();
            AddMappingDependency<Domain.Student.Entities.TransferWork.EquivalentGeneralCredit, Dtos.Student.TransferWork.EquivalentGeneralCredit>();
        }
    }
}
