// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Different callers of the sections/{id}/grades PUT endpoint, which affects the behavior of the endpoint.
    /// </summary>
    [Serializable]
    public enum GradesPutCallerTypes
    {
        Standard, ILP
    }
}
