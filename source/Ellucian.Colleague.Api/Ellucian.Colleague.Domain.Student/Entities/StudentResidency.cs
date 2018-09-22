// Copyright 2016 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student residency and date
    /// </summary>
    [Serializable]
    public class StudentResidency
    {
        private readonly string _residency;
        private readonly DateTime? _date;

        /// <summary>
        /// Student residency
        /// Ex: INST (in state)
        /// </summary>
        public string Residency { get { return _residency; } }        
        /// <summary>
        /// Date for residency
        /// </summary>
        public DateTime? Date { get { return _date; } }

        /// <summary>
        /// Initialize the Student Residency method
        /// </summary>
        /// <param name="residency">Student residency</param>
        /// <param name="date">Student residency date</param>
        public StudentResidency(string residency, DateTime? date)
        {
            if (string.IsNullOrEmpty(residency))
            {
                throw new ArgumentNullException("residency");
            }
            _residency = residency;
            _date = date;
        }
    }
}