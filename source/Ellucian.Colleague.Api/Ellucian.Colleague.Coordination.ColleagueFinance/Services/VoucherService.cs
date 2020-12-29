// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IVoucherService interface.
    /// </summary>
    [RegisterType]
    public class VoucherService : BaseCoordinationService, IVoucherService
    {
        private IVoucherRepository voucherRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;
        private IStaffRepository staffRepository;

        // This constructor initializes the private attributes.
        public VoucherService(IVoucherRepository voucherRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IStaffRepository staffRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.voucherRepository = voucherRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
            this.staffRepository = staffRepository;
        }

        /// <summary>
        /// Returns the DTO for the specified voucher.
        /// </summary>
        /// <param name="id">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        [Obsolete("OBSOLETE as of API 1.15. Please use GetVoucher2Async")]
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.Voucher> GetVoucherAsync(string id)
        {
            // Check the permission code to view a voucher.
            CheckViewVoucherPermission();

            // Get the GL Configuration so we know how to format the GL numbers, and get the full GL access role
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "glClassConfiguration cannot be null");
            }

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
            if (generalLedgerUser == null)
            {
                throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
            }

            // Get the voucher domain entity from the repository.
            int versionNumber = 1;
            var voucherDomainEntity = await voucherRepository.GetVoucherAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts, versionNumber);

            if (voucherDomainEntity == null)
            {
                throw new ArgumentNullException("voucherDomainEntity", "voucherDomainEntity cannot be null.");
            }

            // Convert the project and all its child objects into DTOs.
            var voucherDtoAdapter = new VoucherEntityToDtoAdapter(_adapterRegistry, logger);
            var voucherDto = voucherDtoAdapter.MapToType(voucherDomainEntity, glConfiguration.MajorComponentStartPositions);

            // Throw an exception if there are no line items being returned since access to the document
            // is governed by access to the GL numbers on the line items, and a line item will not be returned
            // if the user does not have access to at least one of the line items.
            if (voucherDto.LineItems == null || voucherDto.LineItems.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access voucher.");
            }

            return voucherDto;
        }

        /// <summary>
        /// Returns the DTO for the specified voucher.
        /// </summary>
        /// <param name="id">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2> GetVoucher2Async(string id)
        {
            // Check the permission code to view a voucher.
            CheckViewVoucherPermission();

            // Get the GL Configuration so we know how to format the GL numbers, and get the full GL access role
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "glClassConfiguration cannot be null");
            }

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
            }

            // Get the voucher domain entity from the repository.
            int versionNumber = 2;
            var voucherDomainEntity = await voucherRepository.GetVoucherAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts, versionNumber);

            if (voucherDomainEntity == null)
            {
                throw new ArgumentNullException("voucherDomainEntity", "voucherDomainEntity cannot be null.");
            }

            await AssignGlDescription(glConfiguration, voucherDomainEntity);

            // Convert the project and all its child objects into DTOs.
            var voucherDtoAdapter = new Voucher2EntityToDtoAdapter(_adapterRegistry, logger);
            var voucherDto = voucherDtoAdapter.MapToType(voucherDomainEntity, glConfiguration.MajorComponentStartPositions);

            // Throw an exception if there are no line items being returned since access to the document
            // is governed by access to the GL numbers on the line items, and a line item will not be returned
            // if the user does not have access to at least one of the line items.
            if (voucherDto.LineItems == null || voucherDto.LineItems.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access voucher.");
            }

            return voucherDto;
        }

        /// <summary>
        /// Get Voucher summary list for the given user
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<VoucherSummary>> GetVoucherSummariesAsync(string personId)
        {
            List<VoucherSummary> voucherSummaryDtos = new List<VoucherSummary>();

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }

            // check if personId passed is same currentuser
            CheckIfUserIsSelf(personId);

            //check if personId has staff record            
            await CheckStaffRecordAsync(personId);

            // Check the permission code to view a voucher.
            CheckViewVoucherPermission();

            // Get the list of voucher summary domain entity from the repository
            var voucherSummaryDomainEntities = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);

            if (voucherSummaryDomainEntities == null || !voucherSummaryDomainEntities.Any())
            {
                return voucherSummaryDtos;
            }
            //sorting
            var sortOrderSequence = new List<string> { Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.InProgress.ToString(), Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.NotApproved.ToString(), Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Outstanding.ToString(), Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Paid.ToString(), Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Reconciled.ToString(), Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Voided.ToString() };
            voucherSummaryDomainEntities = voucherSummaryDomainEntities.OrderBy(item => sortOrderSequence.IndexOf(item.Status.ToString())).ThenByDescending(item=> item.Id);

            // Convert the voucher summary and all its child objects into DTOs
            var voucherSummaryDtoAdapter = new VoucherSummaryEntityDtoAdapter(_adapterRegistry, logger);
            foreach (var voucherDomainEntity in voucherSummaryDomainEntities)
            {
                voucherSummaryDtos.Add(voucherSummaryDtoAdapter.MapToType(voucherDomainEntity));
            }

            return voucherSummaryDtos;
        }


        /// <summary>
        /// Create/Update a voucher.
        /// </summary>
        /// <param name="voucherCreateUpdateRequest">The voucher create update request DTO.</param>        
        /// <returns>The voucher create update response DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateResponse> CreateUpdateVoucherAsync(Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest voucherCreateUpdateRequest)
        {
            if (voucherCreateUpdateRequest == null)
            {
                throw new ArgumentNullException("voucherCreateUpdateRequest", "Must provide a VoucherCreateUpdateRequest object");
            }

            if (string.IsNullOrEmpty(voucherCreateUpdateRequest.PersonId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }
            if (voucherCreateUpdateRequest.Voucher == null)
            {
                throw new ArgumentNullException("voucher", "Voucher must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(voucherCreateUpdateRequest.PersonId);

            //check if personId has staff record
            await CheckStaffRecordAsync(voucherCreateUpdateRequest.PersonId);

            //Change to create or update permission, after creating new permission.            
            CheckVoucherCreateUpdatePermission();

            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "glClassConfiguration cannot be null");
            }

            //Convert DTO to domain entity            
            var voucherCreateUpdateRequestEntity = ConvertCreateUpdateRequestDtoToEntity(voucherCreateUpdateRequest, glConfiguration, _adapterRegistry, logger);
            Domain.ColleagueFinance.Entities.VoucherCreateUpdateResponse responseEntity = null;

            //Check if voucher sent for modify / create.            
            if (string.IsNullOrEmpty(voucherCreateUpdateRequest.Voucher.VoucherId))
            {
                responseEntity = await voucherRepository.CreateVoucherAsync(voucherCreateUpdateRequestEntity);
            }
            else
            {
                // Get the voucher domain entity from the repository.
                int versionNumber = 2;

                // Get the ID for the person who is logged in, and use the ID to get their GL access level.
                var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
                if (generalLedgerUser == null)
                {
                    throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
                }

                // Get the voucher domain entity from the repository
                var originalVoucher = await voucherRepository.GetVoucherAsync(voucherCreateUpdateRequestEntity.Voucher.Id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts, versionNumber);
                if (originalVoucher == null)
                {
                    var message = string.Format("{0} Voucher doesn't exist for modify.", voucherCreateUpdateRequestEntity.Voucher.Id);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                responseEntity = await voucherRepository.UpdateVoucherAsync(voucherCreateUpdateRequestEntity, originalVoucher);
            }

            VoucherCreateUpdateResponse response = new VoucherCreateUpdateResponse();

            var createResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.VoucherCreateUpdateResponse, Dtos.ColleagueFinance.VoucherCreateUpdateResponse>();

            if (responseEntity != null)
            {
                response = createResponseAdapter.MapToType(responseEntity);
            }
            return response;
        }

        /// <summary>
        /// Gets Reimburse person address details for voucher
        /// </summary>
        /// <returns>Reimburse person address details for voucher as DTO response</returns>
        public async Task<Dtos.ColleagueFinance.VendorsVoucherSearchResult> GetReimbursePersonAddressForVoucherAsync()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult addressDtos = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult();

            // Check the permission code to view vendor information.
            CheckVoucherCreateUpdatePermission();

            // Get the address domain entity from the repository
            var personAddressDomainEntities = await voucherRepository.GetReimbursePersonAddressForVoucherAsync(CurrentUser.PersonId);

            if (personAddressDomainEntities == null)
            {
                return addressDtos;
            }

            // Convert the result into DTOs
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult, Dtos.ColleagueFinance.VendorsVoucherSearchResult>();

            addressDtos = dtoAdapter.MapToType(personAddressDomainEntities);

            return addressDtos;

        }

        /// <summary>
        /// Void a voucher.
        /// </summary>
        /// <param name="voucherVoidRequest">The voucher void request DTO.</param>        
        /// <returns>The voucher void response DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidResponse> VoidVoucherAsync(Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest voucherVoidRequest)
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidResponse response = new Dtos.ColleagueFinance.VoucherVoidResponse();
            if (voucherVoidRequest == null)
            {
                throw new ArgumentNullException("voucherVoidRequest", "Must provide a voucherVoidRequest object");
            }

            if (string.IsNullOrEmpty(voucherVoidRequest.PersonId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }

            if (string.IsNullOrEmpty(voucherVoidRequest.VoucherId))
            {
                throw new ArgumentNullException("VoucherId", "Voucher Id must be specified.");
            }

            if (string.IsNullOrEmpty(voucherVoidRequest.ConfirmationEmailAddresses))
            {
                throw new ArgumentNullException("confirmationEmailAddresses", "confirmationEmailAddresses must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(voucherVoidRequest.PersonId);

            //check if personId has staff record
            await CheckStaffRecordAsync(voucherVoidRequest.PersonId);

            //Change to create or update permission.            
            CheckVoucherCreateUpdatePermission();

            //Convert DTO to domain entity            
            var voucherVoidRequestEntity = ConvertVoidRequestDtoToEntity(voucherVoidRequest);
            Domain.ColleagueFinance.Entities.VoucherVoidResponse responseEntity = null;

            responseEntity = await voucherRepository.VoidVoucherAsync(voucherVoidRequestEntity);


            var createResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.VoucherVoidResponse, Dtos.ColleagueFinance.VoucherVoidResponse>();

            if (responseEntity != null)
            {
                response = createResponseAdapter.MapToType(responseEntity);
            }
            return response;
        }

        /// <summary>
        /// Get the list of voucher's by vendor id and invoice number.
        /// </summary>
        /// <param name="vendorId">Vendor Id</param>
        /// <param name="invoiceNo">Invoice number</param>
        /// <returns>List of <see cref="Voucher2">Vouchers</see></returns>
        public async Task<IEnumerable<Voucher2>> GetVouchersByVendorAndInvoiceNoAsync(string vendorId, string invoiceNo)
        {
            if (string.IsNullOrEmpty(vendorId))
                throw new ArgumentNullException("vendorId", "vendorId is required");

            if (string.IsNullOrEmpty(invoiceNo))
                throw new ArgumentNullException("invoiceNo", "invoice number is required");

            // Check the permission code to view voucher information.
            CheckViewVoucherPermission();

            // Get the GL Configuration so we know how to format the GL numbers, and get the full GL access role
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            List<Voucher2> voucherDtos = new List<Voucher2>();

            // Get the list of voucher id's from the repository
            var voucherDomainEntities = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNo);

            if (voucherDomainEntities != null && voucherDomainEntities.Any())
            {
                // Convert the voucher domain entity and all its child objects into voucher2 DTOs.
                var voucherDtoAdapter = new Voucher2EntityToDtoAdapter(_adapterRegistry, logger);

                foreach (var voucherDomainEntity in voucherDomainEntities)
                {
                    var voucherDto = voucherDtoAdapter.MapToType(voucherDomainEntity, glConfiguration.MajorComponentStartPositions);

                    voucherDtos.Add(voucherDto);
                }
            }

            return voucherDtos;
        }


        /// <summary>
        /// convert void voucher Request to Entity
        /// </summary>
        /// <param name="voucherVoidRequest"></param>
        /// <returns>VoucherVoidRequest</returns>
        private static Domain.ColleagueFinance.Entities.VoucherVoidRequest ConvertVoidRequestDtoToEntity(Dtos.ColleagueFinance.VoucherVoidRequest voucherVoidRequest)
        {
            Domain.ColleagueFinance.Entities.VoucherVoidRequest voidRequestEntity = new Domain.ColleagueFinance.Entities.VoucherVoidRequest();
            voidRequestEntity.PersonId = voucherVoidRequest.PersonId;
            voidRequestEntity.VoucherId = voucherVoidRequest.VoucherId;
            voidRequestEntity.ConfirmationEmailAddresses = voucherVoidRequest.ConfirmationEmailAddresses;
            voidRequestEntity.Comments = voucherVoidRequest.Comments;

            return voidRequestEntity;
        }

        /// <summary>
        /// Permission code that allows a READ operation on a voucher.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewVoucherPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewVoucher) || HasPermission(ColleagueFinancePermissionCodes.CreateUpdateVoucher);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view vouchers.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }


        /// <summary>
        /// Permission code that allows a WRITE operation on a voucher.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckVoucherCreateUpdatePermission()
        {

            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.CreateUpdateVoucher);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to create or modify Vouchers.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Determine if personId is has staff record
        /// </summary>
        /// <param name="personId">ID of person from data</param>
        /// <returns></returns>
        private async Task CheckStaffRecordAsync(string personId)
        {
            try
            {
                var staffRecord = await staffRepository.GetAsync(personId);
                if (staffRecord == null)
                {
                    var message = string.Format("{0} does not have staff record.", CurrentUser.PersonId);
                    logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            catch (KeyNotFoundException knfe)
            {
                var message = string.Format("{0} does not have staff record.", CurrentUser.PersonId);
                logger.Error(knfe, message);
                throw new PermissionsException(message);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Convert Voucher Dto to Entity
        /// </summary>
        /// <param name="voucher"></param>
        /// <param name="glAccountStructure"></param>
        /// <returns>Voucher Entity</returns>
        private static Domain.ColleagueFinance.Entities.Voucher ConvertVoucherDtoToEntity(Voucher2 voucher, Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure glAccountStructure, IAdapterRegistry _adapterRegistry, ILogger logger)
        {

            if (voucher == null)
                throw new ArgumentNullException("Voucher", "Must provide voucher object");

            var voucherStatus = ConvertVoucherStatusDtoToEntity(voucher.Status);
            var voucherId = !(string.IsNullOrEmpty(voucher.VoucherId)) ? voucher.VoucherId.Trim() : "NEW";

            var voucherEntity = new Domain.ColleagueFinance.Entities.Voucher(
                voucherId, voucher.Date, voucherStatus,voucher.VendorName);


            if ((voucher.Approvers != null) && (voucher.Approvers.Any()))
            {
                foreach (var approver in voucher.Approvers)
                {
                    if (approver != null && !string.IsNullOrEmpty(approver.ApproverId))
                    {
                        // Approver Initials needs to be uppercase
                        var approverEntity = new Domain.ColleagueFinance.Entities.Approver(approver.ApproverId.ToUpper());
                        voucherEntity.AddApprover(approverEntity);
                    }
                }
            }

            if ((voucher.LineItems != null) && (voucher.LineItems.Any()))
            {
                foreach (var lineItem in voucher.LineItems)
                {
                    if (lineItem != null)
                    {
                        var description = !(string.IsNullOrEmpty(lineItem.Description)) ? lineItem.Description.Trim() : string.Empty;
                        decimal quantity = lineItem.Quantity;

                        var lineItemId = !(string.IsNullOrEmpty(lineItem.Id)) ? lineItem.Id.Trim() : "NEW";

                        var apLineItem = new Domain.ColleagueFinance.Entities.LineItem(lineItemId, description, quantity, lineItem.Price, lineItem.ExtendedPrice);
                        apLineItem.VendorPart = !(string.IsNullOrEmpty(lineItem.VendorPart)) ? lineItem.VendorPart.Trim() : string.Empty;
                        apLineItem.UnitOfIssue = !(string.IsNullOrEmpty(lineItem.UnitOfIssue)) ? lineItem.UnitOfIssue.Trim() : string.Empty;
                        apLineItem.Comments = !(string.IsNullOrEmpty(lineItem.Comments)) ? lineItem.Comments.Trim() : string.Empty;
                        apLineItem.TradeDiscountPercentage = lineItem.TradeDiscountPercentage;
                        apLineItem.TradeDiscountAmount = lineItem.TradeDiscountAmount;
                        apLineItem.FixedAssetsFlag = !(string.IsNullOrEmpty(lineItem.FixedAssetsFlag)) ? lineItem.FixedAssetsFlag.Trim() : string.Empty;

                        apLineItem.CommodityCode = lineItem.CommodityCode;
                        apLineItem.ExpectedDeliveryDate = lineItem.ExpectedDeliveryDate;
                        apLineItem.TaxForm = lineItem.TaxForm;
                        apLineItem.TaxFormCode = lineItem.TaxFormCode;
                        apLineItem.TaxFormLocation = lineItem.TaxFormLocation;
                        
                        foreach (var glAccount in lineItem.GlDistributions)
                        {
                            string glAccountNo = !(string.IsNullOrEmpty(glAccount.FormattedGlAccount)) ? glAccount.FormattedGlAccount : "MASKED";
                            var internalGlAccountNo = GlAccountUtility.ConvertGlAccountToInternalFormat(glAccountNo, glAccountStructure.MajorComponentStartPositions);

                            apLineItem.AddGlDistributionForSave(new Domain.ColleagueFinance.Entities.LineItemGlDistribution(internalGlAccountNo, glAccount.Quantity, glAccount.Amount)
                            {
                                ProjectNumber = glAccount.ProjectNumber
                            });
                        }
                        if (voucherId.Equals("NEW"))
                        {
                            //add tax codes to the entity
                            if (lineItem.ReqLineItemTaxCodes != null && lineItem.ReqLineItemTaxCodes.Any())
                            {
                                foreach (var taxcode in lineItem.ReqLineItemTaxCodes)
                                {
                                    if (taxcode != null && !string.IsNullOrEmpty(taxcode.TaxReqTaxCode))
                                    {
                                        apLineItem.AddReqTax(new Domain.ColleagueFinance.Entities.LineItemReqTax(taxcode.TaxReqTaxCode));
                                    }
                                }
                            }
                        }
                        else
                        {
                            //add tax codes to the entity
                            if (lineItem.ReqLineItemTaxCodes != null && lineItem.ReqLineItemTaxCodes.Any())
                            {
                                foreach (var taxcode in lineItem.ReqLineItemTaxCodes)
                                {
                                    if (!string.IsNullOrEmpty(taxcode.TaxReqTaxCode))
                                    {
                                        apLineItem.AddReqTax(new Domain.ColleagueFinance.Entities.LineItemReqTax(taxcode.TaxReqTaxCode));
                                    }
                                }
                            }

                        }
                        voucherEntity.AddLineItem(apLineItem);
                    }
                }
            }
            //TODO: now vendor Id alone
            voucherEntity.VendorId = voucher.VendorId;

            if (!(string.IsNullOrWhiteSpace(voucher.ApType)))
            {
                voucherEntity.ApType = voucher.ApType.ToUpper();
            }
            if ((voucher.Approvers != null) && (voucher.Approvers.Any()))
            {
                foreach (var approver in voucher.Approvers)
                {
                    if (!string.IsNullOrEmpty(approver.ApproverId))
                    {
                        var approverEntity = new Domain.ColleagueFinance.Entities.Approver(approver.ApproverId);
                        voucherEntity.AddApprover(approverEntity);
                    }
                }
            }

            voucherEntity.Comments = voucher.Comments;
            voucherEntity.DueDate = voucher.DueDate;
            voucherEntity.InvoiceDate = voucher.InvoiceDate;
            voucherEntity.InvoiceNumber = voucher.InvoiceNumber;
            return voucherEntity;

        }

        /// <summary>
        /// convert create/Update Voucer Request to Entity
        /// </summary>
        /// <param name="voucherCreateUpdateRequest"></param>
        /// <param name="glAccountStructure"></param>
        /// <returns>VoucherCreateUpdateRequest</returns>
        private static Domain.ColleagueFinance.Entities.VoucherCreateUpdateRequest ConvertCreateUpdateRequestDtoToEntity(Dtos.ColleagueFinance.VoucherCreateUpdateRequest voucherCreateUpdateRequest, Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure glAccountStructure, IAdapterRegistry _adapterRegistry, ILogger logger)
        {
            Domain.ColleagueFinance.Entities.VoucherCreateUpdateRequest createUpdateRequestEntity = new Domain.ColleagueFinance.Entities.VoucherCreateUpdateRequest();
            createUpdateRequestEntity.PersonId = voucherCreateUpdateRequest.PersonId;
            createUpdateRequestEntity.ConfEmailAddresses = voucherCreateUpdateRequest.ConfEmailAddresses;
            createUpdateRequestEntity.Voucher = ConvertVoucherDtoToEntity(voucherCreateUpdateRequest.Voucher, glAccountStructure, _adapterRegistry, logger);

            if (voucherCreateUpdateRequest.VendorsVoucherInfo != null)
            {
                // Convert the vendor info and all its child objects into Entity
                var vendorInfoAdapter = new VendorsVoucherSearchResultDtoToEntityAdapter(_adapterRegistry, logger);
                createUpdateRequestEntity.VendorsVoucherInfo = vendorInfoAdapter.MapToType(voucherCreateUpdateRequest.VendorsVoucherInfo);
            }
            return createUpdateRequestEntity;
        }

        /// <summary>
        /// Converts a VoucherStatus DTO enumeration to a VoucherStatus domain enum 
        /// </summary>
        /// <param name="sourceStatus">VoucherStatus DTO enumeration</param>
        /// <returns><see cref="VoucherStatus">voucherStatus domain enum</returns>
        private static Domain.ColleagueFinance.Entities.VoucherStatus ConvertVoucherStatusDtoToEntity(Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus sourceStatus)
        {
            var voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.InProgress;

            switch (sourceStatus)
            {
                case (Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.InProgress):
                    voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.InProgress; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Cancelled):
                    voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.Cancelled; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.NotApproved):
                    voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.NotApproved; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Outstanding):
                    voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.Outstanding; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Paid):
                    voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.Paid; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Reconciled):
                    voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.Reconciled; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.VoucherStatus.Voided):
                    voucherStatus = Domain.ColleagueFinance.Entities.VoucherStatus.Voided; break;

                default:
                    break;
            }
            return voucherStatus;
        }

        private async Task AssignGlDescription(Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure glAccountStructure, Ellucian.Colleague.Domain.ColleagueFinance.Entities.Voucher voucherDomainEntity)
        {
            List<string> glAccountNumbers = new List<string>();
            foreach (var lineItem in voucherDomainEntity.LineItems)
            {
                foreach (var glDistribution in lineItem.GlDistributions)
                {
                    if (!glDistribution.Masked)
                    {
                        glAccountNumbers.Add(glDistribution.GlAccountNumber);
                    }
                }
            }
            if (glAccountNumbers.Any())
            {
                var glAccountDescriptionsDictionary = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(glAccountNumbers, glAccountStructure);
                foreach (var lineItem in voucherDomainEntity.LineItems)
                {
                    foreach (var glDistribution in lineItem.GlDistributions)
                    {
                        if (!glDistribution.Masked)
                        {
                            string description = "";
                            if (!string.IsNullOrEmpty(glDistribution.GlAccountNumber))
                            {
                                glAccountDescriptionsDictionary.TryGetValue(glDistribution.GlAccountNumber, out description);
                            }
                            glDistribution.GlAccountDescription = description;
                        }
                    }
                }
            }
        }
    }
}
