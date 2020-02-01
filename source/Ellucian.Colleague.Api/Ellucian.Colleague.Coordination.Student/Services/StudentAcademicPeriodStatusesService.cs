//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAcademicPeriodStatusesService : BaseCoordinationService, IStudentAcademicPeriodStatusesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public StudentAcademicPeriodStatusesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-academic-period-statuses
        /// </summary>
        /// <returns>Collection of StudentAcademicPeriodStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses>> GetStudentAcademicPeriodStatusesAsync(bool bypassCache = false)
        {
            var studentAcademicPeriodStatusesCollection = new List<Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses>();

            var studentAcademicPeriodStatusesEntities = await _referenceDataRepository.GetStudentStatusesAsync(bypassCache);
            if (studentAcademicPeriodStatusesEntities != null && studentAcademicPeriodStatusesEntities.Any())
            {
                foreach (var studentAcademicPeriodStatuses in studentAcademicPeriodStatusesEntities)
                {
                    studentAcademicPeriodStatusesCollection.Add(ConvertStudentAcademicPeriodStatusesEntityToDto(studentAcademicPeriodStatuses));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return studentAcademicPeriodStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentAcademicPeriodStatuses from its GUID
        /// </summary>
        /// <returns>StudentAcademicPeriodStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses> GetStudentAcademicPeriodStatusesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertStudentAcademicPeriodStatusesEntityToDto((await _referenceDataRepository.GetStudentStatusesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No student-academic-period-statuses was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No student-academic-period-statuses was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a StudentStatuses domain entity to its corresponding StudentAcademicPeriodStatuses DTO
        /// </summary>
        /// <param name="source">StudentStatuses domain entity</param>
        /// <returns>StudentAcademicPeriodStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses ConvertStudentAcademicPeriodStatusesEntityToDto(Domain.Student.Entities.StudentStatus source)
        {
            var studentAcademicPeriodStatuses = new Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses();

            studentAcademicPeriodStatuses.Id = source.Guid;
            studentAcademicPeriodStatuses.Code = source.Code;
            studentAcademicPeriodStatuses.Title = source.Description;
            studentAcademicPeriodStatuses.Description = null;

            if (!string.IsNullOrEmpty(source.SpecialProcessing1))
            {
                var usages = new List<string>();
                switch (source.SpecialProcessing1)
                {
                    case "P":
                        usages.Add("Preregistered");
                        break;
                    case "R":
                        usages.Add("Registered");
                        break;
                    case "T":
                        usages.Add("Transcripted");
                        break;
                    case "W":
                        usages.Add("Withdrawn");
                        break;
                    case "X":
                        usages.Add("Deleted");
                        break;
                    default: break;
                }
                if (usages.Any())
                {
                    studentAcademicPeriodStatuses.Usages = usages;
                }
            }
            return studentAcademicPeriodStatuses;
        }
    }
}