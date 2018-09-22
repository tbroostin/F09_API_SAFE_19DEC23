// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps a mid term grade entity to a mid term grade DTO
    /// </summary>
    public class MidTermGradeEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>
    {
        /// <summary>
        /// Constructor for MidTermGradeEntityAdapter - current MidTermGrade to legacy (version 1) dto
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public MidTermGradeEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Mapping for current MidTermGrade object to legacy (version 1) MidTermGrade dto
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public override Ellucian.Colleague.Dtos.Student.MidTermGrade MapToType(Ellucian.Colleague.Domain.Student.Entities.MidTermGrade Source)
        {
            var midTermGradeDto = new Ellucian.Colleague.Dtos.Student.MidTermGrade();
            
            midTermGradeDto.GradeId = Source.GradeId;
            midTermGradeDto.GradeTimestamp = Source.GradeTimestamp.HasValue ? Source.GradeTimestamp.Value.DateTime : (DateTime?)null;
            midTermGradeDto.Position = Source.Position;

            return midTermGradeDto;
        }
    }
}
