using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IReceiveProcurementsService interface
    /// </summary>
    [RegisterType]
    public class ReceiveProcurementsService : BaseCoordinationService, IReceiveProcurementsService
    {
        private IReceiveProcurementsRepository receiveProcurementsRepository;
        private readonly IConfigurationRepository configurationRepository;
        private IStaffRepository staffRepository;

        public ReceiveProcurementsService(IReceiveProcurementsRepository receiveProcurementsRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IStaffRepository staffRepository, 
            ILogger logger):base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository) {
            this.receiveProcurementsRepository = receiveProcurementsRepository;
            this.configurationRepository = configurationRepository;
            this.staffRepository = staffRepository;
        }
        public async Task<IEnumerable<ReceiveProcurementSummary>> GetReceiveProcurementsByPersonIdAsync(string personId)
        {
            List<Ellucian.Colleague.Dtos.ColleagueFinance.ReceiveProcurementSummary> receiveProcurementSummaryDtos = new List<Ellucian.Colleague.Dtos.ColleagueFinance.ReceiveProcurementSummary>();
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }

            // Check if personId passed is same currentuser
            CheckIfUserIsSelf(personId);

            // Check if personId has staff record            
            await CheckStaffRecordAsync(personId);

            // Check the permission code to view/update a procurement receiving.
            CheckReceiveProcurementsPermission();

            // Get the list of procurement recevingsummary domain entity from the repository
            var receiveProcurementSummaryDomainEntities = await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(personId);

            if (receiveProcurementSummaryDomainEntities == null || !receiveProcurementSummaryDomainEntities.Any())
            {
                return receiveProcurementSummaryDtos;
            }

            //sorting - by vendor name then by PO number
            receiveProcurementSummaryDomainEntities = receiveProcurementSummaryDomainEntities.OrderBy(p => string.IsNullOrEmpty(p.VendorInformation.VendorName) ? p.VendorInformation.VendorMiscName : p.VendorInformation.VendorName).ThenBy(x => x.Number);
            
            // Convert the receive procurement summary and all its child objects into DTOs
            var receiveProcurementDtoAdapter = new ReceiveProcurementsEntityToDtoAdapter(_adapterRegistry, logger);
            foreach (var receiveProcurementDomainEntity in receiveProcurementSummaryDomainEntities)
            {
                receiveProcurementSummaryDtos.Add(receiveProcurementDtoAdapter.MapToType(receiveProcurementDomainEntity));
            }
            
            return receiveProcurementSummaryDtos;
        }

        /// <summary>
        /// Used to Receive/Return Pocurement items
        /// </summary>
        /// <param name="procurementAcceptOrReturnItemInformationRequest"> The procurement accept/return request DTO.</param>
        /// <returns>The procurement accept/return response DTO.</returns>
        public async Task<ProcurementAcceptReturnItemInformationResponse> AcceptOrReturnProcurementItemsAsync(ProcurementAcceptReturnItemInformationRequest procurementAcceptReturnItemInformationRequest) {

            ProcurementAcceptReturnItemInformationResponse response = new ProcurementAcceptReturnItemInformationResponse();
            if (procurementAcceptReturnItemInformationRequest == null)
            {
                throw new ArgumentNullException("procurementAcceptReturnItemInformationRequest", "Must provide a procurementAcceptOrReturnItemInformationRequest object");
            }

            if (string.IsNullOrEmpty(procurementAcceptReturnItemInformationRequest.StaffUserId))
            {
                throw new ArgumentNullException("staffUserId", "staff user id must be specified.");
            }
            if (procurementAcceptReturnItemInformationRequest.ProcurementItemsInformation == null)
            {
                throw new ArgumentNullException("procurementItemsInformation", "ProcurementItemsInformation must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(procurementAcceptReturnItemInformationRequest.StaffUserId);

            //check if personId has staff record
            await CheckStaffRecordAsync(procurementAcceptReturnItemInformationRequest.StaffUserId);
            
            // Check the permission code to view/update a procurement receiving.
            CheckReceiveProcurementsPermission();

            //Convert DTO to domain entity            
            var procurementAcceptReturnItemInformationRequestEntity = ConvertAcceptReturnRequestDtoToEntity(procurementAcceptReturnItemInformationRequest);
            Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationResponse responseEntity = null;

            responseEntity = await receiveProcurementsRepository.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequestEntity);


            //var createResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationResponse, Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationResponse>();

            var receiveProcurementDtoAdapter = new ReceiveProcurementsResponseEntityToDtoAdapter(_adapterRegistry, logger);

            if (responseEntity != null)
            {
                response = receiveProcurementDtoAdapter.MapToType(responseEntity);
            }

            return response;
        }

        #region Private Methods
            /// <summary>
            /// Determine if personId is has staff record
            /// </summary>
            /// <param name="personId">ID of person from data</param>
            /// <returns></returns>
        private async Task CheckStaffRecordAsync(string personId)
        {
            var staffRecord = await staffRepository.GetAsync(personId);
            if (staffRecord == null)
            {
                var message = string.Format("{0} does not have staff record.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows a READ operation on a procurement receiving.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckReceiveProcurementsPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewUpdateProcurementReceiving);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view purchase orders for receiving items.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        private static Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationRequest ConvertAcceptReturnRequestDtoToEntity(Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationRequest procurementAcceptOrReturnItemInformationRequest)
        {
            Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationRequest acceptReturnItemRequestEntity = new Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationRequest();
            acceptReturnItemRequestEntity.AcceptAll = procurementAcceptOrReturnItemInformationRequest.AcceptAll;
            acceptReturnItemRequestEntity.IsPoFilterApplied = procurementAcceptOrReturnItemInformationRequest.IsPoFilterApplied;
            acceptReturnItemRequestEntity.PackingSlip = procurementAcceptOrReturnItemInformationRequest.PackingSlip;
            acceptReturnItemRequestEntity.StaffUserId = procurementAcceptOrReturnItemInformationRequest.StaffUserId;
            
            if (!(string.IsNullOrWhiteSpace(procurementAcceptOrReturnItemInformationRequest.ArrivedVia)))
            {
                acceptReturnItemRequestEntity.ArrivedVia = procurementAcceptOrReturnItemInformationRequest.ArrivedVia.ToUpper();
            }

            foreach (var item in procurementAcceptOrReturnItemInformationRequest.ProcurementItemsInformation) {
                acceptReturnItemRequestEntity.ProcurementItemsInformation.Add(ConvertItemInfoDtoToEntity(item));
            }
            
            return acceptReturnItemRequestEntity;
        }

        private static Domain.ColleagueFinance.Entities.ProcurementItemInformation ConvertItemInfoDtoToEntity(ProcurementItemInformation procurementItemInformation) {

            Domain.ColleagueFinance.Entities.ProcurementItemInformation itemEntity = 
                new Domain.ColleagueFinance.Entities.ProcurementItemInformation(procurementItemInformation.PurchaseOrderId, procurementItemInformation.PurchaseOrderNumber, procurementItemInformation.ItemId, procurementItemInformation.QuantityOrdered, procurementItemInformation.QuantityAccepted, procurementItemInformation.QuantityRejected, procurementItemInformation.ReturnDate, procurementItemInformation.ReturnReason);

            if (!(string.IsNullOrWhiteSpace(procurementItemInformation.ReturnVia)))
            {
                itemEntity.ReturnVia = procurementItemInformation.ReturnVia.ToUpper();
            }

            if (!(string.IsNullOrWhiteSpace(procurementItemInformation.ReturnReason)))
            {
                itemEntity.ReturnReason = procurementItemInformation.ReturnReason.ToUpper();
            }

            itemEntity.Vendor = procurementItemInformation.Vendor;
            itemEntity.ItemDescription = procurementItemInformation.ItemDescription;
            itemEntity.ItemMsdsFlag = procurementItemInformation.ItemMsdsFlag;
            itemEntity.ItemMsdsReceived = procurementItemInformation.ItemMsdsReceived;
            itemEntity.ReturnAuthorizationNumber = procurementItemInformation.ReturnAuthorizationNumber;
            itemEntity.ReturnComments = procurementItemInformation.ReturnComments;
            itemEntity.ReOrder = procurementItemInformation.ReOrder;
            itemEntity.ConfirmationEmail = procurementItemInformation.ConfirmationEmail;
            return itemEntity;
        }

        #endregion
    }
}
