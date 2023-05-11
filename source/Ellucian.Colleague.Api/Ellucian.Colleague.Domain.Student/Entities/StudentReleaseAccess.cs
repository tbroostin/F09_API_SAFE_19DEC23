// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{

    /// <summary>
    /// Student Release Access Codes
    /// </summary>
    [Serializable]
    public class StudentReleaseAccess : CodeItem
    {
        /// <summary>
        /// Student Release Access Comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="StudentReleaseAccess"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="comments">The access comments.</param>
        public StudentReleaseAccess(string code, string description, string comments)
             : base(code, description)
        {
            Comments = comments;
        }
    }
}
