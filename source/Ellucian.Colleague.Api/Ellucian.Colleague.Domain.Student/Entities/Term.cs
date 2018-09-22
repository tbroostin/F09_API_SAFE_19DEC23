// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Term //: CodeItem  (Term is not a CodeItem - CodeItems have required descriptions)
    {
        private readonly string _RecordGuid;
        public string RecordGuid { get { return _RecordGuid; } }

        // Required
        private readonly string _Code;
        public string Code { get { return _Code; } }

        public string Description;
        public string SessionId { get; set; }
        
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
        private readonly bool _DefaultOnPlan;
        public bool DefaultOnPlan { get { return _DefaultOnPlan; } }

        // Required
        private readonly bool _ForPlanning;
        public bool ForPlanning { get { return _ForPlanning; } }

        public PeriodType? FinancialPeriod { get; set; }

        // Required
        private readonly string _ReportingTerm;
        public string ReportingTerm { get { return _ReportingTerm; } }

        //Optional
        private readonly List<RegistrationDate> _RegistrationDates;
        public List<RegistrationDate> RegistrationDates { get { return _RegistrationDates; } }
        public bool IsActive { get; set; }
        public List<int?> FinancialAidYears { get; set; }
        public bool UseTermInBestFitCalculations { get; set; }

        // Required
        private readonly bool _RegistrationPriorityRequired;
        public bool RegistrationPriorityRequired { get { return _RegistrationPriorityRequired; } }

        /// <summary>
        /// List of valid academic levels for the term
        /// </summary>
        private List<string> _academicLevels = new List<string>();
        public ReadOnlyCollection<string> AcademicLevels { get; set; }

        /// <summary>
        /// List of applicable session cycles for the term
        /// </summary>
        private List<string> _sessionCycles = new List<string>();
        public ReadOnlyCollection<string> SessionCycles { get; set; }

        /// <summary>
        /// List of applicable yearly cycles for the term
        /// </summary>
        private List<string> _yearlyCycles = new List<string>();
        public ReadOnlyCollection<string> YearlyCycles { get; set; }
        
        // Optional
        // If session has SESS.INTG.CATEGORY then include it
        public string Category { get; set; }


        public Term(string recordGuid, string code, string description, DateTime startDate, DateTime endDate, int reportingYear, int sequence, bool defaultOnPlan, bool forPlanning, string reportingTerm, bool registrationPriorityRequired)
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

            _RecordGuid = recordGuid;
            _Code = code;
            // If there is no description (which is valid in colleague), use the code instead
            Description = string.IsNullOrEmpty(description) ? code : description;
            _StartDate = startDate;
            _EndDate = endDate;
            _ReportingYear = reportingYear;
            _Sequence = sequence;
            _DefaultOnPlan = defaultOnPlan;
            _ForPlanning = forPlanning;
            _ReportingTerm = reportingTerm;
            _RegistrationPriorityRequired = registrationPriorityRequired;
            _RegistrationDates = new List<RegistrationDate>();
            AcademicLevels = _academicLevels.AsReadOnly();
            SessionCycles = _sessionCycles.AsReadOnly();
            YearlyCycles = _yearlyCycles.AsReadOnly();
        }


        public Term(string code, string description, DateTime startDate, DateTime endDate, int reportingYear, int sequence, bool defaultOnPlan, bool forPlanning, string reportingTerm, bool registrationPriorityRequired)
            //: base(code, description)
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


            _Code = code;
            // If there is no description (which is valid in colleague), use the code instead
            Description = string.IsNullOrEmpty(description) ? code : description;
            _StartDate = startDate;
            _EndDate = endDate;
            _ReportingYear = reportingYear;
            _Sequence = sequence;
            _DefaultOnPlan = defaultOnPlan;
            _ForPlanning = forPlanning;
            _ReportingTerm = reportingTerm;
            _RegistrationPriorityRequired = registrationPriorityRequired;
            _RegistrationDates = new List<RegistrationDate>();
            AcademicLevels = _academicLevels.AsReadOnly();
            SessionCycles = _sessionCycles.AsReadOnly();
            YearlyCycles = _yearlyCycles.AsReadOnly();
        }


        public void AddRegistrationDates(RegistrationDate registrationDates)
        {
            _RegistrationDates.Add(registrationDates);
        }

        /// <summary>
        /// Add an academic level for this term
        /// </summary>
        /// <param name="code">Academic level code to add</param>
        public void AddAcademicLevel(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "An academic level code is required.");
            }
            _academicLevels.Add(code);
        }

        /// <summary>
        /// Add an session cycle for this term
        /// </summary>
        /// <param name="code">Academic level code to add</param>
        public void AddSessionCycle(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "A session cycle is required.");
            }
            _sessionCycles.Add(code);
        }

        /// <summary>
        /// Add an session cycle for this term
        /// </summary>
        /// <param name="code">Academic level code to add</param>
        public void AddYearlyCycle(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "A yearly cycle is required.");
            }
            _yearlyCycles.Add(code);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Term other = obj as Term;
            if (other == null)
            {
                return false;
            }
            return other.Code.Equals(_Code);
        }

        public override int GetHashCode()
        {
            return _Code.GetHashCode();
        }
    }
}
