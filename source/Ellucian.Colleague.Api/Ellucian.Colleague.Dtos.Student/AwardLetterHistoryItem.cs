/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates*/
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// AwardLetterHistoryItem class that contains award letter 
    /// history item id and created date
    /// </summary>
    public class AwardLetterHistoryItem
    {
        /// <summary>
        /// Award letter history item id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Award letter history item created date
        /// </summary>
        public DateTime CreatedDate { get; set; }        
    }
}
