/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestOutsideAwardsRepository : IOutsideAwardsRepository
    {
        public class OutsideAwardRecord {
            public string recordId;
            public string studentId;
            public string awardName;
            public string awardType;
            public string awardYear;
            public string fundingSource;
            public decimal awardAmount;
        }

        public OutsideAwardRecord updateOutsideAwardRecord = new OutsideAwardRecord()
        {
            recordId = "1",
            studentId = "0003914",
            awardName = "outside scholarship",
            awardType = "scholarship",
            awardYear = "2016",
            fundingSource = "girl scouts",
            awardAmount = 1500
        };

        public List<OutsideAwardRecord> outsideAwardRecords = new List<OutsideAwardRecord>(){
            new OutsideAwardRecord(){
                recordId = "1",
                studentId = "0003914",
                awardName = "outside scholarship",
                awardType = "scholarship",
                awardYear = "2016",
                fundingSource = "girl scouts",
                awardAmount = 2000
            },
            new OutsideAwardRecord(){
                recordId = "4",
                studentId = "0003914",
                awardName = "outside grant",
                awardType = "grant",
                awardYear = "2016",
                fundingSource = "eagle bank",
                awardAmount = 500
            },
            new OutsideAwardRecord(){
                recordId = "76",
                studentId = "0003914",
                awardName = "outside loan",
                awardType = "loan",
                awardYear = "2016",
                fundingSource = "sallie mae",
                awardAmount = 5467.65m
            },
            new OutsideAwardRecord(){
                recordId = "86",
                studentId = "0003914",
                awardName = "2017 loan",
                awardType = "loan",
                awardYear = "2017",
                fundingSource = "sallie mae",
                awardAmount = 345.00m
            },
            new OutsideAwardRecord(){
                recordId = "28",
                studentId = "0003914",
                awardName = "2017 scholarship",
                awardType = "scholarship",
                awardYear = "2017",
                fundingSource = "bank of america",
                awardAmount = 2500.00m
            }
        };

        public Task<OutsideAward> CreateOutsideAwardAsync(OutsideAward outsideAward)
        {
            return Task.FromResult(new OutsideAward(outsideAward.Id, outsideAward.StudentId, 
                outsideAward.AwardYearCode, outsideAward.AwardName, outsideAward.AwardType, 
                outsideAward.AwardAmount, outsideAward.AwardFundingSource));
        }

        public Task<IEnumerable<OutsideAward>> GetOutsideAwardsAsync(string studentId, string awardYearCode)
        {
            List<OutsideAward> entities = new List<OutsideAward>();
            var outsideAwards = outsideAwardRecords.Where(a => a.awardYear == awardYearCode);
            foreach (var award in outsideAwards)
            {
                entities.Add(new OutsideAward(award.recordId, award.studentId, award.awardYear, award.awardName,
                    award.awardType, award.awardAmount, award.fundingSource));
            }
            return Task.FromResult(entities.AsEnumerable());
        }

        public Task DeleteOutsideAwardAsync(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<OutsideAward> UpdateOutsideAwardAsync(OutsideAward outsideAward)
        {
           // throw new NotImplementedException();

            // find the recordId I want to update and then echo back the updated record

            return Task.FromResult(new OutsideAward(outsideAward.Id, outsideAward.StudentId,
                outsideAward.AwardYearCode, outsideAward.AwardName, outsideAward.AwardType,
                outsideAward.AwardAmount, outsideAward.AwardFundingSource));

            //Echo back what was sent to me
            //return Task.FromResult(outsideAward);
        }
    }
}
