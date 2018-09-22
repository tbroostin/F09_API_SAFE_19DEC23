/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Exposes Bank Routing Information functionality
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BanksController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IBankRepository bankRepository;
        private readonly IAdapterRegistry adapterRegistry;

        /// <summary>
        /// Instantiate a new BankRoutingInformationController
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="bankRepository"></param>
        /// <param name="adapterRegistry"></param>
        public BanksController(ILogger logger, IBankRepository bankRepository, IAdapterRegistry adapterRegistry)
        {
            this.logger = logger;
            this.bankRepository = bankRepository;
            this.adapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Get a bank resource by its id. This endpoint looks for banks known to Colleague's payroll system as well as the
        /// universe of US banks that participate in electronic bank transfers. Canadian banks are only included if they are entered in Colleague's payroll system.
        /// For US Banks, the id is the bank's routing id. For Canadian banks, the id is the combination of the bank's institution id and the branch number, separated by a hyphen
        /// in the following format: {institutionId}-{branchNumber}
        /// </summary>
        /// <param name="id">Routing id of a US bank or institutionId-branchNumber of a Canadian bank</param>
        /// <returns>A Bank object.</returns>
        [HttpGet]
        public async Task<Bank> GetBankAsync([FromUri] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new KeyNotFoundException("Id is required", CreateHttpResponseException("Id is required", HttpStatusCode.BadRequest));
            }

            try
            {
                var domainBank = await bankRepository.GetBankAsync(id);
                var domainToDtoBankAdapter = adapterRegistry.GetAdapter<Domain.Base.Entities.Bank, Dtos.Base.Bank>();
                var dtoBank = domainToDtoBankAdapter.MapToType(domainBank);
                return dtoBank;
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, "Unable to find bank");
                throw CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown exception occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }
    }
}



