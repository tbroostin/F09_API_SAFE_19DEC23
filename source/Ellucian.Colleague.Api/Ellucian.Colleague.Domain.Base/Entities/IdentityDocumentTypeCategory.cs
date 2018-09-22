// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an Identity Document Type
    /// </summary>
    [Serializable]
    public enum IdentityDocumentTypeCategory
    {
        /// <summary>
        /// Passport
        /// </summary>
        Passport,
        /// <summary>
        /// Photo ID
        /// </summary>
        PhotoId,
        /// <summary>
        /// other
        /// </summary>
        Other,
    }
}