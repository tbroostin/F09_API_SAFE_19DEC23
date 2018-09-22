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
    /// Convert PilotAcademicHistoryLevel entity to PilotStudentTermLevelGpa DTO
    /// </summary>
    public class PilotAcademicHistoryLevelToPilotStudentTermLevelGpaDtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.PilotAcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.PilotStudentTermLevelGpa>
    {
        public PilotAcademicHistoryLevelToPilotStudentTermLevelGpaDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Dtos.Student.PilotStudentTermLevelGpa MapToType(Domain.Student.Entities.PilotAcademicHistoryLevel source)
        {
            var studentTermLevelGpaDto = new Ellucian.Colleague.Dtos.Student.PilotStudentTermLevelGpa();

            studentTermLevelGpaDto.StudentId = source.StudentId;
            studentTermLevelGpaDto.AcademicLevelCode = source.AcademicLevelCode;          
            var academicTerm = source.AcademicLevelHistory.AcademicTerms.FirstOrDefault(); // our academic level history in this context only has one term worth of credits
            studentTermLevelGpaDto.TermGpa = academicTerm != null ? academicTerm.GradePointAverage : source.AcademicLevelHistory.OverallGradePointAverage;
            // academicTerm GPA is the GPA for the term; AcademicLevelHistory GPA is the GPA for the term, excluding replaced credits.
            
            return studentTermLevelGpaDto;
        }
    }
}