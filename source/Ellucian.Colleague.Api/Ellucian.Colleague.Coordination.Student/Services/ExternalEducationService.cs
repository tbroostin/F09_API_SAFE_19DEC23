// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using ExternalEducation = Ellucian.Colleague.Domain.Base.Entities.ExternalEducation;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class ExternalEducationService : BaseCoordinationService, IExternalEducationService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IExternalEducationRepository _externalEducationRepository;
        private readonly IInstitutionsAttendRepository _institutionsAttendRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public ExternalEducationService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
            IPersonRepository personRepository, IExternalEducationRepository externalEducationRepository,
            IInstitutionsAttendRepository institutionsAttendRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _externalEducationRepository = externalEducationRepository;
            _institutionsAttendRepository = institutionsAttendRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
        }

        private IEnumerable<Domain.Base.Entities.OtherDegree> _otherDegrees = null;

        private async Task<IEnumerable<Domain.Base.Entities.OtherDegree>> GetOtherDegreesAsync(bool bypassCache)
        {
            if (_otherDegrees == null)
            {
                _otherDegrees = await _referenceDataRepository.GetOtherDegreesAsync(bypassCache);
            }
            return _otherDegrees;
        }

        private IEnumerable<Domain.Base.Entities.OtherMajor> _otherMajor = null;

        private async Task<IEnumerable<Domain.Base.Entities.OtherMajor>> GetOtherMajorsAsync(bool bypassCache)
        {
            if (_otherMajor == null)
            {
                _otherMajor = await _referenceDataRepository.GetOtherMajorsAsync(bypassCache);
            }
            return _otherMajor;
        }

        private IEnumerable<Domain.Base.Entities.OtherMinor> _otherMinor = null;

        private async Task<IEnumerable<Domain.Base.Entities.OtherMinor>> GetOtherMinorsAsync(bool bypassCache)
        {
            if (_otherMinor == null)
            {
                _otherMinor = await _referenceDataRepository.GetOtherMinorsAsync(bypassCache);
            }
            return _otherMinor;
        }

        private IEnumerable<Domain.Base.Entities.OtherSpecial> _otherSpecials = null;

        private async Task<IEnumerable<Domain.Base.Entities.OtherSpecial>> GetOtherSpecialsAsync(bool bypassCache)
        {
            if (_otherSpecials == null)
            {
                _otherSpecials = await _referenceDataRepository.GetOtherSpecialsAsync(bypassCache);
            }
            return _otherSpecials;
        }

        private IEnumerable<Domain.Base.Entities.OtherHonor> _otherHonors = null;

        private async Task<IEnumerable<Domain.Base.Entities.OtherHonor>> GetOtherHonorsAsync(bool bypassCache)
        {
            if (_otherHonors == null)
            {
                _otherHonors = await _referenceDataRepository.GetOtherHonorsAsync(bypassCache);
            }
            return _otherHonors;
        }

        #region ExternalEducation
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all External Educations
        /// </summary>
        /// <returns>Collection of External Education DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.ExternalEducation>, int>> GetExternalEducationsAsync(int offset, int limit, bool bypassCache = false, string personGuid = "")
        {
            //this.CheckViewExternalEducationPermission();

            var externalEducationsCollection = new List<Dtos.ExternalEducation>();
            // Convert and validate all input parameters
            var personId = string.Empty;
            if (!string.IsNullOrEmpty(personGuid))
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(personGuid);
                    if (string.IsNullOrEmpty(personId))
                    {
                        throw new ArgumentException(string.Concat("GUID not found for person: ", personGuid));
                    }
                }
                catch (KeyNotFoundException e)
                {
                    return new Tuple<IEnumerable<Dtos.ExternalEducation>, int>(new List<Dtos.ExternalEducation>(), 0);
                }
            }

            var externalEducationEntitiesTuple = await _externalEducationRepository.GetExternalEducationAsync(offset, limit, bypassCache, personId);
            if (externalEducationEntitiesTuple != null)
            {
                var externalEducationProgEntities = externalEducationEntitiesTuple.Item1;
                var totalCount = externalEducationEntitiesTuple.Item2;
               
                if (externalEducationProgEntities != null && externalEducationProgEntities.Any())
                {
                    foreach (var acadCred in externalEducationProgEntities)
                    {
                        externalEducationsCollection.Add(await ConvertExternalEducationToDtoAsync(acadCred, bypassCache));
                    }
                }

                return new Tuple<IEnumerable<Dtos.ExternalEducation>, int>(externalEducationsCollection, totalCount);
            }
            else
            {
                throw new KeyNotFoundException("External Education records not found ");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an External Education from its GUID
        /// </summary>
        /// <returns>ExternalEducation DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ExternalEducation> GetExternalEducationByGuidAsync(string guid)
        {          
            try
            {
                //this.CheckViewExternalEducationPermission();

                var acadCredentialId = await _externalEducationRepository.GetExternalEducationIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(acadCredentialId))
                    throw new KeyNotFoundException("Academic Credential ID associated to guid '" + guid + "' not found in repository");

                var acadCredentialEntity = await _externalEducationRepository.GetExternalEducationByIdAsync(acadCredentialId);

                if (acadCredentialEntity == null)
                    throw new KeyNotFoundException("Academic Credential '" + guid + "' not found in repository");

                return await ConvertExternalEducationToDtoAsync(acadCredentialEntity);

            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("External Education not found for GUID  " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("External Education not found for GUID " + guid, ex);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException("External Education not found for GUID " + guid, ex);
            }
        }
        #endregion

        #region Private methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an ExternalEducation domain entity to its corresponding ExternalEducation DTO
        /// </summary>
        /// <param name="source">ExternalEducationl domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>ExternalEducation DTO</returns>
        private async Task<Dtos.ExternalEducation> ConvertExternalEducationToDtoAsync(ExternalEducation source, bool bypassCache = false)
        {

            if (source == null)
                throw new ArgumentNullException("source", "Must provide a source to convert.");

            var externalEducation = new Dtos.ExternalEducation
            {
                Id = source.Guid,
                PerformanceMeasure = source.AcadGpa.HasValue ? source.AcadGpa.Value.ToString() : null,
                GraduatedOn = source.AcadCommencementDate,
                CredentialsDate = source.AcadDegreeDate,       
                ThesisTitle = source.AcadThesis == "" ? null : source.AcadThesis,
                ClassSize = source.AcadRankDenominator,
                ClassRank = source.AcadRankNumerator
            };

            if (!string.IsNullOrEmpty(source.AcadPersonId))
            {
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.AcadPersonId);
                if (!string.IsNullOrEmpty(personGuid))
                    externalEducation.Person = new GuidObject2(personGuid);
            }

            if (!string.IsNullOrEmpty(source.AcadInstitutionsId))
            {
                var institutionGuid = await _personRepository.GetPersonGuidFromIdAsync(source.AcadInstitutionsId);
                if (!string.IsNullOrEmpty(institutionGuid))
                    externalEducation.Institution = new GuidObject2(institutionGuid);
            }

            if (source.InstTransciptDate != null)
                externalEducation.TranscriptReceivedOn = source.InstTransciptDate.Max();

            try
            {
                var instExtCredits = Convert.ToInt32(source.InstExtCredits);
                if (instExtCredits != 0)
                    externalEducation.CreditsEarned = instExtCredits;

                var acadRankPercent = Convert.ToInt32(source.AcadRankPercent);
                if (acadRankPercent != 0)
                    externalEducation.ClassPercentile = acadRankPercent;
            }
            catch (InvalidCastException)
            {
                throw new ColleagueWebApiException("Unable to extract credits earned and/or class percentile");
            }

            if (source.AcadStartDate.HasValue)
            {
                var startDate = new DateDtoProperty
                {
                    Month = source.AcadStartDate.Value.Month,
                    Day = source.AcadStartDate.Value.Day,
                    Year = source.AcadStartDate.Value.Year
                };
                externalEducation.StartOn = startDate;
            }

            if (source.AcadEndDate.HasValue)
            {
                var endDate = new DateDtoProperty
                {
                    Month = source.AcadEndDate.Value.Month,
                    Day = source.AcadEndDate.Value.Day,
                    Year = source.AcadEndDate.Value.Year
                };
                externalEducation.EndOn = endDate;
            }

            var degree = (await GetOtherDegreesAsync(bypassCache)).FirstOrDefault(d => d.Code == source.AcadDegree);
            if (degree != null)
                externalEducation.Credential = new GuidObject2(degree.Guid);

            var disciplines = new List<GuidObject2>();

            foreach (var acadMajor in source.AcadMajors)
            {
                var major = (await GetOtherMajorsAsync(bypassCache)).FirstOrDefault(x => x.Code == acadMajor);
                if (major != null)
                {
                    disciplines.Add(new GuidObject2(major.Guid));
                }
            }

            foreach (var acadMinor in source.AcadMinors)
            {
                var minor = (await GetOtherMinorsAsync(bypassCache)).FirstOrDefault(x => x.Code == acadMinor);
                if (minor != null)
                {
                    disciplines.Add(new GuidObject2(minor.Guid));
                }
            }

            foreach (var acadSpecialization in source.AcadSpecialization)
            {
                var specialization = (await GetOtherSpecialsAsync(bypassCache)).FirstOrDefault(x => x.Code == acadSpecialization);
                if (specialization != null)
                {
                    disciplines.Add(new GuidObject2(specialization.Guid));
                }
            }
            externalEducation.Disciplines = disciplines.Any() ? disciplines : null;

            var honors = new List<GuidObject2>();
            foreach (var acadHonor in source.AcadHonors)
            {
                var honor = (await GetOtherHonorsAsync(bypassCache)).FirstOrDefault(oh => oh.Code == acadHonor);
                if (honor != null)
                    honors.Add(new GuidObject2(honor.Guid));
            }
            externalEducation.Recognition = honors.Any() ? honors : null;

            return externalEducation;
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewExternalEducationPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewExternalEducation);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view external education.");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation on person external education. 
        /// This API will integrate information that could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewPerExternalEducationPermission()
        {
            var hasPermission = HasPermission(StudentPermissionCodes.ViewPerExternalEducation);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view person external education.");
            }
        }

        private DateDtoProperty ConvertDateTimeToDateDtoProperty(DateTime? value)
        {
            DateDtoProperty dateDtoProperty = null;

            if ((value.HasValue) && (value != default(DateTime)))
            {
                var convertedDateTime = Convert.ToDateTime(value);

                dateDtoProperty = new DateDtoProperty
                {
                    Month = convertedDateTime.Month, 
                    Day = convertedDateTime.Day,
                    Year = convertedDateTime.Year
                };
            }
            return dateDtoProperty;
        }
        #endregion

    }
}