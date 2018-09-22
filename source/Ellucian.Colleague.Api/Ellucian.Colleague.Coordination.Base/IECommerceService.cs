// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base
{
    /// <summary>
    /// Interface for E-Commerce services
    /// </summary>
    public interface IECommerceService
    {
        /// <summary>
        /// Get all convenience fees
        /// </summary>
        /// <returns>List of convenience fees</returns>
        IEnumerable<ConvenienceFee> GetConvenienceFees();
    }
}
