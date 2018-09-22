// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes IpedsInstitution data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class IpedsInstitutionsController : BaseCompressedApiController
    {
        private readonly IIpedsInstitutionRepository ipedsInstitutionRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Instantiates a new instance of the IpedsInstitutionsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="ipedsInstitutionRepository">IpedsInstitutionRepository</param>
        /// <param name="logger">Logger</param>
        public IpedsInstitutionsController(IAdapterRegistry adapterRegistry, IIpedsInstitutionRepository ipedsInstitutionRepository, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.ipedsInstitutionRepository = ipedsInstitutionRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Query by post method used to get IpedsInstitution objects for the given OPE (Office of Postsecondary Education) Ids
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources.
        /// </accessComments>
        /// <param name="opeIds">List of OPE Ids</param>
        /// <returns>The requested IpedsInstitution DTOs</returns>
        [HttpPost]
        public async Task<IEnumerable<IpedsInstitution>> QueryByPostIpedsInstitutionsByOpeIdAsync([FromBody]IEnumerable<string> opeIds)
        {
            if (opeIds == null || !opeIds.Any())
            {
                var message = "At least one item in list of opeIds must be provided";
                logger.Info(message);
                return new List<IpedsInstitution>();
            }

            var ipedsInstitutionEntityList = await ipedsInstitutionRepository.GetIpedsInstitutionsAsync(opeIds);

            var ipedsInstitutionDtoAdapter = adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.IpedsInstitution, IpedsInstitution>();

            var ipedsInstitutionDtoList = new List<IpedsInstitution>();
            foreach (var ipedsInstitutionEntity in ipedsInstitutionEntityList)
            {
                ipedsInstitutionDtoList.Add(ipedsInstitutionDtoAdapter.MapToType(ipedsInstitutionEntity));
            }

            return ipedsInstitutionDtoList;

        }


    }
}