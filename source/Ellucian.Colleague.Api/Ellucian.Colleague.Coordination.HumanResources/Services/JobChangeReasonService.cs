/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class JobChangeReasonService : BaseCoordinationService, IJobChangeReasonService
    {

        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;

        public JobChangeReasonService(

            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IConfigurationRepository configurationRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {

            _hrReferenceDataRepository = hrReferenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all job change reasons
        /// </summary>
        /// <returns>Collection of JobChangeReasons DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.JobChangeReason>> GetJobChangeReasonsAsync(bool bypassCache = false)
        {
            var jobChangeReasonCollection = new List<Ellucian.Colleague.Dtos.JobChangeReason>();

            var jobChangeReasonEntities = await _hrReferenceDataRepository.GetJobChangeReasonsAsync(bypassCache);
            if (jobChangeReasonEntities != null && jobChangeReasonEntities.Count() > 0)
            {
                foreach (var jobChangeReason in jobChangeReasonEntities)
                {
                    jobChangeReasonCollection.Add(ConvertJobChangeReasonEntityToDto(jobChangeReason));
                }
            }
            return jobChangeReasonCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a job change reason from its GUID
        /// </summary>
        /// <returns>JobChangeReason DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.JobChangeReason> GetJobChangeReasonByGuidAsync(string guid)
        {
            try
            {
                return ConvertJobChangeReasonEntityToDto((await _hrReferenceDataRepository.GetJobChangeReasonsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Job Change Reason not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a JobChangeReason domain entity to its corresponding JobChangeReason DTO
        /// </summary>
        /// <param name="source">JobChangeReason domain entity</param>
        /// <returns>JobChangeReason DTO</returns>
        private Ellucian.Colleague.Dtos.JobChangeReason ConvertJobChangeReasonEntityToDto(JobChangeReason source)
        {
            var jobChangeReason = new Ellucian.Colleague.Dtos.JobChangeReason();

            jobChangeReason.Id = source.Guid;
            jobChangeReason.Code = source.Code;
            jobChangeReason.Title = source.Description;
            jobChangeReason.Description = null;

            return jobChangeReason;
        }
    }
}