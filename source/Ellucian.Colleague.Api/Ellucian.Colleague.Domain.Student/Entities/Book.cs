// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Book
    {
        private readonly string _Id;
        private readonly string _Isbn;
        private readonly string _Title;
        private readonly string _Author;
        private readonly string _Publisher;
        private readonly string _Copyright;
        private decimal? _Price;
        private decimal? _PriceUsed;

        /// <summary>
        /// The unique book ID
        /// </summary>
        public string Id { get { return _Id; } }

        /// <summary>
        /// The new price of the book
        /// </summary>
        public decimal? Price
        {
            get { return _Price; }
            set
            {
                if (value < 0 || value >= 10000)
                {
                    throw new ArgumentOutOfRangeException("Price", "The new price must be between $0 and $9999");
                }
                _Price = value;
            }
        }

        /// <summary>
        /// The used price of the book
        /// </summary>
        public decimal? PriceUsed
        {
            get { return _PriceUsed; }
            set
            {
                if (value < 0 || value >= 10000)
                {
                    throw new ArgumentOutOfRangeException("PriceUsed", "The used price must be between $0 and $9999");
                }
                _PriceUsed = value;
            }
        }

        /// <summary>
        /// The book's ISBN
        /// </summary>
        public string Isbn { get { return _Isbn; } }


        /// <summary>
        /// The book's Title
        /// </summary>
        public string Title { get { return _Title; } }

        /// <summary>
        /// The book's Author
        /// </summary>
        public string Author { get { return _Author; } }

        /// <summary>
        /// The book's publisher
        /// </summary>
        public string Publisher { get { return _Publisher; } }

        /// <summary>
        /// The copyright year of the book
        /// </summary>
        public string Copyright { get { return _Copyright; } }
        
        /// <summary>
        /// The book's edition
        /// </summary>
        public string Edition { get; set; }

        /// <summary>
        /// Boolean for if the book is marked as active
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

        /// <summary>
        /// Constructor for Book
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <param name="isbn">Book's ISBN</param>
        /// <param name="title">Title of Book</param>
        /// <param name="author">Author of Book</param>
        /// <param name="publisher">Publisher of Book</param>
        /// <param name="copyright">Copyright Year</param>
        /// <param name="edition">Edition of Book</param>
        /// <param name="isActive">If the Book is Active</param>
        /// <param name="priceUsed">The used price of the Book</param>
        /// <param name="priceNew">The new price of the Book</param>
        /// <param name="comment">Comment on Book</param>
        /// <param name="externalComments">External Comments</param>
        /// <param name="alternateID1">Alternate ID 1</param>
        /// <param name="alternateID2">Alternate ID 2</param>
        /// <param name="alternateID3">Alternate ID 3</param>
        public Book(string id, string isbn, string title, string author, string publisher, string copyright, string edition,
            bool isActive, decimal? priceUsed, decimal? priceNew, string comment, 
            string externalComments, string alternateID1, string alternateID2, string alternateID3)
        {
            if (string.IsNullOrEmpty(isbn))
            {
                if(string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(publisher) || string.IsNullOrEmpty(copyright))
                {
                    throw new ArgumentNullException("isbn", "ISBN must be provided if book info not provided");
                }
            }

            _Id = id;
            Price = priceNew;
            PriceUsed = priceUsed;
            _Isbn = isbn;
            _Title = title;
            _Author = author;
            _Publisher = publisher;
            _Copyright = copyright;
            Edition = edition;
            IsActive = isActive;
            Comment = comment;
            ExternalComments = externalComments;
            AlternateID1 = alternateID1;
            AlternateID2 = alternateID2;
            AlternateID3 = alternateID3;

        }
    }
}
