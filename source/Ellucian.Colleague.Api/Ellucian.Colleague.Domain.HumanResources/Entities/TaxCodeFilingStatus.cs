/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class TaxCodeFilingStatus : CodeItem
    {
        public TaxCodeFilingStatus(string code, string description)
            : base(code, description)
        {

        }

    }
}
