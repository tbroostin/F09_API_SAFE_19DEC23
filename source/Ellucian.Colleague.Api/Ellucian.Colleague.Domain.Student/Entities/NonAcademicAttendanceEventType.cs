// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A nonacademic attendance event type
    /// </summary>
    [Serializable]
    public class NonAcademicAttendanceEventType : CodeItem
    {
        public NonAcademicAttendanceEventType(string code, string description)
            : base(code, description)
        {
        }
    }
}
