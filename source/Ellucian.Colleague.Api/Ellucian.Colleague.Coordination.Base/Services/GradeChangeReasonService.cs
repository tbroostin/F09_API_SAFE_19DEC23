// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class GradeChangeReasonService :BaseCoordinationService, IGradeChangeReasonService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger repoLogger;

        /// <summary>
        /// Constructor for Grade Change Reason
        /// </summary>
        /// <param name="configurationRepository">configurationRepository</param>
        /// <param name="referenceDataRepository">referenceDataRepository</param>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="currentUserFactory">currentUserFactory</param>
        /// <param name="roleRepository">roleRepository</param>
        /// <param name="logger">logger</param>
        public GradeChangeReasonService(IConfigurationRepository configurationRepository, IReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry,
                                        ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
                                        : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            if(logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.repoLogger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all Grade Change Reasons
        /// </summary>
        /// <returns>Collection of grade change reason DTO objects</returns>
        public async Task<IEnumerable<Dtos.GradeChangeReason>> GetAsync(bool bypassCache)
        {
            var gradeChangeReasonCollection = new List<Dtos.GradeChangeReason>();

            var gradeChangeReasonEntities = await _referenceDataRepository.GetGradeChangeReasonAsync(bypassCache);
            if(gradeChangeReasonEntities != null && gradeChangeReasonEntities.Count() > 0)
            {
                foreach(var gradeChangeReason in gradeChangeReasonEntities)
                {
                    gradeChangeReasonCollection.Add(ConvertGradeChangeReasonEntityToGradeChangeReasonDto(gradeChangeReason));
                }
            }
            return gradeChangeReasonCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a grade change reason from its id
        /// </summary>
        /// <returns>GradeChangeReason DTO object</returns>
        public async Task<Dtos.GradeChangeReason> GetGradeChangeReasonByIdAsync(string id)
        {
            try
            {
                return ConvertGradeChangeReasonEntityToGradeChangeReasonDto((await _referenceDataRepository.GetGradeChangeReasonAsync(true)).Where(gcr => gcr.Guid == id).First());
            }
            catch(InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Grade Change Reason not found for id " + id, ex);
            }
            catch(Exception ex)
            {
                throw new Exception("Grade Change Reason not found for id " + id, ex);
            }
        }

        /// <summary>
        /// Converts grade change reason entity to dto
        /// </summary>
        /// <param name="gradeChangeReason">gradeChangeReason</param>
        /// <returns>gradeChangeReason</returns>
        private Dtos.GradeChangeReason ConvertGradeChangeReasonEntityToGradeChangeReasonDto(GradeChangeReason gradeChangeReason)
        {
            Dtos.GradeChangeReason gcr = new Dtos.GradeChangeReason();
            
            gcr.Id = gradeChangeReason.Guid;
            gcr.Code = gradeChangeReason.Code;
            gcr.Title = gradeChangeReason.Description;
            gcr.Description = string.Empty;
            
            return gcr;
        }
    }
}
