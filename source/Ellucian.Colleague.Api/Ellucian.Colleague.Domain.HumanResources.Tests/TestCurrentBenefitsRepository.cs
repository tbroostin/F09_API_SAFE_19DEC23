/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestCurrentBenefitsRepository : ICurrentBenefitsRepository
    {
        public class EmployeeBenefitsRecord
        {
            public string PersonId { get; set; }
            public string AdditionalInformation { get; set; }
            public List<CurrentBenefitRecord> CurrentBenefits { get; set; }
        }
        public class CurrentBenefitRecord
        {
            public string BenefitDescription { get; set; }
            public string Coverage { get; set; }
            public string EmployeeCost { get; set; }
            public List<string> Dependents { get; set; }
            public List<string> HealthCareProviders { get; set; }
            public List<string> Beneficiaries { get; set; }
        }

        public List<EmployeeBenefitsRecord> EmployeeBenefitsRecordList = new List<EmployeeBenefitsRecord>()
        {
            new EmployeeBenefitsRecord() {
            PersonId = "0014697",
            AdditionalInformation = "Ellucian University also provides the following non-contributory benefits to all employees: Short Term Disability, Long Term Disability, Employee Assistance Program, Tuition Reimbursement, Credit Union, Car Buying Service, Fitness Benefit up to $130/year, Fitness Facility, Entertainment Benefit up to $75/year, Free Tickets Program.  For detailed information, log into the HR website or contact your HR representative.",
            CurrentBenefits = new List<CurrentBenefitRecord>()
                {
                    new CurrentBenefitRecord()
                    {
                        BenefitDescription = "Dental Employee Plus One",
                        Coverage = "Employee plus One",
                        EmployeeCost = "$8.65",
                        Dependents = new List<string>() { "Jason Richerdson", "Mark Tester" },
                        HealthCareProviders = new List<string>() {  "Self - Rick Dalton #1729", "Pavithra Shetty, Sr. - Subanna and Co #98765" },
                        Beneficiaries = new List<string>() {  "Spike Stubin 100% (Beneficiary)" }
                    },
                    new CurrentBenefitRecord()
                    {
                        BenefitDescription = "Medical - Employee Only",
                        Coverage = "Employee Only - Single",
                        EmployeeCost = "$31.63",
                        Dependents = new List<string>() { "Lily" },
                        HealthCareProviders = new List<string>() { "Self - Medic #0604", "Rose - Mili" },
                        Beneficiaries = new List<string>() { "ELLUCIAN UNIVERSITY 25% (Beneficiary)", "Powell High School 15% (Beneficiary)", "Psun B Avila 60% (Beneficiary)" }
                    }
                }
            },
             new EmployeeBenefitsRecord() {
            PersonId = "0014698",
            AdditionalInformation = "Ellucian University also provides the following non-contributory benefits to all employees: Short Term Disability, Long Term Disability, Employee Assistance Program, Tuition Reimbursement, Credit Union, Car Buying Service, Fitness Benefit up to $130/year, Fitness Facility, Entertainment Benefit up to $75/year, Free Tickets Program.  For detailed information, log into the HR website or contact your HR representative.",
            CurrentBenefits = new List<CurrentBenefitRecord>()
                {
                    new CurrentBenefitRecord()
                    {
                        BenefitDescription = "Eye Check up",
                        Coverage = "Employee",
                        EmployeeCost = "$8.65",
                        Dependents = new List<string>() { "Ross", "Joey" },
                        HealthCareProviders = new List<string>() {  "Self - Lucifer", "Joey - Maze" },
                        Beneficiaries = new List<string>() {  "ABC 100% (Beneficiary)" }
                    },
                    new CurrentBenefitRecord()
                    {
                        BenefitDescription = "Medical - Employee Only",
                        Coverage = "Employee Only - Single",
                        EmployeeCost = "$31.63",
                        Dependents = new List<string>() { "Dan, Jr." },
                        HealthCareProviders = new List<string>() { "Self - Medic #0604", "Dan, Jr. - Healy" },
                        Beneficiaries = new List<string>() { "ELLUCIAN UNIVERSITY 25% (Beneficiary)", "Powell High School 15% (Beneficiary)", "Psun B Avila 60% (Beneficiary)" }
                    }
                }
            }
        };
        public async Task<EmployeeBenefits> GetEmployeeCurrentBenefitsAsync(string effectivePersonId)
        {
            var empBenefits = EmployeeBenefitsRecordList
            .Where(r => r.PersonId == effectivePersonId).FirstOrDefault();

            return await Task.FromResult(BuildEmployeeBenefitsEntity(empBenefits));
        }

        public EmployeeBenefits BuildEmployeeBenefitsEntity(EmployeeBenefitsRecord record)
        {
            if(record == null)
            {
                throw new ArgumentNullException("record");
            }

            List<CurrentBenefit> CurrentBenefitList = null;
            if (record.CurrentBenefits != null)
            {
                CurrentBenefitList = new List<CurrentBenefit>();
                foreach (var item in record.CurrentBenefits)
                {
                    CurrentBenefit currentBenefit = new CurrentBenefit(
                            item.BenefitDescription,
                            item.Coverage,
                            item.EmployeeCost,
                            item.Dependents,
                            item.HealthCareProviders,
                            item.Beneficiaries
                        );
                    CurrentBenefitList.Add(currentBenefit);
                }
            }

            return new EmployeeBenefits(record.PersonId, record.AdditionalInformation, CurrentBenefitList);        
        }

        #region Transaction Response
        public CurrentBenefitsResponse CurrentBenefitsResponse = new CurrentBenefitsResponse()
        {
            EmployeeId = "0014697",
            AdditionalInformation = "Ellucian University also provides the following non-contributory benefits to all employees: Short Term Disability, Long Term Disability, Employee Assistance Program, Tuition Reimbursement, Credit Union, Car Buying Service, Fitness Benefit up to $130/year, Fitness Facility, Entertainment Benefit up to $75/year, Free Tickets Program.  For detailed information, log into the HR website or contact your HR representative.",
            BenefitDesc = new List<string>()
            {
                "Dental Employee Plus One",
                "Medical - Employee Only"
            },
            BenefitCoverage = new List<string>()
            {
                "Employee plus One - $31.63",
                "Employee Only - Single - $31.63"
            },
            BenefitDependents = new List<string>()
            {
                "Pavithra Shetty, Sr.; Mark Tester",
                "Ganesh_ssu, Jr."
            },       
           BenefitHealthcareProvider = new List<string>()
           {
               "Self - Rick Dalton #1729; Pavithra Shetty, Sr. - Subanna and Co #98765",
               "Self - Medic #0604; Ganesh_ssu, Jr. - Ganesh - Test Provider #12345"
           },
           BenefitBeneficiaries = new List<string>()
           {
               "Spike Stubin 100% (Beneficiary)",
               "ELLUCIAN UNIVERSITY 25% (Beneficiary); Powell High School 15% (Beneficiary); Psun B Avila 60% (Beneficiary)"
           }
        };

        public CurrentBenefitsResponse CurrentBenefitsResponseWithError = new CurrentBenefitsResponse()
        {
            EmployeeId = "0014600",
            ErrorMessage = "Error Message"
        };
        #endregion
    }
}
