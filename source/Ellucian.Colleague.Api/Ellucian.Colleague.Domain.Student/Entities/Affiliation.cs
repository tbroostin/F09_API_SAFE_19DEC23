// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Represents all Campus Orgs as Affiliations
    /// </summary>
    [Serializable]
    public class Affiliation :CodeItem
    {
        public Affiliation(string code, string description)
            : base(code, description)
           
        {
            
        }
     
    }

}

