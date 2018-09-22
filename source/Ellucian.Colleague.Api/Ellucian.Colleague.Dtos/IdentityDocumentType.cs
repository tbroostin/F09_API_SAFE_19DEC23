// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a identity document type
    /// </summary>
    [DataContract]
    public class IdentityDocumentType : CodeItem2
    {
        /// <summary>
        /// The <see cref="IdentityDocumentType">entity type</see> for the identity document types
        /// </summary>
        [DataMember(Name = "category")]
        public IdentityDocumentTypeCategory identityDocumentTypeCategory { get; set; }
    }
}