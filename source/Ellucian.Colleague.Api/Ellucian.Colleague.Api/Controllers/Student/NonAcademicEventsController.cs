// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller to manage nonacademic events
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class NonAcademicEventsController : BaseCompressedApiController
    {
        private readonly INonAcademicEventRepository _eventRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        /// <summary>
        /// NonAcademicEventsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="eventRepository"></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public NonAcademicEventsController(IAdapterRegistry adapterRegistry, INonAcademicEventRepository eventRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _eventRepository = eventRepository;
            _logger = logger;
        }

        /// <summary>
        /// Accepts a list of event IDs and returns NonAcademicEvents DTOs
        /// </summary>
        /// <param name="criteria">Object that contains the list of ids to retrieve</param>
        /// <returns>List of <see cref="NonAcademicEvent">Nonacademic Events</see></returns>
        [HttpPost]
        public async Task<IEnumerable<NonAcademicEvent>> QueryNonAcademicEventsAsync([FromBody] NonAcademicEventQueryCriteria criteria)
        {
            if (criteria == null)
            {
                string errorText = "Query criteria must be provided.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            IEnumerable<string> eventIds = criteria.EventIds;
            if (eventIds == null || eventIds.Count() == 0)
            {
                string errorText = "At least one item in list of eventIds must be provided.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            var eventDtos = new List<NonAcademicEvent>();
            try
            {
                var eventEntities = await _eventRepository.GetEventsByIdsAsync(eventIds);
                var eventDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.NonAcademicEvent, NonAcademicEvent>();
                foreach (var eventEntity in eventEntities)
                {
                    eventDtos.Add(eventDtoAdapter.MapToType(eventEntity));
                }
                return eventDtos;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to retrieve NonAcademicEvents");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }
    }
}
