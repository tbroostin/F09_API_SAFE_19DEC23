using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    [RegisterType]
    public class UpdateStudentRestrictionService : BaseCoordinationService, IUpdateStudentRestrictionService
    {
        private readonly IUpdateStudentRestrictionRepository _UpdateStudentRestrictionRepository;

        public UpdateStudentRestrictionService(IAdapterRegistry adapterRegistry, IUpdateStudentRestrictionRepository UpdateStudentRestrictionRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._UpdateStudentRestrictionRepository = UpdateStudentRestrictionRepository;
        }

        public async Task<UpdateStudentRestrictionResponseDto> GetStudentRestrictionAsync(string personId)
        {
            var profile = await _UpdateStudentRestrictionRepository.GetStudentRestrictionAsync(personId);
            var dto = this.ConvertToDTO(profile);

            return dto;
        }

        public async Task<UpdateStudentRestrictionResponseDto> UpdateStudentRestrictionAsync(Ellucian.Colleague.Dtos.F09.UpdateStudentRestrictionRequestDto request)
        {
            Ellucian.Colleague.Domain.F09.Entities.UpdateStudentRestrictionRequest studentRequest = new Domain.F09.Entities.UpdateStudentRestrictionRequest();
            studentRequest.Id = request.Id;
            studentRequest.Restriction = request.Restriction;
            studentRequest.Action = request.Action;
            studentRequest.StartDate = request.StartDate;
            studentRequest.EndDate = request.EndDate;
            studentRequest.Comments = request.Comments;
            studentRequest.Options = request.Options;

            var student = await _UpdateStudentRestrictionRepository.UpdateStudentRestrictionAsync(studentRequest);
            var dto = this.ConvertToDTO(student);

            return dto;
        }

        private UpdateStudentRestrictionResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.UpdateStudentRestrictionResponse student)
        {
            var dto = new UpdateStudentRestrictionResponseDto
            (
                student.Error,
                student.Message
            );

            return dto;
        }
    }
}
