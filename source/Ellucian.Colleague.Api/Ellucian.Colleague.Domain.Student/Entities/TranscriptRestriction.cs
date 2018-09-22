// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A restriction that prevents the student from seeing or requesting their transcript.  Can be related
    /// to a StudentRestriction but is activated by a separate set of rules within Colleague.
    /// </summary>
    [Serializable]
    public class TranscriptRestriction
    {
        /// <summary>
        /// Restriction Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Restriction Description
        /// </summary>
        public string Description { get; set; }
    }
}