// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Defines the type of SAP evaluation.
    /// </summary>
    [Serializable]
    public class AcademicProgressType : CodeItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Code;

        /// <summary>
        /// The description for this Academic Progress Type
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Text explaining to the student information about this SAP Type.
        /// I expect this to be coming in the near future. PBW 06/17/15.
        /// </summary>
       // public string Explanation { get { return _explanation; } }
       // private readonly string _explanation;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="desc">Description</param>
        public AcademicProgressType(string code, string desc)
            : base(code, desc)
        {
        }

    }
}
