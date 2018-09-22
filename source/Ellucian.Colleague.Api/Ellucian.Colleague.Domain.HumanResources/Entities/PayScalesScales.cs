//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// The pay structure based on grades with step levels. 
    /// </summary>
    [Serializable]
    public class PayScalesScales
    {        


        /// <summary>
       /// The grouping of salary or wage steps within a pay structure.
       /// </summary>
       public string Grade { get; set; }
     
        /// <summary>
       /// The different level of compensation for a position within the salary grade.
       /// </summary>
       public string Step { get; set; }
     
        /// <summary>
       /// The amount associated with the specific pay scale.
       /// </summary>
       public decimal? Amount { get; set; }
     }      
}  

