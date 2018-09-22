// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Exceptions
{
    public class ExistingSectionWaiverException : System.Exception
    {
        private readonly string _ExistingSectionWaiverId;
        public string ExistingSectionWaiverId { get { return _ExistingSectionWaiverId; } }

        public ExistingSectionWaiverException()
        {

        }

        public ExistingSectionWaiverException(string message, string existingStudentReqWaiverId)
            : base(message)
        {
            _ExistingSectionWaiverId = existingStudentReqWaiverId;
        }
    }
}
