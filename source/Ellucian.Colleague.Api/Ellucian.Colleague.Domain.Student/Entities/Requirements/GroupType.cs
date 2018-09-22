// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public enum GroupType
    {
        TakeAll,            // old EVAL type 30
        TakeSelected,       // old EVAL type 31
        TakeCredits,        // old EVAL type 32
        TakeCourses,        // old EVAL type 33
        CustomMatch         // old EVAL type 34
    }
}
