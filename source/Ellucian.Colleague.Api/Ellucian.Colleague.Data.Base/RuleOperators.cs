// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// Rule operator definitions
    /// </summary>
    public static class RuleOperators
    {
        /// <summary>
        /// The EQ operator
        /// </summary>
        public const string Equal = "EQ";

        /// <summary>
        /// The NE operator
        /// </summary>
        public const string NotEqual = "NE";

        /// <summary>
        /// The GT operator
        /// </summary>
        public const string GreaterThan = "GT";

        /// <summary>
        /// The LT operator
        /// </summary>
        public const string LessThan = "LT";

        /// <summary>
        /// The GE operator
        /// </summary>
        public const string GreaterThanOrEqual = "GE";

        /// <summary>
        /// The LE operator
        /// </summary>
        public const string LessThanOrEqual = "LE";

        /// <summary>
        /// The LIKE operator
        /// </summary>
        public const string Like = "LIKE";

        /// <summary>
        /// The UNLIKE operator
        /// </summary>
        public const string Unlike = "UNLIKE";
    }
}
