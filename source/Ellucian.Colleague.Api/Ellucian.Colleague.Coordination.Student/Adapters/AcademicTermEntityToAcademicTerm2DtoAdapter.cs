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
    public class AcademicTermEntityToAcademicTerm2DtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm2>
    {
        /// <summary>
        /// Constructor for AcademicTerm entity adapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public AcademicTermEntityToAcademicTerm2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Map AcademicTerm entity to legacy AcademicTerm2 Dto. This custom mapping needed to preserve backwards compatibility 
        /// after changes to make GPA fields nullable in the entity/repository.
        /// </summary>
        /// <param name="Source">AcademicTerm entity</param>
        /// <returns><see cref="AcademicTerm">AcademicTerm2</see> Dto</returns>
        public override Dtos.Student.AcademicTerm2 MapToType(Domain.Student.Entities.AcademicTerm Source)
        {
            var academicTermDto = new Dtos.Student.AcademicTerm2();

            var academicCreditAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>();
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