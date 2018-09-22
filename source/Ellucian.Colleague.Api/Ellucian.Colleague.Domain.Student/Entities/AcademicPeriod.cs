// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AcademicPeriod : GuidCodeItem
    {
        public string SessionId { get; set; }

        public string Category { get; set; }

        // Required
        private readonly DateTime _StartDate;
        public DateTime StartDate { get { return _StartDate; } }

        // Required
        private readonly DateTime _EndDate;
        public DateTime EndDate { get { return _EndDate; } }

        // Required
        private readonly int _ReportingYear;
        public int ReportingYear { get { return _ReportingYear; } }

        // Required
        private readonly int _Sequence;
        public int Sequence { get { return _Sequence; } }

        // Required
        private readonly string _ReportingTerm;
        public string ReportingTerm { get { return _ReportingTerm; } }

        private string _ParentId;
        public string ParentId { get { return _ParentId; } }

        private string _PrecedingId;
        public string PrecedingId { get { return _PrecedingId; } }

        private readonly List<RegistrationDate> _RegistrationDates;
        public List<RegistrationDate> RegistrationDates { get { return _RegistrationDates; } }

        public AcademicPeriod(string guid, string code, string description, DateTime startDate, DateTime endDate, int reportingYear, int sequence, string reportingTerm, string preceedingId, string parentId, List<RegistrationDate> registrationDates)
            : base(guid, code, description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate");
            }
            if (endDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("endDate");
            }
            if (reportingYear < 0)
            {
                throw new ArgumentException("Term Reporting Year can not be negative");
            }
            if (sequence < 0)
            {
                throw new ArgumentException("Term Sequence can not be negative");
            }
            if (String.IsNullOrEmpty(reportingTerm))
            {
                throw new ArgumentNullException("reportingTerm");
            }

            _StartDate = startDate;
            _EndDate = endDate;
            _ReportingYear = reportingYear;
            _Sequence = sequence;
            _PrecedingId = preceedingId;
            _ReportingTerm = reportingTerm;
            _ParentId = parentId;
            _RegistrationDates = registrationDates;
        }

        public void AddRegistrationDates(RegistrationDate registrationDates)
        {
            _RegistrationDates.Add(registrationDates);
        }
    }
}
