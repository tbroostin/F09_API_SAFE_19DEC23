// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ApplicationInfluence : CodeItem
    {
        public ApplicationInfluence(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}