//Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEmployeeLeavePlanRepository : IEmployeeLeavePlansRepository
    {
        public class EmployeeLeavePlanRecord
        {
            public string id;
            public string personId;
            public string leavePlanId;
            public DateTime? startDate;
            public DateTime? endDate;
            public DateTime? allowedDate;
            public decimal? balance;

        }

        public class LeaveTransactionRecord
        {
            public string id;
            public string personId;
            public string employeeLeavePlanId;
            public DateTime? date;
            public string leavePlanId;
            public decimal? hours;
            public string action;
            public decimal? forwardingBalance;
        }

        public List<EmployeeLeavePlanRecord> employeeLeavePlanRecords = new List<EmployeeLeavePlanRecord>()
        {
            new EmployeeLeavePlanRecord()
            {
                id = "20",
                personId = "0003914",
                leavePlanId = "VACH",
                startDate = new DateTime(2016, 1,1),
                endDate = null,
                allowedDate = new DateTime(2016, 2,1),
                balance = 100
            },
            new EmployeeLeavePlanRecord()
            {
                id = "21",
                personId = "0004888",
                leavePlanId = "SICH",
                startDate = new DateTime(2017, 1, 1),
                endDate = new DateTime(2018,1,1),
                allowedDate = null,
                balance = 0
            },
            new EmployeeLeavePlanRecord()
            {
                id = "22",
                personId = "0004888",
                leavePlanId = "NOYEARLYSTARTDATE",
                startDate = new DateTime(2010, 1,1),
                endDate = new DateTime(2010, 12, 31),
                allowedDate = null,
                balance =null
            }

        };

        public List<LeaveTransactionRecord> leaveTransactionRecords = new List<LeaveTransactionRecord>()
        {
            new LeaveTransactionRecord()
            {
                id = "1",
                personId = "0003914",
                action = "A",
                date = new DateTime(2018, 1, 1),
                employeeLeavePlanId = "20",
                leavePlanId = "VACH",
                forwardingBalance = 50,
                hours = 50
            },
            new LeaveTransactionRecord()
            {
                id = "2",
                personId = "0003914",
                action = "U",
                date = new DateTime(2018, 2, 1),
                employeeLeavePlanId = "20",
                leavePlanId = "VACH",
                forwardingBalance = 25,
                hours = -25
            },
            new LeaveTransactionRecord()
            {
                id ="3",
                personId = "0004888",
                action = "J",
                date = new DateTime(2017, 6, 4),
                employeeLeavePlanId = "21",
                leavePlanId = "SICH",
                forwardingBalance = 17.5m,
                hours = 4.25m
            },
            new LeaveTransactionRecord()
            {
                id ="4",
                personId = "0004888",
                action = "A",
                date = new DateTime(2017, 7, 1),
                employeeLeavePlanId = "21",
                leavePlanId = "SICH",
                forwardingBalance = null,
                hours = null
            }
        };

        public List<LeavePlan> leavePlans = new List<LeavePlan>()
        {
            new LeavePlan("guid1", "VACH", new DateTime(2000,1,1), "VacationHourly","VACA", "accrual", new List<string>() { "VAC" })
            {
                AllowNegative = "Y",
                YearlyStartDate = new DateTime(2000, 1,1),
                IsLeaveReportingPlan = true
            },
            new LeavePlan("guid2", "SICH", new DateTime(2000,1,1), "SickHourly", "SICK", "accrual", new List<string>() {"SIC" })
            {
                AllowNegative = "N",
                YearlyStartDate = new DateTime(2000, 7, 1),
                IsLeaveReportingPlan = true
            },
            new LeavePlan("guid3", "INVALIDEARNTYPE", new DateTime(2000,1,1), "Error", "OTH", "accrual", new List<string>() { "FOOBAR" } )
            {
                AllowNegative = "N",
                YearlyStartDate = new DateTime(2005, 1,1),
                IsLeaveReportingPlan = true
            },
            new LeavePlan("guid3", "NOYEARLYSTARTDATE", new DateTime(2000,1,1), "NoStartDate", "OTH", "accrual", new List<string>() { "PER" } )
            {
                AllowNegative = "N",
                YearlyStartDate = new DateTime(2005, 1,1),
                IsLeaveReportingPlan = true
            },

        };
        public List<LeaveType> leaveTypes = new List<LeaveType>()
        {
            new LeaveType("guid1", "SICK", "Sick type") { TimeType = LeaveTypeCategory.Sick },
            new LeaveType("guid2", "VACA", "Vacation type") { TimeType = LeaveTypeCategory.Vacation },
            new LeaveType("guid3", "OTH", "Other type") { TimeType = LeaveTypeCategory.None },
            new LeaveType("guid4", "COMPA", "Comp time accrual") { TimeType = LeaveTypeCategory.Compensatory },


        };
        public List<EarningType2> earnTypes = new List<EarningType2>()
        {
            new EarningType2("guid1", "VAC", "Vacation EArnings Type"),
            new EarningType2("guid2", "SIC", "Sick LEave Earnigns Type"),
            new EarningType2("guid3", "PER", "Personal Leave")
        };

        public Decimal? accrualRate = 80;
        public Decimal? accrualLimit = 50;
        public Decimal? accrualMaxCarryOver = 20;
        public decimal? accrualMaxRollOver = 20;

        /// <summary>
        /// This one is owned by the HR team for Self Service
        /// </summary>
        /// <param name="employeeIds"></param>
        /// <param name="leavePlans"></param>
        /// <param name="leaveTypes"></param>
        /// <param name="earnTypes"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EmployeeLeavePlan>> GetEmployeeLeavePlansByEmployeeIdsAsync(IEnumerable<string> employeeIds, IEnumerable<LeavePlan> leavePlans, IEnumerable<LeaveType> leaveTypes, IEnumerable<EarningType2> earnTypes, bool includeLeavePlansWithNoEarningsTypes = false)
        {
            var employeeLeavePlans = new List<EmployeeLeavePlan>();
            var leavePlanDictionary = leavePlans.ToDictionary(lp => lp.Id);
            var earningsTypesDictionary = earnTypes.ToDictionary(e => e.Code);

            foreach (var employeeLeavePlanRecord in employeeLeavePlanRecords.Where(lp => employeeIds.Contains(lp.personId)))
            {
                try
                {
                    if (leavePlanDictionary.ContainsKey(employeeLeavePlanRecord.leavePlanId))
                    {
                        var leavePlan = leavePlanDictionary[employeeLeavePlanRecord.leavePlanId];
                        if (earningsTypesDictionary.ContainsKey(leavePlan.EarningsTypes.FirstOrDefault()))
                        {
                            var earnType = earningsTypesDictionary[leavePlan.EarningsTypes.FirstOrDefault()];
                            var leaveType = leaveTypes.FirstOrDefault(lt => lt.Code == leavePlan.Type);


                            var employeeLeavePlan = new EmployeeLeavePlan(employeeLeavePlanRecord.id,
                                employeeLeavePlanRecord.personId,
                                employeeLeavePlanRecord.startDate.Value,
                                employeeLeavePlanRecord.endDate,
                                leavePlan.Id,
                                leavePlan.Title,
                                leavePlan.StartDate.Value,
                                leavePlan.EndDate,
                                leavePlan.Type,
                                leaveType == null ? LeaveTypeCategory.None : leaveType.TimeType,
                                earnType.Code,
                                earnType.Description,
                                employeeLeavePlanRecord.allowedDate ?? employeeLeavePlanRecord.startDate.Value,
                                employeeLeavePlanRecord.balance.Value,
                                leavePlan.YearlyStartDate.HasValue ? leavePlan.YearlyStartDate.Value.Month : 1,
                                leavePlan.YearlyStartDate.HasValue ? leavePlan.YearlyStartDate.Value.Day : 1,
                                true,
                                leavePlan.EarningsTypes,
                                accrualRate,
                                accrualLimit,
                                accrualMaxCarryOver,
                                accrualMaxRollOver,
                                leavePlan.AccrualMethod,
                                false,
                                new DateTime(2022, 1, 1),
                                true);

                            employeeLeavePlans.Add(employeeLeavePlan);

                            var transactionRecords = leaveTransactionRecords.Where(lt => lt.employeeLeavePlanId == employeeLeavePlanRecord.id);
                            foreach (var transactionRecord in transactionRecords)
                            {
                                try
                                {
                                    if (transactionRecord.date.HasValue)
                                    {
                                        int transactionId = int.Parse(transactionRecord.id);

                                        var transaction = new EmployeeLeaveTransaction(transactionId,
                                            transactionRecord.leavePlanId,
                                            transactionRecord.employeeLeavePlanId,
                                            transactionRecord.hours ?? 0,
                                            transactionRecord.date.Value,
                                            translateTransactionType(transactionRecord.action),
                                            transactionRecord.forwardingBalance ?? 0);

                                        employeeLeavePlan.AddLeaveTransaction(transaction);
                                    }
                                }
                                catch (Exception) { }
                            }

                        }
                    }
                }
                catch (Exception) { }

            }

            return await Task.FromResult(employeeLeavePlans);
        }

        public Task<Tuple<IEnumerable<Perleave>, int>> GetEmployeeLeavePlansAsync(int offset, int limit, bool bypassCache = false)
        {
            throw new NotImplementedException();
        }

        public Task<Perleave> GetEmployeeLeavePlansByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<Perleave> GetEmployeeLeavePlansByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        private LeaveTransactionType translateTransactionType(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentNullException("action");
            }

            switch (action.ToUpper())
            {
                case "A":
                    return LeaveTransactionType.Earned;
                case "U":
                    return LeaveTransactionType.Used;
                case "J":
                    return LeaveTransactionType.Adjusted;
                case "L":
                    return LeaveTransactionType.LeaveReporting;
                case "S":
                    return LeaveTransactionType.StartingBalanceAdjustment;
                case "B":
                    return LeaveTransactionType.StartingBalance;
                case "C":
                    return LeaveTransactionType.MidYearBalanceAdjustment;
                case "R":
                    return LeaveTransactionType.Rollover;
                default:
                    throw new ArgumentException("leave transaction action does not match allowable values, A, U, L, J, S, B, C or R", "action");
            }
        }

        public Task<Dictionary<string, string>> GetPerleaveGuidsCollectionAsync(IEnumerable<string> perleaveIds)
        {
            throw new NotImplementedException();
        }
    }
}
