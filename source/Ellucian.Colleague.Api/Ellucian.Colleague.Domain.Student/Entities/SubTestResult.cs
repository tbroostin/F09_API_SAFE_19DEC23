// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SubTestResult
    {
        private readonly string _code;
        private readonly string _description;
        private readonly DateTime _dateTaken;

        public string Code { get { return _code; } }
        public string Description { get { return _description; } }
        public DateTime DateTaken { get { return _dateTaken; } }
        public decimal? Score { get; set; }
        public int? Percentile { get; set; }
        public string StatusCode { get; set; }
        public DateTime? StatusDate { get; set; }

        public SubTestResult(string code, string description, DateTime dateTaken)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            _code = code;
            _description = description;
            _dateTaken = dateTaken;
        }
    }
}
