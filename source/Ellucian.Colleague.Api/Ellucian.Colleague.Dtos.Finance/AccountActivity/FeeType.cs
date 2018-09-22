// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Charges with the "Fee" type
    /// </summary>
    public partial class FeeType : NamedType
    {
        /// <summary>
        /// List of <see cref="ActivityDateTermItem">fee charges</see>
        /// </summary>
        public List<ActivityDateTermItem> FeeCharges { get; set; }
    }
}