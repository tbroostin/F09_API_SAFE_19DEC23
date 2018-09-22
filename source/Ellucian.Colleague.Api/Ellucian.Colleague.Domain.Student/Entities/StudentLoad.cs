// Copyright 2013-2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentLoad : CodeItem
    {
        public string Sp1 { get; set; }

        public StudentLoad(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}