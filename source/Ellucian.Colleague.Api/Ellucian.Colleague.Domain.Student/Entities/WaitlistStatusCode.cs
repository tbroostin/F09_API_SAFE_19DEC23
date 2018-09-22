// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A waitlist status code
    /// </summary>
    [Serializable]
    public class WaitlistStatusCode : CodeItem
    {
        private readonly WaitlistStatus _status;
        /// <summary>
        /// Status of the waitlist code
        /// </summary>
        public WaitlistStatus Status { get { return _status; } }

        public WaitlistStatusCode(string code, string description, WaitlistStatus status)
            : base(code, description)
        {
            _status = status;
        }
    }
}
