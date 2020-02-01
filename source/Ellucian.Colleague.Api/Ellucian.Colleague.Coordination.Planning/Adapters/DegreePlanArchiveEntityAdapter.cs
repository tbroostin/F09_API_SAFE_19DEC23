// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class DegreePlanArchiveEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive, Ellucian.Colleague.Dtos.Planning.DegreePlanArchive>
    {
        public DegreePlanArchiveEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Dtos.Planning.DegreePlanArchive MapToType(Domain.Planning.Entities.DegreePlanArchive Source)
        {
            var dpArchiveDto = base.MapToType(Source);
            // for some unknown reason the DateTimeOffsetToDateTimeAdapter assigns a default DateTime
            // to the target object's CreatedDate and ReviewedDate properties. 
            // So we still need to manually handle these mappings.
            dpArchiveDto.CreatedDate = Source.CreatedDate.HasValue ? Source.CreatedDate.Value.DateTime : new DateTime();
            dpArchiveDto.ReviewedDate = Source.ReviewedDate.HasValue ? Source.ReviewedDate.Value.DateTime : (DateTime?)null;
            return dpArchiveDto;
        }
    }
}
