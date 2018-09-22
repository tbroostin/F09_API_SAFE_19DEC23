﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;

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