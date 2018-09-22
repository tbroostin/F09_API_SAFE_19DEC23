// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Services
{
    /// <summary>
    /// Pay Statement Report Domain Service
    /// </summary>
    [RegisterType]
    public class PayStatementDomainService : IPayStatementDomainService
    {
        public PayStatementDomainService()
        {

        }



        private Dictionary<string, Dictionary<int, List<PayStatementReportDataContext>>> Context;

        private PayStatementReferenceDataUtility dataUtility;

        public void SetContext(IEnumerable<PayStatementSourceData> sourceDatas,
            IEnumerable<PayrollRegisterEntry> payrollRegister,
            IEnumerable<PersonBenefitDeduction> personBenefitDeductions,
            IEnumerable<PersonEmploymentStatus> personEmploymentStatuses,
            PayStatementReferenceDataUtility dataUtility)
        {
            if (sourceDatas == null)
            {
                throw new ArgumentNullException("sourceDatas");
            }
            if (payrollRegister == null)
            {
                throw new ArgumentNullException("payrollRegister");
            }
            if (personBenefitDeductions == null)
            {
                throw new ArgumentNullException("personBenefitDeductions");
            }
            if (personEmploymentStatuses == null)
            {
                throw new ArgumentNullException("personEmploymentStatuses");
            }
            if (dataUtility == null)
            {
                throw new ArgumentNullException("dataUtility");
            }

            Context = new Dictionary<string, Dictionary<int, List<PayStatementReportDataContext>>>();

            this.dataUtility = dataUtility;

            var payrollRegisterDictByPayStatementReferenceId = new Dictionary<string, PayrollRegisterEntry>();
            foreach (var payrollRegisterEntry in payrollRegister)
            {
                if (!payrollRegisterDictByPayStatementReferenceId.ContainsKey(payrollRegisterEntry.ReferenceKey))
                {
                    payrollRegisterDictByPayStatementReferenceId.Add(payrollRegisterEntry.ReferenceKey, payrollRegisterEntry);
                }
                else
                {
                    if (payrollRegisterDictByPayStatementReferenceId[payrollRegisterEntry.ReferenceKey].SequenceNumber < payrollRegisterEntry.SequenceNumber)
                    {
                        payrollRegisterDictByPayStatementReferenceId[payrollRegisterEntry.ReferenceKey] = payrollRegisterEntry;
                    }
                }
            }

            var personBenefitDeductionLookup = personBenefitDeductions.ToLookup(p => p.PersonId);
            var personStatusLookup = personEmploymentStatuses.ToLookup(p => p.PersonId);

            foreach (var data in sourceDatas)
            {
                if (payrollRegisterDictByPayStatementReferenceId.ContainsKey(data.ReferenceKey))
                {
                    if (!Context.ContainsKey(data.EmployeeId))
                    {
                        Context.Add(
                            data.EmployeeId,
                            new Dictionary<int, List<PayStatementReportDataContext>>
                            {
                                {
                                    data.PayDate.Year,
                                    new List<PayStatementReportDataContext>()
                                    {
                                        new PayStatementReportDataContext(data,
                                            payrollRegisterDictByPayStatementReferenceId[data.ReferenceKey],
                                            personBenefitDeductionLookup.Contains(data.EmployeeId) ? personBenefitDeductionLookup[data.EmployeeId] : new List<PersonBenefitDeduction>(),
                                            personStatusLookup.Contains(data.EmployeeId) ? personStatusLookup[data.EmployeeId] : new List<PersonEmploymentStatus>())
                                    }
                                }
                            }
                        );
                    }
                    else
                    {
                        if (!Context[data.EmployeeId].ContainsKey(data.PayDate.Year))
                        {
                            Context[data.EmployeeId].Add(
                                data.PayDate.Year,
                                new List<PayStatementReportDataContext>()
                                {
                                    new PayStatementReportDataContext(data,
                                        payrollRegisterDictByPayStatementReferenceId[data.ReferenceKey],
                                        personBenefitDeductionLookup.Contains(data.EmployeeId) ? personBenefitDeductionLookup[data.EmployeeId] : new List<PersonBenefitDeduction>(),
                                        personStatusLookup.Contains(data.EmployeeId) ? personStatusLookup[data.EmployeeId] : new List<PersonEmploymentStatus>())
                                }
                            );
                        }
                        else
                        {
                            Context[data.EmployeeId][data.PayDate.Year].Add(
                                new PayStatementReportDataContext(data,
                                    payrollRegisterDictByPayStatementReferenceId[data.ReferenceKey],
                                    personBenefitDeductionLookup.Contains(data.EmployeeId) ? personBenefitDeductionLookup[data.EmployeeId] : new List<PersonBenefitDeduction>(),
                                    personStatusLookup.Contains(data.EmployeeId) ? personStatusLookup[data.EmployeeId] : new List<PersonEmploymentStatus>()));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceData"></param>
        /// <returns></returns>
        public PayStatementReport BuildPayStatementReport(PayStatementSourceData sourceData)
        {

            if (sourceData == null)
            {
                throw new ArgumentNullException("sourceData");
            }
            if (Context == null || Context.Count == 0)
            {
                throw new InvalidOperationException("Pay Statement context not set");
            }
            if (!Context.ContainsKey(sourceData.EmployeeId))
            {
                throw new KeyNotFoundException("EmployeeId not found");
            }

            if (!Context[sourceData.EmployeeId].ContainsKey(sourceData.PayDate.Year))
            {
                throw new KeyNotFoundException("Year not found");
            }

            var sourceContext = Context[sourceData.EmployeeId][sourceData.PayDate.Year].FirstOrDefault(c => c.sourceData.IdNumber == sourceData.IdNumber);
            var yearToDateContext = Context[sourceData.EmployeeId][sourceData.PayDate.Year].Where(c => c.sourceData.IdNumber <= sourceData.IdNumber);

            return new PayStatementReport(sourceContext, yearToDateContext, dataUtility);

        }


    }
}
