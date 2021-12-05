//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PersonEmploymentProficienciesService : BaseCoordinationService, IPersonEmploymentProficienciesService
    {
        private readonly IPersonEmploymentProficienciesRepository _referenceDataRepository;

        public PersonEmploymentProficienciesService(
            IPersonEmploymentProficienciesRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
                : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all person-employment-proficiencies
        /// </summary>
        /// <returns>Collection of PersonEmploymentProficiencies DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonEmploymentProficiencies>, int>> GetPersonEmploymentProficienciesAsync(int offset, int limit, bool bypassCache = false)
        {
            var personEmploymentProficienciesCollection = new List<Ellucian.Colleague.Dtos.PersonEmploymentProficiencies>();
            int totalCount = 0;
            try
            {
                var tuplePEP = await _referenceDataRepository.GetPersonEmploymentProficienciesAsync(offset, limit, bypassCache);
                var personEmploymentProficienciesEntities = tuplePEP.Item1;
                totalCount = tuplePEP.Item2;
                if (personEmploymentProficienciesEntities != null && personEmploymentProficienciesEntities.Any())
                {
                    foreach (var personEmploymentProficiencies in personEmploymentProficienciesEntities)
                    {
                        try
                        {
                            personEmploymentProficienciesCollection.Add(await ConvertPersonEmploymentProficienciesEntityToDto(personEmploymentProficiencies));
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "Bad.Data", personEmploymentProficiencies.Guid, personEmploymentProficiencies.RecordKey);
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(ex.Message, "Bad.Data", personEmploymentProficiencies.Guid, personEmploymentProficiencies.RecordKey);
                        }
                    }
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return new Tuple<IEnumerable<PersonEmploymentProficiencies>, int>(personEmploymentProficienciesCollection, totalCount);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonEmploymentProficiencies from its GUID
        /// </summary>
        /// <returns>PersonEmploymentProficiencies DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonEmploymentProficiencies> GetPersonEmploymentProficienciesByGuidAsync(string guid)
        {
            try
            {
                var personEmployment = await ConvertPersonEmploymentProficienciesEntityToDto(await _referenceDataRepository.GetPersonEmploymentProficiency(guid));
                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }
                return personEmployment;
            }
            catch (KeyNotFoundException ex)
            {
                //throw new KeyNotFoundException("person-employment-proficiencies not found for GUID " + guid, ex);
                IntegrationApiExceptionAddError("person-employment-proficiencies not found for GUID " + guid, "GUID.Not.Found", guid, "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("person-employment-proficiencies not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PersonEmploymentProficiencies domain entity to its corresponding PersonEmploymentProficiencies DTO
        /// </summary>
        /// <param name="source">PersonEmploymentProficiencies domain entity</param>
        /// <returns>PersonEmploymentProficiencies DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PersonEmploymentProficiencies> ConvertPersonEmploymentProficienciesEntityToDto(PersonEmploymentProficiency source)
        {
            var personEmploymentProficiencies = new Ellucian.Colleague.Dtos.PersonEmploymentProficiencies();
            if (string.IsNullOrWhiteSpace(source.Guid))
            {
                IntegrationApiExceptionAddError("Record is missing GUID, Entity: ‘HR.IND.SKILL’, Record ID: ‘" + source.RecordKey + "’", "Bad.Data", source.Guid, source.RecordKey);
            }
            else
            {
                personEmploymentProficiencies.Id = source.Guid;
            }

            if (string.IsNullOrWhiteSpace(source.PersonId))
            {
                IntegrationApiExceptionAddError("Record is missing Person ID, Entity: ‘HR.IND.SKILL’, Record ID: ‘" + source.RecordKey + "’", "Bad.Data", source.Guid, source.RecordKey);
            }
            else
            {
                var personGuid = await _referenceDataRepository.GetGuidFromID(source.PersonId, "PERSON");
                personEmploymentProficiencies.Person = new GuidObject2(personGuid);
            }

            if (string.IsNullOrWhiteSpace(source.ProficiencyId))
            {
                IntegrationApiExceptionAddError("Record is missing Proficiency ID, Entity: ‘JOBSKILLS’, Record ID: ‘" + source.RecordKey + "’", "Bad.Data", source.Guid, source.RecordKey);
            }
            else
            {
                var proficiencyGuid = await _referenceDataRepository.GetGuidFromID(source.ProficiencyId, "JOBSKILLS");
                personEmploymentProficiencies.Proficiency = new GuidObject2(proficiencyGuid);
            }

            personEmploymentProficiencies.StartOn = source.StartOn.HasValue ? source.StartOn.Value : new DateTime();
            personEmploymentProficiencies.EndOn = source.EndOn.HasValue ? source.EndOn.Value : new DateTime();

            if (!string.IsNullOrEmpty(source.Comment))
            {
                personEmploymentProficiencies.Comment = source.Comment;
            }

            return personEmploymentProficiencies;
        }
    }
}