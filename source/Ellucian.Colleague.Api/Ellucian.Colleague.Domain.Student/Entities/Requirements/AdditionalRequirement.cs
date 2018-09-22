// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Additional Majors, Minors, CCDs, and Specializations.  Each has a code, a spelled-out name, type, and may (usually) a link to a requirement.
    /// </summary>
    [Serializable]
    public class AdditionalRequirement
    {
        /// <summary>
        /// Ex: MATH
        /// </summary>
        public string AwardCode { get; set; }  
        /// <summary>
        /// Ex: Mathematics
        /// </summary>
        public string AwardName { get; set; }
        /// <summary>
        /// Enumerated: Ccd, Major, Minor, Specialization
        /// </summary>
        public AwardType Type { get; set; }
        /// <summary>
        /// Code linking award to requirements
        /// </summary>
        public string RequirementCode { get; set; }
        
        public AdditionalRequirement(string awardcode, string reqcode, AwardType type, string awardname)
        {
            AwardCode = awardcode;
            Type = type;
            RequirementCode = reqcode;  // This *can* be null.  
            AwardName = awardname;
        }
    }
}