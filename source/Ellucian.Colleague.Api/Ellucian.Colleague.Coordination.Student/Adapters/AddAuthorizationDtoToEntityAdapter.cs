// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class AddAuthorizationDtoToEntityAdapter : BaseAdapter<Dtos.Student.AddAuthorization, Domain.Student.Entities.AddAuthorization>
    {
        public AddAuthorizationDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Domain.Student.Entities.AddAuthorization MapToType(Dtos.Student.AddAuthorization source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The source add authorization can not be null.");
            }

            var authorizationEntity = new Domain.Student.Entities.AddAuthorization(source.Id, source.SectionId)
            {
                AddAuthorizationCode = source.AddAuthorizationCode,
                StudentId = source.StudentId,
                AssignedBy = source.AssignedBy,
                AssignedTime = source.AssignedTime,
                IsRevoked = source.IsRevoked,
                RevokedBy = source.RevokedBy,
                RevokedTime = source.RevokedTime
            };
            return authorizationEntity;
        }
    }
}
