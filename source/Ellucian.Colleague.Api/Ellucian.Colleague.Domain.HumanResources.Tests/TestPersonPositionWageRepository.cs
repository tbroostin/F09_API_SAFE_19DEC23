/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPersonPositionWageRepository : IPersonPositionWageRepository
    {
        public class PersonPositionWageRecord
        {
            public string id;
            public string personId;
            public string positionId;
            public string personPositionId;
            public string payClassId;
            public string payCycleId;
            public string PpwgPospayId;
            public DateTime? startDate;
            public DateTime? endDate;
            public string regularWorkEarningsType;
            public string paySuspendedFlag;
            public List<PayItemRecord> payItems;
            public string earnTypeGroupingId;
            public List<string> earnTypeGroupings;
            public string wageType;
        }

        public class PayItemRecord
        {
            public string fundSourceId;
            public string projectId;
        }

        public List<Pospay> positionPayRecords = new List<Pospay>()
        {
            new Pospay()
            {
                Recordkey = "555",
                PospayEarntypeGrouping = "ADMIN"
            },
            new Pospay()
            {
                Recordkey = "143",
                PospayEarntypeGrouping = "ADMIN"
            },
            new Pospay()
            {
                Recordkey = "123",
                PospayEarntypeGrouping = "NOTADMIN"
            }
        };
        //public List<EarntypeGroupings> earnTypeGroupingsRecords = new List<EarntypeGroupings>()
        //{
        //    new EarntypeGroupings()
        //    {
        //        Recordkey = "ADMIN",
        //        EtpgEarntype = new List<string>()
        //        {
        //            "HRLY"
        //        },
        //        EtpgEarntypeDesc = new List<string>()
        //        {
        //            "This is hourly"
        //        }
        //    },
        //    new EarntypeGroupings()
        //    {
        //        Recordkey = "NOTADMING",
        //        EtpgEarntype = new List<string>()
        //        {
        //            "NTHRLY"
        //        },
        //        EtpgEarntypeDesc = new List<string>()
        //        {
        //            "NOT HOURLY "
        //        }
        //    }
        //};
        public List<PersonPositionWageRecord> personPositionWageRecords = new List<PersonPositionWageRecord>() 
        {
            new PersonPositionWageRecord() {
                id = "5",
                personId = "0003914",
                positionId = "MUSICPROF12345",
                personPositionId = "12345",
                payClassId = "MC",
                payCycleId = "BM",
                PpwgPospayId = "555",
                startDate = new DateTime(2010, 1, 1),
                endDate = new DateTime(2015, 12, 31),
                regularWorkEarningsType = "REG",        
                paySuspendedFlag = "N",
                payItems = new List<PayItemRecord>()
                {
                    new PayItemRecord() {fundSourceId = "COMP"},
                    new PayItemRecord() {fundSourceId = "COMP"}
                },
                wageType = "W"
            },
            new PersonPositionWageRecord() {
                id = "6",
                personId = "0003914",
                positionId = "MUSICPROF12345",
                personPositionId = "45432",
                payClassId = "MC",
                payCycleId = "BM",
                PpwgPospayId = "555",
                startDate = new DateTime(2016, 1, 1),
                endDate = null,
                regularWorkEarningsType = "REG",  
                paySuspendedFlag = "N",
                payItems = new List<PayItemRecord>()
                {
                    new PayItemRecord() {fundSourceId = "COMP"},
                    new PayItemRecord() {fundSourceId = "PROJ", projectId = "11"}
                },
                wageType = "W"
            },

            new PersonPositionWageRecord() {
                id = "7",
                personId = "0004409",
                positionId = "COMPSCIASSOC",
                personPositionId = "44432",
                payClassId = "CK",
                payCycleId = "MW",
                PpwgPospayId = "143",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                regularWorkEarningsType = "REG",       
                paySuspendedFlag = "Y",
                payItems = new List<PayItemRecord>()
                {
                    new PayItemRecord() {fundSourceId = "COMP"},
                    new PayItemRecord() {fundSourceId = "COMP"}
                },
                wageType = "W"
            },

            new PersonPositionWageRecord() {
                id = "8",
                personId = "0004409",
                positionId = "LABTECH12343",
                personPositionId = "67676",
                payClassId = "CK",
                payCycleId = "MW",
                PpwgPospayId = "143",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                regularWorkEarningsType = "REG",                
                payItems = new List<PayItemRecord>()
                {
                    new PayItemRecord() {fundSourceId = "COMP"},
                    new PayItemRecord() {fundSourceId = "COMP"}
                },
                wageType = "W"
            },
             new PersonPositionWageRecord() {
                id = "9",
                personId = "0003916",
                positionId = "PRINT SHOP ASSISTANT",
                personPositionId = "67676",
                payClassId = "CK",
                payCycleId = "MW",
                PpwgPospayId = "143",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                regularWorkEarningsType = "REG",
                payItems = new List<PayItemRecord>()
                {
                    new PayItemRecord() {fundSourceId = "COMP"},
                    new PayItemRecord() {fundSourceId = "COMP"}
                },
                wageType = "W"
            },
        };

        public IEnumerable<string> PersonIdsUsedInTestData
        {
            get
            {
                return personPositionWageRecords.Select(ppw => ppw.personId).Distinct();
            }
        }

        //public List<EarningsTypeGroup> BuildEarnTypeGrouping(EarntypeGroupings earnTypeGroupingsRecords)
        //{
        //    if (earnTypeGroupingsRecords == null)
        //    {
        //        throw new ArgumentNullException("earntypegroupingsrecord");
        //    }
        //    var listOfEntities = new List<EarningsTypeGroup>();
        //    for (var i = 0; i < earnTypeGroupingsRecords.EtpgEarntypeDesc.Count; i++)
        //    {
        //        listOfEntities.Add(new EarningsTypeGroup(earnTypeGroupingsRecords.Recordkey, earnTypeGroupingsRecords.EtpgEarntype.ElementAt(i), earnTypeGroupingsRecords.EtpgEarntypeDesc.ElementAt(i)));
        //    }
        //    return listOfEntities;
            
        //}

        public async Task<IEnumerable<PersonPositionWage>> GetPersonPositionWagesAsync(IEnumerable<string> personIds, DateTime? startDate = null)
        {
            var earnTypeGroupingEntities = new List<EarningsTypeGroup>();
            //foreach (var earnTypeGroupingRecord in earnTypeGroupingsRecords)
            //{
            //    if (earnTypeGroupingRecord != null)
            //    {
            //        try
            //        {
            //            earnTypeGroupingEntities.AddRange(BuildEarnTypeGrouping(earnTypeGroupingRecord));
            //        }
            //        catch (Exception e)
            //        {
            //            return null;
            //        }
            //    }
            //}
            var personPositionWages = personPositionWageRecords
                    .Where(r => personIds.Contains(r.personId))
                    .Select(r =>
                    {
                        try
                        {
                            return BuildPersonPositionWage(r, positionPayRecords);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    })
                    .Where(e => e != null);

            return await Task.FromResult(personPositionWages);
        }

        public PersonPositionWage BuildPersonPositionWage(PersonPositionWageRecord record, List<Pospay> posPayRecords)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            var earningsTypeGroupId = posPayRecords.Where(rec => record.PpwgPospayId == rec.Recordkey).Select(rec => rec.PospayEarntypeGrouping).First().ToString();
            //var earningsTypeGroupEntries = earnTypeGroupingsEntities.Where(entry => entry.EarningsTypeGroupId == earningsTypeGroupId).ToList();
            bool isRegularWage = !string.IsNullOrEmpty(record.wageType) && record.wageType.Equals("W", StringComparison.InvariantCultureIgnoreCase);
            return new PersonPositionWage(record.id, record.personId, record.positionId, record.personPositionId, record.PpwgPospayId, record.payClassId, record.payCycleId,
                record.regularWorkEarningsType, record.startDate.Value, earningsTypeGroupId)
            {
                EndDate = record.endDate,
                IsPaySuspended = !string.IsNullOrEmpty(record.paySuspendedFlag) && record.paySuspendedFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                FundingSources = record.payItems
                    .Select(item => new PositionFundingSource(item.fundSourceId, record.payItems.IndexOf(item)) { ProjectId = item.projectId }).ToList()
            };
        }        
    }
}
