// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Get student records release deny access
    /// </summary>
    [Serializable]
    public class StudentRecordsReleaseDenyAccess
    {
        /// <summary>
        /// Flag indicates whether student can deny access to all its student records release
        /// </summary>
        public bool DenyAccessToAll { get; set; }
    }
}
