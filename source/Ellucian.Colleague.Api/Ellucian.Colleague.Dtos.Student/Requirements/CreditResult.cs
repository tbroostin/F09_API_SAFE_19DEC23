// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// <summary>
    /// holds result of an evaluation of a planned academic credit against a group
    /// </summary>    /// </summary>
    
    public class CreditResult:AcadResult
    {
        /// <summary>
        /// Id of the academic credit
        /// /// </summary>
        public string AcademicCreditId { get; set; }
       
    }
}
