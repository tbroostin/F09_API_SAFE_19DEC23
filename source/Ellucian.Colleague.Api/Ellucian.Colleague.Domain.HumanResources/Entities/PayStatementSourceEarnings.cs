/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO mcd: This class is no longer used. It can be deleted.
namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Pay Statement Source Earning
    /// </summary>
    [Serializable]
    public class PayStatementSourceEarnings
    {
        public string EarningsTypeId { get; private set; }
        public string EarningsTypeDescription { get; private set; }

        /// <summary>
        /// Units worked for the give Pay Statement. For Salaried employees, this will be null
        /// </summary>
        public decimal? UnitsWorked { get; private set; }
        public decimal Rate { get; private set; }
        public decimal PeriodPaymentAmount { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="earningsTypeId"></param>
        /// <param name="earningsTypeDescription"></param>
        /// <param name="unitsWorked"></param>
        /// <param name="rate"></param>
        /// <param name="periodPaymentAmount"></param>
        public PayStatementSourceEarnings(string earningsTypeId, string earningsTypeDescription, decimal? unitsWorked, decimal rate, decimal periodPaymentAmount)
        {
            if(string.IsNullOrWhiteSpace(earningsTypeId))
            {
                throw new ArgumentNullException("earningsTypeId");
            }
            if (string.IsNullOrWhiteSpace(earningsTypeDescription))
            {
                throw new ArgumentNullException("earningsTypeDescription");
            }

            EarningsTypeId = earningsTypeId;
            EarningsTypeDescription = earningsTypeDescription;
            UnitsWorked = unitsWorked;
            Rate = rate;
            PeriodPaymentAmount = periodPaymentAmount;
        }

        //public void AddHoursUnits(decimal? hoursUnits)
        //{
        //    UnitsWorked += hoursUnits;
        //}

        //public void AddPeriodAmount(decimal amount)
        //{
        //    PeriodPaymentAmount += amount;
        //}

        //public override bool Equals(object obj)
        //{
        //    if (obj == null || obj.GetType() != GetType())
        //    {
        //        return false;
        //    }
        //    var comparison = (PayStatementEarnings)obj;

        //    return EarningsTypeId == comparison.EarningsTypeId && Rate == comparison.Rate;
        //}
        //public override int GetHashCode()
        //{
        //    return EarningsTypeId.GetHashCode() ^ Rate.GetHashCode();
        //}
    }
}
