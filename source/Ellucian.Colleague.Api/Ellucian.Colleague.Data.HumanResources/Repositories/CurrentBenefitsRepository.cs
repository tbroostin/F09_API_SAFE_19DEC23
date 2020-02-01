/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Repository class for Employee Current Benefits Retrieval
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CurrentBenefitsRepository : BaseColleagueRepository, ICurrentBenefitsRepository
    {
        private readonly ApiSettings apiSettings;

        public CurrentBenefitsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings) : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Returns Employee Current Benefits Details
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user.</param>
        /// <returns>Employee Benefits Domain Entity.</returns>
        public async Task<EmployeeBenefits> GetEmployeeCurrentBenefitsAsync(string effectivePersonId)
        {

            if (string.IsNullOrEmpty(effectivePersonId))
                throw new ArgumentNullException("effectivePersonId");

            EmployeeBenefits employeeBenefitsEntity = null;
            CurrentBenefitsRequest currentBenefitsRequest = new CurrentBenefitsRequest();
            currentBenefitsRequest.EmployeeId = effectivePersonId;
            CurrentBenefitsResponse currentBenefitsResponse = null;
            try
            {
                //Invoke Employee Compensation Transaction to fetch compensation details
                currentBenefitsResponse = await transactionInvoker.ExecuteAsync<CurrentBenefitsRequest, CurrentBenefitsResponse>(currentBenefitsRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }         

            // Check if Transaction has a response.
            if (currentBenefitsResponse == null)
            {
                var message = "Could not retreive Current Benefit Information.Unexpected null response from CTX";
                logger.Error(message);
                throw new RepositoryException(message);
            }
            //Check for error message from Transaction execution
            if (!string.IsNullOrEmpty(currentBenefitsResponse.ErrorMessage))
            {
                var message = string.Format("Current Benefit Transaction execution failed . Error message - {0}", currentBenefitsResponse.ErrorMessage);
                logger.Error(message);
                throw new RepositoryException(currentBenefitsResponse.ErrorMessage);
            }

            employeeBenefitsEntity = BuildEmployeeBenefitsEntity(effectivePersonId, currentBenefitsResponse, currentBenefitsResponse.BenefitDesc.Count);

            return employeeBenefitsEntity;
        }

        /// <summary>
        /// Builds Employee Compensation Domain Entity from Transaction Response
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <param name="calcTotalCompResponse">Response object obtained from CurrentBenefits Transaction execution</param>
        /// <param name="totalBenefitsCount">Total Number of benefits for the employee</param>
        /// <returns></returns>
        private EmployeeBenefits BuildEmployeeBenefitsEntity(string effectivePersonId, CurrentBenefitsResponse currentBenefitsResponse, int totalBenefitsCount)
        {
            try
            {

                var currentBenefits = new List<CurrentBenefit>();

                if (currentBenefitsResponse.BenefitDesc !=null && currentBenefitsResponse.BenefitDesc.Any())
                {
                    for (int i = 0; i < currentBenefitsResponse.BenefitDesc.Count; i++)
                    {
                        List<string> dependents = null, healthCareProviders = null, beneficiaries = null;
                        string description = currentBenefitsResponse.BenefitDesc[i];
                        string coverage = string.Empty, employeeCost = string.Empty;

                         //Setting the value for coverage 
                        if (currentBenefitsResponse.BenefitCoverage != null && currentBenefitsResponse.BenefitCoverage.Any())
                        {
                            coverage = currentBenefitsResponse.BenefitCoverage[i];
                        }

                        //Set Employee Cost Value
                        if (currentBenefitsResponse.EmployeeCost != null && currentBenefitsResponse.EmployeeCost.Any())
                        {
                            employeeCost = currentBenefitsResponse.EmployeeCost[i];
                        }

                        if (currentBenefitsResponse.BenefitDependents != null && currentBenefitsResponse.BenefitDependents.Any())
                        {
                            if (!string.IsNullOrEmpty(currentBenefitsResponse.BenefitDependents[i]))
                            {
                                // Splitting the dependent list based on the saperator ';'.
                                dependents = currentBenefitsResponse.BenefitDependents[i].Split(';').Select(x => x.Trim()).ToList();
                            }
                        }


                        if (currentBenefitsResponse.BenefitHealthcareProvider != null && currentBenefitsResponse.BenefitHealthcareProvider.Any())
                        {
                            if (!string.IsNullOrEmpty(currentBenefitsResponse.BenefitHealthcareProvider[i]))
                            {
                                // Splitting the health care provider list based on the saperator ';'.
                                healthCareProviders = currentBenefitsResponse.BenefitHealthcareProvider[i].Split(';').Select(x => x.Trim()).ToList();
                            }
                        }

                        if (currentBenefitsResponse.BenefitBeneficiaries != null && currentBenefitsResponse.BenefitBeneficiaries.Any())
                        {
                            if (!string.IsNullOrEmpty(currentBenefitsResponse.BenefitBeneficiaries[i]))
                            {
                                // Splitting the beneficiaries list based on the saperator ';'.
                                beneficiaries = currentBenefitsResponse.BenefitBeneficiaries[i].Split(';').Select(x => x.Trim()).ToList();
                            }
                        }


                        CurrentBenefit currentBenefitEntity = new CurrentBenefit(description,
                            coverage,
                            employeeCost,
                            dependents,
                            healthCareProviders,
                            beneficiaries);

                        currentBenefits.Add(currentBenefitEntity);
                    }
                }

                return new EmployeeBenefits(effectivePersonId,
                    currentBenefitsResponse.AdditionalInformation,
                    currentBenefits);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.ToString());
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw new Exception("Unable to build Employee Current Benefits", ex);
            }
        }
    }
}

