// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ScheduleTerm : GuidCodeItem
    {
        /// <summary>
        /// Constructor for ScheduleTerm
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public ScheduleTerm(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
