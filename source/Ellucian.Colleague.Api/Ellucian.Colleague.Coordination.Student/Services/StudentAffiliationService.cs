// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentAffiliationService : StudentCoordinationService, IStudentAffiliationService
    {
        private readonly IStudentAffiliationRepository _studentAffiliationRepository;
        private ILogger _logger;
        private ITermRepository _termRepository;
        private IEnumerable<Term> termList;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentAffiliationService(IAdapterRegistry adapterRegistry, IStudentAffiliationRepository studentAffiliationRepository, ITermRepository termRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentrepository, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentrepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _studentAffiliationRepository = studentAffiliationRepository;
            _logger = logger;
            _termRepository = termRepository;
        }

        public async Task<IEnumerable<Dtos.Student.StudentAffiliation>> QueryStudentAffiliationsAsync(Dtos.Student.StudentAffiliationQueryCriteria criteria)
        {
            var studentIds = criteria.StudentIds;
            var termId = criteria.Term;
            var affiliationId = criteria.AffiliationCode;

            ICollection<Dtos.Student.StudentAffiliation> studentAffiliationsDto = new List<Dtos.Student.StudentAffiliation>();          

            if (termId == null)
            {
                throw new ArgumentNullException("termId");
            }
            Ellucian.Colleague.Domain.Student.Entities.Term termData = _termRepository.Get(termId);
     
            if (HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                IEnumerable<Domain.Student.Entities.StudentAffiliation> studentAffiliations = await _studentAffiliationRepository.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, affiliationId);
                foreach (var studentAffiliation in studentAffiliations)
                {
                    studentAffiliation.Term = FindTermForAffiliation(studentAffiliation.StartDate, studentAffiliation.EndDate, termId);
                    if ((!string.IsNullOrEmpty(termId) && studentAffiliation.Term == termId) || string.IsNullOrEmpty(termId))
                    {
                        // Get the right adapter for the type mapping
                        var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<StudentAffiliation, Ellucian.Colleague.Dtos.Student.StudentAffiliation>();

                        // Map the StudentAffiliation entity to the StudentAffiliation DTO
                        var stuAffiliationDto = studentProgramDtoAdapter.MapToType(studentAffiliation);

                        studentAffiliationsDto.Add(stuAffiliationDto);
                    }
                }
            }
            else
            {
                // Person doesn't have View Student Information Permissions. Throw Permission exception.
                throw new PermissionsException("User does not have View Student Information Permissions.");
            }
            return studentAffiliationsDto;
        }
        private string FindTermForAffiliation(DateTime? startDate, DateTime? endDate, string checkTerm)
        {
            string term = "";
            if (startDate.HasValue)
            {
                // fetch this once, and only once needed
                if (termList == null) termList = _termRepository.Get();
                if (termList != null && termList.Count() > 0)
                {
                    var testTerms = termList.Where(t => ((t.StartDate.CompareTo(startDate.Value) <= 0 && t.EndDate.CompareTo(startDate.Value) >= 0) ||
                            (t.StartDate.CompareTo(startDate.Value) >= 0 && (endDate.HasValue && t.StartDate.CompareTo(endDate.Value) <= 0)) ||
                            (t.StartDate.CompareTo(startDate.Value) >= 0 && !endDate.HasValue)));
                    if (testTerms != null && testTerms.Count() > 0)
                    {
                        foreach (var singleTerm in testTerms)
                        {
                            if (singleTerm.Code == checkTerm)
                            {
                                term = singleTerm.Code;
                            }
                        }
                        if (string.IsNullOrEmpty(term))
                        {
                            term = testTerms.First().Code;
                        }
                    }
                }
            }
            return term;
        }
    }
}
