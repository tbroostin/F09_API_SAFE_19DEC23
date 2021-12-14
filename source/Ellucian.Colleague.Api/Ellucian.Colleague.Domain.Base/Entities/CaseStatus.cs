// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Case Priority code and description
    /// </summary>
    [Serializable]
    public class CaseStatus : CodeItem
    {
        public string ActionCode { get; set; }
        public CaseStatus(string code, string description, string actionCode)
           : base(code, description)
        {
            ActionCode = actionCode;
        }
    }
}
