// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Attributes of any requirement or subrequirement.
    /// </summary>
    [Serializable]
    public abstract class RequirementBlock : BlockBase
    {
        // ACRB.COURSE.REUSE.FLAG
        // Default is false
        public bool AllowsCourseReuse { get; set; }

        // ACRB.MERGE.METHOD
        // Default TBD
        public bool WaitToMerge { get; set; }

        protected RequirementBlock(string id, string code)
            :base(id,code)
        {
        }
           
    }
}
