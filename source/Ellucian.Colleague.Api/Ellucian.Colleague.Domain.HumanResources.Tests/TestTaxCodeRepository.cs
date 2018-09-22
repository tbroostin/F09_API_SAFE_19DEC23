/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestTaxCodeRepository
    {
        public class TaxCodeRecord
        {
            public string code;
            public string description;
            public string type;
            public string filingStatus;
        }

        public class TaxCodeFilingStatusRecord
        {
            public string code;
            public string description;
        }


        public List<TaxCodeRecord> taxCodeRecords = new List<TaxCodeRecord>()
        {
            new TaxCodeRecord()
            {
                code = "MEDI",
                description = "Medicare Tax",
                type = "FWHT",
                filingStatus = "FHH"
            },
            new TaxCodeRecord()
            {
                code = "FICA",
                description = "Social Security Tax",
                type = "FICA",
                filingStatus = "SNG"
            },
            new TaxCodeRecord()
            {
                code = "CB",
                description = "City of Brownell Tax",
                type = "CITY",
                filingStatus = "SNG"
            }
        };

        public List<TaxCodeFilingStatusRecord> filingStatusRecords = new List<TaxCodeFilingStatusRecord>()
        {
            new TaxCodeFilingStatusRecord()
            {
                code = "FHH",
                description = "Federal Head of Household",
            },
            new TaxCodeFilingStatusRecord()
            {
                code = "SNG",
                description = "Single"
            }
        };

        public IEnumerable<TaxCode> GetTaxCodes()
        {
            var taxCodes = taxCodeRecords.Select(t => new TaxCode(t.code, t.description, ConvertInternalCode(t.type))
            {
                FilingStatus = GetTaxCodeFilingStatuses().FirstOrDefault(fs => fs.Code == t.filingStatus)
            });
            return taxCodes;
        }

        public IEnumerable<TaxCodeFilingStatus> GetTaxCodeFilingStatuses()
        {
            var filingStatuses = filingStatusRecords.Select(fs => new TaxCodeFilingStatus(fs.code, fs.description));
            return filingStatuses;
        }

        public async Task<IEnumerable<TaxCode>> GetTaxCodesAsync()
        {
            return await Task.FromResult(GetTaxCodes());
        }


        public TaxCodeType ConvertInternalCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return TaxCodeType.FicaWithholding;
            }

            switch (code.ToUpperInvariant())
            {
                case "FICA":
                    return TaxCodeType.FicaWithholding;
                case "FWHT":
                    return TaxCodeType.FederalWithholding;
                case "EIC":
                    return TaxCodeType.EarnedIncomeCredit;
                case "STATE":
                    return TaxCodeType.StateWithholding;
                case "FUTA":
                    return TaxCodeType.FederalUnemploymentTax;
                case "INSURANCE":
                    return TaxCodeType.UnemployementAndInsurance;
                case "CITY":
                    return TaxCodeType.CityWithholding;
                case "COUNTY":
                    return TaxCodeType.CountyWithholding;
                case "SDIST":
                    return TaxCodeType.SchoolDistrictWithholding;
                case "LOCAL":
                    return TaxCodeType.LocalWithholding;
                case "CPP":
                    return TaxCodeType.CanadianPensionPlan;
                case "UI":
                    return TaxCodeType.CanadianUnemploymentInsurance;
                case "CINC":
                    return TaxCodeType.CanadianFederalIncomeTax;
                case "PROV":
                    return TaxCodeType.CanadianProvincialTax;
                case "WKCOMP":
                    return TaxCodeType.WorkmansCompensation;
                default:
                    return TaxCodeType.FicaWithholding;
            }
        }
    }
}
