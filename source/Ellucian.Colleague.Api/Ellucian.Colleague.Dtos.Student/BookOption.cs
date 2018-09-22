// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about the book options
    /// </summary>
    public class BookOption
    {
        /// <summary>
        /// Unique code for this book option
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this book option
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Bool for if the book is required for a section
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
