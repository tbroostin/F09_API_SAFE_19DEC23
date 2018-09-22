// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to AccountsPayable Type code information.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class AccountsPayableTypesController : BaseCompressedApiController
    {
        private readonly IAccountsPayableTypeService accountsPayableTypeService;

        /// <summary>
        /// Constructor to initialize AccountsPayableTypesController object.
        /// </summary>
        public AccountsPayableTypesController(IAccountsPayableTypeService accountsPayableTypeService)
        {
            this.accountsPayableTypeService = accountsPayableTypeService;
        }

        /// <summary>
        /// Get all of the AccountsPayable Type codes.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AccountsPayableType>> GetAccountsPayableTypesAsync()
        {
            var apTypeCodes = await accountsPayableTypeService.GetAccountsPayableTypesAsync();
            return apTypeCodes;
        }
    }
}