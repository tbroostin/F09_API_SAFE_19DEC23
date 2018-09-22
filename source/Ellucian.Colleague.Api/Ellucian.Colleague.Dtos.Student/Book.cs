// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The Id and other information for a book
    /// </summary>
    public class Book
    {
        /// <summary>
        /// The unique book ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The new price of the book
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// The used price of the book
        /// </summary>
        public decimal? PriceUsed { get; set; }

        /// <summary>
        /// The book's ISBN
        /// </summary>
        public string Isbn { get; set; }


        /// <summary>
        /// The book's Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The book's Author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The book's publisher
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// The copyright year of the book
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// The book's edition
        /// </summary>
        public string Edition { get; set; }

        /// <summary>
        /// Boolean for if the book is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// A comment on the book
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// External Comments with the entry
        /// </summary>
        public string ExternalComments { get; set; }

        /// <summary>
        /// Alternate ID 1
        /// </summary>
        public string AlternateID1 { get; set; }

        /// <summary>
        /// Alternate ID 2
        /// </summary>
        public string AlternateID2 { get; set; }

        /// <summary>
        /// Alternate ID 3
        /// </summary>
        public string AlternateID3 { get; set; }
    }
}
