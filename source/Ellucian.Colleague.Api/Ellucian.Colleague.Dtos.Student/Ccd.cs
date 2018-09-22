using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Certificate degree
    /// </summary>
    public class Ccd
    {
        /// <summary>
        /// Unique code for this certificate, credential, or diploma
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Ccd description
        /// </summary>
        public string Description { get; set; }
    }
}