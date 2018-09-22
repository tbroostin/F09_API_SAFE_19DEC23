// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SectionBook
    {
        private readonly string _BookId;
        /// <summary>
        /// Id of the book
        /// </summary>
        public string BookId { get { return _BookId; } }

        private readonly string _RequirementStatusCode;
        /// <summary>
        /// The book requirement status for the section
        /// </summary>
        public string RequirementStatusCode { get { return _RequirementStatusCode; } }

        private readonly bool _IsRequired;
        /// <summary>
        /// Indicates if the book is required for the section.
        /// </summary>
        public bool IsRequired { get { return _IsRequired; } }


        /// <summary>
        /// Section Book constructor
        /// </summary>
        /// <param name="requirementStatus">enumeration value</param>
        public SectionBook(string bookId, string requirementStatusCode, bool isRequired)
        {
            if (string.IsNullOrEmpty(bookId))
            {
                throw new ArgumentNullException("bookId", "Book Id must be provided");
            }
            _BookId = bookId;
            _RequirementStatusCode = requirementStatusCode;
            _IsRequired = isRequired;
        }
    }
}
