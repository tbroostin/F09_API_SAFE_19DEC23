// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Different reasons that a student might want to hold a transcript request
    /// </summary>
    [Serializable]
    public class HoldRequestType : CodeItem
    {
        /// <summary>
        /// The Cap Size constructor
        /// </summary>
        /// <param name="code"> The code of the hold request type </param>
        /// <param name="description"> The description of the hold request type</param>
        public HoldRequestType(string code, string description)
            : base(code, description)
        {
        }
    }

}
