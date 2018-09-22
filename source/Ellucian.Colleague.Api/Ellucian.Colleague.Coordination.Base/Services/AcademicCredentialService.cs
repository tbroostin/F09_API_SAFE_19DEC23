// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class AcademicCredentialService : BaseCoordinationService, IAcademicCredentialService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public AcademicCredentialService(IAdapterRegistry adapterRegistry,
            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all Academic Credentials
        /// </summary>
        /// <returns>Collection of Academic Credential DTO objects</returns>
        public async Task<IEnumerable<Dtos.AcademicCredential>> GetAcademicCredentialsAsync(bool bypassCache = false)
        {         
            var academicCredentialsCollection = new List<Dtos.AcademicCredential>();

            var acadCredentialEntities = await _referenceDataRepository.GetAcadCredentialsAsync(bypassCache);
            if (acadCredentialEntities != null && acadCredentialEntities.Any())
            {
                foreach (var acadCredential in acadCredentialEntities)
                {
                    academicCredentialsCollection.Add(ConvertAcadCredentialEntityToAcademicCredentialDto(acadCredential));
                }
            }
            
            return academicCredentialsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an Academic Credential from its GUID
        /// </summary>
        /// <returns>AcademicCredential DTO object</returns>
        public async Task<Dtos.AcademicCredential> GetAcademicCredentialByGuidAsync(string guid)
        {
            try
            {
                var academicCredentialsCollection = new List<Dtos.AcademicCredential>();

                var acadCredentialEntities = await _referenceDataRepository.GetAcadCredentialsAsync(true);
                if (acadCredentialEntities != null && acadCredentialEntities.Any())
                {
                    foreach (var acadCredential in acadCredentialEntities)
                    {
                        academicCredentialsCollection.Add(ConvertAcadCredentialEntityToAcademicCredentialDto(acadCredential));
                    }
                }

                return academicCredentialsCollection.First(om => om.Id == guid);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Credential not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Academic Credential not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an AcadCredential domain entity to its corresponding AcademicCredential DTO
        /// </summary>
        /// <param name="source">AcadCredential domain entity</param>
        /// <returns>AcademicCredential DTO</returns>
        private Dtos.AcademicCredential ConvertAcadCredentialEntityToAcademicCredentialDto(AcadCredential source)
        {

            var academicCredential = new Dtos.AcademicCredential
            {
                Id = source.Guid,
                Abbreviation = source.Code,
                Title = source.Description,
                Description = null,
                AcademicCredentialType = ConvertAcademicCredentialTypeEnumToAcademicCredentialTypeEntityEnum(source.AcademicCredentialType)

            };

            return academicCredential;
        }

        private Dtos.AcademicCredentialType ConvertAcademicCredentialTypeEnumToAcademicCredentialTypeEntityEnum(Domain.Base.Entities.AcademicCredentialType? academicCredentialTypeType)
        {
            if (academicCredentialTypeType == null)
                throw new ArgumentNullException("AcademicDisciplineType is a required field");

            switch (academicCredentialTypeType)
            {
                case Domain.Base.Entities.AcademicCredentialType.Certificate:
                    return Dtos.AcademicCredentialType.Certificate;
                case Domain.Base.Entities.AcademicCredentialType.Degree:
                    return Dtos.AcademicCredentialType.Degree;
                case Domain.Base.Entities.AcademicCredentialType.Diploma:
                    return Dtos.AcademicCredentialType.Diploma;
                default:
                    throw new InvalidOperationException(string.Format("Unhandled AcademicCredentialType value: {0}", academicCredentialTypeType.ToString()));    
            }
        }
    }
}