/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Adapters
{
    [TestClass]
    public class EmployeeLeavePlanEntityToDtoAdapterTests
    {
        #region Setup
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public EmployeeLeavePlanEntityToDtoAdapter adapter;

        public Dtos.HumanResources.EmployeeLeavePlan EmployeeLeavePlanDTO;
        public EmployeeLeavePlan EmployeeLeavePlanEntity;
        public FunctionEqualityComparer<Dtos.HumanResources.EmployeeLeaveTransaction> employeeLeaveTransactionDtoComparer;

        #region Empolyee Leave Plan
        public string Id { get; private set; }
        public string EmployeeId { get; private set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string LeavePlanId { get; private set; }
        public string LeavePlanDescription { get; private set; }
        public DateTime LeavePlanStartDate { get; private set; }
        public DateTime? LeavePlanEndDate { get; private set; }
        public DateTime LeaveAllowedDate { get; set; }
        public decimal PriorPeriodLeaveBalance { get; private set; }
        public bool AllowNegativeBalance { get; private set; }
        public LeaveTypeCategory LeavePlanTypeCategory { get; private set; }
        public string EarningsTypeId { get; private set; }
        public string EarningsTypeDescription { get; private set; }
        public int PlanYearStartMonth { get; private set; }
        public int PlanYearStartDay { get; private set; }
        public Decimal? AccrualRate { get; private set; }
        public Decimal? AccrualLimit { get; private set; }
        public Decimal? AccrualMaxCarryOver { get; private set; }
        public decimal? AccrualMaxRollOver { get; private set; }
        public string AccrualMethod { get; private set; }
        public bool IsLeaveReportingPlan { get; private set; }
        public IEnumerable<string> EarningTypeIDList { get; private set; }
        public bool IsPlanYearStartDateDefined { get; private set; }

        public DateTime? LatestCarryoverDate { get; private set; }
        #endregion

        public void EmployeeLeavePlanInitialize()
        {
            Id = "406";
            EmployeeId = "0004699";
            StartDate = new DateTime(2016, 1, 1);
            EndDate = null;
            LeavePlanId = "VACH";
            LeavePlanDescription = "Vacation - Hourly";
            LeavePlanStartDate = new DateTime(1964, 1, 1);
            LeavePlanEndDate = null;
            LeaveAllowedDate = new DateTime(2016, 1, 1);
            AllowNegativeBalance = false;
            IsLeaveReportingPlan = true;
            LeavePlanTypeCategory = LeaveTypeCategory.Vacation;
            EarningsTypeId = "VHM";
            EarningsTypeDescription = "Vacation Hourly Monthly";
            PriorPeriodLeaveBalance = 194.54m;
            AccrualRate = 0.0769m;
            AccrualLimit = 280.00m;
            AccrualMaxCarryOver = 160.00m;
            AccrualMaxRollOver = 10.00m;
            AccrualMethod = "H";
            var earningTypeIDList = new List<string>(); earningTypeIDList.Add("VHM");
            EarningTypeIDList = earningTypeIDList;
            IsPlanYearStartDateDefined = true;
            PlanYearStartMonth = 1;
            PlanYearStartDay = 1;
            LatestCarryoverDate = new DateTime(2022, 1, 1);
            EmployeeLeavePlanEntity = new EmployeeLeavePlan(Id, EmployeeId, StartDate, EndDate, LeavePlanId, LeavePlanDescription, LeavePlanStartDate, LeavePlanEndDate,
                                LeavePlanTypeCategory, EarningsTypeId, EarningsTypeDescription, LeaveAllowedDate, PriorPeriodLeaveBalance, PlanYearStartMonth, PlanYearStartDay,
                                IsLeaveReportingPlan, EarningTypeIDList, AccrualRate, AccrualLimit, AccrualMaxCarryOver, AccrualMaxRollOver, AccrualMethod, IsPlanYearStartDateDefined, LatestCarryoverDate, AllowNegativeBalance);
            employeeLeaveTransactionDtoComparer = this.EmployeeLeaveTransactionDtoComparer();
        }

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.HumanResources.EmployeeLeaveTransaction, Domain.HumanResources.Entities.EmployeeLeaveTransaction>())
                .Returns(() => new AutoMapperAdapter<Dtos.HumanResources.EmployeeLeaveTransaction, Domain.HumanResources.Entities.EmployeeLeaveTransaction>(adapterRegistryMock.Object, loggerMock.Object));

            EmployeeLeavePlanInitialize();

            adapter = new EmployeeLeavePlanEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            EmployeeLeavePlanDTO = adapter.MapToType(EmployeeLeavePlanEntity);
        }
        #endregion

        #region Tests
        [TestMethod]
        public void EmployeeLeavePlanAdpater_BasicPropertiesTests()
        {
            Assert.AreEqual(EmployeeLeavePlanEntity.Id, EmployeeLeavePlanDTO.Id);
            Assert.AreEqual(EmployeeLeavePlanEntity.IsLeaveReportingPlan, EmployeeLeavePlanDTO.IsLeaveReportingPlan);
            Assert.AreEqual(EmployeeLeavePlanEntity.IsPlanYearStartDateDefined, EmployeeLeavePlanDTO.IsPlanYearStartDateDefined);
            Assert.AreEqual(EmployeeLeavePlanEntity.LeaveAllowedDate, EmployeeLeavePlanDTO.LeaveAllowedDate);
            Assert.AreEqual(EmployeeLeavePlanEntity.LeavePlanDescription, EmployeeLeavePlanDTO.LeavePlanDescription);
            Assert.AreEqual(EmployeeLeavePlanEntity.AccrualLimit, EmployeeLeavePlanDTO.AccrualLimit);
            Assert.AreEqual(EmployeeLeavePlanEntity.AccrualMaxCarryOver, EmployeeLeavePlanDTO.AccrualMaxCarryOver);
            Assert.AreEqual(EmployeeLeavePlanEntity.AccrualMaxRollOver, EmployeeLeavePlanDTO.AccrualMaxRollOver);
            Assert.AreEqual(EmployeeLeavePlanEntity.AccrualMethod, EmployeeLeavePlanDTO.AccrualMethod);
            Assert.AreEqual(EmployeeLeavePlanEntity.AccrualRate, EmployeeLeavePlanDTO.AccrualRate);
            Assert.AreEqual(EmployeeLeavePlanEntity.AllowNegativeBalance, EmployeeLeavePlanDTO.AllowNegativeBalance);
            Assert.AreEqual(EmployeeLeavePlanEntity.EarningsTypeDescription, EmployeeLeavePlanDTO.EarningsTypeDescription);
            Assert.AreEqual(EmployeeLeavePlanEntity.EarningsTypeId, EmployeeLeavePlanDTO.EarningsTypeId);
            CollectionAssert.AreEqual(EmployeeLeavePlanEntity.EarningTypeIDList.ToList(), EmployeeLeavePlanDTO.EarningTypeIDList.ToList());
            Assert.AreEqual(EmployeeLeavePlanEntity.EmployeeId, EmployeeLeavePlanDTO.EmployeeId);
            Assert.AreEqual(EmployeeLeavePlanEntity.EndDate, EmployeeLeavePlanDTO.EndDate);
            Assert.AreEqual(EmployeeLeavePlanEntity.LeavePlanEndDate, EmployeeLeavePlanDTO.LeavePlanEndDate);
            Assert.AreEqual(EmployeeLeavePlanEntity.LeavePlanId, EmployeeLeavePlanDTO.LeavePlanId);
            Assert.AreEqual(EmployeeLeavePlanEntity.LeavePlanStartDate, EmployeeLeavePlanDTO.LeavePlanStartDate);
            Assert.AreEqual(EmployeeLeavePlanEntity.LeavePlanTypeCategory.ToString(), EmployeeLeavePlanDTO.LeavePlanTypeCategory.ToString());
            Assert.AreEqual(EmployeeLeavePlanEntity.PriorPeriodLeaveBalance, EmployeeLeavePlanDTO.PriorPeriodLeaveBalance);
            CollectionAssert.AreEqual(EmployeeLeavePlanEntity.EmployeeLeaveTransactions.ToList(), EmployeeLeavePlanDTO.EmployeeLeaveTransactions.ToList(), employeeLeaveTransactionDtoComparer);
        }

        [TestMethod]
        public void EmployeeLeavePlanAdpater_EmployeeLeaveTransactionsTests()
        {
            Assert.IsNotNull(EmployeeLeavePlanDTO.EmployeeLeaveTransactions);
            Assert.AreEqual(EmployeeLeavePlanEntity.EmployeeLeaveTransactions.Count(), EmployeeLeavePlanDTO.EmployeeLeaveTransactions.Count());
            CollectionAssert.AreEqual(EmployeeLeavePlanEntity.EmployeeLeaveTransactions.ToList(), EmployeeLeavePlanDTO.EmployeeLeaveTransactions.ToList(), employeeLeaveTransactionDtoComparer);
        }
        #endregion

        #region Helpers
        private FunctionEqualityComparer<Dtos.HumanResources.EmployeeLeaveTransaction> EmployeeLeaveTransactionDtoComparer()
        {
            return new FunctionEqualityComparer<Dtos.HumanResources.EmployeeLeaveTransaction>(
                (lr1, lr2) =>
                    lr1.Id == lr2.Id &&
                    lr1.Date == lr2.Date &&
                    lr1.TransactionHours == lr2.TransactionHours &&
                    lr1.Type == lr2.Type,
                    (lr) => lr.Id.GetHashCode()
                );
        }
        #endregion
    }
}
