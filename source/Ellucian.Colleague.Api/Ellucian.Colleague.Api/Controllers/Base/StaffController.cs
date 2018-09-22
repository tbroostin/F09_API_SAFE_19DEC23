// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Staff data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class StaffController : BaseCompressedApiController
    {
        private readonly IStaffService _staffService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StaffController class.
        /// </summary>
        /// <param name="staffService">Service of type <see cref="IStaffService">IStaffService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StaffController(IStaffService staffService, ILogger logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        /// <summary>
        /// Get the staff record for the provided ID
        /// </summary>
        /// <param name="staffId">ID for the staff member</param>
        /// <returns>A staff record</returns>
        public async Task<Staff> GetAsync(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                _logger.Error("Invalid staffId " + staffId);
                throw CreateHttpResponseException("Invalid staffId " + staffId, HttpStatusCode.BadRequest);
            }
            try
            {
                var staff = await _staffService.GetAsync(staffId);
                return staff;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the restrictions for the indicated staff.
        /// </summary>
        /// <param name="staffId">ID of the staff</param>
        /// <returns>The list of <see cref="PersonRestriction"></see> restrictions.</returns>
        public async Task<IEnumerable<PersonRestriction>> GetStaffRestrictions(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                _logger.Error("Invalid staffId " + staffId);
                throw CreateHttpResponseException("Invalid staffId " + staffId, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _staffService.GetStaffRestrictionsAsync(staffId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }
    }
}