// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Bitwise enumeration used when limiting file reads in the AcademicCreditRepository
    /// </summary>
    [Serializable]
    [Flags]
    public enum AcademicCreditDataSubset { None = 0, StudentCourseSec = 1, StudentEquivEvals = 2 };   
}
