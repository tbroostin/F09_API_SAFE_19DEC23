/*Copyright 2017-2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// PayableDepositDirective Service
    /// </summary>
    [RegisterType]
    public class PayableDepositDirectiveService : BaseCoordinationService, IPayableDepositDirectiveService
    {

        private readonly IPayableDepositDirectiveRepository payableDepositDirectiveRepository;
        private readonly IBankingAuthenticationClaimRepository bankingAuthenticationClaimRepository;
        private IBankingInformationConfigurationRepository bankingInformationConfigurationRepository;

        /// <summary>
        /// Constructor for PayableDepositDirectiveService
        /// </summary>
        /// <param name="payableDepositDirectiveRepository"></param>
        /// <param name="bankingAuthenticationClaimRepository"></param>
        /// <param name="bankingInformationConfigurationRepository"
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public PayableDepositDirectiveService(
            IPayableDepositDirectiveRepository payableDepositDirectiveRepository,
            IBankingAuthenticationClaimRepository bankingAuthenticationClaimRepository,
            IBankingInformationConfigurationRepository bankingInformationConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.payableDepositDirectiveRepository = payableDepositDirectiveRepository;
            this.bankingAuthenticationClaimRepository = bankingAuthenticationClaimRepository;
            this.bankingInformationConfigurationRepository = bankingInformationConfigurationRepository;
        }

        /// <summary>
        /// Get the Payable Deposit Directives for the current user
        /// </summary>
        /// <returns>A list of PayableDepositDirectives</returns>
        public async Task<IEnumerable<PayableDepositDirective>> GetPayableDepositDirectivesAsync()
        {
            var domainPayableDepositDirectives = await payableDepositDirectiveRepository.GetPayableDepositDirectivesAsync(CurrentUser.PersonId);
            if (domainPayableDepositDirectives == null)
            {
                logger.Warn("Unexpected null domainPayableDepositDirectives returned from payableDepositDirectiveRepository.GetPayableDepositDirectivesAsync");
                return new List<PayableDepositDirective>();
            }
            logger.Debug(string.Format("************Start- Service to Get Payable Deposit Directive for {0}- Start************", CurrentUser.PersonId));
            var dtoPayableDepositDirectives = new List<PayableDepositDirective>();
            var payableDepositDirectiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PayableDepositDirective, Dtos.Base.PayableDepositDirective>();

            foreach (var directive in domainPayableDepositDirectives)
            {
                dtoPayableDepositDirectives.Add(payableDepositDirectiveEntityToDtoAdapter.MapToType(directive));
            }
            logger.Debug(string.Format("************End- Service to Get Payable Deposit Directive for {0} - End************", CurrentUser.PersonId));
            return dtoPayableDepositDirectives;

        }

        /// <summary>
        /// Gets a payable deposit directive for the current user with a payableDepositDirectiveId
        /// </summary>
        /// <param name="payableDepositDirectiveId"></param>
        /// <returns></returns>
        public async Task<PayableDepositDirective> GetPayableDepositDirectiveAsync(string payableDepositDirectiveId)
        {
            if (string.IsNullOrEmpty(payableDepositDirectiveId))
            {
                logger.Debug("************Payable Deposit Directive Id must be provided ************");
                throw new ArgumentNullException("payableDepositDirectiveId");
            }
            logger.Debug(string.Format("************Start- Service to Get Payable Deposit Directive for {0} - Start************",CurrentUser.PersonId));
            var domainPayableDepositDirectiveListOfOne = await payableDepositDirectiveRepository.GetPayableDepositDirectivesAsync(CurrentUser.PersonId, payableDepositDirectiveId);
            if (domainPayableDepositDirectiveListOfOne == null || !domainPayableDepositDirectiveListOfOne.Any())
            {
                var message = "Null domainPayableDepositDirectives returned from payableDepositDirectiveRepository.GetPayableDepositDirectiveAsync";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var payableDepositDirectiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PayableDepositDirective, PayableDepositDirective>();
            logger.Debug(string.Format("************End- Service to Get Payable Deposit Directive for {0} - End************", CurrentUser.PersonId));
            return payableDepositDirectiveEntityToDtoAdapter.MapToType(domainPayableDepositDirectiveListOfOne.First());

        }

        /// <summary>
        /// Create a PayableDepositDirective for the current user
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="newPayableDepositDirective"></param>
        /// <returns></returns>
        public async Task<PayableDepositDirective> CreatePayableDepositDirectiveAsync(string token, PayableDepositDirective newPayableDepositDirective)
        {
            if (newPayableDepositDirective == null)
            {
                logger.Debug("************New Payable Deposit Directive must be provided ************");
                throw new ArgumentNullException("newPayableDepositDirective");
            }

            if (!CurrentUser.IsPerson(newPayableDepositDirective.PayeeId))
            {
                var message = string.Format("Access to deposit directives for {0} is denied for current user {1}. Self access permitted only.", newPayableDepositDirective.PayeeId, CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // current user must have permissions as an employee or vendor to create a payable deposit...
            if (!(HasPermission(BasePermissionCodes.EditEChecksBankingInformation) || HasPermission(BasePermissionCodes.EditVendorBankingInformation)))
            {
                var message = string.Format("User {0} does not have permissions to create payable deposit.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            await VerifyAuthenticationTokenAsync(token);

            //convert the PayableDepositDirective DTO to a PayableDepositDirective Domain entity using a custom adapter
            var payableDepositDirectiveDtoToEntityAdapter = _adapterRegistry.GetAdapter<PayableDepositDirective, Domain.Base.Entities.PayableDepositDirective>();
            var inputPayableDepositDirectiveEntity = payableDepositDirectiveDtoToEntityAdapter.MapToType(newPayableDepositDirective);

            logger.Debug("************Start- Service to Create Payable Deposit Directive - Start************");
            //pass new payableDepositDirective domain entity to repository
            var newPayableDepositDirectiveEntity = await payableDepositDirectiveRepository.CreatePayableDepositDirectiveAsync(inputPayableDepositDirectiveEntity);

           

            if (newPayableDepositDirectiveEntity == null)
            {
                var message = string.Format("Unexpected null returned from CreatePayableDepositDirectiveAsync for personId {0} ", newPayableDepositDirective.PayeeId);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            //convert the Domain entity to DTO
            var payableDepositDirectiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PayableDepositDirective, PayableDepositDirective>();
            logger.Debug("************End- Service to Create Payable Deposit Directive - End************");
            return payableDepositDirectiveEntityToDtoAdapter.MapToType(newPayableDepositDirectiveEntity);
        }

        /// <summary>
        /// Updates the selected payable deposit directive for the current user
        /// </summary>
        /// <param name="inputPayableDepositDirective"></param>
        /// <returns></returns>
        public async Task<PayableDepositDirective> UpdatePayableDepositDirectiveAsync(string token, PayableDepositDirective inputPayableDepositDirective)
        {
            if (inputPayableDepositDirective == null)
            {
                logger.Debug("************Payable Deposit Directive Object must be provided ************");
                throw new ArgumentNullException("inputPayableDepositDirective");
            }

            if (string.IsNullOrEmpty(inputPayableDepositDirective.Id))
            {
                logger.Debug("************Id of input directive must be provided ************");
                throw new ArgumentException("id of input directive is required", "inputPayableDepositDirective");
            }
            if (string.IsNullOrEmpty(inputPayableDepositDirective.PayeeId))
            {
                logger.Debug("************Payee Id of input directive is required ************");
                throw new ArgumentException("payee Id of input directive is required", "inputPayableDepositDirective");
            }

            if (!CurrentUser.IsPerson(inputPayableDepositDirective.PayeeId))
            {
                var message = string.Format("Access to deposit directives for {0} is denied for current user {1}. Self access permitted only.", inputPayableDepositDirective.PayeeId, CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }



            // current user must have permissions as an employee or vendor to update a payable deposit...
            if (!(HasPermission(BasePermissionCodes.EditEChecksBankingInformation) || HasPermission(BasePermissionCodes.EditVendorBankingInformation)))
            {
                var message = string.Format("User {0} does not have permissions to update payable deposit.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            await VerifyAuthenticationTokenAsync(token);
            logger.Debug("************Start- Service to Update Payable Deposit Directive - Start************");
            //convert the PayableDepositDirective DTO to a PayableDepositDirective Domain entity using a custom adapter
            var payableDepositDirectiveDtoToEntityAdapter = _adapterRegistry.GetAdapter<PayableDepositDirective, Domain.Base.Entities.PayableDepositDirective>();
            var payableDepositDirectiveEntityToUpdate = payableDepositDirectiveDtoToEntityAdapter.MapToType(inputPayableDepositDirective);

            logger.Debug("************ Fetching payable deposit directives ************");
            var currentPayableDepositDirectiveListOfOne = await payableDepositDirectiveRepository.GetPayableDepositDirectivesAsync(inputPayableDepositDirective.PayeeId, inputPayableDepositDirective.Id);
            if (currentPayableDepositDirectiveListOfOne == null || !currentPayableDepositDirectiveListOfOne.Any())
            {
                var message = string.Format("Payable Deposit Directive {0} does not exist", inputPayableDepositDirective.Id);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            logger.Debug("************ Successfully Fetched payable deposit directives ************");

            var currentPayableDepositDirective = currentPayableDepositDirectiveListOfOne.First();
            logger.Debug("************ Updating payable deposit directive ************");
            var updatedPayableDepositDirectiveEntity = await payableDepositDirectiveRepository.UpdatePayableDepositDirectiveAsync(payableDepositDirectiveEntityToUpdate);

            if (updatedPayableDepositDirectiveEntity == null)
            {
                var message = "Unexpected null returned from payableDepositDirectiveRepository.UpdatePayableDepositDirectiveAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            logger.Debug("************ Successfully Updated payable deposit directive ************");
            //convert the Domain entity to DTO
            var payableDepositDirectiveEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PayableDepositDirective, PayableDepositDirective>();
            logger.Debug("************End- Service to Update Payable Deposit Directive - End************");
            return payableDepositDirectiveEntityToDtoAdapter.MapToType(updatedPayableDepositDirectiveEntity);

        }

        /// <summary>
        /// Deletes the selected payable deposit directive for the current user
        /// </summary>
        /// <param name="payableDepositDirectiveId"></param>
        /// <returns></returns>
        public async Task DeletePayableDepositDirectiveAsync(string token, string payableDepositDirectiveId)
        {
            if (string.IsNullOrEmpty(payableDepositDirectiveId))
            {
                logger.Debug("************Payable Deposit Directive Id must be provided ************");
                throw new ArgumentNullException("payableDepositDirectiveId");
            }

            // current user must have permissions as an employee or vendor to update a payable deposit...
            if (!(HasPermission(BasePermissionCodes.EditEChecksBankingInformation) || HasPermission(BasePermissionCodes.EditVendorBankingInformation)))
            {
                var message = string.Format("User {0} does not have permissions to update payable deposit.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            logger.Debug("************Start- Service to Delete Payable Deposit Directive - Start************");
            await VerifyAuthenticationTokenAsync(token);

            logger.Debug("************ Fetching payable deposit directives ************");
            var payableDepositDirectiveListOfOne = await payableDepositDirectiveRepository.GetPayableDepositDirectivesAsync(CurrentUser.PersonId, payableDepositDirectiveId);
            if (payableDepositDirectiveListOfOne == null || !payableDepositDirectiveListOfOne.Any())
            {
                var message = "Null or empty payableDepositDirectiveList returned from payableDepositDirectiveRepository.GetPayableDepositDirectiveAsync during payableDepositDirectiveService.DeletePayableDepositAsync";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            logger.Debug("************ Successfully Fetched payable deposit directives ************");
            var deletedPayableDepositDirective = payableDepositDirectiveListOfOne.First();

            if (!CurrentUser.IsPerson(deletedPayableDepositDirective.PayeeId))
            {
                var message = string.Format("Access to deposit directives for {0} is denied for current user {1}. Self access permitted only.", deletedPayableDepositDirective.PayeeId, CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            logger.Debug("************ Deleting payable deposit directive ************");
            await payableDepositDirectiveRepository.DeletePayableDepositDirectiveAsync(payableDepositDirectiveId);
            logger.Debug("************ Successfully Deleted payable deposit directive ************");
            logger.Debug("************End- Service to Delete Payable Deposit Directive - End************");
        }

        /// <summary>
        /// Get an authentication token to create or update a payable deposit directive
        /// </summary>
        /// <param name="depositDirectiveId"></param>
        /// <param name="accountId"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public async Task<BankingAuthenticationToken> AuthenticatePayableDepositDirectiveAsync(string depositDirectiveId, string accountId, string addressId)
        {
            var bankingInformationConfiguration = await bankingInformationConfigurationRepository.GetBankingInformationConfigurationAsync();
            if (bankingInformationConfiguration == null)
            {
                logger.Debug("************Unable to retrieve banking information configuration while processing payable deposit directive authentication request. ************");
                throw new ApplicationException("Unable to retrieve banking information configuration while processing payable deposit directive authentication request.");
            }
            if (bankingInformationConfiguration.IsAccountAuthenticationDisabled)
            {
                logger.Debug("************Cannot process payable deposit step-up authentication request. Step-up authentication is disabled. ************");
                throw new ApplicationException("Cannot process payable deposit step-up authentication request. Step-up authentication is disabled.");
            }
            logger.Debug("************Start- Service to Authenticate Payable Deposit Directive - Start************");
            var depositDirectiveEntities = await payableDepositDirectiveRepository.GetPayableDepositDirectivesAsync(CurrentUser.PersonId);

            //the depositDirective id is allowed to be null or empty when the Current User has no existing directives.
            //if any exist, throw an exception
            if (string.IsNullOrEmpty(depositDirectiveId))
            {
                //if (depositDirectiveEntities == null || !depositDirectiveEntities.Any())
                var hasAdressSpecificDirectives = (depositDirectiveEntities == null) ?
                    false :
                    depositDirectiveEntities.Where(dep => dep.AddressId == addressId).Any();

                if (hasAdressSpecificDirectives)
                {
                    logger.Debug("************Current User must authenticate with an existing deposit directive ************");
                    throw new PermissionsException("Current User must authenticate with an existing deposit directive");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    logger.Debug("************ Account Id cannot be null or empty ************");
                    throw new ArgumentNullException(accountId);
                }

                //if the directive id was passed in, verify the Current User owns a directive with that id.
                if (depositDirectiveEntities == null ||
                    depositDirectiveEntities.FirstOrDefault(d => d.Id == depositDirectiveId) == null)
                {
                    logger.Debug("************ Current user is not authorized to update Deposit Directive Id ************");
                    throw new PermissionsException("Current user is not authorized to update depositDirectiveId");
                }
            }

            var authenticationTokenEntity = await payableDepositDirectiveRepository.AuthenticatePayableDepositDirectiveAsync(CurrentUser.PersonId, depositDirectiveId, accountId, addressId);
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.BankingAuthenticationToken, BankingAuthenticationToken>();
            logger.Debug("************End- Service to Authenticate Payable Deposit Directive - End************");
            return adapter.MapToType(authenticationTokenEntity);
        }

        /// <summary>
        /// Helper method to verify the incoming token on requests to update and create PayableDepositDirectives
        /// If account step-up authentication is disabled, does nothing
        /// If account step-up authentication is enabled, checks the given token and throws an exception if it is invalid
        /// </summary>
        /// <param name="token">Step-up authentication token to validate</param>
        private async Task VerifyAuthenticationTokenAsync(string token)
        {
            var bankingInformationConfiguration = await bankingInformationConfigurationRepository.GetBankingInformationConfigurationAsync();
            if (bankingInformationConfiguration == null)
            {
                logger.Debug("************ Unable to retrieve banking information configuration during token authentication. ************");
                throw new ApplicationException("Unable to retrieve banking information configuration during token authentication.");
            }
            if (!bankingInformationConfiguration.IsAccountAuthenticationDisabled)
            {
                logger.Debug("************Start- Service to Verify authentication token - Start************");
                if (string.IsNullOrWhiteSpace(token))
                {
                    logger.Debug("************ Token cannot be null or empty ************");
                    throw new ArgumentNullException("token");
                }

                Guid parsedToken;
                if (!Guid.TryParse(token, out parsedToken))
                {
                    logger.Debug("************ token format is invalid. must be a guid ************");
                    throw new ArgumentException("token format is invalid. must be a guid", "token");
                }

                Domain.Base.Entities.BankingAuthenticationToken authenticationToken;
                try
                {
                    authenticationToken = await bankingAuthenticationClaimRepository.Get(parsedToken);
                }
                catch (Exception e)
                {
                    logger.Debug("************ Token is invalid ************");
                    throw new PermissionsException("Token is invalid\n" + e.Message, e);
                }

                if (authenticationToken.ExpirationDateTimeOffset < DateTimeOffset.Now)
                {
                    logger.Debug("************ Token is expired ************");
                    throw new PermissionsException("Token is expired");
                }
                logger.Debug("************End- Service to Verify authentication token - End************");
            }
        }
    }
}
