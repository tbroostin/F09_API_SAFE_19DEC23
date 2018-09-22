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
    public class PilotAcademicHistoryLevelDtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.PilotAcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.PilotAcademicHistoryLevel>
    {
        public PilotAcademicHistoryLevelDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Dtos.Student.PilotAcademicHistoryLevel MapToType(Domain.Student.Entities.PilotAcademicHistoryLevel Source)
        {
            var academicHistoryDto = new Ellucian.Colleague.Dtos.Student.PilotAcademicHistoryLevel();

            academicHistoryDto.StudentId = Source.StudentId;
            academicHistoryDto.AcademicLevelCode = Source.AcademicLevelCode;
            academicHistoryDto.TotalCreditsEarned = Source.AcademicLevelHistory.TotalCreditsCompleted;
            academicHistoryDto.CumulativeGradePointAverage = Source.AcademicLevelHistory.OverallGradePointAverage;
            academicHistoryDto.TransferGradePointAverage = Source.AcademicLevelHistory.OverallTransferGradePointAverage;
            academicHistoryDto.FirstTermEnrolled = Source.AcademicLevelHistory.FirstTermEnrolled;

            return academicHistoryDto;
        }
    }
}