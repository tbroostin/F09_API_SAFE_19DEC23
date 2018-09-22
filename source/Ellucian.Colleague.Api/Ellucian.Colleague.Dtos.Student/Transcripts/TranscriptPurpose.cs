using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Transcript purpose given for a transcript request as defined in the PESC XML transcript request standard 
    /// </summary>
    public enum TranscriptPurpose
    {
        /// <summary>
        /// Admission
        /// </summary>
        Admission,
        /// <summary>
        /// Admission Registrar
        /// </summary>
        AdmissionRegistrar,
        /// <summary>
        /// Admission Service
        /// </summary>
        AdmissionService,
        /// <summary>
        /// Certification License
        /// </summary>
        CertificationLicensure,
        /// <summary>
        /// Employment
        /// </summary>
        Employment,
        /// <summary>
        /// Graduate Admissions
        /// </summary>
        GraduateAdmissions,
        /// <summary>
        /// Law School Admissions
        /// </summary>
        LawSchoolAdmissions,
        /// <summary>
        /// Medical School Admissions
        /// </summary>
        MedicalSchoolAdmissions,
        /// <summary>
        /// Other
        /// </summary>
        Other,
        /// <summary>
        /// Registrar
        /// </summary>
        Registrar,
        /// <summary>
        /// Scholarship
        /// </summary>
        Scholarship,
        /// <summary>
        /// Scholarship Grant Fellowship
        /// </summary>
        ScholarshipGrantFellowship,
        /// <summary>
        /// Self
        /// </summary>
        Self,
        /// <summary>
        /// Self Managed Package
        /// </summary>
        SelfManagedPackage,
        /// <summary>
        /// Transfer
        /// </summary>
        Transfer,
        /// <summary>
        /// Undergraduate Admissions
        /// </summary>
        UndergraduateAdmissions
    }
}
