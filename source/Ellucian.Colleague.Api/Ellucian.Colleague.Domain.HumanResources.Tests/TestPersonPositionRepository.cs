/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPersonPositionRepository : IPersonPositionRepository
    {
        public class PersonPositionRecord
        {
            public string id;
            public string personId;
            public string positionId;
            public DateTime? startDate;
            public DateTime? endDate;
            public string supervisorId;
            public string alternateSupervisorId;
            public Decimal? fullTimeEquivalent;
        }

        public List<PersonPositionRecord> personPositionRecords = new List<PersonPositionRecord>()
        {
            new PersonPositionRecord()
            {
                id = "123",
                personId = "0003914",
                positionId = "ZBASEDF1234",
                startDate = new DateTime(2010, 1, 1),
                endDate = new DateTime(2014, 2, 28),
                supervisorId = "0001928",
                alternateSupervisorId = "1230203",
                fullTimeEquivalent = 0.5m
            },
            new PersonPositionRecord() 
            {
                id = "111",
                personId = "0003914",
                positionId = "ASDLFJA23232",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                supervisorId = "0001929",
                alternateSupervisorId = "1230204",
                fullTimeEquivalent = 0.52m
            },
            new PersonPositionRecord()
            {
                id = "321",
                personId = "0003915",
                positionId = "FASDFS2323",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                supervisorId = "0003939",
                alternateSupervisorId = "8383838",
                fullTimeEquivalent = 0.5m
            },
            new PersonPositionRecord()
            {
                id = "222",
                personId = "0003915",
                positionId = "PPPPPPPQWER23232323",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                supervisorId = "0003939",
                alternateSupervisorId = "8383838",
                fullTimeEquivalent = 0.1m
            },
            new PersonPositionRecord()
            {
                id = "223",
                personId = "0003916",
                positionId = "ZPRINT101ASST",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                supervisorId = "00019739",
                alternateSupervisorId = "00011625",
                fullTimeEquivalent = 0.2m
            }
        };

        /// <summary>
        /// Helper property returns personids used in the test data
        /// </summary>
        public IEnumerable<string> personIdsUsedInTestData
        {
            get
            {
                return personPositionRecords.Select(rec => rec.personId).Distinct();
            }
        }

        public async Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(IEnumerable<string> personIds)
        {
            var records = personPositionRecords.Where(p => personIds.Contains(p.personId));

            var entities = new List<PersonPosition>();
            foreach (var record in records)
            {
                if (record != null)
                {
                    try
                    {
                        entities.Add(BuildPersonPosition(record));
                    }
                    catch (Exception) { }
                }
            }

            return await Task.FromResult(entities);
        }

        public PersonPosition BuildPersonPosition(PersonPositionRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (!record.startDate.HasValue)
            {
                throw new InvalidOperationException("startDate must have value");
            }
            return new PersonPosition(record.id, record.personId, record.positionId, record.startDate.Value, record.fullTimeEquivalent)
            {
                EndDate = record.endDate,
                SupervisorId = record.supervisorId,
                AlternateSupervisorId = record.alternateSupervisorId,
                FullTimeEquivalent = record.fullTimeEquivalent                
            };
        }
    }
}
