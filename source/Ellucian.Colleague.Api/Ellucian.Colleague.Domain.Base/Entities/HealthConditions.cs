/* Copyright 2014 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Health condition code and description
    /// </summary>
    [Serializable]
    public class HealthConditions : CodeItem
    {
        public HealthConditions(string code, string description)
            : base(code, description)
        {

        }
    }
}
