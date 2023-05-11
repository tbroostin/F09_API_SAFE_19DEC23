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
    public class EmploymentPerformanceReviewsService : BaseCoordinationService, IEmploymentPerformanceReviewsService
    {

        private readonly IEmploymentPerformanceReviewsRepository _employmentPerformanceReviewRepository;
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public EmploymentPerformanceReviewsService(

            IEmploymentPerformanceReviewsRepository employmentPerformanceReviewRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IPersonRepository personRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _employmentPerformanceReviewRepository = employmentPerformanceReviewRepository;
            _hrReferenceDataRepository = hrReferenceDataRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all employment-performance-reviews
        /// </summary>
        /// <returns>Collection of EmploymentPerformanceReviews DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.EmploymentPerformanceReviews>, int>> GetEmploymentPerformanceReviewsAsync(int offset, int limit, bool bypassCache = false)
        {
            var employmentPerformanceReviewsCollection = new List<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews>();
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentPerformanceReview>, int> employmentPerformanceReviewsData = null;

            try
            {
                employmentPerformanceReviewsData = await _employmentPerformanceReviewRepository.GetEmploymentPerformanceReviewsAsync(offset, limit, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (employmentPerformanceReviewsData != null)
            {
                var employmentPerformanceReviewsEntities = employmentPerformanceReviewsData.Item1;
                int totalRecords = employmentPerformanceReviewsData.Item2;

                if (employmentPerformanceReviewsEntities != null && employmentPerformanceReviewsEntities.Any())
                {
                    employmentPerformanceReviewsCollection = (await BuildEmploymentPerformanceReviewsDtoAsync(employmentPerformanceReviewsEntities, bypassCache)).ToList();
                    return new Tuple<IEnumerable<Dtos.EmploymentPerformanceReviews>, int>(employmentPerformanceReviewsCollection, totalRecords);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.EmploymentPerformanceReviews>, int>(new List<Dtos.EmploymentPerformanceReviews>(), 0);
                }
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.EmploymentPerformanceReviews>, int>(employmentPerformanceReviewsCollection, 0);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmploymentPerformanceReviews from its GUID
        /// </summary>
        /// <returns>EmploymentPerformanceReviews DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews> GetEmploymentPerformanceReviewsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain employment-performance-reviews.");
            }

            try
            {
                var entity = await _employmentPerformanceReviewRepository.GetEmploymentPerformanceReviewByIdAsync(guid);
                var employmentPerformanceReviewsDto = await BuildEmploymentPerformanceReviewsDtoAsync(new List<Domain.HumanResources.Entities.EmploymentPerformanceReview>(){entity});

                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

                if (employmentPerformanceReviewsDto.Any())
                {
                    return employmentPerformanceReviewsDto.FirstOrDefault();
                }
                else
                {
                    throw new KeyNotFoundException("No employment-performance-reviews found for GUID " + guid);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No employment-performance-reviews found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No employment-performance-reviews found for GUID " + guid, ex);
            }
        }

        #region POST Methods

        public async Task<EmploymentPerformanceReviews> PostEmploymentPerformanceReviewsAsync(EmploymentPerformanceReviews employmentPerformanceReviewsDto)
        {
            if (employmentPerformanceReviewsDto == null)
            {
                throw new ArgumentNullException("employmentPerformanceReviewsDto", "Must provide an employmentPerformanceReviews for create");
            }

            if (string.IsNullOrEmpty(employmentPerformanceReviewsDto.Id))
            {
                throw new ArgumentNullException("employmentPerformanceReviewsDto", "Must provide a guid for employmentPerformanceReviews create");
            }
            // verify the user has the permission to create a employmentPerformanceReviews
            //CheckUserEmploymentPerformanceReviewsCreateUpdatePermissions();

            _employmentPerformanceReviewRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // map the DTO to entities
            var employmentPerformanceReviewsEntity = await ConvertEmploymentPerformanceReviewsDtoToEntityAsync(employmentPerformanceReviewsDto.Id, employmentPerformanceReviewsDto);

            // create the entity in the database
            var newEntity = await _employmentPerformanceReviewRepository.CreateEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsEntity);

            return await ConvertEmploymentPerformanceReviewsEntityToDto(newEntity, true);

        }

        #endregion

        #region PUT Methods
        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Put (Update) an EmploymentPerformanceReviews domain entity
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="employmentPerformanceReviewsDto"><see cref="Dtos.EmploymentPerformanceReviews">EmploymentPerformanceReviews</see></param>
        /// <returns><see cref="Dtos.EmploymentPerformanceReviews">EmploymentPerformanceReviews</see></returns>
        /// <exception><see cref="ArgumentNullException">ArgumentNullException</see></exception>
        public async Task<EmploymentPerformanceReviews> PutEmploymentPerformanceReviewsAsync(string guid, Dtos.EmploymentPerformanceReviews employmentPerformanceReviewsDto)
        {
            if (employmentPerformanceReviewsDto == null)
                throw new ArgumentNullException("employmentPerformanceReviews", "Must provide a employmentPerformanceReviews for update");
            if (string.IsNullOrEmpty(employmentPerformanceReviewsDto.Id))
                throw new ArgumentNullException("employmentPerformanceReviews", "Must provide a guid for employmentPerformanceReviews update");

            // get the person ID associated with the incoming guid
            var EmploymentPerformanceReviewsId = await _employmentPerformanceReviewRepository.GetIdFromGuidAsync(employmentPerformanceReviewsDto.Id, "PERPOS");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(EmploymentPerformanceReviewsId))
            {
                // verify the user has the permission to update a employmentPerformanceReviews
                //CheckUserEmploymentPerformanceReviewsCreateUpdatePermissions();

                _employmentPerformanceReviewRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the DTO to entities
                var employmentPerformanceReviewsEntity
                    = await ConvertEmploymentPerformanceReviewsDtoToEntityAsync(EmploymentPerformanceReviewsId, employmentPerformanceReviewsDto, true);

                // update the entity in the database
                var updatedEmploymentPerformanceReviewsEntity =
                    await _employmentPerformanceReviewRepository.UpdateEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsEntity);

                return await ConvertEmploymentPerformanceReviewsEntityToDto(updatedEmploymentPerformanceReviewsEntity, true);


            }
            // perform a create instead
            return await PostEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsDto);
        }
        #endregion

        #region DELETE Methods
        /// <summary>
        /// Delete employment performance review based on employment performance review id
        /// </summary>
        /// <param name="employmentPerformanceReviewsId"></param>
        /// <returns></returns>
        public async Task DeleteEmploymentPerformanceReviewAsync(string employmentPerformanceReviewsId)
        {
            // get user permissions
            //CheckUserEmploymentPerformanceReviewsDeletePermissions();

            try
            {
                var entity = await _employmentPerformanceReviewRepository.GetEmploymentPerformanceReviewByIdAsync(employmentPerformanceReviewsId);

                if (entity == null)
                {
                    throw new KeyNotFoundException();
                }

                await _employmentPerformanceReviewRepository.DeleteEmploymentPerformanceReviewsAsync(employmentPerformanceReviewsId);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("Employment-performance-reviews not found for guid: '{0}'.", employmentPerformanceReviewsId));
            }
        }
        #endregion

        private IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewType> _employmentPerformanceReviewTypes = null;
        private async Task<IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewType>> GetAllEmploymentPerformanceReviewTypesAsync(bool bypassCache)
        {
            if (_employmentPerformanceReviewTypes == null)
            {
                _employmentPerformanceReviewTypes = await _hrReferenceDataRepository.GetEmploymentPerformanceReviewTypesAsync(bypassCache);
            }
            return _employmentPerformanceReviewTypes;
        }

        private IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewRating> _employmentPerformanceReviewRatings = null;
        private async Task<IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReviewRating>> GetAllEmploymentPerformanceReviewRatingsAsync(bool bypassCache)
        {
            if (_employmentPerformanceReviewRatings == null)
            {
                _employmentPerformanceReviewRatings = await _hrReferenceDataRepository.GetEmploymentPerformanceReviewRatingsAsync(bypassCache);
            }
            return _employmentPerformanceReviewRatings;
        }

        /// <summary>
        /// BuildEmploymentPerformanceReviewsDtoAsync
        /// </summary>
        /// <param name="sources">Collection of EmploymentPerformanceReview domain entities</param>
        /// <param name="bypassCache">bypassCache flag.  Defaulted to false</param>
        /// <returns>Collection of EmploymentPerformanceReviews DTO objects </returns>
        private async Task<IEnumerable<Dtos.EmploymentPerformanceReviews>> BuildEmploymentPerformanceReviewsDtoAsync(IEnumerable<Domain.HumanResources.Entities.EmploymentPerformanceReview> sources,
            bool bypassCache = false)
        {

            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }

            var employmentPerformanceReviews = new List<Dtos.EmploymentPerformanceReviews>();
            
            Dictionary<string, string> personGuidCollection = null;
            try
            {
                var personIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
                     .Select(x => x.PersonId).Distinct().ToList();
                var reviewerIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.ReviewedById)))
                     .Select(x => x.ReviewedById).Distinct().ToList();
                personIds.AddRange(reviewerIds);
                personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            Dictionary<string, string> perposGuidCollection = null;
            try
            {
                var perposIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.PerposId)))
                     .Select(x => x.PerposId).Distinct().ToList();
                perposGuidCollection = await _employmentPerformanceReviewRepository.GetGuidsCollectionAsync(perposIds, "PERPOS");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            foreach (var source in sources)
            {
                try
                {
                    employmentPerformanceReviews.Add(await ConvertEmploymentPerformanceReviewsEntityToDto(source, personGuidCollection, perposGuidCollection, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, id: source.PersonId, guid: source.Guid);
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return employmentPerformanceReviews;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a EmploymentPerformanceReviews domain entity to its corresponding EmploymentPerformanceReviews DTO
        /// </summary>
        /// <param name="source">EmploymentPerformanceReviews domain entity</param>
        /// <returns>EmploymentPerformanceReviews DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews> ConvertEmploymentPerformanceReviewsEntityToDto(EmploymentPerformanceReview source, bool bypassCache)
        {
            var employmentPerformanceReviews = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviews();

            employmentPerformanceReviews.Id = source.Guid;
            var personGuid = await _employmentPerformanceReviewRepository.GetGuidFromIdAsync(source.PersonId, "PERSON");
            employmentPerformanceReviews.Person = new GuidObject2(personGuid);
            var jobGuid = await _employmentPerformanceReviewRepository.GetGuidFromIdAsync(source.PerposId, "PERPOS");
            employmentPerformanceReviews.Job = new GuidObject2(jobGuid);
            employmentPerformanceReviews.CompletedOn = (DateTime)source.CompletedDate;
            var typeEntities = await GetAllEmploymentPerformanceReviewTypesAsync(bypassCache);
            if (typeEntities.Any())
            {
                var typeEntity = typeEntities.FirstOrDefault(ep => ep.Code == source.RatingCycleCode);
                if (typeEntity != null)
                {
                    employmentPerformanceReviews.Type = new GuidObject2(typeEntity.Guid);
                }
            }
            if (!string.IsNullOrEmpty(source.ReviewedById))
            {
                var reviewGuid = await _employmentPerformanceReviewRepository.GetGuidFromIdAsync(source.ReviewedById, "PERSON");
                employmentPerformanceReviews.ReviewedBy = new GuidObject2(reviewGuid);
            }
            var ratingEntities = await GetAllEmploymentPerformanceReviewRatingsAsync(bypassCache);
            if (ratingEntities.Any())
            {
                var ratingEntity = ratingEntities.FirstOrDefault(ep => ep.Code == source.RatingCode);
                if (ratingEntity != null)
                {
                    var rating = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviewsRatingDtoProperty() { Detail = new GuidObject2(ratingEntity.Guid) };
                    employmentPerformanceReviews.Rating = rating;
                }
            }
            if (!string.IsNullOrWhiteSpace(source.Comment))
                employmentPerformanceReviews.Comment = source.Comment;

            return employmentPerformanceReviews;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an EmploymentPerformanceReviews domain entity to its corresponding EmploymentPerformanceReviews DTO
        /// Uses incoming person and perpos GUID collections
        /// </summary>
        /// <param name="source">EmploymentPerformanceReviews domain entity</param>
        /// <returns>EmploymentPerformanceReviews DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews> ConvertEmploymentPerformanceReviewsEntityToDto(EmploymentPerformanceReview source,
            Dictionary<string, string> personGuidCollection, Dictionary<string, string> perposGuidCollection, bool bypassCache)
        {
            var employmentPerformanceReviews = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviews();
            
            // get guid
            employmentPerformanceReviews.Id = source.Guid;
            
            // get person
            if (!string.IsNullOrEmpty(source.PersonId))
            {
                if (personGuidCollection != null)
                {
                    var personGuid = string.Empty;
                    personGuidCollection.TryGetValue(source.PersonId, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for person id: '", source.PersonId, "'"),
                           "GUID.Not.Found", source.Guid, source.PerposId);
                    }
                    else
                    {
                        employmentPerformanceReviews.Person = new Dtos.GuidObject2(personGuid);
                    }                    
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("GUID not found for person id '", source.PersonId, "'"),
                        "", source.Guid, source.PerposId);
                }
            }
            else
            {
                IntegrationApiExceptionAddError(string.Concat("GUID not found for person id: '", source.PersonId, "'"),
                           "GUID.Not.Found", source.Guid, source.PerposId);
            }
            
            // get job
            if (!string.IsNullOrEmpty(source.PerposId))
            {
                if (perposGuidCollection != null)
                {
                    var perposGuid = string.Empty;
                    perposGuidCollection.TryGetValue(source.PerposId, out perposGuid);
                    if (string.IsNullOrEmpty(perposGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for perpos id: '", source.PerposId, "'"),
                           "GUID.Not.Found", source.Guid, source.PerposId);
                    }
                    else
                    {
                        employmentPerformanceReviews.Job = new Dtos.GuidObject2(perposGuid);
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("GUID not found for perpos id '", source.PerposId, "'"),
                        "", source.Guid, source.PerposId);
                }
            }

            // get completedOn
            employmentPerformanceReviews.CompletedOn = (DateTime)source.CompletedDate;

            // get type
            if (!string.IsNullOrEmpty(source.RatingCycleCode))
            {
                var typeEntities = await GetAllEmploymentPerformanceReviewTypesAsync(bypassCache);
                if (typeEntities != null)
                {
                    var typeEntity = typeEntities.FirstOrDefault(ep => ep.Code == source.RatingCycleCode);
                    if (typeEntity != null)
                    {
                        employmentPerformanceReviews.Type = new GuidObject2(typeEntity.Guid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("GUID not found for employment performance review type '{0}'", source.RatingCycleCode), "GUID.Not.Found", source.Guid, source.PerposId);
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("GUID not found for employment performance review type '{0}'", source.RatingCycleCode), "GUID.Not.Found", source.Guid, source.PerposId);
                }
            }

            // get reviewedBy
            if (!string.IsNullOrEmpty(source.ReviewedById))
            {
                if (personGuidCollection != null)
                {
                    var personGuid = string.Empty;
                    personGuidCollection.TryGetValue(source.ReviewedById, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("GUID not found for reviewed by person id: '", source.ReviewedById, "'"),
                           "GUID.Not.Found", source.Guid, source.PerposId);
                    }
                    else
                    {
                        employmentPerformanceReviews.ReviewedBy = new Dtos.GuidObject2(personGuid);
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("GUID not found for person id '", source.ReviewedById, "'"),
                        "", source.Guid, source.PerposId);                    
                } 
            }

            // get rating
            var ratingEntities = await GetAllEmploymentPerformanceReviewRatingsAsync(bypassCache);
            if (!string.IsNullOrEmpty(source.RatingCode))
            {
                if (ratingEntities != null)
                {
                    var ratingEntity = ratingEntities.FirstOrDefault(ep => ep.Code == source.RatingCode);
                    if (ratingEntity != null)
                    {
                        var rating = new Ellucian.Colleague.Dtos.EmploymentPerformanceReviewsRatingDtoProperty() { Detail = new GuidObject2(ratingEntity.Guid) };
                        employmentPerformanceReviews.Rating = rating;
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("GUID not found for employment performance review rating '{0}'", source.RatingCode), "GUID.Not.Found", source.Guid, source.PerposId);
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("GUID not found for employment performance review rating '{0}'", source.RatingCode), "GUID.Not.Found", source.Guid, source.PerposId);
                }
            }
            // get comment
            if (!string.IsNullOrWhiteSpace(source.Comment))
                employmentPerformanceReviews.Comment = source.Comment;

            return employmentPerformanceReviews;
        }

        /// <summary>
        /// Convert an EmploymentPerformanceReviews Dto to an EmploymentPerformanceReviews Entity
        /// </summary>
        /// <param name="employmentPerformanceReviewsId">guid</param>
        /// <param name="employmentPerformanceReviewsDto"><see cref="Dtos.EmploymentPerformanceReviews">EmploymentPerformanceReviews</see></param>
        /// <returns><see cref="Domain.HumanResources.Entities.EmploymentPerformanceReviews source">EmploymentPerformanceReviews</see></returns>
        private async Task<Domain.HumanResources.Entities.EmploymentPerformanceReview> ConvertEmploymentPerformanceReviewsDtoToEntityAsync(string employmentPerformanceReviewsId, EmploymentPerformanceReviews employmentPerformanceReviews, bool bypassCache = false)
        {
            if (employmentPerformanceReviews == null || string.IsNullOrEmpty(employmentPerformanceReviews.Id))
                throw new ArgumentNullException("EmploymentPerformanceReviews", "Must provide guid for an Employment Performance Reviews");
            if (string.IsNullOrEmpty(employmentPerformanceReviewsId))
                throw new ArgumentNullException("EmploymentPerformanceReviews", string.Concat("Must provide an id for Institution Job.  Guid: ", employmentPerformanceReviews.Id));

            if ((employmentPerformanceReviews.Person == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Person.Id)))
            {
                throw new ArgumentNullException("Person ID is required for Employment Performance Reviews.");
            }

            string personId;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(employmentPerformanceReviews.Person.Id);

                if (string.IsNullOrEmpty(personId))
                {
                    throw new ArgumentException("Person not found for Id:" + employmentPerformanceReviews.Person.Id);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Person not found for Id:" + employmentPerformanceReviews.Person.Id);
            }

            if ((employmentPerformanceReviews.CompletedOn == DateTime.MinValue))
            {
                throw new ArgumentNullException("The completedOn date is a required field for Colleague.");
            }

            if ((employmentPerformanceReviews.Job == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Job.Id)))
            {
                throw new ArgumentNullException("Job ID is required for Employment Performance Reviews.");
            }

            string job;
            try
            {
                job = await _employmentPerformanceReviewRepository.GetIdFromGuidAsync(employmentPerformanceReviews.Job.Id, "PERPOS");
                if ((job == null) || (string.IsNullOrEmpty(job)))
                {
                    throw new ArgumentException("Job not found for Id:" + employmentPerformanceReviews.Job.Id);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Job not found for Id:" + employmentPerformanceReviews.Job.Id);
            }

            if ((employmentPerformanceReviews.Type == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Type.Id)))
            {
                throw new ArgumentNullException("Type ID is required for Employment Performance Reviews.");
            }

            Ellucian.Data.Colleague.GuidLookupResult type;
            try
            {
                type = await _employmentPerformanceReviewRepository.GetInfoFromGuidAsync(employmentPerformanceReviews.Type.Id);
                if ((type == null) || (string.IsNullOrEmpty(type.SecondaryKey)))
                {
                    throw new ArgumentException("The type ID supplied does not exist for EVALUTION.CYCLES valcode.");
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("The type ID supplied does not exist for EVALUTION.CYCLES valcode.");
            }

            if ((employmentPerformanceReviews.Rating == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Rating.Detail.Id)))
            {
                throw new ArgumentNullException("Rating ID is required for Employment Performance Reviews.");
            }

            Ellucian.Data.Colleague.GuidLookupResult rating;
            try
            {
                rating = await _employmentPerformanceReviewRepository.GetInfoFromGuidAsync(employmentPerformanceReviews.Rating.Detail.Id);
                if ((rating == null) || (string.IsNullOrEmpty(rating.SecondaryKey)))
                {
                    throw new ArgumentException("The rating detail id does not exist for PERFORMANCE.EVAL.RATINGS valcode.");
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("The rating detail id does not exist for PERFORMANCE.EVAL.RATINGS valcode.");
            }

            EmploymentPerformanceReview response = new EmploymentPerformanceReview(employmentPerformanceReviews.Id, personId, job, employmentPerformanceReviews.CompletedOn, type.SecondaryKey, rating.SecondaryKey);

            if ((employmentPerformanceReviews.ReviewedBy != null) && (!string.IsNullOrEmpty(employmentPerformanceReviews.ReviewedBy.Id)))
            {
                try
                {
                    response.ReviewedById = await _employmentPerformanceReviewRepository.GetIdFromGuidAsync(employmentPerformanceReviews.ReviewedBy.Id, "PERSON");
                }
                catch (Exception)
                {
                    throw new ArgumentException("Person not found for Id:" + employmentPerformanceReviews.ReviewedBy.Id);
                }
            }

            if (!string.IsNullOrEmpty(employmentPerformanceReviews.Comment))
            {
                response.Comment = employmentPerformanceReviews.Comment;
            }

            return response;
        }
    }
}