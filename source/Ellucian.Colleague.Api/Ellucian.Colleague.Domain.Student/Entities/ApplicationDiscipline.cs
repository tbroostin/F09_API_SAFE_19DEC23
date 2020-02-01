//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ApplicationDiscipline
    {
        public string RecordKey { get; set; }
        public string RecordGuid { get; set; }
        public Domain.Base.Entities.AcademicDisciplineType DisciplineType { get; set; }
        public string Code { get; set; }
        public string AdministeringInstitutionUnit { get; set; }
        public DateTime? StartOn { get; set; }

    }
}