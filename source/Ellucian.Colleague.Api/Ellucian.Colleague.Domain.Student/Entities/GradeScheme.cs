// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class GradeScheme : GuidCodeItem
    {
        /// <summary>
        /// The date on which the grade scheme becomes valid.
        /// </summary>
        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// The date after which the grade scheme is no longer valid.
        /// </summary>
        public DateTime? EffectiveEndDate { get; set; }

        public GradeScheme(string guid, string code, string description)
            : base(guid, code, description)
        {
            // no additional work to do
        }
    }
}
