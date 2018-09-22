// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentAffiliation
    {
        private string _StudentId;
        private string _AffiliationCode;

        public string StudentId { get { return _StudentId; } }
        public string AffiliationCode { get { return _AffiliationCode; } }

        public string Term { get; set; }
        public string AffiliationName { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StatusCode { get; set; }
        public string StatusName { get; set; }

        public StudentAffiliation(string studentId, string affiliationId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(affiliationId))
            {
                throw new ArgumentNullException("affiliationId cannot be null or empty");
            }
            _StudentId = studentId;
            _AffiliationCode = affiliationId;
        }
    }
}
