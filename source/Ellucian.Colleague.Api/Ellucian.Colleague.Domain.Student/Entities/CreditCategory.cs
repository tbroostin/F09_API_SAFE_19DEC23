// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

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

        /// <summary>
        /// Cregit category like "I" for Institutional, "C" Continuing ed etc.
        /// </summary>
        public string Category { get; set; }

        public CreditCategory(string guid, string code, string description, CreditType creditType)
            : base(guid, code, description)
        {
            _creditType = creditType;
        }
    }
}