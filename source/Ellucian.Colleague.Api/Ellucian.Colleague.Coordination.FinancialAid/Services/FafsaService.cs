/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Service for Financial Aid FAFSA
    /// </summary>
    [RegisterType]
    public class FafsaService : AwardYearCoordinationService, IFafsaService
    {
        private readonly IFafsaRepository fafsaRepository;
        private readonly ITermRepository termRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="fafsaRepository">FafsaRepository</param>
        /// <param name="officeRepository">OfficeRepository</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository</param>
        /// <param name="termRepository">TermRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public FafsaService(
            IAdapterRegistry adapterRegistry,
            IFafsaRepository fafsaRepository,
            ITermRepository termRepository,
            IFinancialAidOfficeRepository officeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, officeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.fafsaRepository = fafsaRepository;
            this.termRepository = termRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get Financial Aid FAFSA data for all years that the student has data.
        /// May optionally specify either an award year or a term but must have one or the other.
        /// </summary>
        /// <param name="criteria">Object containing student Ids, award year and term</param>
        /// <returns>Fafsa DTO objects</returns>
        public async Task<IEnumerable<Fafsa>> QueryFafsaAsync(FafsaQueryCriteria criteria)
        {
            IEnumerable<string> studentIds = criteria.StudentIds;
            string awardYear = criteria.AwardYear;
            string term = criteria.Term;

            if (studentIds == null)
            {
                throw new ArgumentNullException("studentIds");
            }
            if (string.IsNullOrEmpty(awardYear) && string.IsNullOrEmpty(term))
            {
                throw new ArgumentNullException("awardYear or term");
            }
            // Check Student View Permissions
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                var message = string.Format("{0} does not have permission to view Student Information", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            // If the term has multiple FA years defined, return all Fafsa Data for all FA Years.
            List<string> awardYears = new List<string>();
            if (string.IsNullOrEmpty(awardYear))
            {
                Ellucian.Colleague.Domain.Student.Entities.Term termData = termRepository.Get(term);

                var termFaYears = termData.FinancialAidYears;
                foreach (var faYear in termFaYears)
                {
                    awardYears.Add(faYear.ToString());
                }
                if (awardYears == null)
                {
                    awardYears.Add(termData.ReportingYear.ToString());
                }
                if (awardYears == null)
                {
                    throw new ArgumentException("Could not determine FA Year from Term");
                }
            }
            else
            {
                awardYears.Add(awardYear);
            }

            var fafsaDtoCollection = new List<Fafsa>();
            // Go through each academic year since we may have multiple coming
            // from a single term record.
            if (awardYears != null && awardYears.Count > 0)
            {
                foreach (var year in awardYears)
                {
                    var fafsaData = await fafsaRepository.GetFafsaByStudentIdsAsync(studentIds, year);
                    var fafsaDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.FinancialAid.Entities.Fafsa, Fafsa>();

                    //Map the fafsa entity to the fafsa DTO
                    foreach (var fafsaRecord in fafsaData)
                    {
                        fafsaDtoCollection.Add(fafsaDtoAdapter.MapToType(fafsaRecord));
                    }
                }
            }
            return fafsaDtoCollection;
        }

        /// <summary>
        /// Get a list of all FAFSAs that a student submitted and corrected for student award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get FAFSAs</param>
        /// <param name="getActiveYearsOnly"> flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of all FAFSAs from the given student id</returns>
        public async Task<IEnumerable<Fafsa>> GetStudentFafsasAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access fafsa information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has no award years", studentId));
                return new List<Fafsa>();
            }

            var fafsaEntities = await fafsaRepository.GetFafsasAsync(new List<string>() { studentId }, studentAwardYears.Select(y => y.Code));
            if (fafsaEntities == null || fafsaEntities.Count() == 0)
            {
                logger.Info(string.Format("Student {0} has no fafsas", studentId));
                return new List<Fafsa>();
            }

            var fafsaDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.Fafsa, Fafsa>();

            return fafsaEntities.Select(fafsaEntity => fafsaDtoAdapter.MapToType(fafsaEntity));
        }
    }
}
