// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A book that is listed in the list of books for a section
    /// </summary>
    public class SectionBook
    {
        /// <summary>
        /// The unique Id of the book
        /// </summary>
        public string BookId { get; set; }
        /// <summary>
        /// Indicates whether this book is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// The book requirement status for the section
        /// </summary>
        public string RequirementStatusCode { get; set; }
    }
}
