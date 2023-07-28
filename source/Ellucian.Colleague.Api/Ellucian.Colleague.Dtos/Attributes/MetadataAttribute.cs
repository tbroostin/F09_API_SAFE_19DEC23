// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Attributes
{
    /// <summary>
    /// Attribute used on Controller methods and on DTO Properties to add documentation for the OpenApi documentation
    /// created by the schemas API and published to clients on the Developer Resources Site.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class MetadataAttribute : Attribute
    {
        /// <summary>
        /// Controller Attribute for API domain: Advancement, Financial Aid, Finance, Foundation, Human Resources, Recruitment or Student.
        /// </summary>
        public string ApiDomain { get; set; }

        /// <summary>
        /// Describe the functionality of the API for documentation using OpenAPi specifications.
        /// </summary>
        public string ApiDescription { get; set; }

        /// <summary>
        /// Status of the API version.  Attach to the version specific controller methods to identify the version status
        /// for a specific version of the API. Use a status of "B" for Beta or "R" for General Availability.
        /// </summary>
        public string ApiVersionStatus { get; set; }

        /// <summary>
        ///  HTTP methods supported method permission required.
        /// </summary>
        public string HttpMethodPermission { get; set; }

        /// <summary>
        ///  HTTP methods supported summary identified on each controller method.  Summarize the purpose of this method.
        /// </summary>
        public string HttpMethodSummary { get; set; }

        /// <summary>
        ///  HTTP methods supported summary identified on each controller method.  Summarize the purpose of this method.
        /// </summary>
        public string HttpMethodDescription { get; set; }

        /// <summary>
        /// Used to identify which Colleague File Name the data comes from.
        /// </summary>
        public string DataFileName { get; set; }

        /// <summary>
        /// Used to identify which data element (CDD) stores the data in Colleague.
        /// </summary>
        public string DataElementName { get; set; }

        /// <summary>
        /// Used to identify the maximum length of a data element (CDD) stored in Colleague.
        /// </summary>
        public int DataMaxLength { get; set; }

        /// <summary>
        /// Used to describe the actual data being exposed through the API.
        /// </summary>
        public string DataDescription { get; set; }

        /// <summary>
        /// Contains an entry for Reference File Name for this data element, such as AR.CODES or CORE.VALCODES
        /// </summary>
        public string DataReferenceFileName { get; set; }

        /// <summary>
        /// Contains an entry for Reference Valcode Table for this data element, such as ADREL.TYPES or PERSON.STATUSES
        /// </summary>
        public string DataReferenceTableName { get; set; }

        /// <summary>
        /// Contains an entry for Reference Field name for this data element, such as ARC.DESC or VAL.INTERNAL.CODE
        /// </summary>
        public string DataReferenceColumnName { get; set; }

        /// <summary>
        /// Used to identify if the data is required within the schema (PUT, POST or GET)
        /// </summary>
        public bool DataRequired { get; set; }

        /// <summary>
        /// Used to identify that a data element is only available on GET and cannot be updated by PUT or POST.
        /// </summary>
        public bool DataIsInquiryOnly { get; set; }

        /// <summary>
        /// Initializes a new instance of the Ellucian.Colleague.Dtos.Base.SchemasAttribute class.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="required"></param>
        /// <param name="inquiry"></param>
        /// <param name="maxLength"></param>
        public MetadataAttribute(string columnName, bool required = false, bool inquiry = false, int maxLength = 0)
        {
            DataElementName = columnName;
            DataRequired = required;
            DataIsInquiryOnly = inquiry;
            if (string.IsNullOrEmpty(DataElementName))
            {
                DataIsInquiryOnly = true;
            }
            DataMaxLength = maxLength;
            DataFileName = string.Empty;
            DataDescription = string.Empty;
            DataReferenceColumnName = string.Empty;
            DataReferenceFileName = string.Empty;
            DataReferenceTableName = string.Empty;
            ApiDomain = string.Empty;
            ApiDescription = string.Empty;
            ApiVersionStatus = string.Empty;
            HttpMethodPermission = string.Empty;
            HttpMethodSummary = string.Empty;
            HttpMethodDescription = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the Ellucian.Colleague.Dtos.Base.SchemasAttribute class.
        /// </summary>
        public MetadataAttribute()
        {
            ApiDomain = string.Empty;
            ApiDescription = string.Empty;
            ApiVersionStatus = string.Empty;
            HttpMethodPermission = string.Empty;
            HttpMethodSummary = string.Empty;
            HttpMethodDescription = string.Empty;
            DataFileName = string.Empty;
            DataElementName = string.Empty;
            DataMaxLength = 0;
            DataDescription = string.Empty;
            DataReferenceColumnName = string.Empty;
            DataReferenceFileName = string.Empty;
            DataReferenceTableName = string.Empty;
            DataIsInquiryOnly = false;
            DataRequired = false;
        }
    }
}