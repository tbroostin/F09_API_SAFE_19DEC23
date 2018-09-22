// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Charges with the "Other" type
    /// </summary>
    public partial class OtherType : NamedType
    {
        /// <summary>
        /// List of <see cref="ActivityDateTermItem">other charges</see>
        /// </summary>
        public List<ActivityDateTermItem> OtherCharges { get; set; }
    }
}