// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Information about the book options
    /// </summary>
    [Serializable]
    public class BookOption : CodeItem
    {
        private readonly bool _IsRequired;
        /// <summary>
        /// Bool for if the book is required for a section
        /// </summary>
        public bool IsRequired { get { return _IsRequired; } }

        /// <summary>
        /// An option for what to do with a book
        /// </summary>
        /// <param name="code">Unique code for this book option</param>
        /// <param name="description">Description of this book option</param>
        /// <param name="isRequired">Bool for if the book is required for a section</param>
        public BookOption(string code, string description, bool isRequired)
            : base(code, description)
        {
            _IsRequired = isRequired;
        }
    }
}
