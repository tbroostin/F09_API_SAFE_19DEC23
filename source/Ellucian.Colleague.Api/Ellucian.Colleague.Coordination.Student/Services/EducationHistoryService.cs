// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using StudentClassification = Ellucian.Colleague.Domain.Student.Entities.StudentClassification;
using StudentCohort = Ellucian.Colleague.Domain.Student.Entities.StudentCohort;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class EducationHistoryService : BaseCoordinationService, IEducationHistoryService
    {
        private readonly IEducationHistoryRepository _educationHistoryRepository;
        private ILogger _logger;

        public EducationHistoryService(IAdapterRegistry adapterRegistry,  ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IEducationHistoryRepository educationHistoryRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _educationHistoryRepository = educationHistoryRepository;
            _logger = logger;
        }
        /// <summary>
        /// This returns Education History for given studentIds.
        /// </summary>
        /// <param name="studentIds">List of studentIds</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.EducationHistory"></see> List of Education History</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.EducationHistory>> QueryEducationHistoryByIdsAsync(IEnumerable<string> studentIds)
        {

            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                throw new PermissionsException("User does not have permissions to query education history of students");
            }
            var educationHistoryDtoCollection = new List<Ellucian.Colleague.Dtos.Student.EducationHistory>();
            var educationHistoryCollection = await _educationHistoryRepository.GetAsync(studentIds);
            // Get the right adapter for the type mapping
            var educationHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.EducationHistory, Ellucian.Colleague.Dtos.Student.EducationHistory>();
            // Map the Address entity to the Address DTO
            foreach (var address in educationHistoryCollection)
            {
                educationHistoryDtoCollection.Add(educationHistoryDtoAdapter.MapToType(address));
            }
            return educationHistoryDtoCollection;
        }

    }
}
