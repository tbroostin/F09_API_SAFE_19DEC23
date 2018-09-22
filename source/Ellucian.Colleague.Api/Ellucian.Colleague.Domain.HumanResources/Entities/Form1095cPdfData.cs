// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Represents a 1095-C tax form PDF data
    /// </summary>
    [Serializable]
    public class Form1095cPdfData
    {
        /// <summary>
        /// Tax year for the tax form
        /// </summary>
        public string TaxYear { get { return taxYear;} }
        private readonly string taxYear;

        /// <summary>
        /// Employer's tax identification number
        /// </summary>
        public string EmployerEin { get { return employerEin; } }
        private readonly string employerEin;

        /// <summary>
        /// Employee's social security number
        /// </summary>
        public string EmployeeSsn { get { return this.employeeSsn; } }
        private readonly string employeeSsn;

        /// <summary>
        /// The pdf is voided
        /// </summary>
        public bool IsVoided { get; set; }

        /// <summary>
        /// The pdf is corrected
        /// </summary>
        public bool IsCorrected { get; set; }

        /// <summary>
        /// Employer's name
        /// </summary>
        public string EmployerName { get; set; }

        /// <summary>
        /// Employer's address line
        /// </summary>
        public string EmployerAddressLine { get; set; }

        /// <summary>
        /// Employer's city
        /// </summary>
        public string EmployerCityName { get; set; }

        /// <summary>
        /// Employer's state code
        /// </summary>
        public string EmployerStateCode { get; set; }

        /// <summary>
        /// Employee's zip code
        /// </summary>
        public string EmployerZipCode { get; set; }

        /// <summary>
        /// Employer's contact phone number
        /// </summary>
        public string EmployerContactPhoneNumber { get; set; }

        /// <summary>
        /// Employer's contact phone number extension
        /// </summary>
        public string EmployerContactPhoneExtension { get; set; }

        /// <summary>
        /// Employee's first name
        /// </summary>
        public string EmployeeFirstName { get; set; }

        /// <summary>
        /// Employee's middle name
        /// </summary>
        public string EmployeeMiddleName { get; set; }

        /// <summary>
        /// Employee's last name
        /// </summary>
        public string EmployeeLastName { get; set; }

        /// <summary>
        /// Employee's suffix
        /// </summary>
        public string EmployeeSuffix { get; set; }

                /// <summary>
        /// Employee's full name
        /// </summary>
        /// <returns></returns>
        public string EmployeeName()
        {
            string name = EmployeeFirstName;
            if (!string.IsNullOrEmpty(name))
            {
                if (!string.IsNullOrEmpty(EmployeeMiddleName))
                {
                    name = name + " " + EmployeeMiddleName.Substring(0, 1);
                }
                if (!string.IsNullOrEmpty(EmployeeLastName))
                {
                    name = name + " " + EmployeeLastName;
                }
                if (!string.IsNullOrEmpty(EmployeeSuffix))
                {
                    name = name + ", " + EmployeeSuffix;
                }
            }
            return name;
        }
        

        /// <summary>
        /// Employee's first address line
        /// </summary>
        public string EmployeeAddressLine1 { get; set; }

        /// <summary>
        /// Employee's second address line
        /// </summary>
        public string EmployeeAddressLine2 { get; set; }

        /// <summary>
        /// Employee's city
        /// </summary>
        public string EmployeeCityName { get; set; }

        /// <summary>
        /// Employee's state code
        /// </summary>
        public string EmployeeStateCode { get; set; }

        /// <summary>
        /// Employee's zip code
        /// </summary>
        public string EmployeePostalCode { get; set; }

        /// <summary>
        /// Employee's Ip extension
        /// </summary>
        public string EmployeeZipExtension { get; set; }

        /// <summary>
        /// Employee's country
        /// </summary>
        public string EmployeeCountry { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for the 12 months
        /// </summary>
        public string OfferOfCoverage12Month { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for January
        /// </summary>
        public string OfferOfCoverageJanuary { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for February
        /// </summary>
        public string OfferOfCoverageFebruary { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for March
        /// </summary>
        public string OfferOfCoverageMarch { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for April
        /// </summary>
        public string OfferOfCoverageApril { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for May
        /// </summary>
        public string OfferOfCoverageMay { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for June
        /// </summary>
        public string OfferOfCoverageJune { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for July
        /// </summary>
        public string OfferOfCoverageJuly { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for August
        /// </summary>
        public string OfferOfCoverageAugust { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for September
        /// </summary>
        public string OfferOfCoverageSeptember { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for October
        /// </summary>
        public string OfferOfCoverageOctober { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for November
        /// </summary>
        public string OfferOfCoverageNovember { get; set; }

        /// <summary>
        /// Employee's offer of coverage code for December
        /// </summary>
        public string OfferOfCoverageDecember { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for the 12 months
        /// </summary>
        public decimal? LowestCostAmount12Month { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for January
        /// </summary>
        public decimal? LowestCostAmountJanuary { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for February
        /// </summary>
        public decimal? LowestCostAmountFebruary { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for March
        /// </summary>
        public decimal? LowestCostAmountMarch { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for April
        /// </summary>
        public decimal? LowestCostAmountApril { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for May
        /// </summary>
        public decimal? LowestCostAmountMay { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for June
        /// </summary>
        public decimal? LowestCostAmountJune { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for July
        /// </summary>
        public decimal? LowestCostAmountJuly { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for August
        /// </summary>
        public decimal? LowestCostAmountAugust { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for September
        /// </summary>
        public decimal? LowestCostAmountSeptember { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for October
        /// </summary>
        public decimal? LowestCostAmountOctober { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for November
        /// </summary>
        public decimal? LowestCostAmountNovember { get; set; }

        /// <summary>
        /// Employee's share of lowest cost monthly premium for December
        /// </summary>
        public decimal? LowestCostAmountDecember { get; set; }

        /// <summary>
        /// Safe harbor code for the 12 months
        /// </summary>
        public string SafeHarborCode12Month { get; set; }

        /// <summary>
        /// Safe harbor code for January
        /// </summary>
        public string SafeHarborCodeJanuary { get; set; }

        /// <summary>
        /// Safe harbor code for February
        /// </summary>
        public string SafeHarborCodeFebruary { get; set; }

        /// <summary>
        /// Safe harbor code for March
        /// </summary>
        public string SafeHarborCodeMarch { get; set; }

        /// <summary>
        /// Safe harbor code for April
        /// </summary>
        public string SafeHarborCodeApril { get; set; }

        /// <summary>
        /// Safe harbor code for May
        /// </summary>
        public string SafeHarborCodeMay { get; set; }

        /// <summary>
        /// Safe harbor code for June
        /// </summary>
        public string SafeHarborCodeJune { get; set; }

        /// <summary>
        /// Safe harbor code for July
        /// </summary>
        public string SafeHarborCodeJuly { get; set; }

        /// <summary>
        /// Safe harbor code for August
        /// </summary>
        public string SafeHarborCodeAugust { get; set; }

        /// <summary>
        /// Safe harbor code for September
        /// </summary>
        public string SafeHarborCodeSeptember { get; set; }

        /// <summary>
        /// Safe harbor code for October
        /// </summary>
        public string SafeHarborCodeOctober { get; set; }

        /// <summary>
        /// Safe harbor code for November
        /// </summary>
        public string SafeHarborCodeNovember { get; set; }

        /// <summary>
        /// Safe harbor code for December
        /// </summary>
        public string SafeHarborCodeDecember { get; set; }

        /// <summary>
        /// Employer provides self-insured coverage?
        /// </summary>
        public bool EmployeeIsSelfInsured { get; set; }

        /// <summary>
        /// Plan start month code
        /// </summary>
        public string PlanStartMonthCode { get; set; }

        /// <summary>
        /// List of individuals covered by the employee's insurance when self-insured
        /// </summary>
        public ReadOnlyCollection<Form1095cCoveredIndividualsPdfData> CoveredIndividuals {get; private set; }
        private readonly List<Form1095cCoveredIndividualsPdfData> coveredIndividuals = new List<Form1095cCoveredIndividualsPdfData>();

        /// <summary>
        /// Constructor to instantiate a 1095-C tax form PDF for an employee
        /// </summary>
        /// <param name="taxYear">The tax year</param>
        /// <param name="employerEin">The employer's EIN</param>
        /// <param name="ssn">The employee's SSN</param>
        public Form1095cPdfData(string taxYear, string employerEin, string ssn)
        {
            if (string.IsNullOrEmpty(taxYear))
                throw new ArgumentNullException("taxYear", "taxYear is required.");

            if (string.IsNullOrEmpty(employerEin))
                throw new ArgumentNullException("employerEin", "employerEin is required.");

            this.taxYear = taxYear;
            this.employerEin = employerEin;
            this.employeeSsn = ssn ?? "";
            CoveredIndividuals = coveredIndividuals.AsReadOnly();
        }

        /// <summary>
        /// Add a covered individual to the list of covered individuals for the employee
        /// </summary>
        /// <param name="coveredIndividual">The covered individual</param>
        public void AddCoveredIndividual(Form1095cCoveredIndividualsPdfData coveredIndividual)
        {
            if (coveredIndividual == null)
            {
                throw new ArgumentNullException("coveredIndividual", "CoveredIndividual object cannot be null");
            }
            if (coveredIndividuals != null)
            {
                coveredIndividuals.Add(coveredIndividual);
            }
        }
    }
}
