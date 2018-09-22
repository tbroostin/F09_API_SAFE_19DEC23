// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Specialization : CodeItem
    {
        public Specialization(string code, string desc)
            : base(code, desc)
        {

        }
    }
}
