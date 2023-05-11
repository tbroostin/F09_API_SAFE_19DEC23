// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
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
        /// <summary>
        /// First action code options for the address relation type
        /// </summary>
        public string SpecialProcessingAction1 
        { 
            get { return _specialProcessingAction1; }
            set { if (value != null) { _specialProcessingAction1 = value; } }

        }
        private string _specialProcessingAction1;

        /// <summary>
        /// Second action code option for the address relation type
        /// </summary>
        public string SpecialProcessingAction2 
        { 
            get { return _specialProcessingAction2; }
            set { if (value != null) { _specialProcessingAction2 = value; } }
        }
        private string _specialProcessingAction2;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressRelationType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public AddressRelationType(string code, string description, string action1, string action2)
            : base(code, description)
        {
            _specialProcessingAction1 = action1;
            _specialProcessingAction2 = action2;
        }
    }
}