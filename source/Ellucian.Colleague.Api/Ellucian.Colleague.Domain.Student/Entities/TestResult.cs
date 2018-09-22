// Copyright 2013-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class TestResult
    {
        private readonly string _code;
        private readonly string _description;
        private readonly string _studentId;
        private readonly DateTime _dateTaken;
        private readonly TestType _category;

        /// <summary>
        /// Test Code
        /// </summary>
        public string Code { get { return _code; } }
        /// <summary>
        /// Test description - this is not always the name of the test because it can be changed by student (required)
        /// </summary>
        public string Description { get { return _description; } }
        /// <summary>
        /// Student of the test result (required)
        /// </summary>
        public string StudentId { get { return _studentId; } }
        /// <summary>
        /// Date the Test was taken (required)
        /// </summary>
        public DateTime DateTaken { get { return _dateTaken; } }
        /// <summary>
        /// Score received
        /// </summary>
        public decimal? Score { get; set; }
        /// <summary>
        /// Test percentile based on the score
        /// </summary>
        public int? Percentile { get; set; }
        /// <summary>
        /// Status code
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// Status date
        /// </summary>
        public DateTime? StatusDate { get; set; }
        /// <summary>
        /// Test Category - admissions, placement, other
        /// </summary>
        public TestType Category { get { return _category; } }
        /// <summary>
        /// Component Tests 
        /// </summary>
        public List<ComponentTest> ComponentTests { get; set; }
        public List<SubTestResult> SubTests { get; set; }

        public TestResult(string studentId, string code, string description, DateTime dateTaken, TestType category)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            _studentId = studentId;
            _code = code;
            _description = description;
            _dateTaken = dateTaken;
            _category = category;
            ComponentTests = new List<ComponentTest>();
            SubTests = new List<SubTestResult>();
        }
    }



}
