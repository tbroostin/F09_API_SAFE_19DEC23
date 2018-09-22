// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible Confidentiality Types
    /// </summary>
    [Serializable]
    public enum ConfidentialityType
    {
        /// <summary>
        /// Public
        /// </summary>
        Public,
        /// <summary>
        /// Private
        /// </summary>
        Private,
    }
}