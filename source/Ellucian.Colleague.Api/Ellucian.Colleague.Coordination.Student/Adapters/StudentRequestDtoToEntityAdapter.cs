// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentRequestDtoToEntityAdapter : BaseAdapter<Dtos.Student.StudentRequest, Domain.Student.Entities.StudentRequest> {
        public StudentRequestDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Domain.Student.Entities.StudentRequest MapToType(Dtos.Student.StudentRequest source)
        {
            if(source==null)
            {
                throw new ArgumentNullException("student request sent for conversion of dto to domain object is null");
            }
            Domain.Student.Entities.StudentRequest sr=null;
            if(source is Ellucian.Colleague.Dtos.Student.StudentTranscriptRequest)
            {
                var requestEntityAdapter = adapterRegistry.GetAdapter<Dtos.Student.StudentTranscriptRequest, Domain.Student.Entities.StudentTranscriptRequest>();
                sr= requestEntityAdapter.MapToType(source as Dtos.Student.StudentTranscriptRequest);

            }
            else if(source is Dtos.Student.StudentEnrollmentRequest)
            {
                var requestEntityAdapter = adapterRegistry.GetAdapter<Dtos.Student.StudentEnrollmentRequest, Domain.Student.Entities.StudentEnrollmentRequest>();
                sr= requestEntityAdapter.MapToType(source as Dtos.Student.StudentEnrollmentRequest);
            }
            return sr;
           
        }
    }
}
