// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Address Relation Type
    /// </summary>
    [Serializable]
    public class AddressRelationType : CodeItem
    {
        public string SpecialProcessingAction1;
        public string SpecialProcessingAction2;
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressRelationType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public AddressRelationType(string code, string description, string action1, string action2)
            : base(code, description)
        {
            SpecialProcessingAction1 = action1;
            SpecialProcessingAction2 = action2;
        }
    }
}