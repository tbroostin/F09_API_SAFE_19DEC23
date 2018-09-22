/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Describes the specific earnings a person earned during a pay period for which they were paid.
    /// 
    /// </summary>
    [Serializable]
    public class PayrollRegisterEarningsEntry
    {
        /// <summary>
        /// The Id of the EarningsType
        /// </summary>
        public string EarningsTypeId { get; private set; }

        /// <summary>
        /// The total earnings amount the employee earned during the period. 
        /// TotalPeriodEarningsAmount = BasePeriodEarningsAmount + EarningsFactorPeriodAmount + DifferentialPeriodEarningsAmount
        /// </summary>
        public decimal TotalPeriodEarningsAmount { get; private set; }


        /// <summary>
        /// The standard number of units worked during the period.
        /// </summary>
        public decimal? StandardUnitsWorked { get; set; }

        /// <summary>
        /// The standard rate at which the employee is paid during the period.
        /// </summary>
        public decimal? StandardRate { get; private set; }

        /// <summary>
        /// The Base Period Earnings Amount is the amount that was calculated from base rate on the 
        /// earnings type. This amount is a part of the StandardPeriodEarningsAmount.
        /// </summary>
        public decimal BasePeriodEarningsAmount { get; private set; }

        /// <summary>
        /// The Earnings Factor Period Amount is the amount that was calculated from the earnings factor
        /// on the earnings type. Generally, Overtime earnings have a value in this attribute.
        /// </summary>
        public decimal EarningsFactorPeriodAmount { get; private set; }

        /// <summary>
        /// The Id of the EarningsDifferential. If null, use the Standard earnings properties. Otherwise, use
        /// the Differential Earnings properties.
        /// </summary>
        public string EarningsDifferentialId { get; private set; }

        /// <summary>
        /// The amount earned by the employee during the period because they worked a special shift and recieved 
        /// increased (different from standard) earnings. 
        /// </summary>
        public decimal DifferentialPeriodEarningsAmount { get; private set; }

        /// <summary>
        /// The rate at which the employee was paid for the differential shift. 
        /// </summary>
        public decimal? DifferentialRate { get; private set; }

        /// <summary>
        /// The number of units worked during the differential shift. 
        /// </summary>
        public decimal? DifferentialUnitsWorked { get; private set; }

        /// <summary>
        /// Determines whether this earnings has differential earnings or not.
        /// </summary>
        public bool HasDifferentialEarnings
        {
            get
            {
                return !string.IsNullOrEmpty(EarningsDifferentialId);
            }
        }
     

        /// <summary>
        /// Indicates whether earnings are hourly or salary
        /// </summary>
        public HourlySalaryIndicator HourlySalaryIndication { get; private set; }


        /// <summary>
        /// Id of an associated stipend
        /// This should be null if there is no associated stipend
        /// </summary>
        public string StipendId { get; private set; }

        public bool IsStipendEarnings
        {
            get
            {
                return !string.IsNullOrEmpty(StipendId);
            }
        }


        /// <summary>
        /// Constructor builds a standard earnings entry for hourly earnings. 
        /// To set the differential, use the SetEarningsDifferential method.
        /// </summary>
        /// <param name="earningsTypeId"></param>
        /// <param name="totalPeriodEarningsAmount"></param>
        /// <param name="unitsWorked"></param>
        /// <param name="rate"></param>
        /// <param name="hourlySalaryFlag"></param>
        public PayrollRegisterEarningsEntry(string earningsTypeId, 
            decimal totalPeriodEarningsAmount, 
            decimal basePeriodEarningsAmount,
            decimal earningsFactorPeriodAmount,
            decimal? unitsWorked, 
            decimal? rate, 
            HourlySalaryIndicator hourlySalaryIndication)
        {
            if (string.IsNullOrEmpty(earningsTypeId))
            {
                throw new ArgumentNullException("earningsTypeId");
            }
            EarningsTypeId = earningsTypeId;
            TotalPeriodEarningsAmount = totalPeriodEarningsAmount;
            BasePeriodEarningsAmount = basePeriodEarningsAmount;
            EarningsFactorPeriodAmount = earningsFactorPeriodAmount;
            StandardUnitsWorked = unitsWorked;
            StandardRate = rate;
            HourlySalaryIndication = hourlySalaryIndication;
        }

        /// <summary>
        /// Constructor builds a stipend earnings entry.
        /// </summary>
        /// <param name="earningsTypeId"></param>
        /// <param name="stipendId"></param>
        /// <param name="totalPeriodEarningsAmount"></param>
        /// <param name="unitsWorked"></param>
        /// <param name="rate"></param>
        public PayrollRegisterEarningsEntry(string earningsTypeId, 
            string stipendId, 
            decimal totalPeriodEarningsAmount, 
            decimal basePeriodEarningsAmount,
            decimal earningsFactorPeriodAmount,
            decimal? unitsWorked, 
            decimal? rate, 
            HourlySalaryIndicator hourlySalaryIndication)
        {
            if (string.IsNullOrEmpty(earningsTypeId))
            {
                throw new ArgumentNullException("earningsTypeId");
            }
            EarningsTypeId = earningsTypeId;
            TotalPeriodEarningsAmount = totalPeriodEarningsAmount;
            BasePeriodEarningsAmount = basePeriodEarningsAmount;
            EarningsFactorPeriodAmount = earningsFactorPeriodAmount;
            StandardUnitsWorked = unitsWorked;
            StandardRate = rate;
            StipendId = stipendId;
            HourlySalaryIndication = hourlySalaryIndication;
        }

        /// <summary>
        /// If this earnings entry receives special treatment due to the employee working a special shift, use this method
        /// to set the entry's earnings differential properties.
        /// </summary>
        /// <param name="earningsDifferentialId"></param>
        /// <param name="earningsAmount"></param>
        /// <param name="unitsWorked"></param>
        /// <param name="rate"></param>
        public void SetEarningsDifferential(string earningsDifferentialId, 
            decimal earningsAmount, 
            decimal? unitsWorked, 
            decimal rate)
        {
            if (string.IsNullOrEmpty(earningsDifferentialId))
            {
                throw new ArgumentNullException("earningsDifferentialId");
            }

            EarningsDifferentialId = earningsDifferentialId;
            DifferentialPeriodEarningsAmount = earningsAmount;
            DifferentialRate = rate;
            DifferentialUnitsWorked = unitsWorked;
        }





    }
}
