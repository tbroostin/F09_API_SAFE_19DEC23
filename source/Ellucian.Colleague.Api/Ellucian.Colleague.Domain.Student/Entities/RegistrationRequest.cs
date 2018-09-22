// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RegistrationRequest
    {
        public string StudentId { get; set; }
        public List<SectionRegistration> Sections { get; set; }
        public bool CreateStudentFlag { get; set; }

        public RegistrationRequest(string studentId, IEnumerable<SectionRegistration> sections)
        {
            StudentId = studentId;
            Sections = sections.ToList();
        }
    }
}
