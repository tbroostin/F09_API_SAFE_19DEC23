// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    ///  Academic Credential
    /// </summary>
    [Serializable]
    public class AcadCredential : GuidCodeItem
    {

        public AcademicCredentialType AcademicCredentialType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcadCredential"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the Academic Credential</param>
        /// <param name="description">Description or Title of the Academic Credential</param>
        /// <param name="academicCredentialType">A type of Academic Credential</param>
        public AcadCredential(string guid, string code, string description, AcademicCredentialType academicCredentialType)
            : base (guid, code, description)
        {
            AcademicCredentialType = academicCredentialType;
        }
    }
}