
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
using Ellucian.Colleague.Coordination.Base.Services;


namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AttendanceCategoriesService : BaseCoordinationService, IAttendanceCategoriesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public AttendanceCategoriesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all attendance-categories
        /// </summary>
        /// <returns>Collection of AttendanceCategories DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AttendanceCategories>> GetAttendanceCategoriesAsync(bool bypassCache = false)
        {
            var attendanceTypesCollection = new List<AttendanceTypes>();
            var attendanceCategoriesCollection = new List<AttendanceCategories>();

            var attendanceCategoriesEntities = await _referenceDataRepository.GetAttendanceTypesAsync(bypassCache);
            if (attendanceCategoriesEntities != null && attendanceCategoriesEntities.Any())
            {
                foreach (var attendanceCategories in attendanceCategoriesEntities)
                {
                    attendanceCategoriesCollection.Add(ConvertAttendanceTypesEntityToDto(attendanceCategories));
                }
            }
            return attendanceCategoriesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AttendanceCategories from its GUID
        /// </summary>
        /// <returns>AttendanceCategories DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AttendanceCategories> GetAttendanceCategoriesByGuidAsync(string guid)
        {
            try
            {
                return ConvertAttendanceTypesEntityToDto((await _referenceDataRepository.GetAttendanceTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("attendance-categories not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("attendance-categories not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AttendanceCategories domain entity to its corresponding AttendanceCategories DTO
        /// </summary>
        /// <param name="source">AttendanceCategories domain entity</param>
        /// <returns>AttendanceCategories DTO</returns>
        private Ellucian.Colleague.Dtos.AttendanceCategories ConvertAttendanceTypesEntityToDto(AttendanceTypes source)
        {
            var attendanceCategories = new Ellucian.Colleague.Dtos.AttendanceCategories();

            attendanceCategories.Id = source.Guid;
            attendanceCategories.Code = source.Code;
            attendanceCategories.Title = source.Description;
            attendanceCategories.Description = null;

            return attendanceCategories;
        }


    }
}
