﻿// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// AdvisorsController
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class AdvisorsController : BaseCompressedApiController
    {
        private readonly IAdvisorService _advisorService;
        private readonly ILogger _logger;

        /// <summary>
        /// AdvisorsController constructor
        /// </summary>
        /// <param name="advisorService">Service of type <see cref="IAdvisorService">IAdvisorService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AdvisorsController(IAdvisorService advisorService, ILogger logger)
        {
            _advisorService = advisorService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves basic advisor information, such as advisor name. 
        /// This is intended to retrieve merely reference information for a person who may have currently or previously performed the functions of an advisor. 
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// </summary>
        /// <param name="id">Id of the advisor to retrieve</param>
        /// <returns>An <see cref="Advisor">Advisor</see> object containing advisor name</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden exception returned if user does not have role and permission to access this advisor's data. No special role is needed to get this basic information.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest exception returned for any other unexpected error.</exception>
        /// <accessComments>Any authenticated user can retrieve advisor name and professional email information.</accessComments>
        public async Task<Advisor> GetAdvisorAsync(string id)
        {
            try
            {
                return await _advisorService.GetAdvisorAsync(id);
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
        /// Retrieves basic advisor information for a the given Advisor query criteria (list of advisor ids). 
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// This is intended to retrieve reference information (name and professional email) for any person who may have currently 
        /// or previously performed the functions of an advisor. If a specified ID not found to be a potential advisor,
        /// does not cause an exception, item is simply not returned in the list.
        /// NOTE: The AdvisorQueryCriteria object has a property indicating whether to retrieve OnlyActiveAdvisees,
        /// however, since the Advisor DTO is name/email info only this property has no effect on the results.
        /// </summary>
        /// <param name="advisorQueryCriteria">Criteria of the advisors to retrieve</param>
        /// <returns>A list of <see cref="Advisor">Advisors</see> object containing advisor name</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden exception returned if user does not have role and permission to access this endpoint. Advisor permissions required.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest exception returned for any other unexpected error.</exception>
        /// <accessComments>Any authenticated user can query advisor name information.</accessComments>
        [HttpPost]
        [Obsolete("Obsolete as API 1.19, use version 2 instead.")]
        public async Task<IEnumerable<Advisor>> QueryAdvisorsByPostAsync([FromBody] AdvisorQueryCriteria advisorQueryCriteria)
        {
            try
            {
                return await _advisorService.GetAdvisorsAsync(advisorQueryCriteria);
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
        /// Retrieves basic advisor information for a the given Advisor query criteria (list of advisor ids). 
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// This is intended to retrieve merely reference information (name and professional email) for any person who may have currently 
        /// or previously performed the functions of an advisor. If a specified ID not found to be a potential advisor,
        /// does not cause an exception, item is simply not returned in the list.
        /// </summary>
        /// <param name="advisorIds">Advisor IDs for whom data will be retrieved</param>
        /// <returns>A list of <see cref="Advisor">Advisors</see> object containing advisor name</returns>
        /// <accessComments>Any authenticated user can query advisor name information.</accessComments>

        [HttpPost]
        public async Task<IEnumerable<Advisor>> QueryAdvisorsByPost2Async([FromBody]IEnumerable<string> advisorIds)
        {
            try
            {
                if (advisorIds == null || !advisorIds.Any())
                {
                    throw new ArgumentNullException("advisorIds", "At least one advisor ID must be provided to search for advisors.");
                }
                return await _advisorService.QueryAdvisorsByPostAsync(advisorIds);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Retrieves an advisor's list of assigned advisees.
        /// </summary>
        /// <param name="advisorId">id of advisor</param>
        /// <param name="pageIndex">Index of page to return</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <param name="activeAdviseesOnly">If true, only current advisees are returns - this excludes former advisees and future advisees.</param>
        /// <returns>A list of <see cref="Advisee">Advisees</see> including associated program and approval request status for each advisee. Advisee privacy is enforced 
        ///  by this response. If any advisee has an assigned privacy code that the advisor is not authorized to access, the Advisee response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of advisees. In this situation, 
        /// all details except the advisee name are cleared from the specific advisee object.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden exception returned if user does not have role and permission to access this advisee data.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest exception returned for any other unexpected error.</exception>
        /// <accessComments>
        /// An authenticated user (advisor) with any of the following permission codes may retrieve their own list of assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 
        /// Advisee privacy is enforced by this response. If any advisee has an assigned privacy code that the advisor is not authorized to access, 
        /// the Advisee response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of advisees. 
        /// In this situation, all details except the advisee name are cleared from the specific advisee object.
        /// </accessComments>
        public async Task<IEnumerable<Advisee>> GetAdvisees2Async(string advisorId, int pageSize = int.MaxValue, int pageIndex = 1, bool activeAdviseesOnly = false)
        {
            _logger.Info("Entering GetAdvisees2Async");
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var privacyWrapper = await _advisorService.GetAdviseesAsync(advisorId, pageSize, pageIndex, activeAdviseesOnly);
                var advisees = privacyWrapper.Dto as List<Advisee>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }

                watch.Stop();
                _logger.Info("GetAdvisees2Async... completed in " + watch.ElapsedMilliseconds.ToString());

                return advisees;
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
        /// Retrieves an advisee for this advisor if advisor has required permission.
        /// </summary>
        /// <param name="advisorId">id of advisor</param>
        /// <param name="adviseeId">id of the advisee requested</param>
        /// <returns>An <see cref="Advisee">Advisee</see> including associated program and approval request status. Advisee privacy is enforced by this 
        /// response. If an advisee has an assigned privacy code that the advisor is not authorized to access, the Advisee response object is returned with a
        /// X-Content-Restricted header with a value of "partial" to indicate only partial information is returned. In this situation, all details except 
        /// the advisee name are cleared from the response object.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden exception returned if user does not have role and permission to access this advisee data.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest exception returned for any other unexpected error.</exception>
        /// <accessComments>
        /// An authenticated user (advisor) may retrieve an advisee from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) may retrieve any advisee if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 
        /// Advisee privacy is enforced by this response. If the advisee has an assigned privacy code that the advisor is not authorized to access, 
        /// the Advisee response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of advisees. 
        /// In this situation, all details except the advisee name are cleared from the specific advisee object.
        /// </accessComments>
        public async Task<Advisee> GetAdvisee2Async(string advisorId, string adviseeId)
        {
            try
            {
                var privacyWrapper = await _advisorService.GetAdviseeAsync(advisorId, adviseeId);
                var advisee = privacyWrapper.Dto as Advisee;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return advisee;
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException kex)
            {
                _logger.Error(kex.ToString());
                throw CreateNotFoundException("Advisee", adviseeId.ToString());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves permissions for the current user to determine which functions the user is allowed, such as ability to advise all students or only their own advisees, or whether advisor can just view or may also update a advisee's plan.
        /// </summary>
        /// <returns>List of strings representing the permissions of this user</returns>
        /// <accessComments>Users may retrieve their own advising permissions</accessComments>
        [Obsolete("Obsolete as of Colleague Web API 1.21. Use GetAdvisingPermissions2Async")]
        public async Task<IEnumerable<string>> GetPermissionsAsync()
        {
            try
            {
                return await _advisorService.GetAdvisorPermissionsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("An error occurred while retrieving advising permissions.", HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Returns the advising permissions for the authenticated user.
        /// </summary>
        /// <returns>Advising permissions for the authenticated user.</returns>
        /// <accessComments>Users may retrieve their own advising permissions</accessComments>
        public async Task<AdvisingPermissions> GetAdvisingPermissions2Async()
        {
            try
            {
                return await _advisorService.GetAdvisingPermissions2Async();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("An error occurred while retrieving advising permissions.", HttpStatusCode.BadRequest);
            }
        } 
    }
}

