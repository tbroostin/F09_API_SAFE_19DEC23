// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    ///  This class substitutes the TaxForms DTO enum so that, if new
    ///  tax forms are added, the APIs do no have to be versioned.
    /// </summary>
    public static class TaxFormTypes
    {
        /// <summary>
        /// Tax form W-2
        /// </summary>
        public const string FormW2 = "FormW2";

        /// <summary>
        /// Tax form W-2c.
        /// </summary>
        public const string FormW2C = "W2C";

        /// <summary>
        /// Tax form 1095-C.
        /// </summary>
        public const string Form1095C = "Form1095C";

        /// <summary>
        /// Tax form 1098(Es and Ts combined).
        /// </summary>
        public const string Form1098 = "Form1098";

        /// <summary>
        /// Tax form 1098-T.
        /// </summary>
        public const string Form1098T = "Form1098T";

        /// <summary>
        /// Tax form 1098-E.
        /// </summary>
        public const string Form1098E = "Form1098E";

        /// <summary>
        /// Tax form T4.
        /// </summary>
        public const string FormT4 = "FormT4";

        /// <summary>
        /// Tax form T4A.
        /// </summary>
        public const string FormT4A = "FormT4A";

        /// <summary>
        /// Tax form T2202A.
        /// </summary>
        public const string FormT2202A = "FormT2202A";

        /// <summary>
        /// Tax form 1099-MISC.
        /// </summary>
        public const string Form1099MI = "Form1099MI";

        /// <summary>
        /// Tax form 1099-NEC.
        /// </summary>
        public const string Form1099NEC = "Form1099NEC";
    }
}
