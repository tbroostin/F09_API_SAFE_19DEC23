/* Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPayrollRegisterRepository : IPayrollRegisterRepository
    {
        public TestPayrollRegisterRepository()
        {
            payToDateRecords = createPayToDateRecord("0003914", "BW");
            payControlRecords = createPayControlRecords("BW");
        }
        public class PayToDateRecord
        {
            public string id;
            public string checkNumber;
            public string adviceNumber;
            public List<PayToDateEarningsRecord> earnings;
            public List<PayToDateTaxRecord> taxes;
            public List<PayToDateExpandedTaxRecord> expandedTaxes;
            public List<PayToDateBenefitRecord> benefits;
            public List<PayToDateExpandedBenefitRecord> expandedBenefits;
            public List<PayToDateLeaveRecord> leave;
            public List<PayToDateLeaveTakenRecord> leaveTaken;
            public List<PayToDateTaxableBenefitRecord> expandedTaxableBenefits;
        }

        public class PayToDateEarningsRecord
        {
            public string earningsCode;
            public string hourlySalaryFlag;
            public decimal? totalAmount;
            public decimal? baseAmount;
            public decimal? earningsFactorAmount;
            public decimal? hours;
            public decimal? rate;

            public string differentialId;
            public decimal? diffAmount;
            public decimal? diffHours;
            public decimal? diffRate;

            public string stipendId;
        }

        public class PayToDateTaxRecord
        {
            public string taxCode;
            public int? exemptions;
            public string processingCode;
            public decimal? specialProcessingAmount;
            public decimal? employeeTaxAmount;
            public decimal? employeeTaxableAmount;
            public decimal? employerTaxableAmount;
        }

        public class PayToDateExpandedTaxRecord
        {
            public string expandedId;
            public decimal? employerTaxAmount;
        }

        public class PayToDateBenefitRecord
        {
            public string benefitCode;
            public decimal? employeeAmount;
            public decimal? employeeBaseAmount;
            public decimal? employerBaseAmount;
        }

        public class PayToDateExpandedBenefitRecord
        {
            public string expandedId;
            public decimal? employerAmount;
        }

        public class PayToDateLeaveRecord
        {
            public string code;
            public string leaveType;
            public decimal? accruedHours;
            public decimal? priorBalanceHours;
        }

        public class PayToDateLeaveTakenRecord
        {
            public string controller;
            public decimal? takenHours;
        }

        public class PayToDateTaxableBenefitRecord
        {
            public string controller;
            public decimal? employerAmount;
        }

        public class PayControlRecord
        {
            public string id;
            public DateTime? periodStartDate;
        }

        public List<PayToDateRecord> payToDateRecords = new List<PayToDateRecord>();
        //createPayToDateRecord("0003914", "BW");

        public List<PayToDateRecord> createPayToDateRecord(string employeeId, string payCycleId)
        {
            var list = new List<PayToDateRecord>()
            {
                #region record1
                new PayToDateRecord()
                {
                    id = string.Format("12345*{0}*{1}*1", payCycleId, employeeId),
                    adviceNumber = "333",
                    checkNumber = "111",
                    earnings = new List<PayToDateEarningsRecord>()
                    {
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 123,
                            hours = 10,
                            rate = 12.3m,
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "OVT",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 60,
                            earningsFactorAmount = 63,
                            hours = 10,
                            rate = 12.3m,

                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 22.3m,
                            baseAmount = 2.3m,
                            hours = 1,
                            rate = 2.3m,
                            differentialId = "NITE",
                            diffAmount = 20,
                            diffHours = 1,
                            diffRate = 20
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "S",
                            totalAmount = 124.5m,
                            baseAmount = 124.5m,
                            hours = 1,
                            rate = 124.5m,
                            stipendId = "RHEIA"
                        }
                    },
                    taxes = new List<PayToDateTaxRecord>()
                    {
                        new PayToDateTaxRecord()
                        {
                            taxCode = "MEDI",
                            exemptions = 1,
                            specialProcessingAmount = 100,
                            processingCode = "A",
                            employeeTaxAmount = 10,
                            employeeTaxableAmount = 143,
                            employerTaxableAmount = 143,
                        }
                    },
                    expandedTaxes = new List<PayToDateExpandedTaxRecord>()
                    {
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*BLAHLBAHLBAH",
                            employerTaxAmount = 5
                        },
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*FOOBAR*POSITION",
                            employerTaxAmount = 5
                        }
                    },
                    benefits = new List<PayToDateBenefitRecord>()
                    {
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "BEN",
                            employeeAmount = 10,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143,
                        },
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "DED",
                            employeeAmount = 20,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143
                        }
                    },
                    expandedBenefits = new List<PayToDateExpandedBenefitRecord>()
                    {
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*BLAHLBAHLBAH",
                            employerAmount = 10,
                        },
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*FOOBAR*POSITION",
                            employerAmount = 15,
                        }
                    },
                    leave = new List<PayToDateLeaveRecord>()
                    {
                        new PayToDateLeaveRecord()
                        {
                            code = "SICK",
                            leaveType = "SIC",
                            accruedHours = 1,
                            priorBalanceHours = 8
                        }
                    },
                    leaveTaken = new List<PayToDateLeaveTakenRecord>()
                    {
                        new PayToDateLeaveTakenRecord()
                        {
                            controller = "SICK*SIC",
                            takenHours = 4
                        }
                    },
                    expandedTaxableBenefits = new List<PayToDateTaxableBenefitRecord>()
                    {
                        new PayToDateTaxableBenefitRecord()
                        {
                            controller = "FICA*LIFE",
                            employerAmount = 15.55m
                        },
                        new PayToDateTaxableBenefitRecord()
                        {
                            controller = "MEDI*LIFE",
                            employerAmount = 15.55m
                        }
                    }
                },
                #endregion

                #region record2
                new PayToDateRecord()
                {
                    id = string.Format("12345*{0}*{1}*1", payCycleId, employeeId),
                    adviceNumber = "334",
                    checkNumber = "111",
                    earnings = new List<PayToDateEarningsRecord>()
                    {
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 123,
                            hours = 10,
                            rate = 12.3m,
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "OVT",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 60,
                            earningsFactorAmount = 63,
                            hours = 10,
                            rate = 12.3m,

                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 22.3m,
                            baseAmount = 2.3m,
                            hours = 1,
                            rate = 2.3m,
                            differentialId = "NITE",
                            diffAmount = 20,
                            diffHours = 1,
                            diffRate = 20
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "S",
                            totalAmount = 124.5m,
                            baseAmount = 124.5m,
                            hours = 1,
                            rate = 124.5m,
                            stipendId = "RHEIA"
                        }
                    },
                    taxes = new List<PayToDateTaxRecord>()
                    {
                        new PayToDateTaxRecord()
                        {
                            exemptions = 0,
                            specialProcessingAmount = null,
                            processingCode = "R",
                            taxCode = "MEDI",
                            employeeTaxAmount = 10,
                            employeeTaxableAmount = 143,
                            employerTaxableAmount = 143,
                        }
                    },
                    expandedTaxes = new List<PayToDateExpandedTaxRecord>()
                    {
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*BLAHLBAHLBAH",
                            employerTaxAmount = 5
                        },
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*FOOBAR*POSITION",
                            employerTaxAmount = 5
                        }
                    },
                    benefits = new List<PayToDateBenefitRecord>()
                    {
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "BEN",
                            employeeAmount = 10,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143,
                        },
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "DED",
                            employeeAmount = 20,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143
                        }
                    },
                    expandedBenefits = new List<PayToDateExpandedBenefitRecord>()
                    {
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*BLAHLBAHLBAH",
                            employerAmount = 10,
                        },
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*FOOBAR*POSITION",
                            employerAmount = 15,
                        }
                    },
                    leave = new List<PayToDateLeaveRecord>()
                    {
                        new PayToDateLeaveRecord()
                        {
                            code = "SICK",
                            leaveType = "SIC",
                            accruedHours = 1,
                            priorBalanceHours = 8
                        }
                    },
                    leaveTaken = new List<PayToDateLeaveTakenRecord>()
                    {
                        new PayToDateLeaveTakenRecord()
                        {
                            controller = "SICK*SIC",
                            takenHours = 4
                        }
                    },
                    expandedTaxableBenefits = new List<PayToDateTaxableBenefitRecord>()
                    {
                        new PayToDateTaxableBenefitRecord()
                        {
                            controller = "FICA*LIFE",
                            employerAmount = 15.55m
                        },
                        new PayToDateTaxableBenefitRecord()
                        {
                            controller = "MEDI*LIFE",
                            employerAmount = 15.55m
                        }
                    }
                },
                #endregion

                #region record3
                new PayToDateRecord()
                {
                    id = string.Format("12345*{0}*{1}*1", payCycleId, employeeId),
                    adviceNumber = "335",
                    checkNumber = "111",
                    earnings = new List<PayToDateEarningsRecord>()
                    {
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 123,
                            hours = 10,
                            rate = 12.3m,
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "OVT",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 60,
                            earningsFactorAmount = 63,
                            hours = 10,
                            rate = 12.3m,

                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 22.3m,
                            baseAmount = 2.3m,
                            hours = 1,
                            rate = 2.3m,
                            differentialId = "NITE",
                            diffAmount = 20,
                            diffHours = 1,
                            diffRate = 20
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "S",
                            totalAmount = 124.5m,
                            baseAmount = 124.5m,
                            hours = 1,
                            rate = 124.5m,
                            stipendId = "RHEIA"
                        }
                    },
                    taxes = new List<PayToDateTaxRecord>()
                    {
                        new PayToDateTaxRecord()
                        {
                            exemptions = 2,
                            specialProcessingAmount = null,
                            processingCode = "D",
                            taxCode = "MEDI",
                            employeeTaxAmount = 10,
                            employeeTaxableAmount = 143,
                            employerTaxableAmount = 143,
                        }
                    },
                    expandedTaxes = new List<PayToDateExpandedTaxRecord>()
                    {
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*BLAHLBAHLBAH",
                            employerTaxAmount = 5
                        },
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*FOOBAR*POSITION",
                            employerTaxAmount = 5
                        }
                    },
                    benefits = new List<PayToDateBenefitRecord>()
                    {
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "BEN",
                            employeeAmount = 10,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143,
                        },
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "DED",
                            employeeAmount = 20,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143
                        }
                    },
                    expandedBenefits = new List<PayToDateExpandedBenefitRecord>()
                    {
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*BLAHLBAHLBAH",
                            employerAmount = 10,
                        },
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*FOOBAR*POSITION",
                            employerAmount = 15,
                        }
                    },
                    leave = new List<PayToDateLeaveRecord>()
                    {
                        new PayToDateLeaveRecord()
                        {
                            code = "SICK",
                            leaveType = "SIC",
                            accruedHours = 1,
                            priorBalanceHours = 8
                        }
                    },
                    leaveTaken = new List<PayToDateLeaveTakenRecord>()
                    {
                        new PayToDateLeaveTakenRecord()
                        {
                            controller = "SICK*SIC",
                            takenHours = 4
                        }
                    },
                    expandedTaxableBenefits = new List<PayToDateTaxableBenefitRecord>(){ }
                },
                #endregion

                #region record4
                new PayToDateRecord()
                {
                    id = string.Format("12345*{0}*{1}*1", payCycleId, employeeId),
                    adviceNumber = "336",
                    checkNumber = "111",
                    earnings = new List<PayToDateEarningsRecord>()
                    {
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 123,
                            hours = 10,
                            rate = 12.3m,
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "OVT",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 60,
                            earningsFactorAmount = 63,
                            hours = 10,
                            rate = 12.3m,

                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 22.3m,
                            baseAmount = 2.3m,
                            hours = 1,
                            rate = 2.3m,
                            differentialId = "NITE",
                            diffAmount = 20,
                            diffHours = 1,
                            diffRate = 20
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "S",
                            totalAmount = 124.5m,
                            baseAmount = 124.5m,
                            hours = 1,
                            rate = 124.5m,
                            stipendId = "RHEIA"
                        }
                    },
                    taxes = new List<PayToDateTaxRecord>()
                    {
                        new PayToDateTaxRecord()
                        {
                            exemptions = 0,
                            specialProcessingAmount = null,
                            processingCode = null,
                            taxCode = "MEDI",
                            employeeTaxAmount = 10,
                            employeeTaxableAmount = 143,
                            employerTaxableAmount = 143,
                        }
                    },
                    expandedTaxes = new List<PayToDateExpandedTaxRecord>()
                    {
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*BLAHLBAHLBAH",
                            employerTaxAmount = 5
                        },
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*FOOBAR*POSITION",
                            employerTaxAmount = 5
                        }
                    },
                    benefits = new List<PayToDateBenefitRecord>()
                    {
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "BEN",
                            employeeAmount = 10,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143,
                        },
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "DED",
                            employeeAmount = 20,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143
                        }
                    },
                    expandedBenefits = new List<PayToDateExpandedBenefitRecord>()
                    {
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*BLAHLBAHLBAH",
                            employerAmount = 10,
                        },
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*FOOBAR*POSITION",
                            employerAmount = 15,
                        }
                    },
                    leave = new List<PayToDateLeaveRecord>()
                    {
                        new PayToDateLeaveRecord()
                        {
                            code = "SICK",
                            leaveType = "SIC",
                            accruedHours = 1,
                            priorBalanceHours = 8
                        }
                    },
                    leaveTaken = new List<PayToDateLeaveTakenRecord>()
                    {
                        new PayToDateLeaveTakenRecord()
                        {
                            controller = "SICK*SIC",
                            takenHours = 4
                        }
                    },
                    expandedTaxableBenefits = new List<PayToDateTaxableBenefitRecord>(){ }
                },
                #endregion

                #region record5
                new PayToDateRecord()
                {
                    id = string.Format("12345*{0}*{1}*1", payCycleId, employeeId),
                    adviceNumber = "444",
                    checkNumber = "111",
                    earnings = new List<PayToDateEarningsRecord>()
                    {
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 123,
                            hours = 10,
                            rate = 12.3m,
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "OVT",
                            hourlySalaryFlag = "H",
                            totalAmount = 123,
                            baseAmount = 60,
                            earningsFactorAmount = 63,
                            hours = 10,
                            rate = 12.3m,

                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "H",
                            totalAmount = 22.3m,
                            baseAmount = 2.3m,
                            hours = 1,
                            rate = 2.3m,
                            differentialId = "NITE",
                            diffAmount = 20,
                            diffHours = 1,
                            diffRate = 20
                        },
                        new PayToDateEarningsRecord()
                        {
                            earningsCode = "REG",
                            hourlySalaryFlag = "S",
                            totalAmount = 124.5m,
                            baseAmount = 124.5m,
                            hours = 1,
                            rate = 124.5m,
                            stipendId = "RHEIA"
                        }
                    },
                    taxes = new List<PayToDateTaxRecord>()
                    {
                        new PayToDateTaxRecord()
                        {
                            exemptions = 1,
                            specialProcessingAmount = 100,
                            processingCode = "A",
                            taxCode = "MEDI",
                            employeeTaxAmount = 10,
                            employeeTaxableAmount = 143,
                            employerTaxableAmount = 143,
                        }
                    },
                    expandedTaxes = new List<PayToDateExpandedTaxRecord>()
                    {
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*BLAHLBAHLBAH",
                            employerTaxAmount = 5
                        },
                        new PayToDateExpandedTaxRecord()
                        {
                            expandedId = "MEDI*FOOBAR*POSITION",
                            employerTaxAmount = 5
                        }
                    },
                    benefits = new List<PayToDateBenefitRecord>()
                    {
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "BEN",
                            employeeAmount = 10,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143,
                        },
                        new PayToDateBenefitRecord()
                        {
                            benefitCode = "DED",
                            employeeAmount = 20,
                            employeeBaseAmount = 143,
                            employerBaseAmount = 143
                        }
                    },
                    expandedBenefits = new List<PayToDateExpandedBenefitRecord>()
                    {
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*BLAHLBAHLBAH",
                            employerAmount = 10,
                        },
                        new PayToDateExpandedBenefitRecord()
                        {
                            expandedId = "BEN*FOOBAR*POSITION",
                            employerAmount = 15,
                        }
                    },
                    leave = new List<PayToDateLeaveRecord>()
                    {
                        new PayToDateLeaveRecord()
                        {
                            code = "SICK",
                            leaveType = "SIC",
                            accruedHours = 1,
                            priorBalanceHours = 8
                        }
                    },
                    leaveTaken = new List<PayToDateLeaveTakenRecord>()
                    {
                        new PayToDateLeaveTakenRecord()
                        {
                            controller = "SICK*SIC",
                            takenHours = 4
                        }
                    },
                    expandedTaxableBenefits = new List<PayToDateTaxableBenefitRecord>(){ }
                }
                #endregion
            };
            payToDateRecords.AddRange(list);
            return list;
        }

        public List<PayControlRecord> payControlRecords;

        public List<PayControlRecord> createPayControlRecords(string payCycle)
        {
            return new List<PayControlRecord>()
            {
                //try to create a paycontrol record with ids with the same first parts as the payToDateRecords
                new PayControlRecord()
                {
                    id = "12345*BW", //matches records 1-5
                    periodStartDate = new DateTime(2010, 1, 1)
                }
            };
        }

        /// <summary>
        /// This is just a local helper
        /// </summary>
        /// <param name="employeeIds"></param>
        /// <returns></returns>
        public IEnumerable<PayrollRegisterEntry> GetPayrollRegisterByEmployeeIds(IEnumerable<string> employeeIds)
        {
            if (payToDateRecords == null)
            {
                return null;
            }

            var register = new List<PayrollRegisterEntry>();

            foreach (var employeeId in employeeIds)
            {
                foreach (var ptd in payToDateRecords)
                {
                    try
                    {
                        var keyParts = ptd.id.Split('*');

                        var payControl = payControlRecords.FirstOrDefault(pc => pc.id == string.Format("{0}*{1}", keyParts[0], keyParts[1]));
                        var entry = new PayrollRegisterEntry(ptd.id, employeeId, payControl.periodStartDate.Value, DmiString.PickDateToDateTime(int.Parse(keyParts[0])), keyParts[1], int.Parse(keyParts[3]), ptd.checkNumber, ptd.adviceNumber);

                        foreach (var earn in ptd.earnings)
                        {
                            var hsIndicator = earn.hourlySalaryFlag.Equals("H", StringComparison.InvariantCultureIgnoreCase) ? HourlySalaryIndicator.Hourly : HourlySalaryIndicator.Salary;

                            if (string.IsNullOrEmpty(earn.stipendId))
                            {
                                var earningsEntry = new PayrollRegisterEarningsEntry(earn.earningsCode,
                                    earn.totalAmount.Value,
                                    earn.baseAmount ?? 0,
                                    earn.earningsFactorAmount ?? 0,
                                    earn.hours,
                                    earn.rate.Value,
                                    hsIndicator);
                                //new PayrollRegisterEarningsEntry(earn.earningsCode, earn.amount.Value, earn.hours, );
                                if (!string.IsNullOrEmpty(earn.differentialId))
                                {
                                    earningsEntry.SetEarningsDifferential(earn.differentialId, earn.diffAmount.Value, earn.diffHours, earn.diffRate.Value);
                                }
                                entry.EarningsEntries.Add(earningsEntry);
                            }
                            else
                            {
                                var stipendEntry = new PayrollRegisterEarningsEntry(earn.earningsCode, 
                                    earn.stipendId, 
                                    earn.totalAmount.Value, 
                                    earn.baseAmount ?? 0,
                                    earn.earningsFactorAmount ?? 0,
                                    earn.hours, 
                                    earn.rate.Value, 
                                    hsIndicator);
                            }
                        }

                        foreach (var tax in ptd.taxes)
                        {
                            var associatedEmployerTaxes = ptd.expandedTaxes.Where(exp => exp.expandedId.StartsWith(tax.taxCode));

                            var taxEntry = new PayrollRegisterTaxEntry(tax.taxCode, ConvertInternalCode(tax.processingCode))
                            {
                                SpecialProcessingAmount = tax.specialProcessingAmount,
                                Exemptions = tax.exemptions ?? 0,
                                EmployeeTaxAmount = tax.employeeTaxAmount,
                                EmployeeTaxableAmount = tax.employeeTaxableAmount,
                                EmployerTaxAmount = associatedEmployerTaxes.Any() ? associatedEmployerTaxes.Sum(t => t.employerTaxAmount) : null,
                                EmployerTaxableAmount = tax.employerTaxableAmount
                            };
                            entry.TaxEntries.Add(taxEntry);
                        }

                        foreach (var bended in ptd.benefits)
                        {
                            var associatedEmployerBendeds = ptd.expandedBenefits.Where(exp => exp.expandedId.StartsWith(bended.benefitCode));

                            var benefitEntry = new PayrollRegisterBenefitDeductionEntry(bended.benefitCode)
                            {
                                EmployeeAmount = bended.employeeAmount,
                                EmployeeBasisAmount = bended.employeeBaseAmount,
                                EmployerAmount = associatedEmployerBendeds.Any() ? associatedEmployerBendeds.Sum(b => b.employerAmount) : null,
                                EmployerBasisAmount = bended.employerBaseAmount
                            };
                            entry.BenefitDeductionEntries.Add(benefitEntry);
                        }
                        register.Add(entry);

                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return register;
        }

        /// <summary>
        /// This is the implementation of the interface
        /// </summary>
        /// <param name="employeeIds"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollRegisterEntry>> GetPayrollRegisterByEmployeeIdsAsync(IEnumerable<string> employeeIds, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await Task.FromResult(GetPayrollRegisterByEmployeeIds(employeeIds));
        }



        public PayrollTaxProcessingCode ConvertInternalCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return PayrollTaxProcessingCode.Regular;
            }

            switch (code.ToUpperInvariant())
            {
                case "F":
                    return PayrollTaxProcessingCode.FixedAmount;
                case "A":
                    return PayrollTaxProcessingCode.AdditionalTaxAmount;
                case "T":
                    return PayrollTaxProcessingCode.AdditionalTaxableAmount;
                case "E":
                    return PayrollTaxProcessingCode.TaxExempt;
                case "R":
                    return PayrollTaxProcessingCode.Regular;
                case "D":
                    return PayrollTaxProcessingCode.Inactive;
                case "X":
                    return PayrollTaxProcessingCode.TaxableExempt;
                default:
                    return PayrollTaxProcessingCode.Regular;
            }
        }


    }
}
