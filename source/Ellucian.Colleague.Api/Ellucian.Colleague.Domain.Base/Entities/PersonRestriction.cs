// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonRestriction
    {
        private readonly string _Id;
        private readonly string _StudentId;
        private readonly string _RestrictionId;
        private readonly DateTime? _StartDate;
        private readonly DateTime? _EndDate;
        private readonly int? _Severity;
        private readonly bool _OfficeUseOnly;
        private readonly string _Guid;

        public string Guid { get { return _Guid; } }
        public string Id { get { return _Id; } }
        public string StudentId { get { return _StudentId; } }
        public string RestrictionId { get { return _RestrictionId; } }
        public DateTime? StartDate { get { return _StartDate; } }
        public DateTime? EndDate { get { return _EndDate; } }
        public int? Severity { get { return _Severity; } }
        public bool OfficeUseOnly { get { return _OfficeUseOnly; } }
        public string Comment { get; set; }
        public string NotificationIndicator { get; set; }

        public PersonRestriction(string id, string studentId, string restrictionId, DateTime? startDate, DateTime? endDate, int? severity, string visibleToUsers)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id is required");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id is required");
            }
            if (string.IsNullOrEmpty(restrictionId))
            {
                throw new ArgumentNullException("restrictionId", "Restriction Id is required");
            }

            _Id = id;
            _StudentId = studentId;
            _RestrictionId = restrictionId;
            _StartDate = startDate;
            _EndDate = endDate;
            _Severity = severity;
            _OfficeUseOnly = true;
            if (!string.IsNullOrEmpty(visibleToUsers) && visibleToUsers.Equals("Y"))
            {
                _OfficeUseOnly = false;
            }
        }

        public PersonRestriction(string guid, string id, string studentId, string restrictionId, DateTime? startDate, DateTime? endDate, int? severity, string visibleToUsers)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", string.Format("STUDENT.RESTRICTIONS GUID is required, ID: '{0}'.", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id is required");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id is required");
            }
            if (string.IsNullOrEmpty(restrictionId))
            {
                throw new ArgumentNullException("restrictionId", "Restriction Id is required");
            }

            _Guid = guid;
            _Id = id;
            _StudentId = studentId;
            _RestrictionId = restrictionId;
            _StartDate = startDate;
            _EndDate = endDate;
            _Severity = severity;
            _OfficeUseOnly = true;
            if (!string.IsNullOrEmpty(visibleToUsers) && visibleToUsers.Equals("Y"))
            {
                _OfficeUseOnly = false;
            }
        }
    }
}
