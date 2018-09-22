/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Each option identifies which EFC to use on the ShoppingSheet
    /// </summary>
    public enum ShoppingSheetEfcOption
    {
        /// <summary>
        /// Always use the Federally Flagged ISIR EFC
        /// </summary>
        IsirEfc,

        /// <summary>
        /// Use the Institutionally flagged PROFILE EFC until an ISIR becomes Federally Flagged.
        /// </summary>
        ProfileEfcUntilIsirExists,

        /// <summary>
        /// Always use the Institutionally flagged PROFILE EFC
        /// </summary>
        ProfileEfc
    }
}
