// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base;
using System.Web;

namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// AdviseesController
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class AdviseesController : BaseCompressedApiController
    {
        private readonly IAdvisorService _advisorService;
        private readonly ILogger _logger;

        /// <summary>
        /// AdvisorsController constructor
        /// </summary>
        /// <param name="advisorService">Service of type <see cref="IAdvisorService">IAdvisorService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AdviseesController(IAdvisorService advisorService, ILogger logger)
        {
            _advisorService = advisorService;
            _logger = logger;
        }
        /// <summary>
        /// Search advisees by their name or ID, or by their assigned advisor's name or ID. This returns a set of advisees.
        /// These searches can only be done by advisors or staff with permissions to view an appropriate group of advisees.
        /// </summary>
        /// <param name="criteria"><see cref="AdviseeSearchCriteria">Advisee search criteria</see></param>
        /// <param name="pageIndex">Index of page to return</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <returns>All Advisees that matched the search criteria. Advisee privacy is enforced by this 
        /// response. If any advisee has an assigned privacy code that the advisor is not authorized to access, the Advisee response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of advisees. In this situation, 
        /// all details except the advisee name are cleared from the specific advisee object.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden exception returned if user does not have role and permission to access this advisor's data. No special role is needed to get this basic information.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest exception returned for any other unexpected error.</exception>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes may query their own list of assigned advisees using the search criteria 
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may search for any student using the search criteria 
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 
        /// Advisee privacy is enforced by this response. If any advisee has an assigned privacy code that the advisor is not authorized to access, 
        /// the Advisee response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of advisees. 
        /// In this situation, all details except the advisee name are cleared from the specific advisee object.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<Advisee>> QueryAdviseesByPost2Async([FromBody]AdviseeSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            _logger.Info("Entering QueryAdviseesByPost2Async");
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                // The service to execute this search is in AdvisorService since only an authorized advisor can do these types of searches.
                var privacyWrapper = await _advisorService.Search3Async(criteria, pageSize, pageIndex);
                var advisees = privacyWrapper.Dto as List<Advisee>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                watch.Stop();
                _logger.Info("QueryAdviseesByPost2Async... completed in " + watch.ElapsedMilliseconds.ToString());

                return (IEnumerable<Advisee>)advisees;
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Search advisees by their name or ID, or by their assigned advisor's name or ID. This returns a set of advisees.
        /// These searches can only be done by advisors or staff with permissions to view an appropriate group of advisees.
        /// </summary>
        /// <param name="criteria"><see cref="AdviseeSearchCriteria">Advisee search criteria</see></param>
        /// <param name="pageIndex">Index of page to return</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <returns>All Advisees that matched the search criteria. Advisee privacy is enforced by this 
        /// response. If any advisee has an assigned privacy code that the advisor is not authorized to access, the Advisee response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of advisees. In this situation, 
        /// all details except the advisee name are cleared from the specific advisee object.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden exception returned if user does not have role and permission to access this advisor's data. No special role is needed to get this basic information.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest exception returned for any other unexpected error.</exception>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes may query their own list of assigned advisees using the search criteria 
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may search for any student using the search criteria 
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 
        /// Advisee privacy is enforced by this response. If any advisee has an assigned privacy code that the advisor is not authorized to access, 
        /// the Advisee response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of advisees. 
        /// In this situation, all details except the advisee name are cleared from the specific advisee object.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<Advisee>> QueryAdviseesForExactMatchByPostAsync([FromBody]AdviseeSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            _logger.Info("Entering QueryAdviseesForExactMatchByPostAsync");
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                // The service to execute this search is in AdvisorService since only an authorized advisor can do these types of searches.
                var privacyWrapper = await _advisorService.SearchForExactMatchAsync(criteria, pageSize, pageIndex);
                var advisees = privacyWrapper.Dto as List<Advisee>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                watch.Stop();
                _logger.Info("QueryAdviseesForExactMatchByPostAsync... completed in " + watch.ElapsedMilliseconds.ToString());

                return (IEnumerable<Advisee>)advisees;
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
