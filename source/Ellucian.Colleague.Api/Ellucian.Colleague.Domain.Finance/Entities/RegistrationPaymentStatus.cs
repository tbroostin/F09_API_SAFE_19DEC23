// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// The possible statuses for the registration payment status in Immediate Payment Control.
    /// </summary>
    [Serializable]
    public enum RegistrationPaymentStatus
    {
        /// <summary>
        /// The status is New - it has new activity that must be approved.
        /// </summary>
        New, 
        /// <summary>
        /// The terms and conditions have been accepted.
        /// </summary>
        Accepted, 
        /// <summary>
        /// The student has completed his payment requirements.
        /// </summary>
        Complete, 
        /// <summary>
        /// An error has occurred and prevented the student from completing his payment requirements.
        /// </summary>
        Error
    }
}
