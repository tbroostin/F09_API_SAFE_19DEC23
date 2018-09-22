// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Define the Test Results DTOS
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Test Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Name of Test
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The Id of the student who took the test
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Date the test was taken
        /// </summary>
        public DateTime DateTaken { get; set; }
        /// <summary>
        /// Score received for the test
        /// </summary>
        public int? Score { get; set; }
        /// <summary>
        /// Percentile for Admissions Test
        /// </summary>
        public int? Percentile { get; set; }
        /// <summary>
        /// Status of the Test
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// Status Date for the Test
        /// </summary>
        public DateTime? StatusDate { get; set; }
        /// <summary>
        /// Category of the test - Admissions, Placement, Other
        /// </summary>
        public TestType Category { get; set; }
        /// <summary>
        /// Set of Component Tests which make up this test
        /// </summary>
        public List<ComponentTest> ComponentTests { get; set; }
        /// <summary>
        /// Sub Tests which make up this test
        /// </summary>
        public List<SubTestResult> SubTests { get; set; }
    }
}
