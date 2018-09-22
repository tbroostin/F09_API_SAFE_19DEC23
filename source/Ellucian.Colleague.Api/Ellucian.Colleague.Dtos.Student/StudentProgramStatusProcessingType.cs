﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
     /// <summary>
     /// Defines the various types of optional Graduation Questions
     /// </summary>
     [JsonConverter(typeof(StringEnumConverter))]
    public enum StudentProgramStatusProcessingType
     {
         /// <summary>
         /// Value is "0" to specify there is no processing thype exist
         /// </summary>
         None,
         /// <summary>
         /// special processing code of 1.
         /// The value with the special processing code  of 1 is the value that will be assigned to the student program record when the record is created for an application
         /// This indicatex that this is a preliminary student program.
         /// </summary>
         Potential,
         /// <summary>
         /// special processing code of 2. 
         /// The value with the special processing codeof 2 is the value that will be assigned to the student program record when an application is moved to STUDENTS.
         /// </summary>
         Active,
         /// <summary>
         /// special processing code of 3.  The value with the special processing code of 3 is the value that will be assigned to the student program record when the record is moved to the person's academic credentials (ACAD.CREDENTIALS file). 
         /// The special processing code of 3 is also used to designate completion.
         /// </summary>
         Graduated,
         /// <summary>
         /// The special processing code of 3 is also used to designate completion.
         /// </summary>
         InActive,
         /// <summary>
         ///  special processing code of 5.  This is the code that will be put on a student's active academic programs when a student withdraws from the institution
         /// </summary>
         Withdrawn
     }
}
