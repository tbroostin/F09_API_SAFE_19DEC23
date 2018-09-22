/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// The reference data utility provides helpers for the PayStatement Report to get
    /// reference data objects using pointers provided in the Payroll Register.
    /// </summary>
    [Serializable]
    public class PayStatementReferenceDataUtility
    {

        private IEnumerable<BenefitDeductionType> benefitDeductionTypes;
        private IEnumerable<TaxCode> taxCodes;
        private IEnumerable<EarningsType> earningsTypes;
        private IEnumerable<EarningsDifferential> earningsDifferentials;
        private IEnumerable<LeaveType> leaveTypes;
        private IDictionary<string, Position> positions;
        public PayStatementConfiguration Configuration { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="earningsTypes"></param>
        /// <param name="earningsDifferentials"></param>
        /// <param name="taxCodes"></param>
        /// <param name="benefitDeductionTypes"></param>
        public PayStatementReferenceDataUtility(IEnumerable<EarningsType> earningsTypes,
            IEnumerable<EarningsDifferential> earningsDifferentials,
            IEnumerable<TaxCode> taxCodes,
            IEnumerable<BenefitDeductionType> benefitDeductionTypes,
            IEnumerable<LeaveType> leaveTypes,
            IEnumerable<Position> positions,
            PayStatementConfiguration configuration)
        {
            this.earningsTypes = earningsTypes;
            this.earningsDifferentials = earningsDifferentials;
            this.taxCodes = taxCodes;
            this.benefitDeductionTypes = benefitDeductionTypes;
            this.leaveTypes = leaveTypes;
            this.positions = positions.ToDictionary(p => p.Id);
            this.Configuration = configuration;
        }

        public EarningsType GetEarningsTypeAsStipend(string earningsTypeId)
        {
            var earnType = GetEarningsType(earningsTypeId);
            if (earnType == null)
            {
                return null;
            }

            var stipendDescription = string.Format("{0} - Stipend", earnType.Description);
            var earnTypeAsStipend = new EarningsType(earnType.Id, stipendDescription, earnType.IsActive, earnType.Category, earnType.Method, earnType.Factor, earnType.EarningsLeaveType);
            return earnTypeAsStipend;
        }

        public EarningsType GetEarningsType(string earningsTypeId)
        {
            return earningsTypes.FirstOrDefault(earn => earningsTypeId == earn.Id);
        }

        public LeaveTypeCategory GetEarnTypeLeaveTimeType(string earningsTypeId)
        {
            var earnType = earningsTypes.FirstOrDefault(earn => earn.Id == earningsTypeId);

            return GetLeaveTimeType(earnType.EarningsLeaveType);
        }

        public LeaveTypeCategory GetLeaveTypeLeaveTimeType(string leaveTypeId)
        {
            var leaveType = leaveTypes.FirstOrDefault(leave => leave.Code == leaveTypeId);
            return GetLeaveTimeType(leaveType);
        }

        public LeaveTypeCategory GetLeaveTimeType(LeaveType leaveType)
        {
            return leaveType == null ? LeaveTypeCategory.None : leaveType.TimeType;
        }

        public EarningsDifferential GetEarningsDifferential(string earningsDifferentialId)
        {
            return earningsDifferentials.FirstOrDefault(diff => earningsDifferentialId == diff.Code);
        }

        /// <summary>
        /// Get the TaxCode object for the given taxCodeId
        /// </summary>
        /// <param name="taxCode"></param>
        /// <returns></returns>
        public TaxCode GetTaxCode(string taxCode)
        {
            return taxCodes.FirstOrDefault(tax => tax.Code == taxCode);
        }

        /// <summary>
        /// Get a list of Tax Codes for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<TaxCode> GetTaxCodesForType(TaxCodeType type)
        {
            return taxCodes.Where(tax => tax.Type == type);
        }

        /// <summary>
        /// Get the BenefitDeductionType object for the given BenefitDeductionID
        /// </summary>
        /// <param name="benefitDeductionId"></param>
        /// <returns></returns>
        public BenefitDeductionType GetBenefitDeductionType(string benefitDeductionId)
        {
            return benefitDeductionTypes.FirstOrDefault(bd => bd.Id == benefitDeductionId);
        }

        /// <summary>
        /// Get the LeaveType for the given code
        /// </summary>
        /// <param name="leaveCode"></param>
        /// <returns></returns>
        public LeaveType GetLeaveType(string leaveCode)
        {
            return leaveTypes.FirstOrDefault(l => l.Code == leaveCode);
        }

        public Position GetPosition(string positionId)
        {
            return positions.ContainsKey(positionId) ? positions[positionId] : null;
        }

    }
}
