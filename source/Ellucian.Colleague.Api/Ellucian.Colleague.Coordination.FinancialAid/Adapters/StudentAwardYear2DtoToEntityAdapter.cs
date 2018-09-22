//Copyright 2015-2018 Ellucian Company L.P. and its affiliates
using System;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// StudentAwardYear2 DTO to entity adapter class
    /// </summary>
    public class StudentAwardYear2DtoToEntityAdapter : BaseAdapter<Dtos.Student.StudentAwardYear2, Domain.FinancialAid.Entities.StudentAwardYear>
    {
        /// <summary>
        /// Constructor for the StudentAwardYearDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public StudentAwardYear2DtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Map an StudentAwardYear2 DTO to a StudentAwardYear2 entity
        /// </summary>
        /// <param name="studentAwardYearDto">studentAwardYear dto</param>
        /// <returns>student award year entity</returns>
        public override Domain.FinancialAid.Entities.StudentAwardYear MapToType(Dtos.Student.StudentAwardYear2 studentAwardYearDto)
        {
            if (studentAwardYearDto == null)
            {
                throw new ArgumentNullException("studentAwardYearDto");
            }

            //Create studentAwardYear entity
            var studentAwardYearEntity = new Domain.FinancialAid.Entities.StudentAwardYear(studentAwardYearDto.StudentId, studentAwardYearDto.Code);
            //We need the IsPaperCopyOptionSelected flag only
            studentAwardYearEntity.IsPaperCopyOptionSelected = studentAwardYearDto.IsPaperCopyOptionSelected;

            return studentAwardYearEntity;
        }
    }
}
