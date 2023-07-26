// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Aid application type
    /// </summary>
    [Serializable]
    public class AidApplicationType : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AidApplicationType"/> class.
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="description">The description</param>
        public AidApplicationType(string code, string description)
            : base(code, description)
        {
        }
    }
}
