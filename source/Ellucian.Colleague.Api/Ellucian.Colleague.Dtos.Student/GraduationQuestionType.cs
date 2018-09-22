// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
     /// <summary>
     /// Defines the various types of optional Graduation Questions
     /// </summary>
     [JsonConverter(typeof(StringEnumConverter))]
     public enum GraduationQuestionType
     {
          /// <summary>
          /// Optional question for name to print on the diploma
          /// </summary>
          DiplomaName,
          /// <summary>
          /// Optional question for phonetic spelling of the student's name
          /// </summary>
          PhoneticSpelling,
          /// <summary>
          /// Optional question for student to indicate whether they are planning to attend commencement
          /// </summary>
          AttendCommencement,
          /// <summary>
          /// Optional question for the number of guests the student is bringing to commencement
          /// </summary>
          NumberGuests,
          /// <summary>
          /// Optional question for the student to indicate if they want their name included in the graduation program
          /// </summary>
          NameInProgram,
          /// <summary>
          /// Optional question for student's hometown to show in the graduation program
          /// </summary>
          Hometown,
          /// <summary>
          /// Optional question for cap size (in the event that the institution is not redirecting to another website or not doing caps and gowns.
          /// </summary>
          CapSize,
          /// <summary>
          /// Optional question for gown size (in the event that the institution is not redirecting to another website or not doing caps and gowns.
          /// </summary>
          GownSize,
          /// <summary>
          /// Optional question for which commencement location the student plans to attend
          /// </summary>
          CommencementLocation,
          /// <summary>
          /// Optional question to determine if student is going to pick up their diploma if not coming to commencement.
          /// </summary>
          PickUpDiploma,
          /// <summary>
          /// Optional question to determine if the student has served in the military
          /// </summary>
          MilitaryStatus,
          /// <summary>
          /// Optional question to determine if the student will require special accommodations at commencement
          /// </summary>
          SpecialAccommodations,
          /// <summary>
          /// Optional question to determine the location that the student considers his primary location
          /// </summary>
          PrimaryLocation,
          /// <summary>
          /// Optional question to determine if the student would like to request an address change
          /// </summary>
          RequestAddressChange

     }
}
