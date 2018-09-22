// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SectionTextbook
    {
        private readonly Book _textbook;
        private readonly string _sectionId;
        private readonly string _requirementStatusCode;
        private readonly SectionBookAction _action;

        /// <summary>
        /// The textbook being added to a section
        /// </summary>
        public Book Textbook { get { return _textbook; } }

        /// <summary>
        /// The Id of the section that the book is being added to
        /// </summary>
        public string SectionId { get { return _sectionId; } }

        /// <summary>
        /// The requirement status of the book being added to the section.
        /// </summary>
        public string RequirementStatusCode { get { return _requirementStatusCode; } }

        /// <summary>
        /// Action to be taken for a section book assignment
        /// </summary>
        public SectionBookAction Action { get { return _action; } }

        /// <summary>
        /// Constructor for Book
        /// </summary>
        /// <param name="sectionId">The Id of the section to add a book to.</param>
        /// <param name="bookId">The Id of the book to add to a section.</param>
        /// <param name="bookStatus">The requirement status of the book being added to the section.</param>
        /// <param name="action"><see cref="SectionBookAction"/></param>
        public SectionTextbook(Book textbook, string sectionId, string bookStatusCode, SectionBookAction action)
        {
            if (textbook == null)
            {
                throw new ArgumentNullException("textbook", "Textbook can not be null");
            }
            
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section Id can not be null or empty.");
            }

            if (string.IsNullOrEmpty(textbook.Isbn))
            {
                if (string.IsNullOrEmpty(textbook.Title) || string.IsNullOrEmpty(textbook.Author) || string.IsNullOrEmpty(textbook.Publisher) || string.IsNullOrEmpty(textbook.Copyright))
                {
                    throw new ApplicationException("Either the isbn or the standard book info must all be provided.");
                }
            }

            _sectionId = sectionId;
            _requirementStatusCode = bookStatusCode;
            _action = action;
            _textbook = textbook;
        }
    }
}
