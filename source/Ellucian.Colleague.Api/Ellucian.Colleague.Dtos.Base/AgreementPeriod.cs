// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Period of time to which an agreement can apply
    /// </summary>
    public class AgreementPeriod
    {
        /// <summary>
        /// Unique code for the agreement period
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the agreement period
        /// </summary>
        public string Description { get; set; }
    }
}