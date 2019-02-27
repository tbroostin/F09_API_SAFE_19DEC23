// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Exceptions
{
    public class AlreadyApprovedByUserException : Exception
    {
        public AlreadyApprovedByUserException()
            : base()
        {

        }

        public AlreadyApprovedByUserException(string message)
            : base(message)
        {

        }
    }
}
