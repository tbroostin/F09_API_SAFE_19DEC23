/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Repository class for Employee Compensation retrieval
    /// </summary>

    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EmployeeCompensationRepository : BaseColleagueRepository, IEmployeeCompensationRepository
    {
        private readonly ApiSettings apiSettings;

        private EmployeeCompensation EmployeeCompensationEntity;

        public EmployeeCompensationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings) : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Returns Employee Compensation Details
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <param name="salaryAmount">Estimated Annual Salary amount used in compensation re-calculation(if provided)</param>
        /// <param name="isAdminView">flag to indicate if API is called from Total Comp Admin View.</param>
        /// <returns>Employee Compensation Domain Entity containing Benefit-Deductions,Taxes and Stipends.</returns>
        public async Task<EmployeeCompensation> GetEmployeeCompensationAsync(string effectivePersonId, decimal? salaryAmount, bool isAdminView)
        {

            if (string.IsNullOrEmpty(effectivePersonId))
                throw new ArgumentNullException("Person ID cannot be null");

            CalcTotalCompensationRequest calcTotalCompRequest = new CalcTotalCompensationRequest();
            calcTotalCompRequest.EmployeeId = effectivePersonId;
            calcTotalCompRequest.SalaryAmount = salaryAmount;
            calcTotalCompRequest.IsAdminView = isAdminView;

            //Invoke Employee Compensation Transaction to fetch compensation details
            var calcTotalCompResponse = await transactionInvoker.ExecuteAsync<CalcTotalCompensationRequest, CalcTotalCompensationResponse>(calcTotalCompRequest);

            // Check if Transaction has a response.
            if (calcTotalCompResponse == null)
            {
                var message = "Could not retreive Total Compensation Information.Unexpected null response from CTX";
                logger.Error(message);
                throw new RepositoryException(message);
            }
           
            await BuildEmployeeCompensationEntity(effectivePersonId, calcTotalCompResponse);

            return EmployeeCompensationEntity;
        }

        /// <summary>
        /// Builds Employee Compensation Domain Entity from Transaction Response
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <param name="calcTotalCompResponse">Response object obtained from EmpCompensation Transaction execution</param>
        /// <returns></returns>
        private async Task<EmployeeCompensation> BuildEmployeeCompensationEntity(string effectivePersonId, CalcTotalCompensationResponse calcTotalCompResponse)
        {
            try
            {
                List<EmployeeBended> EmployeeBendedList = null;
                List<EmployeeTax> EmployeeTaxesList = null;
                List<EmployeeStipend> EmployeeStipendsList = null;
                string TotalCompensationPageHeader = string.Empty;
                string DisplayEmployeeCosts = string.Empty;
                string OtherBenefits = string.Empty;

                //Check if there is any error from CTX
                
                if (!string.IsNullOrEmpty(calcTotalCompResponse.ErrorCode))
                {
                    #region Build Entity with Error Details
                    EmployeeCompensationEntity = new EmployeeCompensation(effectivePersonId,calcTotalCompResponse.ErrorCode, calcTotalCompResponse.ErrorMessage);
                    #endregion
                }

               else
                {
                    #region Build Entity with Compensation Information
                    //Extract Employee Benefit & Deductions
                    if (calcTotalCompResponse.Bended != null && calcTotalCompResponse.Bended.Any())
                    {
                        EmployeeBendedList = new List<EmployeeBended>();

                        calcTotalCompResponse.Bended.ForEach(empBended => EmployeeBendedList.Add(new EmployeeBended(empBended.BendedCodes, empBended.BendedDescriptions, empBended.BendedEmployerAmounts, empBended.BendedEmployeeAmounts)));

                    }

                    //Extract Employee Tax Information
                    if (calcTotalCompResponse.Taxes != null && calcTotalCompResponse.Taxes.Any())
                    {
                        EmployeeTaxesList = new List<EmployeeTax>();

                        calcTotalCompResponse.Taxes.ForEach(empTax => EmployeeTaxesList.Add(new EmployeeTax(empTax.TaxCodes, empTax.TaxDescriptions, empTax.TaxEmployerAmounts, empTax.TaxEmployeeAmounts)));

                    }

                    if (calcTotalCompResponse.Stipends != null && calcTotalCompResponse.Stipends.Any())
                    {
                        EmployeeStipendsList = new List<EmployeeStipend>();

                        calcTotalCompResponse.Stipends.ForEach(stipend => EmployeeStipendsList.Add(new EmployeeStipend(stipend.StipendDescriptions, stipend.StipendAmounts)));
                    }

                    //Get Total Compensation Page Header text and other benefits from HrWebDefaults
                    var hrWebDefaults = await DataReader.ReadRecordAsync<HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");

                    if (hrWebDefaults != null)
                    {
                        TotalCompensationPageHeader = hrWebDefaults.HrwebTotCompPageHeadrSs;
                        OtherBenefits = hrWebDefaults.HrwebBeneText;
                        DisplayEmployeeCosts = hrWebDefaults.HrwebDispEmployeeCostsSs;
                    }

                    //Create Employee Compensation Entity
                    EmployeeCompensationEntity = new EmployeeCompensation(effectivePersonId, OtherBenefits, DisplayEmployeeCosts, TotalCompensationPageHeader, calcTotalCompResponse.SalaryAmount, EmployeeBendedList, EmployeeTaxesList, EmployeeStipendsList);

                    #endregion
                }

            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.ToString());
                throw new ArgumentNullException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new Exception("Unable to build Employee Compensation Domain Entity");
            }
            return EmployeeCompensationEntity;
        }
    }
}
