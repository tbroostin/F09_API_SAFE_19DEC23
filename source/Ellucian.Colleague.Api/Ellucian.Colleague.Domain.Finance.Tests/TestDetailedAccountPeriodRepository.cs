// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestDetailedAccountPeriodRepository
    {
        private static DetailedAccountPeriod _detailedAccountPeriod = new DetailedAccountPeriod();
        public static DetailedAccountPeriod FullDetailedAccountPeriod(string personId)
        {
            GenerateFullDetailedAccountPeriod(personId);
            return _detailedAccountPeriod;
        }

        public static DetailedAccountPeriod PartialDetailedAccountPeriod1(string personId)
        {
            GeneratePartialDetailedAccountPeriod1(personId);
            return _detailedAccountPeriod;
        }

        public static DetailedAccountPeriod PartialDetailedAccountPeriod2(string personId)
        {
            GeneratePartialDetailedAccountPeriod2(personId);
            return _detailedAccountPeriod;
        }

        public static DetailedAccountPeriod PartialDetailedAccountPeriod3(string personId)
        {
            GeneratePartialDetailedAccountPeriod3(personId);
            return _detailedAccountPeriod;
        }

        public static DetailedAccountPeriod PartialDetailedAccountPeriod4(string personId)
        {
            GeneratePartialDetailedAccountPeriod4(personId);
            return _detailedAccountPeriod;
        }

        public static DetailedAccountPeriod PartialDetailedAccountPeriod5(string personId)
        {
            GeneratePartialDetailedAccountPeriod5(personId);
            return _detailedAccountPeriod;
        }

        public static DetailedAccountPeriod PartialDetailedAccountPeriod6(string personId)
        {
            GeneratePartialDetailedAccountPeriod6(personId);
            return _detailedAccountPeriod;
        }

        private static void GenerateFullDetailedAccountPeriod(string personId)
        {
            _detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = new Domain.Finance.Entities.AccountActivity.ChargesCategory()
                {
                    FeeGroups = new List<Domain.Finance.Entities.AccountActivity.FeeType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.FeeType()
                        {
                            DisplayOrder = 1,
                            FeeCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 100m,
                                    Date = DateTime.Today.AddDays(1),
                                    Description = "Fee 1 Description 1",
                                    Id = "100",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 200m,
                                    Date = DateTime.Today.AddDays(2),
                                    Description = "Fee 1 Description 2",
                                    Id = "101",
                                    TermId = "2014/FA"
                                }
                            },
                            Name = "Fees 1"
                        },
                        new Domain.Finance.Entities.AccountActivity.FeeType()
                        {
                            DisplayOrder = 2,
                            FeeCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 300m,
                                    Date = DateTime.Today.AddDays(3),
                                    Description = "Fee 2 Description 1",
                                    Id = "102",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 400m,
                                    Date = DateTime.Today.AddDays(4),
                                    Description = "Fee 2 Description 2",
                                    Id = "103",
                                    TermId = "2014/FA"
                                }
                            },
                            Name = "Fees 2"
                        }
                    },
                    Miscellaneous = new Domain.Finance.Entities.AccountActivity.OtherType()
                    {
                        DisplayOrder = 3,
                        Name = "Miscellaneous",
                        OtherCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                        {
                            new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                            {
                                Amount = 125m,
                                Date = DateTime.Today.AddDays(5),
                                Description = "Misc Description 1",
                                Id = "104",
                                TermId = "2014/FA"
                            },
                            new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                            {
                                Amount = 875m,
                                Date = DateTime.Today.AddDays(6),
                                Description = "Misc Description 2",
                                Id = "105",
                                TermId = "2014/FA"
                            }                       
                        }
                    },
                    OtherGroups = new List<Domain.Finance.Entities.AccountActivity.OtherType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.OtherType()
                        {
                            DisplayOrder = 4,
                            Name = "Other Charges",
                            OtherCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 250m,
                                    Date = DateTime.Today.AddDays(7),
                                    Description = "Other Description 1",
                                    Id = "106",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 750m,
                                    Date = DateTime.Today.AddDays(8),
                                    Description = "Other Description 2",
                                    Id = "107",
                                    TermId = "2014/FA"
                                }                       
                            }
                        }
                    },
                    RoomAndBoardGroups = new List<Domain.Finance.Entities.AccountActivity.RoomAndBoardType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.RoomAndBoardType()
                        {
                            DisplayOrder = 5,
                            Name = "Room and Board 1",
                            RoomAndBoardCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 350m,
                                    Date = DateTime.Today.AddDays(9),
                                    Description = "Room and Board 1 Description 1",
                                    Id = "108",
                                    Room = "Room and Board 1 Room 1",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 650m,
                                    Date = DateTime.Today.AddDays(10),
                                    Description = "Room and Board 1 Description 2",
                                    Id = "109",
                                    Room = "Room and Board 1 Room 2",
                                    TermId = "2014/FA"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.RoomAndBoardType()
                        {
                            DisplayOrder = 6,
                            Name = "Room and Board 2",
                            RoomAndBoardCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 450m,
                                    Date = DateTime.Today.AddDays(11),
                                    Description = "Room and Board 2 Description 1",
                                    Id = "110",
                                    Room = "Room and Board 2 Room 1",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 550m,
                                    Date = DateTime.Today.AddDays(12),
                                    Description = "Room and Board 2 Description 2",
                                    Id = "111",
                                    Room = "Room and Board 2 Room 2",
                                    TermId = "2014/FA"
                                }
                            }
                        }
                    },
                    TuitionBySectionGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionBySectionType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.TuitionBySectionType()
                        {
                            DisplayOrder = 7,
                            Name = "Tuition by Section 1",
                            SectionCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 200m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 1",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "3:00 PM",
                                    Instructor = "Professor Jones",
                                    StartTime = "1:30 PM",
                                    Status = "Active"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 800m,
                                    BillingCredits = 4m,
                                    Ceus = 1.5m,
                                    Classroom = "Classroom 2",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "10:00 AM",
                                    Instructor = "Dr. Smith",
                                    StartTime = "2:30 PM",
                                    Status = "Pending"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.TuitionBySectionType()
                        {
                            DisplayOrder = 8,
                            Name = "Tuition by Section 2",
                            SectionCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 300m,
                                    BillingCredits = 4m,
                                    Ceus = null,
                                    Classroom = "Classroom 3",
                                    Credits = 4m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "9:00 AM",
                                    Instructor = "Professor Duncan",
                                    StartTime = "10:30 AM",
                                    Status = "Dropped"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 700m,
                                    BillingCredits = 2m,
                                    Ceus = 2m,
                                    Classroom = "Classroom 4",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "12:00 PM",
                                    Instructor = "Dr. Rigby",
                                    StartTime = "1:45 PM",
                                    Status = "Active"
                                }
                            }
                        }
                    },
                    TuitionByTotalGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionByTotalType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.TuitionByTotalType()
                        {
                            DisplayOrder = 7,
                            Name = "Tuition by Section 1",
                            TotalCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 130m,
                                    BillingCredits = 13m,
                                    Ceus = 1m,
                                    Classroom = "Classroom 5",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "3:00 PM",
                                    Instructor = "Professor Jones",
                                    StartTime = "1:30 PM",
                                    Status = "Active"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 8070m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 6",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "10:00 AM",
                                    Instructor = "Dr. Smith",
                                    StartTime = "2:30 PM",
                                    Status = "Pending"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.TuitionByTotalType()
                        {
                            DisplayOrder = 8,
                            Name = "Tuition by Section 2",
                            TotalCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 460m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 7",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "9:00 AM",
                                    Instructor = "Professor Duncan",
                                    StartTime = "10:30 AM",
                                    Status = "Dropped"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 540m,
                                    BillingCredits = 3m,
                                    Ceus = 3m,
                                    Classroom = "Classroom 8",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "12:00 PM",
                                    Instructor = "Dr. Rigby",
                                    StartTime = "1:45 PM",
                                    Status = "Active"
                                }
                            }
                        }
                    }
                },
                Deposits = new Domain.Finance.Entities.AccountActivity.DepositCategory()
                {
                    Deposits = new List<Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem()
                        {
                            Amount = 300m,
                            Date = DateTime.Today.AddDays(-3),
                            Description = "Deposit 1",
                            Id = "112",
                            OtherAmount = null,
                            PaidAmount = null,
                            RefundAmount = null,
                            RemainingAmount = 300m,
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem()
                        {
                            Amount = 400m,
                            Date = DateTime.Today.AddDays(-6),
                            Description = "Deposit 2",
                            Id = "113",
                            OtherAmount = null,
                            PaidAmount = 250m,
                            RefundAmount = null,
                            RemainingAmount = 150m,
                            TermId = "2014/FA"
                        }
                    }
                },
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = new Domain.Finance.Entities.AccountActivity.FinancialAidCategory()
                {
                    AnticipatedAid = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem()
                        {
                            AwardAmount = 1000m,
                            AwardDescription = "Anticipated Award 1",
                            AwardTerms = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 100m,
                                    AwardTerm = "2014/FA",
                                    DisbursedAmount = 0m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 900m,
                                    AwardTerm = "2014/WI",
                                    DisbursedAmount = 0m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 1000m,
                                    AwardTerm = "2014/SP",
                                    DisbursedAmount = 2000m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = null,
                                    AwardTerm = "2013/SP",
                                    DisbursedAmount = 1000m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 200M,
                                    AwardTerm = "2013/WI",
                                    DisbursedAmount = null
                                }
                            },
                            Comments = "Anticipated Aid Comments 1",
                            IneligibleAmount = null,
                            LoanFee = 50m,
                            OtherTermAmount = null,
                            PeriodAward = "Anticipated Award 1"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem()
                        {
                            AwardAmount = 2000m,
                            AwardDescription = "Anticipated Award 2",
                            AwardTerms = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 300m,
                                    AwardTerm = "2014/FA",
                                    DisbursedAmount = 100m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 1050m,
                                    AwardTerm = "2014/WI",
                                    DisbursedAmount = 900m
                                }
                            },
                            Comments = "Anticipated Aid Comments 2",
                            IneligibleAmount = 500m,
                            LoanFee = null,
                            OtherTermAmount = 125m,
                            PeriodAward = "Anticipated Award 2"
                        }
                    },
                    DisbursedAid = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                        {
                            Amount = 450m,
                            Date = DateTime.Today.AddDays(13),
                            Description = "Disbursed Award 1",
                            Id = "114",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                        {
                            Amount = 550m,
                            Date = DateTime.Today.AddDays(13),
                            Description = "Disbursed Award 2",
                            Id = "115",
                            TermId = "2014/FA"
                        }
                    }
                },
                Id = personId,
                PaymentPlans = new Domain.Finance.Entities.AccountActivity.PaymentPlanCategory()
                {
                    PaymentPlans = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem()
                        {
                            Amount = 9000m,
                            CurrentBalance = 700m,
                            Description = "Payment Plan 1",
                            Id = "116",
                            OriginalAmount = 1500m,
                            PaymentPlanSchedules = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem()
                                {
                                    Amount = 600m,
                                    AmountPaid = 500m,
                                    Date = DateTime.Today.AddDays(-7),
                                    DatePaid = DateTime.Today.AddDays(-1),
                                    Description = "Plan 1 Scheduled Payment 1",
                                    Id = "117",
                                    LateCharge = 100m,
                                    NetAmountDue = 100m,
                                    SetupCharge = 200m,
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem()
                                {
                                    Amount = 400m,
                                    AmountPaid = null,
                                    Date = DateTime.Today,
                                    DatePaid = null,
                                    Description = "Plan 1 Scheduled Payment 2",
                                    Id = "118",
                                    LateCharge = null,
                                    NetAmountDue = 400m,
                                    SetupCharge = null,
                                    TermId = "2014/FA"
                                }
                            },
                            TermId = "2014/FA",
                            Type = "01"
                        },
                    }
                },
                Refunds = new Domain.Finance.Entities.AccountActivity.RefundCategory()
                {
                    Refunds = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem()
                        {
                            Amount = 333m,
                            Date = DateTime.Today.AddDays(-4),
                            Description = "Refund 1",
                            Id = "119",
                            Method = "CC",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem()
                        {
                            Amount = 664m,
                            Date = DateTime.Today.AddDays(-5),
                            Description = "Refund 2",
                            Id = "120",
                            Method = "ECHK",
                            TermId = "2014/FA"
                        }
                    }
                },
                Sponsorships = new Domain.Finance.Entities.AccountActivity.SponsorshipCategory()
                {
                    SponsorItems = new List<Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem()
                        {
                            Amount = 777m,
                            Date = DateTime.Today.AddDays(-6),
                            Description = "Sponsorship 1",
                            Id = "121",
                            Sponsorship = "SPON1",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem()
                        {
                            Amount = 223m,
                            Date = DateTime.Today.AddDays(-7),
                            Description = "Sponsorship 2",
                            Id = "122",
                            Sponsorship = "SPON2",
                            TermId = "2014/FA"
                        }
                    }
                },
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = new Domain.Finance.Entities.AccountActivity.StudentPaymentCategory()
                {
                    StudentPayments = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem()
                        {
                            Amount = 2000m,
                            Date = DateTime.Today.AddDays(-8),
                            Description = "Payment 1",
                            Id = "121",
                            Method = "ECHK",
                            ReferenceNumber = "12345",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem()
                        {
                            Amount = 1000m,
                            Date = DateTime.Today.AddDays(-9),
                            Description = "Payment 2",
                            Id = "122",
                            Method = "CC",
                            ReferenceNumber = "23456",
                            TermId = "2014/FA"
                        }
                    }
                }
            };
        }

        private static void GeneratePartialDetailedAccountPeriod1(string personId)
        {
            _detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = null,
                Deposits = new Domain.Finance.Entities.AccountActivity.DepositCategory()
                {
                    Deposits = new List<Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem()
                        {
                            Amount = 300m,
                            Date = DateTime.Today.AddDays(-3),
                            Description = "Deposit 1",
                            Id = "112",
                            OtherAmount = null,
                            PaidAmount = null,
                            RefundAmount = null,
                            RemainingAmount = 300m,
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem()
                        {
                            Amount = 400m,
                            Date = DateTime.Today.AddDays(-6),
                            Description = "Deposit 2",
                            Id = "113",
                            OtherAmount = null,
                            PaidAmount = 250m,
                            RefundAmount = null,
                            RemainingAmount = 150m,
                            TermId = "2014/FA"
                        }
                    }
                },
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = null,
                Id = personId,
                PaymentPlans = new Domain.Finance.Entities.AccountActivity.PaymentPlanCategory()
                {
                    PaymentPlans = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem()
                        {
                            Amount = 9000m,
                            CurrentBalance = 700m,
                            Description = "Payment Plan 1",
                            Id = "116",
                            OriginalAmount = 1500m,
                            PaymentPlanSchedules = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem()
                                {
                                    Amount = 600m,
                                    AmountPaid = 500m,
                                    Date = DateTime.Today.AddDays(-7),
                                    DatePaid = DateTime.Today.AddDays(-1),
                                    Description = "Plan 1 Scheduled Payment 1",
                                    Id = "117",
                                    LateCharge = 100m,
                                    NetAmountDue = 100m,
                                    SetupCharge = 200m,
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem()
                                {
                                    Amount = 400m,
                                    AmountPaid = null,
                                    Date = DateTime.Today,
                                    DatePaid = null,
                                    Description = "Plan 1 Scheduled Payment 2",
                                    Id = "118",
                                    LateCharge = null,
                                    NetAmountDue = 400m,
                                    SetupCharge = null,
                                    TermId = "2014/FA"
                                }
                            },
                            TermId = "2014/FA",
                            Type = "01"
                        },
                    }
                },
                Refunds = null,
                Sponsorships = new Domain.Finance.Entities.AccountActivity.SponsorshipCategory()
                {
                    SponsorItems = new List<Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem()
                        {
                            Amount = 777m,
                            Date = DateTime.Today.AddDays(-6),
                            Description = "Sponsorship 1",
                            Id = "121",
                            Sponsorship = "SPON1",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem()
                        {
                            Amount = 223m,
                            Date = DateTime.Today.AddDays(-7),
                            Description = "Sponsorship 2",
                            Id = "122",
                            Sponsorship = "SPON2",
                            TermId = "2014/FA"
                        }
                    }
                },
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = null
            };
        }

        private static void GeneratePartialDetailedAccountPeriod2(string personId)
        {
            _detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = new Domain.Finance.Entities.AccountActivity.ChargesCategory()
                {
                    FeeGroups = new List<Domain.Finance.Entities.AccountActivity.FeeType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.FeeType()
                        {
                            DisplayOrder = 1,
                            FeeCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 100m,
                                    Date = DateTime.Today.AddDays(1),
                                    Description = "Fee 1 Description 1",
                                    Id = "100",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 200m,
                                    Date = DateTime.Today.AddDays(2),
                                    Description = "Fee 1 Description 2",
                                    Id = "101",
                                    TermId = "2014/FA"
                                }
                            },
                            Name = "Fees 1"
                        },
                        new Domain.Finance.Entities.AccountActivity.FeeType()
                        {
                            DisplayOrder = 2,
                            FeeCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 300m,
                                    Date = DateTime.Today.AddDays(3),
                                    Description = "Fee 2 Description 1",
                                    Id = "102",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 400m,
                                    Date = DateTime.Today.AddDays(4),
                                    Description = "Fee 2 Description 2",
                                    Id = "103",
                                    TermId = "2014/FA"
                                }
                            },
                            Name = "Fees 2"
                        }
                    },
                    Miscellaneous = new Domain.Finance.Entities.AccountActivity.OtherType()
                    {
                        DisplayOrder = 3,
                        Name = "Miscellaneous",
                        OtherCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                        {
                            new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                            {
                                Amount = 125m,
                                Date = DateTime.Today.AddDays(5),
                                Description = "Misc Description 1",
                                Id = "104",
                                TermId = "2014/FA"
                            },
                            new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                            {
                                Amount = 875m,
                                Date = DateTime.Today.AddDays(6),
                                Description = "Misc Description 2",
                                Id = "105",
                                TermId = "2014/FA"
                            }                       
                        }
                    },
                    OtherGroups = new List<Domain.Finance.Entities.AccountActivity.OtherType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.OtherType()
                        {
                            DisplayOrder = 4,
                            Name = "Other Charges",
                            OtherCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 250m,
                                    Date = DateTime.Today.AddDays(7),
                                    Description = "Other Description 1",
                                    Id = "106",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                                {
                                    Amount = 750m,
                                    Date = DateTime.Today.AddDays(8),
                                    Description = "Other Description 2",
                                    Id = "107",
                                    TermId = "2014/FA"
                                }                       
                            }
                        }
                    },
                    RoomAndBoardGroups = new List<Domain.Finance.Entities.AccountActivity.RoomAndBoardType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.RoomAndBoardType()
                        {
                            DisplayOrder = 5,
                            Name = "Room and Board 1",
                            RoomAndBoardCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 350m,
                                    Date = DateTime.Today.AddDays(9),
                                    Description = "Room and Board 1 Description 1",
                                    Id = "108",
                                    Room = "Room and Board 1 Room 1",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 650m,
                                    Date = DateTime.Today.AddDays(10),
                                    Description = "Room and Board 1 Description 2",
                                    Id = "109",
                                    Room = "Room and Board 1 Room 2",
                                    TermId = "2014/FA"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.RoomAndBoardType()
                        {
                            DisplayOrder = 6,
                            Name = "Room and Board 2",
                            RoomAndBoardCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 450m,
                                    Date = DateTime.Today.AddDays(11),
                                    Description = "Room and Board 2 Description 1",
                                    Id = "110",
                                    Room = "Room and Board 2 Room 1",
                                    TermId = "2014/FA"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem()
                                {
                                    Amount = 550m,
                                    Date = DateTime.Today.AddDays(12),
                                    Description = "Room and Board 2 Description 2",
                                    Id = "111",
                                    Room = "Room and Board 2 Room 2",
                                    TermId = "2014/FA"
                                }
                            }
                        }
                    },
                    TuitionBySectionGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionBySectionType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.TuitionBySectionType()
                        {
                            DisplayOrder = 7,
                            Name = "Tuition by Section 1",
                            SectionCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 200m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 1",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "3:00 PM",
                                    Instructor = "Professor Jones",
                                    StartTime = "1:30 PM",
                                    Status = "Active"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 800m,
                                    BillingCredits = 4m,
                                    Ceus = 1.5m,
                                    Classroom = "Classroom 2",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "10:00 AM",
                                    Instructor = "Dr. Smith",
                                    StartTime = "2:30 PM",
                                    Status = "Pending"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.TuitionBySectionType()
                        {
                            DisplayOrder = 8,
                            Name = "Tuition by Section 2",
                            SectionCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 300m,
                                    BillingCredits = 4m,
                                    Ceus = null,
                                    Classroom = "Classroom 3",
                                    Credits = 4m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "9:00 AM",
                                    Instructor = "Professor Duncan",
                                    StartTime = "10:30 AM",
                                    Status = "Dropped"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 700m,
                                    BillingCredits = 2m,
                                    Ceus = 2m,
                                    Classroom = "Classroom 4",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "12:00 PM",
                                    Instructor = "Dr. Rigby",
                                    StartTime = "1:45 PM",
                                    Status = "Active"
                                }
                            }
                        }
                    },
                    TuitionByTotalGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionByTotalType>()
                    {
                        new Domain.Finance.Entities.AccountActivity.TuitionByTotalType()
                        {
                            DisplayOrder = 7,
                            Name = "Tuition by Section 1",
                            TotalCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 130m,
                                    BillingCredits = 13m,
                                    Ceus = 1m,
                                    Classroom = "Classroom 5",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "3:00 PM",
                                    Instructor = "Professor Jones",
                                    StartTime = "1:30 PM",
                                    Status = "Active"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 8070m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 6",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "10:00 AM",
                                    Instructor = "Dr. Smith",
                                    StartTime = "2:30 PM",
                                    Status = "Pending"
                                }
                            }
                        },
                        new Domain.Finance.Entities.AccountActivity.TuitionByTotalType()
                        {
                            DisplayOrder = 8,
                            Name = "Tuition by Section 2",
                            TotalCharges = new List<Domain.Finance.Entities.AccountActivity.ActivityTuitionItem>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 460m,
                                    BillingCredits = 3m,
                                    Ceus = null,
                                    Classroom = "Classroom 7",
                                    Credits = 3m,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                                    EndTime = "9:00 AM",
                                    Instructor = "Professor Duncan",
                                    StartTime = "10:30 AM",
                                    Status = "Dropped"
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityTuitionItem()
                                {
                                    Amount = 540m,
                                    BillingCredits = 3m,
                                    Ceus = 3m,
                                    Classroom = "Classroom 8",
                                    Credits = null,
                                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                                    EndTime = "12:00 PM",
                                    Instructor = "Dr. Rigby",
                                    StartTime = "1:45 PM",
                                    Status = "Active"
                                }
                            }
                        }
                    }
                },
                Deposits = null,
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = new Domain.Finance.Entities.AccountActivity.FinancialAidCategory()
                {
                    AnticipatedAid = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem()
                        {
                            AwardAmount = 1000m,
                            AwardDescription = "Anticipated Award 1",
                            AwardTerms = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 100m,
                                    AwardTerm = "2014/FA",
                                    DisbursedAmount = 0m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 900m,
                                    AwardTerm = "2014/WI",
                                    DisbursedAmount = 0m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 1000m,
                                    AwardTerm = "2014/SP",
                                    DisbursedAmount = 2000m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = null,
                                    AwardTerm = "2013/SP",
                                    DisbursedAmount = 1000m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 200M,
                                    AwardTerm = "2013/WI",
                                    DisbursedAmount = null
                                }
                            },
                            Comments = "Anticipated Aid Comments 1",
                            IneligibleAmount = null,
                            LoanFee = 50m,
                            OtherTermAmount = null,
                            PeriodAward = "Anticipated Award 1"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem()
                        {
                            AwardAmount = 2000m,
                            AwardDescription = "Anticipated Award 2",
                            AwardTerms = new List<Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm>()
                            {
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 300m,
                                    AwardTerm = "2014/FA",
                                    DisbursedAmount = 100m
                                },
                                new Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm()
                                {
                                    AnticipatedAmount = 1050m,
                                    AwardTerm = "2014/WI",
                                    DisbursedAmount = 900m
                                }
                            },
                            Comments = "Anticipated Aid Comments 2",
                            IneligibleAmount = 500m,
                            LoanFee = null,
                            OtherTermAmount = 125m,
                            PeriodAward = "Anticipated Award 2"
                        }
                    },
                    DisbursedAid = new List<Domain.Finance.Entities.AccountActivity.ActivityDateTermItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                        {
                            Amount = 450m,
                            Date = DateTime.Today.AddDays(13),
                            Description = "Disbursed Award 1",
                            Id = "114",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityDateTermItem()
                        {
                            Amount = 550m,
                            Date = DateTime.Today.AddDays(13),
                            Description = "Disbursed Award 2",
                            Id = "115",
                            TermId = "2014/FA"
                        }
                    }
                },
                Id = personId,
                PaymentPlans = null,
                Refunds = new Domain.Finance.Entities.AccountActivity.RefundCategory()
                {
                    Refunds = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem()
                        {
                            Amount = 333m,
                            Date = DateTime.Today.AddDays(-4),
                            Description = "Refund 1",
                            Id = "119",
                            Method = "CC",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem()
                        {
                            Amount = 664m,
                            Date = DateTime.Today.AddDays(-5),
                            Description = "Refund 2",
                            Id = "120",
                            Method = "ECHK",
                            TermId = "2014/FA"
                        }
                    }
                },
                Sponsorships = null,
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = new Domain.Finance.Entities.AccountActivity.StudentPaymentCategory()
                {
                    StudentPayments = new List<Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem>()
                    {
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem()
                        {
                            Amount = 2000m,
                            Date = DateTime.Today.AddDays(-8),
                            Description = "Payment 1",
                            Id = "121",
                            Method = "ECHK",
                            ReferenceNumber = "12345",
                            TermId = "2014/FA"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem()
                        {
                            Amount = 1000m,
                            Date = DateTime.Today.AddDays(-9),
                            Description = "Payment 2",
                            Id = "122",
                            Method = "CC",
                            ReferenceNumber = "23456",
                            TermId = "2014/FA"
                        }
                    }
                }
            };
        }

        private static void GeneratePartialDetailedAccountPeriod3(string personId)
        {
            _detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = new Domain.Finance.Entities.AccountActivity.ChargesCategory()
                {
                    FeeGroups = new List<Domain.Finance.Entities.AccountActivity.FeeType>(),
                    Miscellaneous = new Domain.Finance.Entities.AccountActivity.OtherType(),
                    OtherGroups = new List<Domain.Finance.Entities.AccountActivity.OtherType>(),
                    RoomAndBoardGroups = new List<Domain.Finance.Entities.AccountActivity.RoomAndBoardType>(),
                    TuitionBySectionGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionBySectionType>(),
                    TuitionByTotalGroups = new List<Domain.Finance.Entities.AccountActivity.TuitionByTotalType>()
                },
                Deposits = new Domain.Finance.Entities.AccountActivity.DepositCategory(),
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = new Domain.Finance.Entities.AccountActivity.FinancialAidCategory(),
                Id = personId,
                PaymentPlans = new Domain.Finance.Entities.AccountActivity.PaymentPlanCategory(),
                Refunds = new Domain.Finance.Entities.AccountActivity.RefundCategory(),
                Sponsorships = new Domain.Finance.Entities.AccountActivity.SponsorshipCategory(),
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = new Domain.Finance.Entities.AccountActivity.StudentPaymentCategory()
            };
        }

        private static void GeneratePartialDetailedAccountPeriod4(string personId)
        {
            _detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = new Domain.Finance.Entities.AccountActivity.ChargesCategory()
                {
                    FeeGroups = null,
                    Miscellaneous = new OtherType()
                    {
                        DisplayOrder = 1,
                        Name = "Name",
                        OtherCharges = null
                    },
                    OtherGroups = null,
                    RoomAndBoardGroups = null,
                    TuitionBySectionGroups = null,
                    TuitionByTotalGroups = null
                },
                Deposits = new Domain.Finance.Entities.AccountActivity.DepositCategory(),
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = new Domain.Finance.Entities.AccountActivity.FinancialAidCategory(),
                Id = personId,
                PaymentPlans = new Domain.Finance.Entities.AccountActivity.PaymentPlanCategory(),
                Refunds = new Domain.Finance.Entities.AccountActivity.RefundCategory(),
                Sponsorships = new Domain.Finance.Entities.AccountActivity.SponsorshipCategory(),
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = new Domain.Finance.Entities.AccountActivity.StudentPaymentCategory()
            };
        }

        private static void GeneratePartialDetailedAccountPeriod5(string personId)
        {
            _detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = new Domain.Finance.Entities.AccountActivity.ChargesCategory()
                {
                    FeeGroups = null,
                    Miscellaneous = new OtherType
                    {
                        DisplayOrder = 1,
                        Name = "Name",
                        OtherCharges = new List<ActivityDateTermItem>()
                    },
                    OtherGroups = null,
                    RoomAndBoardGroups = null,
                    TuitionBySectionGroups = null,
                    TuitionByTotalGroups = null
                },
                Deposits = new Domain.Finance.Entities.AccountActivity.DepositCategory(),
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = new Domain.Finance.Entities.AccountActivity.FinancialAidCategory(),
                Id = personId,
                PaymentPlans = new Domain.Finance.Entities.AccountActivity.PaymentPlanCategory(),
                Refunds = new Domain.Finance.Entities.AccountActivity.RefundCategory(),
                Sponsorships = new Domain.Finance.Entities.AccountActivity.SponsorshipCategory(),
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = new Domain.Finance.Entities.AccountActivity.StudentPaymentCategory()
            };
        }

        private static void GeneratePartialDetailedAccountPeriod6(string personId)
        {
            _detailedAccountPeriod = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
            {
                AmountDue = 10000m,
                AssociatedPeriods = new List<string>() { "Period1", "Period2" },
                Balance = 7500m,
                Charges = new Domain.Finance.Entities.AccountActivity.ChargesCategory()
                {
                    FeeGroups = null,
                    Miscellaneous = null,
                    OtherGroups = null,
                    RoomAndBoardGroups = null,
                    TuitionBySectionGroups = null,
                    TuitionByTotalGroups = null
                },
                Deposits = new Domain.Finance.Entities.AccountActivity.DepositCategory(),
                Description = "Current Period",
                DueDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(30),
                FinancialAid = new Domain.Finance.Entities.AccountActivity.FinancialAidCategory(),
                Id = personId,
                PaymentPlans = new Domain.Finance.Entities.AccountActivity.PaymentPlanCategory(),
                Refunds = new Domain.Finance.Entities.AccountActivity.RefundCategory(),
                Sponsorships = new Domain.Finance.Entities.AccountActivity.SponsorshipCategory(),
                StartDate = DateTime.Today.AddDays(-30),
                StudentPayments = new Domain.Finance.Entities.AccountActivity.StudentPaymentCategory()
            };
        }
    }
}
