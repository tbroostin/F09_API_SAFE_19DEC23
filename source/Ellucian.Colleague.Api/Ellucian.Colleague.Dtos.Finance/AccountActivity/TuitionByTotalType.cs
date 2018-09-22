// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Charges with the "Tuition by Total" type
    /// </summary>
    public partial class TuitionByTotalType : NamedType
    {
        /// <summary>
        /// A list of <see cref="ActivityTuitionItem">tuition</see> items
        /// </summary>
        public List<ActivityTuitionItem> TotalCharges { get; set; }
    }
}