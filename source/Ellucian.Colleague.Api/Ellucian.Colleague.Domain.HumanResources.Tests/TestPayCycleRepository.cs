/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.DataContracts;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPayCycleRepository : IPayCycleRepository
    {        
        public class PayCycleRecord
        {
            public string id;
            public string description;
            public string payFrequency;
            public string workWeekStartDay;
            public DayOfWeek startDay;
            public List<string> payClassIds;
            public List<PayPeriodRecord> payPeriods;
            public List<string> benDedPeriods;
            public List<DateTime?> startDate;
            public List<DateTime?> endDate;
            public List<string> excludeEarnTypes;
            public List<DateTime?> paycheckDate;
            public List<string> periodStatus;
            public List<string> takeBenefits;
        }

        public class PayPeriodRecord
        {
            public DateTime? startDate;
            public DateTime? endDate;
        }

        public class PayControlRecord
        {
            public string Recordkey;
            public string LastProgram;
            public List<string> CurrentProgram;
            public DateTime? PeriodStartDate;
            public string TimeHistoryUpdated;
        }

        //public IEnumerable<PayCycleFrequency> cachedFrequencies = new List<PayCycleFrequency>()
        //{
        //    new PayCycleFrequency("TIME.FREQUENCIES","DA",365),
        //    new PayCycleFrequency("TIME.FREQUENCIES","WK",52),
        //    new PayCycleFrequency("TIME.FREQUENCIES","MO",12),
        //    new PayCycleFrequency("TIME.FREQUENCIES","QT",4),
        //    new PayCycleFrequency("TIME.FREQUENCIES","SA",2),
        //    new PayCycleFrequency("TIME.FREQUENCIES","AN",1),
        //    new PayCycleFrequency("TIME.FREQUENCIES","M9",9),
        //    new PayCycleFrequency("TIME.FREQUENCIES","BM",6),
        //    new PayCycleFrequency("TIME.FREQUENCIES","SM",24),
        //};

        public ApplValcodes validationCodeRecords 
        {
            get
            {
                return this.buildValidationCodeRecords();
            }
        }

        private ApplValcodes buildValidationCodeRecords()
        {
            ApplValcodes timeFrequencies = new ApplValcodes();
            timeFrequencies.Recordkey = "TIME.FREQUENCIES";
            timeFrequencies.ValsEntityAssociation = new List<ApplValcodesVals>()
            {
                new ApplValcodesVals("", "Daily", "365", "DA","","",""),
                new ApplValcodesVals("", "Weekly", "52", "WK","","",""),
                new ApplValcodesVals("", "Bi-Weekly", "26", "BW","","",""),
                new ApplValcodesVals("", "Monthly", "12", "MO","","",""),
                new ApplValcodesVals("", "Quarterly", "4", "QT","","",""),
                new ApplValcodesVals("", "Semiannual", "2", "SA","","",""),
                new ApplValcodesVals("", "Annual", "1", "AN","","",""),
                new ApplValcodesVals("", "9 Month", "9", "M9","","",""),
                new ApplValcodesVals("", "Bimonthly", "6", "BM","","",""),
                new ApplValcodesVals("", "Semimonthly", "24", "SM","","",""),
            };

            return timeFrequencies;
        }

        public List<PayCycleFrequency> payCycleFrequencies
        {
            get
            {
                return this.buildPayCycleFrequencies();
            }
        }


        private List<PayCycleFrequency> buildPayCycleFrequencies()
        {
            var frequencies = new List<PayCycleFrequency>();

            return validationCodeRecords.ValsEntityAssociation
                .Select(v => 
                    new PayCycleFrequency(v.ValInternalCodeAssocMember, 
                    v.ValExternalRepresentationAssocMember, 
                    Int32.Parse(v.ValActionCode1AssocMember))
                    ).ToList();

        }
       

        public List<PayControlRecord> payControlRecords = new List<PayControlRecord>()
        {
            new PayControlRecord()
            {
                Recordkey = "001*BW",
                LastProgram = "5",
                CurrentProgram = new List<string>(){"5"},
                PeriodStartDate = new DateTime(2015,12,25),
                TimeHistoryUpdated = "Y"
            },
            new PayControlRecord()
            {
                Recordkey = "002*SM",
                LastProgram = "5",
                CurrentProgram = new List<string>(){"5"},
                PeriodStartDate = new DateTime(2016,01,01),
                TimeHistoryUpdated = "Y"
            },
            new PayControlRecord()
            {
                Recordkey = "003*MO",
                LastProgram = "5",
                CurrentProgram = new List<string>(){"5"},
                PeriodStartDate = new DateTime(2016,01,09),
                TimeHistoryUpdated = "Y"
            }
        };

        
        public List<PayCycleRecord> payCycleRecords = new List<PayCycleRecord>()
        {
            new PayCycleRecord()
            {
                id = "BW",
                description = "Bi-weekly 26/year",
                payFrequency = "BW",
                workWeekStartDay = "SU",
                payClassIds = new List<string>() {"BWS", "BWH"},                
                payPeriods = new List<PayPeriodRecord>()
                {
                    new PayPeriodRecord() {startDate = new DateTime(2015,12,25), endDate = new DateTime(2016,01,08)},
                },
                benDedPeriods = new List<string>(),
                startDate = new List<DateTime?>(),
                endDate = new List<DateTime?>(),
                excludeEarnTypes = new List<string>(),
                paycheckDate = new List<DateTime?>(),
                periodStatus = new List<string>(),
                takeBenefits = new List<string>(),
            },
            new PayCycleRecord()
            {
                id = "SM",
                description = "Semi-monthly 24/year",
                payFrequency = "SM",
                workWeekStartDay = "M",
                payClassIds = new List<string>() {"SMS", "ADSM", "ADM"},
                payPeriods = new List<PayPeriodRecord>()
                {
                    new PayPeriodRecord() {startDate = new DateTime(2016,01,01), endDate = new DateTime(2016,01,15)},
                },                
                benDedPeriods = new List<string>(),
                startDate = new List<DateTime?>(),
                endDate = new List<DateTime?>(),
                excludeEarnTypes = new List<string>(),
                paycheckDate = new List<DateTime?>(),
                periodStatus = new List<string>(),
                takeBenefits = new List<string>(),
            },
            new PayCycleRecord()
            {
                id = "MO",
                description = "Monthly 12/year",
                payFrequency = "MO",
                workWeekStartDay = "M",
                payClassIds = new List<string> {"ADMO", "MNT"},
                payPeriods = new List<PayPeriodRecord>()
                {
                    new PayPeriodRecord() {startDate = new DateTime(2016,01,09), endDate = new DateTime(2016,01,15)},
                },
                benDedPeriods = new List<string>(),
                startDate = new List<DateTime?>(),
                endDate = new List<DateTime?>(),
                excludeEarnTypes = new List<string>(),
                paycheckDate = new List<DateTime?>(),
                periodStatus = new List<string>(),
                takeBenefits = new List<string>(),
            }
        };


        public async Task<IEnumerable<PayCycle>> GetPayCyclesAsync()
        {
            var payCycles = payCycleRecords
                .Select(pc =>
                {
                    try{
                        return BuildPayCycle(pc);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                })
                .Where(e => e != null);

            return await Task.FromResult(payCycles);
        }

        public PayCycle BuildPayCycle(PayCycleRecord record)
        {
 	        if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            int annualPayFrequency = -1;            
            if (payCycleFrequencies.Any(f => f.Code == record.payFrequency))
            {
                annualPayFrequency = payCycleFrequencies.FirstOrDefault(f => f.Code == record.payFrequency).AnnualPayFrequency;
            }
            return new PayCycle(record.id, record.description, record.startDay)
            {
                PayClassIds = record.payClassIds,
                PayPeriods = record.payPeriods.Select(dateRange => new PayPeriod(dateRange.startDate.Value, dateRange.endDate.Value)).ToList(),
                AnnualPayFrequency = annualPayFrequency,
                Description = record.description
            };
        }
    }
}
