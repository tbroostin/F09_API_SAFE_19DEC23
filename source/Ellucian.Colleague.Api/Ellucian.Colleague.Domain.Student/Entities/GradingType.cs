// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Denotes whether a class is being taken for a grade, pass/fail, or audit only.
    /// Used in both the academic credit and in the planned course.
    /// </summary>
    [Serializable]
    public enum GradingType
    {
        Graded,
        PassFail,
        Audit
    }

}
