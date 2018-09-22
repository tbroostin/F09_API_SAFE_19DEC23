/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Services
{
    /// <summary>
    /// Interface to a PayStatementDomainService
    /// </summary>
    public interface IPayStatementDomainService
    {

        PayStatementReport BuildPayStatementReport(PayStatementSourceData sourceData);

        void SetContext(IEnumerable<PayStatementSourceData> sourceDatas, 
            IEnumerable<PayrollRegisterEntry> payrollRegister, 
            IEnumerable<PersonBenefitDeduction> personBenefitDeductions,
            IEnumerable<PersonEmploymentStatus> personEmploymentStatuses,
            PayStatementReferenceDataUtility dataUtility);
        
    }
}
