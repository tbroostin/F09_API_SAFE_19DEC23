//Copyright 2014-2015 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// StudentAwardYear DTO to entity adapter class
    /// </summary>
    public class StudentAwardYearDtoToEntityAdapter : BaseAdapter<Dtos.FinancialAid.StudentAwardYear, Domain.FinancialAid.Entities.StudentAwardYear>
    {
        /// <summary>
        /// Constructor for the StudentAwardYearDtoToEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public StudentAwardYearDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            
        }

        /// <summary>
        /// Map an StudentAwardYear DTO to a StudentAwardYear entity
        /// </summary>
        /// <param name="studentAwardYearDto">studentAwardYear dto</param>
        /// <returns>student award year entity</returns>
        public override Domain.FinancialAid.Entities.StudentAwardYear MapToType(Dtos.FinancialAid.StudentAwardYear studentAwardYearDto)
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
