// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class ChargeCode : CodeItem
    {
        private readonly int _Priority;

        /// <summary>
        /// Priority of this charge for allocation purposes; 1 is high, 999 is low
        /// </summary>
        public int Priority { get { return _Priority; } }

        /// <summary>
        /// Charge group to which this charge belongs
        /// </summary>
        public string ChargeGroup { get; set; }

        /// <summary>
        /// Constructor for ChargeCode
        /// </summary>
        /// <param name="code">Charge code</param>
        /// <param name="description">Charge description</param>
        /// <param name="priority">Priority of charge</param>
        public ChargeCode(string code, string description, int? priority) : base(code, description)
        {
            // No priority specified means the lowest possible priority: 999
            if (!priority.HasValue || priority.Value == 0)
            {
                priority = 999;
            }
            // Priority must be 1 to 999
            if (priority < 1 || priority  > 999)
            {
                throw new ArgumentOutOfRangeException("priority", "Priority must be between 1 and 999");
            }

            _Priority = priority.Value;
        }
    }
}
