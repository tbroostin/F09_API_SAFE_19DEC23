/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// This class contains the objects that the Pay Statement Report needs in context
    /// to build itself. It holds "joined" objects - a Pay Statement Source joined with a PayrollRegisterEntry
    /// </summary>
    [Serializable]
    public class PayStatementReportDataContext
    {
        /// <summary>
        /// The PayStatementSource object for which to build the report.
        /// </summary>
        public PayStatementSourceData sourceData { get; private set; }


        /// <summary>
        /// The associated PayrollRegisterEntry containing most of the data used to build the report.
        /// </summary>
        public PayrollRegisterEntry payrollRegisterEntry { get; private set; }

        /// <summary>
        /// The benefits and deductions that are active for this person during the time period of this pay statement.
        /// </summary>
        public IEnumerable<PersonBenefitDeduction> personBenefitDeductions { get; private set; }

        /// <summary>
        /// The object that describes the status of the person's employment during the time period of the pay statement
        /// </summary>
        public PersonEmploymentStatus personEmploymentStatus { get; private set; }


        public PayStatementReportDataContext(PayStatementSourceData sourceData, 
            PayrollRegisterEntry registerEntry,
            IEnumerable<PersonBenefitDeduction> personBenefitDeductions,
            IEnumerable<PersonEmploymentStatus> personEmploymentStatuses)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException("sourceData");
            }
            if (registerEntry == null)
            {
                throw new ArgumentNullException("registerEntry");
            }
            if (personBenefitDeductions == null)
            {
                throw new ArgumentNullException("personBenefitDeductions");
            }
            if (sourceData.ReferenceKey != registerEntry.ReferenceKey)
            {
                throw new ArgumentException("sourceData and registerEntry must have the same ReferenceKey");
            }

            this.sourceData = sourceData;
            payrollRegisterEntry = registerEntry;

            this.personBenefitDeductions = personBenefitDeductions.Where(pbd =>
                pbd.EnrollmentDate <= sourceData.PeriodEndDate &&
                (!pbd.LastPayDate.HasValue || pbd.LastPayDate.Value >= sourceData.PeriodEndDate));

            personEmploymentStatus = personEmploymentStatuses.FirstOrDefault(status =>
                status.StartDate <= registerEntry.PayPeriodEndDate &&
                (!status.EndDate.HasValue || status.EndDate.Value >= registerEntry.PayPeriodEndDate));
        }
    }
}
