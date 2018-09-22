// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CreditCategory : GuidCodeItem
    {
        /// <summary>
        /// The credit type
        /// </summary>
        private readonly CreditType _creditType;
        public CreditType CreditType { get { return _creditType; } }

        public CreditCategory(string guid, string code, string description, CreditType creditType)
            : base(guid, code, description)
        {
            _creditType = creditType;
        }
    }
}