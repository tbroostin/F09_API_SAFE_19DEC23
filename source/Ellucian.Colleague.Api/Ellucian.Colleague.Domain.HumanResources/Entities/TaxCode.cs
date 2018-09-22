/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// The tax code class defines the properties of a Tax Code, which describes how a tax is applied to 
    /// a person's earnings.
    /// </summary>
    [Serializable]
    public class TaxCode : CodeItem
    {
        /// <summary>
        /// The category of the tax code, eg. Federal or State withholding
        /// </summary>
        public TaxCodeType Type { get; private set; }
        

        /// <summary>
        /// The expected filing status of a person who pays tax based on this TaxCode.
        /// Could be null.
        /// </summary>
        public TaxCodeFilingStatus FilingStatus { get; set; }     
        
           

        /// <summary>
        /// Creates an instance of a tax code item
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public TaxCode(string code, string description, TaxCodeType type) 
            : base(code, description)
        {
            Type = type;
        }
    }
}
