// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An external system code
    /// </summary>
    [Serializable]
    public class ExternalSystem : CodeItem
    {
        public ExternalSystem(string code, string description)
            : base(code, description)
        {
        }
    }
}
