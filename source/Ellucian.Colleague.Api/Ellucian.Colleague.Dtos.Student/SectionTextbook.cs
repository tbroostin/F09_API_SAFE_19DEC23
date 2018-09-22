// Copyright 2017 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Textbook assignment for a course section
    /// </summary>
    public class SectionTextbook
    {
        /// <summary>
        /// The textbook being added to a section
        /// </summary>
        public Book Textbook { get; set; }

        /// <summary>
        /// The Id of the section that the book is being added to
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// The requirement status of the book being added to the section.
        /// </summary>
        public string RequirementStatusCode { get; set; }

        /// <summary>
        /// Action to be taken for a section book assignment
        /// </summary>
        public SectionBookAction Action { get; set; }
    }
}
