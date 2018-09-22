// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A restriction or notification specific to a person
    /// </summary>
    public class PersonRestriction
    {
        /// <summary>
        /// Restriction Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Person or Student Id for the Restriction
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Restriction Code for translation
        /// </summary>
        public string RestrictionId { get; set; }
        /// <summary>
        /// Restriction Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Date Restriction became active
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Date Restriction will become or has become inactive
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Restriction Details
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// Hyperlink to website for resolution
        /// </summary>
        public string Hyperlink { get; set; }
        /// <summary>
        /// Hyperlink Text
        /// </summary>
        public string HyperlinkText { get; set; }
        /// <summary>
        /// Flag indicating whether or not restriction is visible to end users
        /// </summary>
        public bool OfficeUseOnly { get; set; }
        /// <summary>
        /// Nullable integer that indicates the severity of a Person Restriction.
        /// </summary>
        public int? Severity { get; set; }
    }
}
