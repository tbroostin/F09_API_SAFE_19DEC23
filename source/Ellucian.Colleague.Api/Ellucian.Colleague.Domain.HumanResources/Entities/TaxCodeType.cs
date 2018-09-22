/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    public enum TaxCodeType
    {
        FicaWithholding,
        FederalWithholding,
        EarnedIncomeCredit,
        StateWithholding,
        FederalUnemploymentTax,
        UnemployementAndInsurance,
        CityWithholding,
        CountyWithholding,
        SchoolDistrictWithholding,
        LocalWithholding,
        CanadianPensionPlan,
        CanadianUnemploymentInsurance,
        CanadianFederalIncomeTax,
        CanadianProvincialTax,
        WorkmansCompensation
    }
}
