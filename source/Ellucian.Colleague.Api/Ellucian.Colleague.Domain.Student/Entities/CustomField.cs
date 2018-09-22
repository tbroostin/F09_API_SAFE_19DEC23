// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CustomField
    {
        public String EntitySchema { get; set; }
        public String AttributeSchema { get; set; }
        public String Value { get; set; }
    }
}
