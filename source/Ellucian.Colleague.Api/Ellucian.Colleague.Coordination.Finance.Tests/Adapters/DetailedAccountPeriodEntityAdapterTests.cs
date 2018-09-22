// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class DetailedAccountPeriodEntityAdapterTests
    {
        Dtos.Finance.AccountActivity.DetailedAccountPeriod detailedAccountPeriodDto;
        Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod detailedAccountPeriodEntity;
        DetailedAccountPeriodEntityAdapter detailedAccountPeriodEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            detailedAccountPeriodEntityAdapter = new DetailedAccountPeriodEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var activityDateTermItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDateTermItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDateTermItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityDepositItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDepositItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDepositItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityFinancialAidTermAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidTerm>(adapterRegistryMock.Object, loggerMock.Object);
            var activityFinancialAidItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityRemainingAmountItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRemainingAmountItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityPaymentMethodItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentMethodItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityPaymentPaidItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPaidItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityPaymentPlanScheduleItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanScheduleItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityPaymentPlanDetailsItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanDetailsItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityRoomAndBoardItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRoomAndBoardItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activitySponsorPaymentItem = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivitySponsorPaymentItem>(adapterRegistryMock.Object, loggerMock.Object);
            var activityTuitionItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityTuitionItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityTuitionItem>(adapterRegistryMock.Object, loggerMock.Object);
            var feeTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FeeType, Ellucian.Colleague.Dtos.Finance.AccountActivity.FeeType>(adapterRegistryMock.Object, loggerMock.Object);
            var otherTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.OtherType, Ellucian.Colleague.Dtos.Finance.AccountActivity.OtherType>(adapterRegistryMock.Object, loggerMock.Object);
            var roomAndBoardTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RoomAndBoardType, Ellucian.Colleague.Dtos.Finance.AccountActivity.RoomAndBoardType>(adapterRegistryMock.Object, loggerMock.Object);
            var tuitionBySectionTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionBySectionType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionBySectionType>(adapterRegistryMock.Object, loggerMock.Object);
            var tuitionByTotalTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionByTotalType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionByTotalType>(adapterRegistryMock.Object, loggerMock.Object);
            var chargesCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ChargesCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.ChargesCategory>(adapterRegistryMock.Object, loggerMock.Object);
            var depositCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DepositCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.DepositCategory>(adapterRegistryMock.Object, loggerMock.Object);
            var financialAidCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FinancialAidCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.FinancialAidCategory>(adapterRegistryMock.Object, loggerMock.Object);
            var paymentPlanCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.PaymentPlanCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.PaymentPlanCategory>(adapterRegistryMock.Object, loggerMock.Object);
            var refundCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RefundCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.RefundCategory>(adapterRegistryMock.Object, loggerMock.Object);
            var sponsorshipCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.SponsorshipCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.SponsorshipCategory>(adapterRegistryMock.Object, loggerMock.Object);
            var studentPaymentCategoryAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.StudentPaymentCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.StudentPaymentCategory>(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDateTermItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDateTermItem>()).Returns(activityDateTermItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDepositItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDepositItem>()).Returns(activityDepositItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidTerm>()).Returns(activityFinancialAidTermAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidItem>()).Returns(activityFinancialAidItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRemainingAmountItem>()).Returns(activityRemainingAmountItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentMethodItem>()).Returns(activityPaymentMethodItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPaidItem>()).Returns(activityPaymentPaidItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanScheduleItem>()).Returns(activityPaymentPlanScheduleItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanDetailsItem>()).Returns(activityPaymentPlanDetailsItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRoomAndBoardItem>()).Returns(activityRoomAndBoardItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivitySponsorPaymentItem>()).Returns(activitySponsorPaymentItem);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityTuitionItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityTuitionItem>()).Returns(activityTuitionItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FeeType, Ellucian.Colleague.Dtos.Finance.AccountActivity.FeeType>()).Returns(feeTypeAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.OtherType, Ellucian.Colleague.Dtos.Finance.AccountActivity.OtherType>()).Returns(otherTypeAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RoomAndBoardType, Ellucian.Colleague.Dtos.Finance.AccountActivity.RoomAndBoardType>()).Returns(roomAndBoardTypeAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionBySectionType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionBySectionType>()).Returns(tuitionBySectionTypeAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionByTotalType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionByTotalType>()).Returns(tuitionByTotalTypeAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ChargesCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.ChargesCategory>()).Returns(chargesCategoryAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DepositCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.DepositCategory>()).Returns(depositCategoryAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FinancialAidCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.FinancialAidCategory>()).Returns(financialAidCategoryAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.PaymentPlanCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.PaymentPlanCategory>()).Returns(paymentPlanCategoryAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RefundCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.RefundCategory>()).Returns(refundCategoryAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.SponsorshipCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.SponsorshipCategory>()).Returns(sponsorshipCategoryAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.StudentPaymentCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.StudentPaymentCategory>()).Returns(studentPaymentCategoryAdapter);

            detailedAccountPeriodEntity = new Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod()
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
                Id = "0003315",
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
                            TermId = "2014/FA",
                            CheckDate = DateTime.Today.AddDays(-3),
                            CheckNumber = "1234",
                            Status = Domain.Finance.Entities.RefundVoucherStatus.InProgress,
                            StatusDate = DateTime.Today.AddDays(-1),
                            TransactionNumber = "TRANS1234"
                        },
                        new Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem()
                        {
                            Amount = 664m,
                            Date = DateTime.Today.AddDays(-5),
                            Description = "Refund 2",
                            Id = "120",
                            Method = "ECHK",
                            TermId = "2014/FA",
                            CreditCardLastFourDigits = "1234",
                            Status = Domain.Finance.Entities.RefundVoucherStatus.Paid,
                            StatusDate = DateTime.Today.AddDays(-2),
                            TransactionNumber = "TRANS1235"
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

            detailedAccountPeriodDto = detailedAccountPeriodEntityAdapter.MapToType(detailedAccountPeriodEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_AmountDue()
        {
            Assert.AreEqual(detailedAccountPeriodEntity.AmountDue, detailedAccountPeriodDto.AmountDue);
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_AssociatedPeriods()
        {
            CollectionAssert.AreEqual(detailedAccountPeriodEntity.AssociatedPeriods, detailedAccountPeriodDto.AssociatedPeriods);
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Balance()
        {
            Assert.AreEqual(detailedAccountPeriodEntity.Balance, detailedAccountPeriodDto.Balance);
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Charges_FeeGroups()
        {
            if (detailedAccountPeriodEntity.Charges != null)
            {
                if (detailedAccountPeriodEntity.Charges.FeeGroups != null && detailedAccountPeriodEntity.Charges.FeeGroups.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodDto.Charges.FeeGroups.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.FeeGroups[i].DisplayOrder, detailedAccountPeriodDto.Charges.FeeGroups[i].DisplayOrder);
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.FeeGroups[i].Name, detailedAccountPeriodDto.Charges.FeeGroups[i].Name);
                        if (detailedAccountPeriodEntity.Charges.FeeGroups[i].FeeCharges != null && detailedAccountPeriodEntity.Charges.FeeGroups[i].FeeCharges.Count > 0)
                        {
                            for (int j = 0; j < detailedAccountPeriodDto.Charges.FeeGroups.Count; j++)
                            {
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.FeeGroups[i].FeeCharges[j].Amount, detailedAccountPeriodDto.Charges.FeeGroups[i].FeeCharges[j].Amount);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.FeeGroups[i].FeeCharges[j].Date, detailedAccountPeriodDto.Charges.FeeGroups[i].FeeCharges[j].Date);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.FeeGroups[i].FeeCharges[j].Description, detailedAccountPeriodDto.Charges.FeeGroups[i].FeeCharges[j].Description);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.FeeGroups[i].FeeCharges[j].Id, detailedAccountPeriodDto.Charges.FeeGroups[i].FeeCharges[j].Id);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.FeeGroups[i].FeeCharges[j].TermId, detailedAccountPeriodDto.Charges.FeeGroups[i].FeeCharges[j].TermId);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Charges_Miscellaneous()
        {
            if (detailedAccountPeriodEntity.Charges != null)
            {
                if (detailedAccountPeriodEntity.Charges.Miscellaneous != null)
                {
                    Assert.AreEqual(detailedAccountPeriodEntity.Charges.Miscellaneous.DisplayOrder, detailedAccountPeriodDto.Charges.Miscellaneous.DisplayOrder);
                    Assert.AreEqual(detailedAccountPeriodEntity.Charges.Miscellaneous.Name, detailedAccountPeriodDto.Charges.Miscellaneous.Name);
                    if (detailedAccountPeriodEntity.Charges.Miscellaneous.OtherCharges != null && detailedAccountPeriodEntity.Charges.Miscellaneous.OtherCharges.Count > 0)
                    {
                        for (int i = 0; i < detailedAccountPeriodDto.Charges.FeeGroups.Count; i++)
                        {
                            Assert.AreEqual(detailedAccountPeriodEntity.Charges.Miscellaneous.OtherCharges[i].Amount, detailedAccountPeriodDto.Charges.Miscellaneous.OtherCharges[i].Amount);
                            Assert.AreEqual(detailedAccountPeriodEntity.Charges.Miscellaneous.OtherCharges[i].Date, detailedAccountPeriodDto.Charges.Miscellaneous.OtherCharges[i].Date);
                            Assert.AreEqual(detailedAccountPeriodEntity.Charges.Miscellaneous.OtherCharges[i].Description, detailedAccountPeriodDto.Charges.Miscellaneous.OtherCharges[i].Description);
                            Assert.AreEqual(detailedAccountPeriodEntity.Charges.Miscellaneous.OtherCharges[i].Id, detailedAccountPeriodDto.Charges.Miscellaneous.OtherCharges[i].Id);
                            Assert.AreEqual(detailedAccountPeriodEntity.Charges.Miscellaneous.OtherCharges[i].TermId, detailedAccountPeriodDto.Charges.Miscellaneous.OtherCharges[i].TermId);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Charges_OtherGroups()
        {
            if (detailedAccountPeriodEntity.Charges != null)
            {
                if (detailedAccountPeriodEntity.Charges.OtherGroups != null && detailedAccountPeriodEntity.Charges.OtherGroups.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodDto.Charges.OtherGroups.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.OtherGroups[i].DisplayOrder, detailedAccountPeriodDto.Charges.OtherGroups[i].DisplayOrder);
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.OtherGroups[i].Name, detailedAccountPeriodDto.Charges.OtherGroups[i].Name);
                        if (detailedAccountPeriodEntity.Charges.OtherGroups[i].OtherCharges != null && detailedAccountPeriodEntity.Charges.OtherGroups[i].OtherCharges.Count > 0)
                        {
                            for (int j = 0; j < detailedAccountPeriodDto.Charges.OtherGroups.Count; j++)
                            {
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.OtherGroups[i].OtherCharges[j].Amount, detailedAccountPeriodDto.Charges.OtherGroups[i].OtherCharges[j].Amount);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.OtherGroups[i].OtherCharges[j].Date, detailedAccountPeriodDto.Charges.OtherGroups[i].OtherCharges[j].Date);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.OtherGroups[i].OtherCharges[j].Description, detailedAccountPeriodDto.Charges.OtherGroups[i].OtherCharges[j].Description);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.OtherGroups[i].OtherCharges[j].Id, detailedAccountPeriodDto.Charges.OtherGroups[i].OtherCharges[j].Id);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.OtherGroups[i].OtherCharges[j].TermId, detailedAccountPeriodDto.Charges.OtherGroups[i].OtherCharges[j].TermId);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Charges_RoomAndBoardGroups()
        {
            if (detailedAccountPeriodEntity.Charges != null)
            {
                if (detailedAccountPeriodEntity.Charges.RoomAndBoardGroups != null && detailedAccountPeriodEntity.Charges.RoomAndBoardGroups.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodDto.Charges.RoomAndBoardGroups.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].DisplayOrder, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].DisplayOrder);
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].Name, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].Name);
                        if (detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges != null && detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges.Count > 0)
                        {
                            for (int j = 0; j < detailedAccountPeriodDto.Charges.RoomAndBoardGroups.Count; j++)
                            {
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Amount, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Amount);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Date, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Date);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Description, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Description);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Id, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Id);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Room, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].Room);                               
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].TermId, detailedAccountPeriodDto.Charges.RoomAndBoardGroups[i].RoomAndBoardCharges[j].TermId);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Charges_TuitionBySectionGroups()
        {
            if (detailedAccountPeriodEntity.Charges != null)
            {
                if (detailedAccountPeriodEntity.Charges.TuitionBySectionGroups != null && detailedAccountPeriodEntity.Charges.TuitionBySectionGroups.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodDto.Charges.TuitionBySectionGroups.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].DisplayOrder, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].DisplayOrder);
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].Name, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].Name);
                        if (detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges != null && detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges.Count > 0)
                        {
                            for (int j = 0; j < detailedAccountPeriodDto.Charges.TuitionBySectionGroups.Count; j++)
                            {
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Amount, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Amount);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].BillingCredits, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].BillingCredits);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Ceus, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Ceus);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Classroom, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Classroom);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Credits, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Credits);
                                CollectionAssert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Days, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Days);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Description, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Description);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].EndTime, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].EndTime);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Id, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Id);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Instructor, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Instructor);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].StartTime, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].StartTime);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].Status, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].Status);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionBySectionGroups[i].SectionCharges[j].TermId, detailedAccountPeriodDto.Charges.TuitionBySectionGroups[i].SectionCharges[j].TermId);                           
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Charges_TuitionByTotalGroups()
        {
            if (detailedAccountPeriodEntity.Charges != null)
            {
                if (detailedAccountPeriodEntity.Charges.TuitionByTotalGroups != null && detailedAccountPeriodEntity.Charges.TuitionByTotalGroups.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodDto.Charges.TuitionByTotalGroups.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].DisplayOrder, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].DisplayOrder);
                        Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].Name, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].Name);
                        if (detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges != null && detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges.Count > 0)
                        {
                            for (int j = 0; j < detailedAccountPeriodDto.Charges.TuitionByTotalGroups.Count; j++)
                            {
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Amount, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Amount);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].BillingCredits, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].BillingCredits);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Ceus, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Ceus);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Classroom, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Classroom);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Credits, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Credits);
                                CollectionAssert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Days, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Days);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Description, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Description);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].EndTime, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].EndTime);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Id, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Id);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Instructor, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Instructor);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].StartTime, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].StartTime);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].Status, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].Status);
                                Assert.AreEqual(detailedAccountPeriodEntity.Charges.TuitionByTotalGroups[i].TotalCharges[j].TermId, detailedAccountPeriodDto.Charges.TuitionByTotalGroups[i].TotalCharges[j].TermId);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Deposits()
        {
            if (detailedAccountPeriodEntity.Deposits != null)
            {
                if (detailedAccountPeriodEntity.Deposits.Deposits != null && detailedAccountPeriodEntity.Deposits.Deposits.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodEntity.Deposits.Deposits.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].Amount, detailedAccountPeriodDto.Deposits.Deposits[i].Amount);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].Date, detailedAccountPeriodDto.Deposits.Deposits[i].Date);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].Description, detailedAccountPeriodDto.Deposits.Deposits[i].Description);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].Id, detailedAccountPeriodDto.Deposits.Deposits[i].Id);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].OtherAmount, detailedAccountPeriodDto.Deposits.Deposits[i].OtherAmount);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].PaidAmount, detailedAccountPeriodDto.Deposits.Deposits[i].PaidAmount);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].RefundAmount, detailedAccountPeriodDto.Deposits.Deposits[i].RefundAmount);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].RemainingAmount, detailedAccountPeriodDto.Deposits.Deposits[i].RemainingAmount);
                        Assert.AreEqual(detailedAccountPeriodEntity.Deposits.Deposits[i].TermId, detailedAccountPeriodDto.Deposits.Deposits[i].TermId);                 
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Description()
        {
            Assert.AreEqual(detailedAccountPeriodEntity.Description, detailedAccountPeriodDto.Description);
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_DueDate()
        {
            Assert.AreEqual(detailedAccountPeriodEntity.DueDate, detailedAccountPeriodDto.DueDate);
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_EndDate()
        {
            Assert.AreEqual(detailedAccountPeriodEntity.EndDate, detailedAccountPeriodDto.EndDate);
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_FinancialAid()
        {
            if (detailedAccountPeriodEntity.FinancialAid != null)
            {
                if (detailedAccountPeriodEntity.FinancialAid.AnticipatedAid != null && detailedAccountPeriodEntity.FinancialAid.AnticipatedAid.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodEntity.FinancialAid.AnticipatedAid.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardAmount, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].AwardAmount);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardDescription, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].AwardDescription);
                        if (detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardTerms != null && detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardTerms.Count > 0)
                        {
                            for (int j = 0; j < detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardTerms.Count; j++)
                            {
                                Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardTerms[j].AnticipatedAmount, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].AwardTerms[j].AnticipatedAmount);
                                Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardTerms[j].AwardTerm, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].AwardTerms[j].AwardTerm);
                                Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].AwardTerms[j].DisbursedAmount, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].AwardTerms[j].DisbursedAmount);                  
                            }
                        }
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].Comments, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].Comments);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].IneligibleAmount, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].IneligibleAmount);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].LoanFee, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].LoanFee);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].OtherTermAmount, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].OtherTermAmount);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].PeriodAward, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].PeriodAward);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].StudentStatementAnticipatedAmounts, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].StudentStatementAnticipatedAmounts);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].StudentStatementAwardTerms, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].StudentStatementAwardTerms);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.AnticipatedAid[i].StudentStatementDisbursedAmounts, detailedAccountPeriodDto.FinancialAid.AnticipatedAid[i].StudentStatementDisbursedAmounts);
                    }
                }

                if (detailedAccountPeriodEntity.FinancialAid.DisbursedAid != null && detailedAccountPeriodEntity.FinancialAid.DisbursedAid.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodEntity.FinancialAid.DisbursedAid.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.DisbursedAid[i].Amount, detailedAccountPeriodDto.FinancialAid.DisbursedAid[i].Amount);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.DisbursedAid[i].Date, detailedAccountPeriodDto.FinancialAid.DisbursedAid[i].Date);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.DisbursedAid[i].Description, detailedAccountPeriodDto.FinancialAid.DisbursedAid[i].Description);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.DisbursedAid[i].Id, detailedAccountPeriodDto.FinancialAid.DisbursedAid[i].Id);
                        Assert.AreEqual(detailedAccountPeriodEntity.FinancialAid.DisbursedAid[i].TermId, detailedAccountPeriodDto.FinancialAid.DisbursedAid[i].TermId);
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_PaymentPlans()
        {
            if (detailedAccountPeriodEntity.PaymentPlans != null)
            {
                if (detailedAccountPeriodEntity.PaymentPlans.PaymentPlans != null && detailedAccountPeriodEntity.PaymentPlans.PaymentPlans.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodEntity.PaymentPlans.PaymentPlans.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].Amount, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].Amount);
                        Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].CurrentBalance, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].CurrentBalance);
                        Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].Description, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].Description);
                        Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].Id, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].Id);
                        Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].OriginalAmount, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].OriginalAmount);
                        if (detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules != null && detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules.Count > 0)
                        {
                            for (int j = 0; j < detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules.Count; j++)
                            {
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Amount, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Amount);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].AmountPaid, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].AmountPaid);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Date, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Date);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].DatePaid, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].DatePaid);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Description, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Description);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Id, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].Id);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].LateCharge, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].LateCharge);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].NetAmountDue, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].NetAmountDue);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].SetupCharge, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].SetupCharge);
                                Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].TermId, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].PaymentPlanSchedules[j].TermId);                           
                            }
                        }
                        Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].TermId, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].TermId);
                        Assert.AreEqual(detailedAccountPeriodEntity.PaymentPlans.PaymentPlans[i].Type, detailedAccountPeriodDto.PaymentPlans.PaymentPlans[i].Type);
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Refunds()
        {
            if (detailedAccountPeriodEntity.Refunds != null)
            {
                if (detailedAccountPeriodEntity.Refunds.Refunds != null && detailedAccountPeriodEntity.Refunds.Refunds.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodEntity.Refunds.Refunds.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].Amount, detailedAccountPeriodDto.Refunds.Refunds[i].Amount);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].Date, detailedAccountPeriodDto.Refunds.Refunds[i].Date);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].Description, detailedAccountPeriodDto.Refunds.Refunds[i].Description);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].Id, detailedAccountPeriodDto.Refunds.Refunds[i].Id);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].Method, detailedAccountPeriodDto.Refunds.Refunds[i].Method);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].TermId, detailedAccountPeriodDto.Refunds.Refunds[i].TermId);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].CheckDate, detailedAccountPeriodDto.Refunds.Refunds[i].CheckDate);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].CheckNumber, detailedAccountPeriodDto.Refunds.Refunds[i].CheckNumber);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].CreditCardLastFourDigits, detailedAccountPeriodDto.Refunds.Refunds[i].CreditCardLastFourDigits);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].Status.ToString(), detailedAccountPeriodDto.Refunds.Refunds[i].Status.ToString());
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].StatusDate, detailedAccountPeriodDto.Refunds.Refunds[i].StatusDate);
                        Assert.AreEqual(detailedAccountPeriodEntity.Refunds.Refunds[i].TransactionNumber, detailedAccountPeriodDto.Refunds.Refunds[i].TransactionNumber);
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_Sponsorships()
        {
            if (detailedAccountPeriodEntity.Sponsorships != null)
            {
                if (detailedAccountPeriodEntity.Sponsorships.SponsorItems != null && detailedAccountPeriodEntity.Sponsorships.SponsorItems.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodEntity.Sponsorships.SponsorItems.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.Sponsorships.SponsorItems[i].Amount, detailedAccountPeriodDto.Sponsorships.SponsorItems[i].Amount);
                        Assert.AreEqual(detailedAccountPeriodEntity.Sponsorships.SponsorItems[i].Date, detailedAccountPeriodDto.Sponsorships.SponsorItems[i].Date);
                        Assert.AreEqual(detailedAccountPeriodEntity.Sponsorships.SponsorItems[i].Description, detailedAccountPeriodDto.Sponsorships.SponsorItems[i].Description);
                        Assert.AreEqual(detailedAccountPeriodEntity.Sponsorships.SponsorItems[i].Id, detailedAccountPeriodDto.Sponsorships.SponsorItems[i].Id);
                        Assert.AreEqual(detailedAccountPeriodEntity.Sponsorships.SponsorItems[i].Sponsorship, detailedAccountPeriodDto.Sponsorships.SponsorItems[i].Sponsorship);
                        Assert.AreEqual(detailedAccountPeriodEntity.Sponsorships.SponsorItems[i].TermId, detailedAccountPeriodDto.Sponsorships.SponsorItems[i].TermId);
                    }
                }
            }
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_StartDate()
        {
            Assert.AreEqual(detailedAccountPeriodEntity.StartDate, detailedAccountPeriodDto.StartDate);
        }

        [TestMethod]
        public void DetailedAccountPeriodEntityAdapterTests_StudentPayments()
        {
            if (detailedAccountPeriodEntity.StudentPayments != null)
            {
                if (detailedAccountPeriodEntity.StudentPayments.StudentPayments != null && detailedAccountPeriodEntity.StudentPayments.StudentPayments.Count > 0)
                {
                    for (int i = 0; i < detailedAccountPeriodEntity.StudentPayments.StudentPayments.Count; i++)
                    {
                        Assert.AreEqual(detailedAccountPeriodEntity.StudentPayments.StudentPayments[i].Amount, detailedAccountPeriodDto.StudentPayments.StudentPayments[i].Amount);
                        Assert.AreEqual(detailedAccountPeriodEntity.StudentPayments.StudentPayments[i].Date, detailedAccountPeriodDto.StudentPayments.StudentPayments[i].Date);
                        Assert.AreEqual(detailedAccountPeriodEntity.StudentPayments.StudentPayments[i].Description, detailedAccountPeriodDto.StudentPayments.StudentPayments[i].Description);
                        Assert.AreEqual(detailedAccountPeriodEntity.StudentPayments.StudentPayments[i].Id, detailedAccountPeriodDto.StudentPayments.StudentPayments[i].Id);
                        Assert.AreEqual(detailedAccountPeriodEntity.StudentPayments.StudentPayments[i].Method, detailedAccountPeriodDto.StudentPayments.StudentPayments[i].Method);
                        Assert.AreEqual(detailedAccountPeriodEntity.StudentPayments.StudentPayments[i].ReferenceNumber, detailedAccountPeriodDto.StudentPayments.StudentPayments[i].ReferenceNumber);
                        Assert.AreEqual(detailedAccountPeriodEntity.StudentPayments.StudentPayments[i].TermId, detailedAccountPeriodDto.StudentPayments.StudentPayments[i].TermId);
                    }
                }
            }
        }
    }
}