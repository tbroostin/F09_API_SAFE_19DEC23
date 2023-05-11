// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Http.Filters;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Access to Section and Faculty Events
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EventsController : BaseCompressedApiController
    {
        private readonly IEventService _eventService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// EventsController constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IEventService">IEventService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public EventsController(IEventService service, ILogger logger)
        {
            _eventService = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves events, or meetings, for a given course section in the given date range. If no dates specified, retrieves all meetings for the course section.
        /// </summary>
        /// <param name="sectionId">A list of section Ids, comma-delimited</param>
        /// <param name="startDate">The starting date for which to begin returning events.</param>
        /// <param name="endDate">The ending date at which to stop returning events.</param>
        /// <returns>Events as an ICal string</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound if a section Id is not specified.</exception>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "sectionId" })]
        [Obsolete("Obsolete as of Api version 1.22, use qapi/sections/QuerySectionEventsICal of this API")]
        public EventsICal GetSectionEvents(string sectionId, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<string> sectionList = new List<string>();
            if (!string.IsNullOrEmpty(sectionId))
            {
                string[] sectionArray = sectionId.Split(new char[] { ',' });
                foreach (var secId in sectionArray)
                {
                    sectionList.Add(secId);
                }
            }
            try
            {
                return _eventService.GetSectionEvents(sectionList, startDate, endDate);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("section events", sectionId);
            }
        }

        /// <summary>
        /// Retrieves events for a given faculty and a given date range.
        /// </summary>
        /// <param name="facultyId">A list of faculty IDs, comma-delimited</param>
        /// <param name="startDate">The starting date for which to begin returning events.</param>
        /// <param name="endDate">The ending date at which to stop returning events.</param>
        /// <returns>Events as an ICal string</returns>
        /// <accessComments>
        /// Any authenticated user can access faculty section calendar events.
        /// </accessComments>
        /// <see cref="HttpResponseException">HttpReponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound if faculty Id is not specified.
        public EventsICal GetFacultyEvents(string facultyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<string> facultyList = new List<string>();
            if (!string.IsNullOrEmpty(facultyId))
            {
                string[] facultyArray = facultyId.Split(new char[] { ',' });
                foreach (var facId in facultyArray)
                {
                    facultyList.Add(facId);
                }
            }
            try
            {
                return _eventService.GetFacultyEvents(facultyList, startDate, endDate);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("faculty events", facultyId);
            }
        }

        /// <summary>
        /// Get all Campus Calendars
        /// </summary>
        /// <returns></returns>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        public async Task<IEnumerable<CampusCalendar>> GetCampusCalendarsAsync()
        {
            try
            {
                return await _eventService.GetCampusCalendarsAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                throw CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }
    }
}
