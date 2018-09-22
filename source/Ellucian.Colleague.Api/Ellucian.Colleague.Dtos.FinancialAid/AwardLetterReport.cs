/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Award Letter Report class containing the byte array for the PDF representation 
    /// of award letter and the file name
    /// </summary>
    public class AwardLetterReport
    {
        /// <summary>
        /// Award letter report name
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Award letter report content
        /// </summary>
        public byte[] FileContent { get; set; }
    }
}
