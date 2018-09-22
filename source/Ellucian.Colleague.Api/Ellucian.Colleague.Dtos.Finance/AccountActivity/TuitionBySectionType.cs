// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Charges with the "Tuition by Section" type
    /// </summary>
    public partial class TuitionBySectionType : NamedType
    {
        /// <summary>
        /// A list of <see cref="ActivityTuitionItem">tuition</see> items
        /// </summary>
        public List<ActivityTuitionItem> SectionCharges { get; set; }
    }
}