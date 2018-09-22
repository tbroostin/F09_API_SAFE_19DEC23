// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Indicates whether a test is of type Admissions, Placement, or Other
    /// </summary>
    [Serializable]
    public enum TestType
    {
        Admissions, Placement, Other
    }
}
