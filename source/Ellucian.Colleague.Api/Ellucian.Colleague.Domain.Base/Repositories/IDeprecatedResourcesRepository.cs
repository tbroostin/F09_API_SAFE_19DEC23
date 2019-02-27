// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IDeprecatedResourcesRepository
    {
        List<DeprecatedResources> Get();
    }
}