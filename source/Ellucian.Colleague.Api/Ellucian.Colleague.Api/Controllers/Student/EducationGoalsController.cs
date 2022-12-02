// Copyright 2019 - 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the education goal data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class EducationGoalsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        
        /// <summary>
        /// Initializes a new instance of the EducationGoalsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public EducationGoalsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Returns all education goals
        /// </summary>
        /// <returns>Collection of <see cref="EducationGoal">education goals</see></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<IEnumerable<EducationGoal>> GetEducationGoalsAsync()
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var educationGoalDtoCollection = new List<Ellucian.Colleague.Dtos.Student.EducationGoal>();
                var educationGoalCollection = await referenceDataRepository.GetAllEducationGoalsAsync(bypassCache);
                var educationGoalDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.EducationGoal, EducationGoal>();
                if (educationGoalCollection != null && educationGoalCollection.Count() > 0)
                {
                    foreach (var educationGoal in educationGoalCollection)
                    {
                        try
                        {
                            educationGoalDtoCollection.Add(educationGoalDtoAdapter.MapToType(educationGoal));
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error encountered converting education goal entity to DTO.");
                        }
                    }
                }
                return educationGoalDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving educational goals data";
                logger.Error(csse, message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve education goals.");
            }
        }
    }
}