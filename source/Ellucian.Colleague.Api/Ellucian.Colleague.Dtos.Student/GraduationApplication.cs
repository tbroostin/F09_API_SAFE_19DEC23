// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
     /// <summary>
     /// Dto for Graduation Application
     /// </summary>
     public class GraduationApplication
     {
          /// <summary>
          /// Graduation Application Id. 
          /// </summary>
          public string Id { get; set; }

          /// <summary>
          /// Student ID of person who has submitted the application
          /// </summary>
          public string StudentId { get; set; }

          /// <summary>
          /// Program for which the student is applying for graduation
          /// </summary>
          public string ProgramCode { get; set; }

          /// <summary>
          /// Graduation Term when the student is applying to graduate
          /// </summary>
          public string GraduationTerm { get; set; }

          /// <summary>
          /// Name the student wants to be printed on the diploma
          /// </summary>
          public string DiplomaName { get; set; }

          /// <summary>
          /// Indicates whether the student will pickup their diploma
          /// </summary>
          public bool? WillPickupDiploma { get; set; }

          /// <summary>
          /// Address where the diploma should be mailed.
          /// </summary>
          public List<string> MailDiplomaToAddressLines { get; set; }

          /// <summary>
          /// City where diploma should be mailed.
          /// </summary>
          public string MailDiplomaToCity { get; set; }

          /// <summary>
          /// State where diploma should be mailed.
          /// </summary>
          public string MailDiplomaToState { get; set; }

          /// <summary>
          /// Postal Code where diploma should be mailed.
          /// </summary>
          public string MailDiplomaToPostalCode { get; set; }

          /// <summary>
          /// Country where diploma should be mailed.
          /// </summary>
          public string MailDiplomaToCountry { get; set; }

          /// <summary>
          /// Phonetic Spelling of student's name for Commencement Ceremony
          /// </summary>
          public string PhoneticSpellingOfName { get; set; }

          /// <summary>
          /// Indicates whether the student will be attending commencement
          /// </summary>
          public bool? AttendingCommencement { get; set; }

          /// <summary>
          /// Indicates whether the student wishes their name to be in the printed program
          /// </summary>
          public bool? IncludeNameInProgram { get; set; }

          /// <summary>
          /// Location where student will attend commencement
          /// </summary>
          public string CommencementLocation { get; set; }

          /// <summary>
          /// Date student has chosen to attend commencement.
          /// </summary>
          public DateTimeOffset? CommencementDate { get; set; }

          /// <summary>
          /// Date student graduation application was added.
          /// </summary>
          public DateTimeOffset? SubmittedDate { get; set; }

          /// <summary>
          /// Number of guests the student wishes to invite to commencement
          /// </summary>
          public int NumberOfGuests { get; set; }

          /// <summary>
          /// Hometown the student wishes to be associated with 
          /// </summary>
          public string Hometown { get; set; }

          /// <summary>
          /// Cap Size
          /// </summary>
          public string CapSize { get; set; }

          /// <summary>
          /// Gown Size
          /// </summary>
          public string GownSize { get; set; }

          /// <summary>
          /// Veteran status of the applicant
          /// </summary>
          public GraduateMilitaryStatus? MilitaryStatus { get; set; }

          /// <summary>
          /// The specified accommodations that the student will need at commencement
          /// </summary>
          public string SpecialAccommodations { get; set; }

          /// <summary>
          /// If an invoice was generated as a result of this application this is the invoice number.
          /// </summary>
          public string InvoiceNumber { get; set; }

         /// <summary>
         /// Primary location of a student for a given program.
         /// </summary>
          public string PrimaryLocation { get; set; }
         
         /// <summary>
         ///  Has the Academic Credintials Record been updated in Colleague (from the Graduates)?
         /// </summary>
          public bool AcadCredentialsUpdated { get; set; }

         /// <summary>
         /// Is MailToDiploma address in graduation application same as student preferred address?
         /// </summary>
          public bool IsDiplomaAddressSameAsPreferred { get; set; }

          /// <summary>
          /// Default Constructor.
          /// </summary>
          public GraduationApplication()
          { }

          /// <summary>
          /// Parameter constructor.
          /// </summary>
          /// <param name="studentId">Student Id</param>
          /// <param name="programCode">program Code</param>
          public GraduationApplication(string studentId, string programCode)
          {
               this.StudentId = studentId;
               this.ProgramCode = programCode;
               IsDiplomaAddressSameAsPreferred = false;
          }
     }
}
