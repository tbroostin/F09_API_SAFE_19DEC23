/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPersonStipendRepository : IPersonStipendRepository
    {
        public class PersonStipendRecord
        {
            public string id;
            public string personId;
            public string positionId;
            public DateTime? startDate;
            public DateTime? endDate;
            public string description;
            public string baseAmount;
            public string payrollDesignation;
            public int? numberOfPayments;
            public int? numberOfPaymentsTaken;
            public List<string> courseSectionAssignments;
            public List<string> advisorAssignments;
            public List<string> membershipAssignment;
        }

        public List<PersonStipendRecord> personStipendRecords = new List<PersonStipendRecord>()
        {
            new PersonStipendRecord()
            {
                id = "4931",
                personId = "0016390",
                positionId = "MANAGER",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                description = "Restricted Stipend",
                baseAmount = "10000",
                payrollDesignation = "R",
                numberOfPayments = 3,
                numberOfPaymentsTaken = 2,
                courseSectionAssignments = new List<string>()
                {
                    "23560",
                    "18905"
                },
                advisorAssignments = new List<string>()
                {
                    "FTBPL",
                    "BIOS"
                },
                membershipAssignment = new List<string>()
                {
                    "CYC"
                }
            },
            new PersonStipendRecord()
            {
                id = "4932",
                personId = "0010491",
                positionId = "ADJ",
                startDate = new DateTime(2010, 1, 1),
                endDate = new DateTime(2010, 12, 31),
                description = "Ongoing Stipend",
                baseAmount = "10000",
                payrollDesignation = "O",
                advisorAssignments = new List<string>()
                {
                    "BIOS"
                },
            },
            new PersonStipendRecord()
            {
                id = "4933",
                personId = "0004363",
                positionId = "PROF",
                startDate = new DateTime(2014, 5, 6),
                endDate = null,
                description = "Restricted Stipend 2",
                baseAmount = "3444",
                payrollDesignation = "R",
                numberOfPayments = 2,
                numberOfPaymentsTaken = 1,
                courseSectionAssignments = new List<string>()
                {
                    "10063",
                    "10068"
                },
                advisorAssignments = new List<string>()
                {
                    "ART",
                    "BSB"
                },
            },
            new PersonStipendRecord()
            {
                id = "4934",
                personId = "0010994",
                positionId = "ASST",
                startDate = new DateTime(2019, 1, 1),
                endDate = null,
                description = "Ongoing ASST Stipend",
                baseAmount = "10000",
                payrollDesignation = "O",
                courseSectionAssignments = new List<string>()
                {
                    "10125",
                    "10126"
                },
                advisorAssignments = new List<string>()
                {
                    "COE",
                    "FACL"
                },
                membershipAssignment = new List<string>()
                {
                    "CYC"
                }
            },
            new PersonStipendRecord()
            {
                id = "4935",
                personId = "0011124",
                positionId = "INSTRUCTOR",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                description = "Restricted INST Stipend",
                baseAmount = "10000",
                payrollDesignation = "R",
                numberOfPayments = 6,
                numberOfPaymentsTaken = 5,
                advisorAssignments = new List<string>()
                {
                    "PGD",
                    "BIOS"
                },
                membershipAssignment = new List<string>()
                {
                    "FUT"
                }
            },
            new PersonStipendRecord()
            {
                id = "4936",
                personId = "0003054",
                positionId = "LIBRARIAN",
                startDate = new DateTime(2010, 1, 1),
                endDate = null,
                description = "Restricted LIB Stipend",
                baseAmount = "34564",
                payrollDesignation = "R",
                numberOfPayments = 3,
                numberOfPaymentsTaken = 2,
                courseSectionAssignments = new List<string>()
                {
                    "10260",
                    "10278"
                },
                advisorAssignments = new List<string>()
                {
                    "SJC"
                },
                membershipAssignment = new List<string>()
                {
                    "TEN"
                }
            }

        };

        public IEnumerable<string> PersonIdsUsedInTestData
        {
            get
            {
                return personStipendRecords.Select(pst => pst.personId).Distinct();
            }
        }

        public async Task<IEnumerable<PersonStipend>> GetPersonStipendAsync(IEnumerable<string> personIds)
        {
            var peronStipends = personStipendRecords
                .Where(r => personIds.Contains(r.personId))
                .Select(r =>
                {
                    try
                    {
                        return BuildPersonStipend(r);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                })
                .Where(e => e != null);

            return await Task.FromResult(peronStipends);
        }

        public PersonStipend BuildPersonStipend(PersonStipendRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return new PersonStipend(record.id, record.personId, record.positionId, record.startDate.Value, record.endDate, record.description,
                record.baseAmount, record.payrollDesignation, record.numberOfPayments, record.numberOfPaymentsTaken, record.courseSectionAssignments,
                record.advisorAssignments, record.membershipAssignment);
        }
    }
}
