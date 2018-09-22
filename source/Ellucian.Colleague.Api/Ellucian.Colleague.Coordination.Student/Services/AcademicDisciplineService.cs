// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AcademicDisciplineService : StudentCoordinationService, IAcademicDisciplineService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IConfigurationRepository _configurationRepository;


        public AcademicDisciplineService(IAdapterRegistry adapterRegistry,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository  studentReferenceDataRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IStudentRepository studentRepository,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _studentRepository = studentRepository;
            
        }

        
        private IEnumerable<Major> _actualMajors = null;
        private async Task<IEnumerable<Major>> GetActualMajorsAsync(bool ignoreCache = false)
        {
            if (_actualMajors == null)
            {
                _actualMajors = await _studentReferenceDataRepository.GetMajorsAsync(ignoreCache);
            }

            return _actualMajors;
        }

        private IEnumerable<Minor> _actualMinors = null;
        private async Task<IEnumerable<Minor>> GetActualMinorsAsync(bool ignoreCache = false)
        {
            if (_actualMinors == null)
            {
                _actualMinors = await _studentReferenceDataRepository.GetMinorsAsync(ignoreCache);
            }

            return _actualMinors;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM V15</remarks>
        /// <summary>
        ///     Gets all academic disciplines
        /// </summary>
        /// <returns>Collection of AcademicDiscipline DTO objects</returns>
        public async Task<IEnumerable<AcademicDiscipline3>> GetAcademicDisciplines3Async(Dtos.EnumProperties.MajorStatus status, string type, bool bypassCache = false)
        {
            // only difference between 2 and 3 is that the enum is nullable 
            // so use an automapper to avoid duplicating the convert code

            var mapper = _adapterRegistry.GetAdapter<AcademicDiscipline2, AcademicDiscipline3>();

            var academicDisciplineCollection = new List<AcademicDiscipline3>();

            // only USA and CAN are currently supported by the model. Default is USA.
            var hostCountryIso = await _studentReferenceDataRepository.GetHostCountryAsync();


            var academicDiscplinesEntities = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache);
            if (academicDiscplinesEntities != null && academicDiscplinesEntities.Any())
            {
                foreach (var academicDiscpline in academicDiscplinesEntities)
                {
                    // if status named query used
                    if (status == Dtos.EnumProperties.MajorStatus.Active)
                    {
                        // filter this discipline out if not an active major or not offered at this institution
                        if (academicDiscpline.ActiveMajor == null || academicDiscpline.ActiveMajor == false) continue;
                    }
                    else if (status == Dtos.EnumProperties.MajorStatus.Inactive)
                    {
                        // filter out non-major disciplines and active majors
                        if (academicDiscpline.AcademicDisciplineType != Domain.Base.Entities.AcademicDisciplineType.Major || academicDiscpline.ActiveMajor == true) continue;
                    }

                    // Separate if blocks for filter and named query, futureproofing in case they're ever wanted together

                    // if discipline type filter used
                    if (type == "major")
                    {
                        if (academicDiscpline.AcademicDisciplineType != Domain.Base.Entities.AcademicDisciplineType.Major) continue;
                    }
                    else if (type == "minor")
                    {
                        if (academicDiscpline.AcademicDisciplineType != Domain.Base.Entities.AcademicDisciplineType.Minor) continue;
                    }
                    else if (type == "concentration")
                    {
                        if (academicDiscpline.AcademicDisciplineType != Domain.Base.Entities.AcademicDisciplineType.Concentration) continue;
                    }

                    // discipline passed filters, add to collection
                    var academicDisciplineDto2 = await ConvertAcademicDisciplineEntityToAcademicDiscipline2Dto(academicDiscpline, hostCountryIso, bypassCache);

                    // Convert to v3
                    var academicDisciplineDto3 = mapper.MapToType(academicDisciplineDto2);

                    academicDisciplineCollection.Add(academicDisciplineDto3);
                }
            }
            return academicDisciplineCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM V7, V10</remarks>
        /// <summary>
        ///     Gets all academic disciplines
        /// </summary>
        /// <returns>Collection of AcademicDiscipline DTO objects</returns>
        public async Task<IEnumerable<AcademicDiscipline2>> GetAcademicDisciplines2Async(bool bypassCache = false)
        {
            var academicDisciplineCollection = new List<AcademicDiscipline2>();
            
            // only USA and CAN are currently supported by the model. Default is USA.
            var hostCountryIso = await _studentReferenceDataRepository.GetHostCountryAsync();


            var academicDiscplinesEntities = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache);
            if (academicDiscplinesEntities != null && academicDiscplinesEntities.Any())
            {

                foreach (var academicDiscpline in academicDiscplinesEntities)
                {
                    var academicDisciplineDto = await ConvertAcademicDisciplineEntityToAcademicDiscipline2Dto(academicDiscpline, hostCountryIso, bypassCache);
                    academicDisciplineCollection.Add(academicDisciplineDto);
                }
                
            }

            return academicDisciplineCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Gets all academic disciplines
        /// </summary>
        /// <returns>Collection of AcademicDiscipline DTO objects</returns>
        /// 

        public async Task<IEnumerable<AcademicDiscipline>> GetAcademicDisciplinesAsync(bool bypassCache = false)
        {
            var academicDisciplineCollection = new List<AcademicDiscipline>();

            var academicDiscplinesEntities = await _referenceDataRepository.GetAcademicDisciplinesAsync(bypassCache);
            if (academicDiscplinesEntities != null && academicDiscplinesEntities.Any())
            {
                
                foreach (var academicDiscpline in academicDiscplinesEntities)
                {
                    var academicDisciplineDto = ConvertAcademicDisciplineEntityToAcademicDisciplineDto(academicDiscpline);
                    academicDisciplineCollection.Add(academicDisciplineDto);

                }

            }

            return academicDisciplineCollection;
        }
        

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Get an academic discipline type from its GUID
        /// </summary>
        /// <returns>AcademicDiscipline DTO object</returns>
        public async Task<AcademicDiscipline2> GetAcademicDiscipline2ByGuidAsync(string guid, bool bypassCache = false)
        {

            // only USA and CAN are currently supported by the model. Default is USA.
            var hostCountryIso = await _studentReferenceDataRepository.GetHostCountryAsync();

            try
            {
                var academicDiscipline = new AcademicDiscipline2();
                var entity = await _referenceDataRepository.GetRecordInfoFromGuidReferenceDataRepoAsync(guid);

                switch (entity.Entity)
                {
                    case ("OTHER.MAJORS"):
                        var majors = await _referenceDataRepository.GetAcademicDisciplinesMajorAsync(entity.PrimaryKey, true);
                        if (majors != null)
                        {
                            academicDiscipline = await ConvertAcademicDisciplineEntityToAcademicDiscipline2Dto(majors, hostCountryIso, bypassCache);
                        }
                        break;
                    case ("OTHER.MINORS"):
                        var minors = await _referenceDataRepository.GetAcademicDisciplinesMinorAsync(entity.PrimaryKey, true);
                        if (minors != null)
                        {
                            academicDiscipline = await ConvertAcademicDisciplineEntityToAcademicDiscipline2Dto(minors, hostCountryIso, bypassCache);
                        }
                        break;
                    case ("OTHER.SPECIALS"):
                        var specials = await _referenceDataRepository.GetAcademicDisciplinesSpecialAsync(entity.PrimaryKey, true);
                        if (specials != null)
                        {
                            academicDiscipline = await ConvertAcademicDisciplineEntityToAcademicDiscipline2Dto(specials, hostCountryIso, bypassCache);
                        }
                        break;
                }
                return academicDiscipline;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Discipline not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Get an academic discipline type from its GUID
        /// </summary>
        /// <returns>AcademicDiscipline DTO object</returns>
        public async Task<AcademicDiscipline> GetAcademicDisciplineByGuidAsync(string guid)
        {
            try
            {
                var academicDiscipline = new AcademicDiscipline();
                var entity = await _referenceDataRepository.GetRecordInfoFromGuidReferenceDataRepoAsync(guid);

                switch (entity.Entity)
                {
                    case ("OTHER.MAJORS"):
                        var majorsEntity = await _referenceDataRepository.GetAcademicDisciplinesMajorAsync(entity.PrimaryKey, true);
                        if (majorsEntity != null)
                        {
                            academicDiscipline = ConvertAcademicDisciplineEntityToAcademicDisciplineDto(majorsEntity);
                        }
                        break;
                    case ("OTHER.MINORS"):
                        var minorsEntity = await _referenceDataRepository.GetAcademicDisciplinesMinorAsync(entity.PrimaryKey, true);
                        if (minorsEntity != null)
                        {
                            academicDiscipline = ConvertAcademicDisciplineEntityToAcademicDisciplineDto(minorsEntity);
                        }
                        break;
                    case ("OTHER.SPECIALS"):
                        var specialsEntity = await _referenceDataRepository.GetAcademicDisciplinesSpecialAsync(entity.PrimaryKey, true);
                        if (specialsEntity != null)
                        {
                            academicDiscipline = ConvertAcademicDisciplineEntityToAcademicDisciplineDto(specialsEntity);
                        }
                        break;
                }
                return academicDiscipline;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Discipline not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Converts an AcademicDiscipline domain entity to its corresponding AcademicDiscipline DTO
        /// </summary>
        /// <param name="source">OtherSpecial domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private AcademicDiscipline ConvertAcademicDisciplineEntityToAcademicDisciplineDto(Domain.Base.Entities.AcademicDiscipline source)
        {
            if (source == null)
                throw new ArgumentNullException("Academic Discipline is a required field");

            var academicDiscipline = new AcademicDiscipline
            {
                Id = source.Guid,
                Abbreviation = source.Code,
                Title = source.Description,
                Description = null,
                Type = ConvertAcademicDisciplineTypeEnumToAcademicDisciplineTypeEntityEnum(source.AcademicDisciplineType),
            };

            return academicDiscipline;
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///     Converts an AcademicDiscipline domain entity to its corresponding AcademicDiscipline DTO
        /// </summary>
        /// <param name="source">OtherSpecial domain entity</param>
        /// <returns>AcademicDiscipline DTO</returns>
        private async Task<AcademicDiscipline2> ConvertAcademicDisciplineEntityToAcademicDiscipline2Dto(Domain.Base.Entities.AcademicDiscipline source, String hostCountryIso, bool ignoreCache = false)
        {
            if (source == null)
                throw new ArgumentNullException("Academic Discipline is a required field");

            Dtos.EnumProperties.IsoCode hostCountryEnum;

            if (hostCountryIso.ToUpper() == "CAN" || hostCountryIso.ToUpper() == "CANADA")
            {
                hostCountryEnum = Dtos.EnumProperties.IsoCode.CAN;
            }
            else
            {
                hostCountryEnum = Dtos.EnumProperties.IsoCode.USA;
            }
            var academicDiscipline = new AcademicDiscipline2
            {
                Id = source.Guid,
                Abbreviation = source.Code,
                Title = source.Description,
                Description = null,
                Type = ConvertAcademicDisciplineTypeEnumToAcademicDisciplineTypeEntityEnum(source.AcademicDisciplineType),
            };

            if (academicDiscipline.Type == AcademicDisciplineType.Major)
            {
                var actualMajors = await GetActualMajorsAsync();
                var actualMajor = actualMajors.FirstOrDefault(am => am.Code == academicDiscipline.Abbreviation);
                if (actualMajor != null && !String.IsNullOrEmpty(actualMajor.FederalCourseClassification))
                {
                    academicDiscipline.Reporting = new List<ReportingDtoProperty>() {
                        new ReportingDtoProperty(){
                            Value = new ReportingCountryDtoProperty()
                                    {
                                    Code = hostCountryEnum,
                                    Value = actualMajor.FederalCourseClassification
                                    }
                        }
                    };

                }
            }

            if (academicDiscipline.Type == AcademicDisciplineType.Minor)
            {
                var actualMinors = await GetActualMinorsAsync();
                var actualMinor = actualMinors.FirstOrDefault(am => am.Code == academicDiscipline.Abbreviation);
                if (actualMinor != null && !String.IsNullOrEmpty(actualMinor.FederalCourseClassification))
                {
                    academicDiscipline.Reporting = new List<ReportingDtoProperty>() {
                        new ReportingDtoProperty(){
                            Value = new ReportingCountryDtoProperty()
                                    {
                                    Code = hostCountryEnum,
                                    Value = actualMinor.FederalCourseClassification
                                    }
                        }
                    };

                }
            }

            return academicDiscipline;
        }


        private AcademicDisciplineType ConvertAcademicDisciplineTypeEnumToAcademicDisciplineTypeEntityEnum(Domain.Base.Entities.AcademicDisciplineType? academicDisciplineType)
        {
            if (academicDisciplineType == null)
                throw new ArgumentNullException("AcademicDisciplineType is a required field");

            switch (academicDisciplineType)
            {
                case Domain.Base.Entities.AcademicDisciplineType.Major:
                    return AcademicDisciplineType.Major;
                case Domain.Base.Entities.AcademicDisciplineType.Minor:
                    return AcademicDisciplineType.Minor;
                case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                    return AcademicDisciplineType.Concentration;
                default:
                    throw new InvalidOperationException(string.Format("Unhandled AcademicCredentialType value: {0}", academicDisciplineType.ToString()));    
            }
        }

        private AcademicDisciplineType2 ConvertAcademicDisciplineTypeEnumToAcademicDisciplineType2EntityEnum(Domain.Base.Entities.AcademicDisciplineType? academicDisciplineType)
        {
            switch (academicDisciplineType)
            {
                case Domain.Base.Entities.AcademicDisciplineType.Major:
                    return AcademicDisciplineType2.Major;
                case Domain.Base.Entities.AcademicDisciplineType.Minor:
                    return AcademicDisciplineType2.Minor;
                case Domain.Base.Entities.AcademicDisciplineType.Concentration:
                    return AcademicDisciplineType2.Concentration;
                default:
                    throw new InvalidOperationException(string.Format("Unhandled AcademicCredentialType value: {0}", academicDisciplineType.ToString()));
            }
        }
    }
}