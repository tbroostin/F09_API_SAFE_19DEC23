// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Selection Criteria for querying Books
    /// </summary>
    public class BookQueryCriteria
    {
        /// <summary>
        /// List of Book Ids identifying books to retrieve
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// Query string to be used for searches against book titles, authors, and International Standard Book Numbers (ISBN)
        /// </summary>
        public string QueryString { get; set; }
    }
}
