// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// EnrollmentStatus
    /// </summary>
    [Serializable]
    public class EnrollmentStatus : GuidCodeItem
    {

        private EnrollmentStatusType _enrollmentStatusType;
        public EnrollmentStatusType EnrollmentStatusType { get { return _enrollmentStatusType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnrollmentStatus"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the EnrollmentStatus</param>
        /// <param name="description">Description or Title of the EnrollmentStatus</param>
        /// <param name="enrollmentStatus">Enrollment status type of EnrollmentStatus</param>
        public EnrollmentStatus(string guid, string code, string description, EnrollmentStatusType enrollmentStatusType)
            : base (guid, code, description)
        {
            _enrollmentStatusType = enrollmentStatusType;
        }
    }
}
