// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Preferred Sections Response
    /// </summary>
    public class PreferredSectionsResponse
    {
        /// <summary>
        /// List of student's preferred sections
        /// </summary>
        public List<PreferredSection> PreferredSections { get; set; }
        /// <summary>
        /// List of messages about preferred sections
        /// </summary>
        public List<PreferredSectionMessage> Messages { get; set; }
    }
}
