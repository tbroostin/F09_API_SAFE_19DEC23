// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Fee assessed for e-commerce transactions
    /// </summary>
    public class ConvenienceFee
    {
        /// <summary>
        /// Unique code of this convenience fee
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Convenience fee description
        /// </summary>
        public string Description { get; set; }
    }
}