// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    [RegisterType]
    public class GradeSchemeService : BaseCoordinationService, IGradeSchemeService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";
        private readonly IConfigurationRepository _configurationRepository;

        public GradeSchemeService(
            IStudentReferenceDataRepository studentReferenceDataRepository, 
            ILogger logger, 
            IConfigurationRepository configurationRepository,
             IAdapterRegistry adapterRegistry,
             ICurrentUserFactory currentUserFactory,
             IRoleRepository roleRepository
             )
             : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all grade schemes
        /// </summary>
        /// <returns>Collection of GradeScheme DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme>> GetGradeSchemesAsync()
        {
            var gradeSchemeCollection = new List<Ellucian.Colleague.Dtos.GradeScheme>();

            var gradeSchemeEntities = await _studentReferenceDataRepository.GetGradeSchemesAsync();
            if (gradeSchemeEntities != null && gradeSchemeEntities.Count() > 0)
            {
                foreach (var gradeScheme in gradeSchemeEntities)
                {
                    gradeSchemeCollection.Add(ConvertGradeSchemeEntityToDto(gradeScheme));
                }
            }
            return gradeSchemeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a grade scheme from its GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>GradeScheme DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.GradeScheme> GetGradeSchemeByGuidAsync(string guid)
        {
            try
            {
                return ConvertGradeSchemeEntityToDto((await _studentReferenceDataRepository.GetGradeSchemesAsync(true)).Where(gs => gs.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Grade Scheme not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Gets all grade schemes
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Collection of GradeScheme DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme2>> GetGradeSchemes2Async(bool bypassCache)
        {
            var gradeSchemeCollection = new List<Ellucian.Colleague.Dtos.GradeScheme2>();

            var gradeSchemeEntities = await _studentReferenceDataRepository.GetGradeSchemesAsync(bypassCache);
            if (gradeSchemeEntities != null && gradeSchemeEntities.Count() > 0)
            {
                foreach (var gradeScheme in gradeSchemeEntities)
                {
                    gradeSchemeCollection.Add(ConvertGradeSchemeEntityToDto2(gradeScheme));
                }
            }
            return gradeSchemeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a grade scheme from its ID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>GradeScheme DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.GradeScheme2> GetGradeSchemeByIdAsync(string id)
        {
            try
            {
                return ConvertGradeSchemeEntityToDto2((await _studentReferenceDataRepository.GetGradeSchemesAsync(true)).Where(gs => gs.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Grade Scheme not found for ID " + id, ex);
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Ellucian.Colleague.Dtos.Student.GradeScheme"/> object by ID
        /// </summary>
        /// <param name="id">Grade Scheme ID</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Student.GradeScheme"/> object</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.GradeScheme> GetNonEthosGradeSchemeByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A grade scheme ID is required to retrieve a grade scheme.");
            }
            Dtos.Student.GradeScheme gradeSchemeDto = null;
            var gradeSchemeEntities = await _studentReferenceDataRepository.GetGradeSchemesAsync();
            if (gradeSchemeEntities != null && gradeSchemeEntities.Any())
            {
                var gradeSchemeEntity = gradeSchemeEntities.Where(gs => gs.Code == id).FirstOrDefault();
                if (gradeSchemeEntity == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not retrieve a grade scheme with ID {0}.", id));
                }
                // Get the right adapter for the type mapping
                var gradeSchemeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeScheme, Dtos.Student.GradeScheme>();

                gradeSchemeDto = gradeSchemeDtoAdapter.MapToType(gradeSchemeEntity);
            }
            if (gradeSchemeDto == null)
            {
                throw new KeyNotFoundException(string.Format("Could not retrieve a grade scheme with ID {0}.", id));
            }
            return gradeSchemeDto;

        }

        /// <summary>
        /// Retrieves a <see cref="Ellucian.Colleague.Dtos.Student.GradeSubscheme"/> object by ID
        /// </summary>
        /// <param name="id">Grade Subscheme ID</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Student.GradeSubscheme"/> object</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.GradeSubscheme> GetGradeSubschemeByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A grade subscheme ID is required to retrieve a grade subscheme.");
            }
            Dtos.Student.GradeSubscheme gradeSubschemeDto = null;
            var gradeSubschemeEntities = await _studentReferenceDataRepository.GetGradeSubschemesAsync();
            if (gradeSubschemeEntities != null && gradeSubschemeEntities.Any())
            {
                var gradeSubschemeEntity = gradeSubschemeEntities.Where(gs => gs.Code == id).FirstOrDefault();
                if (gradeSubschemeEntity == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not retrieve a grade subscheme with ID {0}.", id));
                }
                // Get the right adapter for the type mapping
                var gradeSchemeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeSubscheme, Dtos.Student.GradeSubscheme>();

                gradeSubschemeDto = gradeSchemeDtoAdapter.MapToType(gradeSubschemeEntity);
            }
            if (gradeSubschemeDto == null)
            {
                throw new KeyNotFoundException(string.Format("Could not retrieve a grade subscheme with ID {0}.", id));
            }
            return gradeSubschemeDto;

        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a GradeScheme domain entity to its corresponding GradeScheme DTO
        /// </summary>
        /// <param name="source">GradeScheme domain entity</param>
        /// <returns>GradeScheme DTO</returns>
        private Ellucian.Colleague.Dtos.GradeScheme ConvertGradeSchemeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.GradeScheme source)
        {
            var gradeScheme = new Ellucian.Colleague.Dtos.GradeScheme();

            //gradeScheme.Metadata = new Dtos.MetadataObject(_dataOrigin);
            gradeScheme.Guid = source.Guid;
            gradeScheme.Abbreviation = source.Code;
            gradeScheme.Title = source.Description;
            gradeScheme.Description = null;
            gradeScheme.EffectiveStartDate = source.EffectiveStartDate;
            gradeScheme.EffectiveEndDate = source.EffectiveEndDate;

            return gradeScheme;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a GradeScheme domain entity to its corresponding GradeScheme DTO
        /// </summary>
        /// <param name="source">GradeScheme domain entity</param>
        /// <returns>GradeScheme DTO</returns>
        private Ellucian.Colleague.Dtos.GradeScheme2 ConvertGradeSchemeEntityToDto2(Ellucian.Colleague.Domain.Student.Entities.GradeScheme source)
        {
            var gradeScheme = new Ellucian.Colleague.Dtos.GradeScheme2();

            gradeScheme.Id = source.Guid;
            gradeScheme.Code = source.Code;
            gradeScheme.Title = source.Description;
            gradeScheme.Description = null;
            gradeScheme.StartOn = source.EffectiveStartDate;
            gradeScheme.EndOn = source.EffectiveEndDate;
            return gradeScheme;
        }
    }
}