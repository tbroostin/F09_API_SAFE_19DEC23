// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ClassLevel : CodeItem
    {
        public int? SortOrder { get; private set; }
        public ClassLevel(string code, string description,int? sortOrder )
            : base(code, description)
        {
            this.SortOrder = sortOrder;
        }
    }
}
