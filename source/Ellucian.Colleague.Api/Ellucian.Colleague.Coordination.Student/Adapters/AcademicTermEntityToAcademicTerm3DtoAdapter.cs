// Copyright 2015 Ellucian Company L.P. and its affiliates.
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
    /// Convert current AcademicTerm entity to AcademicTerm3 DTO
    /// </summary>
    public class AcademicTermEntityToAcademicTerm3DtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>
    {
        /// <summary>
        /// Constructor for AcademicTerm entity adapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public AcademicTermEntityToAcademicTerm3DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Map AcademicTerm entity to AcademicTerm3 Dto.
        /// </summary>
        /// <param name="Source">AcademicTerm entity</param>
        /// <returns><see cref="AcademicTerm">AcademicTerm3</see> Dto</returns>
        public override Dtos.Student.AcademicTerm3 MapToType(Domain.Student.Entities.AcademicTerm Source)
        {
            var academicTermDto = new Dtos.Student.AcademicTerm3();

            var academicCreditAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>();
            foreach (var academicCredit in Source.AcademicCredits)
            {
                academicTermDto.AcademicCredits.Add(academicCreditAdapter.MapToType(academicCredit));
            }
            academicTermDto.Credits = Source.Credits;
            academicTermDto.ContinuingEducationUnits = Source.ContinuingEducationUnits;
            academicTermDto.GradePointAverage = Source.GradePointAverage;
            academicTermDto.TermId = Source.TermId;

            return academicTermDto;
        }
    }
}