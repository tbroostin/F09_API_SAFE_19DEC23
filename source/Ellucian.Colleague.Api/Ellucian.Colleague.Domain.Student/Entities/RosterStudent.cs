// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RosterStudent
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; private set; }
        public string Id { get; private set; }

        public RosterStudent(string studentId, string lastName)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            Id = studentId;
            LastName = lastName;
        }
    }
}
