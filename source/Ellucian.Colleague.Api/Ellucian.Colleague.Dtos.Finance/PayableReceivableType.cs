// Copyright 2016 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A receivable type and flag indicating whether or not the type is payable
    /// </summary>
    public class PayableReceivableType
    {
        /// <summary>
        /// Receivable type code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Flag indicating whether or not the receivable type may be paid against by account holders
        /// </summary>
        public bool IsPayable { get; set; }
    }
}
