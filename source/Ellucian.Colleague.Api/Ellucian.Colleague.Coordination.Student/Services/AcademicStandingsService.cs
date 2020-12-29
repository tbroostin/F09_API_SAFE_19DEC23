// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AcademicStandingsService : BaseCoordinationService, IAcademicStandingsService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger _logger;

        public AcademicStandingsService(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ICurrentUserFactory currentUserFactory, IConfigurationRepository configurationRepository,
                                         IRoleRepository roleRepository, ILogger logger) : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }
        #region IAcademicStandingsService Members

        /// <summary>
        /// Returns all academic standings
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicStanding>> GetAcademicStandingsAsync(bool bypassCache)
        {
            var academicStandingCollection = new List<Ellucian.Colleague.Dtos.AcademicStanding>();

            var academicStandings = await _studentReferenceDataRepository.GetAcademicStandings2Async(bypassCache);
            if (academicStandings != null && academicStandings.Any())
            {
                foreach (var academicStanding in academicStandings)
                {
                    academicStandingCollection.Add(ConvertAcademicStandingEntityToDto(academicStanding));
                }
            }
            return academicStandingCollection;
        }      

        /// <summary>
        /// Returns an academic standing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.AcademicStanding> GetAcademicStandingByIdAsync(string id)
        {
            var academicStandingEntity = (await _studentReferenceDataRepository.GetAcademicStandings2Async(true)).FirstOrDefault(ac => ac.Guid == id);
            if (academicStandingEntity == null)
            {
                throw new KeyNotFoundException("Academic Standing Code is not found.");
            }

            var academicStanding = ConvertAcademicStandingEntityToDto(academicStandingEntity);
            return academicStanding;
        }
        #endregion

        #region Convert method(s)

        /// <summary>
        /// Converts from AcademicStanding entity to AcademicStanding dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.AcademicStanding ConvertAcademicStandingEntityToDto(AcademicStanding2 source)
        {
            Dtos.AcademicStanding academicStanding = new Dtos.AcademicStanding();
            academicStanding.Id = source.Guid;
            academicStanding.Code = source.Code;
            academicStanding.Title = source.Description;
            academicStanding.Description = string.Empty;
            return academicStanding;
        }

        #endregion
    }
}
