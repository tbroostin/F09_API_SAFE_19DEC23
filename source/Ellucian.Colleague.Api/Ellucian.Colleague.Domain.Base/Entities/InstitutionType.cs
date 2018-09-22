// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;
namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Institution type codes
    /// </summary>
    [Serializable]
    public class InstitutionType : CodeItem
    {
        public string Category { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public InstitutionType(string code, string description, string category)
            : base(code, description)
        {
            Category = category;
        }
    }
}