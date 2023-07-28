// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get and update Accounts Receivable information.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    [Metadata(ApiDescription = "Provides access to get and update Accounts Receivable information.", ApiDomain = "Finance")]
    public class DepositsController : BaseCompressedApiController
    {
        private readonly IAccountsReceivableService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// AccountsReceivableController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IAccountsReceivableService">IAccountsReceivableService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public DepositsController(IAccountsReceivableService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get the deposits due for a specified student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <returns>A list of <see cref="DepositDue">deposits due</see> for the student</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [HttpGet]
        public IEnumerable<DepositDue> GetDepositsDue(string studentId)
        {
            try
            {
                return _service.GetDepositsDue(studentId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.Error(csee, csee.Message);
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves all Deposit Types
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>All <see cref="DepositType">deposit type</see> codes and descriptions.</returns>
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public IEnumerable<DepositType> GetDepositTypes()
        {
            return _service.GetDepositTypes();
        }
    }
}
