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
    public class TestPositionRepository : IPositionRepository
    {

        public class PositionRecord
        {
            public string id;
            public string title;
            public string shortTitle;
            public string positionDept;
            public string positionLocation;
            public DateTime? endDate;
            public string hourlyOrSalary;
            public string exemptOrNot;
            public List<string> positionPayIds;
            public DateTime? startDate;
            public string supervisorPositionId;
            public string alternateSupervisorPositionId;
            public string timeEntryForm;
        }

        public List<PositionRecord> positionRecords = new List<PositionRecord>()
        {
            new PositionRecord() 
            {
                id = "ZFIDI62100BUMA",
                title = "Manager of Budgetary Needs",
                shortTitle = "Budget Manager",
                positionDept = "FIDI",
                positionLocation = "MC",
                endDate = null,
                hourlyOrSalary = "s",
                exemptOrNot = "e",
                positionPayIds = new List<string>() {"149", "115"},
                startDate = new DateTime(2001, 1,1),
                supervisorPositionId = "ZFIDI62100FIDI",
                alternateSupervisorPositionId = "",
                timeEntryForm = "SUMMARY"
            },
            new PositionRecord() 
            {
                id = "ZECED20201ADFA",
                title = "Adjunct Faculty, Early Childhood Education",
                shortTitle = "Adj Fac - Early Child Ed",
                positionDept = "ECED",
                positionLocation = "MC",
                endDate = DateTime.Today.AddYears(1), // position with an end date, ensuring that its always "active"
                hourlyOrSalary = "h",
                exemptOrNot = "n",
                positionPayIds = new List<string>() {"63"},
                startDate = new DateTime(2010, 12, 1),
                supervisorPositionId = "",
                alternateSupervisorPositionId = "",
                timeEntryForm = "DETAIL"
            }
        };



        public IEnumerable<Position> GetPositions()
        {
            var positionEntities = new List<Position>();
            if (positionRecords == null)
            {
                return positionEntities;
            }
            foreach (var positionRecord in positionRecords)
            {
                try
                {
                    positionEntities.Add(BuildPosition(positionRecord));
                }
                catch(Exception)
                {

                }
            }
            return positionEntities;
        }

        public async Task<IEnumerable<Position>> GetPositionsAsync()
        {
            return await Task.FromResult(GetPositions());
        }

        public Task<Position> GetPositionByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Position>, int>> GetPositionsAsync(int offset, int limit, string code = "", string campus = "", string status = "", string bargainingUnit = "",
            List<string> reportsToPosition = null, string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPositionGuidFromIdAsync(string positionId)
        {
            throw new NotImplementedException();
        }

        public Position BuildPosition(PositionRecord positionRecord)
        {
            var isSalary = positionRecord.hourlyOrSalary.Equals("S", StringComparison.InvariantCultureIgnoreCase);
            return new Position(positionRecord.id, positionRecord.title, positionRecord.shortTitle, positionRecord.positionDept, positionRecord.startDate.Value, isSalary)
            {
                EndDate = positionRecord.endDate,
                IsExempt = positionRecord.exemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase),
                SupervisorPositionId = positionRecord.supervisorPositionId,
                AlternateSupervisorPositionId = positionRecord.alternateSupervisorPositionId,
                PositionPayScheduleIds = positionRecord.positionPayIds,
                PositionLocation = positionRecord.positionLocation
            };
            
        }


        public Task<PositionPay> GetPositionPayByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PositionPay>> GetPositionPayByIdsAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPositionIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }
    }
}
