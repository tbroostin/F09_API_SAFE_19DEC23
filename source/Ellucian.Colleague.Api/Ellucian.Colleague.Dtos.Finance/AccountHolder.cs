// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// An accountholder - inherits from <see cref="Person">Person</see>
    /// </summary>
    public class AccountHolder : Person
    {
        /// <summary>
        /// A list of <see cref="DepositDue">Deposits Due</see> for the accountholder
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public IEnumerable<DepositDue> DepositsDue { get; set; }
    }
}
