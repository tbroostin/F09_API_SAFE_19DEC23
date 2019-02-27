//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.Base.Repositories;

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
           CheckUserEmploymentPerformanceReviewsViewPermissions();

            var employmentPerformanceReviewsCollection = new List<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews>();

            var pageOfItems = await _employmentPerformanceReviewRepository.GetEmploymentPerformanceReviewsAsync(offset, limit, bypassCache);
            
            var employmentPerformanceReviewsEntities = pageOfItems.Item1;
            int totalRecords = pageOfItems.Item2;

            if (employmentPerformanceReviewsEntities != null && employmentPerformanceReviewsEntities.Any())
            {
                foreach (var employmentPerformanceReviews in employmentPerformanceReviewsEntities)
                {
                    employmentPerformanceReviewsCollection.Add(await ConvertEmploymentPerformanceReviewsEntityToDto(employmentPerformanceReviews, bypassCache));
                }

                return new Tuple<IEnumerable<Dtos.EmploymentPerformanceReviews>, int>(employmentPerformanceReviewsCollection, totalRecords);
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.EmploymentPerformanceReviews>, int>(new List<Dtos.EmploymentPerformanceReviews>(), 0);
            }
            
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmploymentPerformanceReviews from its GUID
        /// </summary>
        /// <returns>EmploymentPerformanceReviews DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentPerformanceReviews> GetEmploymentPerformanceReviewsByGuidAsync(string guid)
        {
            try
            {
                var entity = await _employmentPerformanceReviewRepository.GetEmploymentPerformanceReviewByIdAsync(guid);

                if (entity != null) 
                {
                    return await ConvertEmploymentPerformanceReviewsEntityToDto(entity, true);
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("employment-performance-reviews not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("employment-performance-reviews not found for GUID " + guid, ex);
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
            CheckUserEmploymentPerformanceReviewsCreateUpdatePermissions();

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
            var EmploymentPerformanceReviewsId = await _employmentPerformanceReviewRepository.GetIdFromGuidAsync(employmentPerformanceReviewsDto.Id);

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(EmploymentPerformanceReviewsId))
            {
                // verify the user has the permission to update a employmentPerformanceReviews
                CheckUserEmploymentPerformanceReviewsCreateUpdatePermissions();

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
            CheckUserEmploymentPerformanceReviewsDeletePermissions();

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
                throw new KeyNotFoundException(string.Format("Employment-performance-reviews not found for guid: '{0}'.",  employmentPerformanceReviewsId));
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
            var jobGuid = await _employmentPerformanceReviewRepository.GetJobGuidFromIdAsync(source.PerposId, "PERPOS");
            employmentPerformanceReviews.Job = new GuidObject2(jobGuid);
            employmentPerformanceReviews.CompletedOn = (DateTime) source.CompletedDate;
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


            Domain.HumanResources.Entities.EmploymentPerformanceReview response = null;

            if ((employmentPerformanceReviews.Person == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Person.Id)))
            {
                throw new ArgumentNullException("Person ID is required for Employment Performance Reviews.");
            }

            var personId = await _personRepository.GetPersonIdFromGuidAsync(employmentPerformanceReviews.Person.Id);

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentException("Person not found for Id:" + employmentPerformanceReviews.Person.Id);
            }

            if ((employmentPerformanceReviews.CompletedOn == DateTime.MinValue))
            {
                throw new ArgumentNullException("The completedOn date is a required field for colleague.");
            }

            if ((employmentPerformanceReviews.Job == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Job.Id)))
            {
                throw new ArgumentNullException("Job ID is required for Employment Performance Reviews.");
            }
            var job = await _employmentPerformanceReviewRepository.GetIdFromGuidAsync(employmentPerformanceReviews.Job.Id);
            if ((job == null) || (string.IsNullOrEmpty(job)))
            {
                throw new ArgumentException("Job not found for Id:" + employmentPerformanceReviews.Job.Id);
            }

            if ((employmentPerformanceReviews.Type == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Type.Id)))
            {
                throw new ArgumentNullException("Type ID is required for Employment Performance Reviews.");
            }
            var type = await _employmentPerformanceReviewRepository.GetInfoFromGuidAsync(employmentPerformanceReviews.Type.Id);
            if ((type == null) || (string.IsNullOrEmpty(type.SecondaryKey)))
            {
                throw new ArgumentException("The type ID supplied does not exist for EVALUTION.CYCLES valcode.");
            }

            if ((employmentPerformanceReviews.Rating == null) || (string.IsNullOrEmpty(employmentPerformanceReviews.Rating.Detail.Id)))
            {
                throw new ArgumentNullException("Rating ID is required for Employment Performance Reviews.");
            }
            var rating = await _employmentPerformanceReviewRepository.GetInfoFromGuidAsync(employmentPerformanceReviews.Rating.Detail.Id);
            if ((rating == null) || (string.IsNullOrEmpty(rating.SecondaryKey)))
            {
                throw new ArgumentException("The rating detail id does not exist for PERFORMANCE.EVAL.RATINGS valcode.");
            }

            response = new Domain.HumanResources.Entities.EmploymentPerformanceReview(employmentPerformanceReviews.Id, personId, job, employmentPerformanceReviews.CompletedOn, type.SecondaryKey, rating.SecondaryKey);

            if ((employmentPerformanceReviews.ReviewedBy != null) && (!string.IsNullOrEmpty(employmentPerformanceReviews.ReviewedBy.Id)))
            {
                response.ReviewedById = await _employmentPerformanceReviewRepository.GetIdFromGuidAsync(employmentPerformanceReviews.ReviewedBy.Id);
            }

            if (!string.IsNullOrEmpty(employmentPerformanceReviews.Comment))
            {
                response.Comment = employmentPerformanceReviews.Comment;
            }

            return response;
        }

        /// <summary>
        /// Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague.
        /// </summary>
        private void CheckUserEmploymentPerformanceReviewsViewPermissions()
        {
            // access is ok if the current user has the view employment-performance-reviews permission
            if (!HasPermission(HumanResourcesPermissionCodes.ViewEmploymentPerformanceReview))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view employment-performance-reviews.");
                throw new PermissionsException("User is not authorized to view employment-performance-reviews.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague.
        /// </summary>
        private void CheckUserEmploymentPerformanceReviewsCreateUpdatePermissions()
        {
            // access is ok if the current user has the create/update employment-performance-reviews permission
            if (!HasPermission(HumanResourcesPermissionCodes.CreateUpdateEmploymentPerformanceReview))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create/update employment-performance-reviews.");
                throw new PermissionsException("User is not authorized to create/update employment-performance-reviews.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to delete a hold (a.k.a. a record from STUDENT.RESTRICTIONS) in Colleague.
        /// </summary>
        private void CheckUserEmploymentPerformanceReviewsDeletePermissions()
        {
            // access is ok if the current user has the delete employment-performance-reviews permission
            if (!HasPermission(HumanResourcesPermissionCodes.DeleteEmploymentPerformanceReview))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to delete employment-performance-reviews.");
                throw new PermissionsException("User is not authorized to delete employment-performance-reviews.");
            }
        }

    }

}