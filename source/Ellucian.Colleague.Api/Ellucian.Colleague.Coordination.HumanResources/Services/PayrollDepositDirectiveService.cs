﻿/*Copyright 2017-2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Cooridination Service for PayrollDepositDirectives
    /// </summary>
    [RegisterType]
    public class PayrollDepositDirectiveService : BaseCoordinationService, IPayrollDepositDirectiveService
    {
        private readonly IPayrollDepositDirectivesRepository payrollDepositDirectivesRepository;
        private readonly IBankRepository bankRepository;
        private readonly IBankingAuthenticationClaimRepository bankingAuthenticationClaimRepository;
        private readonly IBankingInformationConfigurationRepository bankingInformationConfigurationRepository;

        public PayrollDepositDirectiveService(
            IPayrollDepositDirectivesRepository payrollDepositDirectivesRepository,
            IBankRepository bankRepository,
            IBankingAuthenticationClaimRepository bankingAuthenticationClaimRepository,
            IBankingInformationConfigurationRepository bankingInformationConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.payrollDepositDirectivesRepository = payrollDepositDirectivesRepository;
            this.bankRepository = bankRepository;
            this.bankingAuthenticationClaimRepository = bankingAuthenticationClaimRepository;
            this.bankingInformationConfigurationRepository = bankingInformationConfigurationRepository;
        }

        /// <summary>
        /// Get all PayrollDepositDirectives owned by the current user
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollDepositDirective>> GetPayrollDepositDirectivesAsync()
        {
            logger.Debug("************ Start- Service to get Payroll Deposit Directives - Start ************");
            var domainDirectives = await payrollDepositDirectivesRepository.GetPayrollDepositDirectivesAsync(CurrentUser.PersonId);
            logger.Debug("************Payroll Deposit Directives data retrieved************");
            if (domainDirectives == null)
            {
                var message = "Null PayrollDepositDirectives returned from repository";
                logger.Error(message);
                logger.Debug("No Payroll Deposit Directives returned from repository");
                throw new ColleagueWebApiException(message);
            }

            var dtoDirectives = new List<PayrollDepositDirective>();
            var directiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayrollDepositDirective, Dtos.Base.PayrollDepositDirective>();
            foreach (var directive in domainDirectives)
            {
                dtoDirectives.Add(directiveEntityToDtoAdapter.MapToType(directive));
            }
            logger.Debug("************ End - Service to get Payroll Deposit Directives - End ************");
            return dtoDirectives;
        }

        /// <summary>
        /// Get a single PayrollDepositDirective owned by the current user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PayrollDepositDirective> GetPayrollDepositDirectiveAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Debug("Payroll Deposit Directive ID is not provided");
                throw new ArgumentNullException("id");
            }
            logger.Debug("************ Start- Service to get Payroll Deposit Directives - Start ************");
            var domainDirective = await payrollDepositDirectivesRepository.GetPayrollDepositDirectiveAsync(id, CurrentUser.PersonId);
            if (domainDirective == null)
            {
                var message = "Null PayrollDepositDirective returned from repository";
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }
            if (domainDirective.PersonId != CurrentUser.PersonId)
            {
                var message = string.Format("Current user {0} cannot access deposit for person {1}", CurrentUser.PersonId, domainDirective.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var directiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayrollDepositDirective, Dtos.Base.PayrollDepositDirective>();
            logger.Debug("************ End - Service to get Payroll Deposit Directive - End ************");
            return directiveEntityToDtoAdapter.MapToType(domainDirective);
        }

        /// <summary>
        /// Batch update a list of PayrollDepositDirectives. Input token must be valid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="payrollDepositDirectives"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollDepositDirective>> UpdatePayrollDepositDirectivesAsync(string token, IEnumerable<PayrollDepositDirective> payrollDepositDirectives)
        {
            if (payrollDepositDirectives == null)
            {
                logger.Debug("Payroll Deposit Directives cannot be null or empty");
                throw new ArgumentNullException("payrollDepositDirectives");
            }
            await VerifyAuthenticationTokenAsync(token);
            logger.Debug("************ Start- Service to Update Multiple Payroll Deposit Directives - Start ************");
            var directiveDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Base.PayrollDepositDirective, Domain.HumanResources.Entities.PayrollDepositDirective>();
            var directiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayrollDepositDirective, Dtos.Base.PayrollDepositDirective>();
            var domainDirectivesToUpdate = new Domain.HumanResources.Entities.PayrollDepositDirectiveCollection(CurrentUser.PersonId);
            var updatedDtoDirectives = new List<PayrollDepositDirective>();
            foreach (var directive in payrollDepositDirectives)
            {
                if (CurrentUser.PersonId != directive.PersonId)
                {
                    var message = string.Format("Current user {0} cannot access deposit for person {1}", CurrentUser.PersonId, directive.PersonId);
                    logger.Error(message);
                    throw new PermissionsException(message);
                }

                // ensure the change operator is set to current user
                directive.Timestamp.ChangeOperator = CurrentUser.PersonId;

                domainDirectivesToUpdate.Add(directiveDtoToEntityAdapter.MapToType(directive));
            }

            var updatedDomainDirectives = await payrollDepositDirectivesRepository.UpdatePayrollDepositDirectivesAsync(domainDirectivesToUpdate);
            logger.Debug("Successfully Updated Payroll Deposit Directives");
            if (updatedDomainDirectives == null)
            {
                var message = "Null PayrollDepositDirectives returned from repository";
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }

            foreach (var directive in updatedDomainDirectives)
            {
                updatedDtoDirectives.Add(directiveEntityToDtoAdapter.MapToType(directive));
            }
            logger.Debug("************ End- Service to Update Multiple Payroll Deposit Directives - End ************");
            return updatedDtoDirectives;
        }

        /// <summary>
        /// Update a single PayrollDepositDirective
        /// </summary>
        /// <param name="token"></param>
        /// <param name="payrollDepositDirective"></param>
        /// <returns></returns>
        public async Task<PayrollDepositDirective> UpdatePayrollDepositDirectiveAsync(string token, PayrollDepositDirective payrollDepositDirective)
        {
            if (payrollDepositDirective == null)
            {
                logger.Debug("Payroll Deposit Directives cannot be null or empty");
                throw new ArgumentNullException("payrollDepositDirective");
            }
            if (CurrentUser.PersonId != payrollDepositDirective.PersonId)
            {
                var message = string.Format("Current user {0} cannot access deposit for person {1}", CurrentUser.PersonId, payrollDepositDirective.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            logger.Debug("************ Start - Service to Update Single Payroll Deposit Directives - Start ************");
            // ensure the change operator is set to current user
            payrollDepositDirective.Timestamp.ChangeOperator = CurrentUser.PersonId;

            await VerifyAuthenticationTokenAsync(token);

            var directiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayrollDepositDirective, Dtos.Base.PayrollDepositDirective>();
            var directiveDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Base.PayrollDepositDirective, Domain.HumanResources.Entities.PayrollDepositDirective>();

            var domainDirectiveToUpdate = directiveDtoToEntityAdapter.MapToType(payrollDepositDirective);

            var updatedDomainDirectives = await payrollDepositDirectivesRepository.UpdatePayrollDepositDirectivesAsync(new Domain.HumanResources.Entities.PayrollDepositDirectiveCollection(CurrentUser.PersonId) { domainDirectiveToUpdate });
            
            if (updatedDomainDirectives == null)
            {
                var message = "Null PayrollDepositDirectives returned from repository";
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }
            logger.Debug("Successfully Updated Payroll Deposit Directives");
            var updatedDirective = updatedDomainDirectives.First(dir => dir.Id == payrollDepositDirective.Id);
            logger.Debug("************ End - Service to Update Single Payroll Deposit Directives - End ************");
            return directiveEntityToDtoAdapter.MapToType(updatedDirective);
        }

        /// <summary>
        /// Creates a Payroll Deposit Directive
        /// </summary>
        /// <param name="token"></param>
        /// <param name="payrollDepositDirective"></param>
        /// <returns>a task awaiting the creation of a payrollDepositDirective</returns>
        public async Task<PayrollDepositDirective> CreatePayrollDepositDirectiveAsync(string token, PayrollDepositDirective payrollDepositDirective)
        {
            if (payrollDepositDirective == null)
            {
                logger.Debug("Payroll Deposit Directive cannot be null or empty");
                throw new ArgumentNullException("payrollDepositDirective");
            }
            if (CurrentUser.PersonId != payrollDepositDirective.PersonId)
            {
                var message = string.Format("Current user {0} cannot create deposit for person {1}", CurrentUser.PersonId, payrollDepositDirective.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            await VerifyAuthenticationTokenAsync(token);
            logger.Debug("************ Start - Service to Create Payroll Deposit Directive - Start ************");
            var directiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayrollDepositDirective, Dtos.Base.PayrollDepositDirective>();
            var directiveDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Base.PayrollDepositDirective, Domain.HumanResources.Entities.PayrollDepositDirective>();

            var domainDirectiveToCreate = directiveDtoToEntityAdapter.MapToType(payrollDepositDirective);

            var createdDomainDirective = await payrollDepositDirectivesRepository.CreatePayrollDepositDirectiveAsync(directiveDtoToEntityAdapter.MapToType(payrollDepositDirective));
            if (createdDomainDirective == null)
            {
                var message = "Null PayrollDepositDirective returned from repository";
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }
            logger.Debug("Successfully Created Payroll Deposit Directive");
            logger.Debug("************ End - Service to Create Payroll Deposit Directive - End ************");
            return directiveEntityToDtoAdapter.MapToType(createdDomainDirective);
        }

        /// <summary>
        /// Deletes a single payrollDepositDirective
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns>a bool awaiting the deletion of a record</returns>
        public async Task<bool> DeletePayrollDepositDirectiveAsync(string token, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Debug("Payroll Deposit Directive Id cannot be null or empty");
                throw new ArgumentNullException("id");
            }

            await VerifyAuthenticationTokenAsync(token);
            logger.Debug("************ Start - Service to Delete Payroll Deposit Directive - Start ************");
            var isSuccess = await payrollDepositDirectivesRepository.DeletePayrollDepositDirectiveAsync(id, CurrentUser.PersonId);
            if (isSuccess)
            {
                logger.Debug("Successfully deleted Payroll Deposit Directive");
            }
            logger.Debug("************ End - Service to Delete Payroll Deposit Directive - End ************");
            return isSuccess;
        }

        /// <summary>
        /// Deletes one or more payrollDepositDirective
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ids"></param>
        /// <returns>a bool awaiting the deletion of records</returns>
        public async Task<bool> DeletePayrollDepositDirectivesAsync(string token, IEnumerable<string> ids)
        {
            if (!ids.Any())
            {
                logger.Debug("Payroll Deposit Directive Ids cannot be null or empty");
                throw new ArgumentNullException("ids");
            }

            await VerifyAuthenticationTokenAsync(token);
            logger.Debug("************ Start - Service to Delete Multiple Payroll Deposit Directive - Start ************");
            var isSuccess = await payrollDepositDirectivesRepository.DeletePayrollDepositDirectivesAsync(ids, CurrentUser.PersonId);
            logger.Debug("************ End - Service to Delete Multiple Payroll Deposit Directive - End ************");
            return isSuccess;
        }

        /// <summary>
        /// Verify the consumer is requesting authentication for a deposit directive owned by the Current User.
        /// Then invoke the repository to authenticate the accountId matches the deposit directive's accountId.
        /// </summary>
        /// <param name="depositDirectiveId">Required when the user has existing directives</param>
        /// <param name="accountId">Required when the user has existing directives</param>
        /// <returns></returns>
        public async Task<BankingAuthenticationToken> AuthenticateCurrentUserAsync(string depositDirectiveId, string accountId)
        {
            var bankingInformationConfiguration = await bankingInformationConfigurationRepository.GetBankingInformationConfigurationAsync();
            if (bankingInformationConfiguration == null)
            {
                logger.Debug("Unable to retrieve banking information configuration while processing payroll deposit directive authentication request.");
                throw new ApplicationException("Unable to retrieve banking information configuration while processing payroll deposit directive authentication request.");
            }
            if (bankingInformationConfiguration.IsAccountAuthenticationDisabled)
            {
                logger.Debug("Cannot process payroll deposit step-up authentication request. Step-up authentication is disabled.");
                throw new ApplicationException("Cannot process payroll deposit step-up authentication request. Step-up authentication is disabled.");
            }
            logger.Debug("************ Start - Service to Authenticate Current User - Start ************");
            var depositDirectiveEntities = await payrollDepositDirectivesRepository.GetPayrollDepositDirectivesAsync(CurrentUser.PersonId);


            //the depositDirective id is allowed to be null or empty when the Current User has no existing directives.
            //if any exist, throw an exception
            if (string.IsNullOrEmpty(depositDirectiveId))
            {
                if (depositDirectiveEntities != null && depositDirectiveEntities.Any())
                {
                    logger.Debug("Current User must authenticate with an existing deposit directive");
                    throw new PermissionsException("CurrentUser must authenticate with an existing deposit directive");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    logger.Debug("Account Id cannot be null or Empty");
                    throw new ArgumentNullException("accountId");
                }
                //if the directive id was passed in, verify the Current User owns a directive with that id.
                if (depositDirectiveEntities == null ||
                    depositDirectiveEntities.FirstOrDefault(d => d.Id == depositDirectiveId) == null)
                {
                    logger.Debug("Current user is not authorized to update deposit Directive Id");
                    throw new PermissionsException("Current user is not authorized to update depositDirectiveId " + depositDirectiveId);
                }
            }

            //go down to the repository
            try
            {
                var authenticationTokenEntity = await payrollDepositDirectivesRepository.AuthenticatePayrollDepositDirective(CurrentUser.PersonId, depositDirectiveId, accountId);
                var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.BankingAuthenticationToken, BankingAuthenticationToken>();
                logger.Debug("Current user successfully authenticated");
                logger.Debug("************ End - Service to Authenticate Current User - End ************");
                return adapter.MapToType(authenticationTokenEntity);
            }
            catch (ColleagueSessionExpiredException)
            {
                logger.Error("Colleague Session expired");
                throw;
            }
            catch (BankingAuthenticationException bae)
            {
                logger.Error("Authentication failed");
                throw new PermissionsException("Authentication failed\n" + bae.Message, bae);
            }
            
        }

        /// <summary>
        /// Helper method to verify the incoming token on requests to update and create PayrollDepositDirectives
        /// If account step-up authentication is disabled, does nothing
        /// If account step-up authentication is enabled, checks the given token and throws an exception if it is invalid
        /// </summary>
        /// <param name="token">Step-up authentication token to validate</param>
        private async Task VerifyAuthenticationTokenAsync(string token)
        {
            var bankingInformationConfiguration = await bankingInformationConfigurationRepository.GetBankingInformationConfigurationAsync();
            logger.Debug("************ Start - Verifying authentication token - Start ************");
            if (bankingInformationConfiguration == null)
            {
                logger.Debug("Unable to retrieve banking information configuration during token authentication");
                throw new ApplicationException("Unable to retrieve banking information configuration during token authentication.");
            }
            if (!bankingInformationConfiguration.IsAccountAuthenticationDisabled)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    logger.Debug("Authentication token cannot be null or empty");
                    throw new ArgumentNullException("token");
                }

                Guid parsedToken;
                if (!Guid.TryParse(token, out parsedToken))
                {
                    logger.Debug("token format is invalid. must be a guid");
                    throw new ArgumentException("token format is invalid. must be a guid", "token");
                }

                Domain.Base.Entities.BankingAuthenticationToken authenticationToken;
                try
                {
                    
                    authenticationToken = await bankingAuthenticationClaimRepository.Get(parsedToken);
                    logger.Debug("Successfully verified token");
                    logger.Debug("************ End - Verifying authentication token - End ************");
                }
                catch (Exception e)
                {
                    logger.Error("Token is invalid");
                    throw new PermissionsException("Token is invalid\n" + e.Message, e);
                }

                if (authenticationToken == null || authenticationToken.ExpirationDateTimeOffset < DateTimeOffset.Now)
                {
                    logger.Error("Token is expired");
                    throw new PermissionsException("Token is expired");
                }
            }
        }
    }
}
