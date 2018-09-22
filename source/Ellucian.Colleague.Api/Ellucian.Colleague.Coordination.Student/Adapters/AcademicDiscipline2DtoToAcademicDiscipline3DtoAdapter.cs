// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class AcademicDiscipline2DtoToAcademicDiscipline3DtoAdapter : AutoMapperAdapter<Dtos.AcademicDiscipline2, Dtos.AcademicDiscipline3>
    {
        public AcademicDiscipline2DtoToAcademicDiscipline3DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            //AddMappingDependency<Dtos.AcademicDisciplineType, Dtos.EnumProperties.AcademicDisciplineType2>();  
        }

        /// <summary>
        /// Custom mapping of AcademicDiscipline2 DTO to AcademicDiscipline3 DTO
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns><see cref="AcademicCredit2">AcademicCredit2</see> data transfer object</returns>
        public override Dtos.AcademicDiscipline3 MapToType(Dtos.AcademicDiscipline2 Source)
        {
            var academicDiscipline3Dto  = base.MapToType(Source);
            if (Source.Type == Dtos.AcademicDisciplineType.Major) academicDiscipline3Dto.Type = Dtos.EnumProperties.AcademicDisciplineType2.Major;
            if (Source.Type == Dtos.AcademicDisciplineType.Minor) academicDiscipline3Dto.Type = Dtos.EnumProperties.AcademicDisciplineType2.Minor;
            if (Source.Type == Dtos.AcademicDisciplineType.Concentration) academicDiscipline3Dto.Type = Dtos.EnumProperties.AcademicDisciplineType2.Concentration;
            if (Source.Reporting == null || Source.Reporting.Count == 0) academicDiscipline3Dto.Reporting = null;
            return academicDiscipline3Dto;
        }
    }
}
