// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class AddAuthorizationInputDtoToEntityAdapter : BaseAdapter<Dtos.Student.AddAuthorizationInput, Domain.Student.Entities.AddAuthorization>
    {
        public AddAuthorizationInputDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Domain.Student.Entities.AddAuthorization MapToType(Dtos.Student.AddAuthorizationInput source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The source add authorization can not be null.");
            }

            var authorizationEntity = new Domain.Student.Entities.AddAuthorization(null, source.SectionId)
            {
                StudentId = source.StudentId,
                AssignedBy = source.AssignedBy,
                AssignedTime = source.AssignedTime,
            };
            return authorizationEntity;
        }
    }
}
