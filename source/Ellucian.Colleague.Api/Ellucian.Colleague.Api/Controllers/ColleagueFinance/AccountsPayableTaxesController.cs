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
    /// Provides access to Accounts Payable Tax code information.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class AccountsPayableTaxesController : BaseCompressedApiController
    {
        private readonly IAccountsPayableTaxService accountsPayableTaxService;

        /// <summary>
        /// Constructor to initialize AccountsPayableTaxesController object.
        /// </summary>
        public AccountsPayableTaxesController(IAccountsPayableTaxService accountsPayableTaxService)
        {
            this.accountsPayableTaxService = accountsPayableTaxService;
        }

        /// <summary>
        /// Get all of the Accounts Payable Tax codes.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AccountsPayableTax>> GetAccountsPayableTaxesAsync()
        {
            var accountsPayableTaxCodes = await accountsPayableTaxService.GetAccountsPayableTaxesAsync();
            return accountsPayableTaxCodes;
        }
    }
}
