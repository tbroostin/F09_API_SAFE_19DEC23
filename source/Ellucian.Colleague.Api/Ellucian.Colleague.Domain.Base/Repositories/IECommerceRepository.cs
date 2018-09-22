// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for e-commerce functions
    /// </summary>
    public interface IECommerceRepository
    {
        /// <summary>
        /// Convenience fees
        /// </summary>
        IEnumerable<ConvenienceFee> ConvenienceFees { get; }

        /// <summary>
        /// Distributions
        /// </summary>
        IEnumerable<Distribution> Distributions { get; } 

        /// <summary>
        /// Payment methods
        /// </summary>
        IEnumerable<PaymentMethod> PaymentMethods { get; } 
    }
}
