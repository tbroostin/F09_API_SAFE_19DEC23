// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Petition status code and description.
    /// </summary>
    [Serializable]
    public class PetitionStatus : CodeItem
    {
        /// <summary>
        /// Indicates whether this petition status grants the student the petition or not.
        /// </summary>
        public bool IsGranted { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PetitionStatus"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public PetitionStatus(string code, string description, bool isGranted = false)
            : base(code, description)
        {
            IsGranted = isGranted;
        }
    }
}
