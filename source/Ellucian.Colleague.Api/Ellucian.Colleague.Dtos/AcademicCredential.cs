// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// An Academic Credential Type
    /// </summary>
    [DataContract]
    public class AcademicCredential : AbbreviationItem
    {
        /// <summary>
        /// A type of academic credential
        /// </summary>
        [DataMember(Name = "type")]
        public AcademicCredentialType AcademicCredentialType { get; set; }

    }
}
