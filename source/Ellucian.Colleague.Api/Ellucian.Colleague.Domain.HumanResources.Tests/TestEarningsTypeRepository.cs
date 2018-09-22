/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEarningsTypeRepository : IEarningsTypeRepository
    {
        public class EarningsTypeRecord
        {
            public string id;
            public string description;
            public string activeFlag = "A";
            public string category;
            public string method = null;
            public decimal? factor;

        }

        public List<EarningsTypeRecord> earningsTypeRecords = new List<EarningsTypeRecord>()
        {
            new EarningsTypeRecord()
            {
                id = "REG",
                description = "Regular Pay",
                category = "R",
                
            },
            new EarningsTypeRecord()
            {
                id = "OVT",
                description = "Overtime @ 1.5x",
                category = "O",
                factor = 1.5m
            },
            new EarningsTypeRecord()
            {
                id = "VAC",
                description = "Paid Vacation",
                category = "L"
            },
            new EarningsTypeRecord()
            {
                id = "CE15",
                description = "Comp Time Earned @ 1.5x",
                category = "L",
                method = "A",
                factor = 1.5m
            },
            new EarningsTypeRecord()
            {
                id = "CTIM",
                description = "Comp Time Taken",
                category = "L",
                method = "P"
            },
            new EarningsTypeRecord()
            {
                id = "LWOP",
                description = "Leave Without Pay",
                category = "L",
                method = "N"
            },
            new EarningsTypeRecord()
            {
                id = "CWS",
                description = "College Work Study",
                category = "C"
            },
            new EarningsTypeRecord()
            {
                id = "BON",
                description = "Bonus Pay",
                category = "M"
            },
            new EarningsTypeRecord()
            {
                id = "OLDE",
                description = "Inactive Leave EarningsType",
                activeFlag = null,
                category = "L",
                method = "A"
            }
        };

        public IEnumerable<EarningsType> GetEarningsTypes()
        {
            var earningsTypeEntities = new List<EarningsType>();
            if (earningsTypeRecords == null)
            {
                return earningsTypeEntities;
            }
            foreach (var earningsTypeRecord in earningsTypeRecords)
            {
                try
                {
                    earningsTypeEntities.Add(BuildEarningsType(earningsTypeRecord));
                }
                catch (Exception)
                {

                }
            }
            return earningsTypeEntities;
        }

        public async Task<IEnumerable<EarningsType>> GetEarningsTypesAsync()
        {
            return await Task.FromResult(GetEarningsTypes());
        }

        public EarningsType BuildEarningsType(EarningsTypeRecord earningsTypeRecord)
        {
            var isActive = earningsTypeRecord.activeFlag == null ? false : earningsTypeRecord.activeFlag.Equals("A", StringComparison.InvariantCultureIgnoreCase);
            return new EarningsType(earningsTypeRecord.id, earningsTypeRecord.description, isActive, ConvertRecordColumnToEarningsCategory(earningsTypeRecord.category), ConvertRecordColumnToEarningsMethod(earningsTypeRecord.method), earningsTypeRecord.factor)
            { };
        }

        private EarningsCategory ConvertRecordColumnToEarningsCategory(string earningsCategoryCode)
        {
            if (string.IsNullOrEmpty(earningsCategoryCode))
            {
                throw new ArgumentNullException("earningsCategoryCode", "Cannot convert null or empty earningsCategoryCode");
            }

            switch (earningsCategoryCode.ToUpperInvariant())
            {
                case "R":
                    return EarningsCategory.Regular;
                case "O":
                    return EarningsCategory.Overtime;
                case "L":
                    return EarningsCategory.Leave;
                case "C":
                    return EarningsCategory.CollegeWorkStudy;
                case "M":
                    return EarningsCategory.Miscellaneous;
                default:
                    throw new ApplicationException("Unknown earningsTypeCategory " + earningsCategoryCode);
            }

        }

        private EarningsMethod ConvertRecordColumnToEarningsMethod(string earningsMethodCode)
        {
            if (string.IsNullOrEmpty(earningsMethodCode))
            {
                return EarningsMethod.None;
            }
          
            switch (earningsMethodCode.ToUpperInvariant())
            {
                case "A":
                    return EarningsMethod.Accrued;
                case "P":
                    return EarningsMethod.Taken;
                case "N":
                    return EarningsMethod.NoPay;
                default:
                throw new ApplicationException("Unknown earningsMethodCode " + earningsMethodCode);
            }
        }
    }
}
