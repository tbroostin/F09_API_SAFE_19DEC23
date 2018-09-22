// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentWaiverDtoToEntityAdapter : BaseAdapter<Dtos.Student.StudentWaiver, Domain.Student.Entities.StudentWaiver> {
        public StudentWaiverDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Domain.Student.Entities.StudentWaiver MapToType(Dtos.Student.StudentWaiver source)
        {
            var waiverEntity = new Domain.Student.Entities.StudentWaiver(source.Id, source.StudentId, null, source.SectionId, source.ReasonCode, source.Comment);
            waiverEntity.AuthorizedBy = source.AuthorizedBy;
            waiverEntity.ChangedBy = source.ChangedBy;
            waiverEntity.DateTimeChanged = source.DateTimeChanged;
            foreach (var item in source.RequisiteWaivers)
            {
                Domain.Student.Entities.WaiverStatus status = Domain.Student.Entities.WaiverStatus.NotSelected;
                switch (item.Status)
                {
                    case Dtos.Student.WaiverStatus.Denied:
                        status = Domain.Student.Entities.WaiverStatus.Denied;
                        break;
                    case Dtos.Student.WaiverStatus.Waived:
                        status = Domain.Student.Entities.WaiverStatus.Waived;
                        break;
                }
                waiverEntity.AddRequisiteWaiver(new Domain.Student.Entities.RequisiteWaiver(item.RequisiteId, status));
            }

            return waiverEntity;
        }
    }
}
