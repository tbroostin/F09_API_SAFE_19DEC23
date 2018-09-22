// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert PilotAcademicHistory entity to Pilot specific Academic History Levels DTO
    /// </summary>
    public class PilotAcademicCreditToPilotStudentSectionDtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.PilotAcademicCredit, Ellucian.Colleague.Dtos.Student.PilotStudentSection>
    {
        public PilotAcademicCreditToPilotStudentSectionDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Dtos.Student.PilotStudentSection MapToType(Domain.Student.Entities.PilotAcademicCredit source)
        {
            var studentSectionDto = new Ellucian.Colleague.Dtos.Student.PilotStudentSection();

            studentSectionDto.AcademicLevel = source.AcademicLevelCode;
            studentSectionDto.Campus = source.Location;
            studentSectionDto.Credits = source.Credit;
            studentSectionDto.EndDate = source.EndDate;
            studentSectionDto.Name = source.CourseName;
            studentSectionDto.PassFail = (source.GradingType == Domain.Student.Entities.GradingType.PassFail);
            studentSectionDto.Section = source.SectionId;
            studentSectionDto.StartDate = source.StartDate;
            studentSectionDto.Status = ConvertCreditStatus(source.Status, source.HasVerifiedGrade);
            studentSectionDto.StatusDate = source.StatusDate;
            studentSectionDto.Student = source.StudentId;
            studentSectionDto.Term = source.TermCode;

            return studentSectionDto;
        }

        private string ConvertCreditStatus(Ellucian.Colleague.Domain.Student.Entities.CreditStatus status, bool hasVerified)
        {
            string result = "Inactive";
            if (status != null)
            {
                switch (status)
                {
                    case Ellucian.Colleague.Domain.Student.Entities.CreditStatus.New: result = hasVerified ? "Completed" : "Active"; break;
                    case Ellucian.Colleague.Domain.Student.Entities.CreditStatus.Add: result = hasVerified ? "Completed" : "Active"; break;
                    case Ellucian.Colleague.Domain.Student.Entities.CreditStatus.Dropped: result = "Dropped"; break;
                    case Ellucian.Colleague.Domain.Student.Entities.CreditStatus.Withdrawn: result = "Withdrawn"; break;
                    default: result = "Inactive"; break;
                }
            }
            return result;
        }
    }
}