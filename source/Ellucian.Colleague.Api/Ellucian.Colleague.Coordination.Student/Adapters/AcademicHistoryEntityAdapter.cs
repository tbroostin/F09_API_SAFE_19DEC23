// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
    /// Convert current AcademicHistory entity to legacy (version 1) AcademicHistory dto
    /// </summary>
    public class AcademicHistoryEntityAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory>
    {
        public AcademicHistoryEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Dtos.Student.AcademicHistory MapToType(Domain.Student.Entities.AcademicHistory Source)
        {
            var academicHistoryDto = new Ellucian.Colleague.Dtos.Student.AcademicHistory();
            var academicTermAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm>();
            academicHistoryDto.AcademicTerms = new List<Ellucian.Colleague.Dtos.Student.AcademicTerm>();
            foreach (var academicTerm in Source.AcademicTerms)
            {
                academicHistoryDto.AcademicTerms.Add(academicTermAdapter.MapToType(academicTerm));
            }
            var academicCreditAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>();
            academicHistoryDto.NonTermAcademicCredits = new List<Ellucian.Colleague.Dtos.Student.AcademicCredit>();
            foreach (var academicCredit in Source.NonTermAcademicCredits)
            {
                academicHistoryDto.NonTermAcademicCredits.Add(academicCreditAdapter.MapToType(academicCredit));
            }
            var gradeRestrictionAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>();
            academicHistoryDto.GradeRestriction = gradeRestrictionAdapter.MapToType(Source.GradeRestriction);

            return academicHistoryDto;
        }
    }
}