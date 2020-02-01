using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Planning
{
    /// <summary>
    /// A note added to a student's degree plan
    /// This archived version has the name and formatted date for use in reporting
    /// </summary>
    public class ArchivedDegreePlanNote
    {
        /// <summary>
        /// Unique system id of this note (zero if new)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Date and time this note was added
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Formatted name of the person who added this note
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Note text, free-form
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Indicates the <see cref="PersonType">type of person</see> who authored the note so that correct endpoint may be used to get person's name
        /// May be Student or Advisor 
        /// </summary>
        public string PersonType { get; set; }
    }
}
