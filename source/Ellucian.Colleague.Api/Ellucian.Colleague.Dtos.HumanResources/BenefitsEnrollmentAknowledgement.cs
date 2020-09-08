/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Employee Benefits Enrollment Acknowledgement Information DTO
    /// </summary>
    public class BenefitsEnrollmentAknowledgement
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BenefitsEnrollmentAknowledgement() { }

        /// <summary>
        /// Index to sort in report
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Benefit Type
        /// </summary>
        public string BenefitType { get; set; }

        /// <summary>
        /// Benefit Type Description
        /// </summary>
        public string BenefitTypeDescription { get; set; }

        /// <summary>
        /// Benefit Plan
        /// </summary>
        public string BenefitPlan { get; set; }

        /// <summary>
        /// Benefit Plan Description
        /// </summary>
        public string BenefitPlanDescription { get; set; }

        /// <summary>
        /// Dependents or Beneficiaries Information
        /// </summary>
        public string DependentsOrBeneficiaries { get; set; }

        /// <summary>
        /// Health Care Provider Information
        /// </summary>
        public string HealthCareProviderInformation { get; set; }

        /// <summary>
        /// Coverage or Participation Information
        /// </summary>
        public string CoverageOrParticipation { get; set; }

        /// <summary>
        /// Constructor to build acknowledgement information
        /// </summary>
        /// <param name="type"></param>
        /// <param name="detail"></param>
        public BenefitsEnrollmentAknowledgement(EmployeeBenefitType type, EmployeeBenefitsEnrollmentDetail detail)
        {
            BenefitType = type.BenefitType;
            BenefitTypeDescription = type.BenefitTypeDescription;

            if (detail != null)
            {
                BenefitPlan = detail.BenefitId;
                BenefitPlanDescription = detail.BenefitDescription;
                DependentsOrBeneficiaries = GetDependentsAndBeneficiaries(detail);
                HealthCareProviderInformation = GetHealthCareProviders(detail);
                CoverageOrParticipation = GetCoverageOrParticipation(detail);
            }
        }

        /// <summary>
        /// Constructor to build acknowledgement information
        /// </summary>
        /// <param name="type"></param>
        public BenefitsEnrollmentAknowledgement(EmployeeBenefitType type)
        {
            BenefitType = type.BenefitType;
            BenefitTypeDescription = type.BenefitTypeDescription;
        }

        /// <summary>
        /// Format depenents and beneficiaries
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private string GetDependentsAndBeneficiaries(EmployeeBenefitsEnrollmentDetail detail)
        {
            var dependentsOrBneficiaries = new StringBuilder();

            if (detail.DependentNames.Any() && detail.DependentNames.Any(name => !string.IsNullOrEmpty(name)))
            {
                dependentsOrBneficiaries.Append(string.Join(Environment.NewLine, detail.DependentNames.Select(n => n.Trim()).ToArray()));
            }

            if (!string.IsNullOrEmpty(dependentsOrBneficiaries.ToString()))
            {
                dependentsOrBneficiaries.AppendLine();
            }

            if (detail.BeneficiaryDisplayInformation.Any() && detail.BeneficiaryDisplayInformation.Any(name => !string.IsNullOrEmpty(name)))
            {
                dependentsOrBneficiaries.Append(string.Join(Environment.NewLine, detail.BeneficiaryDisplayInformation.Select(n => n.Trim()).ToArray()));
            }

            return dependentsOrBneficiaries.ToString();
        }

        /// <summary>
        /// Format health care provider information
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private string GetHealthCareProviders(EmployeeBenefitsEnrollmentDetail detail)
        {
            var providers = new List<string>();

            if (detail.DependentNames.Any() && detail.DependentNames.Any(name => !string.IsNullOrEmpty(name)))
            {
                for (int i = 0; i < detail.DependentNames.Count; i++)
                {
                    if (!string.IsNullOrEmpty(detail.DependentNames[i]))
                    {
                        if (!string.IsNullOrEmpty(detail.DependentProviderIds[i]) && !string.IsNullOrEmpty(detail.DependentProviderNames[i]))
                        {
                            providers.Add(string.Format("{0} - {1} #{2}", detail.DependentNames[i], detail.DependentProviderNames[i], detail.DependentProviderIds[i]));
                        }
                        else if (!string.IsNullOrEmpty(detail.DependentProviderNames[i].Trim()) && string.IsNullOrEmpty(detail.DependentProviderIds[i].Trim()))
                        {
                            providers.Add(string.Format("{0} - {1}", detail.DependentNames[i].Trim(), detail.DependentProviderNames[i].Trim()));
                        }
                        else if (!string.IsNullOrEmpty(detail.DependentProviderIds[i]) && string.IsNullOrEmpty(detail.DependentProviderNames[i]))
                        {
                            providers.Add(string.Format("{0} - #{1}", detail.DependentNames[i].Trim(), detail.DependentProviderIds[i].Trim()));
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(detail.EmployeeProviderId) && !string.IsNullOrEmpty(detail.EmployeeProviderName))
            {
                providers.Add(string.Format("Self - {0} #{1}", detail.EmployeeProviderName.Trim(), detail.EmployeeProviderId.Trim()));
            }
            else if (!string.IsNullOrEmpty(detail.EmployeeProviderName) && string.IsNullOrEmpty(detail.EmployeeProviderId))
            {
                providers.Add(string.Format("Self - {0}", detail.EmployeeProviderName.Trim()));
            }
            else if (!string.IsNullOrEmpty(detail.EmployeeProviderId) && string.IsNullOrEmpty(detail.EmployeeProviderName))
            {
                providers.Add(string.Format("Self - #{0}", detail.EmployeeProviderId.Trim()));
            }

            return (providers.Any()) ? string.Join(Environment.NewLine, providers.ToArray()) : string.Empty;
        }

        /// <summary>
        /// Format coverage or participation
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private string GetCoverageOrParticipation(EmployeeBenefitsEnrollmentDetail detail)
        {
            var coverageOrParticipation = string.Empty;

            if (detail.CoverageLevels.Any() && detail.CoverageLevels.Any(name => !string.IsNullOrEmpty(name)))
            {
                coverageOrParticipation = string.Join(Environment.NewLine, detail.CoverageLevels.Select(c => c.Trim()).ToArray());
            }

            return coverageOrParticipation;
        }
    }
}
