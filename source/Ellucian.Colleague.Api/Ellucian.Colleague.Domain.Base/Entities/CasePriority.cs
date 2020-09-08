// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;


namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Case Priority code and description
    /// </summary>
    [Serializable]
    public class CasePriority : CodeItem
    {
        public CasePriority(string code, string description)
            : base(code, description)
        {

        }
    }
}
