// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentAcademicCredentialProgramInfo
    {
        public string AcademicCredentialsId { get; set; }
        public string AcadPersonId { get; set; }
        public string AcademicAcadProgram { get; set; }
        public string StudentProgramGuid { get; set; }
        public DateTime? AcadDegreeDate { get; set; }
        public DateTime? AcadCcdDate { get; set; }        
    }
}