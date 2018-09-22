// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// OfficeCodeType identifies an office code as belonging to specific office.
    /// For instance, the FA and F office codes might both belong to the FinancialAid office type.
    /// </summary>
    [Serializable]
    public enum OfficeCodeType
    {
        /// <summary>
        /// The FinancialAid OfficeCodeType
        /// </summary>
        FinancialAid,

        /// <summary>
        /// The Other OfficeCodeType
        /// </summary>
        Other
    }
}
