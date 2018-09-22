// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{

    public class StudentPetitionDtoToEntityAdapter : BaseAdapter<Dtos.Student.StudentPetition, Domain.Student.Entities.StudentPetition>
    {
        public StudentPetitionDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Domain.Student.Entities.StudentPetition MapToType(Dtos.Student.StudentPetition source)
        {
            StudentPetitionType petitionType = StudentPetitionType.StudentPetition;
            switch (source.Type)
            {
                case Ellucian.Colleague.Dtos.Student.StudentPetitionType.StudentPetition:
                    petitionType = StudentPetitionType.StudentPetition;
                    break;
                case Ellucian.Colleague.Dtos.Student.StudentPetitionType.FacultyConsent:
                    petitionType = StudentPetitionType.FacultyConsent;
                    break;
                default:
                    break;
            }
            var petitionEntity = new Domain.Student.Entities.StudentPetition(source.Id, null, source.SectionId, source.StudentId, petitionType, source.StatusCode);
            petitionEntity.Comment = source.Comment;
            petitionEntity.ReasonCode = source.ReasonCode;
            petitionEntity.DateTimeChanged = source.DateTimeChanged;
            petitionEntity.UpdatedBy = source.UpdatedBy;
            return petitionEntity;
        }
    }
}
