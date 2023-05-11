/* Copyright 2017-2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestBenefitDeductionTypeRepository
    {
        public class BendedRecord
        {
            public string code;
            public string description;
            public string selfServiceDescription;
            public string type;
        }

        public class BendedTypeValcode
        {
            public string code;
            public string description;
            public string specialCode2;
        }

        public List<BendedRecord> bendedRecords = new List<BendedRecord>()
        {
            new BendedRecord()
            {
                code = "DED",
                description = "401k plan",
                selfServiceDescription = "401kelvin",
                type = "RET",
            },
            new BendedRecord()
            {
                code = "BEN",
                description = "Dental Family Plan",
                selfServiceDescription ="",
                type = "DENT"
            },
            new BendedRecord()
            {
                code = "401k",
                description = "401k plan b",
                type = "RET"
            },
            new BendedRecord()
            {
                code = "TRPF",
                description = "Teacher's retirement plan",
                type = "RET",
            },
            new BendedRecord()
            {
                code = "DENT",
                description = "Dental Individual",
                type = "DENT"
            },
            new BendedRecord()
            {
                code = "BNUS",
                description = "Bonus Benefit",
                type = "SB"
            }
        };

        public List<BendedTypeValcode> bendedTypes = new List<BendedTypeValcode>()
        {
            new BendedTypeValcode()
            {
                code = "RET",
                description = "retirement plans",
                specialCode2 = "D"
            },
            new BendedTypeValcode()
            {
                code = "DENT",
                description = "dental plans",
                specialCode2 = "B",
            },
            new BendedTypeValcode()
            {
                code = "SB",
                description = "special benefits",
                specialCode2 = "B"
            }

        };

        public async Task<IEnumerable<BenefitDeductionType>> GetBenefitDeductionTypesAsync()
        {
            return await Task.FromResult(GetBenefitDeductionTypes());
        }

        public IEnumerable<BenefitDeductionType> GetBenefitDeductionTypes()
        {
            var benefitDeductions = bendedRecords == null ? null : bendedRecords.Select(bd =>
                new BenefitDeductionType(bd.code, bd.description, bd.selfServiceDescription, TranslateCode(bd.type)));
            return benefitDeductions;
        }


        public BenefitDeductionTypeCategory TranslateCode(string code)
        {
            var valcode = bendedTypes.FirstOrDefault(t => t.code == code);
            
            switch(valcode.specialCode2.ToUpper())
            {
                case "B":
                    return BenefitDeductionTypeCategory.Benefit;
                case "D":
                    return BenefitDeductionTypeCategory.Deduction;
                default:
                    throw new ColleagueWebApiException("unknown institution type code");
            }            
        }
    }
}
