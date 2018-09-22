// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Status of a registration payment
    /// </summary>
    public enum RegistrationPaymentStatus
    {
        /// <summary>
        /// The registration has changes that require payment processing
        /// </summary>
        New, 
        /// <summary>
        /// The registration terms and conditions have been accepted
        /// </summary>
        Accepted, 
        /// <summary>
        /// The registration payment requirements have been met
        /// </summary>
        Complete, 
        /// <summary>
        /// The registration payment failed
        /// </summary>
        Error
    }
}
