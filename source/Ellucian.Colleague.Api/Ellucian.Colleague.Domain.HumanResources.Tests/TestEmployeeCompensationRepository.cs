/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEmployeeCompensationRepository : IEmployeeCompensationRepository
    {
        public class EmployeeCompensationRecord
        {
            public string PersonId { get; set; }
            public string OtherBenefits { get; set; }
            public string DisplayEmployeeCosts { get; set; }
            public string TotalCompensationPageHeader { get; set; }
            public decimal? SalaryAmount { get; set; }
            public List<EmployeeBendedRecord> Bended { get; set; }
            public List<EmployeeTaxRecord> Taxes { get; set; }
            public List<EmployeeStipendRecord> Stipends { get; set; }

            public EmployeeCompensationErrorRecord EmployeeCompensationError { get; set; }
        }
        public class EmployeeBendedRecord
        {
            public string BenededCode { get; set; }
            public string BenededDescription { get; set; }
            public decimal? BenededEmployerAmount { get; set; }
            public decimal? BenededEmployeeAmount { get; set; }
            public EmployeeBendedRecord(string benededCode, string benededCodeDescription, decimal? benededEmployerAmount, decimal? benededEmployeeAmount)
            {
                if (string.IsNullOrEmpty(benededCode))
                    throw new ArgumentNullException("Benefit-Deduction Code cannot be empty");

                BenededCode = benededCode;
                BenededDescription = benededCodeDescription;
                BenededEmployerAmount = benededEmployerAmount;
                BenededEmployeeAmount = benededEmployeeAmount;
            }
        }
        public class EmployeeTaxRecord
        {
            public string TaxCode { get; set; }
            public string TaxDescription { get; set; }
            public decimal? TaxEmployerAmount { get; set; }
            public decimal? TaxEmployeeAmount { get; set; }
            public EmployeeTaxRecord(string taxCode, string taxCodeDescription, decimal? taxEmployerAmount, decimal? taxEmployeeAmount)
            {
                if (string.IsNullOrEmpty(taxCode))
                    throw new ArgumentNullException("Tax Code cannot be empty");

                TaxCode = taxCode;
                TaxDescription = taxCodeDescription;
                TaxEmployerAmount = taxEmployerAmount;
                TaxEmployeeAmount = taxEmployeeAmount;
            }
        }
        public class EmployeeStipendRecord
        {
            public string StipendDescription { get; set; }
            public decimal? StipendAmount { get; set; }
            public EmployeeStipendRecord(string stipendDescription, decimal? stipendAmount)
            {
                StipendDescription = stipendDescription;
                StipendAmount = stipendAmount;
            }
        }

        public class EmployeeCompensationErrorRecord
        {
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }

        }

        public List<EmployeeCompensationRecord> EmployeeCompensationRecordList = new List<EmployeeCompensationRecord>()
        {
            new EmployeeCompensationRecord()
        {

            PersonId = "0014697",
            OtherBenefits = "Additional benefits are available for all full-time employees. These benefits, including free concert and sports tickets, free parking, and use of the athletic facilities, are not listed on this form.",
            DisplayEmployeeCosts = "Y",
            TotalCompensationPageHeader = "This Total Compensation Statement is intended to summarize the estimated value of your current benefits. While every effort has been taken to accurately report this information, discrepancies are possible.",
            SalaryAmount = 19200.00m,
            Bended = new List<EmployeeBendedRecord>()
            {
                new EmployeeBendedRecord("DEP1","Dental Employee Plus One",780.72m,207.48m),
                new EmployeeBendedRecord("MEDE","Medical - Employee Only",3415.44m,379.56m)
            },
            Taxes = new List<EmployeeTaxRecord>()
            {
                new EmployeeTaxRecord("FICA","FICA Withholding",1154m,1154m),
                new EmployeeTaxRecord("FWHM","Federal Withholding - Married",null,681.30m)
            },

            Stipends = new List<EmployeeStipendRecord>()
            {
                new EmployeeStipendRecord("Restricted Stipend",1200.00m),
                new EmployeeStipendRecord("Test GL  Distribution",1000.00m)
            }

        },
            new EmployeeCompensationRecord()
             {

            PersonId = "0014698",
            OtherBenefits = "Additional benefits are available for all full-time employees. These benefits, including free concert and sports tickets, free parking, and use of the athletic facilities, are not listed on this form.",
            DisplayEmployeeCosts = "Y",
            TotalCompensationPageHeader = "This Total Compensation Statement is intended to summarize the estimated value of your current benefits.",
            SalaryAmount = 19200.00m,
            Bended = new List<EmployeeBendedRecord>()
            {
                new EmployeeBendedRecord("DEP2","Dental Employee Plus Two",780.72m,207.48m),
                new EmployeeBendedRecord("MED","Medical Only",3415.44m,379.56m)
            },
            Taxes = new List<EmployeeTaxRecord>()
            {
                new EmployeeTaxRecord("FICA","FICA Withholding",1154m,1154m),
            },

            Stipends = new List<EmployeeStipendRecord>()
            {
                new EmployeeStipendRecord("Ongoing Stipend",1250.45m),

            }

        },

                new EmployeeCompensationRecord()
                {

                  PersonId="0014888",
                  EmployeeCompensationError = new EmployeeCompensationErrorRecord()
                  {
                        ErrorCode="RestrictedByRules",
                        ErrorMessage="This user is restricted from using Total Compensation Page"
                  }
          
            }
                

        };



        public async Task<HumanResources.Entities.EmployeeCompensation> GetEmployeeCompensationAsync(string effectivePersonId, decimal? salaryAmount, bool isAdminView)
        {
            var empCompensation = EmployeeCompensationRecordList
              .Where(r => r.PersonId == effectivePersonId).FirstOrDefault();

            return await Task.FromResult(BuildEmployeeCompensationEntity(empCompensation));


        }
        public EmployeeCompensation BuildEmployeeCompensationEntity(EmployeeCompensationRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            if (record.EmployeeCompensationError !=null && !string.IsNullOrEmpty(record.EmployeeCompensationError.ErrorCode))
            {
                #region Build Entity with Error Details
                return new EmployeeCompensation(record.PersonId, record.EmployeeCompensationError.ErrorCode, record.EmployeeCompensationError.ErrorMessage);
                #endregion
            }

            //Bended Extraction
            List<EmployeeBended> BendedList = null;
            if (record.Bended != null && record.Bended.Any())
            {
                BendedList = new List<EmployeeBended>();
                record.Bended.ForEach(r => BendedList.Add(new EmployeeBended(r.BenededCode, r.BenededDescription, r.BenededEmployerAmount, r.BenededEmployeeAmount)));
            }

            //Taxes Extraction
            List<EmployeeTax> EmpTaxes = null;
            if (record.Taxes != null && record.Taxes.Any())
            {
                EmpTaxes = new List<EmployeeTax>();
                record.Taxes.ForEach(r => EmpTaxes.Add(new EmployeeTax(r.TaxCode, r.TaxDescription, r.TaxEmployerAmount, r.TaxEmployeeAmount)));
            }

            //Stipend Extraction
            List<EmployeeStipend> EmpStipends = null;
            if (record.Taxes != null && record.Taxes.Any())
            {
                EmpStipends = new List<EmployeeStipend>();
                record.Stipends.ForEach(r => EmpStipends.Add(new EmployeeStipend(r.StipendDescription, r.StipendAmount)));
            }

            return new EmployeeCompensation(record.PersonId, record.OtherBenefits, record.DisplayEmployeeCosts, record.TotalCompensationPageHeader,
                                            record.SalaryAmount, BendedList, EmpTaxes, EmpStipends);
        }

        #region Transaction Response
        public CalcTotalCompensationResponse CalcTotalCompensationResponse = new CalcTotalCompensationResponse()
        {

            SalaryAmount = 19200.00m,
            Bended = new List<Bended>()
            {
                new Bended() {BendedCodes= "DEP2",BendedDescriptions="Dental Employee Plus Two",BendedEmployeeAmounts=780.72m,BendedEmployerAmounts =207.48m },
                new Bended() { BendedCodes="MED",BendedDescriptions="Medical Only",BendedEmployeeAmounts=3415.44m,BendedEmployerAmounts=379.56m}
            },
            Taxes = new List<Taxes>()
            {
                new Taxes() {TaxCodes= "FICA", TaxDescriptions="FICA Withholding",TaxEmployeeAmounts=1154m,TaxEmployerAmounts=1154m },
            },

            Stipends = new List<Stipends>()
            {
                new Stipends {StipendDescriptions ="Ongoing Stipend", StipendAmounts=1250.45m },

            },

        };

        public CalcTotalCompensationResponse CalcTotalCompensationResponseWithError = new CalcTotalCompensationResponse()
        {
            EmployeeId = "0014888",
            ErrorCode = "RestrictedByRules",
            ErrorMessage = "This user is restricted from using Total Compensation Page"

        };

        public Data.HumanResources.DataContracts.HrwebDefaults HrwebDefaults()
        {
            return new Data.HumanResources.DataContracts.HrwebDefaults()
            {
                HrwebDispEmployeeCostsSs = "Y",
                HrwebBeneText = "Additional benefits are available for all full-time employees. These benefits, including free concert and sports tickets, free parking, and use of the athletic facilities, are not listed on this form.",
                HrwebTotCompPageHeadrSs = "This Total Compensation Statement is intended to summarize the estimated value of your current benefits."
            };
        }

        #endregion


    }
}
