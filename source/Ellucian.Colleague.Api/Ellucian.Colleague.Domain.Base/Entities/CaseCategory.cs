// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Attributes of Case Category related to Retention Alert
    /// </summary>
    [Serializable]
    public class CaseCategory : CodeItem
    {
        /// <summary>
        /// The Category Id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// A list of case type ids in this category
        /// </summary>
        public List<string> CaseTypes { get; set; }

        /// <summary>
        /// A list of case closure reason in this category
        /// </summary>
        public List<string> CaseClosureReasons { get; set; }

        /// <summary>
        /// Case worker email hierarchy code
        /// </summary>
        public string CaseWorkerEmailHierarchyCode { get; set; }

        /// <summary>
        /// Case worker email hierarchies
        /// </summary>
        public List<string> CaseWorkerEmailHierarchy { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="categoryId"></param>
        /// <param name="caseTypes"></param>
        /// <param name="caseClosureReasons"></param>
        public CaseCategory(string code, string description, string categoryId, List<string> caseTypes, List<string> caseClosureReasons, string caseWorkerEmailHierarchyCode)
            : base(code, description)
        {
            CategoryId = categoryId;
            CaseTypes = caseTypes;
            CaseClosureReasons = caseClosureReasons;
            CaseWorkerEmailHierarchyCode = caseWorkerEmailHierarchyCode;
        }
    }
}


