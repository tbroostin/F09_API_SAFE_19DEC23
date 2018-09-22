//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class MealTypesService : StudentCoordinationService, IMealTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public MealTypesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
			IStudentRepository studentRepository,
			IStaffRepository staffRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository, staffRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all meal-types
        /// </summary>
        /// <returns>Collection of MealTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MealTypes>> GetMealTypesAsync(bool bypassCache = false)
        {
            var mealTypesCollection = new List<Ellucian.Colleague.Dtos.MealTypes>();

            var mealTypesEntities = await _referenceDataRepository.GetMealTypesAsync(bypassCache);
            if (mealTypesEntities != null && mealTypesEntities.Any())
            {
                foreach (var mealTypes in mealTypesEntities)
                {
                    mealTypesCollection.Add(ConvertMealTypesEntityToDto(mealTypes));
                }
            }
            return mealTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a MealTypes from its GUID
        /// </summary>
        /// <returns>MealTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.MealTypes> GetMealTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertMealTypesEntityToDto((await _referenceDataRepository.GetMealTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("meal-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("meal-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MealType domain entity to its corresponding MealTypes DTO
        /// </summary>
        /// <param name="source">MealType domain entity</param>
        /// <returns>MealTypes DTO</returns>
        private Ellucian.Colleague.Dtos.MealTypes ConvertMealTypesEntityToDto(MealType source)
        {
            var mealTypes = new Ellucian.Colleague.Dtos.MealTypes();

            mealTypes.Id = source.Guid;
            mealTypes.Code = source.Code;
            mealTypes.Title = source.Description;
            mealTypes.Description = null;           
                                                                        
            return mealTypes;
        }

      
    }
 
}