// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Period of time to which an agreement can apply
    /// </summary>
    [Serializable]
    public class AgreementPeriod
    {
        /// <summary>
        /// Unique code for the agreement period
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Description of the agreement period
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="AgreementPeriod"/> object.
        /// </summary>
        /// <param name="code">Agreement period code</param>
        /// <param name="description">Agreement period description</param>
        public AgreementPeriod(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Agreement periods must have a code.");
            }
            Code = code;
            Description = description;
        }
    }
}
