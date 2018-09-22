using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An institutionally-defined Test such as Admissions or Placement
    /// </summary>
    public class Test
    {
        /// <summary>
        /// Unique Code for this Test
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Test Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Test Type
        /// </summary>
        public TestType Type { get; set; }
        /// <summary>
        /// Maximum score that this test can have.
        /// </summary>
        public int? MaximumScore { get; set; }
        /// <summary>
        /// List of Sub-Tests for this primary Test
        /// </summary>
        public IEnumerable<string> SubTestCodes { get; set; }
    }

}
