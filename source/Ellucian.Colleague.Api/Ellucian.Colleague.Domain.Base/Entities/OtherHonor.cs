// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class OtherHonor : GuidCodeItem
    {
        public OtherHonor(string guid, string code, string description)
            : base(guid, code, description)
        {
            // No additional work to do
        }
    }
}
