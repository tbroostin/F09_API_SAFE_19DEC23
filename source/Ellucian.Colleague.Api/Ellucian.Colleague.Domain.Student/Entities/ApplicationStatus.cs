// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ApplicationStatus : CodeItem
    {
        private readonly string _SpecialProcessingCode;
        public string SpecialProcessingCode { get { return _SpecialProcessingCode; } }
        // ^ Define getter for the special processing code.

        public ApplicationStatus(string code, string description, string specialProcessingCode)
            : base(code, description)
        {
            _SpecialProcessingCode = specialProcessingCode; // assign the one from the parameter to the one in the class.
        }
    }
}