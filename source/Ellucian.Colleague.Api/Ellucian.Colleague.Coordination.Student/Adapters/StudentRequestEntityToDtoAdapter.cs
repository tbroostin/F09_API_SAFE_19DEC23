// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentRequestEntityToDtoAdapter : BaseAdapter<Domain.Student.Entities.StudentRequest, Dtos.Student.StudentRequest> {
        public StudentRequestEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Dtos.Student.StudentRequest MapToType(Domain.Student.Entities.StudentRequest source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("student request sent for conversion of domain to dto object is null");
            }
            Dtos.Student.StudentRequest sr = null;
            if(source is Domain.Student.Entities.StudentTranscriptRequest)
            {
                var requestDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.StudentTranscriptRequest, Dtos.Student.StudentTranscriptRequest>();
                sr = requestDtoAdapter.MapToType(source as Domain.Student.Entities.StudentTranscriptRequest);

            }
            else if (source is Domain.Student.Entities.StudentEnrollmentRequest)
            {
                var requestDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.StudentEnrollmentRequest, Dtos.Student.StudentEnrollmentRequest>();
                sr = requestDtoAdapter.MapToType(source as Domain.Student.Entities.StudentEnrollmentRequest);
            }
            return sr;
           
        }
    }
}
