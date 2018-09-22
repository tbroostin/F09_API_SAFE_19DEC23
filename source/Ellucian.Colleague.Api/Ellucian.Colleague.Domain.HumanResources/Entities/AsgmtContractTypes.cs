// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Assignment Contract types for type modification on faculty contracts
    /// <param name="code">Code representing the ID of Assignment Contract Type abbreviation</param>
    /// <param name="description">Spelled out type</param>
    /// <param name="academicDisciplineType">A type of Academic Discipline (Major, Minor, Concentration)</param>
    /// </summary>
    [Serializable]
    public class AsgmtContractTypes : CodeItem
    {
        public AsgmtContractTypes(string code, string description)
            : base(code, description)
        {
            
        }
    }
}
