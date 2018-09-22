// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;
namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Suffix codes
    /// </summary>
    [Serializable]
    public class Suffix : CodeItem
    {
        private string _abbreviation;
        /// <summary>
        /// Abbreviation corresponding to the suffix code
        /// </summary>
        public string Abbreviation { get { return _abbreviation; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Suffix"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="abbreviation">The abbreviation.</param>
        public Suffix(string code, string description, string abbreviation)
            :base(code, description)
        {
            if (string.IsNullOrEmpty(abbreviation))
            {
                throw new ArgumentNullException("abbreviation", "Abbreviation must be provided.");
            }
            _abbreviation = abbreviation;
        }
    }
}