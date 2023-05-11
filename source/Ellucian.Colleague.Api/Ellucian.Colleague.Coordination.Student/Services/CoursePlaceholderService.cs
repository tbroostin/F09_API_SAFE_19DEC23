// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for course placeholder data
    /// </summary>
    [RegisterType]
    public class CoursePlaceholderService : BaseCoordinationService, ICoursePlaceholderService
    {
        private readonly ICoursePlaceholderRepository _repository;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="CoursePlaceholderService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">Interface to adapter registry</param>
        /// <param name="repository">Interface to Course Placeholder Repository</param>
        /// <param name="currentUserFactory">Interface to current user factory</param>
        /// <param name="roleRepository">Interface to role repository</param>
        /// <param name="logger">Interface to logger</param>
        public CoursePlaceholderService(IAdapterRegistry adapterRegistry, ICoursePlaceholderRepository repository, ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository, ILogger logger) : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve a collection of course placeholders by ID
        /// </summary>
        /// <param name="coursePlaceholderIds">Unique identifiers for course placeholders to retrieve</param>
        /// <param name="bypassCache">Flag indicating whether or not to bypass the API's cached course placeholder data and retrieve the data directly from Colleague; defaults to false</param>
        /// <returns>Collection of <see cref="CoursePlaceholder"/></returns>
        public async Task<IEnumerable<CoursePlaceholder>> GetCoursePlaceholdersByIdsAsync(IEnumerable<string> coursePlaceholderIds, bool bypassCache = false)
        {
            var coursePlaceholderDtos = new List<CoursePlaceholder>();
            if (coursePlaceholderIds == null || !coursePlaceholderIds.Any())
            {
                throw new ArgumentNullException("coursePlaceholderIds", "At least one course placeholder ID is required when retrieving course placeholders by ID.");
            }
            try
            {
                var coursePlaceholderEntities = await _repository.GetCoursePlaceholdersByIdsAsync(coursePlaceholderIds, bypassCache);
                if (coursePlaceholderEntities != null && coursePlaceholderEntities.Any())
                {
                    var entityToDtoAdapter = new CoursePlaceholderEntityToCoursePlaceholderDtoAdapter(_adapterRegistry, _logger);
                    foreach (var entity in coursePlaceholderEntities)
                    {
                        coursePlaceholderDtos.Add(entityToDtoAdapter.MapToType(entity));
                    }
                }
                return coursePlaceholderDtos;
            }
            catch (Ellucian.Data.Colleague.Exceptions.ColleagueSessionExpiredException)
            {
                throw;
            }

            catch (Exception ex)
            {
                logger.Error(ex, string.Format("An error occurred while trying to retrieve course placeholder data for IDs {0}.", string.Join(",", coursePlaceholderIds)));
                throw;
            }
        }
    }
}
