// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Exceptions
{
    public class ExistingGraduationException : System.Exception
    {
        private readonly string _ExistingGraduationApplicationId;
        public string ExistingGraduationApplicationId { get { return _ExistingGraduationApplicationId; } }

        public ExistingGraduationException()
        {

        }

        public ExistingGraduationException(string message, string existingGraduationApplicationId)
            : base(message)
        {
            _ExistingGraduationApplicationId = existingGraduationApplicationId;
        }
    }
}
