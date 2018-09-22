// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public enum TranscriptPurpose
    {
        Admission,
        AdmissionRegistrar,
        AdmissionService,
        CertificationLicensure,
        Employment,
        GraduateAdmissions,
        LawSchoolAdmissions,
        MedicalSchoolAdmissions,
        Other,
        Registrar,
        Scholarship,
        ScholarshipGrantFellowship,
        Self,
        SelfManagedPackage,
        Transfer,
        UndergraduateAdmissions
    }
}
