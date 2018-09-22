// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Denomination codes
    /// </summary>
    [Serializable]
    public class Denomination : GuidCodeItem
    {
        public Denomination(string guid, string code, string description)
            : base(guid, code, description)
        {
            // no additional work to do
        }
    }
}