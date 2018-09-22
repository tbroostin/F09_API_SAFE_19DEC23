// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
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
    /// Convert current AcademicTerm entity to legacy (version 1) AcademicTerm dto
    /// </summary>
    public class AcademicTermEntityAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm>
    {
        /// <summary>
        /// Constructor for AcademicTerm entity adapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public AcademicTermEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Map AcademicTerm entity to legacy AcademicTerm (version 1) Dto. This custom mapping needed due to custom
        /// mapping needed to handle mapping DateTimeOffset properties back to legacy DateTime definition.
        /// </summary>
        /// <param name="Source">AcademicTerm entity</param>
        /// <returns><see cref="AcademicTerm">AcademicTerm</see> Dto</returns>
        public override Dtos.Student.AcademicTerm MapToType(Domain.Student.Entities.AcademicTerm Source)
        {
            var academicTermDto = new Dtos.Student.AcademicTerm();

            var academicCreditAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>();
            foreach (var academicCredit in Source.AcademicCredits)
            {
                academicTermDto.AcademicCredits.Add(academicCreditAdapter.MapToType(academicCredit));
            }
            academicTermDto.Credits = Source.Credits;
            academicTermDto.ContinuingEducationUnits = Source.ContinuingEducationUnits;
            academicTermDto.GradePointAverage = Source.GradePointAverage.HasValue ? Source.GradePointAverage.Value : 0;
            academicTermDto.TermId = Source.TermId;

            return academicTermDto;
        }
    }
}