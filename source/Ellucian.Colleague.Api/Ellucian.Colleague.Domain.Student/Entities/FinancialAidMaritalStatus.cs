// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Registration reason
    /// </summary>
    [Serializable]
    public class FinancialAidMaritalStatus: CodeItem
    {
        public string FinancialAidYear { get; set; }       

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationReason"/> class.
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="description">The description</param>
        public FinancialAidMaritalStatus( string code, string description)
            : base(code, description)
        {
        }
    }
}