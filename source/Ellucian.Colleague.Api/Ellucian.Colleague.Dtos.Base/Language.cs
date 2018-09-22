using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Language code and description
    /// </summary>
    public class Language
    {
        /// <summary>
        /// Unique system code for this language
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Language description
        /// </summary>
        public string Description { get; set; }
    }
}