//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to TaxFormCodesController
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class TaxFormCodesController : BaseCompressedApiController
    {
        private readonly ITaxFormsService _taxFormsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TaxFormCodesController class.
        /// </summary>
        /// <param name="taxFormsService">Service of type <see cref="ITaxFormsService">ITaxFormsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public TaxFormCodesController(ITaxFormsService taxFormsService, ILogger logger)
        {
            _taxFormsService = taxFormsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all Tax forms
        /// </summary>
        /// <returns>List of Tax forms <see cref="Dtos.ColleagueFinance.TaxForm"/> objects representing matching TaxForm</returns>
        /// <accessComments>
        /// Any authenticated user can get the TaxForms
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.TaxForm>> GetTaxFormsAsync()
        {
            try
            {
                var taxForms = await _taxFormsService.GetTaxFormsAsync();
                return taxForms;
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get taxforms.", HttpStatusCode.BadRequest);
            }
        }
    }
}