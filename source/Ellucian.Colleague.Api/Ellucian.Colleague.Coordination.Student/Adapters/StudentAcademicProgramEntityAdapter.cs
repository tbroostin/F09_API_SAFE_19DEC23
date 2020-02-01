// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps a StudentAcademicProgram DTO to a StudentAcademicProgram domain entity.
    /// </summary>
    public class StudentAcademicProgramEntityAdapter : BaseAdapter<Dtos.Student.StudentAcademicProgram, StudentAcademicProgram>
    {
        /// <summary>
        /// Initializes a new instance of the StudentAcademicProgramEntityAdapter class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentAcademicProgramEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Custom mapping of StudentAcademicProgram entity to StudentAcademicProgram DTO.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns></returns>
        public override Domain.Student.Entities.StudentAcademicProgram MapToType(Dtos.Student.StudentAcademicProgram source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The source Academic program can not be null.");
            }
            StudentAcademicProgram prog = new StudentAcademicProgram(source.StudentId, source.AcadamicProgramId, source.CatalogYear, Convert.ToDateTime(source.StartDate))
            {
                DepartmentCode = source.Department,
                Location = source.Location
            };
            return prog;
        }
    }
}
