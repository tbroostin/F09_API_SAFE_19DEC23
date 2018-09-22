//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentResidentialCategoriesService : StudentCoordinationService, IStudentResidentialCategoriesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentResidentialCategoriesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IStudentRepository studentRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-residential-categories
        /// </summary>
        /// <returns>Collection of StudentResidentialCategories DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentResidentialCategories>> GetStudentResidentialCategoriesAsync(bool bypassCache = false)
        {
            var studentResidentialCategoriesCollection = new List<Ellucian.Colleague.Dtos.StudentResidentialCategories>();

            var studentResidentialCategoriesEntities = await _referenceDataRepository.GetStudentResidentialCategoriesAsync(bypassCache);
            if (studentResidentialCategoriesEntities != null && studentResidentialCategoriesEntities.Any())
            {
                foreach (var studentResidentialCategories in studentResidentialCategoriesEntities)
                {
                    studentResidentialCategoriesCollection.Add(ConvertStudentResidentialCategoriesEntityToDto(studentResidentialCategories));
                }
            }
            return studentResidentialCategoriesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentResidentialCategories from its GUID
        /// </summary>
        /// <returns>StudentResidentialCategories DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentResidentialCategories> GetStudentResidentialCategoriesByGuidAsync(string guid)
        {
            try
            {
                return ConvertStudentResidentialCategoriesEntityToDto((await _referenceDataRepository.GetStudentResidentialCategoriesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-residential-categories not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-residential-categories not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentResidentialCategories domain entity to its corresponding StudentResidentialCategories DTO
        /// </summary>
        /// <param name="source">StudentResidentialCategories domain entity</param>
        /// <returns>StudentResidentialCategories DTO</returns>
        private Ellucian.Colleague.Dtos.StudentResidentialCategories ConvertStudentResidentialCategoriesEntityToDto(Domain.Student.Entities.StudentResidentialCategories source)
        {
            var studentResidentialCategories = new Ellucian.Colleague.Dtos.StudentResidentialCategories();

            studentResidentialCategories.Id = source.Guid;
            studentResidentialCategories.Code = source.Code;
            studentResidentialCategories.Title = source.Description;
            studentResidentialCategories.Description = null;           
                                                                        
            return studentResidentialCategories;
        }

      
    }
   }
